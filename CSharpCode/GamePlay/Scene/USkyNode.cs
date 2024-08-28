using EngineNS.Graphics.Pipeline;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace EngineNS.GamePlay.Scene
{
    //https://zhuanlan.zhihu.com/p/621412675
    [Bricks.CodeBuilder.ContextMenu("Sky", "Sky", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtSkyNode.TtSkyNodeData), DefaultNamePrefix = "Sky")]
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.USkyNode@EngineCore" })]
    public class TtSkyNode : TtMeshNode
    {
        [Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.Scene.USkyNode.TtSkyNodeData@EngineCore" })]
        public class TtSkyNodeData : TtMeshNodeData
        {
            public TtSkyNodeData()
            {
                SunMaterialName = RName.GetRName("material/default_sun.uminst", RName.ERNameType.Engine);
                SunDirection = new Vector3(1,1,1);
                SunDirection.Normalize();
            }
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)]
            public RName SunMaterialName { get; set; }
            [Rtti.Meta]
            public Vector3 SunDirection { get; set; }
        }
        [Category("Option")]
        [Rtti.Meta]
        public Vector3 SunDirection
        {
            get => GetNodeData<TtSkyNodeData>().SunDirection;
            set => GetNodeData<TtSkyNodeData>().SunDirection = value;
        }
        Graphics.Pipeline.Shader.TtMaterialInstance SunMaterial;
        [Category("Option")]
        [Rtti.Meta]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtMaterialInstance.AssetExt)]
        public RName SunMaterialName
        {
            get => GetNodeData<TtSkyNodeData>().SunMaterialName;
            set
            {
                GetNodeData<TtSkyNodeData>().SunMaterialName = value;
                var action = async () =>
                {
                    SunMaterial = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(value);
                };
                action();
            }
        }

        public Graphics.Mesh.TtMesh SunMesh = new Graphics.Mesh.TtMesh();
        public override async Thread.Async.TtTask<bool> InitializeNode(UWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var skyData = data as TtSkyNodeData;
            if (skyData == null)
            {
                skyData = new TtSkyNodeData();
                var meta = Rtti.TtClassMetaManager.Instance.GetMeta(Rtti.UTypeDescGetter<TtSkyNodeData>.TypeDesc);
                meta.CopyObjectMetaField(skyData, data);
                skyData.SunMaterialName = RName.GetRName("material/default_sun.uminst", RName.ERNameType.Engine);
                data = skyData;
                SunMaterial = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(skyData.SunMaterialName);
            }
            else
            {
                SunMaterial = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(skyData.SunMaterialName);
            }
            await base.InitializeNode(world, data, bvType, placementType);
            var rect = Graphics.Mesh.UMeshDataProvider.MakeRect2D(-0.5f, -0.5f, 1, 1, 0);
            var rectMesh = rect.ToMesh();
            var materials = new Graphics.Pipeline.Shader.TtMaterialInstance[1];
            materials[0] = SunMaterial;
            SunMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            SunMesh.UserShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<Graphics.Pipeline.Shader.CommanShading.UDrawViewportShading>();
            return true;
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);

            if (SunMesh != null)
            {
                //rp.VisibleMeshes.Add(SunMesh);
            }
        }
        public override bool OnTickLogic(UWorld world, TtRenderPolicy policy)
        {
            SunMesh.IsUnlit = true;
            this.Mesh.IsUnlit = true;

            var camPos = policy.DefaultCamera.GetPosition();
            camPos = new DVector3(camPos.X, Placement.TransformRef.mPosition.Y, camPos.Z);
            if (Placement.Position != camPos)
            {
                Placement.Position = camPos;
            }
            //var transform = FTransform.CreateTransform(camPos + SunDirection * 800, new Vector3(80.0f), in Quaternion.Identity);
            //SunMesh.SetWorldTransform(in transform, world, false);
            var matrix = policy.DefaultCamera.GetJitterViewProjectionInverse();
            SunMesh.DirectSetWorldMatrix(in matrix);

            return base.OnTickLogic(world, policy);
        }
    }
}
