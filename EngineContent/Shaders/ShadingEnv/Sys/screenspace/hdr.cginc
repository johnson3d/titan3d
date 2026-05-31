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
	//https://www.desmos.com/calculator/h8rbdpawxj?lang=zh-CN
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


Texture2D GSourceTarget DX_AUTOBIND;
SamplerState Samp_GSourceTarget DX_AUTOBIND;

// Color Grading dual-LUT blend (each Volume owns its LUT, GPU blends by weight)
Texture3D   ColorGradingLUT0 DX_AUTOBIND;       // Primary volume LUT
Texture3D   ColorGradingLUT1 DX_AUTOBIND;       // Secondary volume LUT (may be unbound)
SamplerState Samp_ColorGradingLUT0 DX_AUTOBIND;

cbuffer cbColorGradingBlend DX_AUTOBIND
{
	float LutBlendWeight;   // 0 = 100% LUT0, 1 = 100% LUT1
	float HasSecondLut;     // 0 = single LUT, 1 = dual LUT blend
	float2 _cgPad;
};

float3 ApplyColorGradingLUT3D(float3 ldrColor)
{
	float3 lutUV = saturate(ldrColor);
	float3 color0 = ColorGradingLUT0.SampleLevel(Samp_ColorGradingLUT0, lutUV, 0).rgb;

	// Fallback: if LUT is unbound or empty (samples black), skip color grading
	if (dot(color0, 1) < 0.0001)
		return ldrColor;

	if (HasSecondLut > 0.5)
	{
		float3 color1 = ColorGradingLUT1.SampleLevel(Samp_ColorGradingLUT0, lutUV, 0).rgb;
		color0 = lerp(color0, color1, LutBlendWeight);
	}
	// F16 LUT may store values >1 from aggressive gain; final clamp to LDR output
	return saturate(color0);
}

PS_INPUT VS_Main(VS_INPUT input1)
{
	VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
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

#if ENABLE_COLOR_GRADING_LUT
	// 1. Exposure + ACES tonemapping: HDR → LDR [0,1]
	BaseData.rgb = ToneMap2((half3)BaseData.rgb);
	// 2. LUT contains color grading only (white balance + shadow/mid/highlight CC + sRGB)
	BaseData.rgb = ApplyColorGradingLUT3D(BaseData.rgb);
#else
	// Legacy path: direct tonemapping without color grading
	BaseData.rgb = ToneMap2((half3)BaseData.rgb);
	BaseData.rgb = Linear2sRGB((half3)BaseData.rgb);
#endif

	output.RT0 = BaseData;

	return output;
}

#endif
///