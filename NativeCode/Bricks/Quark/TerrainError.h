#pragma once

#include "Cluster.h"
#include <cstdint>

NS_BEGIN

// 全分辨率参考高度场 F：Size×Size 采样，行主序 Heights[z * Size + x]，xz 单位为 texel。
// 每个 quad 按 a=(x,z) → c=(x+1,z+1) 对角线剖分为 (a,b,c)/(a,c,d)，与地形 DAG 输入网格一致。
struct FTerrainHeightGrid
{
    const float* Heights = nullptr;
    UINT Size = 0;
    // max |height|，用于双精度舍入的保守 padding。
    double MaxAbsHeight = 0.0;
};

// 单个三角形相对 F 的精确竖直 L∞（未加 padding）。e = P - F 在三角形与 F 剖分的叠加上分片线性，
// 极值只出现在：闭三角形内的格点、三角形边与 x=i / z=j / x-z=c 三族网格线的交点（顶点是格点）。
// 所有求值都是凸组合，不外推。xz 越界、xz 投影有向面积 <= 0 时返回 +inf。
double TerrainTriangleVerticalError(const FTerrainHeightGrid& grid,
    const int64_t x[3], const int64_t z[3], const double y[3]);

// cluster 全部三角形的竖直 L∞ 上界：最大值加舍入 padding 后向上取整为 float。
// 非整数 xz、越界、退化或结果非有限时返回 FLT_MAX（不用 INF，避免 shader 中产生 NaN）。
float TerrainClusterVerticalError(const FTerrainHeightGrid& grid, const QuarkCluster& cluster);

NS_END
