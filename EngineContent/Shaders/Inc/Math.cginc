#ifndef _MATH_INC_
#define _MATH_INC_

#define Pi  3.14159265h
#define PI 3.1415926535897932f
#define ECCd 1.0h //Pi/Pi
#define AO_M 255.0h
#define MAX_POINT_LIGHT_PER_OBJ 4
#define BIG_FLOAT 1000000.0f

#define ROUNDUP(x,n) ((x+(n-1))&(~(n-1)))

half3 sRGB2Linear(half3 sRGBColor)
{
	//you can find the formula in the Wikipedia;
	/*sRGBColor.r = max(6.10352e-5, sRGBColor.r);
	sRGBColor.g = max(6.10352e-5, sRGBColor.g);
	sRGBColor.b = max(6.10352e-5, sRGBColor.b);*/

	// 1.0f / 1.055f = 0.9478673f;        1.0f / 12.92f = 0.07739938f;
	return   sRGBColor > 0.04045h ? pow(sRGBColor  * 0.9478673h + 0.0521327h, 2.4h) : sRGBColor * 0.07739938h;
}

half3 Linear2sRGB(half3 LinearColor)
{
	//you can find the formula in the Wikipedia;
	/*LinearColor.r = max(6.10352e-5, LinearColor.r);
	LinearColor.g = max(6.10352e-5, LinearColor.g);
	LinearColor.b = max(6.10352e-5, LinearColor.b);*/

	//1/2.4=0.4166667f;
	//return min(LinearColor * 12.92h, 1.055h * pow(max(LinearColor, 0.00313067h), 0.4166667h) - 0.055h);
	return LinearColor > 0.0031308h ? 1.055h * pow(LinearColor, 0.4166667h) - 0.055h : LinearColor * 12.92h;
}

half Pow2(half x)
{
	return x * x;
}

half Pow4(half x)
{
	half x2 = x * x;
	return x2 * x2;
}

half Pow5(half x)
{
	half x2 = x * x;
	return x2 * x2 * x;
}


half ASinMobile(half x)
{
	half Xa = abs(x);
	half Xb = -0.156583 * Xa + 1.570796;
	Xb *= sqrt(1.0 - Xa);
	return (x >= 0) ? 1.570796 - Xb : Xb - 1.570796;
}


half Square(half x)
{
	return x * x;
}


half CalcLuminance(half3 Color)
{
	return dot(Color, half3(0.299h, 0.587h, 0.114h));
}

half CalcLuminanceYCbCr(half3 Color)
{
	return dot(Color, half3(0.2126h, 0.7152h, 0.0722h));
}

half3 EyeNoise(half2 seed)
{
	return frac(sin(dot(seed.xy, half2(34.483h, 89.637h))) * half3(29156.4765h, 38273.5639h, 47843.7546h));
}

half4 RGBMEncode(half3 color, half range) 
{
	if (all(color == 0))
	{
		return (half4)0;
	}
	half4 rgbm;
	color /= range;
	rgbm.a = clamp(max(max(color.r, color.g), max(color.b, 1e-6)), 0.0, 1.0);
	rgbm.a = ceil(rgbm.a * 255.0) / 255.0;
	rgbm.rgb = color / rgbm.a;
	return rgbm;
}

half3 RGBMDecode(half4 rgbm, half range) {
	return range * rgbm.rgb * rgbm.a;
}


void MergeAABB(inout float3 boxmin, inout float3 boxmax, float3 pos)
{
	boxmin = min(boxmin, pos);
	boxmax = max(boxmax, pos);
}

bool Overlap_AABB_Sphere(float3 boxmin, float3 boxmax, float3 center, float radiusSq, out float distSq)
{
	float3 clamped = clamp(center, boxmin, boxmax);
	float3 a = center - clamped;
	distSq = dot(a, a);

	bool ret = (distSq <= radiusSq);
	return ret;
}

bool Contain_AABB_Point(float3 boxmin, float3 boxmax, float3 pos)
{
	if (any(pos > boxmax) || any(pos < boxmin))
		return false;
	return true;
}

//half InverseDepth(half Depth)
//{
//	return 1.0h - Depth;
//}

// Clamp the base, so it's never <= 0.0f (INF/NaN).

#define POW_CLAMP 0.000001f
float ClampedPow(float X, float Y)
{
	return pow(max(abs(X), POW_CLAMP), Y);
}

uint ReverseBits32(uint bits)
{
#if ShaderModel >= 5
	return reversebits(bits);
#else
	bits = (bits << 16) | (bits >> 16);
	bits = ((bits & 0x00ff00ff) << 8) | ((bits & 0xff00ff00) >> 8);
	bits = ((bits & 0x0f0f0f0f) << 4) | ((bits & 0xf0f0f0f0) >> 4);
	bits = ((bits & 0x33333333) << 2) | ((bits & 0xcccccccc) >> 2);
	bits = ((bits & 0x55555555) << 1) | ((bits & 0xaaaaaaaa) >> 1);
	return bits;
#endif
}

/** Reverses all the <BitCount> lowest significant bits. */
uint ReverseBitsN(uint Bitfield, const uint BitCount)
{
	return ReverseBits32(Bitfield) >> (32 - BitCount);
}

float3 QuatRotatePosition(in float3 inPos, in float4 inQuat)
{
	float3 uv = cross(inQuat.xyz, inPos);
	float3 uuv = cross(inQuat.xyz, uv);
	uv = uv * (2.0f * inQuat.w);
	uuv *= 2.0f;

	return inPos + uv + uuv;
}

#endif