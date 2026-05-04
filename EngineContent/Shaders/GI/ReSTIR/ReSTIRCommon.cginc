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
// Hi-Z 屏幕空间 ray march (linear view-space z 版本):
//   通用工具函数, 不依赖任何全局资源 -> 函数定义无 #if 宏门.
//   调用方按需把 hzb 纹理 / 采样器作为参数传入, 由 entry point 决定是否调用.
//
// Hzb 数据约定 (来自 TtHzbNode, linear-z 版):
//   - Texture2D<float2>, multi-mip
//   - mip0 尺寸 = 屏幕分辨率 / 2 (整除)
//   - 每 texel: R = min(2x2 邻域 view-space linear z), G = max
//   - 单调"越远值越大", 与 USE_INVERSE_Z 完全无关
//
// 接口:
//   - hzbTex / hzbSamp: 调用方传入的 hi-z 纹理和 point clamp 采样器
//   - hzbMip0Size: hzb mip0 在 uv 坐标系下的像素数 (= 屏幕分辨率 / 2),
//     由调用方传入避免函数内 GetDimensions (DXC 在某些 mip 下会取不到正确值)
//   - startSS / endSS: ray 端点的屏幕坐标 (xy = uv [0,1], z = NDC depth, 仅供 [0,1] 出屏判定)
//   - startLinearZ / endLinearZ: 端点的 view-space linear z (来自 WorldToScreen 的 clip.w),
//     hi-z 比较的真正口径
//   - thicknessBias: view-space 距离 (世界单位/米), 表示"ray 在表面前方多近视为命中"
//   - 输出 hitUV: 命中像素的屏幕 uv. 调用方拿到后用它重采 DepthBuffer 拿原 NDC z 做
//     ReconstructWorldPos, 与原线性 march 路径一致, 不再返回 hitSceneZ.
//
// 算法 (Wronski / Frostbite SSR 简化版):
//   1. 当前 mip k 上, 找到 ray 当前位置所在 tile
//   2. 求 ray 在该 tile 内沿屏幕方向到 tile 边界的归一化 t 推进
//   3. 段在 linear-z 上的 [segMinZ, segMaxZ] 与 tile 的 [minZ, maxZ] 求相交:
//      - 整段更近或整段更远 -> 整 tile 不可能命中, 推进到 tile 边界, mip++
//      - 否则 -> 相交, mip-- 细化, mip 0 上找具体命中
//   4. 命中条件 (mip 0): ray.linearZ 落在 [tile.minZ, tile.minZ + thicknessBias] 内
//   5. 出屏 / 超 maxSteps -> 返回 false (调用方按 miss 处理)
//
// 设计取舍:
//   - linear-z 单调"越远越大", 比较逻辑无 USE_INVERSE_Z 分支, shader 简单且数值稳定
//   - thicknessBias 在 linear-z 下的物理含义 = 米 (e.g. 0.05 = 5cm), 用户预期能对齐
//   - tile 边界推进用 ray-AABB slab 法的 2D 简化版
// -----------------------------------------------------------------------------

// 求 ray (rayUV, rayDirUV) 在以 cellMin/cellMax 为边界的 tile 内, 走到任一边界
// 所需的最小正向 t. dirUV 任一分量为 0 时该轴 t = +inf.
float HiZ_DistToCellBoundary(float2 rayUV, float2 dirUV, float2 cellMin, float2 cellMax)
{
    // 相对于当前 ray 位置, 朝着 dir 方向的 tile 边界 (x/y 各取一条)
    float2 boundary;
    boundary.x = (dirUV.x >= 0.0f) ? cellMax.x : cellMin.x;
    boundary.y = (dirUV.y >= 0.0f) ? cellMax.y : cellMin.y;

    float2 t2 = (boundary - rayUV);
    // dir 接近 0 的轴, 给一个极大值, 让 min 选另一轴
    t2.x = (abs(dirUV.x) > 1e-6f) ? (t2.x / dirUV.x) : 1e10f;
    t2.y = (abs(dirUV.y) > 1e-6f) ? (t2.y / dirUV.y) : 1e10f;
    // 让推进略微越过边界, 避免下一次迭代仍然停在同一个 tile (浮点抖动)
    return min(t2.x, t2.y) + 1e-5f;
}

// hi-z trace. 命中返回 true + hitUV; 未命中 (出屏 / 步数耗尽) 返回 false.
// hzbTex / hzbSamp / hzbMip0Size 由调用方传入, 函数本身不依赖任何全局资源.
bool HiZTraceScreenSpace(Texture2D<float2> hzbTex, SamplerState hzbSamp,
                         float2 hzbMip0Size,
                         float3 startSS, float3 endSS,
                         float startLinearZ, float endLinearZ,
                         uint maxSteps, float thicknessBias,
                         out float2 hitUV)
{
    hitUV = 0;

    // ---------- 屏幕方向 ----------
    float2 dirUV = endSS.xy - startSS.xy;
    float dirLen2 = dot(dirUV, dirUV);
    if (dirLen2 < 1e-12f)
        return false;
    float invDirLen = rsqrt(dirLen2);

    // ---------- linear-z 段方向 ----------
    // ray 沿 [start->end] 走到 t∈[0,1], cur.linearZ = lerp(startLinearZ, endLinearZ, t).
    // 所以"段在 linear z 方向"的方向 = (endLinearZ - startLinearZ) (沿整段 t 的导数).
    float linearZDir = endLinearZ - startLinearZ;

    // hzb 总 mip 数保守上限. SampleLevel 越界自动 clamp 到最高 mip, 不会 OOB.
    const uint kMaxMip = 12u;  // 4096x4096 / 2 = 2048 -> 11 mip, 12 安全

    // 当前 ray 状态: uv 位置, 沿整段 [start->end] 的归一化进度 t∈[0,1], 当前 mip
    float2 curUV = startSS.xy;
    float  curT  = 0.0f;
    uint   mip   = 0u;

    [loop]
    for (uint i = 0u; i < maxSteps; ++i)
    {
        // 出屏 / 段已走完 -> 失败
        if (any(curUV < 0.0f) || any(curUV > 1.0f) || curT > 1.0f)
            return false;
        // 当前 ray 在 NDC z 上的位置, 仅用于"NDC 出 [0,1] 范围"的廉价兜底
        // (例如 view 矩阵把端点投到相机后方, ndc.z 会出界).
        float curNdcZ = lerp(startSS.z, endSS.z, curT);
        if (curNdcZ < 0.0f || curNdcZ > 1.0f)
            return false;

        // 当前 mip 的 tile 网格尺寸 (uv 单位)
        float2 mipSize = max(float2(1.0f, 1.0f), hzbMip0Size / exp2(float(mip)));
        float2 cellSize = 1.0f / mipSize;

        // 当前 ray 所在 tile 的 [cellMin, cellMax] (uv)
        float2 cellIdx = floor(curUV * mipSize);
        float2 cellMin = cellIdx * cellSize;
        float2 cellMax = cellMin + cellSize;

        // 取 tile 的 min/max linear z
        float2 tileCenter = cellMin + cellSize * 0.5f;
        float mipClamped = (float)min(mip, kMaxMip);
        float2 tileMinMaxZ = hzbTex.SampleLevel(hzbSamp, tileCenter, mipClamped).rg;
        float tileMinZ = tileMinMaxZ.x;
        float tileMaxZ = tileMinMaxZ.y;

        // 推进到当前 tile 在 ray 方向上的边界, 算出对应的 t (整段 [0,1] 上的归一化)
        float tCell_uv = HiZ_DistToCellBoundary(curUV, dirUV, cellMin, cellMax);
        // tCell_uv 是 dirUV 单位的推进, dirUV 长度 = sqrt(dirLen2), 所以归一化到整段 [0,1] 上要除以长度
        float dT = tCell_uv * invDirLen;
        float nextT = min(curT + dT, 1.0f);
        float2 nextUV = curUV + dirUV * dT;
        // ray 段 [curT, nextT] 在 linear z 上的范围
        float curLinearZ  = startLinearZ + linearZDir * curT;
        float nextLinearZ = startLinearZ + linearZDir * nextT;
        float segMinZ = min(curLinearZ, nextLinearZ);
        float segMaxZ = max(curLinearZ, nextLinearZ);

        // 段与 tile linear-z 范围 [tileMinZ, tileMaxZ] 不相交 -> 整 tile 不可能命中,
        // 推进到 tile 边界后用更粗 mip 继续跳
        if (segMaxZ < tileMinZ || segMinZ > tileMaxZ)
        {
            curUV = nextUV;
            curT  = nextT;
            mip = min(mip + 1u, kMaxMip);
            continue;
        }

        if (mip == 0u)
        {
            // mip 0 + 相交 -> 该 tile 内有命中
            // 用 tileMinZ 作为表面参考深度: ray.z 必须 >= tileMinZ (即在表面后方或表面上),
            // 且 ray.z <= tileMinZ + thicknessBias (在表面厚度范围内).
            float thickFar = tileMinZ + thicknessBias;
            if (segMaxZ >= tileMinZ && segMinZ <= thickFar)
            {
                // 取段内 ray 第一次进入 [tileMinZ, thickFar] 的 t 作为命中位置.
                // ray 沿 t 方向 linear z 单调 (lerp), 用 cross over 解析求 t.
                float tHit = curT;
                if (abs(linearZDir) > 1e-6f)
                {
                    // ray 进表面厚度区间的 t (取 ray 接近表面那一侧的 z 边界)
                    float zEnter = (linearZDir > 0.0f) ? tileMinZ : thickFar;
                    float tEnter = (zEnter - startLinearZ) / linearZDir;
                    tHit = clamp(tEnter, curT, nextT);
                }
                hitUV = startSS.xy + dirUV * tHit;
                return true;
            }
            // 厚度不满足 -> 越过本 tile 继续找下一个
            curUV = nextUV;
            curT  = nextT;
            continue;
        }

        // 非 mip 0 但有相交 -> 细化 mip, ray 位置不前进, 用更细 tile 重新判
        mip = mip - 1u;
    }

    return false;
}

#endif // _ReSTIR_COMMON_H_
