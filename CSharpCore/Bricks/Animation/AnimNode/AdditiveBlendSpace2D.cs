using EngineNS.Bricks.Animation.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.AnimNode
{
    public class AdditiveBlendSpace2D : AdditiveBlendSpace
    {
        public async static Task<AdditiveBlendSpace2D> Create(RName name)
        {
            var abs = new AdditiveBlendSpace2D();
            if (await abs.Load(CEngine.Instance.RenderContext, name))
                return abs;
            return null;
        }
        public static AdditiveBlendSpace2D CreateSync(RName name)
        {
            var abs = new AdditiveBlendSpace2D();
            if (abs.SyncLoad(CEngine.Instance.RenderContext, name))
                return abs;
            return null;
        }
        public AdditiveBlendSpace2D()
        {
            mBlendAxises[0] = new BlendAxis("None");
            mBlendAxises[1] = new BlendAxis("None");
            mBlendAxises[2] = new BlendAxis("None");
        }
        ~AdditiveBlendSpace2D()
        {

        }
        public override void Tick()
        {
            base.Tick();
        }
        protected override void  GetRawSamplesFromBlendInput(Vector3 input, List<GridElementSample> gridSamples)
        {
            var griddingIuput = GriddingInput(input);
            var gridIndex = new Vector3((int)Math.Truncate(griddingIuput.X), (int)Math.Truncate(griddingIuput.Y), 0);
            var remainder = griddingIuput - gridIndex;

            GridElement EleLT = GetEditorElement((int)gridIndex.X, (int)gridIndex.Y + 1);
            var LeftTop = new GridElementSample();
            if (EleLT != null)
            {
                LeftTop.GridElement = EleLT;
                // now calculate weight - distance to each corner since input is already normalized within grid, we can just calculate distance 
                LeftTop.BlendWeight = (1.0f - remainder.X) * remainder.Y;
            }
            else
            {
                LeftTop.GridElement = new GridElement();
                LeftTop.BlendWeight = 0.0f;
            }
            gridSamples.Add(LeftTop);

            GridElement EleRT = GetEditorElement((int)gridIndex.X + 1, (int)gridIndex.Y + 1);
            var RightTop = new GridElementSample();
            if (EleRT != null)
            {
                RightTop.GridElement = EleRT;
                RightTop.BlendWeight = remainder.X * remainder.Y;
            }
            else
            {
                RightTop.GridElement = new GridElement();
                RightTop.BlendWeight = 0.0f;
            }
            gridSamples.Add(RightTop);

            GridElement EleLB = GetEditorElement((int)gridIndex.X, (int)gridIndex.Y);
            var LeftBottom = new GridElementSample();
            if (EleLB != null)
            {
                LeftBottom.GridElement = EleLB;
                LeftBottom.BlendWeight = (1.0f - remainder.X) * (1.0f - remainder.Y);
            }
            else
            {
                LeftBottom.GridElement = new GridElement();
                LeftBottom.BlendWeight = 0.0f;
            }
            gridSamples.Add(LeftBottom);

            GridElement EleRB = GetEditorElement((int)gridIndex.X + 1, (int)gridIndex.Y);
            var RightBottom = new GridElementSample();
            if (EleRB != null)
            {
                RightBottom.GridElement = EleRB;
                RightBottom.BlendWeight = remainder.X * (1.0f - remainder.Y);
            }
            else
            {
                RightBottom.GridElement = new GridElement();
                RightBottom.BlendWeight = 0.0f;
            }
            gridSamples.Add(RightBottom);

        }
        GridElement GetEditorElement(int XIndex, int YIndex)
        {
            int Index = XIndex * (mBlendAxises[1].GridNum + 1) + YIndex;
            if (mGridElements.Count > Index)
                return mGridElements[Index];
            return null;
        }
        protected override void ResampleData()
        {
            var axisX = mBlendAxises[0];
            var axisY = mBlendAxises[1];
            DelaunayTriangleMaker maker = new DelaunayTriangleMaker();
            BlendSpaceGrid grid = new BlendSpaceGrid();
            grid.SetGridInfo(axisX, axisY);
            maker.SetGridBox(axisX, axisY);
            for (int i = 0; i < mSamples.Count; ++i)
            {
                maker.AddSample(mSamples[i].Value, i);
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
    }
}
