#ifndef __LUMINANCE_THRESHOLE_SHADING_H__
#define __LUMINANCE_THRESHOLE_SHADING_H__

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

struct FLuminanceThresholeStruct
{
    float Threshole;
};

cbuffer cbShadingEnv DX_AUTOBIND
{
    FLuminanceThresholeStruct LuminanceThresholeStruct;
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;

    half4 color = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
    half lum = CalcLuminance(color.rgb);
    if (lum > LuminanceThresholeStruct.Threshole)
    {
#if defined(ENV_OUT_COLOR)
        output.RT0 = color;
#elif defined(ENV_OUT_LUMINANCE)
        output.RT0.rgb = lum;
#endif
    }
    else
    {
        output.RT0 = float4(0, 0, 0, 1);
    }
    return output;
}

#endif
//