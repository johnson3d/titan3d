
#include "v3dxQuaternion.h"
#include "v3dxMath.h"
#include "v3dxMatrix4.h"

// Construction/Destruction ======================================================
inline v3dxQuaternion::v3dxQuaternion()
{
	w = 1.0f;
	x = 0.0f;
	y = 0.0f;
	z = 0.0f;
}

inline v3dxQuaternion::v3dxQuaternion(float p_x, float p_y, float p_z, float p_w)
{
	w = p_w;
	x = p_x;
	y = p_y;
	z = p_z;
}

//-----------------------------------------------------------------------
inline v3dxQuaternion::v3dxQuaternion(const v3dxQuaternion& rkQ)
{
	w = rkQ.w;
	x = rkQ.x;
	y = rkQ.y;
	z = rkQ.z;
}

inline v3dxQuaternion::v3dxQuaternion(v3dxVector3 v3, float fRadianAngle)
{
	axisRadianAngleToQuat(v3, fRadianAngle);
}


//! use beyond forumle
//
//   q*u*q^{-1} = q*<0,x0,y0,z0>*q^{-1}
//     = q*(x0*i+y0*j+z0*k)*q^{-1}
//     = x0*(q*i*q^{-1})+y0*(q*j*q^{-1})+z0*(q*k*q^{-1})
//
//Rotate() applies the quaternion rotation to a set of vectors.
//inline void v3dxQuaternion::rotate(const v3dxVector3 vIn[], v3dxVector3 vOut[], int nCount) const
//{
//	v3dxQuaternion q;
//
//	//TODO:  For large numbers of vectors, build a temporary 3x3 matrix and use
//	//       it to do the rotations.
//
//	for(int i = 0; i < nCount; i++, vIn++, vOut++)
//	{
//		// q = this * vec
//		q.x = y * vIn[0][2] - z * vIn[0][1] + x + w * vIn[0][0];
//		q.y = z * vIn[0][0] - x * vIn[0][2] + y + w * vIn[0][1];
//		q.z = x * vIn[0][1] - y * vIn[0][0] + z + w * vIn[0][2];
//		q.w = w - x * vIn[0][0] - vIn[0][1] * y - z * vIn[0][2];
//
//		//vec = q * conjugate(this)
//		vOut[0][0] = q.z * y - q.y * z + w * q.x - q.w * x;
//		vOut[0][1] = q.x * z - q.z * x + w * q.y - q.w * y;
//		vOut[0][2] = q.y * x - q.x * y + w * q.z - q.w * z;
//		//		out->w = q.w * w + q.x * x + y * q.y + q.z * z;
//	}
//}

//inline void v3dxQuaternion::operator *=(const v3dxVector3 &v3)
//{
//	float qx, qy, qz, qw;
//	qx = x;
//	qy = y;
//	qz = z;
//	qw = w;
//
//	x = qw * v3[0]            + qy * v3[2] - qz * v3[1];
//	y = qw * v3[1] - qx * v3[2]            + qz * v3[0];
//	z = qw * v3[2] + qx * v3[1] - qy * v3[0];
//	w =          - qx * v3[0] - qy * v3[1] - qz * v3[2];
//}

//-----------------------------------------------------------------------
inline bool v3dxQuaternion::operator== (const v3dxQuaternion &rhs) const
{
	return (rhs.x == x) && (rhs.y == y) &&
		(rhs.z == z) && (rhs.w == w);
}

/*
w   =   cos(theta/2)
x   =   ax * sin(theta/2)
y   =   ay * sin(theta/2)
z   =   az * sin(theta/2)
*/
//inline void v3dxQuaternion::axisRadianAngleToQuat(v3dxVector3 v3, float fRadianAngle)
//{
//	float fHalfAngle = fRadianAngle/2.f;
//	float fSin = Math::Sin(fHalfAngle);
//
//	w = Math::Cos(fHalfAngle);
//	x = fSin * v3.x;
//	y = fSin * v3.y;
//	z = fSin * v3.z;
//}

//-----------------------------------------------------------------------
inline v3dxQuaternion v3dxQuaternion::unitInverse() const
{
	// assert:  'this' is unit length
	return v3dxQuaternion(w, -x, -y, -z);
}

inline float v3dxQuaternion::dot(const v3dxQuaternion& rkQ) const
{
	return w * rkQ.w + x * rkQ.x + y * rkQ.y + z * rkQ.z;
}
//-----------------------------------------------------------------------
inline v3dxQuaternion v3dxQuaternion::operator- () const
{
	return v3dxQuaternion(-x, -y, -z, -w);
}
//-----------------------------------------------------------------------
inline v3dxQuaternion v3dxQuaternion::operator+ (const v3dxQuaternion& rkQ) const
{
	return v3dxQuaternion(x + rkQ.x, y + rkQ.y, z + rkQ.z, w + rkQ.w);
}
//-----------------------------------------------------------------------
inline v3dxQuaternion v3dxQuaternion::operator- (const v3dxQuaternion& rkQ) const
{
	return v3dxQuaternion(x - rkQ.x, y - rkQ.y, z - rkQ.z, w - rkQ.w);
}
//-----------------------------------------------------------------------
inline v3dxQuaternion v3dxQuaternion::operator* (float fScalar) const
{
	return v3dxQuaternion(fScalar*x, fScalar*y, fScalar*z, fScalar*w);
}
//-----------------------------------------------------------------------
inline v3dxQuaternion operator* (float fScalar, const v3dxQuaternion& rkQ)
{
	return v3dxQuaternion(fScalar*rkQ.x, fScalar*rkQ.y,
		fScalar*rkQ.z, fScalar*rkQ.w);
}


/*
// takes upper 3 by 3 portion of matrix (rotation sub matrix)
// and generates a v3dxQuaternion
v3dxQuaternion::v3dxQuaternion(const Matrix &p_Matrix)
{
float fTrace,fSqrt;

fTrace = p_Matrix.m_ax + p_Matrix.m_by + p_Matrix.m_cz;

if (fTrace > 0.0f)
{
fSqrt = (float)sqrt(fTrace + 1.0f);
m_w = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;

m_x = (p_Matrix.m_cy - p_Matrix.m_bz) * fSqrt;
m_y = (p_Matrix.m_az - p_Matrix.m_cx) * fSqrt;
m_z = (p_Matrix.m_bx - p_Matrix.m_ax) * fSqrt;
}
else
{
int biggest;
enum {A,E,I};
if (p_Matrix.m_ax > p_Matrix.m_by)
{
if (p_Matrix.m_cz > p_Matrix.m_ax)
biggest = I;
else
biggest = A;
}
else
{
if (p_Matrix.m_cz > p_Matrix.m_ax)
biggest = I;
else
biggest = E;
}

// in the unusual case the original trace fails to produce a good sqrt, try others...
switch (biggest)
{
case A:
fSqrt = (float)sqrt( p_Matrix.m_ax - (p_Matrix.m_by + p_Matrix.m_cz) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_x = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_cz - p_Matrix.m_bz) * fSqrt;
m_y = (p_Matrix.m_ay + p_Matrix.m_bx) * fSqrt;
m_z = (p_Matrix.m_az + p_Matrix.m_cx) * fSqrt;
break;
}
// I
fSqrt = (float)sqrt( p_Matrix.m_cz - (p_Matrix.m_ax + p_Matrix.m_by) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_z = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_bx - p_Matrix.m_ay) * fSqrt;
m_x = (p_Matrix.m_cx + p_Matrix.m_az) * fSqrt;
m_y = (p_Matrix.m_cy + p_Matrix.m_bz) * fSqrt;
break;
}
// E
fSqrt = (float)sqrt( p_Matrix.m_by - (p_Matrix.m_cz + p_Matrix.m_ax) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_y = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_az - p_Matrix.m_cx) * fSqrt;
m_z = (p_Matrix.m_bz + p_Matrix.m_cy) * fSqrt;
m_x = (p_Matrix.m_bx + p_Matrix.m_ay) * fSqrt;
break;
}
break;
case E:
fSqrt = (float)sqrt( p_Matrix.m_by - (p_Matrix.m_cz + p_Matrix.m_ax) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_y = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_az - p_Matrix.m_cx) * fSqrt;
m_z = (p_Matrix.m_bz + p_Matrix.m_cy) * fSqrt;
m_x = (p_Matrix.m_bx + p_Matrix.m_ay) * fSqrt;
break;
}
// I
fSqrt = (float)sqrt( p_Matrix.m_cz - (p_Matrix.m_ax + p_Matrix.m_by) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_z = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_bx - p_Matrix.m_ay) * fSqrt;
m_x = (p_Matrix.m_cx + p_Matrix.m_az) * fSqrt;
m_y = (p_Matrix.m_cy + p_Matrix.m_bz) * fSqrt;
break;
}
// A
fSqrt = (float)sqrt( p_Matrix.m_ax - (p_Matrix.m_by + p_Matrix.m_cz) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_x = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_cy - p_Matrix.m_bz) * fSqrt;
m_y = (p_Matrix.m_ay + p_Matrix.m_bx) * fSqrt;
m_z = (p_Matrix.m_az + p_Matrix.m_cx) * fSqrt;
break;
}
break;
case I:
fSqrt = (float)sqrt( p_Matrix.m_cz - (p_Matrix.m_ax + p_Matrix.m_by) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_z = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_bx - p_Matrix.m_ay) * fSqrt;
m_x = (p_Matrix.m_cx + p_Matrix.m_az) * fSqrt;
m_y = (p_Matrix.m_cy + p_Matrix.m_bz) * fSqrt;
break;
}
// A
fSqrt = (float)sqrt( p_Matrix.m_ax - (p_Matrix.m_by + p_Matrix.m_cz) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_x = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_cy - p_Matrix.m_bz) * fSqrt;
m_y = (p_Matrix.m_ay + p_Matrix.m_bx) * fSqrt;
m_z = (p_Matrix.m_az + p_Matrix.m_cx) * fSqrt;
break;
}
// E
fSqrt = (float)sqrt( p_Matrix.m_by - (p_Matrix.m_cz + p_Matrix.m_ax) + 1.0);
if (fSqrt > TRACE_QZERO_TOLERANCE)
{
m_y = fSqrt * 0.5f;
fSqrt = 0.5f / fSqrt;
m_w = (p_Matrix.m_az - p_Matrix.m_cx) * fSqrt;
m_z = (p_Matrix.m_bz + p_Matrix.m_cy) * fSqrt;
m_x = (p_Matrix.m_bx + p_Matrix.m_ay) * fSqrt;
break;
}
break;
default:
//assert(0);
break;
}
}
}
*/

inline v3dxQuaternion::v3dxQuaternion(float p_x, float p_y, float p_z)
{
	eulerRadianAnglesToQuat(p_x, p_y, p_z);
}

inline v3dxQuaternion::~v3dxQuaternion()
{
}

// Basic Operations ==============================================================
inline float& v3dxQuaternion::operator [] (int i)
{
	return *(&x + i);
}
inline float v3dxQuaternion::operator [](int i) const
{
	return *(&x + i);
}

inline void v3dxQuaternion::set(float p_w, float p_x, float p_y, float p_z)
{
	w = p_w;
	x = p_x;
	y = p_y;
	z = p_z;
}

inline void v3dxQuaternion::identity()
{
	x = 0.0f;
	y = 0.0f;
	z = 0.0f;
	w = 1.0f;
}

inline float v3dxQuaternion::norm() const
{
	return w * w + x * x + y * y + z * z;
}

inline v3dxQuaternion &v3dxQuaternion::operator =(const v3dxQuaternion& Quaternion)
{
	x = Quaternion[0];
	y = Quaternion[1];
	z = Quaternion[2];
	w = Quaternion[3];
	return *this;
}

/*
Q1=(w1, x1, y1, z1);
Q2=(w2, x2, y2, z2);

A combined rotation of unit two v3dxQuaternions is achieved by

Q1 * Q2 =( w1.w2 - v1.v2, w1.v2 + w2.v1 + v1*v2),
此公式见<<3D Math Primer for Graphics and Game Development>> 10.4.8 Equation10.13

where

v1= (x1, y1, z1)
v2 = (x2, y2, z2)

and both . and * are the standard v3dxVector3 dot and cross product.
*/
inline v3dxQuaternion v3dxQuaternion::operator *(const v3dxQuaternion &rkQ) const
{
	v3dxQuaternion temp;
	v3dxQuaternionMultiply(&temp, this, &rkQ);
	return temp;
}

inline v3dxQuaternion* v3dxQuaternion::Multiply(const v3dxQuaternion* rhs)
{
	return v3dxQuaternionMultiply(this, this, rhs);
}

//inline void v3dxQuaternion::operator *=(const v3dxQuaternion& Quaternion)
//{
//	//	*this = *this*p_Quatern;
//
//	float qx, qy, qz, qw;
//	qx = x;
//	qy = y;
//	qz = z;
//	qw = w;
//
//	x = qw * Quaternion[0] + qx * Quaternion[3] + qy * Quaternion[2] - qz * Quaternion[1];
//	y = qw * Quaternion[1] - qx * Quaternion[2] + qy * Quaternion[3] + qz * Quaternion[0];
//	z = qw * Quaternion[2] + qx * Quaternion[1] - qy * Quaternion[0] + qz * Quaternion[3];
//	w = qw * Quaternion[3] - qx * Quaternion[0] - qy * Quaternion[1] - qz * Quaternion[2];
//}

// ||Quanternion|| 
inline float v3dxQuaternion::magnitude() const
{
	return (float)sqrt(w*w + x * x + y * y + z * z);
}

//inline  v3dxQuaternion operator + (const v3dxQuaternion & quat1,const v3dxQuaternion & quat2)
//{
//	return v3dxQuaternion(quat1.x+quat2.x, quat1.y+quat2.y, quat1.z+quat2.z, quat1.w+quat2.w);
//}
//
//inline  v3dxQuaternion operator -(const v3dxQuaternion& quat1,const v3dxQuaternion& quat2)
//{
//	return v3dxQuaternion(quat1.x-quat2.x,quat1.y-quat2.y,quat1.z-quat2.z,quat1.w-quat2.w);
//}

