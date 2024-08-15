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
    public delegate bool FPostEventCondition(TtAsyncTaskStateBase state);
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
        public struct FUserArguments : IDisposable
        {
            public Vector2ui Value0;
            public object Obj0;
            public object Obj1;
            public object Obj2;
            public object Obj3;
            public void Dispose()
            {
                Obj0 = null;
                Obj1 = null;
                Obj2 = null;
                Obj3 = null;
                Value0 = Vector2ui.Zero;
            }
        }
        public FUserArguments UserArguments = new FUserArguments();

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

            UserArguments.Dispose();
        }

        public void DoContinueAction()
        {
            //long t1 = Support.Time.HighPrecision_GetTickCount();
            try
            {
                if (ExceptionInfo != null)
                {
                    throw ExceptionInfo;
                }
                ContinueAction();
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }
            finally
            {
                TaskState = EAsyncTaskState.Completed;
                //var t2 = Support.Time.HighPrecision_GetTickCount();
                //if (t2 - t1 > 10000)
                //{
                //    Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Performance", $"Continue({t2 - t1}): {this}");
                //}
                this.Dispose();
            }
        }

        public abstract void Dispose();
        public abstract TtAsyncTaskStateBase ExecutePostEvent();
        public abstract bool ExecutePostEventCondition();

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
        public FPostEventCondition PostActionCondition;
        public override string ToString()
        {
            var result = "Post=>\n";
            if (PostAction != null)
                result += PostAction.ToString();
            if (PostActionCondition != null)
                result += PostActionCondition.ToString();
            result += "Coninue=>\n";
            result += ContinueAction?.ToString();
            result += "Result=>\n";
            result += Result?.ToString();
            return result;
        }
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
            try
            {
                Result = PostAction(this);
            }
            catch (Exception exp)
            {
                this.ExceptionInfo = exp;
            }
            return this;
        }
        public override bool ExecutePostEventCondition()
        {
            return PostActionCondition(this);
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
                    return TtEngine.Instance.IsThread(target);
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
        public delegate void Delegate_ParrallelForAction(int index, object arg1, object arg2);
        public bool EnableMTForeach = true;
        private TtPooledSemaphoreAllocator ParrallelForSmpAllocator = new TtPooledSemaphoreAllocator();
        public void ParrallelFor(int num, Delegate_ParrallelForAction action, object userData1 = null, object userData2 = null)
        {
            if (num == 0)
                return;
            if (EnableMTForeach == false)
            {
                for (int i = 0; i < num; i++)
                {
                    action(i, userData1, userData2);
                }
            }
            else
            {
                var smp = ParrallelForSmpAllocator.QueryObjectSync();
                smp.Reset(num);
                for (int i = 0; i < num; i++)
                {
                    var userArgs = new TtAsyncTaskStateBase.FUserArguments();
                    userArgs.Obj0 = action;
                    userArgs.Obj1 = smp;
                    userArgs.Obj2 = userData1;
                    userArgs.Obj3 = userData2;
                    userArgs.Value0.X = (uint)i;
                    TtEngine.Instance.EventPoster.RunParallel(static (state) =>
                    {
                        var action = (Delegate_ParrallelForAction)state.UserArguments.Obj0;
                        action((int)state.UserArguments.Value0.X, state.UserArguments.Obj2, state.UserArguments.Obj3);
                        ((TtPooledSemaphore)state.UserArguments.Obj1).Semaphore.Release();
                        return true;
                    }, in userArgs/*, smp.WaitEvent*/);
                }
                smp.WaitEvent.WaitOne(int.MaxValue);
                ParrallelForSmpAllocator.ReleaseObject(smp);
            }
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
                    return TtEngine.Instance.GetContext(target);
            }
        }

        #region post event
        public void RunParallel<T>(FPostEvent<T> evt, in TtAsyncTaskStateBase.FUserArguments userArgs, System.Threading.AutoResetEvent completedEvent = null)
        {
            var eh = TtAsyncTaskState<T>.CreateInstance();
            eh.PostAction = evt;
            eh.ContinueThread = null;
            eh.AsyncType = EAsyncType.ParallelTasks;
            eh.UserArguments = userArgs;
            eh.CompletedEvent = completedEvent;

            lock (TPoolEvents)
            {
                TPoolEvents.Enqueue(eh);
                mTPoolTrigger.Set();
            }
        }
        public void RunOn<T>(FPostEvent<T> evt, EAsyncTarget target = EAsyncTarget.AsyncIO, object userArgs = null, System.Threading.AutoResetEvent completedEvent = null)
        {
            var eh = TtAsyncTaskState<T>.CreateInstance();
            //eh.PostAction = static (state) =>
            //{
            //    var ret = ((FPostEvent<T>)state.UserArguments.Obj1)(state);
            //    state.Dispose();
            //    return ret;
            //};
            eh.PostAction = evt;
            eh.ContinueThread = null;
            eh.AsyncType = EAsyncType.ParallelTasks;
            eh.UserArguments.Obj0 = userArgs;
            eh.UserArguments.Obj1 = evt;
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
        }
        public void RunOnUntilFinish(FPostEventCondition evt, EAsyncTarget target = EAsyncTarget.AsyncIO, object userArgs = null)
        {
            var eh = TtAsyncTaskState<bool>.CreateInstance();
            eh.PostActionCondition = evt;
            eh.ContinueThread = null;
            eh.AsyncType = EAsyncType.ParallelTasks;
            eh.UserArguments.Obj0 = userArgs;

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
            if (eh.ContinueThread == TtEngine.Instance.ThreadRHI)
            {
                switch(TtContextThread.TickStage)
                {
                    case 1:
                        eh.ContinueThread = TtEngine.Instance.ThreadMain;
                        break;
                    case 2:
                        eh.ContinueThread = TtEngine.Instance.ThreadRHI;
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
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"AwaitSemaphore is null");
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
                Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error, $"AwaitSemaphore is null");
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
                        lock (TtEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                        {
                            TtEngine.Instance.ContextThreadManager.AsyncIOEmptys.Add(PEvent);
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
                        lock (TtEngine.Instance.ContextThreadManager.TPoolEvents)
                        {
                            TtEngine.Instance.ContextThreadManager.TPoolEvents.Enqueue(PEvent);
                            TtEngine.Instance.ContextThreadManager.mTPoolTrigger.Set();
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
    partial class TtEngine
    {
        internal Thread.Async.TtContextThreadManager ContextThreadManager = new Thread.Async.TtContextThreadManager();
    }
}
