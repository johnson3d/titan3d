using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Profiler.Trace
{
    public class TtAction
    {
        /// <summary>协议 id。用固定常量而不是按注册顺序分配, 收发两端才不会错位</summary>
        public virtual ushort GetTypeId()
        {
            return 0;
        }
        public static TtAction CreateByTypeId(ushort id)
        {
            switch (id)
            {
                case TtCpuFrameProfilerAction.TypeId: return new TtCpuFrameProfilerAction();
                case TtLogAction.TypeId: return new TtLogAction();
                case TtEventStreamAction.TypeId: return new TtEventStreamAction();
                default: return null;
            }
        }
        public virtual ETraceChannel GetChannel()
        {
            return ETraceChannel.None;
        }
        public virtual void Write(IO.IWriter writer)
        {

        }
        public virtual void Read(IO.IReader reader)
        {

        }
    }
    public class TtChannel
    {
        public ETraceChannel Channel { get; set; }
        public string Name { get; set; } = "";
        public List<TtAction> Actions = new List<TtAction>();
        /// <summary>读到未知 action id: 流从这里开始错位, 后面的字段全部不可信</summary>
        public bool Broken = false;
        public void Write(IO.IWriter writer)
        {
            writer.Write(Actions.Count);
            foreach (var act in Actions)
            {
                // 以前写 Rtti 类型名(每个 action 一次字符串), 现在只写 2 字节 id
                writer.Write(act.GetTypeId());
                act.Write(writer);
            }
        }
        public void Read(IO.IReader reader)
        {
            int count;
            reader.Read(out count);
            // action 数量不可能到这个量级, 超了就是流已经坏了
            if (count < 0 || count > 4096)
            {
                Broken = true;
                return;
            }
            for (int i = 0; i<count; i++)
            {
                ushort typeId;
                reader.Read(out typeId);
                var act = TtAction.CreateByTypeId(typeId);
                if (act == null)
                {
                    // 协议版本不匹配或新增了 action 而接收端还是旧的: 必须整帧丢弃, 否则会错位读下去
                    Broken = true;
                    return;
                }
                act.Read(reader);
                Actions.Add(act);
            }   
        }
    }

    public class TtCpuFrameProfilerAction : TtAction
    {
        public const ushort TypeId = 1;
        public override ushort GetTypeId() => TypeId;
        public override ETraceChannel GetChannel()
        {
            return ETraceChannel.Cpu;
        }
        public override void Write(IO.IWriter writer)
        {
            writer.Write(Profiler.TimeScopeManager.AllThreadInstance.Count);
            foreach (var i in Profiler.TimeScopeManager.AllThreadInstance)
            {
                writer.Write(i.ThreadName);
                writer.Write(i.Scopes.Count);
                foreach (var s in i.Scopes)
                {
                    writer.Write(s.Value.GetName());
                    writer.Write(s.Value.GetFriendName());
                    writer.Write(s.Value.mCoreObject.mAvgTime);
                    writer.Write(s.Value.mCoreObject.mAvgHit);
                    writer.Write(s.Value.mCoreObject.mMaxTimeInLife);
                    writer.Write(s.Value.mCoreObject.GetDebugSourceLine());
                    writer.Write(s.Value.mCoreObject.GetDebugSourceFile());
                    var num = s.Value.mCoreObject.GetNumOfCaller();
                    if (s.Value.mCoreObject.mParent.IsValidPointer == false)
                    {
                        writer.Write((int)0);
                    }
                    else
                    {
                        writer.Write(num);
                        for (int j = 0; j < num; j++)
                        {
                            writer.Write(s.Value.mCoreObject.GetCaller(j).GetName());
                            writer.Write(s.Value.mCoreObject.GetCallerRatio(j));
                        }
                    }
                }
            }
        }
        public override void Read(IO.IReader reader)
        {
            int threadCount;
            reader.Read(out threadCount);
            Threads.Clear();
            for (int i = 0; i<threadCount; i++)
            {
                var thread = new TtThreadScopeInfo();
                Threads.Add(thread);

                reader.Read(out thread.ThreadName);
                int scopeCount;
                reader.Read(out scopeCount);
                for (int j=0; j<scopeCount; j++)
                {
                    var scope = new TtRpcProfiler.RpcProfilerData.ScopeInfo();
                    reader.Read(out scope.Name);
                    reader.Read(out scope.ShowName);
                    reader.Read(out scope.AvgTime);
                    reader.Read(out scope.AvgHit);
                    reader.Read(out scope.MaxTime);
                    reader.Read(out scope.SourceLine);
                    reader.Read(out scope.SourceFile);
                    int callerNum;
                    reader.Read(out callerNum);
                    if (callerNum>0)
                    {
                        scope.Callers = new KeyValuePair<string, float>[callerNum];
                        for (int k = 0; k<callerNum; k++)
                        {
                            string callerName;
                            reader.Read(out callerName);
                            float callerRatio;
                            reader.Read(out callerRatio);
                            scope.Callers[k] = new KeyValuePair<string, float>(callerName, callerRatio);
                        }
                    }
                    thread.Scopes.Add(scope);
                    // Here you can store or process the read data as needed
                }
            }
        }

        public class TtThreadScopeInfo
        {
            public string ThreadName;
            public List<TtRpcProfiler.RpcProfilerData.ScopeInfo> Scopes = new List<TtRpcProfiler.RpcProfilerData.ScopeInfo>();
        }
        public List<TtThreadScopeInfo> Threads = new List<TtThreadScopeInfo>();
    }
    public class TtLogAction : TtAction
    {
        public const ushort TypeId = 2;
        public override ushort GetTypeId() => TypeId;
        public ELogTag Tag;
        public string Category;
        public string MemberName;
        public string SourceFilePath;
        public int SourceLineNumber;
        public string Info;
        public TtLogAction()
        {

        }
        public TtLogAction(ELogTag tag, string category, string memberName, string sourceFilePath, int sourceLineNumber, string info)
        {
            Tag =tag;
            Category = category;
            MemberName = memberName;
            SourceFilePath = sourceFilePath;
            SourceLineNumber = sourceLineNumber;
            Info = info;
        }
        public override ETraceChannel GetChannel()
        {
            return ETraceChannel.Log;
        }
        public override void Write(IO.IWriter writer)
        {
            writer.Write(Tag);
            writer.Write(Category);
            writer.Write(MemberName);
            writer.Write(SourceFilePath);
            writer.Write(SourceLineNumber);
            writer.Write(Info);
        }
        public override void Read(IO.IReader reader)
        {
            reader.Read(out Tag);
            reader.Read(out Category);
            reader.Read(out MemberName);
            reader.Read(out SourceFilePath);
            reader.Read(out SourceLineNumber);
            reader.Read(out Info);
        }
    }
}
