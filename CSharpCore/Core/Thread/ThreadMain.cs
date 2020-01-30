using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadMain : ContextThread
    {
        public ThreadMain()
        {
            Async.ContextThreadManager.Instance.ContextMain = this;
        }
        public static Profiler.TimeScope ScopeTickSync = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadMain), nameof(TickSync));
        public static Profiler.TimeScope ScopeWaitTickLogic = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadMain), "WaitTickLogic");
        public override void Tick()
        {
            //if (CEngine.Instance.PauseGameTick)
            if(CEngine.Instance.ThreadLogic.IsTicking)
                return;

            BeforeFrame();

            if (CEngine.Instance.Desc.RenderMT == false)
            {
                CEngine.Instance.TryTickLogic();
                CEngine.Instance.TryTickRender();
            }
            else
            {
                RenderMT();
            }

            if(CEngine.Instance.ThreadLogic.IsTicking==false)
                TickSync();
        }
        private void BeforeFrame()
        {
            CEngine.Instance.BeforeFrame();
        }
        private void TickSync()
        {
            ScopeTickSync.Begin();

            TickFlag = 1;
#if PWindow
            var saved = System.Threading.SynchronizationContext.Current;
            System.Threading.SynchronizationContext.SetSynchronizationContext(null);
            this.TickAwaitEvent();
            System.Threading.SynchronizationContext.SetSynchronizationContext(saved);
#else
            this.TickAwaitEvent();
#endif

            CEngine.Instance.TickSync();
            Profiler.TimeScopeManager.Instance.Tick();

            TickFlag = 0;

            ScopeTickSync.End();
        }
        public bool WaitResumeOk = true;
        private void RenderMT()
        {
            if (WaitResumeOk)
            {
                CEngine.Instance.ThreadLogic.mLogicEnd.Reset();
                CEngine.Instance.ThreadLogic.mLogicBegin.Set();

                CEngine.Instance.ThreadRHI.Tick();
            }
            ScopeWaitTickLogic.Begin();
            CEngine.Instance.ThreadLogic.mLogicEnd.WaitOne();
            CEngine.Instance.ThreadLogic.mLogicEnd.Reset();
            WaitResumeOk = true;
            ScopeWaitTickLogic.End();
        }
    }
}
