using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Thread
{
    public class TtThreadAsync : TtContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick 
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtThreadAsync), nameof(Tick));
                return mScopeTick;
            }
        }
        public override void Tick()
        {
            //把异步事件做完
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                this.TickAwaitEvent();
            }

            //if (TtEngine.Instance.TextureManager.WaitStreamingCount == 0 &&
            //    TtEngine.Instance.SkeletonActionManager.PendingActions.Count == 0 &&
            //    TtEngine.Instance.MeshPrimitivesManager.PendingMeshPrimitives.Count == 0 &&
            //    (AsyncEvents.Count + ContinueEvents.Count == 0))
            if (AsyncEvents.Count + ContinueEvents.Count == 0)
            {
                System.Threading.Thread.Sleep(50);

                lock (TtEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                {
                    foreach (var i in TtEngine.Instance.ContextThreadManager.AsyncIOEmptys)
                    {
                        i.ExecutePostEvent();
                        i.ExecuteContinue();
                    }
                }
            }

            base.Tick();
        }
        protected override void OnThreadStart()
        {
            
        }
        protected override void OnThreadExited()
        {

        }
    }
}
