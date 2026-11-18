#ifndef _VgDagDrawModifier_cginc_
#define _VgDagDrawModifier_cginc_

// 地形 cluster DAG 远景绘制 modifier
// 单个 DrawIndexedInstancedIndirect: instance = 可见 cluster(经 GPU cull 压缩的下标),
// 每 instance 画 VG_CLUSTER_MAX_INDEX 个顺序 index 的 dummy patch,
// 顶点按 cluster 局部索引(相对 BaseVertex)从 bake 顶点池拉取。
// cull/参数写入见 VgDagCull.compute。

#include "../Inc/GlobalDefine.cginc"

ByteAddressBuffer               VgDrawClusters;
StructuredBuffer<FVgVisCluster> VgClusterBuffer;
StructuredBuffer<FVgVisVertex>  VgVertexBuffer;
ByteAddressBuffer               VgIndexBuffer;

cbuffer cbVgVisParams
{
    FVgVisParams VgVisParams;
};

// vVertexID: dummy patch 的顺序 index(0..VG_CLUSTER_MAX_INDEX-1)
// vInstanceId: DrawClusters 的压缩槽位
void DoVgDagDrawModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
    uint clusterIndex = VgDrawClusters.Load(vert.vInstanceId * 4);
    FVgVisCluster c = VgClusterBuffer[clusterIndex];

    float3 w = VgVisParams.OriginAndTexel.xyz;
    uint id = vert.vVertexID;
    if (id < c.IndexCount)
    {
        // DAG 三角在 xz 平面 area2 > 0, 与引擎地形网格(MakeGridForTerrain, area2 < 0)
        // 绕向相反: 按 (0,2,1) 读角点翻转
        uint corner = id % 3;
        uint src = id - corner + (corner == 0 ? 0 : 3 - corner);
        uint localIndex = VgIndexBuffer.Load((c.IndexStart + src) * 4);
        FVgVisVertex v = VgVertexBuffer[c.BaseVertex + localIndex];
        float texel = VgVisParams.OriginAndTexel.w;
        w += float3(v.TexelX * texel, v.Y, v.TexelZ * texel);
    }
    // 超出 IndexCount 的三角三个角都落在原点 -> 零面积, 光栅化丢弃

    // modifier 输出 world 位置, 主 VS 负责 world->clip (同 MorphModifier)
    vert.vPosition.xyz = w;
    vsOut.vPosition.xyz = w;
}

#endif
