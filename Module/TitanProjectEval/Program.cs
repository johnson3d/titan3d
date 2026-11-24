using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Titan.Build
{
    public static class Program
    {
        const string Usage =
            "用法:\n" +
            "  TitanProjectEval refresh --root <仓库根> --project <平台工程> --platform <名称> --platform-dir <目录>\n" +
            "      --prefix <前缀> --modules-file <模块文件> [--sources-out <路径>] [--check]\n" +
            "      [--exclude <正则>]... [--impl-input <文件或目录>]... [--platform-name ARM64]\n" +
            "  TitanProjectEval model --root <仓库根> --project <平台工程> --configuration Debug|Release\n" +
            "      --platform <名称> [--platform-name ARM64] [--json <路径或 ->] [--exclude <正则>]...\n";

        public static int Main(string[] args)
        {
            if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
            {
                Console.Out.WriteLine(Usage);
                return args.Length == 0 ? 2 : 0;
            }
            try
            {
                switch (args[0])
                {
                    case "refresh":
                        return Refresh(ParseArgs(args, 1));
                    case "model":
                        return Model(ParseArgs(args, 1));
                    default:
                        Console.Error.WriteLine("[titan-eval] 未知命令: " + args[0]);
                        Console.Error.WriteLine(Usage);
                        return 2;
                }
            }
            catch (TitanEvalException ex)
            {
                Console.Error.WriteLine(ex.Message);
                return 2;
            }
        }

        static int Refresh(Dictionary<string, List<string>> args)
        {
            var root = Require(args, "root");
            var sourcesOut = Optional(args, "sources-out");
            if (string.IsNullOrWhiteSpace(sourcesOut))
            {
                sourcesOut = Path.Combine(root, Require(args, "platform-dir"), "generated", "TitanSources.cmake");
            }
            else if (!Path.IsPathRooted(sourcesOut))
            {
                sourcesOut = Path.GetFullPath(Path.Combine(root, sourcesOut));
            }

            var debug = Evaluate(root, args, "Debug");
            var release = Evaluate(root, args, "Release");
            var check = args.ContainsKey("check");
            var stale = Artifacts.WriteAll(debug, release, sourcesOut, check);
            if (stale.Count == 0)
                return 0;
            foreach (var path in stale)
                Console.Error.WriteLine("[titan-eval] STALE: " + path);
            Console.Error.WriteLine("[titan-eval] 请运行 cmake -P " + debug.PlatformDir +
                                      "/RefreshSources.cmake 重新生成并提交");
            return 1;
        }

        static int Model(Dictionary<string, List<string>> args)
        {
            var root = Require(args, "root");
            var eval = Evaluate(root, args, Optional(args, "configuration") ?? "Debug");
            var json = Optional(args, "json");
            var text = eval.ToJson();
            if (string.IsNullOrWhiteSpace(json) || json == "-")
            {
                Console.Out.WriteLine(text);
            }
            else
            {
                var path = Path.IsPathRooted(json) ? json : Path.GetFullPath(Path.Combine(root, json));
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, text, new UTF8Encoding(false));
                Console.Out.WriteLine("[titan-eval] model: " + path);
            }
            return 0;
        }

        static ProjectEvaluation Evaluate(string root, Dictionary<string, List<string>> args, string configuration)
        {
            var options = new EvalOptions
            {
                Root = root,
                ProjectRel = Require(args, "project"),
                Platform = Require(args, "platform"),
                PlatformDir = Optional(args, "platform-dir") ?? string.Empty,
                Prefix = Optional(args, "prefix") ?? string.Empty,
                ModulesRel = Optional(args, "modules-file") ?? string.Empty,
                Configuration = configuration,
                PlatformName = Optional(args, "platform-name") ?? "ARM64",
                Excludes = Values(args, "exclude"),
                ImplInputs = Values(args, "impl-input"),
                Properties = HostProperties.ForPlatform(Require(args, "platform")),
            };
            if (options.PlatformDir.Length == 0)
                options.PlatformDir = "Core." + options.Platform;
            if (options.Prefix.Length == 0)
                options.Prefix = options.Platform.ToUpperInvariant();
            return ProjectEvaluator.Evaluate(options);
        }

        static Dictionary<string, List<string>> ParseArgs(string[] args, int start)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            for (var i = start; i < args.Length; i++)
            {
                var arg = args[i];
                if (!arg.StartsWith("--", StringComparison.Ordinal))
                    throw new TitanEvalException("[titan-eval] 无法识别的参数: " + arg);
                var name = arg.Substring(2);
                string value = null;
                var eq = name.IndexOf('=');
                if (eq >= 0)
                {
                    value = name.Substring(eq + 1);
                    name = name.Substring(0, eq);
                }
                else if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                {
                    value = args[++i];
                }
                if (!result.TryGetValue(name, out var list))
                {
                    list = new List<string>();
                    result[name] = list;
                }
                if (value != null)
                    list.Add(value);
            }
            return result;
        }

        static string Require(Dictionary<string, List<string>> args, string name)
        {
            var value = Optional(args, name);
            if (string.IsNullOrWhiteSpace(value))
                throw new TitanEvalException("[titan-eval] 缺少参数 --" + name);
            return value;
        }

        static string Optional(Dictionary<string, List<string>> args, string name)
            => args.TryGetValue(name, out var list) && list.Count > 0 ? list[0] : null;

        static List<string> Values(Dictionary<string, List<string>> args, string name)
            => args.TryGetValue(name, out var list) ? new List<string>(list) : new List<string>();
    }
}
