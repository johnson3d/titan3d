#pragma once

#include "Cluster.h"
#include "TerrainError.h"

NS_BEGIN

// 仅地形构建显式启用；统计归属单次构建，不使用全局状态。
struct FTerrainSimplifyStats
{
    UINT Groups = 0;
    UINT Attempts = 0;
    UINT RetriedGroups = 0;
    UINT PreservedGroups = 0;
    UINT MissedTargets = 0;
    UINT LockedVertices = 0;
    UINT UnsupportedGroups = 0;
    UINT RejectedCandidates = 0;
};

// M1-B：只支持 xz 唯一的整数格点、常量属性、材质 0 的位置网格（原始数组入口）。
// 临时缩放高度，锁边简化，精确检查投影与拓扑，失败则加锁重试或保留输入。
// M2：提交后返回简化结果相对全分辨率高度场 heightGrid 的竖直 L∞ 上界 (TerrainClusterVerticalError)；
// 保留输入时返回 0，DAG 按 max 继承子误差。
float SimplifyTerrainCluster(QuarkCluster& cluster, UINT targetTris, float targetError,
    const FTerrainHeightGrid& heightGrid, FTerrainSimplifyStats& stats);

NS_END
