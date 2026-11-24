using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Titan.Build
{
    public sealed class EvalOptions
    {
        public string Root;
        /// <summary>平台标识（Mac / Android / Windows），只作为 HasModule_&lt;平台&gt; 与产物命名的一部分。</summary>
        public string Platform;
        /// <summary>平台工程目录（Core.Mac），用于生成文件里的提示语与默认输出位置。</summary>
        public string PlatformDir;
        /// <summary>变量前缀（MAC / ANDROID），决定 set(TITAN_&lt;PREFIX&gt;_*) 的变量名。</summary>
        public string Prefix;
        public string ProjectRel;
        public string ModulesRel;
        public string Configuration;
        public string PlatformName = "ARM64";
        /// <summary>仓库相对路径排除规则（CMake 正则语义），只作用于源文件。</summary>
        public List<string> Excludes = new List<string>();
        /// <summary>求值器自身实现文件（相对仓库根或绝对路径），必须进入指纹。</summary>
        public List<string> ImplInputs = new List<string>();
        /// <summary>
        /// 已知 MSBuild 属性值（如 WindowsSDK_IncludePath），在拆分前展开；
        /// 未提供的 $(...) 一律按未解析属性失败，不允许静默空串。
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>已按配置过滤的单个元数据值，保留来源供需要平台策略的消费方自行投影。</summary>
    public sealed class AppliedValue
    {
        public string OwnerRel;
        public bool IsInclude;
        /// <summary>已展开属性、按分号拆分后的单个值（include 为原始写法，define 为宏字符串）。</summary>
        public string Value;
        /// <summary>include 的绝对路径；define 为 null。</summary>
        public string AbsPath;
    }

    /// <summary>某一平台 + 配置的求值结果：所有消费方（CMake 清单、codegen、一致性核对）共用。</summary>
    public sealed class ProjectEvaluation
    {
        public string Platform;
        public string PlatformDir;
        public string Prefix;
        public string Configuration;
        public string PlatformName;
        public string ProjectRel;
        public List<string> Inputs = new List<string>();
        public List<string> InputHashes = new List<string>();
        public List<string> Modules = new List<string>();
        public List<string> ModuleDefines = new List<string>();
        public List<string> ModuleDirs = new List<string>();
        public List<string> Sources = new List<string>();
        public List<string> Includes = new List<string>();
        public List<string> Defines = new List<string>();
        public List<string> SourceDefines = new List<string>();
        public string Fingerprint;
        /// <summary>已生效的 include/宏（含来源），供 codegen 等按平台策略投影。</summary>
        public List<AppliedValue> AppliedValues = new List<AppliedValue>();
        /// <summary>原料，供 codegen 等需要逐输入细节的消费方自行投影。</summary>
        public List<RawInput> RawInputs = new List<RawInput>();

        /// <summary>
        /// 求值结果的 JSON 投影。字段集合只有这一份：CLI 的 model 与 TitanCMD 的 eval 都调它，
        /// 避免两个入口各自拼一份、慢慢漂移。
        /// </summary>
        public string ToJson()
        {
            var payload = new
            {
                platform = Platform,
                platformDir = PlatformDir,
                configuration = Configuration,
                platformName = PlatformName,
                project = ProjectRel,
                fingerprint = Fingerprint,
                inputs = Inputs,
                inputHashes = InputHashes,
                modules = Modules,
                moduleDefines = ModuleDefines,
                moduleDirs = ModuleDirs,
                sources = Sources,
                includes = Includes,
                defines = Defines,
                sourceDefines = SourceDefines,
            };
            return JsonSerializer.Serialize(payload, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }) + "\n";
        }
    }

    public static class ProjectEvaluator
    {
        static readonly (string From, string To)[] ModuleAliases =
        {
            ("NextRHI", "RHI"),
            ("NullDevice", "VirtualDevice"),
            ("Canvas", "TextFont"),
        };

        /// <summary>从 start 向上找含 EngineAll.sln 的目录；找不到返回 null。</summary>
        public static string FindRoot(string start)
        {
            var dir = new DirectoryInfo(start);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "EngineAll.sln")))
                    return dir.FullName;
                dir = dir.Parent;
            }
            return null;
        }

        public static ProjectEvaluation Evaluate(EvalOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.Root))
                throw new TitanEvalException("[titan-eval] 缺少仓库根");
            var root = Path.GetFullPath(options.Root);
            var projectAbs = Path.GetFullPath(Path.Combine(root, options.ProjectRel));
            if (!File.Exists(projectAbs))
                throw new TitanEvalException($"[titan-eval] 平台工程不存在: {options.ProjectRel}");

            var matcher = new ConditionMatcher(options.Configuration, options.PlatformName);
            var imports = VcxReader.ReadSharedImports(projectAbs);
            var moduleRels = imports.Select(x => VcxReader.Relative(root, x)).ToList();

            var eval = new ProjectEvaluation
            {
                Platform = options.Platform,
                PlatformDir = options.PlatformDir,
                Prefix = options.Prefix,
                Configuration = options.Configuration,
                PlatformName = options.PlatformName,
                ProjectRel = VcxReader.ToSlash(options.ProjectRel),
                Modules = moduleRels.ToList(),
            };

            eval.Inputs.Add(eval.ProjectRel);
            if (!string.IsNullOrWhiteSpace(options.ModulesRel))
                eval.Inputs.Add(VcxReader.ToSlash(options.ModulesRel));
            foreach (var impl in options.ImplInputs)
                eval.Inputs.AddRange(ExpandImplInput(root, impl));
            eval.Inputs.AddRange(moduleRels);
            eval.Inputs = Distinct(eval.Inputs);

            var rawInputs = new List<RawInput> { VcxReader.ReadInput(root, projectAbs, true) };
            foreach (var import in imports)
            {
                if (!File.Exists(import))
                    throw new TitanEvalException($"[titan-eval] 平台工程 {eval.ProjectRel} 的 Shared Import 不存在: {import}");
                rawInputs.Add(VcxReader.ReadInput(root, import, false));
            }
            eval.RawInputs = rawInputs;

            // 缺少配置必须失败：无条件组掩盖不了"这个工程根本没有该配置"。
            foreach (var raw in rawInputs.Where(x => x.IsProject))
            {
                var matched = raw.GroupConditions.Any(c => !string.IsNullOrWhiteSpace(c) && matcher.Matches(c));
                if (!matched)
                    throw new TitanEvalException(
                        $"[titan-eval] {raw.RelPath} 缺少 {options.Configuration}|{options.PlatformName} 配置");
            }

            BuildModuleShape(eval);
            BuildSources(eval, root, matcher, options.Excludes);
            BuildMetadata(eval, root, matcher, options.Properties);
            BuildFingerprint(eval, root);
            return eval;
        }

        static void BuildModuleShape(ProjectEvaluation eval)
        {
            var defines = new List<string> { "HasModule_" + eval.Platform };
            var dirs = new List<string>();
            foreach (var module in eval.Modules)
            {
                var name = VcxReader.ModuleNameOf(module);
                defines.Add("HasModule_" + name);
                dirs.Add(Path.GetDirectoryName(VcxReader.ToSlash(module))?.Replace('\\', '/') ?? string.Empty);
            }
            foreach (var alias in ModuleAliases)
            {
                if (defines.Contains("HasModule_" + alias.From))
                    defines.Add("HasModule_" + alias.To);
            }
            eval.ModuleDefines = defines.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToList();
            eval.ModuleDirs = dirs;
        }

        static void BuildSources(ProjectEvaluation eval, string root, ConditionMatcher matcher, List<string> excludes)
        {
            var sources = new List<string>();
            foreach (var input in eval.RawInputs)
            {
                // 源清单只由 Shared Import（.vcxitems）贡献；平台工程自身的 ClCompile 是平台胶水，
                // 由平台 CMakeLists 显式加入，进清单会与那里重复。
                if (input.IsProject)
                    continue;
                foreach (var source in input.Sources)
                {
                    if (!matcher.Matches(source.Condition))
                        continue;
                    if (source.AlwaysExcluded)
                        continue;
                    if (source.ConditionalExclusion)
                    {
                        Console.Error.WriteLine(
                            $"[titan-eval] 警告: {input.RelPath} 的条件 ExcludedFromBuild 在 {eval.Configuration}|{eval.PlatformName} 下保留: {source.Value}");
                    }
                    var abs = VcxReader.Resolve(Path.GetDirectoryName(input.AbsPath), source.Value);
                    if (!File.Exists(abs))
                        throw new TitanEvalException($"[titan-eval] {input.RelPath} 引用的源文件不存在: {abs}");
                    var rel = VcxReader.Relative(root, abs);
                    var excluded = false;
                    foreach (var rule in excludes)
                    {
                        if (Regex.IsMatch(rel, rule))
                        {
                            excluded = true;
                            break;
                        }
                    }
                    if (!excluded)
                        sources.Add(rel);
                }
            }
            eval.Sources = sources.Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal).ToList();
        }

        static void BuildMetadata(ProjectEvaluation eval, string root, ConditionMatcher matcher, Dictionary<string, string> properties)
        {
            var includes = new List<string>();
            var defines = new List<string>();
            var sourceDefines = new List<string>();

            foreach (var input in eval.RawInputs)
            {
                foreach (var meta in input.Metadata)
                {
                    if (!matcher.Matches(meta.GroupCondition))
                        continue;
                    if (!matcher.Matches(meta.ElementCondition))
                        continue;
                    // 属性先展开再拆分：一个属性可能展开成多个目录。
                    var text = ExpandProperties(meta.Value, input.AbsPath, properties);
                    foreach (var value in VcxReader.SplitValues(text))
                    {
                        if (!meta.IsInclude)
                        {
                            if (value.Contains("$("))
                                throw new TitanEvalException($"[titan-eval] 工程 {input.RelPath} 的未解析属性: {value}");
                            if (!defines.Contains(value))
                                defines.Add(value);
                            eval.AppliedValues.Add(new AppliedValue
                            {
                                OwnerRel = input.RelPath,
                                IsInclude = false,
                                Value = value,
                            });
                            continue;
                        }

                        if (value.Contains("$("))
                            throw new TitanEvalException($"[titan-eval] 工程 {input.RelPath} 的未解析属性: {value}");

                        var abs = VcxReader.Resolve(Path.GetDirectoryName(input.AbsPath), value);
                        var rel = VcxReader.Relative(root, abs);
                        // 仓库根本身作为 include 目录：CMake 侧相对路径为空串、foreach 会跳过，这里保持同口径。
                        if (rel == ".")
                            continue;
                        if (!includes.Contains(rel))
                            includes.Add(rel);
                        eval.AppliedValues.Add(new AppliedValue
                        {
                            OwnerRel = input.RelPath,
                            IsInclude = true,
                            Value = value,
                            AbsPath = abs,
                        });
                    }
                }

                foreach (var source in input.Sources)
                {
                    if (!matcher.Matches(source.Condition))
                        continue;
                    if (source.AlwaysExcluded)
                        continue;
                    var abs = VcxReader.Resolve(Path.GetDirectoryName(input.AbsPath), source.Value);
                    var rel = VcxReader.Relative(root, abs);
                    foreach (var meta in source.Defines)
                    {
                        if (!matcher.Matches(meta.ElementCondition))
                            continue;
                        foreach (var value in VcxReader.SplitValues(meta.Value))
                        {
                            if (value.Contains("$("))
                                throw new TitanEvalException($"[titan-eval] 工程 {input.RelPath} 的未解析属性: {value}");
                            sourceDefines.Add(rel + "|" + eval.Configuration + "|" + value);
                        }
                    }
                }
            }

            eval.Includes = includes;
            eval.Defines = defines;
            eval.SourceDefines = sourceDefines;
        }

        /// <summary>
        /// 展开 $(MSBuildThisFileDirectory) 与调用方提供的属性；不认识的 $(...) 原样保留，
        /// 由下游的未解析属性检查报错，不在这里静默吃掉。
        /// </summary>
        static string ExpandProperties(string text, string ownerAbsPath, Dictionary<string, string> properties)
        {
            if (text.IndexOf("$(", StringComparison.Ordinal) < 0)
                return text;
            var sb = new System.Text.StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] != '$' || i + 1 >= text.Length || text[i + 1] != '(')
                {
                    sb.Append(text[i]);
                    continue;
                }
                var end = text.IndexOf(')', i + 2);
                if (end < 0)
                {
                    sb.Append(text, i, text.Length - i);
                    break;
                }
                var name = text.Substring(i + 2, end - i - 2);
                if (name.Equals("MSBuildThisFileDirectory", StringComparison.OrdinalIgnoreCase))
                    sb.Append(VcxReader.ToSlash(Path.GetDirectoryName(ownerAbsPath))).Append('/');
                else if (properties.TryGetValue(name, out var value))
                    sb.Append(value);
                else
                    sb.Append(text, i, end - i + 1);
                i = end;
            }
            return sb.ToString();
        }

        static void BuildFingerprint(ProjectEvaluation eval, string root)
        {
            var acc = new System.Text.StringBuilder();
            foreach (var rel in eval.Inputs)
            {
                var abs = Path.IsPathRooted(rel) ? rel : Path.Combine(root, rel);
                if (!File.Exists(abs))
                    throw new TitanEvalException($"[titan-eval] 指纹输入缺失: {rel}");
                var hash = Sha.OfFileWithoutCr(abs);
                eval.InputHashes.Add(rel + "=" + hash);
                acc.Append(rel).Append('=').Append(hash).Append('\n');
            }
            eval.Fingerprint = Sha.OfString(acc.ToString());
        }

        /// <summary>
        /// 实现输入可以是文件或目录。目录按 *.cs 展开（排除 obj/bin），让"改了求值器就必然让入库清单过期"
        /// 由指纹强制，不靠人记得。
        /// </summary>
        static IEnumerable<string> ExpandImplInput(string root, string impl)
        {
            var abs = Path.IsPathRooted(impl) ? Path.GetFullPath(impl) : Path.GetFullPath(Path.Combine(root, impl));
            if (!Directory.Exists(abs))
            {
                yield return RelativeImpl(root, abs);
                yield break;
            }
            foreach (var file in Directory.EnumerateFiles(abs, "*.cs", SearchOption.AllDirectories)
                         .OrderBy(x => x, StringComparer.Ordinal))
            {
                var rel = RelativeImpl(root, file);
                if (rel.Contains("/obj/") || rel.Contains("/bin/"))
                    continue;
                yield return rel;
            }
        }

        static string RelativeImpl(string root, string abs)
        {
            var rel = Path.GetRelativePath(root, abs);
            if (!rel.StartsWith("..", StringComparison.Ordinal) && !Path.IsPathRooted(rel))
                return VcxReader.ToSlash(rel);
            return VcxReader.ToSlash(abs);
        }

        static List<string> Distinct(List<string> values)
        {
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var result = new List<string>();
            foreach (var value in values)
            {
                if (seen.Add(value))
                    result.Add(value);
            }
            return result;
        }
    }
}
