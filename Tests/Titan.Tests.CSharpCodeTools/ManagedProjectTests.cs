using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;

namespace Titan.Tests.CSharpCodeTools;

[TestFixture]
public class ManagedProjectTests
{
    internal static string Root()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "EngineAll.sln"))) dir = dir.Parent;
        return dir?.FullName ?? throw new DirectoryNotFoundException("找不到仓库");
    }

    // 必须运行随本次测试构建的工具，不能读部署目录里可能过期的副本。
    internal static string Tool() => typeof(global::CSharpCodeTools.LegacyGeneratedBlocks).Assembly.Location;

    internal static (int Code, string Log) Run(params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, WorkingDirectory = Root(), StandardOutputEncoding = System.Text.Encoding.UTF8, StandardErrorEncoding = System.Text.Encoding.UTF8 };
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        if (!process.WaitForExit(60000)) { process.Kill(true); throw new TimeoutException("MSBuild 超时"); }
        return (process.ExitCode, output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult());
    }

    [TestCase("Engine.Window", "Mac")]
    [TestCase("Engine.Mac", "Windows")]
    public void EngineProject_RejectsMismatchedPlatform(string name, string platform)
    {
        var dir = Path.Combine(Path.GetTempPath(), "TitanEval_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        try
        {
            // 只用于求值，避免缺失生成 import 抢先掩盖工程平台校验。
            File.WriteAllText(Path.Combine(dir, "CodeGenCSharp.projitems"), "<Project />");
            var result = Run("msbuild", Path.Combine(Root(), "Module", name, name + ".csproj"), "-nologo", "-t:TitanValidatePlatform", "-p:TitanPlatform=" + platform, "-p:TitanNativeBinderDirectory=" + dir);
            Assert.That(result.Code, Is.Not.Zero, "工程不能接受另一平台");
            Assert.That(result.Log, Does.Contain(name));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Test]
    public void MacProject_EvaluatesPortableSourceClosureWithoutBuildingEngine()
    {
        var project = Path.Combine(Root(), "Module/Engine.Mac/Engine.Mac.csproj");
        Assert.That(File.Exists(project), Is.True, "缺少 Engine.Mac 工程");
        var result = Run("msbuild", project, "-nologo", "-getProperty:TitanPlatform,AssemblyName,DefineConstants", "-getItem:Compile");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var props = json.RootElement.GetProperty("Properties");
        Assert.That(props.GetProperty("TitanPlatform").GetString(), Is.EqualTo("Mac"));
        Assert.That(props.GetProperty("AssemblyName").GetString(), Is.EqualTo("Engine.Mac"));
        Assert.That(props.GetProperty("DefineConstants").GetString(), Does.Contain("PMac").And.Not.Contain("PWindow"));
        var files = json.RootElement.GetProperty("Items").GetProperty("Compile").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
        Assert.That(files, Has.Some.EndsWith("/Platform/Mac/PlatformMac.cs"));
        Assert.That(files, Has.Some.Contains("/Terrain/").And.Some.Contains("/NxPhysics/"));
        // PhysicsCore/NvPhysX5 已随 Core.Mac 静态库接入 Mac 闭包，不能再当作 Windows 专属排除。
        Assert.That(files, Has.Some.Contains("/Bricks/PhysicsCore/").And.Some.Contains("/Bricks/DestinyPX/NxPhysics/"));
        foreach (var banned in new[] { "/Platform/Windows/PlatformWindow.cs", "/Bricks/FBX/", "/Bricks/TcpServer/", "/Bricks/PythonRuntime/" })
            Assert.That(files, Has.None.Contains(banned));
    }

    [TestCase("Plugins/AIGC/AIAgent/AIAgent.All/AIAgent.All.csproj")]
    [TestCase("Plugins/AIGC/MCPServer/MCPServer.All/MCPServer.All.csproj")]
    [TestCase("Plugins/AIGC/TencentAIGC/TencentAIGC.All/TencentAIGC.All.csproj")]
    [TestCase("Plugins/DataCopyer/DataCopyer.All/DataCopyer.All.csproj")]
    [TestCase("Plugins/Game/Survivor/Survivor.All/Survivor.All.csproj")]
    [TestCase("Plugins/GameCore/Inventory/Inventory.All/Inventory.All.csproj")]
    [TestCase("Plugins/GameItems/GameItems.All/GameItems.All.csproj")]
    [TestCase("Plugins/GameTasks/GameTasks.All/GameTasks.All.csproj")]
    public void AllPlugin_UsesMacEngineAndOwnPlatformSymbols(string relativeProject)
    {
        var result = Run("msbuild", Path.Combine(Root(), relativeProject), "-nologo", "-p:TitanPlatform=Mac", "-getProperty:DefineConstants,TitanEngineProject,TitanEngineAssemblyPath,OutDir,BaseIntermediateOutputPath", "-getItem:ProjectReference,Reference");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var props = json.RootElement.GetProperty("Properties");
        Assert.That(props.GetProperty("DefineConstants").GetString(), Does.Contain("PMac").And.Not.Contain("PWindow"));
        foreach (var property in new[] { "OutDir", "BaseIntermediateOutputPath" })
        {
            var path = props.GetProperty(property).GetString()!;
            Assert.That(Path.IsPathFullyQualified(path), Is.True, property + " 不能依赖 SolutionDir 或调用者工作目录");
            Assert.That(Path.GetFullPath(path).StartsWith(Root() + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), Is.True);
        }
        var items = json.RootElement.GetProperty("Items");
        var engineProjects = items.GetProperty("ProjectReference").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!).Where(p => p.Contains("Engine.")).ToArray();
        var engineRefs = items.GetProperty("Reference").EnumerateArray().Where(i => i.GetProperty("Identity").GetString()!.StartsWith("Engine.")).ToArray();
        Assert.That(engineProjects.Length + engineRefs.Length, Is.EqualTo(1));
        if (engineProjects.Length != 0) Assert.That(engineProjects[0].Replace('\\', '/'), Does.EndWith("/Engine.Mac/Engine.Mac.csproj"));
        else
        {
            Assert.That(engineRefs[0].GetProperty("Identity").GetString(), Is.EqualTo("Engine.Mac"));
            Assert.That(engineRefs[0].GetProperty("HintPath").GetString()!.Replace('\\', '/'), Does.EndWith("/Engine.Mac.dll"));
        }
    }

    [TestCase("BuildScript/msbuild/TitanManagedEngine.props")]
    [TestCase("Core.Window/Core.Window.vcxproj")]
    public void PlatformConsumers_DoNotImportGeneratedProjectsDuringEvaluation(string file)
    {
        var xml = System.Xml.Linq.XDocument.Load(Path.Combine(Root(), file));
        var generatedImports = xml.Descendants().Where(e => e.Name.LocalName == "Import")
            .Select(e => (string?)e.Attribute("Project"))
            .Where(p => p != null && (p.Contains("CodeGenCSharp.projitems") || p.Contains("EngineMacross.projitems") || p.Contains("CodeGen.vcxitems"))).ToArray();
        Assert.That(generatedImports, Is.Empty, "首次生成必须通过生成后的受控编译输入消费");
    }

    [Test]
    public void ProjectFiles_DoNotReferenceUnprefixedLegacyCodegenDirectories()
    {
        var root = Root();
        var legacy = new System.Text.RegularExpressions.Regex(@"codegen[\\/](?:NativeBinder|Rpc|Property|Macross|Serializer)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var skipped = new[] { "bin", "obj", "codegen", "artifacts", "intermediate", "binaries", ".git", "3rd", "cache" };
        var offenders = new[] { "*.csproj", "*.vcxproj", "*.props", "*.targets", "*.projitems", "*.vcxitems", "*.sln" }
            .SelectMany(pattern => Directory.EnumerateFiles(root, pattern, SearchOption.AllDirectories))
            .Where(p => !Path.GetRelativePath(root, p).Split(Path.DirectorySeparatorChar).Any(part => skipped.Contains(part, StringComparer.OrdinalIgnoreCase)))
            .Where(p => legacy.IsMatch(File.ReadAllText(p)))
            .Select(p => Path.GetRelativePath(root, p)).ToArray();
        Assert.That(offenders, Is.Empty, "生成目录已统一为 codegen/<Platform>，工程不得继续引用旧目录");
    }

    [Test]
    public void RpcCallerHost_ConsumesSharedCallersThroughValidatedOutputs()
    {
        var project = Path.Combine(Root(), "Plugins/RpcCaller/RpcCaller.Window/RpcCaller.Window.csproj");
        var result = Run("msbuild", project, "-nologo", "-getProperty:TitanConsumeRpcCallers");
        Assert.That(result.Code, Is.Zero, result.Log);
        Assert.That(result.Log.Trim(), Is.EqualTo("true"));
        var targets = File.ReadAllText(Path.Combine(Root(), "BuildScript/msbuild/TitanCodeGen.targets"));
        Assert.That(targets, Does.Contain("RpcCallers=$(TitanConsumeRpcCallers)"));
    }

    [Test]
    public void SourceToolsTests_AreRegisteredInQualityManifestAndSolution()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(Root(), "content/config/quality-manifest.json")));
        var project = "Tests/Titan.Tests.CSharpCodeTools/Titan.Tests.CSharpCodeTools.csproj";
        Assert.That(manifest.RootElement.GetProperty("suites").EnumerateArray().Any(s => s.GetProperty("level").GetString() == "L1" && s.GetProperty("projects").EnumerateArray().Any(p => p.GetString() == project)), Is.True);
        var solution = File.ReadAllText(Path.Combine(Root(), "EngineAll.sln"));
        Assert.That(solution.Replace('\\', '/'), Does.Contain(project).And.Contain("Module/Engine.Mac/Engine.Mac.csproj"));
    }

    [Test]
    public void MacSolutionSharedAssociations_MatchActualNonGeneratedImports()
    {
        var inputs = global::CSharpCodeTools.ProjectSourceInputs.Load("", "Mac", "Debug");
        var expected = inputs.ImportedFiles.Where(p => p.EndsWith(".projitems", StringComparison.OrdinalIgnoreCase) && !Path.GetRelativePath(Root(), p).StartsWith("codegen", StringComparison.OrdinalIgnoreCase))
            .Select(p => Path.GetRelativePath(Root(), p).Replace('\\', '/')).ToArray();
        var actual = File.ReadAllLines(Path.Combine(Root(), "EngineAll.sln"))
            .Where(line => line.Contains("*{c9407bb7-f57e-46c8-ba6e-04f303da50b1}*SharedItemsImports = 5", StringComparison.OrdinalIgnoreCase))
            .Select(line => line.Trim().Split('*')[0].Replace('\\', '/')).ToArray();
        Assert.That(actual, Is.EquivalentTo(expected));
        Assert.That(File.ReadAllLines(Path.Combine(Root(), "EngineAll.sln")).Where(l => l.Contains("{C9407BB7-F57E-46C8-BA6E-04F303DA50B1}")).Any(l => l.Contains("Build.0")), Is.False);
    }

    [Test]
    public void ToolSolutionEntries_HaveNoEngineSharedImportsAndReflectionHostIsNotSolutionBuilt()
    {
        var lines = File.ReadAllLines(Path.Combine(Root(), "EngineAll.sln"));
        // 轻量工具与反射宿主都只编译自身源码，解决方案不能再登记引擎共享源码关联。
        Assert.That(lines.Where(l => l.Contains("*{9536c797-8d72-4b0d-be38-bd1ea4a2fc97}*SharedItemsImports", StringComparison.OrdinalIgnoreCase)), Is.Empty);
        var host = lines.SingleOrDefault(l => l.Replace('\\', '/').Contains("\"Module/CSharpReflectionTools/CSharpReflectionTools.csproj\""));
        Assert.That(host, Is.Not.Null);
        var guid = host!.Split('"')[^2];
        Assert.That(lines.Where(l => l.Contains("*" + guid + "*SharedItemsImports", StringComparison.OrdinalIgnoreCase)), Is.Empty);
        // 宿主由引擎后置的 TitanRunDataCopyerStage 构建；解决方案并行构建会与该阶段争用同一输出。
        Assert.That(lines.Where(l => l.TrimStart().StartsWith(guid + ".", StringComparison.Ordinal)).Any(l => l.Contains("Build.0")), Is.False);
        Assert.That(lines.Count(l => l.TrimStart().StartsWith(guid + ".", StringComparison.Ordinal) && l.Contains(".ActiveCfg")), Is.EqualTo(9));
    }

    [TestCase("", "Mac", "Engine.Mac")]
    [TestCase("Category/Nested", "Mac", "Engine.Mac")]
    [TestCase("", "Windows", "Engine.Window")]
    [TestCase("Category/Nested", "Windows", "Engine.Window")]
    public void NewAllPluginTemplate_RoutesEngineWithoutSolutionDir(string category, string platform, string engine)
    {
        if (!OperatingSystem.IsWindows()) Assert.Ignore("PowerShell 5.1 模板运行契约");
        var dir = Path.Combine(Path.GetTempPath(), "Titan Plugin Template " + Guid.NewGuid().ToString("N"));
        foreach (var subdir in new[] { "common", "msbuild", "scaffold" })
            Directory.CreateDirectory(Path.Combine(dir, "BuildScript", subdir));
        Directory.CreateDirectory(Path.Combine(dir, "Plugins"));
        Directory.CreateDirectory(Path.Combine(dir, "Core.Mac", "generated"));
        try
        {
            // 夹具镜像 BuildScript 的功能分目录布局，脚本按 PSScriptRoot 的相对路径查找依赖。
            foreach (var (subdir, script) in new[] { ("scaffold", "New-TitanPlugin.ps1"), ("common", "SolutionTools.ps1"), ("msbuild", "TitanPlatform.props"), ("msbuild", "TitanManagedProject.props") })
                File.Copy(Path.Combine(Root(), "BuildScript", subdir, script), Path.Combine(dir, "BuildScript", subdir, script));
            File.WriteAllText(Path.Combine(dir, "Core.Mac", "generated", "TitanMacCapabilities.props"), "<Project />");
            var solution = Path.Combine(dir, "EngineAll.sln");
            File.WriteAllText(solution, "Microsoft Visual Studio Solution File, Format Version 12.00\nProject(\"{2150E333-8FDC-42A3-9474-1A3956D46DE8}\") = \"Plugins\", \"Plugins\", \"{C201BD42-0AA2-4345-B2AD-500F12E57D41}\"\nEndProject\nGlobal\n\tGlobalSection(SolutionConfigurationPlatforms) = preSolution\n\t\tDebug|Any CPU = Debug|Any CPU\n\tEndGlobalSection\nEndGlobal\n");
            var start = new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "WindowsPowerShell", "v1.0", "powershell.exe"))
            {
                UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true,
                WorkingDirectory = Path.GetTempPath()
            };
            foreach (var argument in new[] { "-NoProfile", "-NonInteractive", "-File", Path.Combine(dir, "BuildScript", "scaffold", "New-TitanPlugin.ps1"), "-Name", "Portable", "-Category", category, "-SolutionPath", solution })
                start.ArgumentList.Add(argument);
            using var process = Process.Start(start)!;
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(60000)) { process.Kill(true); throw new TimeoutException("插件模板生成超时"); }
            Assert.That(process.ExitCode, Is.Zero, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
            var project = Path.Combine(dir, "Plugins", category, "Portable", "Portable.All", "Portable.All.csproj");
            var result = Run("msbuild", project, "-nologo", "-p:TitanPlatform=" + platform, "-getProperty:TitanEngineAssemblyName,TtTitanRoot", "-getItem:ProjectReference,Reference");
            Assert.That(result.Code, Is.Zero, result.Log);
            using var json = JsonDocument.Parse(result.Log);
            Assert.That(json.RootElement.GetProperty("Properties").GetProperty("TitanEngineAssemblyName").GetString(), Is.EqualTo(engine));
            Assert.That(json.RootElement.GetProperty("Properties").GetProperty("TtTitanRoot").GetString(), Is.EqualTo(Path.TrimEndingDirectorySeparator(dir) + Path.DirectorySeparatorChar));
            var references = json.RootElement.GetProperty("Items").GetProperty("ProjectReference").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
            Assert.That(references, Has.Some.EndsWith("/Module/" + engine + "/" + engine + ".csproj"));
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestCase("Mac", "Engine.Mac")]
    [TestCase("Windows", "Engine.Window")]
    public void TitanCmd_ReferencesSelectedPlatformEngine(string platform, string expected)
    {
        var result = Run("msbuild", Path.Combine(Root(), "Module/TitanCMD/TitanCMD.csproj"), "-nologo", "-p:TitanPlatform=" + platform, "-getItem:ProjectReference");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var refs = json.RootElement.GetProperty("Items").GetProperty("ProjectReference").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
        Assert.That(refs, Has.Some.EndsWith("/" + expected + "/" + expected + ".csproj"));
        Assert.That(refs.Count(p => p.Contains("/Engine.")), Is.EqualTo(1));
    }

    [Test]
    public void MainEditor_DefaultsToHostPlatform()
    {
        var result = Run("msbuild", Path.Combine(Root(), "Module/MainEditor/MainEditor.csproj"), "-nologo", "-getProperty:TitanPlatform,TitanPlatformPropsImported", "-getItem:ProjectReference");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var props = json.RootElement.GetProperty("Properties");
        // 平台默认值必须来自 TitanPlatform.props 的主机探测，不能在工程里写死。
        Assert.That(props.GetProperty("TitanPlatformPropsImported").GetString(), Is.EqualTo("true"));
        var host = OperatingSystem.IsMacOS() ? "Mac" : "Windows";
        var engine = host == "Mac" ? "Engine.Mac" : "Engine.Window";
        Assert.That(props.GetProperty("TitanPlatform").GetString(), Is.EqualTo(host));
        var refs = json.RootElement.GetProperty("Items").GetProperty("ProjectReference").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
        Assert.That(refs, Has.Some.EndsWith("/" + engine + "/" + engine + ".csproj"));
    }

    [TestCase("Mac", "Engine.Mac", "PMac", "PWindow")]
    [TestCase("Windows", "Engine.Window", "PWindow", "PMac")]
    public void MainEditor_UsesSelectedPlatformEngineAndSymbols(string platform, string expected, string symbol, string banned)
    {
        var result = Run("msbuild", Path.Combine(Root(), "Module/MainEditor/MainEditor.csproj"), "-nologo", "-p:TitanPlatform=" + platform, "-getProperty:DefineConstants,PlatformTarget", "-getItem:ProjectReference");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var props = json.RootElement.GetProperty("Properties");
        var defines = props.GetProperty("DefineConstants").GetString()!.Split(';');
        Assert.That(defines, Has.Member(symbol).And.No.Member(banned));
        // arm64 mac 载入 AMD64 标记的入口程序集会 FileLoadException，x64 只给 Windows。
        if (platform == "Mac") Assert.That(defines, Has.Member("TITAN_HAS_VULKAN"), "Mac 需与引擎一致注入原生能力宏");
        Assert.That(props.GetProperty("PlatformTarget").GetString(), platform == "Mac" ? Is.Not.EqualTo("x64") : Is.EqualTo("x64"));
        var refs = json.RootElement.GetProperty("Items").GetProperty("ProjectReference").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
        Assert.That(refs, Has.Some.EndsWith("/" + expected + "/" + expected + ".csproj"));
        Assert.That(refs.Count(p => p.Contains("/Engine.")), Is.EqualTo(1));
    }

    [TestCase("Mac", "Engine.Mac")]
    [TestCase("Windows", "Engine.Window")]
    public void AloneGame_ReferencesSelectedPlatformEngine(string platform, string expected)
    {
        var result = Run("msbuild", Path.Combine(Root(), "Module/SlateApp/AloneGame/AloneGame.csproj"), "-nologo", "-p:TitanPlatform=" + platform, "-getItem:ProjectReference");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var refs = json.RootElement.GetProperty("Items").GetProperty("ProjectReference").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
        Assert.That(refs, Has.Some.EndsWith("/" + expected + "/" + expected + ".csproj"));
        Assert.That(refs.Count(p => p.Contains("/Engine.")), Is.EqualTo(1));
    }

    [Test]
    public void AndroidEngine_EvaluatesExplicitPlatformAndIsolatedOutput()
    {
        var project = Path.Combine(Root(), "Module/Engine.Android/Engine.Android.csproj");
        var result = Run("msbuild", project, "-nologo", "-p:TitanPlatform=Android", "-p:SkipTitanCodeGen=true",
            "-getProperty:TitanPlatform,TargetFramework,TitanPlatformDefine,TitanCodeGenRoot,OutputPath,AssemblyName");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var p = json.RootElement.GetProperty("Properties");
        Assert.That(p.GetProperty("TitanPlatform").GetString(), Is.EqualTo("Android"));
        Assert.That(p.GetProperty("TargetFramework").GetString(), Is.EqualTo("net8.0"));
        Assert.That(p.GetProperty("TitanPlatformDefine").GetString(), Is.EqualTo("PAndroid"));
        Assert.That(p.GetProperty("AssemblyName").GetString(), Is.EqualTo("Engine.Android"));
        Assert.That(p.GetProperty("OutputPath").GetString()!.Replace('\\', '/'), Does.EndWith("binaries/android/net8.0/"));
        Assert.That(p.GetProperty("TitanCodeGenRoot").GetString()!.Replace('\\', '/'), Does.EndWith("codegen/Android"));
    }

    [Test]
    public void AndroidBuild_RequiresRefreshedCapabilities()
    {
        // 能力清单只能由 Core.Android 的 CMake 刷新入口生成；缺失时不得静默继续编译。
        var missing = Path.Combine(Path.GetTempPath(), "TitanAndroidCapabilities-" + Guid.NewGuid().ToString("N") + ".props");
        var result = Run("msbuild", Path.Combine(Root(), "Module/Engine.Android/Engine.Android.csproj"), "-nologo",
            "-t:TitanValidateAndroidCapabilities", "-p:TitanPlatform=Android", "-p:SkipTitanCodeGen=true",
            "-p:TitanAndroidCapabilitiesFile=" + missing);
        Assert.That(result.Code, Is.Not.Zero, "缺少能力清单时不能继续编译");
        Assert.That(result.Log, Does.Contain("RefreshSources"));
    }

    [Test]
    public void AndroidEngine_CompileClosureUsesSharedFileManagerAndAndroidWindow()
    {
        var project = Path.Combine(Root(), "Module/Engine.Android/Engine.Android.csproj");
        var result = Run("msbuild", project, "-nologo", "-p:TitanPlatform=Android", "-p:SkipTitanCodeGen=true",
            "-getItem:Compile");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = JsonDocument.Parse(result.Log);
        var files = json.RootElement.GetProperty("Items").GetProperty("Compile").EnumerateArray()
            .Select(i => i.GetProperty("FullPath").GetString()!.Replace('\\', '/')).ToArray();
        // Android 复用 Windows 的 FileManager 实现，窗口入口换成 Android 静态桩。
        Assert.That(files, Has.Some.EndsWith("/Platform/Windows/FileManager.cs"));
        Assert.That(files, Has.Some.EndsWith("/Platform/Android/NativeWindowAndroid.cs"));
        foreach (var banned in new[] { "/Platform/Windows/PlatformWindow.cs", "/Platform/Mac/PlatformMac.cs" })
            Assert.That(files, Has.None.EndsWith(banned));
    }
}
