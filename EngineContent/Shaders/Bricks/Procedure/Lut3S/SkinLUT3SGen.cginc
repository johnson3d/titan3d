#ifndef	_PGC_SKINLUT3SGEN_INCWATER_H_
#define _PGC_SKINLUT3SGEN_INCWATER_H_
#include "../../../Inc/GlobalDefine.cginc"
//https://blog.csdn.net/wolf96/article/details/41925731

RWTexture2D<float4> _SSSLUT;
float _Resoultion;
float _IntegralInterval;

float3 Gaussian(float v,float r)
{
    return 1.0/sqrt(2.0 * PI * v) * exp(-(r*r)/(2.0*v));
}

#define A 0.15
#define B 0.50
#define C 0.10
#define D 0.20
#define E 0.02
#define F 0.30
#define W 11.2

float3 Tonemap(float3 x)
{
    return ((x * ( A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E/F;
}

float3 DiffusionProfile(float r)
{
    return float3(0.0, 0.0, 0.0)
        + Gaussian(0.0064, r) * float3(0.233, 0.455, 0.649)
        + Gaussian(0.0484, r) * float3(0.100, 0.336, 0.344)
        + Gaussian(0.187, r) * float3(0.118, 0.198, 0.0)
        + Gaussian(0.567, r) * float3(0.113, 0.007, 0.007)
        + Gaussian(1.99, r) * float3(0.358, 0.004, 0.0)
        + Gaussian(7.41, r) * float3(0.233, 0.0, 0.0);
}

float3 BakeSkinLUT(float2 uv)
{
    //ring integrate
    float NoL = uv.x;
    float INV_R = uv.y;

    float theta = acos(NoL * 2.0 - 1.0);
    float R = 1.0/INV_R;

    float3 scatteringFactor = float3(0.0, 0.0, 0.0);
    float3 normalizationFactor = float3(0.0, 0.0, 0.0);

    for(float x = -PI * _IntegralInterval /2 ;x < PI * _IntegralInterval /2 ;x+=PI*0.001)
    {
        float dis = 2.0 * R * sin(x/2);
        scatteringFactor += saturate(cos(x + theta)) * DiffusionProfile(dis);
        normalizationFactor += DiffusionProfile(dis);
    }

    float3 result = scatteringFactor/normalizationFactor;

#ifdef USE_TONEMAP
    float3 tonedResult = Tonemap(result * 12.0);
    float3 whiteScale = 1.0 / Tonemap(float3(W, W, W));
    tonedResult = tonedResult * whiteScale;
    return tonedResult;
#endif
    return result;
}

[numthreads(DispatchX, DispatchY, DispatchZ)]
void CSMain (uint3 id : SV_DispatchThreadID)
{
    // TODO: insert actual code here!
    _SSSLUT[id.xy] = float4(BakeSkinLUT(id.xy / _Resoultion).rgb, 1);
}

#endif//_PGC_SKINLUT3SGEN_INCWATER_H_