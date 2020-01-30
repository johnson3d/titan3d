using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadRHI : ContextThread
    {
        public ThreadRHI()
        {
            Async.ContextThreadManager.Instance.ContextRHI = this;
        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadRHI), nameof(Tick));
        public static Profiler.TimeScope ScopeTickAwaitEvent = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadRHI), nameof(TickAwaitEvent));
        public override void Tick()
        {
            TickFlag = 2;

            ScopeTick.Begin();

            ScopeTickAwaitEvent.Begin();
            this.TickAwaitEvent();
            ScopeTickAwaitEvent.End();

            CEngine.Instance.TryTickRender();

            ScopeTick.End();
            TickFlag = 0;
        }
        protected override void OnThreadStart()
        {
            Thread_StartRHIThread();
        }
        protected override void OnThreadExited()
        {

        }
    }
}
