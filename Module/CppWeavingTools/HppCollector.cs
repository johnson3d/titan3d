using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Titan.Build;

namespace CppWeaving
{
    public class HppCollector
    {
        public static HppCollector Instance = new HppCollector();
        public void Reset()
        {
            Headers.Clear();
            IncludePath.Clear();
            MacroDefines.Clear();
        }
        public class HppUnit
        {
            public string File;
            public string Module;
        }
        public Dictionary<string, HppUnit> Headers = new Dictionary<string, HppUnit>();
        public HppUnit FindHpp(string file)
        {
            bool error;
            file = UTypeManagerBase.NormalizePath(file, out error);
            if (string.IsNullOrEmpty(file))
                return null;
            if (OperatingSystem.IsWindows())
            {
                // Windows 上 clang 会把 #include 里的反斜杠与 .. 原样拼进文件名，先规范成绝对路径再查表。
                file = Path.GetFullPath(file).Replace('\\', '/');
            }
            file = file.ToLower();
            lock (this)
            {
                HppUnit result;
                if (Headers.TryGetValue(file, out result))
                {
                    return result;
                }
                return null;
            }
        }

        public List<string> IncludePath = new List<string>();
        public List<string> MacroDefines = new List<string>();
        public void AddExtraIncludePath(string cfg)
        {
            var text = System.IO.File.ReadAllText(cfg);
            var segs = text.Split('\n');
            foreach(var i in segs)
            {
                if (string.IsNullOrEmpty(i))
                    continue;
                IncludePath.Add(i);
            }
        }
        public void AddExtraMacroDefine(string cfg)
        {
            var text = System.IO.File.ReadAllText(cfg);
            var segs = text.Split('\n');
            foreach (var i in segs)
            {
                if (string.IsNullOrEmpty(i))
                    continue;
                MacroDefines.Add(i);
            }
        }
        /// <summary>
        /// 读取平台工程在该配置下的头文件清单、include 目录与宏。
        /// 工程求值由 Module/TitanProjectEval 承担（与 CMake 源清单同一实现），
        /// 这里只保留 clang 相关的投影：绝对路径、host-only 过滤、平台工程宏的采用规则。
        /// </summary>
        public void Collect(string proj, string buildConfig = @"'$(Configuration)|$(Platform)'=='Debug|x64'", TargetPlatform platform = TargetPlatform.Windows)
        {
            Headers.Clear();
            IncludePath.Clear();
            MacroDefines.Clear();

            var project = Path.GetFullPath(proj.Replace("\\", "/"));
            var (configuration, platformName) = ParseBuildCondition(buildConfig);
            var projectDir = Path.GetDirectoryName(project);
            var root = PlatformProject.FindRoot(projectDir) ?? projectDir;

            var properties = HostProperties.ForPlatform(platform.ToString());
            var eval = ProjectEvaluator.Evaluate(new EvalOptions
            {
                Root = root,
                ProjectRel = VcxReader.Relative(root, project),
                Platform = platform.ToString(),
                PlatformDir = Path.GetFileName(projectDir),
                Prefix = platform.ToString().ToUpperInvariant(),
                Configuration = configuration,
                PlatformName = platformName,
                Properties = properties,
            });

            CollectHeaders(eval);
            CollectCompileFlags(eval, platform, platform != TargetPlatform.Windows);
        }

        /// <summary>只消费共享项声明的头文件；平台工程自身的 ClInclude 不是解析入口。</summary>
        void CollectHeaders(ProjectEvaluation eval)
        {
            foreach (var input in eval.RawInputs)
            {
                if (input.IsProject)
                    continue;
                var module = VcxReader.ModuleNameOf(input.RelPath);
                foreach (var header in input.Headers)
                {
                    if (header.Value.EndsWith("TypeUtility.h") || header.Value.EndsWith(".inl.h"))
                        continue;
                    var file = Path.GetFullPath(header.Value).Replace('\\', '/');
                    if (!File.Exists(file))
                        throw new TitanEvalException($"共享工程 {input.RelPath} 的头文件不存在: {file}");
                    Headers[file.ToLowerInvariant()] = new HppUnit { File = file, Module = module };
                }
            }
        }

        void CollectCompileFlags(ProjectEvaluation eval, TargetPlatform platform, bool portable)
        {
            var platformMacro = platform == TargetPlatform.Android ? "PLATFORM_DROID" : "PLATFORM_MAC";
            // 非 Windows 平台下，平台工程只有自身声明了目标平台宏时它的宏才被采用，避免把 Windows 工程的宏
            // （PLATFORM_WIN/NOMINMAX/...）塞进 Mac/Android 的解析；共享项的宏始终采用。
            var useProjectDefines = !portable || eval.RawInputs
                .Where(i => i.IsProject)
                .SelectMany(i => i.Metadata)
                .Where(m => !m.IsInclude)
                .SelectMany(m => VcxReader.SplitValues(m.Value))
                .Contains(platformMacro);

            foreach (var applied in eval.AppliedValues)
            {
                if (applied.IsInclude)
                {
                    if (ClangArgs.IsHostOnlyInclude(platform, applied.Value))
                        continue;
                    var path = Path.GetFullPath(applied.AbsPath).Replace('\\', '/');
                    if (!IncludePath.Contains(path))
                        IncludePath.Add(path);
                    continue;
                }
                if (!useProjectDefines && applied.OwnerRel == eval.ProjectRel)
                    continue;
                if (!MacroDefines.Contains(applied.Value))
                    MacroDefines.Add(applied.Value);
            }
        }

        static (string Configuration, string PlatformName) ParseBuildCondition(string buildConfig)
        {
            var withPlatform = Regex.Match(buildConfig, @"=='(Debug|Release)\|([A-Za-z0-9_]+)'");
            if (withPlatform.Success)
                return (withPlatform.Groups[1].Value, withPlatform.Groups[2].Value);
            var configOnly = Regex.Match(buildConfig, @"=='(Debug|Release)'");
            if (configOnly.Success)
                return (configOnly.Groups[1].Value, "ARM64");
            throw new TitanEvalException($"[titan-eval] 不支持的配置: {buildConfig}");
        }

        static string NormalizePath(string path, out bool error)
        {
            error = false;

            path = path.Replace("\\", "/");

            path = path.Replace("../", "$/");

            path = path.Replace("./", "");

            //path = path.ToLower();

            int UpDirLength = "$/".Length;
            int startPos = path.LastIndexOf("$/");
            while (startPos >= 0)
            {
                int rmvNum = 1;
                var head = path.Substring(0, startPos);
                var tail = path.Substring(startPos + UpDirLength);
                while (head.Length > UpDirLength && head.EndsWith("$/"))
                {
                    rmvNum++;
                    head = head.Substring(0, head.Length - "$/".Length);
                }
                if (head.EndsWith('/'))
                    head = head.Substring(0, head.Length - 1);
                int discardPos = -1;
                for (int i = 0; i < rmvNum; i++)
                {
                    discardPos = head.LastIndexOf("/");
                    if (discardPos < 0)
                    {
                        error = true;
                        return null;
                    }
                    else
                    {
                        head = head.Substring(0, discardPos);
                    }
                }
                path = head + '/' + tail;

                startPos = path.LastIndexOf("$/");
            }

            return path;
        }
    }
}
