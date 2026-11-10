using EngineNS.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace EngineNS.Tests.Cook;

/// <summary>
/// cook IPC 协议编解码测试 —— 刻意不依赖 TtEngine.Instance, 与 CookManager.md 的
/// "协议与状态机测试尽量写成纯逻辑" 要求一致。
/// </summary>
[TestFixture]
public class CookProtocolTests
{
    [Test]
    public void Encode_JobBegin_IsSingleProtocolLine()
    {
        var line = TtCookProtocol.Encode(TtCookProtocol.JobBegin(7, "utest/terrain/terrain_scene.scene", "Scene"));

        Assert.That(line, Does.StartWith(TtCookProtocol.Sentinel));
        Assert.That(line, Does.Not.Contain("\n"), "协议必须是单行, 否则对端按行解析会错位");
        Assert.That(TtCookProtocol.IsProtocolLine(line), Is.True);

        Assert.That(TtCookProtocol.TryDecode(line, out var msg), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(msg.Type, Is.EqualTo(TtCookMsgTypes.JobBegin));
            Assert.That(msg.JobId, Is.EqualTo(7));
            Assert.That(msg.Asset, Is.EqualTo("utest/terrain/terrain_scene.scene"));
            Assert.That(msg.AssetType, Is.EqualTo("Scene"));
            Assert.That(msg.Version, Is.EqualTo(TtCookProtocol.Version));
        });
    }

    [Test]
    public void Encode_OmittedFields_AreNotWritten()
    {
        var line = TtCookProtocol.Encode(TtCookProtocol.Beat(null, 3, 12.5));

        Assert.That(line, Does.Contain("\"seq\":3"));
        Assert.That(line, Does.Not.Contain("asset"), "未赋值的可选字段不应出现在线上");
        Assert.That(TtCookProtocol.TryDecode(line, out var msg), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(msg.JobId, Is.Null);
            Assert.That(msg.Asset, Is.Null);
            Assert.That(msg.Seq, Is.EqualTo(3));
            Assert.That(msg.ElapsedSec, Is.EqualTo(12.5).Within(0.001));
        });
    }

    [TestCase("Hello Titan", Description = "worker 的人读输出首行")]
    [TestCase("[TITAN-STAGE] gradle assemble", Description = "AndroidBuild 的哨兵行")]
    [TestCase("", Description = "空行")]
    [TestCase("   ", Description = "空白行")]
    public void TryDecode_NonProtocolLine_ReturnsFalseWithoutThrowing(string line)
    {
        var ok = TtCookProtocol.TryDecode(line, out var msg);

        Assert.Multiple(() =>
        {
            Assert.That(ok, Is.False);
            Assert.That(msg, Is.Null);
            Assert.That(TtCookProtocol.IsProtocolLine(line), Is.False);
        });
    }

    [Test]
    public void TryDecode_MalformedJson_IsTreatedAsLogLine()
    {
        // 协议解析失败不能抛: 对端必须能把它当日志转发, 否则一个坏行会拖垮整条会话
        Assert.That(TtCookProtocol.TryDecode(TtCookProtocol.Sentinel + "{not json", out var msg), Is.False);
        Assert.That(msg, Is.Null);
        Assert.That(TtCookProtocol.TryDecode(TtCookProtocol.Sentinel, out _), Is.False);
        Assert.That(TtCookProtocol.TryDecode(TtCookProtocol.Sentinel + "{}", out _), Is.False, "缺 t 字段的行不算消息");
    }

    [Test]
    public void TryDecode_LeadingWhitespace_StillRecognized()
    {
        // 管道/终端可能给行首加上空白, 不该因此丢协议行
        var line = "  " + TtCookProtocol.Encode(TtCookProtocol.Stats(1, 2, 3, 4.0));

        Assert.That(TtCookProtocol.TryDecode(line, out var msg), Is.True);
        Assert.That(msg.Type, Is.EqualTo(TtCookMsgTypes.Stats));
    }

    [Test]
    public void TryDecode_UnknownField_IsIgnored()
    {
        // 新端加字段时旧端必须照常工作 (向前兼容的代价是未知字段忽略而非报错)
        var line = TtCookProtocol.Sentinel + "{\"v\":1,\"t\":\"add\",\"id\":9,\"asset\":\"a/scene\",\"future\":[1,2]}";

        Assert.That(TtCookProtocol.TryDecode(line, out var msg), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(msg.Type, Is.EqualTo(TtCookMsgTypes.Add));
            Assert.That(msg.JobId, Is.EqualTo(9));
            Assert.That(msg.Asset, Is.EqualTo("a/scene"));
        });
    }

    [Test]
    public void TryDecode_NewerVersion_IsDecodableButFlagged()
    {
        var line = TtCookProtocol.Sentinel + "{\"v\":99,\"t\":\"hello\"}";

        Assert.That(TtCookProtocol.TryDecode(line, out var msg), Is.True);
        Assert.That(msg.IsVersionSupported, Is.False, "版本不匹配由握手侧硬拒, 不在解码侧丢行");
    }

    [Test]
    public void Hello_RoundTripsCapabilityList()
    {
        var hello = TtCookProtocol.Hello(TtCookProtocol.Version, "1.0.0.0", "hash123",
            "d:/repo/content/", new List<string> { "Scene", "Texture" }, 4242);

        Assert.That(TtCookProtocol.TryDecode(TtCookProtocol.Encode(hello), out var msg), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(msg.Type, Is.EqualTo(TtCookMsgTypes.Hello));
            Assert.That(msg.Proto, Is.EqualTo(TtCookProtocol.Version));
            Assert.That(msg.EngineVer, Is.EqualTo("1.0.0.0"));
            Assert.That(msg.CfgHash, Is.EqualTo("hash123"));
            Assert.That(msg.Pid, Is.EqualTo(4242));
            Assert.That(msg.Types, Is.EqualTo(new List<string> { "Scene", "Texture" }));
        });
    }

    [Test]
    public void JobDone_RoundTripsArtifacts()
    {
        var line = TtCookProtocol.Encode(TtCookProtocol.JobDone(5, 61.5,
            new List<string> { "cache/cookedassets/a.trlvl", "cache/cookedassets/a.trvgdag" }));

        Assert.That(TtCookProtocol.TryDecode(line, out var msg), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(msg.Type, Is.EqualTo(TtCookMsgTypes.JobDone));
            Assert.That(msg.Artifacts, Has.Count.EqualTo(2));
            Assert.That(msg.Artifacts[0], Is.EqualTo("cache/cookedassets/a.trlvl"));
        });
    }

    [TestCase("Cancel", ECookErrorKind.Cancel)]
    [TestCase("timeout", ECookErrorKind.Timeout)]
    [TestCase("CONFLICT", ECookErrorKind.Conflict)]
    public void JobError_Kind_IsParsedCaseInsensitively(string kind, ECookErrorKind expected)
    {
        var line = TtCookProtocol.Encode(TtCookProtocol.JobError(1, expected, "boom", null, 2.0));

        Assert.That(TtCookProtocol.TryDecode(line, out var msg), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(msg.Kind, Is.EqualTo(kind).Or.EqualTo(expected.ToString()));
            Assert.That(msg.GetErrorKind(), Is.EqualTo(expected));
            Assert.That(msg.Message, Is.EqualTo("boom"));
            Assert.That(msg.Stack, Is.Null);
        });
    }

    [Test]
    public void JobError_UnknownKind_IsNullSoCallerCanDecide()
    {
        var line = TtCookProtocol.Sentinel + "{\"v\":1,\"t\":\"job.error\",\"id\":1,\"kind\":\"Whatever\"}";

        Assert.That(TtCookProtocol.TryDecode(line, out var msg), Is.True);
        Assert.That(msg.GetErrorKind(), Is.Null, "无法识别的 kind 不能被静默当成某一类");
    }

    [Test]
    public void AddCommand_RoundTripsForceFlag()
    {
        var add = new TtCookMessage
        {
            Type = TtCookMsgTypes.Add,
            JobId = 11,
            Asset = "utest/a.scene",
            Force = true,
        };

        Assert.That(TtCookProtocol.TryDecode(TtCookProtocol.Encode(add), out var msg), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(msg.Force, Is.True);
            Assert.That(msg.IsVersionSupported, Is.True);
        });
    }

    [Test]
    public void Encode_DoesNotPutLocalOnlyPropertiesOnWire()
    {
        var line = TtCookProtocol.Encode(TtCookProtocol.Ready(3));

        Assert.Multiple(() =>
        {
            Assert.That(line, Does.StartWith(TtCookProtocol.Sentinel));
            Assert.That(line, Does.Contain("\"queued\":3"));
            // IsVersionSupported 是解码侧由 v 字段本地算出来的, 上线只会每行多一个冗余字段
            Assert.That(line, Does.Not.Contain("IsVersionSupported"));
        });
    }

    [Test]
    public void Encode_NullMessage_ReturnsNull()
    {
        Assert.That(TtCookProtocol.Encode(null), Is.Null);
    }
}
