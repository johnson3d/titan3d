#ifndef _DeferredBasePassPS_H_
#define _DeferredBasePassPS_H_

#include "../../Inc/ShadingCommon.cginc"
#include "../../Inc/Math.cginc"

// [ Jimenez et al. 2016, "Practical Realtime Strategies for Accurate Indirect Occlusion" ]
float3 AOMultiBounce( float3 BaseColor, float AO )
{
	float3 a =  2.0404 * BaseColor - 0.3324;
	float3 b = -4.7951 * BaseColor + 0.6417;
	float3 c =  2.7552 * BaseColor + 0.6903;
	return max( AO, ( ( AO * a + b ) * AO + c ) * AO );
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;//R8G8B8A8:abedo.rgb - metallicty
	float4 RT1 : SV_Target1;//R10G10B10A2:normal.xyz - Flags
	float4 RT2 : SV_Target2;//R8G8B8A8:Roughness,Specular,unused
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
    GBuffer.MtlColorRaw = (half3)mtl.mAlbedo + (half3)mtl.mEmissive;
    GBuffer.WorldNormal = mtl.GetWorldNormal(input);
	GBuffer.Metallicity = (half)mtl.mMetallic;
	GBuffer.Specular = (half)mtl.mAbsSpecular;
	GBuffer.Roughness = (half)mtl.mRough;

	float3 SpecularColor = ComputeF0(GBuffer.Specular, GBuffer.MtlColorRaw, GBuffer.Metallicity);
	float DiffOcclusion = mtl.mAO;
	float SpecOcclusion = mtl.mAO;
	// TODO: ApplyBentNormal
	// float3 BentNormal = GBuffer.WorldNormal;
	GBuffer.CustomData.r = (half)SpecOcclusion;
	GBuffer.AO = (half)(AOMultiBounce( CalcLuminance( SpecularColor ), SpecOcclusion ).g);

	GBuffer.ObjectFlags_2Bit = ObjectFLags_2Bit;
#ifdef MTL_ID_UNLIT
	GBuffer.SetUnlit(true);
#endif
    GBuffer.RenderFlags_10Bit = MaterialRenderFlags;
	//GBuffer.MotionVector.xy = input.psCustomUV0.xy;
	
	float2 previousScreenPos = (input.psCustomUV1.xy / input.psCustomUV1.w) * 0.5 + 0.5;
	float2 currentScreenPos = (input.psCustomUV2.xy / input.psCustomUV2.w) * 0.5 + 0.5;
	//previousScreenPos.y = 1.0 - previousScreenPos.y;
	//currentScreenPos.y = 1.0 - currentScreenPos.y;
    GBuffer.MotionVector.xy = (half2) (currentScreenPos - previousScreenPos);
	GBuffer.MotionVector.y = -GBuffer.MotionVector.y;

	//float2 noJitterScreenPos = (input.psCustomUV3.xy / input.psCustomUV3.w) * 0.5 + 0.5;
	//if (any(abs(currentScreenPos - (noJitterScreenPos + JitterOffset)) > gViewportSizeAndRcp.zw * 0.25f))
	////if (any(abs(currentScreenPos != (noJitterScreenPos + JitterOffset))))
	//{
	//	GBuffer.MtlColorRaw = half3(1,0,0);
	//}

#if (MTL_RENDERFLAGS & 1) == 1
	
#endif
	
    GBuffer.EncodeGBuffer(output.RT0, output.RT1, output.RT2, output.RT3);
	
	return output;
}

#endif//_MobileBasePassPS_H_