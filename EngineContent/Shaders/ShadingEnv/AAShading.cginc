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
    float2 JitterUV;
    float TaaBlendAlpha;
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;
#if ENV_TypeAA == ETypeAA_None//none
    half4 rt0 = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
#elif ENV_TypeAA == TETypeAA_Fsaa//fsaa
    FxaaTex TempTex;
    TempTex.smpl = Samp_ColorBuffer;
    TempTex.tex = ColorBuffer;
    half4 rt0 = FxaaMobilePS(
        uv.xy,																//FxaaFloat2 pos,
        TempTex,																//FxaaTex tex,
        gViewportSizeAndRcp.zw,															//FxaaFloat2 fxaaQualityRcpFrame,
        1.0,																			//highest value,FxaaFloat fxaaQualitySubpix,
        0.166,																		//default value,FxaaFloat fxaaQualityEdgeThreshold,
        0.0833																		//default value,FxaaFloat fxaaQualityEdgeThresholdMin,
    );
#elif ENV_TypeAA == TETypeAA_Taa//taa
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

    if (true)
    {
        /*float2 motionVector = GBufferData::DecodeMotionVector(rt3.rg);
        float2 prev_uv = uv.xy - motionVector.xy;

        half4 prev_rt0 = (half4)PrevRT0.SampleLevel(Samp_PrevRT0, prev_uv.xy, 0);
        rt0.rgb = lerp(prev_rt0.rgb, rt0.rgb, TaaBlendAlpha);*/
        rt0.rgb = taa.GetTAAColor(input.vUV.xy, JitterUV, TaaBlendAlpha);
    }
    else
    {
        rt0.rgb = taa.GetTAAColor2(input.vUV.xy, JitterUV, TaaBlendAlpha);
    }
#else
    half4 rt0 = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
#endif

    output.RT0 = rt0;

    return output;
}

#endif
//