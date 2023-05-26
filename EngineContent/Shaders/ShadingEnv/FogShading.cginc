#ifndef __FOG_SHADING_H__
#define __FOG_SHADING_H__

#include "../Inc/VertexLayout.cginc"
#include "../Inc/PostEffectCommon.cginc"
#include "ExpHeightFog.cginc"

#include "Material"
#include "MdfQueue"

#include "../Inc/SysFunctionDefImpl.cginc"

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT)0;

    output.vPosition = float4(input.vPosition.xyz, 1.0f);
    output.vUV = input.vUV;

    output.psCustomUV0.xyz = CornerRays[input.vVertexID];

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

Texture2D NoiseBuffer;
SamplerState Samp_NoiseBuffer;

cbuffer cbShadingEnv DX_AUTOBIND
{
    FFogStruct FogStruct;
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;

    float4 color = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
    
#if ENV_FOGFACTOR_TYPE == TypeFog_None
    output.RT0 = color;
#elif ENV_FOGFACTOR_TYPE == TypeFog_ExpHeight
    float depth = DepthBuffer.Sample(Samp_DepthBuffer, uv).r;
    output.RT0.rgb = FogStruct.GetExpHeightFogColor(GetWorldPositionFromDepthValue(uv, depth, false), color.rgb, LinearFromDepth(depth), input.psCustomUV0.xyz, uv);
    output.RT0.a = color.a;
#endif
    
    return output;
}

#endif
//