#ifndef _ReSTIR_COMMON_H_
#define _ReSTIR_COMMON_H_

// ReSTIR GI - 屏幕空间 GI 的公共数据结构和采样工具
// 参考: "ReSTIR GI: Path Resampling for Real-Time Path Tracing", Y. Ouyang et al., HPG 2021

#include "../../Inc/GlobalDefine.cginc"
#include "../../Inc/Math.cginc"
#include "../../CBuffer/VarBase_PerCamera.cginc"
#include "../../Inc/SysFunction.cginc"
#include "../../ShadingEnv/Deferred/DeferredCommon.cginc"

// -----------------------------------------------------------------------------
// Sample —— 一条候选 GI 路径的几何与辐射信息
// 注: 我们只复用一次反弹的 indirect diffuse, 因此存储 visiblePos/sampleHit 即可
// -----------------------------------------------------------------------------
struct ReSTIRSample
{
    float3 VisiblePosition;     // 可见点（即当前像素重建出来的世界坐标）
    float3 VisibleNormal;       // 可见点法线
    float3 SamplePosition;      // 采样命中点世界坐标
    float3 SampleNormal;        // 采样命中点法线
    float3 Radiance;            // 采样命中点出射辐射 (来自上一帧 lighting buffer)
};

// -----------------------------------------------------------------------------
// Reservoir —— 加权水库, 用于 RIS / 时空重采样
// 参考 ReSTIR 原论文 Algorithm 2
// -----------------------------------------------------------------------------
struct ReSTIRReservoir
{
    ReSTIRSample Sample;
    float WeightSum;            // 累计的源 PDF 反比权重 sum(p_hat / src_pdf)
    float M;                    // 已合并的样本数（confidence）
    float W;                    // unbiased contribution weight: WeightSum / (M * p_hat(Y))
};

// -----------------------------------------------------------------------------
// 打包: 7 个 float4 = 112 bytes 每像素 (一份), 两份 ping-pong 共 224 bytes/px
// 1080p 下约 460MB —— 可接受。如果显存吃紧后续可改为半精度打包
// -----------------------------------------------------------------------------
struct ReSTIRPackedReservoir
{
    float4 Data0;   // VisiblePosition.xyz, WeightSum
    float4 Data1;   // VisibleNormal.xyz,   M
    float4 Data2;   // SamplePosition.xyz,  W
    float4 Data3;   // SampleNormal.xyz,    Radiance.r
    float4 Data4;   // Radiance.gb,         _pad0, _pad1
};

void PackReservoir(ReSTIRReservoir r, out ReSTIRPackedReservoir p)
{
    p.Data0 = float4(r.Sample.VisiblePosition, r.WeightSum);
    p.Data1 = float4(r.Sample.VisibleNormal,   r.M);
    p.Data2 = float4(r.Sample.SamplePosition,  r.W);
    p.Data3 = float4(r.Sample.SampleNormal,    r.Sample.Radiance.r);
    p.Data4 = float4(r.Sample.Radiance.gb, 0.0f, 0.0f);
}

ReSTIRReservoir UnpackReservoir(ReSTIRPackedReservoir p)
{
    ReSTIRReservoir r;
    r.Sample.VisiblePosition = p.Data0.xyz;
    r.WeightSum              = p.Data0.w;
    r.Sample.VisibleNormal   = p.Data1.xyz;
    r.M                      = p.Data1.w;
    r.Sample.SamplePosition  = p.Data2.xyz;
    r.W                      = p.Data2.w;
    r.Sample.SampleNormal    = p.Data3.xyz;
    r.Sample.Radiance        = float3(p.Data3.w, p.Data4.x, p.Data4.y);
    return r;
}

void InitReservoir(out ReSTIRReservoir r)
{
    r.Sample = (ReSTIRSample)0;
    r.WeightSum = 0.0f;
    r.M = 0.0f;
    r.W = 0.0f;
}

// 目标函数 p_hat: 用 sample 在 visible point 上的余弦项 * radiance luminance 估计
float TargetFunctionLuminance(ReSTIRSample s)
{
    float3 toSample = s.SamplePosition - s.VisiblePosition;
    float dist2 = max(dot(toSample, toSample), 1e-6f);
    float3 dir = toSample * rsqrt(dist2);
    float NoL = max(dot(s.VisibleNormal, dir), 0.0f);
    float lum = dot(s.Radiance, float3(0.299f, 0.587f, 0.114f));
    return lum * NoL;
}

// WRS: 把候选样本以概率 (weight / WeightSum_new) 接受
bool UpdateReservoir(inout ReSTIRReservoir r, ReSTIRSample candidate, float weight, float rnd)
{
    r.WeightSum += weight;
    r.M += 1.0f;
    if (r.WeightSum > 0.0f && rnd < (weight / r.WeightSum))
    {
        r.Sample = candidate;
        return true;
    }
    return false;
}

// 合并两个 reservoir (用于时空复用)
bool CombineReservoir(inout ReSTIRReservoir self, ReSTIRReservoir other, float pHatOther, float rnd)
{
    float weight = pHatOther * other.W * other.M;
    self.WeightSum += weight;
    self.M += other.M;
    bool replaced = false;
    if (self.WeightSum > 0.0f && rnd < (weight / self.WeightSum))
    {
        self.Sample = other.Sample;
        replaced = true;
    }
    return replaced;
}

void FinalizeReservoir(inout ReSTIRReservoir r)
{
    float pHat = TargetFunctionLuminance(r.Sample);
    if (pHat > 0.0f && r.M > 0.0f)
    {
        r.W = r.WeightSum / (r.M * pHat);
    }
    else
    {
        r.W = 0.0f;
    }
}

// -----------------------------------------------------------------------------
// 随机数 (PCG hash)
// -----------------------------------------------------------------------------
uint PcgHash(uint seed)
{
    uint state = seed * 747796405u + 2891336453u;
    uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
    return (word >> 22u) ^ word;
}

float Rand01(inout uint rngState)
{
    rngState = PcgHash(rngState);
    return float(rngState) * (1.0f / 4294967296.0f);
}

float3 SampleCosineHemisphere(float2 u, float3 N)
{
    float r = sqrt(u.x);
    float phi = 2.0f * 3.14159265f * u.y;
    float3 dir;
    dir.x = r * cos(phi);
    dir.y = r * sin(phi);
    dir.z = sqrt(max(0.0f, 1.0f - u.x));

    // 构造法线坐标系
    float3 up = abs(N.z) < 0.999f ? float3(0, 0, 1) : float3(1, 0, 0);
    float3 T = normalize(cross(up, N));
    float3 B = cross(N, T);
    return normalize(T * dir.x + B * dir.y + N * dir.z);
}

// -----------------------------------------------------------------------------
// 屏幕空间工具
// 复用引擎在 VarBase_PerCamera.cginc 中提供的 GetWorldPositionFromDepthValue,
// 保证 y 翻转/投影矩阵与引擎其他 Pass 完全一致.
// (jitter 已从投影矩阵剥离, 由 cbPerCamera.JitterOffset 单独消费, 这里不需要再选择矩阵分支)
// -----------------------------------------------------------------------------
float3 ReconstructWorldPos(float2 uv, float depth)
{
    return GetWorldPositionFromDepthValue(uv, depth).xyz;
}

// 与 GetWorldPositionFromDepthValue 互逆: 该函数把 (uv, depth) 通过
//     H = (uv.x*2-1, 1 - uv.y*2, depth, 1) 反投影到世界坐标.
// 因此正向投影后, ndc.x -> uv.x = ndc.x*0.5+0.5, ndc.y -> uv.y = 0.5 - ndc.y*0.5
// 返回 (uv, ndc.z), linearDepth = clip.w (即 view 空间 z, 透视除法前)
float3 WorldToScreen(float3 worldPos, out float linearDepth)
{
    //float4 H = float4(uv.x * 2.0f - 1.0f, 1.0f - uv.y * 2.0f, depthNdc, 1.0f);
    //float4 D = mul(H, GetViewPrjMtxInverse());
    //return D / D.w;
    float4 clip = mul(float4(worldPos, 1.0f), GetViewPrjMtx());
    linearDepth = clip.w;
    float3 ndc = clip.xyz / max(abs(clip.w), 1e-6f) * sign(clip.w);
    float2 uv;
    uv.x = ndc.x * 0.5f + 0.5f;
    uv.y = 0.5f - ndc.y * 0.5f;
    return float3(uv, ndc.z);
}

// -----------------------------------------------------------------------------
// cbReSTIR: 4 个 ReSTIR pass 共享同一份 CBuffer layout
// C# 端 TtReSTIRGINode.GetOrCreateSharedCBuffer 按此 layout 统一写入,
// 各 pass 只使用其中一部分字段, 未使用字段保持为 0
// -----------------------------------------------------------------------------
cbuffer cbReSTIR
{
    uint2  ScreenSize;
    uint   FrameIndex;
    float  MaxRayDistance;
    uint   MaxRayMarchSteps;
    float  ThicknessBias;
    float  TemporalMaxM;
    float  NormalThreshold;
    float  DepthThreshold;
    uint   SpatialSampleCount;
    float  SpatialRadius;
    float  Intensity;
    float  MaxRadiance;
    uint   InitialSampleCount;
    float3 SkyColor;            // 天光 fallback 颜色 (linear, 未乘 intensity)
    float  SkyIntensity;        // 天光强度倍率, 0 表示禁用 miss 时的天光贡献
    uint   HzbMaxMip;           // HZB 纹理的最大 mip 层级, 由 C# 端传入
};

// -----------------------------------------------------------------------------
// 天光采样 helper:
//   ENV_USE_SKY_CUBE == 0: 用 cbReSTIR.SkyColor * SkyIntensity (常量天光)
//   ENV_USE_SKY_CUBE == 1: 采样 EnvMap (TextureCube), 再乘 SkyIntensity
//
// EnvMap / Samp_EnvMap 由调用方 (ReSTIRInitialSampling.compute) 负责声明,
// 因为只有 Initial pass 需要在 ray miss 时采天光.
// -----------------------------------------------------------------------------
float3 SampleSkyRadiance(float3 dirWS)
{
#if defined(ENV_USE_SKY_CUBE) && (ENV_USE_SKY_CUBE == 1)
    return EnvMap.SampleLevel(Samp_EnvMap, dirWS, 0).rgb * SkyIntensity;
#else
    return SkyColor * SkyIntensity;
#endif
}

// -----------------------------------------------------------------------------
// Hi-Z 屏幕空间 ray march (linear view-space z)
//
// 算法: 从最粗 mip 起步, 纯粹"由粗到细"的层级遍历.
//   - 不相交 → 推进当前 mip cell 边界 (mip 不变, 步长 = 当前 mip cell 大小)
//   - 相交且 mip > 0 → mip-- 细化, ray 不动
//   - 相交且 mip == 0 → 精确命中判定; miss 则推进 mip 0 cell 边界, mip 重置回 startMip
// mip 只有"向下细化"和"重置回起始"两种变化, 永远不会出现 k ↔ k-1 振荡.
//
// Hzb 约定 (TtHzbNode):
//   Texture2D<float2>, mip0 = screen/2, R = tile minZ, G = tile maxZ (linear view-space z)
//
// 调用方约定:
//   起点防自相交由调用方负责 (originWS 沿 dirWS 推一小步再算 startSS).
// -----------------------------------------------------------------------------

// 2^mip 查表, 替代循环内 exp2(float(mip)) 的浮点运算. 支持 mip 0~12.
static const float kMipScale[13] = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096 };

// 推进 ray 到当前 mip 网格下的下一个 cell 内部. 返回新 uv, 通过 dT 返回 t 步进量.
float2 HiZ_StepToNextCell(float2 curUV, float2 dirUV, float invDirLen,
                          float2 mipSize, out float dT)
{
    float2 cellSize = 1.0f / mipSize;
    float2 cellIdx  = floor(curUV * mipSize);

    // 目标边: dir >= 0 取 cell 右/上边, dir < 0 取左/下边
    float2 sideSel  = step(0.0f, dirUV); // 0 or 1
    float2 boundary = (cellIdx + sideSel) * cellSize;

    // 偏置 1/16 cell, 保证严格落入下一 cell 内部 (避开边界浮点歧义)
    float2 crossDir = sign(dirUV);
    // sign(0) = 0, 这种轴不参与推进, 对应 t = +inf
    boundary += crossDir * (cellSize * 0.0625f);

    float2 dist = boundary - curUV;
    float2 t2;
    t2.x = (abs(dirUV.x) > 1e-6f) ? (dist.x / dirUV.x) : 1e10f;
    t2.y = (abs(dirUV.y) > 1e-6f) ? (dist.y / dirUV.y) : 1e10f;
    float tStep = max(min(t2.x, t2.y), 0.0f); // max(,0) 防负值

    dT = tStep * invDirLen;
    return curUV + dirUV * tStep;
}

// 计算 hzb 有意义的最大 mip 层级 (mip0 Size 逐级除 2 直到 1x1)
// 使用 firstbithigh 做整数 floor(log2), 避免浮点精度问题 (如 512.0 经 log2 得到 8.999… 被截断为 8)
uint HiZ_CalcMaxMip(float2 hzbMip0Size)
{
    uint maxDim = (uint)max(hzbMip0Size.x, hzbMip0Size.y);
    return (maxDim >= 2u) ? firstbithigh(maxDim) : 0u;
}

bool HiZTraceScreenSpace(Texture2D<float2> hzbTex, SamplerState hzbSamp,
                         float2 hzbMip0Size,
                         float3 startSS, float3 endSS,
                         float startLinearZ, float endLinearZ,
                         uint maxSteps, float thicknessBias,
                         out float2 hitUV)
{
    hitUV = 0;

    // ---- ray 参数 ----
    float2 dirUV   = endSS.xy - startSS.xy;
    float  dirLen2 = dot(dirUV, dirUV);
    if (dirLen2 < 1e-12f)
        return false;
    float invDirLen  = rsqrt(dirLen2);
    float linearZDir = endLinearZ - startLinearZ;

    // ---- mip 范围: 由 C# 端通过 cbReSTIR.HzbMaxMip 传入 ----
    uint startMip = min(HzbMaxMip, 12u);

    // ---- ray 状态: uv 位置, t∈[0,1] 沿整段进度, 当前 mip ----
    float2 curUV = startSS.xy;
    float  curT  = 0.0f;
    uint   mip   = startMip;

    [loop]
    for (uint i = 0u; i < maxSteps; ++i)
    {
        // ---- 终止判定 ----
        // UV xy 出界由调用方 (CastIndirectRay) 预截断保证不发生.
        // 深度出界用 linear z 判定 (NDC z 经过 1/w 透视除法是非线性的, 线性插值不准确).
        // linearZ <= 0 意味着射线跑到了摄像机后面.
        if (curT >= 1.0f)
            return false;
        float curLinearZ = startLinearZ + linearZDir * curT;
        if (curLinearZ <= 0.0f)
            return false;

        // ---- 取当前 mip tile 的 [minZ, maxZ] ----
        float2 mipSize    = max(float2(1.0f, 1.0f), hzbMip0Size / kMipScale[mip]);
        float2 cellSize   = 1.0f / mipSize;
        float2 cellIdx    = floor(curUV * mipSize);
        float2 tileCenter = (cellIdx + 0.5f) * cellSize;
        float2 tileMinMax = hzbTex.SampleLevel(hzbSamp, tileCenter, (float)mip).rg;
        float  tileMinZ   = tileMinMax.x;
        float  tileMaxZ   = tileMinMax.y;

        // ---- ray 段 z 范围 (当前 cell 内) ----
        float dT_step;
        float2 nextUV = HiZ_StepToNextCell(curUV, dirUV, invDirLen, mipSize, dT_step);
        float  nextT  = min(curT + dT_step, 1.0f);
        float  curZ   = startLinearZ + linearZDir * curT;
        float  nextZ  = startLinearZ + linearZDir * nextT;
        float  segMinZ = min(curZ, nextZ);
        float  segMaxZ = max(curZ, nextZ);

        bool intersect = !(segMaxZ < tileMinZ || segMinZ > tileMaxZ);

        if (!intersect)
        {
            // 整 tile 不可能命中 → 推进到当前 mip cell 边界, mip 不变.
            // 因为 mip 不变, ray 下次循环判的是同 mip 的下一个 cell, 不存在
            // "跨子 cell 但仍在父 cell 内 → 立刻被父 cell 拉回"的振荡.
            curUV = nextUV;
            curT  = nextT;
            continue;
        }

        // ---- 相交: 需要细化或精确判定 ----

        if (mip > 0u)
        {
            // 细化: ray 不动, 用更细 mip 的更小 tile 重新判
            mip = mip - 1u;
            continue;
        }
        else
        {
            hitUV = startSS.xy + dirUV * curT;
            return true;
        }
    }

    return false;
}

#endif // _ReSTIR_COMMON_H_
