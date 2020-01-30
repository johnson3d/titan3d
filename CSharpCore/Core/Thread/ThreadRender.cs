using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Thread
{
    public class ThreadRender : ContextThread
    {
        public ThreadRender()
        {
            Async.ContextThreadManager.Instance.ContextRender = this;
            Interval = 0;
        }
        List<GamePlay.Component.GComponent> TickBeforeRenderCommitComps = new List<GamePlay.Component.GComponent>();
        public void RegBeforeRenderCommitComp(GamePlay.Component.GComponent comp)
        {
            TickBeforeRenderCommitComps.Add(comp);
        }
        public static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadRender), nameof(Tick));
        public override void Tick()
        {
            mRenderBegin.WaitOne();
            mRenderBegin.Reset();

            ScopeTick.Begin();

            this.TickAwaitEvent();

            try
            {
                for (int i = 0; i < TickBeforeRenderCommitComps.Count; i++)
                {
                    var comp = TickBeforeRenderCommitComps[i];
                    comp.Tick(comp.Host.Placement);
                }
                TickBeforeRenderCommitComps.Clear();
                if (RenderAction != null)
                {
                    RenderAction();
                    RenderAction = null;
                }
            }
            catch(Exception ex)
            {
                Profiler.Log.WriteException(ex);
            }

            ScopeTick.End();

            mRenderEnd.Set();
        }
        public System.Threading.AutoResetEvent mRenderBegin = new System.Threading.AutoResetEvent(false);
        public System.Threading.AutoResetEvent mRenderEnd = new System.Threading.AutoResetEvent(false);
        System.Action RenderAction;
        public void PostRenderAction(System.Action action)
        {
            if (CEngine.Instance.Desc.RenderMT == false)
                action();
            if (this.IsFinished)
            {
                return;
            }
            RenderAction = action;
            mRenderBegin.Set();
        }
        public static Profiler.TimeScope ScopeWaitRender = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadRender), nameof(WaitRender));
        public void WaitRender()
        {
            ScopeWaitRender.Begin();
            mRenderEnd.WaitOne();
            mRenderEnd.Reset();
            ScopeWaitRender.End();
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
