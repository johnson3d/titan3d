using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [UNode(NodeDataType = typeof(UPointLightNode.ULightNodeData), DefaultNamePrefix = "PointLight")]
    public partial class UPointLightNode : USceneActorNode
    {
        public class ULightNodeData : UNodeData
        {
            internal UPointLightNode HostNode;
            Vector3 mColor;
            [Rtti.Meta]
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
            public float Intensity { get; set; }
            [Rtti.Meta]
            public float Radius { get; set; }
        }
        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new ULightNodeData();
            }
            
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            GetNodeData<ULightNodeData>().HostNode = this;
            return ret;
        }
        public static async System.Threading.Tasks.Task<UPointLightNode> AddPointLightNode(UWorld world, UNode parent, ULightNodeData data, DVector3 pos)
        {
            var scene = parent.GetNearestParentScene();
            var scale = new Vector3(data.Radius);

            var meshNode = await scene.NewNode(world, typeof(UPointLightNode), data, EBoundVolumeType.Box, typeof(UPlacement)) as UPointLightNode;            
            meshNode.Parent = parent;
            
            meshNode.Placement.SetTransform(in pos, in scale, in Quaternion.Identity);

            return meshNode;
        }
        public UInt16 IndexInGpuScene = UInt16.MaxValue;
        internal void OnLightColorChanged()
        {
            if (mDebugMesh != null)
            {
                var colorVar = mDebugMesh.MaterialMesh.Materials[0].FindVar("clr4_0");
                if (colorVar != null)
                {
                    Vector4 clr4 = new Vector4(GetNodeData<ULightNodeData>().Color, 1);
                    colorVar.SetValue(in clr4);
                }
            }
        }
        Graphics.Mesh.UMesh mDebugMesh;
        public Graphics.Mesh.UMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    var cookedMesh = UEngine.Instance.GfxDevice.MeshPrimitiveManager.UnitSphere;
                    var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                    materials1[0] = UEngine.Instance.GfxDevice.MaterialInstanceManager.WireColorMateria.CloneMaterialInstance();
                    var mesh2 = new Graphics.Mesh.UMesh();
                    var ok1 = mesh2.Initialize(cookedMesh, materials1,
                        Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
                    if (ok1)
                    {
                        mDebugMesh = mesh2;

                        mDebugMesh.HostNode = this;

                        BoundVolume.LocalAABB = mDebugMesh.MaterialMesh.Mesh.mCoreObject.mAABB;

                        this.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;

                        UpdateAbsTransform();
                        UpdateAABB();
                        Parent?.UpdateAABB();

                        OnLightColorChanged();
                    }
                }
                return mDebugMesh;
            }
        }
        public override void OnNodeLoaded(UNode parent)
        {
            base.OnNodeLoaded(parent);
            UpdateAbsTransform();
        }
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.UMesh> meshes)
        {
            meshes.Add(mDebugMesh);
            foreach (var i in Children)
            {
                if (i.HitproxyType == Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent)
                    i.GetHitProxyDrawMesh(meshes);
            }
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            if (rp.VisibleNodes != null)
            {//灯光比较特殊，无论是否debug都要加入visnode列表，否则Tiling过程得不到可见灯光集
                rp.VisibleNodes.Add(this);
            }

            //if (UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug) == false)
            //    return;
            if ((rp.CullFilters & GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug) == 0)
                return;

            if (DebugMesh != null)
                rp.VisibleMeshes.Add(mDebugMesh);
        }
        protected override void OnAbsTransformChanged()
        {
            var lightData = NodeData as ULightNodeData;
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
        public override bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            //test temp code 
            LightData.Intensity = 120 * (float)Math.Sin(UEngine.Instance.TickCountSecond * 0.005f);

            return true;
        }
        public ULightNodeData LightData
        {
            get
            {
                return NodeData as ULightNodeData;
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
    [UNode(NodeDataType = typeof(UDirLightNode.UDirLightNodeData), DefaultNamePrefix = "Sun")]
    public partial class UDirLightNode : USceneActorNode
    {
        public class UDirLightNodeData : UNodeData
        {
            [Rtti.Meta]
            public UDirectionLight LightData { get; set; } = new UDirectionLight();
        }
        public UDirLightNodeData LightData
        {
            get
            {
                return NodeData as UDirLightNodeData;
            }
        }
        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new UDirLightNodeData();
            }
            
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            world.DirectionLight = LightData.LightData;
            return ret;
        }
        Graphics.Mesh.UMesh mDebugMesh;
        public Graphics.Mesh.UMesh DebugMesh
        {
            get
            {
                if (mDebugMesh == null)
                {
                    var cookedMesh = UEngine.Instance.GfxDevice.MeshPrimitiveManager.FindMeshPrimitive(RName.GetRName("axis/movex.vms", RName.ERNameType.Engine));
                    if (cookedMesh == null)
                        return null;
                    var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                    materials1[0] = UEngine.Instance.GfxDevice.MaterialInstanceManager.FindMaterialInstance(RName.GetRName("axis/axis_x_d.uminst", RName.ERNameType.Engine));
                    var mesh2 = new Graphics.Mesh.UMesh();
                    var ok1 = mesh2.Initialize(cookedMesh, materials1,
                        Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMeshPermutation<Graphics.Pipeline.Shader.UMdf_NoShadow>>.TypeDesc);
                    if (ok1)
                    {
                        mDebugMesh = mesh2;

                        mDebugMesh.HostNode = this;

                        BoundVolume.LocalAABB = mDebugMesh.MaterialMesh.Mesh.mCoreObject.mAABB;

                        this.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;

                        UpdateAbsTransform();
                        UpdateAABB();
                        Parent?.UpdateAABB();
                    }
                }
                return mDebugMesh;
            }
        }
        public override void OnNodeLoaded(UNode parent)
        {
            base.OnNodeLoaded(parent);
            UpdateAbsTransform();

            var world = this.GetWorld();
            if (world != null)
            {
                world.DirectionLight = LightData.LightData;
            }
        }
        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.UMesh> meshes)
        {
            meshes.Add(mDebugMesh);
            foreach (var i in Children)
            {
                if (i.HitproxyType == Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent)
                    i.GetHitProxyDrawMesh(meshes);
            }
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            //if (UEngine.Instance.EditorInstance.Config.IsFilters(GamePlay.UWorld.UVisParameter.EVisCullFilter.LightDebug) == false)
            //    return;

            if ((rp.CullFilters & UWorld.UVisParameter.EVisCullFilter.LightDebug) == 0)
                return;

            if (DebugMesh != null)
                rp.VisibleMeshes.Add(mDebugMesh);
        }
        protected override void OnAbsTransformChanged()
        {
            var lightData = NodeData as UDirLightNodeData;
            if (lightData == null)
                return;

            lightData.LightData.Direction = Quaternion.RotateVector3(in Placement.AbsTransform.mQuat, in Vector3.UnitX);
            if (mDebugMesh == null)
                return;

            var world = this.GetWorld();
            var quat = Quaternion.Multiply(Quaternion.RotationAxis(in Vector3.Up, -(float)Math.PI * 0.5f), Placement.AbsTransform.mQuat);
            var trans = FTransform.CreateTransform(in Placement.AbsTransform.mPosition, in Placement.AbsTransform.mScale, quat);
            mDebugMesh.SetWorldTransform(in trans, world, false);
            //mDebugMesh.SetWorldTransform(in Placement.AbsTransform, world, false);
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

        public override void AddAssetReferences(IO.IAssetMeta ameta)
        {
            ameta.AddReferenceAsset(RName.GetRName("axis/movex.vms", RName.ERNameType.Engine));
            ameta.AddReferenceAsset(RName.GetRName("axis/axis_x_d.uminst", RName.ERNameType.Engine));
        }
    }
}
