using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;

namespace EngineNS.Thread.Async
{
    public interface ITask : IDisposable
    {
        bool IsCompleted { get; }
    }
    #region task<T>
    public struct AsyncFiberMethodBuilder<T>
    {
        private TtTask<T> mTask;

        #region mandatory methods for async state machine builder
        public AsyncFiberMethodBuilder()
        {
            mTask = new TtTask<T>();
        }
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
            var t = mTask.Result;
            mTask.Dispose();
            return t;
        }

        public void OnCompleted(Action continuation)
        {
            mTask.RegisterContinuation(continuation);
        }

        #endregion
    }

    public enum ETtTaskStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2
    }

    public class TtTaskData<T> : IPooledObject, IDisposable
    {
        public bool IsAlloc { get; set; } = false;
        internal ETtTaskStatus mStatus;
        internal T mResult;
        internal Action mContinuation;
        internal Exception mException;
        public void Reset()
        {
            mStatus = ETtTaskStatus.Pending;
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
            mStatus = ETtTaskStatus.Success;
            mResult = result;

            mContinuation = null;
            mException = null;
        }

        public void Init(Exception exception)
        {
            mStatus = ETtTaskStatus.Failed;
            mException = exception;

            mResult = default;
            mContinuation = null;
        }

        public void Init()
        {
            this.mStatus = ETtTaskStatus.Pending;

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
                    case ETtTaskStatus.Success:
                        return mResult;
                    case ETtTaskStatus.Failed:
                        ExceptionDispatchInfo.Capture(mException).Throw();
                        return default;
                    default:
                        throw new InvalidOperationException("TtTask didn't complete");
                }
            }
        }
        internal bool TrySetResult(T result)
        {
            if (mStatus != ETtTaskStatus.Pending)
            {
                return false;
            }
            else
            {
                mStatus = ETtTaskStatus.Success;
                this.mResult = result;
                if (this.mContinuation != null)
                    this.mContinuation();
                return true;
            }
        }

        internal bool TrySetException(Exception exception)
        {
            if (mStatus != ETtTaskStatus.Pending)
            {
                return false;
            }
            else
            {
                mStatus = ETtTaskStatus.Failed;
                this.mException = exception;
                this.mContinuation?.Invoke();
                return true;
            }
        }

        internal void RegisterContinuation(Action cont)
        {
            if (mStatus == ETtTaskStatus.Pending)
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

        public TtTask()
        {
            mTaskData = TtTaskData<T>.CreateInstance();
        }

        public T Result
        {
            get
            {
                var ret = mTaskData.Result;
                return ret;
            }
        }

        public Exception Exception 
        { 
            get => mTaskData.mException; 
            private set => mTaskData.mException = value; 
        }

        public bool IsCompleted => mTaskData.mStatus != ETtTaskStatus.Pending;
        public void Wait()
        {
            while (IsCompleted == false)
            {
                System.Threading.Thread.Sleep(0);
            }
        }

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
        internal ETtTaskStatus mStatus;
        internal Action mContinuation;
        internal Exception mException;
        public void Reset()
        {
            mStatus = ETtTaskStatus.Pending;
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

        public void Init()
        {
            mStatus = ETtTaskStatus.Pending;
            
            mContinuation = null;
            mException = null;
        }

        public void Init(Exception exception)
        {
            mStatus = ETtTaskStatus.Failed;
            mException = exception;

            mContinuation = null;
        }
        internal bool TrySetResult()
        {
            if (mStatus != ETtTaskStatus.Pending)
            {
                return false;
            }
            else
            {
                mStatus = ETtTaskStatus.Success;
                if (this.mContinuation != null)
                    this.mContinuation();
                return true;
            }
        }
        internal bool TrySetException(Exception exception)
        {
            if (mStatus != ETtTaskStatus.Pending)
            {
                return false;
            }
            else
            {
                mStatus = ETtTaskStatus.Failed;
                this.mException = exception;
                this.mContinuation?.Invoke();
                return true;
            }
        }

        internal void RegisterContinuation(Action cont)
        {
            if (mStatus == ETtTaskStatus.Pending)
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
        public AsyncFiberMethodBuilder()
        {
            mTask = new TtTask();
        }
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
    public struct TtTask : ITask, IDisposable
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
        public bool IsCompleted => mTaskData.mStatus != ETtTaskStatus.Pending;

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
