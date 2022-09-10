#ifndef _PICKED_INNER_
#define _PICKED_INNER_

#include "../../../Inc/VertexLayout.cginc"

#include "Material"
#include "MdfQueue"

#include "../../../Inc/SysFunctionDefImpl.cginc"

Texture2D gPickedSetUpTex DX_AUTOBIND;
SamplerState Samp_gPickedSetUpTex DX_AUTOBIND;

Texture2D gPickedBlurTex DX_AUTOBIND;
SamplerState Samp_gPickedBlurTex DX_AUTOBIND;

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;

	output.vPosition = float4(input.vPosition.xyz, 1.0f);
	output.vUV = input.vUV;

	return output;
}

struct PS_OUTPUT
{
	float2 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	float2 uv = input.vUV;
	float2 BaseData = gPickedSetUpTex.Sample(Samp_gPickedSetUpTex, uv).rg;
	float2 BlurredData = gPickedBlurTex.Sample(Samp_gPickedBlurTex, uv).rg;

	half2 Ifinal = half2(0.0h, 0.0h);
	/*if (BaseData.g - BlurredData.g > min(max(Pow2(100.0h * BaseData.g), 0.1h), 40.0h) * rcp((half)gZFar))
	{
		Ifinal = half2(BlurredData.r, BlurredData.g);
	}
	else
	{
		Ifinal = half2(0.0h, BaseData.g);
	}*/

	if (BlurredData.r - BaseData.r == 0.0h)
	{
		Ifinal = half2(0.0h, BaseData.g);
	}
	else
	{
		Ifinal = half2(1.0h, BlurredData.g);
	}

	output.RT0 = (float2)Ifinal;

	return output;
}

#endif
///