#ifndef _SSS_BLUR_H_
#define _SSS_BLUR_H_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/SysFunctionDefImpl.cginc"
#include "DeferredCommon.cginc"

#include "MdfQueue"

Texture2D ColorBuffer DX_AUTOBIND;
SamplerState Samp_ColorBuffer DX_AUTOBIND;

Texture2D SpecularBuffer DX_AUTOBIND;
SamplerState Samp_SpecularBuffer DX_AUTOBIND;

Texture2D DepthBuffer DX_AUTOBIND;
SamplerState Samp_DepthBuffer DX_AUTOBIND;

Texture2D GBufferRT0 DX_AUTOBIND;
SamplerState Samp_GBufferRT0 DX_AUTOBIND;

Texture2D GBufferRT3 DX_AUTOBIND;
SamplerState Samp_GBufferRT3 DX_AUTOBIND;

struct FSubsurfaceProfile
{
    float3 ScatterColor;
    float ScatterRadius;
    float3 FalloffColor;
    float SubsurfaceOpacity;
};
StructuredBuffer<FSubsurfaceProfile> SubsurfaceProfiles DX_AUTOBIND;

cbuffer cbSSSBlur DX_AUTOBIND
{
    float2 BlurDirection; // (1/w, 0) for horizontal, (0, 1/h) for vertical
    float SSSWidth;       // global baseline blur width in pixels
    int SSSPassIndex;     // 0=horizontal blur, 1=vertical blur + compose
};

// Separable SSS kernel — 7 samples, tight offsets for subtle skin scatter.
// Actual pixel stride = offset * effectiveWidth * BlurDirection.
static const int SSS_NUM_SAMPLES = 7;
static const float SSS_Offsets[SSS_NUM_SAMPLES] = {
    -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0
};
static const float SSS_GaussWeights[SSS_NUM_SAMPLES] = {
    0.006, 0.061, 0.242, 0.382, 0.242, 0.061, 0.006
};

bool IsSSSPixel(float2 uv)
{
    half4 rt3 = (half4)GBufferRT3.SampleLevel(Samp_GBufferRT3, uv, 0);
    int renderFlags = (int)(rt3.b * 1023.0h);
    int shadingMode = (renderFlags & SHADINGMODE_BIT_MASK) >> SHADINGMODE_BIT_OFFSET;
    return shadingMode == EShadingMode_Subsurface;
}

// Read per-pixel Opacity mask from GBufferRT3.a (2-bit quantized: 0, 0.33, 0.67, 1.0)
// Used to exclude fine features (nails, eyebrows) from SSS blur.
half GetSSSOpacityMask(float2 uv)
{
    half4 rt3 = (half4)GBufferRT3.SampleLevel(Samp_GBufferRT3, uv, 0);
    return rt3.a;
}

int GetProfileIndex(float2 uv)
{
    half4 rt0 = (half4)GBufferRT0.SampleLevel(Samp_GBufferRT0, uv, 0);
    return (int)(rt0.a * 255.0h + 0.5h);
}

float GetLinearDepth(float2 uv)
{
    float rawDepth = DepthBuffer.SampleLevel(Samp_DepthBuffer, uv, 0).r;
    return LinearFromDepth(rawDepth);
}

// Per-channel Burley diffusion weight based on FalloffColor.
// FalloffColor controls how fast each RGB channel decays with distance:
//   - higher falloff → slower decay → wider scatter (red for skin)
//   - lower  falloff → faster decay → narrower scatter (blue for skin)
half3 BurleyFalloff(float3 falloffColor, float distSq)
{
    // Approximate sum-of-Gaussians: exp(-dist^2 / (2 * falloff^2))
    // falloffColor acts as per-channel variance scale
    half3 falloff2 = (half3)(falloffColor * falloffColor);
    half3 exponent = (half3)(-distSq) / max(2.0h * falloff2, 0.001h);
    return (half3)exp(exponent);
}

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT)0;

    output.vPosition = float4(input.vPosition.xyz, 1.0f);
    output.vUV = input.vUV;
#if RHI_TYPE == RHI_GL
    output.vUV.y = 1 - input.vUV.y;
#endif
    return output;
}

struct PS_OUTPUT
{
    float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV.xy;
    half4 centerColor = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);

    if (!IsSSSPixel(uv))
    {
        // Non-SSS pixel: pass through, compose specular on final pass
        if (SSSPassIndex == 1)
        {
            half4 spec = (half4)SpecularBuffer.SampleLevel(Samp_SpecularBuffer, uv, 0);
            output.RT0.rgb = centerColor.rgb + spec.rgb;
        }
        else
        {
            output.RT0.rgb = centerColor.rgb;
        }
        output.RT0.a = centerColor.a;
        return output;
    }

    // Lookup profile for this pixel
    int profileIdx = GetProfileIndex(uv);
    FSubsurfaceProfile profile = SubsurfaceProfiles[profileIdx];

    // Opacity mask: controls how much this pixel participates in blur.
    // Opacity=0 means no blur (nails, eyebrows), Opacity=1 means full blur (skin).
    half centerOpacity = GetSSSOpacityMask(uv);

    // Effective blur radius in pixels:
    //   SSSWidth       = global baseline (pixels, default 3.0)
    //   ScatterRadius  = per-profile scale (0.3~1.0)
    //   BlurDirection  = (1/w, 0) or (0, 1/h), one pixel step in UV space
    //   Max offset = 3 * effectiveWidth pixels (from SSS_Offsets)
    float centerDepth = GetLinearDepth(uv);
    float effectiveWidth = profile.ScatterRadius * SSSWidth * centerOpacity;

    // Depth threshold for edge rejection: 1% of center depth (matches UE5 convention).
    // Preserves geometric edges (nose bridge, chin, eye sockets) while allowing
    // smooth scatter across gentle curvature.
    float depthThreshold = centerDepth * 0.01;

    half3 blurResult = half3(0, 0, 0);
    half3 totalWeight = half3(0, 0, 0);

    for (int i = 0; i < SSS_NUM_SAMPLES; i++)
    {
        float2 sampleUV = uv + BlurDirection * SSS_Offsets[i] * effectiveWidth;
        half gaussW = (half)SSS_GaussWeights[i];

        // Skip non-SSS pixels to prevent bleeding from hair/armor/background
        if (!IsSSSPixel(sampleUV))
        {
            // Fall back to center color with base weight
            half3 channelW = gaussW;
            blurResult += centerColor.rgb * channelW;
            totalWeight += channelW;
            continue;
        }

        // Depth-aware weight: reject samples across geometric edges
        float sampleDepth = GetLinearDepth(sampleUV);
        float depthDiff = abs(centerDepth - sampleDepth);
        half depthWeight = (half)saturate(1.0 - depthDiff / depthThreshold);

        // Neighbor opacity mask: low-opacity neighbors contribute less scatter
        half sampleOpacity = GetSSSOpacityMask(sampleUV);

        // Per-channel diffusion weight using FalloffColor
        // Normalized distance² in kernel space for Burley falloff
        float normalizedDist = SSS_Offsets[i] / max((float)SSS_NUM_SAMPLES * 0.5, 1.0);
        float distSq = normalizedDist * normalizedDist;
        half3 channelFalloff = BurleyFalloff(profile.FalloffColor, distSq);

        // Combined per-channel weight = gauss * depth * falloff * neighbor opacity
        half3 channelWeight = gaussW * depthWeight * channelFalloff * sampleOpacity;

        half3 sampleColor = (half3)ColorBuffer.SampleLevel(Samp_ColorBuffer, sampleUV, 0).rgb;
        blurResult += sampleColor * channelWeight;
        totalWeight += channelWeight;
    }

    blurResult /= max(totalWeight, 0.001h);

    // Lerp between original color and blurred result based on center Opacity.
    // Opacity=0 (nails/eyebrows): keep original, Opacity=1 (skin): fully blurred.
    blurResult = lerp(centerColor.rgb, blurResult, centerOpacity);

    if (SSSPassIndex == 1)
    {
        // Final pass: compose blurred diffuse + un-blurred separated specular
        half4 spec = (half4)SpecularBuffer.SampleLevel(Samp_SpecularBuffer, uv, 0);
        output.RT0.rgb = blurResult + spec.rgb;
    }
    else
    {
        output.RT0.rgb = blurResult;
    }
    output.RT0.a = centerColor.a;

    return output;
}

#endif
