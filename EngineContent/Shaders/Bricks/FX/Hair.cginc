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
    float ToH = dot(T, H);
    float sinTH = sqrt(1.0 - ToH * ToH);
    float dirAtten = smoothstep(-1.0, 0.0, dot(T, H));
    return dirAtten * pow(sinTH, exponent);
}

void GetAnisotropicRoughness(float roughness, float anisotropic, out float ax, out float az)
{
    float aspect = sqrt(1.0 - 0.9 * anisotropic);
    float roughnessSq = roughness * roughness;
    ax = roughnessSq / aspect;
    az = roughnessSq * aspect;
}

// Anisotropic GGX
// [Burley 2012, "Physically-Based Shading at Disney"]
float D_GGXaniso(float RoughnessX, float RoughnessZ, float NoH, float3 H, float3 T, float3 B)
{
    float ax = RoughnessX * RoughnessX;
    float az = RoughnessZ * RoughnessZ;
    float ToH = dot(T, H);
    float BoH = dot(B, H);
    float d = ToH * ToH / (ax * ax) + BoH * BoH / (az * az) + NoH * NoH;
    return 1 / (PI * ax * az * d * d);
}

#endif//__FX_HAIR_H__