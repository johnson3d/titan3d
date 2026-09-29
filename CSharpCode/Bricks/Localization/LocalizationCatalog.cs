using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EngineNS.Localization
{
    // 多国化目录的配置与数据模型。运行时仅依赖 config + 各语言字典；manifest 只给工具使用。
    // 翻译键策略：以源语言原文作为键（source-as-key），译文表为“原文 -> 译文”。
    public class TtLocalizationConfig
    {
        // 源语言 culture（原文所使用的语言），缺省 en-US。
        public string SourceCulture { get; set; } = "en-US";
        // 未显式指定语言时使用的 culture。
        public string DefaultCulture { get; set; } = "en-US";
        // 目录支持的全部 culture 列表。
        public List<string> Cultures { get; set; } = new List<string>() { "en-US" };
    }

    // 翻译条目的源码位置。File 使用相对于引擎仓库根目录的正斜杠路径，Line 为 1-based 行号。
    public class TtLocalizationSourceLocation
    {
        public string File { get; set; } = string.Empty;
        public int Line { get; set; }
    }

    // 单个翻译条目。对象格式允许在不影响运行时查表的前提下扩充供翻译人员查阅的元数据。
    public class TtLocalizationCatalogEntry
    {
        public string Translation { get; set; } = string.Empty;
        public List<TtLocalizationSourceLocation> Sources { get; set; } = new List<TtLocalizationSourceLocation>();
    }

    // 单一语言的翻译目录（原文 -> 翻译条目）。
    public class TtLocalizationCatalog
    {
        public string Culture { get; set; } = "en-US";
        public Dictionary<string, TtLocalizationCatalogEntry> Entries { get; set; } = new Dictionary<string, TtLocalizationCatalogEntry>(StringComparer.Ordinal);
    }

    // 静态工具方法：占位符校验与目录文件的读写。工具与运行时共用同一份实现，保证格式一致。
    public static class TtLocalizationSerializer
    {
        static readonly Regex PlaceholderRegex = new Regex(@"\{(\d+)(?:,[^}]*)?(?::[^}]*)?\}", RegexOptions.Compiled);

        static readonly JsonSerializerOptions WriteOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            // 保留非 ASCII 字符（中文等）原样输出，便于人工校对。
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        // 提取 composite format 字符串中出现的占位符编号集合（忽略 alignment / format clause）。
        // 转义花括号 {{ }} 不计入。
        public static SortedSet<int> GetPlaceholderIndices(string format)
        {
            var result = new SortedSet<int>();
            if (string.IsNullOrEmpty(format))
                return result;

            for (int i = 0; i < format.Length; i++)
            {
                var c = format[i];
                if (c == '{')
                {
                    if (i + 1 < format.Length && format[i + 1] == '{')
                    {
                        i++; // 转义 {{
                        continue;
                    }
                    var match = PlaceholderRegex.Match(format, i);
                    if (match.Success && match.Index == i)
                    {
                        result.Add(int.Parse(match.Groups[1].Value));
                        i += match.Length - 1;
                    }
                }
                else if (c == '}')
                {
                    if (i + 1 < format.Length && format[i + 1] == '}')
                        i++; // 转义 }}
                }
            }
            return result;
        }

        // 校验译文与原文占位符集合一致；返回 true 表示译文可安全用于 string.Format。
        public static bool ValidatePlaceholders(string sourceFormat, string translatedFormat, out string error)
        {
            error = null;
            var src = GetPlaceholderIndices(sourceFormat);
            var dst = GetPlaceholderIndices(translatedFormat);
            if (src.SetEquals(dst) == false)
            {
                error = $"Placeholder mismatch: source={{{string.Join(",", src)}}} translated={{{string.Join(",", dst)}}}";
                return false;
            }
            return true;
        }

        public static string SerializeConfig(TtLocalizationConfig config)
        {
            return JsonSerializer.Serialize(config, WriteOptions);
        }

        public static TtLocalizationConfig DeserializeConfig(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new TtLocalizationConfig();
            try
            {
                return JsonSerializer.Deserialize<TtLocalizationConfig>(json) ?? new TtLocalizationConfig();
            }
            catch (Exception)
            {
                return new TtLocalizationConfig();
            }
        }

        // 将一层 JSON 配置覆盖到基础配置；缺失属性保留基础值，Cultures 数组整体覆盖。
        public static TtLocalizationConfig MergeConfig(TtLocalizationConfig baseConfig, string overlayJson)
        {
            var source = baseConfig ?? new TtLocalizationConfig();
            var result = new TtLocalizationConfig
            {
                SourceCulture = source.SourceCulture,
                DefaultCulture = source.DefaultCulture,
                Cultures = source.Cultures != null ? new List<string>(source.Cultures) : new List<string>(),
            };
            if (string.IsNullOrEmpty(overlayJson))
                return result;

            try
            {
                using var document = JsonDocument.Parse(overlayJson);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return result;
                var root = document.RootElement;
                if (root.TryGetProperty(nameof(TtLocalizationConfig.SourceCulture), out var sourceCulture) &&
                    sourceCulture.ValueKind == JsonValueKind.String)
                    result.SourceCulture = sourceCulture.GetString() ?? result.SourceCulture;
                if (root.TryGetProperty(nameof(TtLocalizationConfig.DefaultCulture), out var defaultCulture) &&
                    defaultCulture.ValueKind == JsonValueKind.String)
                    result.DefaultCulture = defaultCulture.GetString() ?? result.DefaultCulture;
                if (root.TryGetProperty(nameof(TtLocalizationConfig.Cultures), out var cultures) &&
                    cultures.ValueKind == JsonValueKind.Array)
                {
                    result.Cultures.Clear();
                    foreach (var culture in cultures.EnumerateArray())
                    {
                        if (culture.ValueKind == JsonValueKind.String)
                            result.Cultures.Add(culture.GetString() ?? string.Empty);
                    }
                }
            }
            catch (Exception)
            {
                // 损坏的覆盖层不影响已经加载的基础配置。
            }
            return result;
        }

        // 序列化对象格式目录，按 key 与源码位置稳定排序，方便人工查阅、diff 与合并。
        public static string SerializeCatalog(Dictionary<string, TtLocalizationCatalogEntry> entries)
        {
            var sorted = new SortedDictionary<string, TtLocalizationCatalogEntry>(StringComparer.Ordinal);
            if (entries != null)
            {
                foreach (var kv in entries)
                {
                    var entry = kv.Value ?? new TtLocalizationCatalogEntry();
                    sorted[kv.Key] = new TtLocalizationCatalogEntry
                    {
                        Translation = entry.Translation ?? string.Empty,
                        Sources = (entry.Sources ?? new List<TtLocalizationSourceLocation>())
                            .Where(s => s != null && string.IsNullOrEmpty(s.File) == false)
                            .OrderBy(s => s.File, StringComparer.Ordinal)
                            .ThenBy(s => s.Line)
                            .Select(s => new TtLocalizationSourceLocation
                            {
                                File = s.File.Replace('\\', '/'),
                                Line = s.Line,
                            })
                            .ToList(),
                    };
                }
            }
            return JsonSerializer.Serialize(sorted, WriteOptions);
        }

        // 兼容旧 key:string 与新 key:object 格式；工具保存后会统一升级为对象格式。
        public static Dictionary<string, TtLocalizationCatalogEntry> DeserializeCatalogEntries(string json)
        {
            var result = new Dictionary<string, TtLocalizationCatalogEntry>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(json))
                return result;
            try
            {
                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                    return result;

                foreach (var property in document.RootElement.EnumerateObject())
                {
                    var entry = new TtLocalizationCatalogEntry();
                    if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        entry.Translation = property.Value.GetString() ?? string.Empty;
                    }
                    else if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        if (property.Value.TryGetProperty("Translation", out var translation) &&
                            translation.ValueKind == JsonValueKind.String)
                            entry.Translation = translation.GetString() ?? string.Empty;

                        if (property.Value.TryGetProperty("Sources", out var sources) &&
                            sources.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var source in sources.EnumerateArray())
                            {
                                if (source.ValueKind != JsonValueKind.Object)
                                    continue;
                                string file = string.Empty;
                                int line = 0;
                                if (source.TryGetProperty("File", out var fileNode) && fileNode.ValueKind == JsonValueKind.String)
                                    file = fileNode.GetString() ?? string.Empty;
                                if (source.TryGetProperty("Line", out var lineNode) && lineNode.TryGetInt32(out var parsedLine))
                                    line = parsedLine;
                                if (string.IsNullOrEmpty(file) == false)
                                    entry.Sources.Add(new TtLocalizationSourceLocation { File = file.Replace('\\', '/'), Line = line });
                            }
                        }
                    }
                    result[property.Name] = entry;
                }
            }
            catch (Exception)
            {
                // 数据损坏时返回已成功读取的条目；运行时对缺失项回退到原文，不阻断启动。
            }
            return result;
        }

        // 运行时只需要原文 -> 译文平面字典，元数据仅供工具和人工翻译使用。
        public static Dictionary<string, string> DeserializeCatalog(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var kv in DeserializeCatalogEntries(json))
                result[kv.Key] = kv.Value.Translation ?? string.Empty;
            return result;
        }
    }
}
