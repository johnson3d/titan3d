#ifndef __AA_SHADING_H__
#define __AA_SHADING_H__

#define FXAA_GREEN_AS_LUMA		1
#define FXAA_QUALITY__PRESET		10
#define FXAA_HLSL_4 1

#include "../Inc/VertexLayout.cginc"
#include "../Inc/GpuSceneCommon.cginc"
#include "../Inc/LightCommon.cginc"
#include "../Inc/PostEffectCommon.cginc"
#include "../Inc/FXAAMobile.cginc"
#include "TemporalAA.cginc"

#include "Material"
#include "MdfQueue"

#include "../Inc/SysFunctionDefImpl.cginc"

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT)0;

    output.vPosition = float4(input.vPosition.xyz, 1.0f);
    output.vUV = input.vUV;
    output.Set_vSpecialDataX(input1.vVertexID);

    return output;
}

struct PS_OUTPUT
{
    float4 RT0 : SV_Target0;
};

Texture2D ColorBuffer;
SamplerState Samp_ColorBuffer;
Texture2D DepthBuffer;
SamplerState Samp_DepthBuffer;
Texture2D MotionBuffer;
SamplerState Samp_MotionBuffer;

Texture2D PrevColorBuffer;
SamplerState Samp_PrevColorBuffer;
Texture2D PrevDepthBuffer;
SamplerState Samp_PrevDepthBuffer;

cbuffer cbShadingEnv DX_AUTOBIND
{
    // jitter 已统一由 cbPerCamera.JitterOffset / PreJitterOffset 提供 (UV 单位),
    // 这里不再单独写一份, 避免两路写入约定不一致 (历史 bug 见 git log).
    float TaaBlendAlpha;
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;
#if ENV_TypeAA == ETypeAA_None//none
    half4 rt0 = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
#elif ENV_TypeAA == ETypeAA_Fsaa//fsaa
    FxaaTex TempTex;
    TempTex.smpl = Samp_ColorBuffer;
    TempTex.tex = ColorBuffer;
    half4 rt0 = FxaaMobilePS(
        uv.xy,																//FxaaFloat2 pos,
        TempTex,																//FxaaTex tex,
        ViewportSizeAndRcp.zw,															//FxaaFloat2 fxaaQualityRcpFrame,
        1.0,																			//highest value,FxaaFloat fxaaQualitySubpix,
        0.166,																		//default value,FxaaFloat fxaaQualityEdgeThreshold,
        0.0833																		//default value,FxaaFloat fxaaQualityEdgeThresholdMin,
    );
#elif ENV_TypeAA == ETypeAA_Taa//taa
    half4 rt0;
    TAA taa;
    taa.ColorBuffer = ColorBuffer;
    taa.Samp_ColorBuffer = Samp_ColorBuffer;

    taa.PrevColorBuffer = PrevColorBuffer;
    taa.Samp_PrevColorBuffer = Samp_PrevColorBuffer;

    taa.DepthBuffer = DepthBuffer;
    taa.Samp_DepthBuffer = Samp_DepthBuffer;

    taa.PrevDepthBuffer = PrevDepthBuffer;
    taa.Samp_PrevDepthBuffer = Samp_PrevDepthBuffer;

    taa.MotionBuffer = MotionBuffer;
    taa.Samp_MotionBuffer = Samp_MotionBuffer;

    // JitterOffset / PreJitterOffset 来自 cbPerCamera (与 GBuffer VS 注入 SV_Position 的 jitter 同源同方向).
    // TAA 在 PS 端用 currUV = screen_uv - JitterOffset 反偏当前帧回像素中心采样,
    // 同时 HistoryUV 也要减去 PreJitterOffset 反偏上一帧的 jitter, 否则静止场景会抖动.
    if (true)
    {
        rt0.rgb = taa.GetTAAColor(input.vUV.xy, JitterOffset, PreJitterOffset, TaaBlendAlpha);
    }
    else
    {
        rt0.rgb = taa.GetTAAColor2(input.vUV.xy, JitterOffset, PreJitterOffset, TaaBlendAlpha);
    }
#else
    half4 rt0 = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
#endif

    output.RT0 = rt0;

    return output;
}

#endif
//