using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadAsyncForEditor : ContextThread
    {
        public ThreadAsyncForEditor()
        {
            this.Interval = 10;
            this.LimitTime = long.MaxValue;
        }
        public override void Tick()
        {
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

    public class ThreadAsyncEditorSlow : ContextThread
    {
        public ThreadAsyncEditorSlow()
        {
            this.Interval = 10;
            this.LimitTime = long.MaxValue;
        }
        public override void Tick()
        {
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
