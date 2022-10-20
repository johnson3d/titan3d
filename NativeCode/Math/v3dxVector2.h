/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxvector2.h
	Created Time:		30:6:2002   16:28
	Modify Time:
	Original Author:	johnson
	Modify		   :	2003-05-20	flymemory
							- fixed some bug
							- Standardization interface 
	Abstract:			
	
	Note:				

*********************************************************************/

#ifndef __V3DXVECTOR2__H__
#define __V3DXVECTOR2__H__

#include "vfxGeomTypes.h"

#pragma pack(push,4)

class v3dxVector2 : public v3dVector2_t
{
public:
	static v3dxVector2 Zero;
	v3dxVector2();
	v3dxVector2(float ix,float iy);
	v3dxVector2(const v3dxVector2& v){
		x = v.x; y = v.y;
	}

	void setValue(float ix,float iy);

	float getLength() const;
	float getLengthSq() const;
	void normalize();
	void rotate (float angle);

	v3dxVector2& operator+= (const v3dxVector2& v){ 
		x += v.x;  y += v.y;  return *this; 
	}
	v3dxVector2& operator-= (const v3dxVector2& v){ 
		x -= v.x;  y -= v.y;  return *this; 
	}
	v3dxVector2& operator*= (float f) { 
		x *= f;  y *= f;  return *this; 
	}
	v3dxVector2& operator/= (float f) {
		x /= f;  y /= f;  return *this; 
	}

	inline v3dxVector2 operator+ () const { 
		return v3dxVector2(this->x,this->y); 
	}
	inline v3dxVector2 operator- () const { 
		return v3dxVector2(-x,-y); 
	}

	friend inline v3dxVector2 operator+ (const v3dxVector2& v1, const v3dxVector2& v2){ 
		return v3dxVector2(v1.x+v2.x, v1.y+v2.y); 
	}
	friend inline v3dxVector2 operator- (const v3dxVector2& v1, const v3dxVector2& v2){ 
		return v3dxVector2(v1.x-v2.x, v1.y-v2.y); 
	}
	friend inline float operator* (const v3dxVector2& v1, const v3dxVector2& v2){ 
		return v1.x*v2.x+v1.y*v2.y; 
	}

	friend inline v3dxVector2 operator* (const v3dxVector2& v, float f){ 
		return v3dxVector2(v.x*f,v.y*f); 
	}
	friend inline v3dxVector2 operator* (float f, const v3dxVector2& v){ 
		return v3dxVector2(v.x*f,v.y*f); 
	}
	friend inline v3dxVector2 operator/ (const v3dxVector2& v, float f){ 
		return v3dxVector2(v.x/f, v.y/f); 
	}

	friend inline bool operator == (const v3dxVector2& v1, const v3dxVector2& v2){ 
		return (v1.x==v2.x) && (v1.y==v2.y); 
	}
	friend inline bool operator != (const v3dxVector2& v1, const v3dxVector2& v2){ 
		return (v1.x!=v2.x) || (v1.y!=v2.y); 
	}
	inline bool Equals(const v3dxVector2& v, float epsilon = 0.00001f)
	{
		return std::abs(x - v.x) < epsilon &&  std::abs(y - v.y) < epsilon;
	}
	inline friend bool Equals(const v3dxVector2& v1, const v3dxVector2& v2, float epsilon = 0.00001f)
	{
		return  std::abs(v1.x - v2.x) < epsilon &&  std::abs(v1.y - v2.y) < epsilon;
	}
	friend inline bool operator< (const v3dxVector2& v, float f){ 
		return false;
		//return ::std::abs(v.x)<f && ::std::abs(v.y)<f;
	}
	friend inline bool operator> (float f, const v3dxVector2& v){ 
		return false;
		//return ::std::abs(v.x)<f && ::std::abs(v.y)<f;
	}
};

#include "v3dxVector2.inl.h"

#pragma pack(pop)

#endif//#ifndef __V3DXVECTOR2__H__