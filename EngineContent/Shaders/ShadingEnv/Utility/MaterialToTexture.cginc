#ifndef __MATERIAL_TO_TEXTURE__
#define __MATERIAL_TO_TEXTURE__

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/PostEffectCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

#ifdef MDFQUEUE_FUNCTION
MDFQUEUE_FUNCTION
#endif

PS_INPUT VS_Main(VS_INPUT input1)
{
    VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
    PS_INPUT output = (PS_INPUT) 0;

    output.vPosition = float4(input.vPosition.xyz, 1.0f);
    output.vUV = input.vUV;

    output.psCustomUV0.xyz = CornerRays[input.vVertexID];

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
	half3 Albedo = sRGB2Linear((half3)mtl.mAlbedo);

#ifdef ALPHA_TEST
	clip(Alpha - AlphaTestThreshold);
#endif // AlphaTest

	{
		half3 Emissive = (half3)mtl.mEmissive;
		half3 BaseShading = half3(0.0h, 0.0h, 0.0h);

		BaseShading = (half3)mtl.mAlbedo;

		BaseShading += Emissive;

		//BaseShading.b = (half)floor(BaseShading.b * AO_M);
		output.RT0.rgb = BaseShading;
		output.RT0.a = Alpha;
	}

	return output;
}

#endif