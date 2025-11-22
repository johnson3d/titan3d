using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Profiler.Trace
{
    public class TtAction
    {
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
        public void Write(IO.IWriter writer)
        {
            writer.Write(Actions.Count);
            foreach (var act in Actions)
            {
                writer.Write(Rtti.TtTypeDesc.TypeStr(act.GetType()));
                act.Write(writer);
            }
        }
        public void Read(IO.IReader reader)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i<count; i++)
            {
                string typeStr;
                reader.Read(out typeStr);
                var type = Rtti.TtTypeDesc.TypeOf(typeStr);
                var act = Rtti.TtTypeDescManager.CreateInstance(type) as TtAction;
                act.Read(reader);
                Actions.Add(act);
            }   
        }
    }

    public class TtCpuFrameProfilerAction : TtAction
    {
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
