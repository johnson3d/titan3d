using System.Diagnostics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using CSharpReflectionTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Titan.Tests.CSharpCodeTools;

[TestFixture]
public class ReflectionHostTests
{
    static string HostPlatform => OperatingSystem.IsMacOS() ? "Mac" : "Windows";

    static string NewDirectory(string name)
    {
        var directory = Path.Combine(Path.GetTempPath(), name + " " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    // 只合成程序集身份和引用关系；不能用真实引擎替代宿主参数和插件选择契约。
    static string Emit(string path, string assemblyName, string source, params string[] references)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var refs = new List<MetadataReference> { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };
        refs.AddRange(references.Select(r => MetadataReference.CreateFromFile(r)));
        var compilation = CSharpCompilation.Create(assemblyName, new[] { CSharpSyntaxTree.ParseText(source) }, refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var result = compilation.Emit(path);
        Assert.That(result.Success, Is.True, string.Join("\n", result.Diagnostics));
        return path;
    }

    // platform 决定原生库的文件名与字节格式；fileName 用于故意让两者不匹配的用例。
    // Android 与 Windows 共用 Core.Window.dll 的 PE 格式，机器类型必须匹配当前宿主架构。
    static void WriteNative(string directory, string platform, string fileName = "")
    {
        Directory.CreateDirectory(directory);
        var bytes = new byte[0x100];
        if (platform == "Mac")
        {
            new byte[] { 0xCF, 0xFA, 0xED, 0xFE }.CopyTo(bytes, 0);
            BitConverter.GetBytes(System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? 0x0100000C : 0x01000007).CopyTo(bytes, 4);
        }
        else
        {
            bytes[0] = (byte)'M'; bytes[1] = (byte)'Z';
            BitConverter.GetBytes(0x80).CopyTo(bytes, 0x3C);
            Encoding.ASCII.GetBytes("PE\0\0").CopyTo(bytes, 0x80);
            BitConverter.GetBytes((ushort)(System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? 0xAA64 : 0x8664)).CopyTo(bytes, 0x84);
        }
        File.WriteAllBytes(Path.Combine(directory, string.IsNullOrEmpty(fileName) ? (platform == "Mac" ? "Core.Mac.dylib" : "Core.Window.dll") : fileName), bytes);
    }

    static (string Root, string Engine) FakeRoot(string platform)
    {
        var root = NewDirectory("Titan Reflection Host");
        var name = platform switch { "Mac" => "Engine.Mac", "Android" => "Engine.Android", _ => "Engine.Window" };
        var engine = Emit(Path.Combine(root, "binaries", "net8.0", name + ".dll"), name, "namespace EngineNS { public class Marker {} }");
        WriteNative(Path.Combine(root, "binaries", "debug"), platform);
        return (root, engine);
    }

    static string[] Args(string root, string platform, string engine, params string[] extra)
        => new[] { "TitanRoot=" + root, "Platform=" + platform, "Configuration=Debug", "EngineAssembly=" + engine }.Concat(extra).ToArray();

    static void WritePlugin(string root, string name, string platforms, bool enable = true)
    {
        var directory = Path.Combine(root, "Plugins", "Group", name);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, name + ".plugin"), "{ \"Enable\": " + (enable ? "true" : "false") + ", \"Platforms\": [" + platforms + "] }");
    }

    [Test]
    public void HostProject_ReferencesBuiltEngineWithoutSharedSourcesOrProjectReferences()
    {
        var project = Path.Combine(ManagedProjectTests.Root(), "Module", "CSharpReflectionTools", "CSharpReflectionTools.csproj");
        Assert.That(File.Exists(project), Is.True, "缺少独立反射宿主工程");
        var xml = XDocument.Load(project);
        Assert.That(xml.Descendants().Where(e => e.Name.LocalName == "Import").Select(e => (string?)e.Attribute("Project") ?? ""), Has.None.Contains(".projitems"));
        Assert.That(xml.Descendants().Where(e => e.Name.LocalName == "ProjectReference"), Is.Empty, "不能经 ProjectReference 重建父引擎");
        foreach (var (platform, configuration) in new[] { ("Windows", "Debug"), ("Mac", "Release"), ("Android", "Debug") })
        {
            var engine = Path.Combine(Path.GetTempPath(), "Engine Probe", platform switch { "Mac" => "Engine.Mac.dll", "Android" => "Engine.Android.dll", _ => "Engine.Window.dll" });
            var result = ManagedProjectTests.Run("msbuild", project, "-nologo", "-p:TitanPlatform=" + platform, "-p:Configuration=" + configuration, "-p:TitanEngineAssemblyPath=" + engine,
                "-getProperty:OutDir,BaseIntermediateOutputPath,AssemblyName", "-getItem:Reference,Compile");
            Assert.That(result.Code, Is.Zero, result.Log);
            using var json = JsonDocument.Parse(result.Log);
            var props = json.RootElement.GetProperty("Properties");
            var outDir = props.GetProperty("OutDir").GetString()!.Replace('\\', '/');
            var objDir = props.GetProperty("BaseIntermediateOutputPath").GetString()!.Replace('\\', '/');
            Assert.That(outDir, Does.EndWith("binaries/Tools/Reflection/" + platform + "/" + configuration + "/net8.0/"));
            Assert.That(objDir, Does.Contain("/CSharpReflectionTools/" + platform + "/"));
            var references = json.RootElement.GetProperty("Items").GetProperty("Reference").EnumerateArray().ToArray();
            Assert.That(references.Any(r => r.TryGetProperty("HintPath", out var hint) && hint.GetString() == engine), Is.True, "宿主必须以普通引用消费父构建传入的真实引擎");
            var compile = json.RootElement.GetProperty("Items").GetProperty("Compile").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
            Assert.That(compile, Has.All.Contains("/Module/CSharpReflectionTools/"), "宿主只编译自身源码");
            Assert.That(compile, Has.None.EndsWith("/TitanAndroidCapabilities.cs").And.None.EndsWith("/TitanMacCapabilities.cs"), "平台能力源码属于引擎工程");
        }
    }

    [TestCase("Platform=Linux")]
    [TestCase("Configuration=Profile")]
    [TestCase("Module=EngineCore")]
    public void Parse_RejectsUnknownArgumentsPlatformsAndConfigurations(string bad)
    {
        var (root, engine) = FakeRoot(HostPlatform);
        try
        {
            var args = Args(root, HostPlatform, engine).Where(a => !a.StartsWith(bad.Split('=')[0] + "=")).Append(bad).ToArray();
            Assert.Throws<ArgumentException>(() => ReflectionHostOptions.Parse(args, HostPlatform, HostPlatform));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestCase("Windows", "Mac", "Windows")]
    [TestCase("Windows", "Windows", "Mac")]
    [TestCase("Mac", "Windows", "Windows")]
    public void Parse_RejectsPlatformOtherThanHostAndCompiledPlatform(string requested, string host, string compiled)
    {
        var (root, engine) = FakeRoot(requested);
        try
        {
            var error = Assert.Throws<ArgumentException>(() => ReflectionHostOptions.Parse(Args(root, requested, engine), host, compiled));
            Assert.That(error!.Message, Does.Contain(requested));
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void Parse_RejectsEngineAssemblyOfAnotherPlatform()
    {
        var (root, _) = FakeRoot("Windows");
        try
        {
            var other = Emit(Path.Combine(root, "binaries", "net8.0", "Engine.Mac.dll"), "Engine.Mac", "namespace EngineNS { public class Marker {} }");
            var error = Assert.Throws<ArgumentException>(() => ReflectionHostOptions.Parse(Args(root, "Windows", other), "Windows", "Windows"));
            Assert.That(error!.Message, Does.Contain("Engine.Mac"));
            Assert.Throws<ArgumentException>(() => ReflectionHostOptions.Parse(Args(root, "Windows", Path.Combine(root, "missing", "Engine.Window.dll")), "Windows", "Windows"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void Parse_RequiresMatchingNativeLibraryForConfiguration()
    {
        var (root, engine) = FakeRoot("Windows");
        try
        {
            var options = ReflectionHostOptions.Parse(Args(root, "Windows", engine), "Windows", "Windows");
            Assert.That(options.NativeLibrary, Is.EqualTo(Path.Combine(root, "binaries", "debug", "Core.Window.dll")));
            Assert.That(options.GeneratedFile, Is.EqualTo(Path.Combine(root, "codegen", "Windows", "DataCopyer", "Copyer.gen.cs")));
            Assert.That(options.DataCopyerAssembly, Is.EqualTo(Path.Combine(root, "binaries", "Plugins", "DataCopyer", "DataCopyer.All.dll")));
            Assert.That(options.ReferencesFile, Is.EqualTo(Path.Combine(root, "codegen", "Windows", "DataCopyer", "Copyer.references")));
            Assert.That(options.EngineAssembly, Is.EqualTo(engine));
            // Release 没有对应配置 native，不能借用 Debug 的库。
            var release = Args(root, "Windows", engine).Select(a => a == "Configuration=Debug" ? "Configuration=Release" : a).ToArray();
            Assert.That(Assert.Throws<ArgumentException>(() => ReflectionHostOptions.Parse(release, "Windows", "Windows"))!.Message, Does.Contain("Core.Window.dll"));
            WriteNative(Path.Combine(root, "binaries", "debug"), "Mac", "Core.Window.dll");
            Assert.That(Assert.Throws<ArgumentException>(() => ReflectionHostOptions.Parse(Args(root, "Windows", engine), "Windows", "Windows"))!.Message, Does.Contain("Windows"));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void Parse_RejectsMacNativeOfAnotherCpuArchitecture(bool fat)
    {
        var (root, engine) = FakeRoot("Mac");
        try
        {
            Assert.DoesNotThrow(() => ReflectionHostOptions.Parse(Args(root, "Mac", engine), "Mac", "Mac"));
            var cpu = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64 ? 0x01000007 : 0x0100000C;
            var bytes = new byte[0x100];
            if (fat)
            {
                new byte[] { 0xCA, 0xFE, 0xBA, 0xBE, 0, 0, 0, 1 }.CopyTo(bytes, 0);
                System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(bytes.AsSpan(8), cpu);
            }
            else
            {
                new byte[] { 0xCF, 0xFA, 0xED, 0xFE }.CopyTo(bytes, 0);
                BitConverter.GetBytes(cpu).CopyTo(bytes, 4);
            }
            File.WriteAllBytes(Path.Combine(root, "binaries", "debug", "Core.Mac.dylib"), bytes);
            Assert.That(Assert.Throws<ArgumentException>(() => ReflectionHostOptions.Parse(Args(root, "Mac", engine), "Mac", "Mac"))!.Message, Does.Contain("架构"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void AndroidEngineProject_CarriesDependencyClosureForReflectionHost()
    {
        // Engine.Android 是库工程：SDK 默认不给库复制包资产，但反射宿主靠“引擎目录”探测依赖，
        // 所以必须像 plugins 那样打开 CopyLocalLockFileAssemblies，否则 GetTypes 会报缺失程序集。
        var project = Path.Combine(ManagedProjectTests.Root(), "Module", "Engine.Android", "Engine.Android.csproj");
        var result = ManagedProjectTests.Run("msbuild", project, "-nologo", "-p:TitanPlatform=Android", "-getProperty:CopyLocalLockFileAssemblies,OutDir,AssemblyName");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var props = json.RootElement.GetProperty("Properties");
        Assert.That(props.GetProperty("AssemblyName").GetString(), Is.EqualTo("Engine.Android"));
        Assert.That(props.GetProperty("OutDir").GetString()!.Replace('\\', '/'), Does.EndWith("binaries/android/net8.0/"));
        Assert.That(props.GetProperty("CopyLocalLockFileAssemblies").GetString(), Is.EqualTo("true"), "反射宿主加载的引擎必须自带依赖闭包");
    }

    [Test]
    [Platform("Win")]
    public void AndroidHost_ConsumesAndroidEngineAndWindowsNative()
    {
        var (root, engine) = FakeRoot("Android");
        try
        {
            var options = ReflectionHostOptions.Parse(Args(root, "Android", engine), "Windows", "Android");
            Assert.That(options.EngineAssemblyName, Is.EqualTo("Engine.Android"));
            Assert.That(options.NativeLibrary, Does.EndWith("Core.Window.dll"));
            Assert.That(ReflectionHostOptions.ImportNameFor("Android"), Is.EqualTo("Core.Android"));
            // Android 只在 Windows 主机上、以 Android 编译的宿主运行；其余组合仍必须拒绝。
            Assert.That(() => ReflectionHostOptions.Parse(Args(root, "Android", engine), "Mac", "Android"), Throws.ArgumentException);
            Assert.That(() => ReflectionHostOptions.Parse(Args(root, "Android", engine), "Windows", "Windows"), Throws.ArgumentException);
            Assert.That(() => ReflectionHostOptions.Parse(Args(root, "Android", engine), "Windows", "Mac"), Throws.ArgumentException);
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void AndroidPlugins_UseAndroidSuffixPlatformDeclarationAndPluginDirectory()
    {
        var (root, engine) = FakeRoot("Android");
        try
        {
            var plugins = Path.Combine(root, "binaries", "android", "Plugins");
            string Plugin(string name, string file)
                => Emit(Path.Combine(plugins, name, file), Path.GetFileNameWithoutExtension(file), "namespace P { public class " + name + " {} }");
            WritePlugin(root, "Alpha", "\"PLTF_ALL\""); Plugin("Alpha", "Alpha.All.dll");
            WritePlugin(root, "Beta", "\"PLTF_Android\""); Plugin("Beta", "Beta.Android.dll");
            WritePlugin(root, "Gamma", "\"PLTF_Windows\""); Plugin("Gamma", "Gamma.Window.dll");
            WritePlugin(root, "Delta", "\"PLTF_Mac\""); Plugin("Delta", "Delta.Mac.dll");
            WritePlugin(root, "DataCopyer", "\"PLTF_ALL\""); Plugin("DataCopyer", "DataCopyer.All.dll");

            var options = ReflectionHostOptions.Parse(Args(root, "Android", engine), "Windows", "Android");
            Assert.That(options.PluginSuffix, Is.EqualTo("Android"));
            Assert.That(options.PluginDirectory, Is.EqualTo(plugins), "Android 默认插件目录必须与桌面隔离");
            Assert.That(options.DataCopyerAssembly, Is.EqualTo(Path.Combine(plugins, "DataCopyer", "DataCopyer.All.dll")));
            Assert.That(options.SelectPlugins(_ => { }).Select(p => p.Name), Is.EqualTo(new[] { "Alpha", "Beta" }));

            // 描述符也必须跟随 Android 插件目录，不能复用桌面已部署的副本。
            Assert.That(options.IsDataCopyerDescriptorCurrent(), Is.False);
            var source = Path.Combine(root, "Plugins", "DataCopyer", "DataCopyer.plugin");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            Directory.CreateDirectory(plugins);
            File.WriteAllText(source, "{\"Enable\":true}");
            File.Copy(source, Path.Combine(plugins, "DataCopyer.plugin"));
            Assert.That(options.IsDataCopyerDescriptorCurrent(), Is.True);
        }
        finally { Directory.Delete(root, true); }
    }

    [TestCase("Engine.Window")]
    [TestCase("Engine.Mac")]
    public void SelectPlugins_RejectsAndroidPluginBuiltAgainstDesktopEngine(string engineName)
    {
        var (root, engine) = FakeRoot("Android");
        try
        {
            var desktop = Emit(Path.Combine(root, "other", engineName + ".dll"), engineName, "namespace EngineNS { public class Marker {} }");
            WritePlugin(root, "Alpha", "\"PLTF_ALL\"");
            Emit(Path.Combine(root, "binaries", "android", "Plugins", "Alpha", "Alpha.All.dll"), "Alpha.All", "namespace P { public class Alpha : EngineNS.Marker {} }", desktop);
            var options = ReflectionHostOptions.Parse(Args(root, "Android", engine), "Windows", "Android");
            var error = Assert.Throws<InvalidOperationException>(() => options.SelectPlugins(_ => { }));
            Assert.That(error!.Message, Does.Contain("Alpha").And.Contain(engineName));
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void AndroidDataCopyer_BindsGeneratedHashAndOutputToAndroidEngine()
    {
        var root = ManagedProjectTests.Root();
        var stateFile = Path.Combine(root, "codegen", "Android", "DataCopyer", ".datacopyer.state.json");
        var generated = Path.Combine(root, "codegen", "Android", "DataCopyer", "Copyer.gen.cs");
        var output = Path.Combine(root, "binaries", "android", "Plugins", "DataCopyer", "DataCopyer.All.dll");
        if (!File.Exists(stateFile) || !File.Exists(generated) || !File.Exists(output))
            Assert.Ignore("需要先完成 Android S1：msbuild Engine.Android.csproj -t:TitanRunDataCopyerStage -p:TitanPlatform=Android");

        var state = DataCopyerState.Load(stateFile)!;
        Assert.That(state.Platform, Is.EqualTo("Android"));
        Assert.That(state.Engine.Path.Replace('\\', '/'), Does.EndWith("binaries/android/net8.0/Engine.Android.dll"));
        Assert.That(File.ReadLines(generated).First(), Does.Contain("DataCopyerHash=" + state.Hash), "生成文件必须来自同一份 metadata");

        using var pe = new PEReader(File.OpenRead(output));
        var metadata = pe.GetMetadataReader();
        var references = metadata.AssemblyReferences.Select(h => metadata.GetString(metadata.GetAssemblyReference(h).Name)).ToArray();
        Assert.That(references, Does.Contain("Engine.Android"), "DataCopyer 产物必须引用真实的 Android 引擎");
        Assert.That(references, Does.Not.Contain("Engine.Window").And.Not.Contain("Engine.Mac"), "DataCopyer 产物不能引用桌面引擎");
    }

    [Test]
    [Platform("Win")]
    public void AndroidDataCopyer_OfflineHashProbeMatchesState()
    {
        var root = ManagedProjectTests.Root();
        var engine = Path.Combine(root, "binaries", "android", "net8.0", "Engine.Android.dll");
        var native = Path.Combine(root, "binaries", "debug", "Core.Window.dll");
        var host = Path.Combine(root, "binaries", "Tools", "Reflection", "Android", "Debug", "net8.0", "CSharpReflectionTools.dll");
        var stateFile = Path.Combine(root, "codegen", "Android", "DataCopyer", ".datacopyer.state.json");
        if (!File.Exists(engine) || !File.Exists(native) || !File.Exists(host) || !File.Exists(stateFile))
            Assert.Ignore("需要先完成 Android S1 构建，才有可校验的宿主与 state");
        var state = DataCopyerState.Load(stateFile)!;

        var directory = NewDirectory("Titan S1 Probe");
        try
        {
            File.WriteAllText(Path.Combine(directory, "S1Probe.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework><Nullable>disable</Nullable><ImplicitUsings>disable</ImplicitUsings><AssemblyName>S1Probe</AssemblyName></PropertyGroup><ItemGroup><Reference Include=\"CSharpReflectionTools\"><HintPath>" + host + "</HintPath></Reference></ItemGroup></Project>");
            // 独立进程、只走 BCL 与反射：不调用 GenCode，与生成器不共用进程，避免用生成器自己的结果自证。
            File.WriteAllText(Path.Combine(directory, "Program.cs"), """
                using System;
                using System.Reflection;
                using System.Runtime.Loader;
                using CSharpReflectionTools;

                static class Probe
                {
                    // 宿主内部成员不公开：探针按名字反射，保持与生成器同一条注册路径而不复制实现。
                    static object Invoke(Type type, string name, params object[] args)
                    {
                        var flags = BindingFlags.NonPublic | BindingFlags.Static;
                        foreach (var method in type.GetMethods(flags))
                        {
                            if (method.Name != name || method.GetParameters().Length != args.Length) continue;
                            try { return method.Invoke(null, args); } catch (ArgumentException) { }
                        }
                        throw new MissingMethodException(type.FullName + "." + name);
                    }

                    static int Main(string[] args)
                    {
                        var options = ReflectionHostOptions.Parse(args, "Windows", "Android");
                        var plugins = options.SelectPlugins(_ => { });
                        var host = typeof(ReflectionHostOptions).Assembly;
                        var program = host.GetType("CSharpReflectionTools.Program")!;
                        var generator = host.GetType("CSharpReflectionTools.DataCopyerGenerator")!;
                        var engine = (Assembly)Invoke(program, "InstallLoadRules", options, plugins);
                        // 与生成器一致：先登记插件类型与别名，再登记引擎，最后读历史 metadata。
                        var manager = engine.GetType("EngineNS.Rtti.TtTypeDescManager")!.GetProperty("Instance")!.GetValue(null)!;
                        var init = manager.GetType().GetMethod("InitAssembly", new[] { engine.GetType("EngineNS.TtEngine"), typeof(Assembly) })!;
                        foreach (PluginAssembly plugin in plugins)
                        {
                            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(plugin.Path);
                            Invoke(program, "RegisterNativeResolver", assembly, options);
                            init.Invoke(manager, new object[] { null, assembly });
                            Invoke(generator, "RegisterPluginTypeAliases", plugin.Name, assembly);
                        }
                        Invoke(generator, "RegisterEngineAssemblies", engine);
                        Invoke(generator, "LoadMetas", options.TitanRoot.Replace('\\', '/') + "/");
                        var hash = engine.GetType("EngineNS.Bricks.DataCopyer.TtDataCopyer")!.GetMethod("CalcVersionHash")!.Invoke(null, null)!.ToString();
                        var state = DataCopyerState.Load(options.StateFile)!;
                        if (hash != state.Hash) throw new Exception($"独立哈希不匹配：{hash}/{state.Hash}");
                        Console.WriteLine("S1 OfflineHash=" + hash);
                        return 0;
                    }
                }
                """);

            var build = ManagedProjectTests.Run("build", Path.Combine(directory, "S1Probe.csproj"), "-c", "Debug", "--nologo", "-v", "q");
            Assert.That(build.Code, Is.Zero, build.Log);

            var pluginDirectory = Path.Combine(root, "binaries", "android", "Plugins");
            var run = ManagedProjectTests.Run(Path.Combine(directory, "bin", "Debug", "net8.0", "S1Probe.dll"),
                "TitanRoot=" + root, "Platform=Android", "Configuration=Debug", "EngineAssembly=" + engine,
                "NativeDirectory=" + Path.GetDirectoryName(native), "PluginDirectory=" + pluginDirectory);
            Assert.That(run.Code, Is.Zero, run.Log);
            Assert.That(run.Log, Does.Contain("S1 OfflineHash=" + state.Hash), "独立进程必须复算出同一 VersionHash");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void State_TracksDataCopyerBuildSourcesAndImportsButNotBuildOutputs()
    {
        var (root, engine) = FakeRoot("Windows");
        try
        {
            var options = ReflectionHostOptions.Parse(Args(root, "Windows", engine), "Windows", "Windows");
            var directory = Path.Combine(root, "Plugins", "DataCopyer", "DataCopyer");
            Directory.CreateDirectory(directory);
            var source = Path.Combine(directory, "Plugin.cs");
            File.WriteAllText(source, "class Plugin {}");
            var state = DataCopyerState.Create(options, Array.Empty<PluginAssembly>());
            File.WriteAllText(source, "class Plugin { int Changed; }");
            var changed = DataCopyerState.Create(options, Array.Empty<PluginAssembly>());
            Assert.That(changed.SameInputs(state), Is.False, "DataCopyer 自身修改必须重建");
            Directory.CreateDirectory(Path.Combine(directory, "obj"));
            File.WriteAllText(Path.Combine(directory, "obj", "Temp.cs"), "生成物");
            Assert.That(DataCopyerState.Create(options, Array.Empty<PluginAssembly>()).SameInputs(changed), Is.True);
            Directory.CreateDirectory(Path.Combine(root, "BuildScript", "msbuild"));
            File.WriteAllText(Path.Combine(root, "BuildScript", "msbuild", "TitanManagedProject.props"), "<Project />");
            Assert.That(DataCopyerState.Create(options, Array.Empty<PluginAssembly>()).SameInputs(changed), Is.False, "导入变化必须重建");
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void ReflectionOptions_RequiresMatchingDeployedDataCopyerDescriptor()
    {
        var (root, engine) = FakeRoot("Windows");
        try
        {
            var options = ReflectionHostOptions.Parse(Args(root, "Windows", engine), "Windows", "Windows");
            bool Current() => options.IsDataCopyerDescriptorCurrent();
            Assert.That(Current(), Is.False);
            var source = Path.Combine(root, "Plugins", "DataCopyer", "DataCopyer.plugin");
            var output = Path.Combine(root, "binaries", "Plugins", "DataCopyer.plugin");
            Directory.CreateDirectory(Path.GetDirectoryName(source)!);
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            File.WriteAllText(source, "{\"Enable\":true}");
            Assert.That(Current(), Is.False, "缺失部署必须自愈");
            File.Copy(source, output);
            Assert.That(Current(), Is.True);
            File.WriteAllText(source, "{\"Enable\":false}");
            Assert.That(Current(), Is.False, "描述符变更必须重新部署");
            File.Copy(source, output, true);
            Assert.That(Current(), Is.True);
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void ReflectionGate_SerializesTheSamePlatformAndReleasesAfterFailure()
    {
        var (root, engine) = FakeRoot("Windows");
        try
        {
            var options = ReflectionHostOptions.Parse(Args(root, "Windows", engine), "Windows", "Windows");
            var method = typeof(ReflectionHostOptions).GetMethod("AcquireGenerationGate");
            Assert.That(method, Is.Not.Null, "反射生成和子构建全过程必须持有平台写锁");
            using (var gate = (IDisposable)method!.Invoke(options, new object[] { TimeSpan.FromSeconds(1) })!)
            {
                var error = Assert.Throws<System.Reflection.TargetInvocationException>(() => method.Invoke(options, new object[] { TimeSpan.Zero }));
                Assert.That(error!.InnerException, Is.TypeOf<IOException>());
            }
            using var released = (IDisposable)method!.Invoke(options, new object[] { TimeSpan.Zero })!;
            Assert.That(File.ReadAllText(Path.Combine(ManagedProjectTests.Root(), "Module", "CSharpReflectionTools", "Program.cs")), Does.Contain("using var gate = options.AcquireGenerationGate("));
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void SelectPlugins_FollowsPlatformDeclarationsWithoutWildcardFallback()
    {
        var (root, engine) = FakeRoot("Windows");
        try
        {
            var plugins = Path.Combine(root, "binaries", "Plugins");
            string Plugin(string name, string file, string? engineRef = null)
                => Emit(Path.Combine(plugins, name, file), Path.GetFileNameWithoutExtension(file), "namespace P { public class " + name + (engineRef != null ? " : EngineNS.Marker" : "") + " {} }", engineRef == null ? Array.Empty<string>() : new[] { engineRef });
            WritePlugin(root, "Alpha", "\"PLTF_ALL\""); Plugin("Alpha", "Alpha.All.dll", engine);
            WritePlugin(root, "Beta", "\"PLTF_Windows\""); Plugin("Beta", "Beta.Window.dll");
            WritePlugin(root, "Gamma", "\"PLTF_Mac\""); Plugin("Gamma", "Gamma.Mac.dll");
            WritePlugin(root, "Delta", "\"PLTF_ALL\"", enable: false); Plugin("Delta", "Delta.All.dll");
            WritePlugin(root, "Epsilon", "\"PLTF_ALL\"");
            WritePlugin(root, "Zeta", "\"PLTF_ALL\""); Plugin("Zeta", "Zeta.Tools.dll");
            WritePlugin(root, "DataCopyer", "\"PLTF_ALL\""); Plugin("DataCopyer", "DataCopyer.All.dll");
            var log = new List<string>();
            var options = ReflectionHostOptions.Parse(Args(root, "Windows", engine), "Windows", "Windows");
            var selected = options.SelectPlugins(log.Add);
            Assert.That(selected.Select(p => p.Name), Is.EqualTo(new[] { "Alpha", "Beta" }));
            Assert.That(selected[0].Path, Is.EqualTo(Path.Combine(plugins, "Alpha", "Alpha.All.dll")));
            Assert.That(log, Has.Some.Contains("Epsilon").And.Some.Contains("Zeta"), "缺失的兼容插件需要提示");

            WritePlugin(root, "Eta", "\"PLTF_Windows\""); Plugin("Eta", "Eta.Mac.dll");
            Assert.That(Assert.Throws<InvalidOperationException>(() => options.SelectPlugins(log.Add))!.Message, Does.Contain("Eta"), "只有其他平台产物时必须失败");
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void SelectPlugins_RejectsPluginBuiltAgainstAnotherPlatformEngine()
    {
        var (root, engine) = FakeRoot("Windows");
        try
        {
            var macEngine = Emit(Path.Combine(root, "other", "Engine.Mac.dll"), "Engine.Mac", "namespace EngineNS { public class Marker {} }");
            WritePlugin(root, "Alpha", "\"PLTF_ALL\"");
            Emit(Path.Combine(root, "binaries", "Plugins", "Alpha", "Alpha.All.dll"), "Alpha.All", "namespace P { public class Alpha : EngineNS.Marker {} }", macEngine);
            var options = ReflectionHostOptions.Parse(Args(root, "Windows", engine), "Windows", "Windows");
            var error = Assert.Throws<InvalidOperationException>(() => options.SelectPlugins(_ => { }));
            Assert.That(error!.Message, Does.Contain("Alpha").And.Contain("Engine.Mac"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void State_TracksPlatformConfigurationEngineAndPluginIdentity()
    {
        var (root, engine) = FakeRoot("Windows");
        try
        {
            var options = ReflectionHostOptions.Parse(Args(root, "Windows", engine), "Windows", "Windows");
            var state = DataCopyerState.Create(options, Array.Empty<PluginAssembly>());
            Assert.That(state.Engine.Mvid, Is.Not.Empty);
            var file = Path.Combine(root, "state.json");
            state.Hash = "H"; state.GeneratedSha256 = "G";
            state.Save(file);
            var loaded = DataCopyerState.Load(file)!;
            Assert.That(loaded.SameInputs(state), Is.True);
            Assert.That(loaded.Hash, Is.EqualTo("H"));

            var release = DataCopyerState.Load(file)!; release.Configuration = "Release";
            Assert.That(release.SameInputs(state), Is.False, "配置切换必须重建");
            var mac = DataCopyerState.Load(file)!; mac.Platform = "Mac";
            Assert.That(mac.SameInputs(state), Is.False, "不同平台同 hash 不能复用");
            Emit(engine, "Engine.Window", "namespace EngineNS { public class Marker { public int Changed; } }");
            Assert.That(DataCopyerState.Create(options, Array.Empty<PluginAssembly>()).SameInputs(state), Is.False, "引擎身份变化必须重建");
            var plugin = Emit(Path.Combine(root, "binaries", "Plugins", "Alpha", "Alpha.All.dll"), "Alpha.All", "namespace P { public class Alpha {} }");
            Assert.That(DataCopyerState.Create(options, new[] { new PluginAssembly("Alpha", plugin) }).SameInputs(DataCopyerState.Create(options, Array.Empty<PluginAssembly>())), Is.False);
            Assert.That(DataCopyerState.Load(Path.Combine(root, "missing.json")), Is.Null);
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void SourceTargets_NoLongerRunDataCopyerAndPostBuildStageUsesExactEngine()
    {
        var scripts = Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild");
        var codegen = File.ReadAllText(Path.Combine(scripts, "TitanCodeGen.targets"));
        Assert.That(codegen, Does.Not.Contain("mode=DataCopyer"), "源码生成阶段不能加载引擎");
        Assert.That(codegen, Does.Contain("TitanDataCopyer.targets"));
        var stage = File.ReadAllText(Path.Combine(scripts, "TitanDataCopyer.targets"));
        Assert.That(stage, Does.Contain("EngineAssembly=&quot;$(TargetPath)&quot;").And.Contain("TitanEngineAssemblyPath=&quot;$(TargetPath)&quot;"));
        Assert.That(stage, Does.Contain("SkipTitanCodeGen=true").And.Contain("SkipTitanDataCopyerGen=true"));
        Assert.That(stage, Does.Not.Contain("ProjectReference"));
        var generator = File.ReadAllText(Path.Combine(ManagedProjectTests.Root(), "Module", "CSharpReflectionTools", "DataCopyerGenerator.cs"));
        Assert.That(generator, Does.Contain("\"-p:OutDir=\" + Path.GetDirectoryName(options.DataCopyerAssembly)"), "子构建输出应遵守父传入的插件目录");
        Assert.That(generator, Does.Contain("\"-p:TtOutputBasePath=\" + Path.GetDirectoryName(options.DataCopyerAssembly)"), "插件 Pre/PostBuild 必须使用同一输出目录，不能清空默认部署");
        Assert.That(stage, Does.Contain("NativeDirectory=&quot;$(TitanReflectionNativeDirectory)&quot;").And.Contain("PluginDirectory=&quot;$(TitanReflectionPluginDirectory)&quot;"), "父构建应显式传入实际 native 与插件输出目录");

        var directory = NewDirectory("Titan DataCopyer Stage");
        try
        {
            var project = Path.Combine(directory, "Engine.Probe.proj");
            File.WriteAllText(project, "<Project><PropertyGroup><TitanEnginePlatform>Windows</TitanEnginePlatform><TargetPath>" + Path.Combine(directory, "missing", "Engine.Window.dll") + "</TargetPath><Configuration>Debug</Configuration></PropertyGroup><Import Project=\"" + Path.Combine(scripts, "TitanPlatform.props") + "\" /><Import Project=\"" + Path.Combine(scripts, "TitanDataCopyer.targets") + "\" /></Project>");
            var result = ManagedProjectTests.Run("msbuild", project, "-nologo", "-t:TitanRunDataCopyerStage", "-p:TitanRoot=" + directory + Path.DirectorySeparatorChar, "-p:TitanPlatform=" + HostPlatform);
            Assert.That(result.Code, Is.Not.Zero, result.Log);
            Assert.That(result.Log, Does.Contain("Engine.Window.dll"));
            Assert.That(Directory.Exists(Path.Combine(directory, "codegen")), Is.False);
        }
        finally { Directory.Delete(directory, true); }
    }

    [TestCase("Windows", "Window")]
    [TestCase("Mac", "Mac")]
    public void CodegenPaths_AreNormalizedAndFallbackSourceProjectMatchesPlatform(string platform, string suffix)
    {
        var directory = NewDirectory("Titan Codegen Paths");
        try
        {
            var project = Path.Combine(directory, "Probe.proj");
            File.WriteAllText(project, "<Project><Import Project=\"" + Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanCodeGen.targets") + "\" /></Project>");
            var result = ManagedProjectTests.Run("msbuild", project, "-nologo", "-p:TitanPlatform=" + platform, "-getProperty:_TitanSourceProject,_CSharpCodeToolsDll");
            Assert.That(result.Code, Is.Zero, result.Log);
            using var json = JsonDocument.Parse(result.Log);
            var props = json.RootElement.GetProperty("Properties");
            Assert.That(props.GetProperty("_TitanSourceProject").GetString()!.Replace('\\', '/'), Does.EndWith("/Module/Engine." + suffix + "/Engine." + suffix + ".csproj"));
            Assert.That(props.GetProperty("_CSharpCodeToolsDll").GetString()!.Replace('\\', '/'), Is.EqualTo(Path.Combine(ManagedProjectTests.Root(), "binaries", "Tools", "net8.0", "CSharpCodeTools.dll").Replace('\\', '/')));
        }
        finally { Directory.Delete(directory, true); }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void PluginCmdDeployment_QuotesSourceOutputAndIntermediatePathsWithSpaces(bool forwardSlashes)
    {
        if (!OperatingSystem.IsWindows()) Assert.Ignore("Windows cmd 部署分支");
        var directory = Path.Combine(ManagedProjectTests.Root(), "intermediate", "migration-tests", "cmd_" + Guid.NewGuid().ToString("N"));
        var projectDirectory = Path.Combine(directory, "Source Files", "Probe.All");
        var output = Path.Combine(directory, "Custom Plugins", "Probe");
        var intermediate = Path.Combine(directory, "Intermediate Files");
        Directory.CreateDirectory(projectDirectory);
        Directory.CreateDirectory(output);
        Directory.CreateDirectory(Path.Combine(intermediate, "Probe.All"));
        try
        {
            var descriptorText = "{\"Enable\":true,\"Platforms\":[\"PLTF_ALL\"]}";
            File.WriteAllText(Path.Combine(directory, "Source Files", "Probe.plugin"), descriptorText);
            File.WriteAllText(Path.Combine(output, "old.txt"), "旧部署");
            var project = Path.Combine(projectDirectory, "Probe.All.proj");
            new XDocument(new XElement("Project",
                new XElement("PropertyGroup",
                    new XElement("ProjectDir", projectDirectory + Path.DirectorySeparatorChar),
                    new XElement("TtTitanRoot", directory + Path.DirectorySeparatorChar),
                    new XElement("TtOutputBasePath", forwardSlashes ? output.Replace('\\', '/') + "/" : output + Path.DirectorySeparatorChar),
                    new XElement("TtIntermediateBasePath", forwardSlashes ? intermediate.Replace('\\', '/') + "/" : intermediate + Path.DirectorySeparatorChar)),
                new XElement("Import", new XAttribute("Project", Path.Combine(ManagedProjectTests.Root(), "Plugins", "Directory.Build.targets"))))).Save(project);
            (int Code, string Log) RunTarget(string target)
            {
                // 所有未引用的旧命令相对路径也限定在独立探针目录，避免红灯影响真实部署。
                var start = new ProcessStartInfo("dotnet") { WorkingDirectory = directory, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
                foreach (var argument in new[] { "msbuild", project, "-nologo", "-t:" + target }) start.ArgumentList.Add(argument);
                using var process = Process.Start(start)!;
                var stdout = process.StandardOutput.ReadToEndAsync();
                var stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(60000)) { process.Kill(true); throw new TimeoutException("插件 cmd 部署超时"); }
                return (process.ExitCode, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
            }
            var pre = RunTarget("PreBuild");
            Assert.That(pre.Code, Is.Zero, pre.Log);
            Assert.That(File.ReadAllText(Path.Combine(directory, "Custom Plugins", "Probe.plugin")), Is.EqualTo(descriptorText));
            Assert.That(File.Exists(Path.Combine(output, "old.txt")), Is.False);
            File.WriteAllText(Path.Combine(output, "Probe.All.pdb"), "符号");
            var intermediatePdb = Path.Combine(intermediate, "Probe.All", "Probe.pdb");
            File.WriteAllText(intermediatePdb, "中间符号");
            var post = RunTarget("PostBuild");
            Assert.That(post.Code, Is.Zero, post.Log);
            Assert.That(File.ReadAllText(Path.Combine(output, "Probe.All.tpdb")), Is.EqualTo("符号"));
            Assert.That(File.Exists(intermediatePdb), Is.False);
        }
        finally { Directory.Delete(directory, true); }
    }

    [TestCase("Windows", false)]
    [TestCase("Mac", false)]
    [TestCase("Windows", true)]
    [TestCase("Mac", true)]
    public void DataCopyerDescriptor_PortableCopyFollowsPluginOutputDirectory(string platform, bool custom)
    {
        var directory = NewDirectory("Titan DataCopyer Deployment");
        try
        {
            var project = Path.Combine(ManagedProjectTests.Root(), "Plugins", "DataCopyer", "DataCopyer.All", "DataCopyer.All.csproj");
            var arguments = new List<string>
            {
                "msbuild", project, "-nologo", "-t:TtCopyPluginDescriptor", "-p:TtUseCmdShell=false",
                "-p:TitanPlatform=" + platform, "-p:TitanRoot=" + directory + Path.DirectorySeparatorChar
            };
            var pluginRoot = Path.Combine(directory, custom ? "Custom Plugins" : "binaries/Plugins");
            if (custom)
            {
                var output = Path.Combine(pluginRoot, "DataCopyer") + Path.DirectorySeparatorChar;
                arguments.Add("-p:OutDir=" + output);
                arguments.Add("-p:TtOutputBasePath=" + output);
            }
            var result = ManagedProjectTests.Run(arguments.ToArray());
            Assert.That(result.Code, Is.Zero, result.Log);
            var descriptor = Path.Combine(pluginRoot, "DataCopyer.plugin");
            Assert.That(File.Exists(descriptor), Is.True, "描述符必须与父传入的插件根目录一致");
            Assert.That(File.ReadAllBytes(descriptor), Is.EqualTo(File.ReadAllBytes(Path.Combine(ManagedProjectTests.Root(), "Plugins", "DataCopyer", "DataCopyer.plugin"))));
            if (custom)
                Assert.That(Directory.Exists(Path.Combine(directory, "binaries", "Plugins")), Is.False, "自定义部署不能写默认插件目录");
        }
        finally { Directory.Delete(directory, true); }
    }

    [TestCase("Windows")]
    [TestCase("Mac")]
    public void DataCopyerPlugin_CompilesPlatformGeneratedCopyerOnly(string platform)
    {
        var project = Path.Combine(ManagedProjectTests.Root(), "Plugins", "DataCopyer", "DataCopyer.All", "DataCopyer.All.csproj");
        var result = ManagedProjectTests.Run("msbuild", project, "-nologo", "-p:TitanPlatform=" + platform, "-getItem:Compile");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var files = json.RootElement.GetProperty("Items").GetProperty("Compile").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
        Assert.That(files, Has.Some.EndsWith("/codegen/" + platform + "/DataCopyer/Copyer.gen.cs"));
        Assert.That(files, Has.None.EndsWith("/Plugins/DataCopyer/DataCopyer/Copyer.gen.cs"));
        Assert.That(files.Where(f => f.EndsWith("/Copyer.gen.cs")).Count(), Is.EqualTo(1));

        var directory = NewDirectory("Titan DataCopyer Missing");
        try
        {
            var missing = ManagedProjectTests.Run("msbuild", project, "-nologo", "-t:TitanValidateDataCopyerGenerated", "-p:TitanPlatform=" + platform, "-p:TitanCodeGenRoot=" + Path.Combine(directory, "codegen", platform) + Path.DirectorySeparatorChar);
            Assert.That(missing.Code, Is.Not.Zero, missing.Log);
            Assert.That(missing.Log, Does.Contain("Copyer.gen.cs").And.Contain("TitanRunDataCopyerStage"));
        }
        finally { Directory.Delete(directory, true); }
    }
}
