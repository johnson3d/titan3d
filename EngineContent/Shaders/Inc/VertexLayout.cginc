#ifndef _VertexLayout_cginc_
#define _VertexLayout_cginc_

#include "GlobalDefine.cginc"

//CBuffers
#include "../CBuffer/VarBase_PerCamera.cginc"
#include "../CBuffer/VarBase_PerFrame.cginc"
#include "../CBuffer/VarBase_PerMaterial.cginc"
#include "../CBuffer/VarBase_PerViewport.cginc"
#include "../CBuffer/VarBase_PerMesh.cginc"
#include "../CBuffer/VarBase_PreFramePerMesh.cginc"

//Functions
#include "SysFunction.cginc"

FVSInstanceData GetInstanceData(VS_MODIFIER input);

#define FIX_NAN_NORMAL 1

MTL_OUTPUT Default_PSInput2Material(PS_INPUT input)
{
	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
    mtl.mNormal = input.Get_vNormal();
	mtl.mAbsSpecular = 0.0;
	// mtl.mAbsSpecular = 0.5h;
	mtl.mRough = 1.0h;
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

void DoDefaultVSModifier(inout VS_MODIFIER vert)
{

}

void DoDefaultPSMaterial(in PS_INPUT pixel, inout MTL_OUTPUT mtl)
{
	
}

Texture2D gDefaultTextue2D;
SamplerState gDefaultSamplerState;

float3 GetTerrainDiffuse(float2 uv, PS_INPUT input);
float3 GetTerrainNormal(float2 uv, PS_INPUT input);

#endif //_VertexLayout_cginc_