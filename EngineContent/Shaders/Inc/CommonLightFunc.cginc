#ifndef	 _CommonLightFunc_FXH_
#define _CommonLightFunc_FXH_

half GetTexMipLevelFromRoughness(half Roughness, half MipMaxLevel)
{
	//return max(0.0, MipMaxIndex + log2(Roughness) * 1.5
	return MipMaxLevel * Roughness;
}

half3 EnvBRDFMobile(half3 LightColorSpec, half3 OptSpecShading, half Roughness, half NoV)
{
	//the algorithm is inspired by Call of Duty: Black Ops II;
	half4 coeff0 = half4(-1.0, -0.0275, -0.572, 0.022);
	half4 coeff1 = half4(1.0, 0.0425, 1.04, -0.04);

	half4 coeff2 = Roughness * coeff0 + coeff1;
	half coeff3 = min(coeff2.x * coeff2.x, exp2(-9.28 * NoV)) * coeff2.x + coeff2.y;
	half2 Reflectance = half2(-1.04, 1.04) * coeff3 + coeff2.zw;

	half3 TempBRDF = OptSpecShading * Reflectance.x + half3(Reflectance.y, Reflectance.y, Reflectance.y);
	return LightColorSpec * TempBRDF;
}

/*!	Pixel Lighting : Phong
*/
void	SimpleLight_Diffuse( out half3 LightShadingDiffuse, half3 N, half3 L, half3 LightColorDiffuse, half SSS)
{
	//// 
	//float	fNdotL		= max( 0.0f, dot( N, L) );
	//if(fNdotL==0)
	//{
	//	OutDiffuse		= 0;
	//	return 0;
	//}

	//OutDiffuse		= fNdotL * InDiffuseColor;
	//return fNdotL;

	//we now use enegy conservation diffuse shading model in simple shading;
	/*float NoL = saturate(dot(N, L));
	float LightIntensity = saturate((NoL + SSS) / Square(1.0f + SSS)) * 0.31831f;
	LightShadingDiffuse = LightColorDiffuse * LightIntensity;*/

	half NoL = (half)saturate(dot(N, L));
	LightShadingDiffuse = half3(LightColorDiffuse.r * NoL, LightColorDiffuse.g * NoL, LightColorDiffuse.b * NoL);
}

//no fresnel function;
half BRDF_BlinnPhongApprox(half Roughness, half NoH)
{
	half r2 = Roughness * Roughness;			
	// avoid underflow in FP16, next sqr should be bigger than 6.1e-5;
	r2 = max(r2, 0.008);						
	half r4 = r2 * r2;						
	half r4_rcp = 1.0 / r4;					
	// spherical gaussian approximation: pow( NoH, Shininess ) ~= exp( (Shininess + 0.775) * (NoH - 1) )
	// BlinnPhong: Shininess = 0.5 / r4 - 0.5
	// 0.5 / ln(2) = 0.72134752 , 0.275 / ln(2) = 0.39674113;
	half SpecIntensity = 0.72134752 * r4_rcp + 0.39674113;
	half I_SpecDistrib = r4_rcp * (half)exp2(SpecIntensity * NoH - SpecIntensity);
	//avoid overflow/underflow on Mali GPUs;
	return min(I_SpecDistrib, r4_rcp);
}

half3 BRDFMobile(half3 LightColorSpec, half3 MtlColorSpec, half Roughness, half NoH, half HoV)
{
//#ifdef NOF
	//blinn-phong-based;
	//half r2 = Roughness * Roughness;
	//half r4 = r2 * r2;
	////we are using fp16 precision for mobile platform;
	//r4 = max(r4, 0.000064);
	//half SpecIntensity = 1.0 / r4;
	//half I_SpecDistrib = SpecIntensity * exp2(SpecIntensity * NoH - SpecIntensity);

	//half Fc = 1.0 - HoV;
	//half Fc2 = Fc * Fc;
	//Fc = Fc2 * Fc2;
	//half FcInv = 1.0 - Fc;
	//half3 FresnelMobile = half3(Fc, Fc, Fc) + FcInv * MtlColorSpec;
	//return half3(I_SpecDistrib, I_SpecDistrib, I_SpecDistrib) * FresnelMobile * LightColorSpec;

	//cook-torrance-based;
	half r2 = Roughness * Roughness;
	half r4 = r2 * r2;
	//fp16 min = 0.000061 and avoid dividing 0;
	r4 = max(r4, 0.000064);
	half r4Sub1 = r4 - 1.0;
	half denom = NoH * r4Sub1 + 1.0;
	half I_SpecDistrib = min(r4 / (denom * denom), 15625.0);

	return half3(I_SpecDistrib, I_SpecDistrib, I_SpecDistrib) * MtlColorSpec * LightColorSpec;
/*
#else
	//cook-torrance-based;
	half r2 = Roughness * Roughness;
	half r4 = r2 * r2;
	//fp16 min = 0.000061 and avoid dividing 0;
	r4 = max(r4, 0.000064);
	half r4Sub1 = r4 - 1.0;
	half denom = NoH * r4Sub1 + 1.0;
	half I_SpecDistrib = min(r4 / (denom * denom), 15625.0);

	////use SG to optimize fresnel function;
	//half Fc = -5.55473 * HoV - 6.98316;
	//Fc = exp2(Fc * HoV);

	half Fc = 1.0 - HoV;
	half Fc2 = Fc * Fc;
	Fc = Fc2 * Fc2;
	half FcInv = 1.0 - Fc;
	half3 FresnelMobile = half3(Fc, Fc, Fc) + FcInv * MtlColorSpec;
	return half3(I_SpecDistrib, I_SpecDistrib, I_SpecDistrib) * FresnelMobile * LightColorSpec;

	//////high quality brdf;
	//half r2 = Roughness * Roughness;
	//half r4 = r2 * r2;
	////we are using fp16 precision for mobile platform;
	//r4 = max(r4, 0.000064);
	//half r4Sub1 = r4 - 1.0;
	//half denom = NoH * NoH * r4Sub1 + 1.0;
	//half I_SpecDistrib = r4 / (denom * denom);

	//half Fc = 1.0 - HoV;
	//half Fc2 = Fc * Fc;
	//half Fc4 = Fc2 * Fc2;
	//Fc = Fc4 * Fc;
	//half FcInv = 1.0 - Fc;
	//half3 FresnelMobile = half3(Fc, Fc, Fc) + FcInv * MtlColorSpec;
	//return half3(I_SpecDistrib, I_SpecDistrib, I_SpecDistrib) * FresnelMobile * LightColorSpec;
#endif
*/
}

//half3 BRDFMobileUFO(half Roughness, half3 N, half3 H, half NoH, half LoH, half3 OptSpecShading)
//{
//	float3 NxH = cross(N, H);
//	float OneMinusNoHSqr = dot(NxH, NxH);
//
//	half r2 = max(0.001f, Roughness * Roughness);
//	float n = NoH * r2;
//	float d = r2 / (OneMinusNoHSqr + n * n);
//	half NDF = min(d* d, 65504.0) * (Roughness * 0.25f + 0.25f);
//
//	half Fc = 1.0 - LoH;
//	half Fc2 = Fc * Fc;
//	half Fc4 = Fc2 * Fc2;
//	Fc = Fc4 * Fc;
//	half FcInv = 1.0 - Fc;
//	half3 Fm = half3(Fc, Fc, Fc) + FcInv * OptSpecShading;
//	return Fm * NDF;
//}

//Shininess==>Roughness;
void SimpleLight_Specular(out half3 MobileShadingSpec, half3 L, half3 V, half3 N, half3 LightColorSpec, 
	half3 MtlColorSpec, half Shininess)
{
	// float3 R = reflect(-L, N);
	// MobileShadingSpec = pow(max(dot(R, V), 0.0f), Shininess) * LightColorSpec;
	
	/*half3 H = normalize(V + L);
	half NoH = saturate(dot(N, H));
	MobileShadingSpec = pow(NoH, Shininess) * LightColorSpec;*/
	
	//half3 H = normalize(V + L);
	//half NoH = saturate(dot(N, H));
	//MobileShadingSpec = LightColorSpec * BRDF_BlinnPhongApprox(Shininess, NoH);

	half3 H = (half3)normalize(V + L);
	half NoH = (half)saturate(dot(N, H));
	half HoV = (half)saturate(dot(H, V));

	MobileShadingSpec = BRDFMobile(LightColorSpec, MtlColorSpec, Shininess, NoH, HoV);

	/*half3 R = reflect(-L, N);
	half RoV = saturate(dot(R, V));
	MobileShadingSpec = LightColorSpec * BRDF_PhongApprox(Shininess, RoV);*/
	
}

/*!	Pixel Lighting : Light-Distance Auttenuation
*/
float Attenuation ( float distance, float range, float a, float b, float c )
{
	//float Atten = 1.0f / ( a * distance * distance + b * distance + c );
	//float Atten = 1.0f / ( b * distance + c );
	//float Atten = 1.0f / ( c );

	float Atten = 1.0f - (distance / range);

	//Atten = pow(Atten, a);
	// Use the step() intrinsic to clamp light to zero out of its defined range

	return step(distance,range) * saturate( Atten );
}

float AttenuationTerm_Beast(float3 LightPos, float3 Pos, float Range, float3 Atten)
{
	//falloff = ( 1 / (Constant + Linear * r + Quadratic * r 2 ))
	float3 DistVec = LightPos - Pos;
	float DistSquare = dot(DistVec, DistVec);
	float r = sqrt(DistSquare);
	return (1 / (Atten.x + Atten.y*r + Atten.z*DistSquare));
}

float AttenuationTerm(float3 LightPos, float3 Pos, float Range, float3 Atten)
{
	float3 v = LightPos - Pos;
	float d2 = dot(v, v);
	if(d2<Range*Range)
	{
		float d = sqrt(d2);
		return 1 / dot(Atten, float3(1, d, d2));
	}
	else
	{
		return 0;
	}
}

//(max((maxRange - distance), 0) / maxRange) ^ exponent;
half CalcAttenForBake(half3 PointLitViewPos, half3 PixelViewPos, half MaxLitRange, half exponent)
{
	half3 DistVec = PointLitViewPos - PixelViewPos;
	half DistSquare = dot(DistVec, DistVec);
	if(DistSquare<MaxLitRange*MaxLitRange)
	{
		half Obj2LitDist = (half)sqrt(DistSquare);
		return (half)pow((max((MaxLitRange - Obj2LitDist), 0.0f) / MaxLitRange), exponent);
	}
	else
	{
		return 0;
	}
}

half FallOff_Exp(half3 LitPosV, half3 PixelPosV, half RadiusRcp, half Exp)
{
	half3 P2LVec = LitPosV - PixelPosV;
	P2LVec = P2LVec * RadiusRcp;
	half DistNormalized = dot(P2LVec, P2LVec);
	half Power = (half)saturate(1.0 - DistNormalized);
	return (half)pow(Power, Exp);
}

//formula: Attenuation = saturate(1.0f - (distance / LightRadius)^4)^2 / (distance^2 + 1.0f);
half FallOff_InverseSquare(half3 LitPosV, half3 PixelPosV, half RadiusRcp)
{
	half3 P2LVec = LitPosV - PixelPosV;
	half P2LDistSqr = dot(P2LVec, P2LVec);
	half FallOffDenomRcp = 1.0 / (P2LDistSqr + 1.0);
	half FallOffNumer = P2LDistSqr * RadiusRcp * RadiusRcp;
	FallOffNumer = (half)saturate(1.0 - FallOffNumer * FallOffNumer);
	return FallOffNumer * FallOffNumer * FallOffDenomRcp;
}

#define SHADOW_EPSILON	0.00005
float GetShadowDepth(Texture2D tex, SamplerState samp, float2 tcShadow)
{
	float lightSpaceDepth = tex.Sample(samp, tcShadow).r;
	return lightSpaceDepth + SHADOW_EPSILON	;
}
//static const float4 g_shadowOffsets[2] = {
//	{ -0.5, -0.5, -0.5, 0.5 },
//	{ 0.5, -0.5, 0.5, 0.5 }
//};

/*
float GetShadowPCF(Texture2D tex, sampler2D samp, float4 tcShadow, float2 sampSize)
{
	float4 g_shadowOffsets[2] = {
		float4( -0.5, -0.5, -0.5, 0.5 ),
		float4( 0.5, -0.5, 0.5, 0.5 )
	};
	tcShadow.z -= SHADOW_EPSILON;

	float lightAmount = 0;
	sampSize = 1/sampSize;
	for(int i = 0; i < 2; i++) {
		float4 tc = float4(tcShadow.xy + g_shadowOffsets[i].xy*sampSize, tcShadow.zw);
		lightAmount += vise_tex2Dproj(tex, samp, tc);
		tc = float4(tcShadow.xy + g_shadowOffsets[i].zw*sampSize, tcShadow.zw);
		lightAmount += vise_tex2Dproj(tex, samp, tc);
	}
	lightAmount *= 0.25f;
	//float lightAmount = vise_tex2Dproj(texShadow, tcShadow).x;
	return lightAmount;
}
*/

float GetShadowDepthVSM(Texture2D tex, SamplerState samp, float2 tcShadow, float t)
{
	//float4 moments = vise_tex2D(tex, samp, tcShadow.xy); // Standard shadow map comparison 
	float2 moments = tex.Sample(samp, tcShadow.xy).xy; // Standard shadow map comparison 
	float lit_factor = (t <= moments.x); // Variance shadow mapping 
	float E_x2 = moments.y; 
	float Ex_2 = moments.x * moments.x; 
	float variance = max(E_x2 - Ex_2, 0.000001); 
	//float variance = max(E_x2 - Ex_2, 0.00000001); 
	//float variance = max(E_x2 - Ex_2, 0.000001); 
	float m_d = (t - moments.x); 
	float p_max = variance / (variance + m_d * m_d); // Adjust the light color based on the shadow attenuation 
	return max(lit_factor, p_max); 
}
float linstep(float min, float max, float v)
{
    return clamp((v - min) / (max - min), 0, 1);
}
// Light bleeding reduction
float LBR(float p, float LBRAmount)
{
    // Lots of options here if we don't care about being an upper bound.
    // Use whatever falloff function works well for your scene.
    return linstep(LBRAmount, 1, p);
    //return smoothstep(LBRAmount, 1, p);
}

void ShadowMapping( int shadowType, float4 viewPos, float4x4 lightViewProj, Texture2D texShadowDepth, SamplerState txShadowDepth, float LBRAmount, float2 sampSize, out float lightAmount, out float lightAmbientAmount )
{
	lightAmount = 1;
	lightAmbientAmount = 1.0f;
	float4 lightSpacePos = mul( viewPos, lightViewProj);
	lightSpacePos /= lightSpacePos.w;
	float4 tcShadow = lightSpacePos;
#ifdef D3D_EFFECT
	tcShadow.xy = tcShadow.xy * 0.5 + 0.5;
	tcShadow.y = 1.0f - tcShadow.y;
#else
	#ifdef METAL
		tcShadow.xy = tcShadow.xy * 0.5 + 0.5;
		tcShadow.y = 1.0f - tcShadow.y;
	#else
		tcShadow.xy = tcShadow.xy * 0.5 + 0.5;
		tcShadow.z = tcShadow.z * 0.5 + 0.5;
	#endif
#endif

	if(shadowType==1)			// Standard
	{
		//lightAmount = GetShadowPCF(texShadowDepth, txShadowDepth, tcShadow, sampSize);
		lightAmbientAmount = lightAmount;
	}
	else if(shadowType==2)		// VSM
	{
		float t = lightSpacePos.z / lightSpacePos.w;
		lightAmount = GetShadowDepthVSM(texShadowDepth, txShadowDepth, tcShadow.xy, t);
		lightAmount = LBR(lightAmount, LBRAmount);
		lightAmbientAmount = lightAmount;
	}
}


#ifdef D3D11
float CalcShadowFactor(SamplerComparisonState TexSampler,
	Texture2D ShadowDepthMap,
	float4 PixelPosInNDCSpaceofLight, float UVStridePerPixel)
{
	float OffsetU = 0.0f;
	float OffsetV = 0.0f;
	float NotInShadow = 0.0f;
	[unroll]
	for(OffsetV = -0.5f; OffsetV <= 0.5f; OffsetV += 1.0f)
		for (OffsetU = -0.5f; OffsetU <= 0.5f; OffsetU += 1.0f)
		{
			NotInShadow += ShadowDepthMap.SampleCmpLevelZero(TexSampler,
				PixelPosInNDCSpaceofLight.xy + float2(OffsetU, OffsetV) * UVStridePerPixel, PixelPosInNDCSpaceofLight.z);
		}
	NotInShadow = NotInShadow / 4.0f;

	return NotInShadow;
	//// Complete projection by doing division by w.
	//shadowPosH.xyz /= shadowPosH.w;

	//// Depth in NDC space.
	//float depth = shadowPosH.z;

	//float dx = pixelSize;

	//float percentLit = 0.0f;
	//const float2 offsets[9] =
	//{
	//	float2(-dx,  -dx), float2(0.0f,  -dx), float2(dx,  -dx),
	//	float2(-dx, 0.0f), float2(0.0f, 0.0f), float2(dx, 0.0f),
	//	float2(-dx,  +dx), float2(0.0f,  +dx), float2(dx,  +dx)
	//};

	//[unroll]
	//for (int i = 0; i < 9; ++i)
	//{
	//	percentLit += shadowMap.SampleCmpLevelZero(samShadow,
	//		shadowPosH.xy + offsets[i], depth).r;
	//}

	//return percentLit /= 9.0f;
}
#endif


#endif // CommonLightFunc_FXH_