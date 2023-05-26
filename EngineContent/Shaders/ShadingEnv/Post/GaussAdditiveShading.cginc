#ifndef __GAUSS_ADDITIVE_SHADING_H__
#define __GAUSS_ADDITIVE_SHADING_H__

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

struct FGaussStruct
{
    float2 StrideUV1;
    float2 StrideUV2;

    float Stride1;
    float Stride2;
    int BlurSize;
    float BlurSigma;
};

cbuffer cbShadingEnv DX_AUTOBIND
{
    FGaussStruct GaussStruct;
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;
    
    float3 clr1 = GaussNxN(Color1Buffer, Samp_Color1Buffer, uv, GaussStruct.BlurSize, GaussStruct.StrideUV1, GaussStruct.BlurSigma);
    float3 clr2 = GaussNxN(Color2Buffer, Samp_Color2Buffer, uv, GaussStruct.BlurSize, GaussStruct.StrideUV2, GaussStruct.BlurSigma);
    output.RT0.rgb = clr1 + clr2;
    output.RT0.a = 1.0f;

    return output;
}

#endif
//