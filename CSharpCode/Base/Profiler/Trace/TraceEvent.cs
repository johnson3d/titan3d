using System;
using System.Collections.Generic;

namespace EngineNS.Profiler.Trace
{
    /// <summary>
    /// 槽状态机: Free -> Recording -> CpuDone -> Sent。
    /// 中间没有单独的 Ready: "可以发了"这件事等价于 Sent, 少一个状态就少一处不一致。
    /// </summary>
    public enum ETraceSlotState : byte
    {
        Free = 0,
        Recording,
        CpuDone,
        Sent,
    }

    /// <summary>
    /// 一次 Begin/End 的原始记录。只含值类型, 可直接 memcpy 序列化(16 字节)。
    /// 与 SampResult 的聚合口径不同: 这里每次调用一条, 同名重入也各自成条。
    /// </summary>
    public struct TtTraceEvent
    {
        /// <summary>TtTraceNameTable 的下标, -1 表示已作废(丢弃/超时)</summary>
        public int NameId;
        /// <summary>同一条 Stream 内的父事件下标, -1 表示根。父一定先于子写入, 所以恒小于自身下标</summary>
        public int ParentId;
        /// <summary>相对 capture 起点的微秒</summary>
        public long StartUS;
        /// <summary>时长微秒, -1 表示未结束或已丢弃</summary>
        public int DurUS;
    }

    /// <summary>
    /// 一条泳道: 一个线程或一个 GPU 队列在一帧内的全部事件。
    /// 只 append, 下标永不移动, 所以 ParentId 一旦写入就永久有效。
    /// </summary>
    public sealed class TtTraceStream
    {
        const int DefaultCapacity = 256;
        // 单帧爆量过一次之后不该长期占着那块内存
        const int ShrinkThreshold = 4096;

        public string Name = "";
        public ETraceChannel Channel;
        /// <summary>
        /// GPU 流里的事件是否带真实时间戳(query 的 begin/end)。
        /// 为 true 时 StartUS 存的是 GPU 时钟里的绝对微秒, 封帧时再加一次 clock offset 归到 capture 时间轴;
        /// 为 false 则回到"按提交顺序首尾相接"的旧降级模式
        /// </summary>
        public bool GpuRealTime;
        public TtTraceEvent[] Events = new TtTraceEvent[DefaultCapacity];
        public int Count;
        /// <summary>只有 Reserve(可能扩容) 与封帧时的整段拷取会碰它, 其余字段读写在所属线程内单线程完成</summary>
        public readonly object Locker = new object();

        public void Reset()
        {
            lock (Locker)
            {
                Count = 0;
                GpuRealTime = false;
                if (Events.Length > ShrinkThreshold)
                    Events = new TtTraceEvent[DefaultCapacity];
            }
        }

        public int Reserve(int nameId, int parentId, long startUS)
        {
            lock (Locker)
            {
                if (Count >= Events.Length)
                {
                    var cap = Events.Length * 2;
                    if (cap < 256)
                        cap = 256;
                    Array.Resize(ref Events, cap);
                }
                var index = Count++;
                ref var ev = ref Events[index];
                ev.NameId = nameId;
                ev.ParentId = parentId;
                ev.StartUS = startUS;
                ev.DurUS = -1;
                return index;
            }
        }

        /// <summary>把当前内容整段拷出来。跟 Reserve 抢同一个锁, 避免扩容时拷到半截</summary>
        public TtTraceEvent[] CopyEvents()
        {
            lock (Locker)
            {
                if (Count == 0)
                    return Array.Empty<TtTraceEvent>();
                var data = new TtTraceEvent[Count];
                Array.Copy(Events, data, Count);
                return data;
            }
        }

        /// <summary>所属线程调用</summary>
        public void Fill(int index, int durUS)
        {
            if (index < 0 || index >= Count)
                return;
            Events[index].DurUS = durUS;
        }

        /// <summary>
        /// GPU 结算回来: 同时回填时间戳与时长。startUS 是 GPU 时钟里的值(未加 offset),
        /// 真正的 capture 时间轴在封帧时由 FinalizeGpuStartTimes 一次性平移
        /// </summary>
        public void FillGpu(int index, long startUS, int durUS)
        {
            if (index < 0 || index >= Count)
                return;
            ref var ev = ref Events[index];
            ev.StartUS = startUS;
            ev.DurUS = durUS;
            GpuRealTime = true;
        }

        /// <summary>所属线程调用</summary>
        public void Invalidate(int index)
        {
            if (index < 0 || index >= Count)
                return;
            Events[index].NameId = -1;
            Events[index].DurUS = -1;
        }

        /// <summary>
        /// 把未结束(NameId>=0 且 DurUS&lt;0)的事件作废, 返回条数。
        /// 封帧时由别的线程调用, 所以整段处理都在 Locker 内完成
        /// </summary>
        public int InvalidateUnfinished()
        {
            lock (Locker)
            {
                int n = 0;
                for (int i = 0; i < Count; i++)
                {
                    ref var ev = ref Events[i];
                    if (ev.NameId >= 0 && ev.DurUS < 0)
                    {
                        ev.NameId = -1;
                        ev.DurUS = -1;
                        n++;
                    }
                }
                return n;
            }
        }

        /// <summary>
        /// 封帧时把 GPU 事件放到时间轴上。
        ///
        /// 有真实时间戳: StartUS 存的是 GPU 时钟的绝对微秒, 加上 clockOffsetUS(CPU 锚点 - GPU 终点)
        /// 就归到 capture 时间轴, pass 间的真实间隔/并行关系都能看见。
        /// 没有真实时间戳(后端不支持): 退回旧行为, 从 frameOriginUS 起按提交顺序首尾相接累加。
        /// </summary>
        public void FinalizeGpuStartTimes(long frameOriginUS, long clockOffsetUS)
        {
            lock (Locker)
            {
                if (GpuRealTime && AllEventsHaveRealTime())
                {
                    for (int i = 0; i < Count; i++)
                    {
                        ref var ev = ref Events[i];
                        if (ev.NameId < 0)
                            continue;
                        ev.StartUS += clockOffsetUS;
                    }
                    return;
                }

                long acc = frameOriginUS;
                for (int i = 0; i < Count; i++)
                {
                    ref var ev = ref Events[i];
                    if (ev.NameId < 0)
                        continue;
                    ev.StartUS = acc;
                    if (ev.DurUS > 0)
                        acc += ev.DurUS;
                }
            }
        }

        /// <summary>两种情况(有/无时间戳)混在同一条泳道里时, 只能整条回退到顺序模式</summary>
        bool AllEventsHaveRealTime()
        {
            for (int i = 0; i < Count; i++)
            {
                ref var ev = ref Events[i];
                if (ev.NameId < 0 || ev.DurUS < 0)
                    continue;
                // Reserve 时 StartUS 占位为 0, 真实 GPU 时钟不可能恰好是 0
                if (ev.StartUS == 0)
                    return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 一个槽 = 一帧。槽按 frameIndex % SlotCount 定位, GPU 事件用"录制帧号"回来找自己那一帧,
    /// 所以结算延迟不会把数据写到错误的帧上。
    /// </summary>
    public sealed class TtTraceFrameSlot
    {
        public int FrameIndex = -1;
        /// <summary>采集线程会在热路径上无锁读它, 必须 volatile</summary>
        public volatile ETraceSlotState State = ETraceSlotState.Free;
        /// <summary>
        /// 每次 Reset 自增。还留在调用栈里的旧 scope 用它认出"我那一帧已经没了",
        /// 否则槽被复用后它的 End 会写到新一轮 capture 的事件上
        /// </summary>
        public int Generation;
        public readonly List<TtTraceStream> Streams = new List<TtTraceStream>();
        /// <summary>本帧还没结算的 GPU 采样数; 归零前不能封帧</summary>
        public int PendingGpuScopes;
        public int IncompleteEvents;
        public long StartUS;
        /// <summary>对应的传输帧, 由 TtTracer.BindFrame 绑定</summary>
        public TtFrame Frame;

        public TtTraceFrameSlot() { }
        readonly object mStreamLocker = new object();

        public TtTraceStream GetStream(string name, ETraceChannel channel)
        {
            // 多个工作线程会同时进来建自己的泳道
            lock (mStreamLocker)
            {
                for (int i = 0; i < Streams.Count; i++)
                {
                    var s = Streams[i];
                    if (s.Channel == channel && s.Name == name)
                        return s;
                }
                var ns = new TtTraceStream { Name = name, Channel = channel };
                Streams.Add(ns);
                return ns;
            }
        }

        /// <summary>封帧遍历用: 此刻其它线程可能还在往 Streams 里加泳道, 不能直接遍历 List</summary>
        public TtTraceStream[] SnapshotStreams()
        {
            lock (mStreamLocker)
            {
                if (Streams.Count == 0)
                    return Array.Empty<TtTraceStream>();
                var arr = new TtTraceStream[Streams.Count];
                Streams.CopyTo(arr);
                return arr;
            }
        }

        public void Reset(int frameIndex, long startUS)
        {
            FrameIndex = frameIndex;
            Generation++;
            StartUS = startUS;
            PendingGpuScopes = 0;
            IncompleteEvents = 0;
            Frame = null;
            lock (mStreamLocker)
            {
                for (int i = 0; i < Streams.Count; i++)
                    Streams[i].Reset();
            }
            // 状态最后置: 别人看到 Recording 时, 上面的字段已经全部就位
            State = ETraceSlotState.Recording;
        }
    }

    /// <summary>
    /// 名字表: id 单调递增, 永不回收。
    /// 发送端与接收端各自维护一份, 发送端只把"新增段"随帧发出去;
    /// 因为 id 只增不减, TimeScope 可以安全缓存 id 而不必担心失效。
    /// </summary>
    public static class TtTraceNameTable
    {
        /// <summary>scope 没名字时的兜底: 事件必须有 id, 否则整段父链会断在它这里</summary>
        public const string Unnamed = "<unnamed>";
        /// <summary>接收端还没收到该 id 的名字</summary>
        public const string Unknown = "<unknown>";

        static readonly object msLocker = new object();
        static readonly List<string> msNames = new List<string>();
        static readonly Dictionary<string, int> msIds = new Dictionary<string, int>();

        public static int Count
        {
            get { lock (msLocker) { return msNames.Count; } }
        }

        public static int GetId(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = Unnamed;
            lock (msLocker)
            {
                if (msIds.TryGetValue(name, out var id))
                    return id;
                id = msNames.Count;
                msNames.Add(name);
                msIds.Add(name, id);
                return id;
            }
        }

        public static string GetName(int id)
        {
            lock (msLocker)
            {
                if (id < 0 || id >= msNames.Count)
                    return "";
                var n = msNames[id];
                // 接收端可能还没收到这个名字: 显示占位符, 而不是看起来像没名字
                return string.IsNullOrEmpty(n) ? Unknown : n;
            }
        }

        /// <summary>接收端用: 按发送端给的 id 落到本地表里</summary>
        public static void SetRemote(int id, string name)
        {
            if (id < 0 || string.IsNullOrEmpty(name))
                return;
            lock (msLocker)
            {
                while (msNames.Count <= id)
                    msNames.Add("");
                msNames[id] = name;
                if (msIds.ContainsKey(name) == false)
                    msIds.Add(name, id);
            }
        }
    }
}
