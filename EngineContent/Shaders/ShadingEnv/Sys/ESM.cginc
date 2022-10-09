#ifndef _ESM_
#define _ESM_

#include "../../Inc/VertexLayout.cginc"
#include "../../Inc/LightCommon.cginc"
#include "../../Inc/Math.cginc"

#include "Material"
#include "MdfQueue"

#include "../../Inc/SysFunctionDefImpl.cginc"

Texture2D GShadowMap DX_AUTOBIND;
SamplerState Samp_GShadowMap DX_AUTOBIND;

PS_INPUT VS_Main(VS_INPUT input)
{
	PS_INPUT output = (PS_INPUT)0;

	output.vPosition = float4(input.vPosition.xyz, 1.0f);
	output.vUV = input.vUV;

	return output;
}

struct PS_OUTPUT
{
	float4 RT0 : SV_Target0;//Debug..  Release-> float
};

PS_OUTPUT PS_Main(PS_INPUT input)
{
	PS_OUTPUT output = (PS_OUTPUT)0;

    //Debug..
    float blurnum = 3;
    float sigma = 0.4;
    float imgw = 1024;
    float imgh = 1024;
    float offset = 1;

	float2 uv = input.vUV;
	//float2 BaseData = gPickedSetUpTex.Sample(Samp_gPickedSetUpTex, uv).rg;
	//float2 BlurredData = gPickedBlurTex.Sample(Samp_gPickedBlurTex, uv).rg;

    float stepx = offset / imgw;
    float stepy = offset / imgh;

    float pi = 3.14159;
    float3 increamentalGaussian;
    increamentalGaussian.x = 1.0/(sqrt(2.0*pi)*sigma);
    increamentalGaussian.y= exp(-0.5/(sigma*sigma));
    increamentalGaussian.z = increamentalGaussian.y * increamentalGaussian.y;
        
    float4 avgValue = float4(0.0, 0.0, 0.0, 0.0);
    float coefficientSum = 0.0;
    
    avgValue +=GShadowMap.Sample(Samp_GShadowMap, uv)*increamentalGaussian.x;
    coefficientSum += increamentalGaussian.x;
    increamentalGaussian.xy *= increamentalGaussian.yz;
    
    for(int i = 1; i <= blurnum; i ++ )
    {
        for(int j = 1; j <= blurnum; j ++ )
        {
            avgValue +=GShadowMap.Sample(Samp_GShadowMap, uv + float2(i * stepx, j * stepy)) * increamentalGaussian.x;
            avgValue +=GShadowMap.Sample(Samp_GShadowMap, uv + float2(i * -stepx, j * stepy)) * increamentalGaussian.x;
            avgValue +=GShadowMap.Sample(Samp_GShadowMap, uv + float2(i * -stepx, j * -stepy)) * increamentalGaussian.x;
            avgValue +=GShadowMap.Sample(Samp_GShadowMap, uv + float2(i * stepx, j * -stepy)) * increamentalGaussian.x;

            coefficientSum+= 4.0*increamentalGaussian.x;
            increamentalGaussian.xy *= increamentalGaussian.yz;
        }

    }
	
    float4 texcolor = avgValue/coefficientSum;
    
    //texcolor.g =GShadowMap.Sample(Samp_GShadowMap, uv).r;
    //return texcolor;

	output.RT0 = texcolor;

	return output;
}

#endif
///