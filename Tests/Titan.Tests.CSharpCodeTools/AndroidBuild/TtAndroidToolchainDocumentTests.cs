using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using EngineNS.Editor.AndroidBuild;

namespace Titan.Tests.CSharpCodeTools.AndroidBuild;

[TestFixture]
public class TtAndroidToolchainDocumentTests
{
    private string mTempRoot;

    [SetUp]
    public void SetUp()
    {
        mTempRoot = Path.Combine(Path.GetTempPath(), "titan-tc-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(mTempRoot);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(mTempRoot)) Directory.Delete(mTempRoot, true);
    }

    private string WriteJson(string json)
    {
        var path = Path.Combine(mTempRoot, "android-toolchain.json");
        File.WriteAllText(path, json, new UTF8Encoding(false));
        return path;
    }

    [Test]
    public void Set_OnlyTouchesTargetKey_AndKeepsUnknownFields()
    {
        var path = WriteJson(@"{
  ""schemaVersion"": 1,
  ""abi"": ""arm64-v8a"",
  ""apiLevel"": 28,
  ""ndk"": ""C:\\ndk\\27"",
  ""androidSdk"": ""C:\\sdk"",
  ""requirements"": { ""ndkMajor"": 27, ""dotnetSdkMajor"": 10 }
}");
        var doc = TtAndroidToolchainDocument.Load(path);
        doc.Set("ndk", "D:\\ndk\\27.2");
        doc.Save(path);

        var text = File.ReadAllText(path, Encoding.UTF8);
        var reloaded = TtAndroidToolchainDocument.Parse(text);
        Assert.Multiple(() =>
        {
            Assert.That(reloaded.Get("ndk"), Is.EqualTo("D:\\ndk\\27.2"));
            Assert.That(reloaded.Get("androidSdk"), Is.EqualTo("C:\\sdk"));
            Assert.That(reloaded.Get("abi"), Is.EqualTo("arm64-v8a"));
            Assert.That(reloaded.GetApiLevel(), Is.EqualTo(28));
            Assert.That(text, Does.Contain("\"ndkMajor\": 27"));
            Assert.That(text, Does.Contain("\"schemaVersion\": 1"));
        });
    }

    [Test]
    public void Save_WritesUtf8WithoutBom()
    {
        var path = WriteJson("{}");
        var doc = TtAndroidToolchainDocument.Load(path);
        doc.Set("ndk", "C:\\ndk\\27");
        doc.Save(path);

        var bytes = File.ReadAllBytes(path);
        Assert.That(bytes.Length, Is.GreaterThan(0));
        Assert.That(bytes[0], Is.Not.EqualTo(0xEF), "脚本用 Get-Content -Encoding UTF8 读，不要写 BOM 之外的多余字节");
    }

    [Test]
    public void MissingRequiredEntries_ListsMissingInFixedOrder()
    {
        var doc = TtAndroidToolchainDocument.Parse(@"{ ""ndk"": ""C:\\ndk\\27"", ""abi"": ""arm64-v8a"" }");
        var missing = doc.MissingRequiredEntries();
        Assert.That(missing, Is.EqualTo(new[] { "androidSdk", "cmake", "ninja", "dotnet", "javaHome", "apiLevel" }));
    }

    [Test]
    public void MissingRequiredEntries_EmptyWhenComplete()
    {
        var doc = TtAndroidToolchainDocument.Parse(@"{
  ""abi"": ""arm64-v8a"", ""apiLevel"": 28,
  ""ndk"": ""a"", ""androidSdk"": ""b"", ""cmake"": ""c"", ""ninja"": ""d"",
  ""dotnet"": ""e"", ""javaHome"": ""f""
}");
        Assert.That(doc.MissingRequiredEntries(), Is.Empty);
    }

    [Test]
    public void Parse_InvalidJson_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => TtAndroidToolchainDocument.Parse("{ not json"));
    }

    [Test]
    public void FindRoot_WalksUpToDirectoryContainingBuildScript()
    {
        var repo = Path.Combine(mTempRoot, "repo");
        Directory.CreateDirectory(Path.Combine(repo, "BuildScript", "android"));
        File.WriteAllText(Path.Combine(repo, "BuildScript", "android", "build-android.ps1"), "#");
        var deep = Path.Combine(repo, "intermediate", "tmp", "x");
        Directory.CreateDirectory(deep);

        Assert.That(TtAndroidRepository.FindRoot(deep), Is.EqualTo(repo));
        Assert.That(TtAndroidRepository.FindRoot(mTempRoot), Is.Null);
    }

    [Test]
    public void ToFullPath_NormalizesSeparatorsAndTrailingSlash()
    {
        var root = @"C:\repo";
        Assert.That(TtAndroidRepository.ToFullPath(root, "cache/config/"), Is.EqualTo(@"C:\repo\cache\config"));
        Assert.That(TtAndroidRepository.ToFullPath(root, @"D:\ndk\27\"), Is.EqualTo(@"D:\ndk\27"));
    }
}
