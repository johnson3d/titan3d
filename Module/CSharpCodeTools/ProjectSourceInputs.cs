using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CSharpCodeTools
{
    public sealed class ProjectSourceInputs
    {
        public string ProjectFile { get; private set; } = "";
        public string AssemblyName { get; private set; } = "";
        public string TargetFramework { get; private set; } = "";
        public string Platform { get; private set; } = "";
        public string Configuration { get; private set; } = "";
        public string[] SourceFiles { get; private set; } = Array.Empty<string>();
        public string[] ImportedFiles { get; private set; } = Array.Empty<string>();
        public CSharpParseOptions ParseOptions { get; private set; } = CSharpParseOptions.Default;

        public static ProjectSourceInputs Load(string projectFile, string platform, string configuration)
        {
            return LoadFromRoot(FindRoot(), projectFile, platform, configuration);
        }

        public static ProjectSourceInputs LoadFromRoot(string titanRoot, string projectFile, string platform, string configuration)
        {
            if (string.IsNullOrEmpty(platform)) platform = OperatingSystem.IsMacOS() ? "Mac" : "Windows";
            if (string.IsNullOrEmpty(configuration)) configuration = "Debug";
            if (platform != "Windows" && platform != "Mac" && platform != "Android") throw new ArgumentException("Platform 必须是 Windows、Mac 或 Android", nameof(platform));
            if (configuration != "Debug" && configuration != "Release") throw new ArgumentException("Configuration 必须是 Debug 或 Release", nameof(configuration));
            var root = Path.GetFullPath(titanRoot);
            var engine = platform switch
            {
                "Mac" => "Engine.Mac",
                "Android" => "Engine.Android",
                _ => "Engine.Window"
            };
            var project = Path.GetFullPath(string.IsNullOrEmpty(projectFile) ? Path.Combine(root, "Module", engine, engine + ".csproj") : Path.Combine(root, projectFile));
            if (!File.Exists(project)) throw new InvalidOperationException("找不到工程：" + project);
            var target = Path.Combine(root, "BuildScript", "msbuild", "TitanSourceInputs.targets");
            if (!File.Exists(target)) throw new InvalidOperationException("找不到采集 target：" + target);
            var arguments = new List<string>
            {
                "msbuild", project, "-nologo", "-verbosity:quiet", "-p:TitanPlatform=" + platform,
                "-p:Configuration=" + configuration, "-p:TitanRoot=" + EscapeProperty(root),
                "-p:SkipTitanCodeGen=true", "-p:SkipTitanDataCopyerGen=true", "-p:TitanCollectingSourceInputs=true"
            };
            // 先让 MSBuild 求值工程原有扩展入口；适配器保留原 import 后再接入采集目标。
            using var adapter = CommonTargetsAdapter.Create(root, arguments, target);
            arguments.Add("-p:CustomAfterMicrosoftCommonTargets=" + EscapeProperty(adapter.FilePath));
            var query = Run(root, arguments.Concat(new[]
            {
                "-t:TitanCollectSourceInputs",
                "-getProperty:MSBuildProjectFullPath,AssemblyName,TargetFramework,LangVersion,DefineConstants,TitanPlatform,Configuration",
                "-getItem:TitanSourceFile"
            }));
            using var json = JsonDocument.Parse(query);
            var properties = json.RootElement.GetProperty("Properties");
            string Property(string name) => properties.GetProperty(name).GetString() ?? "";
            if (Property("TitanPlatform") != platform || Property("Configuration") != configuration)
                throw new InvalidOperationException("工程求值改变了请求的平台或配置：" + project);
            if (!LanguageVersionFacts.TryParse(Property("LangVersion"), out var language))
                throw new InvalidOperationException("无效 LangVersion：" + Property("LangVersion"));
            var sources = json.RootElement.GetProperty("Items").GetProperty("TitanSourceFile").EnumerateArray()
                .Select(item => Path.GetFullPath(item.GetProperty("FullPath").GetString() ?? throw new InvalidOperationException("缺少 Compile.FullPath")))
                .Distinct(StringComparer.Ordinal).OrderBy(path => path, StringComparer.Ordinal).ToArray();
            // MSBuild 的预处理输出记录实际展开的 imports。读取其边界元数据，不解析 Condition。
            // MSBuildAllProjects 在较新版本会省略不少 SDK/共享 imports，不能单独作为指纹清单。
            var preprocessedText = Run(root, arguments.Concat(new[] { "-preprocess" }));
            XDocument preprocessed;
            try { preprocessed = XDocument.Parse(preprocessedText); }
            catch (System.Xml.XmlException ex)
            {
                var lines = preprocessedText.Split('\n');
                throw new InvalidOperationException("MSBuild 预处理诊断：\n" + string.Join("\n", lines.Skip(Math.Max(0, ex.LineNumber - 3)).Take(6)), ex);
            }
            var imports = new HashSet<string>(StringComparer.Ordinal) { project, target };
            foreach (var comment in preprocessed.DescendantNodes().OfType<XComment>())
            {
                foreach (Match match in Regex.Matches(comment.Value, @"(?:^|\r?\n)([^\r\n]+)\r?\n={10,}(?:\r?\n|$)"))
                {
                    var path = match.Groups[1].Value.Trim();
                    if (!Path.IsPathFullyQualified(path)) continue;
                    path = Path.GetFullPath(path);
                    if (path == adapter.FilePath) continue;
                    if (!File.Exists(path)) throw new InvalidOperationException("实际 import 已消失：" + path);
                    imports.Add(path);
                }
            }
            return new ProjectSourceInputs
            {
                ProjectFile = project, AssemblyName = Property("AssemblyName"), TargetFramework = Property("TargetFramework"),
                Platform = platform, Configuration = configuration, SourceFiles = sources,
                ImportedFiles = imports.OrderBy(path => path, StringComparer.Ordinal).ToArray(),
                ParseOptions = new CSharpParseOptions(language, DocumentationMode.Parse, SourceCodeKind.Regular,
                    Property("DefineConstants").Split(new[] { ';', ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.Ordinal))
            };
        }

        public sealed class PluginProject
        {
            public string Name { get; }
            public string DescriptorFile { get; }
            public string ProjectFile { get; }
            internal PluginProject(string name, string descriptor, string project)
            {
                Name = name; DescriptorFile = descriptor; ProjectFile = project;
            }
        }

        public static PluginProject[] FindPluginProjects(string titanRoot, string platform)
        {
            if (platform != "Windows" && platform != "Mac" && platform != "Android") throw new ArgumentException("Platform 必须是 Windows、Mac 或 Android", nameof(platform));
            var root = Path.Combine(Path.GetFullPath(titanRoot), "Plugins");
            if (!Directory.Exists(root)) return Array.Empty<PluginProject>();
            var projects = new List<PluginProject>();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in Directory.EnumerateFiles(root, "*.plugin", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
            {
                var segments = Path.GetRelativePath(root, file).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (segments.Take(segments.Length - 1).Any(s => new[] { "bin", "obj", "binaries", "intermediate" }.Contains(s, StringComparer.OrdinalIgnoreCase))) continue;
                using var descriptor = JsonDocument.Parse(File.ReadAllText(file));
                var json = descriptor.RootElement;
                if (json.TryGetProperty("Enable", out var enabled) && !enabled.GetBoolean()) continue;
                var platforms = json.GetProperty("Platforms").EnumerateArray().Select(p => p.GetString()).ToArray();
                var all = platforms.Contains("PLTF_ALL", StringComparer.Ordinal);
                var platformId = platform switch
                {
                    "Windows" => "PLTF_Windows",
                    "Mac" => "PLTF_Mac",
                    _ => "PLTF_Android"
                };
                if (!all && !platforms.Contains(platformId, StringComparer.Ordinal)) continue;
                var name = Path.GetFileNameWithoutExtension(file);
                if (!names.Add(name)) throw new InvalidOperationException("重复插件名称：" + name);
                var suffix = platform switch { "Windows" => "Window", "Mac" => "Mac", _ => "Android" };
                var assembly = name + "." + (all ? "All" : suffix);
                var project = Path.Combine(Path.GetDirectoryName(file)!, assembly, assembly + ".csproj");
                if (!File.Exists(project)) throw new InvalidOperationException("兼容插件缺少所声明平台工程：" + project);
                projects.Add(new PluginProject(name, Path.GetFullPath(file), Path.GetFullPath(project)));
            }
            return projects.ToArray();
        }

        public static ProjectSourceInputs[] LoadPlatformProjects(string titanRoot, string projectFile, string platform, string configuration)
        {
            var engine = LoadFromRoot(titanRoot, projectFile, platform, configuration);
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var pluginsRoot = Path.TrimEndingDirectorySeparator(Path.Combine(Path.GetFullPath(titanRoot), "Plugins"));
            if (engine.ProjectFile.StartsWith(pluginsRoot + Path.DirectorySeparatorChar, comparison))
                throw new InvalidDataException("csproj 入口必须是引擎源码工程；插件由平台声明逐工程选择：" + engine.ProjectFile);
            var result = new List<ProjectSourceInputs> { engine };
            foreach (var plugin in FindPluginProjects(titanRoot, engine.Platform))
            {
                if (string.Equals(plugin.ProjectFile, engine.ProjectFile, comparison)) continue;
                var inputs = LoadFromRoot(titanRoot, plugin.ProjectFile, engine.Platform, engine.Configuration);
                inputs.ImportedFiles = inputs.ImportedFiles.Append(plugin.DescriptorFile).OrderBy(p => p, StringComparer.Ordinal).ToArray();
                result.Add(inputs);
            }
            return result.ToArray();
        }

        private sealed class CommonTargetsAdapter : IDisposable
        {
            public string FilePath { get; }
            private CommonTargetsAdapter(string filePath) { FilePath = filePath; }

            public static CommonTargetsAdapter Create(string root, IEnumerable<string> arguments, string target)
            {
                using var query = JsonDocument.Parse(Run(root, arguments.Concat(new[]
                {
                    "-getProperty:CustomAfterMicrosoftCommonTargets,MSBuildToolsPath"
                })));
                var properties = query.RootElement.GetProperty("Properties");
                var original = properties.GetProperty("CustomAfterMicrosoftCommonTargets").GetString() ?? "";
                if (!string.IsNullOrEmpty(original) && !Path.IsPathFullyQualified(original))
                    original = Path.GetFullPath(Path.Combine(properties.GetProperty("MSBuildToolsPath").GetString()!, original));
                var project = new XElement("Project");
                if (!string.IsNullOrEmpty(original))
                    project.Add(new XElement("Import", new XAttribute("Project", EscapeProperty(original)),
                        new XAttribute("Condition", "Exists('" + EscapeProperty(original).Replace("'", "%27") + "')")));
                project.Add(new XElement("Import", new XAttribute("Project", EscapeProperty(target))));
                var path = Path.Combine(Path.GetTempPath(), "TitanSourceInputs-" + Guid.NewGuid().ToString("N") + ".targets");
                new XDocument(project).Save(path);
                return new CommonTargetsAdapter(path);
            }

            public void Dispose() { File.Delete(FilePath); }
        }

        private static string EscapeProperty(string value)
        {
            return value.Replace("%", "%25").Replace(";", "%3B").Replace(",", "%2C");
        }

        private static string FindRoot()
        {
            foreach (var location in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(location);
                while (directory != null)
                {
                    if (File.Exists(Path.Combine(directory.FullName, "BuildScript", "msbuild", "TitanSourceInputs.targets"))) return directory.FullName;
                    directory = directory.Parent;
                }
            }
            throw new DirectoryNotFoundException("找不到 Titan 根目录；部署工具到仓库或显式传入 TitanRoot");
        }

        private static string Run(string root, IEnumerable<string> arguments)
        {
            var start = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = root, UseShellExecute = false,
                RedirectStandardOutput = true, RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
            };
            foreach (var argument in arguments) start.ArgumentList.Add(argument);
            using var process = Process.Start(start) ?? throw new InvalidOperationException("无法启动 dotnet msbuild");
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(120000))
            {
                process.Kill(true);
                process.WaitForExit();
                throw new TimeoutException("源码求值超时：" + string.Join(" ", start.ArgumentList));
            }
            var text = output.GetAwaiter().GetResult();
            var diagnostics = error.GetAwaiter().GetResult();
            if (process.ExitCode != 0)
                throw new InvalidOperationException($"MSBuild 源码求值失败 ({process.ExitCode})：\n{text}\n{diagnostics}");
            if (!string.IsNullOrWhiteSpace(diagnostics)) Console.Error.WriteLine(diagnostics);
            return text;
        }
    }
}
