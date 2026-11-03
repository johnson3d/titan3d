using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using EngineNS.Editor.AndroidBuild;

namespace Titan.Tests.CSharpCodeTools.AndroidBuild;

[TestFixture]
public class TtAndroidToolchainLocatorTests
{
    private string mTempRoot = "";

    [SetUp]
    public void SetUp()
    {
        mTempRoot = Path.Combine(Path.GetTempPath(), "titan-tcl-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(mTempRoot);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(mTempRoot)) Directory.Delete(mTempRoot, true);
    }

    private static string WriteAllText(string path, string text)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        File.WriteAllText(path, text, new UTF8Encoding(false));
        return path;
    }

    [Test]
    public void TryParseNdkRevision_ParsesThreeSegments()
    {
        var version = TtAndroidToolchainLocator.TryParseNdkRevision(
            "Pkg.Desc = Android NDK\nPkg.Revision = 27.1.12297006\n");
        Assert.That(version, Is.EqualTo(new Version(27, 1, 12297006)));
    }

    [Test]
    public void TryParseNdkRevision_ToleratesSuffixAndTwoSegments()
    {
        Assert.That(TtAndroidToolchainLocator.TryParseNdkRevision("Pkg.Revision = 27.2-rc1\n"),
            Is.EqualTo(new Version(27, 2, 0)));
        Assert.That(TtAndroidToolchainLocator.TryParseNdkRevision("Pkg.Revision = 26.3\n"),
            Is.EqualTo(new Version(26, 3, 0)));
    }

    [Test]
    public void TryParseNdkRevision_InvalidInputs_ReturnNull()
    {
        Assert.That(TtAndroidToolchainLocator.TryParseNdkRevision("Pkg.Revision = abc\n"), Is.Null);
        Assert.That(TtAndroidToolchainLocator.TryParseNdkRevision("Pkg.Desc = Android NDK\n"), Is.Null);
        Assert.That(TtAndroidToolchainLocator.TryParseNdkRevision(null), Is.Null);
    }

    [Test]
    public void SelectHighestNdk_OnlyAcceptsRequiredMajor_AndPicksHighest()
    {
        WriteAllText(Path.Combine(mTempRoot, "26.0", "source.properties"), "Pkg.Revision = 26.0.1\n");
        WriteAllText(Path.Combine(mTempRoot, "27.0", "source.properties"), "Pkg.Revision = 27.0.100\n");
        WriteAllText(Path.Combine(mTempRoot, "27.2", "source.properties"), "Pkg.Revision = 27.2.12479018\n");
        // 没有 source.properties 的目录不参与选择。
        Directory.CreateDirectory(Path.Combine(mTempRoot, "27.9"));

        var best = TtAndroidToolchainLocator.SelectHighestNdk(Directory.GetDirectories(mTempRoot), File.ReadAllText, TtAndroidToolchainLocator.DefaultNdkMajor);
        Assert.That(best, Is.EqualTo(Path.Combine(mTempRoot, "27.2")));
    }

    [Test]
    public void SelectHighestNdk_RespectsConfiguredMajor()
    {
        WriteAllText(Path.Combine(mTempRoot, "27.2", "source.properties"), "Pkg.Revision = 27.2.12479018\n");
        WriteAllText(Path.Combine(mTempRoot, "30.0", "source.properties"), "Pkg.Revision = 30.0.1\n");

        var best = TtAndroidToolchainLocator.SelectHighestNdk(Directory.GetDirectories(mTempRoot), File.ReadAllText, 30);
        Assert.That(best, Is.EqualTo(Path.Combine(mTempRoot, "30.0")));
    }

    [Test]
    public void SelectHighestNdk_NoMatchingMajor_ReturnsNull()
    {
        WriteAllText(Path.Combine(mTempRoot, "26.0", "source.properties"), "Pkg.Revision = 26.0.1\n");
        var best = TtAndroidToolchainLocator.SelectHighestNdk(Directory.GetDirectories(mTempRoot), File.ReadAllText, TtAndroidToolchainLocator.DefaultNdkMajor);
        Assert.That(best, Is.Null);
    }

    [Test]
    public void IsValidSdkRoot_RequiresKnownSubdirectory()
    {
        var sdk = Path.Combine(mTempRoot, "sdk");
        Assert.That(TtAndroidToolchainLocator.IsValidSdkRoot(sdk), Is.False);
        Directory.CreateDirectory(Path.Combine(sdk, "platforms"));
        Assert.That(TtAndroidToolchainLocator.IsValidSdkRoot(sdk), Is.True);
        Assert.That(TtAndroidToolchainLocator.IsValidSdkRoot(null), Is.False);
    }

    [Test]
    public void IsValidJavaHome_RequiresBinJavaExe()
    {
        var jdk = Path.Combine(mTempRoot, "jdk");
        Assert.That(TtAndroidToolchainLocator.IsValidJavaHome(jdk), Is.False);
        WriteAllText(Path.Combine(jdk, "bin", "java.exe"), "MZ");
        Assert.That(TtAndroidToolchainLocator.IsValidJavaHome(jdk), Is.True);
    }

    [Test]
    public void PickHighestVersionSubdir_IgnoresNonVersionNames()
    {
        Directory.CreateDirectory(Path.Combine(mTempRoot, "3.22.1"));
        Directory.CreateDirectory(Path.Combine(mTempRoot, "4.0.2"));
        Directory.CreateDirectory(Path.Combine(mTempRoot, "not-a-version"));
        Assert.That(TtAndroidToolchainLocator.PickHighestVersionSubdir(mTempRoot),
            Is.EqualTo(Path.Combine(mTempRoot, "4.0.2")));
    }

    [Test]
    public void FindOnPath_SkipsEmptyAndQuotedEntries_FindsFirstMatch()
    {
        var bin = Path.Combine(mTempRoot, "bin");
        WriteAllText(Path.Combine(bin, "tool.exe"), "MZ");
        var pathValue = "\"" + bin + "\"; ;C:\\definitely-missing";
        Assert.That(TtAndroidToolchainLocator.FindOnPath("tool.exe", pathValue),
            Is.EqualTo(Path.Combine(bin, "tool.exe")));
        Assert.That(TtAndroidToolchainLocator.FindOnPath("missing.exe", pathValue), Is.Null);
        Assert.That(TtAndroidToolchainLocator.FindOnPath("tool.exe", null), Is.Null);
    }

    [Test]
    public void ReadGlobalJsonSdkBand_ParsesMajorMinor()
    {
        File.WriteAllText(Path.Combine(mTempRoot, "global.json"),
            @"{ ""sdk"": { ""version"": ""10.0.100"", ""rollForward"": ""latestFeature"" } }");
        Assert.That(TtAndroidToolchainLocator.ReadGlobalJsonSdkBand(mTempRoot), Is.EqualTo("10.0"));
    }

    [Test]
    public void ReadGlobalJsonSdkBand_ReturnsNullWhenMissingOrInvalid()
    {
        Assert.That(TtAndroidToolchainLocator.ReadGlobalJsonSdkBand(mTempRoot), Is.Null);
        File.WriteAllText(Path.Combine(mTempRoot, "global.json"), "{ not json");
        Assert.That(TtAndroidToolchainLocator.ReadGlobalJsonSdkBand(mTempRoot), Is.Null);
        Assert.That(TtAndroidToolchainLocator.ReadGlobalJsonSdkBand(null), Is.Null);
    }

    [Test]
    public void FindHighestInstalledSdk_PicksHighestInBand_AndIgnoresOtherBands()
    {
        Assert.That(TtAndroidToolchainLocator.FindHighestInstalledSdk("10.0"),
            Is.EqualTo("10.0.403"), "本机已装 10.0.403 与 11.0.100-rc.3，只能取 10.0 带");
        Assert.That(TtAndroidToolchainLocator.FindHighestInstalledSdk("99.0"), Is.Null);
        Assert.That(TtAndroidToolchainLocator.FindHighestInstalledSdk(""), Is.Null);
    }

    [Test]
    public void GetWorkloadInstallCommand_AppendsSdkVersion_WhenBandResolvable()
    {
        var command = TtAndroidToolchainLocator.GetWorkloadInstallCommand(mTempRoot);
        Assert.That(command, Is.EqualTo("dotnet workload install maui-android --sdk-version 10.0.403").Or
            .EqualTo(TtAndroidToolchainLocator.MauiWorkloadInstallCommand),
            "仓库 global.json 锁 10.0 带时应附 --sdk-version，取不到已装 SDK 时回退兜底命令");
    }
}
