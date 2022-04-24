#ifndef _VertexLayout_cginc_
#define _VertexLayout_cginc_

#include "GlobalDefine.cginc"

//#define USE_VS_Position 1
//#define USE_VS_Normal 1
//#define USE_VS_Tangent 1
//#define USE_VS_Color 1
//#define USE_VS_UV 1
//#define USE_VS_LightMap 1
//#define USE_VS_SkinIndex 1
//#define USE_VS_SkinWeight 1
//#define USE_VS_TerrainIndex 1
//#define USE_VS_TerrainGradient 1
//#define USE_VS_InstPos 1
//#define USE_VS_InstQuat 1
//#define USE_VS_InstScale 1
//#define USE_VS_F4_1 1
//#define USE_VS_F4_2 1
//#define USE_VS_F4_3 1

struct VS_INPUT
{
#if USE_VS_Position == 1
	VK_LOCATION(0) float3 vPosition : POSITION;
#endif

#if USE_VS_Normal == 1
	VK_LOCATION(1) float3 vNormal : NORMAL;
#endif

#if USE_VS_Tangent == 1
	VK_LOCATION(2) float4 vTangent : TEXCOORD;
#endif

#if USE_VS_Color == 1
	VK_LOCATION(3) float4 vColor : COLOR;
#endif

#if USE_VS_UV == 1
	VK_LOCATION(4) float2 vUV : TEXCOORD1;
#endif

#if USE_VS_LightMap == 1
	VK_LOCATION(5) float2 vLightMap : TEXCOORD2;
#endif

#if USE_VS_SkinIndex == 1
	VK_LOCATION(6) uint4 vSkinIndex : TEXCOORD3;
#endif

#if USE_VS_SkinWeight == 1
	VK_LOCATION(7) float4 vSkinWeight : TEXCOORD4;
#endif

#if USE_VS_TerrainIndex == 1
	VK_LOCATION(8) uint4 vTerrainIndex : TEXCOORD5;
#endif

#if USE_VS_TerrainGradient == 1
	VK_LOCATION(9) uint4 vTerrainGradient : TEXCOORD6;
#endif

#if USE_VS_InstPos == 1
	VK_LOCATION(10) float3 vInstPos : TEXCOORD7;
#endif

#if USE_VS_InstQuat == 1
	VK_LOCATION(11) float4 vInstQuat : TEXCOORD8;
#endif

#if USE_VS_InstScale == 1
	VK_LOCATION(12) float4 vInstScale : TEXCOORD9;
#endif

#if USE_VS_F4_1 == 1
	VK_LOCATION(13) uint4 vF4_1 : TEXCOORD10;
#endif

#if USE_VS_F4_2 == 1
	VK_LOCATION(14) float4 vF4_2 : TEXCOORD11;
#endif

#if USE_VS_F4_3 == 1
	VK_LOCATION(15) float4 vF4_3 : TEXCOORD12;
#endif

	VK_LOCATION(16) uint vVertexID : SV_VertexID;
	VK_LOCATION(17) uint vInstanceId : SV_InstanceID;
};

struct PS_INPUT
{
	VK_LOCATION(0) float4 vPosition		: SV_POSITION;
	VK_LOCATION(1) float3 vNormal			: NORMAL;
	VK_LOCATION(2) float4 vColor			: COLOR;

	VK_LOCATION(3) float2 vUV				: TEXCOORD;
	VK_LOCATION(4) float3 vWorldPos		: TEXCOORD1;   //the 4th channel is unused just for now;
	VK_LOCATION(5) float4 vTangent			: TEXCOORD2;
	VK_LOCATION(6) float4 vLightMap		: TEXCOORD3;
	
	VK_LOCATION(7) float4 psCustomUV0	: TEXCOORD4;
	VK_LOCATION(8) float4 psCustomUV1	: TEXCOORD5;
	VK_LOCATION(9) float4 psCustomUV2	: TEXCOORD6;
	VK_LOCATION(10) float4 psCustomUV3	: TEXCOORD7;
	VK_LOCATION(11) float4 psCustomUV4	: TEXCOORD8;
	
	VK_LOCATION(12) nointerpolation uint4 PointLightIndices	: TEXCOORD9;
	VK_LOCATION(13) nointerpolation uint4 vF4_1 : TEXCOORD10;
	VK_LOCATION(14) float4 vF4_2 : TEXCOORD11;
	VK_LOCATION(15) float4 vF4_3 : TEXCOORD12;

	VK_LOCATION(16) nointerpolation uint4 SpecialData : TEXCOORD13;
};

#if HW_VS_STRUCTUREBUFFER
struct VSInstantData
{
	matrix WorldMatrix;
	uint4 CustomData;
	uint4 PointLightIndices;
};

StructuredBuffer<VSInstantData> VSInstantDataArray DX_NOBIND;//: register(t13);
#endif

void Default_VSInput2PSInput(inout PS_INPUT output, VS_INPUT input)
{
#if USE_VS_Position == 1
	output.vPosition = float4(input.vPosition.xyz, 1);
#endif

#if USE_VS_Normal == 1
    output.vNormal = input.vNormal;
#endif

#if USE_VS_Tangent == 1
	output.vTangent = input.vTangent;
#endif

	//output.vBinormal = input.vBinormal; cross

#if USE_VS_Color == 1
	output.vColor = input.vColor;
#endif

#if USE_VS_UV == 1
	output.vUV = input.vUV;
#endif

#if USE_VS_LightMap == 1
	output.vLightMap.xy = input.vLightMap;
#endif

#if USE_VS_Position == 1
	output.vWorldPos = input.vPosition;
#endif

#if USE_VS_F4_1 == 1
	output.vF4_1 = input.vF4_1;
#endif

#if USE_VS_F4_2 == 1
	output.vF4_2 = input.vF4_2;
#endif

#if USE_VS_F4_3 == 1
	output.vF4_3 = input.vF4_3;
#endif
}

struct MTL_OUTPUT
{
	float3 mAlbedo;
	float3 mNormal;
	float   mMetallic;
	float   mRough;   //in the editer,we call it smoth,so rough = 1.0f - smoth;
	float   mAbsSpecular;
	float   mTransmit;
	float3   mEmissive;
	float   mFuzz;
	float   mIridescence;
	float   mDistortion;
	float   mAlpha;
	float   mAlphaTest;
	float3 mVertexOffset;
	float3 mSubAlbedo; 
	float   mAO;
	float   mMask;

	float3 mShadowColor;
	float   mDeepShadow;
	float3 mMoodColor;
};

MTL_OUTPUT Default_PSInput2Material(PS_INPUT input)
{
	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	mtl.mNormal = input.vNormal;
	mtl.mAbsSpecular = 0.0h;
	//mtl.mEmissive = float3(0,0,0);
	//mtl.mSubAlbedo = half3(0.3h, 0.3h, 0.3h);
	mtl.mAO = 1.0h;
	mtl.mAlpha = 1.0h;

	mtl.mShadowColor = half3(0.5h, 0.5h, 0.5h);
	mtl.mDeepShadow = 1.0h;
	mtl.mMoodColor = half3(1.0h, 0.5h, 0.5h);

	
#ifdef MTL_ID_HAIR
	mtl.mTransmit = 1.0h;
	mtl.mMetallic = 1.0h;
	mtl.mRough = 0.65h;
#endif
	return mtl;
}

void DoDefaultVSModifier(inout VS_INPUT vert)
{

}

void DoDefaultPSMaterial(in PS_INPUT pixel, inout MTL_OUTPUT mtl)
{
	
}

Texture2D gDefaultTextue2D;
SamplerState gDefaultSamplerState;

float3 GetTerrainDiffuse(float2 uv, PS_INPUT input);

#endif //_VertexLayout_cginc_