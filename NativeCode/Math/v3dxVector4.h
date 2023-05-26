#pragma once
#include "vfxGeomTypes.h"

class v3dxVector3;
class v3dxMatrix4;
class v3dxVector4 : public v3dVector4_t
{
public:
	v3dxVector4()
	{

	}
	v3dxVector4(float v)
	{
		x = v;
		y = v;
		z = v;
		w = v;
	}
	v3dxVector4(float ix, float iy, float iz, float iw)
	{
		x = ix;
		y = iy;
		z = iz;
		w = iw;
	}
	operator v3dxVector3() const;
	inline friend v3dxVector4 operator*(const v3dxVector4& v, const v3dxMatrix4&);
};

#include "v3dxVector4.inl.h"