#pragma once

#include "../../Base/IUnknown.h"
#include "../../Math/v3dxVector3.h"
#include <atomic>
#include <vector>

NS_BEGIN

class FClusterDAG;
struct FTerrainHeightGrid;

// Raw-array entry of the Quark cluster DAG builder (FClusterDAG), independent of FMeshPrimitives/GPU.
// Results are flattened per cluster; groups are exported with monotone bounds for runtime LOD cuts.
class TR_CLASS()
	FQuarkDAGBuilder : public VIUnknown
{
public:
	FQuarkDAGBuilder() {}

	// 普通网格（仅位置：法线/UV 填常量，焊点只取决于位置）。
	// simplifyTargetError <= 0：传统 5% meshopt 相对 QEM 预算；FLT_MAX：不限制 QEM 预算。
	// 返回总 cluster 数，空或非法输入返回 0，RequestCancel 后返回 -1。
	int Build(const v3dxVector3* positions, UINT vertexCount, const UINT* indices, UINT indexCount,
		UINT maxGroupSize, UINT clusterSize, float simplifyTargetError);

	// 地形：heights 为 size×size 行主序全分辨率高度 (xz 单位 texel)。网格取行列 {0, step, 2*step, ...} ∪ {size-1}，
	// 顶点高度直接取原始采样，每个 quad 按 a=(x,z)→c=(x+1,z+1) 对角线剖分；简化走 TerrainSimplifier (M1-B)。
	// 所有 LODError（含 step>1 时的叶子）都是相对全分辨率高度场的保守竖直 L∞ 上界 (M2)。
	// 返回总 cluster 数，非法输入返回 0，RequestCancel 后返回 -1（输出清空）。
	int BuildTerrain(const float* heights, UINT size, UINT step,
		UINT maxGroupSize, UINT clusterSize, float simplifyTargetError);

	// 任意线程可调用；一次性标志，置位后本实例正在进行和之后的构建都会取消。
	void RequestCancel() { mCancelRequested.store(true, std::memory_order_relaxed); }
	bool IsCancelRequested() const { return mCancelRequested.load(std::memory_order_relaxed); }

	UINT GetMipLevelCount() const { return mMipLevelCount; }
	UINT GetClusterCount() const { return (UINT)mMipLevels.size(); }
	UINT GetGroupCount() const { return (UINT)mGroupMipLevels.size(); }
	UINT GetVertexCount() const { return (UINT)mPositions.size(); }
	UINT GetIndexCount() const { return (UINT)mIndices.size(); }

	UINT CopyPositions(v3dxVector3* outPositions, UINT maxCount) const;
	// Indices already point into the flattened position array
	UINT CopyIndices(UINT* outIndices, UINT maxCount) const;
	// outGeneratingGroup: ~0u for leaf clusters
	UINT CopyClusters(int* outMipLevel, float* outLODError, UINT* outIndexStart, UINT* outIndexCount,
		UINT* outGeneratingGroup, UINT maxCount) const;
	// 每个 cluster 作为子节点所在的 group（被它简化出父层）；根 cluster 为 ~0u。
	UINT CopyClusterParents(UINT* outParentGroup, UINT maxCount) const;
	// group 的 ParentLODError 不小于全部子 cluster 的 LODError；包围盒 = 子 cluster AABB ∪ 子的生成 group 包围盒，
	// 因此沿 DAG 向上误差与包围盒都单调，运行时按组投影误差判定得到一致的 cut。
	UINT CopyGroups(int* outMipLevel, float* outParentLODError, v3dxVector3* outBoundsMin, v3dxVector3* outBoundsMax,
		UINT maxCount) const;

private:
	void Reset();
	int BuildMesh(const std::vector<v3dxVector3>& verts, const std::vector<UINT>& indexes,
		UINT maxGroupSize, UINT clusterSize, float simplifyTargetError,
		const FTerrainHeightGrid* terrainGrid, bool leafError);
	void Export(const FClusterDAG& dag);

	std::atomic<bool> mCancelRequested{ false };
	UINT mMipLevelCount = 0;
	std::vector<v3dxVector3> mPositions;
	std::vector<UINT> mIndices;
	std::vector<int> mMipLevels;
	std::vector<float> mLODErrors;
	std::vector<UINT> mIndexStarts;
	std::vector<UINT> mIndexCounts;
	std::vector<UINT> mGeneratingGroups;
	std::vector<UINT> mParentGroups;
	std::vector<int> mGroupMipLevels;
	std::vector<float> mGroupErrors;
	std::vector<v3dxVector3> mGroupBoundsMin;
	std::vector<v3dxVector3> mGroupBoundsMax;
};

NS_END
