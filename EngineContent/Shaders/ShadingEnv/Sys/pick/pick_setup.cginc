#ifndef _PICKED_SETUP_
#define _PICKED_SETUP_

#include "../../../Inc/VertexLayout.cginc"
#include "../../../Inc/LightCommon.cginc"
#include "../../../Inc/Math.cginc"
#include "../../../Inc/ShadowCommon.cginc"
#include "../../../Inc/FogCommon.cginc"
#include "../../../Inc/MixUtility.cginc"
#include "../../../Inc/SysFunction.cginc"

#include "Material"
#include "MdfQueue"

#include "../../../Inc/SysFunctionDefImpl.cginc"

//WARNING:don't change vs_main or ps_main's parameters name cause we also use it in c++;It's an appointment;
PS_INPUT VS_Main(VS_INPUT input1)
{
	VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

#if defined(VS_NO_WorldTransform)
	output.PointLightIndices = PointLightIndices;
	output.SpecialData.x = PointLightNum;
#endif

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	{
#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif

#ifdef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif
	}

#if !defined(VS_NO_WorldTransform)
	output.vPosition.xyz += mtl.mVertexOffset;
#endif

	output.vPosition = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(true));

	//output.psCustomUV0.xy = float2(output.vPosition.xy / output.vPosition.w) * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
	//output.psCustomUV0.z = float(output.vPosition.z / output.vPosition.w);
	output.psCustomUV0.w = output.vPosition.w;

	return output;
}

struct PS_OUTPUT
{
	float2 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;
	float PerPixelViewerDistance = input.psCustomUV0.w;
	output.RT0 = float2(1.0f, PerPixelViewerDistance * rcp(gZFar));

	return output;
}
#endif