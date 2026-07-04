#ifndef __TEMPORAL_AA_H__
#define __TEMPORAL_AA_H__
#include "../Inc/Math.cginc"

static const int2 kOffsets3x3[9] =
{
    int2(-1, -1),
    int2(0, -1),
    int2(1, -1),
    int2(-1,  0),
    int2(0,  0),
    int2(1,  0),
    int2(-1,  1),
    int2(0,  1),
    int2(1,  1),
};

struct TAA
{
    static float2 ViewportClamp(float2 uv)
    {
        return uv;
    }
    static float CmpDepth(float a, float b)
    {
        return step(a, b);
    }
    Texture2D ColorBuffer;
    SamplerState Samp_ColorBuffer;

    Texture2D PrevColorBuffer;
    SamplerState Samp_PrevColorBuffer;

    Texture2D DepthBuffer;
    SamplerState Samp_DepthBuffer;

    Texture2D PrevDepthBuffer;
    SamplerState Samp_PrevDepthBuffer;

    Texture2D MotionBuffer;
    SamplerState Samp_MotionBuffer;

    float GetDepth(float2 uv)
    {
        return DepthBuffer.SampleLevel(Samp_DepthBuffer, uv.xy, 0).r;
    }
    float2 GetClosestUV(float2 uv)
    {
        float2 k = ViewportSizeAndRcp.xy;
        const float4 neighborhood = float4(
            GetDepth(ViewportClamp(uv - k)),
            GetDepth(ViewportClamp(uv + float2(k.x, -k.y))),
            GetDepth(ViewportClamp(uv + float2(-k.x, k.y))),
            GetDepth(ViewportClamp(uv + k))
            );

        float3 result = float3(0.0, 0.0, GetDepth(uv));
        result = lerp(result, float3(-1.0, -1.0, neighborhood.x), CmpDepth(neighborhood.x, result.z));
        result = lerp(result, float3(1.0, -1.0, neighborhood.y), CmpDepth(neighborhood.y, result.z));
        result = lerp(result, float3(-1.0, 1.0, neighborhood.z), CmpDepth(neighborhood.z, result.z));
        result = lerp(result, float3(1.0, 1.0, neighborhood.w), CmpDepth(neighborhood.w, result.z));
        return (uv + result.xy * k);
    }

    float3 ClipHistory(float3 History, float3 BoxMin, float3 BoxMax)
    {
        float3 Filtered = (BoxMin + BoxMax) * 0.5f;
        float3 RayOrigin = History;
        float3 RayDir = Filtered - History;
        RayDir = V_Select(abs(RayDir) < (1.0 / 65536.0), (1.0 / 65536.0), RayDir);
        float3 InvRayDir = rcp(RayDir);

        float3 MinIntersect = (BoxMin - RayOrigin) * InvRayDir;
        float3 MaxIntersect = (BoxMax - RayOrigin) * InvRayDir;
        float3 EnterIntersect = min(MinIntersect, MaxIntersect);
        float ClipBlend = max(EnterIntersect.x, max(EnterIntersect.y, EnterIntersect.z));
        ClipBlend = saturate(ClipBlend);
        return lerp(History, Filtered, ClipBlend);
    }

    // Luma scaled by 4 for efficiency (matches UE4 Luma4).
    static float Luma4(float3 Color)
    {
        return (Color.g * 2.0) + (Color.r + Color.b);
    }

    // HDR weight for neighborhood sampling (UE4 Karis 2014).
    // Attenuates bright specular outliers so they don't dominate the AABB.
    static float HdrWeight4(float3 Color)
    {
        return rcp(Luma4(Color) + 4.0);
    }

    // HDR weight for final temporal blend (standard Karis luminance weight).
    static float HdrWeight(float3 Color)
    {
        return rcp(1.0 + dot(Color, float3(0.2126, 0.7152, 0.0722)));
    }

    // Compute HDR-weighted filtered current-frame color from the 3×3 neighborhood.
    // This spatially low-passes bright specular pixels so a single-pixel highlight
    // doesn't cause the output to flash when it appears/disappears between frames.
    // Reference: UE FilterCurrentFrameInputSamples (TemporalAA.usf)
    float3 FilterCurrentFrame(float2 uv)
    {
        // Plus-shaped (5-tap) HDR-weighted filter for speed.
        // Cardinal neighbors get weight 1, center gets weight 2 (Bartlett-like).
        static const int2 filterOffsets[5] = {
            int2( 0,  0),
            int2( 0, -1),
            int2(-1,  0),
            int2( 1,  0),
            int2( 0,  1),
        };
        static const float filterSpatialWeights[5] = { 2.0, 1.0, 1.0, 1.0, 1.0 };

        float3 filteredColor = 0;
        float totalWeight = 0;

        [unroll]
        for (int i = 0; i < 5; i++)
        {
            float3 sampleRGB = ColorBuffer.SampleLevel(Samp_ColorBuffer,
                uv + (filterOffsets[i] * ViewportSizeAndRcp.zw), 0).rgb;
            sampleRGB = sRGB2Linear(sampleRGB);
            float hdrW = HdrWeight4(sampleRGB);
            float w = filterSpatialWeights[i] * hdrW;
            filteredColor += sampleRGB * w;
            totalWeight += w;
        }

        return filteredColor / totalWeight;
    }

    // Variance Clipping + HDR-weighted neighborhood in YCoCg space.
    // Replaces min/max AABB clamp with mu ± gamma*sigma for tighter rejection
    // of stale history, which stabilizes specular highlights without a separate pass.
    float GetBlendFactor(half4 FilteredColor, inout half4 HistoryColor, float2 Depth, float2 uv, float2 HistoryUV, float2 Motion, float alpha)
    {
        // Accumulate HDR-weighted moments in YCoCg space for variance clipping.
        float3 m1 = 0;
        float3 m2 = 0;
        float totalWeight = 0;

        for (int k = 0; k < 9; k++)
        {
            float3 sampleRGB = ColorBuffer.SampleLevel(Samp_ColorBuffer, uv + (kOffsets3x3[k] * ViewportSizeAndRcp.zw), 0).rgb;
            sampleRGB = sRGB2Linear(sampleRGB);
            float3 sampleYCoCg = RGBToYCoCg(sampleRGB);
            float w = HdrWeight4(sampleRGB);

            m1 += sampleYCoCg * w;
            m2 += sampleYCoCg * sampleYCoCg * w;
            totalWeight += w;
        }

        // Compute mean and standard deviation for variance clipping.
        float3 mu = m1 / totalWeight;
        float3 sigma = sqrt(abs(m2 / totalWeight - mu * mu));
        float gamma = 1.0; // Tighter = less ghosting, looser = less flickering.

        float3 varianceMin = mu - gamma * sigma;
        float3 varianceMax = mu + gamma * sigma;

        // Clip history to the variance box (tighter than min/max AABB).
        float3 histYCoCg = RGBToYCoCg(HistoryColor.rgb);
        float3 clippedYCoCg = ClipHistory(histYCoCg, varianceMin, varianceMax);
        HistoryColor.rgb = YCoCgToRGB(clippedYCoCg);

        // Blend factor: base alpha + velocity-driven increase.
        float velocityScale = 1000;
        float BlendFactor = saturate(alpha + length(Motion) * velocityScale);

        // Luma-contrast anti-flicker: 高对比度邻域 (高光紧邻暗区) 中, variance box 帧间
        // 振荡剧烈, 需要增加新帧权重来快速跟踪变化, 避免闪烁.
        // 低对比度区域 variance 小, history 稳定可信, 不应拉高 blend factor.
        float LumaMin = RGBToYCoCg(mu - sigma).x;
        float LumaMax = RGBToYCoCg(mu + sigma).x;
        float LumaContrast = LumaMax - LumaMin;
        float LumaContrastFactor = 32.0;
        // 高对比度时 antiFlicker → 1 (多用新帧); 低对比度时 → 0 (信任 history).
        float antiFlicker = saturate(LumaContrast * LumaContrastFactor);
        BlendFactor = max(BlendFactor, antiFlicker * alpha * 4.0);

        // Luminance-difference stability: 当 history 和当前帧差异极大时 (disocclusion/
        // 场景切换), 增加新帧权重加速收敛. 差异小时不抬高 blend.
        float LumaHistory = Luma4(RGBToYCoCg(HistoryColor.rgb));
        float LumaFiltered = Luma4(RGBToYCoCg(FilteredColor.rgb));
        float lumaDiff = abs(LumaFiltered - LumaHistory);
        // 只在差异足够大（相对 history 亮度超过 10%）时才抬高 blend factor.
        float lumaReject = saturate(lumaDiff / max(LumaHistory * 0.1, 0.01));
        BlendFactor = max(BlendFactor, lumaReject * 0.5);

        // History off-screen → discard.
        if (any(HistoryUV < 0) || any(HistoryUV > 1.0f))
        {
            BlendFactor = 1.0f;
        }

        return BlendFactor;
    }
    float3 GetTAAColor(float2 screen_uv, float2 JitterUV, float2 PreJitterUV, float alpha)
    {
        float2 currUV = screen_uv;

        // Use HDR-weighted filtered color instead of raw center pixel.
        // This spatially spreads bright specular highlights so they don't
        // cause per-frame on/off flickering in the temporal blend.
        half4 Color;
        Color.rgb = (half3)FilterCurrentFrame(currUV);
        Color.a = 1;

        float2 Depth;
        Depth.x = DepthBuffer.SampleLevel(Samp_DepthBuffer, currUV, 0).r;
        
        float2 Motion = DecodeMotionVector(MotionBuffer.SampleLevel(Samp_MotionBuffer, currUV.xy, 0).xy);
        // Jitter 不改变像素在 RT 中的存储坐标 (只影响光栅化的亚像素采样偏移),
        // 因此 history reprojection 只需 screen_uv - Motion, 不应包含任何 jitter 项.
        // 旧写法 "screen_uv - JitterUV - Motion + PreJitterUV" 会在每帧引入不同的
        // 亚像素偏移到 HistoryUV, 导致静止场景全像素抖动.
        float2 HistoryUV = screen_uv.xy - Motion.xy;
        HistoryUV = saturate(HistoryUV.xy);
        half4 HistoryColor = (half4) PrevColorBuffer.SampleLevel(Samp_PrevColorBuffer, HistoryUV.xy, 0);
        if (any(isnan(HistoryColor)))
        {
            HistoryColor = (half4) 0;
        }
        HistoryColor.rgb = sRGB2Linear((half3) HistoryColor.rgb);
        Depth.y = PrevDepthBuffer.SampleLevel(Samp_PrevDepthBuffer, HistoryUV.xy, 0).r;

        Depth = LinearFromDepth(Depth);

        // 邻域 AABB 也用 currUV 作为中心, 保证 clamp 范围与 Color 来自同一空间.
        float blendFactor = GetBlendFactor(Color, HistoryColor, Depth, currUV, HistoryUV, Motion, alpha);

        // HDR-weighted blend (Karis 2014): prevents bright specular highlights
        // in the combined buffer from being erased by dark current frames or vice versa.
        float wCurr = HdrWeight(Color.rgb) * blendFactor;
        float wHist = HdrWeight(HistoryColor.rgb) * (1.0 - blendFactor);
        float3 result = (Color.rgb * wCurr + HistoryColor.rgb * wHist)
            / max(wCurr + wHist, 1e-5);
        result.rgb = Linear2sRGB((half3) result.rgb);
        return result;
    }
    // GetTAAColor2: 用 closest depth 邻域选 motion 的版本 (适合处理边缘/遮挡变化更稳).
    // 反 jitter 的处理与 GetTAAColor 一致, 详见上面的注释.
    // GetTAAColor2: 用 closest depth 邻域选 motion 的版本 (适合处理边缘/遮挡变化更稳).
    float3 GetTAAColor2(float2 screen_uv, float2 JitterUV, float2 PreJitterUV, float alpha)
    {
        float2 currUV = screen_uv - JitterUV;

        // Use HDR-weighted filtered color instead of raw center pixel.
        half4 Color;
        Color.rgb = (half3)FilterCurrentFrame(currUV);
        Color.a = 1;

        float2 Depth;
        Depth.x = DepthBuffer.Sample(Samp_DepthBuffer, currUV).r;

        // 在 3x3 邻域里挑离镜头最近的点的 UV, 用它去采 motion vector.
        // 物体边缘上选最近点能避免 disocclusion 像素拿到错误的 motion (背景的 motion).
        float2 closest = GetClosestUV(currUV.xy);
        float2 Motion = DecodeMotionVector(MotionBuffer.SampleLevel(Samp_MotionBuffer, closest.xy, 0).xy);
        // 同 GetTAAColor: HistoryUV 只跟 motion 有关, 不含 jitter 项.
        float2 HistoryUV = screen_uv.xy - Motion;
        half4 HistoryColor = PrevColorBuffer.Sample(Samp_PrevColorBuffer, HistoryUV);
        HistoryColor.rgb = sRGB2Linear((half3)HistoryColor.rgb);
        Depth.y = PrevDepthBuffer.Sample(Samp_PrevDepthBuffer, HistoryUV.xy).r;

        float blendFactor = GetBlendFactor(Color, HistoryColor, Depth, currUV, HistoryUV, Motion, alpha);

        // HDR-weighted blend, same as GetTAAColor.
        float wCurr = HdrWeight(Color.rgb) * blendFactor;
        float wHist = HdrWeight(HistoryColor.rgb) * (1.0 - blendFactor);
        float3 result = (Color.rgb * wCurr + HistoryColor.rgb * wHist)
            / max(wCurr + wHist, 1e-5);
        result.rgb = Linear2sRGB((half3)result.rgb);
        return result;
    }
};

#endif
//