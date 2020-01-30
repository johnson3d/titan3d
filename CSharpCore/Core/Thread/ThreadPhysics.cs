using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadPhysics : ContextThread
    {
        public ThreadPhysics()
        {
            Async.ContextThreadManager.Instance.ContextPhysics = this;
            this.LimitTime = long.MaxValue;
            Interval = 15;
        }
        long mPrevFrameCount;
        long mPrevTestTime = 0;
        public override void Tick()
        {
            var now = Support.Time.GetTickCount();
            if(now - mPrevTestTime>100)
            {
                if(CEngine.Instance.FrameCount == mPrevFrameCount)
                {
                    int xxx = 0;
                    xxx++;
                }
                mPrevFrameCount = CEngine.Instance.FrameCount;
                mPrevTestTime = now;
            }
            
            //把异步事件做完
            this.TickAwaitEvent();

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
