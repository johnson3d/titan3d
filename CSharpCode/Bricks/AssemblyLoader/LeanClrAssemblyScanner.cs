using System;
using System.Collections.Generic;

namespace EngineNS.LeanCLR
{
    /// <summary>
    /// 一次扫描得到的 VM 程序集清单；Errors 非空时不能启动 VM。
    /// </summary>
    public sealed class TtLeanClrAssemblySet
    {
        /// <summary>程序集元数据名 → 完整路径，按注册顺序排列。</summary>
        public readonly List<KeyValuePair<string, string>> Assemblies = new();
        public readonly List<string> Errors = new();
        public readonly List<string> Warnings = new();
        /// <summary>扫到 EngineStub 或配置了额外程序集时才启动 VM，避免未用热更的工程白加载 BCL。</summary>
        public bool ShouldStartRuntime { get; internal set; }
    }

    /// <summary>
    /// 收集要注册进 LeanCLR VM 的程序集：content\LeanCLR 递归下全部 dll + TtEngineConfig.LeanClrAssemblies 补充项。
    /// </summary>
    public static class TtLeanClrAssemblyScanner
    {
        public static TtLeanClrAssemblySet Collect(string scanDirectory, IEnumerable<string> extraFiles)
        {
            var result = new TtLeanClrAssemblySet();
            var pathsByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var hasExtra = false;

            if (System.IO.Directory.Exists(scanDirectory))
            {
                var files = System.IO.Directory.GetFiles(scanDirectory, "*.dll", System.IO.SearchOption.AllDirectories);
                // GetFiles 不保证顺序，排序让注册顺序与日志稳定
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                foreach (var file in files)
                    Add(result, pathsByName, file);
            }

            if (extraFiles != null)
            {
                foreach (var file in extraFiles)
                {
                    if (string.IsNullOrEmpty(file))
                        continue;
                    hasExtra = true;
                    if (!System.IO.File.Exists(file))
                    {
                        result.Errors.Add($"LeanCLR assembly was not found: {file}");
                        continue;
                    }
                    Add(result, pathsByName, file);
                }
            }

            result.ShouldStartRuntime = hasExtra || pathsByName.ContainsKey(TtLeanClrHostBinding.EngineStubAssemblyName);
            return result;
        }

        private static void Add(TtLeanClrAssemblySet result, Dictionary<string, string> pathsByName, string file)
        {
            var path = System.IO.Path.GetFullPath(file);
            string name;
            try
            {
                // RName 会把路径转成小写，程序集名必须取自元数据才能匹配 VM 按名字的查找
                name = System.Reflection.AssemblyName.GetAssemblyName(path).Name;
            }
            catch (BadImageFormatException)
            {
                result.Warnings.Add($"LeanCLR skipped non-managed dll: {path}");
                return;
            }

            if (pathsByName.TryGetValue(name, out var existing))
            {
                if (!string.Equals(existing, path, StringComparison.OrdinalIgnoreCase))
                    result.Errors.Add($"LeanCLR assembly {name} is provided twice: {existing} and {path}");
                return;
            }
            pathsByName.Add(name, path);
            result.Assemblies.Add(new KeyValuePair<string, string>(name, path));
        }
    }
}
