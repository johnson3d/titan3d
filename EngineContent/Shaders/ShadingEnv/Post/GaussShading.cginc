#ifndef __GAUSS_SHADING_H__
#define __GAUSS_SHADING_H__

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

Texture2D ColorBuffer;
SamplerState Samp_ColorBuffer;

struct FGaussStruct
{
    float2 StrideUV;
    float Stride;
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

    float4 color = float4(0, 0, 0, 1);
    float2 uv = input.vUV;
    
    color.rgb = GaussNxN(ColorBuffer, Samp_ColorBuffer, uv, GaussStruct.BlurSize, GaussStruct.StrideUV, GaussStruct.BlurSigma);
    output.RT0 = color;

    return output;
}

#endif
//