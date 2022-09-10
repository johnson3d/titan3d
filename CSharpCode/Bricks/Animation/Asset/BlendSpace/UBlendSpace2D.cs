using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset.BlendSpace
{
    [Rtti.Meta]
    public class UBlendSpace2DAMeta : IO.IAssetMeta
    {
        public override string GetAssetExtType()
        {
            return UAnimationClip.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
    }


    public class UBlendSpace2D : UBlendSpace
    {
        public UBlendSpace2D() 
        {
            BlendAxises[0] = new UBlendSpace_Axis("None");
            BlendAxises[1] = new UBlendSpace_Axis("None");
            BlendAxises[2] = new UBlendSpace_Axis("None");
        }
        ~UBlendSpace2D()
        {

        }

        protected override void ExtractWeightedTrianglesByInput(Vector3 input, List<WeightedTriangle> gridSamples)
        {
            var griddingIuput = GriddingInput(input);
            var gridIndex = new Vector3((int)Math.Truncate(griddingIuput.X), (int)Math.Truncate(griddingIuput.Y), 0);
            var remainder = griddingIuput - gridIndex;

            UBlentSpace_Triangle EleLT = GetEditorElement((int)gridIndex.X, (int)gridIndex.Y + 1);
            var LeftTop = new WeightedTriangle();
            if (EleLT != null)
            {
                LeftTop.Triangle = EleLT;
                // now calculate weight - distance to each corner since input is already normalized within grid, we can just calculate distance 
                LeftTop.BlendWeight = (1.0f - remainder.X) * remainder.Y;
            }
            else
            {
                LeftTop.Triangle = new UBlentSpace_Triangle();
                LeftTop.BlendWeight = 0.0f;
            }
            gridSamples.Add(LeftTop);

            UBlentSpace_Triangle EleRT = GetEditorElement((int)gridIndex.X + 1, (int)gridIndex.Y + 1);
            var RightTop = new WeightedTriangle();
            if (EleRT != null)
            {
                RightTop.Triangle = EleRT;
                RightTop.BlendWeight = remainder.X * remainder.Y;
            }
            else
            {
                RightTop.Triangle = new UBlentSpace_Triangle();
                RightTop.BlendWeight = 0.0f;
            }
            gridSamples.Add(RightTop);

            UBlentSpace_Triangle EleLB = GetEditorElement((int)gridIndex.X, (int)gridIndex.Y);
            var LeftBottom = new WeightedTriangle();
            if (EleLB != null)
            {
                LeftBottom.Triangle = EleLB;
                LeftBottom.BlendWeight = (1.0f - remainder.X) * (1.0f - remainder.Y);
            }
            else
            {
                LeftBottom.Triangle = new UBlentSpace_Triangle();
                LeftBottom.BlendWeight = 0.0f;
            }
            gridSamples.Add(LeftBottom);

            UBlentSpace_Triangle EleRB = GetEditorElement((int)gridIndex.X + 1, (int)gridIndex.Y);
            var RightBottom = new WeightedTriangle();
            if (EleRB != null)
            {
                RightBottom.Triangle = EleRB;
                RightBottom.BlendWeight = remainder.X * (1.0f - remainder.Y);
            }
            else
            {
                RightBottom.Triangle = new UBlentSpace_Triangle();
                RightBottom.BlendWeight = 0.0f;
            }
            gridSamples.Add(RightBottom);
        }
        UBlentSpace_Triangle GetEditorElement(int XIndex, int YIndex)
        {
            int Index = XIndex * (BlendAxises[1].GridNum + 1) + YIndex;
            if (GridTriangles.Count > Index)
                return GridTriangles[Index];
            return null;
        }
        protected override void ReConstructTrangles()
        {
            var axisX = BlendAxises[0];
            var axisY = BlendAxises[1];
            DelaunayTriangleMaker maker = new DelaunayTriangleMaker();
            BlendSpaceGrid grid = new BlendSpaceGrid();
            grid.SetGridInfo(axisX, axisY);
            maker.SetGridBox(axisX, axisY);
            for (int i = 0; i < AnimPoints.Count; ++i)
            {
                maker.AddSample(AnimPoints[i].Value, i);
            }

            maker.Triangulate();

            var points = maker.GetSamplePointList();
            var triangles = maker.GetTriangleList();
            grid.GenerateGridElements(points, triangles);
            if (triangles.Count > 0)
            {
                var gridElements = grid.GetElements();
                FillTheGrid(maker.GetIndiceMapping().ToArray(), gridElements);
            }
        }
        #region IAnimationAsset
        public const string AssetExt = ".blendspace2d";
        [Rtti.Meta]
        public override RName AssetName { get; set; }
        public override IAssetMeta CreateAMeta()
        {
            var result = new UBlendSpace2DAMeta();
            return result;
        }

        public override IAssetMeta GetAMeta()
        {
            return UEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }

        public override void UpdateAMetaReferences(IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }

        public override void SaveAssetTo(RName name)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
