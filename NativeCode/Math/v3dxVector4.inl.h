#include "v3dxMath.h"

#include "v3dxVector3.h"
#include "v3dxMatrix4.h"

inline v3dxVector4::operator v3dxVector3() const
{
	return v3dxVector3(X, Y, Z);
}

inline v3dxVector4 operator *(const v3dxVector4& v, const v3dxMatrix4& matrix)
{
	v3dxVector4 result;
	result.X = matrix.m11 * v.X + matrix.m21 * v.Y + matrix.m31 * v.Z + matrix.m41 * v.W;
	result.Y = matrix.m12 * v.X + matrix.m22 * v.Y + matrix.m32 * v.Z + matrix.m42 * v.W;
	result.Z = matrix.m13 * v.X + matrix.m23 * v.Y + matrix.m33 * v.Z + matrix.m43 * v.W;
	result.Z = matrix.m14 * v.X + matrix.m24 * v.Y + matrix.m34 * v.Z + matrix.m44 * v.W;
	return result;
}

