using System;
using System.IO;
using Titan.Build;

namespace CppWeaving
{
    // 平台工程、配置和默认输出的集中解析，不允许回退到其他平台。
    internal sealed class PlatformProject
    {
        public TargetPlatform Platform;
        public string Configuration;
        public string Project;
        public string Pch;
        public string CppOutput;
        public string CsOutput;
        public string BuildCondition => $"'$(Configuration)|$(Platform)'=='{Configuration}|{(Platform == TargetPlatform.Windows ? "x64" : "ARM64")}'";

        // 仓库根识别与求值器同一份实现，避免两条路径各自向上找 EngineAll.sln。
        internal static string FindRoot(string start) => ProjectEvaluator.FindRoot(start);

        public static PlatformProject Resolve(string[] args)
        {
            var platform = ClangArgs.ParsePlatform(Program.FindArgument(args, "Platform="));
            var configuration = Program.FindArgument(args, "Configuration=") ?? "Debug";
            if (configuration != "Debug" && configuration != "Release")
                throw new ArgumentException("Configuration 必须为 Debug 或 Release");
            var explicitProject = Program.FindArgument(args, "vcxproj=");
            var root = Program.FindArgument(args, "TitanRoot=") ?? FindRoot(Environment.CurrentDirectory)
                ?? (explicitProject == null ? null : FindRoot(Path.GetDirectoryName(Path.GetFullPath(explicitProject))))
                ?? FindRoot(AppContext.BaseDirectory);
            if (root == null && explicitProject == null)
                throw new DirectoryNotFoundException("找不到 Titan 仓库，请传入 TitanRoot= 或 vcxproj=");
            var projectName = platform switch
            {
                TargetPlatform.Mac => "Core.Mac",
                TargetPlatform.Android => "Core.Android",
                _ => "Core.Window"
            };
            var project = Path.GetFullPath(explicitProject ?? Path.Combine(root, projectName, projectName + ".vcxproj"));
            if (!File.Exists(project)) throw new FileNotFoundException("平台工程不存在", project);
            var pch = Path.GetFullPath(Program.FindArgument(args, "Pch=") ?? Path.Combine(Path.GetDirectoryName(project), "pch.h"));
            if (!File.Exists(pch)) throw new FileNotFoundException("平台 PCH 不存在", pch);
            var outputRoot = root ?? Path.GetDirectoryName(project);
            var output = Path.Combine(outputRoot, "codegen", platform.ToString(), "NativeBinder");
            return new PlatformProject
            {
                Platform = platform, Configuration = configuration, Project = project.Replace('\\', '/'), Pch = pch.Replace('\\', '/'),
                CppOutput = Path.GetFullPath(Program.FindArgument(args, "CppOut=") ?? output).Replace('\\', '/'),
                CsOutput = Path.GetFullPath(Program.FindArgument(args, "CsOut=") ?? output).Replace('\\', '/')
            };
        }
    }
}
