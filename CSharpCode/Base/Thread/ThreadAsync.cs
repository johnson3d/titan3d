using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Thread
{
    public class TtThreadAsync : TtContextThread
    {
        public TtThreadAsync()
        {
            Interval = 0;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick 
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtThreadAsync), nameof(Tick));
                return mScopeTick;
            }
        }
        public override void Tick()
        {
            mEnqueueTrigger.WaitOne(5);
            //把异步事件做完
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                this.TickAwaitEvent();
            }

            if (AsyncEvents.Count + ContinueEvents.Count == 0)
            {
                lock (TtEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                {
                    foreach (var i in TtEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                    {
                        i.ExecutePostEvent();
                        //i.ExecuteContinue();
                        i.ContinueThread.EnqueueContinue(i);
                    }
                }
            }

            base.Tick();
        }
        protected override void OnThreadStart()
        {
            
        }
        protected override void OnThreadExited()
        {

        }
    }
}
