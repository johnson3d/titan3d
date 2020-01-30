/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxmatrix4.h
	Created Time:		30:6:2002   16:31
	Modify Time:
	Original Author:	johnson
	Modify:				2003-05-21	flymemory
							- fixed some bug
							- Standardization interfaces 
	Abstract:			
	
	Note:				

*********************************************************************/

#if !defined(__JV3DXMATRIX4_H__)
#define __JV3DXMATRIX4_H__

#include "vfxGeomTypes.h"
#include "v3dxVector3.h"
#include "v3dxQuaternion.h"

#pragma pack(push,4)

class v3dxMatrix4 : public v3dMatrix4_t
{
public:
	v3dxMatrix4(){}
	v3dxMatrix4( const float* pUnit ){	
		memcpy(m,pUnit,sizeof(float)*16); 
	}
	v3dxMatrix4( 
		float _11, float _12, float _13, float _14,
		float _21, float _22, float _23, float _24,
		float _31, float _32, float _33, float _34,
		float _41, float _42, float _43, float _44 ){
		m11=_11,m12=_12,m13=_13,m14=_14,
		m21=_21,m22=_22,m23=_23,m24=_24,
		m31=_31,m32=_32,m33=_33,m34=_34,
		m41=_41,m42=_42,m43=_43,m44=_44;
	}

	v3dxMatrix4 operator * ( const v3dxMatrix4& ) const;
	v3dxMatrix4 operator + ( const v3dxMatrix4& Mat) const;
	v3dxMatrix4 operator - ( const v3dxMatrix4& Mat) const;

	void operator = ( const v3dxMatrix3& Mat3 );
	v3dxMatrix4& operator *= ( const v3dxMatrix4& Mat);
	v3dxMatrix4& operator += ( const v3dxMatrix4& Mat);
	v3dxMatrix4& operator -= ( const v3dxMatrix4& Mat);

	v3dxMatrix4 operator * ( float f) const;
	v3dxMatrix4 operator / ( float f) const;
	friend v3dxMatrix4 operator *( float f , const v3dxMatrix4& Mat);

	v3dxMatrix4& operator *= ( float f);
	v3dxMatrix4& operator /= ( float f);

	bool operator == ( const v3dxMatrix4& ) const;
	bool operator != ( const v3dxMatrix4& ) const;
	float* operator[] (int iRow) const{
		return (float*)&m[iRow][0];
	}
	float	operator()( int iRow, int iCol ) const;
	float&	operator()( int iRow, int iCol );

	const v3dxVector3 & getRow1() const;
	const v3dxVector3 & getRow2() const;
	const v3dxVector3 & getRow3() const;
	const v3dxVector3 & getRow4() const;
	v3dxVector3 & getRow1();
	v3dxVector3 & getRow2();
	v3dxVector3 & getRow3();
	v3dxVector3 & getRow4();
	void zeroMatrix();
	void identity();//得到单位矩阵

	void moveMatrix(float x,float y,float z);//得到平移矩阵
	void setTrans(float x,float y,float z);//设置X,Y,Z值
	void setTrans( const v3dxVector3 &v );
	inline void makeTrans( const v3dxVector3& v );
	inline void makeTrans( float tx, float ty, float tz );
	inline void makeTrans( const v3dxVector3& position, const v3dxVector3& scale, 
		const v3dxQuaternion& orientation);
	/** Gets a translation matrix.
	*/
	inline v3dxMatrix4 getTrans( const v3dxVector3& v );
	v3dxVector3 getTrans() const;
	/** Gets a translation matrix - variation for not using a vector.
	*/
	inline v3dxMatrix4 getTrans( float t_x, float t_y, float t_z );

	void scaleMatrix(float x,float y,float z);
	/** Sets the scale part of the matrix.
	*/
	inline void setScale( const v3dxVector3& v );
	/** Gets a scale matrix.
	*/
	inline v3dxMatrix4 getScale( const v3dxVector3& v );
	/** Gets a scale matrix - variation for not using a vector.
	*/
	inline v3dxMatrix4 getScale( float s_x, float s_y, float s_z );

	/** Extracts the rotation / scaling part of the Matrix as a 3x3 matrix. 
	@param m3x3 Destination Matrix3
	*/
	inline void extract3x3Matrix(v3dxMatrix3& m3x3) const;

	inline vBOOL ExtractionTrans( v3dxVector3& vTransPos ) const;
	inline vBOOL ExtractionRotation( v3dxQuaternion& vRotation ) const;
	inline vBOOL ExtractionScale( v3dxVector3& vScale ) const;

	inline HRESULT Decompose(v3dxVector3& Scale, v3dxVector3& Pos, v3dxQuaternion& Quat) const
	{
		return v3dxMatrixDecompose( &Scale , &Quat , &Pos , this );
	}

	void rotationMatrixX(float angle);
	void rotationMatrixY(float angle);
	void rotationMatrixZ(float angle);
	void rotationAxis(const v3dVector3_t* vAxis,float fAngle);
	void transPose();
	vBOOL getInverse(v3dxMatrix4* pMat,float* pDeterminant=NULL);

	v3dxMatrix4 adjoint() const;
	float determinant() const;
	v3dxMatrix4 inverse() const;

	 static const v3dxMatrix4 ZERO;
	 static const v3dxMatrix4 IDENTITY;

};

#include "v3dxMatrix4.inl.h"

#pragma pack(pop)

#endif//#ifndef __JV3DXMATRIX4_H__