//#define HAS_DebugInfo
#define STATE_TIME
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
        AsyncIOAfterEmpty,//特殊的，当ThreadAsync的所有任务清空后执行
        Physics,
        Logic,
        Render,
        Main,
        AsyncEditor,
        TPools,//塞在这个队列的异步处理，必须相互之间没有依赖，可以并行，因为线程池会有多条线程去取出来执行
    }
    public delegate T FPostEvent<T>(TtAsyncTaskStateBase state);
    public delegate bool FPostEventCondition(TtAsyncTaskStateBase state);
    internal enum EAsyncType
    {
        Normal,
        Semaphore,
        AsyncIOAfterEmpty,//特殊的，当ThreadAsync的所有任务清空后执行
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
#if STATE_TIME
        public Int64 CreateTime = 0;
#endif
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
            public Vector4ui Value0;
            public object Obj0;
            public object Obj1;
            public object Obj2;
            public object Obj3;
            public uint TaskIndexOfParallelFor
            {
                get => Value0.X;
            }
            public uint NumOfParallelFor
            {
                get => Value0.Y;
            }
            public uint StrideOfParallelFor
            {
                get => Value0.Z;
            }
            public void Dispose()
            {
                Obj0 = null;
                Obj1 = null;
                Obj2 = null;
                Obj3 = null;
                Value0 = Vector4ui.Zero;
            }
        }
        public FUserArguments UserArguments = new FUserArguments();

        #region ParallelFor
        public int TaskIndexOfParallelFor
        {
            get => (int)UserArguments.TaskIndexOfParallelFor;
        }
        public uint NumOfParallelFor
        {
            get => UserArguments.NumOfParallelFor;
        }
        public uint StrideOfParallelFor
        {
            get => UserArguments.StrideOfParallelFor;
        }
        public T GetForArgument0<T>()
        {
            return (T)UserArguments.Obj2;
        }
        public T GetForArgument1<T>()
        {
            return (T)UserArguments.Obj3;
        }
        #endregion

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
            public override string ShowName
            {
                get
                {
                    return $"TtAsyncTaskState<{typeof(T).FullName}>";
                }
            }
        }
        static TtAsyncTaskStateAllocator mAllocator = new TtAsyncTaskStateAllocator();
        public static TtAsyncTaskState<T> CreateInstance(uint timeOut = uint.MaxValue)
        {
            var result = mAllocator.QueryObjectSync();
#if STATE_TIME
            result.CreateTime = Support.TtTime.HighPrecision_GetTickCount();
#endif
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
            Result = default(T);
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

    public partial class TtContextThreadManager
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
        internal Support.TtBitset IdleThreads;
        internal TtThreadPool[] ContextPools;
        internal List<TtAsyncTaskStateBase> AsyncIOEmptys = new List<TtAsyncTaskStateBase>();
        public int PooledThreadNum
        {
            get { return ContextPools.Length; }
        }

        #region for each
        public delegate void Delegate_ParrallelForAction(int index, TtAsyncTaskStateBase state);
        public bool EnableMTForeach = true;
        internal TtPooledSemaphoreAllocator ParallelForSmpAllocator = new TtPooledSemaphoreAllocator();
        [ThreadStatic]
        private static Profiler.TimeScope mScopeParrallelForWait;
        private static Profiler.TimeScope ScopeParrallelForWait
        {
            get
            {
                if (mScopeParrallelForWait == null)
                    mScopeParrallelForWait = new Profiler.TimeScope(typeof(TtContextThreadManager), nameof(ParallelFor) + ".Wait");
                return mScopeParrallelForWait;
            }
        }
        public void ParallelFor(int numTask, int numTaskGroup, Delegate_ParrallelForAction action, object userData1 = null, object userData2 = null)
        {
            System.Diagnostics.Debug.Assert(Thread.TtContextThread.CurrentContext.GetThreadType()!= EAsyncTarget.TPools);
            if (numTask == 0)
                return;

            var smp = ParallelForSmpAllocator.QueryObjectSync();
            smp.Reset(numTask);
            var userArgs = new TtAsyncTaskStateBase.FUserArguments();
            userArgs.Obj0 = action;
            userArgs.Obj1 = smp;
            userArgs.Obj2 = userData1;
            userArgs.Obj3 = userData2;
            userArgs.Value0.Y = (uint)numTask;
            userArgs.Value0.Z = (uint)(numTask / numTaskGroup);
            if (numTask % numTaskGroup!=0)
            {
                userArgs.Value0.Z += 1;
            }
            for (int i = 0; i < numTaskGroup; i++)
            {
                userArgs.Value0.X = (uint)i;
                this.RunParallel(static (state) =>
                {
                    var action = (Delegate_ParrallelForAction)state.UserArguments.Obj0;
                    var stride = state.UserArguments.StrideOfParallelFor;
                    var start = (int)(state.UserArguments.TaskIndexOfParallelFor * stride);
                    int count = 0;
                    for (int j = 0; j < stride; j++)
                    {
                        var index = start + j;
                        if (index >= state.UserArguments.NumOfParallelFor)
                            break;
                        action(index, state);
                        count++;
                    }
                    ((TtPooledSemaphore)state.UserArguments.Obj1).Semaphore.AddNum(-count);
                    return true;
                }, in userArgs/*, smp.WaitEvent*/);
            }
            using (new Profiler.TimeScopeHelper(ScopeParrallelForWait))
            {
                smp.Wait(true);
            }
            ParallelForSmpAllocator.ReleaseObject(smp);
        }
        #endregion
        public int NumOfPool
        {
            get
            {
                return ContextPools.Length;
            }
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
            IdleThreads = new Support.TtBitset((uint)count);
            IdleThreads.Clear();
            ContextPools = new TtThreadPool[count];
            for (int i = 0; i < count; i++)
            {
                ContextPools[i] = new TtThreadPool(i);
                ContextPools[i].StartThread($"TPool{i}", null);
            }
        }
        public void StopPools()
        {
            foreach(var i in ContextPools)
            {
                i.StopThread(()=>
                {
                    i.HasWork = true;
                });
            }
        }
        #region WaitTaskFast
        internal static int mAliveThread = 0;
        internal Queue<Async.TtAsyncTaskStateBase> GlobalTasks = new Queue<Async.TtAsyncTaskStateBase>();
        internal void PushGlobalTask(Async.TtAsyncTaskStateBase t)
        {
            lock (GlobalTasks)
            {
                GlobalTasks.Enqueue(t);
            }
        }
        internal Async.TtAsyncTaskStateBase PopGlobalTask()
        {
            lock (GlobalTasks)
            {
                if (GlobalTasks.Count == 0)
                    return null;
                var e = GlobalTasks.Dequeue();
                TaskLatency(e);
                return e;
            }
        }
        internal static void TaskLatency(Async.TtAsyncTaskStateBase e)
        {
#if STATE_TIME
            var now = Support.TtTime.HighPrecision_GetTickCount();
            if (now - e.CreateTime > 10)
            {
                System.Threading.Volatile.Read(ref now);
            }
#endif
        }

        public void PushTask(Async.TtAsyncTaskStateBase e)
        {
            var thread = SelectBestThread();
            thread.HasWork = true;
#if STATE_TIME
            e.CreateTime = Support.TtTime.HighPrecision_GetTickCount();
#endif
            if (thread.LoadBalance > 2)
            {
                PushGlobalTask(e);
            }
            else
            {
                thread.PushTask(e);
            }
        }
        private TtThreadPool SelectBestThread()
        {
            TtThreadPool result = null;
            int LoadBalance = int.MaxValue;
            foreach (var t in ContextPools)
            {
                if (t.LoadBalance < LoadBalance)
                {
                    LoadBalance = t.LoadBalance;
                    result = t;
                    if (LoadBalance == 0)
                        return t;
                }
            }
            return result;
        }
        #endregion
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
        [ThreadStatic]
        private static Profiler.TimeScope mScopeRunParallel;
        private static Profiler.TimeScope ScopeRunParallel
        {
            get
            {
                if (mScopeRunParallel == null)
                    mScopeRunParallel = new Profiler.TimeScope(typeof(TtContextThreadManager), nameof(RunParallel));
                return mScopeRunParallel;
            }
        }
        public void RunParallel<T>(FPostEvent<T> evt, in TtAsyncTaskStateBase.FUserArguments userArgs, System.Threading.AutoResetEvent completedEvent = null)
        {
            using (new Profiler.TimeScopeHelper(ScopeRunParallel))
            {
                var eh = TtAsyncTaskState<T>.CreateInstance();
                eh.PostAction = evt;
                eh.ContinueThread = null;
                eh.AsyncType = EAsyncType.ParallelTasks;
                eh.UserArguments = userArgs;
                eh.CompletedEvent = completedEvent;

                if (EnableMTForeach == false || TtContextThread.CurrentContext.IsTaskPoolThread())
                {
                    eh.ExecutePostEvent();
                    eh.TaskState = Async.EAsyncTaskState.Completed;
                    eh.CompletedEvent?.Set();
                    eh.Dispose();
                }
                else
                {
                    this.PushTask(eh);
                }
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
                this.PushTask(eh);
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
            if (target == EAsyncTarget.AsyncIOAfterEmpty)
            {
                eh.AsyncType = EAsyncType.AsyncIOAfterEmpty;
            }
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
                case EAsyncType.AsyncIOAfterEmpty:
                    {
                        lock (TtEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                        {
                            TtEngine.Instance.ContextThreadManager.AsyncIOEmptys.Add(PEvent);
                        }
                        TtEngine.Instance.ThreadAsync.mEnqueueTrigger.Set();
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
                case EAsyncType.ParallelTasks:
                    {
                        TtEngine.Instance.ContextThreadManager.PushTask(PEvent);
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
