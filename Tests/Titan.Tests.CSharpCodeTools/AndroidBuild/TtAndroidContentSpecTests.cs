using System;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using EngineNS.Editor.AndroidBuild;

namespace Titan.Tests.CSharpCodeTools.AndroidBuild;

[TestFixture]
public class TtAndroidContentSpecTests
{
    private const string Sample = "# Android 运行期内容\ncontent/config/engineconfig_android_smoke.jscfg\nenginecontent/metadata/**\n\ncontent/metadata/**\n";

    [Test]
    public void Parse_SkipsCommentsAndBlankLines_KeepsOrder()
    {
        var spec = TtAndroidContentSpec.Parse(Sample);
        Assert.That(spec.Patterns, Is.EqualTo(new[]
        {
            "content/config/engineconfig_android_smoke.jscfg",
            "enginecontent/metadata/**",
            "content/metadata/**",
        }));
    }

    [Test]
    public void Normalize_ConvertsBackslashAndCollapsesSlashes()
    {
        var spec = TtAndroidContentSpec.Parse(string.Empty);
        Assert.Multiple(() =>
        {
            Assert.That(spec.Normalize(@"content\metadata\**"), Is.EqualTo("content/metadata/**"));
            Assert.That(spec.Normalize("  content//metadata/*  "), Is.EqualTo("content/metadata/*"));
            Assert.That(spec.Normalize("./content/x"), Is.EqualTo("content/x"));
        });
    }

    [TestCase("content/metadata/**")]
    [TestCase("enginecontent/metadata/**")]
    [TestCase("binaries/Plugins/DataCopyer.plugin")]
    public void Validate_AcceptsAllowedPrefix(string pattern)
    {
        Assert.That(TtAndroidContentSpec.Validate(pattern), Is.Null);
    }

    [TestCase("gameplay/metadata/**", "只允许 content/、enginecontent/、binaries/ 前缀")]
    [TestCase("content/../etc/passwd", "不允许出现 ..")]
    [TestCase("/content/x", "不允许以 / 开头")]
    [TestCase("content/配置.txt", "只接受 ASCII 路径")]
    [TestCase("C:/content/x", "不允许盘符")]
    [TestCase("", "内容为空")]
    public void Validate_ReportsReason(string pattern, string expected)
    {
        Assert.That(TtAndroidContentSpec.Validate(pattern), Does.Contain(expected));
    }

    [Test]
    public void Add_RejectsDuplicateAndInvalid()
    {
        var spec = TtAndroidContentSpec.Parse(Sample);
        Assert.That(spec.Add(@"content\metadata\**"), Is.False, "重复项（含反斜杠写法）应被拒");
        Assert.That(spec.Add("enginecontent/other/**"), Is.True);
        Assert.That(spec.Patterns.Count, Is.EqualTo(4));
    }

    [Test]
    public void Remove_DropsEntry()
    {
        var spec = TtAndroidContentSpec.Parse(Sample);
        Assert.That(spec.Remove("enginecontent/metadata/**"), Is.True);
        Assert.That(spec.Remove("enginecontent/metadata/**"), Is.False);
        Assert.That(spec.Patterns.Count, Is.EqualTo(2));
    }

    [Test]
    public void Serialize_KeepsHeaderComment_AndRoundTrips()
    {
        var spec = TtAndroidContentSpec.Parse(Sample);
        spec.Add("content/other/**");
        var text = spec.Serialize();
        Assert.That(text, Does.StartWith("# Android 运行期内容"));

        var again = TtAndroidContentSpec.Parse(text);
        Assert.That(again.Patterns, Is.EqualTo(spec.Patterns));
    }

    [Test]
    public void Save_WritesUtf8CrLf_KeepsHeaderComment()
    {
        var path = Path.Combine(Path.GetTempPath(), "titan-content-" + Guid.NewGuid().ToString("N") + ".txt");
        try
        {
            var spec = TtAndroidContentSpec.Parse(Sample);
            spec.Save(path);
            // 中文头注释必须原样落盘（UTF-8），不允许被 ASCII 编码写成 '????？'。
            var text = File.ReadAllText(path, Encoding.UTF8);
            Assert.Multiple(() =>
            {
                Assert.That(text, Does.StartWith("# Android 运行期内容"));
                Assert.That(text, Does.Contain("content/metadata/**\r\n"));
            });
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
