#ifndef	 _SHADING_COMMON_H_
#define _SHADING_COMMON_H_

float DielectricSpecularToF0(float Specular)
{
    return 0.08f * Specular;
}

float3 ComputeF0(float Specular, float3 BaseColor, float Metallic)
{
    return lerp(DielectricSpecularToF0(Specular).xxx, BaseColor, Metallic.xxx);
}

#endif 