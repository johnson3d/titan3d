#ifndef _GPUSCENE_COMMON_INC_
#define _GPUSCENE_COMMON_INC_
#include "Math.cginc"

struct FGpuSceneDesc
{
	uint			ScreenAverageColorI;
	uint			AverageColorDivider;	
	float			ScreenAverageBrightness;	
	float			PrevScreenAverageBrightness;

	float			EyeAdapterTime;
	float			EyeAdapter;
	int				FreeGroupIndex;
};

RWStructuredBuffer<FGpuSceneDesc> GpuSceneDesc DX_AUTOBIND;
StructuredBuffer<FGpuSceneDesc> GpuSceneDescSRV DX_AUTOBIND;

struct FPointLight
{
    float4 PositionAndRadius;
    float4 ColorAndIntensity;
};

struct FRVTArray
{
	int SlotIndex;
	int ArrayIndex;
	float U;
	float V;
};

struct FRVTAtlas
{
	int SlotIndex;
	int ArrayIndex;
	float U;
	float V;
	float USize;
	float VSize;
};

half3 BRDFPointLight(half Roughness, half3 N, half3 H, half NoH, half LoH, half3 OptSpecShading)
{
	float3 NxH = cross(N, H);
	float OneMinusNoHSqr = dot(NxH, NxH);
	Roughness = max(0.031622h, Roughness);//6.1e-5;
	half r2 = Roughness * Roughness;//(half)max(0.001h, Roughness * Roughness);
	float n = NoH * r2;
	float d = r2 / (OneMinusNoHSqr + n * n);
	half NDF = (half)min(d * d, 16.0h);

	half Fc = 1.0 - LoH;
	half Fc2 = Fc * Fc;
	half Fc4 = Fc2 * Fc2;
	Fc = Fc4 * Fc;
	half FcInv = 1.0 - Fc;
	half Gm = -0.8h * r2 + 1.0h;
	Fc = min(1.0h, 50.0h * OptSpecShading.g) * Fc * Gm;
	half3 Fm = half3(Fc, Fc, Fc) + FcInv * OptSpecShading;

	return NDF * Fm;
}

half3 PointLightShading(FPointLight light, float3 WorldPos, half3 V, half3 N, half3 OptDiffShading, half3 OptSpecShading, half Roughness)
{
	half3 Lp = (half3)(light.PositionAndRadius.xyz - WorldPos);
	half DistSqr = dot(Lp, Lp);

	half AttenPL = rcp(DistSqr + 1.0h);
	half radiusSq = (half)light.PositionAndRadius.w * (half)light.PositionAndRadius.w;
	AttenPL = AttenPL * (half)Pow2(saturate(1.0h - (half)Pow2(DistSqr / radiusSq)));

	Lp = normalize(Lp);
	half NoLp = max(0.0h, dot(N, Lp));
	half3 Hp = normalize(Lp + V);
	half NoHp = max(0.0h, dot(N, Hp));
	half LoHp = max(0.0h, dot(Lp, Hp));

	half3 BaseShading = (OptDiffShading * pow(NoLp, -1.5h * Roughness + 2.0h) + BRDFPointLight(Roughness, N, Hp, NoHp, LoHp, OptSpecShading) * (half)sqrt(NoLp))
		* (half3)light.ColorAndIntensity.rgb * (half)light.ColorAndIntensity.a * AttenPL;
	return BaseShading;
}

#define TileSize 32
#define MaxNumOfPointLight 32

StructuredBuffer<FPointLight> GpuScene_PointLights DX_AUTOBIND;

struct FTileData
{
	float3 BoxMin;
	uint Pad0;
	float3 BoxMax;
	uint NumPointLight;
	//uint4 PointLights[NumOfPointLightDataArray];
	uint PointLights[MaxNumOfPointLight];
};

cbuffer cbPerGpuScene DX_AUTOBIND
{
	uint2		TileNum;
	uint		LightNum;
	uint		PixelNum;
	
	float		HdrMiddleGrey;//0.6
	float		HdrMinLuminance;//0.01
	float		HdrMaxLuminance;//16
	float		Exposure;//1.0f

	float		EyeAdapterTimeRange;
}

uint GetTileIndex(uint x, uint y)
{
	return TileNum.x * y + x;
}

struct FDispatchIndirectArg
{
	uint X;
	uint Y;
	uint Z;
	uint W;//for pad
};

#endif