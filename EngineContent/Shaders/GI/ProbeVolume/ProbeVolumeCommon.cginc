#ifndef _PROBE_VOLUME_COMMON_H_
#define _PROBE_VOLUME_COMMON_H_

// World-Space Irradiance Probe Volume
// 为 ReSTIR GI 提供屏幕外 irradiance fallback
//
// 每个 probe 存储 SH2 (L0-L2) x RGB = 9 coefficients x 3 channels = 27 floats
// 打包为 7 个 float4 = 112 bytes per probe
//
// probe 之间通过 Delaunay 四面体化构建空间连接关系,
// 查询时用 BVH 定位四面体, 重心坐标插值 4 个 probe 的 SH

// -----------------------------------------------------------------------------
// Probe SH 数据 (GPU 打包格式)
// 9 个 SH basis x RGB = 27 floats, 打包到 7 个 float4 (28 floats, 最后 1 padding)
// 布局:
//   Data0: SH[0].rgb, SH[1].r
//   Data1: SH[1].gb,  SH[2].rg
//   Data2: SH[2].b,   SH[3].rgb
//   Data3: SH[4].rgb, SH[5].r
//   Data4: SH[5].gb,  SH[6].rg
//   Data5: SH[6].b,   SH[7].rgb
//   Data6: SH[8].rgb, _pad
// -----------------------------------------------------------------------------
struct PackedProbeSH
{
    float4 Data0;
    float4 Data1;
    float4 Data2;
    float4 Data3;
    float4 Data4;
    float4 Data5;
    float4 Data6;
};

// 从 PackedProbeSH 解包出 9 个 SH 系数 (每个 float3 = RGB)
void UnpackProbeSH(PackedProbeSH packed, out float3 shCoeffs[9])
{
    shCoeffs[0] = float3(packed.Data0.x, packed.Data0.y, packed.Data0.z);
    shCoeffs[1] = float3(packed.Data0.w, packed.Data1.x, packed.Data1.y);
    shCoeffs[2] = float3(packed.Data1.z, packed.Data1.w, packed.Data2.x);
    shCoeffs[3] = float3(packed.Data2.y, packed.Data2.z, packed.Data2.w);
    shCoeffs[4] = float3(packed.Data3.x, packed.Data3.y, packed.Data3.z);
    shCoeffs[5] = float3(packed.Data3.w, packed.Data4.x, packed.Data4.y);
    shCoeffs[6] = float3(packed.Data4.z, packed.Data4.w, packed.Data5.x);
    shCoeffs[7] = float3(packed.Data5.y, packed.Data5.z, packed.Data5.w);
    shCoeffs[8] = float3(packed.Data6.x, packed.Data6.y, packed.Data6.z);
}

// 把 9 个 SH 系数打包回 PackedProbeSH
void PackProbeSH(float3 shCoeffs[9], out PackedProbeSH packed)
{
    packed.Data0 = float4(shCoeffs[0],            shCoeffs[1].r);
    packed.Data1 = float4(shCoeffs[1].gb,         shCoeffs[2].rg);
    packed.Data2 = float4(shCoeffs[2].b,          shCoeffs[3]);
    packed.Data3 = float4(shCoeffs[4],            shCoeffs[5].r);
    packed.Data4 = float4(shCoeffs[5].gb,         shCoeffs[6].rg);
    packed.Data5 = float4(shCoeffs[6].b,          shCoeffs[7]);
    packed.Data6 = float4(shCoeffs[8],            0.0f);
}

// 计算 SH2 (L0-L2) 的 9 个 basis 函数值
// 与 SHCoefficients.cginc 中 TtSHCoefficient.SHEval3 完全一致的常量和公式,
// 内联在此避免跨 cginc struct 实例化依赖
void EvaluateSHBasis9(float3 dir, out float basis[9])
{
    float3 d = normalize(dir);
    float x = d.x, y = d.y, z = d.z;

    basis[0] =  0.2820947918f;                         // Y00
    basis[1] = -0.4886025119f * y;                      // Y1-1
    basis[2] =  0.4886025119f * z;                      // Y10
    basis[3] = -0.4886025119f * x;                      // Y11
    basis[4] =  1.0925484306f * x * y;                  // Y2-2
    basis[5] = -1.0925484306f * y * z;                  // Y2-1
    basis[6] =  0.3153915652f * (3.0f * z * z - 1.0f);  // Y20
    basis[7] = -1.0925484306f * x * z;                  // Y21
    basis[8] =  0.5462742153f * (x * x - y * y);        // Y22
}

// 用 SH2 系数对给定方向求值, 返回 RGB irradiance
float3 EvaluateProbeSHRGB(float3 shCoeffs[9], float3 direction)
{
    float basis[9];
    EvaluateSHBasis9(direction, basis);

    float3 result = float3(0, 0, 0);
    [unroll]
    for (int i = 0; i < 9; i++)
    {
        result += shCoeffs[i] * basis[i];
    }
    return max(result, float3(0, 0, 0));
}

// -----------------------------------------------------------------------------
// Probe 位置信息, 由 C# 端 TtProbeVolumeData 构建并上传
// ProbeVolumeUpdate.compute 用此结构获取 probe 世界坐标作为射线起点
// QueryProbeIrradiance 不需要此结构 (只通过 SH buffer 和四面体查询)
// -----------------------------------------------------------------------------
struct ProbeInfo
{
    float3 Position;
    float  Radius;      // probe 有效影响半径 (用于距离衰减, 0 = 无衰减)
};

// -----------------------------------------------------------------------------
// 四面体 (GPU 端, 与 C# FTetrahedronGpu 一一对应)
// 4 个 probe 索引 + 预计算的 4x4 重心坐标变换矩阵
// -----------------------------------------------------------------------------
struct TetrahedronGpu
{
    uint4  ProbeIndices;    // 4 个 probe 的索引
    float4 BaryRow0;        // 重心坐标矩阵第 0 行
    float4 BaryRow1;        // 重心坐标矩阵第 1 行
    float4 BaryRow2;        // 重心坐标矩阵第 2 行
    float4 BaryRow3;        // 重心坐标矩阵第 3 行
};

// 计算世界坐标在四面体内的重心坐标 (w0,w1,w2,w3)
// 返回值: 所有分量 >= 0 表示在四面体内部
float4 GetTetraBarycentricCoords(float3 worldPos, TetrahedronGpu tetra)
{
    float4 homogeneous = float4(worldPos, 1.0f);
    float4 bary;
    bary.x = dot(homogeneous, tetra.BaryRow0);
    bary.y = dot(homogeneous, tetra.BaryRow1);
    bary.z = dot(homogeneous, tetra.BaryRow2);
    bary.w = dot(homogeneous, tetra.BaryRow3);
    return bary;
}

bool IsInsideTetrahedron(float4 baryCoords)
{
    return baryCoords.x >= -1e-5f && baryCoords.y >= -1e-5f &&
           baryCoords.z >= -1e-5f && baryCoords.w >= -1e-5f;
}

// -----------------------------------------------------------------------------
// BVH 节点 (GPU 端), 由 C# 端 TtProbeVolumeData 构建并上传
// 完全二叉树, 叶子节点存四面体起始索引和数量
//
// 内存布局 (32 bytes, 16-byte aligned):
//   float3 AabbMin + uint LeftChild   = 16 bytes
//   float3 AabbMax + uint Payload     = 16 bytes
//
// 编码约定:
//   LeftChild bit31 = 1 → 叶子节点:
//     LeftChild bit30..0 = 四面体起始索引 (tetraBuffer 中的偏移)
//     Payload            = 该叶子包含的四面体数量
//   LeftChild bit31 = 0 → 内部节点:
//     LeftChild          = 左子节点索引
//     Payload            = 右子节点索引
// -----------------------------------------------------------------------------
struct BvhNodeGpu
{
    float3 AabbMin;
    uint   LeftChild;
    float3 AabbMax;
    uint   Payload;
};

bool BvhNodeIsLeaf(BvhNodeGpu node)
{
    return (node.LeftChild & (1u << 31u)) != 0u;
}

uint BvhNodeLeafStart(BvhNodeGpu node)
{
    return node.LeftChild & 0x7FFFFFFFu;
}

uint BvhNodeLeafCount(BvhNodeGpu node)
{
    return node.Payload;
}

uint BvhNodeRightChild(BvhNodeGpu node)
{
    return node.Payload;
}

bool AabbContains(float3 aabbMin, float3 aabbMax, float3 pos)
{
    return all(pos >= aabbMin) && all(pos <= aabbMax);
}

// -----------------------------------------------------------------------------
// QueryProbeIrradiance - HLSL 2021 模板函数
//
// TProbeBuffer 可以是 StructuredBuffer<PackedProbeSH> 或 RWStructuredBuffer<PackedProbeSH>,
// 两者的 operator[] 读取语法完全一致, 用模板统一为一份实现.
//
// 注意: 引用此 cginc 的 .compute 入口需要在 Meta 中声明 HLSL=2021, 例如:
//   /**Meta Begin:(CS_Main)
//   HLSL=2021
//   Meta End:(CS_Main)**/
//
// 流程:
//   1. BVH 遍历找到包含 worldPos 的四面体
//   2. 计算重心坐标
//   3. 用重心坐标加权插值 4 个 probe 的 SH
//   4. 对 direction 求值得到 RGB irradiance
//
// 返回: true 表示查到了有效 probe 数据, false 表示该位置不在任何四面体内
// -----------------------------------------------------------------------------
template<typename TProbeBuffer>
bool QueryProbeIrradiance(
    TProbeBuffer probeBuffer,
    StructuredBuffer<TetrahedronGpu> tetraBuffer,
    StructuredBuffer<BvhNodeGpu> bvhBuffer,
    uint bvhNodeCount,
    float3 worldPos,
    float3 direction,
    out float3 outIrradiance)
{
    outIrradiance = float3(0, 0, 0);

    if (bvhNodeCount == 0u)
        return false;

    // BVH 遍历: 用固定大小栈替代递归
    // 最大深度 16 足以覆盖 2^16 = 65536 个叶子节点 (远超实际 probe 数)
    uint stack[16];
    int stackTop = 0;
    stack[0] = 0u;
    stackTop = 1;

    [loop]
    while (stackTop > 0)
    {
        stackTop--;
        uint nodeIdx = stack[stackTop];
        if (nodeIdx >= bvhNodeCount)
            continue;

        BvhNodeGpu node = bvhBuffer[nodeIdx];

        if (!AabbContains(node.AabbMin, node.AabbMax, worldPos))
            continue;

        if (BvhNodeIsLeaf(node))
        {
            uint tetraStart = BvhNodeLeafStart(node);
            uint tetraCount = BvhNodeLeafCount(node);

            [loop]
            for (uint t = 0; t < tetraCount; t++)
            {
                TetrahedronGpu tetra = tetraBuffer[tetraStart + t];
                float4 bary = GetTetraBarycentricCoords(worldPos, tetra);

                if (IsInsideTetrahedron(bary))
                {
                    float3 blendedSH[9];
                    [unroll]
                    for (int c = 0; c < 9; c++)
                        blendedSH[c] = float3(0, 0, 0);

                    float barySum = bary.x + bary.y + bary.z + bary.w;
                    bary /= max(barySum, 1e-6f);

                    float weights[4] = { bary.x, bary.y, bary.z, bary.w };
                    uint  indices[4] = { tetra.ProbeIndices.x, tetra.ProbeIndices.y,
                                         tetra.ProbeIndices.z, tetra.ProbeIndices.w };

                    [unroll]
                    for (int p = 0; p < 4; p++)
                    {
                        float3 probeSH[9];
                        UnpackProbeSH(probeBuffer[indices[p]], probeSH);
                        [unroll]
                        for (int c2 = 0; c2 < 9; c2++)
                            blendedSH[c2] += probeSH[c2] * weights[p];
                    }

                    outIrradiance = EvaluateProbeSHRGB(blendedSH, direction);
                    return true;
                }
            }
        }
        else
        {
            if (stackTop < 15)
            {
                stack[stackTop] = node.LeftChild;
                stackTop++;
            }
            if (stackTop < 15)
            {
                stack[stackTop] = BvhNodeRightChild(node);
                stackTop++;
            }
        }
    }

    return false;
}

#endif // _PROBE_VOLUME_COMMON_H_
