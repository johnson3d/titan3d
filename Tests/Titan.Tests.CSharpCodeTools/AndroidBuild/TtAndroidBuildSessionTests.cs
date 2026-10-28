using System;
using System.Collections.Generic;
using NUnit.Framework;
using EngineNS.Editor.AndroidBuild;

namespace Titan.Tests.CSharpCodeTools.AndroidBuild;

[TestFixture]
public class TtAndroidBuildSessionTests
{
    private sealed class FakeHost : ITtAndroidProcessHost
    {
        public int ExitCode = 0;
        public bool Exited;
        public bool Killed;
        public System.Text.StringBuilder Output = new System.Text.StringBuilder();

        public bool HasExited => Exited;
        public int GetExitCode() => ExitCode;
        public string GetOutput() => Output.ToString();
        public void AppendLine(string line) { Output.AppendLine(line); }
        public void Kill() { Killed = true; Exited = true; }
    }

    private sealed class FakeLauncher : ITtAndroidProcessLauncher
    {
        public readonly List<string> Commands = new List<string>();
        public readonly List<FakeHost> Hosts = new List<FakeHost>();

        public ITtAndroidProcessHost Launch(string commandLine, string workingDirectory)
        {
            Commands.Add(commandLine);
            var host = new FakeHost();
            Hosts.Add(host);
            return host;
        }
    }

    [SetUp]
    public void SetUp()
    {
        mRepoRoot = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "titan-sess-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(mRepoRoot);
        // Validate 在 CookFirst 时要求 cook 配置与 TitanCMD 存在，预置占位文件。
        var cookPath = System.IO.Path.Combine(mRepoRoot, "content", "config");
        System.IO.Directory.CreateDirectory(cookPath);
        System.IO.File.WriteAllText(System.IO.Path.Combine(cookPath, "engineconfig_windows_cook.jscfg"), "{}");
        var cmdPath = System.IO.Path.Combine(mRepoRoot, "binaries", "net8.0");
        System.IO.Directory.CreateDirectory(cmdPath);
        System.IO.File.WriteAllText(System.IO.Path.Combine(cmdPath, "TitanCMD.exe"), "");
    }

    [TearDown]
    public void TearDown()
    {
        if (System.IO.Directory.Exists(mRepoRoot)) System.IO.Directory.Delete(mRepoRoot, true);
    }

    private string mRepoRoot;

    private TtAndroidBuildRequest MakeRequest()
    {
        return new TtAndroidBuildRequest
        {
            RepositoryRoot = mRepoRoot,
            Configuration = "Debug",
            ToolchainConfig = string.Empty,
        };
    }

    private static void FinishCheck(FakeLauncher launcher)
    {
        var check = launcher.Hosts[0];
        check.Exited = true;
        check.ExitCode = 0;
    }

    [Test]
    public void Start_WithInvalidRequest_EntersFailedWithoutLaunching()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        session.Start(new TtAndroidBuildRequest { RepositoryRoot = @"Z:\no-such-dir", Configuration = "Debug" });
        Assert.Multiple(() =>
        {
            Assert.That(session.Phase, Is.EqualTo(TtAndroidBuildPhase.Failed));
            Assert.That(session.LastError, Does.Contain("仓库根"));
            Assert.That(launcher.Commands, Is.Empty);
        });
    }

    [Test]
    public void Start_ValidRequest_LaunchesCheckFirst()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        session.Start(MakeRequest());
        Assert.Multiple(() =>
        {
            Assert.That(session.Phase, Is.EqualTo(TtAndroidBuildPhase.Check));
            Assert.That(session.CurrentHost, Is.Not.Null, "面板需要通过 CurrentHost 取日志");
            Assert.That(launcher.Commands, Has.Count.EqualTo(1));
            Assert.That(launcher.Commands[0], Does.Contain("check-android-toolchain.ps1"));
        });
    }

    [Test]
    public void Poll_NoCook_GoesCheckToBuildToDone()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        session.Start(MakeRequest());
        FinishCheck(launcher);

        session.Poll();
        Assert.Multiple(() =>
        {
            Assert.That(session.Phase, Is.EqualTo(TtAndroidBuildPhase.Build));
            Assert.That(launcher.Commands[1], Does.Contain("build-android.ps1"));
        });

        var build = launcher.Hosts[1];
        build.AppendLine("compiling...");
        build.AppendLine("APK=C:\\out\\game-Signed.apk");
        build.Exited = true;
        build.ExitCode = 0;
        session.Poll();
        Assert.Multiple(() =>
        {
            Assert.That(session.Phase, Is.EqualTo(TtAndroidBuildPhase.Done));
            Assert.That(session.ApkPath, Is.EqualTo(@"C:\out\game-Signed.apk"));
        });
    }

    [Test]
    public void Poll_WhileRunning_TracksBuildStageMarker()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        session.Start(MakeRequest());
        FinishCheck(launcher);
        session.Poll();
        Assert.That(session.CurrentBuildStage, Is.EqualTo(string.Empty));

        var build = launcher.Hosts[1];
        build.AppendLine("dotnet build output...");
        build.AppendLine("[TITAN-STAGE] Native");
        session.Poll();
        Assert.That(session.CurrentBuildStage, Is.EqualTo("Native"));

        build.AppendLine("[TITAN-STAGE] Apk");
        session.Poll();
        Assert.That(session.CurrentBuildStage, Is.EqualTo("Apk"));
    }

    [Test]
    public void Start_ResetsBuildStageMarker()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        session.Start(MakeRequest());
        FinishCheck(launcher);
        session.Poll();
        var build = launcher.Hosts[1];
        build.AppendLine("[TITAN-STAGE] Native");
        session.Poll();
        build.Exited = true;
        build.ExitCode = 0;
        build.AppendLine("APK=C:\\out\\x.apk");
        session.Poll();

        session.Start(MakeRequest());
        Assert.That(session.CurrentBuildStage, Is.EqualTo(string.Empty));
    }

    [Test]
    public void Poll_WithCook_InsertsCookPhase()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        var request = MakeRequest();
        request.CookFirst = true;
        session.Start(request);
        FinishCheck(launcher);

        session.Poll();
        Assert.Multiple(() =>
        {
            Assert.That(session.Phase, Is.EqualTo(TtAndroidBuildPhase.Cook));
            Assert.That(launcher.Commands[1], Does.Contain("TitanCMD.exe"));
        });

        var cook = launcher.Hosts[1];
        cook.Exited = true;
        session.Poll();
        Assert.Multiple(() =>
        {
            Assert.That(session.Phase, Is.EqualTo(TtAndroidBuildPhase.Build));
            Assert.That(launcher.Commands[2], Does.Contain("build-android.ps1"));
        });
    }

    [Test]
    public void Poll_CheckFailure_EntersFailed()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        session.Start(MakeRequest());
        var check = launcher.Hosts[0];
        check.AppendLine("error: missing cmake");
        check.Exited = true;
        check.ExitCode = 2;

        session.Poll();
        Assert.Multiple(() =>
        {
            Assert.That(session.Phase, Is.EqualTo(TtAndroidBuildPhase.Failed));
            Assert.That(session.LastError, Does.Contain("退出码"));
        });
    }

    [Test]
    public void Cancel_KillsCurrentProcess_AndMarksFailed()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        session.Start(MakeRequest());

        session.Cancel();
        Assert.Multiple(() =>
        {
            Assert.That(launcher.Hosts[0].Killed, Is.True);
            Assert.That(session.Phase, Is.EqualTo(TtAndroidBuildPhase.Failed));
            Assert.That(session.LastError, Does.Contain("取消"));
        });
    }

    [Test]
    public void Start_WhileRunning_IsIgnored()
    {
        var launcher = new FakeLauncher();
        var session = new TtAndroidBuildSession(launcher);
        session.Start(MakeRequest());
        session.Start(MakeRequest());
        Assert.That(launcher.Commands, Has.Count.EqualTo(1));
    }
}
