#ifndef _TerrainCommonModifier_cginc_
#define _TerrainCommonModifier_cginc_

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

cbuffer cbPerTerrain DX_AUTOBIND
{
	float GridSize;
	float PatchSize;
	float TexUVScale;
	float MaterialIdUVStep;
	float DiffuseUVStep;
	LODLayer MorphLODs[10];
};

cbuffer cbPerPatch DX_AUTOBIND
{
	float3 StartPosition;
	int CurrentLOD;

	float3 EyeCenter;

	float2 TexUVOffset;
};

Texture2D		HeightMapTexture DX_AUTOBIND;
SamplerState	Samp_HeightMapTexture DX_AUTOBIND;

float3 GetPosition(float2 uv)
{
	float3 result = float3(0, 0, 0);
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

#endif //_TerrainCommonModifier_cginc_