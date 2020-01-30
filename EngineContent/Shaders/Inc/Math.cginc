#ifndef _MATH_INC_
#define _MATH_INC_

#define Pi  3.14159265h
#define PI 3.1415926535897932f
#define ECCd 1.0h //Pi/Pi
#define AO_M 255.0h
#define MAX_POINT_LIGHT_PER_OBJ 4


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

//half InverseDepth(half Depth)
//{
//	return 1.0h - Depth;
//}

#endif