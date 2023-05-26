using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace EngineNS.Bricks.GpuDriven
{
    public class CompX : IComparer<BoundingBox>
    {
        int IComparer<BoundingBox>.Compare(BoundingBox x, BoundingBox y)
        {
            if (x.GetCenter().X < y.GetCenter().X)
                return -1;
            else
                return 1;
        }
    }

    public class CompY : IComparer<BoundingBox>
    {
        int IComparer<BoundingBox>.Compare(BoundingBox x, BoundingBox y)
        {
            if (x.GetCenter().Y < y.GetCenter().Y)
                return -1;
            else
                return 1;
        }
    }

    public class CompZ : IComparer<BoundingBox>
    {
        int IComparer<BoundingBox>.Compare(BoundingBox x, BoundingBox y)
        {
            if (x.GetCenter().Z < y.GetCenter().Z)
                return -1;
            else
                return 1;
        }
    }

    public class SurfaceAreaHeuristic
	{
        public static int sahSplit(BoundingBox[] aabbsIn, int splitGranularity, int indicesStart, int indicesEnd)
        {
            int numIndices = indicesEnd - indicesStart;
			float bestCost = 3.4E+38f; // Max float

			int bestAxis = -1;
			int bestIndex = -1;

			for (int splitAxis = 0; splitAxis< 3; ++splitAxis)
			{
                //https://learn.microsoft.com/zh-cn/dotnet/api/system.array.sort?view=net-7.0#system-array-sort(system-array-system-array-system-int32-system-int32-system-collections-icomparer)
                // Sort along center position
                if (splitAxis == 0)
				{
					Array.Sort(aabbsIn, indicesStart, numIndices, new CompX());
				}
				else if (splitAxis == 1)
				{
                    Array.Sort(aabbsIn, indicesStart, numIndices, new CompY());
                }
				else if (splitAxis == 2)
				{
                    Array.Sort(aabbsIn, indicesStart, numIndices, new CompZ());
                }


				float[] areasFromLeft = new float[numIndices];
				float[] areasFromRight = new float[numIndices];

				BoundingBox fromLeft = new BoundingBox();
				for (int i = 0; i<numIndices; ++i)
				{
					BoundingBox.Merge(fromLeft, aabbsIn[indicesStart + i]);
					areasFromLeft[i] = fromLeft.GetVolume();
				}

				BoundingBox fromRight = new BoundingBox();
				for (int i = numIndices - 1; i >= 0; --i)
				{
					BoundingBox.Merge(fromRight, aabbsIn[indicesStart + i]);
					areasFromRight[i] = fromLeft.GetVolume();
				}

				for (int splitIndex = splitGranularity; splitIndex < numIndices - splitGranularity; splitIndex += splitGranularity)
				{
					int countLeft = splitIndex;
					int countRight = numIndices - splitIndex;

					float areaLeft = areasFromLeft[splitIndex - 1];
					float areaRight = areasFromRight[splitIndex];
					float scaledAreaLeft = areaLeft * (float)countLeft;
					float scaledAreaRight = areaRight * (float)countRight;

					float cost = scaledAreaLeft + scaledAreaRight;

					if (cost < bestCost)
					{
						bestCost = cost;
						bestAxis = splitAxis;
						bestIndex = splitIndex;
					}
				}
			}

			// Sort again according to best axis
			// Sort along center position
		
            if (bestAxis == 0)
            {
                Array.Sort(aabbsIn, indicesStart, numIndices, new CompX());
            }
            else if (bestAxis == 1)
            {
                Array.Sort(aabbsIn, indicesStart, numIndices, new CompY());
            }
            else if (bestAxis == 2)
            {
                Array.Sort(aabbsIn, indicesStart, numIndices, new CompZ());
            }

            if (bestIndex == -1)
			{
				bestIndex = numIndices;
			}

			return bestIndex;
		}


     public static void generateBatchesRecursive(BoundingBox[] aabbsIn, int targetSize, int splitGranularity, int indicesStart, int indicesEnd, List<List<int>> result)
	{
			int splitIndex = sahSplit(aabbsIn, splitGranularity, indicesStart, indicesEnd);

			int[] range = {indicesStart, indicesStart + splitIndex, indicesEnd };

			for (int i = 0; i< 2; ++i)
			{
				int batchSize = range[i + 1] - range[i];
				if (batchSize <= 0)
				{
					continue;
				}
				else if (batchSize<targetSize)
				{
					List<int> Date = new List<int>();
					for (int j = range[i]; j <= range[i + 1]; j++)
					{
                        Date.Add(j);
                    }

					result.Add(Date);
                    //result.emplace(result.begin() + *range[i], batchSize);//Emplace TODO..
                    //result.Insert(range[i], batchSize);

                }
				else
				{
					generateBatchesRecursive(aabbsIn, targetSize, splitGranularity, range[i], range[i + 1], result);
				}
			}
	}


        public static List<List<int>> GenerateBatches(BoundingBox[] aabbs, int targetSize, int splitGranularity)
        {
            Vector<int> indices;

            List<List<int>> result = new List<List<int>>();
            generateBatchesRecursive(aabbs, targetSize, splitGranularity, 0, aabbs.Length, result);

            return result;
        }


    }

}
