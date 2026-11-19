using System;
using System.Collections.Generic;
using EngineNS.IO;
using EngineNS.Editor.Cook;

// 纯逻辑状态机测试: 用假通道喂录好的协议行, 不起任何子进程。
// 编译方式见 run_cook_tests.ps1 (csc /target:exe + Engine.Window.dll 作引用)。
class FakeChannel : ITtCookWorkerChannel
{
    public readonly List<TtCookMessage> Sent = new List<TtCookMessage>();
    public readonly Queue<TtCookMessage> Inbox = new Queue<TtCookMessage>();
    public bool Alive = true;
    public int QuitCount;

    public Action<TtCookMessage> OnProtocolMessage { get; set; }
    public Action<string> OnLogLine { get; set; }
    public Action<int> OnProcessExited { get; set; }

    public bool IsAlive => Alive;
    public int ProcessId => 4242;

    public bool Send(TtCookMessage msg) { Sent.Add(msg); return true; }

    public int Pump()
    {
        var n = 0;
        while (Inbox.Count > 0)
        {
            OnProtocolMessage?.Invoke(Inbox.Dequeue());
            n++;
        }
        return n;
    }

    public bool RequestQuit(string reason = "manual") { QuitCount++; return true; }
    public bool WaitForExit(int timeoutMs) => true;
    public string[] GetLogSnapshot() => Array.Empty<string>();
    public void ClearLog() { }
    public void Dispose() { Alive = false; }

    public void Post(TtCookMessage msg) { Inbox.Enqueue(msg); }
    /// <summary>模拟子进程崩掉: 走 OnProcessExited 事件通路</summary>
    public void Crash(int code = -1) { Alive = false; OnProcessExited?.Invoke(code); }
}

static class Program
{
    sealed class NullSink : ITtCookProgressSink
    {
        public void OnStage(TtCookContext ctx, string stage) { }
        public void OnProgress(TtCookContext ctx, float fraction) { }
        public void OnArtifact(TtCookContext ctx, string artifactPath) { }
    }
    static int mFailed;
    static string mCurrent = "";

    static void Check(bool cond, string what)
    {
        if (cond)
            Console.WriteLine($"  ok   {what}");
        else
        {
            Console.WriteLine($"  FAIL {what}   [{mCurrent}]");
            mFailed++;
        }
    }

    static TtCookMessage Hello(int version = 1)
    {
        return TtCookProtocol.Hello(TtCookProtocol.Version, "1.0.0.0", "hash", "d:/root/content/",
            new List<string> { "Scene" }, 4242);
    }

    static int AddedCount(FakeChannel f)
    {
        return CountSent(f, TtCookMsgTypes.Add);
    }

    static int CountSent(FakeChannel f, string type)
    {
        var n = 0;
        foreach (var i in f.Sent)
            if (i.Type == type)
                n++;
        return n;
    }

    /// <summary>提交并走完握手拍, 返回当前假通道</summary>
    static FakeChannel SubmitAndHandshake(TtCookManager mgr, params string[] assets)
    {
        mgr.SubmitBatch(assets);
        mgr.Pump();
        return sFake;
    }

    static FakeChannel sFake;
    static List<FakeChannel> sCreated = new List<FakeChannel>();

    static TtCookManager MakeMgr()
    {
        sCreated.Clear();
        sFake = null;
        var mgr = new TtCookManager
        {
            WorkerFactory = (exe, dir, args) =>
            {
                var ch = new FakeChannel();
                // 真 worker 一起来就发 hello; 不模拟这一步则重启后的重投永远发不出去
                ch.Post(TtCookProtocol.Hello(TtCookProtocol.Version, "1.0.0.0", "hash",
                    "d:/root/content/", new List<string> { "Scene" }, 4242));
                sCreated.Add(ch);
                sFake = ch;
                return ch;
            },
        };
        return mgr;
    }

    static void Case(string name, Action body)
    {
        mCurrent = name;
        Console.WriteLine(name);
        try { body(); }
        catch (Exception ex) { Console.WriteLine($"  FAIL exception: {ex}"); mFailed++; }
    }

    static int Main()
    {
        Case("T1 handshake then normal finish", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            mgr.SubmitBatch(new[] { "a.scene" });
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            Check(AddedCount(fake) == 1, "one add dispatched after hello");
            fake.Post(TtCookProtocol.JobBegin(1, "a.scene", "Scene"));
            fake.Post(TtCookProtocol.JobStage(1, "create asset", 0.5));
            fake.Post(TtCookProtocol.JobDone(1, 0.9, new List<string> { "x.trlvl" }));
            mgr.Pump();
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Done, "job Done");
            Check(mgr.SettledCount == 1 && mgr.FailedCount == 0, "settled=1 failed=0");
            Check(mgr.Jobs[0].Artifacts != null && mgr.Jobs[0].Artifacts.Count == 1, "artifact carried");
        });

        Case("T2 late duplicate final line is ignored", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            mgr.SubmitBatch(new[] { "a.scene" });
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            fake.Post(TtCookProtocol.JobDone(1, 1.0, null));
            fake.Post(TtCookProtocol.JobDone(1, 2.0, null));
            fake.Post(TtCookProtocol.JobError(1, ECookErrorKind.Asset, "late", null, 3.0));
            mgr.Pump();
            Check(mgr.Jobs[0].ElapsedSec == 1.0, "first final wins (elapsed 1.0)");
            Check(mgr.SettledCount == 1 && mgr.FailedCount == 0, "counters not double-incremented");
        });

        Case("T3 cancel before start yields single final", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            mgr.SubmitBatch(new[] { "a.scene" });
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            fake.Post(TtCookProtocol.JobError(1, ECookErrorKind.Cancel, "cancelled before start", null, 0));
            fake.Post(TtCookProtocol.JobBegin(1, "a.scene", "Scene"));
            fake.Post(TtCookProtocol.JobDone(1, 3.0, null));
            mgr.Pump();
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Cancelled, "state stays Cancelled");
            Check(mgr.SettledCount == 1 && mgr.FailedCount == 0 && mgr.CancelledCount == 1, "cancelled counts as settled, not failed");
        });

        Case("T4 busy falls back to queue and redispatches on idle", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            mgr.SubmitBatch(new[] { "a.scene" });
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            fake.Post(TtCookProtocol.Busy(1, 512));
            mgr.Pump();
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Queued, "fell back to Queued");
            Check(AddedCount(fake) == 1, "not yet redispatched (no ready window)");
            fake.Post(TtCookProtocol.Idle(0));
            mgr.Pump();
            Check(AddedCount(fake) == 2, "redispatched after idle");
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Dispatched, "dispatched again");
        });

        Case("T5 running-job crash requeues, restarts, then gives up at retry limit", () =>
        {
            var mgr = MakeMgr(); var created = sCreated;
            var fake = SubmitAndHandshake(mgr, "a.scene");
            for (var round = 0; round < 3; round++)
            {
                var ch = created[round];
                Check(AddedCount(ch) == 1, $"round {round}: dispatched on channel {round}");
                ch.Post(TtCookProtocol.JobBegin(1, "a.scene", "Scene"));
                mgr.Pump();
                ch.Crash(unchecked((int)0xC0000005));
                mgr.Pump();   // 收敛退出 + 起新 worker
                if (round < 2)
                {
                    Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Queued, $"round {round}: back to Queued");
                    Check(created.Count == round + 2, $"round {round}: worker restarted");
                    mgr.Pump();   // 新 worker 握手, 重投
                }
            }
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Failed, "third running crash exceeds limit -> Failed");
            Check(mgr.FailedCount == 1, "counted as failed");
            mgr.Pump();
            Check(created.Count == 3, "no restart beyond retry limit");
        });

        Case("T6 duplicate asset is merged", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            var a = mgr.SubmitBatch(new[] { "a.scene", "A.SCENE", "a.scene" });
            Check(a == 1, "only one entry added");
            Check(mgr.TotalCount == 1 && mgr.Jobs.Count == 1, "batch size 1");
        });

        Case("T7 handshake version mismatch blocks dispatch", () =>
        {
            var created = new List<FakeChannel>();
            FakeChannel fake = null;
            var mgr = new TtCookManager
            {
                WorkerFactory = (e, d, s) => { fake = new FakeChannel(); created.Add(fake); return fake; },
            };
            mgr.SubmitBatch(new[] { "a.scene" });
            var msg = Hello();
            msg.Version = TtCookProtocol.Version + 1;
            fake.Post(msg);
            mgr.Pump();
            Check(AddedCount(fake) == 0, "no add dispatched when handshake rejected");
            Check(fake.QuitCount == 1, "quit requested from version mismatch path");
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Failed, "batch failed instead of retrying forever");
            fake.Crash(0);
            mgr.Pump();
            Check(created.Count == 1, "no restart after version mismatch");
        });

        Case("T8 dispatch window is capped", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            var list = new List<string>();
            for (var i = 0; i < 300; i++)
                list.Add($"asset{i}.scene");
            mgr.SubmitBatch(list);
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            Check(AddedCount(fake) == TtCookManager.MaxDispatched, $"exactly {TtCookManager.MaxDispatched} adds in flight");
            Check(mgr.Jobs.Count == 300, "all 300 queued");
            // 让前 100 个跑完, 窗口应当恰好补发 100 条 (总数 300 > 128+100, 不会被总量截断)
            for (long i = 1; i <= 100; i++)
                fake.Post(TtCookProtocol.JobDone(i, 0.1, null));
            mgr.Pump();
            var reopened = AddedCount(fake);
            Check(reopened == TtCookManager.MaxDispatched + 100, $"window reopened by exactly the settled count (adds={reopened})");
            fake.Post(TtCookProtocol.Ready(10));
            mgr.Pump();
            Check(AddedCount(fake) == reopened, "ready does not overfill a full window");
        });

        Case("T9 stage event forwarded", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            long sid = 0; string sname = null;
            mgr.OnJobStage = (id, stage) => { sid = id; sname = stage; };
            mgr.SubmitBatch(new[] { "a.scene" });
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            fake.Post(TtCookProtocol.JobStage(1, "resolve meta", 0.01));
            mgr.Pump();
            Check(sid == 1 && sname == "resolve meta", "OnJobStage got id+stage");
        });

        Case("T10 batch finished callback reports all-succeeded flag", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            bool? finished = null; bool allOk = true;
            mgr.OnBatchFinished = ok => { finished = ok; allOk = ok; };
            mgr.SubmitBatch(new[] { "a.scene", "b.scene" });
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            fake.Post(TtCookProtocol.JobDone(1, 0.1, null));
            mgr.Pump();
            Check(finished == null, "not finished while one still running");
            fake.Post(TtCookProtocol.JobError(2, ECookErrorKind.Asset, "bad", null, 0.2));
            mgr.Pump();
            Check(finished == false, "finished with allOk=false when a job failed");
        });

        Case("T11 cancel all sends cancel per non-final job", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            mgr.SubmitBatch(new[] { "a.scene", "b.scene" });
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            fake.Post(TtCookProtocol.JobDone(1, 0.1, null));
            mgr.Pump();
            fake.Sent.Clear();
            mgr.CancelAll();
            var cancels = 0;
            foreach (var i in fake.Sent)
                if (i.Type == TtCookMsgTypes.Cancel)
                    cancels++;
            Check(cancels == 1, "only the non-final job gets a cancel");
        });

        Case("T12 worker root captured from hello", () =>
        {
            var mgr = MakeMgr(); var fake = sFake; var created = sCreated;
            mgr.SubmitBatch(new[] { "a.scene" });
            fake = sFake;
            mgr.Pump();   // 握手在这一拍完成, 之后 Dispatch 才会发 add
            fake.Post(Hello());
            mgr.Pump();
            Check(mgr.WorkerRootPath == "d:/root/content/", "root from hello.Root");
        });

        Case("T13 planned restart (MaxJobs) requeues unstarted jobs without consuming retries", () =>
        {
            var mgr = MakeMgr(); var created = sCreated;
            var fake = SubmitAndHandshake(mgr, "a.scene", "b.scene");
            // worker 跑够配额: 已接收未开工的直接作废, 不发任何终态
            for (var round = 0; round < 4; round++)
            {
                var ch = created[created.Count - 1];
                ch.Post(TtCookProtocol.Quit("restart"));
                mgr.Pump();
                ch.Crash(0);
                mgr.Pump();
                mgr.Pump();
            }
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Dispatched
                && mgr.Jobs[1].State == TtCookManager.ECookJobState.Dispatched, "both still alive after 4 planned restarts");
            Check(created.Count == 5, $"restarted each time (channels={created.Count})");
        });

        Case("T14 worker dying before hello gives up after bounded attempts", () =>
        {
            var created = new List<FakeChannel>();
            var mgr = new TtCookManager
            {
                WorkerFactory = (e, d, s) => { var ch = new FakeChannel(); created.Add(ch); return ch; },
            };
            bool? finished = null;
            mgr.OnBatchFinished = ok => finished = ok;
            mgr.SubmitBatch(new[] { "a.scene" });
            for (var i = 0; i < 10 && created.Count > 0 && created[created.Count - 1].Alive; i++)
            {
                created[created.Count - 1].Crash(-1);
                mgr.Pump();
            }
            Check(created.Count == TtCookManager.MaxUnattributedExits, $"exactly {TtCookManager.MaxUnattributedExits} start attempts (got {created.Count})");
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Failed, "job failed, not stuck Queued");
            Check(finished == false, "batch finished with failure");
        });

        Case("T15 cancel all: queued cancelled locally, in-flight get one cancel each", () =>
        {
            var mgr = MakeMgr();
            var list = new List<string>();
            for (var i = 0; i < TtCookManager.MaxDispatched + 5; i++)
                list.Add($"asset{i}.scene");
            mgr.SubmitBatch(list);
            mgr.Pump();
            var fake = sFake;
            fake.Sent.Clear();
            mgr.CancelAll();
            mgr.CancelAll();   // 重复点不应重发
            mgr.Pump();
            Check(CountSent(fake, TtCookMsgTypes.Cancel) == TtCookManager.MaxDispatched, $"one cancel per in-flight job (got {CountSent(fake, TtCookMsgTypes.Cancel)})");
            Check(AddedCount(fake) == 0, "no add sent after cancel");
            Check(mgr.CancelledCount == 5, "never-sent jobs cancelled locally");
        });

        Case("T16 exit while cancel pending resolves as Cancelled, not requeue", () =>
        {
            var mgr = MakeMgr(); var created = sCreated;
            var fake = SubmitAndHandshake(mgr, "a.scene");
            fake.Post(TtCookProtocol.JobBegin(1, "a.scene", "Scene"));
            mgr.Pump();
            mgr.CancelAll();
            fake.Crash(1);
            mgr.Pump();
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Cancelled, "Cancelled");
            Check(created.Count == 1, "no restart");
        });

        Case("T17 stop worker: no restart, leftovers cancelled, submit refused while stopping", () =>
        {
            var mgr = MakeMgr(); var created = sCreated;
            var fake = SubmitAndHandshake(mgr, "a.scene", "b.scene");
            fake.Post(TtCookProtocol.JobBegin(1, "a.scene", "Scene"));
            mgr.Pump();
            mgr.RequestWorkerShutdown();
            Check(fake.QuitCount == 1 && mgr.IsStopping, "quit sent, stopping");
            Check(mgr.SubmitBatch(new[] { "c.scene" }) == 0, "submit refused while stopping");
            fake.Post(TtCookProtocol.Quit("manual"));
            mgr.Pump();
            fake.Crash(0);
            mgr.Pump();
            mgr.Pump();
            Check(created.Count == 1, "no restart after stop");
            Check(mgr.CancelledCount == 2 && mgr.SettledCount == 2, "both settled as Cancelled");
            Check(mgr.IsStopping == false && mgr.IsWorkerRunning == false, "stopped state cleared");
        });

        Case("T18 lines drained after process died do not replace the worker", () =>
        {
            var mgr = MakeMgr(); var created = sCreated;
            var fake = SubmitAndHandshake(mgr, "a.scene", "b.scene");
            // 进程已退但退出事件未派发, 队列里还有它最后的 job.done
            fake.Alive = false;
            fake.Post(TtCookProtocol.JobDone(1, 0.1, null));
            mgr.Pump();
            Check(created.Count == 1, "no new worker spawned while old exit is unprocessed");
            Check(mgr.Jobs[0].State == TtCookManager.ECookJobState.Done, "late done accepted");
            fake.OnProcessExited(0);
            mgr.Pump();
            mgr.Pump();
            Check(created.Count == 2, "restart after exit processed");
            Check(AddedCount(created[1]) == 1, "only the unfinished job re-dispatched");
        });

        Case("T19 reset refused while jobs in flight", () =>
        {
            var mgr = MakeMgr();
            var fake = SubmitAndHandshake(mgr, "a.scene");
            Check(mgr.ResetBatch() == false && mgr.TotalCount == 1, "refused while running");
            fake.Post(TtCookProtocol.JobDone(1, 0.1, null));
            mgr.Pump();
            Check(mgr.ResetBatch() && mgr.TotalCount == 0, "allowed after settle");
        });

        Case("T20 stage stored on entry and cleared on final", () =>
        {
            var mgr = MakeMgr();
            var fake = SubmitAndHandshake(mgr, "a.scene");
            fake.Post(TtCookProtocol.JobBegin(1, "a.scene", "Scene"));
            fake.Post(TtCookProtocol.JobStage(1, "create asset", 0.2));
            mgr.Pump();
            Check(mgr.Jobs[0].Stage == "create asset", "stage on entry");
            fake.Post(TtCookProtocol.JobDone(1, 0.3, null));
            mgr.Pump();
            Check(mgr.Jobs[0].Stage == null, "stage cleared");
        });

        Case("T21 busy holds dispatch across frames until ready", () =>
        {
            var mgr = MakeMgr();
            var fake = SubmitAndHandshake(mgr, "a.scene", "b.scene");
            fake.Post(TtCookProtocol.Busy(2, 512));
            mgr.Pump();
            mgr.Pump();
            mgr.Pump();
            Check(AddedCount(fake) == 2, "held for several frames");
            fake.Post(TtCookProtocol.Ready(10));
            mgr.Pump();
            Check(AddedCount(fake) == 3 && mgr.Jobs[1].State == TtCookManager.ECookJobState.Dispatched, "released by ready");
        });

        Case("T22 sub-progress is visible without settling the asset", () =>
        {
            var mgr = MakeMgr();
            var fake = SubmitAndHandshake(mgr, "terrain.scene");
            fake.Post(TtCookProtocol.JobBegin(1, "terrain.scene", "Scene"));
            fake.Post(TtCookProtocol.JobProgress(1, 0.5f / 16));
            mgr.Pump();
            Check(mgr.Jobs[0].ProgressFraction == 0.5f / 16, "first trlvl step recorded");
            Check(mgr.SettledCount == 0 && mgr.Jobs[0].State == TtCookManager.ECookJobState.Running, "sub-progress does not mean asset done");
            fake.Post(TtCookProtocol.JobProgress(1, 2f));
            mgr.Pump();
            Check(mgr.Jobs[0].ProgressFraction == 1f && mgr.SettledCount == 0, "clamped but still running until terminal");
        });

        Case("T23 heartbeat tracks responsiveness, not work-stage advancement", () =>
        {
            var mgr = MakeMgr();
            var fake = SubmitAndHandshake(mgr, "terrain.scene");
            fake.Post(TtCookProtocol.JobBegin(1, "terrain.scene", "Scene"));
            fake.Post(TtCookProtocol.JobStage(1, "QEM LOD 1: triangles=500000", 12));
            mgr.Pump();
            var entry = mgr.Jobs[0];
            entry.LastMessageTick = 1;
            entry.LastStageTick = 2;
            fake.Post(TtCookProtocol.Beat(1, 3, 20));
            mgr.Pump();
            Check(entry.LastMessageTick != 1 && entry.ElapsedSec == 20, "heartbeat updates message age and worker elapsed");
            Check(entry.LastStageTick == 2 && entry.Stage.Contains("triangles=500000"), "heartbeat does not pretend stage advanced");
            Check(entry.ProgressFraction == -1, "heartbeat does not invent percent");
        });

        Case("T24 late telemetry cannot revive a terminal asset", () =>
        {
            var mgr = MakeMgr();
            var fake = SubmitAndHandshake(mgr, "terrain.scene");
            fake.Post(TtCookProtocol.JobBegin(1, "terrain.scene", "Scene"));
            fake.Post(TtCookProtocol.JobDone(1, 2, null));
            mgr.Pump();
            var entry = mgr.Jobs[0];
            var tick = entry.LastMessageTick;
            fake.Post(TtCookProtocol.JobStage(1, "late QEM", 100));
            fake.Post(TtCookProtocol.JobProgress(1, 0.2f));
            fake.Post(TtCookProtocol.Beat(1, 4, 101));
            mgr.Pump();
            Check(entry.Stage == null && entry.ProgressFraction == 1f, "late stage/progress ignored");
            Check(entry.ElapsedSec == 2 && entry.LastMessageTick == tick && mgr.SettledCount == 1, "terminal elapsed and count preserved");
        });

        Case("T25 retry resets progress and invalid telemetry is ignored", () =>
        {
            var mgr = MakeMgr();
            var fake = SubmitAndHandshake(mgr, "terrain.scene");
            fake.Post(TtCookProtocol.JobBegin(1, "terrain.scene", "Scene"));
            fake.Post(TtCookProtocol.JobProgress(1, 0.7f));
            fake.Post(TtCookProtocol.JobStage(1, "old attempt", 30));
            mgr.Pump();
            fake.Crash(1);
            var entry = mgr.Jobs[0];
            Check(entry.ProgressFraction == -1 && entry.ElapsedSec == 0 && entry.Stage == null, "retry has no stale progress");
            mgr.Pump();
            fake = sFake;
            fake.Post(TtCookProtocol.JobBegin(1, "terrain.scene", "Scene"));
            fake.Post(TtCookProtocol.JobProgress(1, float.NaN));
            fake.Post(TtCookProtocol.JobProgress(1, float.PositiveInfinity));
            fake.Post(new TtCookMessage { Type = TtCookMsgTypes.JobProgress, JobId = 1 });
            mgr.Pump();
            Check(entry.ProgressFraction == -1, "invalid/missing fractions ignored");
            fake.Post(TtCookProtocol.JobProgress(1, -0.5f));
            mgr.Pump();
            Check(entry.ProgressFraction == 0, "negative fraction clamped");
        });

        Case("T27 context stall clock resets on any report from any thread", () =>
        {
            var ctx = new TtCookContext(1, "a.scene", "Scene", false, new NullSink());
            System.Threading.Thread.Sleep(200);
            Check(ctx.IdleSec >= 0.15, "idle grows from job start before first report");
            ctx.ReportStage("pgc");
            Check(ctx.IdleSec < 0.15, "stage resets idle");
            ctx.ReportProgress(0.5f);
            System.Threading.Thread.Sleep(200);
            ctx.ReportProgress(0.5f);
            Check(ctx.IdleSec < 0.15, "duplicate progress still counts as activity");
            System.Threading.Thread.Sleep(200);
            var worker = new System.Threading.Thread(() => ctx.ReportArtifact("cache/x.trlvl"));
            worker.Start();
            worker.Join();
            Check(ctx.IdleSec < 0.15, "artifact from another thread resets idle");
            Check(ctx.IsCancellationRequested == false, "activity tracking never cancels by itself");
        });

        Console.WriteLine(mFailed == 0 ? "ALL PASS" : $"FAILED={mFailed}");
        return mFailed == 0 ? 0 : 1;
    }
}
