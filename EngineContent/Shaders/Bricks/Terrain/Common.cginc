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
    float3 EyeCenter;
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

	float2 TexUVOffset;
};

#endif //_TerrainCommonModifier_cginc_