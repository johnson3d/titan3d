#ifndef __VARBASE_PERFRAME_SHADERINC__
#define __VARBASE_PERFRAME_SHADERINC__
#include "../Inc/GlobalDefine.cginc"

VK_BIND(3) cbuffer cbPerFrame DX_BIND_B(3)
{
	float Time;// = 1.0f;
	float TimeFracSecond;//0-1
	float TimeSin;// = 1.0f;
	float TimeCos;// = 1.0f;
	float ElapsedTime;

	#ifndef UserDef_PerFrame
		#define UserDef_PerFrame
	#endif

	UserDef_PerFrame

};

#endif