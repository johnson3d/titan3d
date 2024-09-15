using EngineNS.Graphics.Mesh;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("Sun", "Sun", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSunNode.TtSunNodeData), DefaultNamePrefix = "Sun")]
    public class TtSunNode : TtSceneActorNode
    {
        public class TtSunNodeData : TtNodeData
        {
            public TtSunNodeData()
            {
                SunMaterialName = RName.GetRName("material/default_sun.uminst", RName.ERNameType.Engine);
            }
            public TtDirectionLight DirectionLight { get; set; } = new TtDirectionLight();
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)]
            public RName SunMaterialName { get; set; }
        }
        protected override void OnParentChanged(TtNode prev, TtNode cur)
        {
            if (cur == null)
            {
                prev.GetWorld().mSuns.Remove(this);
            }
            base.OnParentChanged(prev, cur);
        }
        [Category("Option")]
        [Rtti.Meta]
        public TtDirectionLight DirectionLight
        {
            get => GetNodeData<TtSunNodeData>().DirectionLight;
            set => GetNodeData<TtSunNodeData>().DirectionLight = value;
        }
        Graphics.Pipeline.Shader.TtMaterialInstance SunMaterial;
        [Category("Option")]
        [Rtti.Meta]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)]
        public RName SunMaterialName
        {
            get => GetNodeData<TtSunNodeData>().SunMaterialName;
            set
            {
                GetNodeData<TtSunNodeData>().SunMaterialName = value;
                var action = async () =>
                {
                    SunMaterial = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(value);
                };
                action();
            }
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);

            world.mSuns.Add(this);
            return true;
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);

            if (DebugMesh != null)
                rp.AddVisibleMesh(DebugMesh);
        }
        public override void GetHitProxyDrawMesh(List<TtMesh> meshes)
        {
            base.GetHitProxyDrawMesh(meshes);
            if (DebugMesh != null)
                meshes.Add(DebugMesh);
        }

        protected override void OnAbsTransformChanged()
        {
            base.OnAbsTransformChanged();
            DirectionLight.Direction = Quaternion.RotateVector3(in Placement.TransformRef.mQuat, in Vector3.UnitX);
            if (mDebugMesh != null)
                mDebugMesh.DirectSetWorldMatrix(Placement.AbsTransform.ToMatrixNoScale(this.GetWorld().CameraOffset));
        }
        public override void OnHitProxyChanged()
        {
            if (mDebugMesh == null)
                return;
            if (this.HitProxy == null)
            {
                mDebugMesh.IsDrawHitproxy = false;
                return;
            }

            if (HitproxyType != Graphics.Pipeline.UHitProxy.EHitproxyType.None)
            {
                mDebugMesh.IsDrawHitproxy = true;
                var value = HitProxy.ConvertHitProxyIdToVector4();
                mDebugMesh.SetHitproxy(in value);
            }
            else
            {
                mDebugMesh.IsDrawHitproxy = false;
            }
        }
        Graphics.Mesh.TtMesh mDebugMesh;
        public Graphics.Mesh.TtMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    var cookedMesh = TtEngine.Instance.GfxDevice.MeshPrimitiveManager.FindMeshPrimitive(RName.GetRName("axis/movex.vms", RName.ERNameType.Engine));
                    if (cookedMesh == null)
                        return null;
                    var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                    materials1[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.FindMaterialInstance(RName.GetRName("axis/axis_x_d.uminst", RName.ERNameType.Engine));
                    var mesh2 = new Graphics.Mesh.TtMesh();
                    var ok1 = mesh2.Initialize(cookedMesh, materials1,
                        Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    if (ok1)
                    {
                        mesh2.IsAcceptShadow = false;
                        mDebugMesh = mesh2;

                        mDebugMesh.HostNode = this;

                        BoundVolume.LocalAABB = mDebugMesh.MaterialMesh.AABB;

                        this.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;

                        UpdateAbsTransform();
                        UpdateAABB();
                        Parent?.UpdateAABB();
                    }
                }
                return mDebugMesh;
            }
        }
    }
}
