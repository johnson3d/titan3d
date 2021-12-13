#ifndef __VARBASE_PERCAMERA_SHADERINC__
#define __VARBASE_PERCAMERA_SHADERINC__
#include "GlobalDefine.cginc"

VK_BIND(0) cbuffer cbPerCamera DX_BIND_B(0)
{
	matrix CameraViewMatrix;
	matrix CameraViewInverse;
	matrix ViewPrjMtx;
	matrix ViewPrjInvMtx;
	matrix PrjMtx;
	matrix PrjInvMtx;

	float3 CameraPosition;
	float gZNear;

	float3 CameraLookAt;
	float gZFar;

	float3 CameraDirection;
	float pad0;

	float3 CameraRight;
	float pad1;

	float3 CameraUp;
	float pad2;

	float3 CameraOffset;

	#ifndef UserDef_PerCamera
		#define UserDef_PerCamera 
	#endif

	UserDef_PerCamera

};

inline float LinearFromDepth(float z)
{
	//需要优化成 1 / (arg1 - z * arg2)形式，可以减少两个数学运算
	return (gZNear * gZFar) / (gZFar - z * (gZFar - gZNear));
}

float4 GetWorldPositionFromDepthValue(float2 uv, float depth)
{
	float4 H = float4(uv.x * 2.0f - 1.0f, 1.0f - uv.y * 2.0f, depth, 1.0f);
	float4 D = mul(H, ViewPrjInvMtx);
	return D / D.w;
}

#endif