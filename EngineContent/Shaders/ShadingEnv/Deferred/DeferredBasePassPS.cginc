#ifndef _DeferredBasePassPS_H_
#define _DeferredBasePassPS_H_

#include "DeferredCommon.cginc"

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
	float4 RT1 : SV_Target1;
	float4 RT2 : SV_Target2;
};

PS_OUTPUT PS_MobileBasePass(PS_INPUT input)
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
		MdfQueueDoModifiersPS(mtl, input);
#endif
	}

	GBufferData GBuffer = (GBufferData)0;

	half Alpha = (half)mtl.mAlpha;
	half AlphaTestThreshold = (half)mtl.mAlphaTest;
#ifdef ALPHA_TEST
	clip(Alpha - AlphaTestThreshold);
#endif // AlphaTest

	//half3 Albedo = sRGB2Linear((half3)mtl.mAlbedo);
	GBuffer.MtlColorRaw = (half3)mtl.mAlbedo;
	GBuffer.Metallicity = (half)mtl.mMetallic;
	GBuffer.Roughness = (half)mtl.mRough;
	GBuffer.WorldNormal = (half3)normalize(mtl.mNormal);
	GBuffer.Emissive = (half)saturate(CalcLuminance((half3)mtl.mEmissive));
	GBuffer.Specular = (half)mtl.mAbsSpecular;
	GBuffer.ObjectFlags_2Bit = ObjectFLags_2Bit;
		
	EncodeGBuffer(GBuffer, output.RT0, output.RT1, output.RT2);
	return output;
}

#endif//_MobileBasePassPS_H_