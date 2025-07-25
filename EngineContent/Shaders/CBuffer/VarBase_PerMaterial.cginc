#ifndef __VARBASE_PERINSTANCE_SHADERINC__
#define __VARBASE_PERINSTANCE_SHADERINC__
#include "../Inc/GlobalDefine.cginc"

cbuffer cbPerMaterial DX_BIND_B(2)
{
	// ShaderParamAnalyse Start
	float3 PerInst_Noused0;
    uint MaterialRenderFlags;

	// ShaderParamAnalyse End

	#include "MaterialVar"

};

#endif