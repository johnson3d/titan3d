using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("PointLight", "Graphics\\PointLight", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPointLightNode.TtLightNodeData), DefaultNamePrefix = "PointLight")]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.UPointLightNode@EngineCore", "EngineNS.GamePlay.Scene.UPointLightNode" })]
    public partial class TtPointLightNode : TtVisual
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDebugMesh);
            CoreSDK.DisposeObject(ref mHitproxyMesh);
            base.Dispose();
        }
        [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.UPointLightNode.ULightNodeData@EngineCore", "EngineNS.GamePlay.Scene.UPointLightNode.ULightNodeData" })]
        public class TtLightNodeData : TtNodeData
        {
            internal TtPointLightNode HostNode;
            Vector3 mColor;
            [Rtti.Meta("")]
            [Category("Option")]
            [EGui.Controls.PropertyGrid.TtColor3PickerEditor()]
            public Vector3 Color 
            { 
                get=> mColor;
                set
                {
                    mColor = value;
                    HostNode?.OnLightColorChanged();
                }
            }
            [Rtti.Meta("")]
            [Category("Option")]
            public float Intensity { get; set; }
            [Rtti.Meta("")]
            [Category("Option")]
            public float Radius { get; set; }
        }
        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new TtLightNodeData();
            }
            
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            GetNodeData<TtLightNodeData>().HostNode = this;
            this.BoundVolume.LocalAABB = new BoundingBox(Vector3.Zero, 1.0f);

            this.IsForceGatherNode = true;
            return ret;
        }
        [Category("Option")]
        public TtLightNodeData LightData
        {
            get => GetNodeData<TtLightNodeData>();
        }
        public static async Thread.Async.TtTask<TtPointLightNode> AddPointLightNode(TtWorld world, TtNode parent, TtLightNodeData data, DVector3 pos)
        {
            var scene = parent.GetNearestParentScene();
            var scale = new Vector3(data.Radius);

            var meshNode = await GamePlay.Scene.TtNode.SpawnNode<TtPointLightNode>(parent, null, data, EBoundVolumeType.Box, typeof(TtPlacement)) as TtPointLightNode;            
            
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
                    Vector4 clr4 = new Vector4(GetNodeData<TtLightNodeData>().Color, 1);
                    colorVar.SetValue(in clr4);
                }
            }
        }
        Graphics.Mesh.TtRenderMesh mDebugMesh;
        /// <summary>
        /// 仅用于标识灯光范围和颜色的可视化 Mesh，不参与 hitproxy 拾取。
        /// </summary>
        public Graphics.Mesh.TtRenderMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    var wireProvider = Graphics.Mesh.TtMeshDataProvider.MakeSphereWireframe(in Vector3.Zero, 0.5f);
                    var cookedMesh = wireProvider.ToMesh();
                    var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                    materials1[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                    var mesh2 = new Graphics.Mesh.TtRenderMesh();
                    var ok1 = mesh2.Initialize(cookedMesh, materials1,
                        Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                    if (ok1)
                    {
                        mesh2.IsAcceptShadow = false;
                        mesh2.IsDrawHitproxy = false;
                        mDebugMesh = mesh2;

                        UpdateAbsTransform();
                        UpdateAABB();
                        Parent?.UpdateAABB();

                        OnLightColorChanged();
                    }
                }
                return mDebugMesh;
            }
        }

        Graphics.Mesh.TtRenderMesh mHitproxyMesh;
        /// <summary>
        /// 仅用于 hitproxy 拾取的 Mesh（1m×1m Rect），只在 UtilityDebug 可见。
        /// 通过 InitHitproxyMesh 异步初始化。
        /// </summary>
        async Thread.Async.TtTask InitHitproxyMesh()
        {
            if (mHitproxyMesh != null)
                return;

            float rectSize = 0.5f;
            var rectProvider = Graphics.Mesh.TtMeshDataProvider.MakeRect2D(-rectSize*0.5f, -rectSize*0.5f, rectSize, rectSize, 0.0f);
            var rectMesh = rectProvider.ToMesh();
            var mtl = await RName.GetRName("material/utility/point_light.uminst", RName.ERNameType.Engine)
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
            this.BoundVolume.LocalAABB = new BoundingBox(Vector3.Zero, 1.0f);
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
            //灯光比较特殊，无论是否debug都要加入visnode列表，否则Tiling过程得不到可见灯光集
            rp.AddVisibleNode(this);

            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.LightDebug) != 0)
            {
                if (DebugMesh != null)
                    rp.AddVisibleMesh(mDebugMesh);
            }

            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.UtilityEditor) != 0)
            {
                if (mHitproxyMesh != null)
                {
                    // 面向相机 + 屏幕空间固定大小
                    if (rp.CullCamera != null)
                    {
                        var objPos = Placement.AbsTransform.mPosition;
                        var yawQuat = rp.CullCamera.GetYawFaceToCamera(objPos);
                        //float scale = rp.CullCamera.GetScaleWithFixSizeInScreen(in objPos, 32.0f);
                        float scale = 1.0f;
                        var scaleVec = new Vector3(scale);
                        var hitproxyTransform = FTransform.CreateTransform(in objPos, in scaleVec, in yawQuat);
                        mHitproxyMesh.SetWorldTransform(in hitproxyTransform, rp.World, false);
                    }
                    rp.AddVisibleMesh(mHitproxyMesh);
                }
            }
        }
        protected override void OnAbsTransformChanged()
        {
            var lightData = NodeData as TtLightNodeData;
            if (lightData != null)
            {
                lightData.Radius = Placement.Scale.X;
            }

            var world = this.GetWorld();
            if (mDebugMesh != null)
                mDebugMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
            if (mHitproxyMesh != null)
            {
                // hitproxy mesh 不跟随 node 的 Scale，使用固定大小；朝向由 OnGatherVisibleMeshes 中相机驱动
                var hitproxyTransform = FTransform.CreateTransform(in Placement.AbsTransform.mPosition, in Vector3.One, in Quaternion.Identity);
                mHitproxyMesh.SetWorldTransform(in hitproxyTransform, world, true);
            }
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
            get { return false; }
            set { }
        }
    }
}
