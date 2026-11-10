using EngineNS.IO;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace EngineNS.Tests.Cook;

/// <summary>
/// TtCookContext 行为测试 (取消位 / 进度上报 / 产物登记 / ambient)。
/// 同样不依赖 TtEngine.Instance: 上下文只做记录与转发, 引擎只在驱动侧才被触及。
/// </summary>
[TestFixture]
public class CookContextTests
{
    private class FRecordingSink : ITtCookProgressSink
    {
        public readonly List<string> Stages = new List<string>();
        public readonly List<float> Fractions = new List<float>();
        public readonly List<string> Artifacts = new List<string>();

        public void OnStage(TtCookContext ctx, string stage) => Stages.Add($"{ctx.JobId}:{stage}");
        public void OnProgress(TtCookContext ctx, float fraction) => Fractions.Add(fraction);
        public void OnArtifact(TtCookContext ctx, string artifactPath) => Artifacts.Add(artifactPath);
    }

    private static TtCookContext NewContext(FRecordingSink sink)
    {
        return new TtCookContext(7, "utest/a.scene", "Scene", false, sink);
    }

    [Test]
    public void RequestCancel_IsCooperativeFlagAndIdempotent()
    {
        var ctx = NewContext(new FRecordingSink());

        Assert.That(ctx.IsCancellationRequested, Is.False);
        ctx.RequestCancel();
        ctx.RequestCancel();

        // 只置位, 不碰任何任务对象: 长任务的 CancelTask() 会让池化 TtTaskData 提前回池后被后台写污染
        Assert.That(ctx.IsCancellationRequested, Is.True);
    }

    [Test]
    public void ReportStage_ForwardsJobIdAndKeepsLastStage()
    {
        var sink = new FRecordingSink();
        var ctx = NewContext(sink);

        ctx.ReportStage("resolve meta");
        ctx.ReportStage("create asset");

        Assert.Multiple(() =>
        {
            Assert.That(sink.Stages, Is.EqualTo(new List<string> { "7:resolve meta", "7:create asset" }));
            Assert.That(ctx.LastStage, Is.EqualTo("create asset"));
        });
    }

    [TestCase(-0.5f, 0.0f)]
    [TestCase(1.5f, 1.0f)]
    public void ReportProgress_IsClamped(float input, float expected)
    {
        var sink = new FRecordingSink();
        var ctx = NewContext(sink);

        ctx.ReportProgress(input);

        Assert.That(sink.Fractions, Has.Count.EqualTo(1));
        Assert.That(sink.Fractions[0], Is.EqualTo(expected).Within(0.001f));
    }

    [Test]
    public void ReportProgress_RepeatsAreIgnored()
    {
        var sink = new FRecordingSink();
        var ctx = NewContext(sink);

        ctx.ReportProgress(0.25f);
        ctx.ReportProgress(0.25f);
        ctx.ReportProgress(0.5f);

        // IPC 行数正比于进度变化次数, 重复值不转发才不会把管道灌满
        Assert.That(sink.Fractions, Is.EqualTo(new List<float> { 0.25f, 0.5f }));
        Assert.That(ctx.LastFraction, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void ReportArtifact_NormalizesSeparatorsAndMergesDuplicates()
    {
        var sink = new FRecordingSink();
        var ctx = NewContext(sink);

        ctx.ReportArtifact(@"cache\cookedassets\a.trlvl");
        ctx.ReportArtifact("cache/cookedassets/a.trlvl");
        ctx.ReportArtifact(null);

        Assert.Multiple(() =>
        {
            Assert.That(ctx.GetArtifacts(), Has.Count.EqualTo(1));
            Assert.That(ctx.GetArtifacts()[0], Is.EqualTo("cache/cookedassets/a.trlvl"));
            Assert.That(sink.Artifacts, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void GetArtifacts_ReturnsSnapshotNotLiveList()
    {
        var ctx = NewContext(new FRecordingSink());
        ctx.ReportArtifact("cache/cookedassets/a.trlvl");

        var snapshot = ctx.GetArtifacts();
        ctx.ReportArtifact("cache/cookedassets/a.trvgdag");

        Assert.That(snapshot, Has.Count.EqualTo(1), "job.done 序列化期间若有线程继续登记产物不应导致快照变化或枚举异常");
    }

    [Test]
    public void PushAmbient_SetsAndRestoresContext()
    {
        Assert.That(TtCookContext.Current, Is.Null, "不处于 cook 期间时 ambient 必须为空");

        var outer = NewContext(new FRecordingSink());
        using (TtCookContext.PushAmbient(outer))
        {
            Assert.That(TtCookContext.Current, Is.SameAs(outer));

            // TtNode.CookAsset() 这类无参扩展点靠 ambient 取上下文, 因此嵌套时也必须能正确恢复
            var inner = new TtCookContext(8, "utest/b.scene", "Scene", false, new FRecordingSink());
            using (TtCookContext.PushAmbient(inner))
            {
                Assert.That(TtCookContext.Current, Is.SameAs(inner));
            }

            Assert.That(TtCookContext.Current, Is.SameAs(outer));
        }

        Assert.That(TtCookContext.Current, Is.Null);
    }

    [Test]
    public void ProtocolSink_WritesStageLineWithJobId()
    {
        var lines = new List<string>();
        var ctx = new TtCookContext(9, "utest/a.scene", "Scene", false, new TtCookProtocolSink(lines.Add));

        ctx.ReportStage("cook");
        ctx.ReportProgress(0.5f);

        Assert.That(lines, Has.Count.EqualTo(2));
        Assert.That(TtCookProtocol.TryDecode(lines[0], out var stage), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(stage.Type, Is.EqualTo(TtCookMsgTypes.JobStage));
            Assert.That(stage.JobId, Is.EqualTo(9));
            Assert.That(stage.Stage, Is.EqualTo("cook"));
        });
        Assert.That(TtCookProtocol.TryDecode(lines[1], out var progress), Is.True);
        Assert.That(progress.Fraction, Is.EqualTo(0.5f).Within(0.001f));
    }

    [Test]
    public void NullSink_FallsBackToLogSinkWithoutThrowing()
    {
        var ctx = new TtCookContext(1, "utest/a.scene", "Scene", false, null);

        Assert.Multiple(() =>
        {
            Assert.That(ctx.Sink, Is.SameAs(TtCookLogSink.Instance));
            Assert.That(() => ctx.ReportStage("cook"), Throws.Nothing);
        });
    }
}
