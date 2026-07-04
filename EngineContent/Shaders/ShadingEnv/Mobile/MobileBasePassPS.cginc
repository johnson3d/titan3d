#ifndef _MobileBasePassPS_H_
#define _MobileBasePassPS_H_

TextureCube gEnvMap DX_AUTOBIND;
SamplerState Samp_gEnvMap DX_AUTOBIND;

Texture2D gEyeEnvMap DX_AUTOBIND;
SamplerState Samp_gEyeEnvMap DX_AUTOBIND;

Texture2D		gShadowMap DX_AUTOBIND;
SamplerState	Samp_gShadowMap DX_AUTOBIND;

StructuredBuffer<FTileData> TilingBuffer DX_AUTOBIND;

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

int GetShadingMode(uint RenderFlags_10Bit)
{
    return (RenderFlags_10Bit & SHADINGMODE_BIT_MASK) >> SHADINGMODE_BIT_OFFSET;
}

PS_OUTPUT PS_MobileBasePass(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

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

	half3 Albedo = sRGB2Linear((half3)mtl.mAlbedo);
	half Alpha = (half)mtl.mAlpha;
	half AlphaTestThreshold = (half)mtl.mAlphaTest;

#ifdef ALPHA_TEST
	clip(Alpha - AlphaTestThreshold);
#endif // AlphaTest

    uint shadingMode = GetShadingMode(MaterialRenderFlags | MeshRenderFlags);
    if (shadingMode == EShadingMode_Unlit)
	{
		half3 Emissive = (half3)mtl.mEmissive;
		half3 UnlitShading = Albedo + Emissive;
		
		half PerPixelViewerDistance = (half)input.psCustomUV0.w;

#if ENV_DISABLE_AO == 1
#else
		UnlitShading.b = (half)floor(UnlitShading.b * AO_M);
#endif
		output.RT0 = half4(UnlitShading, PerPixelViewerDistance * rcp((half)ZFar));

		output.RT0.a = 1;
	}
	else
	{
		half3 N = normalize((half3)mtl.mNormal);
		half Metallic = (half)mtl.mMetallic;
		half Smoothness = (half)mtl.mRough;
		half Roughness = 1.0h - Smoothness;
		half AbsSpecular = (half)mtl.mAbsSpecular;
		half3 Emissive = (half3)mtl.mEmissive;
		half Transmit = (half)mtl.mTransmit;
		half3 SubAlbedo = sRGB2Linear((half3)mtl.mSubAlbedo);
		half AOs = (half)mtl.mAO;
		half Mask = (half)mtl.mMask;

		//shadow;
		half ShadowValue = 1.0h;

		ShadowFilterData mSFD;
		mSFD.mShadowMap = gShadowMap;
		mSFD.mShadowMapSampler = Samp_gShadowMap;
		mSFD.mShadowMapSizeAndRcp = ShadowMapSizeAndRcp;
		mSFD.mShadowTransitionScale = (half)ShadowTransitionScale;

		float4 ShadowMapUV = float4(0.0f, 0.0f, 0.0f, 0.0f);
		//half PerPixelViewerDistance = (half)input.vPosition.w;
		half PerPixelViewerDistance = (half)input.psCustomUV0.w;

	#if MODE_EDITOR == 1
		for (int CsmIdx = 0; CsmIdx < CsmNum; CsmIdx++)
		{
			if (PerPixelViewerDistance < (half)CsmDistanceArray[CsmIdx])
			{
				ShadowMapUV = mul(float4(input.vWorldPos, 1.0f), Viewer2ShadowMtxArray[CsmIdx]);
				mSFD.mShadowTransitionScale = (half)ShadowTransitionScaleArray[CsmIdx];
				break;
			}
		}

		if (ShadowMapUV.z > 0.0f)
		{
			mSFD.mViewer2ShadowDepth = (half)ShadowMapUV.z;
			
			//#if USE_ESM
			ShadowValue = GetESMValue(ShadowMapUV.xy, mSFD, 10.0);
			//#else
			//ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);
			//#endif
			//ShadowValue = NoFiltering(ShadowMapUV.xy, mSFD);
			
			half FadeValue = (half)saturate(PerPixelViewerDistance * FadeParam.x + FadeParam.y);
			ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
		}
	#else
		if (PerPixelViewerDistance > ShadowDistance)
		{
			ShadowValue = 1.0h;
		}
		else
		{
			ShadowMapUV = mul(float4(input.vWorldPos, 1.0f), Viewer2ShadowMtx[0]);

			mSFD.mViewer2ShadowDepth = (half)ShadowMapUV.z;

//			#if USE_ESM
			ShadowValue = GetESMValue(ShadowMapUV.xy, mSFD, 10.0);
//			#else
//			ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);
//			#endif

			half FadeValue = (half)saturate(PerPixelViewerDistance * FadeParam.x + FadeParam.y);
			ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
		}
	#endif//MODE_EDITOR

#if DISABLE_SHADOW_MDFQUEUE == 1 || DISABLE_SHADOW_ALL == 1
		ShadowValue = 1.0h;
#endif//#if ENV_DISABLE_SHADOW == 1

		half3 BaseShading = half3(0.0h, 0.0h, 0.0h);

		half3 WorldPos = (half3)input.vWorldPos;
		half3 L = -(half3)normalize(DirLight.Direction.xyz);
		half3 V = (half3)normalize(CameraPosition - WorldPos);
		half3 Cdir = (half3)DirLight.SunLightColor.rgb;
		half  Idir = (half)DirLight.SunLightIntensity;
		half Ienv_light = Idir * 0.2h;
		half3 Csky = (half3)DirLight.SkyLightColor;
		half3 Cground = (half3)DirLight.GroundLightColor;
        half DirLightLeak = (half) DirLightLeak.SunLightLeak;
		
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
		half3 R = 2 * dot(V, N) * N - V;
		// Point lobe in off-specular peak direction
		R = GetOffSpecularPeakReflectionDir(N, R, GBuffer.Roughness);
		half EnvMipLevel = GetTexMipLevelFromRoughness(Roughness, (half)EnvMapMaxMipLevel);
		half3 EnvSpecLightColor = (half3) gEnvMap.SampleLevel(Samp_gEnvMap, R, EnvMipLevel).rgb;
		half Ihdr = max(0.6h, CalcLuminanceYCbCr(EnvSpecLightColor));
		Ihdr = exp2((Ihdr - 0.6h) * 7.5h);
		half3 EnvSpec = (half3)EnvBRDFMobile(EnvSpecLightColor, OptSpecShading, Roughness, NoV) * Ihdr;

		half FinalShadowValue = min(1.0h, ShadowValue + DirLightLeak);
		AOs = min((NoL + FinalShadowValue) * 0.25h + AOs, 1.0h);

		half AoOffset = CalcLuminanceYCbCr((EnvSpec + Emissive) * 10.0h);
		AoOffsetEncoded = 0.9999h - min(0.9999h, FinalShadowValue * 0.5h+ AoOffset);

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
			float2 tileIdxF = (uv.xy * ViewportSizeAndRcp.xy) / TileSize;
			uint2 tileIdx = (uint2)tileIdxF;
			uint indexOfTile = GetTileIndex(tileIdx.x, tileIdx.y);
			uint NumOfLights = min(TilingBuffer[indexOfTile].NumPointLight, 32);
			for (int i = 0; i < NumOfLights; i++)
			{
				uint lightIndex = TilingBuffer[indexOfTile].PointLights[i];
				FPointLight light = GpuScene_PointLights[lightIndex];
				BaseShading += PointLightShading(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
			}

			// SpotLight shading
			for (uint si = 0; si < GpuScene_SpotLightNum; si++)
			{
				FSpotLight spotLight = GpuScene_SpotLights[si];
				BaseShading += SpotLightShading(spotLight, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
			}
		}
#endif//#if ENV_DISABLE_POINTLIGHTS == 0
		
		BaseShading += Emissive;

		//half4 FogTRNF = half4(300.0h, 500.0h, 10.0h, 300.0h);
		//half3 SceneVS = input.psCustomUV0.xyz;
		//half FogAlpha = CalcHeighFogAlpha(WorldPos.y, SceneVS, FogTRNF);
		//half3 FogColor = half3(0.8h, 0.8h, 0.8h);
		////half3 FogColor = half3(1.0h, 0.0h, 0.0h);
		//half3 FogShading = FogColor * (Csky + Cdir);
		//BaseShading = lerp(BaseShading, FogShading, FogAlpha);

		//output.RT0 = half4((half2)input.psCustomUV0.xy, 0.0h, 1.0h);
		//float2 ViewportUV = input.vPosition.xy  * ViewportSizeAndRcp.zw;
		//output.RT0 = half4(ViewportUV, 0.0h, 1.0h);
		//output.RT0 = half4(0.0h, (half)input.psCustomUV0.z, 0.0h, 1.0h);
		//output.RT0 = half4(0.0h, input.vPosition.z, 0.0h, 1.0h);

#if ENV_DISABLE_AO == 1
#else
		BaseShading.b = (half)floor(BaseShading.b * AO_M) + AoOffsetEncoded;
#endif

		output.RT0 = half4(BaseShading, PerPixelViewerDistance * rcp((half)ZFar));

		//output.RT0.rgb = mtl.mAlbedo;
		//output.RT0.a = Alpha;
				
		//output.RT0 = half4(BaseShading, PerPixelViewerDistance);
		//output.RT1 = half4(N * 0.5h + 0.5h, 1.0h);
		
		
		//output.RT0 = half4(V * 0.5h + 0.5h, 1.0f);
		//output.RT0 = half4(N * 0.5h + 0.5h, 1.0f);
		//output.RT0 = half4(1.0f, 1.0f, 1.0f, 1.0f);
		//output.RT0 = half4(SkyShading, 1.0f);
		//half3 NDCPos = half3((input.vPosition.xy * ViewportSizeAndRcp.zw - 0.5h) * half2(2.0h, -2.0h), input.vPosition.z);
		//output.RT0 = half4(0.0h, NDCPos.y, 0.0h, 1.0h);

		/*if (input.vPosition.w > 5.0h)
		{
			output.RT0 = half4(1.0h, 0.0h, 0.0h, 1.0h);
		}
		else
		{
			output.RT0 = half4(0.0h, 1.0h, 0.0h, 1.0h);
		}*/
		/*half4 ClipPos = mul(half4(input.vWorldPos, 1.0h), GetViewPrjMtx());
		if (ClipPos.w > 5.0h)
		{
			output.RT0 = half4(1.0h, 0.0h, 0.0h, 1.0h);
		}
		else
		{
			output.RT0 = half4(0.0h, 1.0h, 0.0h, 1.0h);
		}*/

	}

#endif//#ifdef MTL_ID_UNLIT

	//output.RT0.rgb = Albedo;
	//output.RT0.a = Alpha;
	
	return output;
}

#endif//_MobileBasePassPS_H_