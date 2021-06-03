#pragma once

struct v3dInt32_2
{
	int x;
	int y;
};

struct v3dInt32_3
{
	int x;
	int y;
	int z;
};

struct v3dInt32_4
{
	int x;
	int y;
	int z;
	int w;
};

struct v3dUInt32_2
{
	unsigned int x;
	unsigned int y;
};

struct v3dUInt32_3
{
	unsigned int x;
	unsigned int y;
	unsigned int z;
};

struct v3dUInt32_4
{
	unsigned int x;
	unsigned int y;
	unsigned int z;
	unsigned int w;
	static v3dUInt32_4 Zero;
	static v3dUInt32_4 GetVar(int x, int y, int z, int w);
};

struct v3dUInt8_2
{
	unsigned char x;
	unsigned char y;
};

struct v3dUInt8_3
{
	unsigned char x;
	unsigned char y;
	unsigned char z;
};

struct v3dUInt8_4
{
	unsigned char x;
	unsigned char y;
	unsigned char z;
	unsigned char w;
};
