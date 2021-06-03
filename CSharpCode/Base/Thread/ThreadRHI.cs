using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadRHI : ContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadRHI), nameof(Tick), "RHITick");
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTickAwaitEvent = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadRHI), nameof(TickAwaitEvent));
        public override void Tick()
        {
            TickStage = 2;

            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                ScopeTickAwaitEvent.Begin();
                this.TickAwaitEvent();
                ScopeTickAwaitEvent.End();

                UEngine.Instance.TryTickRender();
            }
            TickStage = 0;
        }
        protected override void OnThreadStart()
        {
            
        }
        protected override void OnThreadExited()
        {

        }
    }
}
