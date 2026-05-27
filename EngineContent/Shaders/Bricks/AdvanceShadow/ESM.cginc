#ifndef _ESM_cginc_
#define _ESM_cginc_
#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/PostEffectCommon.cginc"
#include "AdvanceShadow.cginc"

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


Texture2D DepthBuffer DX_AUTOBIND;
SamplerState Samp_DepthBuffer DX_AUTOBIND;

float ESM_GaussNxN(Texture2D Tex, SamplerState Sampler, float2 uv, int n, float2 stride, float sigma)
{
    float color = 0;
    int r = n / 2;
    float weight = 0.0;

    for (int i = -r; i <= r; i++)
    {
        for (int j = -r; j <= r; j++)
        {
            float w = GaussWeight2D(i, j, sigma);
            float2 coord = uv + float2(i, j) * stride;
            
            float depth = Tex.SampleLevel(Sampler, coord, 0).r;
            color += GetESMValue(depth, ZNear, ZFar) * w;
            weight += w;
        }
    }

    color /= weight;
    return color;
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT)0;

    float2 uv = input.vUV;

#if DISABLE_ESM == 1
    // Direct depth copy (traditional depth comparison mode)
    output.RT0.r = DepthBuffer.SampleLevel(Samp_DepthBuffer, uv, 0).r;
#else
    // ESM: Gauss-filtered exponential depth
    output.RT0.r = ESM_GaussNxN(DepthBuffer, Samp_DepthBuffer, uv, 5, float2(1 / 128.0f, 1 / 128.0f), GaussSigma);
#endif

    return output;
}

#endif//_ESM_cginc_