using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    /// <summary>
    /// Base class for all volume-type scene nodes (PostProcessVolume, AudioVolume, etc.).
    /// Manages BVH proxy registration in the World's volume spatial index.
    /// Subclasses define specific data (e.g. ColorGradingSettings) and override ComputeInfluence if needed.
    /// </summary>
    [Rtti.Meta("")]
    public partial class TtVolumeBaseNode : TtVisual
    {
        [Rtti.Meta("")]
        public class TtVolumeBaseData : TtNodeData
        {
            /// <summary>
            /// Higher priority volumes override lower ones when multiple overlap.
            /// </summary>
            [Category("Volume")]
            [Rtti.Meta("")]
            public int Priority { get; set; } = 0;

            /// <summary>
            /// Blend weight (0 = no effect, 1 = full effect).
            /// </summary>
            [Category("Volume")]
            [Rtti.Meta("")]
            [EGui.Controls.PropertyGrid.TtValueChangeStep(0.05f)]
            public float BlendWeight { get; set; } = 1.0f;

            /// <summary>
            /// Blend radius in world units for smooth transitions at volume boundaries. 0 = hard cut.
            /// </summary>
            [Category("Volume")]
            [Rtti.Meta("")]
            [EGui.Controls.PropertyGrid.TtValueChangeStep(0.5f)]
            public float BlendRadius { get; set; } = 0.0f;

            /// <summary>
            /// If true, volume affects the entire world regardless of spatial bounds.
            /// </summary>
            [Category("Volume")]
            [Rtti.Meta("")]
            public bool IsUnbound { get; set; } = false;
        }

        // BVH proxy handle — valid while registered in TtWorld volume BVH
        internal int BvhProxyId { get; set; } = Bricks.Collision.BVH.TtDynamicBVH<TtWorld.TtBvhVolumeEntry>.NullNode;
        // Wrapper entry in the BVH — weakly references this node
        internal TtWorld.TtBvhVolumeEntry VolumeEntry { get; set; }

        // ── Visual meshes ──
        Graphics.Mesh.TtRenderMesh mWireframeMesh;
        Graphics.Mesh.TtRenderMesh mHitproxyMesh;

        protected override async Thread.Async.TtTask<bool> InitializeNode(
            TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            this.IsForceGatherNode = true;

            // Default AABB: 10×10×10
            this.BoundVolume.LocalAABB = new BoundingBox(new Vector3(-5), new Vector3(5));

            // Register into World's volume BVH
            var w = GetWorld();
            w?.RegisterVolume(this);

            return ret;
        }

        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mWireframeMesh);
            CoreSDK.DisposeObject(ref mHitproxyMesh);
            var w = GetWorld();
            w?.UnregisterVolume(this);
            base.Dispose();
        }

        /// <summary>
        /// Compute how much this volume influences a camera at the given world position.
        /// Returns 0 if outside, BlendWeight if fully inside, a smooth value in the blend radius.
        /// Unbound volumes always return BlendWeight.
        /// </summary>
        public virtual float ComputeInfluence(in DVector3 cameraWorldPos)
        {
            var volData = GetNodeData<TtVolumeBaseData>();
            if (volData == null)
                return 0f;

            if (volData.IsUnbound)
                return volData.BlendWeight;

            ref var absAABB = ref BoundVolume.AbsAABB;
            var containment = DBoundingBox.Contains(in absAABB, in cameraWorldPos);
            if (containment == ContainmentType.Contains)
                return volData.BlendWeight;

            if (volData.BlendRadius <= 0f)
                return 0f;

            var clampedPos = DVector3.Clamp(cameraWorldPos, absAABB.Minimum, absAABB.Maximum);
            float distance = (float)(cameraWorldPos - clampedPos).Length();
            if (distance >= volData.BlendRadius)
                return 0f;

            float t = 1.0f - (distance / volData.BlendRadius);
            t = t * t * (3.0f - 2.0f * t); // smoothstep
            return volData.BlendWeight * t;
        }

        /// <summary>
        /// Called when the node's world-space AABB changes. Updates the BVH proxy.
        /// </summary>
        public void UpdateBvhProxy()
        {
            var w = GetWorld();
            if (w == null || BvhProxyId == Bricks.Collision.BVH.TtDynamicBVH<TtWorld.TtBvhVolumeEntry>.NullNode)
                return;
            w.UpdateVolumeProxy(this);
        }

        // ── Wireframe AABB debug mesh ──
        /// <summary>
        /// Wireframe box visualizing the volume's local AABB. Created lazily.
        /// </summary>
        public Graphics.Mesh.TtRenderMesh WireframeMesh
        {
            get
            {
                if (mWireframeMesh == null)
                {
                    var localAABB = BoundVolume.LocalAABB;
                    var size = localAABB.GetSize();
                    var min = localAABB.Minimum;
                    var wireProvider = Graphics.Mesh.TtMeshDataProvider.MakeBoxWireframe(
                        min.X, min.Y, min.Z, size.X, size.Y, size.Z, 0xFF00FF00);
                    var wirePrimitive = wireProvider.ToMesh();
                    var materials = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                    materials[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                    var mesh = new Graphics.Mesh.TtRenderMesh();
                    var ok = mesh.Initialize(wirePrimitive, materials,
                        Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                    if (ok)
                    {
                        mesh.IsAcceptShadow = false;
                        mesh.IsDrawHitproxy = false;
                        mWireframeMesh = mesh;

                        UpdateAbsTransform();
                        UpdateAABB();
                        Parent?.UpdateAABB();
                    }
                }
                return mWireframeMesh;
            }
        }

        /// <summary>
        /// Rebuild the wireframe mesh when the local AABB changes (e.g. user resized the volume).
        /// </summary>
        public void RebuildWireframeMesh()
        {
            CoreSDK.DisposeObject(ref mWireframeMesh);
            // Next access to WireframeMesh will recreate it
        }

        // ── Hitproxy Rect mesh ──
        async Thread.Async.TtTask InitHitproxyMesh()
        {
            if (mHitproxyMesh != null)
                return;

            float rectSize = 0.25f;
            var rectProvider = Graphics.Mesh.TtMeshDataProvider.MakeRect2D(
                -rectSize * 0.5f, -rectSize * 0.5f, rectSize, rectSize, 0.0f);
            var rectMesh = rectProvider.ToMesh();
            var mtl = await RName.GetRName("material/utility/volume.uminst", RName.ERNameType.Engine)
                .GetAsset<Graphics.Pipeline.Shader.TtMaterialInstance>();
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = mtl;
            var mesh = new Graphics.Mesh.TtRenderMesh();
            var ok = mesh.Initialize(rectMesh, materials,
                Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                mesh.IsAcceptShadow = false;
                mHitproxyMesh = mesh;
                mHitproxyMesh.HostNode = this;

                this.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;

                UpdateAbsTransform();
                UpdateAABB();
                Parent?.UpdateAABB();
            }
        }

        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);
            await InitHitproxyMesh();
            UpdateAbsTransform();
        }

        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            if (mHitproxyMesh != null)
                meshes.Add(mHitproxyMesh);
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            rp.AddVisibleNode(this);

            if ((rp.CullFilters & TtWorld.TtVisParameter.EVisCullFilter.UtilityEditor) != 0)
            {
                if (WireframeMesh != null)
                    rp.AddVisibleMesh(mWireframeMesh);
                if (mHitproxyMesh != null)
                {
                    // hitproxy 面向相机位置，固定大小不受 node Scale 影响
                    if (rp.CullCamera != null)
                    {
                        var objPos = Placement.AbsTransform.mPosition;
                        var yawQuat = rp.CullCamera.GetYawFaceToCamera(objPos);
                        var hitproxyTransform = FTransform.CreateTransform(in objPos, in Vector3.One, in yawQuat);
                        mHitproxyMesh.SetWorldTransform(in hitproxyTransform, rp.World, true);
                    }
                    rp.AddVisibleMesh(mHitproxyMesh);
                }
            }
        }
        protected override void OnRemoveFromWorld()
        {
            GetWorld()?.UnregisterVolume(this);
        }
        protected override void OnAbsAABBChanged()
        {
            base.OnAbsAABBChanged();

            var world = this.GetWorld();
            if (mWireframeMesh != null)
                mWireframeMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
            if (mHitproxyMesh != null)
            {
                // hitproxy 不跟随 node Scale，朝向由 OnGatherVisibleMeshes 中相机驱动
                var hitproxyTransform = FTransform.CreateTransform(in Placement.AbsTransform.mPosition, in Vector3.One, in Quaternion.Identity);
                mHitproxyMesh.SetWorldTransform(in hitproxyTransform, world, true);
            }

            // Sync BVH proxy when transform changes
            UpdateBvhProxy();
        }

        public override void OnHitProxyChanged()
        {
            if (mHitproxyMesh == null)
                return;
            if (this.HitProxy == null)
            {
                mHitproxyMesh.IsDrawHitproxy = false;
                return;
            }

            if (HitproxyType != Graphics.Pipeline.TtHitProxy.EHitproxyType.None)
            {
                mHitproxyMesh.IsDrawHitproxy = true;
                var value = HitProxy.ConvertHitProxyIdToVector4();
                mHitproxyMesh.SetHitproxy(in value);
            }
            else
            {
                mHitproxyMesh.IsDrawHitproxy = false;
            }
        }

        public override bool IsAcceptShadow
        {
            get => false;
            set { }
        }

        public override bool IsCastShadow
        {
            get => false;
            set { }
        }
    }
}
