using System;
using System.Collections.Generic;
using System.Text;
using static EngineNS.Thread.TtThreadRender.FRenderAction;

namespace EngineNS.Thread
{
    public class TtThreadRender : TtContextThread
    {
        public override Async.EAsyncTarget GetThreadType()
        {
            return Async.EAsyncTarget.Render;
        }
        public TtThreadRender()
        {
            Interval = 0;
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtThreadRender), nameof(Tick));
                return mScopeTick;
            }
        }
        public override void Tick()
        {
            mRenderBegin.WaitOne();
            mRenderBegin.Reset();

            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                this.TickAwaitEvent();

                try
                {
                    while (RenderActions.Count > 0 && mIsRun)
                    {
                        FRenderAction action;
                        lock (RenderActions)
                        {
                            action = RenderActions.Peek();
                            RenderActions.Dequeue();
                        }
                        if (action.Action != null)
                        {
                            action.Action(in action);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                }
            }
        }
        public System.Threading.AutoResetEvent mRenderBegin = new System.Threading.AutoResetEvent(false);
        public struct FRenderAction
        {
            public string Name;
            public delegate void FRenderActionDelegate(in FRenderAction RAct);
            public FRenderActionDelegate Action;
            public object Arg;
        }
        Queue<FRenderAction> RenderActions = new Queue<FRenderAction>();
        public void QueueRenderAction(string name, FRenderActionDelegate action, object arg)
        {
            if (TtEngine.Instance.Config.UseRenderThread == false)
            {
                FRenderAction RAct;
                RAct.Name = name;
                RAct.Action = action;
                RAct.Arg = arg;
                RAct.Action(in RAct);
                return;
            }
            if (this.IsFinished)
            {
                return;
            }
            lock (RenderActions)
            {
                FRenderAction RAct;
                RAct.Name = name;
                RAct.Action = action;
                RAct.Arg = arg;
                RenderActions.Enqueue(RAct);
            }
            mRenderBegin.Set();
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeWaitRender;
        private static Profiler.TimeScope ScopeWaitRender
        {
            get
            {
                if (mScopeWaitRender == null)
                    mScopeWaitRender = new Profiler.TimeScope(typeof(TtThreadRender), nameof(WaitFinishRenderAction));
                return mScopeWaitRender;
            }
        }
        public void WaitFinishRenderAction(System.Threading.AutoResetEvent finishedEvent)
        {
            if (this.IsFinished)
            {
                return;
            }
            using (new Profiler.TimeScopeHelper(ScopeWaitRender))
            {
                finishedEvent.Reset();

                lock (RenderActions)
                {
                    FRenderAction RAct;
                    RAct.Name = "##FrameFinished##";
                    RAct.Action = static (in FRenderAction RAct) =>
                    {
                        (RAct.Arg as System.Threading.AutoResetEvent).Set();
                    };
                    RAct.Arg = finishedEvent;
                    RenderActions.Enqueue(RAct);
                    mRenderBegin.Set();
                }
                finishedEvent.WaitOne();
            }
        }
        public override bool StartThread(string name, FOnThreadTick action, int stackSize = -1)
        {
            mRenderBegin.Reset();
            return base.StartThread(name, action, stackSize);
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
