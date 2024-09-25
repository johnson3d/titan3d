﻿using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Animation.Asset.BlendSpace
{
    public struct FIndexLinePoint
    {
        public float Position;
        public int Index;
        public FIndexLinePoint(float inPosition, int index)
        {
            Position = inPosition;
            Index = index;
        }
    };
    public class TtLineElement
    {
        public FIndexLinePoint Start;
        public FIndexLinePoint End;
        public bool IsFirst = false;
        public bool IsLast = false;
        public float Range = 0;
        // Explicit constructor since we need to populate the Range value
        public TtLineElement(FIndexLinePoint start, FIndexLinePoint end)
        {
            Start = start;
            End = end;
            IsFirst = false;
            IsLast = false;
            Range = End.Position - Start.Position;
        }

        public bool PopulateElement(float ElementPosition, ref TtBlentSpace_Triangle InOutElement)
        {
            // If the element is left of the line element
            if (ElementPosition < Start.Position)
            {
                // Element can only be left of a point if it is the first one (otherwise line is incorrect)
                if (!IsFirst)
                {
                    return false;
                }

                InOutElement.Indices[0] = Start.Index;
                InOutElement.Weights[0] = 1.0f;
                return true;
            }
            // If the element is right of the line element
            else if (ElementPosition > End.Position)
            {
                // Element can only be right of a point if it is the last one (otherwise line is incorrect)
                if (!IsLast)
                {
                    return false;
                }
                InOutElement.Indices[0] = End.Index;
                InOutElement.Weights[0] = 1.0f;
                return true;
            }
            else
            {
                // If the element is between the start/end point of the line weight according to where it's closest to
                InOutElement.Indices[0] = End.Index;
                InOutElement.Weights[0] = (ElementPosition - Start.Position) / Range;

                InOutElement.Indices[1] = Start.Index;
                InOutElement.Weights[1] = (1.0f - InOutElement.Weights[0]);
                return true;
            }
        }
        bool IsBlendInputOnLine(Vector3 BlendInput)
        {
            return (BlendInput.X >= Start.Position) && (BlendInput.X <= End.Position);
        }
    }

    [Rtti.Meta]
    public class TtBlendSpace1DAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtBlendSpace1D.AssetExt;
        }
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //必须是TextureAsset
            return true;
        }
        public override string GetAssetTypeName()
        {
            return "Blend Space 1D";
        }
    }

    public class TtBlendSpace1D : TtBlendSpace
    {
        public TtBlendSpace1D() 
        {
            BlendAxises[0] = new TtBlendSpace_Axis("None");
            BlendAxises[1] = new TtBlendSpace_Axis("None");
            BlendAxises[2] = new TtBlendSpace_Axis("None");
        }
        ~TtBlendSpace1D()
        {

        }

        protected override void ReConstructTrangles()
        {
            if (AnimPoints.Count == 0)
                return;
            var axis = BlendAxises[0];
            var samplePointList = CalculateBlendSpacePoints();
            int[] PointListToSampleIndices = new int[samplePointList.Count];
            //PointListToSampleIndices.Init(INDEX_NONE, ElementGenerator.SamplePointList.Num());
            for (int PointIndex = 0; PointIndex < samplePointList.Count; ++PointIndex)
            {
                float Point = samplePointList[PointIndex].Value.X;
                for (int SampleIndex = 0; SampleIndex < AnimPoints.Count; ++SampleIndex)
                {
                    if (AnimPoints[SampleIndex].Value.X == Point)
                    {
                        PointListToSampleIndices[PointIndex] = SampleIndex;
                        break;
                    }
                }
            }
            var tempGridElemnet = new List<TtBlentSpace_Triangle>();
            for (int i = 0; i < GridTriangles.Count; ++i)
            {
                var tempGrid = new TtBlentSpace_Triangle(3);
                for (int j = 0; j < 3; ++j)
                {
                    tempGrid.Indices[j] = GridTriangles[i].Indices[j];
                    tempGrid.Weights[j] = GridTriangles[i].Weights[j];
                }
                tempGridElemnet.Add(tempGrid);
            }
            FillTheGrid(PointListToSampleIndices, tempGridElemnet);
        }

        protected override void ExtractWeightedTrianglesByInput(Vector3 input, List<FWeightedTriangle> gridSamples)
        {
            var griddingIuput = GriddingInput(input);
            var gridIndex = (int)Math.Truncate(griddingIuput.X);
            var remainder = griddingIuput.X - gridIndex;
            var beforeIndex = gridIndex;
            if (GridTriangles.Count > beforeIndex)
            {
                FWeightedTriangle newSample = new FWeightedTriangle();
                newSample.Triangle = GridTriangles[beforeIndex];
                newSample.BlendWeight = 1 - remainder;
                gridSamples.Add(newSample);
            }
            else
            {
                FWeightedTriangle newSample = new FWeightedTriangle();
                newSample.Triangle = new TtBlentSpace_Triangle(3);
                newSample.BlendWeight = 0;
                gridSamples.Add(newSample);
            }

            var afterIndex = gridIndex + 1;
            if (GridTriangles.Count > afterIndex)
            {
                FWeightedTriangle newSample = new FWeightedTriangle();
                newSample.Triangle = GridTriangles[afterIndex];
                newSample.BlendWeight = remainder;
                gridSamples.Add(newSample);
            }
            else
            {
                FWeightedTriangle newSample = new FWeightedTriangle();
                newSample.Triangle = new TtBlentSpace_Triangle(3);
                newSample.BlendWeight = 0;
                gridSamples.Add(newSample);
            }
        }

        protected List<TtBlendSpace_Point> CalculateBlendSpacePoints()
        {
            List<TtBlendSpace_Point> samplePoints = new List<TtBlendSpace_Point>();
            List<TtLineElement> lineElements = new List<TtLineElement>();
            GridTriangles.Clear();
            for (int i = 0; i < AnimPoints.Count; ++i)
            {
                samplePoints.Add(new TtBlendSpace_Point(AnimPoints[i].Animation, AnimPoints[i].Value));
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
                    var startPoint = new FIndexLinePoint(samplePoints[index].Value.X, index);
                    var endPoint = new FIndexLinePoint(samplePoints[endIndex].Value.X, endIndex);
                    lineElements.Add(new TtLineElement(startPoint, endPoint));
                }
                lineElements[0].IsFirst = true;
                lineElements[lineElements.Count - 1].IsLast = true;
            }

            var gridRange = BlendAxises[0].Max - BlendAxises[0].Min;
            var gridSize = gridRange / BlendAxises[0].GridNum;

            for (int i = 0; i < BlendAxises[0].GridNum + 1; ++i)
            {
                GridTriangles.Add(new TtBlentSpace_Triangle(3));
            }

            if (lineElements.Count == 0)
            {
                for (int i = 0; i < GridTriangles.Count; ++i)
                {
                    GridTriangles[i].Indices[0] = 0;
                    GridTriangles[i].Weights[0] = 1.0f;
                }
            }
            else
            {
                for (int elementIndex = 0; elementIndex < BlendAxises[0].GridNum + 1; ++elementIndex)
                {
                    var element = GridTriangles[elementIndex];
                    var posIngrid = gridSize * elementIndex + BlendAxises[0].Min;
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
        #region IAnimationAsset
        public const string AssetExt = ".blendspace1d";
        public override string TypeExt { get => AssetExt; }
        [Rtti.Meta]
        public override RName AssetName { get; set; }
        public override IAssetMeta CreateAMeta()
        {
            var result = new TtBlendSpace1DAMeta();
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
        #endregion IAnimationAsset
    }
}
