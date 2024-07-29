#ifndef __TEMPORAL_AA_H__
#define __TEMPORAL_AA_H__

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
        float2 k = gViewportSizeAndRcp.xy;
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
        RayDir = abs(RayDir) < (1.0 / 65536.0) ? (1.0 / 65536.0) : RayDir;
        float3 InvRayDir = rcp(RayDir);

        float3 MinIntersect = (BoxMin - RayOrigin) * InvRayDir;
        float3 MaxIntersect = (BoxMax - RayOrigin) * InvRayDir;
        float3 EnterIntersect = min(MinIntersect, MaxIntersect);
        float ClipBlend = max(EnterIntersect.x, max(EnterIntersect.y, EnterIntersect.z));
        ClipBlend = saturate(ClipBlend);
        return lerp(History, Filtered, ClipBlend);
    }

    float GetBlendFactor(float4 Color, inout float4 HistoryColor, float2 Depth, float2 uv, float2 HistoryUV, float Motion, float alpha)
    {
        // �� YCoCgɫ�ʿռ��н���Clip�ж�
        float3 AABBMin, AABBMax;
        AABBMax = AABBMin = RGBToYCoCg(Color);
        for (int k = 0; k < 9; k++)
        {
            float3 C = RGBToYCoCg(ColorBuffer.Sample(Samp_ColorBuffer, uv, kOffsets3x3[k]));
            AABBMin = min(AABBMin, C);
            AABBMax = max(AABBMax, C);
        }
        float3 HistoryYCoCg = RGBToYCoCg(HistoryColor);
        //����AABB��Χ�н���Clip����:
        //HistoryColor.rgb = YCoCgToRGB(ClipHistory(HistoryYCoCg, AABBMin, AABBMax));
        // Clamp����
        HistoryColor.rgb = YCoCgToRGB(clamp(HistoryYCoCg, AABBMin, AABBMax));

        //�����ٶȱ仯���ϵ��
        float scaleLength = 1000;
        float BlendFactor = saturate(alpha + length(Motion) * scaleLength);

        if (abs(Depth.y - Depth.x) > 0.05)
        //if (Depth.y != Depth.x)
        {
            BlendFactor = 1.0f;
        }
        if (HistoryUV.x < 0 || HistoryUV.y < 0 || HistoryUV.x > 1.0f || HistoryUV.y > 1.0f)
        {
            BlendFactor = 1.0f;
        }
        return BlendFactor;
    }
    float3 GetTAAColor(float2 screen_uv, float2 JitterUV, float alpha)
    {
        float2 Depth;
        float2 uv = screen_uv + JitterUV;
        float4 Color = ColorBuffer.Sample(Samp_ColorBuffer, uv);
        Color.rgb = sRGB2Linear((half3)Color.rgb);
        Depth.x = DepthBuffer.Sample(Samp_DepthBuffer, uv).r;

        float2 Motion = DecodeMotionVector(MotionBuffer.SampleLevel(Samp_MotionBuffer, uv.xy, 0).xy);
        float2 HistoryUV = uv.xy - Motion.xy;
        half4 HistoryColor = (half4)PrevColorBuffer.SampleLevel(Samp_PrevColorBuffer, HistoryUV.xy, 0);
        HistoryColor.rgb = sRGB2Linear((half3)HistoryColor.rgb);
        Depth.y = PrevDepthBuffer.Sample(Samp_PrevDepthBuffer, HistoryUV.xy).r;

        Depth = LinearFromDepth(Depth);
        //Depth.x = LinearFromDepth(Depth.x) * (gZFar - gZNear);
        //Depth.y = LinearFromDepth(Depth.y) * (gZFar - gZNear);
        //if (abs(Depth.y - Depth.x) > 0.05)
        //    //if (Depth.y != Depth.x)
        //{
        //    return float3(1,0,0);
        //}

        float blendFactor = GetBlendFactor(Color, HistoryColor, Depth, uv, HistoryUV, Motion, alpha);
        float3 result = lerp(HistoryColor.rgb, Color.rgb, blendFactor);
        result.rgb = Linear2sRGB((half3)result.rgb);
        return result;
    }
    float3 GetTAAColor2(float2 screen_uv, float2 JitterUV, float alpha)
    {
        float2 Depth;
        float2 uv = screen_uv + JitterUV;
        float4 Color = ColorBuffer.Sample(Samp_ColorBuffer, uv);
        Color.rgb = sRGB2Linear((half3)Color.rgb);
        Depth.x = DepthBuffer.Sample(Samp_DepthBuffer, uv).r;
        //��Ϊ��ͷ���ƶ��ᵼ�����屻�ڵ���ϵ�仯���ⲽ��Ŀ����ѡ�����Χ���뾵ͷ����ĵ�
        float2 closest = GetClosestUV(screen_uv.xy);

        //�õ�����Ļ�ռ��У�����֡���UVƫ�Ƶľ���
        float2 Motion = DecodeMotionVector(MotionBuffer.SampleLevel(Samp_MotionBuffer, closest.xy, 0).xy);
        float2 HistoryUV = screen_uv.xy - Motion;
        half4 HistoryColor = PrevColorBuffer.Sample(Samp_PrevColorBuffer, HistoryUV);
        HistoryColor.rgb = sRGB2Linear((half3)HistoryColor.rgb);
        Depth.y = PrevDepthBuffer.Sample(Samp_PrevDepthBuffer, HistoryUV.xy).r;

        float blendFactor = GetBlendFactor(Color, HistoryColor, Depth, uv, HistoryUV, Motion, alpha);
        float3 result = lerp(HistoryColor.rgb, Color.rgb, blendFactor);
        result.rgb = Linear2sRGB((half3)result.rgb);
        return result;
    }
};

#endif
//