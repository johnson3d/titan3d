using System;
using System.Collections.Generic;

namespace EngineNS.Profiler.Trace
{
    /// <summary>
    /// 事件流传输单元: 一帧内某个 channel 的全部泳道数据 + 字符串表增量。
    ///
    /// 名字表按 id 单调递增, 每帧只把"上次发送之后新增的一段"带上(NewNames + NameIdBase),
    /// 之后同名字事件只发 int id, 这是这套协议最大的一笔省。
    /// </summary>
    public class TtEventStreamAction : TtAction
    {
        public const ushort TypeId = 3;
        public override ushort GetTypeId() => TypeId;

        // 反序列化的硬上限: 数据坏了的时候不能按对方声称的数量去 new 数组
        const int MaxNames = 1 << 20;
        const int MaxStreams = 4096;
        const int MaxEventsPerStream = 1 << 22;

        public int FrameIndex;
        public ETraceChannel Channel;
        /// <summary>NewNames[0] 对应的全局名字 id; -1 表示本 action 不带名字增量</summary>
        public int NameIdBase = -1;

        public List<string> NewNames = new List<string>();
        public List<TtStreamData> Streams = new List<TtStreamData>();

        public class TtStreamData
        {
            public string Name;
            public ETraceChannel Channel;
            public TtTraceEvent[] Events;
            public int Count;
        }

        public override ETraceChannel GetChannel()
        {
            return Channel;
        }

        /// <summary>
        /// 从已封帧的槽生成 action。每个 channel 一个 action。
        /// 注意名字增量不在这里算: 帧不一定按生成顺序发出(前面的帧可能还在等 GPU),
        /// 增量必须在真正发送那一刻按发送顺序切, 见 AttachNameDelta。
        /// </summary>
        public static void CreateFromSlot(TtTraceFrameSlot slot, List<TtEventStreamAction> outActions)
        {
            if (slot == null)
                return;

            TtEventStreamAction cpuAction = null;
            TtEventStreamAction gpuAction = null;

            // 封帧时其它线程可能还在往槽里加泳道, 拿快照
            var streams = slot.SnapshotStreams();
            for (int i = 0; i < streams.Length; i++)
            {
                var stream = streams[i];
                if (stream.Count == 0)
                    continue;

                TtEventStreamAction action;
                if (stream.Channel == ETraceChannel.Gpu)
                {
                    action = gpuAction ??= new TtEventStreamAction { FrameIndex = slot.FrameIndex, Channel = ETraceChannel.Gpu };
                }
                else
                {
                    action = cpuAction ??= new TtEventStreamAction { FrameIndex = slot.FrameIndex, Channel = ETraceChannel.Cpu };
                }

                var data = new TtStreamData
                {
                    Name = stream.Name,
                    Channel = stream.Channel,
                    Count = stream.Count,
                    // CopyEvents 与 Reserve 互排, 不会拷到扩容中的半截数组
                    Events = stream.CopyEvents(),
                };
                data.Count = data.Events.Length;
                action.Streams.Add(data);
            }

            if (cpuAction != null)
                outActions.Add(cpuAction);
            if (gpuAction != null)
                outActions.Add(gpuAction);
        }

        /// <summary>
        /// 在一帧即将被序列化前, 把本帧用到的名字里对端还没见过的那一段挂上去。
        /// 必须在发送顺序下调用: 名字 id 单调递增, 每帧只补 [lastSent, 本帧用的最大 id] 这一段,
        /// 所以不能提前到"挂事件流"的时候算 —— 那时后面的帧可能比前面的帧先发。
        /// </summary>
        public static void AttachNameDelta(TtFrame frame, ref int lastSentNameCount)
        {
            if (frame == null)
                return;

            TtEventStreamAction first = null;
            int maxId = -1;
            foreach (var ch in frame.Channels)
            {
                foreach (var act in ch.Actions)
                {
                    var stream = act as TtEventStreamAction;
                    if (stream == null)
                        continue;
                    first ??= stream;
                    for (int i = 0; i < stream.Streams.Count; i++)
                    {
                        var data = stream.Streams[i];
                        for (int k = 0; k < data.Count; k++)
                        {
                            if (data.Events[k].NameId > maxId)
                                maxId = data.Events[k].NameId;
                        }
                    }
                }
            }
            if (first == null || maxId < 0)
                return;
            if (maxId < lastSentNameCount)
                return;

            first.NameIdBase = lastSentNameCount;
            first.NewNames.Clear();
            int total = TtTraceNameTable.Count;
            int end = Math.Min(maxId, total - 1);
            for (int i = lastSentNameCount; i <= end; i++)
                first.NewNames.Add(TtTraceNameTable.GetName(i));
            lastSentNameCount = end + 1;
        }

        public override void Write(IO.IWriter writer)
        {
            writer.Write(FrameIndex);
            writer.Write((int)Channel);
            writer.Write(NameIdBase);
            writer.Write(NewNames.Count);
            for (int i = 0; i < NewNames.Count; i++)
                writer.Write(NewNames[i]);

            writer.Write(Streams.Count);
            for (int i = 0; i < Streams.Count; i++)
            {
                var s = Streams[i];
                writer.Write(s.Name ?? "");
                writer.Write((int)s.Channel);
                writer.Write(s.Count);
                for (int k = 0; k < s.Count; k++)
                    writer.Write(s.Events[k]);
            }
        }

        public override void Read(IO.IReader reader)
        {
            reader.Read(out FrameIndex);
            int channel;
            reader.Read(out channel);
            Channel = (ETraceChannel)channel;
            reader.Read(out NameIdBase);

            int nameCount;
            reader.Read(out nameCount);
            NewNames.Clear();
            if (nameCount < 0 || nameCount > MaxNames)
                throw new System.IO.InvalidDataException("TtEventStreamAction: bad name count " + nameCount);
            for (int i = 0; i < nameCount; i++)
            {
                reader.Read(out string name);
                NewNames.Add(name);
            }
            if (NameIdBase >= 0)
            {
                for (int i = 0; i < NewNames.Count; i++)
                    TtTraceNameTable.SetRemote(NameIdBase + i, NewNames[i]);
            }

            int streamCount;
            reader.Read(out streamCount);
            Streams.Clear();
            if (streamCount < 0 || streamCount > MaxStreams)
                throw new System.IO.InvalidDataException("TtEventStreamAction: bad stream count " + streamCount);
            for (int i = 0; i < streamCount; i++)
            {
                var s = new TtStreamData();
                reader.Read(out s.Name);
                int sc;
                reader.Read(out sc);
                s.Channel = (ETraceChannel)sc;
                reader.Read(out int count);
                if (count < 0 || count > MaxEventsPerStream)
                    throw new System.IO.InvalidDataException("TtEventStreamAction: bad event count " + count);
                s.Count = count;
                s.Events = new TtTraceEvent[count];
                for (int k = 0; k < count; k++)
                {
                    reader.Read(out TtTraceEvent ev);
                    s.Events[k] = ev;
                }
                Streams.Add(s);
            }
        }
    }
}
