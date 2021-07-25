using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public partial class UGridNode : UMeshNode
    {
        public class UGridNodeData : UNodeData
        {
        }

        public UGridNode(UNodeData data, EBoundVolumeType bvType, Type placementType)
            : base(data, bvType, placementType)
        {

        }
        public Graphics.Pipeline.UViewportSlate ViewportSlate;
        public Graphics.Pipeline.Shader.UMaterialInstance mGridlineMaterial;
        public static async System.Threading.Tasks.Task<UGridNode> AddGridNode(UNode parent)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var material = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("material/gridline.material", RName.ERNameType.Engine));
            var materialInstance = Graphics.Pipeline.Shader.UMaterialInstance.CreateMaterialInstance(material);
            var mesh = Graphics.Mesh.CMeshDataProvider.MakeGridPlane(rc, Vector2.Zero, Vector2.One, 10).ToMesh();

            var gridMesh = new Graphics.Mesh.UMesh();
            var tMaterials = new Graphics.Pipeline.Shader.UMaterial[1];
            tMaterials[0] = materialInstance;
            var ok = gridMesh.Initialize(mesh, tMaterials, Rtti.UTypeDesc.TypeOf(typeof(Graphics.Mesh.UMdfStaticMesh_NoShadow)));
            if (ok == false)
                return null;

            var data = new UGridNodeData();
            var scene = parent.GetNearestParentScene();
            var meshNode = scene.NewNode(typeof(UGridNode), data, EBoundVolumeType.Box, typeof(UPlacement)) as UGridNode;
            meshNode.NodeData.Name = "GridLine";
            meshNode.Mesh = gridMesh;
            meshNode.Parent = parent;
            meshNode.mGridlineMaterial = materialInstance;
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;
            meshNode.SetStyle(ENodeStyles.VisibleFollowParent);

            return meshNode;
        }
        float SnapGridSize = 10.0f; //1,10,50... GEditor->GetGridSize();
        float mEditor3DGridFade = 0.5f;
        float mEditor2DGridFade = 0.5f;
        public override bool OnTickLogic()
        {
            if (mGridlineMaterial == null || mGridlineMaterial.PerMaterialCBuffer == null)
                return true;
            bool bLarger1mGrid = true;
            float WorldToUVScale = 0.001f;

            if (bLarger1mGrid)
            {
                WorldToUVScale *= 0.1f;
            }

            var ShaderIdx_SnapTile = mGridlineMaterial.PerMaterialCBuffer.mCoreObject.FindVar("SnapTile");
            var ShaderIdx_GridColor = mGridlineMaterial.PerMaterialCBuffer.mCoreObject.FindVar("GridColor");
            var ShaderIdx_UVMin = mGridlineMaterial.PerMaterialCBuffer.mCoreObject.FindVar("UVMin");
            var ShaderIdx_UVMax = mGridlineMaterial.PerMaterialCBuffer.mCoreObject.FindVar("UVMax");

            float Darken = 0.11f;
            bool bIsPerspective = true;
            if (bIsPerspective)
            {
                var gridColor = new EngineNS.Vector4(0.6f * Darken, 0.6f * Darken, 0.6f * Darken, mEditor3DGridFade);
                mGridlineMaterial.PerMaterialCBuffer.SetValue(ShaderIdx_GridColor, ref gridColor);
            }
            else
            {
                var gridColor = new EngineNS.Vector4(0.6f * Darken, 0.6f * Darken, 0.6f * Darken, mEditor2DGridFade);
                mGridlineMaterial.PerMaterialCBuffer.SetValue(ShaderIdx_GridColor, ref gridColor);
            }

            float SnapTile = (1.0f / WorldToUVScale) / System.Math.Max(1.0f, SnapGridSize);
            mGridlineMaterial.PerMaterialCBuffer.SetValue(ShaderIdx_SnapTile, ref SnapTile);

            var mPreCameraPos = ViewportSlate.RenderPolicy.GetBasePassNode().GBuffers.Camera.mCoreObject.GetPosition();
            var UVCameraPos = new EngineNS.Vector2(mPreCameraPos.X, mPreCameraPos.Z);
            var ObjectToWorld = EngineNS.Matrix.Identity;
            ObjectToWorld.SetTrans(new Vector3(mPreCameraPos.X, 0, mPreCameraPos.Z));

            // good enough to avoid the AMD artifacts, horizon still appears to be a line
            float Radii = 1000;
            if (bIsPerspective)
            {
                // the higher we get the larger we make the geometry to give the illusion of an infinite grid while maintains the precision nearby
                Radii *= System.Math.Max(1.0f, System.Math.Abs(mPreCameraPos.Y) / 10.0f);
            }

            var UVMid = UVCameraPos * WorldToUVScale;
            float UVRadi = Radii * WorldToUVScale;

            var UVMin = UVMid + new Vector2(-UVRadi, -UVRadi);
            var UVMax = UVMid + new Vector2(UVRadi, UVRadi);

            mGridlineMaterial.PerMaterialCBuffer.SetValue(ShaderIdx_UVMin, ref UVMin);
            mGridlineMaterial.PerMaterialCBuffer.SetValue(ShaderIdx_UVMax, ref UVMax);

            this.Placement.Position = new Vector3(mPreCameraPos.X, 0, mPreCameraPos.Z);
            this.Placement.Scale = new Vector3(Radii);
            return true;
        }
    }
}
