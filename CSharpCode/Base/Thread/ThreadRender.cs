using System;
using System.Collections.Generic;
using System.Text;

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
                    while (true)
                    {
                        FRenderAction action;
                        if (RenderActions.Count > 0)
                        {
                            lock (RenderActions)
                            {
                                action = RenderActions.Peek();
                                RenderActions.Dequeue();
                            }
                            if (action.Action != null)
                            {
                                action.Action();
                            }
                            if (action.Name == "##FrameFinished##")
                            {
                                break;
                            }
                        }
                        else if (mIsRun == false)
                        {
                            break;
                        }
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
        public struct FRenderAction
        {
            public string Name;
            public System.Action Action;
        }
        Queue<FRenderAction> RenderActions = new Queue<FRenderAction>();
        public void PostRenderAction(string name, System.Action action)
        {
            if (this.IsFinished)
            {
                return;
            }
            lock (RenderActions)
            {
                FRenderAction RAct;
                RAct.Name = name;
                RAct.Action = action;
                RenderActions.Enqueue(RAct);
            }
            mRenderBegin.Set();
        }
        public void FinishRenderAction()
        {
            if (this.IsFinished)
            {
                return;
            }
            lock (RenderActions)
            {
                FRenderAction RAct;
                RAct.Name = "##FrameFinished##";
                RAct.Action = null;
                RenderActions.Enqueue(RAct);
            }
        }
        [ThreadStatic]
        private static Profiler.TimeScope mScopeWaitRender;
        private static Profiler.TimeScope ScopeWaitRender
        {
            get
            {
                if (mScopeWaitRender == null)
                    mScopeWaitRender = new Profiler.TimeScope(typeof(TtThreadRender), nameof(WaitRender));
                return mScopeWaitRender;
            }
        }
        
        public void WaitRender()
        {
            using (new Profiler.TimeScopeHelper(ScopeWaitRender))
            {
                mRenderEnd.WaitOne();
                mRenderEnd.Reset();
            }
        }
        public override bool StartThread(string name, FOnThreadTick action, int stackSize = -1)
        {
            mRenderBegin.Reset();
            mRenderEnd.Reset();
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
