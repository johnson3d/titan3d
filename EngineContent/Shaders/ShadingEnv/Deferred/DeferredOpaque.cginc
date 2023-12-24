#ifndef _DEFERRED_OPAQUE_
#define _DEFERRED_OPAQUE_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"
#include "../../Inc/ShadowCommon.cginc"
#include "../../Inc/FogCommon.cginc"
#include "../../Inc/MixUtility.cginc"
#include "../../Inc/SysFunction.cginc"
#include "DeferredCommon.cginc"

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
#if USE_PS_PointLightIndices == 1
	output.PointLightIndices = PointLightIndices;
#endif

#if USE_PS_SpecialData == 1
	output.SpecialData.x = PointLightNum;
#endif
#endif

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	{
#ifdef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif

#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif
	}

#if !defined(VS_NO_WorldTransform)
	output.vPosition.xyz += mtl.mVertexOffset;

	float4 wp4 = mul(float4(output.vPosition.xyz, 1), WorldMatrix);
#if USE_PS_WorldPos == 1
	output.vWorldPos = wp4.xyz;
#endif

#if USE_PS_Normal == 1
	output.vNormal = normalize(mul(float4(output.vNormal.xyz, 0), WorldMatrix).xyz);
#endif

#if USE_PS_Tangent == 1
	output.vTangent.xyz = normalize(mul(float4(output.vTangent.xyz, 0), WorldMatrix).xyz);
#endif

#else
	float4 wp4 = float4(output.vPosition.xyz, 1);
#endif

	float3 preWorldPos = mul(float4(output.vPosition.xyz, 1), PreWorldMatrix).xyz;
	float4 prePos = mul(float4(preWorldPos, 1), GetPreFrameViewPrjMtx(true));
	//float4 noJitterPos = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(false));
	//prePos /= prePos.w;

	//output.vPosition = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(true));
	output.vPosition = mul(wp4, GetViewPrjMtx(true));

	//float2 previousScreenPos = prePos.xy * 0.5 + 0.5;
	//float2 currentScreenPos = (output.vPosition.xy / output.vPosition.w) * 0.5 + 0.5;
	//output.psCustomUV0.xy = currentScreenPos - previousScreenPos;

#if USE_PS_Custom1 == 1
	output.psCustomUV1 = prePos;
#endif

#if USE_PS_Custom2 == 1
	output.psCustomUV2 = output.vPosition;
#endif
	//output.psCustomUV3 = noJitterPos;
	//output.psCustomUV0.xy = float2(output.vPosition.xy / output.vPosition.w) * float2(0.5f, -0.5f) + float2(0.5f, 0.5f);
	//output.psCustomUV0.z = float(output.vPosition.z / output.vPosition.w);
#if USE_PS_Custom0 == 1
	output.psCustomUV0.w = output.vPosition.w;
#endif

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