#ifndef _PICKED_HOLLOW_BLEND_INNER_
#define _PICKED_HOLLOW_BLEND_INNER_

#include "../../../Inc/VertexLayout.cginc"

#include "Material"
#include "MdfQueue"

#include "../../../Inc/SysFunctionDefImpl.cginc"

Texture2D ColorBuffer DX_AUTOBIND;
SamplerState Samp_ColorBuffer DX_AUTOBIND;

Texture2D DepthBuffer DX_AUTOBIND;
SamplerState Samp_DepthBuffer DX_AUTOBIND;

Texture2D GPickedTex DX_AUTOBIND;
SamplerState Samp_GPickedTex DX_AUTOBIND;

float GetDepth(float2 uv)
{
    return DepthBuffer.SampleLevel(Samp_DepthBuffer, uv, 0).r;
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

    float2 uv = input.vUV.xy;
    float rtDepth = GetDepth(uv.xy);
    half PerPixelViewerDistance = (half) LinearFromDepth(rtDepth);
	
    half2 PickedData = (half2) GPickedTex.Sample(Samp_GPickedTex, uv.xy).rg;
    half PickedContrast = 1.0h;
    half3 PickedEdgeColor = 0.0h;
    half linearDepth = (half) (PerPixelViewerDistance / gZFar);
    if (PickedData.g - linearDepth > 0.0h)
    {
        PickedEdgeColor = half3(1.0h, 0.0h, 0.0h) * PickedContrast;
    }
    else
    {
        PickedEdgeColor = half3(0.0h, 1.0h, 0.0h) * PickedContrast;
    }
    
    float4 color = ColorBuffer.SampleLevel(Samp_ColorBuffer, uv, 0);
    half3 colorResult = lerp(color.rgb, PickedEdgeColor, PickedData.r);

	output.RT0.rgb = colorResult.rgb;
    output.RT0.a = color.a;

	return output;
}

#endif
///