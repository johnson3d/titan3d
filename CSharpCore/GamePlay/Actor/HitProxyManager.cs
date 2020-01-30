using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Actor
{
    public partial class GActor
    {
        public HitProxyManager.ActorProxy ActorProxy;
    }
    public class HitProxyManager
    {
        private Dictionary<UInt32, WeakReference<GActor>> ProxyedActors
        {
            get;
        } = new Dictionary<UInt32, WeakReference<GActor>>();
        private UInt32 HitProxyAllocatorId = 0;

        //public bool NeedToUpdateHitProxy = false;
        //public bool HitProxyIsNewest = false;

        public void MapActor(GActor actor)
        {
            lock (ProxyedActors)
            {
                if (actor.HitProxyId != 0)
                    return;

                actor.HitProxyId = ++HitProxyAllocatorId;
                //if(actor.HitProxyId == 0)
                //    actor.HitProxyId = ++HitProxyAllocatorId;
                ProxyedActors[actor.HitProxyId] = new WeakReference<GActor>(actor);
                for (int i = 0; i < actor.Children.Count; ++i)
                {
                    MapActor(actor.Children[i]);
                }
            }
        }
        public void UnmapActor(UInt32 id)
        {
            lock (ProxyedActors)
            {
                ProxyedActors.Remove(id);
            }
        }
        public void UnmapActor(GActor actor)
        {
            lock (ProxyedActors)
            {
                if (actor.HitProxyId == 0)
                    return;
                ProxyedActors.Remove(actor.HitProxyId);
                actor.HitProxyId = 0;
                for (int i = 0; i < actor.Children.Count; ++i)
                {
                    UnmapActor(actor.Children[i]);
                }
            }
        }
        public GActor FindActor(UInt32 id)
        {
            lock (ProxyedActors)
            {
                WeakReference<GActor> actor;
                if (ProxyedActors.TryGetValue(id, out actor))
                {
                    GActor result;
                    if (actor.TryGetTarget(out result))
                    {
                        return result;
                    }
                    else
                    {
                        ProxyedActors.Remove(id);
                        return null;
                    }
                }
                return null;
            }
        }

        //we need this method to send data to gpu;
        public Vector4 ConvertHitProxyIdToVector4(UInt32 HitProxyId)
        {
            return new Vector4(((HitProxyId >> 24) & 0x000000ff) / 255.0f, ((HitProxyId >> 16) & 0x000000ff) / 255.0f, ((HitProxyId >> 8) & 0x000000ff) / 255.0f,
                ((HitProxyId >> 0) & 0x000000ff) / 255.0f);
        }

        public UInt32 ConvertCpuTexColorToHitProxyId(IntColor PixelColor)
        {
            return ((UInt32)PixelColor.R << 24 | (UInt32)PixelColor.G << 16 | (UInt32)PixelColor.B << 8 | (UInt32)PixelColor.A << 0);
        }

        public class ActorProxy
        {
            public bool Checked;
            public GActor Actor;
            public object BindObject;
            public void InitProxy(HitProxyManager mgr)
            {
                Actor = GActor.NewMeshActorDirect(null);
                Checked = false;
                BindObject = null;
                Actor.ActorProxy = this;
                mgr.MapActor(Actor);
            }
            public void SetShowMesh(CCommandList cmd, Graphics.Mesh.CGfxMesh mesh)
            {
                var meshComp = Actor.GetComponent<Component.GMeshComponent>();
                meshComp.SetSceneMesh(cmd, mesh);
            }
            public void SetTarget(object o)
            {
                BindObject = o;
            }
            public T GetTarget<T>() where T : class
            {
                return BindObject as T;
            }
        }

        private Queue<ActorProxy> ValidActorPool = new Queue<ActorProxy>();
        public ActorProxy QueryActor(Graphics.Mesh.CGfxMesh mesh)
        {
            if (ValidActorPool.Count == 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    var ap = new ActorProxy();
                    ap.InitProxy(this);
                    ValidActorPool.Enqueue(ap);
                }
            }
            var result = ValidActorPool.Dequeue();
            result.SetShowMesh(CEngine.Instance.RenderContext.ImmCommandList, mesh);
            result.Checked = true;
            return result;
        }
        public void ReleaseActor(ActorProxy ap)
        {
            ap.Checked = false;
            ap.BindObject = null;
            ap.SetShowMesh(CEngine.Instance.RenderContext.ImmCommandList, null);
            ValidActorPool.Enqueue(ap);
        }
    }
}

namespace EngineNS
{
    partial class CEngine
    {
        public GamePlay.Actor.HitProxyManager HitProxyManager
        {
            get;
        } = new GamePlay.Actor.HitProxyManager();
    }
}
