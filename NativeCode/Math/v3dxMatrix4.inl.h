// v3dxMatrix4.inl
#include "v3dxMath.h"
#include "v3dxMatrix3.h"

inline void v3dxMatrix4::makeTrans( const v3dxVector3& v )
{
	m[0][0] = 1.0; m[0][1] = 0.0; m[0][2] = 0.0; m[0][3] = 0;
	m[1][0] = 0.0; m[1][1] = 1.0; m[1][2] = 0.0; m[1][3] = 0;
	m[2][0] = 0.0; m[2][1] = 0.0; m[2][2] = 1.0; m[2][3] = 0;
	m[3][0] = v.x; m[3][1] = v.y; m[3][2] = v.z; m[3][3] = 1.0;
}
inline void v3dxMatrix4::makeTrans( float tx, float ty, float tz )
{
	m[0][0] = 1.0; m[0][1] = 0.0; m[0][2] = 0.0; m[0][3] = 0.0;
	m[1][0] = 0.0; m[1][1] = 1.0; m[1][2] = 0.0; m[1][3] = 0.0;
	m[2][0] = 0.0; m[2][1] = 0.0; m[2][2] = 1.0; m[2][3] = 0.0;
	m[3][0] = tx; m[3][1] = ty; m[3][2] = tz; m[3][3] = 1.0;
}

inline void v3dxMatrix4::makeTrans( const v3dxVector3& position, 
										 const v3dxVector3& scale, 
										 const v3dxQuaternion& orientation)
{
	v3dxMatrixTransformationOrigin( this , &scale , &orientation , &position);
	/*identity();

	v3dxMatrix3 rot3x3, scale3x3;
	orientation.toRotationMatrix(rot3x3);
	scale3x3 = v3dxMatrix3::ZERO;
	scale3x3[0][0] = scale.x;
	scale3x3[1][1] = scale.y;
	scale3x3[2][2] = scale.z;

	*this = rot3x3 * scale3x3;
	setTrans(position);*/
}

/** Gets a translation matrix.
*/
inline v3dxMatrix4 v3dxMatrix4::getTrans( const v3dxVector3& v )
{
	v3dxMatrix4 r;

	r.m[0][0] = 1.0; r.m[0][1] = 0.0; r.m[0][2] = 0.0; r.m[0][3] = 0.0;
	r.m[1][0] = 0.0; r.m[1][1] = 1.0; r.m[1][2] = 0.0; r.m[1][3] = 0.0;
	r.m[2][0] = 0.0; r.m[2][1] = 0.0; r.m[2][2] = 1.0; r.m[2][3] = 0.0;
	r.m[3][0] = v.x; r.m[3][1] = v.y; r.m[3][2] = v.z; r.m[3][3] = 1.0;

	return r;
}

/** Gets a translation matrix - variation for not using a vector.
*/
inline v3dxMatrix4 v3dxMatrix4::getTrans( float t_x, float t_y, float t_z )
{
	v3dxMatrix4 r;

	r.m[0][0] = 1.0; r.m[0][1] = 0.0; r.m[0][2] = 0.0; r.m[0][3] = 0.0;
	r.m[1][0] = 0.0; r.m[1][1] = 1.0; r.m[1][2] = 0.0; r.m[1][3] = 0.0;
	r.m[2][0] = 0.0; r.m[2][1] = 0.0; r.m[2][2] = 1.0; r.m[2][3] = 0.0;
	r.m[3][0] = t_x; r.m[3][1] = t_y; r.m[3][2] = t_z; r.m[3][3] = 1.0;

	return r;
}

/*
-----------------------------------------------------------------------
Scale Transformation
-----------------------------------------------------------------------
*/
/** Sets the scale part of the matrix.
*/
inline void v3dxMatrix4::setScale( const v3dxVector3& v )
{
	m[0][0] = v.x;
	m[1][1] = v.y;
	m[2][2] = v.z;
}

/** Gets a scale matrix.
*/
inline v3dxMatrix4 v3dxMatrix4::getScale( const v3dxVector3& v )
{
	v3dxMatrix4 r;
	r.m[0][0] = v.x; r.m[0][1] = 0.0; r.m[0][2] = 0.0; r.m[0][3] = 0.0;
	r.m[1][0] = 0.0; r.m[1][1] = v.y; r.m[1][2] = 0.0; r.m[1][3] = 0.0;
	r.m[2][0] = 0.0; r.m[2][1] = 0.0; r.m[2][2] = v.z; r.m[2][3] = 0.0;
	r.m[3][0] = 0.0; r.m[3][1] = 0.0; r.m[3][2] = 0.0; r.m[3][3] = 1.0;

	return r;
}

/** Gets a scale matrix - variation for not using a vector.
*/
inline v3dxMatrix4 v3dxMatrix4::getScale( float s_x, float s_y, float s_z )
{
	v3dxMatrix4 r;
	r.m[0][0] = s_x; r.m[0][1] = 0.0; r.m[0][2] = 0.0; r.m[0][3] = 0.0;
	r.m[1][0] = 0.0; r.m[1][1] = s_y; r.m[1][2] = 0.0; r.m[1][3] = 0.0;
	r.m[2][0] = 0.0; r.m[2][1] = 0.0; r.m[2][2] = s_z; r.m[2][3] = 0.0;
	r.m[3][0] = 0.0; r.m[3][1] = 0.0; r.m[3][2] = 0.0; r.m[3][3] = 1.0;

	return r;
}

/** Extracts the rotation / scaling part of the Matrix as a 3x3 matrix. 
@param m3x3 Destination Matrix3
*/
inline void v3dxMatrix4::extract3x3Matrix(v3dxMatrix3& m3x3) const
{
	m3x3.m[0][0] = m[0][0];
	m3x3.m[0][1] = m[0][1];
	m3x3.m[0][2] = m[0][2];
	m3x3.m[1][0] = m[1][0];
	m3x3.m[1][1] = m[1][1];
	m3x3.m[1][2] = m[1][2];
	m3x3.m[2][0] = m[2][0];
	m3x3.m[2][1] = m[2][1];
	m3x3.m[2][2] = m[2][2];

}

inline float MINOR(const v3dxMatrix4& m, const int r0, const int r1, const int r2, const int c0, const int c1, const int c2)
{
	return m[r0][c0] * (m[r1][c1] * m[r2][c2] - m[r2][c1] * m[r1][c2]) -
		m[r0][c1] * (m[r1][c0] * m[r2][c2] - m[r2][c0] * m[r1][c2]) +
		m[r0][c2] * (m[r1][c0] * m[r2][c1] - m[r2][c0] * m[r1][c1]);
}

inline v3dxMatrix4 v3dxMatrix4::adjoint() const
{
	return v3dxMatrix4( MINOR(*this, 1, 2, 3, 1, 2, 3),
		-MINOR(*this, 0, 2, 3, 1, 2, 3),
		MINOR(*this, 0, 1, 3, 1, 2, 3),
		-MINOR(*this, 0, 1, 2, 1, 2, 3),

		-MINOR(*this, 1, 2, 3, 0, 2, 3),
		MINOR(*this, 0, 2, 3, 0, 2, 3),
		-MINOR(*this, 0, 1, 3, 0, 2, 3),
		MINOR(*this, 0, 1, 2, 0, 2, 3),

		MINOR(*this, 1, 2, 3, 0, 1, 3),
		-MINOR(*this, 0, 2, 3, 0, 1, 3),
		MINOR(*this, 0, 1, 3, 0, 1, 3),
		-MINOR(*this, 0, 1, 2, 0, 1, 3),

		-MINOR(*this, 1, 2, 3, 0, 1, 2),
		MINOR(*this, 0, 2, 3, 0, 1, 2),
		-MINOR(*this, 0, 1, 3, 0, 1, 2),
		MINOR(*this, 0, 1, 2, 0, 1, 2));
}

inline float v3dxMatrix4::determinant() const
{
	return m[0][0] * MINOR(*this, 1, 2, 3, 1, 2, 3) -
		m[0][1] * MINOR(*this, 1, 2, 3, 0, 2, 3) +
		m[0][2] * MINOR(*this, 1, 2, 3, 0, 1, 3) -
		m[0][3] * MINOR(*this, 1, 2, 3, 0, 1, 2);
}

inline v3dxMatrix4 v3dxMatrix4::inverse() const
{
	return adjoint() * (1.0f / determinant());
}

inline v3dxMatrix4 v3dxMatrix4::operator * ( const v3dxMatrix4& Mat) const
{
	v3dxMatrix4 mat;
	v3dxMatrix4Mul( &mat , this , &Mat );
	return mat;
}
inline v3dxMatrix4 v3dxMatrix4::operator + ( const v3dxMatrix4& Mat) const
{
	v3dxMatrix4 mat;
	v3dxMatrix4Add( &mat , this , &Mat );
	return mat;
}
inline v3dxMatrix4 v3dxMatrix4::operator - ( const v3dxMatrix4& Mat) const
{
	v3dxMatrix4 mat;
	v3dxMatrix4Sub( &mat , this , &Mat );
	return mat;
}
inline void v3dxMatrix4::operator = ( const v3dxMatrix3& Mat3 )
{
	m[0][0] = Mat3.m[0][0]; m[0][1] = Mat3.m[0][1]; m[0][2] = Mat3.m[0][2];
	m[1][0] = Mat3.m[1][0]; m[1][1] = Mat3.m[1][1]; m[1][2] = Mat3.m[1][2];
	m[2][0] = Mat3.m[2][0]; m[2][1] = Mat3.m[2][1]; m[2][2] = Mat3.m[2][2];
}
inline v3dxMatrix4& v3dxMatrix4::operator *= ( const v3dxMatrix4& Mat)
{
	v3dxMatrix4 mat = *this;
	return *(v3dxMatrix4 *)v3dxMatrix4Mul( (v3dMatrix4_t*)this , &mat , &Mat );
}
inline v3dxMatrix4& v3dxMatrix4::operator += ( const v3dxMatrix4& Mat)
{
	return *(v3dxMatrix4 *)v3dxMatrix4Add( (v3dMatrix4_t*)this , this , &Mat );
}
inline v3dxMatrix4& v3dxMatrix4::operator -= ( const v3dxMatrix4& Mat)
{
	return *(v3dxMatrix4 *)v3dxMatrix4Sub( (v3dMatrix4_t*)this , this , &Mat );
}
inline v3dxMatrix4 v3dxMatrix4::operator * ( float f) const
{
	v3dxMatrix4 mat;
	v3dxMatrix4Mul( &mat , this , f );
	return mat;
}
inline v3dxMatrix4 v3dxMatrix4::operator / ( float f) const
{
	v3dxMatrix4 mat;
	v3dxMatrix4Mul( &mat , this , 1.f/f );
	return mat;
}
inline v3dxMatrix4& v3dxMatrix4::operator *= ( float f)
{
	return *(v3dxMatrix4 *)v3dxMatrix4Mul( (v3dMatrix4_t*)this , this , f );
}
inline v3dxMatrix4& v3dxMatrix4::operator /= ( float f)
{
	return *(v3dxMatrix4 *)v3dxMatrix4Mul( (v3dMatrix4_t*)this , this , 1.f/f );
}
inline v3dxMatrix4 operator *( float f , const v3dxMatrix4& Mat)
{ 
	return Mat.operator * (f); 
}
inline bool v3dxMatrix4::operator == ( const v3dxMatrix4& Mat) const
{
	for(int i=0;i<4;i++)
	{
		for(int j=0;j<4;j++)
		{
			if(Mat.m[i][j]!=m[i][j])
			{
				return false;
			}
		}
	}
	return true;
}
inline bool v3dxMatrix4::operator != ( const v3dxMatrix4& Mat) const
{
	for(int i=0;i<4;i++)
	{
		for(int j=0;j<4;j++)
		{
			if(Mat.m[i][j]!=m[i][j])
			{
				return true;
			}
		}
	}
	return false;
}

inline const v3dxVector3 & v3dxMatrix4::getRow1() const
{
	return *(v3dxVector3 *)&m11;
}

inline const v3dxVector3 & v3dxMatrix4::getRow2() const
{
	return *(v3dxVector3 *)&m21;
}

inline const v3dxVector3 & v3dxMatrix4::getRow3() const
{
	return *(v3dxVector3 *)&m31;
}

inline const v3dxVector3 & v3dxMatrix4::getRow4() const
{
	return *(v3dxVector3 *)&m41;
}

inline v3dxVector3 & v3dxMatrix4::getRow1()
{
	return *(v3dxVector3 *)&m11;
}

inline v3dxVector3 & v3dxMatrix4::getRow2()
{
	return *(v3dxVector3 *)&m21;
}

inline v3dxVector3 & v3dxMatrix4::getRow3()
{
	return *(v3dxVector3 *)&m31;
}

inline v3dxVector3 & v3dxMatrix4::getRow4()
{
	return *(v3dxVector3 *)&m41;
}

inline void v3dxMatrix4::zeroMatrix()
{
	m11=m12=m13=m14=
		m21=m22=m23=m24=
		m31=m32=m33=m34=
		m41=m42=m43=m44=0.0f;
}

inline void v3dxMatrix4::identity()
{
	m12=m13=m14=
		m21=m23=m24=
		m31=m32=m34=
		m41=m42=m43=0.0f;
	m11=m22=m33=m44=1.0f;
}
inline void v3dxMatrix4::moveMatrix(float x,float y,float z)
{
	m12=m13=m14=
		m21=m23=m24=
		m31=m32=m34=0.0f;
	m11=m22=m33=m44=1.0f;
	m41=x,m42=y,m43=z;
}
inline void v3dxMatrix4::setTrans(float x,float y,float z)
{
	m41=x;
	m42=y; 
	m43=z;
}

inline void v3dxMatrix4::setTrans( const v3dxVector3 &v )
{
	m41 = v.x;
	m42 = v.y;
	m43 = v.z;
}

inline v3dxVector3 v3dxMatrix4::getTrans() const
{
	return v3dxVector3(m41,m42,m43);
}
inline void v3dxMatrix4::scaleMatrix(float x,float y,float z)
{
	v3dxMatrix4Scale( this , x , y , z );
	/*m12=m13=m14=
		m21=m23=m24=
		m31=m32=m34=
		m41=m42=m43=0.0f;
	m11=x,m22=y,m33=z,m44=1.0f;*/
}
inline void v3dxMatrix4::rotationMatrixX(float angle)
{
	v3dxMatrix4RotationX( this , angle );
	/*m12 = m13 = m14 = 
		m21 = m24 = 
		m31 = m34 = 
		m41 = m42 = m43 = 0.0f;
	m11 = m44 = 1.0f;
	m22 = cosf(angle);
	m33 = -cosf(angle);
	m32 = sinf(angle);
	m23 = -m32;*/
}
inline void v3dxMatrix4::rotationMatrixY(float angle)
{
	v3dxMatrix4RotationY( this , angle );
	/*m12 = m14 = 
		m21 = m23 = m24 = 0.0f;
	m32 = m34 = 
		m41 = m42 = m43 = 0.0f;         
	m22 = m44 = 1.0f;
	m11 = m33 = cosf(angle);
	m31 = sinf(angle); 
	m13 = -m31;*/
}
inline void v3dxMatrix4::rotationMatrixZ(float angle)
{
	v3dxMatrix4RotationZ( this , angle );
	/*m13 = m14 = 
		m23 = m24 = 
		m31 = m32 = m34 = 
		m41 = m42 = m43 = 0.0f; 
	m33 = m44 = 1.0f;
	m11 = m22 = cosf(angle);
	m12 = sinf(angle); 
	m21 = -m12;*/
}
inline void v3dxMatrix4::rotationAxis(const v3dVector3_t* vAxis,float fAngle)
{
	v3dxMatrix4RotationAxis(this,vAxis,fAngle);
}
inline void v3dxMatrix4::transPose()
{
	v3dxMatrix4 matTMP(*this);
	m12 = matTMP.m21; m13 = matTMP.m31; m14 = matTMP.m41;
	m21 = matTMP.m12; m23 = matTMP.m32; m24 = matTMP.m42;
	m31 = matTMP.m13; m32 = matTMP.m23; m34 = matTMP.m43;
	m41 = matTMP.m14; m42 = matTMP.m24; m43 = matTMP.m34;
}
inline vBOOL v3dxMatrix4::getInverse(v3dxMatrix4* pMat,float* pDeterminant)
{
	return NULL != v3dxMatrix4Inverse( this , pMat , pDeterminant );
}

inline float	v3dxMatrix4::operator()( int iRow, int iCol ) const
{
	return m[ iRow ][ iCol ];
}

inline float&	v3dxMatrix4::operator()( int iRow, int iCol )
{
	return m[ iRow ][ iCol ];
}

inline vBOOL v3dxMatrix4::ExtractionTrans( v3dxVector3& vTransPos ) const
{
	vTransPos.setValue(m41,m42,m43);
	return TRUE;
}

inline vBOOL v3dxMatrix4::ExtractionRotation( v3dxQuaternion& vRotation ) const
{
	vRotation.fromRotationMatrix( *this );
	return TRUE;
}

inline vBOOL v3dxMatrix4::ExtractionScale( v3dxVector3& vScale ) const
{
	//vScale.x = sqrt(m11*m11 + m21*m21 + m31*m31); // getRow1().getLength();
	//vScale.y = sqrt(m12*m12 + m22*m22 + m32*m32);
	//vScale.z = sqrt(m13*m13 + m23*m23 + m33*m33);
	vScale.x = sqrt(m11*m11 + m12*m12 + m13*m13); // getRow1().getLength();
	vScale.y = sqrt(m21*m21 + m22*m22 + m23*m23);
	vScale.z = sqrt(m31*m31 + m32*m32 + m33*m33);
	return TRUE;
}