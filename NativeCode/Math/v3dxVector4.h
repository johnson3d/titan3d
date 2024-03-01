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
		X = v;
		Y = v;
		Z = v;
		W = v;
	}
	v3dxVector4(float ix, float iy, float iz, float iw)
	{
		X = ix;
		Y = iy;
		Z = iz;
		W = iw;
	}
	operator v3dxVector3() const;
	inline friend v3dxVector4 operator*(const v3dxVector4& v, const v3dxMatrix4&);
};

#include "v3dxVector4.inl.h"