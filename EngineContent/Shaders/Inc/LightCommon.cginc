#ifndef	 _LIGHT_COMMON_INC_
#define _LIGHT_COMMON_INC_

#include "Math.cginc"

////////////////////// high-level begin ///////////////////////////////

float4 Pow4( float4 x )
{
	float4 xx = x*x;
	return xx * xx;
}

float Pow5( float x )
{
	float xx = x*x;
	return xx * xx * x;
}

// GGX / Trowbridge-Reitz
// [Walter et al. 2007, "Microfacet models for refraction through rough surfaces"]
float D_GGX( float a2, float NoH )
{
	float d = ( NoH * a2 - NoH ) * NoH + 1;	// 2 mad
	return a2 / ( Pi*d*d );					// 4 mul, 1 rcp
}
// Appoximation of joint Smith term for GGX
// [Heitz 2014, "Understanding the Masking-Shadowing Function in Microfacet-Based BRDFs"]
float Vis_SmithJointApprox( float a2, float NoV, float NoL )
{
	float a = sqrt(a2);
	float Vis_SmithV = NoL * ( NoV * ( 1 - a ) + a );
	float Vis_SmithL = NoV * ( NoL * ( 1 - a ) + a );
	return 0.5 * rcp( Vis_SmithV + Vis_SmithL );
}
// [Schlick 1994, "An Inexpensive BRDF Model for Physically-Based Rendering"]
float3 F_Schlick( float3 SpecularColor, float VoH )
{
	float Fc = Pow5( 1 - VoH );					// 1 sub, 3 mul
	//return Fc + (1 - Fc) * SpecularColor;		// 1 add, 3 mad
	
	// Anything less than 2% is physically impossible and is instead considered to be shadowing
	return saturate( 50.0 * SpecularColor.g ) * Fc + (1 - Fc) * SpecularColor;	
}

float3 SpecularGGX( float Roughness, float3 SpecularColor, float NoH, float NoV, float NoL, float VoH )
{
	float a2 = Pow4( Roughness );
	// float Energy = EnergyNormalization( a2, Context.VoH, AreaLight );
	
	// Generalized microfacet specular
	float D = D_GGX( a2, NoH );
	// float D = D_GGX( a2, Context.NoH ) * Energy;
	float Vis = Vis_SmithJointApprox( a2, NoV, NoL );
	float3 F = F_Schlick( SpecularColor, VoH );

	return (D * Vis) * F;
}

////////////////////// high-level end ///////////////////////////////

half GetTexMipLevelFromRoughness(half Roughness, half MipMaxLevel)
{
	//return max(0.0h, MipMaxLevel + log2(Roughness));
	return MipMaxLevel * Roughness;
}

half3 CalcSphereMapUV(half3 VrN, half roughness, half MipMaxLevel)
{
	half TempY = VrN.y + 1.0h;
	half SphereMapUVOffset = 0.5h / sqrt(VrN.x * VrN.x + VrN.z * VrN.z + TempY * TempY);

	half3 SphereMapUV = half3(0.0h, 0.0h, 0.0h);
	SphereMapUV.xy = half2(VrN.x * SphereMapUVOffset + 0.5h, 0.5h - VrN.z * SphereMapUVOffset);
	SphereMapUV.z = GetTexMipLevelFromRoughness(roughness, MipMaxLevel);
	return SphereMapUV;
}

float GetSpecularOcclusion(float NoV, float RoughnessSq, float AO)
{
	return saturate( pow( NoV + AO, RoughnessSq ) - 1 + AO );
}

half3 EnvBRDF( half3 LightColorSpec, half3 OptSpecShading, half Roughness, half NoV, Texture2D PreIntegrateGF, SamplerState samp )
{
	// Importance sampled preintegrated G * F
	float2 AB = PreIntegrateGF.Sample(samp, float2( NoV, Roughness )).rg;

	// Anything less than 2% is physically impossible and is instead considered to be shadowing 
	float3 GF = OptSpecShading * AB.x + saturate( 50.0 * OptSpecShading.g ) * AB.y;
	return GF * LightColorSpec;
}

half3 EnvBRDFMobile(half3 LightColorSpec, half3 OptSpecShading, half Roughness, half NoV)
{
	//the algorithm is inspired by Call of Duty: Black Ops II;
	half4 coeff0 = half4(-1.0h, -0.0275h, -0.572h, 0.022h);
	half4 coeff1 = half4(1.0h, 0.0425h, 1.04h, -0.04h);

	half4 coeff2 = Roughness * coeff0 + coeff1;
	half coeff3 = min(coeff2.x * coeff2.x, exp2(-9.28h * NoV)) * coeff2.x + coeff2.y;
	half2 Reflectance = half2(-1.04h, 1.04h) * coeff3 + coeff2.zw;
	Reflectance.y *= min(1.0h, 50.0h * OptSpecShading.g);

	half3 TempBRDF = OptSpecShading * Reflectance.x + half3(Reflectance.y, Reflectance.y, Reflectance.y);
	return LightColorSpec * TempBRDF;
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

half3 BRDFMobileDeprecated(half3 LightColorSpec, half3 MtlColorSpec, half Roughness, half NoH, half HoV)
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

half3 BRDFMobileUFO(half Roughness, half3 N, half3 H, half NoH, half LoH, half3 OptSpecShading)
{
	float3 NxH = cross(N, H);
	float OneMinusNoHSqr = dot(NxH, NxH);

	half r2 = (half)max(0.001h, Roughness * Roughness);
	float n = NoH * r2;
	float d = r2 / (OneMinusNoHSqr + n * n);
	half NDF = (half)min(d * d, 65504.0);

	half Fc = 1.0 - LoH;
	half Fc2 = Fc * Fc;
	half Fc4 = Fc2 * Fc2;
	Fc = Fc4 * Fc;
	half FcInv = 1.0 - Fc;
	half3 Fm = half3(Fc, Fc, Fc) + FcInv * OptSpecShading;

	return NDF * Fm * (Roughness + 1.0h);
}

half3 BRDFMobileSimple(half Roughness, half3 N, half3 H, half NoH, half3 OptSpecShading)
{
	float3 NxH = cross(N, H);
	float OneMinusNoHSqr = dot(NxH, NxH);
	Roughness = max(0.031622h, Roughness);
	half r2 = Roughness * Roughness;
	float n = NoH * r2;
	float d = r2 / (OneMinusNoHSqr + n * n);
	half NDF = (half)min(d * d, 64.0h);
	return NDF * OptSpecShading;
}


half3 BRDFMobile(half Roughness, half3 N, half3 H, half NoH, half LoH, half NoV, half NoL, half3 OptSpecShading)
{
	float3 NxH = cross(N, H);
	float OneMinusNoHSqr = dot(NxH, NxH);
	Roughness = max(0.031622h, Roughness);//6.1e-5;
	half r2 = Roughness * Roughness;//(half)max(0.001h, Roughness * Roughness);
	float n = NoH * r2;
	float d = r2 / (OneMinusNoHSqr + n * n);
	//half NDF = (half)min(d * d, 65504.0);
	half NDF = (half)min(d * d, 16.0h);
	
	half Fc = 1.0 - LoH;
	half Fc2 = Fc * Fc;
	half Fc4 = Fc2 * Fc2;
	Fc = Fc4 * Fc;
	half FcInv = 1.0 - Fc;
	half Gm = -0.8h * r2 + 1.0h;
	Fc = min(1.0h, 50.0h * OptSpecShading.g) * Fc * Gm;
	half3 Fm = half3(Fc, Fc, Fc) + FcInv * OptSpecShading;

	/*half Gv = NoV + sqrt(NoV * (NoV - NoV * r2) + r2);
	half Gl = NoL + sqrt(NoL * (NoL - NoL * r2) + r2);
	half Gm = min(1.0h, rcp(Gv * Gl));*/
	
	return NDF * Fm;
}

half3 BRDFMobilePointLight(half Roughness, half3 N, half3 H, half NoH, half LoH, half3 OptSpecShading)
{
	float3 NxH = cross(N, H);
	float OneMinusNoHSqr = dot(NxH, NxH);
	Roughness = max(0.031622h, Roughness);//6.1e-5;
	half r2 = Roughness * Roughness;//(half)max(0.001h, Roughness * Roughness);
	float n = NoH * r2;
	float d = r2 / (OneMinusNoHSqr + n * n);
	half NDF = (half)min(d * d, 16.0h);

	half Fc = 1.0 - LoH;
	half Fc2 = Fc * Fc;
	half Fc4 = Fc2 * Fc2;
	Fc = Fc4 * Fc;
	half FcInv = 1.0 - Fc;
	half Gm = -0.8h * r2 + 1.0h;
	Fc = min(1.0h, 50.0h * OptSpecShading.g) * Fc * Gm;
	half3 Fm = half3(Fc, Fc, Fc) + FcInv * OptSpecShading;

	return NDF * Fm;
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

half LambertDiffuse(half NoL)
{
	return NoL * (1.0h / Pi);
}

half RetroDiffuseMobile(half NoL, half NoV, half LoH, half Roughness)
{
	half Fd90 = 0.5h + (half)(LoH * LoH) * Roughness * 2.0h;
	half Sl = lerp(1.0h, Fd90, (half)Pow5(1.0h - NoL));
	half Sv = lerp(1.0h, Fd90, (half)Pow5(1.0h - NoV));
	return Sl * Sv * NoL;

	//return NoL;
}



half HairBRDF(half roughness, half shift)
{
	roughness = max(roughness, 0.04h);
	half r2 = roughness * roughness;
	return 0.25h * 2.0h * 1.0h / (exp((shift * shift * 2.5h) / r2) * r2);
}

half HairBRDF_TRT(half roughness, half shift)
{
	roughness = max(roughness, 0.04h);
	half r2 = roughness * roughness;
	return 0.5h * Pi * 1.0h / (exp((shift * shift) / r2) * r2);
}


half HairFresnel(half LoH)
{
	half x = 1.0h - LoH;
	return 0.05h + 0.95h * Pow4(x);
}

half HairFresnelTRT(half LoVMinus)
{
	return 0.05h + 0.95h * Pow4(LoVMinus);
}

half3 HairShadingMobile(half3 Albedo, half3 SubAlbedo, half Roughness, half3 L, half3 V, half3 N, half3 T, half transmit, half Metallic)
{
	half NoLsigned = dot(N, L);
	half NoL = max(NoLsigned, 0.0h);
	half3 HairDiffuse = NoL * Albedo * ECCd * 0.15h;
	
	half3 H = normalize(L + V);
	half LoH = max(dot(L, H), 0.0h);

	half SinThetaL = dot(T, L);
	half SinThetaV = dot(T, V);

	half ShiftBase = SinThetaL + SinThetaV;
	half ShiftOffsetR = 0.12h;

	half Rg = HairBRDF(Roughness, ShiftBase + ShiftOffsetR);
	half TRTg = HairBRDF_TRT(Roughness, ShiftBase);
	half Rf = HairFresnel(LoH);

	half3 Rc = min(half3(0.7h, 0.7h, 0.7h) + SubAlbedo, 1.0h);
	
	half3 HairSpec = (Rg * Rc + TRTg * SubAlbedo) * Rf * NoL * Metallic;
	

	half NoLScattering = 0.5h - NoLsigned * 0.5h;
	half LoVMinus = max(0.0h, dot(-L, V));
	half ScatteringTemp0 = 0.05h + 0.05h * HairFresnelTRT(LoVMinus);
	half3 HairScattering = ScatteringTemp0 * NoLScattering * SubAlbedo * ECCd * transmit;

	return HairDiffuse + HairSpec + HairScattering;
	//return HairSpec;
}

//return Iaa;
half CelAA(half Ir, half It, half Id, half Ib)
{
	half G = fwidth(Ir);
	if (Ir < It + G)
	{
		return lerp(Id, Ib, smoothstep(Id - G, Id + G, Ir));
	}
	return 0.0h;
}

struct PbrPointLight
{
	float4 PointLightPos_RadiusInv;
	float4 PointLightColor_Intensity;
};
#define MaxPointLightNumber		128

half3 PbrPointLightMobile(PbrPointLight light, float3 WorldPos, half3 V, half3 N, half3 OptDiffShading, half3 OptSpecShading, half Roughness)
{
	half3 Lp = (half3)(light.PointLightPos_RadiusInv.xyz - WorldPos);
	half DistSqr = dot(Lp, Lp);

	half AttenPL = rcp(DistSqr + 1.0h);
	AttenPL = AttenPL * (half)Pow2(saturate(1.0h - (half)Pow2(DistSqr * (half)light.PointLightPos_RadiusInv.w * (half)light.PointLightPos_RadiusInv.w)));

	Lp = normalize(Lp);
	half NoLp = max(0.0h, dot(N, Lp));
	half3 Hp = normalize(Lp + V);
	half NoHp = max(0.0h, dot(N, Hp));
	half LoHp = max(0.0h, dot(Lp, Hp));

	half3 BaseShading = (OptDiffShading * pow(NoLp, -1.5h * Roughness + 2.0h) + BRDFMobilePointLight(Roughness, N, Hp, NoHp, LoHp, OptSpecShading) * (half)sqrt(NoLp))
		* (half3)light.PointLightColor_Intensity.rgb * (half)light.PointLightColor_Intensity.a * AttenPL;
	return BaseShading;
}

#endif