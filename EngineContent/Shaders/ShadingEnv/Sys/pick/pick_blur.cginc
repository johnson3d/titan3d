#include "../../../Inc/VertexLayout.cginc"

#include "Material"
#include "MdfQueue"

Texture2D SourceTexture;
SamplerState Samp_SourceTexture;

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;

	output.vPosition = float4(input.vPosition.xyz, 1.0f);
	output.vUV = input.vUV;

	output.psCustomUV0.xy = input.vUV + float2(gViewportSizeAndRcp.z, 0.0f);
	output.psCustomUV0.zw = input.vUV + float2(gViewportSizeAndRcp.z * 2.0f, 0.0f);
	output.psCustomUV1.xy = input.vUV - float2(gViewportSizeAndRcp.z, 0.0f);
	output.psCustomUV1.zw = input.vUV - float2(gViewportSizeAndRcp.z * 2.0f, 0.0f);

	output.psCustomUV2.xy = input.vUV + float2(0.0f, gViewportSizeAndRcp.w);
	output.psCustomUV2.zw = input.vUV + float2(0.0f, gViewportSizeAndRcp.w * 2.0f);
	output.psCustomUV3.xy = input.vUV - float2(0.0f, gViewportSizeAndRcp.w);
	output.psCustomUV3.zw = input.vUV - float2(0.0f, gViewportSizeAndRcp.w * 2.0f);

	return output;
}

struct PS_OUTPUT
{
	half2 RT0 : SV_Target0;
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

	half2 P0 = SourceTexture.Sample(Samp_SourceTexture, input.vUV).rg;

	half2 P1 = SourceTexture.Sample(Samp_SourceTexture, input.psCustomUV0.xy).rg;
	half2 P2 = SourceTexture.Sample(Samp_SourceTexture, input.psCustomUV0.zw).rg;
	half2 P3 = SourceTexture.Sample(Samp_SourceTexture, input.psCustomUV1.xy).rg;
	half2 P4 = SourceTexture.Sample(Samp_SourceTexture, input.psCustomUV1.zw).rg;

	half2 P5 = SourceTexture.Sample(Samp_SourceTexture, input.psCustomUV2.xy).rg;
	half2 P6 = SourceTexture.Sample(Samp_SourceTexture, input.psCustomUV2.zw).rg;
	half2 P7 = SourceTexture.Sample(Samp_SourceTexture, input.psCustomUV3.xy).rg;
	half2 P8 = SourceTexture.Sample(Samp_SourceTexture, input.psCustomUV3.zw).rg;

	half IdExpanded = max(max(max(max(max(max(max(max(P0.r, P1.r), P2.r), P3.r), P4.r), P5.r), P6.r), P7.r), P8.r);
	half DepthExpanded = min(min(min(min(min(min(min(min(P0.g, P1.g), P2.g), P3.g), P4.g), P5.g), P6.g), P7.g), P8.g);

	output.RT0 = half2(IdExpanded, DepthExpanded);

	return output;
}

///