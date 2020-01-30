#ifndef __VARBASE_PERFRAME_SHADERINC__
#define __VARBASE_PERFRAME_SHADERINC__

cbuffer cbPerFrame : register( b3 )
{

	// ShaderParamAnalyse Start
	float Time;// = 1.0f;
	float TimeSin;// = 1.0f;
	float TimeCos;// = 1.0f;
	// ShaderParamAnalyse End
	#ifndef UserDef_PerFrame
		#define UserDef_PerFrame
	#endif

	UserDef_PerFrame

};

#endif