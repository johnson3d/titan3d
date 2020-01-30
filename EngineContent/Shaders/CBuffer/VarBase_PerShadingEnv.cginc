#ifndef __VARBASE_PERSHADINGENV_SHADERINC__
#define __VARBASE_PERSHADINGENV_SHADERINC__

cbuffer cbPerShadingEnv : register( b4 )
{
	// ShaderParamAnalyse Start
	


	// ShaderParamAnalyse End

	#ifndef UserDef_ShadingEnv
	#define UserDef_ShadingEnv 
	#endif
	
		UserDef_ShadingEnv
};

#endif