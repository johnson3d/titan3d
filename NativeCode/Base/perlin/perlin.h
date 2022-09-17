// download form flipcode.com, knowledge base
// http://www.flipcode.com/cgi-bin/fcarticles.cgi?show=64126
/*
Perlin Noise Class
Submitted by John Ratcliff

This source provides a C++ wrapper around Ken Perlin's noise function. I know there already is a Perlin noise function on the COTD collection, but this one serves a specific purpose. The C++ wrapper allows you to create multiple instances of perlin noise functions so you can have completely determinstic and distinct noise textures. Each instance of the 'Perlin' class can be sampled independently of the other, always returning the same randomized results. You construct an instance of Perlin as follows: Perlin *perlin = new Perlin(4,4,1,94); The first parameter is the number of octaves, this is how noisy or smooth the function is. This is valid between 1 and 16. A value of 4 to 8 octaves produces fairly conventional noise results. The second parameter is the noise frequency. Values betwen 1 and 8 are reasonable here. You can try sampling the data and plotting it to the screen to see what numbers you like. The last parameter is the amplitude. Setting this to a value of 1 will return randomized samples between -1 and +1. The last parameter is the random number seed. This number is what causes this instance of the Perlin to be deterministic and distanct from any other instance. The perlin noise function creates some random number tables that are sampled during queries. This random number seed determines the contents of those tables so that you will get the same exact results every time you use it. To retrieve a sample you simply call the method 'Get' and pass it the X and Y sample point to query. X and Y should be in the ranges of 0 to 1. So if you are sampling for a bitmap be sure and scale the pixel co-ordinates down into normalized values. Each instance of Perlin contains it's own random number tables and sampling values. This class is extremely convenient if you just need a quick and dirty way to do some kind
*/

#pragma once

#include "../IUnknown.h"

NS_BEGIN

class TR_CLASS()
	vfxRandom : public VIUnknownBase
{
	int rand_lcg(UINT& rng_state)
	{
		// LCG values from Numerical Recipes
		rng_state = 1664525 * rng_state + 1013904223;
		return (int)rng_state;
	}
	UINT mCurState;
public:
	vfxRandom()
	{
		mCurState = 0;
	}
	void SetSeed(int seed)
	{
		mCurState = (UINT)seed;
	}
	int NextValue() {
		return rand_lcg(mCurState);
	}
	int NextValue16Bit() {
		return (((UINT)rand_lcg(mCurState))>>16) & 0xffff;
	}
	int NextValue8Bit() {
		return (((UINT)rand_lcg(mCurState)) >> 24) & 0xff;
	}
};
//#define SAMPLE_SIZE 1024

class TR_CLASS()
	Perlin : public VIUnknownBase
{
public:
	Perlin(int octaves, double freq, double amp, int seed, int samplerSize = 1024);
	~Perlin();

	double Get(double x, double y)
	{
		double vec[2];
		vec[0] = x;
		vec[1] = y;
		return perlin_noise_2D(vec);
	};

	double GetAmplitude()
	{
		return mAmplitude;
	}
private:
	//void init_perlin(int n,float p);
	double perlin_noise_2D(double vec[2]);

	double noise1(double arg);
	double noise2(double vec[2]);
	double noise3(double vec[3]);
	void normalize2(double v[2]);
	void normalize3(double v[3]);
	void init(vfxRandom* random);

	int mSamplerSize;
	int mOctaves;
	double mFrequency;
	double mAmplitude;
	int mSeed;

	/*int p[SAMPLE_SIZE + SAMPLE_SIZE + 2];
	double g3[SAMPLE_SIZE + SAMPLE_SIZE + 2][3];
	double g2[SAMPLE_SIZE + SAMPLE_SIZE + 2][2];
	double g1[SAMPLE_SIZE + SAMPLE_SIZE + 2];*/

	int* p;
	struct DPoint3
	{
		double X;
		double Y;
		double Z;
		inline double& operator[](int index)
		{
			return ((&X)[index]);
		}
		inline operator double* ()
		{
			return &X;
		}
	};
	DPoint3* g3;
	struct DPoint2
	{
		double X;
		double Y;
		inline double& operator[](int index)
		{
			return ((&X)[index]);
		}
		inline operator double* ()
		{
			return &X;
		}
	};
	DPoint2* g2;
	double* g1;

	//bool  mStart;

};

NS_END
