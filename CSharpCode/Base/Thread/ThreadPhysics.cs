using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadPhysics : ContextThread
    {
        public ThreadPhysics()
        {
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
                mPrevFrameCount = UEngine.Instance.FrameCount;
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
