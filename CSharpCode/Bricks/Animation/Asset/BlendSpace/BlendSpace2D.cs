using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset.BlendSpace
{
    [Rtti.Meta]
    public class TtBlendSpace2DAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtBlendSpace2D.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "Blend Space";
        }
    }


    public class TtBlendSpace2D : TtBlendSpace
    {
        public TtBlendSpace2D() 
        {
            BlendAxises[0] = new TtBlendSpace_Axis("None");
            BlendAxises[1] = new TtBlendSpace_Axis("None");
            BlendAxises[2] = new TtBlendSpace_Axis("None");
        }
        ~TtBlendSpace2D()
        {

        }

        protected override void ExtractWeightedTrianglesByInput(Vector3 input, List<FWeightedTriangle> gridSamples)
        {
            var griddingIuput = GriddingInput(input);
            var gridIndex = new Vector3((int)Math.Truncate(griddingIuput.X), (int)Math.Truncate(griddingIuput.Y), 0);
            var remainder = griddingIuput - gridIndex;

            TtBlentSpace_Triangle EleLT = GetEditorElement((int)gridIndex.X, (int)gridIndex.Y + 1);
            var LeftTop = new FWeightedTriangle();
            if (EleLT != null)
            {
                LeftTop.Triangle = EleLT;
                // now calculate weight - distance to each corner since input is already normalized within grid, we can just calculate distance 
                LeftTop.BlendWeight = (1.0f - remainder.X) * remainder.Y;
            }
            else
            {
                LeftTop.Triangle = new TtBlentSpace_Triangle();
                LeftTop.BlendWeight = 0.0f;
            }
            gridSamples.Add(LeftTop);

            TtBlentSpace_Triangle EleRT = GetEditorElement((int)gridIndex.X + 1, (int)gridIndex.Y + 1);
            var RightTop = new FWeightedTriangle();
            if (EleRT != null)
            {
                RightTop.Triangle = EleRT;
                RightTop.BlendWeight = remainder.X * remainder.Y;
            }
            else
            {
                RightTop.Triangle = new TtBlentSpace_Triangle();
                RightTop.BlendWeight = 0.0f;
            }
            gridSamples.Add(RightTop);

            TtBlentSpace_Triangle EleLB = GetEditorElement((int)gridIndex.X, (int)gridIndex.Y);
            var LeftBottom = new FWeightedTriangle();
            if (EleLB != null)
            {
                LeftBottom.Triangle = EleLB;
                LeftBottom.BlendWeight = (1.0f - remainder.X) * (1.0f - remainder.Y);
            }
            else
            {
                LeftBottom.Triangle = new TtBlentSpace_Triangle();
                LeftBottom.BlendWeight = 0.0f;
            }
            gridSamples.Add(LeftBottom);

            TtBlentSpace_Triangle EleRB = GetEditorElement((int)gridIndex.X + 1, (int)gridIndex.Y);
            var RightBottom = new FWeightedTriangle();
            if (EleRB != null)
            {
                RightBottom.Triangle = EleRB;
                RightBottom.BlendWeight = remainder.X * (1.0f - remainder.Y);
            }
            else
            {
                RightBottom.Triangle = new TtBlentSpace_Triangle();
                RightBottom.BlendWeight = 0.0f;
            }
            gridSamples.Add(RightBottom);
        }
        TtBlentSpace_Triangle GetEditorElement(int XIndex, int YIndex)
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
            TtDelaunayTriangleMaker maker = new TtDelaunayTriangleMaker();
            TtBlendSpaceGrid grid = new TtBlendSpaceGrid();
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
        public override string TypeExt { get => AssetExt; }
        [Rtti.Meta]
        public override RName AssetName { get; set; }
        public override IAssetMeta CreateAMeta()
        {
            var result = new TtBlendSpace2DAMeta();
            return result;
        }

        public override IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
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
