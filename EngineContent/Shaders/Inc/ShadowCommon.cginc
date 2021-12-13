#ifndef	 _SHADOW_COMMON_INC_
#define _SHADOW_COMMON_INC_


struct ShadowFilterData
{
	Texture2D		mShadowMap;
	SamplerState	mShadowMapSampler;
	half			mViewer2ShadowDepth;
	float4			mShadowMapSizeAndRcp;
	half			mShadowTransitionScale;
};

half4 CalcShadowValue(half4 ShadowmapDepth, ShadowFilterData SFD)
{
	return saturate((ShadowmapDepth - SFD.mViewer2ShadowDepth) * SFD.mShadowTransitionScale + 1.0h);
}

half CalcShadowValue(half ShadowmapDepth, ShadowFilterData SFD)
{
	return saturate((ShadowmapDepth - SFD.mViewer2ShadowDepth) * SFD.mShadowTransitionScale + 1.0h);
}

half4 Fetch4SMValuePerRow(half2 TexelPos, half VerticalOffset, ShadowFilterData SFD)
{
	half4 PerRowSMValue = 0.0h;
	PerRowSMValue.x = (half)SFD.mShadowMap.SampleLevel(SFD.mShadowMapSampler, float2(TexelPos + half2(0.0h, VerticalOffset)) * SFD.mShadowMapSizeAndRcp.zw, 0).r;
	PerRowSMValue.y = (half)SFD.mShadowMap.SampleLevel(SFD.mShadowMapSampler, float2(TexelPos + half2(1.0h, VerticalOffset)) * SFD.mShadowMapSizeAndRcp.zw, 0).r;
	PerRowSMValue.z = (half)SFD.mShadowMap.SampleLevel(SFD.mShadowMapSampler, float2(TexelPos + half2(2.0h, VerticalOffset)) * SFD.mShadowMapSizeAndRcp.zw, 0).r;
	PerRowSMValue.w = (half)SFD.mShadowMap.SampleLevel(SFD.mShadowMapSampler, float2(TexelPos + half2(3.0h, VerticalOffset)) * SFD.mShadowMapSizeAndRcp.zw, 0).r;
	return CalcShadowValue(PerRowSMValue, SFD);
}


half PCF4x4(half2 alpha, half4 r0, half4 r1, half4 r2, half4 r3)
{
	half4 Sr;
	Sr.x = r0.x * (1.0h - alpha.x);
	Sr.x += r0.y;
	Sr.y = r1.x * (1.0h - alpha.x);
	Sr.y += r1.y;
	Sr.z = r2.x * (1.0h - alpha.x);
	Sr.z += r2.y;
	Sr.w = r3.x * (1.0h - alpha.x);
	Sr.w += r3.y;
	
	Sr.x += r0.z;
	Sr.x += r0.w * alpha.x;
	Sr.y += r1.z;
	Sr.y += r1.w * alpha.x;
	Sr.z += r2.z;
	Sr.z += r2.w * alpha.x;
	Sr.w += r3.z;
	Sr.w += r3.w * alpha.x;

	return min(1.0h, dot(Sr, half4(1.0h - alpha.y, 1.0h, 1.0h, alpha.y)) * (1.0h / 16.0h * 2.0h));
}


half NoFiltering(float2 SMUV, ShadowFilterData SFD)
{
	//return (half)SFD.mShadowMap.Sample(SFD.mShadowMapSampler, SMUV).r;
	//return SFD.mViewer2ShadowDepth;	
	return (half)CalcShadowValue((half)SFD.mShadowMap.Sample(SFD.mShadowMapSampler, SMUV).r, SFD);
}


half DoPCF4x4(float2 SMUV, ShadowFilterData SFD)
{
	half2 TexelPos = (half2)(SMUV * SFD.mShadowMapSizeAndRcp.xy);
	half2 alpha = frac(TexelPos);
	TexelPos = floor(TexelPos) - 1.5h;
	
	half4 SMValueR0 = Fetch4SMValuePerRow(TexelPos, 0.0h, SFD);
	half4 SMValueR1 = Fetch4SMValuePerRow(TexelPos, 1.0h, SFD);
	half4 SMValueR2 = Fetch4SMValuePerRow(TexelPos, 2.0h, SFD);
	half4 SMValueR3 = Fetch4SMValuePerRow(TexelPos, 3.0h, SFD);
	return PCF4x4(alpha, SMValueR0, SMValueR1, SMValueR2, SMValueR3);
}


#endif