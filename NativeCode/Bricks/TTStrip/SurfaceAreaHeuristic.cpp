#include "SurfaceAreaHeuristic.h"
#include "../../Base/BaseHead.h"

bool compX(const v3dxBox3& i0, const v3dxBox3& i1)
{
	return i0.GetCenter().X < i1.GetCenter().X;
}

bool compY(const v3dxBox3& i0, const v3dxBox3& i1)
{
	return i0.GetCenter().Y < i1.GetCenter().Y;
}

bool compZ(const v3dxBox3& i0, const v3dxBox3& i1)
{
	return i0.GetCenter().Z < i1.GetCenter().Z;
}

UINT sahSplit(const std::vector<v3dxBox3>& aabbsIn, UINT splitGranularity, UINT* indicesStart, UINT* indicesEnd)
{
	UINT numIndices = UINT(indicesEnd - indicesStart);

	float bestCost = 3.4E+38f; // Max float

	int bestAxis = -1;
	int bestIndex = -1;

	for (int splitAxis = 0; splitAxis < 3; ++splitAxis)
	{
		// Sort along center position
		//if (splitAxis == 0)
		//{
		//	std::sort(aabbsIn.begin() + *indicesStart, aabbsIn.begin() + numIndices, compX);
		//}
		//else if (splitAxis == 1)
		//{
		//	std::stable_sort(aabbsIn.begin() + *indicesStart, aabbsIn.begin() + numIndices, compX);
		//}
		//else if (splitAxis == 2)
		//{
		//	std::stable_sort(aabbsIn.begin() + *indicesStart, aabbsIn.begin() + numIndices, compZ);
		//}
		

		std::vector<float> areasFromLeft;
		areasFromLeft.resize(numIndices);

		std::vector<float> areasFromRight;
		areasFromRight.resize(numIndices);

		v3dxBox3 fromLeft;
		for (UINT i = 0; i < numIndices; ++i)
		{
			fromLeft += (aabbsIn[indicesStart[i]]);
			areasFromLeft[i] = fromLeft.GetSurface();
		}

		v3dxBox3 fromRight;
		for (int i = numIndices - 1; i >= 0; --i)
		{
			fromRight +=  (aabbsIn[indicesStart[i]]);
			areasFromRight[i] = fromLeft.GetSurface();
		}

		for (UINT splitIndex = splitGranularity; splitIndex < numIndices - splitGranularity; splitIndex += splitGranularity)
		{
			int countLeft = static_cast<int>(splitIndex);
			int countRight = static_cast<int>(numIndices - splitIndex);

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
	//if (bestAxis == 0)
	//{
	//	std::stable_sort(aabbsIn.begin() + *indicesStart, aabbsIn.begin() + numIndices, compX);
	//}
	//else if (bestAxis == 1)
	//{
	//	std::stable_sort(aabbsIn.begin() + *indicesStart, aabbsIn.begin() + numIndices, compY);
	//}
	//else if (bestAxis == 2)
	//{
	//	std::stable_sort(aabbsIn.begin() + *indicesStart, aabbsIn.begin() + numIndices, compZ);
	//}

	if (bestIndex == -1)
	{
		bestIndex = numIndices;
	}

	return bestIndex;
}

void generateBatchesRecursive(const std::vector<v3dxBox3>& aabbsIn, UINT targetSize, UINT splitGranularity, UINT* indicesStart, UINT* indicesEnd, std::vector<std::vector<UINT>>& result)
{
	auto splitIndex = sahSplit(aabbsIn, splitGranularity, indicesStart, indicesEnd);

	UINT* range[] = { indicesStart, indicesStart + splitIndex, indicesEnd };

	for (int i = 0; i < 2; ++i)
	{
		auto batchSize = range[i + 1] - range[i];
		if (batchSize <= 0)
		{
			continue;
		}
		else if (batchSize < targetSize)
		{
			result.emplace(result.begin() + *range[i], batchSize);//Emplace
		}
		else
		{
			generateBatchesRecursive(aabbsIn, targetSize, splitGranularity, range[i], range[i + 1], result);
		}
	}
}

std::vector<std::vector<UINT>> SurfaceAreaHeuristic::generateBatches(const std::vector<v3dxBox3>& Aabbs, UINT targetSize, UINT splitGranularity)
{
	std::vector<UINT> indices;
	indices.resize(Aabbs.size());
	for (int32_t i = 0; i < indices.size(); ++i)
	{
		indices[i] = i;
	}

	std::vector<std::vector<UINT>> result;
	generateBatchesRecursive(Aabbs, targetSize, splitGranularity, &indices[0], &indices[0] + indices.size(), result);

	return result;
}