#ifndef __VARBASE_PERMESH_SHADERINC__
#define __VARBASE_PERMESH_SHADERINC__
#include "../Inc/GlobalDefine.cginc"

cbuffer cbPerMesh DX_BIND_B(5)
{
	matrix WorldMatrix;
	matrix WorldMatrixInverse;

	matrix PreWorldMatrix;

	float4 HitProxyId;
	float4 ActorId;
	
	float3 CameraPositionInModel;
	float PickedID;

	float4 PointLightIndices;
	int PointLightNum;

    int ObjectFLags_2Bit;
	
	bool IsAcceptShadow()
	{
		return (ObjectFLags_2Bit & 1) != 0;
	}

	bool IsUnlit()
	{
		return (ObjectFLags_2Bit & (2)) != 0;
	}
};

#endif