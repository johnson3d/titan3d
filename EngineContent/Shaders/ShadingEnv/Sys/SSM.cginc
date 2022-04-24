#ifndef _SSM_
#define _SSM_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

//WARNING:don't change vs_main or ps_main's parameters name cause we also use it in c++;It's an appointment;
PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

#if defined(MDF_INSTANCING)
	output.PointLightIndices = PointLightIndices;
	output.SpecialData.x = PointLightNum;
#endif

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	//mtl template stuff;
	{
#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif

#ifndef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif
	}

	output.vPosition.xyz += mtl.mVertexOffset;

	matrix ShadowWVPMtx = mul(WorldMatrix, ViewPrjMtx);
	output.vPosition = mul(float4(output.vPosition.xyz, 1), ShadowWVPMtx);
	
	output.vPosition.z = output.vPosition.z + gDepthBiasAndZFarRcp.x;

	return output;
}

struct PS_OUTPUT
{
	//float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	MTL_OUTPUT mtl = Default_PSInput2Material(input);
	//mtl template stuff;
	{
#ifndef DO_PS_MATERIAL
#define DO_PS_MATERIAL DoDefaultPSMaterial
#endif
		DO_PS_MATERIAL(input, mtl);
	}

#ifdef ALPHA_TEST
	//half Alpha = mtl.mAlpha;
	//half AlphaTestThreshold = mtl.mAlphaTest;
	clip(mtl.mAlpha - mtl.mAlphaTest);
#endif 

	//output.RT0 = half4(0.0f, 0.0f, 0.0f, 1.0f);
	
	return output;
}

#endif//#ifndef _SSM_