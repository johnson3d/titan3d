#ifndef __VARBASE_PERCAMERA_SHADERINC__
#define __VARBASE_PERCAMERA_SHADERINC__
#include "../Inc/GlobalDefine.cginc"

cbuffer cbPerCamera DX_BIND_B(0)
{
	matrix CameraViewMatrix;
	matrix CameraViewInverse;
	
	matrix PrjMtx;
	matrix PrjInvMtx;
	matrix ViewPrjMtx;
	matrix ViewPrjInvMtx;
	matrix PreFrameViewPrjMtx;
	
	matrix JitterPrjMtx;
	matrix JitterPrjInvMtx;
	matrix JitterViewPrjMtx;
	matrix JitterViewPrjInvMtx;
	matrix JitterPreFrameViewPrjMtx;

	float2 JitterOffset;
    float2 JitterOffset_Pad;

	float4 CornerRays[4];
	float4 ClipPlanes[6];
    
	float4 ClipPlanesX;
    float4 ClipPlanesY;
    float4 ClipPlanesZ;
    float4 ClipPlanesW;
	
    float3 ClipMinPoint;
    float ClipMinPoint_Pad;
    float3 ClipMaxPoint;
    float ClipMaxPoint_Pad;

	float3 CameraPosition;
	float ZNear;

	float3 CameraLookAt;
	float ZFar;

	float3 CameraDirection;
	float pad0;

	float3 CameraRight;
	float pad1;

	float3 CameraUp;
	float pad2;

	float3 CameraOffset;
};

inline matrix GetPrjMtx(bool bJitter = true)
{
	return bJitter ? JitterPrjMtx : PrjMtx;
}

inline matrix GetPrjMtxInverse(bool bJitter = true)
{
	return bJitter ? JitterPrjInvMtx : PrjInvMtx;
}

inline matrix GetViewPrjMtx(bool bJitter = true)
{
	return bJitter ? JitterViewPrjMtx : ViewPrjMtx;
}

inline matrix GetViewPrjMtxInverse(bool bJitter = true)
{
	return bJitter ? JitterViewPrjInvMtx : ViewPrjInvMtx;
}

inline matrix GetPreFrameViewPrjMtx(bool bJitter = true)
{
	return bJitter ? JitterPreFrameViewPrjMtx : PreFrameViewPrjMtx;
}

inline float LinearFromDepth(float z, float zNear, float zFar)
{
#if USE_INVERSE_Z == 1
	return (zNear * zFar) / (zNear - z * (zNear - zFar));
#else
	//需要优化成 1 / (arg1 - z * arg2)形式，可以减少两个数学运算
    return (zNear * zFar) / (zFar - z * (zFar - zNear));
#endif
}

inline float LinearFromDepth(float z)
{
    return LinearFromDepth(z, ZNear, ZFar);
}

inline float NormalizedLinearFromDepth(float z)
{
    return (LinearFromDepth(z) - ZNear) / (ZFar - ZNear);
}

inline float2 LinearFromDepth(float2 z)
{
#if USE_INVERSE_Z == 1
	float2 t = float2(ZNear, ZNear) - z * (ZNear - ZFar);
	float t2 = ZNear * ZFar;
	return float2(t2,t2) / t;
#else
    float2 t = float2(ZFar, ZFar) - z * (ZFar - ZNear);
    float t2 = ZNear * ZFar;
    return float2(t2, t2) / t;
#endif
}

float4 GetWorldPositionFromDepthValue(float2 uv, float depthNdc, bool bJitter = true)
{
    float4 H = float4(uv.x * 2.0f - 1.0f, 1.0f - uv.y * 2.0f, depthNdc, 1.0f);
    float4 D = mul(H, GetViewPrjMtxInverse(bJitter));
    return D / D.w;
}

float3 GetViewPositionFromDepthValue(float3 ray, float Depth, bool IsLinear)
{
	float linearDepth = IsLinear ? Depth : LinearFromDepth(Depth);
	return ray * linearDepth;
}

#endif