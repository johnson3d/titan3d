#ifndef _VgDagLod_cginc_
#define _VgDagLod_cginc_

// 地形 cluster DAG 组一致 LOD 选择(Quark + 精确竖直误差, 见 TtVgDagAsset)。
// 使用方必须在 include 之前声明:
//   cbuffer cbVgVisParams { FVgVisParams VgVisParams; };
//   StructuredBuffer<FVgVisGroup> VisGroupBuffer;
//
// 选择规则: visible = (Gen < 0 || Proj(G_gen) <= tau) && (Par < 0 || Proj(G_par) > tau)
// Proj(G) = G.ParentLODError * screenScale / max(min clip.w over G's AABB corners, near)
// ParentLODError 与 group 包围盒都沿 DAG 单调(父 group 包含子 group), 故 Proj 单调,
// 每个区域恰好选中一层; 同一 group 被它的子 cluster(作 Par)和父 cluster(作 Gen)
// 判定时必须得到逐位相同的结果, 所以两次求值走同一段 [loop] 代码, 避免编译器
// 对两份内联副本做不同的 FMA 合并。

// 每 cluster 最多 128 三角; 与 TtVgDagGpuData.MaxClusterIndexCount 一致
#define VG_CLUSTER_MAX_INDEX 384

float3 VgTexelBoxMin(uint minX, uint minZ, float minY)
{
    float texel = VgVisParams.OriginAndTexel.w;
    return VgVisParams.OriginAndTexel.xyz + float3(minX * texel, minY, minZ * texel);
}

float3 VgTexelBoxMax(uint maxX, uint maxZ, float maxY)
{
    float texel = VgVisParams.OriginAndTexel.w;
    return VgVisParams.OriginAndTexel.xyz + float3(maxX * texel, maxY, maxZ * texel);
}

// clip.w 是位置的仿射函数, AABB 上的最小值必在角点取得
float VgMinClipW(float3 bmin, float3 bmax)
{
    float minW = 3.0e38;
    [unroll]
    for (uint i = 0; i < 8; i++)
    {
        float3 p = float3((i & 1) ? bmax.x : bmin.x,
                          (i & 2) ? bmax.y : bmin.y,
                          (i & 4) ? bmax.z : bmin.z);
        float4 clip = mul(float4(p, 1), VgVisParams.ViewProj);
        minW = min(minW, clip.w);
    }
    return minW;
}

// Vis.z = screenScale, Vis.w = near(透视) / -1(正交, 不做透视除法)
float VgGroupProjectedError(uint groupIndex)
{
    FVgVisGroup g = VisGroupBuffer[groupIndex];
    float denom = 1.0;
    if (VgVisParams.Vis.w > 0)
    {
        float3 bmin = VgTexelBoxMin(g.MinTexelX, g.MinTexelZ, g.MinY);
        float3 bmax = VgTexelBoxMax(g.MaxTexelX, g.MaxTexelZ, g.MaxY);
        denom = max(VgMinClipW(bmin, bmax), VgVisParams.Vis.w);
    }
    precise float e = g.ParentLODError * VgVisParams.Vis.z / denom;
    return e;
}

bool VgClusterSelected(FVgVisCluster c)
{
    float tau = VgVisParams.Vis.x;
    float e[2];
    e[0] = -1.0;     // 叶子: 没有更细的层, 自身恒"足够细"
    e[1] = 3.0e38;   // 根: 没有更粗的层, 父恒"不够细"
    [loop]
    for (uint k = 0; k < 2; k++)
    {
        int gi = (k == 0) ? c.GeneratingGroup : c.ParentGroup;
        if (gi >= 0)
            e[k] = VgGroupProjectedError((uint)gi);
    }
    return e[0] <= tau && e[1] > tau;
}

#endif
