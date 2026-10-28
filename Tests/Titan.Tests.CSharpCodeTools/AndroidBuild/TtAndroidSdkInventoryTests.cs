using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using EngineNS.Editor.AndroidBuild;

namespace Titan.Tests.CSharpCodeTools.AndroidBuild;

[TestFixture]
public class TtAndroidSdkInventoryTests
{
    private string mSdkRoot;

    [SetUp]
    public void SetUp()
    {
        mSdkRoot = Path.Combine(Path.GetTempPath(), "titan-sdk-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(mSdkRoot);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(mSdkRoot)) Directory.Delete(mSdkRoot, true);
    }

    private void CreateDir(params string[] parts)
    {
        Directory.CreateDirectory(Path.Combine(new[] { mSdkRoot }.Concat(parts).ToArray()));
    }

    [Test]
    public void GetPlatforms_SortsByApiLevelDescending()
    {
        CreateDir("platforms", "android-28");
        CreateDir("platforms", "android-34");
        CreateDir("platforms", "android-30");
        var platforms = TtAndroidSdkInventory.GetPlatforms(mSdkRoot);
        Assert.That(platforms, Is.EqualTo(new[] { "android-34", "android-30", "android-28" }));
    }

    [Test]
    public void GetPlatforms_MissingDirectory_ReturnsEmpty()
    {
        Assert.That(TtAndroidSdkInventory.GetPlatforms(mSdkRoot), Is.Empty);
    }

    [Test]
    public void GetBuildTools_SortsByVersionDescending()
    {
        CreateDir("build-tools", "33.0.2");
        CreateDir("build-tools", "34.0.0");
        var tools = TtAndroidSdkInventory.GetBuildTools(mSdkRoot);
        Assert.That(tools, Is.EqualTo(new[] { "34.0.0", "33.0.2" }));
    }

    [Test]
    public void GetAdbPath_EmptySdkRoot_ReturnsEmpty()
    {
        Assert.That(TtAndroidSdkInventory.GetAdbPath(string.Empty), Is.EqualTo(string.Empty));
    }

    [Test]
    public void GetAdbPath_Exists_WhenPlatformToolsPresent()
    {
        CreateDir("platform-tools");
        File.WriteAllText(Path.Combine(mSdkRoot, "platform-tools", "adb.exe"), "");
        Assert.That(TtAndroidSdkInventory.GetAdbPath(mSdkRoot),
            Is.EqualTo(Path.Combine(mSdkRoot, "platform-tools", "adb.exe")));
    }
}
