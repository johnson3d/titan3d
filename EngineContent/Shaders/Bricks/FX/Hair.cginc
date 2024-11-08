#ifndef __FX_HAIR_H__
#define __FX_HAIR_H__
#include "../../Inc/SysFunction.cginc"

float3 ShiftTangent(float3 T, float3 N, float shift)
{
    float3 shiftedT = T + (shift * N);
    return normalize(shiftedT);
}

//https://zhuanlan.zhihu.com/p/43248692
float StrandSpecular(float3 T, float3 V, float3 L, float exponent)
{
    float3 H = normalize(L + V);
    float ToH = dot(T, H);
    float sinTH = sqrt(1.0 - ToH * ToH);
    float dirAtten = smoothstep(-1.0, 0.0, dot(T, H));
    return dirAtten * pow(sinTH, exponent);
}

//https://blog.csdn.net/qjh5606/article/details/118117176
void GetAnisotropicNeubelt(float roughness, float anisotropic, out float ax, out float az)
{
    float roughnessSq = roughness * roughness;
    ax = roughnessSq;
    az = lerp(0, roughnessSq, 1 - anisotropic);
}

void GetAnisotropicBurley(float roughness, float anisotropic, out float ax, out float az)
{
    float aspect = sqrt(1.0 - 0.9 * anisotropic);
    float roughnessSq = roughness * roughness;
    ax = roughnessSq / aspect;
    az = roughnessSq * aspect;
}

void GetAnisotropicKulla(float roughness, float anisotropic, out float ax, out float az)
{
    float roughnessSq = roughness * roughness;
    ax = roughnessSq * (1 - anisotropic);
    az = roughnessSq * (1 + anisotropic);
}

float D_Beckmann_aniso(float ax, float az, float NoH, float3 H, float3 T, float3 B)
{
    float ToH = dot(T, H);
    float BoH = dot(B, H);
    float NoH_Sq = NoH * NoH;
    float d = -(ToH * ToH / (ax * ax) + BoH * BoH / (az * az)) / (NoH_Sq);
    return exp(d) / (PI * ax * az * NoH_Sq * NoH_Sq);
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