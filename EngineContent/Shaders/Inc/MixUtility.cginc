#ifndef _MixUtility_cginc_
#define _MixUtility_cginc_

#include "VertexLayout.cginc"

half3 MultiPbrPointLightMobile(PS_INPUT input, float3 WorldPos, half3 V, half3 N, half3 OptDiffShading, half3 OptSpecShading, half Roughness)
{
	half3 BaseShading = (half3)0;
#if defined(MDF_INSTANCING)
#if ShaderModel >= 4
	uint instanceId = input.SpecialData.x;
	VSInstantData InstData = VSInstantDataArray[instanceId];
	for (int i = 0; i < 4; i++)
	{
		uint lightIndex = InstData.PointLightIndices[i];
		if (lightIndex == 0xFFFFFFFF)
			continue;
		PbrPointLight light;
		light.PointLightPos_RadiusInv = PointLightPos_RadiusInv[lightIndex];
		light.PointLightColor_Intensity = PointLightColor_Intensity[lightIndex];
		BaseShading += PbrPointLightMobile(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
	}
#else
	PbrPointLight light;
	if (input.SpecialData.x > 0)
	{
		uint realIndex = input.PointLightIndices.x;
		light.PointLightPos_RadiusInv = PointLightPos_RadiusInv[realIndex];
		light.PointLightColor_Intensity = PointLightColor_Intensity[realIndex];
		BaseShading += PbrPointLightMobile(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
	}
	if (input.SpecialData.x > 1)
	{
		uint realIndex = input.PointLightIndices.y;
		light.PointLightPos_RadiusInv = PointLightPos_RadiusInv[realIndex];
		light.PointLightColor_Intensity = PointLightColor_Intensity[realIndex];
		BaseShading += PbrPointLightMobile(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
	}
	if (input.SpecialData.x > 2)
	{
		uint realIndex = input.PointLightIndices.z;
		light.PointLightPos_RadiusInv = PointLightPos_RadiusInv[realIndex];
		light.PointLightColor_Intensity = PointLightColor_Intensity[realIndex];
		BaseShading += PbrPointLightMobile(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
	}
	if (input.SpecialData.x > 3)
	{
		uint realIndex = input.PointLightIndices.w;
		light.PointLightPos_RadiusInv = PointLightPos_RadiusInv[realIndex];
		light.PointLightColor_Intensity = PointLightColor_Intensity[realIndex];
		BaseShading += PbrPointLightMobile(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
	}
#endif
#else
	for (int idx_p = 0; idx_p < PointLightNum; idx_p++)
	{
		PbrPointLight light;
		light.PointLightPos_RadiusInv = PointLightPos_RadiusInv[PointLightIndices[idx_p]];
		light.PointLightColor_Intensity = PointLightColor_Intensity[PointLightIndices[idx_p]];
		BaseShading += PbrPointLightMobile(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
	}
#endif
	return BaseShading;
}


#endif //_MixUtility_cginc_