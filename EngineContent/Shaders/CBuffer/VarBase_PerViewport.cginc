#ifndef __VARBASE_PERVIEWPORT_SHADERINC__
#define __VARBASE_PERVIEWPORT_SHADERINC__
#include "../Inc/GlobalDefine.cginc"

#include "../Inc/LightCommon.cginc"
#include "../Inc/Math.cginc"

VK_BIND(1) cbuffer cbPerViewport DX_BIND_B(1)
{	
    FDirLight DirLight;
	
	float2 ViewportPos; // leftup position
    int CsmNum;
    float FogStart; // = 30;
    
	float FogHorizontalRange;// = 170;
	float FogCeil;// = 70;
	float FogVerticalRange;// = 150;
	float FogDensity;// = 1.57;

	float4 gViewportSizeAndRcp;
	
	float2 gDepthBiasAndZFarRcp;
	float2 gFadeParam;

	float4 gShadowMapSizeAndRcp;

	matrix gViewer2ShadowMtx[1];

	matrix gViewer2ShadowMtxArray[4];
	
	float4 gCsmDistanceArray;

	float4 gShadowTransitionScaleArray;
	
	float4 gSunPosNDC;
	float4 gAoParam;//radius_platform_bias_dark;

	float gShadowTransitionScale;
	float gShadowDistance;
	float gEnvMapMaxMipLevel;
	float gEyeEnvMapMaxMipLevel;
};

#endif