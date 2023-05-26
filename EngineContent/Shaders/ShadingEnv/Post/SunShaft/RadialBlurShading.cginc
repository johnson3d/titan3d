#ifndef __RADIAL_BLUR_SHADING_H__
#define __RADIAL_BLUR_SHADING_H__

#include "SunShaftCommon.cginc"

#define SAMPLE_COUNT 6

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT)0;

    output.vPosition = float4(input.vPosition.xyz, 1.0f);
    output.vUV = input.vUV;

    //output.psCustomUV0.xy = (SunShaftStruct.SunPosition.xy - input.vUV.xy) * SunShaftStruct.BlurRadius4.xy;
    output.psCustomUV0.xy = (SunShaftStruct.SunPosition.xy - input.vUV.xy) / SAMPLE_COUNT;

    return output;
}

struct PS_OUTPUT
{
    float4 RT0 : SV_Target0;
};

Texture2D ColorBuffer;
SamplerState Samp_ColorBuffer;

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;

    half4 color = half4(0, 0, 0, 0);
    half illuminationDecay = 1.0f;
    for (int j = 0; j < SAMPLE_COUNT; j++)
    {
        half4 tmpColor = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
        color += tmpColor * illuminationDecay;
        uv.xy += input.psCustomUV0.xy;
        illuminationDecay *= SunShaftStruct.BlurDecay;
    }
    output.RT0 = color / (half)SAMPLE_COUNT;

    //output.RT0.rgb = length(input.psCustomUV0.xy);

    return output;
}

#endif
//