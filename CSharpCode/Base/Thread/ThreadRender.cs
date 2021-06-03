using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadRender : ContextThread
    {
        public ThreadRender()
        {
            Interval = 0;
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadRender), nameof(Tick));
        public override void Tick()
        {
            mRenderBegin.WaitOne();
            mRenderBegin.Reset();

            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                this.TickAwaitEvent();

                try
                {
                    if (RenderAction != null)
                    {
                        RenderAction();
                        RenderAction = null;
                    }
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }

            mRenderEnd.Set();
        }
        public System.Threading.AutoResetEvent mRenderBegin = new System.Threading.AutoResetEvent(false);
        public System.Threading.AutoResetEvent mRenderEnd = new System.Threading.AutoResetEvent(false);
        System.Action RenderAction;
        public void PostRenderAction(System.Action action)
        {
            action();
            if (this.IsFinished)
            {
                return;
            }
            RenderAction = action;
            mRenderBegin.Set();
        }
        [ThreadStatic]
        private static Profiler.TimeScope ScopeWaitRender = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadRender), nameof(WaitRender));
        public void WaitRender()
        {
            using (new Profiler.TimeScopeHelper(ScopeWaitRender))
            {
                mRenderEnd.WaitOne();
                mRenderEnd.Reset();
            }
        }
        public override bool StartThread(string name, FOnThreadTick action)
        {
            mRenderBegin.Reset();
            mRenderEnd.Reset();
            return base.StartThread(name, action);
        }
        public override void StopThread(Action waitAction)
        {
            base.StopThread(() =>
            {
                mRenderBegin.Set();
                //CEngine.Instance.StartFrame();
            });
            //mRenderEnd.Set();
        }
    }
}
