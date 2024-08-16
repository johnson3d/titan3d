#ifndef _MATH_INC_
#define _MATH_INC_

#include "GlobalDefine.cginc"

#define Pi  3.14159265h
#define PI 3.1415926535897932f
#define Invc2PI 1.0 / (PI * 2.0)
#define ECCd 1.0h //Pi/Pi
#define AO_M 255.0h
#define MAX_POINT_LIGHT_PER_OBJ 4
#define BIG_FLOAT 1000000.0f

#define FLT_MAX	3.40282347E+38F
#define INT_MAX	2147483647

#define FLT_EPSILON 0.00005f
#define FLT_EPSILON2 0.01f

#define ContainmentType_Disjoint 0
#define ContainmentType_Contains 1
#define ContainmentType_Intersects 2

//mem align pow n
#define ROUNDUP(x,n) ((x+(n-1))&(~(n-1)))

#if defined(HLSL_VERSION)
	#if HLSL_VERSION == 2021
		#define V_Select(cond, a, b) select(cond, a, b);
	#else
		#define V_Select(cond, a, b) (cond) ? a : b;
	#endif
#else
	#define V_Select(cond, a, b) (cond) ? a : b;
#endif

static float2 CreateVector2f(float x, float y)
{
    return float2(x, y);
}

static float3 CreateVector3f(float x, float y, float z)
{
    return float3(x, y, z);
}

static float4 CreateVector4f(float x, float y, float z, float w)
{
    return float4(x, y, z, w);
}

static float3 CreateColor3f(float r, float g, float b)
{
    float3 result;
    result.r = r;
    result.g = g;
    result.b = b;
    return result;
}

static float4 CreateColor4f(float r, float g, float b, float a)
{
    float4 result;
    result.r = r;
    result.g = g;
    result.b = b;
    result.a = a;
    return result;
}


uint RoundUpPow2(uint numToRound, uint multiple)
{
	return (numToRound + multiple - 1) & -multiple;
}

half3 sRGB2Linear(half3 sRGBColor)
{
	//you can find the formula in the Wikipedia;
	/*sRGBColor.r = max(6.10352e-5, sRGBColor.r);
	sRGBColor.g = max(6.10352e-5, sRGBColor.g);
	sRGBColor.b = max(6.10352e-5, sRGBColor.b);*/

	// 1.0f / 1.055f = 0.9478673f;        1.0f / 12.92f = 0.07739938f;

	return V_Select(sRGBColor > 0.04045h, pow(sRGBColor * 0.9478673h + 0.0521327h, 2.4h), sRGBColor * 0.07739938h);
}

half3 Linear2sRGB(half3 LinearColor)
{
	//you can find the formula in the Wikipedia;
	/*LinearColor.r = max(6.10352e-5, LinearColor.r);
	LinearColor.g = max(6.10352e-5, LinearColor.g);
	LinearColor.b = max(6.10352e-5, LinearColor.b);*/

	//1/2.4=0.4166667f;
	//return min(LinearColor * 12.92h, 1.055h * pow(max(LinearColor, 0.00313067h), 0.4166667h) - 0.055h);
	return V_Select(LinearColor > 0.0031308h, 1.055h * pow(LinearColor, 0.4166667h) - 0.055h, LinearColor * 12.92h);
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
	return V_Select(x >= 0, 1.570796 - Xb, Xb - 1.570796);
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
#if CP_SM_major >= 5
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

float4 QuatFromAxisAngle(float3 axis, float angle)
{
    axis = normalize(axis);
	
    float4 result;
    float hA= angle * 0.5f;
    float sinV = sin(hA);
    float cosV = cos(hA);

    result.x = axis.x * sinV;
    result.y = axis.y * sinV;
    result.z = axis.z * sinV;
    result.w = cosV;
    return result;
}

float4 QuatFromEuler(float3 euler)
{
    float4 result;

    float halfRoll = euler.z * 0.5f;
    float sinRoll = sin(halfRoll);
    float cosRoll = cos(halfRoll);
    float halfPitch = euler.x * 0.5f;
    float sinPitch = sin(halfPitch);
    float cosPitch = cos(halfPitch);
    float halfYaw = euler.y * 0.5f;
    float sinYaw = sin(halfYaw);
    float cosYaw = cos(halfYaw);

    result.x = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
    result.y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
    result.z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
    result.w = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);

    return result;
}

float3 QuatRotateVector(in float3 inPos, in float4 inQuat)
{
	float3 uv = cross(inQuat.xyz, inPos);
	float3 uuv = cross(inQuat.xyz, uv);
	uv = uv * (2.0f * inQuat.w);
	uuv *= 2.0f;

	return inPos + uv + uuv;
}

//half3 transform_quat(half3 v, half4 quat)
//{
//    return v + (half3) cross(quat.xyz, cross(quat.xyz, v) + quat.w * v) * 2;
//}


float min3(float a, float b, float c)
{
	return min(a, min(b, c));
}

float max3(float a, float b, float c)
{
	return max(a, max(b, c));
}

float2 min3(float2 a, float2 b, float2 c)
{
	return float2(
		min3(a.x, b.x, c.x),
		min3(a.y, b.y, c.y)
		);
}

float2 max3(float2 a, float2 b, float2 c)
{
	return float2(
		max3(a.x, b.x, c.x),
		max3(a.y, b.y, c.y)
		);
}

float3 max3(float3 a, float3 b, float3 c)
{
	return float3(
		max3(a.x, b.x, c.x),
		max3(a.y, b.y, c.y),
		max3(a.z, b.z, c.z)
		);
}

float3 min3(float3 a, float3 b, float3 c)
{
	return float3(
		min3(a.x, b.x, c.x),
		min3(a.y, b.y, c.y),
		min3(a.z, b.z, c.z)
		);
}

float4 min3(float4 a, float4 b, float4 c)
{
	return float4(
		min3(a.x, b.x, c.x),
		min3(a.y, b.y, c.y),
		min3(a.z, b.z, c.z),
		min3(a.w, b.w, c.w)
		);
}

float4 max3(float4 a, float4 b, float4 c)
{
	return float4(
		max3(a.x, b.x, c.x),
		max3(a.y, b.y, c.y),
		max3(a.z, b.z, c.z),
		max3(a.w, b.w, c.w)
		);
}

half4 ToColor4f(uint color)
{
    half4 result;
    result.b = (half) (color & 0xff);
    result.g = (half) ((color >> 8) & 0xff);
    result.r = (half) ((color >> 16) & 0xff);
    result.a = (half) ((color >> 24) & 0xff);
    result /= 255.0h;
    return result;
}

uint ToColorUint(half4 color)
{
    uint result = 0;
    color *= 255.0h;
    result |= ((uint) color.b);
    result |= ((uint) color.g) << 8;
    result |= ((uint) color.b) << 16;
    result |= ((uint) color.a) << 24;
    return result;
}

// ToRGBE - takes a float RGB value and converts it to a float RGB value with a shared exponent
float4 ToRGBE(float4 inColor)
{
	float base = max(inColor.r, max(inColor.g, inColor.b));
	int e;
	float m = frexp(base, e);
	return float4(saturate(inColor.rgb / exp2(e)), e + 127);
}

// FromRGBE takes a float RGB value with a shared exponent and converts it to a 
//	float RGB value
float4 FromRGBE(float4 inColor)
{
	return float4(inColor.rgb * exp2(inColor.a - 127), inColor.a);
}

// RGBM encode/decode
static const float kRGBMRange = 8.0;
half4 ToRGBM(half3 color)
{
    color *= 1.0h / (half)kRGBMRange;
	half m = max(max(color.x, color.y), max(color.z, 1e-5));
	m = ceil(m * 255) / 255;
	return half4(color / m, m);
}

half3 FromRGBM(half4 rgbm)
{
    return rgbm.xyz * rgbm.w * (half)kRGBMRange;
}

float3 RGBToYCoCg(float3 RGB)
{
	float Y = dot(RGB, float3(1, 2, 1));
	float Co = dot(RGB, float3(2, 0, -2));
	float Cg = dot(RGB, float3(-1, 2, -1));

	float3 YCoCg = float3(Y, Co, Cg);
	return YCoCg;
}

float3 YCoCgToRGB(float3 YCoCg)
{
	float Y = YCoCg.x * 0.25;
	float Co = YCoCg.y * 0.25;
	float Cg = YCoCg.z * 0.25;

	float R = Y + Co - Cg;
	float G = Y + Cg;
	float B = Y - Co - Cg;

	float3 RGB = float3(R, G, B);
	return RGB;
}

#define MOTIONVECTOR_SCALAR 64

static float2 EncodeMotionVector(float2 v)
{
#if MOTIONVECTOR_SCALAR == 0
	return v.xy;
#else
	return v.xy * MOTIONVECTOR_SCALAR + 0.5f;
#endif
}

static float2 DecodeMotionVector(float2 v)
{
#if MOTIONVECTOR_SCALAR == 0
	return v.xy;
#else
	return (v.xy - 0.5) / MOTIONVECTOR_SCALAR;
#endif
}

// Approximation of lancos2 without sin() or rcp(), or sqrt() to get x.
  //  (25/16 * (2/5 * x^2 - 1)^2 - (25/16 - 1)) * (1/4 * x^2 - 1)^2
  //  |_______________________________________|   |_______________|
  //                   base                             window
  // The general form of the 'base' is,
  //  (a*(b*x^2-1)^2-(a-1))
  // Where 'a=1/(2*b-b^2)' and 'b' moves around the negative lobe.

//https://zh.numberempire.com/graphingcalculator.php?functions=2%20*%20sin(3.14%20*%20x)%20*%20sin(3.14%20*%20x%20%2F%202.0)%20%2F%20pow(3.14%20*%20x%2C%202)%2C%20(25%2F16%20*%20pow(2%2F5%20*%20x*x%20-%201%2C%202)%20-%20(25%2F16%20-%201))%20*%20pow(1%2F4%20*%20x*x%20-%201%2C2)&xmin=-2.972472&xmax=2.268316&ymin=-2.311788&ymax=1.182068&var=x
float Lanczos2(float x)
{
	const float A1 = 25 / 16;
	const float A2 = 2 / 5;
	const float A3 = 25 / 16 - 1;
	const float A4 = 1 / 4;
	float sq_x = x * x;
	float B1 = (A2 * sq_x - 1);
	float numerator1 = A1 * B1 * B1 - A3;
	float B2 = A4 * sq_x * sq_x - 1;

	//float denominator = 
	return numerator1 * B2 * B2;
}

//https://www.jeremyong.com/graphics/2023/09/05/f32-interlocked-min-max-hlsl/
float2 SphericalEncode(float3 v3)
{
	float2 v;
    //float PI;
	v.x = atan2(v3.y, v3.x) * Invc2PI;
	v.y = v3.z;

	v.x = v.x * 0.5 + 0.5;
	v.y = v.y * 0.5 + 0.5;
	return v;
}

float3 SphericalDecode(float2 v)
{
	float2 ang = float2(v.x * 2.0 - 1.0, v.x * 2.0 - 1.0);

	float2 scth;

	float r = ang.x * PI * 2.0;
	float d2 = 1.0 - ang.y * ang.y;

	scth.x = cos(r);
	scth.y = sin(r);

	float2 schpi = float2(sqrt(1.0 - ang.y * ang.y), ang.y);

	float3 v3 = float3(scth.x * schpi.x, scth.y * schpi.x, schpi.y);

	return v3;
}

float2 OctEncode(float3 v3)
{
	float dxyz = abs(v3.x) + abs(v3.y) + abs(v3.z);
	v3 = v3 / dxyz;

	float2 n = float2(v3.x, v3.y);

	if (v3.z < 0)
	{
		float nx = n.x;
		float ny = n.y;

		n.x = (1.0 - abs(nx)) * (nx >= 0.0 ? 1.0 : -1.0);
		n.y = (1.0 - abs(ny)) * (ny >= 0.0 ? 1.0 : -1.0);
	}

	n.x = n.x * 0.5 + 0.5;
	n.y = n.y * 0.5 + 0.5;
	return n;
}

float3 OctDecode(float2 v)
{
	v = v * 2.0 - 1.0;

	float3 n = float3(v.x, v.y, 1.0 - abs(v.x) - abs(v.y));
	float t = clamp(-n.z, 0.0, 1.0);
	n.x = n.x + (n.x > 0.0 ? -t : t);
	n.y = n.y + (n.y > 0.0 ? -t : t);
	
	return normalize(n);
}

//Approximation TaylorExpansion
float Sqrt_TaylorExpansion(float x)
{
	//Taylor 4.0
	// return 2.0 + (x - 4.0) / 2.0
	
	//Taylor 1.0
    return 1.0 + (x - 1.0) / 2.0;
}

#endif