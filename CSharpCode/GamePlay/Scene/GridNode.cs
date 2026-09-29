using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [TtNode(NodeDataType = typeof(TtGridNode.TtGridNodeData), DefaultNamePrefix = "Grid")]
    [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.UGridNode@EngineCore", "EngineNS.GamePlay.Scene.UGridNode" })]
    public partial class TtGridNode : TtMeshNode
    {
        [Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.Scene.UGridNode.UGridNodeData@EngineCore", "EngineNS.GamePlay.Scene.UGridNode.UGridNodeData" })]
        public class TtGridNodeData : TtNodeData
        {
        }

        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new TtGridNodeData();
            }
            await base.InitializeNode(world, data, EBoundVolumeType.Box, placementType);
            SetStyle(ENodeStyles.DiscardAABB | ENodeStyles.VisibleAlways | ENodeStyles.Transient);

            //this.ViewportSlate = world.;
            return true;
        }
        public Graphics.Pipeline.TtViewportSlate ViewportSlate;
        public Graphics.Pipeline.Shader.TtMaterialInstance mGridlineMaterial;
        public Graphics.Mesh.UMdfGridUVMesh GridUVModifier;
        public static async Thread.Async.TtTask<TtGridNode> AddGridNode(GamePlay.TtWorld world, TtNode parent)
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var material = await RName.GetRName("material/gridline.material", RName.ERNameType.Engine).GetAsset<Graphics.Pipeline.Shader.TtMaterial>(); // TtEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("material/gridline.material", RName.ERNameType.Engine));
            var materialInstance = Graphics.Pipeline.Shader.TtMaterialInstance.CreateMaterialInstance(material);
            materialInstance.RenderLayer = Graphics.Pipeline.ERenderLayer.RL_PostTranslucent;
            unsafe
            {
                var rsState = materialInstance.Rasterizer;
                rsState.CullMode = NxRHI.ECullMode.CMD_NONE;
                materialInstance.Rasterizer = rsState;

                var blend0 = materialInstance.Blend;
                blend0.RenderTarget[0].BlendEnable = 1;
                materialInstance.Blend = blend0;

                var dsState = materialInstance.DepthStencil;
                dsState.DepthWriteMask = 0;
                materialInstance.DepthStencil = dsState;
            }
            if (materialInstance.UsedSamplerStates.Count > 0)
            {
                var samp = materialInstance.UsedSamplerStates[0].Value;
                // Trilinear filtering avoids abrupt mip transitions in the repeated grid pattern.
                samp.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_MIP_LINEAR;
                samp.m_MaxLOD = float.MaxValue;

                materialInstance.UsedSamplerStates[0].Value = samp;
            }
            var gridColor = materialInstance.FindVar("GridColor");
            if (gridColor != null)
            {
                gridColor.SetValue(new Vector4(0.6f, 0.6f, 0.6f, 1));
            }
            var mesh = Graphics.Mesh.TtMeshDataProvider.MakeGridPlane(rc, Vector2.Zero, Vector2.One, 10).ToMesh();

            var gridMesh = new Graphics.Mesh.TtRenderMesh();
            var tMaterials = new Graphics.Pipeline.Shader.TtMaterial[1];
            tMaterials[0] = materialInstance;
            var ok = gridMesh.Initialize(mesh, tMaterials,
                Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfGridUVMesh>.TypeDesc);
            if (ok == false)
                return null;

            var scene = parent.GetNearestParentScene();
            var meshNode = await GamePlay.Scene.TtNode.SpawnNode<TtGridNode>(parent, null, null, EBoundVolumeType.Box, typeof(TtPlacement)) as TtGridNode;
            meshNode.NodeData.Name = "GridLine";
            meshNode.RenderMesh = gridMesh;
            meshNode.Parent = parent;
            meshNode.mGridlineMaterial = materialInstance;
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;
            meshNode.SetStyle(ENodeStyles.VisibleAlways);

            meshNode.GridUVModifier = gridMesh.MdfQueue as Graphics.Mesh.UMdfGridUVMesh;

            return meshNode;
        }
        float SnapGridSize = 10.0f; //1,10,50... GEditor->GetGridSize();
        float mEditor3DGridFade = 0.5f;
        float mEditor2DGridFade = 0.5f;
        public double GridHeight { get; set; } = 0;
        public double GridRadius { get; set; } = 100000;
        public float GridFade { get; set; } = 0.5f;
        //private static RHI.FNameVarIndex ShaderIdx_SnapTile = new RHI.FNameVarIndex("SnapTile");
        //private static RHI.FNameVarIndex ShaderIdx_GridColor = new RHI.FNameVarIndex("GridColor");
        //private static RHI.FNameVarIndex ShaderIdx_UVMin = new RHI.FNameVarIndex("UVMin");
        //private static RHI.FNameVarIndex ShaderIdx_UVMax = new RHI.FNameVarIndex("UVMax");
        double WorldToUVScale = 0.0001f;
        public override Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtGridNode>.Scope;
        }
        public override bool IsNoTick 
        { 
            get => HasStyle(ENodeStyles.NoTick); 
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            if (mGridlineMaterial == null || mGridlineMaterial.PerMaterialCBuffer == null)
                return true;
            //bool bLarger1mGrid = true;
            
            //if (bLarger1mGrid)
            //{
            //    WorldToUVScale *= 0.1f;
            //}

            bool bIsPerspective = true;
            float Darken = 0.5f;
            var camera = ViewportSlate.RenderPolicy.DefaultCamera;
            var mPreCameraPos = camera.mCoreObject.GetPosition();
            float antiAliasFade = 1.0f;
            if (bIsPerspective)
            {
                // The stripe texture's last mip is 4 texels wide. Fade before a grid cell
                // becomes too small to sample that remaining periodic signal without aliasing.
                double cameraHeight = System.Math.Abs(mPreCameraPos.Y - GridHeight);
                double projectedHeight = 2.0 * System.Math.Tan(camera.Fov * 0.5f) * cameraHeight;
                if (projectedHeight > double.Epsilon && camera.Height > 0.0f)
                {
                    float pixelsPerGridCell = (float)(SnapGridSize * camera.Height / projectedHeight);
                    antiAliasFade = MathHelper.Clamp((pixelsPerGridCell - 2.0f) * 0.5f, 0.0f, 1.0f);
                    antiAliasFade = antiAliasFade * antiAliasFade * (3.0f - 2.0f * antiAliasFade);
                }

                var gridColor = new EngineNS.Vector4(0.6f * Darken, 0.6f * Darken, 0.6f * Darken, MathHelper.Min(mEditor3DGridFade, GridFade) * antiAliasFade);
                mGridlineMaterial.PerMaterialCBuffer.SetValue("GridColor", in gridColor);
            }
            else
            {
                var gridColor = new EngineNS.Vector4(0.6f * Darken, 0.6f * Darken, 0.6f * Darken, MathHelper.Min(mEditor2DGridFade, GridFade));
                mGridlineMaterial.PerMaterialCBuffer.SetValue("GridColor", in gridColor);
            }

            double SnapTile = (1.0 / WorldToUVScale) / System.Math.Max(1.0, SnapGridSize);
            mGridlineMaterial.PerMaterialCBuffer.SetValue("SnapTile", (float)SnapTile);
            var UVCameraPos = new DVector2(mPreCameraPos.X, mPreCameraPos.Z);
            var ObjectToWorld = EngineNS.DMatrix.Identity;
            ObjectToWorld.Translation = new DVector3(mPreCameraPos.X, 0, mPreCameraPos.Z);

            // good enough to avoid the AMD artifacts, horizon still appears to be a line
            double Radii = GridRadius;
            if (bIsPerspective)
            {
                // the higher we get the larger we make the geometry to give the illusion of an infinite grid while maintains the precision nearby
                Radii *= System.Math.Max(1.0, System.Math.Abs(mPreCameraPos.Y) / 1000.0);
            }

            DVector2 UVMid;
            UVMid.X = UVCameraPos.X * WorldToUVScale;
            UVMid.Y = UVCameraPos.Y * WorldToUVScale;
            double UVRadi = Radii * WorldToUVScale;

            var UVMin = UVMid + new DVector2(-UVRadi, -UVRadi);
            var UVMax = UVMid + new DVector2(UVRadi, UVRadi);

            if (GridUVModifier != null)
            {
                var min = UVMin.AsSingleVector();
                var max = UVMax.AsSingleVector();
                GridUVModifier.SetUVMinAndMax(in min, in max);
            }
            //mGridlineMaterial.PerMaterialCBuffer.SetValue("UVMin", UVMin.AsSingleVector());
            //mGridlineMaterial.PerMaterialCBuffer.SetValue("UVMax", UVMax.AsSingleVector());

            var camPos = new DVector3(mPreCameraPos.X, GridHeight, mPreCameraPos.Z);
            if (this.Placement.Position != camPos)
            {
                this.Placement.Position = camPos;
                this.Placement.Scale = new Vector3((float)Radii);
            }
            return true;
        }

        public override void AddAssetReferences(IO.IAssetMeta ameta)
        {
            ameta.AddReferenceAsset(RName.GetRName("material/gridline.material", RName.ERNameType.Engine));
        }

        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtRenderMesh> meshes)
        {
            base.GetHitProxyDrawMesh(meshes);
        }
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            // GridLine 是 VisibleAlways + DiscardAABB, 不走按 AABB 剔除那条路上的编辑器
            // 可见性过滤, 所以这里显式再挡一次 —— Outliner 里网格那个小眼睛靠它生效。
            if (rp.UseEditorVisibilityFilter && IsEditorVisibleInHierarchy == false)
                return;
            base.OnGatherVisibleMeshes(rp);
        }
    }
}
