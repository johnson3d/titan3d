using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.Thread.TtThreadPool;

namespace EngineNS.Thread
{
    public class TtThreadPool : TtContextThread
    {
        public override Async.EAsyncTarget GetThreadType()
        {
            return Async.EAsyncTarget.TPools;
        }
        public override bool IsTaskPoolThread() { return true; }
        public int PoolIndex { get; private set; } = -1;
        public TtThreadPool(int index)
        {
            PoolIndex = index;
            Interval = 0;
        }
        private List<Async.TtAsyncTaskStateBase> Suspended = new List<Async.TtAsyncTaskStateBase>();
        #region WaitTaskFast
        public int LoadBalance
        {
            get
            {
                return PrivateTasks.Count + ((mPoolThreadState == EPoolThreadState.Wait) ? 0 : 1);
            }
        }
        public void PushTask(Async.TtAsyncTaskStateBase e)
        {
            lock (PrivateTasks)
            {
                PrivateTasks.Enqueue(e);
            }
        }
        public Async.TtAsyncTaskStateBase PopTask()
        {
            lock (PrivateTasks)
            {
                if (PrivateTasks.Count == 0)
                    return null;
                var e = PrivateTasks.Dequeue();
                Async.TtContextThreadManager.TaskLatency(e);
                return e;
            }
        }
        internal Queue<Async.TtAsyncTaskStateBase> PrivateTasks = new Queue<Async.TtAsyncTaskStateBase>();
        internal System.Threading.ManualResetEventSlim Trigger = new System.Threading.ManualResetEventSlim(false);
        
        int mHasWork;
        public int NumPrivateTasks
        {
            get { return PrivateTasks.Count; }
        }
        public bool HasWork
        {
            get
            {
                if (TtEngine.Instance.EventPoster.IsInParallelFor)
                    return true;
                if (PrivateTasks.Count > 0 || TtEngine.Instance.EventPoster.GlobalTasks.Count > 0)
                    return true;
                return System.Threading.Interlocked.CompareExchange(ref mHasWork, 1, 1) == 1;
            }
            set
            {
                System.Threading.Interlocked.Exchange(ref mHasWork, value ? 1 : 0);
                Trigger.Set();
            }
        }
        public enum EPoolThreadState
        {
            DoPrivate,
            DoGlobal,
            DoTakeOther,
            Wait,
        }
        EPoolThreadState mPoolThreadState = EPoolThreadState.Wait;
        public EPoolThreadState PoolThreadState
        {
            get { return mPoolThreadState; }
        }
        public override void Tick()
        {
            ref var alive = ref Async.TtContextThreadManager.mAliveThread;
            System.Threading.Interlocked.Increment(ref alive);
            mPoolThreadState = EPoolThreadState.DoPrivate;
            Async.TtAsyncTaskStateBase e = PopTask();
            while (e != null)
            {   
                ExecutePostEvent(e);
                e = PopTask();
            }

            mPoolThreadState = EPoolThreadState.DoGlobal;
            e = TtEngine.Instance.ContextThreadManager.PopGlobalTask();
            while (e != null)
            {
                ExecutePostEvent(e);
                e = TtEngine.Instance.ContextThreadManager.PopGlobalTask();
            }

            mPoolThreadState = EPoolThreadState.DoTakeOther;
            e = TakeFromOtherThreads();
            while (e != null)
            {
                ExecutePostEvent(e);
                e = TakeFromOtherThreads();
            }

            foreach (var i in Suspended)
            {
                //PushTask(i);
                TtEngine.Instance.ContextThreadManager.PushGlobalTask(i);
            }
            Suspended.Clear();

            WaitTask();
        }
        internal Async.TtAsyncTaskStateBase ExecutePostEvent(Async.TtAsyncTaskStateBase e)
        {
            Async.TtAsyncTaskStateBase state = null;
            try
            {
                state = e.ExecutePostEvent();
            }
            catch (Exception ex)
            {
                Profiler.Log.WriteException(ex);
                e.ExceptionInfo = ex;
            }
            if (state.TaskState == Async.EAsyncTaskState.Suspended)
            {
                Suspended.Add(e);
            }
            else
            {
                e.TaskState = Async.EAsyncTaskState.Completed;
                e.CompletedEvent?.Set();
                e.Dispose();
            }
            return state;
        }
        Async.TtAsyncTaskStateBase TakeFromOtherThreads()
        {
            foreach (var i in TtEngine.Instance.EventPoster.ContextPools)
            {
                if (i == null)
                    continue;
                var e = i.PopTask();
                if (e != null)
                    return e;
            }
            return null;
        }
        const int SpinTimes = 20;
        const int YieldTimes = 40;
        const int SleepTimes = int.MaxValue;
        const int TriggerTimeout = 3;
        public void WaitTask()
        {
            HasWork = false;
            mPoolThreadState = EPoolThreadState.Wait;
            ref var alive = ref Async.TtContextThreadManager.mAliveThread;
            System.Threading.Interlocked.Decrement(ref alive);
            while (true)
            {
                //CPU自旋
                for (int i = 0; i < SpinTimes; i++)
                {
                    if (HasWork)
                        return;
                    System.Threading.Volatile.Read(ref i);
                }
                //尝试主动交出线程
                int yieldCount = 0;
                for (int i = 0; i < YieldTimes; i++)
                {
                    if (HasWork)
                        return;
                    if (System.Threading.Thread.Yield())
                        yieldCount++;
                    else
                        System.Threading.Volatile.Read(ref i);
                }
                for (int i = 0; i < SleepTimes; i++)
                {
                    if (HasWork)
                        return;
                    if (Trigger.Wait(TriggerTimeout))
                        Trigger.Reset();
                }
            }
        }
        #endregion
        protected override void OnThreadStart()
        {
            this.LimitTime = long.MaxValue;
        }
        protected override void OnThreadExited()
        {

        }
    }
}
