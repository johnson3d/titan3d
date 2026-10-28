using EngineNS.IO;
using NUnit.Framework;

namespace EngineNS.Tests.IO;

[TestFixture]
public class FileManagerPathTests
{
    [Test]
    public void GetValidDirectory_WindowsHost_TrimsLeadingSlash()
    {
        Assert.That(TtFileManager.GetValidDirectory("/Content/Tex", true), Is.EqualTo("content/tex/"));
        Assert.That(TtFileManager.GetValidDirectory("E:\\GitHub\\Titan\\binaries", true), Is.EqualTo("e:/github/titan/binaries/"));
    }

    [Test]
    public void GetValidDirectory_UnixHost_KeepsAbsoluteRoot()
    {
        Assert.That(TtFileManager.GetValidDirectory("/Users/dev/TitanEngine/binaries/Plugins/", false), Is.EqualTo("/Users/dev/TitanEngine/binaries/Plugins/"));
    }

    [Test]
    public void GetValidFileName_HostDecidesLeadingSlash()
    {
        Assert.That(TtFileManager.GetValidFileName("/Users/dev/a.txt", true), Is.EqualTo("users/dev/a.txt"));
        Assert.That(TtFileManager.GetValidFileName("/Users/dev/A.txt", false), Is.EqualTo("/Users/dev/A.txt"));
    }
}
