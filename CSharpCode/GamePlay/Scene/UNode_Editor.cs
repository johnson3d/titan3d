using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public partial class TtNode : Graphics.Pipeline.IProxiable
    {
        ~TtNode()
        {
            TtEngine.Instance?.GfxDevice.HitproxyManager.UnmapProxy(this);
        }
        public virtual void GetHitProxyDrawMesh(List<Graphics.Mesh.TtMesh> meshes)
        {
            return;
        }
        [Category("Option")]
        public Graphics.Pipeline.TtHitProxy.EHitproxyType HitproxyType
        {
            get
            {
                return (Graphics.Pipeline.TtHitProxy.EHitproxyType)(((uint)(NodeStyles & ENodeStyles.HitproxyMasks)) >> 2);
            }
            set
            {
                var oldValue = HitproxyType;
                uint flags = (((uint)value & ((uint)ENodeStyles.HitproxyMasks >> 2)) << 2);
                NodeStyles = (NodeStyles & (~ENodeStyles.HitproxyMasks)) | (ENodeStyles)flags;
                OnHipproxyTypeChanged(oldValue, HitproxyType);
                OnHitProxyChanged();
            }
        }
        public Graphics.Pipeline.TtHitProxy HitProxy { get; set; }
        public virtual void OnHitProxyChanged()
        {
        }

        private void OnHipproxyTypeChanged(Graphics.Pipeline.TtHitProxy.EHitproxyType oldValue, Graphics.Pipeline.TtHitProxy.EHitproxyType newValue)
        {
            switch(newValue)
            {
                case Graphics.Pipeline.TtHitProxy.EHitproxyType.None:
                    TtEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(this);
                    SetHitProxySubTree(null);
                    break;
                case Graphics.Pipeline.TtHitProxy.EHitproxyType.Root:
                    TtEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(this);
                    TtEngine.Instance.GfxDevice.HitproxyManager.MapProxy(this);
                    SetHitProxySubTree(HitProxy);
                    break;
                case Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent:
                    if (oldValue != Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent)
                        TtEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(this);
                    if (Parent != null)
                    {
                        HitProxy = Parent.HitProxy;
                        SetHitProxySubTree(Parent.HitProxy);
                    }
                    break;
            }
        }
        private void SetHitProxySubTree(Graphics.Pipeline.TtHitProxy proxy)
        {
            foreach (var i in Children)
            {
                if (i.HitproxyType == Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent)
                {
                    TtEngine.Instance.GfxDevice.HitproxyManager.UnmapProxy(i);
                    i.HitProxy = proxy;
                    i.SetHitProxySubTree(proxy);
                    i.OnHitProxyChanged();
                }
            }
        }

        public virtual void AddAssetReferences(IO.IAssetMeta ameta)
        {

        }
    }
}