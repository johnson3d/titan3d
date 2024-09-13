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
#include "DeferredCommon.cginc"

#include "MdfQueue"

#define FXAA_GREEN_AS_LUMA		1
#define FXAA_QUALITY__PRESET		10
#define FXAA_HLSL_4 1

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

TextureCube gEnvMap DX_AUTOBIND;
SamplerState Samp_gEnvMap DX_AUTOBIND;

Texture2D gPreIntegratedGF DX_AUTOBIND;
SamplerState Samp_gPreIntegratedGF DX_AUTOBIND;

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

	//if (GBuffer.ObjectFlags_2Bit == 0)
    if (GBuffer.IsUnlit() && GBuffer.IsAcceptShadow() == false)
    {
        output.RT0.rgb = GBuffer.MtlColorRaw;
        return output;
    }

	bool NoPixel = (all(GBuffer.WorldNormal) == 0);
	
	half3 Albedo = sRGB2Linear((half3)GBuffer.MtlColorRaw);
	half AbsSpecular = GBuffer.Specular;

	half3 N = GBuffer.WorldNormal;
    //N = half3(-0.46236, 0.38808, -0.79864);
    //N = normalize(N);
	half Metallic = (half)GBuffer.Metallicity;
	half Roughness = GetRoughness((half)GBuffer.Roughness, GBuffer.WorldNormal);
    half AOs = GBuffer.AO;

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
        float clip_u_min = 0;
        float clip_u_max = 0.25;
		for (int CsmIdx = 0; CsmIdx < gCsmNum; CsmIdx++)
		{
			if (PerPixelViewerDistance < (half)gCsmDistanceArray[CsmIdx])
			{
				ShadowMapUV = mul(float4(WorldPos, 1.0f), gViewer2ShadowMtxArray[CsmIdx]);
                ShadowMapUV.z = ShadowMapUV.z / ShadowMapUV.w;
				mSFD.mShadowTransitionScale = (half)gShadowTransitionScaleArray[CsmIdx];
                if (ShadowMapUV.x > clip_u_max || ShadowMapUV.x < clip_u_min)
                {
					#if USE_INVERSE_Z == 1
                    ShadowMapUV.z = 1;
					#else
                    ShadowMapUV.z = 0;
					#endif
                }
				break;
            }
            clip_u_min = clip_u_max;
            clip_u_max += 0.25;
        }

		#if USE_INVERSE_Z == 1
		if (ShadowMapUV.z < 1.0f)
		#else
		if (ShadowMapUV.z > 0.0f)
		#endif
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
	float3 OptSpecShading = AbsSpecular - AbsSpecular * Metallic + Metallic * Albedo;

	half3 H = normalize(L + V);
	half NoLsigned = dot(N, L);
	half NoL = saturate(NoLsigned);
	half NoH = saturate(dot(N, H));
	half LoH = saturate(dot(L, H));
	half NoV = saturate(dot(N, V));
	half VoH = saturate(dot(V, H));

	// NoV = saturate( abs( NoV ) + 1e-5 );
    
	// todo: remove Csky Cground
	// sky light; 
	half SkyAtten = min(1.0h, 2.0h - NoL - ShadowValue);
	half3 SkyShading = lerp(Cground, Csky, 0.5h * N.y + 0.5h) * SkyAtten * SkyAtten * OptDiffShading * Ienv_light;
	if (NoPixel)
	{
		SkyShading = 0;
	}

	Roughness = max( Roughness, 0.02 );
	half3 DirLightDiffuseShading = RetroDiffuseMobile(NoL, NoV, LoH, Roughness) * Idir * Cdir * OptDiffShading;

	// SphereMaxNoH
	BxDFContext Context;
    Context.Init(N, V, L);
	// todo: calc SphereSinAlpha from light parameters
	float AreaLightSphereSinAlpha = 0.00467f * (1-Pow2(Roughness));
	// float AreaLightSphereSinAlpha = 0.405f;
	SphereMaxNoH(Context, AreaLightSphereSinAlpha, true);
	Context.NoV = saturate(abs( Context.NoV ) + 1e-5);
    float SpecT;
    half3 DirLightSpecShading = NoL * Idir * Cdir * SpecularGGX(Roughness, OptSpecShading, Context, NoL, AreaLightSphereSinAlpha, SpecT);
	// half3 DirLightSpecShading = NoL * Idir * Cdir * SpecularGGX( Roughness, OptSpecShading, NoH, NoV, NoL, VoH );

	// env mapping;
    half3 R = 2 * dot(V, N) * N - V;
	// Point lobe in off-specular peak direction
    R = GetOffSpecularPeakReflectionDir(N, R, GBuffer.Roughness);
    half3 EnvSpecLightColor = 0;
    if (GBuffer.IsDisableEnvColor() == false)
    {
        half EnvMipLevel = GetTexMipLevelFromRoughness(Roughness, (half)gEnvMapMaxMipLevel);
        EnvSpecLightColor = (half3) gEnvMap.SampleLevel(Samp_gEnvMap, R, EnvMipLevel).rgb;
    }
    else
    {
        EnvSpecLightColor = 0;
    }	
	float RoughnessSq = GBuffer.Roughness * GBuffer.Roughness;
	float SpecularOcclusion = GetSpecularOcclusion(NoV, RoughnessSq, AOs);
	half3 EnvSpec = (half3)EnvBRDF(EnvSpecLightColor, OptSpecShading, Roughness, NoV, gPreIntegratedGF, Samp_gPreIntegratedGF) * SpecularOcclusion;
	EnvSpec = -min(-EnvSpec.rgb, 0.0);

	half FinalShadowValue = min(1.0h, ShadowValue + DirLightLeak);

	BaseShading = DirLightDiffuseShading * FinalShadowValue + DirLightSpecShading * ShadowValue + SkyShading;
    BaseShading = BaseShading * AOs + EnvSpec;

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
#if ENV_EDebugShowMode == EDebugShowMode_None
	output.RT0.rgb = Linear2sRGB(Color);
#elif ENV_EDebugShowMode == EDebugShowMode_N
	output.RT0.rgb = float3(SpecT,SpecT,SpecT);
#elif ENV_EDebugShowMode == EDebugShowMode_NoH
	output.RT0.rgb = NoH;
#elif ENV_EDebugShowMode == EDebugShowMode_LoH
	output.RT0.rgb = LoH;
#elif ENV_EDebugShowMode == EDebugShowMode_NoV
	output.RT0.rgb = NoV;
#elif ENV_EDebugShowMode == EDebugShowMode_VoH
	output.RT0.rgb = VoH;
#elif ENV_EDebugShowMode == EDebugShowMode_NoL
	output.RT0.rgb = NoL;
#elif ENV_EDebugShowMode == EDebugShowMode_Specular
	output.RT0.rgb = DirLightSpecShading;
#endif
	
	return output;
}

#endif//#ifndef _MOBILE_COPY_