using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtThreadRHI : TtContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(TtThreadRHI), nameof(Tick));
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTickAwaitEvent = Profiler.TimeScopeManager.GetTimeScope(typeof(TtThreadRHI), nameof(TickAwaitEvent));
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
