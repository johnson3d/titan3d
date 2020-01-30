#ifndef _POST_EFFECT_COMMON_
#define _POST_EFFECT_COMMON_


half2 CalcVignetteVS(half2 NDCPos)
{
	half AspectRatio = (half)gViewportSizeAndRcp.y * (half)gViewportSizeAndRcp.z;
	half Scale = sqrt(2.0h) / sqrt(1.0h + AspectRatio * AspectRatio);
	return NDCPos * half2(1.0h, AspectRatio) * Scale;
}


half CalcVignettePS(half2 VSData, half Intensity)
{
	half Tan2Angle = dot(VSData, VSData) * Intensity;
	half Cos4Angle = Square(rcp(Tan2Angle + 1.0h));
	return Cos4Angle;
}



half2 CircleSampler(half SliceCount, half Start, half Offset)
{
	half radian = (3.141592h * 2.0h * (1.0h / SliceCount)) * (Start + Offset);
	return half2(sin(radian), cos(radian));
}

half3 ACESMobile(half3 LinearColor)
{
	half3 Oc = LinearColor * 0.6h;
	return saturate((Oc * (2.51h * Oc + 0.03h)) / (Oc * (2.43h * Oc + 0.59h) + 0.14h));
}


half3 RRT_ODT(half3 c)
{
	half3 Numer = c * (c + 0.0245786h) - 0.000090537h;
	half3 Denom = c * (0.983729h * c + 0.432951h) + 0.238081h;
	return Numer / Denom;
}

half3 ACESMobile_HQ(half3 LinearColor)
{
	static const float3x3 ACESInputMtx =
	{
		{0.87914, 0.08380, 0.04361},
		{0.08066, 0.86386, 0.05646},
		{0.04020, 0.05234, 0.89993}
	};

	static const float3x3 ACESOutputMtx =
	{
		{1.19434, -0.11125, -0.12207},
		{-0.08293, 1.21548, -0.10025},
		{-0.11141, -0.10423, 1.22232}
	};

	float3 Oc = mul(LinearColor, ACESInputMtx);
	return (half3)saturate(
			mul((float3)RRT_ODT(Oc), 
			ACESOutputMtx));
	//return saturate(RRT_ODT(Oc));
}

float2 NDC2UV(float2 ndc_uv, float platform_uv_flag)
{
	return ndc_uv * float2(0.5f, -0.5f * platform_uv_flag) + 0.5f;
}

//platform_uv_flag: gles -1.0f; 
//                             else 1.0f;
half3 ReconstructPosVS(float2 uv, float w, matrix PrjInvMtx, float LinearDepth, float platform_uv_flag)
{
	float4 PosCS = float4(uv.x * 2.0f - 1.0f, (1.0f - uv.y * 2.0f) * platform_uv_flag, LinearDepth, 1.0f) * w;
	float4 PosVS = mul(PosCS, PrjInvMtx);
	return half3(PosVS.xyz);
}

half3 ReconstructPosWS(float2 uv, float w, matrix ViewPrjInvMtx, float LinearDepth, float platform_uv_flag)
{
	float4 PosCS = float4(uv.x * 2.0f - 1.0f, (1.0f - uv.y * 2.0f) * platform_uv_flag, LinearDepth, 1.0f) * w;
	float4 PosWS = mul(PosCS, ViewPrjInvMtx);
	return half3(PosWS.xyz);
}

/** Reverses all the 32 bits. */
uint ReverseBits32( uint bits )
{
#if SM5_PROFILE || COMPILER_METAL
	return reversebits( bits );
#else
	bits = ( bits << 16) | ( bits >> 16);
	bits = ( (bits & 0x00ff00ff) << 8 ) | ( (bits & 0xff00ff00) >> 8 );
	bits = ( (bits & 0x0f0f0f0f) << 4 ) | ( (bits & 0xf0f0f0f0) >> 4 );
	bits = ( (bits & 0x33333333) << 2 ) | ( (bits & 0xcccccccc) >> 2 );
	bits = ( (bits & 0x55555555) << 1 ) | ( (bits & 0xaaaaaaaa) >> 1 );
	return bits;
#endif
}

/** Reverses all the <BitCount> lowest significant bits. */
uint ReverseBitsN(uint Bitfield, const uint BitCount)
{
	return ReverseBits32(Bitfield) >> (32 - BitCount);
}

float2 Hammersley2d(uint idx, uint num) 
{
	uint bits = idx;
	bits = (bits << 16u) | (bits >> 16u);
	bits = ((bits & 0x55555555u) << 1u) | ((bits & 0xAAAAAAAAu) >> 1u);
	bits = ((bits & 0x33333333u) << 2u) | ((bits & 0xCCCCCCCCu) >> 2u);
	bits = ((bits & 0x0F0F0F0Fu) << 4u) | ((bits & 0xF0F0F0F0u) >> 4u);
	bits = ((bits & 0x00FF00FFu) << 8u) | ((bits & 0xFF00FF00u) >> 8u);
	const float radicalInverse_VdC = float(bits) * 2.3283064365386963e-10; // / 0x100000000

	return float2(float(idx) / float(num), radicalInverse_VdC);
}

float3 CaclUniformHemispherePoint(float2 uv) 
{
	float phi = uv.y * 2.0 * PI;
	float cosTheta = 1.0 - uv.x;
	float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
	return float3(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta);
}

half rand(half seed, float2 uv)
{
	return (half)frac(sin((half)dot(uv, float2(12.9898f, 78.233f)) * seed) * 43758.5453h);
}

#endif