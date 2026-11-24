using EngineNS.Bricks.CodeBuilder.MacrossNode;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Profiler.Trace
{
    [IO.TtConfig(Path = "tracer.jscfg")]
    public class TtTraceConfig : IO.IConfig
    {
        [Rtti.Meta("")]
        public string Ip { get; set; } = "127.0.0.1";
        [Rtti.Meta("")]
        public ushort Port { get; set; } = 23333;
        /// <summary>一次 Capture 抓多少帧。事件流不能常开, 靠显式抓取控制开销</summary>
        [Rtti.Meta("")]
        public int CaptureFrames { get; set; } = 300;
        /// <summary>GPU 结算最多等多少帧, 超时作废未回填事件并强制封帧</summary>
        [Rtti.Meta("")]
        public int GpuWaitMaxFrames { get; set; } = 8;
        /// <summary>单条泳道单帧事件上限, 超出部分不记录(防止异常循环撑爆内存)</summary>
        [Rtti.Meta("")]
        public int MaxEventsPerStream { get; set; } = 20000;
        /// <summary>发送队列(待发帧)上限, 超出丢最老的</summary>
        [Rtti.Meta("")]
        public int MaxPendingFrames { get; set; } = 1024;
    }
    [Flags]
    public enum ETraceChannel : uint
    {
        None = 0,
        Cpu = 1,
        Gpu = (1<<1),
        Rdg = (1<<2),
        Log = (1<<3),
    }
    [Bricks.Network.RPC.TtRpcClass(RunTarget = Bricks.Network.RPC.ERunTarget.Tracer, Executer = Bricks.Network.RPC.EExecuter.Tracer, CallerInClass = true)]
    public partial class TtTracer : Bricks.Network.RPC.AuxRpcHost<TtTracer>, IDisposable
    {
        public override Bricks.Network.INetConnect GetRpcConnect(UInt16 methodIndex)
        {
            return NetConnect;
        }
        ~TtTracer()
        {
            Dispose();
        }
        public void Dispose()
        {
            // 先收尾录制(回填帧数并 flush), 否则进程退出时文件头的帧数还是 0
            StopRecord();
            CloseTrace();
        }

        public async EngineNS.Thread.Async.TtTask<bool> OpenTrace(string ip=null, ushort port = ushort.MaxValue)
        {
            if (NetConnect!=null)
                NetConnect.Disconnect();
            else
                NetConnect = new Bricks.Network.TtTcpClient();
            if (ip==null)
            {
                ip = TtEngine.Instance.ConfigManager.GetConfig<TtTraceConfig>().Ip;
            }
            if (port== ushort.MaxValue)
            {
                port = TtEngine.Instance.ConfigManager.GetConfig<TtTraceConfig>().Port;
            }
            var ret = await NetConnect.Connect(ip, port, null);
            if (ret)
            {
                Enabled = true;
                // 对端是全新进程, 名字表得从头补一遍
                LastSentNameCount = 0;
                Profiler.Log.OnReportLog += Profiler.Log.OnReportLog_Tracer;
            }
            else 
            {
                Profiler.Log.OnReportLog -= Profiler.Log.OnReportLog_Tracer;
            }
            return ret;
        }
        public void CloseTrace()
        {
            Enabled = false;
            Profiler.Log.OnReportLog -= Profiler.Log.OnReportLog_Tracer;
            if (NetConnect!=null)
            {
                NetConnect.Disconnect();
                NetConnect = null;
            }
            // 录制与网络连接无关: 断线时不能停文件, 否则录制会被静默截断。
            // 队列里已 Ready 的帧先冲进文件, 再丢弃队列
            NetSendFrames();
            // 接收端没了, 队列里的帧已经没有意义; 名字表计数也得复位,
            // 否则下次连上时对端永远收不到前一段名字
            LastSentNameCount = 0;
            lock (this)
            {
                Frames.Clear();
            }
        }

        public static long GetCurrentMicroSecond()
        {
            return Support.TtTime.HighPrecision_GetTickCount();
        }
        public static void PushAction(TtAction action)
        {
            if (TtEngine.Instance==null)
                return;
            if (TtEngine.Instance.Tracer.Current==null)
            {
                return;
            }
            TtEngine.Instance.Tracer.Current.PushAction(TtEngine.Instance.Tracer, action);
        }
        public bool Enabled { get; set; } = false;
        /// <summary>开启的通道; 不带对应通道的事件会被 PushAction 静默丢掉</summary>
        public ETraceChannel Channels { get; set; } = ETraceChannel.Cpu | ETraceChannel.Gpu | ETraceChannel.Log;
        public List<TtFrame> Frames = new List<TtFrame>();
        public TtFrame Current;
        /// <summary>名字表里已经发给对端的数量, 每帧只发新增那一段</summary>
        public int LastSentNameCount = 0;
        public int DroppedFrames { get; private set; }
        public int MaxPendingFrames = 1024;

        /// <summary>
        /// 开一次事件流采集。事件量远大于快照通道, 所以必须显式开、抓满自停。
        /// </summary>
        public bool StartCapture(int frames = 0)
        {
            if (Enabled == false && IsRecording == false)
            {
                // 既没接收端也不在录文件时, 帧根本不会生成(BeginFrame 直接返回 null), 采了也没人消费
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Warning, "Tracer not connected, trace event capture skipped");
                return false;
            }
            var cfg = TtEngine.Instance?.ConfigManager?.GetConfig<TtTraceConfig>();
            if (cfg != null)
            {
                TtTraceSession.Instance.GpuWaitMaxFrames = cfg.GpuWaitMaxFrames;
                TtTraceSession.Instance.MaxEventsPerStream = cfg.MaxEventsPerStream;
                MaxPendingFrames = cfg.MaxPendingFrames;
                if (frames <= 0)
                    frames = cfg.CaptureFrames;
            }
            if (frames <= 0)
                frames = 300;
            return TtTraceSession.Instance.StartCapture(frames);
        }
        public void StopCapture()
        {
            TtTraceSession.Instance.StopCapture();
        }

        public TtFrame BeginFrame()
        {
            // 录制时即使没连 Tracer 也要出帧: 对齐 UE 的 -trace=file, 落盘不依赖接收端在线
            if (Enabled == false && IsRecording == false)
            {
                return null;
            }

            lock (this)
            {
                Current = new TtFrame();
                Current.StartTimeMicroSecond = GetCurrentMicroSecond();
                Current.FrameIndex = (int)(TtEngine.Instance?.CurrentTickFrame ?? 0);
                // 事件 StartUS 是相对 capture 起点, 接收端用这个基准把它换算成帧内时间
                Current.TraceBaseUS = TtTraceSession.Instance.BaseUS;
                Current.InitChannels(Channels);
                Frames.Add(Current);
                TrimFrames();
                // capture 未开启时下面两个调用是空操作
                TtTraceSession.Instance.BeginFrame(Current.FrameIndex);
                TtTraceSession.Instance.BindFrame(Current);
                return Current;
            }
        }
        public void EndFrame(TtFrame frame)
        {
            if (frame==null)
            {
                return;
            }
            TtTracer.PushAction(new TtCpuFrameProfilerAction());
            frame.EndTimeMicroSecond = GetCurrentMicroSecond();
            // CPU 侧采集到此结束; 有槽的帧由 Session 等 GPU 结算后再置 Ready
            TtTraceSession.Instance.EndFrame();

            // 没绑槽的帧(未开 capture / capture 在帧中途开启 / 帧数已抓满)必须立刻放行。
            // 若让它挂在队首等一个永远不会来的封帧, 整条 tracer 通道(含原来的快照和日志)都会停发
            if (frame.TraceSlot == null)
                frame.Ready = true;

            // 先挂事件流再发: capture 刚好在本帧收尾时, 本帧可能刚被封帧,
            // 直接发出去就把最后一帧的事件流丢了
            AttachEventStreams();
            NetSendFrames();

            lock (this)
            {
                if (ReferenceEquals(Current, frame))
                    Current = null;
            }
        }

        /// <summary>每帧由 TtProfilerModule 驱动: 推进封帧、补挂事件流、发送队首已就绪的帧</summary>
        public void Tick()
        {
            // capture 的帧数预算本来是给手动抓取用的; 录制期间用完了自动续一轮,
            // 否则录到一半事件流就断了, 剩下的帧只剩帧耗时没有明细
            if (IsRecording && TtTraceSession.IsCapturing == false)
                StartCapture();
            TtTraceSession.Instance.Tick((int)(TtEngine.Instance?.CurrentTickFrame ?? 0));
            AttachEventStreams();
            NetSendFrames();
        }

        /// <summary>把已封帧槽里的事件流生成 action 挂到对应的传输帧上(每帧只挂一次)</summary>
        void AttachEventStreams()
        {
            // 注意不能以 IsCapturing 作闸: capture 结束后最后几帧刚封帧, 事件流还没挂
            if (Frames.Count == 0)
                return;
            lock (this)
            {
                for (int i = 0; i < Frames.Count; i++)
                {
                    var frame = Frames[i];
                    if (frame.EventAttached)
                        continue;
                    var slot = frame.TraceSlot;
                    if (slot == null)
                        continue;
                    if (ReferenceEquals(slot.Frame, frame) == false)
                    {
                        // 槽已被新一轮 capture 复用或已被丢弃, 这帧再也等不到封帧。
                        // 强制放行, 否则队首会永久堵住后面所有帧
                        frame.TraceSlot = null;
                        frame.EventAttached = true;
                        frame.Ready = true;
                        continue;
                    }
                    if (frame.Ready == false)
                        continue;
                    // 已经封帧, 把槽引用摘掉: 后续槽复用与本次挂流无关了
                    frame.EventAttached = true;
                    var actions = new List<TtEventStreamAction>();
                    TtEventStreamAction.CreateFromSlot(slot, actions);
                    for (int k = 0; k < actions.Count; k++)
                        frame.PushAction(this, actions[k]);
                    frame.TraceSlot = null;
                }
            }
        }

        void TrimFrames()
        {
            if (MaxPendingFrames <= 0)
                return;
            while (Frames.Count > MaxPendingFrames)
            {
                Frames.RemoveAt(0);
                DroppedFrames++;
            }
        }
        public void PushFrame(TtFrame frame)
        {
            //if (Enabled == false)
            //{
            //    return;
            //}
            lock (this)
            {
                Frames.Add(frame);
                TrimFrames();
            }
        }
        public Bricks.Network.TtTcpClient NetConnect = null;
        /// <summary>引擎侧流式录制(对齐 UE 的 trace writer): 帧 Ready 一帧就写一帧, 不等采集结束</summary>
        public TtTraceFileWriter FileWriter = null;
        public bool IsRecording
        {
            get { return FileWriter != null && FileWriter.IsOpen; }
        }
        /// <summary>当前录制文件; 没在录制为 null</summary>
        public string RecordFile
        {
            get { var w = FileWriter; return w != null ? w.Path : null; }
        }
        /// <summary>当前录制已落盘的帧数</summary>
        public int RecordedFrames
        {
            get { var w = FileWriter; return w != null ? w.FrameCount : 0; }
        }
        /// <summary>最近一次录制的文件路径, 停止录制后仍保留给日志/UI 用</summary>
        public string LastRecordFile { get; private set; }
        /// <summary>采集是不是录制自己开的: 是的话结束录制时一并收掉, 不干扰用户手动开的采集</summary>
        bool mRecordOwnsCapture = false;

        /// <summary>开始录到文件; path 传 null 则写到 cache/tracer/trace_时间戳.ttrace</summary>
        public bool StartRecord(string path, out string err)
        {
            StopRecord();
            var writer = new TtTraceFileWriter();
            if (writer.Start(string.IsNullOrEmpty(path) ? TtTraceFile.MakePath() : path, out err) == false)
                return false;
            FileWriter = writer;
            LastRecordFile = writer.Path;
            // 对齐 UE 的 Trace.Start: 录制自带采集, 不用先手动 CaptureTraceEvents
            if (TtTraceSession.IsCapturing == false)
            {
                mRecordOwnsCapture = true;
                StartCapture();
            }
            return true;
        }

        /// <summary>结束录制并返回文件路径; 没在录制返回 null</summary>
        public string StopRecord()
        {
            var writer = FileWriter;
            if (writer == null)
                return null;
            // 队列里可能还有已 Ready 但尚未落盘的帧, 先冲一遍再关文件
            NetSendFrames();
            FileWriter = null;
            writer.Stop();
            LastRecordFile = writer.Path;
            // 录制期间自动开的采集在录制结束后收掉: 否则没人消费的事件还在往帧槽里写
            if (mRecordOwnsCapture)
            {
                mRecordOwnsCapture = false;
                StopCapture();
            }
            return writer.Path;
        }

        /// <summary>
        /// 一条队列、两个消费者(网络 / 文件)。两边各自维护名字增量游标, 但
        /// AttachNameDelta 会覆写帧上的 NewNames, 所以必须先写网络再写文件。
        /// </summary>
        public void NetSendFrames()
        {
            while (true)
            {
                bool needNet = NetConnect != null && NetConnect.Connected;
                var fileWriter = FileWriter;
                bool needFile = fileWriter != null;
                if (needNet == false && needFile == false)
                    return;

                TtFrame frame = null;
                lock (this)
                {
                    if (Frames.Count == 0)
                        return;
                    frame = Frames[0];
                    // 保持帧序: 队首未封帧就不发后面的, 否则接收端帧序会乱
                    if (frame.Ready == false)
                        return;
                    Frames.RemoveAt(0);
                }

                if (needNet)
                {
                    using (var memWriter = IO.TtMemWriter.CreateInstance())
                    {
                        using (var writer = new IO.AuxWriter<IO.TtMemWriter>(memWriter))
                        {
                            // 名字增量必须按发送顺序切, 所以放在真正写之前
                            TtEventStreamAction.AttachNameDelta(frame, ref LastSentNameCount);
                            frame.Write(writer);
                            RPC_ReciveFrame(memWriter, null);
                        }
                    }
                }

                if (needFile)
                {
                    // WriteFrame 内部会用自己的名字游标再算一次增量
                    fileWriter.WriteFrame(frame);
                }
            }
        }
        #region RPC
        [Bricks.Network.RPC.TtRpcMethod(Index = 0)]
        public virtual void ReciveFrame(IO.TtMemWriter frameData, Bricks.Network.RPC.TtCallContext context)
        {
            //using (var reader = IO.TtMemReader.CreateInstance(in frameData))
            //{
            //    using (var ar = new IO.AuxReader<IO.TtMemReader>(reader, this))
            //    {
            //        var frame = new TtFrame();
            //        //frame.Read
            //        //TtEngine.Instance.Tracer.PushFrame(frame);
            //        OnReciveFrame(frame);
            //        //TtEngine.Instance.GfxDevice.SlateApplication as 
            //    }
            //}
            System.Diagnostics.Debug.Assert(false);
        }
        #endregion
    }
    public class TtFrame
    {
        /// <summary>协议版本: 加字段就加一, 接收端不匹配则丢弃并断开, 避免静默错位读</summary>
        public const int ProtocolVersion = 2;
        /// <summary>录制侧的帧号(与 Session 的帧槽一一对应)</summary>
        public int FrameIndex;
        /// <summary>capture 起点的绝对微秒; 事件 StartUS 减去 (StartTimeMicroSecond - TraceBaseUS) 即帧内时间</summary>
        public long TraceBaseUS;
        /// <summary>是否已封帧(可发送)。不序列化: 接收端收到的帧一定是完整的。
        /// 采集线程与主线程都会写它, 发送方会读, 所以 volatile</summary>
        public volatile bool Ready = false;
        /// <summary>事件流 action 是否已挂上。不序列化</summary>
        public bool EventAttached = false;
        /// <summary>对应的帧槽, 由 Session.BindFrame 填。不序列化</summary>
        public TtTraceFrameSlot TraceSlot;
        /// <summary>协议版本不匹配</summary>
        public bool VersionMismatch = false;
        /// <summary>不匹配时对端声明的版本号, 用于日志</summary>
        public int RemoteVersion = -1;
        /// <summary>读到未知 action id 等导致流错位: 这一帧剩下的字段都不可信, 必须整帧丢弃</summary>
        public bool Broken = false;

        public long StartTimeMicroSecond;
        public long EndTimeMicroSecond;
        public int DurationMicroSecond
        {
            get
            {
                return (int)(EndTimeMicroSecond - StartTimeMicroSecond);
            }
        }
        public int DurationMillisecond
        {
            get
            {
                return (int)((EndTimeMicroSecond - StartTimeMicroSecond)/1000);
            }
        }
        public List<TtChannel> Channels = new List<TtChannel>();
        /// <summary>
        /// 串行化 Actions 的「加」与「写」。日志回调可能从任意线程 PushAction 到正在发送的帧上,
        /// 不以它为锁的话 List 的枚举会中途发现集合被改而抛 InvalidOperationException
        /// </summary>
        public readonly object ActionLocker = new object();
        public void InitChannels(ETraceChannel channel)
        {
            for (int i = 0; i<32; i++)
            {
                var flag = (ETraceChannel)(1 << i);
                if((channel & flag) != 0)
                {
                    var ttChannel = new TtChannel();
                    ttChannel.Channel = flag;
                    Channels.Add(ttChannel);
                }
            }
        }
        public TtChannel GetChannel(ETraceChannel channel)
        {
            foreach (var ch in Channels)
            {
                if (ch.Channel == channel)
                    return ch;
            }
            return null;
        }
        public void Write(IO.IWriter writer)
        {
            writer.Write(ProtocolVersion);
            writer.Write(FrameIndex);
            writer.Write(TraceBaseUS);
            writer.Write(StartTimeMicroSecond);
            writer.Write(Channels.Count);
            lock (ActionLocker)
            {
                foreach (var channel in Channels)
                {
                    writer.Write(channel.Channel);
                    channel.Write(writer);
                }
            }
            writer.Write(EndTimeMicroSecond);
        }
        public void Read(IO.IReader reader)
        {
            int version;
            reader.Read(out version);
            RemoteVersion = version;
            if (version != ProtocolVersion)
            {
                VersionMismatch = true;
                return;
            }
            reader.Read(out FrameIndex);
            reader.Read(out TraceBaseUS);
            reader.Read(out StartTimeMicroSecond);
            int count;
            reader.Read(out count);
            // channel 最多 32 个(ETraceChannel 是 uint 标志位), 超了就是数据已经坏了
            if (count < 0 || count > 32)
            {
                Broken = true;
                return;
            }
            for (int i = 0; i<count; i++)
            {
                ETraceChannel cn;
                reader.Read(out cn);
                var ttChannel = new TtChannel();
                ttChannel.Channel = cn;

                ttChannel.Read(reader);
                if (ttChannel.Broken)
                {
                    Broken = true;
                    return;
                }
                Channels.Add(ttChannel);
            }
            reader.Read(out EndTimeMicroSecond);
            Ready = true;
        }

        public bool PushAction(TtTracer tracer, TtAction action)
        {
            if ((tracer.Channels & action.GetChannel()) == 0)
            {
                return false;
            }
            lock (ActionLocker)
            {
                foreach (var ch in Channels)
                {
                    if (ch.Channel == action.GetChannel())
                    {
                        ch.Actions.Add(action);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Profiler.Trace.TtTracer Tracer { get; } = new Profiler.Trace.TtTracer();
    }
}
