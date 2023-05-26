#ifndef _DUMMY_SHADING_
#define _DUMMY_SHADING_

#include "../Inc/VertexLayout.cginc"
#include "../Inc/GpuSceneCommon.cginc"

#include "../Inc/SysFunctionDefImpl.cginc"

PS_INPUT VS_Main(VS_INPUT input1)
{
	VS_MODIFIER input = VS_INPUT_TO_VS_MODIFIER(input1);
	
	PS_INPUT output = (PS_INPUT)0;
	Default_VSInput2PSInput(output, input);

	MTL_OUTPUT mtl = (MTL_OUTPUT)0;
	{
#ifdef MDFQUEUE_FUNCTION
		MdfQueueDoModifiers(output, input);
#endif

#ifdef DO_VS_MATERIAL
		DO_VS_MATERIAL(output, mtl);
#endif
	}

#if !defined(VS_NO_WorldTransform)
	output.vPosition.xyz += mtl.mVertexOffset;

	output.vWorldPos = mul(float4(output.vPosition.xyz, 1), WorldMatrix).xyz;
	output.vNormal = normalize(mul(float4(output.vNormal.xyz, 0), WorldMatrix).xyz);
	output.vTangent.xyz = normalize(mul(float4(output.vTangent.xyz, 0), WorldMatrix).xyz);
#endif
	output.vPosition = mul(float4(output.vWorldPos, 1), GetViewPrjMtx(true));

	return output;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	float result = (float)PickedID + (float)gZFar + (float)gViewportSizeAndRcp.x + Time + HdrMiddleGrey + DepthBiasPerSceneMesh + PreWorldMatrix[0];
	output.RT0 = float4(result, result, result, result);

	return output;
}

#endif
//