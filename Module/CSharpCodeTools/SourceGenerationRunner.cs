using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CSharpCodeTools;

internal static class SourceGenerationRunner
{
    private sealed class OutputManifest
    {
        public string Fingerprint { get; set; } = "";
        public string ProjectFile { get; set; } = "";
        public string Platform { get; set; } = "";
        public string Configuration { get; set; } = "";
        public SortedDictionary<string, string> Files { get; set; } = new(StringComparer.Ordinal);
    }

    private sealed record PendingOutput(string Directory, string Staging, OutputManifest Previous, OutputManifest Current);

    public static void Run(string root, string project, string platform, string configuration, string config, string[] modes)
    {
        root = Path.GetFullPath(root);
        if (platform != "Windows" && platform != "Mac" && platform != "Android") throw new ArgumentException("Platform 必须是 Windows、Mac 或 Android");
        if (configuration != "Debug" && configuration != "Release") throw new ArgumentException("Configuration 必须是 Debug 或 Release");
        if (modes.Any(m => m != "Rpc" && m != "AutoSync" && m != "Macross")) throw new ArgumentException("未知源码生成模式");
        var outputRoot = Path.Combine(root, "codegen", platform);
        Directory.CreateDirectory(outputRoot);
        using var gate = AcquireGate(Path.Combine(outputRoot, ".source.lock"));
        // 不使用外层 MSBuild 的时间戳跳过求值，宏和 Compile 成员变化始终可见。
        var inputs = ProjectSourceInputs.LoadPlatformProjects(root, project, platform, configuration);
        var excludes = ReadExcludes(config);
        var stagingRoot = Path.Combine(outputRoot, ".staging-" + Guid.NewGuid().ToString("N"));
        var pending = new List<PendingOutput>();
        try
        {
            for (var index = 0; index < inputs.Length; index++)
            {
                var input = inputs[index];
                if (string.IsNullOrWhiteSpace(input.AssemblyName) || input.AssemblyName.IndexOfAny(new[] { '/', '\\', ':' }) >= 0 || input.AssemblyName is "." or "..")
                    throw new InvalidDataException("无效程序集身份：" + input.AssemblyName);
                foreach (var mode in modes.Distinct(StringComparer.Ordinal).OrderBy(m => m, StringComparer.Ordinal))
                {
                    var category = mode == "AutoSync" ? "Property" : mode;
                    var ownership = index == 0 ? "Engine" : Path.Combine("Plugins", input.AssemblyName);
                    var directory = Path.Combine(outputRoot, category, ownership);
                    var previous = ReadManifest(directory);
                    // 共享 RPC 调用方与被调方同次解析，但归调用方宿主编译，单独记录成功清单。
                    var callerDirectory = mode == "Rpc" ? Path.Combine(outputRoot, "Rpc", "Callers", ownership) : null;
                    var previousCallers = callerDirectory == null ? null : ReadManifest(callerDirectory);
                    var fingerprint = Fingerprint(input, mode, config, excludes);
                    Console.WriteLine($"Project={input.ProjectFile}; Platform={platform}; Configuration={configuration}; Assembly={input.AssemblyName}; Mode={mode}; Output={directory}; Sources={input.SourceFiles.Length}");
                    if (IsCurrent(directory, previous, fingerprint) && (callerDirectory == null || IsCurrent(callerDirectory, previousCallers, fingerprint)))
                    {
                        Console.WriteLine("SourceGen: no-op");
                        continue;
                    }
                    var staging = Path.Combine(stagingRoot, category, ownership);
                    Directory.CreateDirectory(staging);
                    var callerStaging = callerDirectory == null ? null : Path.Combine(stagingRoot, "Rpc", "Callers", ownership);
                    if (callerStaging != null) Directory.CreateDirectory(callerStaging);
                    UCodeManagerBase manager = mode switch
                    {
                        "Rpc" => new URpcCodeManager(),
                        "AutoSync" => new PropertyGen.UPropertyCodeManager(),
                        _ => new Macross.UMacrossClassManager()
                    };
                    manager.SetProjectInputs(input);
                    manager.SourceCodes.RemoveAll(path => IsExcluded(path, excludes));
                    switch (mode)
                    {
                        case "Rpc": ((URpcCodeManager)manager).GatherRpcClass(staging, callerStaging); break;
                        case "AutoSync": ((PropertyGen.UPropertyCodeManager)manager).GatherAutoSyncClass(staging); break;
                        default:
                            ((Macross.UMacrossClassManager)manager).GatherMacrossClass(staging);
                            var menu = new Macross.UMacrossContextMenuManager();
                            menu.SetProjectInputs(input);
                            menu.SourceCodes.RemoveAll(path => IsExcluded(path, excludes));
                            menu.GatherClass();
                            // 引擎始终提供菜单入口；插件仅在自身声明菜单时才拥有该生成类型。
                            if (index == 0 || menu.ClassDefines.Count > 0)
                                menu.WriteCode(staging);
                            break;
                    }
                    var current = new OutputManifest { Fingerprint = fingerprint, ProjectFile = input.ProjectFile, Platform = platform, Configuration = configuration };
                    foreach (var file in Directory.GetFiles(staging, "*.cs").OrderBy(p => p, StringComparer.Ordinal))
                        current.Files.Add(Path.GetFileName(file), HashFile(file));
                    pending.Add(new PendingOutput(directory, staging, previous, current));
                    if (callerDirectory != null)
                    {
                        var callers = new OutputManifest { Fingerprint = fingerprint, ProjectFile = input.ProjectFile, Platform = platform, Configuration = configuration };
                        foreach (var file in Directory.GetFiles(callerStaging, "*.cs").OrderBy(p => p, StringComparer.Ordinal))
                            callers.Files.Add(Path.GetFileName(file), HashFile(file));
                        pending.Add(new PendingOutput(callerDirectory, callerStaging, previousCallers, callers));
                    }
                }
            }
            // 所有工程和模式解析成功后才发布、清理旧文件、更新成功清单。
            foreach (var output in pending)
            {
                Directory.CreateDirectory(output.Directory);
                foreach (var (name, hash) in output.Current.Files)
                {
                    var destination = Path.Combine(output.Directory, name);
                    if (!File.Exists(destination) || HashFile(destination) != hash)
                        File.Copy(Path.Combine(output.Staging, name), destination, true);
                }
                foreach (var name in output.Previous.Files.Keys.Except(output.Current.Files.Keys, StringComparer.Ordinal))
                    File.Delete(Path.Combine(output.Directory, name));
                var manifest = JsonSerializer.Serialize(output.Current, new JsonSerializerOptions { WriteIndented = true }) + "\n";
                WriteIfChanged(Path.Combine(output.Directory, ".outputs.json"), manifest);
            }
            // 不再属于当前平台工程集合的插件不能继续向调用方宿主提供类型；只删除清单拥有的文件。
            var pluginCallers = Path.Combine(outputRoot, "Rpc", "Callers", "Plugins");
            if (modes.Contains("Rpc") && Directory.Exists(pluginCallers))
            {
                var comparison = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
                var live = inputs.Skip(1).Select(i => i.AssemblyName).ToHashSet(comparison);
                foreach (var stale in Directory.GetDirectories(pluginCallers).Where(d => !live.Contains(Path.GetFileName(d))))
                {
                    var manifestPath = Path.Combine(stale, ".outputs.json");
                    foreach (var name in ReadManifest(stale).Files.Keys)
                        File.Delete(Path.Combine(stale, name));
                    File.Delete(manifestPath);
                    if (!Directory.EnumerateFileSystemEntries(stale).Any()) Directory.Delete(stale);
                }
            }
        }
        finally
        {
            if (Directory.Exists(stagingRoot)) Directory.Delete(stagingRoot, true);
        }
    }

    public static void ExportInputs(string root, string owner, string platform, string configuration, string projectFile, string listFile, string compiler, bool rpcCallers = false)
    {
        root = Path.GetFullPath(root);
        if (platform is not ("Windows" or "Mac" or "Android") || configuration is not ("Debug" or "Release"))
            throw new ArgumentException("编译输入校验必须指定 Windows/Mac/Android 和 Debug/Release");
        if (string.IsNullOrWhiteSpace(owner) || owner.IndexOfAny(new[] { '/', '\\', ':', '\r', '\n' }) >= 0 || owner is "." or "..")
            throw new ArgumentException("无效生成物归属：" + owner);
        if (compiler is not ("CSharp" or "Cpp")) throw new ArgumentException("Compiler 必须是 CSharp 或 Cpp");
        var outputRoot = Path.Combine(root, "codegen", platform);
        if (!Directory.Exists(outputRoot)) throw new InvalidDataException("缺少平台生成目录：" + outputRoot);
        using var gate = AcquireGate(Path.Combine(outputRoot, ".source.lock"));
        var files = new SortedSet<string>(StringComparer.Ordinal);
        if (compiler == "Cpp" && owner != "Engine") throw new ArgumentException("原生胶水仅由引擎拥有");
        if (owner == "Engine")
        {
            var binder = Path.Combine(outputRoot, "NativeBinder");
            var manifestPath = Path.Combine(binder, compiler == "Cpp" ? "CodeGen.vcxitems" : "CodeGenCSharp.projitems");
            if (!File.Exists(manifestPath)) throw new InvalidDataException("缺少原生胶水清单：" + manifestPath);
            var itemName = compiler == "Cpp" ? "ClCompile" : "Compile";
            // ItemDefinitionGroup 中同名节点是编译默认元数据，不是源码项。
            foreach (var element in XDocument.Load(manifestPath).Descendants().Where(e => e.Name.LocalName == itemName && e.Parent?.Name.LocalName == "ItemGroup"))
            {
                var include = (string)element.Attribute("Include") ?? "";
                const string prefix = "$(MSBuildThisFileDirectory)";
                if (!include.StartsWith(prefix, StringComparison.Ordinal) || include[prefix.Length..].Contains("$(", StringComparison.Ordinal))
                    throw new InvalidDataException("原生胶水清单含不受控项：" + manifestPath + ": " + include);
                var file = Path.GetFullPath(Path.Combine(binder, include[prefix.Length..].Replace('\\', Path.DirectorySeparatorChar)));
                var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                if (!file.StartsWith(binder + Path.DirectorySeparatorChar, comparison)) throw new InvalidDataException("原生胶水路径越界：" + file);
                if (!File.Exists(file)) throw new InvalidDataException("缺少必需生成文件：" + file);
                files.Add(file);
            }
        }
        if (compiler == "CSharp")
        {
            if (string.IsNullOrWhiteSpace(projectFile)) throw new ArgumentException("缺少用于归属校验的 csproj");
            var project = Path.GetFullPath(Path.Combine(root, projectFile));
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var category in new[] { "Rpc", "Property", "Macross" })
            {
                var directory = Path.Combine(outputRoot, category, owner == "Engine" ? "Engine" : Path.Combine("Plugins", owner));
                var manifest = ReadManifest(directory);
                if (manifest.Platform != platform || manifest.Configuration != configuration || !string.Equals(manifest.ProjectFile, project, comparison) || manifest.Fingerprint.Length != 64)
                    throw new InvalidDataException("缺少或不匹配的源码成功清单：" + Path.Combine(directory, ".outputs.json"));
                foreach (var (name, hash) in manifest.Files)
                {
                    var file = Path.Combine(directory, name);
                    if (!File.Exists(file) || HashFile(file) != hash) throw new InvalidDataException("缺少或已损坏的必需生成文件：" + file);
                    files.Add(file);
                }
            }
        }
        if (rpcCallers)
        {
            if (compiler != "CSharp") throw new ArgumentException("RPC 共享调用方只能由托管工程编译");
            // 调用方宿主聚合当前平台所有工程的共享调用方；引擎清单缺失说明未完成本平台生成。
            var callers = Path.Combine(outputRoot, "Rpc", "Callers");
            var plugins = Path.Combine(callers, "Plugins");
            var directories = new[] { Path.Combine(callers, "Engine") }.Concat(Directory.Exists(plugins) ? Directory.GetDirectories(plugins).OrderBy(d => d, StringComparer.Ordinal) : Enumerable.Empty<string>());
            foreach (var directory in directories)
            {
                if (directory != Path.Combine(callers, "Engine") && !File.Exists(Path.Combine(directory, ".outputs.json"))) continue;
                var manifest = ReadManifest(directory);
                if (manifest.Platform != platform || manifest.Configuration != configuration || manifest.Fingerprint.Length != 64)
                    throw new InvalidDataException("缺少或不匹配的 RPC 调用方成功清单：" + Path.Combine(directory, ".outputs.json"));
                foreach (var (name, hash) in manifest.Files)
                {
                    var file = Path.Combine(directory, name);
                    if (!File.Exists(file) || HashFile(file) != hash) throw new InvalidDataException("缺少或已损坏的必需生成文件：" + file);
                    files.Add(file);
                }
            }
        }
        if (files.Any(f => f.Contains('\r') || f.Contains('\n'))) throw new InvalidDataException("生成输入路径含换行符");
        listFile = Path.GetFullPath(listFile ?? throw new ArgumentException("缺少 SourcesList"));
        Directory.CreateDirectory(Path.GetDirectoryName(listFile));
        WriteIfChanged(listFile, string.Join("\n", files) + (files.Count == 0 ? "" : "\n"));
    }

    private static FileStream AcquireGate(string path)
    {
        var clock = Stopwatch.StartNew();
        while (true)
        {
            try { return File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
            catch (IOException) when (clock.Elapsed < TimeSpan.FromMinutes(2)) { System.Threading.Thread.Sleep(100); }
        }
    }

    private static bool IsCurrent(string directory, OutputManifest manifest, string fingerprint) =>
        manifest.Fingerprint == fingerprint && manifest.Files.All(f => File.Exists(Path.Combine(directory, f.Key)) && HashFile(Path.Combine(directory, f.Key)) == f.Value);

    private static OutputManifest ReadManifest(string directory)
    {
        var path = Path.Combine(directory, ".outputs.json");
        if (!File.Exists(path)) return new OutputManifest();
        var manifest = JsonSerializer.Deserialize<OutputManifest>(File.ReadAllText(path)) ?? throw new InvalidDataException("无效生成清单：" + path);
        if (manifest.Files == null || manifest.Files.Keys.Any(name => !Regex.IsMatch(name, @"\A(?:[A-F0-9]{64}\.(?:rpc|prop|macross)\.cs|MacrossContextMenu\.macross\.cs)\z")))
            throw new InvalidDataException("生成清单包含不受控路径：" + path);
        return manifest;
    }

    private static string Fingerprint(ProjectSourceInputs input, string mode, string config, string[] excludes)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        void Add(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            hash.AppendData(BitConverter.GetBytes(bytes.Length));
            hash.AppendData(bytes);
        }
        Add("Titan.SourceGen.v1");
        foreach (var value in new[] { input.ProjectFile, input.AssemblyName, input.Platform, input.Configuration, input.TargetFramework, input.ParseOptions.LanguageVersion.ToString(), mode }) Add(value);
        foreach (var symbol in input.ParseOptions.PreprocessorSymbolNames.OrderBy(s => s, StringComparer.Ordinal)) Add(symbol);
        foreach (var excluded in excludes) Add(excluded);
        foreach (var file in input.SourceFiles.Concat(input.ImportedFiles).Append(typeof(SourceGenerationRunner).Assembly.Location).Concat(string.IsNullOrEmpty(config) ? Array.Empty<string>() : new[] { config }).Distinct(StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal))
        {
            Add(file);
            Add(HashFile(file));
        }
        var templates = Path.Combine(AppContext.BaseDirectory, "Templates");
        if (Directory.Exists(templates))
            foreach (var file in Directory.GetFiles(templates, "*", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
            {
                Add(Path.GetRelativePath(templates, file));
                Add(HashFile(file));
            }
        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static string[] ReadExcludes(string config)
    {
        if (string.IsNullOrEmpty(config)) return Array.Empty<string>();
        var line = File.ReadAllLines(config).FirstOrDefault(l => l.StartsWith("Exclude=", StringComparison.Ordinal));
        return (line?["Exclude=".Length..] ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(p => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(config), p))).OrderBy(p => p, StringComparer.Ordinal).ToArray();
    }

    private static bool IsExcluded(string path, string[] excludes)
    {
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return excludes.Any(excluded => string.Equals(path, excluded, comparison) || path.StartsWith(Path.TrimEndingDirectorySeparator(excluded) + Path.DirectorySeparatorChar, comparison));
    }

    private static string HashFile(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static void WriteIfChanged(string path, string content)
    {
        if (File.Exists(path) && File.ReadAllText(path) == content) return;
        var temporary = path + ".tmp-" + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, content, new UTF8Encoding(false));
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
