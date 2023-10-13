using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtThreadPool : TtContextThread
    {
        public TtThreadPool()
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
            var numOfEvent = events.Count;
            while (true)
            {
                TickJobThread();

                Async.TtAsyncTaskStateBase e = null;
                
                lock (events)
                {
                    if (events.Count > 0 && numOfEvent > 0)
                    {
                        e = events.Dequeue();
                        numOfEvent--;
                    }
                    else
                    {
                        UEngine.Instance.ContextThreadManager.mTPoolTrigger.Reset();
                        break;
                    }
                }
                if (e != null)
                {
                    try
                    {
                        var state = e.ExecutePostEvent();
                        if (state.TaskState == Async.EAsyncTaskState.Suspended)
                        {
                            lock (events)
                            {
                                events.Enqueue(e);
                            }
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                        e.ExceptionInfo = ex;
                    }
                    e.TaskState = Async.EAsyncTaskState.Completed;
                    e.CompletedEvent?.Set();
                    e.Dispose();
                }
            }

            //lock (events)
            //{
            //    if (events.Count == 0)
            //        UEngine.Instance.ContextThreadManager.mTPoolTrigger.Reset();
            //}
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
