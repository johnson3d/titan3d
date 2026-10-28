#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;

namespace CSharpReflectionTools
{
    public sealed record PluginAssembly(string Name, string Path);

    // 只依赖 BCL：参数、平台身份和插件选择必须在加载任何引擎类型之前确定。
    public sealed class ReflectionHostOptions
    {
        public const string WindowsNativeLibraryName = "Core.Window.dll";
        public const string MacNativeLibraryName = "Core.Mac.dylib";
        public const string AndroidImportLibraryName = "Core.Android";
        // Android 反射阶段在 Windows 主机上用 Core.Window.dll 承载导入，因此物理库名不变，只有 P/Invoke 模块名不同。
        public static string NativeLibraryNameFor(string platform) => platform == "Mac" ? MacNativeLibraryName : WindowsNativeLibraryName;
        public static string ImportNameFor(string platform) => platform == "Android" ? AndroidImportLibraryName : NativeLibraryNameFor(platform);
        static readonly string[] Keys = { "TitanRoot", "Platform", "Configuration", "EngineAssembly", "NativeDirectory", "PluginDirectory", "Force" };
        static readonly string[] EngineNames = { "Engine.Window", "Engine.Mac", "Engine.Android" };

        public string TitanRoot { get; private set; } = "";
        public string Platform { get; private set; } = "";
        public string Configuration { get; private set; } = "";
        public string EngineAssembly { get; private set; } = "";
        public string NativeDirectory { get; private set; } = "";
        public string NativeLibrary { get; private set; } = "";
        public string PluginDirectory { get; private set; } = "";
        public bool Force { get; private set; }

        public string EngineAssemblyName => EngineName(Platform);
        public string PluginSuffix => Platform switch { "Mac" => "Mac", "Android" => "Android", _ => "Window" };
        public string GeneratedFile => Path.Combine(TitanRoot, "codegen", Platform, "DataCopyer", "Copyer.gen.cs");
        public string StateFile => Path.Combine(TitanRoot, "codegen", Platform, "DataCopyer", ".datacopyer.state.json");
        // DataCopyer.All 与运行时重建共用的插件引用清单，避免各自按通配符猜测插件。
        public string ReferencesFile => Path.Combine(TitanRoot, "codegen", Platform, "DataCopyer", "Copyer.references");
        public string DataCopyerProject => Path.Combine(TitanRoot, "Plugins", "DataCopyer", "DataCopyer.All", "DataCopyer.All.csproj");
        public string DataCopyerAssembly => Path.Combine(PluginDirectory, "DataCopyer", "DataCopyer.All.dll");

        static string EngineName(string platform) => platform switch
        {
            "Mac" => "Engine.Mac",
            "Android" => "Engine.Android",
            _ => "Engine.Window"
        };

        static string FullPath(string path) => Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

        public static ReflectionHostOptions Parse(string[] args, string hostPlatform, string compiledPlatform)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var arg in args)
            {
                var index = arg.IndexOf('=');
                var key = index > 0 ? arg.Substring(0, index) : arg;
                if (index <= 0 || !Keys.Contains(key, StringComparer.Ordinal)) throw new ArgumentException("未知的反射宿主参数：" + arg);
                if (!values.TryAdd(key, arg.Substring(index + 1))) throw new ArgumentException("重复的反射宿主参数：" + key);
            }
            string Value(string key, string fallback) => values.TryGetValue(key, out var value) && value != "" ? value : fallback;

            var options = new ReflectionHostOptions();
            options.Platform = Value("Platform", hostPlatform);
            if (options.Platform != "Windows" && options.Platform != "Mac" && options.Platform != "Android")
                throw new ArgumentException("Platform 必须是 Windows、Mac 或 Android：" + options.Platform);
            // Android 宿主只编译为 PAndroid 并运行在 Windows 主机上；桌面平台仍要求请求平台、主机与宿主编译平台一致。
            var validHost = options.Platform == "Android"
                ? hostPlatform == "Windows" && compiledPlatform == "Android"
                : options.Platform == hostPlatform && options.Platform == compiledPlatform;
            if (!validHost)
                throw new ArgumentException($"平台组合不合法：{options.Platform}/{hostPlatform}/{compiledPlatform}");
            options.Configuration = Value("Configuration", "Debug");
            if (options.Configuration != "Debug" && options.Configuration != "Release")
                throw new ArgumentException("Configuration 必须是 Debug 或 Release：" + options.Configuration);
            if (!values.ContainsKey("TitanRoot")) throw new ArgumentException("缺少 TitanRoot");
            options.TitanRoot = FullPath(values["TitanRoot"]);
            options.Force = Value("Force", "false") switch
            {
                "true" => true,
                "false" => false,
                var other => throw new ArgumentException("Force 必须是 true 或 false：" + other)
            };

            if (!values.TryGetValue("EngineAssembly", out var engine) || engine == "") throw new ArgumentException("缺少 EngineAssembly；必须由父构建传入精确 TargetPath");
            options.EngineAssembly = FullPath(engine);
            if (!File.Exists(options.EngineAssembly)) throw new ArgumentException("找不到已构建的引擎程序集：" + options.EngineAssembly);
            var engineName = AssemblyName.GetAssemblyName(options.EngineAssembly).Name;
            if (engineName != options.EngineAssemblyName)
                throw new ArgumentException($"Platform={options.Platform} 需要 {options.EngineAssemblyName}，实际引擎为 {engineName}：{options.EngineAssembly}");

            var nativeLibraryName = NativeLibraryNameFor(options.Platform);
            options.NativeDirectory = FullPath(Value("NativeDirectory", Path.Combine(options.TitanRoot, "binaries", options.Configuration.ToLowerInvariant())));
            options.NativeLibrary = Path.Combine(options.NativeDirectory, nativeLibraryName);
            if (!File.Exists(options.NativeLibrary))
                throw new ArgumentException($"反射阶段需要 {options.Configuration} 配置的真实原生库 {nativeLibraryName}：{options.NativeLibrary}");
            if (!IsNativeLibraryFor(options.NativeLibrary, options.Platform))
                throw new ArgumentException($"{options.NativeLibrary} 不是当前进程架构可加载的 {options.Platform} 原生库");
            options.PluginDirectory = FullPath(Value("PluginDirectory", options.Platform == "Android"
                ? Path.Combine(options.TitanRoot, "binaries", "android", "Plugins")
                : Path.Combine(options.TitanRoot, "binaries", "Plugins")));
            return options;
        }

        public bool IsDataCopyerDescriptorCurrent()
        {
            var source = Path.Combine(TitanRoot, "Plugins", "DataCopyer", "DataCopyer.plugin");
            var output = Path.Combine(PluginDirectory, "DataCopyer.plugin");
            return File.Exists(source) && File.Exists(output) && DataCopyerState.Sha256(source) == DataCopyerState.Sha256(output);
        }

        public IDisposable AcquireGenerationGate(TimeSpan timeout)
        {
            var file = Path.Combine(TitanRoot, "codegen", Platform, ".datacopyer.lock");
            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            while (true)
            {
                try { return File.Open(file, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
                catch (IOException) when (watch.Elapsed < timeout) { System.Threading.Thread.Sleep(50); }
            }
        }

        static bool IsNativeLibraryFor(string file, string platform)
        {
            using var stream = File.OpenRead(file);
            using var reader = new BinaryReader(stream);
            if (stream.Length < 4) return false;
            var magic = reader.ReadUInt32();
            if (platform == "Mac")
            {
                var cpu = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => 0x01000007u,
                    Architecture.Arm64 => 0x0100000Cu,
                    _ => 0u
                };
                if (cpu == 0 || stream.Length < 8) return false;
                static uint Swap(uint value) => System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(value);
                if (magic == 0xFEEDFACF) return stream.Length >= 32 && reader.ReadUInt32() == cpu;
                if (magic == 0xCFFAEDFE) return stream.Length >= 32 && Swap(reader.ReadUInt32()) == cpu;
                var bigEndian = magic is 0xBEBAFECA or 0xBFBAFECA;
                if (!bigEndian && magic is not (0xCAFEBABE or 0xCAFEBABF)) return false;
                var count = reader.ReadUInt32();
                if (bigEndian) count = Swap(count);
                var stride = magic is 0xBFBAFECA or 0xCAFEBABF ? 32 : 20;
                if (count == 0 || count > (stream.Length - 8) / stride) return false;
                for (var i = 0u; i < count; i++)
                {
                    stream.Position = 8 + (long)i * stride;
                    var architecture = reader.ReadUInt32();
                    if ((bigEndian ? Swap(architecture) : architecture) == cpu) return true;
                }
                return false;
            }
            if ((magic & 0xFFFF) != 0x5A4D || stream.Length < 0x40) return false;
            stream.Position = 0x3C;
            var header = reader.ReadInt32();
            if (header <= 0 || header + 6 > stream.Length) return false;
            stream.Position = header;
            if (reader.ReadUInt32() != 0x00004550) return false;
            var machine = reader.ReadUInt16();
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => machine == 0x8664,
                Architecture.Arm64 => machine == 0xAA64,
                _ => false
            };
        }

        // 与引擎运行时和插件工程一致：PLTF_ALL 用 .All，其余使用本平台后缀；不以任意 *.dll 兜底。
        public IReadOnlyList<PluginAssembly> SelectPlugins(Action<string> log)
        {
            var root = Path.Combine(TitanRoot, "Plugins");
            var result = new List<PluginAssembly>();
            if (!Directory.Exists(root)) return result;
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var excluded = new[] { "bin", "obj", "binaries", "intermediate" };
            foreach (var file in Directory.EnumerateFiles(root, "*.plugin", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
            {
                var segments = Path.GetRelativePath(root, file).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (segments.Take(segments.Length - 1).Any(s => excluded.Contains(s, StringComparer.OrdinalIgnoreCase))) continue;
                using var descriptor = JsonDocument.Parse(File.ReadAllText(file));
                var json = descriptor.RootElement;
                if (json.TryGetProperty("Enable", out var enabled) && !enabled.GetBoolean()) continue;
                var name = Path.GetFileNameWithoutExtension(file);
                if (!names.Add(name)) throw new InvalidOperationException("重复插件名称：" + name);
                if (string.Equals(name, "DataCopyer", StringComparison.OrdinalIgnoreCase)) continue;
                var platforms = json.GetProperty("Platforms").EnumerateArray().Select(p => p.GetString()).ToArray();
                var all = platforms.Contains("PLTF_ALL", StringComparer.Ordinal);
                var platformId = Platform switch { "Mac" => "PLTF_Mac", "Android" => "PLTF_Android", _ => "PLTF_Windows" };
                if (!all && !platforms.Contains(platformId, StringComparer.Ordinal)) continue;

                var directory = Path.Combine(PluginDirectory, name);
                var expected = Path.Combine(directory, name + "." + (all ? "All" : PluginSuffix) + ".dll");
                if (!File.Exists(expected))
                {
                    var foreign = new[] { "All", "Window", "Mac", "Android" }.Select(s => Path.Combine(directory, name + "." + s + ".dll"))
                        .Where(p => p != expected && File.Exists(p)).ToArray();
                    if (foreign.Length != 0)
                        throw new InvalidOperationException($"插件 {name} 缺少 {Platform} 兼容产物 {expected}，只找到：{string.Join(", ", foreign)}");
                    log($"DataCopyer: 跳过未构建的兼容插件 {name}（缺少 {expected}）");
                    continue;
                }
                using (var pe = new PEReader(File.OpenRead(expected)))
                {
                    var metadata = pe.GetMetadataReader();
                    foreach (var handle in metadata.AssemblyReferences)
                    {
                        var reference = metadata.GetString(metadata.GetAssemblyReference(handle).Name);
                        if (EngineNames.Contains(reference, StringComparer.Ordinal) && reference != EngineAssemblyName)
                            throw new InvalidOperationException($"插件 {name} 引用了 {reference}，与 Platform={Platform} 的 {EngineAssemblyName} 不兼容：{expected}");
                    }
                }
                result.Add(new PluginAssembly(name, expected));
            }
            return result.OrderBy(p => p.Name, StringComparer.Ordinal).ToList();
        }
    }

    public sealed class AssemblyIdentity
    {
        public string Path { get; set; } = "";
        public string Mvid { get; set; } = "";

        public static AssemblyIdentity Read(string path)
        {
            if (!File.Exists(path)) return new AssemblyIdentity { Path = path };
            using var pe = new PEReader(File.OpenRead(path));
            var metadata = pe.GetMetadataReader();
            return new AssemblyIdentity { Path = path, Mvid = metadata.GetGuid(metadata.GetModuleDefinition().Mvid).ToString("N") };
        }

        public bool SameAs(AssemblyIdentity other) => Path == other.Path && Mvid == other.Mvid;
    }

    // 增量旁路记录：同一 hash 在不同平台、配置、引擎或插件身份下不能复用旧产物。
    public sealed class DataCopyerState
    {
        public string Platform { get; set; } = "";
        public string Configuration { get; set; } = "";
        public AssemblyIdentity Engine { get; set; } = new();
        public List<AssemblyIdentity> Plugins { get; set; } = new();
        public string Hash { get; set; } = "";
        public string GeneratedSha256 { get; set; } = "";
        public string BuildInputsSha256 { get; set; } = "";
        public AssemblyIdentity Output { get; set; } = new();

        public static DataCopyerState Create(ReflectionHostOptions options, IReadOnlyList<PluginAssembly> plugins)
        {
            return new DataCopyerState
            {
                Platform = options.Platform,
                Configuration = options.Configuration,
                Engine = AssemblyIdentity.Read(options.EngineAssembly),
                Plugins = plugins.Select(p => AssemblyIdentity.Read(p.Path)).ToList(),
                BuildInputsSha256 = BuildFingerprint(options.TitanRoot)
            };
        }

        public bool SameInputs(DataCopyerState other)
        {
            return Platform == other.Platform && Configuration == other.Configuration && Engine.SameAs(other.Engine)
                && BuildInputsSha256 == other.BuildInputsSha256 && Plugins.Count == other.Plugins.Count && Plugins.Zip(other.Plugins).All(p => p.First.SameAs(p.Second));
        }

        // 插件自身及其公共构建配置也会改变 DataCopyer.dll；明确排除旧 Copyer 和 bin/obj，避免生成输出形成循环。
        static string BuildFingerprint(string root)
        {
            var files = new List<string>();
            foreach (var directory in new[] { Path.Combine(root, "Plugins", "DataCopyer"), Path.Combine(root, "BuildScript") })
            {
                if (!Directory.Exists(directory)) continue;
                files.AddRange(Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Where(file =>
                    !Path.GetRelativePath(directory, file).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                        .Any(part => part.Equals("obj", StringComparison.OrdinalIgnoreCase) || part.Equals("bin", StringComparison.OrdinalIgnoreCase))
                    && Path.GetFileName(file) != "Copyer.gen.cs"
                    && new[] { ".cs", ".csproj", ".projitems", ".props", ".targets" }.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase)));
            }
            foreach (var file in new[] { "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json", "Plugins/Directory.Build.props", "Plugins/Directory.Build.targets" })
            {
                var path = Path.Combine(root, file);
                if (File.Exists(path)) files.Add(path);
            }
            var content = string.Join("\n", files.Distinct(StringComparer.Ordinal).OrderBy(p => p, StringComparer.Ordinal)
                .Select(file => Path.GetRelativePath(root, file).Replace('\\', '/') + ":" + Sha256(file)));
            return Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(content)));
        }

        public static string Sha256(string file) => File.Exists(file) ? Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(file))) : "";

        public static DataCopyerState? Load(string file)
        {
            if (!File.Exists(file)) return null;
            try { return JsonSerializer.Deserialize<DataCopyerState>(File.ReadAllText(file)); }
            catch (JsonException) { return null; }
        }

        public void Save(string file)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file)!);
            var temp = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temp, file, true);
        }
    }
}
