#ifndef __VARBASE_PERMESH_SHADERINC__
#define __VARBASE_PERMESH_SHADERINC__

cbuffer cbPerMesh : register( b5 )
{
	// ShaderParamAnalyse Start
	matrix WorldMatrix;
	matrix WorldMatrixInverse;

	float4 HitProxyId;
	float4 ActorId;
	
	float3 CameraPositionInModel;
	float PickedID;

	float4 AbsBonePos[360];
	float4 AbsBoneQuat[360];

	//float4 PointLightPos_RadiusInv[4];
	//float4 PointLightColor_Intensity[4];
	int PointLightNum;
	float4 PointLightIndices;
	// ShaderParamAnalyse End
};

#endif