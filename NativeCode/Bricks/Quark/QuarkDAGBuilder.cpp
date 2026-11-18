#include "QuarkDAGBuilder.h"
#include "ClusterDAG.h"
#include "TerrainSimplifier.h"
#include <algorithm>
#include <cfloat>
#include <cmath>
#include <cstring>

NS_BEGIN

void FQuarkDAGBuilder::Reset()
{
	mMipLevelCount = 0;
	mPositions.clear();
	mIndices.clear();
	mMipLevels.clear();
	mLODErrors.clear();
	mIndexStarts.clear();
	mIndexCounts.clear();
	mGeneratingGroups.clear();
	mParentGroups.clear();
	mGroupMipLevels.clear();
	mGroupErrors.clear();
	mGroupBoundsMin.clear();
	mGroupBoundsMax.clear();
}

int FQuarkDAGBuilder::Build(const v3dxVector3* positions, UINT vertexCount, const UINT* indices, UINT indexCount,
	UINT maxGroupSize, UINT clusterSize, float simplifyTargetError)
{
	Reset();
	if (positions == nullptr || indices == nullptr || vertexCount == 0 || indexCount < 3 || indexCount % 3 != 0)
		return 0;
	for (UINT i = 0; i < indexCount; i++)
	{
		if (indices[i] >= vertexCount)
			return 0;
	}
	std::vector<v3dxVector3> Verts(positions, positions + vertexCount);
	std::vector<UINT> Indexes(indices, indices + indexCount);
	return BuildMesh(Verts, Indexes, maxGroupSize, clusterSize, simplifyTargetError, nullptr, false);
}

int FQuarkDAGBuilder::BuildTerrain(const float* heights, UINT size, UINT step,
	UINT maxGroupSize, UINT clusterSize, float simplifyTargetError)
{
	Reset();
	// 上限保证 texel 坐标可用 16 位存储，且远小于 TerrainSimplifier 的整数坐标上限。
	if (heights == nullptr || size < 2 || size > 65536 || step == 0)
		return 0;
	FTerrainHeightGrid Grid;
	Grid.Heights = heights;
	Grid.Size = size;
	const size_t SampleCount = size_t(size) * size;
	for (size_t i = 0; i < SampleCount; i++)
	{
		if (!std::isfinite(heights[i]))
			return 0;
		Grid.MaxAbsHeight = std::max(Grid.MaxAbsHeight, std::fabs(double(heights[i])));
	}

	std::vector<UINT> Lines;
	for (UINT c = 0; c < size - 1; c += step)
		Lines.push_back(c);
	Lines.push_back(size - 1);
	const UINT M = (UINT)Lines.size();
	std::vector<v3dxVector3> Verts(size_t(M) * M);
	for (UINT j = 0; j < M; j++)
	{
		for (UINT i = 0; i < M; i++)
			Verts[size_t(j) * M + i] = v3dxVector3(float(Lines[i]), heights[size_t(Lines[j]) * size + Lines[i]], float(Lines[j]));
	}
	// a=(x,z), b=(x+1,z), c=(x+1,z+1), d=(x,z+1)，对角线 a→c；与 FTerrainHeightGrid 的参考剖分一致。
	std::vector<UINT> Indexes;
	Indexes.reserve(size_t(M - 1) * (M - 1) * 6);
	for (UINT j = 0; j + 1 < M; j++)
	{
		for (UINT i = 0; i + 1 < M; i++)
		{
			UINT a = j * M + i, b = a + 1, c = a + 1 + M, d = a + M;
			Indexes.insert(Indexes.end(), { a, b, c, a, c, d });
		}
	}
	return BuildMesh(Verts, Indexes, maxGroupSize, clusterSize, simplifyTargetError, &Grid, M != size);
}

int FQuarkDAGBuilder::BuildMesh(const std::vector<v3dxVector3>& verts, const std::vector<UINT>& indexes,
	UINT maxGroupSize, UINT clusterSize, float simplifyTargetError,
	const FTerrainHeightGrid* terrainGrid, bool leafError)
{
	if (IsCancelRequested())
		return -1;
	const UINT vertexCount = (UINT)verts.size();
	std::vector<v3dxVector3> Normals(vertexCount, v3dxVector3(0.0f, 1.0f, 0.0f));
	std::vector<float> Tangents;
	std::vector<float> UVs(vertexCount * 2, 0.0f);
	std::vector<INT32> MaterialIndexes(indexes.size() / 3, 0);

	FClusterDAG DAG(8, std::max(maxGroupSize, 8u), clusterSize == 0 ? QuarkCluster::ClusterSize : clusterSize);
	DAG.SimplifyTargetError = simplifyTargetError;
	DAG.ShouldCancel = [this]() { return IsCancelRequested(); };
	FTerrainSimplifyStats TerrainStats;
	if (terrainGrid != nullptr)
	{
		DAG.SimplifyOverride = [this, terrainGrid, &TerrainStats](QuarkCluster& cluster, UINT target, float error)
		{
			// 取消后不再简化（几何不变、误差由 max 继承），BuildDAG 在下一个检查点返回。
			if (IsCancelRequested())
				return 0.0f;
			return SimplifyTerrainCluster(cluster, target, error, *terrainGrid, TerrainStats);
		};
	}
	if (DAG.AddMesh(verts, Normals, Tangents, UVs, indexes, MaterialIndexes) == 0)
		return IsCancelRequested() ? -1 : 0;
	if (leafError)
	{
		// 降采样叶子与全分辨率参考面不重合：叶子误差同样用 M2 度量，ReduceGroup 的 max 会把它带到所有祖先。
		for (QuarkCluster* Cluster : DAG.Clusters)
		{
			if (IsCancelRequested())
				return -1;
			Cluster->LODError = TerrainClusterVerticalError(*terrainGrid, *Cluster);
		}
	}
	DAG.BuildDAG();
	if (IsCancelRequested())
		return -1;
	if (terrainGrid != nullptr)
	{
		VFX_LTRACE(ELTT_info, "[TerrainSimplifier] groups=%u attempts=%u retried=%u preserved=%u missedTargets=%u locked=%u unsupported=%u rejected=%u\n",
			TerrainStats.Groups, TerrainStats.Attempts, TerrainStats.RetriedGroups,
			TerrainStats.PreservedGroups, TerrainStats.MissedTargets, TerrainStats.LockedVertices,
			TerrainStats.UnsupportedGroups, TerrainStats.RejectedCandidates);
	}
	Export(DAG);
	return (int)mMipLevels.size();
}

void FQuarkDAGBuilder::Export(const FClusterDAG& dag)
{
	mMipLevelCount = dag.NumMipLevels;
	const UINT NumGroups = dag.GetNumGroups();
	const UINT NumClusters = dag.GetNumClusters();
	mMipLevels.resize(NumClusters);
	mLODErrors.resize(NumClusters);
	mIndexStarts.resize(NumClusters);
	mIndexCounts.resize(NumClusters);
	mGeneratingGroups.resize(NumClusters);
	mParentGroups.assign(NumClusters, ~0u);
	std::vector<v3dxVector3> ClusterMin(NumClusters, v3dxVector3(FLT_MAX, FLT_MAX, FLT_MAX));
	std::vector<v3dxVector3> ClusterMax(NumClusters, v3dxVector3(-FLT_MAX, -FLT_MAX, -FLT_MAX));
	for (UINT c = 0; c < NumClusters; c++)
	{
		const QuarkCluster* Cluster = dag.Clusters[c];
		const UINT VertexStart = (UINT)mPositions.size();
		for (UINT v = 0; v < Cluster->NumVerts; v++)
		{
			const v3dxVector3& P = Cluster->GetPositionConst(v);
			mPositions.push_back(P);
			ClusterMin[c] = v3dxVector3(std::min(ClusterMin[c].X, P.X), std::min(ClusterMin[c].Y, P.Y), std::min(ClusterMin[c].Z, P.Z));
			ClusterMax[c] = v3dxVector3(std::max(ClusterMax[c].X, P.X), std::max(ClusterMax[c].Y, P.Y), std::max(ClusterMax[c].Z, P.Z));
		}
		mIndexStarts[c] = (UINT)mIndices.size();
		for (UINT idx : Cluster->Indexes)
			mIndices.push_back(idx + VertexStart);
		mIndexCounts[c] = (UINT)Cluster->Indexes.size();
		mMipLevels[c] = Cluster->MipLevel;
		mLODErrors[c] = Cluster->LODError;
		mGeneratingGroups[c] = Cluster->GeneratingGroupIndex < NumGroups ? Cluster->GeneratingGroupIndex : ~0u;
	}

	mGroupMipLevels.resize(NumGroups);
	mGroupErrors.resize(NumGroups);
	mGroupBoundsMin.assign(NumGroups, v3dxVector3(FLT_MAX, FLT_MAX, FLT_MAX));
	mGroupBoundsMax.assign(NumGroups, v3dxVector3(-FLT_MAX, -FLT_MAX, -FLT_MAX));
	// group 按层追加，子 cluster 的生成 group 下标总小于当前 group，顺序遍历即可累积单调包围盒。
	for (UINT g = 0; g < NumGroups; g++)
	{
		const FClusterGroup& Group = dag.Groups[g];
		mGroupMipLevels[g] = Group.MipLevel;
		mGroupErrors[g] = Group.ParentLODError;
		v3dxVector3 Min = mGroupBoundsMin[g], Max = mGroupBoundsMax[g];
		auto Merge = [&Min, &Max](const v3dxVector3& a, const v3dxVector3& b)
		{
			Min = v3dxVector3(std::min(Min.X, a.X), std::min(Min.Y, a.Y), std::min(Min.Z, a.Z));
			Max = v3dxVector3(std::max(Max.X, b.X), std::max(Max.Y, b.Y), std::max(Max.Z, b.Z));
		};
		for (UINT Child : Group.Children)
		{
			if (Child >= NumClusters)
				continue;
			mParentGroups[Child] = g;
			Merge(ClusterMin[Child], ClusterMax[Child]);
			UINT Gen = mGeneratingGroups[Child];
			if (Gen < g)
				Merge(mGroupBoundsMin[Gen], mGroupBoundsMax[Gen]);
		}
		mGroupBoundsMin[g] = Min;
		mGroupBoundsMax[g] = Max;
	}
}

UINT FQuarkDAGBuilder::CopyPositions(v3dxVector3* outPositions, UINT maxCount) const
{
	UINT count = std::min(maxCount, (UINT)mPositions.size());
	if (outPositions != nullptr && count > 0)
		memcpy(outPositions, mPositions.data(), sizeof(v3dxVector3) * count);
	return count;
}

UINT FQuarkDAGBuilder::CopyIndices(UINT* outIndices, UINT maxCount) const
{
	UINT count = std::min(maxCount, (UINT)mIndices.size());
	if (outIndices != nullptr && count > 0)
		memcpy(outIndices, mIndices.data(), sizeof(UINT) * count);
	return count;
}

UINT FQuarkDAGBuilder::CopyClusters(int* outMipLevel, float* outLODError, UINT* outIndexStart, UINT* outIndexCount,
	UINT* outGeneratingGroup, UINT maxCount) const
{
	UINT count = std::min(maxCount, (UINT)mMipLevels.size());
	for (UINT i = 0; i < count; i++)
	{
		if (outMipLevel) outMipLevel[i] = mMipLevels[i];
		if (outLODError) outLODError[i] = mLODErrors[i];
		if (outIndexStart) outIndexStart[i] = mIndexStarts[i];
		if (outIndexCount) outIndexCount[i] = mIndexCounts[i];
		if (outGeneratingGroup) outGeneratingGroup[i] = mGeneratingGroups[i];
	}
	return count;
}

UINT FQuarkDAGBuilder::CopyClusterParents(UINT* outParentGroup, UINT maxCount) const
{
	UINT count = std::min(maxCount, (UINT)mParentGroups.size());
	if (outParentGroup != nullptr && count > 0)
		memcpy(outParentGroup, mParentGroups.data(), sizeof(UINT) * count);
	return count;
}

UINT FQuarkDAGBuilder::CopyGroups(int* outMipLevel, float* outParentLODError, v3dxVector3* outBoundsMin, v3dxVector3* outBoundsMax,
	UINT maxCount) const
{
	UINT count = std::min(maxCount, (UINT)mGroupMipLevels.size());
	for (UINT i = 0; i < count; i++)
	{
		if (outMipLevel) outMipLevel[i] = mGroupMipLevels[i];
		if (outParentLODError) outParentLODError[i] = mGroupErrors[i];
		if (outBoundsMin) outBoundsMin[i] = mGroupBoundsMin[i];
		if (outBoundsMax) outBoundsMax[i] = mGroupBoundsMax[i];
	}
	return count;
}

NS_END
