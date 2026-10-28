using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Titan.Tests.CSharpCodeTools;

[TestFixture]
public class SourceInputsTests
{
    private static global::CSharpCodeTools.ProjectSourceInputs Load(string project, string platform, string configuration)
    {
        return global::CSharpCodeTools.ProjectSourceInputs.Load(project, platform, configuration);
    }

    private sealed class Fixture : IDisposable
    {
        public string DirectoryPath { get; }
        public string Project => Path.Combine(DirectoryPath, "Engine Fixture.csproj");
        public Fixture(string ancestor = "")
        {
            DirectoryPath = Path.Combine(Path.GetTempPath(), ancestor, "Titan Source Inputs " + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DirectoryPath);
            Write("Main.cs", "public class Main {}");
            Write("Removed.cs", "#error 不应参与生成");
            Write("Shared/Item.cs", "#if PMac && NET8_0_OR_GREATER\npublic class MacOnly {}\n#else\npublic class WindowsOnly {}\n#endif");
            Write("Shared/Portable.projitems", "<Project><ItemGroup><Compile Include=\"$(MSBuildThisFileDirectory)Item.cs\" /></ItemGroup></Project>");
            Write("Chosen.props", "<Project><PropertyGroup><DefineConstants>$(DefineConstants);IMPORTED</DefineConstants></PropertyGroup></Project>");
            Write("Ignored.props", "<Project><PropertyGroup><DefineConstants>$(DefineConstants);WRONG_BRANCH</DefineConstants></PropertyGroup></Project>");
            Write("Engine Fixture.csproj", """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net8.0</TargetFramework>
                    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
                    <LangVersion>12.0</LangVersion>
                    <AssemblyName>Fixture.Engine</AssemblyName>
                    <DefineConstants Condition="'$(TitanPlatform)' == 'Mac'">$(DefineConstants);PMac</DefineConstants>
                    <DefineConstants Condition="'$(TitanPlatform)' == 'Windows'">$(DefineConstants);PWindow</DefineConstants>
                  </PropertyGroup>
                  <Import Project="Chosen.props" Condition="'$(Configuration)' == 'Debug'" />
                  <Import Project="Ignored.props" Condition="'$(Configuration)' == 'Release'" />
                  <Import Project="Shared/Portable.projitems" />
                  <ItemGroup>
                    <Compile Include="Main.cs;Removed.cs;codegen/Windows/Rpc/Engine/Missing.gen.cs;obj/Missing.g.cs;bin/Missing.cs" />
                    <Compile Remove="Removed.cs" />
                    <ProjectReference Include="MissingParent.csproj" />
                  </ItemGroup>
                  <Target Name="Build"><Error Text="不允许编译引擎" /></Target>
                  <Target Name="Restore"><Error Text="不允许还原引擎" /></Target>
                </Project>
                """);
        }
        public void Write(string relative, string content)
        {
            var path = Path.Combine(DirectoryPath, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }
        public void Dispose() { Directory.Delete(DirectoryPath, true); }
    }

    [Test]
    public void Load_UsesEvaluatedCompileAndSdkSymbolsWithoutBuildRestoreOrReferences()
    {
        using var fixture = new Fixture();
        dynamic inputs = Load(fixture.Project, "Mac", "Debug");
        string[] files = inputs.SourceFiles;
        Assert.That(files, Is.EquivalentTo(new[] { Path.Combine(fixture.DirectoryPath, "Main.cs"), Path.Combine(fixture.DirectoryPath, "Shared", "Item.cs") }));
        Assert.That((string)inputs.ProjectFile, Is.EqualTo(fixture.Project));
        Assert.That((string)inputs.AssemblyName, Is.EqualTo("Fixture.Engine"));
        CSharpParseOptions options = inputs.ParseOptions;
        Assert.That(options.LanguageVersion, Is.EqualTo(LanguageVersion.CSharp12));
        Assert.That(options.PreprocessorSymbolNames, Does.Contain("PMac").And.Contain("IMPORTED").And.Contain("DEBUG").And.Contain("NET8_0").And.Contain("NET8_0_OR_GREATER").And.Not.Contain("PWindow"));
        var root = CSharpSyntaxTree.ParseText(File.ReadAllText(files.Single(p => p.EndsWith("Item.cs"))), options).GetRoot();
        Assert.That(root.DescendantNodes().OfType<ClassDeclarationSyntax>().Select(c => c.Identifier.Text), Is.EqualTo(new[] { "MacOnly" }));
        string[] imports = inputs.ImportedFiles;
        Assert.That(imports, Does.Contain(Path.Combine(fixture.DirectoryPath, "Chosen.props")).And.Contain(Path.Combine(fixture.DirectoryPath, "Shared", "Portable.projitems")).And.Not.Contain(Path.Combine(fixture.DirectoryPath, "Ignored.props")));
        Assert.That(imports.Any(p => p.EndsWith("Microsoft.NET.Sdk.targets")), Is.True, "需要实际 SDK imports 参与指纹");
        Assert.That(Directory.Exists(Path.Combine(fixture.DirectoryPath, "obj")), Is.False, "采集不能运行构建或还原");
    }

    [Test]
    public void Load_ReevaluatesConfigurationAndSourceMembership()
    {
        using var fixture = new Fixture();
        dynamic debug = Load(fixture.Project, "Windows", "Debug");
        CSharpParseOptions debugOptions = debug.ParseOptions;
        Assert.That(debugOptions.PreprocessorSymbolNames, Does.Contain("PWindow").And.Contain("IMPORTED").And.Contain("DEBUG"));
        fixture.Write("Main.cs", "public class Changed {}");
        dynamic release = Load(fixture.Project, "Mac", "Release");
        CSharpParseOptions releaseOptions = release.ParseOptions;
        Assert.That(releaseOptions.PreprocessorSymbolNames, Does.Contain("PMac").And.Contain("WRONG_BRANCH").And.Not.Contain("IMPORTED").And.Not.Contain("DEBUG"));
        fixture.Write("Shared/Portable.projitems", "<Project />");
        dynamic changed = Load(fixture.Project, "Mac", "Release");
        Assert.That((string[])changed.SourceFiles, Is.EqualTo(new[] { Path.Combine(fixture.DirectoryPath, "Main.cs") }));
    }

    [TestCase("bin")]
    [TestCase("obj")]
    public void Load_DoesNotExcludeOrdinarySourcesByAncestorDirectory(string ancestor)
    {
        using var fixture = new Fixture(ancestor);
        var inputs = Load(fixture.Project, "Mac", "Debug");
        Assert.That(inputs.SourceFiles, Does.Contain(Path.Combine(fixture.DirectoryPath, "Main.cs")));
    }

    [Test]
    public void Load_AllowsEmptyCompileSet()
    {
        using var fixture = new Fixture();
        fixture.Write("Engine Fixture.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup></Project>");
        Assert.That(Load(fixture.Project, "Mac", "Debug").SourceFiles, Is.Empty);
    }

    [Test]
    public void Load_PreservesCustomCommonTargetsAndTheirSymbols()
    {
        using var fixture = new Fixture();
        fixture.Write("Custom.targets", "<Project><PropertyGroup><DefineConstants>$(DefineConstants);CUSTOM_IMPORT</DefineConstants></PropertyGroup><ItemGroup><Compile Include=\"$(MSBuildThisFileDirectory)Custom.cs\" /></ItemGroup></Project>");
        fixture.Write("Custom.cs", "public class Custom {}");
        fixture.Write("Engine Fixture.csproj", File.ReadAllText(fixture.Project).Replace("<TargetFramework>", "<CustomAfterMicrosoftCommonTargets>$(MSBuildThisFileDirectory)Custom.targets</CustomAfterMicrosoftCommonTargets><TargetFramework>"));
        var inputs = Load(fixture.Project, "Mac", "Debug");
        Assert.That(inputs.SourceFiles, Does.Contain(Path.Combine(fixture.DirectoryPath, "Custom.cs")));
        Assert.That(inputs.ParseOptions.PreprocessorSymbolNames, Does.Contain("CUSTOM_IMPORT"));
        Assert.That(inputs.ImportedFiles, Does.Contain(Path.Combine(fixture.DirectoryPath, "Custom.targets")));
    }

    [Test]
    public void LoadFromRoot_WindowsPathCasingDoesNotChangeInputs()
    {
        if (!OperatingSystem.IsWindows()) Assert.Ignore("Windows 文件系统路径身份契约");
        using var fixture = new Fixture();
        var root = ManagedProjectTests.Root();
        var upper = global::CSharpCodeTools.ProjectSourceInputs.LoadFromRoot(root.ToUpperInvariant(), fixture.Project, "Mac", "Debug");
        var lower = global::CSharpCodeTools.ProjectSourceInputs.LoadFromRoot(root.ToLowerInvariant(), fixture.Project, "Mac", "Debug");
        Assert.That(lower.SourceFiles, Is.EqualTo(upper.SourceFiles));
    }

    [Test]
    public void Load_ExcludesExplicitGeneratedMetadata()
    {
        using var fixture = new Fixture();
        fixture.Write("Engine Fixture.csproj", File.ReadAllText(fixture.Project).Replace("<Compile Remove=", "<Compile Include=\"Missing.Generated.cs\" TitanGenerated=\"True\" /><Compile Remove="));
        Assert.That(Load(fixture.Project, "Mac", "Debug").SourceFiles, Has.None.EndsWith("Missing.Generated.cs"));
    }

    [Test]
    public void LoadPlatformProjects_UsesDescriptorAndSeparatePluginParsingContexts()
    {
        using var fixture = new Fixture();
        File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(fixture.DirectoryPath, "BuildScript.tmp"));
        fixture.Write("BuildScript/msbuild/TitanSourceInputs.targets", File.ReadAllText(Path.Combine(fixture.DirectoryPath, "BuildScript.tmp")));
        foreach (var (name, descriptorPlatform, suffix, symbol) in new[]
        {
            ("Portable", "PLTF_ALL", "All", "PLUGIN_ONLY"),
            ("WindowsOnly", "PLTF_Windows", "Window", "WINDOWS_ONLY"),
            ("MacOnly", "PLTF_Mac", "Mac", "MAC_ONLY")
        })
        {
            fixture.Write($"Plugins/{name}/{name}.plugin", "{\"Enable\":true,\"Platforms\":[\"" + descriptorPlatform + "\"]}");
            fixture.Write($"Plugins/{name}/{name}.{suffix}/{name}.{suffix}.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><DefineConstants>$(DefineConstants);" + symbol + "</DefineConstants></PropertyGroup></Project>");
            fixture.Write($"Plugins/{name}/{name}.{suffix}/Own.cs", "public class Own {}");
        }
        fixture.Write("Plugins/bin/Ignored.plugin", "invalid descriptor in deployment output");
        var method = typeof(global::CSharpCodeTools.ProjectSourceInputs).GetMethod("LoadPlatformProjects");
        Assert.That(method, Is.Not.Null, "缺少按平台描述文件选择并分别求值工程的入口");
        var projects = (global::CSharpCodeTools.ProjectSourceInputs[])method!.Invoke(null, new object[] { fixture.DirectoryPath, fixture.Project, "Mac", "Debug" })!;
        Assert.That(projects.Select(p => Path.GetFileName(p.ProjectFile)), Is.EqualTo(new[] { "Engine Fixture.csproj", "MacOnly.Mac.csproj", "Portable.All.csproj" }));
        Assert.That(projects[0].ParseOptions.PreprocessorSymbolNames, Does.Not.Contain("PLUGIN_ONLY"));
        Assert.That(projects[2].ParseOptions.PreprocessorSymbolNames, Does.Contain("PLUGIN_ONLY").And.Not.Contain("MAC_ONLY").And.Not.Contain("IMPORTED"));
        Assert.That(projects[0].SourceFiles, Has.None.EndsWith("Own.cs"));
        Assert.That(projects[2].ImportedFiles, Does.Contain(Path.Combine(fixture.DirectoryPath, "Plugins", "Portable", "Portable.plugin")));
    }

    [TestCase("Linux", "Debug")]
    [TestCase("Mac", "Profile")]
    public void Load_RejectsUnknownPlatformOrConfiguration(string platform, string configuration)
    {
        using var fixture = new Fixture();
        Assert.Throws<ArgumentException>(() => Load(fixture.Project, platform, configuration));
    }

    [Test]
    public void Load_AcceptsAndroidPlatform()
    {
        using var fixture = new Fixture();
        var inputs = Load(fixture.Project, "Android", "Debug");
        Assert.That(inputs.Platform, Is.EqualTo("Android"));
        Assert.That(inputs.SourceFiles, Does.Contain(Path.Combine(fixture.DirectoryPath, "Main.cs")));
    }

    [Test]
    public void LoadPlatformProjects_SelectsAndroidDescriptorAndSuffix()
    {
        using var fixture = new Fixture();
        fixture.Write("BuildScript/msbuild/TitanSourceInputs.targets", File.ReadAllText(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets")));
        foreach (var (name, descriptorPlatform, suffix, symbol) in new[]
        {
            ("DroidOnly", "PLTF_Android", "Android", "ANDROID_ONLY"),
            ("MacOnly", "PLTF_Mac", "Mac", "MAC_ONLY"),
            ("Portable", "PLTF_ALL", "All", "PLUGIN_ONLY")
        })
        {
            fixture.Write($"Plugins/{name}/{name}.plugin", "{\"Enable\":true,\"Platforms\":[\"" + descriptorPlatform + "\"]}");
            fixture.Write($"Plugins/{name}/{name}.{suffix}/{name}.{suffix}.csproj", "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><DefineConstants>$(DefineConstants);" + symbol + "</DefineConstants></PropertyGroup></Project>");
            fixture.Write($"Plugins/{name}/{name}.{suffix}/Own.cs", "public class Own {}");
        }
        var method = typeof(global::CSharpCodeTools.ProjectSourceInputs).GetMethod("LoadPlatformProjects");
        Assert.That(method, Is.Not.Null, "缺少按平台描述文件选择并分别求值工程的入口");
        var android = (global::CSharpCodeTools.ProjectSourceInputs[])method!.Invoke(null, new object[] { fixture.DirectoryPath, fixture.Project, "Android", "Debug" })!;
        Assert.That(android.Select(p => Path.GetFileName(p.ProjectFile)), Is.EqualTo(new[] { "Engine Fixture.csproj", "DroidOnly.Android.csproj", "Portable.All.csproj" }));
        Assert.That(android[1].ParseOptions.PreprocessorSymbolNames, Does.Contain("ANDROID_ONLY").And.Not.Contain("MAC_ONLY"));
        Assert.That(android[2].ParseOptions.PreprocessorSymbolNames, Does.Contain("PLUGIN_ONLY").And.Not.Contain("ANDROID_ONLY"));
        Assert.That(android[1].SourceFiles, Has.Some.EndsWith("Own.cs"));
        var mac = (global::CSharpCodeTools.ProjectSourceInputs[])method!.Invoke(null, new object[] { fixture.DirectoryPath, fixture.Project, "Mac", "Debug" })!;
        Assert.That(mac.Select(p => Path.GetFileName(p.ProjectFile)), Is.EqualTo(new[] { "Engine Fixture.csproj", "MacOnly.Mac.csproj", "Portable.All.csproj" }));
        // Android 工程不得被其他平台悄悄复用。
        Assert.That(mac.Select(p => p.ProjectFile), Has.None.EndsWith("DroidOnly.Android.csproj"));
    }

    [Test]
    public void Load_FailsOnMissingOrdinarySource()
    {
        using var fixture = new Fixture();
        File.Delete(Path.Combine(fixture.DirectoryPath, "Main.cs"));
        var error = Assert.Throws<InvalidOperationException>(() => Load(fixture.Project, "Mac", "Debug"));
        Assert.That(error!.Message, Does.Contain("Main.cs"));
    }

    [Test]
    public void Load_PreservesOriginalMissingImportDiagnostic()
    {
        using var fixture = new Fixture();
        File.Delete(Path.Combine(fixture.DirectoryPath, "Chosen.props"));
        var error = Assert.Throws<InvalidOperationException>(() => Load(fixture.Project, "Mac", "Debug"));
        Assert.That(error!.Message, Does.Contain("MSB4019").And.Contain("Chosen.props"));
    }

    [TestCase("Windows", "Engine.Window")]
    [TestCase("Mac", "Engine.Mac")]
    public void Load_SelectsRealDefaultEngineWithoutNativeBuild(string platform, string expected)
    {
        dynamic inputs = Load("", platform, "Debug");
        Assert.That((string)inputs.ProjectFile, Is.EqualTo(Path.Combine(ManagedProjectTests.Root(), "Module", expected, expected + ".csproj")));
        Assert.That((string[])inputs.SourceFiles, Has.Some.EndsWith("Platform" + Path.DirectorySeparatorChar + (platform == "Mac" ? "Mac" : "Windows") + Path.DirectorySeparatorChar + (platform == "Mac" ? "PlatformMac.cs" : "PlatformWindow.cs")));
        Assert.That((string[])inputs.SourceFiles, Has.None.EndsWith("TitanMacCapabilities.cs"));
        Assert.That(((string[])inputs.SourceFiles).Select(p => Path.GetRelativePath(ManagedProjectTests.Root(), p)), Has.None.StartsWith("codegen"));
    }

    [Test]
    public void Load_RejectsRealEnginePlatformMismatch()
    {
        var error = Assert.Throws<InvalidOperationException>(() => Load(Path.Combine(ManagedProjectTests.Root(), "Module/Engine.Window/Engine.Window.csproj"), "Mac", "Debug"));
        Assert.That(error!.Message, Does.Contain("Engine.Window").And.Contain("Windows"));
    }
}
