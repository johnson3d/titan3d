using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("SpotLight", "Graphics\\SpotLight", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSpotLightNode.TtSpotLightNodeData), DefaultNamePrefix = "SpotLight")]
    [Rtti.Meta("")]
    public partial class TtSpotLightNode : TtVisual
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDebugMesh);
            base.Dispose();
        }

        [Rtti.Meta("")]
        public class TtSpotLightNodeData : TtNodeData
        {
            internal TtSpotLightNode HostNode;

            Vector3 mColor;
            [Rtti.Meta("")]
            [Category("Option")]
            [EGui.Controls.PropertyGrid.TtColor3PickerEditor()]
            public Vector3 Color
            {
                get => mColor;
                set
                {
                    mColor = value;
                    HostNode?.OnLightColorChanged();
                }
            }

            [Rtti.Meta("")]
            [Category("Option")]
            public float Intensity { get; set; } = 1.0f;

            [Rtti.Meta("")]
            [Category("Option")]
            public float Radius { get; set; } = 10.0f;

            float mInnerConeAngle = 25.0f;
            [Rtti.Meta("")]
            [Category("Option")]
            [Description("Inner cone half-angle in degrees (full intensity region)")]
            public float InnerConeAngle
            {
                get => mInnerConeAngle;
                set => mInnerConeAngle = Math.Clamp(value, 0.0f, OuterConeAngle);
            }

            float mOuterConeAngle = 45.0f;
            [Rtti.Meta("")]
            [Category("Option")]
            [Description("Outer cone half-angle in degrees (falloff boundary)")]
            public float OuterConeAngle
            {
                get => mOuterConeAngle;
                set => mOuterConeAngle = Math.Clamp(value, InnerConeAngle, 89.0f);
            }
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new TtSpotLightNodeData();
            }

            var ret = await base.InitializeNode(world, data, bvType, placementType);
            GetNodeData<TtSpotLightNodeData>().HostNode = this;
            this.BoundVolume.LocalAABB = new BoundingBox(Vector3.Zero, 1.0f);
            this.IsForceGatherNode = true;
            return ret;
        }

        [Category("Option")]
        public TtSpotLightNodeData LightData
        {
            get => GetNodeData<TtSpotLightNodeData>();
        }

        public static async Thread.Async.TtTask<TtSpotLightNode> AddSpotLightNode(TtWorld world, TtNode parent, TtSpotLightNodeData data, DVector3 pos)
        {
            var scene = parent.GetNearestParentScene();
            var scale = new Vector3(data.Radius);

            var meshNode = await scene.SpawnSceneActor<TtSpotLightNode>(parent, null, data, EBoundVolumeType.Box, typeof(TtPlacement)) as TtSpotLightNode;
            meshNode.Placement.SetTransform(in pos, in scale, in Quaternion.Identity);

            return meshNode;
        }

        public int IndexInGpuScene = -1;

        internal void OnLightColorChanged()
        {
            if (mDebugMesh != null)
            {
                var colorVar = mDebugMesh.MaterialMesh.SubMeshes[0].Materials[0].FindVar("clr4_0");
                if (colorVar != null)
                {
                    Vector4 clr4 = new Vector4(GetNodeData<TtSpotLightNodeData>().Color, 1);
                    colorVar.SetValue(in clr4);
                }
            }
        }

        Graphics.Mesh.TtRenderMesh mDebugMesh;
        public Graphics.Mesh.TtRenderMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    var cookedMesh = TtEngine.Instance.GfxDevice.MeshPrimitiveManager.UnitSphere;
                    var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                    materials1[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                    var mesh2 = new Graphics.Mesh.TtRenderMesh();
                    var ok1 = mesh2.Initialize(cookedMesh, materials1,
                        Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                    if (ok1)
                    {
                        mesh2.IsAcceptShadow = false;
                        mDebugMesh = mesh2;
                        mDebugMesh.HostNode = this;

                        this.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;

                        UpdateAbsTransform();
                        UpdateAABB();
                        Parent?.UpdateAABB();

                        OnLightColorChanged();
                    }
                }
                return mDebugMesh;
            }
        }

        protected override async Thread.Async.TtTask OnPostInitNode(TtNode parent, object extArg)
        {
            await base.OnPostInitNode(parent, extArg);
            this.BoundVolume.LocalAABB = new BoundingBox(Vector3.Zero, 1.0f);
            UpdateAbsTransform();
        }

        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            meshes.Add(mDebugMesh);
            foreach (var i in Children)
            {
                if (i.HitproxyType == Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent)
                    i.GetHitProxyDrawMesh(meshes);
            }
        }

        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            rp.AddVisibleNode(this);

            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.LightDebug) == 0)
                return;

            if (DebugMesh != null)
                rp.AddVisibleMesh(mDebugMesh);
        }

        protected override void OnAbsTransformChanged()
        {
            var lightData = NodeData as TtSpotLightNodeData;
            if (lightData != null)
            {
                lightData.Radius = Placement.Scale.X;
            }
            if (mDebugMesh == null)
                return;

            var world = this.GetWorld();
            mDebugMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
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

            if (HitproxyType != Graphics.Pipeline.TtHitProxy.EHitproxyType.None)
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

        public override bool IsAcceptShadow
        {
            get => false;
            set { }
        }

        public override bool IsCastShadow
        {
            get { return false; }
            set { }
        }

        /// <summary>
        /// Get the forward direction of this spot light in world space.
        /// Uses the local -Z axis transformed by the node's rotation.
        /// </summary>
        public Vector3 GetDirection()
        {
            var rotation = Placement.AbsTransform.Quat;
            return Vector3.TransformNormal(Vector3.UnitZ, Matrix.RotationQuaternion(rotation));
        }
    }
}
