#ifndef _MOBILE_COPY_
#define _MOBILE_COPY_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"
#include "../../Inc/ShadowCommon.cginc"
#include "../../Inc/FogCommon.cginc"
#include "../../Inc/MixUtility.cginc"
#include "../../Inc/SysFunction.cginc"
#include "../../Inc/PostEffectCommon.cginc"
#include "../../Inc/GpuSceneCommon.cginc"

#include "MdfQueue"

#define FXAA_GREEN_AS_LUMA		1
#define FXAA_QUALITY__PRESET		10
#define FXAA_HLSL_4 1

#include "DeferredCommon.cginc"
#include "../../Inc/FXAAMobile.cginc"

#include "../../Inc/SysFunctionDefImpl.cginc"

Texture2D DepthBuffer DX_AUTOBIND;
SamplerState Samp_DepthBuffer DX_AUTOBIND;

Texture2D GBufferRT0 DX_AUTOBIND;
SamplerState Samp_GBufferRT0 DX_AUTOBIND;

Texture2D GBufferRT1 DX_AUTOBIND;
SamplerState Samp_GBufferRT1 DX_AUTOBIND;

Texture2D GBufferRT2 DX_AUTOBIND;
SamplerState Samp_GBufferRT2 DX_AUTOBIND;

Texture2D GBufferRT3 DX_AUTOBIND;
SamplerState Samp_GBufferRT3 DX_AUTOBIND;

Texture2D GShadowMap DX_AUTOBIND;
SamplerState Samp_GShadowMap DX_AUTOBIND;

Texture2D gEnvMap DX_AUTOBIND;
SamplerState Samp_gEnvMap DX_AUTOBIND;

Texture2D GVignette DX_AUTOBIND;
SamplerState Samp_GVignette DX_AUTOBIND;

StructuredBuffer<FTileData> TilingBuffer DX_AUTOBIND;

float GetDepth(float2 uv)
{
	return DepthBuffer.SampleLevel(Samp_DepthBuffer, uv, 0).r;
}

float4	GetWorldPosition(float4 PosProj, float vDepth)
{
	// Position
	float4 VPos = PosProj;
	//VPos.xy /= VPos.ww;
	VPos.z = vDepth;
	//VPos.w = 1.0f;
	// Inverse ViewProjection Matrix
	VPos = mul(VPos, GetViewPrjMtxInverse(true));
	VPos.xyzw /= VPos.w;
	return VPos;
}

PS_INPUT VS_Main(VS_INPUT input1)
{
	VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
	PS_INPUT output = (PS_INPUT)0;

	output.vPosition = float4(input.vPosition.xyz, 1.0f);
	output.vUV = input.vUV;
#if RHI_TYPE == RHI_GL
	output.vUV.y = 1 - input.vUV.y;
#endif
	output.vLightMap.xy = gSunPosNDC.xy - input.vPosition.xy;
	output.vLightMap.z = gSunPosNDC.z;
	output.vLightMap.w = gSunPosNDC.w;
	output.vLightMap.xy = CalcVignetteVS((half2)output.vPosition.xy);

	//output.SpecialData.x = input1.vVertexID;
	return output;
}

half GetRoughness(half InRoughness, float3 WorldNormal)
{
	half Roughness = InRoughness;

#if 1 // Speculaer AA
	float roughness2 = Roughness * Roughness;
	float3 dndu = ddx(WorldNormal), dndv = ddy(WorldNormal);
	float variance = 0.2 * (dot(dndu, dndu) + dot(dndv, dndv));
	float kernelRoughness2 = min(2.0 * variance, 0.18);
	float filteredRoughness2 = saturate(roughness2 + kernelRoughness2);
	Roughness = sqrt(filteredRoughness2);
#endif
	return Roughness;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	float2 uv = input.vUV.xy;

	half4 rt0 = (half4)GBufferRT0.SampleLevel(Samp_GBufferRT0, uv.xy, 0);
	half4 rt1 = (half4)GBufferRT1.SampleLevel(Samp_GBufferRT1, uv.xy, 0);
	half4 rt2 = (half4)GBufferRT2.SampleLevel(Samp_GBufferRT2, uv.xy, 0);
	half4 rt3 = (half4)GBufferRT3.SampleLevel(Samp_GBufferRT3, uv.xy, 0);
	float rtDepth = GetDepth(uv.xy);

	GBufferData GBuffer = (GBufferData)0;
	GBuffer.DecodeGBuffer(rt0, rt1, rt2, rt3);

	if (GBuffer.IsUnlit())
	{
		output.RT0.rgb = GBuffer.MtlColorRaw;
		return output;
	}

	bool NoPixel = (all(GBuffer.WorldNormal) == 0);
	
	half3 Albedo = sRGB2Linear((half3)GBuffer.MtlColorRaw);
	half AbsSpecular = GBuffer.Specular;

	half3 N = GBuffer.WorldNormal;
	half Metallic = (half)GBuffer.Metallicity;
	half Smoothness = (half)GBuffer.Roughness;
	half Roughness = GetRoughness(1.0h - Smoothness, GBuffer.WorldNormal);
	half AOs = 0;
	half AoOffsetEncoded = 0.0h;

	half3 BaseShading = half3(0.0h, 0.0h, 0.0h);

	float3 WorldPos = GetWorldPositionFromDepthValue(uv, rtDepth).xyz;//GetWorldPosition(input.vPosition, rtDepth);//
	half3 L = -(half3)normalize(gDirLightDirection_Leak.xyz);
	half3 V = (half3)normalize(CameraPosition - WorldPos);
	half3 Cdir = (half3)gDirLightColor_Intensity.rgb;
	half  Idir = (half)gDirLightColor_Intensity.w;
	half Ienv_light = Idir * 0.2h;
	half3 Csky = (half3)mSkyLightColor;
	half3 Cground = (half3)mGroundLightColor;
	half DirLightLeak = (half)gDirLightDirection_Leak.w;

	//shadow;
	half ShadowValue = 1.0h;

	ShadowFilterData mSFD;
	mSFD.mShadowMap = GShadowMap;
	mSFD.mShadowMapSampler = Samp_GShadowMap;
	mSFD.mShadowMapSizeAndRcp = gShadowMapSizeAndRcp;
	mSFD.mShadowTransitionScale = (half)gShadowTransitionScale;

	float4 ShadowMapUV = float4(0.0f, 0.0f, 0.0f, 0.0f);
	//half PerPixelViewerDistance = (half)input.vPosition.w;
	half PerPixelViewerDistance = (half)LinearFromDepth(rtDepth);

	output.RT0.a = 1.0h;
#if DISABLE_SHADOW_ALL == 1
	ShadowValue = 1.0h;
#else
	if (PerPixelViewerDistance > gShadowDistance || GBuffer.IsAcceptShadow() == false)
	{
		ShadowValue = 1.0h;
	}
	else
	{
		for (int CsmIdx = 0; CsmIdx < gCsmNum; CsmIdx++)
		{
			if (PerPixelViewerDistance < (half)gCsmDistanceArray[CsmIdx])
			{
				ShadowMapUV = mul(float4(WorldPos, 1.0f), gViewer2ShadowMtxArrayEditor[CsmIdx]);
				mSFD.mShadowTransitionScale = (half)gShadowTransitionScaleArrayEditor[CsmIdx];
				break;
			}
		}

		if (ShadowMapUV.z > 0.0f)
		{
			mSFD.mViewer2ShadowDepth = (half)ShadowMapUV.z;
			
			//#if USE_ESM
			//ShadowValue = GetESMValue(ShadowMapUV.xy, mSFD, 10.0);
			//#else
			ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);
			//#endif
			//ShadowValue = NoFiltering(ShadowMapUV.xy, mSFD);
			
			half FadeValue = (half)saturate(PerPixelViewerDistance * gFadeParam.x + gFadeParam.y);
			ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
		}

// 		ShadowMapUV = mul(float4(WorldPos, 1.0f), gViewer2ShadowMtx[0]);

// 		mSFD.mViewer2ShadowDepth = (half)ShadowMapUV.z;

// //#if USE_ESM
// 	ShadowValue = GetESMValue(ShadowMapUV.xy, mSFD, 10);//GetESMValue(float2 SMUV, float CurrentDepth, ShadowFilterData SFD, float ESM_C)
// //#else
// //	ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);
// //#endif

		if (ShadowValue < 1.0f)
			output.RT0.a = 0.0h;

		half FadeValue = (half)saturate(PerPixelViewerDistance * gFadeParam.x + gFadeParam.y);		
		ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
	}
#endif

	half Sdiff = 1.0h - Metallic;
	half3 OptDiffShading = Sdiff * Albedo;

	//AbsSpecular = 0.08h * AbsSpecular;
	half3 OptSpecShading = AbsSpecular - AbsSpecular * Metallic + Metallic * Albedo;

	half3 H = normalize(L + V);
	half NoLsigned = dot(N, L);
	half NoL = max(NoLsigned, 0.0h);
	half NoH = max(dot(N, H), 0.0h);
	half LoH = max(dot(L, H), 0.0h);
	half NoV = max(dot(N, V), 0.0h);

	//sky light;
	//half SkyAtten = 1.0h - NoL;
	half SkyAtten = min(1.0h, 2.0h - NoL - ShadowValue);
	half3 SkyShading = lerp(Cground, Csky, 0.5h * N.y + 0.5h) * SkyAtten * SkyAtten * OptDiffShading * Ienv_light;
	if (NoPixel)
	{
		SkyShading = 0;
	}
	//half3 SkyShading = (0.35h * N.y + 0.65h) * Csky * OptDiffShading * ECCd;
	//half3 SkyShading = (0.35h * N.y + 0.65h) * Ienv_light * Csky * OptDiffShading * ECCd;
	//half3 SkyShading = Ienv_light * Csky * OptDiffShading * ECCd;

	//half3 DirLightDiffuseShading = NoL * Idir * Cdir * OptDiffShading * ECCd;
	half3 DirLightDiffuseShading = RetroDiffuseMobile(NoL, NoV, LoH, Roughness) * Idir * Cdir * OptDiffShading;

	half3 DirLightSpecShading = BRDFMobile(Roughness, N, H, NoH, LoH, NoV, NoL, OptSpecShading) * sqrt(NoL) * Idir * Cdir;

	//sphere env mapping;
	half3 VrN = 2.0h * NoV * N - V;
	half3 EnvMapUV = CalcSphereMapUV(VrN, Roughness, (half)gEnvMapMaxMipLevel);
	half3 EnvSpecLightColor = (half3)gEnvMap.SampleLevel(Samp_gEnvMap, EnvMapUV.xy, EnvMapUV.z).rgb;
	half Ihdr = max(0.6h, CalcLuminanceYCbCr(EnvSpecLightColor));
	Ihdr = exp2((Ihdr - 0.6h) * 7.5h);
	half3 EnvSpec = (half3)EnvBRDFMobile(EnvSpecLightColor, OptSpecShading, Roughness, NoV) * Ihdr;

	half FinalShadowValue = min(1.0h, ShadowValue + DirLightLeak);
	AOs = min((NoL + FinalShadowValue) * 0.25h + AOs, 1.0h);

	half AoOffset = CalcLuminanceYCbCr((EnvSpec) * 10.0h);
	AoOffsetEncoded = 0.9999h - min(0.9999h, FinalShadowValue * 0.5h + AoOffset);

	/////=======
	AOs = 1.0h;
	AoOffsetEncoded = 0.0h;
	/////=======

	BaseShading = DirLightDiffuseShading * FinalShadowValue + DirLightSpecShading * ShadowValue + SkyShading;
	BaseShading = BaseShading * AOs + EnvSpec * min(ShadowValue + 0.85h, 1.0h);
	
#if ENV_DISABLE_POINTLIGHTS == 0
	if (NoPixel == false)
	{
		float2 tileIdxF = (uv * gViewportSizeAndRcp.xy) / TileSize;
		uint2 tileIdx = (uint2)tileIdxF;
		uint indexOfTile = GetTileIndex(tileIdx.x, tileIdx.y);
		uint NumOfLights = min(TilingBuffer[indexOfTile].NumPointLight, 32);
		for (int i = 0; i < NumOfLights; i++)
		{
			uint lightIndex = TilingBuffer[indexOfTile].PointLights[i];
			FPointLight light = GpuScene_PointLights[lightIndex];
			BaseShading += PointLightShading(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
		}
	}
#endif
	
    half3 Color = BaseShading.rgb;

	output.RT0.rgb = Linear2sRGB(Color);
	//output.RT0.rgb = Linear2sRGB(GBuffer.MtlColorRaw);

	return output;
}

#endif//#ifndef _MOBILE_COPY_