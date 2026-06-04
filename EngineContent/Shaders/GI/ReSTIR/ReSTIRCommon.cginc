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
// Hi-Z 屏幕空间 ray march
//
// 薄 wrapper, 委托给通用 HzbRayCast (Inc/HzbRayCast.cginc).
// 保留旧函数签名以兼容 ReSTIRInitialSampling.compute 的调用方.
//
// startSS / endSS: float3(uv.x, uv.y, ndcZ), 由 WorldToScreen 产出.
//   注意: HzbRayCast 需要 Screen 空间 (NDC xy [-1,1] + DeviceZ),
//   此 wrapper 负责 UV→NDC 的转换.
// startLinearZ / endLinearZ: 未使用 (保留签名兼容性).
// HzbMaxMip: 由 C# 端通过 cbReSTIR 传入的最大 mip 层级.
// -----------------------------------------------------------------------------
#include "../../Inc/HzbRayCast.cginc"

bool HiZTraceScreenSpace(Texture2D<float> hzbTex, SamplerState hzbSamp,
                         float2 hzbMip0Size,
                         float3 startSS, float3 endSS,
                         float startLinearZ, float endLinearZ,
                         uint maxSteps, float thicknessBias,
                         out float2 hitUV)
{
    hitUV = 0;

    // startSS/endSS = float3(uv, ndcZ) from WorldToScreen.
    // Convert UV [0,1] back to NDC [-1,1]:
    //   ndc.x = (uv.x - 0.5) * 2    = uv.x * 2 - 1
    //   ndc.y = (0.5 - uv.y) * 2    = 1 - uv.y * 2
    float3 rayStartScreen = float3(startSS.x * 2.0 - 1.0, 1.0 - startSS.y * 2.0, startSS.z);
    float3 rayEndScreen   = float3(endSS.x   * 2.0 - 1.0, 1.0 - endSS.y   * 2.0, endSS.z);
    float3 rayStepScreen  = rayEndScreen - rayStartScreen;

    // Clip to screen edge
    float clipFactor = min(HzbRayCast_ClipToScreenEdge(rayStartScreen.xy, rayStepScreen.xy), 1.0);
    rayStepScreen *= clipFactor;

    // CompareTolerance: full-ray Z span (will be divided by numSteps inside HzbRayCast)
    float compareTolerance = abs(rayStepScreen.z);

    // GI rays use roughness=0 (sharp trace at mip 0)
    FHzbRayCastResult r = HzbRayCast(hzbTex, hzbSamp,
                                     rayStartScreen, rayStepScreen,
                                     compareTolerance,
                                     maxSteps, 0.0,
                                     0.0, 0.0);
    hitUV = r.HitUV;
    return r.bHit;
}

#endif // _ReSTIR_COMMON_H_
