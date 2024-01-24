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

#if defined(FEATURE_USE_RVT)
Texture2DArray	HeightMapTexture DX_AUTOBIND;
#else
Texture2D		HeightMapTexture DX_AUTOBIND;
#endif
SamplerState Samp_HeightMapTexture DX_AUTOBIND;

float GetTerrrainVertexHeight(float2 uv, int uniqueTextureId = 0)
{
#if defined(FEATURE_USE_RVT)
	uint arrayIndex = TextureSlotBuffer.Load((uniqueTextureId & 0xffff) * 4);
	return HeightMapTexture.SampleLevel(Samp_HeightMapTexture, float3(uv.xy, arrayIndex), 0).r;
#else
    return HeightMapTexture.SampleLevel(Samp_HeightMapTexture, uv.xy, 0).r;
#endif
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

#endif //_TerrainCommonModifier_cginc_