#ifndef _MobileBasePassPS_H_
#define _MobileBasePassPS_H_

Texture2D gEnvMap DX_NOBIND;
SamplerState Samp_gEnvMap DX_NOBIND;

Texture2D gEyeEnvMap DX_NOBIND;
SamplerState Samp_gEyeEnvMap DX_NOBIND;

Texture2D		gShadowMap DX_NOBIND;
SamplerState	Samp_gShadowMap DX_NOBIND;

StructuredBuffer<FTileData> TilingBuffer DX_NOBIND;

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

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

#ifdef MTL_ID_UNLIT
	{
		half3 Emissive = (half3)mtl.mEmissive;
		half3 UnlitShading = Albedo + Emissive;
		
		half PerPixelViewerDistance = (half)input.psCustomUV0.w;
		UnlitShading.b = (half)floor(UnlitShading.b * AO_M);
		output.RT0 = half4(UnlitShading, PerPixelViewerDistance * rcp((half)gZFar));

		output.RT0.a = 1;
	}
#else
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
		mSFD.mShadowMapSizeAndRcp = gShadowMapSizeAndRcp;
		mSFD.mShadowTransitionScale = (half)gShadowTransitionScale;

		float4 ShadowMapUV = float4(0.0f, 0.0f, 0.0f, 0.0f);
		//half PerPixelViewerDistance = (half)input.vPosition.w;
		half PerPixelViewerDistance = (half)input.psCustomUV0.w;

	#if MODE_EDITOR == 1
		for (int CsmIdx = 0; CsmIdx < gCsmNum; CsmIdx++)
		{
			if (PerPixelViewerDistance < (half)gCsmDistanceArray[CsmIdx])
			{
				ShadowMapUV = mul(float4(input.vWorldPos, 1.0f), gViewer2ShadowMtxArrayEditor[CsmIdx]);
				mSFD.mShadowTransitionScale = (half)gShadowTransitionScaleArrayEditor[CsmIdx];
				break;
			}
		}

		if (ShadowMapUV.z > 0.0f)
		{
			mSFD.mViewer2ShadowDepth = (half)ShadowMapUV.z;
			
			ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);
			//ShadowValue = NoFiltering(ShadowMapUV.xy, mSFD);
			
			half FadeValue = (half)saturate(PerPixelViewerDistance * gFadeParam.x + gFadeParam.y);
			ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
		}
	#else
		if (PerPixelViewerDistance > gShadowDistance)
		{
			ShadowValue = 1.0h;
		}
		else
		{
			ShadowMapUV = mul(float4(input.vWorldPos, 1.0f), gViewer2ShadowMtx[0]);

			mSFD.mViewer2ShadowDepth = (half)ShadowMapUV.z;

			ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);

			half FadeValue = (half)saturate(PerPixelViewerDistance * gFadeParam.x + gFadeParam.y);
			ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
		}
	#endif//MODE_EDITOR

#if DISABLE_SHADOW_MDFQUEUE == 1 || DISABLE_SHADOW_ALL == 1
		ShadowValue = 1.0h;
#endif//#if ENV_DISABLE_SHADOW == 1

		half3 BaseShading = half3(0.0h, 0.0h, 0.0h);

		half3 WorldPos = (half3)input.vWorldPos;
		half3 L = -(half3)normalize(gDirLightDirection_Leak.xyz);
		half3 V = (half3)normalize(CameraPosition - WorldPos);
		half3 Cdir = (half3)gDirLightColor_Intensity.rgb;
		half  Idir = (half)gDirLightColor_Intensity.w;
		half Ienv_light = Idir * 0.2h;
		half3 Csky = (half3)mSkyLightColor;
		half3 Cground = (half3)mGroundLightColor;
		half DirLightLeak = (half)gDirLightDirection_Leak.w;

#ifdef MTL_ID_SKIN
		/*{
			half Sbrtf = Transmit * 0.25h;
			half Sbrdf = 1.0h - Sbrtf;
			Metallic = 0.2h * Sbrdf;
			half Sdiff = Sbrdf - Metallic;
			AbsSpecular = 0.08h * AbsSpecular;
			half Sspec = AbsSpecular - AbsSpecular * Metallic + Metallic;

			half3 OptDiffShading = Sdiff * Albedo;
			half3 OptSpecShading = Sspec * Albedo;

			half3 SkyDiffuseShading = (0.45h * N.y + 0.55h) * Csky * OptDiffShading * ECCd * Ienv_light;

			half3 H = normalize(L + V);

			half NoLSigned = dot(N, L);
			half NoL = max(NoLSigned, 0.0h);
			half NoH = max(dot(N, H), 0.0h);
			half LoH = max(dot(L, H), 0.0h);
			half NoV = max(dot(N, V), 0.0h);

			half3 DirLightDiffuseShading = RetroDiffuseMobile(NoL, NoV, LoH, Roughness) * ECCd * Idir * Cdir * OptDiffShading;

			half3 DirLightSpecShading = BRDFMobile(Roughness, N, H, NoH, LoH, NoV, NoL, OptSpecShading) * NoL * Idir * Cdir;

			half Td = 1.0h - abs(NoLSigned);
			half InverseNoL = 1.0h - max(0.0h, -NoLSigned);
			half3 TransmitShading = Td * InverseNoL * SubAlbedo * Sbrtf * Idir * Cdir;

			BaseShading = (DirLightDiffuseShading + DirLightSpecShading + TransmitShading) * ShadowValue + SkyDiffuseShading;

		}*/
#elif  defined(MTL_ID_TRANSMIT)
		half Sbrdf = 1.0h - Transmit * 0.5h;
		half Sdiff = 1.0h - Metallic;
		AbsSpecular = 0.08h * AbsSpecular;
		half Sspec = AbsSpecular - AbsSpecular * Metallic + Metallic;

		half3 OptDiffShading = Sbrdf * Sdiff * Albedo;
		half3 OptSpecShading = Sbrdf * Sspec * Albedo;

		half3 H = normalize(L + V);
		half NoLsigned = dot(N, L);
		half NoL = max(NoLsigned, 0.0h);
		half NoH = max(dot(N, H), 0.0h);

		//half SkyAtten = 1.0h - NoL;
		half SkyAtten = min(1.0h, 2.0h - NoL - ShadowValue);
		half3 SkyShading = lerp(Cground, Csky, 0.5h * N.y + 0.5h) * SkyAtten * SkyAtten * OptDiffShading * Ienv_light;

		half3 DirLightDiffuseShading = NoL * Idir * Cdir * OptDiffShading;
		half3 DirLightSpecShading = BRDFMobileSimple(Roughness, N, H, NoH, OptSpecShading) * NoL * Idir * Cdir;

		half TwistedShadowValue = min(ShadowValue + 0.25h * Transmit + DirLightLeak, 1.0h);
		AOs = min((NoL + TwistedShadowValue) * 0.25h + AOs, 1.0h);

		//BaseShading = (DirLightDiffuseShading + DirLightSpecShading) * TwistedShadowValue + SkyShading;
		BaseShading = DirLightDiffuseShading * TwistedShadowValue + DirLightSpecShading * ShadowValue + SkyShading;
		BaseShading *= AOs;

		half3 OptTransmitDiffuseShading = Transmit * max(Sdiff - 0.25h, 0.0h) * Albedo;
		half NoLFlipped = max(-NoLsigned, 0.0h);
		half SSS = 0.25h;
		half3 DirLightTransmitDiffuseShading = pow((NoLFlipped + SSS) / Square(1.0h + SSS), Roughness * (-1.5h) + 2.0h) * Idir * Cdir * OptTransmitDiffuseShading;

		half3 OptTransmitSpecShading = Transmit * min(Sspec + 0.25h, 1.0h) * Albedo;
		half LoVFlipped = max(dot(-L, V), 0.0h);
		half3 DirLightTransmitSpecShading = pow(LoVFlipped, 8.0h - Roughness) * Idir * Cdir * OptTransmitSpecShading;

		BaseShading = BaseShading + (DirLightTransmitDiffuseShading + DirLightTransmitSpecShading) * min(1.0h, ShadowValue + 0.63h);

		AoOffsetEncoded = 0.0h;

#elif defined(MTL_ID_HAIR)
		//{
		//	half3 SkyDiffuseShading = (half3)(0.45h * (half)input.vNormal.y + 0.55h) * Csky * Albedo * ECCd * Ienv_light * 0.15h;

		//	//sphere env mapping;
		//	half NoV = (half)max(dot((half3)input.vNormal, V), 0.0h);
		//	half3 VrN = 2.0h * NoV * (half3)input.vNormal - V;
		//	half3 EnvMapUV = CalcSphereMapUV(VrN, 0.8h, (half)gEnvMapMaxMipLevel);
		//	half3 EnvSpecColor = (half3)gEnvMap.SampleLevel(Samp_gEnvMap, EnvMapUV.xy, EnvMapUV.z).rgb;
		//	half3 EnvSpecShading = (half3)EnvSpecColor * 0.75h * Albedo;

		//	half3 HairShading = (half3)HairShadingMobile(Albedo, SubAlbedo, Roughness, L, V, (half3)input.vNormal, N, Transmit, Metallic) * Idir * Cdir;
		//	
		//	BaseShading = HairShading * ShadowValue + SkyDiffuseShading + EnvSpecShading;
		//	//BaseShading = HairShading + SkyDiffuseShading;
		//	//BaseShading = HairShading;
		//}
#elif defined(MTL_ID_EYE)
		//{
			//half Sdiff = 1.0f - Metallic;

			//half3 OptDiffShading = Sdiff * Albedo;

			//half3 SkyShading = lerp(half3(0.1h, 0.1h, 0.1h), Csky, 0.5h * N.y + 0.5h) * OptDiffShading * ECCd * Ienv_light;
			////half3 SkyShading = (0.35h * N.y + 0.65h) * Csky * OptDiffShading * ECCd * Ienv_light;
			//
			//half NoL = max(dot(N, L), 0.0h);
			//half NoV = max(dot(N, V), 0.0h);

			//half3 DirLightDiffuseShading = NoL * Idir * Cdir * OptDiffShading * ECCd;

			////half NoV = max(dot(input.vNormal, V), 0.0f);
			////half3 VrN = 2.0f * NoV * input.vNormal - V;

			//half3 VrN = 2.0h * NoV * N - V;

			//half3 EnvMapUV = CalcSphereMapUV(VrN, 0.3h, (half)gEyeEnvMapMaxMipLevel);
			//half3 EnvSpecLightColor = (half3)gEyeEnvMap.SampleLevel(Samp_gEyeEnvMap, EnvMapUV.xy, EnvMapUV.z).rgb;
			//half Threshold = 0.5h;
			//half Lum = max(0.0h, CalcLuminance(EnvSpecLightColor) - Threshold) * 2.0h;

			//half EyeSparkIntensity = 2.0h;
			//half3 EnvSpecShading = EnvSpecLightColor * Lum * Lum * Mask * EyeSparkIntensity;

			//BaseShading = SkyShading + DirLightDiffuseShading * ShadowValue/* + DirLightSpecShading*/;
			//BaseShading *= AO;
			//BaseShading += EnvSpecShading;
	//}
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
		half3 EnvMapUV = CalcSphereMapUV(VrN, Roughness, (half)gEnvMapMaxMipLevel);
		half3 EnvSpecLightColor = (half3)gEnvMap.SampleLevel(Samp_gEnvMap, EnvMapUV.xy, EnvMapUV.z).rgb;
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
			float2 tileIdxF = (uv.xy * gViewportSizeAndRcp.xy) / TileSize;
			uint2 tileIdx = (uint2)tileIdxF;
			uint indexOfTile = GetTileIndex(tileIdx.x, tileIdx.y);
			for (int i = 0; i < TilingBuffer[indexOfTile].NumPointLight; i++)
			{
				uint lightIndex = TilingBuffer[indexOfTile].PointLights[i];
				FPointLight light = GpuScene_PointLights[lightIndex];
				BaseShading += PointLightShading(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
			}
		}
#endif//#if ENV_DISABLE_POINTLIGHTS == 0

#endif//#ifdef MTL_ID_SKIN
		BaseShading += Emissive;

		//half4 FogTRNF = half4(300.0h, 500.0h, 10.0h, 300.0h);
		//half3 SceneVS = input.psCustomUV0.xyz;
		//half FogAlpha = CalcHeighFogAlpha(WorldPos.y, SceneVS, FogTRNF);
		//half3 FogColor = half3(0.8h, 0.8h, 0.8h);
		////half3 FogColor = half3(1.0h, 0.0h, 0.0h);
		//half3 FogShading = FogColor * (Csky + Cdir);
		//BaseShading = lerp(BaseShading, FogShading, FogAlpha);

		//output.RT0 = half4((half2)input.psCustomUV0.xy, 0.0h, 1.0h);
		//float2 ViewportUV = input.vPosition.xy  * gViewportSizeAndRcp.zw;
		//output.RT0 = half4(ViewportUV, 0.0h, 1.0h);
		//output.RT0 = half4(0.0h, (half)input.psCustomUV0.z, 0.0h, 1.0h);
		//output.RT0 = half4(0.0h, input.vPosition.z, 0.0h, 1.0h);

#if ENV_DISABLE_AO == 1
#else
		BaseShading.b = (half)floor(BaseShading.b * AO_M) + AoOffsetEncoded;
#endif

		output.RT0 = half4(BaseShading, PerPixelViewerDistance * rcp((half)gZFar));

		//output.RT0.rgb = mtl.mAlbedo;
		//output.RT0.a = Alpha;
				
		//output.RT0 = half4(BaseShading, PerPixelViewerDistance);
		//output.RT1 = half4(N * 0.5h + 0.5h, 1.0h);
		
		
		//output.RT0 = half4(V * 0.5h + 0.5h, 1.0f);
		//output.RT0 = half4(N * 0.5h + 0.5h, 1.0f);
		//output.RT0 = half4(1.0f, 1.0f, 1.0f, 1.0f);
		//output.RT0 = half4(SkyShading, 1.0f);
		//half3 NDCPos = half3((input.vPosition.xy * gViewportSizeAndRcp.zw - 0.5h) * half2(2.0h, -2.0h), input.vPosition.z);
		//output.RT0 = half4(0.0h, NDCPos.y, 0.0h, 1.0h);

		/*if (input.vPosition.w > 5.0h)
		{
			output.RT0 = half4(1.0h, 0.0h, 0.0h, 1.0h);
		}
		else
		{
			output.RT0 = half4(0.0h, 1.0h, 0.0h, 1.0h);
		}*/
		/*half4 ClipPos = mul(half4(input.vWorldPos, 1.0h), ViewPrjMtx);
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