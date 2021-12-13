#pragma once
#include "v3dxVector3.h"

#pragma pack(push,8)

class v3dxDVector3 : public v3dDVector3_t
{
public:
	v3dxDVector3()
	{
		x = 0; y = 0; z = 0;
	}
	v3dxDVector3(const v3dxDVector3& rh)
	{
		x = rh.x; y = rh.y; z = rh.z;
	}
	v3dxDVector3(const v3dxVector3& rh)
	{
		x = rh.x; y = rh.y; z = rh.z;
	}
	v3dxDVector3(double fX, double fY, double fZ)
	{
		x = fX;
		y = fY;
		z = fZ;
	}
	inline v3dxVector3 ToSingleVector() const{
		return v3dxVector3((float)x, (float)y, (float)z);
	}
	friend inline v3dxDVector3 operator +(const v3dxDVector3& vect1, const v3dxDVector3& vect2)
	{
		v3dxDVector3 vRet;
		vRet.x = vect1.x + vect2.x;
		vRet.y = vect1.y + vect2.y;
		vRet.z = vect1.z + vect2.z;
		return vRet;
	}
	friend inline v3dxDVector3 operator -(const v3dxDVector3& vect1, const v3dxDVector3& vect2)
	{
		v3dxDVector3 vRet;
		vRet.x = vect1.x - vect2.x;
		vRet.y = vect1.y - vect2.y;
		vRet.z = vect1.z - vect2.z;
		return vRet;
	}
	friend inline v3dxDVector3 operator +(const v3dxDVector3& vect1, const v3dxVector3& vect2)
	{
		v3dxDVector3 vRet;
		vRet.x = vect1.x + vect2.x;
		vRet.y = vect1.y + vect2.y;
		vRet.z = vect1.z + vect2.z;
		return vRet;
	}
	friend inline v3dxDVector3 operator -(const v3dxDVector3& vect1, const v3dxVector3& vect2)
	{
		v3dxDVector3 vRet;
		vRet.x = vect1.x - vect2.x;
		vRet.y = vect1.y - vect2.y;
		vRet.z = vect1.z - vect2.z;
		return vRet;
	}
};
#pragma pack(pop)