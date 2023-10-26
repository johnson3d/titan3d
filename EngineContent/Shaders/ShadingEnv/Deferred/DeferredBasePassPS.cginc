#ifndef _DeferredBasePassPS_H_
#define _DeferredBasePassPS_H_

#include "DeferredCommon.cginc"

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;//R8G8B8A8:abedo.rgb - metallicty
	float4 RT1 : SV_Target1;//R10G10B10A2:normal.xyz - Flags
	float4 RT2 : SV_Target2;//R8G8B8A8:Roughness,Emissive,Specular,unused
	float4 RT3 : SV_Target3;//R10G10B10A2:motion.xy,unused,unused
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
		MdfQueueDoModifiersPS(input, mtl);
#endif
    }

	GBufferData GBuffer = (GBufferData)0;

	half Alpha = (half)mtl.mAlpha;
	half AlphaTestThreshold = (half)mtl.mAlphaTest;
#ifdef ALPHA_TEST
	clip(Alpha - AlphaTestThreshold);
#endif // AlphaTest

	//half3 Albedo = sRGB2Linear((half3)mtl.mAlbedo);
	GBuffer.MtlColorRaw = (half3)mtl.mAlbedo + mtl.mEmissive;
	GBuffer.Metallicity = (half)mtl.mMetallic;
	GBuffer.Roughness = (half)mtl.mRough;
	GBuffer.WorldNormal = (half3)normalize(mtl.mNormal);
	GBuffer.Specular = (half)mtl.mAbsSpecular;
	GBuffer.ObjectFlags_2Bit = ObjectFLags_2Bit;
	//GBuffer.MotionVector.xy = input.psCustomUV0.xy;
	
	float2 previousScreenPos = (input.psCustomUV1.xy / input.psCustomUV1.w) * 0.5 + 0.5;
	float2 currentScreenPos = (input.psCustomUV2.xy / input.psCustomUV2.w) * 0.5 + 0.5;
	//previousScreenPos.y = 1.0 - previousScreenPos.y;
	//currentScreenPos.y = 1.0 - currentScreenPos.y;
	GBuffer.MotionVector.xy = currentScreenPos - previousScreenPos;
	GBuffer.MotionVector.y = -GBuffer.MotionVector.y;

	//float2 noJitterScreenPos = (input.psCustomUV3.xy / input.psCustomUV3.w) * 0.5 + 0.5;
	//if (any(abs(currentScreenPos - (noJitterScreenPos + JitterOffset)) > gViewportSizeAndRcp.zw * 0.25f))
	////if (any(abs(currentScreenPos != (noJitterScreenPos + JitterOffset))))
	//{
	//	GBuffer.MtlColorRaw = half3(1,0,0);
	//}

	GBuffer.EncodeGBuffer(output.RT0, output.RT1, output.RT2, output.RT3);
	return output;
}

#endif//_MobileBasePassPS_H_