#ifndef __GAUSS_SHADING_H__
#define __GAUSS_SHADING_H__

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/PostEffectCommon.cginc"

#include "../../Inc/SysFunctionDefImpl.cginc"

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT)0;

    output.vPosition = float4(input.vPosition.xyz, 1.0f);
    output.vUV = input.vUV;

    return output;
}

struct PS_OUTPUT
{
    float4 RT0 : SV_Target0;
    float4 RT1 : SV_Target1;
    float4 RT2 : SV_Target2;
    float4 RT3 : SV_Target3;
};

Texture2D		QuarkTexture;
SamplerState	Samp_QuarkTexture;

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;
    float4 color = float4(QuarkTexture.SampleLevel(Samp_QuarkTexture, uv.xy, 0).xyz, 1);//float4(0, 0, 0, 1);
    
    // if (uv.x < 0.2 && uv.y < 0.2)
    //    color = float4(1, 0, 0, 1);
    
    output.RT0 = color;

    return output;
}

#endif