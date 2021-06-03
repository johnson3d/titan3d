#ifndef __VARBASE_PERCAMERA_SHADERINC__
#define __VARBASE_PERCAMERA_SHADERINC__

cbuffer cbPerCamera : register( b0 )
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

	#ifndef UserDef_PerCamera
		#define UserDef_PerCamera 
	#endif

	UserDef_PerCamera

};

#endif