#ifndef __VARBASE_PERVIEWPORT_SHADERINC__
#define __VARBASE_PERVIEWPORT_SHADERINC__
#include "../Inc/GlobalDefine.cginc"

#include "../Inc/LightCommon.cginc"
#include "../Inc/Math.cginc"

cbuffer cbPerViewport DX_BIND_B(1)
{	
    FDirLight DirLight;
	
	float2 ViewportPos; // leftup position
    int CsmNum;
    float FogStart; // = 30;
    
	float FogHorizontalRange;// = 170;
	float FogCeil;// = 70;
	float FogVerticalRange;// = 150;
	float FogDensity;// = 1.57;

	float4 ViewportSizeAndRcp;
	
	float2 DepthBiasAndZFarRcp;
	float2 FadeParam;

	float4 ShadowMapSizeAndRcp;

	matrix Viewer2ShadowMtx[1];

	matrix Viewer2ShadowMtxArray[4];
	
	float4 CsmDistanceArray;

	float4 ShadowTransitionScaleArray;
	
	float4 SunPosNDC;
	float4 AoParam;//radius_platform_bias_dark;

	float ShadowTransitionScale;
	float ShadowDistance;
	float EnvMapMaxMipLevel;
	float EyeEnvMapMaxMipLevel;
};

#endif