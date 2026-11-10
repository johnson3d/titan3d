using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EngineNS;
using EngineNS.IO;

namespace ProjectCooker.Command
{
    /// <summary>
    /// 常驻 cook worker: 引擎启动完不退出, 从 stdin 逐行收 <c>[TITAN-COOK]</c> 指令,
    /// 按 job 逐个调引擎内 <see cref="TtCookDriver"/> (与 ExeCmd=Cook 同一套执行体),
    /// 进度/结果经协议行写回 stdout。设计见 Documents\editor\CookManager.md。
    ///
    /// 用法: TitanCMD.exe CookCfg=... ExeCmd=CookServer
    ///        [WorkerIdleSec=300] [WorkerMaxJobs=200] [WorkerJobTimeout=1200] [WorkerBeatSec=10]
    /// stdin 每行一条指令 (其它行忽略并转 stderr), stdout 混有人读日志与协议行, 由提交侧按哨兵分流。
    ///
    /// 退出码: 0 = 优雅退出 (idle / restart / manual / pipe), 1 = 致命错误 (取消后 job 仍不收尾)。
    /// </summary>
    class TtCookServer : TtCookCommand
    {
        /// <summary>空闲多久后优雅退出 (队列空且无在飞 job 才计时), <=0 表示永不因空闲退出</summary>
        public const int DefaultIdleShutdownSec = 300;
        /// <summary>单进程处理多少个 job 后主动退出 (引擎长跑有内存/状态漂移风险, 由提交侧重新拉起)</summary>
        public const int DefaultMaxJobsPerWorker = 200;
        /// <summary>单 job 超时; 与 TtTerrainNode.CookTerrainAsync(bakeTimeoutMs: 1200000) 对齐</summary>
        public const int DefaultJobTimeoutSec = 1200;
        /// <summary>心跳间隔 (秒)</summary>
        public const int DefaultBeatSec = 10;
        /// <summary>待跑队列容量上限, 超出则对 add 回 busy (提交侧应重投)</summary>
        public const int MaxPendingJobs = 512;
        /// <summary>轮询间隔 (毫秒), 决定 IPC 响应延迟上界</summary>
        public const int PollIntervalMs = 100;
        /// <summary>超时取消后再给多少秒让 job 正常收尾; 仍不返回则判定 worker 状态不可信, 直接退出</summary>
        public const int CancelGraceSec = 60;

        private struct FJobDesc
        {
            public long Id;
            public string Asset;
            public string Type;
            public bool Force;
        }

        /// <summary>
        /// stdin 行收件箱。读 stdin 必须放独立线程: Console.In.ReadLine 是阻塞调用,
        /// 放主线程会卡死 TtEngine.Tick (地形 bake 的续体链路需要 Tick 推进), 放 EventPoster
        /// 的专线则会永久占掉 cook 自己要用的一条上下文线程 (CodingGuidelines §1.6 的"选错 target
        /// 会阻塞专线上排队的真正 IO 任务")。工具进程里一条纯 IO 后台线程是最小代价的做法。
        /// </summary>
        private readonly ConcurrentQueue<string> mInbox = new ConcurrentQueue<string>();
        private readonly Queue<FJobDesc> mPending = new Queue<FJobDesc>();
        private readonly object mWriteLock = new object();

        private System.Threading.Thread mReader;
        private volatile bool mReading;
        /// <summary>stdin 已 EOF (提交侧进程没了)</summary>
        private volatile bool mStdinClosed;
        /// <summary>本拍正在等的信号量; 用 Interlocked 取走, 保证一拍最多被 Release 一次</summary>
        private EngineNS.Thread.TtSemaphore mWakeSem;
        private System.Threading.Timer mPokeTimer;
        private TtCookContext mRunningCtx;
        private string mQuitReason;
        /// <summary>true = 队列里未开工的 job 直接作废; false = 把已接收的队列跑完再退</summary>
        private bool mAbortQueued;
        private bool mFlushPending;
        private bool mFatal;

        private int mIdleShutdownSec = DefaultIdleShutdownSec;
        private int mMaxJobs = DefaultMaxJobsPerWorker;
        private int mJobTimeoutSec = DefaultJobTimeoutSec;
        private double mBeatSec = DefaultBeatSec;

        private long mNextJobId = 1;
        private int mJobsThisWorker;
        /// <summary>在飞 job 已发的心跳序号 (每个 job 归零, 对端据此判断同一个 job 内是否有推进)</summary>
        private int mBeatSeq;
        private int mStatDone;
        private int mStatFailed;
        private int mStatSkipped;
        private readonly System.Diagnostics.Stopwatch mIdleWatch = new System.Diagnostics.Stopwatch();
        private readonly System.Diagnostics.Stopwatch mUptime = new System.Diagnostics.Stopwatch();
        private readonly System.Diagnostics.Stopwatch mBatchWatch = new System.Diagnostics.Stopwatch();
        // 心跳用独立计时器 (每次发出后重启): job 的 elapsed 是 job 相对时间, 空闲时是开机相对时间,
        // 拿任一者做"上次心跳时刻"都会在 job 结束后错位
        private readonly System.Diagnostics.Stopwatch mBeatWatch = new System.Diagnostics.Stopwatch();

        public override async EngineNS.Thread.Async.TtTask<int> ExecuteCommand(string[] args)
        {
            mIdleShutdownSec = FindIntArgument(args, Param_WorkerIdleSec, DefaultIdleShutdownSec);
            mMaxJobs = FindIntArgument(args, Param_WorkerMaxJobs, DefaultMaxJobsPerWorker);
            mJobTimeoutSec = FindIntArgument(args, Param_WorkerJobTimeout, DefaultJobTimeoutSec);
            mBeatSec = Math.Max(1, FindIntArgument(args, Param_WorkerBeatSec, DefaultBeatSec));

            var driver = TtCookDriver.Instance;
            // 命令行批量 cook 的黑名单在这里不参与: 黑名单是"入队侧"策略, worker 只收具体资产
            SendHello(driver, args);
            StartReader();
            // 没有指令时也要按拍醒一次 (心跳/超时判定靠这个节拍); 有指令进来时读线程会立刻再 Poke 一下
            mPokeTimer = new System.Threading.Timer(_ => Poke(), null, PollIntervalMs, PollIntervalMs);

            mUptime.Restart();
            mIdleWatch.Restart();
            mBeatWatch.Restart();
            WriteLine(TtCookProtocol.Ready(mPending.Count));

            while (true)
            {
                DrainInbox();

                // 断管且收件箱已排空才算"提交侧走了": 读线程与主循环有竞态, 一次性灌进来的
                // 指令 (含最后的 quit) 必须先处理完, 否则冒烟脚本会一条 job 都跑不上
                if (mQuitReason == null && mStdinClosed && mInbox.IsEmpty)
                {
                    mQuitReason = "pipe";
                    mAbortQueued = true;
                }
                if (mQuitReason != null && (mAbortQueued || (mInbox.IsEmpty && mPending.Count == 0)))
                    break;

                if (mPending.Count == 0)
                {
                    MaybeBeatIdle();
                    if (mIdleShutdownSec > 0 && mIdleWatch.Elapsed.TotalSeconds >= mIdleShutdownSec)
                    {
                        mQuitReason = "idle";
                        break;
                    }
                    await YieldAsync();
                    continue;
                }

                // 批次边界: 跑够 job 数就主动优雅退出, 由提交侧重新拉起一个干净进程
                // (队列里剩下的不跑了 —— 本意就是尽早换干净进程, 未开工的 job 由提交侧重新入队)
                if (mMaxJobs > 0 && mJobsThisWorker >= mMaxJobs)
                {
                    mQuitReason = "restart";
                    mAbortQueued = true;
                    break;
                }

                mIdleWatch.Stop();
                if (mBatchWatch.IsRunning == false)
                    mBatchWatch.Restart();
                var job = mPending.Dequeue();
                await RunJobAsync(driver, job);
                mJobsThisWorker++;

                // 一个 job 到终态后: 队列还有活就报 ready(可接活), 真空了才报 idle
                if (mPending.Count > 0)
                    WriteLine(TtCookProtocol.Ready(mPending.Count));
                else
                    WriteLine(TtCookProtocol.Idle(0));
                if (mFlushPending && mPending.Count == 0)
                    SendStats();
                if (mFatal)
                {
                    mQuitReason = "fatal";
                    mAbortQueued = true;
                    break;
                }
                mIdleWatch.Restart();
            }

            if (mFlushPending && mPending.Count == 0)
                SendStats();
            // worker 也用 quit 行把自己为何退出告诉对端 (idle/restart/manual/pipe/fatal)
            WriteLine(TtCookProtocol.Quit(mQuitReason));
            mPokeTimer?.Dispose();
            StopReader();
            // ExeCmd=CookServer 是 service command, 引擎主循环不会因为命令返回而退出 (见 Program.Run);
            // 而本命令的存活期就是这个 job 循环, 所以收尾必须由 worker 自己发起
            EngineNS.TtEngine.Instance.PostQuitMessage();
            return mFatal ? 1 : 0;
        }

        #region 握手与统计
        void SendHello(TtCookDriver driver, string[] args)
        {
            var types = driver.GetAssetTypeNames();
            var engineVer = typeof(TtEngine).Assembly.GetName().Version?.ToString() ?? "unknown";
            var root = TtEngine.Instance.FileManager.GetRoot(TtFileManager.ERootDir.Game);
            var hello = TtCookProtocol.Hello(TtCookProtocol.Version, engineVer, LoadCfgHash(args), root, types,
                System.Diagnostics.Process.GetCurrentProcess().Id);
            WriteLine(hello);
        }

        /// <summary>
        /// 配置 hash: 提交侧据此硬拒"不同版本/不同配置写同一份 cache"。
        /// 拿不到 CookCfg= 或未落盘时为 null, 表示本 worker 不声明配置指纹 (握手时按不匹配处理更安全,
        /// 但 M0 只做上报, 硬拒与分发基线归 M3)。
        /// </summary>
        static string LoadCfgHash(string[] args)
        {
            var cfgFile = FindArgument(args, Param_CookCfg);
            if (string.IsNullOrEmpty(cfgFile) || System.IO.File.Exists(cfgFile) == false)
                return null;
            try
            {
                var text = System.IO.File.ReadAllText(cfgFile);
                return Hash160.CreateHash160(System.Text.Encoding.UTF8.GetBytes(text)).ToString();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Cook worker: read config failed for hash: {ex.Message}");
                return null;
            }
        }

        void SendStats()
        {
            mFlushPending = false;
            WriteLine(TtCookProtocol.Stats(mStatDone, mStatFailed, mStatSkipped, mBatchWatch.Elapsed.TotalSeconds));
            mStatDone = mStatFailed = mStatSkipped = 0;
            mBatchWatch.Restart();
        }
        #endregion

        #region 收件箱
        void StartReader()
        {
            mReading = true;
            mReader = new System.Threading.Thread(ReadStdinLoop)
            {
                Name = "CookIpcIn",
                IsBackground = true,
            };
            mReader.Start();
        }

        void StopReader()
        {
            mReading = false;
            mReader = null;
        }

        void ReadStdinLoop()
        {
            try
            {
                while (mReading)
                {
                    var line = Console.In.ReadLine();
                    if (line == null)
                    {
                        // 只报"源断了", 具体何时收摊由主循环判 (它得先把已送达的指令处理完)
                        mStdinClosed = true;
                        return;
                    }
                    if (string.IsNullOrEmpty(line))
                        continue;
                    mInbox.Enqueue(line);
                    // 别让指令等到下一个节拍才被动发现
                    Poke();
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Cook worker: stdin reader stopped: {ex.Message}");
                mStdinClosed = true;
            }
        }

        void DrainInbox()
        {
            while (mInbox.TryDequeue(out var line))
            {
                if (TtCookProtocol.TryDecode(line, out var msg) == false)
                {
                    Console.Error.WriteLine($"Cook worker: ignore non-protocol stdin line: {line}");
                    continue;
                }
                switch (msg.Type)
                {
                    case TtCookMsgTypes.Add:
                        HandleAdd(msg);
                        break;
                    case TtCookMsgTypes.Cancel:
                        HandleCancel(msg);
                        break;
                    case TtCookMsgTypes.Flush:
                        mFlushPending = true;
                        if (mPending.Count == 0 && mRunningCtx == null)
                            SendStats();
                        break;
                    case TtCookMsgTypes.Quit:
                        // 优雅退出语义: 在飞 job 与已接收队列跑完再退 (本意是"不再接新活");
                        // 卡死由 JobTimeout 兜底
                        mQuitReason = string.IsNullOrEmpty(msg.Reason) ? "manual" : msg.Reason;
                        break;
                    default:
                        Console.Error.WriteLine($"Cook worker: unknown command '{msg.Type}', ignored");
                        break;
                }
            }
        }

        void HandleAdd(TtCookMessage msg)
        {
            if (string.IsNullOrEmpty(msg.Asset))
            {
                Console.Error.WriteLine($"Cook worker: add without asset, ignored (id={msg.JobId})");
                if (msg.JobId.HasValue)
                {
                    // 必须给终态行, 否则提交侧那个 job 会永远挂在 in-flight 上
                    WriteLine(TtCookProtocol.JobError(msg.JobId.Value, ECookErrorKind.Asset,
                        "add without asset", null, 0));
                    mStatSkipped++;
                }
                return;
            }
            if (mPending.Count >= MaxPendingJobs)
            {
                WriteLine(TtCookProtocol.Busy(msg.JobId ?? 0, mPending.Count));
                return;
            }
            // 同一资产已在队/在飞则合并, 不重复跑 (worker 侧也挡一道, 与提交侧去重互为兜底)
            if (mRunningCtx != null && string.Equals(mRunningCtx.Asset, msg.Asset, StringComparison.OrdinalIgnoreCase))
            {
                mStatSkipped++;
                return;
            }
            foreach (var i in mPending)
            {
                if (string.Equals(i.Asset, msg.Asset, StringComparison.OrdinalIgnoreCase))
                {
                    mStatSkipped++;
                    return;
                }
            }
            mPending.Enqueue(new FJobDesc
            {
                Id = msg.JobId ?? mNextJobId++,
                Asset = msg.Asset,
                Type = msg.AssetType,
                Force = msg.Force ?? false,
            });
            mIdleWatch.Restart();
        }

        void HandleCancel(TtCookMessage msg)
        {
            var id = msg.JobId ?? 0;
            if (mRunningCtx != null && mRunningCtx.JobId == id)
            {
                // 只置协作式取消位: 长任务调 CancelTask() 会让池化 TtTaskData 提前回池后被后台写污染
                // 终态行由 RunJobAsync 拿到 Cancelled 状态后统一发 job.error(Cancel)
                mRunningCtx.RequestCancel();
                return;
            }
            var kept = new List<FJobDesc>(mPending.Count);
            var matched = false;
            foreach (var i in mPending)
            {
                if (i.Id == id)
                {
                    mStatSkipped++;
                    matched = true;
                }
                else
                    kept.Add(i);
            }
            if (matched)
            {
                // 未开工就被取消: 不会走到 RunJobAsync, 这里直接给终态行替它关掉
                WriteLine(TtCookProtocol.JobError(id, ECookErrorKind.Cancel, "cancelled before start", null, 0));
                return;
            }
            if (msg.JobId.HasValue)
            {
                // 队列/在飞都没这个 job (已结或从未提交): 同样要给终态, 不然提交侧挂住
                WriteLine(TtCookProtocol.JobError(id, ECookErrorKind.Cancel, "job not found in worker", null, 0));
            }
            mPending.Clear();
            foreach (var i in kept)
                mPending.Enqueue(i);
        }
        #endregion

        #region job 执行
        async EngineNS.Thread.Async.TtTask RunJobAsync(TtCookDriver driver, FJobDesc job)
        {
            var ctx = new TtCookContext(job.Id, job.Asset, job.Type, job.Force, new TtCookProtocolSink(WriteLine));
            mRunningCtx = ctx;
            WriteLine(TtCookProtocol.JobBegin(job.Id, job.Asset, job.Type));

            var task = driver.RunJobAsync(job.Asset, ctx);
            mBeatSeq = 0;
            var timedOut = false;
            var deadlineSec = (double)mJobTimeoutSec;
            while (task.IsCompleted == false)
            {
                DrainInbox();
                if (ctx.IsCancellationRequested == false && deadlineSec > 0 && ctx.ElapsedSec >= deadlineSec)
                {
                    ctx.RequestCancel();
                    timedOut = true;
                    WriteLine(TtCookProtocol.JobError(job.Id, ECookErrorKind.Timeout,
                        $"job timeout after {ctx.ElapsedSec:F1}s (limit {mJobTimeoutSec}s)", null, ctx.ElapsedSec));
                    mStatFailed++;
                }
                if (timedOut && ctx.ElapsedSec >= deadlineSec + CancelGraceSec)
                {
                    // 取消后仍不收尾: 引擎态已不可信, 继续跑下一个 job 只会污染产物, 直接退出让提交侧重启
                    mFatal = true;
                    break;
                }
                MaybeBeatJob(ctx);
                await YieldAsync();
            }

            // 致命路径: job 已判定不收尾, 这里绝不能 await 它 —— 那等于卡死在一个我们刚刚
            // 认定不可信的 task 上, 后面的退出与 PostQuitMessage 都到不了
            if (mFatal)
            {
                mRunningCtx = null;
                return;
            }

            var result = await task;
            mRunningCtx = null;
            if (timedOut)
                return;

            switch (result.Status)
            {
                case ECookJobStatus.Success:
                    mStatDone++;
                    WriteLine(TtCookProtocol.JobDone(job.Id, ctx.ElapsedSec, ctx.GetArtifacts()));
                    break;
                case ECookJobStatus.Cancelled:
                    mStatSkipped++;
                    WriteLine(TtCookProtocol.JobError(job.Id, ECookErrorKind.Cancel,
                        result.Error ?? "cancelled", null, ctx.ElapsedSec));
                    break;
                default:
                    mStatFailed++;
                    WriteLine(TtCookProtocol.JobError(job.Id, ECookErrorKind.Asset,
                        result.Error ?? "cook failed", null, ctx.ElapsedSec));
                    break;
            }
        }
        #endregion

        #region 心跳与行写出
        void MaybeBeatJob(TtCookContext ctx)
        {
            if (IsBeatDue() == false)
                return;
            MarkBeat();
            mBeatSeq++;
            WriteLine(TtCookProtocol.Beat(ctx.JobId, mBeatSeq, ctx.ElapsedSec));
        }

        void MaybeBeatIdle()
        {
            if (IsBeatDue() == false)
                return;
            MarkBeat();
            WriteLine(TtCookProtocol.Beat(null, 0, mUptime.Elapsed.TotalSeconds));
        }

        bool IsBeatDue()
        {
            return mBeatWatch.Elapsed.TotalSeconds >= mBeatSec;
        }

        void MarkBeat()
        {
            mBeatWatch.Restart();
        }

        /// <summary>
        /// 从别的线程叫醒 worker 循环: 只 Release 信号量, 不碰任何引擎对象。
        /// 必须 Exchange 取走: TtSemaphore 的 ContinueEnqueued 判查与赋值不是原子的,
        /// 同一个信号量被 Release 两次会把协程续体入队两次, 等于引擎协程重入 MoveNext。
        /// </summary>
        void Poke()
        {
            var sem = System.Threading.Interlocked.Exchange(ref mWakeSem, null);
            if (sem != null)
                sem.Release();
        }

        /// <summary>
        /// 让出 worker 循环。只能等引擎自己的信号量, 不能等 CLR 的 Task.Delay ——
        /// AsyncFiberMethodBuilder.AwaitOnCompleted 就是 awaiter.OnCompleted(stateMachine.MoveNext),
        /// 所以 await 一个 CLR task 等于把本协程余下的部分搬到 CLR 线程池线程上去跑; 那里既没有
        /// TtContextThread.CurrentContext 也没有 tick group, job 内部的引擎 await 就再没人推进
        /// (实测表现: 同一条 driver 代码走 ExeCmd=Cook 能 cook 完, 走 worker 却卡在 create asset 之前,
        ///  而心跳照发 —— 因为循环本身确实还在转, 转的是错的线程)。
        /// AwaitSemaphore 把续体绑到 TtContextThread.CurrentContext, 恢复时回到引擎线程。
        /// TtSemaphore.ContinueEnqueued 全仓没有复位点, 一个实例只能 await 一次, 所以每拍新建。
        /// </summary>
        async EngineNS.Thread.Async.TtTask YieldAsync()
        {
            var sem = EngineNS.Thread.TtSemaphore.CreateSemaphore(1);
            System.Threading.Interlocked.Exchange(ref mWakeSem, sem);
            await sem.Await();
        }

        void WriteLine(TtCookMessage msg)
        {
            WriteLine(TtCookProtocol.Encode(msg));
        }

        void WriteLine(string line)
        {
            if (line == null)
                return;
            try
            {
                // Console.Out 本身是同步包装, 这里的锁是为了"一行 + Flush"整体原子,
                // 避免协议行被人读日志截断
                lock (mWriteLock)
                {
                    Console.Out.WriteLine(line);
                    Console.Out.Flush();
                }
            }
            catch (Exception ex)
            {
                // 提交侧读端挂了(管道被关闭)时写会抛: 这不是 job 失败, 但继续写也没人接,
                // 当成断管处理优雅收摊; 不能让它炸成未观测异常把退出码搅成不行
                mStdinClosed = true;
                mQuitReason = mQuitReason ?? "pipe";
                mAbortQueued = true;
                Console.Error.WriteLine($"Cook worker: stdout write failed, shutting down: {ex.Message}");
            }
        }
        #endregion
    }
}
