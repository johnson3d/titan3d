using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public partial class TtNode : Graphics.Pipeline.IProxiable
    {
        public virtual void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            return;
        }
        public virtual void OnDoubleClick()
        {
        }
        /// <summary>
        /// 选中本节点时编辑器坐标轴 (gizmo) 的原点放在哪里。
        ///
        /// true = gizmo 画在视口中心, 忽略节点自身 position。给"只需要旋转、位置没有意义"的
        /// 节点用 (典型是平行光 <see cref="TtSunNode"/>: 光照方向由旋转推出, position 完全不参与
        /// 计算)。这类节点往往停在原点或跑到视野外, gizmo 贴在它原点上既看不见也难操作, 而且拖动
        /// 时 gizmo 会跟着相机跑; 画在视口中心则任何时候都能直接调角度。
        /// false = gizmo 画在节点原点 (常规行为)。
        ///
        /// 实现必须是纯函数 (按类型返回常量), 不要读写可变状态 —— 它会在每帧 gizmo 原点重算时被取用。
        /// 生效方式见 <see cref="EngineNS.GamePlay.TtAxis.CenterAxisMode"/>。
        /// </summary>
        [Browsable(false)]
        public virtual bool IsAxisAtViewportCenter => false;
        [System.ComponentModel.Category("Option")]
        public Graphics.Pipeline.TtHitProxy.EHitproxyType HitproxyType
        {
            get
            {
                return NodeData.HitproxyType;
                //return (Graphics.Pipeline.TtHitProxy.EHitproxyType)(((uint)(NodeStyles & ENodeStyles.HitproxyMasks)) >> 2);
            }
            set
            {
                var oldValue = HitproxyType;
                //uint flags = (((uint)value & ((uint)ENodeStyles.HitproxyMasks >> 2)) << 2);
                //NodeStyles = (NodeStyles & (~ENodeStyles.HitproxyMasks)) | (ENodeStyles)flags;
                NodeData.HitproxyType = value;
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