using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [Bricks.CodeBuilder.ContextMenu("PointLight", "PointLight", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtPointLightNode.TtLightNodeData), DefaultNamePrefix = "PointLight")]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.UPointLightNode@EngineCore", "EngineNS.GamePlay.Scene.UPointLightNode" })]
    public partial class TtPointLightNode : TtSceneActorNode
    {
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDebugMesh);
            base.Dispose();
        }
        [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.UPointLightNode.ULightNodeData@EngineCore", "EngineNS.GamePlay.Scene.UPointLightNode.ULightNodeData" })]
        public class TtLightNodeData : TtNodeData
        {
            internal TtPointLightNode HostNode;
            Vector3 mColor;
            [Rtti.Meta]
            [Category("Option")]
            [EGui.Controls.PropertyGrid.Color3PickerEditor()]
            public Vector3 Color 
            { 
                get=> mColor;
                set
                {
                    mColor = value;
                    HostNode?.OnLightColorChanged();
                }
            }
            [Rtti.Meta]
            [Category("Option")]
            public float Intensity { get; set; }
            [Rtti.Meta]
            [Category("Option")]
            public float Radius { get; set; }
        }
        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new TtLightNodeData();
            }
            
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            GetNodeData<TtLightNodeData>().HostNode = this;

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

            var meshNode = await scene.NewNode(world, typeof(TtPointLightNode), data, EBoundVolumeType.Box, typeof(TtPlacement)) as TtPointLightNode;            
            meshNode.Parent = parent;
            
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
        Graphics.Mesh.TtMesh mDebugMesh;
        public Graphics.Mesh.TtMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    var cookedMesh = TtEngine.Instance.GfxDevice.MeshPrimitiveManager.UnitSphere;
                    var materials1 = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
                    materials1[0] = TtEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                    var mesh2 = new Graphics.Mesh.TtMesh();
                    var ok1 = mesh2.Initialize(cookedMesh, materials1,
                        Rtti.TtTypeDescGetter<Graphics.Mesh.TtMdfStaticMesh>.TypeDesc);
                    if (ok1)
                    {
                        mesh2.IsAcceptShadow = false;
                        mDebugMesh = mesh2;

                        mDebugMesh.HostNode = this;

                        BoundVolume.LocalAABB = mDebugMesh.MaterialMesh.AABB;

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
        public override async Thread.Async.TtTask OnNodeLoaded(TtNode parent)
        {
            await base.OnNodeLoaded(parent);
            UpdateAbsTransform();
        }
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtMesh> meshes)
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
            //灯光比较特殊，无论是否debug都要加入visnode列表，否则Tiling过程得不到可见灯光集
            rp.AddVisibleNode(this);

            //if (TtEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug) == false)
            //    return;
            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.LightDebug) == 0)
                return;

            if (DebugMesh != null)
                rp.AddVisibleMesh(mDebugMesh);
        }
        protected override void OnAbsTransformChanged()
        {
            var lightData = NodeData as TtLightNodeData;
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
        public override bool OnTickLogic(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy)
        {
            //test temp code 
            //LightData.Intensity = 120 * (float)Math.Sin(TtEngine.Instance.TickCountSecond * 0.005f);

            return true;
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
