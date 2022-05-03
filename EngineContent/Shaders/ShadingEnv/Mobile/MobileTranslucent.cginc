#ifndef _MOBILE_TRANSLUCENT_
#define _MOBILE_TRANSLUCENT_

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

#ifdef MDFQUEUE_FUNCTION
MDFQUEUE_FUNCTION
#endif

//WARNING:don't change vs_main or ps_main's parameters name cause we also use it in c++;It's an appointment;
PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

#if defined(VS_NO_WorldTransform)
	output.PointLightIndices = PointLightIndices;
	output.SpecialData.x = PointLightNum;
#endif

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

	output.vWorldPos = mul(float4(output.vPosition.xyz, 1), WorldMatrix).xyz;
	output.vNormal = normalize(mul(float4(output.vNormal.xyz, 0), WorldMatrix).xyz);
	output.vTangent.xyz = normalize(mul(float4(output.vTangent.xyz, 0), WorldMatrix).xyz);
#endif
	output.vPosition = mul(float4(output.vWorldPos, 1), ViewPrjMtx);

	return output;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
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

#ifdef MDFQUEUE_FUNCTION_PS
		MdfQueueDoModifiersPS(input, mtl);
#endif
	}

	half Alpha = (half)mtl.mAlpha;
	half AlphaTestThreshold = (half)mtl.mAlphaTest;

#ifdef ALPHA_TEST
	clip(Alpha - AlphaTestThreshold);
#endif // AlphaTest

	{
		half3 Emissive = (half3)mtl.mEmissive;
		half3 BaseShading = half3(0.0h, 0.0h, 0.0h);
		half3 Albedo = (half3)mtl.mAlbedo;

		/*half3 N = normalize((half3)mtl.mNormal);
		half Metallic = (half)mtl.mMetallic;
		half Roughness = 1.0h - (half)mtl.mRough;
		half AbsSpecular = (half)mtl.mAbsSpecular;
		
		half Alpha = (half)mtl.mAlpha;
		half Transmit = (half)mtl.mTransmit;
		half3 SubAlbedo = sRGB2Linear((half3)mtl.mSubAlbedo);
		
		half3 WorldPos = (half3)input.vWorldPos;
		half3 L = -(half3)normalize(gDirLightDirection_Leak.xyz);
		half3 V = (half3)normalize(CameraPosition - WorldPos);*/

#ifdef MTL_ID_UNLIT
		BaseShading = Albedo;
#else
		//lighting for translucent
		BaseShading = Albedo;
#endif

		BaseShading += Emissive;

		BaseShading.b = (half)floor(BaseShading.b * AO_M);
		output.RT0.rgb = BaseShading;
		output.RT0.a = Alpha;
	}

	return output;
}

#endif