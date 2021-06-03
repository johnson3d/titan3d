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
            UEngine.Instance.ContextThreadManager.mTPoolTrigger.WaitOne(20);
            
            var events = UEngine.Instance.ContextThreadManager.TPoolEvents;
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
