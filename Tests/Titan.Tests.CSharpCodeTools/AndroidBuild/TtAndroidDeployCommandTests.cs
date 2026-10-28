using System;
using NUnit.Framework;
using EngineNS.Editor.AndroidBuild;

namespace Titan.Tests.CSharpCodeTools.AndroidBuild;

[TestFixture]
public class TtAndroidDeployCommandTests
{
    [Test]
    public void AdbInstall_QuotesAdbAndApk()
    {
        var cmd = TtAndroidDeployCommand.AdbInstall(@"C:\sdk\platform-tools\adb.exe", @"C:\out\game-Signed.apk");
        Assert.That(cmd, Is.EqualTo("\"C:\\sdk\\platform-tools\\adb.exe\" install -r \"C:\\out\\game-Signed.apk\""));
    }

    [Test]
    public void AdbDevices_QuotesAdb()
    {
        var cmd = TtAndroidDeployCommand.AdbDevices(@"C:\sdk\platform-tools\adb.exe");
        Assert.That(cmd, Is.EqualTo("\"C:\\sdk\\platform-tools\\adb.exe\" devices"));
    }
}
