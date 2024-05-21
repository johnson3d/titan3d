/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxvector3.h
	Created Time:		30:6:2002   16:28
	Modify Time:
	Original Author:	johnson
	Modify		   :	2003-05-20	flymemory
							- fixed some bug
							- Standardization interface 
	More Author:	
	Abstract:			
	
	Note:				

*********************************************************************/


#ifndef __JV3DVECTOR3__H__
#define __JV3DVECTOR3__H__

#ifdef IOS
#define FLT_MAX 3.40282347E+38F
#define FLT_MIN 1.17549435E-38F
#endif

#include "vfxGeomTypes.h"


class v3dxQuaternion;
class v3dxMatrix3;
class v3dxVector2;
class v3dxDVector3;

#pragma pack(push,4)
class v3dxVector3 : public v3dVector3_t
{
public:
	v3dxVector3()
	{
	}
	v3dxVector3( float fX, float fY, float fZ ) 
	{
		X = fX;
		Y = fY;
		Z = fZ;
	}
	v3dxVector3( float afCoordinate[3] )
	{
		X = afCoordinate[0];
		Y = afCoordinate[1];
		Z = afCoordinate[2];
	}
	v3dxVector3( int afCoordinate[3] )
	{
		X = (float)afCoordinate[0];
		Y = (float)afCoordinate[1];
		Z = (float)afCoordinate[2];
	}
	v3dxVector3( const float* const r )
	{
		X = r[0];
		Y = r[1];
		Z = r[2];
	}
	v3dxVector3( const v3dxVector3& rkVector )
	{
		X = rkVector.X;
		Y = rkVector.Y;
		Z = rkVector.Z;
	}
	void setValue(const v3dxVector2& v);


	void setValue(float ix,float iy,float iz){ 
		X=ix;Y=iy;Z=iz; 
	}

	void setValue( const v3dxVector3 &rkVector)
	{
		X = rkVector.X;
		Y = rkVector.Y;
		Z = rkVector.Z;
	}
	inline float getMax() const
	{
		return std::max(std::max(X, Y), Z);
	}
	v3dxVector3 operator+ () const;
	v3dxVector3 operator- () const;
	void operator= ( const v3dxVector3 &vect );


	friend v3dxVector3 operator +(const v3dxVector3& vect1,const v3dxVector3& vect2);
	friend v3dxVector3 operator -(const v3dxVector3& vect1,const v3dxVector3& vect2);
	friend v3dxVector3 operator /(const v3dxVector3& vect1,const v3dxVector3& vect2);

	//friend float operator *(const v3dxVector3& vect1,const v3dxVector3& vect2);
	inline v3dxVector3 operator * ( const v3dxVector3& rhs) const
	{
		return v3dxVector3(
			X * rhs.X,
			Y * rhs.Y,
			Z * rhs.Z);
	}

	friend v3dxVector3 operator* (const v3dxVector3& v, float fValue);
	friend v3dxVector3 operator* (float fValue, const v3dxVector3& v);
	friend v3dxVector3 operator ^(const v3dxVector3& vect1,const v3dxVector3& vect2);
	friend v3dxVector3 operator/ (const v3dxVector3& v, const float fValue);
	/** Returns true if the vector's scalar components are all greater
	that the ones of the vector it is compared against.
	*/
	inline bool operator < ( const v3dxVector3& rhs ) const;

	/** Returns true if the vector's scalar components are all smaller
	that the ones of the vector it is compared against.
	*/
	inline bool operator > ( const v3dxVector3& rhs ) const;
	friend v3dxVector3 operator *(const v3dxVector3& vect,const v3dMatrix4_t& mat);

	inline friend bool operator== (const v3dxVector3& v1, const v3dxVector3& v2){ 
		return v1.X==v2.X && v1.Y==v2.Y && v1.Z==v2.Z; 
	}
	inline friend bool operator!= (const v3dxVector3& v1, const v3dxVector3& v2){ 
		return v1.X!=v2.X || v1.Y!=v2.Y || v1.Z!=v2.Z; 
	}

	inline bool Equals(const v3dxVector3& v,float epsilon = 0.00001f)
	{
		return  std::abs(X - v.X) < epsilon &&  std::abs(Y - v.Y) < epsilon &&  std::abs(Z - v.Z) < epsilon;
	}
	inline friend bool Equals(const v3dxVector3& v1, const v3dxVector3& v2, float epsilon = 0.00001f)
	{
		return  std::abs(v1.X - v2.X) < epsilon &&  std::abs(v1.Y - v2.Y) < epsilon &&  std::abs(v1.Z - v2.Z) < epsilon;
	}
	inline void operator = ( const v3dVector3_t& v2 ){ 
		X=v2.X ; Y=v2.Y ; Z=v2.Z; 
	}

	v3dxVector3& operator +=(const v3dxVector3& vect);
	v3dxVector3& operator -=(const v3dxVector3& vect);
	v3dxVector3& operator *=(float fValue);
	v3dxVector3 operator *=(const v3dxVector3 &v3);
	//v3dxVector3 operator *=(const v3dxQuaternion &quaternion);
	//void		operator*= ( const v3dxMatrix3 &m);
	v3dxVector3& operator /=(float fValue);
	//bool operator == ( const v3dxVector3& vect ) const;
	//bool operator != ( const v3dxVector3& vect ) const;

	/// get left value
	float& operator [](int i);
	/// get right value
	float operator [](int i) const;

	inline friend v3dxVector3 operator>> (const v3dxVector3& v1, const v3dxVector3& v2){ 	
		return v2*(v1.dotProduct(v2)/v2.dotProduct(v2)); 
	}

	void Lerp(const v3dxVector3 &v3, float fSlerp);
	//Slerp ==========================================================
	void SLerp(const v3dxVector3 &v3, float fSlerp);

	/// Get the length of the vector3
	float getLength() const;
	/// Get the square length of the vecotr3
	float getLengthSq() const;
	/// Returns true if this vector is zero length.
	inline bool isZeroLength(void) const;
	/// Normalize the vector3
	void normalize();
	float normalized();
	/// Get the normal
	v3dxVector3 getNormal() const;



	/** Returns a vector at a point half way between this and the passed
	in vector.
	*/
	inline v3dxVector3 midPoint( const v3dxVector3& vec ) const;


	/** Sets this vector's components to the minimum of its own and the 
	ones of the passed in vector.
	@remarks
	'Minimum' in this case means the combination of the lowest
	value of x, y and z from both vectors. Lowest is taken just
	numerically, not magnitude, so -1 < 0.
	*/
	void makeFloor( const v3dxVector3& cmp );


	/** Sets this vector's components to the maximum of its own and the 
	ones of the passed in vector.
	@remarks
	'Maximum' in this case means the combination of the highest
	value of x, y and z from both vectors. Highest is taken just
	numerically, not magnitude, so 1 > -3.
	*/
	void makeCeil( const v3dxVector3& cmp );

	inline float dotProduct(const v3dxVector3& vec) const;
	inline v3dxVector3 crossProduct( const v3dxVector3& rkVector ) const;


	/** Generates a vector perpendicular to this vector (eg an 'up' vector).
	*/
	inline v3dxVector3 perpendicular(bool bXFirst = true) const;

	/** Generates a new random vector which deviates from this vector by a
	given angle in a random direction.
	*/
	inline v3dxVector3 randomDeviant(
			float angle, 
			const v3dxVector3& up = v3dxVector3::ZERO ) const;

	/** Gets the shortest arc quaternion to rotate this vector to the destination
	vector. 
	@remarks
	Don't call this if you think the dest vector can be close to the inverse
	of this vector, since then ANY axis of rotation is ok. 
	*/
	v3dxQuaternion getRotationTo(const v3dxVector3& dest) const;

	// special points
	static const v3dxVector3 ZERO;
	static const v3dxVector3 UNIT_X;
	static const v3dxVector3 UNIT_Y;
	static const v3dxVector3 UNIT_Z;

	static const v3dxVector3 UNIT_MINUS_X;
	static const v3dxVector3 UNIT_MINUS_Y;
	static const v3dxVector3 UNIT_MINUS_Z;

	static const v3dxVector3 UNIT_SCALE;
};

//====global fuction to oparate v3dxVector3 class
v3dxVector3 operator +(const v3dxVector3& vect1,const v3dxVector3& vect2);
v3dxVector3 operator -(const v3dxVector3& vect1,const v3dxVector3& vect2);
v3dxVector3 operator /(const v3dxVector3& vect1, const v3dxVector3& vect2);
//float operator *(const v3dxVector3& vect1,const v3dxVector3& vect2);
v3dxVector3 operator ^(const v3dxVector3& vect1,const v3dxVector3& vect2);

v3dxVector3 operator* (const v3dxVector3& v, const float fValue);
v3dxVector3 operator* (float fValue, const v3dxVector3& v);
v3dxVector3 operator/ (const v3dxVector3& v, const float fValue);

v3dxVector3 operator *(const v3dxVector3& vect,const v3dMatrix4_t& mat);

#include "v3dxVector3.inl.h"

#pragma pack(pop)

#endif//#ifndef __JV3DVECTOR3__H__