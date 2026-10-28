using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CSharpCodeTools
{
    class Program
    {
        public static string FindArgument(string[] args, string startWith)
        {
            foreach (var i in args)
            {
                if (i.StartsWith(startWith))
                {
                    return i.Substring(startWith.Length);
                }
            }
            return null;
        }
        public static string[] GetArguments(string[] args, string startWith, char split = '+')
        {
            var types = FindArgument(args, startWith);
            if (types != null)
            {
                return types.Split(split);
            }
            return null;
        }
        static void Main(string[] args)
        {
            Console.OutputEncoding = new UTF8Encoding(false);
            try
            {
                var modes = GetArguments(args, "mode=") ?? new[] { "Rpc", "AutoSync", "Macross" };
                if (HasMode(modes, "DataCopyer"))
                {
                    if (modes.Length != 1) throw new ArgumentException("DataCopyer 必须在独立后置阶段运行");
                    RunDataCopyerPostBuild(args);
                    return;
                }
                var root = Path.GetFullPath(FindArgument(args, "TitanRoot=") ?? Path.Combine(AppContext.BaseDirectory, "../../.."));
                var migration = FindArgument(args, "MigrateLegacyBlocks=");
                if (migration != null)
                {
                    if (migration is not ("Check" or "Apply")) throw new ArgumentException("MigrateLegacyBlocks 必须是 Check 或 Apply");
                    var changed = LegacyGeneratedBlocks.MigrateRepository(root, migration == "Apply");
                    foreach (var file in changed) Console.WriteLine(file);
                    Console.WriteLine($"LegacyMigration: {migration}; Files={changed.Length}");
                    return;
                }
                var platform = FindArgument(args, "Platform=") ?? (OperatingSystem.IsMacOS() ? "Mac" : "Windows");
                var configuration = FindArgument(args, "Configuration=") ?? "Debug";
                if (FindArgument(args, "ValidateOutputs=") == "true")
                {
                    SourceGenerationRunner.ExportInputs(root, FindArgument(args, "SourceOwner="), platform, configuration,
                        FindArgument(args, "csproj="), FindArgument(args, "SourcesList="), FindArgument(args, "Compiler="), FindArgument(args, "RpcCallers=") == "true");
                    return;
                }
                var configArgument = args.FirstOrDefault(a => !a.Contains('='));
                var config = configArgument == null ? null : Path.GetFullPath(configArgument);
                SourceGenerationRunner.Run(root, FindArgument(args, "csproj="), platform, configuration, config, modes);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.ExitCode = 1;
            }
        }
        
        static bool HasMode(string[] modes, string mode)
        {
            if (modes == null)
                return false;

            foreach (var i in modes)
            {
                if (string.Equals(i, mode, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        static void RunDataCopyerPostBuild(string[] args)
        {
            var root = Path.GetFullPath(FindArgument(args, "TitanRoot=") ?? Path.Combine(AppContext.BaseDirectory, "../../.."));
            var platform = FindArgument(args, "Platform=") ?? (OperatingSystem.IsMacOS() ? "Mac" : "Windows");
            var configuration = FindArgument(args, "Configuration=") ?? "Debug";
            if (platform != "Windows" && platform != "Mac" || configuration != "Debug" && configuration != "Release")
                throw new ArgumentException("反射生成只支持 Windows/Mac 和 Debug/Release");
            var host = Path.GetFullPath(FindArgument(args, "ReflectionHost=") ?? Path.Combine(root, "binaries", "Tools", "Reflection", platform, configuration, "net8.0", "CSharpReflectionTools.dll"));
            if (!File.Exists(host))
            {
                Console.Error.WriteLine("缺少已构建的 CSharpReflectionTools 反射宿主：" + host);
                Environment.ExitCode = 1;
                return;
            }
            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = root, UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
            };
            start.ArgumentList.Add(host);
            foreach (var argument in args.Where(a => !a.StartsWith("mode=") && !a.StartsWith("ReflectionHost=") && !a.StartsWith("TitanRoot=") && !a.StartsWith("Platform=") && !a.StartsWith("Configuration=")))
                start.ArgumentList.Add(argument);
            start.ArgumentList.Add("TitanRoot=" + root);
            start.ArgumentList.Add("Platform=" + platform);
            start.ArgumentList.Add("Configuration=" + configuration);
            using var process = Process.Start(start) ?? throw new InvalidOperationException("无法启动反射宿主");
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            Console.Write(stdout.GetAwaiter().GetResult());
            Console.Error.Write(stderr.GetAwaiter().GetResult());
            Environment.ExitCode = process.ExitCode;
        }
    }
}
