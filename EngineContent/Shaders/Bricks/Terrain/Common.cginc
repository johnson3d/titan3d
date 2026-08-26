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
	// RVT 模式下 patch 参数有两个来源:
	//   != 0 : 本 drawcall 不走 instancing (rpolicy 里没有 TtGpuCullingNode),
	//          StartPosition / CurrentLOD / TexUVOffset 与下面三个 TexID 全部取自本 cbuffer;
	//   == 0 : 走 instancing, 取自 FVSInstanceData 的 UserData / UserData2。
	uint UsePatchRVTParams;
	uint HeightMapTexID;

	uint NormalMapTexID;
	uint MaterialIdTexID;
};

#endif //_TerrainCommonModifier_cginc_