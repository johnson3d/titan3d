//#define HAS_DebugInfo
using System;
using System.Collections.Generic;
using System.Text;

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
    public delegate object FPostEvent();
    public delegate System.Threading.Tasks.Task<T> FAsyncPostEvent<T>();
    public delegate T FPostEventReturn<T>();
    internal enum EAsyncType
    {
        Normal,
        Semaphore,
        JobSystem,
        AsyncIOEmpty,
        ParallelTasks,
        Delay,
    }
    public class PostEvent
    {
        internal EAsyncType AsyncType = EAsyncType.Normal;
        public FPostEvent PostAction;
        public TaskAwaiter Awaiter;
        public ContextThread ContinueThread;
        public ContextThread AsyncTarget;
        
        public object Tag = null;
        public System.Exception ExceptionInfo = null;
        public System.Diagnostics.StackTrace CallStackTrace;
        public void Reset()
        {
            AsyncType = EAsyncType.Normal;
            PostAction = null;
            Awaiter = null;
            ContinueThread = null;
            AsyncTarget = null;
            Tag = null;
            ExceptionInfo = null;
            CallStackTrace = null;
        }
    }
    public class UContextThreadManager
    {
        public static bool ImmidiateMode = false;

        public bool IsThread(EAsyncTarget target)
        {
            switch (target)
            {
                case EAsyncTarget.TPools:
                    var ctx = Thread.ContextThread.CurrentContext;
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

        public class PostEventAllocator : UPooledObject<PostEvent>
        {
            protected override bool OnObjectRelease(PostEvent obj)
            {
                obj.Reset();
                return true;
            }
        }
        public PostEventAllocator mRunOnPEAllocator = new PostEventAllocator();
        internal Queue<PostEvent> TPoolEvents = new Queue<PostEvent>();
        internal System.Threading.AutoResetEvent mTPoolTrigger = new System.Threading.AutoResetEvent(false);
        internal ThreadPool[] ContextPools;
        internal List<PostEvent> AsyncIOEmptys = new List<PostEvent>();
        public int PooledThreadNum
        {
            get { return ContextPools.Length; }
        }

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
        public delegate void FMTS_DoForeach2(int index, Thread.ASyncSemaphore smp);
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

                var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(ContextPools.Length, new System.Threading.AutoResetEvent(false));
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    MTPSegment curSeg = segs[i];
                    UEngine.Instance.EventPoster.RunOn(() =>
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
                        return null;
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

                var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(ContextPools.Length, new System.Threading.AutoResetEvent(false));
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    MTPSegment curSeg = segs[i];
                    UEngine.Instance.EventPoster.RunOn(() =>
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
                        return null;
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

                var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(ContextPools.Length, new System.Threading.AutoResetEvent(false));
                for (int i = 0; i < ContextPools.Length; i++)
                {
                    MTPSegment curSeg = segs[i];
                    UEngine.Instance.EventPoster.RunOn(() =>
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
                        return null;
                    }, Thread.Async.EAsyncTarget.TPools);
                }
                smp.Wait(int.MaxValue);
            }            
        }
        public async System.Threading.Tasks.Task AwaitMTS_Foreach(int num, FMTS_DoForeach2 action)
        {
            int stride = num / ContextPools.Length;
            int remain = num % ContextPools.Length;
            var smp = EngineNS.Thread.ASyncSemaphore.CreateSemaphore(ContextPools.Length, new System.Threading.AutoResetEvent(false));
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
                    UEngine.Instance.EventPoster.RunOn(() =>
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
                        return null;
                    }, Thread.Async.EAsyncTarget.TPools);
                }
            }
            await smp.Await();
        }
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
            ContextPools = new ThreadPool[count];
            for (int i = 0; i < count; i++)
            {
                ContextPools[i] = new ThreadPool();
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
        public ContextThread GetContext(EAsyncTarget target)
        {
            switch (target)
            {
                case EAsyncTarget.TPools:
                    {
                        ThreadPool thread = null;
                        int nMinTask = int.MaxValue;
                        foreach (var i in ContextPools)
                        {
                            if (i.AsyncEvents.Count < nMinTask)
                            {
                                nMinTask = i.AsyncEvents.Count;
                                thread = i;
                            }
                        }
                        return thread;
                    }
                default:
                    return UEngine.Instance.GetContext(target);
            }
        }
        public void RunOn<T>(FAsyncPostEvent<T> evt, EAsyncTarget target = EAsyncTarget.AsyncIO)
        {
            if (UContextThreadManager.ImmidiateMode)
            {
                evt();
                return;
            }

            ContextThread ctx = GetContext(target);
            FPostEvent ue = () =>
            {
                return evt();
            };
            var eh = mRunOnPEAllocator.QueryObjectSync();
            eh.PostAction = ue;
            eh.ContinueThread = null;
            eh.AsyncType = EAsyncType.ParallelTasks;
            if (ctx != null)
            {
                lock (ctx.PriorityEvents)
                {
                    ctx.PriorityEvents.Enqueue(eh);
                }
            }
            else
            {
                lock (TPoolEvents)
                {
                    TPoolEvents.Enqueue(eh);
                    mTPoolTrigger.Set();
                }
            }
        }
        public void RunOn(FPostEvent evt, EAsyncTarget target = EAsyncTarget.AsyncIO)
        {
            var eh = mRunOnPEAllocator.QueryObjectSync();
            eh.PostAction = evt;
            eh.ContinueThread = null;
            eh.AsyncType = EAsyncType.ParallelTasks;

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
                ContextThread ctx = GetContext(target);
                if (ctx != null)
                {
                    lock (ctx.PriorityEvents)
                    {
                        ctx.PriorityEvents.Enqueue(eh);
                    }
                }
            }
        }
        public async System.Threading.Tasks.Task DelayTime(int time)
        {
            var eh = new PostEvent();
            eh.PostAction = null;
            eh.ContinueThread = ContextThread.CurrentContext;
            eh.AsyncType = EAsyncType.Delay;
            eh.PostAction = () =>
            {
                System.Threading.Thread.Sleep(time);
                return null;
            };
            var source = new System.Threading.Tasks.TaskCompletionSource<object>();
            await source.Task.AwaitPost(eh);
        }
        public async System.Threading.Tasks.Task<T> Post<T>(FPostEventReturn<T> evt, EAsyncTarget target = EAsyncTarget.AsyncIO)
        {
            var eh = new PostEvent();
            ContextThread ctx = GetContext(target);
            eh.AsyncTarget = ctx;
            eh.ContinueThread = ContextThread.CurrentContext;
            if (eh.ContinueThread == UEngine.Instance.ThreadRHI)
            {
                switch(ContextThread.TickStage)
                {
                    case 1:
                        eh.ContinueThread = UEngine.Instance.ThreadMain;
                        break;
                    case 2:
                        eh.ContinueThread = UEngine.Instance.ThreadRHI;
                        break;
                }
            }
            FPostEvent ue = () =>
            {
                try
                {
                    var result = evt();
                    return result;
                }
                catch(Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                    return default(T);
                }
            };
            eh.PostAction = ue;
            var source = new System.Threading.Tasks.TaskCompletionSource<object>();
            object ret = await source.Task.AwaitPost(eh);
            try
            {
                return (T)ret;
            }
            catch
            {
                return default(T);
            }
        }
        public async System.Threading.Tasks.Task AwaitSemaphore(ASyncSemaphore smp)
        {
            if(smp==null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "", $"AwaitSemaphore is null");
                return;
            }
            var eh = new PostEvent();
            eh.PostAction = null;
            eh.AsyncTarget = null;
            eh.ContinueThread = ContextThread.CurrentContext;
            eh.AsyncType = EAsyncType.Semaphore;
            eh.Tag = smp;
            smp.PostEvent = eh;

            var source = new System.Threading.Tasks.TaskCompletionSource<object>();
            await source.Task.AwaitPost(eh);
            smp = null;
        }
        public async System.Threading.Tasks.Task AwaitJobSystem(Async.IJobSystem smp)
        {
            if (smp == null)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "", $"AwaitSemaphore is null");
                return;
            }
            var eh = new PostEvent();
            eh.PostAction = null;
            eh.AsyncTarget = null;
            eh.ContinueThread = ContextThread.CurrentContext;
            eh.AsyncType = EAsyncType.JobSystem;
            eh.Tag = smp;
            smp.PostEvent = eh;

            var source = new System.Threading.Tasks.TaskCompletionSource<object>();
            await source.Task.AwaitPost(eh);
            smp = null;
        }
    }

    public class TaskAwaiter : System.Runtime.CompilerServices.INotifyCompletion
    {
        public System.Threading.Tasks.Task task;
        PostEvent PEvent;
        private Action ContinueAction;
        private object Result;
        private bool FinishedContinue = false;
        public override string ToString()
        {
            return ContinueAction.ToString();
        }
        public void SetResult(object r)
        {
            Result = r;
            //FinishedContinue = true;
        }
        public bool IsContinueAction()
        {
            return ContinueAction != null;
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
                FinishedContinue = true;
                var t2 = Support.Time.HighPrecision_GetTickCount();
                if (t2 - t1 > 10000)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Performance", $"Continue({t2 - t1}): {this.PEvent.PostAction?.Target?.ToString()}");
                }
            }
        }
        public TaskAwaiter(System.Threading.Tasks.Task task, PostEvent pe)
        {
            PEvent = pe;
            pe.Awaiter = this;
            this.task = task;
        }
        public TaskAwaiter GetAwaiter()
        {
            return this;
        }
        public void OnCompleted(Action continuation)
        {
            bool isTargetIsCurrent = false;
            if (PEvent.AsyncType == EAsyncType.Normal)
            {
                if (ContextThread.CurrentContext == null)
                    isTargetIsCurrent = false;
                else
                    isTargetIsCurrent = PEvent.AsyncTarget == ContextThread.CurrentContext;
            }
            if (UContextThreadManager.ImmidiateMode || isTargetIsCurrent)
            {
                if (PEvent != null && PEvent.PostAction != null)
                {
                    var ret = PEvent.PostAction();
                    Result = ret;
                }
                ContinueAction = continuation;
                DoContinueAction();
                return;
            }
#if HAS_DebugInfo
            PEvent.CallStackTrace = new System.Diagnostics.StackTrace(2);
#endif
            PEvent.Awaiter.ContinueAction = continuation;
            switch (PEvent.AsyncType)
            {
                case EAsyncType.Normal:
                    {
                        lock (PEvent.AsyncTarget.AsyncEvents)
                        {
                            PEvent.AsyncTarget.AsyncEvents.Enqueue(PEvent);
                        }
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
                        var smp = PEvent.Tag as ASyncSemaphore;
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
                            continuation();
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
                case EAsyncType.Delay:
                    {
                        System.Threading.ThreadPool.QueueUserWorkItem((data) =>
                        {
                            PEvent.PostAction();
                            lock (PEvent.ContinueThread.ContinueEvents)
                            {
                                PEvent.ContinueThread.ContinueEvents.Enqueue(PEvent);
                            }
                        });
                    }
                    break;
            }
        }
        public bool IsCompleted
        {
            get
            {
                return FinishedContinue;
                //return task.IsCompleted;
            }
        }
        public object GetResult()
        {
            if (PEvent.ExceptionInfo != null)
            {
                //throw PEvent.ExceptionInfo;
                Profiler.Log.WriteException(PEvent.ExceptionInfo);
            }
            return Result;
        }
    }
    public static class TaskExtensionForPost
    {
        public static TaskAwaiter AwaitPost(this System.Threading.Tasks.Task task, PostEvent pe)
        {
            pe.Awaiter = new TaskAwaiter(task, pe);
            return pe.Awaiter;
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        internal Thread.Async.UContextThreadManager ContextThreadManager = new Thread.Async.UContextThreadManager();
    }
}
