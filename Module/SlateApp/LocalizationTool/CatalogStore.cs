using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using EngineNS.Localization;

namespace LocalizationTool
{
    // 单个 occurrence 的用户状态持久化记录（以指纹为键）。
    public class TtScanStateEntry
    {
        public bool Selected { get; set; }
        public bool Ignored { get; set; }
    }

    // 翻译目录的持久化：
    // - 先加载 enginecontent/localization，再加载 content/localization 作为游戏覆盖层；
    // - 保存到游戏覆盖层：仅将用户选中的键写入目录（无译文写空占位 value=""）、已有人工译文不覆盖、源码中消失的键标记 obsolete 但不删除；
    // - 保存前做占位符校验，错误项拒绝写入；
    // - 扫描的勾选/忽略状态持久化到 cache/localization/scan-state.json（以稳定指纹为键）。
    public class TtCatalogStore
    {
        readonly string mRepoRoot;
        readonly string mEngineCatalogDir;
        readonly string mGameCatalogDir;
        readonly string mScanStateFile;

        public TtLocalizationConfig Config { get; private set; } = new TtLocalizationConfig();

        public TtCatalogStore(string repoRoot, string engineContentRoot, string gameContentRoot)
        {
            mRepoRoot = repoRoot.Replace('\\', '/').TrimEnd('/') + "/";
            mEngineCatalogDir = GetCatalogDir(engineContentRoot);
            mGameCatalogDir = GetCatalogDir(gameContentRoot);
            mScanStateFile = CombineRoot("cache/localization/scan-state.json");
            LoadConfig();
        }

        static string GetCatalogDir(string contentRoot)
        {
            return Path.Combine(contentRoot ?? string.Empty, "localization").Replace('\\', '/');
        }

        string CombineRoot(string relative)
        {
            return (mRepoRoot + relative.Replace('\\', '/').TrimStart('/')).Replace('\\', '/');
        }

        #region config / cultures
        void LoadConfig()
        {
            Config = new TtLocalizationConfig();
            MergeConfigFile(mEngineCatalogDir);
            MergeConfigFile(mGameCatalogDir);
        }

        void MergeConfigFile(string catalogDir)
        {
            var file = Path.Combine(catalogDir, "localization.config.json");
            if (File.Exists(file))
                Config = TtLocalizationSerializer.MergeConfig(Config, File.ReadAllText(file));
        }

        public void SaveConfig()
        {
            Directory.CreateDirectory(mGameCatalogDir);
            var file = Path.Combine(mGameCatalogDir, "localization.config.json");
            File.WriteAllText(file, TtLocalizationSerializer.SerializeConfig(Config));
        }

        public void AddCulture(string culture)
        {
            if (string.IsNullOrWhiteSpace(culture))
                return;
            culture = culture.Trim();
            if (Config.Cultures.Contains(culture) == false)
            {
                Config.Cultures.Add(culture);
                SaveConfig();
            }
        }

        public void RemoveCulture(string culture)
        {
            if (string.Equals(culture, Config.SourceCulture, StringComparison.OrdinalIgnoreCase))
                return; // 源语言不可移除。
            if (Config.Cultures.Remove(culture))
                SaveConfig();
        }

        // 加载某语言的完整目录条目（译文 + 源码位置）；游戏层的非空译文与源码位置覆盖引擎层。
        Dictionary<string, TtLocalizationCatalogEntry> LoadCultureEntries(string culture)
        {
            var result = LoadCultureEntries(mEngineCatalogDir, culture);
            var gameEntries = LoadCultureEntries(mGameCatalogDir, culture);
            foreach (var kv in gameEntries)
            {
                if (result.TryGetValue(kv.Key, out var current) == false)
                {
                    result[kv.Key] = kv.Value;
                    continue;
                }
                if (string.IsNullOrEmpty(kv.Value.Translation) == false || string.IsNullOrEmpty(current.Translation))
                    current.Translation = kv.Value.Translation;
                if (kv.Value.Sources != null && kv.Value.Sources.Count > 0)
                    current.Sources = kv.Value.Sources;
            }
            return result;
        }

        static Dictionary<string, TtLocalizationCatalogEntry> LoadCultureEntries(string catalogDir, string culture)
        {
            var file = Path.Combine(catalogDir, $"{culture}.json");
            if (File.Exists(file) == false)
                return new Dictionary<string, TtLocalizationCatalogEntry>(StringComparer.Ordinal);
            return TtLocalizationSerializer.DeserializeCatalogEntries(File.ReadAllText(file));
        }

        // UI 与运行时编辑流程只需要原文 -> 译文平面字典。
        public Dictionary<string, string> LoadCulture(string culture)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var kv in LoadCultureEntries(culture))
                result[kv.Key] = kv.Value.Translation ?? string.Empty;
            return result;
        }
        #endregion

        #region entries build / merge
        // 将扫描出的 occurrence 按 SourceKey 聚合为翻译条目，并载入各语言已有译文。
        public List<TtTranslationEntry> BuildEntries(TtScanResult scan)
        {
            var map = new Dictionary<string, TtTranslationEntry>(StringComparer.Ordinal);
            foreach (var occ in scan.Occurrences)
            {
                if (string.IsNullOrEmpty(occ.SourceKey))
                    continue;
                if (map.TryGetValue(occ.SourceKey, out var entry) == false)
                {
                    entry = new TtTranslationEntry
                    {
                        Source = occ.SourceKey,
                        IsFormat = occ.RewriteKind == ERewriteKind.Format,
                        PlaceholderCount = TtLocalizationSerializer.GetPlaceholderIndices(occ.SourceKey).Count,
                    };
                    map[occ.SourceKey] = entry;
                }
                if (occ.RewriteKind == ERewriteKind.Format)
                    entry.IsFormat = true;
                entry.Occurrences.Add(occ);
            }

            // 载入各语言现有译文。
            var cultures = TranslatableCultures();
            var loaded = new Dictionary<string, Dictionary<string, string>>();
            foreach (var c in cultures)
                loaded[c] = LoadCulture(c);

            foreach (var entry in map.Values)
            {
                foreach (var c in cultures)
                {
                    if (loaded[c].TryGetValue(entry.Source, out var t))
                        entry.Translations[c] = t;
                }
            }

            // 标记 obsolete：目录里有、但本次扫描已不存在的键。
            var entries = map.Values.ToList();
            var present = new HashSet<string>(map.Keys, StringComparer.Ordinal);
            foreach (var c in cultures)
            {
                foreach (var kv in loaded[c])
                {
                    if (present.Contains(kv.Key))
                        continue;
                    if (map.TryGetValue(kv.Key, out var obsEntry) == false)
                    {
                        obsEntry = new TtTranslationEntry { Source = kv.Key, Obsolete = true };
                        map[kv.Key] = obsEntry;
                        entries.Add(obsEntry);
                    }
                    obsEntry.Translations[c] = kv.Value;
                }
            }

            entries.Sort((a, b) => string.CompareOrdinal(a.Source, b.Source));
            return entries;
        }

        // 除源语言外的可翻译语言。
        public List<string> TranslatableCultures()
        {
            return Config.Cultures
                .Where(c => string.IsNullOrEmpty(c) == false &&
                            string.Equals(c, Config.SourceCulture, StringComparison.OrdinalIgnoreCase) == false)
                .Distinct()
                .ToList();
        }
        #endregion

        #region save catalogs
        // merge 保存指定语言：仅将至少一个 occurrence 被选中的键写入目录。
        // 本次扫描存在但未选中的键会从目录移除；源码中已不存在的 obsolete 键仍保留，避免误删历史人工译文。
        // 每个键保存为包含 Translation 与 Sources 的对象；Sources 使用相对引擎仓库根目录的位置。
        // 磁盘上已有的非空译文不会被空值覆盖。返回被拒绝的占位符错误列表。
        public List<string> SaveCulture(string culture, IEnumerable<TtTranslationEntry> entries, bool includeObsolete)
        {
            var errors = new List<string>();
            var engineEntries = LoadCultureEntries(mEngineCatalogDir, culture);
            var merged = LoadCultureEntries(mGameCatalogDir, culture); // 只更新游戏覆盖层，不回写引擎基础目录。

            foreach (var entry in entries)
            {
                if (entry.Obsolete && includeObsolete == false)
                    continue;
                if (string.IsNullOrEmpty(entry.Source))
                    continue;

                // 选择状态按 Source 聚合：任一 occurrence 被选中且未忽略，该 key 即进入所有语言目录。
                // 对本次扫描存在的未选中 key 显式删除，避免旧的空占位继续残留在 zh-CN.json 等文件中。
                var selectedOccurrences = entry.Occurrences
                    .Where(o => o.Selected && o.Ignored == false)
                    .ToList();
                if (entry.Obsolete == false && selectedOccurrences.Count == 0)
                {
                    merged.Remove(entry.Source);
                    continue;
                }

                entry.Translations.TryGetValue(culture, out var text);
                text = text ?? string.Empty;

                // 未修改的引擎层译文不复制到游戏层，避免游戏目录固化一份引擎目录快照。
                var hasGameEntry = merged.TryGetValue(entry.Source, out var catalogEntry);
                if (hasGameEntry == false &&
                    engineEntries.TryGetValue(entry.Source, out var engineEntry) &&
                    string.Equals(text, engineEntry.Translation ?? string.Empty, StringComparison.Ordinal))
                    continue;
                if (hasGameEntry == false)
                {
                    catalogEntry = new TtLocalizationCatalogEntry();
                    merged[entry.Source] = catalogEntry;
                }

                // 每次保存都根据当前扫描刷新位置，去重并稳定排序；绝不写入绝对路径。
                if (entry.Obsolete == false)
                {
                    catalogEntry.Sources = selectedOccurrences
                        .Where(o => string.IsNullOrEmpty(o.RelativeFilePath) == false)
                        .GroupBy(o => $"{o.RelativeFilePath.Replace('\\', '/')}\n{o.Line}", StringComparer.Ordinal)
                        .Select(g => g.First())
                        .OrderBy(o => o.RelativeFilePath, StringComparer.Ordinal)
                        .ThenBy(o => o.Line)
                        .Select(o => new TtLocalizationSourceLocation
                        {
                            File = o.RelativeFilePath.Replace('\\', '/').TrimStart('/'),
                            Line = o.Line,
                        })
                        .ToList();
                }

                // 译文为空时保留磁盘上已有译文；新条目保持空 Translation，供专业翻译填写。
                if (string.IsNullOrEmpty(text))
                    continue;

                // 有译文时对 Format 项做占位符校验，错误译文不覆盖现有值。
                if (entry.IsFormat)
                {
                    if (TtLocalizationSerializer.ValidatePlaceholders(entry.Source, text, out var err) == false)
                    {
                        errors.Add($"[{culture}] \"{entry.Source}\": {err}");
                        continue;
                    }
                }
                catalogEntry.Translation = text;
            }

            Directory.CreateDirectory(mGameCatalogDir);
            var file = Path.Combine(mGameCatalogDir, $"{culture}.json");
            File.WriteAllText(file, TtLocalizationSerializer.SerializeCatalog(merged));
            return errors;
        }

        // 保存全部可翻译语言，返回所有占位符错误。
        public List<string> SaveAll(IEnumerable<TtTranslationEntry> entries)
        {
            var list = entries.ToList();
            var errors = new List<string>();
            foreach (var c in TranslatableCultures())
                errors.AddRange(SaveCulture(c, list, includeObsolete: false));
            return errors;
        }

        // 显式清理某语言中已 obsolete 的键。
        public void PurgeObsolete(string culture, IEnumerable<TtTranslationEntry> entries)
        {
            var merged = LoadCultureEntries(mGameCatalogDir, culture);
            foreach (var entry in entries)
            {
                if (entry.Obsolete)
                    merged.Remove(entry.Source);
            }
            Directory.CreateDirectory(mGameCatalogDir);
            var file = Path.Combine(mGameCatalogDir, $"{culture}.json");
            File.WriteAllText(file, TtLocalizationSerializer.SerializeCatalog(merged));
        }
        #endregion

        #region scan-state
        public Dictionary<string, TtScanStateEntry> LoadScanState()
        {
            if (File.Exists(mScanStateFile) == false)
                return new Dictionary<string, TtScanStateEntry>(StringComparer.Ordinal);
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, TtScanStateEntry>>(File.ReadAllText(mScanStateFile));
                return parsed != null
                    ? new Dictionary<string, TtScanStateEntry>(parsed, StringComparer.Ordinal)
                    : new Dictionary<string, TtScanStateEntry>(StringComparer.Ordinal);
            }
            catch (Exception)
            {
                return new Dictionary<string, TtScanStateEntry>(StringComparer.Ordinal);
            }
        }

        public void SaveScanState(IEnumerable<TtStringOccurrence> occurrences)
        {
            var map = new Dictionary<string, TtScanStateEntry>(StringComparer.Ordinal);
            foreach (var occ in occurrences)
            {
                if (string.IsNullOrEmpty(occ.Fingerprint))
                    continue;
                map[occ.Fingerprint] = new TtScanStateEntry { Selected = occ.Selected, Ignored = occ.Ignored };
            }
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(mScanStateFile));
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(mScanStateFile, JsonSerializer.Serialize(map, options));
            }
            catch (Exception)
            {
                // scan-state 仅为便利，写失败不影响主流程。
            }
        }

        // 将上次保存的勾选/忽略状态套用到新扫描结果（以指纹匹配）。
        public void ApplyScanState(TtScanResult scan)
        {
            var state = LoadScanState();
            if (state.Count == 0)
                return;
            foreach (var occ in scan.Occurrences)
            {
                if (string.IsNullOrEmpty(occ.Fingerprint))
                    continue;
                if (state.TryGetValue(occ.Fingerprint, out var s))
                {
                    occ.Selected = s.Selected;
                    occ.Ignored = s.Ignored;
                }
            }
        }
        #endregion
    }
}
