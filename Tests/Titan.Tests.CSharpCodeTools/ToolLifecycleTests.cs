using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace Titan.Tests.CSharpCodeTools;

[TestFixture]
public class ToolLifecycleTests
{
    internal static (int Code, string Log) Run(string directory, params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8, StandardErrorEncoding = Encoding.UTF8
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(120000))
        {
            process.Kill(true);
            process.WaitForExit();
            throw new TimeoutException("工具子进程超时");
        }
        return (process.ExitCode, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
    }

    [Test]
    public void Cli_IsolatesPlatformAndPluginOutputsAndRepairsMissingFilesWithoutTouchingSources()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan CLI Generation " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "BuildScript", "msbuild"));
        try
        {
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(directory, "BuildScript", "msbuild", "TitanSourceInputs.targets"));
            var config = Path.Combine(directory, "Rpc_Engine.txt");
            File.WriteAllText(config, "Include=NotAnInputDirectory\nExclude=\nTarget=codegen/Rpc/Engine\nMacross_Target=codegen/Macross/Engine\nProperty_Target=codegen/Property/Engine\n");
            foreach (var platform in new[] { "Window", "Mac" })
            {
                var engine = Path.Combine(directory, "Module", "Engine." + platform);
                Directory.CreateDirectory(engine);
                File.WriteAllText(Path.Combine(engine, "Engine." + platform + ".csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><DefineConstants>$(DefineConstants);" + (platform == "Mac" ? "PMac" : "PWindow") + "</DefineConstants></PropertyGroup></Project>");
                File.WriteAllText(Path.Combine(engine, "Item.cs"), "namespace EngineNS.Test; public partial class Item {\n#if PMac\n[Meta] public void Mac() {}\n#else\n[Meta] public void Windows() {}\n#endif\n}");
            }
            var plugin = Path.Combine(directory, "Plugins", "Portable", "Portable.All");
            Directory.CreateDirectory(plugin);
            File.WriteAllText(Path.Combine(directory, "Plugins", "Portable", "Portable.plugin"), "{\"Enable\":true,\"Platforms\":[\"PLTF_ALL\"]}");
            File.WriteAllText(Path.Combine(plugin, "Portable.All.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><DefineConstants>$(DefineConstants);PLUGIN_ONLY</DefineConstants></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(plugin, "Own.cs"), "namespace PluginNS; public partial class Own {\n#if PLUGIN_ONLY\n[Meta] public void Plugin() {}\n#endif\n}");
            var tool = ManagedProjectTests.Tool();
            (int Code, string Log) Generate(string platform) => Run(directory, tool, config, "mode=Macross", "TitanRoot=" + directory, "Platform=" + platform, "Configuration=Debug");
            var windows = Generate("Windows");
            Assert.That(windows.Code, Is.Zero, windows.Log);
            var windowsRoot = Path.Combine(directory, "codegen", "Windows");
            var before = Directory.GetFiles(windowsRoot, "*.cs", SearchOption.AllDirectories).ToDictionary(p => p, File.ReadAllText);
            var mac = Generate("Mac");
            Assert.That(mac.Code, Is.Zero, mac.Log);
            foreach (var (file, code) in before) Assert.That(File.ReadAllText(file), Is.EqualTo(code));
            var engineOutput = Path.Combine(directory, "codegen", "Mac", "Macross", "Engine");
            var pluginOutput = Path.Combine(directory, "codegen", "Mac", "Macross", "Plugins", "Portable.All");
            var engineCode = string.Join("\n", Directory.GetFiles(engineOutput, "*.cs").Select(File.ReadAllText));
            Assert.That(engineCode, Does.Contain("macross_Mac").And.Not.Contain("macross_Windows").And.Not.Contain("macross_Plugin"));
            Assert.That(string.Join("\n", Directory.GetFiles(pluginOutput, "*.cs").Select(File.ReadAllText)), Does.Contain("macross_Plugin"));
            // ContextMenu 只属于引擎；未声明菜单的插件不能多出生成类型。
            Assert.That(File.Exists(Path.Combine(engineOutput, "MacrossContextMenu.macross.cs")), Is.True);
            Assert.That(File.Exists(Path.Combine(pluginOutput, "MacrossContextMenu.macross.cs")), Is.False);
            var owned = Directory.GetFiles(engineOutput, "*.cs").Single(p => File.ReadAllText(p).Contains("macross_Mac"));
            var timestamp = File.GetLastWriteTimeUtc(owned);
            var again = Generate("Mac");
            Assert.That(again.Code, Is.Zero, again.Log);
            Assert.That(File.GetLastWriteTimeUtc(owned), Is.EqualTo(timestamp));
            File.Delete(owned);
            var repaired = Generate("Mac");
            Assert.That(repaired.Code, Is.Zero, repaired.Log);
            Assert.That(File.Exists(owned), Is.True);
            var untouched = Path.Combine(engineOutput, "User.cs");
            File.WriteAllText(untouched, "// 非工具拥有的文件");
            var source = Path.Combine(directory, "Module", "Engine.Mac", "Item.cs");
            var original = File.ReadAllText(source);
            File.WriteAllText(source, original.Replace("[Meta]", ""));
            var removed = Generate("Mac");
            Assert.That(removed.Code, Is.Zero, removed.Log);
            Assert.That(File.Exists(owned), Is.False, "上轮拥有但本轮无声明的输出应清理");
            Assert.That(File.ReadAllText(untouched), Is.EqualTo("// 非工具拥有的文件"));
            var manifest = Path.Combine(engineOutput, ".outputs.json");
            var savedManifest = File.ReadAllBytes(manifest);
            File.WriteAllText(source, "public class Broken {");
            var failed = Generate("Mac");
            Assert.That(failed.Code, Is.Not.Zero);
            Assert.That(File.ReadAllBytes(manifest), Is.EqualTo(savedManifest), "解析失败不能更新成功清单");
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void Cli_RejectsPluginAsEngineOverrideWithoutPublishingEngineOutputs()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan CLI Ownership " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "BuildScript", "msbuild"));
        try
        {
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(directory, "BuildScript", "msbuild", "TitanSourceInputs.targets"));
            var plugin = Path.Combine(directory, "Plugins", "Portable", "Portable.All");
            Directory.CreateDirectory(plugin);
            File.WriteAllText(Path.Combine(directory, "Plugins", "Portable", "Portable.plugin"), "{\"Enable\":true,\"Platforms\":[\"PLTF_ALL\"]}");
            var project = Path.Combine(plugin, "Portable.All.csproj");
            File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(plugin, "Own.cs"), "namespace PluginNS; public partial class Own { [Meta] public void Plugin() {} }");
            var tool = ManagedProjectTests.Tool();
            var result = Run(directory, tool, "mode=Macross", "TitanRoot=" + directory, "Platform=Mac", "csproj=" + project);
            Assert.That(result.Code, Is.Not.Zero, result.Log);
            Assert.That(result.Log, Does.Contain(project));
            Assert.That(Directory.GetFiles(directory, "*.macross.cs", SearchOption.AllDirectories), Is.Empty);
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void PluginTargets_ConsumeOnlyItsOwnedSourcesWithoutEngineBinder()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Plugin Consumption " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "BuildScript", "msbuild"));
        var plugin = Path.Combine(directory, "Plugins", "Portable", "Portable.All");
        Directory.CreateDirectory(plugin);
        try
        {
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(directory, "BuildScript", "msbuild", "TitanSourceInputs.targets"));
            var engine = Path.Combine(directory, "Synthetic.csproj");
            File.WriteAllText(engine, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup><Compile Include=\"Engine.cs\" /></ItemGroup></Project>");
            File.WriteAllText(Path.Combine(directory, "Engine.cs"), "namespace EngineNS; public partial class EngineItem { [Meta] public void EngineCall() {} }");
            File.WriteAllText(Path.Combine(directory, "Plugins", "Portable", "Portable.plugin"), "{\"Enable\":true,\"Platforms\":[\"PLTF_ALL\"]}");
            var project = Path.Combine(plugin, "Portable.All.csproj");
            var target = Path.Combine(ManagedProjectTests.Root(), "Plugins", "Directory.Build.targets");
            File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><TitanPlatform>Mac</TitanPlatform><SkipTitanCodeGen>true</SkipTitanCodeGen></PropertyGroup><Import Project=\"" + target + "\" /></Project>");
            var source = Path.Combine(plugin, "Own.cs");
            File.WriteAllText(source, "namespace PluginNS; public partial class Own { [Meta] public void PluginCall() {} }");
            global::CSharpCodeTools.SourceGenerationRunner.Run(directory, engine, "Mac", "Debug", null!, new[] { "Rpc", "AutoSync", "Macross" });
            var tool = ManagedProjectTests.Tool();
            var args = new[] { "msbuild", project, "-nologo", "-t:TitanConsumeManagedGeneratedInputs", "-getItem:Compile", "-p:TitanRoot=" + directory, "-p:_CSharpCodeToolsDll=" + tool };
            var result = Run(directory, args);
            Assert.That(result.Code, Is.Zero, result.Log);
            using var json = System.Text.Json.JsonDocument.Parse(result.Log[result.Log.IndexOf('{')..]);
            var files = json.RootElement.GetProperty("Items").GetProperty("Compile").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!).ToArray();
            Assert.That(files, Does.Contain(source));
            var generated = files.Where(p => p.EndsWith(".macross.cs")).ToArray();
            Assert.That(generated, Is.Not.Empty);
            Assert.That(generated.All(p => p.Contains(Path.Combine("Macross", "Plugins", "Portable.All"))), Is.True);
            Assert.That(string.Join("\n", generated.Select(File.ReadAllText)), Does.Contain("macross_PluginCall").And.Not.Contain("macross_EngineCall"));
            Assert.That(files.Any(p => p.Contains("NativeBinder")), Is.False);
            var direct = Run(directory, args.Append("-p:SkipTitanCodeGen=false").ToArray());
            Assert.That(direct.Code, Is.Zero, "插件消费不得启动父引擎/native 生成：" + direct.Log);
            File.Delete(generated.First());
            var missing = Run(directory, args);
            Assert.That(missing.Code, Is.Not.Zero, missing.Log);
            Assert.That(missing.Log, Does.Contain(Path.GetFileName(generated.First())));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void Rpc_SharedCallersAreOwnedByCallerHostAndRemovedWithTheirProject()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Rpc Callers " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "BuildScript", "msbuild"));
        try
        {
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(directory, "BuildScript", "msbuild", "TitanSourceInputs.targets"));
            var engine = Path.Combine(directory, "Synthetic.csproj");
            File.WriteAllText(engine, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup><Compile Include=\"Engine.cs\" /></ItemGroup></Project>");
            File.WriteAllText(Path.Combine(directory, "Engine.cs"), "namespace EngineNS; [TtRpcClass(CallerInClass=true)] public partial class Local { [TtRpcMethod(Index=1)] public void Ping(TtCallContext context) {} }");
            var plugin = Path.Combine(directory, "Plugins", "Portable", "Portable.All");
            Directory.CreateDirectory(plugin);
            var descriptor = Path.Combine(directory, "Plugins", "Portable", "Portable.plugin");
            File.WriteAllText(descriptor, "{\"Enable\":true,\"Platforms\":[\"PLTF_ALL\"]}");
            File.WriteAllText(Path.Combine(plugin, "Portable.All.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(plugin, "Remote.cs"), "using PluginOnly;\nnamespace PluginNS; [TtRpcClass] public partial class Remote { [TtRpcMethod(Index=1)] public void Call(int value, TtCallContext context) {} }");
            void Generate() => global::CSharpCodeTools.SourceGenerationRunner.Run(directory, engine, "Mac", "Debug", null!, new[] { "Rpc", "AutoSync", "Macross" });
            Generate();
            var rpc = Path.Combine(directory, "codegen", "Mac", "Rpc");
            string Code(string path) => string.Join("\n", Directory.GetFiles(path, "*.cs").Select(File.ReadAllText));
            // CallerInClass=false 的调用方由共享调用方宿主编译，被调方仍归拥有源码的插件。
            var callers = Path.Combine(rpc, "Callers", "Plugins", "Portable.All");
            Assert.That(Code(callers), Does.Contain("public partial class Remote_RpcCaller").And.Not.Contain("rpc_Call").And.Not.Contain("using PluginOnly;"));
            var owner = Code(Path.Combine(rpc, "Plugins", "Portable.All"));
            Assert.That(owner, Does.Contain("rpc_Call").And.Contain("Remote_RpcCaller.Call(").And.Not.Contain("class Remote_RpcCaller"));
            var engineCode = Code(Path.Combine(rpc, "Engine"));
            Assert.That(engineCode, Does.Contain("class Local_RpcCaller").And.Contain("rpc_Ping"));
            Assert.That(Directory.Exists(Path.Combine(rpc, "Callers", "Engine")) ? Directory.GetFiles(Path.Combine(rpc, "Callers", "Engine"), "*.cs") : Array.Empty<string>(), Is.Empty);
            var list = Path.Combine(directory, "obj", "callers.inputs");
            var tool = ManagedProjectTests.Tool();
            (int Code, string Log) Export(params string[] extra) => Run(directory, new[] { tool, "ValidateOutputs=true", "SourceOwner=Portable.All", "Compiler=CSharp", "SourcesList=" + list, "TitanRoot=" + directory, "Platform=Mac", "Configuration=Debug", "csproj=" + Path.Combine(plugin, "Portable.All.csproj") }.Concat(extra).ToArray());
            var plain = Export();
            Assert.That(plain.Code, Is.Zero, plain.Log);
            Assert.That(File.ReadAllLines(list), Has.None.Contains(Path.Combine("Rpc", "Callers")));
            var host = Export("RpcCallers=true");
            Assert.That(host.Code, Is.Zero, host.Log);
            Assert.That(File.ReadAllLines(list).Where(p => p.Contains(Path.Combine("Rpc", "Callers"))), Is.EquivalentTo(Directory.GetFiles(callers, "*.cs")));
            var callerFile = Directory.GetFiles(callers, "*.cs").Single();
            File.WriteAllText(callerFile, "// 被篡改");
            Assert.That(Export("RpcCallers=true").Code, Is.Not.Zero, "调用方聚合必须校验清单哈希");
            Generate();
            Assert.That(File.ReadAllText(callerFile), Does.Contain("Remote_RpcCaller"), "调用方输出缺失或损坏时必须自愈");
            File.WriteAllText(descriptor, "{\"Enable\":true,\"Platforms\":[\"PLTF_Windows\"]}");
            Generate();
            Assert.That(File.Exists(callerFile), Is.False, "已不属于当前平台的工程不能继续向调用方宿主提供类型");
        }
        finally { Directory.Delete(directory, true); }
    }

    [TestCase("Rpc", "public partial class Item { [TtRpcMethod(Index=1)] public void Call(TtCallContext context) {} }")]
    [TestCase("Rpc", "[TtRpcClass] public partial class Item { [TtRpcMethod] public void Call(TtCallContext context) {} }")]
    [TestCase("Rpc", "[TtRpcClass] public partial class Item { [TtRpcMethod(Index=1)] public void Call() {} }")]
    [TestCase("Rpc", "[TtRpcClass] public partial class Item { [TtRpcMethod(Index=1)] public void Call(int context) {} }")]
    [TestCase("Rpc", "public partial class Outer { [TtRpcClass] public partial class Item { [TtRpcMethod(Index=1)] public void Call(TtCallContext context) {} } }")]
    [TestCase("AutoSync", "[UAutoSync] public partial class Item { [UAutoSync] public int Value { get; set; } }")]
    public void SourceGen_RejectsInvalidContractsAndKeepsPublishedOutputs(string mode, string invalid)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Generation Gate " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "BuildScript", "msbuild"));
        try
        {
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(directory, "BuildScript", "msbuild", "TitanSourceInputs.targets"));
            var project = Path.Combine(directory, "Synthetic.csproj");
            File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
            var source = Path.Combine(directory, "Item.cs");
            File.WriteAllText(source, "namespace Test; [TtRpcClass(RunTarget=ERunTarget.Client,Executer=EExecuter.Client)] [UAutoSync] public partial class Item { [TtRpcMethod(Index=1)] public void Call(TtCallContext context) {} [UAutoSync(Index=0)] public int Value { get; set; } }");
            void Generate() => global::CSharpCodeTools.SourceGenerationRunner.Run(directory, project, "Mac", "Debug", null!, new[] { mode });
            Generate();
            var output = Path.Combine(directory, "codegen", "Mac", mode == "AutoSync" ? "Property" : mode, "Engine");
            var saved = Directory.GetFiles(output).ToDictionary(p => p, File.ReadAllBytes);
            File.WriteAllText(source, "namespace Test; " + invalid);
            Assert.That(Generate, Throws.TypeOf<InvalidDataException>().With.Message.Contains(source));
            foreach (var (file, content) in saved) Assert.That(File.ReadAllBytes(file), Is.EqualTo(content), file);
            Assert.That(Directory.GetFiles(output), Is.EquivalentTo(saved.Keys));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void Cli_ExportsOnlyOwnedCompilerInputsAndRejectsMissingOutputOrWrongConfiguration()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Compiler Inputs " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "BuildScript", "msbuild"));
        try
        {
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(directory, "BuildScript", "msbuild", "TitanSourceInputs.targets"));
            var project = Path.Combine(directory, "Synthetic.csproj");
            File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(directory, "Item.cs"), "namespace Test; public partial class Item { [Meta] public void Call() {} }");
            global::CSharpCodeTools.SourceGenerationRunner.Run(directory, project, "Mac", "Debug", null!, new[] { "Rpc", "AutoSync", "Macross" });
            var binder = Path.Combine(directory, "codegen", "Mac", "NativeBinder");
            Directory.CreateDirectory(binder);
            File.WriteAllText(Path.Combine(binder, "Type.cpp2cs.cs"), "namespace Native { public struct Type {} }");
            File.WriteAllText(Path.Combine(binder, "CodeGenCSharp.projitems"), "<Project><ItemGroup><Compile Include=\"$(MSBuildThisFileDirectory)Type.cpp2cs.cs\" /></ItemGroup></Project>");
            var generated = Path.Combine(directory, "codegen", "Mac", "Macross", "Engine");
            File.WriteAllText(Path.Combine(generated, "User.cs"), "class NotOwned {}");
            var list = Path.Combine(directory, "obj", "compile.inputs");
            var tool = ManagedProjectTests.Tool();
            (int Code, string Log) Export(string config) => Run(directory, tool, "ValidateOutputs=true", "SourceOwner=Engine", "Compiler=CSharp", "SourcesList=" + list, "TitanRoot=" + directory, "Platform=Mac", "Configuration=" + config, "csproj=" + project);
            var first = Export("Debug");
            Assert.That(first.Code, Is.Zero, first.Log);
            var sources = File.ReadAllLines(list);
            Assert.That(sources, Has.Some.EndsWith("Type.cpp2cs.cs"));
            Assert.That(sources, Has.None.EndsWith("User.cs"));
            Assert.That(sources, Has.Some.EndsWith(".macross.cs"));
            var original = File.ReadAllBytes(list);
            var wrong = Export("Release");
            Assert.That(wrong.Code, Is.Not.Zero);
            Assert.That(File.ReadAllBytes(list), Is.EqualTo(original));
            File.Delete(Path.Combine(binder, "Type.cpp2cs.cs"));
            var missing = Export("Debug");
            Assert.That(missing.Code, Is.Not.Zero);
            Assert.That(missing.Log, Does.Contain("Type.cpp2cs.cs"));
            Assert.That(File.ReadAllBytes(list), Is.EqualTo(original));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void BuildTargets_ConsumeFirstGenerationWithoutReevaluationAndRejectMissingOwnedFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Dynamic Inputs " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(directory, "BuildScript", "msbuild"));
        try
        {
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(directory, "BuildScript", "msbuild", "TitanSourceInputs.targets"));
            var project = Path.Combine(directory, "Synthetic.csproj");
            var tool = ManagedProjectTests.Tool();
            var target = Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanCodeGen.targets");
            File.WriteAllText(project, $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><TitanPlatform>Mac</TitanPlatform><TitanEnginePlatform>Mac</TitanEnginePlatform><SkipTitanCodeGen>true</SkipTitanCodeGen></PropertyGroup><Import Project=\"{target}\" /><Target Name=\"FixtureFirstGeneration\" BeforeTargets=\"TitanConsumeManagedGeneratedInputs\" Condition=\"'$(FixtureGenerate)' == 'true'\"><Exec Command=\"dotnet &amp;quot;{tool}&amp;quot; TitanRoot=&amp;quot;{directory}&amp;quot; Platform=Mac Configuration=Debug csproj=&amp;quot;{project}&amp;quot;\" /></Target></Project>".Replace("&amp;quot;", "&quot;"));
            File.WriteAllText(Path.Combine(directory, "Item.cs"), "namespace Test; public partial class Item { [Meta] public void Call() {} }");
            var binder = Path.Combine(directory, "codegen", "Mac", "NativeBinder");
            Directory.CreateDirectory(binder);
            File.WriteAllText(Path.Combine(binder, "Type.cpp2cs.cs"), "struct Type {}");
            File.WriteAllText(Path.Combine(binder, "CodeGenCSharp.projitems"), "<Project><ItemGroup><Compile Include=\"$(MSBuildThisFileDirectory)Type.cpp2cs.cs\" /></ItemGroup></Project>");
            var common = new[] { "msbuild", project, "-nologo", "-p:TitanRoot=" + directory, "-p:_CSharpCodeToolsDll=" + tool, "-t:TitanConsumeManagedGeneratedInputs", "-getItem:Compile" };
            var first = Run(directory, common.Append("-p:FixtureGenerate=true").ToArray());
            Assert.That(first.Code, Is.Zero, first.Log);
            var jsonStart = first.Log.IndexOf('{');
            using var json = System.Text.Json.JsonDocument.Parse(first.Log[jsonStart..]);
            var files = json.RootElement.GetProperty("Items").GetProperty("Compile").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!).ToArray();
            Assert.That(files, Has.Some.EndsWith("Type.cpp2cs.cs"));
            Assert.That(files, Has.Some.EndsWith(".macross.cs"));
            Assert.That(files.Distinct(StringComparer.OrdinalIgnoreCase).Count(), Is.EqualTo(files.Length));
            File.Delete(files.Single(p => p.EndsWith(".macross.cs") && !p.EndsWith("MacrossContextMenu.macross.cs")));
            var missing = Run(directory, common);
            Assert.That(missing.Code, Is.Not.Zero, missing.Log);
        }
        finally { Directory.Delete(directory, true); }
    }

    [TestCase(false, false)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void BuildTargets_ConsumeNativeFirstGenerationAndRejectMissingOwnedFile(bool heterogeneousMetadata, bool realTemplate)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Native Inputs " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var project = Path.Combine(directory, "Probe.proj");
            var binder = Path.Combine(directory, "codegen", "Windows", "NativeBinder");
            var tool = ManagedProjectTests.Tool();
            var targets = Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanCodeGen.targets");
            Directory.CreateDirectory(binder);
            var cpp = Path.Combine(binder, "Type.cpp2cs.cpp");
            File.WriteAllText(Path.Combine(directory, "Ordinary.cpp"), "// 普通源码");
            var ns = System.Xml.Linq.XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");
            var xml = new System.Xml.Linq.XDocument(new System.Xml.Linq.XElement(ns + "Project",
                new System.Xml.Linq.XElement(ns + "PropertyGroup",
                    new System.Xml.Linq.XElement(ns + "TitanPlatform", "Windows"),
                    new System.Xml.Linq.XElement(ns + "TitanConsumeNativeOutputs", "true"),
                    new System.Xml.Linq.XElement(ns + "SkipTitanCodeGen", "true"),
                    new System.Xml.Linq.XElement(ns + "Configuration", "Debug"),
                    new System.Xml.Linq.XElement(ns + "IntDir", "obj/Debug/")),
                new System.Xml.Linq.XElement(ns + "ItemGroup",
                    new System.Xml.Linq.XElement(ns + "ClCompile", new System.Xml.Linq.XAttribute("Include", "Ordinary.cpp"))),
                new System.Xml.Linq.XElement(ns + "Import", new System.Xml.Linq.XAttribute("Project", targets)),
                new System.Xml.Linq.XElement(ns + "Target", new System.Xml.Linq.XAttribute("Name", "FixtureFirstGeneration"),
                    new System.Xml.Linq.XAttribute("BeforeTargets", "TitanConsumeNativeGeneratedInputs"),
                    new System.Xml.Linq.XAttribute("Condition", "'$(FixtureGenerate)' == 'true'"),
                    new System.Xml.Linq.XElement(ns + "WriteLinesToFile", new System.Xml.Linq.XAttribute("File", cpp), new System.Xml.Linq.XAttribute("Lines", "// 新生成源码")),
                    new System.Xml.Linq.XElement(ns + "WriteLinesToFile", new System.Xml.Linq.XAttribute("File", Path.Combine(binder, "CodeGen.vcxitems")),
                        new System.Xml.Linq.XAttribute("Lines", "<Project><ItemGroup><ClCompile Include=\"$(MSBuildThisFileDirectory)Type.cpp2cs.cpp\" /></ItemGroup></Project>".Replace("$", "%24"))))));
            if (realTemplate)
            {
                var template = System.Xml.Linq.XDocument.Load(Path.Combine(ManagedProjectTests.Root(), "codegen", "Templates", "Empty_CodeGen.vcxitems"));
                template.Root!.Add(new System.Xml.Linq.XElement(ns + "ItemGroup",
                    new System.Xml.Linq.XElement(ns + "ClCompile", new System.Xml.Linq.XAttribute("Include", "$(MSBuildThisFileDirectory)Type.cpp2cs.cpp"))));
                template.Save(Path.Combine(binder, "CodeGen.vcxitems"));
                File.WriteAllText(cpp, "// 真实模板消费");
                xml.Root!.Elements(ns + "Target").Single().Remove();
            }
            if (heterogeneousMetadata)
            {
                var group = xml.Root!.Element(ns + "ItemGroup")!;
                group.Element(ns + "ClCompile")!.Add(new System.Xml.Linq.XAttribute("AdditionalIncludeDirectories", "IncA"));
                group.Add(new System.Xml.Linq.XElement(ns + "ClCompile", new System.Xml.Linq.XAttribute("Include", "Other.cpp"),
                    new System.Xml.Linq.XAttribute("AdditionalIncludeDirectories", "IncB")));
                File.WriteAllText(Path.Combine(directory, "Other.cpp"), "// 普通源码独立配置");
            }
            xml.Save(project);
            var args = new[] { "msbuild", project, "-nologo", "-p:TitanRoot=" + directory, "-p:_CSharpCodeToolsDll=" + tool, "-t:TitanConsumeNativeGeneratedInputs", "-getItem:ClCompile" };
            var first = Run(directory, args.Append("-p:FixtureGenerate=true").ToArray());
            Assert.That(first.Code, Is.Zero, first.Log);
            using var json = System.Text.Json.JsonDocument.Parse(first.Log[first.Log.IndexOf('{')..]);
            var items = json.RootElement.GetProperty("Items").GetProperty("ClCompile").EnumerateArray().ToArray();
            var generated = items.Single(i => i.GetProperty("FullPath").GetString() == cpp);
            Assert.That(generated.GetProperty("AdditionalIncludeDirectories").GetString(), Does.Contain(binder));
            Assert.That(items, Has.Length.EqualTo(heterogeneousMetadata ? 3 : 2));
            var again = Run(directory, args);
            Assert.That(again.Code, Is.Zero, again.Log);
            File.Delete(cpp);
            var missing = Run(directory, args);
            Assert.That(missing.Code, Is.Not.Zero, missing.Log);
            Assert.That(missing.Log, Does.Contain("Type.cpp2cs.cpp"));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Platform("Win")]
    [TestCase("Mac", "")]
    [TestCase("Unknown", "")]
    [TestCase("Windows", "elsewhere")]
    public void WindowsNativeEntry_RejectsPlatformOrOutputRootMismatchBeforeToolBuild(string platform, string overridePath)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Native Entry " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var project = Path.Combine(directory, "Probe.proj");
            File.WriteAllText(project, "<Project><Import Project=\"" + Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanCodeGen.targets") + "\" /></Project>");
            var args = new List<string> { "msbuild", project, "-nologo", "-t:_ValidateTitanCodeGenHost", "-p:TitanRoot=" + directory, "-p:TitanPlatform=" + platform };
            if (overridePath != "") args.Add("-p:TitanNativeBinderDirectory=" + Path.Combine(directory, overridePath));
            var result = Run(directory, args.ToArray());
            Assert.That(result.Code, Is.Not.Zero, result.Log);
            Assert.That(result.Log, Does.Not.Contain("Building CppWeavingTools"));
            Assert.That(Directory.Exists(Path.Combine(directory, "codegen")), Is.False);
            if (overridePath != "")
            {
                var consume = Run(directory, "msbuild", project, "-nologo", "-t:TitanConsumeNativeGeneratedInputs",
                    "-p:TitanRoot=" + directory, "-p:TitanPlatform=Windows", "-p:SkipTitanCodeGen=true",
                    "-p:TitanConsumeNativeOutputs=true", "-p:TitanNativeBinderDirectory=" + Path.Combine(directory, overridePath));
                Assert.That(consume.Code, Is.Not.Zero);
                Assert.That(consume.Log, Does.Contain("不允许覆盖到其他输出根"));
            }
        }
        finally { Directory.Delete(directory, true); }
    }

    // 只替代昂贵的外部构建与环境发现，运行真实 build.ps1 的函数、参数和循环。
    static (string File, string[] Args)[] RunWindowsBuildScript(string directory, params string[] options)
    {
        var scripts = Path.Combine(directory, "BuildScript");
        Directory.CreateDirectory(Path.Combine(scripts, "common"));
        Directory.CreateDirectory(Path.Combine(directory, "Plugins"));
        var tools = Path.Combine(directory, "binaries", "Tools", "net8.0");
        Directory.CreateDirectory(tools);
        File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "build.ps1"), Path.Combine(scripts, "build.ps1"));
        foreach (var name in new[] { "CppWeavingTools", "CSharpCodeTools" })
            File.WriteAllText(Path.Combine(tools, name + ".exe"), "外部编译在此编排测试中仅记录参数");
        File.WriteAllText(Path.Combine(scripts, "common", "common.ps1"), """
            function Assert-WindowsHost { }
            function Get-RepoRoot { Split-Path (Split-Path $PSScriptRoot -Parent) -Parent }
            function Find-DotNet { 'dotnet' }
            function Find-NuGet { 'nuget' }
            function Find-Vs2022MsBuild { 'msbuild' }
            function Import-VsDevEnvironment { param($Path) }
            function Test-RepoFile { param($Project) Join-Path (Get-RepoRoot) $Project }
            function Invoke-External {
                param([string]$File, [string[]]$Arguments, [string]$WorkingDirectory)
                @{ File=$File; Arguments=@($Arguments) } | ConvertTo-Json -Compress |
                    Add-Content -LiteralPath (Join-Path (Get-RepoRoot) 'trace.jsonl') -Encoding UTF8
            }
            """, Encoding.UTF8);
        var start = new ProcessStartInfo("powershell.exe")
        {
            WorkingDirectory = directory, UseShellExecute = false,
            RedirectStandardOutput = true, RedirectStandardError = true
        };
        foreach (var arg in new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", Path.Combine(scripts, "build.ps1"), "-SkipDependencyCheck", "-SkipRestore", "-SkipNative" }.Concat(options))
            start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(60000)) { process.Kill(true); Assert.Fail("Windows 编排子进程超时"); }
        Assert.That(process.ExitCode, Is.Zero, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
        return File.ReadAllLines(Path.Combine(directory, "trace.jsonl")).Select(line =>
        {
            using var json = System.Text.Json.JsonDocument.Parse(line);
            return (File: json.RootElement.GetProperty("File").GetString()!, Args: json.RootElement.GetProperty("Arguments").EnumerateArray().Select(a => a.GetString()!).ToArray());
        }).ToArray();
    }

    [Platform("Win")]
    [TestCase(false)]
    [TestCase(true)]
    public void WindowsBuild_GeneratesEachConfigurationBeforeItsManagedBuild(bool codeGenOnly)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Build Sequence " + Guid.NewGuid().ToString("N"));
        try
        {
            var options = new List<string> { "-Configuration", "All", "-SkipPlugins" };
            if (codeGenOnly) options.Add("-CodeGenOnly");
            var events = RunWindowsBuildScript(directory, options.ToArray());
            Assert.That(events, Has.None.Matches<(string File, string[] Args)>(e => e.Args.Contains("-t:TitanRunDataCopyerStage")), "跳过插件时不能运行 DataCopyer 后置阶段");
            var cppBuild = Array.FindIndex(events, e => e.Args.Contains("build") && e.Args.Any(a => a.EndsWith("CppWeavingTools.csproj")));
            var csBuild = Array.FindIndex(events, e => e.Args.Contains("build") && e.Args.Any(a => a.EndsWith("CSharpCodeTools.csproj")));
            Assert.That(cppBuild, Is.GreaterThanOrEqualTo(0), "缺少 CppWeavingTools 自举事件");
            Assert.That(csBuild, Is.GreaterThanOrEqualTo(0), "缺少 CSharpCodeTools 自举事件");
            var previousEngine = -1;
            foreach (var config in new[] { "Debug", "Release" })
            {
                var cpp = Array.FindIndex(events, e => e.File.EndsWith("CppWeavingTools.exe") && e.Args.Contains("Configuration=" + config));
                var cs = Array.FindIndex(events, e => e.File.EndsWith("CSharpCodeTools.exe") && e.Args.Contains("Configuration=" + config));
                Assert.That(cpp, Is.GreaterThan(Math.Max(cppBuild, csBuild)), "两种轻量工具必须先自举，并传递配置");
                Assert.That(cs, Is.GreaterThan(cpp));
                Assert.That(events[cpp].Args, Does.Contain("Platform=Windows").And.Contain("TitanRoot=" + directory));
                Assert.That(events[cpp].Args, Does.Contain("CppOut=" + Path.Combine(directory, "codegen", "Windows", "NativeBinder")));
                Assert.That(events[cs].Args, Does.Contain("Platform=Windows").And.Contain("TitanRoot=" + directory));
                Assert.That(events[cs].Args, Does.Contain("csproj=" + Path.Combine(directory, "Module", "Engine.Window", "Engine.Window.csproj")));
                var engine = Array.FindIndex(events, e => e.Args.Contains("build") && e.Args.Contains(config) && e.Args.Any(a => a.EndsWith("Engine.Window.csproj")));
                if (codeGenOnly) Assert.That(engine, Is.EqualTo(-1));
                else
                {
                    Assert.That(engine, Is.GreaterThan(cs));
                    Assert.That(cpp, Is.GreaterThan(previousEngine), "不能先发布另一配置的输出再编译当前引擎");
                    Assert.That(events[engine].Args, Does.Contain("/p:SkipTitanCodeGen=true"));
                    previousEngine = engine;
                }
            }
        }
        finally { Directory.Delete(directory, true); }
    }

    [Platform("Win")]
    [Test]
    public void WindowsBuild_RunsDataCopyerStageAfterOtherPluginsForEachConfiguration()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Build DataCopyer " + Guid.NewGuid().ToString("N"));
        try
        {
            foreach (var (name, project) in new[] { ("Alpha", "Alpha.All"), ("DataCopyer", "DataCopyer.All") })
            {
                var plugin = Path.Combine(directory, "Plugins", name);
                Directory.CreateDirectory(Path.Combine(plugin, project));
                File.WriteAllText(Path.Combine(plugin, name + ".plugin"), "{ \"Platforms\": [\"PLTF_ALL\"] }");
                File.WriteAllText(Path.Combine(plugin, project, project + ".csproj"), "<Project />");
            }
            var events = RunWindowsBuildScript(directory, "-Configuration", "All");
            Assert.That(events, Has.None.Matches<(string File, string[] Args)>(e => e.Args.Contains("build") && e.Args.Any(a => a.EndsWith("DataCopyer.All.csproj"))),
                "DataCopyer.All 只能由反射阶段在生成后构建");
            var previous = -1;
            foreach (var config in new[] { "Debug", "Release" })
            {
                var alpha = Array.FindIndex(events, e => e.Args.Contains("build") && e.Args.Contains(config) && e.Args.Any(a => a.EndsWith("Alpha.All.csproj")));
                var stage = Array.FindIndex(events, e => e.Args.Contains("-t:TitanRunDataCopyerStage") && e.Args.Contains("-p:Configuration=" + config));
                Assert.That(alpha, Is.GreaterThan(previous));
                Assert.That(stage, Is.GreaterThan(alpha), "反射阶段需要已构建的兼容插件");
                Assert.That(events[stage].Args, Has.Some.EndsWith(Path.Combine("Module", "Engine.Window", "Engine.Window.csproj")));
                Assert.That(events[stage].Args, Does.Contain("/p:TitanPlatform=Windows"));
                previous = stage;
            }
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void SourceTool_BuildsWithoutEngineSharedSourcesBinderOrNative()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Tool Bootstrap " + Guid.NewGuid().ToString("N"));
        var projectDirectory = Path.Combine(directory, "Module", "CSharpCodeTools");
        Directory.CreateDirectory(projectDirectory);
        try
        {
            var sourceDirectory = Path.Combine(ManagedProjectTests.Root(), "Module", "CSharpCodeTools");
            foreach (var source in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(sourceDirectory, source);
                if (relative.Split(Path.DirectorySeparatorChar).Any(p => p is "bin" or "obj")) continue;
                if (!source.EndsWith(".cs") && !source.EndsWith(".csproj")) continue;
                var destination = Path.Combine(projectDirectory, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                File.Copy(source, destination);
            }
            var templates = Path.Combine(directory, "codegen", "Templates");
            Directory.CreateDirectory(templates);
            foreach (var name in new[] { "Empty_CodeGen.vcxitems", "Empty_CodeGenCSharp.projitems" })
                File.WriteAllText(Path.Combine(templates, name), "<Project />");
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "Directory.Packages.props"), Path.Combine(directory, "Directory.Packages.props"));
            Directory.CreateDirectory(Path.Combine(directory, "BuildScript", "msbuild"));
            File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanPlatform.props"), Path.Combine(directory, "BuildScript", "msbuild", "TitanPlatform.props"));
            var result = Run(directory, "build", Path.Combine(projectDirectory, "CSharpCodeTools.csproj"), "--nologo", "-v:quiet", "-p:TitanPlatform=Windows", "-p:NuGetAudit=false");
            Assert.That(result.Code, Is.Zero, result.Log);
            var dll = Path.Combine(directory, "binaries", "Tools", "net8.0", "CSharpCodeTools.dll");
            Assert.That(File.Exists(dll), Is.True, "源码工具必须能从无引擎和无生成物的 checkout 自举");
            Assert.That(File.ReadAllText(Path.Combine(Path.GetDirectoryName(dll)!, "Templates", "Empty_CodeGenCSharp.projitems")), Is.EqualTo("<Project />"));
            var forwarding = Run(directory, dll, "mode=DataCopyer", "TitanRoot=" + directory, "Platform=Windows", "Configuration=Debug");
            Assert.That(forwarding.Code, Is.Not.Zero, "缺少独立反射宿主时必须失败，不能回到引擎类型副本");
            Assert.That(forwarding.Log, Does.Contain("CSharpReflectionTools"));
            Assert.That(Directory.Exists(Path.Combine(directory, "CSharpCode")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(directory, "codegen", "Windows")), Is.False);
            Assert.That(Directory.Exists(Path.Combine(directory, "codegen", "Mac")), Is.False);
        }
        finally { Directory.Delete(directory, true); }
    }
}
