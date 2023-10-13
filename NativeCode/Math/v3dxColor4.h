/********************************************************************
	created:	2007/07/23
	created:	23:7:2007   21:05
	filename: 	e:\Works\VictoryProject\Victory\Code\vfxgeometry\v3dxColor4.h
	file path:	e:\Works\VictoryProject\Victory\Code\vfxgeometry
	file base:	v3dxColor4
	file ext:	h
	author:		Jones
	
	purpose:	color class
*********************************************************************/

#ifndef __V3DXCOLOR4__H__
#define __V3DXCOLOR4__H__
#include "vfxGeomTypes.h"
#include "v3dxVector3.h"

#pragma pack(push,4)

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wreorder"
#endif

//------------------------------------------------------------------------------
// Rgba, 4 unsigned byte, identical to D3DFMT_A8R8G8B8
//------------------------------------------------------------------------------

class v3dxColor4;

struct  FColor {
	union {
		struct {
			unsigned char b, g, r, a;
		};
		unsigned int dword;
	};

	FColor();
	FColor(unsigned char ir, unsigned char ig, unsigned char ib, unsigned char ia=0xFF);
	FColor(int ir, int ig, int ib, int ia=0xFF);
	FColor(float _r, float _g, float _b, float _a = 1.0f);
	FColor(const v3dxVector3 &vec);
	//Rgba(const v3dxVector4 &vec);
	FColor(const v3dxColor4 &rhs);

	bool operator==(const FColor &color) const;
	bool operator!=(const FColor &color) const ;
	operator const unsigned char*() const;
	void clear();
	FColor &set(unsigned char ir, unsigned char ig, unsigned char ib, unsigned char ia=255);
	//Rgba &set(const Rgb &rgb);
	FColor operator*(const FColor &other) const;
	FColor operator*(float scale) const;
	FColor operator+(const FColor &other) const;
	FColor operator-(const FColor &other) const;
	FColor &operator*=(float scale);
	FColor &operator+=(const FColor &other);
	//Vector4 toVector4() const;
	unsigned char &operator[](int index);
	unsigned char operator[](int index) const;

	//VBaseStringA toStringRgb() const;
	//void parseRgb(const char *text);

	//Rgb rgb() const { return Rgb(r,g,b); }

	//std::string toString() const;
	//void fromString(const char *str);

	static FColor randColor();

	const static FColor Zero;	// r=g=b=a=0
	const static FColor Black;	// r=g=b=0, a=255
	const static FColor Red;
	const static FColor Green;
	const static FColor Blue;
	const static FColor Yellow;
	const static FColor Magenta;
	const static FColor Cyan;
	const static FColor White;
	const static FColor LtGrey;
	const static FColor MdGrey;
	const static FColor DkGrey;
	const static FColor ColorTable[];
};

inline FColor::FColor()
{}

inline FColor::FColor(unsigned char ir, unsigned char ig, unsigned char ib, unsigned char ia)
	: r(ir), g(ig), b(ib), a(ia)
{}

inline FColor::FColor(int ir, int ig, int ib, int ia)
	: r(ir), g(ig), b(ib), a(ia)
{}

inline FColor::FColor(float _r, float _g, float _b, float _a)
	: r(Math::clampByte((int)(_r*255.f)))
	, g(Math::clampByte((int)(_g*255.f)))
	, b(Math::clampByte((int)(_b*255.f)))
	, a(Math::clampByte((int)(_a*255.f)))
{}


inline FColor::FColor(const v3dxVector3 &vec)
	: r(Math::clampByte((int)(vec.x*255.f)))
	, g(Math::clampByte((int)(vec.y*255.f)))
	, b(Math::clampByte((int)(vec.z*255.f)))
	, a(255)
{}

//inline Rgba::Rgba(const Vector4 &vec)
//	: r(Math::clampByte(vec.x*255.f))
//	, g(Math::clampByte(vec.y*255.f))
//	, b(Math::clampByte(vec.z*255.f))
//	, a(Math::clampByte(vec.w*255.f))
//{}

inline bool FColor::operator==(const FColor &rhs) const
{
	return dword == rhs.dword;
}

inline bool FColor::operator!=(const FColor &rhs) const
{
	return dword != rhs.dword;
}

inline FColor::operator const unsigned char*() const
{ return &r; }

inline void FColor::clear()
{ r=g=b=a=0; }

inline FColor &FColor::set(unsigned char ir, unsigned char ig, unsigned char ib, unsigned char ia)
{
	r=ir; g=ig; b=ib; a=ia; return *this;
}

//inline Rgba &Rgba::set(const Rgb &rgb)
//{
//	r=rgb.r; g=rgb.g; b=rgb.b; a=255; return *this;
//}

inline FColor FColor::operator*(const FColor &other) const
{
	FColor c;
	c.r = ((int)r * other.r) >> 8;
	c.g = ((int)g * other.g) >> 8;
	c.b = ((int)b * other.b) >> 8;
	c.a = ((int)a * other.a) >> 8;
	return c;
}

inline FColor FColor::operator*(float scale) const
{
	FColor c;

	c.r = (unsigned char)(scale * r);
	c.g = (unsigned char)(scale * g);
	c.b = (unsigned char)(scale * b);
	c.a = (unsigned char)(scale * a);
	return c;
}

inline FColor FColor::operator+(const FColor &other) const
{
	FColor c;

	c.r = r + other.r;
	c.g = g + other.g;
	c.b = b + other.b;
	c.a = a + other.a;

	return c;
}

inline FColor FColor::operator-(const FColor &other) const
{
	FColor c;

	c.r = r - other.r;
	c.g = g - other.g;
	c.b = b - other.b;
	c.a = a - other.a;

	return c;
}

inline FColor &FColor::operator*=(float scale)
{
	return *this = (*this * scale);
}

inline FColor &FColor::operator+=(const FColor &other)
{
	return *this = (*this + other);
}

//inline Vector4 Rgba::toVector4() const
//{
//	Vector4 result;
//	float inv255 = 1.0f / 255.0f;
//	result.x = r * inv255;
//	result.y = g * inv255;
//	result.z = b * inv255;
//	result.w = a * inv255;
//	return result;
//}

inline unsigned char &FColor::operator[](int index)
{
	//ASSERT(index>=0);
	//ASSERT(index<4);
	return *(&r+index);
}

inline unsigned char FColor::operator[](int index) const
{
	//ASSERT(index>=0);
	//ASSERT(index<4);
	return *(&r+index);
}

//inline VBaseStringA Rgba::toStringRgb() const
//{
//	VBaseStringA result;
//
//	StringUtil::sprintf(result, "%d,%d,%d", r,g,b);
//
//	return result;
//}
//
//inline void Rgba::parseRgb(const char *text)
//{
//	int _r, _g, _b;
//	int v = sscanf(text, "%d,%d,%d", &_r, &_g, &_b);
//	ASSERT(v = 3);
//	r = _r; g = _g; b = _b; a = 255;
//}

inline FColor FColor::randColor()
{
	FColor result;
	result.r = rand() % 255;
	result.g = rand() % 255;
	result.b = rand() % 255;
	result.a = 255;
	return result;
}

/// <summary>
/// ====================================================================================================
/// </summary>

class v3dxColor4 : public v3dVector4_t
{
public:
	v3dxColor4(){}
	v3dxColor4(float rr, float gg, float bb, float aa = 1.f){
		r = rr; g = gg; b = bb; a = aa;
		if ( rr != FLT_MAX )
			r = r < 0 ? 0 : (r > 1 ? 1 : r);
		if ( gg != FLT_MAX )
			g = g < 0 ? 0 : (g > 1 ? 1 : g);
		if ( bb != FLT_MAX )
			b = b < 0 ? 0 : (b > 1 ? 1 : b);
		if ( aa != FLT_MAX )
			a = a < 0 ? 0 : (a > 1 ? 1 : a);
	}
	v3dxColor4(DWORD clr){
		b = (clr & 0xFF) / 255.f;
		g = ((clr & 0xFF00) >> 8) / 255.f;
		r = ((clr & 0xFF0000) >> 16) / 255.f;
		a = ((clr & 0xFF000000) >> 24) / 255.f;
	}
	static v3dxColor4 fromArgb(BYTE rr, BYTE gg, BYTE bb, BYTE aa = 255){
		v3dxColor4 c;
		c.r = (float)rr/255.f; c.g = (float)gg/255.f; c.b = (float)bb/255.f; c.a = (float)aa/255.f;
		return c;
	}
	
	static void convertRBValue(DWORD *ptr)
	{
		if(ptr!=NULL)
			*ptr = ((*ptr & 0x00FF0000) >> 16) | ((*ptr & 0x000000FF) << 16) | (*ptr & 0xFF00FF00);
	}

	friend vBOOL operator ==(const v3dxColor4& c1, const v3dxColor4& c2){
		return c1.r == c2.r && c1.g == c2.g && c1.b == c2.b && c1.a == c2.a;
	}
	friend vBOOL operator !=(const v3dxColor4& c1, const v3dxColor4& c2){
		return c1.r != c2.r || c1.g != c2.g || c1.b != c2.b || c1.a != c2.a;
	}
	v3dxColor4& operator =(const v3dxColor4& c){
		r = c.r; g = c.g; b = c.b; a = c.a;
		r = r < 0 ? 0 : (r > 1 ? 1 : r);
		g = g < 0 ? 0 : (g > 1 ? 1 : g);
		b = b < 0 ? 0 : (b > 1 ? 1 : b);
		a = a < 0 ? 0 : (a > 1 ? 1 : a);
		return *this;
	}
	inline BYTE getA() const {
		return (BYTE)(a*255.f);
	}
	inline BYTE getR() const {
		return (BYTE)(r*255.f);
	}
	inline BYTE getG() const {
		return (BYTE)(g*255.f);
	}
	inline BYTE getB() const {
		return (BYTE)(b*255.f);
	}
	// ARGB format, with alpha
	inline DWORD getD3DVal() const{
		DWORD dw = (DWORD)(b * 255.f);
		dw |= (DWORD)(g * 255.f) << 8;
		dw |= (DWORD)(r * 255.f) << 16;
		dw |= (DWORD)(a * 255.f) << 24;
		return dw;
	}
	inline DWORD getABGR() const {
		DWORD dw = (DWORD)(r * 255.f);
		dw |= (DWORD)(g * 255.f) << 8;
		dw |= (DWORD)(b * 255.f) << 16;
		dw |= (DWORD)(a * 255.f) << 24;
		return dw;
	}
	// BGR format, no alpha
	inline DWORD getRefVal(){
		DWORD dw = (DWORD)(r * 255.f);
		dw |= (DWORD)(g * 255.f) << 8;
		dw |= (DWORD)(b * 255.f) << 16;
		dw |= 0;//(DWORD)(a * 255.f) << 24;
		return (DWORD)dw;
	}
};

#pragma pack(pop)

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif

#endif