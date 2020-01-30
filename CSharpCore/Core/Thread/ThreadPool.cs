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
        public override void Tick()
        {
            Async.ContextThreadManager.Instance.mTPoolTrigger.WaitOne(20);
            
            var events = Async.ContextThreadManager.Instance.TPoolEvents;
            while(true)
            {
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
                    CEngine.Instance.EventPoster.mRunOnPEAllocator.ReleaseObject(e);
                }
            }
            Async.ContextThreadManager.Instance.mTPoolTrigger.Reset();

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
