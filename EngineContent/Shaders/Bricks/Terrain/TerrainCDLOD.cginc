#ifndef _TerrainCDLOD_cginc_
#define _TerrainCDLOD_cginc_

//#define FEATURE_USE_RVT

#include "Common.cginc"
#include "../VirtualTexture/RealtimeVT.cginc"

#if defined(FEATURE_USE_RVT)
Texture2DArray	HeightMapTexture DX_AUTOBIND;
Texture2DArray	NormalMapTexture DX_AUTOBIND;
Texture2DArray	MaterialIdTexture DX_AUTOBIND;
#else
Texture2D HeightMapTexture DX_AUTOBIND;
Texture2D NormalMapTexture DX_AUTOBIND;
Texture2D MaterialIdTexture DX_AUTOBIND;
#endif

SamplerState Samp_HeightMapTexture DX_AUTOBIND;
SamplerState Samp_NormalMapTexture DX_AUTOBIND;
SamplerState Samp_MaterialIdTexture DX_AUTOBIND;

//instance for cbuffer
float3 GetStartPosition(VS_MODIFIER input)
{
    //return StartPosition;
#if defined(FEATURE_USE_RVT)
    VSInstanceData result = GetInstanceData(input);
    return asfloat(result.UserData2.xyz);
#else
    return StartPosition;
#endif
}

uint GetCurrentLOD(VS_MODIFIER input)
{
    //return CurrentLOD;
#if defined(FEATURE_USE_RVT)
    VSInstanceData result = GetInstanceData(input);
    return result.UserData.w;
#else
    return CurrentLOD;
#endif
}

float2 GetTexUVOffset(VS_MODIFIER input)
{
#if defined(FEATURE_USE_RVT)
	VSInstanceData result = GetInstanceData(input);
    return float2(asfloat(result.UserData2.w),asfloat(result.Scale_Pad));
#else
    return TexUVOffset;
#endif	
}
//instance for cbuffer

uint GetHeightmapTextureId(VS_MODIFIER input)
{
#if defined(FEATURE_USE_RVT)
    VSInstanceData result = GetInstanceData(input);
    return result.UserData.x;
#else
    return 0;
#endif	
}

uint GetNormalmapTextureId(VS_MODIFIER input)
{
#if defined(FEATURE_USE_RVT)
    VSInstanceData result = GetInstanceData(input);
    return result.UserData.y;
#else
    return 0;
#endif
}

uint GetMaterailIdTextureId(VS_MODIFIER input)
{
#if defined(FEATURE_USE_RVT)
    VSInstanceData result = GetInstanceData(input);
    return result.UserData.z;
#else
    return 0;
#endif
}

float GetTerrrainVertexHeight(float2 uv, int uniqueTextureId = 0)
{
    return SampleLevelRVT(Samp_HeightMapTexture, HeightMapTexture, uv, uniqueTextureId, 0).r;
}

float3 GetTerrrainVertexPosition(float2 uv, int uniqueTextureId = 0)
{
    float3 result = float3(0, 0, 0);
    result.xz = uv * PatchSize;
    float2 heightUV = uv * TexUVScale;
    heightUV += TexUVOffset.xy;
    result.y = GetTerrrainVertexHeight(heightUV, uniqueTextureId);
    return result;
}

float3 GetTerrrainVertexNormal(float2 uv, int uniqueTextureId)
{
    return SampleLevelRVT(Samp_NormalMapTexture, NormalMapTexture, uv, uniqueTextureId, 0).xyz;
}

float4 GetMaterialId(float2 uv, int uniqueTextureId)
{
#if defined(FEATURE_USE_RVT)
	return MaterialIdTexture.SampleLevel(Samp_MaterialIdTexture, float3(uv.xy, uniqueTextureId), 0);
#else
    return MaterialIdTexture.SampleLevel(Samp_MaterialIdTexture, uv.xy, 0);
#endif
}

Texture2DArray	DiffuseTextureArray DX_AUTOBIND;
SamplerState	Samp_DiffuseTextureArray DX_AUTOBIND;
Texture2DArray	NormalTextureArray DX_AUTOBIND;
SamplerState	Samp_NormalTextureArray DX_AUTOBIND;

float2 UV_PatchToLevel(float2 uv, VS_MODIFIER input)
{
    return uv * TexUVScale + GetTexUVOffset(input);
}

float2 UV_LevelToPatch(float2 uv, VS_MODIFIER input)
{
    return (uv - GetTexUVOffset(input)) / TexUVScale;
}

float3 GetTerrainDiffuse(float2 uvOrig, PS_INPUT input)
{
	/*float2 uvLevel = UV_PatchToLevel(uvOrig); 
	float2 remain = fmod(uvLevel, MaterialIdUVStep);
	remain = saturate(remain / (MaterialIdUVStep));*/
	float2 uvLevel = input.vLightMap.xy;
	float2 remain = fmod(uvLevel, MaterialIdUVStep);
	remain = saturate(remain / (MaterialIdUVStep));
	
    uint materailUniqueID = input.SpecialData.y;
    uint i0_0 = (uint) (GetMaterialId(input.vLightMap.xy, materailUniqueID).r * 255.0f + 0.1f);
    uint i1_0 = (uint) (GetMaterialId(input.vLightMap.xy + float2(MaterialIdUVStep, 0), materailUniqueID).r * 255.0f + 0.1f);
    uint i1_1 = (uint) (GetMaterialId(input.vLightMap.xy + float2(MaterialIdUVStep, MaterialIdUVStep), materailUniqueID).r * 255.0f + 0.1f);
    uint i0_1 = (uint) (GetMaterialId(input.vLightMap.xy + float2(0, MaterialIdUVStep), materailUniqueID).r * 255.0f + 0.1f);
	
	float3 clr0_0 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uvOrig.xy, i0_0)).rgb;
	float3 clr1_0 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uvOrig.xy, i1_0)).rgb;
	float3 clr1_1 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uvOrig.xy, i1_1)).rgb;
	float3 clr0_1 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uvOrig.xy, i0_1)).rgb;

	/*float3 clr0_0 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy, input.SpecialData.x)).rgb;
	float3 clr1_0 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy + float2(DiffuseUVStep, 0), input.SpecialData.y)).rgb;
	float3 clr1_1 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy + float2(DiffuseUVStep, DiffuseUVStep), input.SpecialData.z)).rgb;
	float3 clr0_1 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy + float2(0, DiffuseUVStep), input.SpecialData.w)).rgb;*/

	float3 t1 = lerp(clr0_0, clr1_0, remain.x);
	float3 t2 = lerp(clr0_1, clr1_1, remain.x);

	return lerp(t1, t2, remain.y);
}
#define Def_GetTerrainDiffuse

float3 GetTerrainNormal(float2 uvOrig, PS_INPUT input)
{
	/*float2 uvLevel = UV_PatchToLevel(uvOrig);
	float2 remain = fmod(uvLevel, MaterialIdUVStep);
	remain = saturate(remain / (MaterialIdUVStep));*/
	float2 uvLevel = input.vLightMap.xy;
	float2 remain = fmod(uvLevel, MaterialIdUVStep);
	remain = saturate(remain / (MaterialIdUVStep));

    uint materailUniqueID = input.SpecialData.y;
    uint i0_0 = (uint) (GetMaterialId(input.vLightMap.xy, materailUniqueID).r * 255.0f + 0.1f);
    uint i1_0 = (uint) (GetMaterialId(input.vLightMap.xy + float2(MaterialIdUVStep, 0), materailUniqueID).r * 255.0f + 0.1f);
    uint i1_1 = (uint) (GetMaterialId(input.vLightMap.xy + float2(MaterialIdUVStep, MaterialIdUVStep), materailUniqueID).r * 255.0f + 0.1f);
    uint i0_1 = (uint) (GetMaterialId(input.vLightMap.xy + float2(0, MaterialIdUVStep), materailUniqueID).r * 255.0f + 0.1f);

	float3 clr0_0 = NormalTextureArray.Sample(Samp_NormalTextureArray, float3(uvOrig.xy, i0_0)).rgb;
	float3 clr1_0 = NormalTextureArray.Sample(Samp_NormalTextureArray, float3(uvOrig.xy, i1_0)).rgb;
	float3 clr1_1 = NormalTextureArray.Sample(Samp_NormalTextureArray, float3(uvOrig.xy, i1_1)).rgb;
	float3 clr0_1 = NormalTextureArray.Sample(Samp_NormalTextureArray, float3(uvOrig.xy, i0_1)).rgb;

	/*float3 clr0_0 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy, input.SpecialData.x)).rgb;
	float3 clr1_0 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy + float2(DiffuseUVStep, 0), input.SpecialData.y)).rgb;
	float3 clr1_1 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy + float2(DiffuseUVStep, DiffuseUVStep), input.SpecialData.z)).rgb;
	float3 clr0_1 = DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy + float2(0, DiffuseUVStep), input.SpecialData.w)).rgb;*/

	float3 t1 = lerp(clr0_0, clr1_0, remain.x);
	float3 t2 = lerp(clr0_1, clr1_1, remain.x);

	return normalize(lerp(t1, t2, remain.y));
}
#define Def_GetTerrainNormal

float2 GetGlobalUV(float2 vPosInWorld, VS_MODIFIER vert)
{
    float2 GlobalUV = (vPosInWorld.xy - GetStartPosition(vert).xz) / PatchSize;
	return GlobalUV;
}


float2 MorphVertex(float2 inPos, float2 vertex, float morphLerpK, LODLayer layer)
{
	float2 fracPart = (frac(inPos.xy * float2(layer.HalfDim, layer.HalfDim)) * float2(layer.TwoRcpDim, layer.TwoRcpDim)) * PatchSize;
	return vertex.xy + fracPart * morphLerpK;
}

void DoTerrainModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	float2 uv = vert.vPosition.xz;
	half3 nor = half3(0,1,0);
	
	//todo: heighmapID & normalmapID from instance data
    uint heighmapID = GetHeightmapTextureId(vert);
    uint normalmapID = GetNormalmapTextureId(vert);
    uint materialIdID = GetMaterailIdTextureId(vert);
	
    float3 pos = GetTerrrainVertexPosition(uv, heighmapID);

    float3 eyePos = EyeCenter - GetStartPosition(vert);
	float eyeDist = distance(pos.xyz, eyePos.xyz);

    LODLayer layer = MorphLODs[GetCurrentLOD(vert)];
	float morphLerpK = 1.0f - clamp(layer.MorphEndDivRange - eyeDist * layer.MorphRcqRange, 0.0, 1.0);
	//morphLerpK = 1.0f;

	float2 final_pos = MorphVertex(uv, pos.xz, morphLerpK, layer);
	vsOut.vPosition.xz = final_pos.xy;

	float2 heightUV = uv + (final_pos - pos.xz) / PatchSize;
	heightUV = heightUV * TexUVScale;
    heightUV += GetTexUVOffset(vert).xy;

	//float2 heightUV = pos.xz * TexUVScale;
	//heightUV += GetTexUVOffset(vert).xy;
	//heightUV = final_pos.xy / 1024.0f;
    vsOut.vPosition.y = GetTerrrainVertexHeight(heightUV.xy, heighmapID);
	//vsOut.vPosition.y += GetStartPosition(vert).y;
    vsOut.vPosition.xyz += GetStartPosition(vert);
#if USE_PS_WorldPos == 1
	vsOut.vWorldPos = vsOut.vPosition.xyz;
#endif
	
#if USE_PS_Normal == 1
	vsOut.vNormal = GetTerrrainVertexNormal(heightUV.xy, normalmapID);
	vsOut.vNormal = normalize(vsOut.vNormal * 2.0f - float3(1.0f, 1.0f, 1.0f));
	//vsOut.vNormal.xy = heightUV.xy;
	//vsOut.vTangent.xyz = normalize(mul(float4(vertexData.Tangent.xyz, 0), instData.Matrix).xyz);
#endif

    vsOut.vUV = GetGlobalUV(final_pos.xy, vert);
	//vsOut.vUV = uv;

	vert.vPosition = vsOut.vPosition.xyz;
	vert.vNormal = vsOut.vNormal;
	vert.vUV = vsOut.vUV;
	
	//vert.vLightMap.xy = heightUV.xy;
#if USE_PS_LightMap == 1
	vsOut.vLightMap.xy = heightUV.xy;
#endif

    vsOut.SetSpecialDataY(materialIdID);
	
	/*uint v0_0 = (uint)(GetMaterialId(heightUV.xy).r * 255.0f + 0.1f);
	uint v1_0 = (uint)(GetMaterialId(heightUV.xy + float2(MaterialIdUVStep, 0)).r * 255.0f + 0.1f);
	uint v1_1 = (uint)(GetMaterialId(heightUV.xy + float2(MaterialIdUVStep, MaterialIdUVStep)).r * 255.0f + 0.1f);
	uint v0_1 = (uint)(GetMaterialId(heightUV.xy + float2(0, MaterialIdUVStep)).r * 255.0f + 0.1f);
	vsOut.SpecialData.x = v0_0;
	vsOut.SpecialData.y = v1_0;
	vsOut.SpecialData.z = v1_1;
	vsOut.SpecialData.w = v0_1;*/
}

#define VS_NO_WorldTransform
//#define MDFQUEUE_FUNCTION_PS

//#define INSTANCE_NONE_POS
//#define INSTANCE_NONE_QUAT
//#define INSTANCE_NONE_SCALE

#endif //_TerrainCDLOD_cginc_