//#define HAS_DebugInfo
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Thread.Async
{
    public enum EAsyncTarget
    {
        AsyncIO,
        Physics,
        Logic,
        Render,
        Main,
        AsyncEditor,
        AsyncEditorSlow,
        TPools,//塞在这个队列的异步处理，必须相互之间没有依赖，可以并行，因为线程池会有多条线程去取出来执行
    }
    public delegate T FPostEvent<T>(TtAsyncTaskStateBase state);
    public delegate T FPostEventCondition<T>(out bool bFinish, TtAsyncTaskStateBase state);
    internal enum EAsyncType
    {
        Normal,
        Semaphore,
        JobSystem,
        AsyncIOEmpty,
        ParallelTasks,
    }
    public enum EAsyncTaskState
    {
        Ready,
        Running,
        Suspended,
        Canceled,
        Completed,
    }
    public class TtAsyncTaskToken
    {
        public EAsyncTaskState TaskState = EAsyncTaskState.Ready;
    }
    public abstract class TtAsyncTaskStateBase : IPooledObject, IDisposable
    {
        public bool IsAlloc { get; set; } = false;
        public EAsyncTaskState TaskState = EAsyncTaskState.Ready;
        internal System.Threading.AutoResetEvent CompletedEvent = null;
        internal Action ContinueAction;
        public TtAsyncTaskToken TaskToken = null;
        public override string ToString()
        {
            return ContinueAction?.ToString();
        }

        internal EAsyncType AsyncType = EAsyncType.Normal;
        public TtContextThread ContinueThread;
        public TtContextThread AsyncTarget;
        public object UserArguments = null;
        
        public object Tag = null;
        public System.Exception ExceptionInfo = null;
        public System.Diagnostics.StackTrace CallStackTrace;
        public virtual void Reset()
        {
            ContinueAction = null;
            TaskState = EAsyncTaskState.Ready;
            CompletedEvent = null;

            AsyncType = EAsyncType.Normal;            
            ContinueThread = null;
            AsyncTarget = null;
            Tag = null;
            ExceptionInfo = null;
            CallStackTrace = null;

            UserArguments = null;
        }

        public void DoContinueAction()
        {
            long t1 = Support.Time.HighPrecision_GetTickCount();
            try
            {
                ContinueAction();
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            finally
            {
                TaskState = EAsyncTaskState.Completed;
                var t2 = Support.Time.HighPrecision_GetTickCount();
                if (t2 - t1 > 10000)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Performance", $"Continue({t2 - t1}): {this}");
                }
                this.Dispose();
            }
        }

        public abstract void Dispose();
        public abstract TtAsyncTaskStateBase ExecutePostEvent();
        public abstract void ExecutePostEventCondition(out bool bFinish);

        public void ExecuteContinue()
        {
            if (ContinueAction != null)
            {
#if PWindow
                var saved = System.Threading.SynchronizationContext.Current;
                System.Threading.SynchronizationContext.SetSynchronizationContext(null);
                DoContinueAction();
                System.Threading.SynchronizationContext.SetSynchronizationContext(saved);
#else
                DoContinueAction();
#endif
            }
            else
            {
                TaskState = EAsyncTaskState.Completed;
                CompletedEvent?.Set();
            }
        }
    }

    public class TtAsyncTaskState<T> : TtAsyncTaskStateBase
    {
        internal T Result = default(T);
        public FPostEvent<T> PostAction;
        public FPostEventCondition<T> PostActionCondition;
        #region life manage
        public class TtAsyncTaskStateAllocator : TtObjectPool<TtAsyncTaskState<T>>
        {
            protected override bool OnObjectRelease(TtAsyncTaskState<T> obj)
            {
                obj.Reset();
                return true;
            }
        }
        static TtAsyncTaskStateAllocator mAllocator = new TtAsyncTaskStateAllocator();
        public static TtAsyncTaskState<T> CreateInstance(uint timeOut = uint.MaxValue)
        {
            var result = mAllocator.QueryObjectSync();
            result.PostAction = null;
            result.AsyncTarget = null;
            result.ContinueThread = TtContextThread.CurrentContext;
            return result;
        }
        public override void Dispose()
        {
            mAllocator.ReleaseObject(this);
        }
        #endregion
        public override void Reset()
        {
            PostAction = null;
            PostActionCondition = null;
            base.Reset();
        }
        public override TtAsyncTaskStateBase ExecutePostEvent()
        {
            this.TaskState = EAsyncTaskState.Running;
            Result = PostAction(this);
            return this;
        }
        public override void ExecutePostEventCondition(out bool bFinish)
        {
            Result = PostActionCondition(out bFinish, this);
        }
    }

    public class TtContextThreadManager
    {
        public static bool ImmidiateMode = false;

        public bool IsThread(EAsyncTarget target)
        {
            switch (target)
            {
                case EAsyncTarget.TPools:
                    var ctx = Thread.TtContextThread.CurrentContext;
                    foreach(var i in ContextPools)
                    {
                        if (i == ctx)
                            return true;
                    }
                    break;
                default:
                    return UEngine.Instance.IsThread(target);
            }
            return false;
        }

        internal Queue<TtAsyncTaskStateBase> TPoolEvents = new Queue<TtAsyncTaskStateBase>();
        internal System.Threading.AutoResetEvent mTPoolTrigger = new System.Threading.AutoResetEvent(false);
        internal TtThreadPool[] ContextPools;
        internal List<TtAsyncTaskStateBase> AsyncIOEmptys = new List<TtAsyncTaskStateBase>();
        public int PooledThreadNum
        {
            get { return ContextPools.Length; }
        }

        #region for each
        public struct MTPSegment
        {
            public int Start;
            public int End;
        }
        private MTPSegment[] GetMultThreadSegement(int num)
        {
            int stride = num / ContextPools.Length;
            int remain = num % ContextPools.Length;
            var result = new MTPSegment[ContextPools.Length];
            int startIndex = 0;
            for (int i = 0; i < ContextPools.Length; i++)
            {
                result[i].Start = startIndex;
                startIndex += stride;
                result[i].End = startIndex;
            }
            result[ContextPools.Length - 1].End += remain;
            return result;
        }
        public delegate void FMTS_DoForeach(int index);
        public delegate void FMTS_DoForeach2(int index, Thread.TtSemaphore smp);
        public delegate void FMTS_DoForeach<T>(T v);
        public delegate void FMTS_DoForeachRef<T>(ref T v);
        public bool EnableMTForeach = true;
        public void FMTS_Foreach<T>(T[] lst, FMTS_DoForeachRef<T> action)
        {
            int num = lst.Length;
            if (EnableMTForeach == false)
            {
                for (int i = 0; i < num; i++)
                {
                    action(ref lst[i]);
                }
                return;
            }
            int stride = num / ContextPools.Length;
            int remain = num % ContextPools.Length;

            unsafe
            {
                MTPSegment* segs = stackalloc MTPSegment[ContextPools.Length];

                int startIndex = 0;
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    segs[i].Start = startIndex;
                    startIndex += stride;
                    segs[i].End = startIndex;
                }
                segs[ContextPools.Length - 1].End += remain;

                var smp = EngineNS.Thread.TtSemaphore.CreateSemaphore(ContextPools.Length, new System.Threading.AutoResetEvent(false));
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    MTPSegment curSeg = segs[i];
                    UEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        try
                        {
                            for (int j = curSeg.Start; j < curSeg.End; j++)
                            {
                                action(ref lst[j]);
                            }
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                        }
                        smp.Release();
                        return true;
                    }, Thread.Async.EAsyncTarget.TPools);
                }
                smp.Wait(int.MaxValue);
            }
        }
        public void FMTS_Foreach<T>(List<T> lst, FMTS_DoForeach<T> action)
        {
            int num = lst.Count;
            if (EnableMTForeach == false)
            {
                for (int i = 0; i < num; i++)
                {
                    action(lst[i]);
                }
                return;
            }            
            int stride = num / ContextPools.Length;
            int remain = num % ContextPools.Length;

            unsafe
            {
                MTPSegment* segs = stackalloc MTPSegment[ContextPools.Length];

                int startIndex = 0;
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    segs[i].Start = startIndex;
                    startIndex += stride;
                    segs[i].End = startIndex;
                }
                segs[ContextPools.Length - 1].End += remain;

                var smp = EngineNS.Thread.TtSemaphore.CreateSemaphore(ContextPools.Length, new System.Threading.AutoResetEvent(false));
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    MTPSegment curSeg = segs[i];
                    UEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        try
                        {
                            for (int j = curSeg.Start; j < curSeg.End; j++)
                            {
                                action(lst[j]);
                            }
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                        }
                        smp.Release();
                        return true;
                    }, Thread.Async.EAsyncTarget.TPools);
                }
                smp.Wait(int.MaxValue);
            }
        }
        public void MTS_Foreach(int num, FMTS_DoForeach action)
        {
            if(EnableMTForeach==false)
            {
                for(int i=0;i<num;i++)
                {
                    action(i);
                }
                return;
            }
            int stride = num / ContextPools.Length;
            int remain = num % ContextPools.Length;
            unsafe
            {
                MTPSegment* segs = stackalloc MTPSegment[ContextPools.Length];

                int startIndex = 0;
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    segs[i].Start = startIndex;
                    startIndex += stride;
                    segs[i].End = startIndex;
                }
                segs[ContextPools.Length - 1].End += remain;

                var smp = EngineNS.Thread.TtSemaphore.CreateSemaphore(ContextPools.Length, new System.Threading.AutoResetEvent(false));
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    MTPSegment curSeg = segs[i];
                    UEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        try
                        {
                            for (int j = curSeg.Start; j < curSeg.End; j++)
                            {
                                action(j);
                            }
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                        }                        
                        smp.Release();
                        return true;
                    }, Thread.Async.EAsyncTarget.TPools);
                }
                smp.Wait(int.MaxValue);
            }            
        }
        public async System.Threading.Tasks.Task AwaitMTS_Foreach(int num, FMTS_DoForeach2 action)
        {
            int stride = num / ContextPools.Length;
            int remain = num % ContextPools.Length;
            var smp = EngineNS.Thread.TtSemaphore.CreateSemaphore(ContextPools.Length, new System.Threading.AutoResetEvent(false));
            unsafe
            {
                MTPSegment* segs = stackalloc MTPSegment[ContextPools.Length];

                int startIndex = 0;
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    segs[i].Start = startIndex;
                    startIndex += stride;
                    segs[i].End = startIndex;
                }
                segs[ContextPools.Length - 1].End += remain;

                for (int i = 0; i < ContextPools.Length; i++)
                {
                    MTPSegment curSeg = segs[i];
                    UEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        try
                        {
                            for (int j = curSeg.Start; j < curSeg.End; j++)
                            {
                                action(j, smp);
                            }
                        }
                        catch (Exception ex)
                        {
                            Profiler.Log.WriteException(ex);
                        }
                        smp.Release();
                        return true;
                    }, Thread.Async.EAsyncTarget.TPools);
                }
            }
            await smp.Await();
        }
        #endregion

        public void StartPools(int count)
        {
            if (count == -1)
            {
                count = System.Environment.ProcessorCount - 2;
                if (count <= 0)
                {
                    count = 1;
                }
            }
            ContextPools = new TtThreadPool[count];
            for (int i = 0; i < count; i++)
            {
                ContextPools[i] = new TtThreadPool();
                ContextPools[i].StartThread($"TPool{i}", null);
            }
        }
        public void StopPools()
        {
            foreach(var i in ContextPools)
            {
                i.StopThread(null);
            }
        }
        public TtContextThread GetContext(EAsyncTarget target)
        {
            switch (target)
            {
                case EAsyncTarget.TPools:
                    {
                        TtThreadPool thread = null;
                        int nMinTask = int.MaxValue;
                        foreach (var i in ContextPools)
                        {
                            if (i.AsyncNum < nMinTask)
                            {
                                nMinTask = i.AsyncNum;
                                thread = i;
                            }
                        }
                        return thread;
                    }
                default:
                    return UEngine.Instance.GetContext(target);
            }
        }

        #region post event
        public FTaskAwaiter<T> RunOn<T>(FPostEvent<T> evt, EAsyncTarget target = EAsyncTarget.AsyncIO, object userArgs = null, System.Threading.AutoResetEvent completedEvent = null)
        {
            var eh = TtAsyncTaskState<T>.CreateInstance();
            eh.PostAction = evt;
            eh.ContinueThread = null;
            eh.AsyncType = EAsyncType.ParallelTasks;
            eh.UserArguments = userArgs;
            eh.CompletedEvent = completedEvent;

            if (target == EAsyncTarget.TPools)
            {
                lock (TPoolEvents)
                {
                    TPoolEvents.Enqueue(eh);
                    mTPoolTrigger.Set();
                }
            }
            else
            {
                TtContextThread ctx = GetContext(target);
                if (ctx != null)
                {
                    ctx.EnqueuePriority(eh);
                }
            }
            return new FTaskAwaiter<T>(eh);
        }
        public void RunOnUntilFinish<T>(FPostEventCondition<T> evt, EAsyncTarget target = EAsyncTarget.AsyncIO, object userArgs = null)
        {
            var eh = TtAsyncTaskState<T>.CreateInstance();
            eh.PostActionCondition = evt;
            eh.ContinueThread = null;
            eh.AsyncType = EAsyncType.ParallelTasks;
            eh.UserArguments = userArgs;

            if (target != EAsyncTarget.TPools)
            {
                TtContextThread ctx = GetContext(target);
                if(ctx != null)
                {
                    lock(ctx.RunUntilFinishEvents)
                    {
                        ctx.RunUntilFinishEvents.Add(eh);
                    }
                }
            }
        }
        public FTaskAwaiter<T> Post<T>(FPostEvent<T> evt, EAsyncTarget target = EAsyncTarget.AsyncIO)
        {
            TtContextThread ctx = GetContext(target);

            var eh = TtAsyncTaskState<T>.CreateInstance();
            eh.AsyncTarget = ctx;
            eh.ContinueThread = TtContextThread.CurrentContext;
            if (eh.ContinueThread == UEngine.Instance.ThreadRHI)
            {
                switch(TtContextThread.TickStage)
                {
                    case 1:
                        eh.ContinueThread = UEngine.Instance.ThreadMain;
                        break;
                    case 2:
                        eh.ContinueThread = UEngine.Instance.ThreadRHI;
                        break;
                }
            }
            eh.PostAction = evt;
            return new FTaskAwaiter<T>(eh);
            //return await TaskExtensionForPost.AwaitPost(eh);
        }
        public FTaskAwaiter<bool> AwaitSemaphore(TtSemaphore smp)
        {   
            var eh = TtAsyncTaskState<bool>.CreateInstance();
            eh.ContinueThread = TtContextThread.CurrentContext;
            eh.AsyncType = EAsyncType.Semaphore;
            eh.Tag = smp;
            smp.PostEvent = eh;

            if (smp==null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "", $"AwaitSemaphore is null");
            }

            return new FTaskAwaiter<bool>(eh);
            //return TtTask<bool>.CreateInstance(new FTaskAwaiter<bool>(eh));
            //smp = null;
        }
        public FTaskAwaiter<bool> AwaitJobSystem(Async.IJobSystem smp)
        {
            var eh = TtAsyncTaskState<bool>.CreateInstance();
            eh.AsyncType = EAsyncType.JobSystem;
            if (smp == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "", $"AwaitSemaphore is null");
                return new FTaskAwaiter<bool>(eh);
            }
            eh.Tag = smp;
            smp.PostEvent = eh;
            return new FTaskAwaiter<bool>(eh);
        }
        #endregion

        #region post SyncEvent
        public delegate bool FPostTickSync(long tickCount);
        protected List<FPostTickSync> mPostTickSyncEvents = new List<FPostTickSync>();
        public void PostTickSyncEvent(FPostTickSync evt)
        {
            lock (mPostTickSyncEvents)
            {
                mPostTickSyncEvents.Add(evt);
            }
        }
        public void TickPostTickSyncEvents(long tickCount)
        {
            lock (mPostTickSyncEvents)
            {
                for (int i = 0; i < mPostTickSyncEvents.Count; i++)
                {
                    if (mPostTickSyncEvents[i](tickCount))
                    {
                        mPostTickSyncEvents.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        #endregion
    }

    public struct FTaskAwaiter<T> : System.Runtime.CompilerServices.INotifyCompletion
    {
        TtAsyncTaskState<T> PEvent;

        public FTaskAwaiter(TtAsyncTaskState<T> pe)
        {
            PEvent = pe;
        }
        public FTaskAwaiter<T> GetAwaiter()
        {
            return this;
        }
        public FTaskAwaiter<T> WithToken(TtAsyncTaskToken token)
        {
            PEvent.TaskToken = token;
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            PEvent.ContinueAction = continuation;
            bool isTargetIsCurrent = false;
            
            if (PEvent.AsyncType == EAsyncType.Normal)
            {
                if (TtContextThread.CurrentContext == null)
                    isTargetIsCurrent = false;
                else
                    isTargetIsCurrent = PEvent.AsyncTarget == TtContextThread.CurrentContext;
            }
            if (TtContextThreadManager.ImmidiateMode || isTargetIsCurrent)
            {
                if (PEvent != null && PEvent.PostAction != null)
                {
                    var state = PEvent.ExecutePostEvent();
                    if (state.TaskState == EAsyncTaskState.Suspended)
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                    else
                    {
                        PEvent.DoContinueAction();
                    }
                }
                else
                {
                    PEvent.DoContinueAction();
                }
                
                return;
            }

#if HAS_DebugInfo
            PEvent.CallStackTrace = new System.Diagnostics.StackTrace(2);
#endif
            
            switch (PEvent.AsyncType)
            {
                case EAsyncType.Normal:
                    {
                        PEvent.AsyncTarget.EnqueueAsync(PEvent);
                    }
                    break;
                case EAsyncType.AsyncIOEmpty:
                    {
                        System.Diagnostics.Debug.Assert(PEvent.AsyncTarget == null);
                        System.Diagnostics.Debug.Assert(PEvent.PostAction == null);
                        lock (UEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                        {
                            UEngine.Instance.ContextThreadManager.AsyncIOEmptys.Add(PEvent);
                        }
                    }
                    break;
                case EAsyncType.Semaphore:
                    {
                        var smp = PEvent.Tag as TtSemaphore;
                        if (smp.GetCount() == 0)
                        {
                            //EqueueContinue做了防止重复Enqueue的处理
                            //如果Release导致提前Enqueue了，这里就不会真的在入队列一次
                            //否则会出现已经完成的任务再转换的异常
                            //为什么这里还要入队一次，因为有低概率在Release的时候，PostEvent等待任务
                            //依然没有赋值好，这里做一次擦屁股的处理
                            smp.EqueueContinue();
                        }
                    }
                    break;
                case EAsyncType.JobSystem:
                    {
                        var smp = PEvent.Tag as Async.IJobSystem;
                        if (smp.IsFinshed())
                        {
                            try
                            {
                                continuation();
                            }
                            finally 
                            {
                                PEvent.Dispose(); 
                            }
                        }
                    }
                    break;
                case EAsyncType.ParallelTasks:
                    {
                        lock (UEngine.Instance.ContextThreadManager.TPoolEvents)
                        {
                            UEngine.Instance.ContextThreadManager.TPoolEvents.Enqueue(PEvent);
                            UEngine.Instance.ContextThreadManager.mTPoolTrigger.Set();
                        }
                    }
                    break;
            }
        }
        public bool IsCompleted
        {
            get
            {
                return PEvent.TaskState == EAsyncTaskState.Completed;
                //return task.IsCompleted;
            }
        }
        public T GetResult()
        {
            if (PEvent.ExceptionInfo != null)
            {
                //throw PEvent.ExceptionInfo;
                Profiler.Log.WriteException(PEvent.ExceptionInfo);
            }
            return PEvent.Result;
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        internal Thread.Async.TtContextThreadManager ContextThreadManager = new Thread.Async.TtContextThreadManager();
    }
}
