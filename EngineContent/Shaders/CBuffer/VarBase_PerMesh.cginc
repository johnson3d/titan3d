#ifndef __VARBASE_PERMESH_SHADERINC__
#define __VARBASE_PERMESH_SHADERINC__
#include "GlobalDefine.cginc"

VK_BIND(5) cbuffer cbPerMesh DX_BIND_B(5)
{
	matrix WorldMatrix;
	matrix WorldMatrixInverse;

	float4 HitProxyId;
	float4 ActorId;
	
	float3 CameraPositionInModel;
	float PickedID;

	float4 PointLightIndices;
	int PointLightNum;

	int ObjectFLags_2Bit;

	float4 AbsBonePos[360];
	float4 AbsBoneQuat[360];

	//float4 PointLightPos_RadiusInv[4];
	//float4 PointLightColor_Intensity[4];
};

#endif