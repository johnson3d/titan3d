using System;
using System.Collections.Generic;
using System.IO;
using EngineNS.IO;

namespace EngineNS.Editor.Cook
{
    /// <summary>
    /// 编辑器侧的 cook 批次提交与调度: 把要 cook 的资产排队成批次, 派发给常驻 TitanCMD worker,
    /// 按协议行更新每个 job 的状态, worker 崩了就标 Crash 并在重试上限内拉起新进程重投。
    /// 设计见 Documents\editor\CookManager.md (M1)。
    ///
    /// 线程约定: 所有公开方法与事件都在引擎主线程上调用/派发 (Tick 里调 Pump)。本类不起线程、
    /// 不 await 任何 CLR task —— 线程池回调只存在于 TtCookWorker 的入队环节。
    ///
    /// 状态机铁律 (协议侧要求"一个 job id 只允许一个终态行", 提交侧必须同向防御):
    /// 终态只接受第一次, 之后同 id 的任何终态一律忽略并留日志。worker 若实现有 bug 发出双终态,
    /// 这里也不会把计数算错或让面板状态抖动。
    /// </summary>
    public class TtCookManager : EngineNS.ITickable
    {
        static TtCookManager mInstance;
        static bool mTickRegistered;
        /// <summary>
        /// 编辑器全局唯一入口。注册进 TickableManager 而不是靠面板 OnDraw 驱动:
        /// DrawRootForms 会跳过 Visible == false 的表单, 面板一关批次就静默停摆。
        /// TickableManager 持弱引用, 所以必须靠这个静态字段把实例钉住。
        /// </summary>
        public static TtCookManager Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = new TtCookManager();
                if (mTickRegistered == false && TtEngine.Instance != null)
                {
                    TtEngine.Instance.TickableManager.AddTickable(mInstance);
                    mTickRegistered = true;
                }
                return mInstance;
            }
        }

        /// <summary>编辑器退出时调用; 没创建过就什么都不做 (不能走 Instance, 那会在退出路径上新建并注册 tick)</summary>
        public static void ShutdownIfCreated()
        {
            mInstance?.Shutdown();
        }

        public enum ECookJobState
        {
            Queued,
            Dispatched,
            Running,
            Done,
            Failed,
            Cancelled,
            /// <summary>worker 崩了/退出导致结果未知, 可重投</summary>
            Crash,
        }

        public sealed class FJobEntry
        {
            public long Id;
            public string Asset;
            public string AssetType;
            public bool Force;
            public ECookJobState State = ECookJobState.Queued;
            public string Error;
            /// <summary>worker 最近报的阶段 (job.stage), 浮窗显示用; 终态或回队列时清空</summary>
            public string Stage;
            /// <summary>已发过 cancel 但 worker 还没回终态; worker 若此时退出, 按取消收敛而不是重投</summary>
            public bool CancelRequested;
            public double ElapsedSec;
            /// <summary>worker 上报的当前工作步骤比例; -1 表示未知, 不影响资产终态计数。</summary>
            public float ProgressFraction = -1f;
            public long StartedTick;
            public long LastMessageTick;
            public long LastStageTick;
            public double DisplayElapsedSec => State == ECookJobState.Running
                ? Math.Max(ElapsedSec, (Environment.TickCount64 - StartedTick) / 1000.0) : ElapsedSec;
            public List<string> Artifacts;

            internal void ResetRuntimeProgress()
            {
                Stage = null;
                ProgressFraction = -1f;
                ElapsedSec = 0;
                StartedTick = LastMessageTick = LastStageTick = 0;
            }
            public bool IsFinal
            {
                get
                {
                    return State == ECookJobState.Done || State == ECookJobState.Failed
                        || State == ECookJobState.Cancelled;
                }
            }
        }

        /// <summary>一次最多往 worker 挂多少条 add: worker 队列上限 512, 留出余量并降低崩溃时的重投规模</summary>
        public const int MaxDispatched = 128;
        /// <summary>单个 job 因 worker 崩溃而重投的上限, 超过就判 Failed, 免得坏资产把 worker 反复拖死</summary>
        public const int MaxCrashRetry = 2;
        /// <summary>
        /// 连续多少次"归因不到具体 job"的异常退出 (握手前就崩 / fatal / 无 job 在跑时挂掉) 后整批放弃。
        /// 这类退出不消耗任何 job 的重试次数, 不设上限就会无限拉起进程。
        /// </summary>
        public const int MaxUnattributedExits = 3;

        /// <summary>worker 可执行文件; 留 null 则按 cwd 推导 cwd/net10.0/TitanCMD.exe</summary>
        public string WorkerExePath;
        /// <summary>worker 配置; 相对 cwd, 默认与冒烟一致</summary>
        public string WorkerCfgPath = @"..\content\config\engineconfig_windows_cook.jscfg";
        /// <summary>worker 的 native 集 (NativeDLL=); 留空表示不传, 由 worker 侧配置文件决定, 避免与编辑器当前集错配</summary>
        public string WorkerNativeSet;
        /// <summary>单 job 总时长上限, 0 = 不限; 卡死由 WorkerStallSec (无进度上报时长) 判定</summary>
        public int WorkerJobTimeoutSec = 0;
        public int WorkerStallSec = 600;
        public int WorkerMaxJobs = 200;

        /// <summary>批次进度变化 (settled 已了结数 (含取消), total 总数, failed 其中失败数)</summary>
        public Action<int, int, int> OnProgressChanged;
        /// <summary>整批结束 (队列表里全部终态)</summary>
        public Action<bool> OnBatchFinished;
        /// <summary>给面板/日志用的文本事件</summary>
        public Action<string> OnNotice;

        readonly Dictionary<long, FJobEntry> mJobs = new Dictionary<long, FJobEntry>();
        /// <summary>提交顺序; 派发与重投都按这个顺序走, 保证面板显示稳定</summary>
        readonly List<FJobEntry> mOrder = new List<FJobEntry>();
        readonly Dictionary<long, int> mCrashRetry = new Dictionary<long, int>();
        long mNextId = 1;
        int mDispatchedUnacked;
        ITtCookWorkerChannel mWorker;
        bool mHandshakeOk;
        bool mBatchClosed;
        bool mRestartScheduled;
        /// <summary>收到 busy 后暂停派发, 直到 worker 报 ready/idle; 否则同一帧尾部就会把退回的 add 再塞回去</summary>
        bool mDispatchHeld;
        /// <summary>用户点了 Stop Worker: 不再派发, 退出后剩余未终态 job 判 Cancelled 且不重启</summary>
        bool mStopRequested;
        /// <summary>当前 worker 的退出已收敛完, 等 Pump 释放句柄</summary>
        bool mWorkerExited;
        /// <summary>当前 worker 用 quit 行自报的退出原因; 有原因 (且非 fatal) 的退出是计划内的</summary>
        string mLastQuitReason;
        int mUnattributedExits;

        /// <summary>
        /// worker 通道工厂 (exe, workingDir, args)。留 null = 起真进程;
        /// 单测把它换成假通道, 就能逐行灌录好的协议序列验状态机而不碰任何子进程。
        /// </summary>
        public Func<string, string, string, ITtCookWorkerChannel> WorkerFactory;

        /// <summary>job 阶段推送 (浮窗用, 已在引擎线程上)</summary>
        public Action<long, string> OnJobStage;

        /// <summary>当前 worker 句柄; 面板取日志快照用, 未起为 null</summary>
        public ITtCookWorkerChannel WorkerHost { get { return mWorker; } }
        /// <summary>worker 进程 pid; 未起为 -1</summary>
        public int ProcessId { get { return mWorker != null ? mWorker.ProcessId : -1; } }

        public bool IsWorkerRunning { get { return mWorker != null && mWorker.IsAlive; } }
        public IReadOnlyList<FJobEntry> Jobs { get { return mOrder; } }
        public bool IsBatchEmpty { get { return mOrder.Count == 0; } }

        /// <summary>worker 报回来的 game root (hello.Root); 面板扫资产用它, 不依赖其它子系统</summary>
        public string WorkerRootPath { get; private set; }

        /// <summary>累计了结/失败数, 浮窗直接用</summary>
        public int SettledCount { get; private set; }
        public int TotalCount { get { return mOrder.Count; } }
        public int FailedCount { get; private set; }
        public int CancelledCount { get; private set; }
        /// <summary>Stop Worker 已发出, worker 尚未退出</summary>
        public bool IsStopping { get { return mStopRequested; } }

        public void TickLogic(float ellapse)
        {
            Pump();
        }
        public void TickRender(float ellapse) { }
        public void TickBeginFrame(float ellapse) { }
        public void TickSync(float ellapse) { }

        #region 提交
        /// <summary>
        /// 提交一批资产。已在批次里的同一资产 (大小写不敏感) 直接合并, 不重复 cook。
        /// 返回本次新增的条数。
        /// </summary>
        public int SubmitBatch(IEnumerable<string> assetPaths, string assetType = "Scene", bool force = false)
        {
            if (assetPaths == null)
                return 0;
            if (mStopRequested)
            {
                Notice("worker is stopping; submit again after it exits");
                return 0;
            }
            var added = 0;
            foreach (var path in assetPaths)
            {
                if (string.IsNullOrEmpty(path))
                    continue;
                if (FindByUrl(path) != null)
                    continue;
                var entry = new FJobEntry
                {
                    Id = mNextId++,
                    Asset = path,
                    AssetType = assetType,
                    Force = force,
                };
                mJobs[entry.Id] = entry;
                mOrder.Add(entry);
                added++;
            }
            if (added > 0)
            {
                mBatchClosed = false;
                mUnattributedExits = 0;
                NotifyProgress();
                Dispatch();
            }
            return added;
        }

        FJobEntry FindByUrl(string asset)
        {
            foreach (var i in mOrder)
            {
                if (string.Equals(i.Asset, asset, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return null;
        }

        /// <summary>
        /// 批次跑完后清空状态表, 面板据此回到初始态。还有未终态 job 时拒绝 (返回 false):
        /// 那些 job 还在 worker 手上, 清表会让它们的终态行变成"未知 id", 节流窗口也会算错。
        /// </summary>
        public bool ResetBatch()
        {
            foreach (var i in mOrder)
            {
                if (i.IsFinal == false)
                    return false;
            }
            mJobs.Clear();
            mOrder.Clear();
            mCrashRetry.Clear();
            mDispatchedUnacked = 0;
            mBatchClosed = false;
            NotifyProgress();
            return true;
        }
        #endregion

        #region worker 生命周期
        void EnsureWorker()
        {
            // 只在空位上起新进程。旧 worker 必须先经 HandleProcessExited 收敛、再由 Pump 释放:
            // 若在处理它最后几行协议时 (进程已退、退出事件未派发) 就顶掉它,
            // 迟到的退出事件会打到新 worker 头上, 把新 worker 的握手和队列一起冲掉
            if (mWorker != null)
                return;
            mHandshakeOk = false;
            mRestartScheduled = false;
            mDispatchHeld = false;
            mLastQuitReason = null;

            var cwd = Directory.GetCurrentDirectory();
            var exe = string.IsNullOrEmpty(WorkerExePath)
                ? Path.Combine(cwd, "net10.0", "TitanCMD.exe")
                : WorkerExePath;
            if (WorkerFactory == null && File.Exists(exe) == false)
            {
                // 只有真进程通路才需要校验可执行文件; 工厂接管时 exe 路径不参与创建
                Notice($"cook worker not found: {exe}");
                FailBatch("worker executable missing");
                return;
            }

            var args = $"CookCfg={WorkerCfgPath} ExeCmd=CookServer"
                + $" WorkerJobTimeout={WorkerJobTimeoutSec} WorkerStallSec={WorkerStallSec} WorkerMaxJobs={WorkerMaxJobs}";
            if (string.IsNullOrEmpty(WorkerNativeSet) == false)
                args += $" NativeDLL={WorkerNativeSet}";

            try
            {
                mWorker = WorkerFactory != null
                    ? WorkerFactory(exe, cwd, args)
                    : new TtCookWorker(exe, cwd, args);
                mWorker.OnProtocolMessage = HandleProtocol;
                mWorker.OnProcessExited = HandleProcessExited;
                mWorker.OnLogLine = null;
            }
            catch (Exception ex)
            {
                // 起进程失败绝不能把编辑器带崩
                mWorker = null;
                Notice($"cook worker start failed: {ex.Message}");
                FailBatch("worker start failed");
            }
        }

        /// <summary>
        /// 按 worker 的接受能力补发 add。mDispatchedUnacked 是"已写出去但还没收到任何终态"的条数,
        /// 用它做节流窗口, 免得一次把上千条 add 灌进 worker 的 512 队列。
        /// </summary>
        void Dispatch()
        {
            if (mBatchClosed || mStopRequested)
                return;
            if (mWorker == null)
                EnsureWorker();
            if (IsWorkerRunning == false || mHandshakeOk == false || mDispatchHeld)
                return;

            var sent = 0;
            foreach (var entry in mOrder)
            {
                if (mDispatchedUnacked + sent >= MaxDispatched)
                    break;
                if (entry.State != ECookJobState.Queued)
                    continue;
                var msg = new TtCookMessage
                {
                    Type = TtCookMsgTypes.Add,
                    JobId = entry.Id,
                    Asset = entry.Asset,
                    AssetType = entry.AssetType,
                    Force = entry.Force ? true : (bool?)null,
                };
                if (mWorker.Send(msg) == false)
                    break;
                entry.State = ECookJobState.Dispatched;
                sent++;
            }
            mDispatchedUnacked += sent;
        }

        public void CancelAll()
        {
            // 还没发出去的直接本地判掉: 若也给它发 cancel, 同一帧尾部的 Dispatch 会紧接着把 add 发过去,
            // worker 先回 "not found" 再照常把它跑完 —— 白干一场, 还多一条迟到终态
            var alive = IsWorkerRunning;
            foreach (var i in mOrder)
            {
                if (i.State == ECookJobState.Queued)
                {
                    MarkFinal(i, ECookJobState.Cancelled, "Cancel: cancelled before dispatch");
                }
                else if (i.State == ECookJobState.Dispatched || i.State == ECookJobState.Running)
                {
                    if (alive && i.CancelRequested == false)
                    {
                        i.CancelRequested = true;
                        mWorker.Send(new TtCookMessage { Type = TtCookMsgTypes.Cancel, JobId = i.Id });
                    }
                    else if (alive == false)
                    {
                        // worker 已退/正在退: 没人可发, 由退出收敛按 CancelRequested 判取消
                        i.CancelRequested = true;
                    }
                }
            }
            NotifyProgress();
        }

        /// <summary>
        /// Stop Worker: 取消全部未终态 job 并请 worker 收摊 (不阻塞)。
        /// worker 退出后剩余未终态的判 Cancelled, 不重启。
        /// </summary>
        public void RequestWorkerShutdown()
        {
            CancelAll();
            if (IsWorkerRunning == false)
                return;
            mStopRequested = true;
            mWorker.RequestQuit("manual");
        }

        /// <summary>硬收尾 (编辑器退出用): 阻塞等 worker 退出, 超时硬杀</summary>
        public void Shutdown()
        {
            if (mWorker != null)
            {
                mWorker.Dispose();
                mWorker = null;
            }
            mHandshakeOk = false;
            mRestartScheduled = false;
            mWorkerExited = false;
            mStopRequested = false;
            // Dispose 之后不会再有退出事件, 手上的 job 必须在这里收敛, 否则批次永远停在运行中
            foreach (var i in mOrder)
            {
                if (i.IsFinal == false)
                    MarkFinal(i, ECookJobState.Cancelled, "Cancel: worker shut down");
            }
            mDispatchedUnacked = 0;
            NotifyProgress();
        }
        #endregion

        #region 协议消费
        /// <summary>由编辑器主循环每帧调用</summary>
        public void Pump()
        {
            if (mWorker != null)
            {
                mWorker.Pump();
                if (mWorkerExited)
                {
                    // 退出已在 HandleProcessExited 里收敛完, 到这里才释放句柄、空出位子给重启
                    mWorkerExited = false;
                    mWorker.Dispose();
                    mWorker = null;
                }
            }

            if (mRestartScheduled && mWorker == null)
                EnsureWorker();

            if (IsWorkerRunning && mHandshakeOk)
                Dispatch();

            CheckBatchFinished();
        }

        void HandleProtocol(TtCookMessage msg)
        {
            switch (msg.Type)
            {
                case TtCookMsgTypes.Hello:
                    HandleHello(msg);
                    break;
                case TtCookMsgTypes.Ready:
                case TtCookMsgTypes.Idle:
                    // worker 报"我排完了一段", 补发窗口由此打开 (也解除 busy 造成的暂停)
                    mDispatchHeld = false;
                    Dispatch();
                    break;
                case TtCookMsgTypes.JobBegin:
                    Transition(msg.JobId, ECookJobState.Running);
                    break;
                case TtCookMsgTypes.JobStage:
                    if (TryUpdateRuntimeMessage(msg, out var staged))
                    {
                        staged.Stage = msg.Stage;
                        staged.LastStageTick = Environment.TickCount64;
                        OnJobStage?.Invoke(staged.Id, msg.Stage);
                    }
                    break;
                case TtCookMsgTypes.JobProgress:
                    if (msg.Fraction.HasValue && !float.IsNaN(msg.Fraction.Value) && !float.IsInfinity(msg.Fraction.Value) &&
                        TryUpdateRuntimeMessage(msg, out var progressed))
                        progressed.ProgressFraction = Math.Clamp(msg.Fraction.Value, 0f, 1f);
                    break;
                case TtCookMsgTypes.Beat:
                    TryUpdateRuntimeMessage(msg, out _);
                    break;
                case TtCookMsgTypes.JobDone:
                {
                    if (TryFinal(msg.JobId, out var entry) == false)
                        break;
                    entry.State = ECookJobState.Done;
                    entry.ProgressFraction = 1f;
                    entry.ElapsedSec = msg.ElapsedSec ?? 0.0;
                    entry.Artifacts = msg.Artifacts;
                    RetireDispatched();
                    NotifyProgress();
                    break;
                }
                case TtCookMsgTypes.JobError:
                {
                    if (TryFinal(msg.JobId, out var entry) == false)
                        break;
                    var kind = msg.Kind ?? ECookErrorKind.Asset.ToString();
                    entry.Error = $"{kind}: {msg.Message}";
                    entry.ElapsedSec = msg.ElapsedSec ?? 0.0;
                    // worker 侧的超时/取消都已经是终态, 这里不再重投
                    entry.State = kind == ECookErrorKind.Cancel.ToString()
                        ? ECookJobState.Cancelled : ECookJobState.Failed;
                    RetireDispatched();
                    NotifyProgress();
                    break;
                }
                case TtCookMsgTypes.Busy:
                {
                    // worker 队列满了: 退回 Queued 并暂停派发, 等下一个 ready/idle 窗口, 不算失败
                    mDispatchHeld = true;
                    if (msg.JobId.HasValue && mJobs.TryGetValue(msg.JobId.Value, out var entry))
                    {
                        if (entry.State == ECookJobState.Dispatched)
                        {
                            entry.State = ECookJobState.Queued;
                            mDispatchedUnacked = Math.Max(0, mDispatchedUnacked - 1);
                        }
                    }
                    break;
                }
                case TtCookMsgTypes.Stats:
                    Notice($"worker stats: done={msg.Done} failed={msg.Failed} skipped={msg.Skipped}");
                    break;
                case TtCookMsgTypes.Quit:
                    mLastQuitReason = msg.Reason;
                    Notice($"worker quitting: {msg.Reason}");
                    break;
                default:
                    // beat 等纯心跳行不改动状态; 未知类型按协议约定宽容忽略
                    break;
            }
        }

        bool TryUpdateRuntimeMessage(TtCookMessage msg, out FJobEntry entry)
        {
            entry = null;
            if (!msg.JobId.HasValue || !mJobs.TryGetValue(msg.JobId.Value, out entry) ||
                (entry.State != ECookJobState.Running && entry.State != ECookJobState.Dispatched))
                return false;
            entry.LastMessageTick = Environment.TickCount64;
            if (msg.ElapsedSec.HasValue && double.IsFinite(msg.ElapsedSec.Value) && msg.ElapsedSec.Value >= 0)
                entry.ElapsedSec = Math.Max(entry.ElapsedSec, msg.ElapsedSec.Value);
            return true;
        }

        void HandleHello(TtCookMessage msg)
        {
            mHandshakeOk = false;
            if (msg.Version != TtCookProtocol.Version || msg.IsVersionSupported == false)
            {
                Notice($"worker protocol version {msg.Version} unsupported (engine expects {TtCookProtocol.Version})");
                mWorker?.RequestQuit("version");
                // 版本不符重启多少次都一样, 直接整批判掉
                FailBatch("worker protocol version mismatch");
                return;
            }
            mHandshakeOk = true;
            WorkerRootPath = msg.Root;
            Notice($"worker up: pid={msg.Pid} root={msg.Root} types={msg.Types?.Count ?? 0}");
        }

        void HandleProcessExited(int exitCode)
        {
            mHandshakeOk = false;
            mDispatchHeld = false;
            mDispatchedUnacked = 0;
            mWorkerExited = true;
            var stopped = mStopRequested;
            mStopRequested = false;
            // 用 quit 行自报了原因的退出 (跑够 job 换进程 restart / 空闲 idle / 我们请它走) 是计划内的, 不算崩溃
            var graceful = mLastQuitReason != null && mLastQuitReason != "fatal";
            var requeued = 0;
            var attributed = false;
            foreach (var i in mOrder)
            {
                if (i.IsFinal)
                    continue;
                if (stopped || i.CancelRequested)
                {
                    MarkFinal(i, ECookJobState.Cancelled,
                        stopped ? "Cancel: worker stopped" : "Cancel: worker exited before confirming cancel");
                    continue;
                }
                // 只有正在跑的 job 才可能是拖死 worker 的那个; 排队中/已发未开工的不背锅, 重投不计次数
                // (worker 跑够 MaxJobs 后计划内退出时, 未开工的 job 本来就要提交侧重投)
                if (i.State == ECookJobState.Running && graceful == false)
                {
                    attributed = true;
                    mCrashRetry.TryGetValue(i.Id, out var tried);
                    if (tried >= MaxCrashRetry)
                    {
                        MarkFinal(i, ECookJobState.Failed, $"Crash: worker exit {exitCode}, retry limit reached");
                        continue;
                    }
                    mCrashRetry[i.Id] = tried + 1;
                }
                i.State = ECookJobState.Queued;
                i.ResetRuntimeProgress();
                requeued++;
            }

            if (graceful == false && attributed == false)
                mUnattributedExits++;
            if (requeued > 0 && mUnattributedExits >= MaxUnattributedExits)
            {
                FailBatch($"worker exited {mUnattributedExits} times without progress (last exit={exitCode}), see worker log");
                return;
            }
            if (requeued > 0 && mBatchClosed == false)
            {
                mRestartScheduled = true;
                Notice($"worker exit={exitCode} ({mLastQuitReason ?? "no quit line"}); {requeued} job(s) back to queue, restarting");
            }
            else
            {
                Notice($"worker exit={exitCode} ({mLastQuitReason ?? "no quit line"})");
            }
            NotifyProgress();
        }
        #endregion

        #region 状态收敛
        bool TryFinal(long? id, out FJobEntry entry)
        {
            entry = null;
            if (id.HasValue == false || mJobs.TryGetValue(id.Value, out entry) == false)
            {
                Notice($"protocol line for unknown job id={id}");
                return false;
            }
            if (entry.IsFinal)
            {
                // 双终态: 第二次一律忽略 (计数与面板都不能再动)
                Notice($"job {entry.Id} already final ({entry.State}); ignoring late line");
                entry = null;
                return false;
            }
            if (entry.State == ECookJobState.Queued)
            {
                // 未开工就被判终态 (worker 侧 cancelled before start): 不占节流窗口, 什么都不减
            }
            else
            {
                mDispatchedUnacked = Math.Max(0, mDispatchedUnacked - 1);
            }
            entry.Stage = null;
            // 有 job 正常了结说明 worker 在干活, 之前的"无进展退出"计数作废
            mUnattributedExits = 0;
            return true;
        }

        void Transition(long? id, ECookJobState next)
        {
            if (id.HasValue == false || mJobs.TryGetValue(id.Value, out var entry) == false)
                return;
            if (entry.IsFinal)
                return;
            if (next == ECookJobState.Running && entry.State != ECookJobState.Running)
            {
                entry.ResetRuntimeProgress();
                entry.StartedTick = entry.LastMessageTick = entry.LastStageTick = Environment.TickCount64;
            }
            entry.State = next;
        }

        void MarkFinal(FJobEntry entry, ECookJobState state, string error)
        {
            if (entry.IsFinal)
                return;
            // 先按“离开时”的状态判定是否占着节流窗口, 再改状态
            if (entry.State == ECookJobState.Dispatched || entry.State == ECookJobState.Running)
                mDispatchedUnacked = Math.Max(0, mDispatchedUnacked - 1);
            entry.State = state;
            entry.Error = error;
            entry.Stage = null;
        }

        /// <summary>worker 根本起不来时把批次一次性判掉, 否则面板永远卡在运行中</summary>
        void FailBatch(string reason)
        {
            foreach (var i in mOrder)
            {
                if (i.IsFinal == false)
                    MarkFinal(i, ECookJobState.Failed, reason);
            }
            mBatchClosed = true;
            NotifyProgress();
            Notice($"cook batch aborted: {reason}");
            OnBatchFinished?.Invoke(false);
        }

        /// <summary>向 worker 要一次累计计数 (worker 只在收到 flush 时回 stats)</summary>
        public void RequestStats()
        {
            if (IsWorkerRunning)
                mWorker.Send(new TtCookMessage { Type = TtCookMsgTypes.Flush });
        }

        void RetireDispatched()
        {
            // 节流窗口由 TryFinal 统一递减, 这里只负责把可重投的排上去
            Dispatch();
        }

        void CheckBatchFinished()
        {
            if (mBatchClosed)
                return;
            if (mOrder.Count == 0)
                return;
            foreach (var i in mOrder)
            {
                if (i.IsFinal == false)
                    return;
            }
            mBatchClosed = true;
            var allDone = true;
            foreach (var i in mOrder)
            {
                if (i.State != ECookJobState.Done)
                    allDone = false;
            }
            Notice("cook batch finished");
            OnBatchFinished?.Invoke(allDone);
        }

        /// <summary>最近的调度通知 (面板显示用, 有界)</summary>
        public const int MaxNotices = 200;
        readonly List<string> mNotices = new List<string>();
        public IReadOnlyList<string> Notices { get { return mNotices; } }

        void Notice(string text)
        {
            mNotices.Add($"[{DateTime.Now:HH:mm:ss}] {text}");
            if (mNotices.Count > MaxNotices)
                mNotices.RemoveRange(0, mNotices.Count - MaxNotices);
            OnNotice?.Invoke(text);
        }

        void NotifyProgress()
        {
            var done = 0;
            var failed = 0;
            var cancelled = 0;
            foreach (var i in mOrder)
            {
                if (i.State == ECookJobState.Done)
                    done++;
                else if (i.State == ECookJobState.Failed)
                    failed++;
                else if (i.State == ECookJobState.Cancelled)
                    cancelled++;
            }
            // 取消也是了结: 不计入的话进度条到不了 100%, 浮窗也永远不会自动隐藏
            SettledCount = done + failed + cancelled;
            FailedCount = failed;
            CancelledCount = cancelled;
            OnProgressChanged?.Invoke(SettledCount, mOrder.Count, failed);
        }
        #endregion
    }
}
