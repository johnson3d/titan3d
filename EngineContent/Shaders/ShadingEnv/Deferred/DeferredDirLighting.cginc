#ifndef _DEFERRED_LIGHTING_H_
#define _DEFERRED_LIGHTING_H_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"
#include "../../Inc/ShadowCommon.cginc"
#include "../../Inc/FogCommon.cginc"
#include "../../Inc/MixUtility.cginc"
#include "../../Inc/SysFunction.cginc"
#include "../../Inc/PostEffectCommon.cginc"
#include "../../Inc/GpuSceneCommon.cginc"
#include "../../Inc/FrustumGridCommon.cginc"
#include "DeferredCommon.cginc"

#include "MdfQueue"

#define FXAA_GREEN_AS_LUMA		1
#define FXAA_QUALITY__PRESET		10
#define FXAA_HLSL_4 1

#include "../../Inc/FXAAMobile.cginc"

#include "../../Inc/SysFunctionDefImpl.cginc"
#include "../../Inc/HairBRDF.cginc"

Texture2D DepthBuffer DX_AUTOBIND;
SamplerState Samp_DepthBuffer DX_AUTOBIND;

Texture2D<uint2> StencilBuffer DX_AUTOBIND;

Texture2D GBufferRT0 DX_AUTOBIND;
SamplerState Samp_GBufferRT0 DX_AUTOBIND;

Texture2D GBufferRT1 DX_AUTOBIND;
SamplerState Samp_GBufferRT1 DX_AUTOBIND;

Texture2D GBufferRT2 DX_AUTOBIND;
SamplerState Samp_GBufferRT2 DX_AUTOBIND;

Texture2D GBufferRT3 DX_AUTOBIND;
SamplerState Samp_GBufferRT3 DX_AUTOBIND;

#if ENV_EShadowMode == EShadowMode_Csm
Texture2D GShadowMap DX_AUTOBIND;
#elif ENV_EShadowMode == EShadowMode_Advance
#include "../../Bricks/AdvanceShadow/AdvanceShadow.cginc"
#elif ENV_EShadowMode == EShadowMode_None
#endif
SamplerState Samp_GShadowMap DX_AUTOBIND;

#if ENV_ContactShadowMode == EContactShadowMode_InputNode
Texture2D GContactShadow DX_AUTOBIND;
SamplerState Samp_GContactShadow DX_AUTOBIND;
#elif ENV_ContactShadowMode == EContactShadowMode_Inline
// Inline contact shadow: screen-space ray march directly inside the lighting pass.
// Zero extra bandwidth (no intermediate texture), reuses DepthBuffer already bound here.
// Field names are prefixed InlineCS_ to avoid clashing with the global HLSL namespace.
cbuffer cbInlineContactShadow DX_AUTOBIND
{
    int   InlineCS_NumSteps;
    float InlineCS_Length;
    float InlineCS_DepthBias;
    float InlineCS_FadeDistance;
    float InlineCS_FadeLength;
    float InlineCS_Intensity;
    uint  InlineCS_FrameIndex;
    float InlineCS_Pad0;
};

#if ENV_INLINE_CS_USE_HZB == 1
Texture2D<float> GHzbTexture DX_AUTOBIND;
SamplerState Samp_GHzbTexture DX_AUTOBIND;
#include "../../Inc/HzbRayCast.cginc"
#endif

// Interleaved Gradient Noise with temporal jitter to break up the per-step banding.
float InlineContactShadow_IGN(float2 pixelPos, float frameId)
{
    pixelPos += frameId * float2(47.0, 17.0) * 0.695f;
    float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
    return frac(magic.z * frac(dot(pixelPos, magic.xy)));
}

// Returns 1.0 when unoccluded, 0.0 when an occluder is found along the ray to the light.
float InlineContactShadowRayMarch(float3 rayOriginWorld, float3 lightDir, float dither)
{
    int   numSteps  = max(InlineCS_NumSteps, 1);
    float rayLength = InlineCS_Length;

#if ENV_INLINE_CS_USE_HZB
    // HZB-accelerated hierarchical ray march (matches CastContactShadowRay HZB path).
    float3 biasedOrigin = rayOriginWorld + lightDir * InlineCS_DepthBias;
    FHzbRayCastResult hzbResult = HzbRayCastFromWorldRay(
        GHzbTexture, Samp_GHzbTexture,
        biasedOrigin, lightDir, rayLength,
        (uint)numSteps, dither, 0.0);
    return hzbResult.bHit ? 0.0 : 1.0;
#else
    float4 rayStartClip = mul(float4(rayOriginWorld, 1.0), GetViewPrjMtx());
    float3 rayEndWorld  = rayOriginWorld + lightDir * rayLength;
    float4 rayEndClip   = mul(float4(rayEndWorld, 1.0), GetViewPrjMtx());

    float3 rayStartNDC = rayStartClip.xyz / rayStartClip.w;
    float3 rayEndNDC   = rayEndClip.xyz / rayEndClip.w;

    float2 startUV = float2(rayStartNDC.x * 0.5 + 0.5, 0.5 - rayStartNDC.y * 0.5);
    float2 endUV   = float2(rayEndNDC.x * 0.5 + 0.5,   0.5 - rayEndNDC.y * 0.5);
    float  startZ  = rayStartNDC.z;
    float  endZ    = rayEndNDC.z;

    float2 rayUV = endUV - startUV;
    float  rayZ  = endZ  - startZ;

    float thicknessWorld = rayLength / (float)numSteps * 2.0;
    float stepSize = 1.0 / (float)numSteps;
    float t = stepSize * (dither + 0.5);

    [loop]
    for (int i = 0; i < numSteps; i++)
    {
        float2 sampleUV = startUV + rayUV * t;
        float  sampleZ  = startZ  + rayZ  * t;
        if (any(sampleUV < 0.0) || any(sampleUV > 1.0)) break;

        float sceneDepthNDC = DepthBuffer.SampleLevel(Samp_DepthBuffer, sampleUV, 0).r;
        float sceneLinear = LinearFromDepth(sceneDepthNDC);
        float rayLinear   = LinearFromDepth(sampleZ);
        float depthDiff   = rayLinear - sceneLinear;
        if (depthDiff > InlineCS_DepthBias && depthDiff < thicknessWorld) return 0.0;
        t += stepSize;
    }
    return 1.0;
#endif
}
#endif

#if ENV_ENABLE_SSAO == 1
Texture2D GSSAOTexture DX_AUTOBIND;
SamplerState Samp_GSSAOTexture DX_AUTOBIND;
#endif

TextureCube gEnvMap DX_AUTOBIND;
SamplerState Samp_gEnvMap DX_AUTOBIND;

Texture2D gPreIntegratedGF DX_AUTOBIND;
SamplerState Samp_gPreIntegratedGF DX_AUTOBIND;

Texture2D GVignette DX_AUTOBIND;
SamplerState Samp_GVignette DX_AUTOBIND;

#if ENV_LOCAL_LIGHTS == 1
cbuffer cbFrustumGrid DX_AUTOBIND
{
    int3    FrustumGridSize;
    uint    FrustumGridPixelSizeShift;
    float3  FrustumGridZParams;
    uint    FrustumGridTotalCells;
    uint    FrustumGridMaxPerCellPoint;
    uint    FrustumGridMaxPerCellSpot;
    float2  FrustumGridViewportSize;
};
#endif

struct FSubsurfaceProfile
{
    float3 ScatterColor;
    float ScatterRadius;
    float3 FalloffColor;
    float SubsurfaceOpacity;
};
StructuredBuffer<FSubsurfaceProfile> SubsurfaceProfiles DX_AUTOBIND;

#include "../../Bricks/FX/SubsurfaceLighting.cginc"

float GetDepth(float2 uv)
{
	return DepthBuffer.SampleLevel(Samp_DepthBuffer, uv, 0).r;
}

uint GetStencil(int2 pixelCoord)
{
	return StencilBuffer.Load(int3(pixelCoord, 0)).g;
}

float4	GetWorldPosition(float4 PosProj, float vDepth)
{
	// Position
	float4 VPos = PosProj;
	//VPos.xy /= VPos.ww;
	VPos.z = vDepth;
	//VPos.w = 1.0f;
	// Inverse ViewProjection Matrix
	VPos = mul(VPos, GetViewPrjMtxInverse());
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
	output.vLightMap.xy = SunPosNDC.xy - input.vPosition.xy;
	output.vLightMap.z = SunPosNDC.z;
	output.vLightMap.w = SunPosNDC.w;
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
#if ENV_ENABLE_SEPARATED_SPECULAR == 1
	float4 RT1 : SV_Target1;
#endif
};

struct FDeferredShadingContext
{
	half3 Albedo;
	half3 OptDiffShading;
	float3 OptSpecShading;
	half3 N;
	half3 V;
	half3 L;
	half NoL;
	half NoLsigned;
	half3 SkyShading;
	half3 DirLightDiffuseShading;
	half3 DirLightSpecShading;
	half3 EnvSpec;
	half AOs;
	half ShadowValue;
	half FinalShadowValue;
	half3 Cdir;
	half Idir;
	half Roughness;
	float3 WorldPos;
	half3 Csky;
	half3 Cground;
	half Ienv_light;
	FGBufferData GBuffer;
};

struct FDeferredShadingResult
{
	half3 BaseShading;
#if ENV_ENABLE_SEPARATED_SPECULAR == 1
	half3 SeparatedSpecular;
#endif
};

// ---- PBR (Default Lit) ----
// PBR never separates specular — diffuse+specular always merged into RT0.
FDeferredShadingResult DeferredDirLighting_PBR(FDeferredShadingContext ctx)
{
	FDeferredShadingResult result = (FDeferredShadingResult)0;
	result.BaseShading = ctx.DirLightDiffuseShading * ctx.FinalShadowValue
		+ ctx.DirLightSpecShading * ctx.ShadowValue
		+ ctx.SkyShading;
	result.BaseShading = result.BaseShading * ctx.AOs + ctx.EnvSpec;
	return result;
}

void AccumulateLocalLight_PBR(half3 lightShading, inout FDeferredShadingResult result)
{
	result.BaseShading += lightShading;
}

// ---- Subsurface ----
FDeferredShadingResult DeferredDirLighting_Subsurface(FDeferredShadingContext ctx)
{
	FDeferredShadingResult result = (FDeferredShadingResult)0;

	int profileIdx = (int)ctx.GBuffer.SubsurfaceProfileIndex;
	FSubsurfaceProfile sssProfile = SubsurfaceProfiles[profileIdx];

	FSubsurfaceLightingInput sssInput = (FSubsurfaceLightingInput)0;
	sssInput.Normal         = ctx.N;
	sssInput.ViewDir        = ctx.V;
	sssInput.LightDir       = ctx.L;
	sssInput.NoLsigned      = ctx.NoLsigned;
	sssInput.NoL            = ctx.NoL;
	sssInput.Albedo         = ctx.Albedo;
	sssInput.OptDiffShading = ctx.OptDiffShading;
	sssInput.LightColor     = ctx.Cdir;
	sssInput.LightIntensity = ctx.Idir;

	FSubsurfaceLightingResult sssResult = ComputeSubsurfaceDirLight(sssInput, sssProfile);

	ComposeSubsurfaceShading(
		sssResult.Diffuse, ctx.SkyShading,
		ctx.FinalShadowValue, ctx.ShadowValue,
		ctx.DirLightSpecShading, ctx.EnvSpec, ctx.AOs,
#if ENV_ENABLE_SEPARATED_SPECULAR == 1
		result.BaseShading, result.SeparatedSpecular);
#else
		result.BaseShading, result.BaseShading);
	// When not separated, both outputs go into BaseShading (additive).
#endif
	return result;
}

void AccumulateLocalLight_Subsurface(
	half3 diffuse, half3 specular,
	inout FDeferredShadingResult result)
{
#if ENV_ENABLE_SEPARATED_SPECULAR == 1
	result.BaseShading += diffuse;
	result.SeparatedSpecular += specular;
#else
	result.BaseShading += diffuse + specular;
#endif
}

// ---- Hair (Kajiya-Kay) ----
FDeferredShadingResult DeferredDirLighting_Hair(FDeferredShadingContext ctx)
{
	FDeferredShadingResult result = (FDeferredShadingResult)0;

	half3 T = ctx.GBuffer.WorldTangent;
	half3 N = ctx.N; // Geometric normal (decoded from GBuffer rt0.a + rt2.r for Hair)

	// Hair F0: use material's Specular parameter as base reflectance,
	// tinted by albedo for colored hair highlights.
	half baseF0 = max((half)ctx.GBuffer.Specular, 0.04h);
	half3 specularColor = lerp(half3(baseF0, baseF0, baseF0), ctx.Albedo, 0.3h);

	// Per-pixel shift from GBuffer.ShiftOffset (stored in rt1.b when USE_OCTAHEDRON_NORMAL, 10-bit)
	half shiftOffset = ctx.GBuffer.ShiftOffset;

	half3 hairDiffuse, hairSpecular;
	HairShadingSeparated(T, ctx.L, ctx.V, N,
		ctx.Albedo, specularColor, ctx.Roughness,
		shiftOffset, ctx.Cdir * ctx.Idir,
		hairDiffuse, hairSpecular);

	result.BaseShading = hairDiffuse * ctx.FinalShadowValue
		+ hairSpecular * ctx.ShadowValue
		+ ctx.SkyShading;
	result.BaseShading = result.BaseShading * ctx.AOs + ctx.EnvSpec;
	return result;
}

void AccumulateLocalLight_Hair(half3 lightShading, inout FDeferredShadingResult result)
{
	result.BaseShading += lightShading;
}

void AccumulateLocalLights(
	float3 WorldPos, half3 V, half3 N,
	half3 OptDiffShading, float3 OptSpecShading, half Roughness,
	float2 uv, float rtDepth, int shadingMode,
	inout FDeferredShadingResult shadingResult)
{
	float linearDepth = LinearFromDepth(rtDepth);
	uint2 pixelPos = (uint2)(uv * ViewportSizeAndRcp.xy);
	uint cellIndex = ComputeFrustumGridCellIndexFromPixel(pixelPos, linearDepth,
		FrustumGridPixelSizeShift, FrustumGridZParams, FrustumGridSize);

	// Point lights
	FFrustumGridCellHeader header = PointGridHeaders[cellIndex];
	uint numLights = min(header.Count, FrustumGridMaxPerCellPoint);
	for (uint i = 0; i < numLights; i++)
	{
		uint lightIndex = PointGridDataIndices[header.DataStartOffset + i];
		FPointLight light = GpuScene_PointLights[lightIndex];
		half3 diffuse, specular;
		PointLightShadingSeparated(light, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness, diffuse, specular);
		switch (shadingMode)
		{
			case EShadingMode_Subsurface:
				AccumulateLocalLight_Subsurface(diffuse, specular, shadingResult);
				break;
			case EShadingMode_Hair:
				AccumulateLocalLight_Hair(diffuse + specular, shadingResult);
				break;
			case EShadingMode_PBR:
			default:
				AccumulateLocalLight_PBR(diffuse + specular, shadingResult);
				break;
		}
	}

	// Spot lights
	FFrustumGridCellHeader spotHeader = SpotGridHeaders[cellIndex];
	uint numSpotLights = min(spotHeader.Count, FrustumGridMaxPerCellSpot);
	for (uint si = 0; si < numSpotLights; si++)
	{
		uint spotIndex = SpotGridDataIndices[spotHeader.DataStartOffset + si];
		FSpotLight spotLight = GpuScene_SpotLights[spotIndex];
		half3 diffuse, specular;
		SpotLightShadingSeparated(spotLight, WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness, diffuse, specular);
		switch (shadingMode)
		{
			case EShadingMode_Subsurface:
				AccumulateLocalLight_Subsurface(diffuse, specular, shadingResult);
				break;
			case EShadingMode_Hair:
				AccumulateLocalLight_Hair(diffuse + specular, shadingResult);
				break;
			case EShadingMode_PBR:
			default:
				AccumulateLocalLight_PBR(diffuse + specular, shadingResult);
				break;
		}
	}
}

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	float2 uv = input.vUV.xy;

	half4 rt0 = (half4)GBufferRT0.SampleLevel(Samp_GBufferRT0, uv.xy, 0);
	half4 rt1 = (half4)GBufferRT1.SampleLevel(Samp_GBufferRT1, uv.xy, 0);
	half4 rt2 = (half4)GBufferRT2.SampleLevel(Samp_GBufferRT2, uv.xy, 0);
	half4 rt3 = (half4)GBufferRT3.SampleLevel(Samp_GBufferRT3, uv.xy, 0);
	float rtDepth = GetDepth(uv.xy);

	FGBufferData GBuffer = (FGBufferData)0;
	GBuffer.DecodeGBuffer(rt0, rt1, rt2, rt3);
	
	//bool NoPixel = (dot(GBuffer.WorldNormal, GBuffer.WorldNormal) < 0.01f);
    bool NoPixel = false;
	
	half3 Albedo = sRGB2Linear((half3)GBuffer.MtlColorRaw);
	half AbsSpecular = GBuffer.Specular;
	//ͨ�� 0.04���� 4% �����ʣ���Ӧ�����
    //AbsSpecular = max(AbsSpecular, 0.04h); //ĳЩ���ʣ���ʯ��ˮ�桢��ʯ�ȣ�F0 ���Դﵽ 0.08~0.17

	float3 WorldPos = GetWorldPositionFromDepthValue(uv, rtDepth).xyz;
	half3 L = -(half3)normalize(DirLight.Direction.xyz);
	half3 V = (half3)normalize(CameraPosition - WorldPos);

	half3 N = GBuffer.WorldNormal;
	half Metallic = (half)GBuffer.Metallicity;
	half Roughness = GetRoughness((half)GBuffer.Roughness, N);
    half AOs = GBuffer.AO;
#if ENV_ENABLE_SSAO == 1
    half ssaoValue = (half)GSSAOTexture.SampleLevel(Samp_GSSAOTexture, uv, 0).r;
    AOs = min(AOs, ssaoValue);
#endif

	half3 BaseShading = half3(0.0h, 0.0h, 0.0h);
    half3 Cdir = (half3) DirLight.SunLightColor.rgb;
    half Idir = (half) DirLight.SunLightIntensity;
	half Ienv_light = Idir * 0.2h;
	half3 Csky = (half3)DirLight.SkyLightColor;
	half3 Cground = (half3)DirLight.GroundLightColor;
    half DirLightLeak = (half) DirLight.SunLightLeak;

	//shadow;
	half ShadowValue = 1.0h;
	
	float4 ShadowMapUV = float4(0.0f, 0.0f, 0.0f, 0.0f);
	//half PerPixelViewerDistance = (half)input.vPosition.w;
	half PerPixelViewerDistance = (half)LinearFromDepth(rtDepth);
	
	//测试读取StencilBuffer成功了，后续可以用它来存一些数据
    //uint stencil = GetStencil(int2(uv * ViewportSizeAndRcp.xy));
    //if (stencil != 0)
    //{
    //    Csky.r += 0.1h;
    //}

    output.RT0.a = 1.0h;
#if ENV_EShadowMode == EShadowMode_Csm
	ShadowFilterData mSFD;
	mSFD.mShadowMap = GShadowMap;
	mSFD.mShadowMapSampler = Samp_GShadowMap;
	mSFD.mShadowMapSizeAndRcp = ShadowMapSizeAndRcp;
	mSFD.mShadowTransitionScale = (half)ShadowTransitionScale;
	
	if (PerPixelViewerDistance > ShadowDistance || GBuffer.IsAcceptShadow() == false)
	{
		ShadowValue = 1.0h;
	}
	else
	{
        float clip_u_min = 0;
        float clip_u_max = 0.25;
		for (int CsmIdx = 0; CsmIdx < CsmNum; CsmIdx++)
		{
			if (PerPixelViewerDistance < (half)CsmDistanceArray[CsmIdx])
			{
				ShadowMapUV = mul(float4(WorldPos, 1.0f), Viewer2ShadowMtxArray[CsmIdx]);
                ShadowMapUV.z = ShadowMapUV.z / ShadowMapUV.w;
				mSFD.mShadowTransitionScale = (half)ShadowTransitionScaleArray[CsmIdx];
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
			
			ShadowValue = DoPCF4x4(ShadowMapUV.xy, mSFD);
			//ShadowValue = NoFiltering(ShadowMapUV.xy, mSFD);
			
			half FadeValue = (half)saturate(PerPixelViewerDistance * FadeParam.x + FadeParam.y);
			ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
		}
		
		if (ShadowValue < 1.0f)
			output.RT0.a = 0.0h;

		half FadeValue = (half)saturate(PerPixelViewerDistance * FadeParam.x + FadeParam.y);		
		ShadowValue = lerp(ShadowValue, 1.0h, FadeValue);
	}
#elif ENV_EShadowMode == EShadowMode_Advance
    if (GBuffer.IsAcceptShadow() == false)
    {
        ShadowValue = 1.0h;
    }
    else
    {
        // Try Clipmap first (directional light, near camera)
        float clipmapShadow = GetClipmapShadow(WorldPos);
        if (clipmapShadow < 1.0f || GetClipmapLevel(WorldPos.xz) >= 0)
        {
            // Clipmap handled this pixel
            ShadowValue = (half)clipmapShadow;
        }
        else
        {
            // Fallback to QTree (local lights / outside clipmap range)
            int nodeIndex = GetPageNode(WorldPos.xz);
            if (nodeIndex < 0)
            {
                ShadowValue = 1.0h;
            }
            else
            {
                FAdvShadowNodeData node = QTreeNodeBuffer[nodeIndex];
                if (node.PageIndex < 0)
                {
                    ShadowValue = 1.0h;
                }
                else
                {
                    ShadowMapUV = mul(float4(WorldPos, 1.0f), node.ShadowMatrix);
                    ShadowMapUV.z = ShadowMapUV.z / ShadowMapUV.w;
                    if (ShadowMapUV.z > 1)
                    {
                        ShadowValue = 1.0h;
                    }
                    else
                    {
                        ShadowMapUV.z = max(ShadowMapUV.z, 0);
                        float occluderDepth = GShadowMapArray.SampleLevel(Samp_GShadowMap, float3(ShadowMapUV.xy, node.PageIndex), 0).r;
                        float esmValue = GetESMValue((ShadowMapUV.z - 0.003), node.ZNear, node.ZFar);
                        ShadowValue = saturate(occluderDepth / esmValue);
                    }
                }
            }
        }
    }
#elif ENV_EShadowMode == EShadowMode_None
	ShadowValue = 1.0h;
#endif

#if ENV_ContactShadowMode == EContactShadowMode_InputNode
	// Contact Shadow: 5-tap cross filter to soften dither noise
	{
		float2 texelSize = ViewportSizeAndRcp.zw;
		float contactShadow = GContactShadow.SampleLevel(Samp_GContactShadow, uv, 0).r;
		contactShadow += GContactShadow.SampleLevel(Samp_GContactShadow, uv + float2( texelSize.x, 0), 0).r;
		contactShadow += GContactShadow.SampleLevel(Samp_GContactShadow, uv + float2(-texelSize.x, 0), 0).r;
		contactShadow += GContactShadow.SampleLevel(Samp_GContactShadow, uv + float2(0,  texelSize.y), 0).r;
		contactShadow += GContactShadow.SampleLevel(Samp_GContactShadow, uv + float2(0, -texelSize.y), 0).r;
		contactShadow *= 0.2;
		ShadowValue = min(ShadowValue, (half)contactShadow);
	}
#elif ENV_ContactShadowMode == EContactShadowMode_Inline
	if (GBuffer.IsAcceptShadow())
	{
		float csLinDepth = LinearFromDepth(rtDepth);
		float csFade = saturate((csLinDepth - InlineCS_FadeDistance) / max(InlineCS_FadeLength, 0.001));
		if (csFade < 1.0)
		{
			uint2 csPixel = (uint2)(uv * ViewportSizeAndRcp.xy);
			float csDither = InlineContactShadow_IGN((float2)csPixel + 0.5, (float)InlineCS_FrameIndex);
			float csFactor = InlineContactShadowRayMarch(WorldPos, (float3)L, csDither);
			float csResult = lerp(1.0 - InlineCS_Intensity, 1.0, csFactor);
			csResult = lerp(csResult, 1.0, csFade);
			ShadowValue = min(ShadowValue, (half)csResult);
		}
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
	//todo:��ǰ��SkyShading��Diffuse,Specularһ���AO���������֣����ǵ��������ױ�AOӰ�죬�������Կ��ǵ�����AO*AO����ʹ�ò�ͬ��AOֵ������SkyShading������
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
    half EnvMipLevel = GetTexMipLevelFromRoughness(Roughness, (half) EnvMapMaxMipLevel);
    half3 EnvSpecLightColor = (half3) gEnvMap.SampleLevel(Samp_gEnvMap, R, EnvMipLevel).rgb;
	float RoughnessSq = GBuffer.Roughness * GBuffer.Roughness;
	float SpecularOcclusion = GetSpecularOcclusion(NoV, RoughnessSq, AOs);
	half3 EnvSpec = (half3)EnvBRDF(EnvSpecLightColor, OptSpecShading, Roughness, NoV, gPreIntegratedGF, Samp_gPreIntegratedGF) * SpecularOcclusion;
	EnvSpec = -min(-EnvSpec.rgb, 0.0);

	half FinalShadowValue = min(1.0h, ShadowValue + DirLightLeak);

	// Build shading context
	FDeferredShadingContext shadingCtx = (FDeferredShadingContext)0;
	shadingCtx.Albedo               = Albedo;
	shadingCtx.OptDiffShading       = OptDiffShading;
	shadingCtx.OptSpecShading       = OptSpecShading;
	shadingCtx.N                    = N;
	shadingCtx.V                    = V;
	shadingCtx.L                    = L;
	shadingCtx.NoL                  = NoL;
	shadingCtx.NoLsigned            = NoLsigned;
	shadingCtx.SkyShading           = SkyShading;
	shadingCtx.DirLightDiffuseShading = DirLightDiffuseShading;
	shadingCtx.DirLightSpecShading  = DirLightSpecShading;
	shadingCtx.EnvSpec              = EnvSpec;
	shadingCtx.AOs                  = AOs;
	shadingCtx.ShadowValue          = ShadowValue;
	shadingCtx.FinalShadowValue     = FinalShadowValue;
	shadingCtx.Cdir                 = Cdir;
	shadingCtx.Idir                 = Idir;
	shadingCtx.Roughness            = Roughness;
	shadingCtx.WorldPos             = WorldPos;
	shadingCtx.Csky                 = Csky;
	shadingCtx.Cground              = Cground;
	shadingCtx.Ienv_light           = Ienv_light;
	shadingCtx.GBuffer              = GBuffer;

	// Directional light shading dispatch
	int shadingMode = GBuffer.GetShadingMode();
	FDeferredShadingResult shadingResult = (FDeferredShadingResult)0;
	switch (shadingMode)
	{
        case EShadingMode_Unlit:
            shadingResult.BaseShading = Albedo * FinalShadowValue;
            break;
		case EShadingMode_Subsurface:
			shadingResult = DeferredDirLighting_Subsurface(shadingCtx);
			break;
		case EShadingMode_Hair:
			shadingResult = DeferredDirLighting_Hair(shadingCtx);
			break;
		case EShadingMode_PBR:
		default:
			shadingResult = DeferredDirLighting_PBR(shadingCtx);
			break;
	}

	// Local lights
#if ENV_LOCAL_LIGHTS == 1
	if (NoPixel == false)
	{
		AccumulateLocalLights(WorldPos, V, N, OptDiffShading, OptSpecShading, Roughness,
			uv, rtDepth, shadingMode, shadingResult);
	}
#endif

	// Output
    half3 Color = shadingResult.BaseShading;
	output.RT0.rgb = Linear2sRGB(Color);

#if ENV_ENABLE_SEPARATED_SPECULAR == 1
	output.RT1.rgb = Linear2sRGB(shadingResult.SeparatedSpecular);
	output.RT1.a = 1.0h;
#endif
	
	return output;
}

#endif//#ifndef _DEFERRED_LIGHTING_H_