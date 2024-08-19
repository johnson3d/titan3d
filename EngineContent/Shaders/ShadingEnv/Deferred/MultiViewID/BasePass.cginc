#ifndef _MULTI_VIEWID_BASE_PASS_
#define _MULTI_VIEWID_BASE_PASS_

#include "../../../Inc/VertexLayout.cginc"
#include "../../../Inc/LightCommon.cginc"
#include "../../../Inc/Math.cginc"
#include "../../../Inc/ShadowCommon.cginc"
#include "../../../Inc/FogCommon.cginc"
#include "../../../Inc/MixUtility.cginc"
#include "../../../Inc/SysFunction.cginc"
#include "../../../Inc/GpuSceneCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../../Inc/SysFunctionDefImpl.cginc"
 
/**Meta Begin:(VS_Main)
HLSL=2021
Meta End:(VS_Main)**/
PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT) 0;
    Default_VSInput2PSInput(output, input);

    MTL_OUTPUT mtl = (MTL_OUTPUT) 0;
	{
#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif

#ifdef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif
    }

#if !defined(VS_NO_WorldTransform)
    output.vPosition.xyz += mtl.mVertexOffset;
    float4 wp4 = mul(float4(output.vPosition.xyz, 1), WorldMatrix);
	
    output.Set_vNormal(normalize(mul(float4(output.Get_vNormal().xyz, 0), WorldMatrix).xyz));
    output.Set_vTangent(normalize(mul(float4(output.Get_vTangent().xyz, 0), WorldMatrix).xyz));

#else
	float4 wp4 = float4(output.vPosition.xyz, 1);
#endif

    output.Set_vWorldPos(wp4.xyz);
	
    //if (input.vViewID == 0)
    //{
    //    output.vPosition = mul(wp4, GetViewPrjMtx(false));
    //}
    //else if (input.vViewID == 1)
    //{
    //    output.vPosition = mul(wp4, GetViewPrjMtx(false));
    //}
    output.vPosition = mul(wp4, GetViewPrjMtx(false));

#if USE_PS_Custom0 == 1
#if ENV_DISABLE_POINTLIGHTS == 0
	output.psCustomUV0.xy = float2(output.vPosition.xy / output.vPosition.w) * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
#endif
	//output.psCustomUV0.z = float(output.vPosition.z / output.vPosition.w);
	output.psCustomUV0.w = output.vPosition.w;
#endif

    return output;
	return output;
}

Texture2D gEnvMap DX_AUTOBIND;
SamplerState Samp_gEnvMap DX_AUTOBIND;

Texture2D gEyeEnvMap DX_AUTOBIND;
SamplerState Samp_gEyeEnvMap DX_AUTOBIND;

Texture2D gShadowMap DX_AUTOBIND;
SamplerState Samp_gShadowMap DX_AUTOBIND;

StructuredBuffer<FTileData> TilingBuffer DX_AUTOBIND;

struct PS_OUTPUT
{
    float4 RT0 : SV_Target0;
};

/**Meta Begin:(PS_Main)
HLSL=2021
Meta End:(PS_Main)**/
PS_OUTPUT PS_Main(PS_INPUT input)
{
    PS_OUTPUT output = (PS_OUTPUT) 0;

    MTL_OUTPUT mtl = Default_PSInput2Material(input);
	//mtl template stuff;
	{
#ifndef DO_PS_MATERIAL
#define DO_PS_MATERIAL DoDefaultPSMaterial
#endif
		DO_PS_MATERIAL(input, mtl);

#ifdef MDFQUEUE_FUNCTION_PS
		MdfQueueDoModifiersPS(input, mtl);
#endif
    }

    half AoOffsetEncoded = 0.0h;

    half3 Albedo = sRGB2Linear((half3) mtl.mAlbedo);
    half Alpha = (half) mtl.mAlpha;
    half AlphaTestThreshold = (half) mtl.mAlphaTest;

#ifdef ALPHA_TEST
	clip(Alpha - AlphaTestThreshold);
#endif // AlphaTest

#ifdef MTL_ID_UNLIT
	{
		half3 Emissive = (half3)mtl.mEmissive;
		half3 UnlitShading = Albedo + Emissive;
		
		half PerPixelViewerDistance = (half)input.psCustomUV0.w;

#if ENV_DISABLE_AO == 1
#else
		UnlitShading.b = (half)floor(UnlitShading.b * AO_M);
#endif
		output.RT0 = half4(UnlitShading, PerPixelViewerDistance * rcp((half)gZFar));

		output.RT0.a = 1;
	}
#else
	{
        half3 N = normalize((half3) mtl.GetWorldNormal(input));
        half Metallic = (half) mtl.mMetallic;
        half Smoothness = (half) mtl.mRough;
        half Roughness = 1.0h - Smoothness;
        half AbsSpecular = (half) mtl.mAbsSpecular;
        half3 Emissive = (half3) mtl.mEmissive;
        half Transmit = (half) mtl.mTransmit;
        half3 SubAlbedo = sRGB2Linear((half3) mtl.mSubAlbedo);
        half AOs = (half) mtl.mAO;
        half Mask = (half) mtl.mMask;

		//shadow;
        half ShadowValue = 1.0h;

        ShadowFilterData mSFD;
        mSFD.mShadowMap = gShadowMap;
        mSFD.mShadowMapSampler = Samp_gShadowMap;
        mSFD.mShadowMapSizeAndRcp = gShadowMapSizeAndRcp;
        mSFD.mShadowTransitionScale = (half) gShadowTransitionScale;

        float4 ShadowMapUV = float4(0.0f, 0.0f, 0.0f, 0.0f);
		//half PerPixelViewerDistance = (half)input.vPosition.w;
        half PerPixelViewerDistance = (half) input.psCustomUV0.w;

        if (PerPixelViewerDistance > gShadowDistance || IsAcceptShadow() == false)
        {
            ShadowValue = 1.0h;
        }
        else
        {
            ShadowMapUV = mul(float4(input.vWorldPos, 1.0f), gViewer2ShadowMtx[0]);

            mSFD.mViewer2ShadowDepth = (half) ShadowMapUV.z;

//			#if USE_ESM
            ShadowValue = GetESMValue(ShadowMapUV.xy, mSFD, 10.0);
//			#else
//			ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);
//			#endif

            half FadeValue = (half) saturate(PerPixelViewerDistance * gFadeParam.x + gFadeParam.y);
            ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
        }

#if DISABLE_SHADOW_ALL == 1
		ShadowValue = 1.0h;
#endif//#if ENV_DISABLE_SHADOW == 1

        half3 BaseShading = half3(0.0h, 0.0h, 0.0h);

        half3 WorldPos = (half3) input.vWorldPos;
        half3 L = -(half3) normalize(gDirLightDirection_Leak.xyz);
        half3 V = (half3) normalize(CameraPosition - WorldPos);
        half3 Cdir = (half3) gDirLightColor_Intensity.rgb;
        half Idir = (half) gDirLightColor_Intensity.w;
        half Ienv_light = Idir * 0.2h;
        half3 Csky = (half3) mSkyLightColor;
        half3 Cground = (half3) mGroundLightColor;
        half DirLightLeak = (half) gDirLightDirection_Leak.w;

#ifdef MTL_ID_SKIN
		
#elif  defined(MTL_ID_TRANSMIT)
		
#elif defined(MTL_ID_HAIR)
		
#elif defined(MTL_ID_EYE)
		
#else
        half Sdiff = 1.0h - Metallic;
        half3 OptDiffShading = Sdiff * Albedo;

        AbsSpecular = 0.08h * AbsSpecular;
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
		//half3 SkyShading = (0.35h * N.y + 0.65h) * Csky * OptDiffShading * ECCd;
		//half3 SkyShading = (0.35h * N.y + 0.65h) * Ienv_light * Csky * OptDiffShading * ECCd;
		//half3 SkyShading = Ienv_light * Csky * OptDiffShading * ECCd;

		//half3 DirLightDiffuseShading = NoL * Idir * Cdir * OptDiffShading * ECCd;
        half3 DirLightDiffuseShading = RetroDiffuseMobile(NoL, NoV, LoH, Roughness) * Idir * Cdir * OptDiffShading;

        half3 DirLightSpecShading = BRDFMobile(Roughness, N, H, NoH, LoH, NoV, NoL, OptSpecShading) * sqrt(NoL) * Idir * Cdir;

		//sphere env mapping;
        half3 VrN = 2.0h * NoV * N - V;
        half3 EnvMapUV = CalcSphereMapUV(VrN, Roughness, (half) gEnvMapMaxMipLevel);
        half3 EnvSpecLightColor = (half3) gEnvMap.SampleLevel(Samp_gEnvMap, EnvMapUV.xy, EnvMapUV.z).rgb;
        half Ihdr = max(0.6h, CalcLuminanceYCbCr(EnvSpecLightColor));
        Ihdr = exp2((Ihdr - 0.6h) * 7.5h);
        half3 EnvSpec = (half3) EnvBRDFMobile(EnvSpecLightColor, OptSpecShading, Roughness, NoV) * Ihdr;

        half FinalShadowValue = min(1.0h, ShadowValue + DirLightLeak);
        AOs = min((NoL + FinalShadowValue) * 0.25h + AOs, 1.0h);

        half AoOffset = CalcLuminanceYCbCr((EnvSpec + Emissive) * 10.0h);
        AoOffsetEncoded = 0.9999h - min(0.9999h, FinalShadowValue * 0.5h + AoOffset);

#if ENV_DISABLE_AO == 1
		AOs = 1.0h;
		AoOffsetEncoded = 0.0h;
#endif

        BaseShading = DirLightDiffuseShading * FinalShadowValue + DirLightSpecShading * ShadowValue + SkyShading;
        BaseShading = BaseShading * AOs + EnvSpec * min(ShadowValue + 0.85h, 1.0h);
		
		//point light part;
#if ENV_DISABLE_POINTLIGHTS == 0
		//BaseShading += MultiPbrPointLightMobile(input, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
		if (true)
		{
			float2 uv = input.psCustomUV0.xy;
			/*uv.x = saturate( (input.vPosition.x + 1.0f) * 0.5f );
			uv.y = saturate( (1.0f - input.vPosition.y) * 0.5f );*/
			float2 tileIdxF = (uv.xy * gViewportSizeAndRcp.xy) / TileSize;
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
#endif//#if ENV_DISABLE_POINTLIGHTS == 0

#endif//#ifdef MTL_ID_SKIN
        BaseShading += Emissive;

#if ENV_DISABLE_AO == 1
#else
        BaseShading.b = (half) floor(BaseShading.b * AO_M) + AoOffsetEncoded;
#endif

        output.RT0 = half4(BaseShading, PerPixelViewerDistance * rcp((half) gZFar));
    }

#endif//#ifdef MTL_ID_UNLIT

	//output.RT0.rgb = Albedo;
	//output.RT0.a = Alpha;
	
    return output;
}

#endif//#ifndef _MULTI_VIEWID_BASE_PASS_