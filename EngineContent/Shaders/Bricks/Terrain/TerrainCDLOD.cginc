#ifndef _TerrainCDLOD_cginc_
#define _TerrainCDLOD_cginc_

//#define FEATURE_USE_RVT

#include "Common.cginc"

//Texture2D HeightMapTextureDX_AUTOBIND;
//SamplerState Samp_HeightMapTextureDX_AUTOBIND;
#if defined(FEATURE_USE_RVT)
Texture2DArray	NormalMapTexture DX_AUTOBIND;
ByteAddressBuffer TextureSlotBuffer DX_AUTOBIND;
#else
Texture2D NormalMapTexture DX_AUTOBIND;
#endif
SamplerState Samp_NormalMapTexture DX_AUTOBIND;

float3 GetTerrrainVertexNormal(float2 uv, int uniqueTextureId)
{
#if defined(FEATURE_USE_RVT)
    uint arrayIndex = TextureSlotBuffer.Load((uniqueTextureId & 0xffff) * 4);
	return NormalMapTexture.SampleLevel(Samp_NormalMapTexture, float3(uv.xy, arrayIndex), 0).xyz;
#else
    return NormalMapTexture.SampleLevel(Samp_NormalMapTexture, uv.xy, 0).xyz;
 #endif
}

Texture2D		MaterialIdTexture DX_AUTOBIND;
SamplerState	Samp_MaterialIdTexture DX_AUTOBIND;
Texture2DArray	DiffuseTextureArray DX_AUTOBIND;
SamplerState	Samp_DiffuseTextureArray DX_AUTOBIND;
Texture2DArray	NormalTextureArray DX_AUTOBIND;
SamplerState	Samp_NormalTextureArray DX_AUTOBIND;

float2 UV_PatchToLevel(float2 uv)
{
	return uv * TexUVScale + TexUVOffset;
}

float2 UV_LevelToPatch(float2 uv)
{
	return (uv - TexUVOffset) / TexUVScale;
}

float4 GetMaterialId(float2 uv)
{
	return MaterialIdTexture.SampleLevel(Samp_MaterialIdTexture, uv.xy, 0);
}

float3 GetTerrainDiffuse(float2 uvOrig, PS_INPUT input)
{
	/*float2 uvLevel = UV_PatchToLevel(uvOrig); 
	float2 remain = fmod(uvLevel, MaterialIdUVStep);
	remain = saturate(remain / (MaterialIdUVStep));*/
	float2 uvLevel = input.vLightMap.xy;
	float2 remain = fmod(uvLevel, MaterialIdUVStep);
	remain = saturate(remain / (MaterialIdUVStep));

	uint i0_0 = (uint)(GetMaterialId(input.vLightMap.xy).r * 255.0f + 0.1f);
	uint i1_0 = (uint)(GetMaterialId(input.vLightMap.xy + float2(MaterialIdUVStep, 0)).r * 255.0f + 0.1f);
	uint i1_1 = (uint)(GetMaterialId(input.vLightMap.xy + float2(MaterialIdUVStep, MaterialIdUVStep)).r * 255.0f + 0.1f);
	uint i0_1 = (uint)(GetMaterialId(input.vLightMap.xy + float2(0, MaterialIdUVStep)).r * 255.0f + 0.1f);
	
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

	uint i0_0 = (uint)(GetMaterialId(input.vLightMap.xy).r * 255.0f + 0.1f);
	uint i1_0 = (uint)(GetMaterialId(input.vLightMap.xy + float2(MaterialIdUVStep, 0)).r * 255.0f + 0.1f);
	uint i1_1 = (uint)(GetMaterialId(input.vLightMap.xy + float2(MaterialIdUVStep, MaterialIdUVStep)).r * 255.0f + 0.1f);
	uint i0_1 = (uint)(GetMaterialId(input.vLightMap.xy + float2(0, MaterialIdUVStep)).r * 255.0f + 0.1f);

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

float2 GetGlobalUV(float2 vPosInWorld)
{
	float2 GlobalUV = (vPosInWorld.xy - StartPosition.xz) / PatchSize;
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
    uint heighmapID = 0;
    uint normalmapID = 0;
    float3 pos = GetTerrrainVertexPosition(uv, heighmapID);

	float3 eyePos = EyeCenter;// - StartPosition;
	float eyeDist = distance(pos.xyz, eyePos.xyz);

	LODLayer layer = MorphLODs[CurrentLOD];
	float morphLerpK = 1.0f - clamp(layer.MorphEndDivRange - eyeDist * layer.MorphRcqRange, 0.0, 1.0);
	//morphLerpK = 1.0f;

	float2 final_pos = MorphVertex(uv, pos.xz, morphLerpK, layer);
	vsOut.vPosition.xz = final_pos.xy;

	float2 heightUV = uv + (final_pos - pos.xz) / PatchSize;
	heightUV = heightUV * TexUVScale;
	heightUV += TexUVOffset.xy;

	//float2 heightUV = pos.xz * TexUVScale;
	//heightUV += TexUVOffset.xy;
	//heightUV = final_pos.xy / 1024.0f;
    vsOut.vPosition.y = GetTerrrainVertexHeight(heightUV.xy, heighmapID);
	//vsOut.vPosition.y += StartPosition.y;
	vsOut.vPosition.xyz += StartPosition;
#if USE_PS_WorldPos == 1
	vsOut.vWorldPos = vsOut.vPosition.xyz;
#endif
	
#if USE_PS_Normal == 1
	vsOut.vNormal = GetTerrrainVertexNormal(heightUV.xy, normalmapID);
	vsOut.vNormal = normalize(vsOut.vNormal * 2.0f - float3(1.0f, 1.0f, 1.0f));
	//vsOut.vNormal.xy = heightUV.xy;
	//vsOut.vTangent.xyz = normalize(mul(float4(vertexData.Tangent.xyz, 0), instData.Matrix).xyz);
#endif

	vsOut.vUV = GetGlobalUV(final_pos.xy);
	//vsOut.vUV = uv;

	vert.vPosition = vsOut.vPosition.xyz;
	vert.vNormal = vsOut.vNormal;
	vert.vUV = vsOut.vUV;
	
	//vert.vLightMap.xy = heightUV.xy;
#if USE_PS_LightMap == 1
	vsOut.vLightMap.xy = heightUV.xy;
#endif

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

#endif //_TerrainCDLOD_cginc_