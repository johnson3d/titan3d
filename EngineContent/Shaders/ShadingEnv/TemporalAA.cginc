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

    // YCoCg 颜色空间下的 neighborhood clamp + 时序权重计算
    float GetBlendFactor(half4 Color, inout half4 HistoryColor, float2 Depth, float2 uv, float2 HistoryUV, float2 Motion, float alpha)
    {
        // 3x3 邻域 AABB
        half3 AABBMin, AABBMax;
        AABBMax = AABBMin = RGBToYCoCg(Color.rgb);
        for (int k = 0; k < 9; k++)
        {
            half3 C = RGBToYCoCg(ColorBuffer.SampleLevel(Samp_ColorBuffer, uv + (kOffsets3x3[k] * ViewportSizeAndRcp.zw), 0));
            AABBMin = min(AABBMin, C);
            AABBMax = max(AABBMax, C);
        }
        half3 HistoryYCoCg = RGBToYCoCg(HistoryColor);
        // Clamp 比 Clip 更稳, 不容易出现 disocclusion 时的 ghost 残留
        HistoryColor.rgb = YCoCgToRGB(clamp(HistoryYCoCg, AABBMin, AABBMax));

        // 速度越大, 越偏向当前帧 (减少快速运动时的拖尾)
        float scaleLength = 1000;
        float BlendFactor = saturate(alpha + length(Motion) * scaleLength);
        
        // 深度 reject: 去掉了，这个不应该开启，边缘如果reject，那么线条永远无法AA
        //float depthRefer = max(Depth.x, 1e-3f);
        //float depthThreshold = 0.1f * depthRefer + 0.5f;
        //if (abs(Depth.y - Depth.x) > depthThreshold)
        //{
        //    BlendFactor = 1.0f;
        //}
        // History 采样越界 (上帧物体不在屏幕里), 直接抛弃
        if (HistoryUV.x < 0 || HistoryUV.y < 0 || HistoryUV.x > 1.0f || HistoryUV.y > 1.0f)
        {
            BlendFactor = 1.0f;
        }
        //BlendFactor = alpha;
        return BlendFactor;
    }
    float3 GetTAAColor(float2 screen_uv, float2 JitterUV, float2 PreJitterUV, float alpha)
    {
        float2 currUV = screen_uv;
        half4 Color = (half4) ColorBuffer.SampleLevel(Samp_ColorBuffer, currUV, 0);
        //return Color;
        Color.rgb = sRGB2Linear(Color.rgb);
        float2 Depth;
        Depth.x = DepthBuffer.SampleLevel(Samp_DepthBuffer, currUV, 0).r;
        
        float2 Motion = DecodeMotionVector(MotionBuffer.SampleLevel(Samp_MotionBuffer, currUV.xy, 0).xy);
        // History 采样也需要反偏上一帧的 jitter, 否则静止场景下当前帧和历史帧的
        // jitter 不同会导致混合结果在帧间跳动.
        float2 HistoryUV = screen_uv.xy - JitterUV - Motion.xy + PreJitterUV;
        half4 HistoryColor = (half4) PrevColorBuffer.SampleLevel(Samp_PrevColorBuffer, HistoryUV.xy, 0);
        HistoryColor.rgb = sRGB2Linear((half3) HistoryColor.rgb);
        Depth.y = PrevDepthBuffer.SampleLevel(Samp_PrevDepthBuffer, HistoryUV.xy, 0).r;

        Depth = LinearFromDepth(Depth);

         // 邻域 AABB 也用 currUV 作为中心, 保证 clamp 范围与 Color 来自同一空间.
        float blendFactor = GetBlendFactor(Color, HistoryColor, Depth, currUV, HistoryUV, Motion, alpha);
        float3 result = lerp(HistoryColor.rgb, Color.rgb, blendFactor);
        result.rgb = Linear2sRGB((half3) result.rgb);
        return result;
    }
    // GetTAAColor2: 用 closest depth 邻域选 motion 的版本 (适合处理边缘/遮挡变化更稳).
    // 反 jitter 的处理与 GetTAAColor 一致, 详见上面的注释.
    float3 GetTAAColor2(float2 screen_uv, float2 JitterUV, float2 PreJitterUV, float alpha)
    {
        float2 currUV = screen_uv - JitterUV;

        float2 Depth;
        half4 Color = (half4)ColorBuffer.Sample(Samp_ColorBuffer, currUV);
        Color.rgb = sRGB2Linear(Color.rgb);
        Depth.x = DepthBuffer.Sample(Samp_DepthBuffer, currUV).r;

        // 在 3x3 邻域里挑离镜头最近的点的 UV, 用它去采 motion vector.
        // 物体边缘上选最近点能避免 disocclusion 像素拿到错误的 motion (背景的 motion).
        float2 closest = GetClosestUV(currUV.xy);
        float2 Motion = DecodeMotionVector(MotionBuffer.SampleLevel(Samp_MotionBuffer, closest.xy, 0).xy);
        // History 采样也需要反偏上一帧的 jitter, 与 GetTAAColor 保持一致.
        float2 HistoryUV = screen_uv.xy - Motion - PreJitterUV;
        half4 HistoryColor = PrevColorBuffer.Sample(Samp_PrevColorBuffer, HistoryUV);
        HistoryColor.rgb = sRGB2Linear((half3)HistoryColor.rgb);
        Depth.y = PrevDepthBuffer.Sample(Samp_PrevDepthBuffer, HistoryUV.xy).r;

        // 注意: GetTAAColor2 旧实现没有调用 LinearFromDepth, 这里保持原样.
        // 如果想启用相对深度 reject, 需要先调 LinearFromDepth 再传给 GetBlendFactor.

        float blendFactor = GetBlendFactor(Color, HistoryColor, Depth, screen_uv, HistoryUV, Motion, alpha);
        float3 result = lerp(HistoryColor.rgb, Color.rgb, blendFactor);
        result.rgb = Linear2sRGB((half3)result.rgb);
        return result;
    }
};

#endif
//