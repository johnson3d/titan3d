using System;
using System.Collections.Generic;
using System.Threading;

namespace EngineNS.IO
{
    /// <summary>
    /// cook 进度/产物接收端。引擎内默认实现只写日志, worker 侧换成协议实现后
    /// 同一份 CookAsset 实现就能同时服务"编辑器内 cook"与"子进程 cook + 进度上报"。
    /// 三个回调都可能来自任意线程 (bake 在 TPools / 续体在上下文线程), 实现方不得假设主线程。
    /// </summary>
    public interface ITtCookProgressSink
    {
        void OnStage(TtCookContext ctx, string stage);
        void OnProgress(TtCookContext ctx, float fraction);
        void OnArtifact(TtCookContext ctx, string artifactPath);
    }

    /// <summary>
    /// 单次 cook job 的执行上下文: 取消位 + 进度/产物上报通道。
    /// 由 <see cref="TtCookDriver"/> 创建并经两条路径送达实现方:
    /// 1) <c>IAssetMeta.CookAsset(asset, args)</c> 的 args (显式传入, 首选);
    /// 2) <see cref="Current"/> (进程级 ambient), 供 <c>TtNode.CookAsset()</c> 这类无参扩展点取用。
    /// </summary>
    public class TtCookContext
    {
        // 进程级 ambient 而非 AsyncLocal: 引擎 TtTask 的自定义 builder 直接把 stateMachine.MoveNext
        // 交给 awaiter (Task.cs 的 AwaitOnCompleted), 不做 ExecutionContext 捕获, 续体在完成方线程上跑,
        // AsyncLocal 会跨 await 静默丢失。形态与 TtPgcGraph.ExecutionContext 一致 (同进程同时只有一个在飞 job)。
        private static TtCookContext sAmbient;

        private int mCancelRequested;
        private readonly object mLock = new object();
        private readonly List<string> mArtifacts = new List<string>();
        private readonly System.Diagnostics.Stopwatch mStopwatch = new System.Diagnostics.Stopwatch();
        private float mLastFraction = -1.0f;

        /// <summary>job id, 由提交侧分配; 非协议场景 (命令行批量) 为 0</summary>
        public long JobId { get; }
        /// <summary>资产路径 (相对各自 root, 不带 .meta); 批量扫描产生的 job 可能为 null</summary>
        public string Asset { get; }
        /// <summary>资产类型名 (Scene/Texture/...)</summary>
        public string AssetType { get; }
        /// <summary>true = 强制重烘, 实现方应跳过"已是最新"的快速判定</summary>
        public bool Force { get; }
        public ITtCookProgressSink Sink { get; }

        /// <summary>本 job 已用时 (秒), 用于心跳与 elapsed 字段</summary>
        public double ElapsedSec => mStopwatch.Elapsed.TotalSeconds;
        /// <summary>最近一次上报的阶段文本</summary>
        public string LastStage { get; private set; }
        /// <summary>最近一次上报的子进度 (未上报过为 -1)</summary>
        public float LastFraction => mLastFraction;

        public TtCookContext(long jobId, string asset, string assetType, bool force, ITtCookProgressSink sink)
        {
            JobId = jobId;
            Asset = asset;
            AssetType = assetType;
            Force = force;
            Sink = sink ?? TtCookLogSink.Instance;
            mStopwatch.Start();
        }

        #region 取消 (协作式)
        /// <summary>
        /// 是否已被请求取消。cook 实现应在资产/level/循环边界轮询它, 然后**正常返回** false
        /// 并走已完成的收尾路径; 禁止用 task.CancelTask() (Cancelling 即 IsCompleted, 会让
        /// 池化 TtTaskData 提前回池后被后台写污染, 见 CodingGuidelines §1.6.3)。
        /// </summary>
        public bool IsCancellationRequested => Volatile.Read(ref mCancelRequested) != 0;

        /// <summary>置位取消请求 (幂等, 可从任意线程调用)</summary>
        public void RequestCancel()
        {
            Interlocked.Exchange(ref mCancelRequested, 1);
        }
        #endregion

        #region 上报
        /// <summary>上报阶段文本 (如 "create" / "cook" / "bake pending=3")</summary>
        public void ReportStage(string stage)
        {
            LastStage = stage;
            try
            {
                Sink.OnStage(this, stage);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error,
                    $"Cook context sink failed on stage '{stage}': {ex.Message}");
            }
        }

        /// <summary>上报子进度 0..1 (超出范围会被夹取; 与上次相同则忽略)</summary>
        public void ReportProgress(float fraction)
        {
            if (fraction < 0.0f)
                fraction = 0.0f;
            else if (fraction > 1.0f)
                fraction = 1.0f;
            if (Math.Abs(fraction - mLastFraction) < 0.001f)
                return;
            mLastFraction = fraction;
            try
            {
                Sink.OnProgress(this, fraction);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error,
                    $"Cook context sink failed on progress {fraction:F2}: {ex.Message}");
            }
        }

        /// <summary>
        /// 登记一个产物路径。绝对路径会被折算成相对仓库根 (分隔符统一为 '/'),
        /// 这样远端机器拿到的是可各自映射的路径 —— 绝对路径在多机场景下没有意义。
        /// </summary>
        public void ReportArtifact(string artifactPath)
        {
            if (string.IsNullOrEmpty(artifactPath))
                return;
            var saved = ToRepositoryRelative(artifactPath);
            lock (mLock)
            {
                if (mArtifacts.Contains(saved))
                    return;
                mArtifacts.Add(saved);
            }
            try
            {
                Sink.OnArtifact(this, saved);
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Error,
                    $"Cook context sink failed on artifact '{saved}': {ex.Message}");
            }
        }

        /// <summary>产物快照 (供 job.done 序列化)</summary>
        public List<string> GetArtifacts()
        {
            lock (mLock)
            {
                return new List<string>(mArtifacts);
            }
        }
        #endregion

        #region ambient
        /// <summary>当前在飞 cook job 的上下文; 不在 cook 期间为 null</summary>
        public static TtCookContext Current => Volatile.Read(ref sAmbient);

        /// <summary>
        /// 把 ctx 设为当前 ambient, dispose 时恢复上一层。无参扩展点 (如 TtNode.CookAsset)
        /// 用它取上下文, 不需要改签名。
        /// 前提: 同进程同时只有一个在飞 cook job —— 这是 CookManager 设计里"单 worker 内严格串行"
        /// 的决策, 编辑器进程不做 in-process cook。出现并发 job 时这里会报警而不静默互踩。
        /// </summary>
        public static IDisposable PushAmbient(TtCookContext ctx)
        {
            var prev = Volatile.Read(ref sAmbient);
            if (prev != null && !ReferenceEquals(prev, ctx))
            {
                Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Warning,
                    $"Cook ambient replaced by a second job (prev job={prev.JobId} '{prev.Asset}', new job={ctx?.JobId} '{ctx?.Asset}'); " +
                    "cook 不支持同进程并发多个 job, 无参扩展点 (TtNode.CookAsset) 的进度上报会串扰");
            }
            return new FAmbientScope(prev, ctx);
        }

        private struct FAmbientScope : IDisposable
        {
            private readonly TtCookContext mPrev;
            private bool mDisposed;

            public FAmbientScope(TtCookContext prev, TtCookContext ctx)
            {
                mPrev = prev;
                mDisposed = false;
                Volatile.Write(ref sAmbient, ctx);
            }

            public void Dispose()
            {
                if (mDisposed)
                    return;
                mDisposed = true;
                Volatile.Write(ref sAmbient, mPrev);
            }
        }
        #endregion

        /// <summary>
        /// 绝对路径 -> 相对仓库根路径; 拿不到 FileManager 或折算失败时原样返回
        /// (宁可多写一段绝对路径, 也不要因为折算失败丢产物记录)。
        /// </summary>
        private static string ToRepositoryRelative(string path)
        {
            var normalized = path.Replace('\\', '/');
            try
            {
                var engine = TtEngine.Instance;
                if (engine == null || System.IO.Path.IsPathRooted(path) == false)
                    return normalized;
                var cacheRoot = engine.FileManager.GetRoot(TtFileManager.ERootDir.Cache);
                if (string.IsNullOrEmpty(cacheRoot))
                    return normalized;
                var repoRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(cacheRoot, ".."));
                var relative = System.IO.Path.GetRelativePath(repoRoot, path);
                return relative.Replace('\\', '/');
            }
            catch (Exception)
            {
                return normalized;
            }
        }
    }

    /// <summary>
    /// 默认接收端: 只写引擎日志 (Cook 类别)。编辑器内 in-process cook 与命令行批量 cook 用它,
    /// 行为与没有上下文时一致。
    /// </summary>
    public class TtCookLogSink : ITtCookProgressSink
    {
        public static readonly TtCookLogSink Instance = new TtCookLogSink();

        public void OnStage(TtCookContext ctx, string stage)
        {
            Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info,
                $"Cook stage: {(string.IsNullOrEmpty(ctx.Asset) ? ctx.AssetType : ctx.Asset)} -> {stage}");
        }

        public void OnProgress(TtCookContext ctx, float fraction)
        {
            Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info,
                $"Cook progress: {(string.IsNullOrEmpty(ctx.Asset) ? ctx.AssetType : ctx.Asset)} {fraction:P0}");
        }

        public void OnArtifact(TtCookContext ctx, string artifactPath)
        {
            Profiler.Log.WriteLine<Profiler.TtCookGategory>(Profiler.ELogTag.Info,
                $"Cook artifact: {artifactPath}");
        }
    }

    /// <summary>
    /// 协议接收端: 把阶段/子进度写成 [TITAN-COOK] 行, 交给注入的行写出委托。
    /// 放在引擎侧是为了让 worker 工具层不必自己理解 cook 的进度语义;
    /// 行写出委托 (而不是直接 Console) 是为了单测能注入内存 sink, 也便于将来换 TCP 传输。
    /// </summary>
    public class TtCookProtocolSink : ITtCookProgressSink
    {
        private readonly Action<string> mWriteLine;

        /// <param name="writeLine">输出一行协议 (实现须自行保证跨线程串行写与 Flush)</param>
        public TtCookProtocolSink(Action<string> writeLine)
        {
            mWriteLine = writeLine ?? throw new ArgumentNullException(nameof(writeLine));
        }

        public void OnStage(TtCookContext ctx, string stage)
        {
            mWriteLine(TtCookProtocol.Encode(TtCookProtocol.JobStage(ctx.JobId, stage, ctx.ElapsedSec)));
        }

        public void OnProgress(TtCookContext ctx, float fraction)
        {
            mWriteLine(TtCookProtocol.Encode(TtCookProtocol.JobProgress(ctx.JobId, fraction)));
        }

        public void OnArtifact(TtCookContext ctx, string artifactPath)
        {
            // artifacts 随 job.done 一次性给出, 这里不单独发消息, 只保证被 ctx 记录过
        }
    }
}
