using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtThreadPool : TtContextThread
    {
        private static int mNumOfActiveThreads = 0;
        public static int NumOfActiveThreads { get => mNumOfActiveThreads; }
        private static int mMaxActiveThreads = 0;
        internal static int MaxActiveThreads
        {
            get
            {
                return mMaxActiveThreads;
            }
        }
        internal static void ResetMaxActiveThreads()
        {
            mMaxActiveThreads = 0;
        }
        private static object mLocker = "lockObject";
        public TtThreadPool()
        {
            Interval = 0;
        }
        //Queue<Async.IJobThread> mJobThreads = new Queue<Async.IJobThread>();
        //public void AddJobThread(Async.IJobThread job)
        //{
        //    lock (mJobThreads)
        //    {
        //        mJobThreads.Enqueue(job);
        //    }
        //}
        //private void TickJobThread()
        //{
        //    while (mJobThreads.Count > 0)
        //    {
        //        Async.IJobThread jobs = null;
        //        lock (mJobThreads)
        //        {
        //            jobs = mJobThreads.Dequeue();
        //        }
        //        jobs.DoWorks();
        //    }
        //}
        private List<Async.TtAsyncTaskStateBase> Suspended = new List<Async.TtAsyncTaskStateBase>();
        public override void Tick()
        {
            TtEngine.Instance.ContextThreadManager.mTPoolTrigger.WaitOne();
            var e = TtEngine.Instance.ContextThreadManager.PopPoolEvent();
            if (e == null)
            {
                return;
            }

            System.Threading.Interlocked.Increment(ref mNumOfActiveThreads);
#if DEBUG
            lock (mLocker)
            {
                if (mNumOfActiveThreads > mMaxActiveThreads)
                    mMaxActiveThreads = mNumOfActiveThreads;
            }
#endif

            while (e != null)
            {
                //TickJobThread();

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
                }
                e = TtEngine.Instance.ContextThreadManager.PopPoolEvent();
            }

            foreach(var i in Suspended)
            {
                TtEngine.Instance.ContextThreadManager.PushPoolEvent(i);
            }
            Suspended.Clear();

            //TickAwaitEvent();
            System.Threading.Interlocked.Decrement(ref mNumOfActiveThreads);
        }
        protected override void OnThreadStart()
        {
            this.LimitTime = long.MaxValue;
        }
        protected override void OnThreadExited()
        {

        }
    }
}
