/********************************************************************
V3D					A Powerful 3D Enjine
File:				v3dxvector3.h
Created Time:		21:5:2003   16:28
Modify Time:
Original Author:	flyemory
More Author:	
Abstract:			

Note:				

*********************************************************************/

#pragma once

#include "vfxGeomTypes.h"
//#include "v3dxVector3.h"

class v3dxVector3;
class v3dxQuaternion;

#pragma pack(push,4)

class v3dxMatrix3 : public v3dMatrix3_t
{
	public:
		// construction ------------------------------------
		v3dxMatrix3();
		v3dxMatrix3(const float data[3][3]);
		//v3dxMatrix3(const v3dxQuaternion &quat);
		v3dxMatrix3(const v3dxMatrix3& Matrix);
		v3dxMatrix3(float x11, float x12, float x13,
					float x21, float x22, float x23,
					float x31, float x32, float x33);

		~v3dxMatrix3();

		// data access -------------------------------------
		operator float* ();
		float* operator[] (int nRow) const;

		v3dxVector3 getColumn (int nCol) const;
		void setColumn(int nCol, const v3dxVector3& v3);
		void fromAxes(const v3dxVector3& xAxis, const v3dxVector3& yAxis, const v3dxVector3& zAxis);

		// assignment and comparison -----------------------
		v3dxMatrix3& operator= (const v3dxMatrix3& Matrix);
		 void operator= (const v3dxQuaternion& quat);
		bool operator== (const v3dxMatrix3& Matrix) const;
		bool operator!= (const v3dxMatrix3& Matrix) const;

		// arithmetic operations ---------------------------
		v3dxMatrix3 operator+ (const v3dxMatrix3& Matrix) const;
		v3dxMatrix3 operator- (const v3dxMatrix3& Matrix) const;
		v3dxMatrix3 operator- () const;
		v3dxMatrix3 operator* (const v3dxMatrix3& Matrix) const;

		// matrix * vector [3x3 * 3x1 = 3x1] ---------------
		v3dxVector3 operator* (const v3dxVector3& v3) const;

		// vector * matrix [1x3 * 3x3 = 1x3] ---------------
		friend v3dxVector3 operator* (const v3dxVector3& v3, const v3dxMatrix3& Matrix); 

		// matrix * scalar
		v3dxMatrix3 operator* (float fScalar) const;

		// scalar * matrix
		friend v3dxMatrix3 operator* (float fScalar, const v3dxMatrix3& Matrix);

		// utilities
		v3dxMatrix3 transpose () const;
		bool inverse (v3dxMatrix3& rkInverse, float fTolerance = 1e-06) const;
		v3dxMatrix3 inverse (float fTolerance = 1e-06) const;
		float determinant () const;


		// singular value decomposition
		 void singularValueDecomposition (v3dxMatrix3& rkL, v3dxVector3& rkS,
			v3dxMatrix3& rkR) const;
		 void singularValueComposition (const v3dxMatrix3& rkL,
			const v3dxVector3& rkS, const v3dxMatrix3& rkR);

		// Gram-Schmidt orthonormalization (applied to columns of rotation matrix)
		 void orthonormalize ();

		// orthogonal Q, diagonal D, upper triangular U stored as (u01,u02,u12)
		 void QDUDecomposition (v3dxMatrix3& rkQ, v3dxVector3& rkD,
			v3dxVector3& rkU) const;

		 float spectralNorm () const;

		// matrix must be orthonormal
		void toAxisAngle (v3dxVector3& vAxisRet, float& fRadiansRet) const;
		void fromAxisAngle (const v3dxVector3& vAxis, float fRadians);

		/** The matrix must be orthonormal.  The decomposition is yaw*pitch*roll
		 *	where yaw is rotation about the Up vector, pitch is rotation about the	
		 *	Right axis, and roll is rotation about the Direction axis
		 */	 
		bool toEulerAnglesXYZ (float& fYAngle, float& fPAngle, float& fRAngle) const;
		bool toEulerAnglesXZY (float& fYAngle, float& fPAngle, float& fRAngle) const;
		bool toEulerAnglesYXZ (float& fYAngle, float& fPAngle, float& fRAngle) const;
		bool toEulerAnglesYZX (float& fYAngle, float& fPAngle, float& fRAngle) const;
		bool toEulerAnglesZXY (float& fYAngle, float& fPAngle, float& fRAngle) const;
		bool toEulerAnglesZYX (float& rfYAngle, float& rfPAngle, float& rfRAngle) const;
		void fromEulerAnglesXYZ (float fYAngle, float fPAngle, float fRAngle);
		void fromEulerAnglesXZY (float fYAngle, float fPAngle, float fRAngle);
		void fromEulerAnglesYXZ (float fYAngle, float fPAngle, float fRAngle);
		void fromEulerAnglesYZX (float fYAngle, float fPAngle, float fRAngle);
		void fromEulerAnglesZXY (float fYAngle, float fPAngle, float fRAngle);
		void fromEulerAnglesZYX (float fYAngle, float fPAngle, float fRAngle);

		 const static v3dxMatrix3 ZERO;
		 const static v3dxMatrix3 IDENTITY;
protected:
	// support for eigensolver
	 void Tridiagonal (float afDiag[3], float afSubDiag[3]);
	 bool QLAlgorithm (float afDiag[3], float afSubDiag[3]);

	// support for singular value decomposition
	static const float ms_fSvdEpsilon;
	static const int ms_iSvdMaxIterations;
	 static void Bidiagonalize (v3dxMatrix3& kA, v3dxMatrix3& kL,
		v3dxMatrix3& kR);
	 static void GolubKahanStep (v3dxMatrix3& kA, v3dxMatrix3& kL,
		v3dxMatrix3& kR);

	// support for spectral norm
	 static float MaxCubicRoot (float afCoeff[3]);

};

#pragma pack(pop)

#include "v3dxMatrix3.inl.h"
