#ifndef _VgDagDrawModifier_cginc_
#define _VgDagDrawModifier_cginc_

// 地形 cluster DAG 远景绘制 modifier (M2 第二片)
// 单个 DrawIndexedInstancedIndirect: instance = 可见 cluster(经 GPU cull 压缩),
// 每 instance 画固定 16x16 grid patch, 顶点从 bake 顶点池按 BaseVertex 偏移拉取。
// cull/参数写入见 VgDagCull.compute。

#include "../Inc/GlobalDefine.cginc"

StructuredBuffer<FVgVisCluster> VgDrawClusters;
StructuredBuffer<FVgVisVertex>  VgVertexBuffer;

cbuffer cbVgVisParams
{
    FVgVisParams VgVisParams;
};

// vVertexID: index buffer 取出的行主序 grid 序号(0..255, dummy mesh = MakeGridForTerrain(16,16))
// vInstanceId: DrawClusters 的压缩槽位
void DoVgDagDrawModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
    FVgVisCluster c = VgDrawClusters[vert.vInstanceId];

    // patch 固定 17x17 顶点槽位, cluster 实际顶点数更少时(粗层 LOD)多余的槽位退化剔除
    if (vert.vVertexID >= c.VertCount)
    {
        vsOut.vPosition = float4(0, 0, 0, 0);
        return;
    }

    FVgVisVertex v = VgVertexBuffer[c.BaseVertex + vert.vVertexID];
    float texel = VgVisParams.OriginAndTexel.w;
    float3 w = VgVisParams.OriginAndTexel.xyz
        + float3(v.TexelX * texel, v.Y, v.TexelZ * texel);

    // modifier 输出 world 位置, 主 VS 负责 world->clip (同 MorphModifier)
    vert.vPosition.xyz = w;
    vsOut.vPosition.xyz = w;
}

#endif
