using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using EngineNS.Editor.AndroidBuild;

namespace Titan.Tests.CSharpCodeTools.AndroidBuild;

[TestFixture]
public class TtAndroidBuildCommandLineTests
{
    private string mRepoRoot;

    [SetUp]
    public void SetUp()
    {
        mRepoRoot = Path.Combine(Path.GetTempPath(), "titan-req-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(mRepoRoot);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(mRepoRoot)) Directory.Delete(mRepoRoot, true);
    }

    private TtAndroidBuildRequest MakeRequest()
    {
        return new TtAndroidBuildRequest
        {
            RepositoryRoot = mRepoRoot,
            Configuration = "Release",
            ToolchainConfig = Path.Combine(mRepoRoot, "cache", "config", "android-toolchain.json"),
            CookFirst = false,
        };
    }

    [Test]
    public void Check_BuildsQuotedPowershellInvocation()
    {
        var cmd = TtAndroidBuildCommandLine.Check(MakeRequest(), "Managed");
        Assert.That(cmd.StartsWith("\"powershell.exe\""), Is.True, cmd);
    }

    [Test]
    public void Check_ContainsScriptPathStageAndConfig()
    {
        var cmd = TtAndroidBuildCommandLine.Check(MakeRequest(), "Managed");
        Assert.Multiple(() =>
        {
            Assert.That(cmd, Does.Contain("-NoProfile -NonInteractive -ExecutionPolicy Bypass"));
            Assert.That(cmd, Does.Contain(@"BuildScript\check\check-android-toolchain.ps1"));
            Assert.That(cmd, Does.Contain("-Stage Managed"));
            Assert.That(cmd, Does.Contain("-ConfigPath \"" + mRepoRoot + "\\cache\\config\\android-toolchain.json\""));
        });
    }

    [Test]
    public void Check_EmptyToolchainConfig_OmitsConfigPath()
    {
        var request = MakeRequest();
        request.ToolchainConfig = string.Empty;
        var cmd = TtAndroidBuildCommandLine.Check(request, "Native");
        Assert.That(cmd, Does.Contain("-Stage Native"));
        Assert.That(cmd, Does.Not.Contain("-ConfigPath"));
    }

    [Test]
    public void Apk_ContainsBuildScriptStageAndConfig()
    {
        var cmd = TtAndroidBuildCommandLine.Apk(MakeRequest());
        Assert.Multiple(() =>
        {
            Assert.That(cmd, Does.Contain(@"BuildScript\android\build-android.ps1"));
            Assert.That(cmd, Does.Contain("-Configuration Release -Stage Apk"));
            Assert.That(cmd, Does.Contain("-ToolchainConfig \"" + mRepoRoot + "\\cache\\config\\android-toolchain.json\""));
        });
    }

    [Test]
    public void Cook_ContainsTitanCmdAndCookArgs()
    {
        var request = MakeRequest();
        request.CookFirst = true;
        request.CookConfig = "content/config/engineconfig_windows_cook.jscfg";
        request.CookBlacklist = Path.Combine(mRepoRoot, "cache", "cook_blacklist.txt");
        var cmd = TtAndroidBuildCommandLine.Cook(request);
        Assert.Multiple(() =>
        {
            Assert.That(cmd, Does.Contain("TitanCMD.exe"));
            Assert.That(cmd, Does.Contain("ExeCmd=SaveAsLastest"));
            Assert.That(cmd, Does.Contain("CookCfg=\"" + mRepoRoot + "\\content\\config\\engineconfig_windows_cook.jscfg\""));
            Assert.That(cmd, Does.Contain("Blacklist=\"" + mRepoRoot + "\\cache\\cook_blacklist.txt\""));
        });
    }

    [Test]
    public void Cook_WithoutBlacklist_OmitsBlacklist()
    {
        var cmd = TtAndroidBuildCommandLine.Cook(MakeRequest());
        Assert.That(cmd, Does.Contain("CookCfg="));
        Assert.That(cmd, Does.Not.Contain("Blacklist="));
    }

    [Test]
    public void Validate_ReportsMissingRootAndConfig()
    {
        var request = new TtAndroidBuildRequest { RepositoryRoot = string.Empty, Configuration = string.Empty };
        var errors = request.Validate();
        Assert.Multiple(() =>
        {
            Assert.That(errors.Any(e => e.Contains("仓库根")), Is.True);
            Assert.That(errors.Any(e => e.Contains("配置")), Is.True, string.Join(";", errors));
        });
    }

    [Test]
    public void Validate_ValidRequestIsEmpty()
    {
        Assert.That(MakeRequest().Validate(), Is.Empty);
    }

    [Test]
    public void ExtractApkPath_TakesLastApkLine()
    {
        var log = "some log\nAPK=C:\\first.apk\nmore\nAPK=C:\\repo\\out.apk\ndone";
        Assert.That(TtAndroidBuildLogParser.ExtractApkPath(log),
            Is.EqualTo(@"C:\repo\out.apk"));
    }

    [Test]
    public void ExtractApkPath_ReturnsNullWithoutMarker()
    {
        Assert.That(TtAndroidBuildLogParser.ExtractApkPath("no marker here"), Is.Null);
    }

    [Test]
    public void ParseStageMarker_ReturnsStageName()
    {
        Assert.That(TtAndroidBuildLogParser.ParseStageMarker("[TITAN-STAGE] Native"), Is.EqualTo("Native"));
        Assert.That(TtAndroidBuildLogParser.ParseStageMarker("  [TITAN-STAGE] Content  "), Is.EqualTo("Content"));
    }

    [Test]
    public void ParseStageMarker_ReturnsNullForNonMarkerLines()
    {
        Assert.That(TtAndroidBuildLogParser.ParseStageMarker("dotnet build output"), Is.Null);
        Assert.That(TtAndroidBuildLogParser.ParseStageMarker("[TITAN-STAGE]"), Is.Null);
        Assert.That(TtAndroidBuildLogParser.ParseStageMarker("[TITAN-STAGE] Unknown"), Is.Null);
        Assert.That(TtAndroidBuildLogParser.ParseStageMarker(null), Is.Null);
    }

    [Test]
    public void FindLastStageMarker_TakesLastRecognizedMarker()
    {
        var log = "[TITAN-STAGE] Native\nbuild...\nnoise\n[TITAN-STAGE] Managed\nmore";
        Assert.That(TtAndroidBuildLogParser.FindLastStageMarker(log), Is.EqualTo("Managed"));
        Assert.That(TtAndroidBuildLogParser.FindLastStageMarker("no markers"), Is.Null);
    }

    [TestCase("error MSB1234", true)]
    [TestCase("Build FAILED.", true)]
    [TestCase("发生错误：找不到文件", true)]
    [TestCase("构建失败", true)]
    [TestCase("Unhandled exception", true)]
    [TestCase("一切正常", false)]
    public void LooksLikeFailure_DetectsFailureLines(string line, bool expected)
    {
        Assert.That(TtAndroidBuildLogParser.LooksLikeFailure(line), Is.EqualTo(expected));
    }
}
