using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class TtThreadMain : TtContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTickSync;
        private static Profiler.TimeScope ScopeTickSync
        {
            get
            {
                if (mScopeTickSync == null)
                    mScopeTickSync = new Profiler.TimeScope(typeof(TtThreadMain), nameof(TickSync));
                return mScopeTickSync;
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeWaitTickLogic;
        private static Profiler.TimeScope ScopeWaitTickLogic
        {
            get
            {
                if (mScopeWaitTickLogic == null)
                    mScopeWaitTickLogic = new Profiler.TimeScope(typeof(TtThreadMain), "WaitTickLogic");
                return mScopeWaitTickLogic;
            }
        } 
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
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTickRenderMT;
        private static Profiler.TimeScope ScopeTickRenderMT
        {
            get
            {
                if (mScopeTickRenderMT == null)
                    mScopeTickRenderMT = new Profiler.TimeScope(typeof(TtThreadMain), nameof(RenderMT));
                return mScopeTickRenderMT;
            }
        }
        private void RenderMT()
        {
            using (new Profiler.TimeScopeHelper(ScopeTickRenderMT))
            {
                TtEngine.Instance.ThreadLogic.LogicEnd.Reset();
                TtEngine.Instance.ThreadLogic.LogicBegin.Set();

                TtEngine.Instance.ThreadRHI.Tick();

                using (new Profiler.TimeScopeHelper(ScopeWaitTickLogic))
                {
                    var evtIndex = System.Threading.WaitHandle.WaitAny(TtEngine.Instance.ThreadLogic.LogicEndEvents);
                    if (evtIndex == (int)TtThreadLogic.EEndEvent.MacrossDebug)
                    {
                        System.Diagnostics.Debug.Assert(Macross.TtMacrossDebugger.Instance.CurrrentBreak != null);
                    }
                    //TtEngine.Instance.ThreadLogic.mLogicEnd.WaitOne();
                    TtEngine.Instance.ThreadLogic.LogicEnd.Reset();
                }
            }   
        }
    }
}
