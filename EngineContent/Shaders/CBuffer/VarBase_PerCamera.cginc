#ifndef __VARBASE_PERCAMERA_SHADERINC__
#define __VARBASE_PERCAMERA_SHADERINC__

cbuffer cbPerCamera : register( b0 )
{
	// ShaderParamAnalyse Start
	matrix CameraViewMatrix;
	matrix CameraViewInverse;
	matrix ViewPrjMtx;
	matrix ViewPrjInvMtx;
	matrix PrjMtx;
	matrix PrjInvMtx;


	float3 CameraPosition;
	float3 CameraLookAt;
	float3 CameraDirection;
	float3 CameraRight;
	float3 CameraUp;
	// ShaderParamAnalyse End
	float gZFar;


	#ifndef UserDef_PerCamera
		#define UserDef_PerCamera 
	#endif

	UserDef_PerCamera

};

#endif