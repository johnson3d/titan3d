using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtThreadRHI : TtContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtThreadRHI), nameof(Tick));
                return mScopeTick;
            }
        }
        
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTickAwaitEvent = new Profiler.TimeScope(typeof(TtThreadRHI), nameof(TickAwaitEvent));
        public override void Tick()
        {
            TickStage = 2;

            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                ScopeTickAwaitEvent.Begin();
                this.TickAwaitEvent();
                ScopeTickAwaitEvent.End();

                TtEngine.Instance.TryTickRender();
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
