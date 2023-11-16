#ifndef __VARBASE_PERINSTANCE_SHADERINC__
#define __VARBASE_PERINSTANCE_SHADERINC__
#include "GlobalDefine.cginc"

VK_BIND(2) cbuffer cbPerMaterial DX_BIND_B(2)
{
	// ShaderParamAnalyse Start
	float3 PerInst_Noused0;
    uint MaterialRenderFlags;

	// ShaderParamAnalyse End

	#include "MaterialVar"

};

#endif