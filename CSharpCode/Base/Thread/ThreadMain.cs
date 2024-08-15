using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtThreadMain : TtContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTickSync = Profiler.TimeScopeManager.GetTimeScope(typeof(TtThreadMain), nameof(TickSync));
        [ThreadStatic]
        private static Profiler.TimeScope ScopeWaitTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(TtThreadMain), "WaitTickLogic");
        public override void Tick()
        {
            BeforeFrame();

            RenderMT();

            TickAwaitEvent();
        }
        private void BeforeFrame()
        {
            //TtEngine.Instance.BeforeFrame();
        }
        private void TickSync()
        {
            using(new Profiler.TimeScopeHelper(ScopeTickSync))
            {
                TickStage = 1;
#if PWindow
                var saved = System.Threading.SynchronizationContext.Current;
                System.Threading.SynchronizationContext.SetSynchronizationContext(null);
                this.TickAwaitEvent();
                System.Threading.SynchronizationContext.SetSynchronizationContext(saved);
#else
                this.TickAwaitEvent();
#endif

                TtEngine.Instance.TickSync();

                TickStage = 0;
            }
        }
        private void RenderMT()
        {
            TtEngine.Instance.ThreadLogic.LogicEnd.Reset();
            TtEngine.Instance.ThreadLogic.LogicBegin.Set();

            TtEngine.Instance.ThreadRHI.Tick();

            using (new Profiler.TimeScopeHelper(ScopeWaitTickLogic))
            {
                var evtIndex = System.Threading.WaitHandle.WaitAny(TtEngine.Instance.ThreadLogic.LogicEndEvents);
                if (evtIndex == (int)TtThreadLogic.EEndEvent.MacrossDebug)
                {
                    System.Diagnostics.Debug.Assert(Macross.UMacrossDebugger.Instance.CurrrentBreak != null);
                }
                //TtEngine.Instance.ThreadLogic.mLogicEnd.WaitOne();
                TtEngine.Instance.ThreadLogic.LogicEnd.Reset();
            }
        }
    }
}
