using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    [UNode(NodeDataType = typeof(UGridNode.UGridNodeData), DefaultNamePrefix = "Grid")]
    public partial class UGridNode : UMeshNode
    {
        public class UGridNodeData : UNodeData
        {
        }

        public override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            if (data == null)
            {
                data = new UGridNodeData();
            }
            await base.InitializeNode(world, data, EBoundVolumeType.Box, placementType);
            SetStyle(ENodeStyles.DiscardAABB | ENodeStyles.VisibleFollowParent | ENodeStyles.Transient);

            //this.ViewportSlate = world.;
            return true;
        }
        public Graphics.Pipeline.UViewportSlate ViewportSlate;
        public Graphics.Pipeline.Shader.UMaterialInstance mGridlineMaterial;
        public Graphics.Mesh.UMdfGridUVMesh GridUVModifier;
        public static async Thread.Async.TtTask<UGridNode> AddGridNode(GamePlay.UWorld world, UNode parent)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var material = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("material/gridline.material", RName.ERNameType.Engine));
            var materialInstance = Graphics.Pipeline.Shader.UMaterialInstance.CreateMaterialInstance(material);
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
                //samp.Filter = NxRHI.ESamplerFilter.SPF_COMPARISON_MIN_MAG_LINEAR_MIP_POINT;
                samp.Filter = NxRHI.ESamplerFilter.SPF_MIN_MAG_LINEAR_MIP_POINT;
                samp.m_MaxLOD = float.MaxValue;

                materialInstance.UsedSamplerStates[0].Value = samp;
            }
            var gridColor = materialInstance.FindVar("GridColor");
            if (gridColor != null)
            {
                gridColor.SetValue(new Vector4(0.6f, 0.6f, 0.6f, 1));
            }
            var mesh = Graphics.Mesh.UMeshDataProvider.MakeGridPlane(rc, Vector2.Zero, Vector2.One, 10).ToMesh();

            var gridMesh = new Graphics.Mesh.TtMesh();
            var tMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
            tMaterials[0] = materialInstance;
            var ok = gridMesh.Initialize(mesh, tMaterials,
                Rtti.UTypeDescGetter<Graphics.Mesh.UMdfGridUVMesh>.TypeDesc);
            if (ok == false)
                return null;

            var data = new UGridNodeData();
            var scene = parent.GetNearestParentScene();
            var meshNode = await scene.NewNode(world, typeof(UGridNode), data, EBoundVolumeType.Box, typeof(UPlacement)) as UGridNode;
            meshNode.NodeData.Name = "GridLine";
            meshNode.Mesh = gridMesh;
            meshNode.Parent = parent;
            meshNode.mGridlineMaterial = materialInstance;
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;
            meshNode.SetStyle(ENodeStyles.VisibleFollowParent);

            meshNode.GridUVModifier = gridMesh.MdfQueue as Graphics.Mesh.UMdfGridUVMesh;

            return meshNode;
        }
        float SnapGridSize = 10.0f; //1,10,50... GEditor->GetGridSize();
        float mEditor3DGridFade = 0.5f;
        float mEditor2DGridFade = 0.5f;
        //private static RHI.FNameVarIndex ShaderIdx_SnapTile = new RHI.FNameVarIndex("SnapTile");
        //private static RHI.FNameVarIndex ShaderIdx_GridColor = new RHI.FNameVarIndex("GridColor");
        //private static RHI.FNameVarIndex ShaderIdx_UVMin = new RHI.FNameVarIndex("UVMin");
        //private static RHI.FNameVarIndex ShaderIdx_UVMax = new RHI.FNameVarIndex("UVMax");
        double WorldToUVScale = 0.0001f;
        public override bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
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
            if (bIsPerspective)
            {
                var gridColor = new EngineNS.Vector4(0.6f * Darken, 0.6f * Darken, 0.6f * Darken, mEditor3DGridFade);
                mGridlineMaterial.PerMaterialCBuffer.SetValue("GridColor", in gridColor);
            }
            else
            {
                var gridColor = new EngineNS.Vector4(0.6f * Darken, 0.6f * Darken, 0.6f * Darken, mEditor2DGridFade);
                mGridlineMaterial.PerMaterialCBuffer.SetValue("GridColor", in gridColor);
            }

            double SnapTile = (1.0 / WorldToUVScale) / System.Math.Max(1.0, SnapGridSize);
            mGridlineMaterial.PerMaterialCBuffer.SetValue("SnapTile", (float)SnapTile);

            var mPreCameraPos = ViewportSlate.RenderPolicy.DefaultCamera.mCoreObject.GetPosition();
            var UVCameraPos = new DVector2(mPreCameraPos.X, mPreCameraPos.Z);
            var ObjectToWorld = EngineNS.DMatrix.Identity;
            ObjectToWorld.Translation = new DVector3(mPreCameraPos.X, 0, mPreCameraPos.Z);

            // good enough to avoid the AMD artifacts, horizon still appears to be a line
            double Radii = 100000;
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

            var camPos = new DVector3(mPreCameraPos.X, 0, mPreCameraPos.Z);
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

        public override void GetHitProxyDrawMesh(List<Graphics.Mesh.TtMesh> meshes)
        {
            base.GetHitProxyDrawMesh(meshes);
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            base.OnGatherVisibleMeshes(rp);
        }
    }
}
