#ifndef __ADDITIVE_SHADING_H__
#define __ADDITIVE_SHADING_H__

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/PostEffectCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

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

Texture2D Color1Buffer;
SamplerState Samp_Color1Buffer;

Texture2D Color2Buffer;
SamplerState Samp_Color2Buffer;

struct FAdditiveStruct
{
    float Factor1;
    float Factor2;
};

cbuffer cbShadingEnv DX_AUTOBIND
{
    FAdditiveStruct AdditiveStruct;
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;

    half4 color1 = (half4)Color1Buffer.SampleLevel(Samp_Color1Buffer, uv, 0);
#if defined(ENV_ADD_COLOR)
    half4 color2 = (half4)Color2Buffer.SampleLevel(Samp_Color2Buffer, uv, 0);
    output.RT0 = color1 * AdditiveStruct.Factor1 + color2 * AdditiveStruct.Factor2;
#elif defined(ENV_ADD_LUMINANCE)
    half color2 = (half)Color2Buffer.SampleLevel(Samp_Color2Buffer, uv, 0).r;
    output.RT0 = color1 * AdditiveStruct.Factor1 + color2 * AdditiveStruct.Factor2;
#endif
    return output;
}

#endif
//