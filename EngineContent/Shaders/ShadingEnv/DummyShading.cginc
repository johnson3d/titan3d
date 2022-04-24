#ifndef _DUMMY_SHADING_
#define _DUMMY_SHADING_

#include "../Inc/VertexLayout.cginc"
#include "../Inc/GpuSceneCommon.cginc"

#include "../Inc/SysFunctionDefImpl.cginc"

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;
	output.vPosition = output.vPosition / 0;

	return output;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	float result = (float)PickedID + (float)gZFar + (float)gViewportSizeAndRcp.x + Time + HdrMiddleGrey;
	output.RT0 = float4(result, result, result, result);

	return output;
}

#endif
//