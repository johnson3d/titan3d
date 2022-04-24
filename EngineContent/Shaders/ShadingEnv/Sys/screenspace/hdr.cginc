#ifndef _PICKED_INNER_
#define _PICKED_INNER_

#include "../../../Inc/VertexLayout.cginc"
#include "../../../Inc/GpuSceneCommon.cginc"
#include "../../../Inc/LightCommon.cginc"
#include "../../../Inc/PostEffectCommon.cginc"

#include "Material"
#include "MdfQueue"

#include "../../../Inc/SysFunctionDefImpl.cginc"

float3 ToneMap(float3 color)
{
	float LumHdr = CalcLuminanceYCbCr((half3)color);
	float fLumAvg = GpuSceneDescSRV[0].ScreenAverageBrightness;

	float fLumScaled = (LumHdr * HdrMiddleGrey) / max(HdrMinLuminance, fLumAvg);
	// 引入 Lwhite
	float fLumCompressed = (fLumScaled * (1.0 + (fLumScaled / (HdrMaxLuminance * HdrMaxLuminance)))) / (1.0 + fLumScaled);
	return fLumCompressed * color;
}

float3 ACESToneMapping(float3 color)
{
	const float A = 2.51;
	const float B = 0.03;
	const float C = 2.43;
	const float D = 0.59;
	const float E = 0.14;
	return (color * (A * color + B)) / (color * (C * color + D) + E);
}

float EyeAdaption(float lum)
{
	//return lum;
	return GpuSceneDescSRV[0].EyeAdapter;
}

float3 ToneMap2(float3 color)
{
	//https://xiaoiver.github.io/coding/2019/02/05/HDR-Tone-Mapping.html
	// Adjust exposure
	// From KlayGE
	float fLum = GpuSceneDescSRV[0].ScreenAverageBrightness;
	float adaptedLumDest = 3.0 / (max(0.1, 1.0 + 10.0 * EyeAdaption(fLum)));
	float exposureBias = adaptedLumDest * Exposure;

	// 应用曝光度进入 HDR
	color.rgb *= exposureBias;
	// 通过 ACES ToneMapping 映射成 LDR
	color.rgb = ACESToneMapping(color.rgb);

	return color;
}

//Filmic Tonemapping
float3 ACESToneMapping_Base(float3 x)
{
	const float A = 0.22;   // Shoulder Strength
	const float B = 0.30;   // Linear Strength
	const float C = 0.10;   // Linear Angle
	const float D = 0.20;   // Toe Strength
	const float E = 0.01;   // Toe Numerator
	const float F = 0.30;   //Toe Denominator

	return ((x * (A * x + C * B) + D * E) / (x * (A * x + B) + D * F)) - E / F;
}

float3 ToneMap3(float3 color)
{
	float3 whiteScale = (float3)11.2f;//calc value from GpuSceneDescSRV[0].ScreenAverageBrightness
	return ACESToneMapping_Base(color) / ACESToneMapping_Base(whiteScale);
}


Texture2D GSourceTarget DX_NOBIND;
SamplerState Samp_GSourceTarget DX_NOBIND;

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;

	output.vPosition = float4(input.vPosition.xyz, 1.0f);
	output.vUV = input.vUV;

	return output;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	float2 uv = input.vUV;
	float4 BaseData = GSourceTarget.Sample(Samp_GSourceTarget, uv).rgba;
	BaseData.rgb = sRGB2Linear((half3)BaseData.rgb);
	BaseData.rgb = ToneMap2((half3)BaseData.rgb);
	BaseData.rgb = Linear2sRGB((half3)BaseData.rgb);

	//half LumHdr = CalcLuminanceYCbCr(BaseData);
	//half VignetteWeight = max(1.0h - LumHdr, 0.0h);
	//half VignetteMask =1.0h - CalcVignettePS(input.vLightMap.xy, 0.5h);
	//half VignetteMask = 1.0h - GVignette.Sample(Samp_GVignette, input.vUV.xy).r;
	//BaseData = (1.0h - VignetteMask * VignetteWeight) * BaseData;	
	//BaseData.rgb = ACESMobile_HQ(BaseData.rgb);

	output.RT0 = BaseData;

	return output;
}

#endif
///