#ifndef __VARBASE_PERFRAME_SHADERINC__
#define __VARBASE_PERFRAME_SHADERINC__
#include "GlobalDefine.cginc"

VK_BIND(3) cbuffer cbPerFrame DX_BIND_B(3)
{
	float Time;// = 1.0f;
	float TimeSin;// = 1.0f;
	float TimeCos;// = 1.0f;
	float ElapsedTime;

	#ifndef UserDef_PerFrame
		#define UserDef_PerFrame
	#endif

	UserDef_PerFrame

};

#endif