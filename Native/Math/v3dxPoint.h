#ifndef __V3DXPOINT__H__
#define __V3DXPOINT__H__

#include "vfxGeomTypes.h"

#pragma pack(push,4)

struct  v3dxPoint {
	int x, y;

	// constructor and destructor
	inline v3dxPoint() {}
	inline v3dxPoint(int ix, int iy) : x(ix), y(iy) {}
	inline ~v3dxPoint() {}

	// algo
	inline v3dxPoint operator+(const v3dxPoint &v) const
	{
		return v3dxPoint(x + v.x, y + v.y);
	}

	inline v3dxPoint operator+(int v) const
	{
		return v3dxPoint(x+v, y+v);
	}

	inline v3dxPoint &operator+=(int v)
	{
		x += v; y += v;
		return *this;
	}
	inline v3dxPoint operator-(int v) const {
		return v3dxPoint(x-v, y-v);
	}
	inline v3dxPoint &operator-=(int v)
	{
		x -= v;
		y -= v;
		return *this;
	}
	// negative vector
	inline v3dxPoint operator-() const
	{
		return v3dxPoint(-x, -y);
	}

	inline v3dxPoint operator-(const v3dxPoint &v) const
	{
		return v3dxPoint(x - v.x, y - v.y);
	}

	// scale the vector
	inline v3dxPoint operator*(const int scale) const
	{
		return v3dxPoint(x*scale, y*scale);
	}

	// multiply
	inline v3dxPoint operator*(const v3dxPoint &v) const
	{
		return v3dxPoint(x*v.x, y*v.y);
	}

	// div
	inline v3dxPoint operator/(const v3dxPoint &v) const
	{
		return v3dxPoint(x/v.x, y/v.y);
	}

	// inverse scale
	inline v3dxPoint operator/(int scale) const
	{
		return v3dxPoint(x/scale, y/scale);
	}

	// Dot Product
	inline int operator|(const v3dxPoint &v) const
	{
		return x*v.x + y*v.y;
	}

	// compare
	inline bool operator==(const v3dxPoint &v) const
	{
		return x==v.x && y==v.y;
	}

	bool operator!=(const v3dxPoint &v) const
	{
		return x!=v.x || y!=v.y;
	}

	// assign operator
	inline v3dxPoint operator+=(const v3dxPoint &v)
	{
		x += v.x; y += v.y;
		return *this;
	}

	inline v3dxPoint operator-=(const v3dxPoint &v)
	{
		x -= v.x; y -= v.y;
		return *this;
	}

	inline v3dxPoint operator*=(int scale)
	{
		x *= scale; y *= scale;
		return *this;
	}

	inline v3dxPoint operator/=(int v)
	{
		x /= v; y /= v;
		return *this;
	}

	inline v3dxPoint operator*=(const v3dxPoint &v)
	{
		x *= v.x; y *= v.y;
		return *this;
	}

	inline v3dxPoint operator/=(const v3dxPoint &v)
	{
		x /= v.x; y /= v.y;
		return *this;
	}

	inline int &operator[](int index)
	{
		//assert(index >= 0);
		//ASSERT(index < 2);
		return *(&x + index);
	}

	inline int operator[](int index) const
	{
		//assert(index >= 0);
		//assert(index < 2);
		return *(&x + index);
	}

	// Simple functions.
	inline int getMax() const;
	inline int getAbsMax() const;
	inline int getMin() const;
	inline int getLength() const
	{
		return (int)sqrt((double)(x*x + y*y));
	}

	inline int getLengthSquared() const
	{
		return x*x + y*y;
	}

	inline bool isZero() const
	{
		return x==0 && y==0;
	}

	// Clear self
	inline void clear()
	{
		x=0; y=0;
	}

	// Set value
	inline v3dxPoint &set(int ix=0, int iy=0)
	{
		x = ix; y = iy;
		return *this;
	}

	inline operator const int*() const
	{
		return &x;
	}

	//VBaseStringA toString() const;
	//bool fromString(const char *str);
};

#pragma pack(pop)

#endif // end guardian

