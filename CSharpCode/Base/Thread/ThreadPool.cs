using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadPool : ContextThread
    {
        public ThreadPool()
        {
            Interval = 0;
        }
        Queue<Async.IJobThread> mJobThreads = new Queue<Async.IJobThread>();
        public void AddJobThread(Async.IJobThread job)
        {
            lock (mJobThreads)
            {
                mJobThreads.Enqueue(job);
            }
        }
        private void TickJobThread()
        {
            while (mJobThreads.Count > 0)
            {
                Async.IJobThread jobs = null;
                lock (mJobThreads)
                {
                    jobs = mJobThreads.Dequeue();
                }
                jobs.DoWorks();
            }
        }
        public override void Tick()
        {
            UEngine.Instance.ContextThreadManager.mTPoolTrigger.WaitOne(5);
            
            var events = UEngine.Instance.ContextThreadManager.TPoolEvents;
            while(true)
            {
                TickJobThread();

                Async.PostEvent e = null;
                lock (events)
                {
                    if (events.Count > 0)
                    {
                        e = events.Dequeue();
                    }
                    else
                    {
                        break;
                    }
                }
                if (e != null)
                {
                    try
                    {
                        e.PostAction();
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        e.ExceptionInfo = ex;
                    }
                    UEngine.Instance.EventPoster.mRunOnPEAllocator.ReleaseObject(e);
                }
            }
            UEngine.Instance.ContextThreadManager.mTPoolTrigger.Reset();

            TickAwaitEvent();
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
