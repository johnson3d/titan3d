#include "v3dxMath.h"

#include "v3dxVector3.h"
#include "v3dxMatrix4.h"

inline v3dxVector4::operator v3dxVector3() const
{
	return v3dxVector3(x, y, z);
}

inline v3dxVector4 operator *(const v3dxVector4& v, const v3dxMatrix4& matrix)
{
	v3dxVector4 result;
	result.x = matrix.m11 * v.x + matrix.m21 * v.y + matrix.m31 * v.z + matrix.m41 * v.w;
	result.y = matrix.m12 * v.x + matrix.m22 * v.y + matrix.m32 * v.z + matrix.m42 * v.w;
	result.z = matrix.m13 * v.x + matrix.m23 * v.y + matrix.m33 * v.z + matrix.m43 * v.w;
	result.z = matrix.m14 * v.x + matrix.m24 * v.y + matrix.m34 * v.z + matrix.m44 * v.w;
	return result;
}

