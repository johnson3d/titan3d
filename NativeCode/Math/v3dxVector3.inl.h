// v3dxVector3.inl
// 
// Victory 3D Code
// class v3dxVector3 inline function
//
// Author : johnson
// Modifier : lanzhengpeng
//			  flymemory
// Create time :
// Modify time : 2002-6-16 
// Modify time : 2003-2-20 
//-----------------------------------------------------------------------------

#include "v3dxMath.h"

#include "v3dxMatrix3.h"
#include "v3dxQuaternion.h"
#include "v3dxVector2.h"

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunused-value"
#endif

inline void v3dxVector3::setValue(const v3dxVector2& v)
{
	X = v.X;
	Y = v.Y;
}
//inline void	v3dxVector3::operator*=(const v3dxMatrix3 &m)
//{
//	float ox = x;
//	float oy = y;
//	float oz = z;
//	x = m.m11*ox + m.m12*oy + m.m13*oz;
//	y = m.m21*ox + m.m22*oy + m.m23*oz;
//	z = m.m31*ox + m.m32*oy + m.m33*oz;
//}


inline v3dxQuaternion v3dxVector3::getRotationTo(const v3dxVector3& dest) const
{
	// Stan Melax's article in Game Programming Gems
	v3dxQuaternion q;
	// Copy, since cannot modify local
	v3dxVector3 v0 = *this;
	v3dxVector3 v1 = dest;
	v0.normalize();
	v1.normalize();

	v3dxVector3 c;
	v3dxVec3Cross( &c, &v0, &v1 );

	// NB if the crossProduct approaches zero, we get unstable because ANY axis will do
	// when v0 == -v1
	float d = v0.dotProduct(v1);
	// If dot == 1, vectors are the same
	if (d >= 1.0f)
	{
		return v3dxQuaternion::IDENTITY;
	}
	float s = Math::Sqrt( (1+d)*2 );
	float invs = 1 / s;

	q.X = c.X * invs;
	q.Y = c.Y * invs;
	q.Z = c.Z * invs;
	q.W = s * 0.5f;
	return q;
}


//inline v3dxVector3 v3dxVector3::operator *=(const v3dxQuaternion &quaternion)
//{
//	v3dxQuaternion temp(-quaternion.x, -quaternion.y, -quaternion.z, quaternion.w);
//	temp *= *this;
//	temp *= quaternion;
//
//	x = temp.x;
//	y = temp.y;
//	z = temp.z;
//
//	return *this;
//}

inline v3dxVector3 v3dxVector3::randomDeviant(
	float angle, 
	const v3dxVector3& up ) const
{
	v3dxVector3 newUp;

	if (up == v3dxVector3::ZERO)
	{
		// Generate an up vector
		newUp = this->perpendicular();
	}
	else
	{
		newUp = up;
	}

	// Rotate up vector by random amount around this
	v3dxQuaternion q;
	q.FromAngleAxis( Math::UnitRandom() * Math::TWO_PI, *this );
	newUp = q * newUp;

	// Finally rotate this by given angle around randomised up
	q.FromAngleAxis( angle, newUp );
	return q * (*this);
}

inline v3dxVector3 operator +(const v3dxVector3& vect1,const v3dxVector3& vect2)
{
	v3dxVector3 vRet;
	v3dxVec3Add( &vRet , &vect1 , &vect2 );
	return vRet;
}

inline v3dxVector3 operator -(const v3dxVector3& vect1,const v3dxVector3& vect2)
{
	v3dxVector3 vRet;
	v3dxVec3Sub( &vRet , &vect1 , &vect2 );
	return vRet;
}

inline v3dxVector3 operator /(const v3dxVector3& vect1, const v3dxVector3& vect2)
{
	v3dxVector3 vRet;
	v3dxVec3Div(&vRet, &vect1, &vect2);
	return vRet;
}

//inline float operator *(const v3dxVector3& vect1,const v3dxVector3& vect2)
//{
//	return v3dxVec3Dot( &vect1 , &vect2 );
//}

inline v3dxVector3 operator ^(const v3dxVector3& vect1,const v3dxVector3& vect2)
{
	v3dxVector3 vRet;
	v3dxVec3Cross( &vRet , &vect1 , &vect2 );
	return vRet;
}

inline v3dxVector3 operator* (const v3dxVector3& v, const float fValue)
{
	v3dxVector3 vRet;
	v3dxVec3Mul( &vRet , fValue , &v );
	return vRet;
}

inline v3dxVector3 operator* (const float fValue, const v3dxVector3& v)
{
	v3dxVector3 vRet;
	v3dxVec3Mul( &vRet , fValue , &v );
	return vRet;
}

inline v3dxVector3 operator/ (const v3dxVector3& v, const float fValue)
{
	v3dxVector3 vRet;
	v3dxVec3Mul( &vRet , 1.f/fValue , &v );
	return vRet;
}

inline v3dxVector3 operator *(const v3dxVector3& vect,const v3dMatrix4_t& mat)
{
	return v3dxVector3(mat.m11*vect.X+mat.m21*vect.Y+mat.m31*vect.Z+mat.m41,
					mat.m12*vect.X+mat.m22*vect.Y+mat.m32*vect.Z+mat.m42,
					mat.m13*vect.X+mat.m23*vect.Y+mat.m33*vect.Z+mat.m43);
}


//==============================================================================
inline void v3dxVector3::operator= ( const v3dxVector3 &vect )
{
	X = vect.X;
	Y = vect.Y;
	Z = vect.Z;
}

inline v3dxVector3& v3dxVector3::operator *=(const float fValue)
{ 
	v3dxVec3Mul( this , fValue , this );
	return *this;
}

inline v3dxVector3& v3dxVector3::operator /=(const float fValue)
{ 
	v3dxVec3Mul( this , 1.f/fValue , this );
	return *this;
}

inline v3dxVector3 v3dxVector3::operator+ () const { 
	return v3dxVector3(X,Y,Z); 
}

inline v3dxVector3& v3dxVector3::operator +=(const v3dxVector3& vect)
{ 
	v3dxVec3Add( this , this , &vect );
	return *this;
}

inline v3dxVector3 v3dxVector3::operator- () const { 
	return v3dxVector3(-X,-Y,-Z); 
}

inline v3dxVector3& v3dxVector3::operator -=(const v3dxVector3& vect)
{ 
	v3dxVec3Sub( this , this , &vect );
	return *this;
}

inline v3dxVector3 v3dxVector3::operator *=(const v3dxVector3 &v3)
{
	X *= v3[0];
	Y *= v3[1];
	Z *= v3[2];

	return *this;
}

/// Get the length of the vector3
inline float v3dxVector3::getLength() const{ 
	return v3dxVec3Length( this );
}
/// Get the square length of the vecotr3
inline float v3dxVector3::getLengthSq() const{
	return v3dxVec3LengthSq( this ); 
}

/// Get the unit normal
inline v3dxVector3 v3dxVector3::getNormal() const{
	float fLen=getLength();
	if(fLen>0)
		return(*this/fLen);
	else
		return v3dxVector3(0.0f,0.0f,0.0f);
}

/// Normalize the vector3
inline void v3dxVector3::normalize()
{
	v3dxVec3Normalize( this , this );
}

inline float& v3dxVector3::operator [](int i)
{
	return *(&X+i);
}

inline float v3dxVector3::operator [](int i) const
{
	return *(&X+i);
}

inline void v3dxVector3::Lerp(const v3dxVector3 &v3, float fSlerp)
{
	X += fSlerp * (v3[0] - X);
	Y += fSlerp * (v3[1] - Y);
	Z += fSlerp * (v3[2] - Z);
}


//-----------------------------------------------------------------------
inline float v3dxVector3::normalized()
{
	float fLength = Math::Sqrt( X * X + Y * Y + Z * Z );

	// Will also work for zero-sized vectors, but will change nothing
	if ( fLength > 1e-06 )
	{
		float fInvLength = 1.0f / fLength;
		X *= fInvLength;
		Y *= fInvLength;
		Z *= fInvLength;
	}

	return fLength;
}
//-----------------------------------------------------------------------
inline bool v3dxVector3::operator < ( const v3dxVector3& rhs ) const
{
	if( X < rhs.X && Y < rhs.Y && Z < rhs.Z )
		return true;
	return false;
}
//-----------------------------------------------------------------------
inline bool v3dxVector3::operator > ( const v3dxVector3& rhs ) const
{
	if( X > rhs.X && Y > rhs.Y && Z > rhs.Z )
		return true;
	return false;
}
//-----------------------------------------------------------------------
inline bool v3dxVector3::isZeroLength(void) const
{
	float sqlen = (X * X) + (Y * Y) + (Z * Z);
	return (sqlen < (1e-06 * 1e-06));

}
//-----------------------------------------------------------------------
inline v3dxVector3 v3dxVector3::midPoint( const v3dxVector3& vec ) const
{
	return v3dxVector3( 
		( X + vec.X ) * 0.5f, 
		( Y + vec.Y ) * 0.5f, 
		( Z + vec.Z ) * 0.5f );
}
//-----------------------------------------------------------------------
inline void v3dxVector3::makeFloor( const v3dxVector3& cmp )
{
	if( cmp.X < X ) X = cmp.X;
	if( cmp.Y < Y ) Y = cmp.Y;
	if( cmp.Z < Z ) Z = cmp.Z;
}
//-----------------------------------------------------------------------
inline void v3dxVector3::makeCeil( const v3dxVector3& cmp )
{
	if( cmp.X > X ) X = cmp.X;
	if( cmp.Y > Y ) Y = cmp.Y;
	if( cmp.Z > Z ) Z = cmp.Z;
}
//-----------------------------------------------------------------------
inline float v3dxVector3::dotProduct(const v3dxVector3& vec) const
{
	return v3dxVec3Dot( this , &vec );
}
//-----------------------------------------------------------------------
inline v3dxVector3 v3dxVector3::crossProduct( const v3dxVector3& rkVector ) const
{
	v3dxVector3 kCross;
	kCross.X = Y * rkVector.Z - Z * rkVector.Y;
	kCross.Y = Z * rkVector.X - X * rkVector.Z;
	kCross.Z = X * rkVector.Y - Y * rkVector.X;
	return kCross;
}
//-----------------------------------------------------------------------
inline v3dxVector3 v3dxVector3::perpendicular(bool bXFirst /*= true*/) const
{
	static const float fSquareZero = static_cast<float>(1e-06 * 1e-06);
	v3dxVector3 perp = this->crossProduct( bXFirst ? v3dxVector3::UNIT_X : v3dxVector3::UNIT_Y );
	// check length
	if( perp.getLengthSq() < fSquareZero )
	{
		//This vector is the Y axis multiplied by a scalar, so we have to use another axis.
		perp = this->crossProduct( bXFirst ? v3dxVector3::UNIT_Y : v3dxVector3::UNIT_X );
	}

	return perp;
}


//===========================================================================================

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif