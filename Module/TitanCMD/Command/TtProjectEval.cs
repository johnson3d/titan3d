using System;
using System.IO;
using System.Linq;
using System.Text;
using Titan.Build;

namespace ProjectCooker.Command
{
    /// <summary>
    /// eval 子命令：把平台工程的求值模型（Module/TitanProjectEval）输出成 JSON。
    /// 纯构建期操作——不开引擎、不读 engine.jscfg、不写仓库产物，所以由 Program.Main
    /// 在引擎启动前直接派发（见 Documents/editor/TitanCMD.md 的“命令生命周期”）。
    /// 用法：TitanCMD ExeCmd=eval Project=Core.Mac/Core.Mac.vcxproj Platform=Mac
    ///        [Configuration=Debug] [PlatformName=ARM64] [ModulesFile=...] [Exclude=&lt;正则&gt;]...
    ///        [Json=&lt;输出路径&gt;] [TitanRoot=&lt;仓库根&gt;]
    /// </summary>
    static class TtProjectEval
    {
        public const string CommandName = "eval";

        const string Usage =
            "Usage: ExeCmd=eval Project=<相对仓库根的平台工程> Platform=<Mac|Android|Windows>\n" +
            "       [Configuration=Debug|Release] [PlatformName=ARM64|x64] [PlatformDir=Core.<Platform>] [Prefix=<大写平台>]\n" +
            "       [ModulesFile=<相对仓库根的模块文件>] [Exclude=<源排除正则，可重复>]...\n" +
            "       [Json=<输出路径，相对当前目录>] [TitanRoot=<仓库根>]";

        public static bool IsRequested(string[] args)
        {
            return string.Equals(TtCookCommand.FindArgument(args, "ExeCmd="), CommandName, StringComparison.OrdinalIgnoreCase);
        }

        static string DefaultPlatformDir(string platform)
        {
            // 平台目录名不是简单的 Core.<Platform>：Windows 的工程目录是 Core.Window。
            if (string.Equals(platform, "Mac", StringComparison.OrdinalIgnoreCase)) return "Core.Mac";
            if (string.Equals(platform, "Android", StringComparison.OrdinalIgnoreCase)) return "Core.Android";
            if (string.Equals(platform, "Windows", StringComparison.OrdinalIgnoreCase)) return "Core.Window";
            return "Core." + platform;
        }

        public static int Run(string[] args)
        {
            var project = TtCookCommand.FindArgument(args, "Project=");
            var platform = TtCookCommand.FindArgument(args, "Platform=");
            if (string.IsNullOrWhiteSpace(project) || string.IsNullOrWhiteSpace(platform))
            {
                Console.Error.WriteLine(Usage);
                return 1;
            }

            var root = TtCookCommand.FindArgument(args, "TitanRoot=");
            if (string.IsNullOrWhiteSpace(root))
                root = ProjectEvaluator.FindRoot(Directory.GetCurrentDirectory())
                       ?? ProjectEvaluator.FindRoot(AppContext.BaseDirectory);
            if (string.IsNullOrWhiteSpace(root))
            {
                Console.Error.WriteLine("[eval] 找不到 Titan 仓库（无 EngineAll.sln），请传 TitanRoot=");
                return 1;
            }
            root = Path.GetFullPath(root);

            var configuration = TtCookCommand.FindArgument(args, "Configuration=") ?? "Debug";
            if (configuration != "Debug" && configuration != "Release")
            {
                Console.Error.WriteLine("[eval] Configuration 必须为 Debug 或 Release");
                return 1;
            }
            var platformDir = TtCookCommand.FindArgument(args, "PlatformDir=") ?? DefaultPlatformDir(platform);
            // Windows 的工程条件写 x64，Mac/Android 写 ARM64；与各自 vcxproj 的 Condition 对齐。
            var platformName = TtCookCommand.FindArgument(args, "PlatformName=")
                               ?? (string.Equals(platform, "Windows", StringComparison.OrdinalIgnoreCase) ? "x64" : "ARM64");
            var prefix = TtCookCommand.FindArgument(args, "Prefix=") ?? platform.ToUpperInvariant();

            var modulesFile = TtCookCommand.FindArgument(args, "ModulesFile=");
            if (string.IsNullOrWhiteSpace(modulesFile))
            {
                // 平台模块文件只维护排除规则；Windows 没有这个文件，缺省就按“无模块文件”求值。
                var candidate = platformDir + "/" + platform + "Modules.cmake";
                if (File.Exists(Path.Combine(root, candidate)))
                    modulesFile = candidate;
            }

            var options = new EvalOptions
            {
                Root = root,
                ProjectRel = project,
                Platform = platform,
                PlatformDir = platformDir,
                Prefix = prefix,
                ModulesRel = modulesFile ?? string.Empty,
                Configuration = configuration,
                PlatformName = platformName,
                // Windows 目标的 SDK include 由宿主（注册表）提供，非 Windows 目标为空串；
                // 不在表里的属性一律按未解析失败。
                Properties = HostProperties.ForPlatform(platform),
                // 正则里可能有 '+'，因此 Exclude= 支持重复出现，不按 '+' 拆分。
                Excludes = args.Where(a => a.StartsWith("Exclude=", StringComparison.Ordinal))
                               .Select(a => a.Substring("Exclude=".Length))
                               .Where(v => v.Length > 0)
                               .ToList(),
            };

            ProjectEvaluation eval;
            try
            {
                eval = ProjectEvaluator.Evaluate(options);
            }
            catch (TitanEvalException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 2;
            }

            var text = eval.ToJson();
            var json = TtCookCommand.FindArgument(args, "Json=");
            if (string.IsNullOrWhiteSpace(json) || json == "-")
            {
                // 只把 JSON 写 stdout，摘要走 stderr，方便管道直接吃结果。
                Console.Out.WriteLine(text);
            }
            else
            {
                var path = Path.GetFullPath(json);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, text, new UTF8Encoding(false));
                Console.Out.WriteLine("[eval] model: " + path.Replace('\\', '/'));
            }
            Console.Error.WriteLine($"[eval] {eval.Platform}|{eval.Configuration} fingerprint={eval.Fingerprint} sources={eval.Sources.Count} modules={eval.Modules.Count}");
            return 0;
        }
    }
}
