using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace Titan.Build
{
    /// <summary>定义组里的一个值（include 目录或预处理宏），保留所属工具与两级条件供各消费方投影。</summary>
    public sealed class RawMetadata
    {
        public string Tool;
        public string GroupCondition;
        public string ElementCondition;
        public bool IsInclude;
        public string Value;
    }

    /// <summary>ItemGroup 里的一个编译项。</summary>
    public sealed class RawSource
    {
        public string Value;
        public string Condition;
        public bool AlwaysExcluded;
        public bool ConditionalExclusion;
        public List<RawMetadata> Defines = new List<RawMetadata>();
    }

    public sealed class RawHeader
    {
        public string Value;
        public string Condition;
    }

    /// <summary>一个输入文件（平台工程或共享项工程）的原始求值结果，不含任何平台策略。</summary>
    public sealed class RawInput
    {
        public string AbsPath;
        public string RelPath;
        public bool IsProject;
        public List<RawSource> Sources = new List<RawSource>();
        public List<RawHeader> Headers = new List<RawHeader>();
        public List<RawMetadata> Metadata = new List<RawMetadata>();
        /// <summary>文档顺序出现的定义组条件（null 表示无条件组），供"缺少配置"门禁判定。</summary>
        public List<string> GroupConditions = new List<string>();
    }

    public static class VcxReader
    {
        const string MacroThisDir = "$(MSBuildThisFileDirectory)";

        static readonly HashSet<string> MetadataElements = new HashSet<string>(StringComparer.Ordinal)
        {
            "AdditionalIncludeDirectories",
            "PreprocessorDefinitions",
        };

        public static string ToSlash(string path) => path.Replace('\\', '/');

        /// <summary>
        /// 绝对路径转仓库相对（正斜杠、无尾分隔符），与 CMake file(RELATIVE_PATH) 口径一致：
        /// 同一路径在 CMake 侧得到空串，这里得到 "."，由调用方按消费语义决定丢弃。
        /// </summary>
        public static string Relative(string root, string absPath)
        {
            var rel = ToSlash(Path.GetRelativePath(root, absPath)).TrimEnd('/');
            return rel.Length == 0 ? "." : rel;
        }

        public static string Resolve(string baseDir, string value)
        {
            var v = ToSlash(value);
            if (Path.IsPathRooted(v))
                return Path.GetFullPath(v);
            return Path.GetFullPath(Path.Combine(baseDir, v));
        }

        /// <summary>读取平台工程 Label="Shared" 的共享项 Import，按文档顺序去重，排除生成胶水。</summary>
        public static List<string> ReadSharedImports(string projectPath)
        {
            var doc = Load(projectPath);
            var dir = Path.GetDirectoryName(Path.GetFullPath(projectPath));
            var result = new List<string>();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name != "ItemGroup" && node.Name != "ImportGroup")
                    continue;
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name != "Import")
                        continue;
                    var label = child.Attributes?["Label"]?.Value;
                    if (label != "Shared")
                        continue;
                    var project = child.Attributes?["Project"]?.Value;
                    if (string.IsNullOrWhiteSpace(project))
                        continue;
                    var resolved = Resolve(dir, project.Replace(MacroThisDir, ToSlash(dir) + "/"));
                    if (!resolved.EndsWith(".vcxitems", StringComparison.OrdinalIgnoreCase))
                        continue;
                    // 生成胶水不是声明输入，首次生成前允许不存在。
                    if (Path.GetFileName(resolved).Equals("CodeGen.vcxitems", StringComparison.OrdinalIgnoreCase))
                        continue;
                    if (!result.Contains(resolved))
                        result.Add(resolved);
                }
            }
            if (result.Count == 0)
                throw new TitanEvalException($"[titan-eval] {projectPath} 缺少普通 Shared Import");
            return result;
        }

        public static RawInput ReadInput(string root, string absPath, bool isProject)
        {
            var doc = Load(absPath);
            var dir = Path.GetDirectoryName(Path.GetFullPath(absPath));
            var input = new RawInput
            {
                AbsPath = Path.GetFullPath(absPath),
                RelPath = Relative(root, absPath),
                IsProject = isProject,
            };

            foreach (XmlNode group in doc.DocumentElement.ChildNodes)
            {
                if (group.Name == "ItemGroup")
                {
                    CollectItems(input, group, dir);
                }
                else if (group.Name == "ItemDefinitionGroup")
                {
                    CollectMetadata(input, group, dir);
                }
            }
            return input;
        }

        static void CollectItems(RawInput input, XmlNode group, string dir)
        {
            var groupCondition = group.Attributes?["Condition"]?.Value;
            if (!string.IsNullOrWhiteSpace(groupCondition))
            {
                // ItemGroup 条件目前不属于受支持的条件集合；出现时必须显式失败而不是静默丢掉整组源文件。
                if (groupCondition.IndexOf("$(Configuration)", StringComparison.Ordinal) >= 0
                    || groupCondition.IndexOf("$(Platform)", StringComparison.Ordinal) >= 0)
                {
                    throw new TitanEvalException(
                        $"[titan-eval] {input.RelPath}: 不支持 ItemGroup 级条件: {groupCondition}");
                }
            }

            foreach (XmlNode item in group.ChildNodes)
            {
                if (item.Name == "ClCompile")
                {
                    var include = item.Attributes?["Include"]?.Value;
                    if (string.IsNullOrWhiteSpace(include))
                        continue;
                    var source = new RawSource
                    {
                        Value = Expand(include, dir),
                        Condition = item.Attributes?["Condition"]?.Value,
                    };
                    foreach (XmlNode meta in item.ChildNodes)
                    {
                        if (meta.Name == "ExcludedFromBuild")
                        {
                            var condition = meta.Attributes?["Condition"]?.Value;
                            var enabled = meta.InnerText.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
                            if (!enabled)
                                continue;
                            if (string.IsNullOrWhiteSpace(condition))
                                source.AlwaysExcluded = true;
                            else
                                source.ConditionalExclusion = true;
                        }
                        else if (meta.Name == "PreprocessorDefinitions" && !IsEmptyElement(meta) && !HasElementChildren(meta))
                        {
                            source.Defines.Add(new RawMetadata
                            {
                                Tool = "ClCompile",
                                GroupCondition = null,
                                ElementCondition = meta.Attributes?["Condition"]?.Value,
                                IsInclude = false,
                                Value = meta.InnerText,
                            });
                        }
                    }
                    input.Sources.Add(source);
                }
                else if (item.Name == "ClInclude")
                {
                    var include = item.Attributes?["Include"]?.Value;
                    if (string.IsNullOrWhiteSpace(include))
                        continue;
                    input.Headers.Add(new RawHeader
                    {
                        Value = Expand(include, dir),
                        Condition = item.Attributes?["Condition"]?.Value,
                    });
                }
            }
        }

        static void CollectMetadata(RawInput input, XmlNode group, string dir)
        {
            var groupCondition = group.Attributes?["Condition"]?.Value;
            input.GroupConditions.Add(groupCondition);
            foreach (XmlNode tool in group.ChildNodes)
            {
                if (tool.NodeType != XmlNodeType.Element)
                    continue;
                foreach (XmlNode element in tool.ChildNodes)
                {
                    if (element.NodeType != XmlNodeType.Element)
                        continue;
                    if (!MetadataElements.Contains(element.Name))
                        continue;
                    // 自闭合或被嵌套元素打断的值在旧实现里会被整体跳过，保持一致以免凭空多出条目。
                    if (IsEmptyElement(element) || HasElementChildren(element))
                        continue;
                    input.Metadata.Add(new RawMetadata
                    {
                        Tool = tool.Name,
                        GroupCondition = groupCondition,
                        ElementCondition = element.Attributes?["Condition"]?.Value,
                        IsInclude = element.Name == "AdditionalIncludeDirectories",
                        Value = element.InnerText,
                    });
                }
            }
        }

        static bool IsEmptyElement(XmlNode node) => node is XmlElement element && element.IsEmpty;

        static bool HasElementChildren(XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.NodeType == XmlNodeType.Element)
                    return true;
            }
            return false;
        }

        static string Expand(string value, string dir) => value.Replace(MacroThisDir, ToSlash(dir) + "/");

        static XmlDocument Load(string path)
        {
            if (!File.Exists(path))
                throw new TitanEvalException($"[titan-eval] 工程不存在: {path}");
            var doc = new XmlDocument { PreserveWhitespace = true };
            try
            {
                doc.Load(path);
            }
            catch (XmlException ex)
            {
                throw new TitanEvalException($"[titan-eval] 工程不是合法 XML: {path}: {ex.Message}", ex);
            }
            if (doc.DocumentElement == null)
                throw new TitanEvalException($"[titan-eval] 工程缺少根元素: {path}");
            return doc;
        }

        /// <summary>按 CMake 侧口径拆分值列表：分号分隔、去空白、丢弃空值与 %(...) 继承占位。</summary>
        public static IEnumerable<string> SplitValues(string text)
        {
            foreach (var raw in text.Split(';'))
            {
                var value = raw.Trim();
                if (value.Length == 0 || value.StartsWith("%(", StringComparison.Ordinal))
                    continue;
                yield return value;
            }
        }

        public static string ModuleNameOf(string vcxitemsRelPath)
        {
            var name = Path.GetFileNameWithoutExtension(vcxitemsRelPath);
            return name.Replace('.', '_');
        }
    }
}
