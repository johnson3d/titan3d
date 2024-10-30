#ifndef _DUMMY_SHADING_
#define _DUMMY_SHADING_

#include "../Inc/VertexLayout.cginc"
#include "../Inc/GpuSceneCommon.cginc"
#include "../CBuffer/VarBase_PerSkinMesh.cginc"
#include "../Inc/SysFunctionDefImpl.cginc"

float GetReferValue()
{
    float result = (float) PickedID; //cbPerMesh
    result += (float) gZFar; //cbPerCamera
    result += (float) gViewportSizeAndRcp.x; //cbPerViewport
    result += Time; //cbPerFrame
    result += HdrMiddleGrey; //cbPerGpuScene
    result += MaterialRenderFlags; //cbPerMaterial
    result += AbsBonePos[0].x; //cbSkinMesh
    return result;
}

PS_INPUT VS_Main(VS_INPUT input1)
{
	VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
	
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	{
#ifdef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif

#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif
	}

#if !defined(VS_NO_WorldTransform)
	output.vPosition.xyz += mtl.mVertexOffset;

	output.vWorldPos = mul(float4(output.vPosition.xyz, 1), WorldMatrix).xyz;
	output.vNormal = normalize(mul(float4(output.vNormal.xyz, 0), WorldMatrix).xyz);
	output.vTangent.xyz = normalize(mul(float4(output.vTangent.xyz, 0), WorldMatrix).xyz);
#endif
	output.vPosition = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(true));
    output.vPosition.x = GetReferValue();

	return output;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

    float result = GetReferValue();
    
	//todo: use other cbuffer

	output.RT0 = float4(result, result, result, result);

	return output;
}

#endif
//