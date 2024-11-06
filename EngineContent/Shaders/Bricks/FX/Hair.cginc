#ifndef __FX_HAIR_H__
#define __FX_HAIR_H__
#include "../../Inc/SysFunction.cginc"

//https://blog.csdn.net/qjh5606/article/details/118117176

float3 ShiftTangent(float3 T, float3 N, float shift)
{
    float3 shiftedT = T + (shift * N);
    return normalize(shiftedT);
}

float StrandSpecular(float3 T, float3 V, float3 L, float exponent)
{
    float3 H = normalize(L + V);
    float dotTH = dot(T, H);
    float sinTH = sqrt(1.0 - dotTH * dotTH);
    float dirAtten = smoothstep(-1.0, 0.0, dot(T, H));
    return dirAtten * pow(sinTH, exponent);
}

#endif//__FX_HAIR_H__