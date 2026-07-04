using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace EngineNS.Thread.Async
{
    public interface ITask : IDisposable
    {
        bool IsCompleted { get; }
        void WaitCompleted();
    }
    public class TtTaskCollector : IDisposable
    {
        public struct FWaitTask
        {
            public ITask Task;
            public FOnTaskFinished OnFinished;
            public bool IsCompletedDispose;
        }
        public List<FWaitTask> Tasks { get; } = new ();
        public delegate void FOnTaskFinished(ITask task);
        public void AddWaitTask(ITask task, FOnTaskFinished fn = null, bool bCompletedDispose = true)
        {
            lock (Tasks)
            {
                if (task.IsCompleted)
                {
                    if (fn != null)
                        fn(task);
                    if (bCompletedDispose)
                        task.Dispose();
                    return;
                }
                FWaitTask wt;
                wt.Task = task;
                wt.OnFinished = fn;
                wt.IsCompletedDispose = bCompletedDispose;
                Tasks.Add(wt);
            }
        }
        public void Tick()
        {
            lock (Tasks)
            {
                for (int i = 0; i < Tasks.Count; i++)
                {
                    if (Tasks[i].Task.IsCompleted)
                    {
                        if (Tasks[i].OnFinished != null)
                        {
                            Tasks[i].OnFinished(Tasks[i].Task);
                        }
                        if (Tasks[i].IsCompletedDispose)
                            Tasks[i].Task.Dispose();
                        Tasks.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        public void Dispose()
        {
            for (int i = 0; i < Tasks.Count; i++)
            {
                Tasks[i].Task.Dispose();
            }
            Tasks.Clear();
        }
    }

    #region task<T>
    public struct AsyncFiberMethodBuilder<T>
    {
        private TtTask<T> mTask;

        #region mandatory methods for async state machine builder
        [DebuggerNonUserCode]
        public AsyncFiberMethodBuilder()
        {
            mTask = new TtTask<T>();
        }
        [DebuggerNonUserCode]
        public static AsyncFiberMethodBuilder<T> Create()
        {
            return new AsyncFiberMethodBuilder<T>();
        }

        public TtTask<T> Task
        {
            get
            {
                return mTask;
            }
        }
        public void SetException(Exception e) => Task.TrySetException(e);

        public void SetResult(T result) => Task.TrySetResult(result);

        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
        [DebuggerNonUserCode]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            Action move = stateMachine.MoveNext;
            move();
            //ThreadPool.QueueUserWorkItem(_ =>
            //{
            //    move();
            //});
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // nothing to do
        }

        #endregion
    }

    public readonly struct TtFiberAwaiter<T> : INotifyCompletion
    {
        private readonly TtTask<T> mTask;

        public TtFiberAwaiter(TtTask<T> fiber)
        {
            this.mTask = fiber;
        }

        #region mandatory awaiter methods 

        public bool IsCompleted => mTask.IsCompleted;

        public T GetResult()
        {
            //normal:Call By Compile services at await
            //special:TtTask<T>.GetResultAndRelease()
            var t = mTask.DirectResult;
            mTask.Dispose();
            return t;
        }

        public void OnCompleted(Action continuation)
        {
            mTask.RegisterContinuation(continuation);
        }

        #endregion
    }

    public enum ETaskStatus
    {
        NotInit = 0,
        Pending,
        Success,
        Failed
    }

    public class TtTaskData<T> : IPooledObject
    {
        public bool IsAlloc { get; set; } = false;
        internal ETaskStatus mStatus = ETaskStatus.NotInit;
        internal T mResult;
        internal Action mContinuation;
        internal Exception mException;
        public void Reset()
        {
            mStatus = ETaskStatus.Pending;
            mResult = default(T);
            mContinuation = null;
            mException = null;
        }

        #region life manage
        public class TtTaskDataAllocator : TtObjectPool<TtTaskData<T>>
        {
            protected override bool OnObjectRelease(TtTaskData<T> obj)
            {
                obj.Reset();
                return true;
            }
            public override string ShowName
            {
                get => $"TtTaks<{typeof(T).FullName}>";
            }
        }
        static TtTaskDataAllocator mAllocator = new TtTaskDataAllocator();
        public static TtTaskData<T> CreateInstance(T result)
        {
            var ret = mAllocator.QueryObjectSync();
            ret.Init(result);
            return ret;
        }
        public static TtTaskData<T> CreateInstance(Exception exception)
        {
            var ret = mAllocator.QueryObjectSync();
            ret.Init(exception);
            return ret;
        }
        public static TtTaskData<T> CreateInstance()
        {
            var ret = mAllocator.QueryObjectSync();
            ret.Init();
            return ret;
        }
        public void Dispose()
        {
            mAllocator.ReleaseObject(this);
        }
        #endregion

        public void Init(T result)
        {
            mStatus = ETaskStatus.Success;
            mResult = result;

            mContinuation = null;
            mException = null;
        }

        public void Init(Exception exception)
        {
            mStatus = ETaskStatus.Failed;
            mException = exception;

            mResult = default;
            mContinuation = null;
        }

        public void Init()
        {
            this.mStatus = ETaskStatus.Pending;

            mException = null;
            mResult = default;
            mContinuation = null;
        }
        public T Result
        {
            get
            {
                switch (mStatus)
                {
                    case ETaskStatus.Success:
                        return mResult;
                    case ETaskStatus.Failed:
                        ExceptionDispatchInfo.Capture(mException).Throw();
                        return default;
                    default:
                        throw new InvalidOperationException("TtTask didn't complete");
                }
            }
        }
        internal bool TrySetResult(T result)
        {
            if (mStatus != ETaskStatus.Pending)
            {
                return false;
            }
            else
            {
                mStatus = ETaskStatus.Success;
                this.mResult = result;
                if (this.mContinuation != null)
                    this.mContinuation();
                return true;
            }
        }

        internal bool TrySetException(Exception exception)
        {
            if (mStatus != ETaskStatus.Pending)
            {
                return false;
            }
            else
            {
                mStatus = ETaskStatus.Failed;
                this.mException = exception;
                this.mContinuation?.Invoke();
                return true;
            }
        }

        internal void RegisterContinuation(Action cont)
        {
            if (mStatus == ETaskStatus.Pending)
            {
                if (this.mContinuation is null)
                {
                    this.mContinuation = cont;
                }
                else
                {
                    var prev = this.mContinuation;
                    this.mContinuation = () =>
                    {
                        prev();
                        cont();
                    };
                }
            }
            else
            {
                cont();
            }
        }
    }
    [DebuggerNonUserCode]
    [AsyncMethodBuilder(typeof(AsyncFiberMethodBuilder<>))]
    //public sealed class TtTask<T>
    public struct TtTask<T> : ITask, IDisposable
    {
        //make the members as a class type pointer;
        TtTaskData<T> mTaskData;
        public bool IsTimeout;
        public void Dispose()
        {
            mTaskData.Dispose();
            mTaskData = null;
            IsTimeout = false;
        }

        public TtTask(T result)
        {
            mTaskData = TtTaskData<T>.CreateInstance(result);
        }

        public TtTask(Exception exception)
        {
            mTaskData = TtTaskData<T>.CreateInstance(exception);
        }
        [DebuggerNonUserCode]
        public TtTask()
        {
            mTaskData = TtTaskData<T>.CreateInstance();
        }
        public void AddWaitTask(TtTaskCollector.FOnTaskFinished cb = null)
        {
            TtEngine.Instance.TaskCollector.AddWaitTask(this, cb);
        }
        public T GetResultUntilCompleted()
        {
            WaitCompleted();
            return this.UnsafeGetResultAndRelease();
        }
        public void WaitCompleted()
        {
            TtContextThread.CurrentContext.WaitTask(this);
        }
        public T DirectResult
        {
            get
            {
                var ret = mTaskData.Result;
                return ret;
            }
        }

        public T UnsafeGetResultAndRelease()
        {
            return GetAwaiter().GetResult();
        }

        public Exception Exception 
        { 
            get => mTaskData.mException; 
            private set => mTaskData.mException = value; 
        }
        public ETaskStatus TaskState
        {
            get
            {
                if (mTaskData == null)
                    return ETaskStatus.NotInit;
                return mTaskData.mStatus;
            }
        }
        public bool IsCompleted => mTaskData.mStatus != ETaskStatus.Pending;
        
        public TtFiberAwaiter<T> GetAwaiter()
        {
            return new TtFiberAwaiter<T>(this);
        }

        internal bool TrySetResult(T result)
        {
            return mTaskData.TrySetResult(result);
        }

        internal bool TrySetException(Exception exception)
        {
            return mTaskData.TrySetException(exception);
        }

        internal void RegisterContinuation(Action cont)
        {
            mTaskData.RegisterContinuation(cont);
        }
    }
    #endregion

    #region task<void>
    public readonly struct TtFiberAwaiter : INotifyCompletion
    {
        private readonly TtTask mTask;

        public TtFiberAwaiter(TtTask fiber)
        {
            this.mTask = fiber;
        }

        #region mandatory awaiter methods 

        public bool IsCompleted => mTask.IsCompleted;
        public void SetResult()
        {
            mTask.TrySetResult();
        }
        public void GetResult()
        {
            mTask.Dispose();
        }
        public void OnCompleted(Action continuation)
        {
            mTask.RegisterContinuation(continuation);
        }

        #endregion
    }
    public class TtTaskData : IPooledObject, IDisposable
    {
        public bool IsAlloc { get; set; } = false;
        internal ETaskStatus mStatus;
        internal Action mContinuation;
        internal Exception mException;
        public void Reset()
        {
            mStatus = ETaskStatus.Pending;
            mContinuation = null;
            mException = null;
        }

        #region life manage
        public class TtTaskDataAllocator : TtObjectPool<TtTaskData>
        {
            protected override bool OnObjectRelease(TtTaskData obj)
            {
                obj.Reset();
                return true;
            }
        }
        static TtTaskDataAllocator mAllocator = new TtTaskDataAllocator();
        public static TtTaskData CreateInstance()
        {
            var ret = mAllocator.QueryObjectSync();
            ret.Init();
            return ret;
        }
        public static TtTaskData CreateInstance(Exception exception)
        {
            var ret = mAllocator.QueryObjectSync();
            ret.Init(exception);
            return ret;
        }
        public void Dispose()
        {
            mAllocator.ReleaseObject(this);
        }
        #endregion

        private void Init()
        {
            mStatus = ETaskStatus.Pending;
            
            mContinuation = null;
            mException = null;
        }

        private void Init(Exception exception)
        {
            mStatus = ETaskStatus.Failed;
            mException = exception;

            mContinuation = null;
        }
        internal bool TrySetResult()
        {
            if (mStatus != ETaskStatus.Pending)
            {
                return false;
            }
            else
            {
                mStatus = ETaskStatus.Success;
                if (this.mContinuation != null)
                    this.mContinuation();
                return true;
            }
        }
        internal bool TrySetException(Exception exception)
        {
            if (mStatus != ETaskStatus.Pending)
            {
                return false;
            }
            else
            {
                mStatus = ETaskStatus.Failed;
                this.mException = exception;
                this.mContinuation?.Invoke();
                return true;
            }
        }

        internal void RegisterContinuation(Action cont)
        {
            if (mStatus == ETaskStatus.Pending)
            {
                if (this.mContinuation is null)
                {
                    this.mContinuation = cont;
                }
                else
                {
                    var prev = this.mContinuation;
                    this.mContinuation = () =>
                    {
                        prev();
                        cont();
                    };
                }
            }
            else
            {
                cont();
            }
        }
    }
    public struct AsyncFiberMethodBuilder
    {
        private TtTask mTask;

        #region mandatory methods for async state machine builder
        [DebuggerNonUserCode]
        public AsyncFiberMethodBuilder()
        {
            mTask = new TtTask();
        }
        [DebuggerNonUserCode]
        public static AsyncFiberMethodBuilder Create()
        {
            return new AsyncFiberMethodBuilder();
        }

        public TtTask Task
        {
            get
            {
                return mTask;
            }
        }
        public void SetException(Exception e) => Task.TrySetException(e);
        public void SetResult()
        {
            Task.TrySetResult();
        }
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
        [DebuggerNonUserCode]
        //[DebuggerStepThrough]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            Action move = stateMachine.MoveNext;
            move();
            //ThreadPool.QueueUserWorkItem(_ =>
            //{
            //    move();
            //});
        }

        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            // nothing to do
        }

        #endregion
    }
    [AsyncMethodBuilder(typeof(AsyncFiberMethodBuilder))]
    public struct TtTask : ITask
    {
        //make the members as a class type pointer;
        TtTaskData mTaskData;

        public void Dispose()
        {
            mTaskData.Dispose();
            mTaskData = null;
        }
        public TtTask(Exception exception)
        {
            mTaskData = TtTaskData.CreateInstance(exception);
        }
        public TtTask()
        {
            mTaskData = TtTaskData.CreateInstance();
        }
        public Exception Exception
        {
            get => mTaskData.mException;
            private set => mTaskData.mException = value;
        }
        public bool IsCompleted => mTaskData.mStatus != ETaskStatus.Pending;
        public void WaitCompleted()
        {
            TtContextThread.CurrentContext.WaitTask(this);
        }
        public void WaitCompletedAndDispose()
        {
            WaitCompleted();
            Dispose();
        }

        public void AddWaitTask(TtTaskCollector.FOnTaskFinished cb = null)
        {
            TtEngine.Instance.TaskCollector.AddWaitTask(this, cb);
        }

        public TtFiberAwaiter GetAwaiter()
        {
            return new TtFiberAwaiter(this);
        }
        internal bool TrySetResult()
        {
            return mTaskData.TrySetResult();
        }
        internal bool TrySetException(Exception exception)
        {
            return mTaskData.TrySetException(exception);
        }
        internal void RegisterContinuation(Action cont)
        {
            mTaskData.RegisterContinuation(cont);
        }
    }
    #endregion
}

namespace EngineNS
{
    public partial class TtEngine
    {
        public Thread.Async.TtTaskCollector TaskCollector { get; } = new Thread.Async.TtTaskCollector();
    }
}