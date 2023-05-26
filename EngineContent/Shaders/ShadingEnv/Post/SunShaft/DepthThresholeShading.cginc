#ifndef __DEPTH_THRESHOLE_SHADING_H__
#define __DEPTH_THRESHOLE_SHADING_H__

#include "SunShaftCommon.cginc"


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

half TransformColor(half4 skyboxValue) {
    return dot(max(skyboxValue.rgb - SunShaftStruct.LumThreshold, half3(0, 0, 0)), half3(1, 1, 1)); // threshold and convert to greyscale
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;

    half4 color = (half4)ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
    float depth = DepthBuffer.SampleLevel(Samp_DepthBuffer, uv, 0).r;
    float linearDepth = NormalizedLinearFromDepth(depth);
    half2 vec = (half2)SunShaftStruct.SunPosition.xy - (half2)input.vUV.xy;
    half dist = saturate(SunShaftStruct.SunPosition.w - length(vec.xy));

    if (linearDepth >= SunShaftStruct.DepthThreshole)
    {
        float lum = CalcLuminance(color.rgb);
        output.RT0.rgb = V_Select(lum > SunShaftStruct.LumThreshold, lum, 0);
        output.RT0.rgb *= dist;
        output.RT0.a = 1;
        //output.RT0 = TransformColor(color)* dist;
    }
    else
    {
        output.RT0 = float4(0, 0, 0, 1);
    }

    return output;
}

#endif
//