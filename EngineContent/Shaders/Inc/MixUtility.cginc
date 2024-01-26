#ifndef _MixUtility_cginc_
#define _MixUtility_cginc_

#include "VertexLayout.cginc"

half3 MultiPbrPointLightMobile(PS_INPUT input, float3 WorldPos, half3 V, half3 N, half3 OptDiffShading, half3 OptSpecShading, half Roughness)
{
	half3 BaseShading = (half3)0;
    for (int idx_p = 0; idx_p < PointLightNum; idx_p++)
    {
        PbrPointLight light;
        light.PointLightPos_RadiusInv = PointLightPos_RadiusInv[PointLightIndices[idx_p]];
        light.PointLightColor_Intensity = PointLightColor_Intensity[PointLightIndices[idx_p]];
        BaseShading += PbrPointLightMobile(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
    }
	return BaseShading;
}


#endif //_MixUtility_cginc_