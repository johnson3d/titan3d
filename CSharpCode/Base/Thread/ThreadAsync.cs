using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Thread
{
    public class ThreadAsync : ContextThread
    {
        [ThreadStatic]
        private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(ThreadAsync), nameof(Tick));
        public override void Tick()
        {
            //把异步事件做完
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                this.TickAwaitEvent();
            }

            //if (UEngine.Instance.TextureManager.WaitStreamingCount == 0 &&
            //    UEngine.Instance.SkeletonActionManager.PendingActions.Count == 0 &&
            //    UEngine.Instance.MeshPrimitivesManager.PendingMeshPrimitives.Count == 0 &&
            //    (AsyncEvents.Count + ContinueEvents.Count == 0))
            if (AsyncEvents.Count + ContinueEvents.Count == 0)
            {
                System.Threading.Thread.Sleep(50);
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
