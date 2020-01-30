using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Animation.AnimNode
{
    public class AdditiveBlendSpace1D: AdditiveBlendSpace
    {
        public async static Task<AdditiveBlendSpace1D> Create(RName name)
        {
            var abs = new AdditiveBlendSpace1D();
            if (await abs.Load(CEngine.Instance.RenderContext, name))
                return abs;
            return null;
        }
        public static AdditiveBlendSpace1D CreateSync(RName name)
        {
            var abs = new AdditiveBlendSpace1D();
            if (abs.SyncLoad(CEngine.Instance.RenderContext, name))
                return abs;
            return null;
        }
        public AdditiveBlendSpace1D()
        {
            mBlendAxises[0] = new BlendAxis("None");
            mBlendAxises[1] = new BlendAxis("None");
            mBlendAxises[2] = new BlendAxis("None");
        }
        ~AdditiveBlendSpace1D()
        {

        }
        public override void Tick()
        {
            base.Tick();
        }

        //CGfxAnimationSequence GetGridElement(int index)
        //{
        //    if(mGrids[index] != -1)
        //    {
        //        return mAnimationElement[mGrids[index]];
        //    }
        //    return null;
        //}

        protected override void ResampleData()
        {
            if (mSamples.Count == 0)
                return;
            var axis = mBlendAxises[0];
            var samplePointList = CalculateElements();
            int[] PointListToSampleIndices = new int[samplePointList.Count];
            //PointListToSampleIndices.Init(INDEX_NONE, ElementGenerator.SamplePointList.Num());
            for (int PointIndex = 0; PointIndex < samplePointList.Count; ++PointIndex)
            {
                float Point = samplePointList[PointIndex].Value.X;
                for (int SampleIndex = 0; SampleIndex < mSamples.Count; ++SampleIndex)
                {
                    if (mSamples[SampleIndex].Value.X == Point)
                    {
                        PointListToSampleIndices[PointIndex] = SampleIndex;
                        break;
                    }
                }
            }
            var tempGridElemnet = new List<GridElement>();
            for (int i = 0; i < mGridElements.Count; ++i)
            {
                var tempGrid = new GridElement(3);
                for (int j = 0; j < 3; ++j)
                {
                    tempGrid.Indices[j] = mGridElements[i].Indices[j];
                    tempGrid.Weights[j] = mGridElements[i].Weights[j];
                }
                tempGridElemnet.Add(tempGrid);
            }
            FillTheGrid(PointListToSampleIndices, tempGridElemnet);
        }

        protected override void GetRawSamplesFromBlendInput(Vector3 input, List<GridElementSample> gridSamples)
        {
            var griddingIuput = GriddingInput(input);
            var gridIndex = (int)Math.Truncate(griddingIuput.X);
            var remainder = griddingIuput.X - gridIndex;
            var beforeIndex = gridIndex;
            if (mGridElements.Count > beforeIndex)
            {
                GridElementSample newSample = new GridElementSample();
                newSample.GridElement = mGridElements[beforeIndex];
                newSample.BlendWeight = 1 - remainder;
                gridSamples.Add(newSample);
            }
            else
            {
                GridElementSample newSample = new GridElementSample();
                newSample.GridElement = new GridElement(3);
                newSample.BlendWeight = 0;
                gridSamples.Add(newSample);
            }

            var afterIndex = gridIndex + 1;
            if (mGridElements.Count > afterIndex)
            {
                GridElementSample newSample = new GridElementSample();
                newSample.GridElement = mGridElements[afterIndex];
                newSample.BlendWeight = remainder;
                gridSamples.Add(newSample);
            }
            else
            {
                GridElementSample newSample = new GridElementSample();
                newSample.GridElement = new GridElement(3);
                newSample.BlendWeight = 0;
                gridSamples.Add(newSample);
            }
        }

        protected List<AnimationSample> CalculateElements()
        {
            List<AnimationSample> samplePoints = new List<AnimationSample>();
            List<LineElement> lineElements = new List<LineElement>();
            mGridElements.Clear();
            for (int i = 0; i < mSamples.Count; ++i)
            {
                samplePoints.Add(new AnimationSample(mSamples[i].Animation, mSamples[i].Value));
            }
            if (samplePoints.Count > 1)
            {
                samplePoints.Sort((a, b) =>
                {
                    if (a.Value.X > b.Value.X)
                        return 1;
                    if (a.Value.X == b.Value.X)
                        return 0;
                    else
                        return -1;
                });

                for (int index = 0; index < samplePoints.Count - 1; ++index)
                {
                    var endIndex = index + 1;
                    var startPoint = new IndexLinePoint(samplePoints[index].Value.X, index);
                    var endPoint = new IndexLinePoint(samplePoints[endIndex].Value.X, endIndex);
                    lineElements.Add(new LineElement(startPoint, endPoint));
                }
                lineElements[0].IsFirst = true;
                lineElements[lineElements.Count - 1].IsLast = true;
            }

            var gridRange = mBlendAxises[0].Max - mBlendAxises[0].Min;
            var gridSize = gridRange / mBlendAxises[0].GridNum;

            for (int i = 0; i < mBlendAxises[0].GridNum + 1; ++i)
            {
                mGridElements.Add(new GridElement(3));
            }

            if (lineElements.Count == 0)
            {
                for (int i = 0; i < mGridElements.Count; ++i)
                {
                    mGridElements[i].Indices[0] = 0;
                    mGridElements[i].Weights[0] = 1.0f;
                }
            }
            else
            {
                for (int elementIndex = 0; elementIndex < mBlendAxises[0].GridNum + 1; ++elementIndex)
                {
                    var element = mGridElements[elementIndex];
                    var posIngrid = gridSize * elementIndex + mBlendAxises[0].Min;
                    // Try and populate the editor element
                    bool bPopulatedElement = false;
                    for (int i = 0; i < lineElements.Count; ++i)
                    {
                        bPopulatedElement |= lineElements[i].PopulateElement(posIngrid, ref element);
                        if (bPopulatedElement)
                        {
                            break;
                        }
                    }

                    // Ensure that the editor element is populated using the available sample data
                }
            }
            return samplePoints;
        }
    }
}
