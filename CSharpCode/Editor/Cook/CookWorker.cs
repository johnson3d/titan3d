using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using EngineNS.IO;

namespace EngineNS.Editor.Cook
{
    /// <summary>
    /// Manager 依赖的 worker 通道抽象。存在只有一个理由: 让状态机能在没有真进程的条件下,
    /// 逐行喂入 recorded 的协议行做纯逻辑测试 (崩溃重投 / 双终态 / busy 回退这些分支靠真进程
    /// 几乎不可复现)。真进程实现就是下面的 TtCookWorker。
    /// </summary>
    public interface ITtCookWorkerChannel : IDisposable
    {
        bool IsAlive { get; }
        int ProcessId { get; }
        bool Send(TtCookMessage msg);
        int Pump();
        bool RequestQuit(string reason = "manual");
        bool WaitForExit(int timeoutMs);
        string[] GetLogSnapshot();
        void ClearLog();

        Action<TtCookMessage> OnProtocolMessage { get; set; }
        Action<string> OnLogLine { get; set; }
        Action<int> OnProcessExited { get; set; }
    }
    /// <summary>
    /// 编辑器侧的一个常驻 cook 子进程句柄: 起 TitanCMD 的 ExeCmd=CookServer, 往 stdin 写指令行,
    /// 从 stdout 读回 <c>[TITAN-COOK]</c> 协议行与非协议日志行。协议本身见 TtCookProtocol,
    /// 整体设计见 Documents\editor\CookManager.md。
    ///
    /// 线程约定 (与 worker 侧对称, 也是本项目踩过的坑): Process 的 OutputDataReceived/ErrorDataReceived/
    /// Exited 一律在 CLR 线程池线程回调, 那里没有 TtContextThread.CurrentContext 也没有 tick group,
    /// 直接在这些回调里动引擎对象等于把引擎代码搬到裸线程上。所以本类只做一件事——把行塞进
    /// ConcurrentQueue; 事件的派发全部推迟到主线程调 Pump() 时进行, 上层因此可以安全读写引擎状态。
    /// 同理, 本类不 await 任何 CLR task, 也不提供 async 接口。
    /// </summary>
    public sealed class TtCookWorker : ITtCookWorkerChannel
    {
        /// <summary>人读日志的保留行数上限, 防止长跑把内存吃光</summary>
        public const int MaxLogLines = 8000;
        /// <summary>优雅退出时等 worker 自己收摊的时间; 超过才 Kill</summary>
        public const int GracefulExitMs = 5000;

        readonly Process mProcess;
        readonly object mWriteLock = new object();
        readonly StreamWriter mStdin;

        readonly ConcurrentQueue<string> mLineInbox = new ConcurrentQueue<string>();
        readonly ConcurrentQueue<string> mProtocolInbox = new ConcurrentQueue<string>();
        volatile bool mProcessExited;
        /// <summary>stdout 异步读已到 EOF (最后一行已入队)</summary>
        volatile bool mStdoutEof;
        long mExitTick;
        /// <summary>进程已退但 stdout EOF 迟迟不来时, 最多等这么久就照样派发退出 (孙进程持有管道句柄会让 EOF 永不到来)</summary>
        public const int ExitDrainTimeoutMs = 2000;
        int mExitCode = -1;
        bool mQuitSent;
        bool mDisposed;
        int mPumpedLines;

        /// <summary>收到一条协议消息 (已在 Pump 的调用线程上)</summary>
        public Action<TtCookMessage> OnProtocolMessage { get; set; }
        /// <summary>收到一条非协议日志行 (已在 Pump 的调用线程上); 不需要就留 null</summary>
        public Action<string> OnLogLine { get; set; }
        /// <summary>子进程已退出 (已在 Pump 的调用线程上, 只发一次)</summary>
        public Action<int> OnProcessExited { get; set; }

        public TtCookWorker(string exePath, string workingDir, string arguments)
        {
            var info = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                WorkingDirectory = workingDir,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                // worker 的协议行与人读日志混在同一个 stdout 里按哨兵分流, 编码必须与 worker 写出端一致
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            mProcess = Process.Start(info);
            mStdin = mProcess.StandardInput;

            mProcess.OutputDataReceived += (s, e) =>
            {
                if (e.Data == null)
                    mStdoutEof = true;
                else
                    EnqueueLine(e.Data, mProtocolInbox, mLineInbox);
            };
            mProcess.ErrorDataReceived += (s, e) => EnqueueLine(e.Data, null, mLineInbox);
            mProcess.Exited += (s, e) => OnExited();
            mProcess.EnableRaisingEvents = true;
            mProcess.BeginOutputReadLine();
            mProcess.BeginErrorReadLine();
        }

        /// <summary>worker 进程的 pid; 日志与崩溃定位要用</summary>
        public int ProcessId
        {
            get
            {
                try { return mProcess.Id; }
                catch { return -1; }
            }
        }

        public bool IsAlive
        {
            get { return mDisposed == false && mProcessExited == false; }
        }

        /// <summary>子进程已退出时的退出码; 还活着或未取到为 -1</summary>
        public int ExitCode { get { return mExitCode; } }

        #region 收
        /// <summary>
        /// 线程池回调: 只分类入队。哨兵判定放这里是因为行一旦进错队列就再也分不出来了,
        /// 而"入队"本身不涉及任何引擎对象。
        /// </summary>
        static void EnqueueLine(string line, ConcurrentQueue<string> protocolQueue, ConcurrentQueue<string> logQueue)
        {
            if (string.IsNullOrEmpty(line))
                return;
            if (protocolQueue != null && line.StartsWith(TtCookProtocol.Sentinel))
            {
                protocolQueue.Enqueue(line);
                return;
            }
            logQueue?.Enqueue(line);
        }

        void OnExited()
        {
            try { mExitCode = mProcess.ExitCode; }
            catch { mExitCode = -1; }
            MarkExited();
        }

        void MarkExited()
        {
            if (System.Threading.Interlocked.Read(ref mExitTick) == 0)
                System.Threading.Interlocked.CompareExchange(ref mExitTick, Environment.TickCount64, 0);
            mProcessExited = true;
        }

        /// <summary>
        /// 在引擎线程上排空队列并派发事件。每帧调用一次即可; 返回处理过的协议行数,
        /// 便于上层判断这一帧是否有进展。
        /// </summary>
        public int Pump()
        {
            if (mDisposed)
                return 0;

            var count = 0;
            while (mProtocolInbox.TryDequeue(out var line))
            {
                count++;
                if (TtCookProtocol.TryDecode(line, out var msg) == false)
                {
                    // 协议行解码失败不能当成没收到: 交给日志侧留痕, 由上层按需要报警
                    mLineInbox.Enqueue("(worker) undecodable protocol line: " + line);
                    continue;
                }
                OnProtocolMessage?.Invoke(msg);
            }

            // 日志按帧限量: 一次全倒出来会让主循环长出一帧 (worker 的 stdout 混有引擎全部日志)
            var budget = MaxPumpLogLinesPerFrame;
            while (budget-- > 0 && mLineInbox.TryDequeue(out var logLine))
            {
                mPumpedLines++;
                lock (mLogLock)
                {
                    mLogLines.Add(logLine);
                    if (mLogLines.Count > MaxLogLines)
                        mLogLines.RemoveRange(0, mLogLines.Count - MaxLogLines);
                }
                OnLogLine?.Invoke(logLine);
            }

            // Process.Exited 可能早于 stdout 最后几行到达 (异步读另有线程): 必须等 EOF 且协议队列排空才派发退出,
            // 否则上层先把在飞 job 当崩溃重投, 紧接着又收到它的 job.done / quit 行, 重启归因也会判错
            if (mProcessExited && mExitNotified == false && mProtocolInbox.IsEmpty
                && (mStdoutEof || Environment.TickCount64 - System.Threading.Interlocked.Read(ref mExitTick) > ExitDrainTimeoutMs))
            {
                mExitNotified = true;
                OnProcessExited?.Invoke(mExitCode);
            }
            return count;
        }
        public const int MaxPumpLogLinesPerFrame = 256;

        readonly object mLogLock = new object();
        readonly List<string> mLogLines = new List<string>();
        volatile bool mExitNotified;

        /// <summary>取当前保留的人读日志快照 (供面板绘制)</summary>
        public string[] GetLogSnapshot()
        {
            lock (mLogLock)
            {
                return mLogLines.ToArray();
            }
        }

        /// <summary>清空日志缓冲 (重启 worker 后调用, 免得旧日志混着新批次看)</summary>
        public void ClearLog()
        {
            lock (mLogLock)
            {
                mLogLines.Clear();
            }
            mPumpedLines = 0;
        }
        #endregion

        #region 发
        /// <summary>写一条指令行到 worker 的 stdin; 失败视为断管, 返回 false 让上层走重启</summary>
        public bool Send(TtCookMessage msg)
        {
            if (msg == null || mDisposed || mProcessExited)
                return false;
            try
            {
                lock (mWriteLock)
                {
                    mStdin.WriteLine(TtCookProtocol.Encode(msg));
                    mStdin.Flush();
                }
                return true;
            }
            catch (Exception)
            {
                // 对端读端挂了 (进程正在退): 不能让写失败炸到编辑器主循环
                MarkExited();
                return false;
            }
        }
        #endregion

        #region 收尾
        /// <summary>
        /// 请求 worker 优雅收摊: 发 quit 让它在飞 job 跑完后自退。发了之后不阻塞等待,
        /// 由上层继续 Pump(); 真要硬杀用 Dispose()。
        /// </summary>
        public bool RequestQuit(string reason = "manual")
        {
            if (mQuitSent || IsAlive == false)
                return false;
            mQuitSent = true;
            return Send(TtCookProtocol.Quit(reason));
        }

        /// <summary>等子进程自己退出, 最多等 timeoutMs; 返回是否真的退干净</summary>
        public bool WaitForExit(int timeoutMs)
        {
            if (mProcessExited)
                return true;
            try
            {
                if (mProcess.WaitForExit(timeoutMs))
                {
                    OnExited();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }

        public void Dispose()
        {
            if (mDisposed)
                return;
            mDisposed = true;

            RequestQuit("shutdown");
            if (WaitForExit(GracefulExitMs) == false)
            {
                // 过了宽限期还不退 (job 卡死 / worker 自己挂了): 只有硬杀这一条路,
                // cook 产物是缓存侧派生数据, 被硬杀不会污染源文件
                try
                {
                    if (mProcess.HasExited == false)
                        mProcess.Kill();
                }
                catch (Exception)
                {
                }
                WaitForExit(1000);
            }
            OnExited();

            try { mStdin.Dispose(); } catch (Exception) { }
            try { mProcess.Dispose(); } catch (Exception) { }
        }
        #endregion
    }
}
