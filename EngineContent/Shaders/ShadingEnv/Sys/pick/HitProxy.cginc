#ifndef _HIT_PROXY_
#define _HIT_PROXY_

#include "../../../Inc/VertexLayout.cginc"
#include "../../../Inc/LightCommon.cginc"
#include "../../../Inc/Math.cginc"

#include "Material"
#include "MdfQueue"

#include "../../../Inc/SysFunctionDefImpl.cginc"

//WARNING:don't change vs_main or ps_main's parameters name cause we also use it in c++;It's an appointment;
PS_INPUT VS_Main(VS_INPUT input1)
{
	VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	//mtl template stuff;
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
#if USE_PS_WorldPos == 1
	output.vWorldPos = mul(float4(output.vPosition.xyz, 1), WorldMatrix).xyz;
#endif

#endif

	output.vPosition = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(true));

	return output;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

float4 ConvertHitProxyIdToVector4(uint ProxyId)
{
	return float4(((ProxyId >> 24) & 0x000000ff) / 255.0f, ((ProxyId >> 16) & 0x000000ff) / 255.0f, ((ProxyId >> 8) & 0x000000ff) / 255.0f,
		((ProxyId >> 0) & 0x000000ff) / 255.0f);
}

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
	half Alpha = mtl.mAlpha;
	half AlphaTestThreshold = mtl.mAlphaTest;

	clip(Alpha - AlphaTestThreshold);
#endif // AlphaTest

#if defined(VPS_SpecialData_X_HitProxy)
	output.RT0 = ConvertHitProxyIdToVector4(input.SpecialData.x);
#else
	output.RT0 = HitProxyId;
#endif
	//output.RT0 = float4(0.0f, HitProxyId.a * 30.0f, 0.0f, 1.0f);
	//output.RT0 = float4(1,0,1,1);

	return output;
}


#endif