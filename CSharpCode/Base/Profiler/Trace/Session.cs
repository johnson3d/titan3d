using System;
using System.Collections.Generic;
using System.Threading;

namespace EngineNS.Profiler.Trace
{
    /// <summary>
    /// 事件流采集会话。
    ///
    /// 结构要点(不能简化成单一 ring buffer):
    ///  - 缓冲是 (帧槽 x 流) 二维: 槽按 frameIndex % SlotCount 定位, 流按线程/GPU 队列分。
    ///  - CPU 事件在 Begin 时立即 reserve 下标(位置此刻确定), End 时回填时长。
    ///  - GPU 事件同样在 BeginRecord 时 reserve, fence 结算回来时只回填时长。
    ///    它挂的是"录制这一刻的活跃槽"而不是按帧号查找: GpuFrameContainerManager.FrameIndex
    ///    与引擎的 CurrentTickFrame 不是同一个计数器; 槽被强制封帧复用后的迟到结算靠世代号丢弃。
    ///  - 槽在 PendingGpuScopes > 0 时不可复用; capture 模式槽数 = 帧数 + 在途余量,
    ///    所以正常情况下这条降级路径不会被触发。
    /// </summary>
    public sealed class TtTraceSession
    {
        static readonly TtTraceSession msInstance = new TtTraceSession();
        public static TtTraceSession Instance => msInstance;

        // 热路径短路: TimeScope.Begin/End 每帧几千到几万次, 这里必须是字段读取而不是属性+null 判断
        static volatile bool msCapturing = false;
        public static bool IsCapturing => msCapturing;

        /// <summary>GPU 结算最多等这么多帧, 超时则作废未回填事件并强制封帧</summary>
        const int DefaultGpuWaitMaxFrames = 8;
        const int MaxGpuInFlight = 4;
        const int DefaultMaxEventsPerStream = 20000;
        const int DefaultMaxDepth = 32;

        struct TtTraceScopeFrame
        {
            public TimeScope Scope;
            public TtTraceFrameSlot Slot;
            /// <summary>记录那一刻槽的世代号, End 时对不上就说明槽已被新一轮 capture 复用</summary>
            public int Generation;
            public TtTraceStream Stream;
            public int EventIndex;
        }

        readonly object mLocker = new object();
        TtTraceFrameSlot[] mSlots = Array.Empty<TtTraceFrameSlot>();
        int mSlotCount;
        int mRemainFrames;
        int mFrameCount;
        long mBaseUS;
        volatile TtTraceFrameSlot mActiveSlot;

        // GPU 时钟→capture 时间轴的平移量。查询时间戳是 GPU 时钟域, 与 CPU 的 HighPrecision_GetTickCount
        // 不同源, 不能直接混用。每次结算采样 cpuNow - gpuEnd, 读取总是晚于真实完成, 所以误差恒为正,
        // 取历史最小值就能逼近真实偏移(不需要逐帧重标, 也不会因轮询间隔抖动而跳)
        long mGpuClockOffsetUS;
        bool mGpuClockOffsetValid;

        public int GpuWaitMaxFrames = DefaultGpuWaitMaxFrames;
        public int MaxEventsPerStream = DefaultMaxEventsPerStream;
        public int MaxDepth = DefaultMaxDepth;

        // 联调诊断: 旁路到底进来了多少次, 以及封帧时从槽里真正取出了多少条事件。
        // 前者为 0 说明 TimeScope 那边没接上, 后者为 0 说明入了槽但泳道是空的
        static int msBeginEventCount = 0;
        int mSealedEventCount = 0;

        public int DroppedSlots { get; private set; }
        public int IncompleteFrames { get; private set; }
        public int SentFrames { get; private set; }
        public int CapturedFrames { get; private set; }
        public int RemainFrames => mRemainFrames;
        /// <summary>capture 起点的绝对微秒。发送端用它把事件的 StartUS 换算成帧内相对时间</summary>
        public long BaseUS => mBaseUS;

        [ThreadStatic] static List<TtTraceScopeFrame> msStack;
        [ThreadStatic] static TtTraceFrameSlot msStreamSlot;
        [ThreadStatic] static TtTraceStream msStream;
        [ThreadStatic] static string msThreadName;

        #region Capture control
        public bool StartCapture(int frames)
        {
            if (frames <= 0)
                return false;
            if (msCapturing)
                return false;

            lock (mLocker)
            {
                // 槽要同时装得下 capture 帧数和 GPU 在途量, 否则超时降级路径会被当成常态触发
                mSlotCount = frames + Math.Max(MaxGpuInFlight, GpuWaitMaxFrames + 1);
                if (mSlots.Length != mSlotCount)
                {
                    var slots = new TtTraceFrameSlot[mSlotCount];
                    for (int i = 0; i < mSlotCount; i++)
                        slots[i] = i < mSlots.Length ? mSlots[i] : new TtTraceFrameSlot();
                    mSlots = slots;
                }
                for (int i = 0; i < mSlots.Length; i++)
                {
                    mSlots[i].State = ETraceSlotState.Free;
                    mSlots[i].FrameIndex = -1;
                    mSlots[i].Frame = null;
                }
                mRemainFrames = frames;
                mFrameCount = 0;
                mBaseUS = Support.TtTime.HighPrecision_GetTickCount();
                DroppedSlots = 0;
                IncompleteFrames = 0;
                mSealedEventCount = 0;
                SentFrames = 0;
                CapturedFrames = 0;
                mActiveSlot = null;
                mGpuClockOffsetUS = 0;
                mGpuClockOffsetValid = false;
            }
            msCapturing = true;
            return true;
        }

        public void StopCapture()
        {
            if (!msCapturing)
                return;
            lock (mLocker)
            {
                for (int i = 0; i < mSlots.Length; i++)
                {
                    var slot = mSlots[i];
                    if (slot.State == ETraceSlotState.Recording || slot.State == ETraceSlotState.CpuDone)
                        SealSlot(slot, true);
                }
                mRemainFrames = 0;
                mActiveSlot = null;
                msCapturing = false;
            }
            // [ThreadStatic] 只能由本线程清; 仍在栈里的 scope 会在 EndEvent 里找不到槽而直接丢弃
            var stack = msStack;
            if (stack != null)
                stack.Clear();
            msStreamSlot = null;
            msStream = null;
            ReportSummary();
        }

        long NowUS()
        {
            return Support.TtTime.HighPrecision_GetTickCount() - mBaseUS;
        }
        #endregion

        #region Frame lifecycle
        /// <summary>帧起点: 由 TtTracer.BeginFrame 调用(主线程)</summary>
        public void BeginFrame(int frameIndex)
        {
            if (!msCapturing)
                return;

            lock (mLocker)
            {
                if (mRemainFrames <= 0)
                {
                    // 帧数抓够了, 后面的帧不占槽; 这里必须清 mActiveSlot,
                    // 否则 BindFrame 会把新帧挂到上一帧的槽上
                    mActiveSlot = null;
                    return;
                }

                // 帧号是引擎的 tick 计数, 理论上不会为负; 取模写成对负数也正确的形式
                int idx = frameIndex % mSlotCount;
                if (idx < 0)
                    idx += mSlotCount;
                var slot = mSlots[idx];
                if (slot.FrameIndex != frameIndex)
                {
                    if (slot.State != ETraceSlotState.Sent && slot.State != ETraceSlotState.Free)
                    {
                        // 上一轮同槽位还没封帧。正常不会走到(capture 模式下槽数够), 只做计数
                        DroppedSlots++;
                        mActiveSlot = null;
                        return;
                    }
                    slot.Reset(frameIndex, NowUS());
                    mFrameCount++;
                    CapturedFrames = mFrameCount;
                }
                mRemainFrames--;
                mActiveSlot = slot;
            }
        }

        /// <summary>把本帧的事件槽绑定到要发送的传输帧上</summary>
        public void BindFrame(TtFrame frame)
        {
            if (frame == null)
                return;
            var slot = mActiveSlot;
            if (slot == null)
                return;
            lock (mLocker)
            {
                slot.Frame = frame;
                // 反向引用: Tick 封帧后据此把事件流 action 挂到该帧上
                frame.TraceSlot = slot;
            }
        }

        /// <summary>帧末: CPU 侧采集结束, 等 GPU 结算后由 Tick 封帧</summary>
        public void EndFrame()
        {
            if (!msCapturing)
                return;

            lock (mLocker)
            {
                var slot = mActiveSlot;
                mActiveSlot = null;
                if (slot == null || slot.State != ETraceSlotState.Recording)
                    return;
                slot.State = ETraceSlotState.CpuDone;
                if (slot.PendingGpuScopes <= 0)
                    SealSlot(slot, false);
            }
        }

        /// <summary>每帧调用: 扫描超时的槽、按帧号顺序封帧、抓满后收尾</summary>
        public void Tick(int curFrameIndex)
        {
            if (!msCapturing)
                return;

            bool finished = false;
            lock (mLocker)
            {
                for (int i = 0; i < mSlots.Length; i++)
                {
                    var slot = mSlots[i];
                    if (slot.State == ETraceSlotState.Recording)
                    {
                        // 帧中途没走 EndFrame(异常/跳帧), 超过两帧就当它已经结束
                        if (curFrameIndex - slot.FrameIndex > 1)
                        {
                            slot.State = ETraceSlotState.CpuDone;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (slot.State != ETraceSlotState.CpuDone)
                        continue;

                    if (slot.PendingGpuScopes > 0)
                    {
                        if (curFrameIndex - slot.FrameIndex <= GpuWaitMaxFrames)
                            continue;
                        // GPU 迟迟不结算: 未回填的事件作废, 照常发出, 但记一笔
                        slot.IncompleteEvents += slot.PendingGpuScopes;
                        slot.PendingGpuScopes = 0;
                        IncompleteFrames++;
                    }
                    SealSlot(slot, false);
                }

                if (mRemainFrames <= 0 && AllSlotsConsumed())
                {
                    mActiveSlot = null;
                    msCapturing = false;
                    finished = true;
                }
            }
            // 锁外报: OnReportLog 的回调可能反过来碰采集状态
            if (finished)
                ReportSummary();
        }

        /// <summary>抓取结束的汇总。DroppedSlots / IncompleteFrames 在 P0 应该恒为 0, 不为 0 就是有 bug</summary>
        void ReportSummary()
        {
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                $"Trace capture finished: frames={CapturedFrames} sent={SentFrames} droppedSlots={DroppedSlots} incompleteFrames={IncompleteFrames} beginEvents={msBeginEventCount} sealedEvents={mSealedEventCount}");
        }

        bool AllSlotsConsumed()
        {
            for (int i = 0; i < mSlots.Length; i++)
            {
                var st = mSlots[i].State;
                if (st != ETraceSlotState.Sent && st != ETraceSlotState.Free)
                    return false;
            }
            return true;
        }

        // 调用方必须持有 mLocker
        void SealSlot(TtTraceFrameSlot slot, bool force)
        {
            if (slot.PendingGpuScopes > 0)
            {
                if (!force)
                    return;
                slot.IncompleteEvents += slot.PendingGpuScopes;
                slot.PendingGpuScopes = 0;
            }

            // 槽里的流可能还有别的线程在往里写, 拿快照遍历
            var streams = slot.SnapshotStreams();
            for (int i = 0; i < streams.Length; i++)
            {
                var stream = streams[i];
                // 没 End 就跳帧的事件: 作废并计数, 分析侧会跳过
                mSealedEventCount += stream.Count;
                int unFinished = stream.InvalidateUnfinished();
                if (unFinished > 0)
                    slot.IncompleteEvents += unFinished;
                if (stream.Channel == ETraceChannel.Gpu)
                    stream.FinalizeGpuStartTimes(slot.StartUS, mGpuClockOffsetUS);
            }

            slot.State = ETraceSlotState.Sent;
            var frame = slot.Frame;
            if (frame != null)
                frame.Ready = true;
            SentFrames++;
        }
        #endregion

        #region Gpu event
        /// <summary>
        /// 一条在途 GPU 采样的句柄。GPU 时长要等 fence 结算才知道,
        /// 所以 Begin 只占位(Reserve), 结算回来再回填 DurUS。
        /// </summary>
        public struct TtTraceGpuHandle
        {
            public TtTraceFrameSlot Slot;
            public TtTraceStream Stream;
            public int EventIndex;
            public int Generation;
            public bool Valid { get { return Stream != null; } }
        }

        /// <summary>GPU 泳道名: 目前所有 pass 都提交到默认队列, 所以只有一条 GPU 泳道</summary>
        public const string GpuLaneName = "GPU Queue";

        /// <summary>
        /// GPU pass 开始录制时调。事件挂在"当前这一帧"的槽上, 并让该槽等结算后才封帧;
        /// 槽被强制封帧后复用的话, 迟到回来的结算按世代号丢弃。
        /// </summary>
        public TtTraceGpuHandle BeginGpuEvent(string eventName)
        {
            var result = default(TtTraceGpuHandle);
            if (!msCapturing)
                return result;
            var slot = mActiveSlot;
            if (slot == null)
                return result;
            lock (mLocker)
            {
                // 帧已经 EndFrame(CpuDone) 或已封帧: 挂上去也不会被发出去
                if (slot.State != ETraceSlotState.Recording)
                    return result;
                var stream = slot.GetStream(GpuLaneName, ETraceChannel.Gpu);
                if (stream.Count >= MaxEventsPerStream)
                    return result;
                int index = stream.Reserve(TtTraceNameTable.GetId(eventName), -1, 0);
                // 计数先加: 否则 Tick 可能在本帧封帧前把槽封掉, 这条事件白录
                slot.PendingGpuScopes++;
                result.Slot = slot;
                result.Stream = stream;
                result.EventIndex = index;
                result.Generation = slot.Generation;
            }
            return result;
        }

        /// <summary>GPU 结算回来: 回填时长与时间轴位置, 并放行所属帧</summary>
        /// <param name="durUS">已换算成微秒的时长, 后端没给时间戳时只用它</param>
        /// <param name="beginTicks">query 的起始原始值(与 freq 同单位), 0 表示没采到</param>
        /// <param name="endTicks">query 的结束原始值, endTicks &lt;= beginTicks 也当没采到</param>
        /// <param name="freq">提交队列的时间戳频率</param>
        public void EndGpuEvent(in TtTraceGpuHandle handle, int durUS, ulong beginTicks, ulong endTicks, ulong freq)
        {
            if (handle.Stream == null)
                return;
            lock (mLocker)
            {
                // 世代号对不上: 槽已被强制封帧并复用, 计数器已归零重来, 再减会把新帧的计数减错
                if (handle.Slot.Generation != handle.Generation)
                    return;

                if (freq > 0 && endTicks > beginTicks)
                {
                    long beginUS = TicksToUS(beginTicks, freq);
                    long endUS = TicksToUS(endTicks, freq);
                    long dur = endUS - beginUS;
                    if (dur > int.MaxValue)
                        dur = int.MaxValue;
                    handle.Stream.FillGpu(handle.EventIndex, beginUS, (int)dur);

                    // 采样: 读取时刻一定晚于 GPU 真实完成, 所以 cpuNow - gpuEnd 是真实偏移的上界,
                    // 取历史最小值逐步逼近。同一帧内的多次采样也会一起把偏移收紧
                    long sample = NowUS() - endUS;
                    if (mGpuClockOffsetValid == false || sample < mGpuClockOffsetUS)
                    {
                        mGpuClockOffsetUS = sample;
                        mGpuClockOffsetValid = true;
                    }
                }
                else
                {
                    // 后端没给时间戳(或驱动回读失败): 保持旧的 delta 路径
                    handle.Stream.Fill(handle.EventIndex, durUS);
                }

                if (handle.Slot.PendingGpuScopes > 0)
                    handle.Slot.PendingGpuScopes--;
            }
        }

        /// <summary>query 的原始 tick 换算成微秒。tick 可能很大, 用 double 避开 ticks*1000000 溢出</summary>
        static long TicksToUS(ulong ticks, ulong freq)
        {
            if (freq == 0)
                return 0;
            return (long)((double)ticks * 1000000.0 / (double)freq);
        }

        /// <summary>采样被丢弃(不参与统计): 作废事件并放行所属帧</summary>
        public void CancelGpuEvent(in TtTraceGpuHandle handle)
        {
            if (handle.Stream == null)
                return;
            lock (mLocker)
            {
                if (handle.Slot.Generation != handle.Generation)
                    return;
                handle.Stream.Invalidate(handle.EventIndex);
                if (handle.Slot.PendingGpuScopes > 0)
                    handle.Slot.PendingGpuScopes--;
            }
        }
        #endregion

        #region Event recording
        static string GetThreadName()
        {
            var n = msThreadName;
            if (n != null)
                return n;
            // EngineNS.Thread 是工程自己的命名空间, 会遮住 System.Threading.Thread
            var t = System.Threading.Thread.CurrentThread;
            n = t.Name;
            if (string.IsNullOrEmpty(n))
                n = "T" + t.ManagedThreadId;
            msThreadName = n;
            return n;
        }

        /// <summary>TimeScope.Begin 的旁路入口。热路径, 不要在这里做任何分配以外的事</summary>
        public void BeginEvent(TimeScope scope)
        {
            System.Threading.Interlocked.Increment(ref msBeginEventCount);

            var stack = msStack;
            if (stack == null)
            {
                stack = new List<TtTraceScopeFrame>(64);
                msStack = stack;
            }
            // 极端情况下(End 不配对导致栈顶不弹)给个硬上限, 避免栈无限增长
            if (stack.Count > MaxDepth * 4)
                stack.Clear();

            var slot = mActiveSlot;
            if (slot == null || slot.State != ETraceSlotState.Recording || stack.Count >= MaxDepth)
            {
                // 不在采集区间也入栈, 保证 End 时栈深度与 Begin 深度严格一致
                stack.Add(new TtTraceScopeFrame { Scope = scope, EventIndex = -1 });
                return;
            }

            int slotGen = slot.Generation;
            if (msStreamSlot != slot)
            {
                msStreamSlot = slot;
                msStream = slot.GetStream(GetThreadName(), ETraceChannel.Cpu);
                // 槽换了说明上一帧(或上一次 capture)有没 End 的外层 scope, 栈已经无意义
                if (stack.Count > 0 && stack[stack.Count - 1].Slot != null && stack[stack.Count - 1].Slot != slot)
                    stack.Clear();
            }
            var stream = msStream;
            if (stream == null || stream.Count >= MaxEventsPerStream)
            {
                stack.Add(new TtTraceScopeFrame { Scope = scope, Slot = slot, Generation = slotGen, EventIndex = -1 });
                return;
            }

            int parent = -1;
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                if (stack[i].EventIndex >= 0 && stack[i].Slot == slot && stack[i].Generation == slotGen)
                {
                    parent = stack[i].EventIndex;
                    break;
                }
            }

            // 名字表永不回收, 所以可以缓存在 TimeScope 上, 稳态下每次 Begin 不查字典
            int nameId = scope.TraceNameId;
            if (nameId < 0)
            {
                nameId = TtTraceNameTable.GetId(scope.GetFriendName());
                scope.TraceNameId = nameId;
            }
            int index = stream.Reserve(nameId, parent, NowUS());
            stack.Add(new TtTraceScopeFrame { Scope = scope, Slot = slot, Generation = slotGen, Stream = stream, EventIndex = index });
        }

        /// <summary>TimeScope.End 的旁路入口</summary>
        public void EndEvent(TimeScope scope)
        {
            var stack = msStack;
            if (stack == null || stack.Count == 0)
                return;

            int last = stack.Count - 1;
            var top = stack[last];
            if (ReferenceEquals(top.Scope, scope) == false)
            {
                // begin 发生在 capture 之外(或中途开启), 这里没有对应的栈帧, 不能动栈
                return;
            }
            stack.RemoveAt(last);

            if (top.EventIndex < 0 || top.Stream == null)
                return;

            var slot = top.Slot;
            if (slot == null || slot.State != ETraceSlotState.Recording)
            {
                top.Stream.Invalidate(top.EventIndex);
                return;
            }
            // 槽被复用过: 同一个下标现在装的是新那轮 capture 的事件, 不能改写
            if (slot.Generation != top.Generation)
                return;
            long dur = NowUS() - top.Stream.Events[top.EventIndex].StartUS;
            top.Stream.Fill(top.EventIndex, (int)dur);
        }
        #endregion
    }
}
