
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
	float MaterialIdUVStep;
	float DiffuseUVStep;
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

Texture2D		NormalMapTexture DX_NOBIND;
SamplerState	Samp_NormalMapTexture DX_NOBIND;

//test code
Texture2DArray	HeightMapTextureArray DX_NOBIND;
Texture2D		ArrayTextures[3];
//test code

Texture2D		MaterialIdTexture DX_NOBIND;
SamplerState	Samp_MaterialIdTexture DX_NOBIND;

Texture2DArray	DiffuseTextureArray DX_NOBIND;
SamplerState	Samp_DiffuseTextureArray DX_NOBIND;

Texture2DArray	NormalTextureArray DX_NOBIND;
SamplerState	Samp_NormalTextureArray DX_NOBIND;

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
	
	//vert.vLightMap.xy = heightUV.xy;
	vsOut.vLightMap.xy = heightUV.xy;
	
	/*uint v0_0 = (uint)(GetMaterialId(heightUV.xy).r * 255.0f + 0.1f);
	uint v1_0 = (uint)(GetMaterialId(heightUV.xy + float2(MaterialIdUVStep, 0)).r * 255.0f + 0.1f);
	uint v1_1 = (uint)(GetMaterialId(heightUV.xy + float2(MaterialIdUVStep, MaterialIdUVStep)).r * 255.0f + 0.1f);
	uint v0_1 = (uint)(GetMaterialId(heightUV.xy + float2(0, MaterialIdUVStep)).r * 255.0f + 0.1f);
	vsOut.SpecialData.x = v0_0;
	vsOut.SpecialData.y = v1_0;
	vsOut.SpecialData.z = v1_1;
	vsOut.SpecialData.w = v0_1;*/
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
