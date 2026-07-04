#ifndef __VARBASE_PERMESH_SHADERINC__
#define __VARBASE_PERMESH_SHADERINC__
#include "../Inc/GlobalDefine.cginc"
#include "../Inc/SystemEnumDefine.cginc"

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

    int MeshRenderFlags;
	
	bool IsAcceptShadow()
	{
		return (MeshRenderFlags & ERenderFlags_AcceptShadow) != 0;
	}
};

#endif