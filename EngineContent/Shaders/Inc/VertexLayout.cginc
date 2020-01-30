#ifndef _VertexLayout_cginc_
#define _VertexLayout_cginc_

#include "GlobalDefine.cginc"

struct VS_INPUT
{
	float3 vPosition : POSITION;
	float3 vNormal : NORMAL;
	float4 vTangent : TEXCOORD;
	float4 vColor : COLOR;
	float2 vUV : TEXCOORD1;
	float2 vLightMap : TEXCOORD2;
	uint4 vSkinIndex : TEXCOORD3;
	float4 vSkinWeight : TEXCOORD4;
	uint4 vTerrainIndex : TEXCOORD5;
	uint4 vTerrainGradient : TEXCOORD6;
	float3 vInstPos : TEXCOORD7;
	float4 vInstQuat : TEXCOORD8;
	float4 vInstScale : TEXCOORD9;
	uint4 vF4_1 : TEXCOORD10;
	float4 vF4_2 : TEXCOORD11;
	float4 vF4_3 : TEXCOORD12;

#if ShaderModel >= 4
	uint vInstanceId : SV_InstanceID;
#endif
};

struct PS_INPUT
{
    float4 vPosition		: SV_POSITION;
    float3 vNormal			: NORMAL;
	float4 vColor			: COLOR;

	float2 vUV				: TEXCOORD;
	float3 vWorldPos		: TEXCOORD1;   //the 4th channel is unused just for now;
	float4 vTangent			: TEXCOORD2;
	float4 vLightMap		: TEXCOORD3;
	
	float4 psCustomUV0	: TEXCOORD4;
	float4 psCustomUV1	: TEXCOORD5;
	float4 psCustomUV2	: TEXCOORD6;
	float4 psCustomUV3	: TEXCOORD7;
	float4 psCustomUV4	: TEXCOORD8;
	
	nointerpolation uint4 PointLightIndices	: TEXCOORD9;
	nointerpolation uint4 vF4_1 : TEXCOORD10;
	float4 vF4_2 : TEXCOORD11;
	float4 vF4_3 : TEXCOORD12;

	nointerpolation uint4 SpecialData : TEXCOORD13;
};

#if ShaderModel >= 4
struct VSInstantData
{
	matrix WorldMatrix;
	uint4 CustomData;
	uint4 PointLightIndices;
};

StructuredBuffer<VSInstantData> VSInstantDataArray : register(t13);
#endif

void Default_VSInput2PSInput(inout PS_INPUT output, VS_INPUT input)
{
	output.vPosition = float4(input.vPosition.xyz, 1);
    output.vNormal = input.vNormal;
	output.vTangent = input.vTangent;
	//output.vBinormal = input.vBinormal; cross
	output.vColor = input.vColor;
	output.vUV = input.vUV;
	output.vLightMap.xy = input.vLightMap;
	output.vWorldPos = input.vPosition;

	output.vF4_1 = input.vF4_1;
	output.vF4_2 = input.vF4_2;
	output.vF4_3 = input.vF4_3;
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
	mtl.mAbsSpecular = 0.1h;
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

#endif //_VertexLayout_cginc_