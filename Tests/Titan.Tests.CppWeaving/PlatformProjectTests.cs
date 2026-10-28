using CppWeaving;
using NUnit.Framework;
using System.Xml.Linq;
using System.Diagnostics;
using System.Text.Json;

namespace Titan.Tests.CppWeaving;

[TestFixture, NonParallelizable]
public class PlatformProjectTests
{
    private string mDir = "";
    [SetUp] public void Setup() { mDir = Path.Combine(Path.GetTempPath(), "TitanPlatform_" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(mDir); }
    [TearDown] public void Cleanup() { Directory.Delete(mDir, true); }

    private string Fixture()
    {
        File.WriteAllText(Path.Combine(mDir, "Module.vcxitems"), """
            <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003"><ItemGroup>
              <ClInclude Include="$(MSBuildThisFileDirectory)Module.h" />
            </ItemGroup></Project>
            """);
        File.WriteAllText(Path.Combine(mDir, "Module.h"), "// TR_CLASS\nstruct Module {};\n");
        var path = Path.Combine(mDir, "Mac.vcxproj");
        File.WriteAllText(path, """
            <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <ImportGroup Label="Shared">
                <Import Project="Module.vcxitems" Label="Shared" />
                <Import Project="CodeGen.vcxitems" Label="Shared" Condition="Exists('CodeGen.vcxitems')" />
              </ImportGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|ARM64'"><ClCompile>
                <AdditionalIncludeDirectories>inc;../pthread/include;$(WindowsSDK_IncludePath);%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>
                <PreprocessorDefinitions>PLATFORM_MAC;PTR_64;_DEBUG;%(PreprocessorDefinitions)</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Release|ARM64'"><ClCompile>
                <AdditionalIncludeDirectories>release-inc</AdditionalIncludeDirectories>
                <PreprocessorDefinitions>PLATFORM_MAC;PTR_64;NDEBUG</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
            </Project>
            """);
        return path;
    }

    [TestCase("Debug", "_DEBUG", "NDEBUG")]
    [TestCase("Release", "NDEBUG", "_DEBUG")]
    public void Collector_UsesSelectedMacConfiguration(string config, string enabled, string disabled)
    {
        var collector = new HppCollector();
        Assert.DoesNotThrow(() => collector.Collect(Fixture(), $"'$(Configuration)|$(Platform)'=='{config}|ARM64'", TargetPlatform.Mac));
        Assert.That(collector.MacroDefines, Does.Contain("PLATFORM_MAC").And.Contain(enabled).And.Not.Contain(disabled));
        Assert.That(collector.IncludePath, Has.None.Contains("pthread").And.None.Contains("WindowsSDK"));
        Assert.That(collector.Headers.Values.Select(h => h.Module), Is.EqualTo(new[] { "Module" }));
    }

    [Test] public void Collector_ReadsSharedCompileMetadataOnMac()
    {
        var project = Fixture();
        var shared = Path.Combine(mDir, "Module.vcxitems");
        File.WriteAllText(shared, File.ReadAllText(shared).Replace("<ItemGroup>", "<ItemDefinitionGroup><ClCompile><AdditionalIncludeDirectories>$(MSBuildThisFileDirectory)shared-inc;%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories><PreprocessorDefinitions>SHARED_DEFINE=1;%(PreprocessorDefinitions)</PreprocessorDefinitions></ClCompile></ItemDefinitionGroup><ItemGroup>"));
        var collector = new HppCollector();
        collector.Collect(project, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Mac);
        Assert.That(collector.MacroDefines, Does.Contain("SHARED_DEFINE=1"));
        Assert.That(collector.IncludePath.Select(Path.GetFullPath), Does.Contain(Path.Combine(mDir, "shared-inc")));
    }

    [TestCase("Debug", "SHARED_DEBUG", "SHARED_RELEASE")]
    [TestCase("Release", "SHARED_RELEASE", "SHARED_DEBUG")]
    public void Collector_ReadsConfigurationOnlySharedConditions(string config, string expected, string absent)
    {
        var project = Fixture();
        var shared = Path.Combine(mDir, "Module.vcxitems");
        File.WriteAllText(shared, File.ReadAllText(shared).Replace("<ItemGroup>", "<ItemDefinitionGroup Condition=\" '$(Configuration)' == 'Debug' \"><ClCompile><PreprocessorDefinitions>SHARED_DEBUG</PreprocessorDefinitions></ClCompile></ItemDefinitionGroup><ItemDefinitionGroup Condition=\"'$(Configuration)'=='Release'\"><ClCompile><PreprocessorDefinitions>SHARED_RELEASE</PreprocessorDefinitions></ClCompile></ItemDefinitionGroup><ItemGroup>"));
        var collector = new HppCollector();
        collector.Collect(project, $"'$(Configuration)|$(Platform)'=='{config}|ARM64'", TargetPlatform.Mac);
        Assert.That(collector.MacroDefines, Does.Contain(expected).And.Not.Contain(absent));
    }

    [Test] public void Collector_RejectsUnknownSharedMetadataCondition()
    {
        var project = Fixture();
        var shared = Path.Combine(mDir, "Module.vcxitems");
        File.WriteAllText(shared, File.ReadAllText(shared).Replace("<ItemGroup>", "<ItemDefinitionGroup Condition=\"'$(Unknown)'=='Yes'\"><ClCompile><PreprocessorDefinitions>UNKNOWN</PreprocessorDefinitions></ClCompile></ItemDefinitionGroup><ItemGroup>"));
        Assert.Throws<InvalidDataException>(() => new HppCollector().Collect(project, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Mac));
    }

    [TestCase("Debug", "ELEMENT_DEBUG", "ELEMENT_RELEASE", "debug-inc")]
    [TestCase("Release", "ELEMENT_RELEASE", "ELEMENT_DEBUG", "release-inc")]
    public void Collector_ReadsElementConditions(string config, string expected, string absent, string include)
    {
        var project = Fixture();
        var shared = Path.Combine(mDir, "Module.vcxitems");
        File.WriteAllText(shared, File.ReadAllText(shared).Replace("<ItemGroup>", """
            <ItemDefinitionGroup><ClCompile>
              <PreprocessorDefinitions Condition="'$(Configuration)'=='Debug'">ELEMENT_DEBUG</PreprocessorDefinitions>
              <PreprocessorDefinitions Condition="'$(Configuration)'=='Release'">ELEMENT_RELEASE</PreprocessorDefinitions>
              <AdditionalIncludeDirectories Condition="'$(Configuration)'=='Debug'">$(MSBuildThisFileDirectory)debug-inc</AdditionalIncludeDirectories>
              <AdditionalIncludeDirectories Condition="'$(Configuration)'=='Release'">$(MSBuildThisFileDirectory)release-inc</AdditionalIncludeDirectories>
            </ClCompile></ItemDefinitionGroup><ItemGroup>
            """));
        var collector = new HppCollector();
        collector.Collect(project, $"'$(Configuration)|$(Platform)'=='{config}|ARM64'", TargetPlatform.Mac);
        Assert.That(collector.MacroDefines, Does.Contain(expected).And.Not.Contain(absent));
        Assert.That(collector.IncludePath.Select(Path.GetFullPath), Does.Contain(Path.Combine(mDir, include)));
    }

    [Test] public void Collector_ReadsUnconditionalMacProjectMetadata()
    {
        var project = Fixture();
        File.WriteAllText(project, File.ReadAllText(project).Replace("<ImportGroup", "<ItemDefinitionGroup><ClCompile><PreprocessorDefinitions>MAC_COMMON</PreprocessorDefinitions><AdditionalIncludeDirectories>$(MSBuildThisFileDirectory)common-inc</AdditionalIncludeDirectories></ClCompile></ItemDefinitionGroup><ImportGroup"));
        var collector = new HppCollector();
        collector.Collect(project, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Mac);
        Assert.That(collector.MacroDefines, Does.Contain("MAC_COMMON"));
        Assert.That(collector.IncludePath.Select(Path.GetFullPath), Does.Contain(Path.Combine(mDir, "common-inc")));
    }

    [TestCase("<PreprocessorDefinitions Condition=\"'$(Unknown)'=='Yes'\">UNKNOWN</PreprocessorDefinitions>")]
    [TestCase("<PreprocessorDefinitions>$(UnknownDefine)</PreprocessorDefinitions>")]
    [TestCase("<AdditionalIncludeDirectories>$(UnknownPath)/include</AdditionalIncludeDirectories>")]
    public void Collector_RejectsUnresolvedElementMetadata(string metadata)
    {
        var project = Fixture();
        var shared = Path.Combine(mDir, "Module.vcxitems");
        File.WriteAllText(shared, File.ReadAllText(shared).Replace("<ItemGroup>", "<ItemDefinitionGroup><ClCompile>" + metadata + "</ClCompile></ItemDefinitionGroup><ItemGroup>"));
        Assert.Throws<InvalidDataException>(() => new HppCollector().Collect(project, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Mac));
    }

    [Test] public void Collector_RejectsMissingDeclaredHeader()
    {
        var project = Fixture();
        File.Delete(Path.Combine(mDir, "Module.h"));
        Assert.Throws<FileNotFoundException>(() => new HppCollector().Collect(project, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Mac));
    }

    [Test] public void Collector_UnconditionalMetadataDoesNotHideMissingConfiguration()
    {
        var project = Fixture();
        var xml = XDocument.Load(project); var ns = xml.Root!.Name.Namespace;
        xml.Root.Elements(ns + "ItemDefinitionGroup").First().Remove();
        xml.Root.Add(new XElement(ns + "ItemDefinitionGroup", new XElement(ns + "ClCompile", new XElement(ns + "PreprocessorDefinitions", "COMMON"))));
        xml.Save(project);
        Assert.Throws<InvalidDataException>(() => new HppCollector().Collect(project, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Mac));
    }

    [Test] public void Collector_RejectsAbsentConfiguration()
    {
        Assert.Throws<InvalidDataException>(() => new HppCollector().Collect(Fixture(), "'$(Configuration)|$(Platform)'=='Profile|ARM64'", TargetPlatform.Mac));
    }

    private string AndroidFixture()
    {
        var android = Path.Combine(mDir, "Android.vcxproj");
        File.WriteAllText(android, File.ReadAllText(Fixture()).Replace("PLATFORM_MAC", "PLATFORM_DROID"));
        return android;
    }

    [Test] public void Collector_AndroidRejectsAbsentConfiguration()
    {
        Assert.Throws<InvalidDataException>(() => new HppCollector().Collect(AndroidFixture(), "'$(Configuration)|$(Platform)'=='Profile|ARM64'", TargetPlatform.Android));
    }

    [Test] public void Collector_AndroidReadsUnconditionalSharedMetadata()
    {
        var project = AndroidFixture();
        var shared = Path.Combine(mDir, "Module.vcxitems");
        File.WriteAllText(shared, File.ReadAllText(shared).Replace("<ItemGroup>", "<ItemDefinitionGroup><ClCompile><AdditionalIncludeDirectories>$(MSBuildThisFileDirectory)shared-inc;%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories><PreprocessorDefinitions>SHARED_DROID=1;%(PreprocessorDefinitions)</PreprocessorDefinitions></ClCompile></ItemDefinitionGroup><ItemGroup>"));
        var collector = new HppCollector();
        collector.Collect(project, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Android);
        Assert.That(collector.MacroDefines, Does.Contain("SHARED_DROID=1").And.Contain("PLATFORM_DROID").And.Not.Contain("PLATFORM_MAC"));
        Assert.That(collector.IncludePath.Select(Path.GetFullPath), Does.Contain(Path.Combine(mDir, "shared-inc")));
        Assert.That(collector.IncludePath, Has.None.Contains("pthread").And.None.Contains("WindowsSDK"));
    }

    [Test] public void Collector_AndroidDropsWindowsProjectMacros()
    {
        var project = Path.Combine(mDir, "Windows.vcxproj");
        File.WriteAllText(project, """
            <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <ImportGroup Label="Shared" />
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|x64'"><ClCompile>
                <AdditionalIncludeDirectories>inc;../pthread/include;%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>
                <PreprocessorDefinitions>PLATFORM_WIN;PTR_64;_DEBUG;%(PreprocessorDefinitions)</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
            </Project>
            """);
        var collector = new HppCollector();
        collector.Collect(project, "'$(Configuration)|$(Platform)'=='Debug|x64'", TargetPlatform.Android);
        Assert.That(collector.MacroDefines, Is.Empty);
        Assert.That(collector.IncludePath, Has.None.Contains("pthread"));
    }

    [Test] public void Collector_RejectsMissingOrdinaryImport()
    {
        var project = Fixture();
        File.Delete(Path.Combine(mDir, "Module.vcxitems"));
        Assert.Throws<FileNotFoundException>(() => new HppCollector().Collect(project, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Mac));
    }

    internal static string Root()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "EngineAll.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("找不到 Titan 仓库");
    }

    [Test] public void MacProject_CollectsOnlyPortableModules()
    {
        var path = Path.Combine(Root(), "Core.Mac", "Core.Mac.vcxproj");
        Assert.That(File.Exists(path), Is.True, "缺少独立 Mac 平台工程");
        var collector = new HppCollector();
        collector.Collect(path, "'$(Configuration)|$(Platform)'=='Debug|ARM64'", TargetPlatform.Mac);
        var modules = collector.Headers.Values.Select(h => h.Module).Distinct().ToArray();
        // Mac 已接入 Vulkan/PhysX5（4369f3b79、fd925a7bd），此处只排除 Windows 专属模块。
        Assert.That(modules, Does.Contain("NxPhysics").And.Contain("ImGui").And.Contain("LeanCLR"));
        Assert.That(modules, Does.Contain("Vulkan").And.Contain("PhysicsCore").And.Contain("NvPhysX5"));
        foreach (var banned in new[] { "Windows", "Dx11", "Dx12", "FBX", "TcpServer", "Collision" })
            Assert.That(modules, Does.Not.Contain(banned));
        Assert.That(collector.MacroDefines, Does.Contain("PLATFORM_MAC").And.Not.Contain("PLATFORM_WIN"));
    }

    [TestCase("Debug")]
    [TestCase("Release")]
    public void AndroidProject_HasIsolatedOutputs(string config)
    {
        var root = PlatformProject.FindRoot(TestContext.CurrentContext.TestDirectory);
        var resolved = PlatformProject.Resolve(new[] { "TitanRoot=" + root, "Platform=Android", "Configuration=" + config });
        Assert.That(resolved.Project, Does.EndWith("/Core.Android/Core.Android.vcxproj"));
        Assert.That(resolved.BuildCondition, Is.EqualTo($"'$(Configuration)|$(Platform)'=='{config}|ARM64'"));
        Assert.That(resolved.CppOutput, Does.EndWith("/codegen/Android/NativeBinder"));
        Assert.That(resolved.CsOutput, Is.EqualTo(resolved.CppOutput));
        var collector = new HppCollector();
        collector.Collect(resolved.Project, resolved.BuildCondition, TargetPlatform.Android);
        Assert.That(collector.MacroDefines, Does.Contain("PLATFORM_DROID"));
        Assert.That(collector.MacroDefines, Does.Not.Contain("PLATFORM_WIN"));
        Assert.That(collector.MacroDefines, Does.Not.Contain("PLATFORM_MAC"));
    }

    private JsonElement Evaluate(string project, string platform, params string[] extraArgs)
    {
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var arg in new[] { "msbuild", project, "-nologo", "-p:Configuration=Debug", "-p:TitanPlatform=" + platform, "-getProperty:TitanNativeBinderDirectory,TitanPlatformDefine,DefineConstants", "-getItem:Compile" }.Concat(extraArgs)) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync(); var errors = process.StandardError.ReadToEndAsync();
        Assert.That(process.WaitForExit(30000), Is.True, "MSBuild 求值超时");
        var text = output.GetAwaiter().GetResult();
        Assert.That(process.ExitCode, Is.Zero, text + errors.GetAwaiter().GetResult());
        return JsonDocument.Parse(text).RootElement.Clone();
    }

    [TestCase("Mac", "codegen/Mac/NativeBinder", "PMac")]
    [TestCase("Windows", "codegen/Windows/NativeBinder", "PWindow")]
    public void ManagedPlatform_DefaultBinderPathIsIsolated(string platform, string expected, string define)
    {
        var project = Path.Combine(mDir, "Probe.proj");
        new XDocument(new XElement("Project", new XElement("Import", new XAttribute("Project", Path.Combine(Root(), "BuildScript/msbuild/TitanPlatform.props"))))).Save(project);
        var properties = Evaluate(project, platform).GetProperty("Properties");
        Assert.That(properties.GetProperty("TitanNativeBinderDirectory").GetString()!.Replace('\\', '/').TrimEnd('/'), Is.EqualTo(Path.Combine(Root(), expected).Replace('\\', '/')));
        Assert.That(properties.GetProperty("TitanPlatformDefine").GetString(), Is.EqualTo(define));
    }

    [TestCase("Engine.Mac")]
    public void ManagedProject_MacUsesCapabilitiesAndMacPlatform(string name)
    {
        // 空 binder 仅用于 MSBuild 求值，不编译、不伪造 native 导出，也不作为托管闭包验收。
        File.WriteAllText(Path.Combine(mDir, "CodeGenCSharp.projitems"), "<Project />");
        var result = Evaluate(Path.Combine(Root(), "Module", name, name + ".csproj"), "Mac", "-p:TitanNativeBinderDirectory=" + mDir);
        var defines = result.GetProperty("Properties").GetProperty("DefineConstants").GetString()!.Split(';');
        // Mac 已接入 PhysX5，Windows 专属平台模块仍必须缺席。
        Assert.That(defines, Does.Contain("PMac").And.Not.Contain("PWindow").And.Contain("TITAN_HAS_NXPHYSICS").And.Not.Contain("TITAN_HAS_DX12").And.Not.Contain("TITAN_HAS_FBX"));
        var sources = result.GetProperty("Items").GetProperty("Compile").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
        // Mac.projitems 复用既有 FileManager/ExcelOperator；只排除 Windows 平台实现。
        Assert.That(sources, Has.Some.EndsWith("/Platform/Mac/PlatformMac.cs").And.None.EndsWith("/Platform/Windows/PlatformWindow.cs"));
        Assert.That(sources, Has.Some.EndsWith("/TitanMacCapabilities.cs"));
    }

    [TestCase("Mac")]
    [TestCase("Windows")]
    public void PlatformGeneratedProjectListsAreIgnored(string platform)
    {
        var start = new ProcessStartInfo("git") { WorkingDirectory = Root(), RedirectStandardOutput = true, UseShellExecute = false };
        foreach (var arg in new[] { "check-ignore", $"codegen/{platform}/NativeBinder/CodeGen.vcxitems", $"codegen/{platform}/NativeBinder/CodeGenCSharp.projitems", $"codegen/{platform}/Rpc/Engine/.outputs.json" }) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEnd(); process.WaitForExit();
        Assert.That(process.ExitCode, Is.Zero);
        Assert.That(output, Does.Contain("CodeGen.vcxitems").And.Contain("CodeGenCSharp.projitems"));
    }

    private (int Code, string Log) RunDotnet(params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = mDir, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync(); var errors = process.StandardError.ReadToEndAsync();
        Assert.That(process.WaitForExit(60000), Is.True, "dotnet 构建超时");
        return (process.ExitCode, output.GetAwaiter().GetResult() + errors.GetAwaiter().GetResult());
    }

    [Test]
    public void WindowsSolution_SerializesSharedCodegenHost()
    {
        var solution = File.ReadAllText(Path.Combine(Root(), "EngineAll.sln"));
        var engineStart = solution.IndexOf("= \"Engine.Window\"", StringComparison.Ordinal);
        var engineEnd = solution.IndexOf("EndProject", engineStart, StringComparison.Ordinal);
        var engineBlock = solution.Substring(engineStart, engineEnd - engineStart);
        Assert.That(engineBlock, Does.Contain("{AD0E23E3-ECA5-4E52-B69D-BB6E669C48C6}"),
            "Engine.Window 与 Core.Window 共享 Windows binder，解决方案必须串行化两个 codegen 入口");
    }

    [Test]
    public void WindowsTargets_RejectsNonWindowsHostBeforeToolBuild()
    {
        var xml = XDocument.Load(Path.Combine(Root(), "BuildScript/msbuild/TitanCodeGen.targets"));
        var ns = xml.Root!.Name.Namespace;
        var validation = xml.Root.Elements(ns + "Target").Single(t => (string?)t.Attribute("Name") == "_ValidateTitanCodeGenHost");
        var error = validation.Element(ns + "Error")!;
        Assert.That((string?)error.Attribute("Condition"), Does.Contain("IsOSPlatform('Windows')"));
        Assert.That((string?)error.Attribute("Text"), Does.Contain("Core.Mac/GenerateCodegen.cmake"));
        foreach (var targetName in new[] { "_EnsureCppWeavingBuilt", "_EnsureCSharpCodeToolsBuilt" })
        {
            var target = xml.Root.Elements(ns + "Target").Single(t => (string?)t.Attribute("Name") == targetName);
            Assert.That((string?)target.Attribute("DependsOnTargets"), Does.StartWith("_ValidateTitanCodeGenHost;"));
        }
    }

    [Platform("Win")]
    [TestCase("MissingCppTemplate", false)]
    [TestCase("MissingCsTemplate", false)]
    [TestCase("MissingExe", true)]
    [TestCase("MissingDll", true)]
    [TestCase("MissingDeps", true)]
    [TestCase("MissingRuntimeConfig", true)]
    [TestCase("MissingNativeRuntime", true)]
    [TestCase("StaleTemplate", false)]
    [TestCase("StaleTool", true)]
    [TestCase("StaleProject", true)]
    [TestCase("PlatformProps", true)]
    [TestCase("UpToDate", false)]
    [TestCase("Skip", false)]
    public void WindowsTargets_RefreshesWeavingDeployment(string scenario, bool rebuild)
    {
        // 构建真实的最小工具，验证入库 targets 的部署判断；不触碰仓库 binaries 或 binder。
        var source = Path.Combine(mDir, "Module/CppWeavingTools");
        var templates = Path.Combine(mDir, "codegen/Templates");
        var output = Path.Combine(mDir, "binaries/Tools/net8.0");
        Directory.CreateDirectory(source); Directory.CreateDirectory(templates);
        Directory.CreateDirectory(Path.Combine(mDir, "BuildScript", "msbuild"));
        var platformProps = Path.Combine(mDir, "BuildScript/msbuild/TitanPlatform.props");
        File.WriteAllText(platformProps, "<Project />");
        var project = Path.Combine(source, "CppWeavingTools.csproj");
        File.WriteAllText(project, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework>
                <OutputPath>../../binaries/Tools/net8.0/</OutputPath><AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
              </PropertyGroup>
              <ItemGroup>
                <Content Include="../../codegen/Templates/Empty_CodeGen.vcxitems" Link="Templates/Empty_CodeGen.vcxitems" CopyToOutputDirectory="PreserveNewest" />
                <Content Include="../../codegen/Templates/Empty_CodeGenCSharp.projitems" Link="Templates/Empty_CodeGenCSharp.projitems" CopyToOutputDirectory="PreserveNewest" />
                <Content Include="ClangSharp.dll;ClangSharp.Interop.dll;libclang.dll;libClangSharp.dll" CopyToOutputDirectory="PreserveNewest" />
              </ItemGroup>
            </Project>
            """);
        var program = Path.Combine(source, "Program.cs");
        File.WriteAllText(program, "System.Console.WriteLine(\"baseline\");");
        foreach (var name in new[] { "ClangSharp.dll", "ClangSharp.Interop.dll", "libclang.dll", "libClangSharp.dll" })
            File.WriteAllText(Path.Combine(source, name), "runtime");
        foreach (var name in new[] { "Empty_CodeGen.vcxitems", "Empty_CodeGenCSharp.projitems" })
            File.WriteAllText(Path.Combine(templates, name), "baseline template");
        var initial = RunDotnet("build", project, "-c", "Debug", "--nologo", "-v", "q");
        Assert.That(initial.Code, Is.Zero, initial.Log);
        var probe = Path.Combine(mDir, "Probe.proj");
        new XDocument(new XElement("Project", new XElement("Import", new XAttribute("Project", Path.Combine(Root(), "BuildScript/msbuild/TitanCodeGen.targets"))))).Save(probe);
        var baseline = RunDotnet("msbuild", probe, "-nologo", "-t:_EnsureCppWeavingBuilt", "-p:TitanRoot=" + mDir + "/");
        Assert.That(baseline.Code, Is.Zero, baseline.Log);
        var cppTemplate = Path.Combine(output, "Templates/Empty_CodeGen.vcxitems");
        var csTemplate = Path.Combine(output, "Templates/Empty_CodeGenCSharp.projitems");
        var old = DateTime.UtcNow.AddHours(-1);
        foreach (var input in new[] { project, program, platformProps, Path.Combine(templates, "Empty_CodeGen.vcxitems"), Path.Combine(templates, "Empty_CodeGenCSharp.projitems") })
            File.SetLastWriteTimeUtc(input, old);
        if (scenario is "MissingCppTemplate" or "Skip") File.Delete(cppTemplate);
        if (scenario == "MissingCsTemplate") File.Delete(csTemplate);
        var missingDeployment = scenario switch
        {
            "MissingExe" => "CppWeavingTools.exe",
            "MissingDll" => "CppWeavingTools.dll",
            "MissingDeps" => "CppWeavingTools.deps.json",
            "MissingRuntimeConfig" => "CppWeavingTools.runtimeconfig.json",
            "MissingNativeRuntime" => "libclang.dll",
            _ => null,
        };
        if (missingDeployment != null) File.Delete(Path.Combine(output, missingDeployment));
        if (scenario == "StaleTemplate")
        {
            File.WriteAllText(Path.Combine(templates, "Empty_CodeGen.vcxitems"), "updated template");
            File.SetLastWriteTimeUtc(cppTemplate, old.AddHours(-1));
        }
        if (scenario == "StaleTool")
        {
            File.WriteAllText(program, "System.Console.WriteLine(\"updated tool\");");
            File.SetLastWriteTimeUtc(Path.Combine(output, "CppWeavingTools.exe"), old.AddHours(-1));
        }
        if (scenario is "StaleProject" or "PlatformProps")
        {
            Thread.Sleep(1100);
            var changedFile = scenario == "PlatformProps" ? platformProps : project;
            File.AppendAllText(changedFile, "\n<!-- changed -->");
            File.SetLastWriteTimeUtc(changedFile, DateTime.UtcNow);
        }
        var result = RunDotnet("msbuild", probe, "-nologo", "-t:_EnsureCppWeavingBuilt", "-p:TitanRoot=" + mDir + "/", "-p:SkipTitanCodeGen=" + (scenario == "Skip" ? "true" : "false"));
        Assert.That(result.Code, Is.Zero, result.Log);
        Assert.That(result.Log.Contains("Building CppWeavingTools"), Is.EqualTo(rebuild), result.Log);
        if (scenario == "Skip") Assert.That(File.Exists(cppTemplate), Is.False);
        else
        {
            if (missingDeployment != null)
                Assert.That(File.Exists(Path.Combine(output, missingDeployment)), Is.True, "未恢复工具部署文件");
            Assert.That(File.Exists(cppTemplate), Is.True, "未恢复 C++ 清单模板");
            Assert.That(File.Exists(csTemplate), Is.True, "未恢复 C# 清单模板");
            Assert.That(File.ReadAllText(cppTemplate), Is.EqualTo(scenario == "StaleTemplate" ? "updated template" : "baseline template"));
            Assert.That(File.ReadAllText(csTemplate), Is.EqualTo("baseline template"));
        }
        if (scenario == "StaleTool")
        {
            var run = RunDotnet(Path.Combine(output, "CppWeavingTools.dll"));
            Assert.That(run.Code, Is.Zero, run.Log);
            Assert.That(run.Log.Trim(), Is.EqualTo("updated tool"));
        }
        if (scenario is "StaleProject" or "PlatformProps")
        {
            var repeated = RunDotnet("msbuild", probe, "-nologo", "-t:_EnsureCppWeavingBuilt", "-p:TitanRoot=" + mDir + "/");
            Assert.That(repeated.Code, Is.Zero, repeated.Log);
            Assert.That(repeated.Log, Does.Not.Contain("Building CppWeavingTools"), repeated.Log);
        }
    }

    [Platform("Win")]
    [TestCase("Tool", true)]
    [TestCase("Template", true)]
    [TestCase("CSharpTool", false)]
    [TestCase("RpcSchema", false)]
    [TestCase("Project", true)]
    [TestCase("SharedProject", true)]
    [TestCase("ThirdPartyHeader", true)]
    [TestCase("IncludedFragment", true)]
    [TestCase("Packages", true)]
    [TestCase("PlatformProps", true)]
    [TestCase("GeneratedBinder", false)]
    [TestCase("SharedCSharp", false)]
    [TestCase("CSharpSharedProject", false)]
    [TestCase("ProjectReference", false)]
    [TestCase("PluginRpcSource", false)]
    [TestCase("MissingPOD", true)]
    [TestCase("MissingCppList", true)]
    [TestCase("MissingCsList", true)]
    public void WindowsTargets_ChangedGeneratorInputInvalidatesGlue(string changed, bool nativeRegenerates)
    {
        // 用真实编译的轻量 CLI 记录调用次数，验证工具、工程元数据和声明输入的失效契约。
        var templates = Path.Combine(mDir, "codegen/Templates");
        var binder = Path.Combine(mDir, "codegen/Windows/NativeBinder");
        Directory.CreateDirectory(binder);
        var native = Path.Combine(mDir, "NativeCode");
        var thirdParty = Path.Combine(mDir, "3rd/native/Lib");
        var coreWindow = Path.Combine(mDir, "Core.Window");
        var csharpCode = Path.Combine(mDir, "CSharpCode/Base");
        var pluginCode = Path.Combine(mDir, "Plugins/Test");
        var projectReference = Path.Combine(mDir, "Module/CompilingGenerator");
        var buildScript = Path.Combine(mDir, "BuildScript", "msbuild");
        Directory.CreateDirectory(templates); Directory.CreateDirectory(native);
        Directory.CreateDirectory(thirdParty); Directory.CreateDirectory(coreWindow);
        Directory.CreateDirectory(csharpCode); Directory.CreateDirectory(pluginCode);
        Directory.CreateDirectory(projectReference); Directory.CreateDirectory(buildScript);
        var inputs = new Dictionary<string, string>
        {
            ["Template"] = Path.Combine(templates, "Empty_CodeGen.vcxitems"),
            ["RpcSchema"] = Path.Combine(mDir, "Rpc_Engine.txt"),
            ["Project"] = Path.Combine(coreWindow, "Core.Window.vcxproj"),
            ["SharedProject"] = Path.Combine(native, "Module.vcxitems"),
            ["ThirdPartyHeader"] = Path.Combine(thirdParty, "Input.h"),
            ["IncludedFragment"] = Path.Combine(thirdParty, "Input.inl"),
            ["Packages"] = Path.Combine(mDir, "Directory.Packages.props"),
            ["PlatformProps"] = Path.Combine(buildScript, "TitanPlatform.props"),
            ["GeneratedBinder"] = Path.Combine(binder, "Input.cpp2cs.cs"),
            ["SharedCSharp"] = Path.Combine(csharpCode, "Input.cs"),
            ["CSharpSharedProject"] = Path.Combine(csharpCode, "Base.projitems"),
            ["ProjectReference"] = Path.Combine(projectReference, "Input.cs"),
            ["PluginRpcSource"] = Path.Combine(pluginCode, "Input.cs"),
            ["MissingPOD"] = Path.Combine(binder, "PODStructDefine.h"),
            ["MissingCppList"] = Path.Combine(binder, "CodeGen.vcxitems"),
            ["MissingCsList"] = Path.Combine(binder, "CodeGenCSharp.projitems"),
        };
        File.WriteAllText(Path.Combine(native, "Input.h"), "// input");
        File.WriteAllText(inputs["RpcSchema"], "rpc baseline");
        File.WriteAllText(inputs["Project"], "<Project />");
        File.WriteAllText(inputs["SharedProject"], "<Project />");
        File.WriteAllText(inputs["ThirdPartyHeader"], "// third-party input");
        File.WriteAllText(inputs["IncludedFragment"], "// included fragment");
        File.WriteAllText(inputs["Packages"], "<Project />");
        File.WriteAllText(inputs["PlatformProps"], "<Project />");
        File.WriteAllText(inputs["GeneratedBinder"], "// generated binder");
        File.WriteAllText(inputs["SharedCSharp"], "// managed input");
        File.WriteAllText(inputs["CSharpSharedProject"], "<Project />");
        File.WriteAllText(inputs["ProjectReference"], "// analyzer input");
        File.WriteAllText(inputs["PluginRpcSource"], "// plugin RPC input");
        File.WriteAllText(Path.Combine(templates, "CodeGenCSharp.projitems"), "<Project />");
        foreach (var name in new[] { "Empty_CodeGen.vcxitems", "Empty_CodeGenCSharp.projitems" })
            File.WriteAllText(Path.Combine(templates, name), "<Project />");
        foreach (var name in new[] { "CppWeavingTools", "CSharpCodeTools" })
        {
            var dir = Path.Combine(mDir, "Module", name); Directory.CreateDirectory(dir);
            var project = Path.Combine(dir, name + ".csproj");
            File.WriteAllText(project, """
                <Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
                  <OutputType>Exe</OutputType><TargetFramework>net8.0</TargetFramework>
                  <OutputPath>../../binaries/Tools/net8.0/</OutputPath><AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>
                </PropertyGroup></Project>
                """);
            File.WriteAllText(Path.Combine(dir, "Program.cs"), name == "CppWeavingTools" ? "var output = System.Array.Find(System.Environment.GetCommandLineArgs(), x => x.StartsWith(\"CppOut=\"))!.Substring(7); System.IO.Directory.CreateDirectory(output); System.IO.File.AppendAllText(\"calls.txt\", \"generation\\n\"); System.IO.File.WriteAllText(System.IO.Path.Combine(output, \"PODStructDefine.h\"), \"#pragma once\\n\"); System.IO.File.WriteAllText(System.IO.Path.Combine(output, \"CodeGen.vcxitems\"), \"<Project />\"); System.IO.File.WriteAllText(System.IO.Path.Combine(output, \"CodeGenCSharp.projitems\"), \"<Project />\");" : "System.Console.WriteLine(\"managed generation \" + string.Join(\" \", System.Environment.GetCommandLineArgs()));");
            var build = RunDotnet("build", project, "-c", "Debug", "--nologo", "-v", "q");
            Assert.That(build.Code, Is.Zero, build.Log);
        }
        var toolOutput = Path.Combine(mDir, "binaries/Tools/net8.0");
        foreach (var name in new[] { "ClangSharp.dll", "ClangSharp.Interop.dll", "libclang.dll", "libClangSharp.dll" })
            File.WriteAllText(Path.Combine(toolOutput, name), "runtime");
        inputs["Tool"] = Path.Combine(mDir, "Module/CppWeavingTools/Program.cs");
        inputs["CSharpTool"] = Path.Combine(mDir, "Module/CSharpCodeTools/Program.cs");
        var probe = Path.Combine(mDir, "Probe.proj");
        new XDocument(new XElement("Project", new XElement("Import", new XAttribute("Project", Path.Combine(Root(), "BuildScript/msbuild/TitanCodeGen.targets"))))).Save(probe);
        var args = new[] { "msbuild", probe, "-nologo", "-t:TitanGenerateGlueCode", "-p:TitanRoot=" + mDir, "-p:TitanPlatform=Windows", "-p:Configuration=Debug" };
        var first = RunDotnet(args); Assert.That(first.Code, Is.Zero, first.Log);
        Assert.That(File.ReadAllLines(Path.Combine(mDir, "calls.txt")), Has.Length.EqualTo(1));
        var input = inputs[changed];
        Thread.Sleep(1100);
        if (changed.StartsWith("Missing", StringComparison.Ordinal))
            File.Delete(input);
        else
        {
            if (changed == "CSharpTool")
                File.WriteAllText(input, "System.Console.WriteLine(\"managed generation updated \" + string.Join(\" \", System.Environment.GetCommandLineArgs()));");
            else
                File.AppendAllText(input, changed.EndsWith("Header") || changed is "IncludedFragment" or "Tool" ? "\n// changed" : "\n<!-- changed -->");
            File.SetLastWriteTimeUtc(input, DateTime.UtcNow);
        }
        var second = RunDotnet(args); Assert.That(second.Code, Is.Zero, second.Log);
        Assert.That(File.ReadAllLines(Path.Combine(mDir, "calls.txt")), Has.Length.EqualTo(nativeRegenerates ? 2 : 1), second.Log);
        Assert.That(second.Log.Contains("Building CSharpCodeTools"), Is.EqualTo(changed is "CSharpTool" or "PlatformProps" or "Packages"), second.Log);
        if (changed == "CSharpTool")
        {
            Assert.That(second.Log, Does.Contain("managed generation updated"), second.Log);
            var repeated = RunDotnet("msbuild", probe, "-nologo", "-t:_EnsureCSharpCodeToolsBuilt", "-p:TitanRoot=" + mDir + "/");
            Assert.That(repeated.Code, Is.Zero, repeated.Log);
            Assert.That(repeated.Log, Does.Not.Contain("Building CSharpCodeTools"), repeated.Log);
        }
        var noOp = RunDotnet(args); Assert.That(noOp.Code, Is.Zero, noOp.Log);
        Assert.That(File.ReadAllLines(Path.Combine(mDir, "calls.txt")), Has.Length.EqualTo(nativeRegenerates ? 2 : 1), noOp.Log);
        Assert.That(noOp.Log, Does.Not.Contain("Generating C++"), noOp.Log);
        Assert.That(noOp.Log, Does.Contain("managed generation"), "源码每次重新求值，由工具内容指纹决定是否写入");
        Assert.That(noOp.Log, Does.Contain("Platform=Windows").And.Contain("Configuration=Debug"));
    }

    [TestCase("Debug")]
    [TestCase("Release")]
    public void MacProject_IntelliSenseMatchesCompileMetadata(string config)
    {
        var dir = Path.Combine(Root(), "Core.Mac");
        var xml = XDocument.Load(Path.Combine(dir, "Core.Mac.vcxproj")); var ns = xml.Root!.Name.Namespace;
        var condition = $"'$(Configuration)|$(Platform)'=='{config}|ARM64'";
        var properties = xml.Root.Elements(ns + "PropertyGroup").Where(g => g.Attribute("Condition") == null || g.Attribute("Condition")!.Value == condition).SelectMany(g => g.Elements()).ToArray();
        var includes = properties.Last(e => e.Name.LocalName == "NMakeIncludeSearchPath").Value.Split(';')
            .Select(p => Path.GetFullPath(Path.Combine(dir, p.Replace("$(ProjectDir)", dir + "/").Replace('\\', '/'))).TrimEnd('/', '\\')).ToArray();
        var defines = properties.Last(e => e.Name.LocalName == "NMakePreprocessorDefinitions").Value.Split(';');
        var compile = xml.Root.Elements(ns + "ItemDefinitionGroup").Single(g => g.Attribute("Condition")!.Value == condition).Element(ns + "ClCompile")!;
        foreach (var include in compile.Element(ns + "AdditionalIncludeDirectories")!.Value.Split(';').Where(p => !p.StartsWith("%(")))
            Assert.That(includes, Does.Contain(Path.GetFullPath(Path.Combine(dir, include))), include);
        foreach (var define in compile.Element(ns + "PreprocessorDefinitions")!.Value.Split(';').Where(p => !p.StartsWith("%(")))
            Assert.That(defines, Does.Contain(define));
        Assert.That(defines, Does.Not.Contain(config == "Debug" ? "NDEBUG" : "_DEBUG"));
    }

    [Test] public void MacProject_IsNotInWindowsDefaultBuild()
    {
        var path = Path.Combine(Root(), "Core.Mac", "Core.Mac.vcxproj");
        Assert.That(File.Exists(path), Is.True);
        var xml = XDocument.Load(path); var ns = xml.Root!.Name.Namespace;
        var guid = xml.Descendants(ns + "ProjectGuid").Single().Value;
        var sln = File.ReadAllText(Path.Combine(Root(), "EngineAll.sln"));
        Assert.That(sln, Does.Contain($"\"Core.Mac\", \"Core.Mac\\Core.Mac.vcxproj\", \"{guid}\""));
        Assert.That(sln.Split('\n').Where(l => l.Contains(guid) && l.Contains(".Build.0")), Is.Empty);
        Assert.That(xml.Descendants(ns + "ConfigurationType").Select(n => n.Value), Is.All.EqualTo("Makefile"));
    }
}
