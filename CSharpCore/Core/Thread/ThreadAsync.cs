using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Thread
{
    public class ThreadAsync : ContextThread
    {
        public ThreadAsync()
        {
            Async.ContextThreadManager.Instance.ContextAsyncIO = this;
        }
        public override void Tick()
        {
            //把异步事件做完
            this.TickAwaitEvent();

            if (CEngine.Instance.TextureManager.WaitStreamingCount == 0 &&
                CEngine.Instance.SkeletonActionManager.PendingActions.Count == 0 &&
                CEngine.Instance.MeshPrimitivesManager.PendingMeshPrimitives.Count == 0 &&
                (AsyncEvents.Count + ContinueEvents.Count == 0))
            {
                System.Threading.Thread.Sleep(50);
            }

            //加载一个动作
            CEngine.Instance.SkeletonActionManager.RestoreOneAction();
            
            //加载一个模型
            var mesh = CEngine.Instance.MeshPrimitivesManager.RestoreOneMesh();
            if(mesh!=null)
            {
                //可以想办法处理一下mesh加载后，优先加载他用到的Texture 
            }

            if (CEngine.Instance.SkeletonActionManager.PendingActions.Count == 0 &&
                CEngine.Instance.MeshPrimitivesManager.PendingMeshPrimitives.Count == 0)
            {
                //最后加载纹理
                CEngine.Instance.TextureManager.TickStreaming(CEngine.Instance.RenderContext);
                //CEngine.Instance.TextureManager.RestoreOneTexture();
            }

            var num =  CEngine.Instance.SkeletonActionManager.PendingActions.Count;
            num += CEngine.Instance.MeshPrimitivesManager.PendingMeshPrimitives.Count;
            num += CEngine.Instance.TextureManager.WaitStreamingCount;
            if(num==0)
            {
                var list = Async.ContextThreadManager.Instance.AsyncIOEmptys;
                lock(list)
                {
                    foreach(var i in list)
                    {
                        lock(i.ContinueThread.ContinueEvents)
                        {
                            i.ContinueThread.ContinueEvents.Enqueue(i);
                        }
                    }
                    list.Clear();
                }
            }

            base.Tick();
        }
        protected override void OnThreadStart()
        {
            Thread_StartIOThread();
        }
        protected override void OnThreadExited()
        {

        }
    }
}
