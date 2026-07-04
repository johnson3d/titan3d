#ifndef _Forword_BasePassPS_H_
#define _Forword_BasePassPS_H_

TextureCube gEnvMap DX_AUTOBIND;
SamplerState Samp_gEnvMap DX_AUTOBIND;

Texture2D gEyeEnvMap DX_AUTOBIND;
SamplerState Samp_gEyeEnvMap DX_AUTOBIND;

Texture2D gShadowMap DX_AUTOBIND;
SamplerState Samp_gShadowMap DX_AUTOBIND;

StructuredBuffer<FTileData> TilingBuffer DX_AUTOBIND;

int GetShadingMode(uint RenderFlags_10Bit)
{
    return (RenderFlags_10Bit & SHADINGMODE_BIT_MASK) >> SHADINGMODE_BIT_OFFSET;
}
struct PS_OUTPUT
{
    float4 RT0 : SV_Target0;
#if ENABLE_MOTION_VECTOR == 1
	float4 RT1 : SV_Target1;
#endif
};

PS_OUTPUT PS_MobileBasePass(PS_INPUT input)
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
        mSFD.mShadowMapSizeAndRcp = ShadowMapSizeAndRcp;
        mSFD.mShadowTransitionScale = (half) ShadowTransitionScale;

        float4 ShadowMapUV = float4(0.0f, 0.0f, 0.0f, 0.0f);
		//half PerPixelViewerDistance = (half)input.vPosition.w;
        half PerPixelViewerDistance = (half) input.psCustomUV0.w;
		
        for (int CsmIdx = 0; CsmIdx < CsmNum; CsmIdx++)
        {
            if (PerPixelViewerDistance < (half) CsmDistanceArray[CsmIdx])
            {
                ShadowMapUV = mul(float4(input.vWorldPos, 1.0f), Viewer2ShadowMtxArray[CsmIdx]);
                mSFD.mShadowTransitionScale = (half) ShadowTransitionScaleArray[CsmIdx];
                break;
            }
        }

        if (ShadowMapUV.z > 0.0f)
        {
            mSFD.mViewer2ShadowDepth = (half) ShadowMapUV.z;
			
			//#if USE_ESM
            ShadowValue = GetESMValue(ShadowMapUV.xy, mSFD, 10.0);
			//#else
			//ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);
			//#endif
			//ShadowValue = NoFiltering(ShadowMapUV.xy, mSFD);
			
            half FadeValue = (half) saturate(PerPixelViewerDistance * FadeParam.x + FadeParam.y);
            ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
        }
	
#if DISABLE_SHADOW_ALL == 1
		ShadowValue = 1.0h;
#endif//#if ENV_DISABLE_SHADOW == 1

        half3 BaseShading = half3(0.0h, 0.0h, 0.0h);

        half3 WorldPos = (half3) input.vWorldPos;
        half3 L = -(half3) normalize(DirLight.Direction.xyz);
        half3 V = (half3) normalize(CameraPosition - WorldPos);
        half3 Cdir = (half3) DirLight.SunLightColor.rgb;
        half Idir = (half) DirLight.SunLightIntensity;
        half Ienv_light = Idir * 0.2h;
        half3 Csky = (half3) DirLight.SkyLightColor;
        half3 Cground = (half3) DirLight.GroundLightColor;
        half DirLightLeak = (half) DirLight.SunLightLeak;

        uint shadingMode = GetShadingMode(MaterialRenderFlags | MeshRenderFlags);
        if (shadingMode == EShadingMode_Subsurface)
        {
        }
        else if (shadingMode == EShadingMode_Hair)
        {
            BaseShading = Albedo;
        }
        else if (shadingMode == EShadingMode_Unlit)
        {
            half3 Emissive = (half3) mtl.mEmissive;
            half3 UnlitShading = Albedo + Emissive;
		
            half PerPixelViewerDistance = (half) input.psCustomUV0.w;

            UnlitShading.b = (half) floor(UnlitShading.b * AO_M);

            output.RT0 = half4(UnlitShading, PerPixelViewerDistance * rcp((half) ZFar));

            output.RT0.a = 1;
        }
        else
        {
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
            R = GetOffSpecularPeakReflectionDir(N, R, Roughness);
            half EnvMipLevel = GetTexMipLevelFromRoughness(Roughness, (half) EnvMapMaxMipLevel);
            half3 EnvSpecLightColor = (half3) gEnvMap.SampleLevel(Samp_gEnvMap, R, EnvMipLevel).rgb;
            half Ihdr = max(0.6h, CalcLuminanceYCbCr(EnvSpecLightColor));
            Ihdr = exp2((Ihdr - 0.6h) * 7.5h);
            half3 EnvSpec = (half3) EnvBRDFMobile(EnvSpecLightColor, OptSpecShading, Roughness, NoV) * Ihdr;

            half FinalShadowValue = min(1.0h, ShadowValue + DirLightLeak);
            AOs = min((NoL + FinalShadowValue) * 0.25h + AOs, 1.0h);

            half AoOffset = CalcLuminanceYCbCr((EnvSpec + Emissive) * 10.0h);
            AoOffsetEncoded = 0.9999h - min(0.9999h, FinalShadowValue * 0.5h + AoOffset);

            BaseShading = DirLightDiffuseShading * FinalShadowValue + DirLightSpecShading * ShadowValue + SkyShading;
            BaseShading = BaseShading * AOs + EnvSpec * min(ShadowValue + 0.85h, 1.0h);
		
		    //point light part;
		    //BaseShading += MultiPbrPointLightMobile(input, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness);
            if (true)
            {
                float2 uv = input.psCustomUV0.xy;
			    /*uv.x = saturate( (input.vPosition.x + 1.0f) * 0.5f );
			    uv.y = saturate( (1.0f - input.vPosition.y) * 0.5f );*/
                float2 tileIdxF = (uv.xy * ViewportSizeAndRcp.xy) / TileSize;
                uint2 tileIdx = (uint2) tileIdxF;
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
			
            BaseShading += Emissive;            
            BaseShading.b = (half) floor(BaseShading.b * AO_M) + AoOffsetEncoded;
            output.RT0 = half4(BaseShading, PerPixelViewerDistance * rcp((half) ZFar));
        }
    }
	
	//output.RT0.rgb = Albedo;
	//output.RT0.a = Alpha;

#if ENABLE_MOTION_VECTOR == 1
	{
		float2 previousScreenPos = (input.psCustomUV1.xy / input.psCustomUV1.w) * 0.5 + 0.5;
		float2 currentScreenPos = (input.psCustomUV2.xy / input.psCustomUV2.w) * 0.5 + 0.5;
		float2 motionVector = currentScreenPos - previousScreenPos;
		output.RT1.rg = EncodeMotionVector(motionVector);
		output.RT1.ba = float2(0, 0);
	}
#endif
	
    return output;
}

#endif//_MobileBasePassPS_H_