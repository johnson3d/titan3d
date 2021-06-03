/********************************************************************
V3D					A Powerful 3D Engine
File:				v3dxRect.h
Original Author:	johnson
*********************************************************************/


#ifndef __V3DXRECT__H__
#define __V3DXRECT__H__

#include "vfxGeomTypes.h"
#include "v3dxPoint.h"
#include "v3dxSize.h"

#pragma pack(push,4)

//class  v3dxRect
//{
//public:
//	// left top
//	float x1,z1;
//	// right bottom
//	float x2,z2;
//	
//public:
//	v3dxRect(void);
//	v3dxRect(float p_x1, float p_z1, float p_x2, float p_z2);
//	~v3dxRect(void){}
//
//	void set(float p_x1, float p_z1, float p_x2, float p_z2);
//
//	bool isContain(float x, float z) const;
//};
//
//inline v3dxRect::v3dxRect()
//{
//	x1 = z1 = x2 = z2 = 0.f;
//}
//
//inline v3dxRect::v3dxRect(float p_x1, float p_z1, float p_x2, float p_z2)
//{
//	set(p_x1, p_z1, p_x2, p_z2);
//}
//
//inline void v3dxRect::set(float p_x1, float p_z1, float p_x2, float p_z2)
//{
//	x1 = p_x1;
//	z1 = p_z1;
//
//	x2 = p_x2;
//	z2 = p_z2;
//}
//
//inline bool v3dxRect::isContain(float x, float z) const
//{
//	if ( (x>x1 && z>z1) && (x<x2 && z<z2) )
//		return true;
//
//	return false;
//}


// 2D screen coordinate rectangle
struct  v3dxRect
{
	int x, y, width, height;				// x, y, width, height

	v3dxRect();
	v3dxRect(int ix, int iy, int iw, int ih);
	v3dxRect(const v3dxRect &rect);
	explicit v3dxRect(const v3dxSize &size) { x=0; y=0; width=size.width; height=size.height; }
	v3dxRect(const v3dxPoint &pos, const v3dxSize &size) { x=pos.x; y=pos.y; width=size.width; height=size.height; }
	~v3dxRect();

	// add need function below
	v3dxRect& set(int _x, int _y, int _w, int _h) { x=_x; y=_y; width=_w; height=_h; return *this; }
	void clear();
	bool operator==(const v3dxRect &other) const;
	bool contains(int ix, int iy) const;
	bool contains(const v3dxPoint &point) const;
	bool contains(const v3dxRect &r) const;
	bool intersects(const v3dxRect &r);
	v3dxPoint getCenter() const;
	v3dxPoint getMins() const;
	v3dxPoint getMaxs() const;
	int xMin() const;
	int yMin() const;
	int xMax() const;
	int yMax() const;

	template< class Q >
	v3dxRect &operator/=(Q scale);
	template< class Q >
	v3dxRect operator/(Q scale) const;
	template< class Q >
	v3dxRect &operator*=(Q scale);
	template< class Q >
	v3dxRect operator*(Q scale) const;

	// if two rect don't intersected, returned rect have zero or negative width
	// and height, check isEmpty()
	// self rect and other rect must have positive width and height to do this function
	v3dxRect intersect(const v3dxRect &other) const;

	// like intersect, but returned rect is self offset base, for texture clipping
	v3dxRect intersectToLocal(const v3dxRect &other) const;

	// return a new rect contain both two rect
	v3dxRect unite(const v3dxRect &other) const;

	// just do intersect
	v3dxRect operator&(const v3dxRect &other) const;
	v3dxRect &operator&=(const v3dxRect &other);
	v3dxRect operator|(const v3dxRect &other) const;
	v3dxRect &operator|=(const v3dxRect &other);
	// just do offset, no resize
	v3dxRect operator-(const v3dxPoint &point) const;
	v3dxRect &operator-=(const v3dxPoint &point);
	// just do offset, no resize
	v3dxRect operator+(const v3dxPoint &point) const;
	v3dxRect &operator+=(const v3dxPoint &point);
	bool isEmpty() const;
	void deflate(int x, int y);
	void deflate(const v3dxPoint &point);
	void inflate(int x, int y);
	void inflate(const v3dxPoint &point);

	void offset(int x);
	void offset(int x, int y);
	void offset(const v3dxPoint &point);

	//Vector4 toVector4() const;

	//VBaseStringA toString() const;
	//bool fromString(const char *str);
};

#pragma pack(pop)

#endif