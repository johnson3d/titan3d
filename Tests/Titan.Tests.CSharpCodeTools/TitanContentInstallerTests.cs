using System.Text;
using AloneGame.Maui.Services;
using NUnit.Framework;

namespace Titan.Tests.CSharpCodeTools;

// 内容安装只依赖 BCL：测试直接链接同一个源码，不加载 MAUI/Android 运行时。
[TestFixture]
public class TitanContentInstallerTests
{
    static string NewDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), "Titan Installer " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    static Func<string, Stream> Assets(params (string Name, string Text)[] files)
    {
        var map = files.ToDictionary(f => f.Name, f => f.Text, StringComparer.Ordinal);
        return name => new MemoryStream(Encoding.UTF8.GetBytes(map[name]));
    }

    [Test]
    public void ContentInstaller_ReplacesContentButPreservesCache()
    {
        var dir = NewDirectory();
        Directory.CreateDirectory(Path.Combine(dir, "titan/cache"));
        File.WriteAllText(Path.Combine(dir, "titan/cache/keep.txt"), "cache");
        Directory.CreateDirectory(Path.Combine(dir, "titan/content"));
        File.WriteAllText(Path.Combine(dir, "titan/content/old.txt"), "old");
        var assets = new Dictionary<string, string>
        {
            ["titan/manifest.txt"] = "build-1\ncontent/config/smoke.jscfg\n",
            ["titan/content/config/smoke.jscfg"] = "new"
        };
        Stream Open(string name) => new MemoryStream(Encoding.UTF8.GetBytes(assets[name]));
        try
        {
            var root = TitanContentInstaller.Install(dir, Open);
            Assert.That(root, Does.EndWith("/titan/"));
            Assert.That(File.ReadAllText(Path.Combine(root, "cache/keep.txt")), Is.EqualTo("cache"));
            Assert.That(File.Exists(Path.Combine(root, "content/old.txt")), Is.False, "旧内容必须清空");
            Assert.That(File.ReadAllText(Path.Combine(root, "content/config/smoke.jscfg")), Is.EqualTo("new"));
            Assert.That(File.ReadAllText(Path.Combine(root, ".installed")), Is.EqualTo("build-1"));
            // 资产缺失时不得因为已安装标记而假成功：同 build-id 才可跳过。
            assets.Remove("titan/content/config/smoke.jscfg");
            Assert.DoesNotThrow(() => TitanContentInstaller.Install(dir, Open));
        }
        finally { Directory.Delete(dir, true); }
    }

    [Test]
    public void ContentInstaller_RejectsTraversalPathAndKeepsOldContent()
    {
        var dir = NewDirectory();
        Directory.CreateDirectory(Path.Combine(dir, "titan/content"));
        File.WriteAllText(Path.Combine(dir, "titan/content/old.txt"), "old");
        try
        {
            var open = Assets(
                ("titan/manifest.txt", "build-2\ncontent/../outside.txt\n"),
                ("titan/content/../outside.txt", "escape"));
            Assert.Throws<InvalidDataException>(() => TitanContentInstaller.Install(dir, open));
            Assert.That(File.ReadAllText(Path.Combine(dir, "titan/content/old.txt")), Is.EqualTo("old"), "非法 manifest 不能删除旧内容");
            Assert.That(File.Exists(Path.Combine(dir, "titan/.installed")), Is.False);
        }
        finally { Directory.Delete(dir, true); }
    }

    [Test]
    public void ContentInstaller_RejectsPathOutsideKnownRoots()
    {
        var dir = NewDirectory();
        try
        {
            foreach (var path in new[] { "other/file.txt", "content//a.txt", "binaries\\a.dll", "C:/a.txt", "contentx/a.txt" })
            {
                var manifest = "build-3\n" + path + "\n";
                Assert.Throws<InvalidDataException>(
                    () => TitanContentInstaller.Install(dir, Assets(("titan/manifest.txt", manifest), ("titan/" + path, "x"))),
                    "必须拒绝：" + path);
            }
        }
        finally { Directory.Delete(dir, true); }
    }

    [Test]
    public void ContentInstaller_InterruptedCopyDoesNotWriteMarker()
    {
        var dir = NewDirectory();
        try
        {
            var map = new Dictionary<string, string>
            {
                ["titan/manifest.txt"] = "build-4\ncontent/a.txt\ncontent/b.txt\n",
                ["titan/content/a.txt"] = "a"
            };
            Stream Open(string name) => map.TryGetValue(name, out var text)
                ? new MemoryStream(Encoding.UTF8.GetBytes(text))
                : throw new FileNotFoundException(name);
            Assert.Throws<FileNotFoundException>(() => TitanContentInstaller.Install(dir, Open));
            Assert.That(File.Exists(Path.Combine(dir, "titan/.installed")), Is.False, "复制中断不能写安装标记");
        }
        finally { Directory.Delete(dir, true); }
    }

    [Test]
    public void ContentInstaller_SameBuildIdSkipsCopyAndNewBuildIdKeepsCacheOnly()
    {
        var dir = NewDirectory();
        Directory.CreateDirectory(Path.Combine(dir, "titan/cache"));
        File.WriteAllText(Path.Combine(dir, "titan/cache/keep.txt"), "cache");
        try
        {
            var first = Assets(
                ("titan/manifest.txt", "build-5\ncontent/a.txt\nbinaries/Plugins/x.plugin\n"),
                ("titan/content/a.txt", "one"),
                ("titan/binaries/Plugins/x.plugin", "plugin"));
            Assert.That(TitanContentInstaller.Install(dir, first), Does.EndWith("/titan/"));
            Assert.That(File.ReadAllText(Path.Combine(dir, "titan/content/a.txt")), Is.EqualTo("one"));

            // 同 build-id：读完 manifest 就返回，其余资产一律不读取。
            Assert.DoesNotThrow(() => TitanContentInstaller.Install(dir, name =>
                name == "titan/manifest.txt"
                    ? new MemoryStream(Encoding.UTF8.GetBytes("build-5\ncontent/a.txt\nbinaries/Plugins/x.plugin\n"))
                    : throw new FileNotFoundException("不应读取资产：" + name)));

            var second = Assets(
                ("titan/manifest.txt", "build-6\ncontent/b.txt\n"),
                ("titan/content/b.txt", "two"));
            TitanContentInstaller.Install(dir, second);
            Assert.That(File.Exists(Path.Combine(dir, "titan/content/a.txt")), Is.False, "新 build-id 必须清旧内容");
            Assert.That(File.ReadAllText(Path.Combine(dir, "titan/content/b.txt")), Is.EqualTo("two"));
            Assert.That(Directory.Exists(Path.Combine(dir, "titan/binaries")), Is.False, "新 build-id 必须清旧 binaries");
            Assert.That(File.ReadAllText(Path.Combine(dir, "titan/cache/keep.txt")), Is.EqualTo("cache"));
            Assert.That(File.ReadAllText(Path.Combine(dir, "titan/.installed")), Is.EqualTo("build-6"));
        }
        finally { Directory.Delete(dir, true); }
    }
}
