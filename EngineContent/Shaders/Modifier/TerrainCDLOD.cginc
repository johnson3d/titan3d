
struct LODLayer
{
	float MorphStart;
	float MorphRcqRange;
	float MorphEndDivRange;
	float MorphEnd;

	int Dimension;
	float HalfDim;
	float TwoRcpDim;
	float LODPad0;
};

cbuffer cbPerTerrain
{
	float GridSize;
	float PatchSize;
	float TexUVScale;
	LODLayer MorphLODs[10];
};

cbuffer cbPerPatch
{
	float3 StartPosition;
	int CurrentLOD;

	float3 EyeCenter;

	float2 TexUVOffset;
};

Texture2D		HeightMapTexture DX_NOBIND;
SamplerState	Samp_HeightMapTexture DX_NOBIND;

//test code
Texture2DArray	HeightMapTextureArray DX_NOBIND;
Texture2D		ArrayTextures[3];
//test code

Texture2D		NormalMapTexture DX_NOBIND;
SamplerState	Samp_NormalMapTexture DX_NOBIND;

Texture2D		MaterialIdTexture DX_NOBIND;
SamplerState	Samp_MaterialIdTexture DX_NOBIND;

Texture2DArray	DiffuseTextureArray DX_NOBIND;
SamplerState	Samp_DiffuseTextureArray DX_NOBIND;

float3 GetTerrainDiffuse(float2 uv, PS_INPUT input)
{
	return DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy, input.SpecialData.x)).rgb;
	//return DiffuseTextureArray.Sample(Samp_DiffuseTextureArray, float3(uv.xy, 2)).rgb;
	//return float3(1,1,1);
}
#define Def_GetTerrainDiffuse

float3 GetPosition(float2 uv)
{
	float3 result = float3(0,0,0);
	result.xz = uv * PatchSize;
	float2 heightUV = uv * TexUVScale;
	heightUV += TexUVOffset.xy;
	result.y = HeightMapTexture.SampleLevel(Samp_HeightMapTexture, heightUV.xy, 0).r;

	//test code
	//FRVTArray rvt = (FRVTArray)0;
	//float3 myCoord = float3(heightUV.x, heightUV.y, rvt.ArrayIndex);
	//result.y += HeightMapTextureArray.SampleLevel(Samp_HeightMapTexture, myCoord, 0).r;
	//result.y += ArrayTextures[1].SampleLevel(Samp_HeightMapTexture, heightUV, 0).r;
	//test code

	//result += StartPosition;
	return result;
}

float4 GetMaterialId(float2 uv)
{
	return MaterialIdTexture.SampleLevel(Samp_MaterialIdTexture, uv.xy, 0);
}

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

void DoTerrainModifierVS(inout PS_INPUT vsOut, inout VS_INPUT vert)
{
	float2 uv = vert.vPosition.xz;
	half3 nor = half3(0,1,0);
	float3 pos = GetPosition(uv);

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
	vsOut.vPosition.y = HeightMapTexture.SampleLevel(Samp_HeightMapTexture, heightUV.xy, 0).r;
	//vsOut.vPosition.y += StartPosition.y;
	vsOut.vPosition.xyz += StartPosition;
	vsOut.vWorldPos = vsOut.vPosition.xyz;

	vsOut.vNormal = NormalMapTexture.SampleLevel(Samp_NormalMapTexture, heightUV.xy, 0).xyz;
	vsOut.vNormal = normalize(vsOut.vNormal * 2.0f - float3(1.0f, 1.0f, 1.0f));
	//vsOut.vNormal.xy = heightUV.xy;
	//vsOut.vTangent.xyz = normalize(mul(float4(vertexData.Tangent.xyz, 0), instData.Matrix).xyz);

	vsOut.vUV = GetGlobalUV(final_pos.xy);
	//vsOut.vUV = uv;

	vert.vPosition = vsOut.vPosition.xyz;
	vert.vNormal = vsOut.vNormal;
	vert.vUV = vsOut.vUV;
	vsOut.SpecialData.x = (uint)(GetMaterialId(heightUV.xy).r * 255.0f + 0.1f);
	if (vsOut.SpecialData.x >= 2)
		vsOut.SpecialData.x = 1;
}

void MdfQueueDoModifiers(inout PS_INPUT output, VS_INPUT input)
{
	DoTerrainModifierVS(output, input);
}

void MdfQueueDoModifiersPS(inout MTL_OUTPUT output, PS_INPUT input)
{
	output.mNormal = NormalMapTexture.SampleLevel(Samp_NormalMapTexture, input.vNormal.xy, 0).xyz;
	output.mNormal = normalize(output.mNormal * 2.0f - float3(1.0f, 1.0f, 1.0f));
	//output.mNormal.xyz = float3(0,1,0);
}

#define MDFQUEUE_FUNCTION
#define VS_NO_WorldTransform

//#define MDFQUEUE_FUNCTION_PS

//PS_INPUT VS_Main( uint verteID : SV_VertexID )
//{
//	PS_INPUT output = (PS_INPUT)0;
//	VS_INPUT input = (VS_INPUT)0;
//
//	uint x = verteID & 0x0000FFFF;
//	uint z = (verteID >> 16);
//	
//	half2 uv;
//	half3 nor;
//	half3 pos = GetPosition(x, z, uv, nor);
//
//	input.vPosition = pos;
//	input.vNormal = nor;
//	//input.vTangent.xyz = normalize(mul(float4(vertexData.Tangent.xyz, 0), instData.Matrix).xyz);
//
//	input.vUV = uv;
//
//	Default_VSInput2PSInput(output, input);
//	
//	output.vWorldPos = output.vPosition.xyz;
//	output.vPosition = mul(float4(output.vWorldPos, 1), ViewPrjMtx);
//	
//	output.psCustomUV0.w = output.vPosition.w;
//
//	return output;
//}
//
//PS_OUTPUT PS_Main(PS_INPUT input)
//{
//	/*PS_OUTPUT output = (PS_OUTPUT)0;
//	MTL_OUTPUT mtl = Default_PSInput2Material(input);
//
//#ifndef DO_PS_MATERIAL
//#define DO_PS_MATERIAL DoDefaultPSMaterial
//#endif
//	DO_PS_MATERIAL(input, mtl);
//
//	output.RT0 = half4(mtl.mAlbedo.xyz, mtl.mAlpha);
//	return output;*/
//	return PS_MobileBasePass(input);
//}