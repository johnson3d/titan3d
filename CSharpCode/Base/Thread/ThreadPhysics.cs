using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtThreadPhysics : TtContextThread
    {
        public TtThreadPhysics()
        {
            this.LimitTime = long.MaxValue;
            Interval = 0;
        }
        long mPrevFrameCount;
        long mPrevTestTime = 0;
        public override void Tick()
        {
            mEnqueueTrigger.WaitOne();
            var now = Support.TtTime.GetTickCount();
            if(now - mPrevTestTime>100)
            {
                mPrevFrameCount = TtEngine.Instance.FrameCount;
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
