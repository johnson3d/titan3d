using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public partial class UNode : Graphics.Pipeline.IProxiable
    {
        public virtual void GetDrawMesh(List<Graphics.Mesh.UMesh> meshes)
        {
            return;
        }
        public Graphics.Pipeline.UHitProxy.EHitproxyType HitproxyType
        {
            get
            {
                return (Graphics.Pipeline.UHitProxy.EHitproxyType)(((uint)(NodeStyles & ENodeStyles.HitproxyMasks)) >> 2);
            }
            set
            {
                var oldValue = HitproxyType;
                uint flags = ((uint)value & ((uint)ENodeStyles.HitproxyMasks >> 2)) << 2;
                SetStyle((ENodeStyles)flags);
                OnHipproxyTypeChanged(oldValue, HitproxyType);
                OnHitProxyChanged();
            }
        }
        public Graphics.Pipeline.UHitProxy HitProxy { get; set; }
        public virtual void OnHitProxyChanged()
        {
        }

        private void OnHipproxyTypeChanged(Graphics.Pipeline.UHitProxy.EHitproxyType oldValue, Graphics.Pipeline.UHitProxy.EHitproxyType newValue)
        {
            switch(newValue)
            {
                case Graphics.Pipeline.UHitProxy.EHitproxyType.None:
                    UEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(this);
                    SetHitProxySubTree(null);
                    break;
                case Graphics.Pipeline.UHitProxy.EHitproxyType.Root:
                    UEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(this);
                    UEngine.Instance.GfxDevice.HitproxyManager.MapProxy(this);
                    SetHitProxySubTree(HitProxy);
                    break;
                case Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent:
                    UEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(this);
                    if (Parent != null)
                    {
                        HitProxy = Parent.HitProxy;
                        SetHitProxySubTree(Parent.HitProxy);
                    }
                    break;
            }
        }
        private void SetHitProxySubTree(Graphics.Pipeline.UHitProxy proxy)
        {
            foreach (var i in Children)
            {
                if (i.HitproxyType == Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent)
                {
                    UEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(i);
                    i.HitProxy = proxy;
                    i.SetHitProxySubTree(proxy);
                    i.OnHitProxyChanged();
                }
            }
        }
    }
}