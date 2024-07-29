#pragma once
#include "v3dxVector3.h"

#pragma pack(push,8)

class v3dxDVector3 : public v3dDVector3_t
{
public:
	v3dxDVector3()
	{
		X = 0; Y = 0; Z = 0;
	}
	v3dxDVector3(const v3dxDVector3& rh)
	{
		X = rh.X; Y = rh.Y; Z = rh.Z;
	}
	v3dxDVector3(const v3dxVector3& rh)
	{
		X = rh.X; Y = rh.Y; Z = rh.Z;
	}
	v3dxDVector3(double fX, double fY, double fZ)
	{
		X = fX;
		Y = fY;
		Z = fZ;
	}
	inline v3dxVector3 ToSingleVector() const{
		return v3dxVector3((float)X, (float)Y, (float)Z);
	}
	inline double dotProduct(const v3dxDVector3& vec) const
	{
		return v3dxDVec3Dot(this, &vec);
	}
	inline v3dxDVector3 crossProduct(const v3dxDVector3& rkVector) const
	{
		v3dxDVector3 kCross;
		kCross.X = Y * rkVector.Z - Z * rkVector.Y;
		kCross.Y = Z * rkVector.X - X * rkVector.Z;
		kCross.Z = X * rkVector.Y - Y * rkVector.X;
		return kCross;
	}
	friend inline v3dxDVector3 operator +(const v3dxDVector3& vect1, const v3dxDVector3& vect2)
	{
		v3dxDVector3 vRet;
		vRet.X = vect1.X + vect2.X;
		vRet.Y = vect1.Y + vect2.Y;
		vRet.Z = vect1.Z + vect2.Z;
		return vRet;
	}
	friend inline v3dxDVector3 operator -(const v3dxDVector3& vect1, const v3dxDVector3& vect2)
	{
		v3dxDVector3 vRet;
		vRet.X = vect1.X - vect2.X;
		vRet.Y = vect1.Y - vect2.Y;
		vRet.Z = vect1.Z - vect2.Z;
		return vRet;
	}
	friend inline v3dxDVector3 operator +(const v3dxDVector3& vect1, const v3dxVector3& vect2)
	{
		v3dxDVector3 vRet;
		vRet.X = vect1.X + vect2.X;
		vRet.Y = vect1.Y + vect2.Y;
		vRet.Z = vect1.Z + vect2.Z;
		return vRet;
	}
	friend inline v3dxDVector3 operator -(const v3dxDVector3& vect1, const v3dxVector3& vect2)
	{
		v3dxDVector3 vRet;
		vRet.X = vect1.X - vect2.X;
		vRet.Y = vect1.Y - vect2.Y;
		vRet.Z = vect1.Z - vect2.Z;
		return vRet;
	}
	friend inline v3dxDVector3 operator *(const v3dxDVector3& vect1, double fValue)
	{
		v3dxDVector3 vRet;
		vRet.X = vect1.X * fValue;
		vRet.Y = vect1.Y * fValue;
		vRet.Z = vect1.Z * fValue;
		return vRet;
	}
	friend inline v3dxDVector3 operator / (const v3dxDVector3& vect1, double fValue)
	{
		v3dxDVector3 vRet;
		vRet.X = vect1.X / fValue;
		vRet.Y = vect1.Y / fValue;
		vRet.Z = vect1.Z / fValue;
		return vRet;
	}
	v3dxDVector3& operator *= (const v3dxDVector3& v3)
	{
		X *= v3.X;
		Y *= v3.Y;
		Z *= v3.Z;
		return *this;
	}
	v3dxDVector3& operator *= (const v3dxVector3& v3)
	{
		X *= v3.X;
		Y *= v3.Y;
		Z *= v3.Z;
		return *this;
	}
};
#pragma pack(pop)