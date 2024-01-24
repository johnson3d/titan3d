#ifndef _GrassModifier_cginc_
#define _GrassModifier_cginc_

#include "Common.cginc"

cbuffer cbPerGrassType DX_AUTOBIND
{
	float MinScale;
	float MaxScale;
	float HeightMapMinHeight;
	float PatchIdxX;
	float PatchIdxZ;
	int MaxGrassInstanceNum;
};

struct VSGrassData
{
	//float3 GrassPosition;
	//float GrassScale;
	//float4 GrassQuat;
	uint Data;
	float TerrainHeight;
	int GrassDataPad0;
	int GrassDataPad1;
};

#if HW_VS_STRUCTUREBUFFER == 1
StructuredBuffer<VSGrassData> VSGrassDataArray DX_AUTOBIND;
VSGrassData GetGrassInstanceData(VS_MODIFIER input)
{
	VSGrassData result = VSGrassDataArray[min(input.vInstanceId, MaxGrassInstanceNum)];//(VSGrassData)0;//
	//VSGrassData result = VSGrassDataArray[min(input.vInstanceId, 0)];
	//VSGrassData result = (VSGrassData)0;
	//result = VSGrassDataArray[0];
	//result.InstanceId = input.vInstanceId;
	return result;
}
#define Def_GetGrassInstanceData
#endif


float3 InstancingRotateVec(in float3 inPos, in float4 inQuat)
{
	float3 uv = cross(inQuat.xyz, inPos);
	float3 uuv = cross(inQuat.xyz, uv);
	uv = uv * (2.0f * inQuat.w);
	uuv *= 2.0f;
	
	return inPos + uv + uuv;
}

float3 GetInstancingPosition(VSGrassData data)
{
	float3 pos = float3(((data.Data & 0xfff00000) >> 20) * PatchSize / 4096.0f, 0.0f, ((data.Data & 0xfff00) >> 8) * PatchSize / 4096.0f);
//#if GRASS_VERTEXFOLLOWHEIGHT == 1
//	float3 outPos = GetPosition(float2(pos.x / PatchSize + PatchIdxX, pos.z / PatchSize + PatchIdxZ));
//	outPos.y += HeightMapMinHeight;
//	return outPos;
//#else
	pos.y = data.TerrainHeight;
	return pos;
//#endif
}
float3 GetTerrainPos(float2 pos)
{
    float3 outPos = GetTerrrainVertexPosition(float2(pos.x / PatchSize + PatchIdxX, pos.y / PatchSize + PatchIdxZ), 0);
	outPos.y += HeightMapMinHeight;
	return outPos;
}
float4 GetInstancingRot(uint data)
{
	float angle = ((data & 0xf0) >> 4) * 3.1415926f * 0.06667f;
	return float4(0, sin(angle), 0, cos(angle));
}
float3 GetInstancingScale(uint data)
{
	float scale = (data & 0xf) * (MaxScale - MinScale) * 0.06667f + MinScale;
	return float3(scale, scale, scale);
}

//#define VS_Grass_VertexOnTerrain

void DoGrassModifierVS(inout PS_INPUT vsOut, inout VS_MODIFIER vert)
{
	VSGrassData instData = GetGrassInstanceData(vert);
	float3 instPos = GetInstancingPosition(instData);
	float4 instRot = GetInstancingRot(instData.Data);
	float3 instScale = GetInstancingScale(instData.Data);

	float3 vertPos = InstancingRotateVec(vert.vPosition.xyz * instScale, instRot);
#if GRASS_VERTEXFOLLOWHEIGHT == 1
	//float3 Pos = instPos.xyz + vertPos;
	float3 tPos = GetTerrainPos(vertPos.xz + instPos.xz);
	float3 Pos = vertPos + float3(instPos.x, tPos.y, instPos.z);
#else
	float3 Pos = vertPos + instPos;
#endif
	//Pos = vertPos + instPos;
	//Pos = vertPos + instPos;
	//float3 ttt = (float3)0;
	//ttt.y = HeightMapTexture.SampleLevel(Samp_HeightMapTexture, float2(0,0), 0).r;
	////ttt.y += HeightMapMinHeight;
	//Pos = ttt;

	//float2 posUV = float2(instData.GrassPosition.x / PatchSize, instData.GrassPosition.z / PatchSize);
	//float3 posInTerrain = GetPosition(posUV);
	//posInTerrain.y += HeightMapMinHeight;
	//float3 Pos = posInTerrain + InstancingRotateVec(vert.vPosition.xyz * instData.GrassScale, instData.GrassQuat);
#if USE_PS_WorldPos == 1
	vsOut.vWorldPos = Pos.xyz;
#endif

	vert.vPosition.xyz = Pos;
	//vert.vNormal.xyz = InstancingRotateVec(vert.vNormal.xyz, instData.GrassQuat);
	vert.vNormal.xyz = InstancingRotateVec(vert.vNormal.xyz, instRot);

	vsOut.vPosition.xyz = Pos;
#if USE_PS_Normal == 1
	vsOut.vNormal = vert.vNormal;
#endif
}

//#define MDFQUEUE_FUNCTION
//#define VS_NO_WorldTransform

#endif //_GrassModifier_cginc_