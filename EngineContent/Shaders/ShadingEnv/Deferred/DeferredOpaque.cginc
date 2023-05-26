#ifndef _DEFERRED_OPAQUE_
#define _DEFERRED_OPAQUE_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"
#include "../../Inc/ShadowCommon.cginc"
#include "../../Inc/FogCommon.cginc"
#include "../../Inc/MixUtility.cginc"
#include "../../Inc/SysFunction.cginc"
 
#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"
 
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

	output.vWorldPos = mul(float4(output.vPosition.xyz, 1), WorldMatrix).xyz;
	output.vNormal = normalize(mul(float4(output.vNormal.xyz, 0), WorldMatrix).xyz);
	output.vTangent.xyz = normalize(mul(float4(output.vTangent.xyz, 0), WorldMatrix).xyz);
#endif
	float3 preWorldPos = mul(float4(output.vPosition.xyz, 1), PreWorldMatrix).xyz;
	float4 prePos = mul(float4(preWorldPos, 1), GetPreFrameViewPrjMtx(true));
	//float4 noJitterPos = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(false));
	//prePos /= prePos.w;

	output.vPosition = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(true));

	//float2 previousScreenPos = prePos.xy * 0.5 + 0.5;
	//float2 currentScreenPos = (output.vPosition.xy / output.vPosition.w) * 0.5 + 0.5;
	//output.psCustomUV0.xy = currentScreenPos - previousScreenPos;

	output.psCustomUV1 = prePos;
	output.psCustomUV2 = output.vPosition;
	//output.psCustomUV3 = noJitterPos;
	//output.psCustomUV0.xy = float2(output.vPosition.xy / output.vPosition.w) * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
	//output.psCustomUV0.z = float(output.vPosition.z / output.vPosition.w);
	output.psCustomUV0.w = output.vPosition.w;

	return output;
}

#include "DeferredBasePassPS.cginc"

PS_OUTPUT PS_Main(PS_INPUT input)
{	
	/*PS_OUTPUT output = (PS_OUTPUT)0;
	output.RT0 = float4(1, 1, 1, 1);
	return output;*/
	return PS_MobileBasePass(input);
}

#endif//#ifndef _DEFERRED_OPAQUE_