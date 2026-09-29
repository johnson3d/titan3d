using System;
using System.Collections.Generic;
using System.Globalization;

namespace EngineNS.Localization
{
    // 多国化运行时状态。与现有断词逻辑同属 LocalizationManager（partial），此文件承载翻译目录的
    // 加载、按 culture 查询、缺失回退与运行时语言切换。任何数据缺失都回退到原文，绝不因本地化
    // 数据损坏而中断引擎启动。
    public partial class LocalizationManager
    {
        bool mLocLoaded = false;
        string mSourceCulture = "en-US";
        string mCurrentCulture = "en-US";
        TtLocalizationConfig mLocConfig = new TtLocalizationConfig();
        // 当前语言的不可变翻译字典（原文 -> 译文）。加载后不再修改，切换语言时整体替换。
        Dictionary<string, string> mTranslations = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly HashSet<string> mMissingWarned = new HashSet<string>(StringComparer.Ordinal);
        readonly object mLocLock = new object();

        public string CurrentCulture => mCurrentCulture;
        public string SourceCulture => mSourceCulture;

        // 固定的引擎层与游戏层目录；加载顺序保证游戏内容覆盖引擎默认内容。
        static string GetLocalizationRoot(IO.TtFileManager.ERootDir rootType)
        {
            var contentRoot = TtEngine.Instance.FileManager.GetRoot(rootType);
            return IO.TtFileManager.CombinePath(contentRoot, "localization/");
        }

        // 将引擎配置里的 EditorLanguage（历史值 "English"/"Chinese" 或直接的 culture 代码）映射为 culture。
        public static string MapEditorLanguageToCulture(string editorLanguage)
        {
            if (string.IsNullOrEmpty(editorLanguage))
                return "en-US";
            switch (editorLanguage.Trim().ToLowerInvariant())
            {
                case "english":
                case "en":
                case "en-us":
                    return "en-US";
                case "chinese":
                case "zh":
                case "zh-cn":
                case "chs":
                    return "zh-CN";
                default:
                    return editorLanguage.Trim();
            }
        }

        void EnsureLocalizationLoaded()
        {
            if (mLocLoaded)
                return;
            lock (mLocLock)
            {
                if (mLocLoaded)
                    return;
                try
                {
                    LoadConfig();
                    var culture = MapEditorLanguageToCulture(TtEngine.Instance?.Config?.EditorLanguage);
                    if (string.IsNullOrEmpty(culture))
                        culture = mLocConfig.DefaultCulture;
                    LoadCultureInternal(culture);
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, $"Localization load failed: {ex.Message}");
                    mTranslations = new Dictionary<string, string>(StringComparer.Ordinal);
                }
                mLocLoaded = true;
            }
        }

        void LoadConfig()
        {
            mLocConfig = new TtLocalizationConfig();
            MergeConfigFile(IO.TtFileManager.ERootDir.Engine);
            MergeConfigFile(IO.TtFileManager.ERootDir.Game);
            mSourceCulture = string.IsNullOrEmpty(mLocConfig.SourceCulture) ? "en-US" : mLocConfig.SourceCulture;
        }

        void MergeConfigFile(IO.TtFileManager.ERootDir rootType)
        {
            var configFile = IO.TtFileManager.CombinePath(GetLocalizationRoot(rootType), "localization.config.json");
            if (System.IO.File.Exists(configFile))
                mLocConfig = TtLocalizationSerializer.MergeConfig(mLocConfig, System.IO.File.ReadAllText(configFile));
        }

        void LoadCultureInternal(string culture)
        {
            mCurrentCulture = string.IsNullOrEmpty(culture) ? mSourceCulture : culture;
            mMissingWarned.Clear();

            // 当前语言即源语言时，无需查表，直接返回原文。
            if (string.Equals(mCurrentCulture, mSourceCulture, StringComparison.OrdinalIgnoreCase))
            {
                mTranslations = new Dictionary<string, string>(StringComparer.Ordinal);
                return;
            }

            var next = new Dictionary<string, string>(StringComparer.Ordinal);
            // 每一层先加载父 culture 再加载具体 culture；整个游戏层最后加载，覆盖引擎层。
            var parent = GetParentCulture(mCurrentCulture);
            MergeCultureRoot(next, IO.TtFileManager.ERootDir.Engine, parent, mCurrentCulture);
            MergeCultureRoot(next, IO.TtFileManager.ERootDir.Game, parent, mCurrentCulture);
            mTranslations = next;
        }

        static string GetParentCulture(string culture)
        {
            if (string.IsNullOrEmpty(culture))
                return null;
            var idx = culture.IndexOf('-');
            return idx > 0 ? culture.Substring(0, idx) : null;
        }

        void MergeCultureRoot(Dictionary<string, string> target, IO.TtFileManager.ERootDir rootType, string parent, string culture)
        {
            if (string.IsNullOrEmpty(parent) == false &&
                string.Equals(parent, mSourceCulture, StringComparison.OrdinalIgnoreCase) == false)
                MergeCultureFile(target, rootType, parent);
            MergeCultureFile(target, rootType, culture);
        }

        static void MergeCultureFile(Dictionary<string, string> target, IO.TtFileManager.ERootDir rootType, string culture)
        {
            var file = IO.TtFileManager.CombinePath(GetLocalizationRoot(rootType), $"{culture}.json");
            if (System.IO.File.Exists(file) == false)
                return;
            var entries = TtLocalizationSerializer.DeserializeCatalog(System.IO.File.ReadAllText(file));
            foreach (var kv in entries)
            {
                if (string.IsNullOrEmpty(kv.Value))
                    continue;
                target[kv.Key] = kv.Value;
            }
        }

        // 运行时切换语言，重新加载翻译字典。
        public void SetCulture(string culture)
        {
            lock (mLocLock)
            {
                if (mLocLoaded == false)
                    LoadConfig();
                LoadCultureInternal(culture);
                mLocLoaded = true;
            }
        }

        // 核心查表：命中译文则返回，未命中回退原文并仅告警一次。
        public string Translate(string source)
        {
            if (string.IsNullOrEmpty(source))
                return source;
            EnsureLocalizationLoaded();

            if (string.Equals(mCurrentCulture, mSourceCulture, StringComparison.OrdinalIgnoreCase))
                return source;

            if (mTranslations.TryGetValue(source, out var translated) && string.IsNullOrEmpty(translated) == false)
                return translated;

            if (mMissingWarned.Add(source))
            {
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                    $"Localization missing [{mCurrentCulture}]: \"{source}\"");
            }
            return source;
        }

        // composite format 版本：翻译格式串后再 string.Format；占位符异常时逐级回退，保证不抛出。
        public string TranslateFormat(string sourceFormat, params object[] args)
        {
            var format = Translate(sourceFormat);
            if (args == null || args.Length == 0)
                return format;
            try
            {
                return string.Format(CultureInfo.CurrentCulture, format, args);
            }
            catch (FormatException)
            {
                try
                {
                    return string.Format(CultureInfo.CurrentCulture, sourceFormat, args);
                }
                catch (FormatException)
                {
                    return sourceFormat;
                }
            }
        }
    }
}

namespace EngineNS
{
    // 源码改写的目标接口：所有用户可见 UI 文本统一通过此入口。
    // 键策略为“原文作为键”，因此 Tr("Close") 既是运行时查询也是给工具扫描的翻译键。
    public static class TtLocalization
    {
        // 翻译纯文本。TtEngine 尚未就绪时安全回退原文。
        public static string Tr(string source)
        {
            var mgr = TtEngine.Instance?.LocalizationManager;
            if (mgr == null)
                return source;
            return mgr.Translate(source);
        }

        // 翻译带占位符的格式串并填充参数。对应插值字符串 $"..." 的改写目标。
        public static string Format(string sourceFormat, params object[] args)
        {
            var mgr = TtEngine.Instance?.LocalizationManager;
            if (mgr == null)
            {
                if (args == null || args.Length == 0)
                    return sourceFormat;
                try { return string.Format(sourceFormat, args); }
                catch (FormatException) { return sourceFormat; }
            }
            return mgr.TranslateFormat(sourceFormat, args);
        }

        // 翻译可见文本，并原样保留 ImGui 的 "##id" / "###id" 标识后缀（idSuffix 可为常量或插值串）。
        public static string Label(string visibleSource, string idSuffix)
        {
            return Tr(visibleSource) + idSuffix;
        }
    }
}
