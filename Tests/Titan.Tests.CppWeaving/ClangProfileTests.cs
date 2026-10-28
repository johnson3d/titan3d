using CppWeaving;
using CppWeaving.Cpp2CS;
using NUnit.Framework;

namespace Titan.Tests.CppWeaving;

[TestFixture]
[NonParallelizable]
public class ClangProfileTests
{
    private string mTempDir = "";

    [SetUp]
    public void SetUp()
    {
        mTempDir = Path.Combine(Path.GetTempPath(), "TitanCppWeaving_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(mTempDir);
        ParseFailures.Clear();
    }

    [TearDown]
    public void TearDown()
    {
        ParseFailures.Clear();
        Directory.Delete(mTempDir, true);
    }

    [Test]
    public void WindowsArgs_MatchHistoricalCommandLine()
    {
        var args = ClangArgs.Build(new ClangProfile { Platform = TargetPlatform.Windows }, new[] { "A" }, new[] { "X" });

        Assert.That(args, Is.EqualTo(new[]
        {
            "-std=c++17", "-xc++", "-Wno-pragma-once-outside-header", "-Wno-address-of-temporary",
            "-m64", "-target", "x86_64-pc-windows-msvc", "-fms-extensions", "-fms-compatibility",
            "-D_M_X64=1", "-D__EMSCRIPTEN__", "-IA", "-DX",
        }));
    }

    [Test]
    public void MacArgs_TargetAppleSiliconWithSdkAndResourceDir()
    {
        var profile = new ClangProfile { Platform = TargetPlatform.Mac, SysRoot = "/sdk", ResourceDir = "/res" };

        var args = ClangArgs.Build(profile, new[] { "A" }, new[] { "X" });

        Assert.That(args, Is.EqualTo(new[]
        {
            "-std=c++17", "-xc++", "-Wno-pragma-once-outside-header", "-Wno-address-of-temporary",
            "-target", "arm64-apple-macosx11.0", "-stdlib=libc++",
            "-isysroot", "/sdk", "-resource-dir", "/res", "-IA", "-DX",
        }));
    }

    [Test]
    public void MacArgs_OmitEmptySysRootAndResourceDir()
    {
        var args = ClangArgs.Build(new ClangProfile { Platform = TargetPlatform.Mac }, Array.Empty<string>(), Array.Empty<string>());

        Assert.That(args, Does.Not.Contain("-isysroot"));
        Assert.That(args, Does.Not.Contain("-resource-dir"));
    }

    [Test]
    public void AndroidArgs_UseNdkArm64Api28()
    {
        var args = ClangArgs.Build(new ClangProfile
        {
            Platform = TargetPlatform.Android,
            SysRoot = "E:/NDK/sysroot",
            ResourceDir = "E:/NDK/lib/clang/18"
        }, new[] { "A" }, new[] { "PLATFORM_DROID" });

        Assert.That(args, Does.Contain("--target=aarch64-linux-android28"));
        Assert.That(args, Does.Contain("--sysroot=E:/NDK/sysroot"));
        Assert.That(args, Does.Contain("-resource-dir"));
        Assert.That(args, Does.Contain("E:/NDK/lib/clang/18"));
        Assert.That(args, Does.Not.Contain("-fms-compatibility"));
        Assert.That(args, Does.Not.Contain("-D_M_X64=1"));
        Assert.That(ClangArgs.ParsePlatform("android"), Is.EqualTo(TargetPlatform.Android));
        Assert.That(ClangArgs.IsHostOnlyInclude(TargetPlatform.Android, "../pthread/include"), Is.True);
    }

    [TestCase(TargetPlatform.Mac, "../3rd/native/pthread/include", true)]
    [TestCase(TargetPlatform.Mac, "..\\3rd\\native\\ATLMFC\\Include\\", true)]
    [TestCase(TargetPlatform.Mac, "$(WindowsSDK_IncludePath)", true)]
    [TestCase(TargetPlatform.Mac, "../3rd/native/boost-1.89.0", false)]
    [TestCase(TargetPlatform.Windows, "../3rd/native/pthread/include", false)]
    [TestCase(TargetPlatform.Windows, "$(WindowsSDK_IncludePath)", false)]
    public void IsHostOnlyInclude_FiltersWindowsOnlyDirsOnMac(TargetPlatform platform, string include, bool expected)
    {
        Assert.That(ClangArgs.IsHostOnlyInclude(platform, include), Is.EqualTo(expected));
    }

    [TestCase("Mac", TargetPlatform.Mac)]
    [TestCase("mac", TargetPlatform.Mac)]
    [TestCase("Windows", TargetPlatform.Windows)]
    [TestCase("Android", TargetPlatform.Android)]
    [TestCase("android", TargetPlatform.Android)]
    public void ParsePlatform_IsCaseInsensitive(string text, TargetPlatform expected)
    {
        Assert.That(ClangArgs.ParsePlatform(text), Is.EqualTo(expected));
    }

    [Test]
    public void ParsePlatform_DefaultsToHost()
    {
        var host = OperatingSystem.IsMacOS() ? TargetPlatform.Mac : TargetPlatform.Windows;

        Assert.That(ClangArgs.ParsePlatform(null), Is.EqualTo(host));
        Assert.That(ClangArgs.ParsePlatform(""), Is.EqualTo(host));
    }

    [Test]
    public void ParsePlatform_RejectsUnknown()
    {
        Assert.Throws<ArgumentException>(() => ClangArgs.ParsePlatform("Linux"));
    }

    [TestCase(TargetPlatform.Windows, "long", "int", "int", "int")]
    [TestCase(TargetPlatform.Windows, "unsigned long", "unsigned int", "uint", "unsigned int")]
    [TestCase(TargetPlatform.Mac, "long", "long", "long", "long")]
    [TestCase(TargetPlatform.Mac, "unsigned long", "unsigned long", "ulong", "unsigned long")]
    [TestCase(TargetPlatform.Android, "long", "long", "long", "long")]
    [TestCase(TargetPlatform.Android, "unsigned long", "unsigned long", "ulong", "unsigned long")]
    public void SystemTypes_MapLongByDataModel(TargetPlatform platform, string type, string cpp, string cs, string pod)
    {
        var kls = (USystemTypeStruct)new USysClassManager(platform).FindClass(type);

        Assert.That(kls.CppName, Is.EqualTo(cpp));
        Assert.That(kls.CSName, Is.EqualTo(cs));
        Assert.That(kls.RetPodName, Is.EqualTo(pod));
    }

    [Test]
    public void Collect_OnMac_DropsWindowsOnlyIncludesAndVcxprojMacros()
    {
        var proj = WriteVcxproj();

        var collector = new HppCollector();
        collector.Collect(proj, @"'$(Configuration)|$(Platform)'=='Debug|x64'", TargetPlatform.Mac);

        var dir = Path.GetDirectoryName(proj)!.Replace('\\', '/') + "/";
        Assert.That(collector.IncludePath, Is.EqualTo(new[] { Path.GetFullPath(dir + "../inc").Replace('\\', '/') }));
        Assert.That(collector.MacroDefines, Is.Empty);
    }

    [Test]
    public void Collect_OnWindows_KeepsVcxprojIncludesAndMacros()
    {
        var proj = WriteVcxproj();

        var collector = new HppCollector();
        collector.Collect(proj, @"'$(Configuration)|$(Platform)'=='Debug|x64'", TargetPlatform.Windows);

        var dir = Path.GetDirectoryName(proj)!.Replace('\\', '/') + "/";
        Assert.That(collector.IncludePath, Does.Contain(dir + "../inc"));
        Assert.That(collector.IncludePath, Does.Contain(dir + "../pthread/include"));
        Assert.That(collector.MacroDefines, Is.EqualTo(new[] { "PLATFORM_WIN", "_DEBUG" }));
    }

    [Test]
    public void CreateTU_RecordsHeaderThatFailsToParse()
    {
        var hpp = WriteHeader("Broken.h", "// TR_CLASS\nstruct Broken { int a }\n");

        var ok = new TUCreator().CreateTU(hpp, new List<string>(), new List<string>(), HostProfile());

        Assert.That(ok, Is.False);
        Assert.That(ParseFailures.Snapshot().Keys, Is.EquivalentTo(new[] { hpp.File }));
    }

    [Test]
    public void CreateTU_ParsesValidHeaderWithoutRecordingFailure()
    {
        var hpp = WriteHeader("Good.h", "// TR_CLASS\nstruct Good { long a; };\n");

        var ok = new TUCreator().CreateTU(hpp, new List<string>(), new List<string>(), HostProfile());

        Assert.That(ok, Is.True);
        Assert.That(ParseFailures.Snapshot(), Is.Empty);
    }

    [Test]
    public void Report_ReturnsZeroWhenNothingFailed()
    {
        var output = new StringWriter();

        Assert.That(ParseFailures.Report(new Dictionary<string, string>(), false, output), Is.EqualTo(0));
    }

    [Test]
    public void Report_FailsAndListsHeadersWhenNotAllowed()
    {
        var output = new StringWriter();
        var failures = new Dictionary<string, string> { ["a/B.h"] = "error: boom" };

        Assert.That(ParseFailures.Report(failures, false, output), Is.EqualTo(2));
        Assert.That(output.ToString(), Does.Contain("a/B.h").And.Contain("error: boom"));
    }

    [Test]
    public void Report_PassesWhenFailuresAreAllowed()
    {
        var output = new StringWriter();
        var failures = new Dictionary<string, string> { ["a/B.h"] = "error: boom" };

        Assert.That(ParseFailures.Report(failures, true, output), Is.EqualTo(0));
        Assert.That(output.ToString(), Does.Contain("a/B.h"));
    }

    private static ClangProfile HostProfile()
    {
        return new ClangProfile { Platform = ClangArgs.ParsePlatform(null) };
    }

    private HppCollector.HppUnit WriteHeader(string name, string code)
    {
        var file = Path.Combine(mTempDir, name).Replace('\\', '/');
        File.WriteAllText(file, code);
        return new HppCollector.HppUnit { File = file, Module = "Test" };
    }

    private string WriteVcxproj()
    {
        var proj = Path.Combine(mTempDir, "Test.vcxproj");
        File.WriteAllText(proj, """
            <?xml version="1.0" encoding="utf-8"?>
            <Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
              <ImportGroup Label="Shared" />
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|x64'">
                <ClCompile>
                  <AdditionalIncludeDirectories>../inc;../pthread/include;../atlmfc/include;$(WindowsSDK_IncludePath);%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>
                  <PreprocessorDefinitions>PLATFORM_WIN;_DEBUG</PreprocessorDefinitions>
                </ClCompile>
              </ItemDefinitionGroup>
            </Project>
            """);
        return proj;
    }
}
