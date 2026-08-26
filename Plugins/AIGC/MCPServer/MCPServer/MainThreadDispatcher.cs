using System;
using System.Threading;

namespace EngineNS.Plugins.MCPServer
{
    /// <summary>
    /// 把工具逻辑从 MCP 工作线程投递到引擎主线程执行, 并同步等待结果。
    ///
    /// 为什么必须有这一层: TtMCPPlugin.InvokeTool 是直接 method.Invoke(null, args), 没有任何
    /// 线程调度, 工具方法体跑在 ThreadPool.QueueUserWorkItem 起的工作线程上 (见 HandleRequest)。
    /// 纯只读地取几个状态字段勉强能用, 但一旦要改场景数据 / 写 GPU 资源 (地形编辑、抓帧开关),
    /// 就必须回到主线程, 否则是竞态, 轻则数据错乱重则崩在驱动里。
    ///
    /// 用 EventPoster.PostTickSyncEvent 而不是 EventPoster.Post: 后者返回的 FTaskAwaiter
    /// 只在 OnCompleted (也就是被 await) 时才真正 EnqueueAsync, 同步上下文里 Post 完轮询
    /// 永远等不到。PostTickSyncEvent 内部带 lock, 可从任意线程调用, 主线程在 TickSync 里
    /// 消费, 回调返回 true 表示做完可以移除。
    ///
    /// 注意工作线程阻塞等主线程是安全的 —— 两者是独立线程, 主线程不会反过来等 MCP。
    /// 但绝不能从主线程自己调 Invoke, 那必然超时 (会等到 timeout 而不是死锁, 因为有超时兜底)。
    /// </summary>
    internal static class TtMainThreadDispatcher
    {
        /// <summary>
        /// 默认超时。取得比较宽松: 引擎卡在资源加载 / 编译 shader 时一帧可能好几秒。
        /// </summary>
        public const int DefaultTimeoutMs = 15000;

        /// <summary>
        /// 阻塞当前线程, 直到 action 在主线程上执行完毕。
        /// </summary>
        /// <returns>true = 已执行完毕; false = 超时 (action 可能压根没跑, 也可能正跑着)</returns>
        public static bool Invoke(Action action, int timeoutMs = DefaultTimeoutMs)
        {
            if (action == null)
                return true;

            Exception error = null;
            // 故意不 using / 不 Dispose: 超时返回后回调仍可能被主线程执行并 Set(),
            // 提前 Dispose 会把 ObjectDisposedException 抛到主线程上, 那就把引擎带崩了。
            // 一个 ManualResetEventSlim 的代价交给 GC 完全可以接受。
            var done = new ManualResetEventSlim(false);

            TtEngine.Instance.EventPoster.PostTickSyncEvent((tickCount) =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    done.Set();
                }
                return true;
            });

            if (done.Wait(timeoutMs) == false)
                return false;

            if (error != null)
                throw new TtMainThreadInvokeException(error);
            return true;
        }

        /// <summary>
        /// 带返回值的版本。func 的返回值在主线程上算好后带回来。
        /// </summary>
        public static bool Invoke<T>(Func<T> func, out T result, int timeoutMs = DefaultTimeoutMs)
        {
            T local = default(T);
            var ok = Invoke(() => { local = func(); }, timeoutMs);
            result = ok ? local : default(T);
            return ok;
        }

        /// <summary>
        /// 让主线程连续跑 frames 帧后再返回。用来等跨帧才生效的操作 (典型是 RenderDoc 抓帧)。
        /// </summary>
        /// <returns>true = 等到了; false = 超时</returns>
        public static bool WaitFrames(int frames, int timeoutMs = DefaultTimeoutMs)
        {
            if (frames <= 0)
                return true;

            var done = new ManualResetEventSlim(false);
            int remaining = frames;
            TtEngine.Instance.EventPoster.PostTickSyncEvent((tickCount) =>
            {
                remaining--;
                if (remaining > 0)
                    return false;   // 返回 false = 留在队列里, 下一帧继续
                done.Set();
                return true;
            });
            return done.Wait(timeoutMs);
        }
    }

    /// <summary>
    /// 主线程回调里抛出的异常。包一层是为了在工具的 catch 里能和"投递失败/超时"区分开。
    /// </summary>
    internal class TtMainThreadInvokeException : Exception
    {
        public TtMainThreadInvokeException(Exception inner)
            : base($"exception on main thread: {inner.Message}", inner)
        {
        }
    }
}
