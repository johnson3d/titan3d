using EngineNS;
using EngineNS.Bricks.Network.RPC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tracer.TtCpuProfilerVisual;

namespace Tracer
{
    public class TtMyTracer : EngineNS.Profiler.Trace.TtTracer
    {
        static bool msVersionWarned = false;
        // 接收线程是唯一能证明“帧已经到哪里”的地方: 崩在原生层时这个计数就是最后的线索
        static int msRecvTraceCount = 0;
        static int msEventStreamRecv = 0;
        public override void ReciveFrame(EngineNS.IO.TtMemWriter frameData, EngineNS.Bricks.Network.RPC.TtCallContext context)
        {
            bool trace = System.Threading.Interlocked.Increment(ref msRecvTraceCount) <= 3;
            if (trace)
                Console.WriteLine("[Tracer] rx begin");
            using (var reader = EngineNS.IO.TtMemReader.CreateInstance(in frameData))
            {
                using (var ar = new EngineNS.IO.AuxReader<EngineNS.IO.TtMemReader>(reader, this))
                {
                    var frame = new EngineNS.Profiler.Trace.TtFrame();
                    try
                    {
                        frame.Read(ar);
                    }
                    catch (Exception e)
                    {
                        // 收到脏数据不能把接收线程打死, 整帧丢弃即可
                        Console.WriteLine($"[Tracer] drop broken frame: {e.Message}");
                        return;
                    }
                    if (trace)
                    {
                        int acts = 0;
                        foreach (var c in frame.Channels)
                            acts += c.Actions.Count;
                        Console.WriteLine($"[Tracer] rx read ok v={frame.RemoteVersion} fi={frame.FrameIndex} ch={frame.Channels.Count} acts={acts} broken={frame.Broken}");
                    }
                    if (frame.VersionMismatch)
                    {
                        // 版本不一致时后面的帧必然也都不对, 只提醒一次, 不要刷屏
                        if (msVersionWarned == false)
                        {
                            msVersionWarned = true;
                            Console.WriteLine($"[Tracer] protocol version mismatch: engine={frame.RemoteVersion}, tracer={EngineNS.Profiler.Trace.TtFrame.ProtocolVersion}, 后续帧将被丢弃");
                        }
                        return;
                    }
                    if (frame.Broken)
                    {
                        Console.WriteLine($"[Tracer] drop broken frame {frame.FrameIndex}");
                        return;
                    }

                    var app = TtTraceApplication.Instance;
                    if (app == null)
                    {
                        if (trace)
                            Console.WriteLine("[Tracer] rx app is null");
                        return;
                    }
                    app.OnFrame(frame);
                    if (trace)
                        Console.WriteLine("[Tracer] rx cached");

                    // 以前这里 break 只取第一个 action; 现在必须遍历全部:
                    // 同一帧里可能同时挂着快照(Cpu)/日志(Log)/事件流(Cpu) 多个 action
                    foreach (var ch in frame.Channels)
                    {
                        foreach (var a in ch.Actions)
                        {
                            if (a is EngineNS.Profiler.Trace.TtCpuFrameProfilerAction)
                            {
                                app.CpuProfilerVisual.FrameProfiler = a as EngineNS.Profiler.Trace.TtCpuFrameProfilerAction;
                            }
                            else if (a is EngineNS.Profiler.Trace.TtLogAction)
                            {
                                var act = a as EngineNS.Profiler.Trace.TtLogAction;
                                app.LogVisual.OnReportLog(act.Tag, act.Category, act.MemberName, act.SourceFilePath, act.SourceLineNumber, act.Info);
                            }
                            else if (a is EngineNS.Profiler.Trace.TtEventStreamAction)
                            {
                                // 事件流收到但 UI 不画泳道时, 这一行是唯一能区分“没收到”/“收到但空的”依据
                                var es = (EngineNS.Profiler.Trace.TtEventStreamAction)a;
                                int total = 0;
                                for (int i = 0; i < es.Streams.Count; i++)
                                    total += es.Streams[i].Count;
                                if (System.Threading.Interlocked.Increment(ref msEventStreamRecv) <= 5)
                                {
                                    Console.WriteLine($"[Tracer] event stream fi={es.FrameIndex} ch={es.Channel} streams={es.Streams.Count} events={total} names={es.NewNames.Count}");
                                    // 泳道明细: 泳道数/事件数对不上时, 要能立刻看出是哪条泳道被整段作废了
                                    for (int i = 0; i < es.Streams.Count; i++)
                                    {
                                        var sd = es.Streams[i];
                                        int valid = 0;
                                        for (int k = 0; k < sd.Count; k++)
                                        {
                                            if (sd.Events[k].NameId >= 0 && sd.Events[k].DurUS > 0)
                                                valid++;
                                        }
                                        Console.WriteLine($"[Tracer]   lane[{i}] name={sd.Name} ch={sd.Channel} count={sd.Count} visible={valid}");
                                    }
                                }
                            }
                            // TtEventStreamAction 不用分发: 时间线按帧号从 app 的帧缓存里自己取
                        }
                    }
                    if (trace)
                        Console.WriteLine("[Tracer] rx done");
                }
            }
        }
    }
    [TtRpcClass(RunTarget = ERunTarget.None, Executer = EExecuter.Root, CallerInClass = true)]
    public partial class TtTraceRpcManager : EngineNS.Bricks.Network.RPC.TtRpcManager
    {
        #region IRpcHost
        static TtRpcClass smRpcClass = null;
        public override TtRpcClass GetRpcClass()
        {
            if (smRpcClass==null)
                smRpcClass = new TtRpcClass(typeof(TtTraceRpcManager));
            return smRpcClass;
        }
        public override IRpcHost GetExecuter(in FRouter router)
        {
            if (router.Executer == EExecuter.Tracer)
            {
                return Tracer;
            }
            return base.GetExecuter(router);
        }
        #endregion

        public TtMyTracer Tracer { get; set; } = new TtMyTracer();
        public TtTraceRpcManager()
        {
            this.CurrentTarget = ERunTarget.Tracer;
        }
    }
    public partial class TtTraceApplication : TtSlateApplication, ITickable
    {
        #region ITickable
        public int GetTickOrder()
        {
            return 0;
        }
        #region Tick
        public void TickLogic(float ellapse)
        {
            //WorldViewportSlate.TickLogic(ellapse);
            TcpServer?.Tick();
        }
        public void TickRender(float ellapse)
        {
            //WorldViewportSlate.TickRender(ellapse);
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            //WorldViewportSlate.TickSync(ellapse);

            //OnDrawSlate();
        }
        #endregion
        #endregion

        public static TtTraceApplication Instance
        {
            get
            {
                return TtEngine.Instance.GfxDevice.SlateApplication as TtTraceApplication;
            }
        }

        public EngineNS.Bricks.TcpServer.TtTcpServer TcpServer { get; private set; } = new EngineNS.Bricks.TcpServer.TtTcpServer();
        public TtTracerEditor TracerEditor = null;
        public TtCpuProfilerVisual CpuProfilerVisual = null;
        public TtLogVisual LogVisual = null;
        public TtTimelineVisual TimelineVisual = null;

        #region Frame cache
        // 接收线程写、UI 线程读, 必须加锁。旧代码直接读 TtEngine.Instance.Tracer.Frames
        // (那是发送端侧的实例, 在 Tracer 进程里恒为空) 而且没有任何同步
        readonly object mFrameLocker = new object();
        readonly List<EngineNS.Profiler.Trace.TtFrame> mFrames = new List<EngineNS.Profiler.Trace.TtFrame>();
        public int MaxFrames = 1024;

        public void OnFrame(EngineNS.Profiler.Trace.TtFrame frame)
        {
            lock (mFrameLocker)
            {
                mFrames.Add(frame);
                while (mFrames.Count > MaxFrames)
                {
                    // capture 只抓 300 帧就自停, 之后仍有源源不断的快照帧进来。
                    // 若按入库顺序淘汰, 几秒内本次采集的成果就会被挤干净, UI 上永远只剩 no-trace。
                    // 所以淘汰时优先丢“没有事件流”的帧
                    int drop = -1;
                    for (int i = 0; i < mFrames.Count; i++)
                    {
                        if (HasEventStream(mFrames[i]) == false)
                        {
                            drop = i;
                            break;
                        }
                    }
                    if (drop < 0)
                        drop = 0;
                    mFrames.RemoveAt(drop);
                }
            }
        }
        public static bool HasEventStream(EngineNS.Profiler.Trace.TtFrame frame)
        {
            if (frame == null)
                return false;
            for (int i = 0; i < frame.Channels.Count; i++)
            {
                var acts = frame.Channels[i].Actions;
                for (int k = 0; k < acts.Count; k++)
                {
                    if (acts[k] is EngineNS.Profiler.Trace.TtEventStreamAction)
                        return true;
                }
            }
            return false;
        }
        public int FrameCount
        {
            get { lock (mFrameLocker) { return mFrames.Count; } }
        }
        public EngineNS.Profiler.Trace.TtFrame GetFrame(int index)
        {
            lock (mFrameLocker)
            {
                if (index < 0 || index >= mFrames.Count)
                    return null;
                return mFrames[index];
            }
        }
        #endregion

        #region Recording
        // 录制在引擎进程里完成(边跑边写 .ttrace, 对齐 UE 的 trace writer), 这里只负责打开文件回放
        public string LastRecordFile = null;
        public string LastRecordError = null;

        /// <summary>加载 .ttrace 并替换当前帧缓存, 返回帧数; 失败返回 -1</summary>
        public int LoadRecording(string path)
        {
            if (EngineNS.Profiler.Trace.TtTraceFile.Load(path, out var frames, out var err) == false)
            {
                LastRecordError = err;
                return -1;
            }
            LastRecordError = err;   // 部分损坏时这里带着提示, 但已读到的帧依旧可用
            if (frames.Count == 0)
            {
                if (string.IsNullOrEmpty(LastRecordError))
                    LastRecordError = "文件里没有帧";
                return -1;
            }
            lock (mFrameLocker)
            {
                mFrames.Clear();
                mFrames.AddRange(frames);
                // 录制文件可能远大于实时缓存窗口: 上限至少装得下这个文件,
                // 否则引擎继续推实时帧时, 几秒内就把加载的内容淘汰光了
                if (MaxFrames < frames.Count)
                    MaxFrames = frames.Count;
            }
            LastRecordFile = path;
            // 选中第一个带事件流的帧, 让 Timeline 立刻有内容(与 JumpToTrace 同口径)
            EngineNS.Profiler.Trace.TtFrame pick = null;
            for (int i = frames.Count - 1; i >= 0; i--)
            {
                if (HasEventStream(frames[i]))
                {
                    pick = frames[i];
                    break;
                }
            }
            if (pick == null)
                pick = frames[frames.Count - 1];
            TimelineVisual?.SelectFrame(pick);
            return frames.Count;
        }
        #endregion
        public override async EngineNS.Thread.Async.TtTask<bool> InitializeApplication(EngineNS.NxRHI.TtGpuDevice rc, RName rpName)
        {
            await base.InitializeApplication(rc, rpName);
            var traceCfg = TtEngine.Instance.ConfigManager.GetConfig<EngineNS.Profiler.Trace.TtTraceConfig>();
            TcpServer.StartServer("0.0.0.0", traceCfg.Port);
            // 接收端也做环形上限: 默认 300 + 64, 与发送端的 capture 帧数对齐
            MaxFrames = traceCfg.CaptureFrames + 64;

            TracerEditor = new TtTracerEditor();
            await TracerEditor.Initialize();
            TracerEditor.Visible = true;

            CpuProfilerVisual = new TtCpuProfilerVisual();
            await CpuProfilerVisual.Initialize();
            CpuProfilerVisual.Visible = true;

            LogVisual = new TtLogVisual();
            await LogVisual.Initialize();
            LogVisual.Visible = true;

            TimelineVisual = new TtTimelineVisual();
            await TimelineVisual.Initialize();
            TimelineVisual.Visible = true;

            TtEngine.Instance.TickableManager.AddTickable(this);
            Console.WriteLine("Tracer Server start success.");
            return true;
        }
        public override void Cleanup()
        {
            TcpServer.StopServer();
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            EngineNS.CoreSDK.DisposeObject(ref CpuProfilerVisual);
            EngineNS.CoreSDK.DisposeObject(ref LogVisual);
            EngineNS.CoreSDK.DisposeObject(ref TimelineVisual);
            EngineNS.CoreSDK.DisposeObject(ref TracerEditor);

            base.Cleanup();
        }
    }
}
