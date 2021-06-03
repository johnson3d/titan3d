#include "v3dxMath.h"
#include "v3dxVector3.h"

/////////////////////////////////////////////////////////////////////


inline v3dxMatrix3::v3dxMatrix3()
{
}

inline v3dxMatrix3::v3dxMatrix3(const float data[3][3])
{
	memcpy(m,data, 9);
}

inline v3dxMatrix3::v3dxMatrix3(const v3dxMatrix3& Matrix)
{
	*this = Matrix;
}

inline v3dxMatrix3::v3dxMatrix3(float x11, float x12, float x13,
						float x21, float x22, float x23,
						float x31, float x32, float x33)
{
	m[0][0] = x11; m[0][1] = x12; m[0][2] = x13;
	m[1][0] = x21; m[1][1] = x22; m[1][2] = x23;
	m[2][0] = x31; m[2][1] = x32; m[2][2] = x33;
}

inline v3dxMatrix3::~v3dxMatrix3()
{
}

// data access -------------------------------------------------------


/** get the private data buffer
*��	@param strPara
*		String of the paramater
*	@return 
*		The pointer of private data
*/
inline v3dxMatrix3::operator float*()
{
	return &m[0][0];
}


//*-------------------------------------------------------------*
//Function Name: operator []
//Useage:		 random access routine (get a row of private data)
//Note:			 
//Parament:		 nRow : row of u want get
//return:		 the row pointer of private data
//*-------------------------------------------------------------*
inline float* v3dxMatrix3::operator [](int nRow) const
{
	if (nRow >= 3) return NULL;

	return (float*)&m[nRow][0];
}

//*-------------------------------------------------------------*
//Function Name: SetColumn
//Useage:		 Set a column by a vector
//Note:			 
//Parament:		 nCol : Column of u want get
//return:		 N/A
//*-------------------------------------------------------------*
inline void v3dxMatrix3::setColumn(int nCol, const v3dxVector3& v3)
{
	if (nCol >= 3) return;

	m[nCol][0] = v3.x;
	m[nCol][1] = v3.y;
	m[nCol][2] = v3.z;
}

//*-------------------------------------------------------------*
inline void v3dxMatrix3::fromAxes(const v3dxVector3& xAxis, const v3dxVector3& yAxis, const v3dxVector3& zAxis)
{
	setColumn(0, xAxis);
	setColumn(1, yAxis);
	setColumn(2, zAxis);
}

inline v3dxVector3 v3dxMatrix3::getColumn (int nCol) const
{
	v3dxVector3 vTemp(m[0][nCol], m[1][nCol], m[2][nCol]);

	return vTemp;
}

//*-------------------------------------------------------------*
inline v3dxMatrix3& v3dxMatrix3::operator= (const v3dxMatrix3& Matrix)
{
	memcpy(m,Matrix.m,9*sizeof(float));
	return *this;
}

//*-------------------------------------------------------------*
//Function Name: operator ==
//Useage:		 Set a column by a vector
//Note:			 
//Parament:		 nCol : Column of u want get
//return:		 N/A
//*-------------------------------------------------------------*
inline bool v3dxMatrix3::operator == (const v3dxMatrix3& Matrix) const
{
	for (int nRow=0; nRow<3; nRow++)
		for (int nCol=0; nCol<3; nCol++)
		{
			if (Matrix[nRow][nCol] != m[nRow][nCol])
				return false;
		}

		return true;
}

inline bool v3dxMatrix3::operator != (const v3dxMatrix3& Matrix) const
{
	return !(*this == Matrix);
}

// arithmetic operations ---------------------------
inline v3dxMatrix3 v3dxMatrix3::operator+ (const v3dxMatrix3& Matrix) const
{
	v3dxMatrix3 matrix3;

	for (int nRow=0; nRow<3; nRow++)
		for (int nCol=0; nCol<3; nCol++)
		{
			matrix3[nRow][nCol] = m[nRow][nCol] + Matrix[nRow][nCol];
		}

		return matrix3;
}

inline v3dxMatrix3 v3dxMatrix3::operator- (const v3dxMatrix3& Matrix) const
{
	v3dxMatrix3 matrix3;

	for (int nRow=0; nRow<3; nRow++)
		for (int nCol=0; nCol<3; nCol++)
		{
			matrix3[nRow][nCol] = m[nRow][nCol] - Matrix[nRow][nCol];
		}

		return matrix3;
}

inline v3dxMatrix3 v3dxMatrix3::operator- () const
{
	v3dxMatrix3 matrix3;

	for (int nRow=0; nRow<3; nRow++)
		for (int nCol=0; nCol<3; nCol++)
		{
			matrix3[nRow][nCol] = -m[nRow][nCol];
		}

		return matrix3;
}

inline v3dxMatrix3 v3dxMatrix3::operator* (const v3dxMatrix3& Matrix) const
{
	v3dxMatrix3 matrix3;

	for (int nRow=0; nRow<3; nRow++)
		for (int nCol=0; nCol<3; nCol++)
		{
			matrix3[nRow][nCol] = 
				m[nRow][0] * Matrix[0][nCol]+
				m[nRow][1] * Matrix[1][nCol]+
				m[nRow][2] * Matrix[2][nCol];
		}

		return matrix3;
}

//*-------------------------------------------------------------*
//Function Name: operator *
//Useage:		 get the private data buffer
//Note:			 
//Parament:		 N/A
//return:		 the pointer of private data
//*-------------------------------------------------------------*
inline v3dxVector3 v3dxMatrix3::operator *(const v3dxVector3& v3) const
{
	v3dxVector3 vector3;
	for (int nRow = 0; nRow < 3; nRow++)
	{
		vector3[nRow] =
			m[0][nRow]*v3.x +
			m[1][nRow]*v3.y +
			m[2][nRow]*v3.z;
	}
	return vector3;	
}

inline v3dxVector3 operator* (const v3dxVector3& v3, const v3dxMatrix3& Matrix)
{

	v3dxVector3 vector3;
	for (int nRow = 0; nRow < 3; nRow++)
	{
		vector3[nRow] =
			v3[0]*Matrix[0][nRow] +
			v3[1]*Matrix[1][nRow] +
			v3[2]*Matrix[2][nRow];
	}

	return vector3;
}

inline v3dxMatrix3 v3dxMatrix3::operator *(float fScalar) const
{
	v3dxMatrix3 matrix3;

	for (int nRow=0; nRow<3; nRow++)
		for (int nCol=0; nCol<3; nCol++)
		{
			matrix3[nRow][nCol] = m[nRow][nCol]*fScalar;
		}

		return matrix3;
}

inline v3dxMatrix3 operator*(float fScalar, const v3dxMatrix3 &Matrix)
{
	v3dxMatrix3 matrix3;

	for (int nRow=0; nRow<3; nRow++)
		for (int nCol=0; nCol<3; nCol++)
		{
			matrix3[nRow][nCol] = Matrix[nRow][nCol]*fScalar;
		}

		return matrix3;
}

inline v3dxMatrix3 v3dxMatrix3::transpose() const
{
	v3dxMatrix3 matrix3;

	for (int nRow=0; nRow<3; nRow++)
		for (int nCol=0; nCol<3; nCol++)
		{
			matrix3[nRow][nCol] = m[nCol][nRow];
		}

	return matrix3;
}

inline bool v3dxMatrix3::inverse(v3dxMatrix3 &retMatrix, float fTolerance /* = 1e-06 */) const 
{
	// Invert a 3x3 using cofactors.  This is about 8 times faster than
	// the Numerical Recipes code which uses Gaussian elimination.

	retMatrix[0][0] = m[1][1]*m[2][2] -
		m[1][2]*m[2][1];
	retMatrix[0][1] = m[0][2]*m[2][1] -
		m[0][1]*m[2][2];
	retMatrix[0][2] = m[0][1]*m[1][2] -
		m[0][2]*m[1][1];
	retMatrix[1][0] = m[1][2]*m[2][0] -
		m[1][0]*m[2][2];
	retMatrix[1][1] = m[0][0]*m[2][2] -
		m[0][2]*m[2][0];
	retMatrix[1][2] = m[0][2]*m[1][0] -
		m[0][0]*m[1][2];
	retMatrix[2][0] = m[1][0]*m[2][1] -
		m[1][1]*m[2][0];
	retMatrix[2][1] = m[0][1]*m[2][0] -
		m[0][0]*m[2][1];
	retMatrix[2][2] = m[0][0]*m[1][1] -
		m[0][1]*m[1][0];

	// Determinant of this matrix
	float fDet =
		m[0][0]*retMatrix[0][0] +
		m[0][1]*retMatrix[1][0]+
		m[0][2]*retMatrix[2][0];

	if ( fabs(fDet) <= fTolerance )
		return false;

	float fInvDet = 1.0f/fDet;
	for (int nRow = 0; nRow < 3; nRow++)
	{
		for (int nCol = 0; nCol < 3; nCol++)
			retMatrix[nRow][nCol] *= fInvDet;
	}

	return true;
}

inline v3dxMatrix3 v3dxMatrix3::inverse(float fTolerance /* = 1e-06 */) const
{
	v3dxMatrix3 matrix3 = v3dxMatrix3::ZERO;

	inverse(matrix3, fTolerance);

	return matrix3;
}

inline float v3dxMatrix3::determinant() const
{
	float fCofactor00 = m[1][1]*m[2][2] - m[1][2]*m[2][1];
	float fCofactor10 = m[1][2]*m[2][0] - m[1][0]*m[2][2];
	float fCofactor20 = m[1][0]*m[2][1] - m[1][1]*m[2][0];

	float fDet =
		m[0][0]*fCofactor00 +
		m[0][1]*fCofactor10 +
		m[0][2]*fCofactor20;

	return fDet;
}

inline void v3dxMatrix3::toAxisAngle(v3dxVector3& vAxisRet, float& fRadiansRet) const
{
	float fTrace = m[0][0] + m[1][1] + m[2][2];
	float fCos = 0.5f*(fTrace-1.0f);
	fRadiansRet = acosf(fCos);  // in [0,PI]

	if ( fRadiansRet > 0.0f )
	{
		if ( fRadiansRet < V_PI )
		{
			vAxisRet[0] = m[2][1]-m[1][2];
			vAxisRet[1] = m[0][2]-m[2][0];
			vAxisRet[2] = m[1][0]-m[0][1];
			vAxisRet.normalize();
		}
		else
		{
			// angle is PI
			float fHalfInverse;
			if ( m[0][0] >= m[1][1] )
			{
				// r00 >= r11
				if ( m[0][0] >= m[2][2] )
				{
					// r00 is maximum diagonal term
					vAxisRet[0] = 0.5f*sqrtf(m[0][0] -
						m[1][1] - m[2][2] + 1.0f);
					fHalfInverse = 0.5f/vAxisRet[0];
					vAxisRet[1] = fHalfInverse*m[0][1];
					vAxisRet[2] = fHalfInverse*m[0][2];
				}
				else
				{
					// r22 is maximum diagonal term
					vAxisRet[2] = 0.5f*sqrtf(m[2][2] -
						m[0][0] - m[1][1] + 1.0f);
					fHalfInverse = 0.5f/vAxisRet[2];
					vAxisRet[0] = fHalfInverse*m[0][2];
					vAxisRet[1] = fHalfInverse*m[1][2];
				}
			}
			else
			{
				// r11 > r00
				if ( m[1][1] >= m[2][2] )
				{
					// r11 is maximum diagonal term
					vAxisRet[1] = 0.5f*sqrtf(m[1][1] -
						m[0][0] - m[2][2] + 1.0f);
					fHalfInverse  = 0.5f/vAxisRet[1];
					vAxisRet[0] = fHalfInverse*m[0][1];
					vAxisRet[2] = fHalfInverse*m[1][2];
				}
				else
				{
					// r22 is maximum diagonal term
					vAxisRet[2] = 0.5f*sqrtf(m[2][2] -
						m[0][0] - m[1][1] + 1.0f);
					fHalfInverse = 0.5f/vAxisRet[2];
					vAxisRet[0] = fHalfInverse*m[0][2];
					vAxisRet[1] = fHalfInverse*m[1][2];
				}
			}
		}
	}
	else
	{
		// The angle is 0 and the matrix is the identity.  Any axis will
		// work, so just use the x-axis.
		vAxisRet[0] = 1.0f;
		vAxisRet[1] = 0.0f;
		vAxisRet[2] = 0.0f;
	}
}

inline void v3dxMatrix3::fromAxisAngle(const v3dxVector3& vAxis, float fRadians)
{
	float fCos = cosf(fRadians);
	float fSin = sinf(fRadians);
	float fOneMinusCos = 1.0f-fCos;
	float fX2 = vAxis[0]*vAxis[0];
	float fY2 = vAxis[1]*vAxis[1];
	float fZ2 = vAxis[2]*vAxis[2];
	float fXYM = vAxis[0]*vAxis[1]*fOneMinusCos;
	float fXZM = vAxis[0]*vAxis[2]*fOneMinusCos;
	float fYZM = vAxis[1]*vAxis[2]*fOneMinusCos;
	float fXSin = vAxis[0]*fSin;
	float fYSin = vAxis[1]*fSin;
	float fZSin = vAxis[2]*fSin;

	m[0][0] = fX2*fOneMinusCos+fCos;
	m[0][1] = fXYM-fZSin;
	m[0][2] = fXZM+fYSin;
	m[1][0] = fXYM+fZSin;
	m[1][1] = fY2*fOneMinusCos+fCos;
	m[1][2] = fYZM-fXSin;
	m[2][0] = fXZM-fYSin;
	m[2][1] = fYZM+fXSin;
	m[2][2] = fZ2*fOneMinusCos+fCos;
}

//void EulerToMatrix(const Vector3f& v, Matrix3x3f& matrix)
//{
//	float cx = cos(v.x);
//	float sx = sin(v.x);
//	float cy = cos(v.y);
//	float sy = sin(v.y);
//	float cz = cos(v.z);
//	float sz = sin(v.z);
//
//	matrix.Get(0, 0) = cy*cz + sx*sy*sz;
//	matrix.Get(0, 1) = cz*sx*sy - cy*sz;
//	matrix.Get(0, 2) = cx*sy;
//
//	matrix.Get(1, 0) = cx*sz;
//	matrix.Get(1, 1) = cx*cz;
//	matrix.Get(1, 2) = -sx;
//
//	matrix.Get(2, 0) = -cz*sy + cy*sx*sz;
//	matrix.Get(2, 1) = cy*cz*sx + sy*sz;
//	matrix.Get(2, 2) = cx*cy;
//}
//
//bool MatrixToEuler(const Matrix3x3f& matrix, Vector3f& v)
//{
//	// from http://www.geometrictools.com/Documentation/EulerAngles.pdf
//	// YXZ order
//	if (matrix.Get(1, 2) < 0.999F) // some fudge for imprecision
//	{
//		if (matrix.Get(1, 2) > -0.999F) // some fudge for imprecision
//		{
//			v.x = asin(-matrix.Get(1, 2));
//			v.y = atan2(matrix.Get(0, 2), matrix.Get(2, 2));
//			v.z = atan2(matrix.Get(1, 0), matrix.Get(1, 1));
//			SanitizeEuler(v);
//			return true;
//		}
//		else
//		{
//			// WARNING.  Not unique.  YA - ZA = atan2(r01,r00)
//			v.x = kPI * 0.5F;
//			v.y = atan2(matrix.Get(0, 1), matrix.Get(0, 0));
//			v.z = 0.0F;
//			SanitizeEuler(v);
//
//			return false;
//		}
//	}
//	else
//	{
//		// WARNING.  Not unique.  YA + ZA = atan2(-r01,r00)
//		v.x = -kPI * 0.5F;
//		v.y = atan2(-matrix.Get(0, 1), matrix.Get(0, 0));
//		v.z = 0.0F;
//		SanitizeEuler(v);
//		return false;
//	}
//}

inline bool v3dxMatrix3::toEulerAnglesXYZ (float& fYAngle, float& fPAngle, float& fRAngle) const
{
	// rot =  cy*cz          -cy*sz           sy
	//        cz*sx*sy+cx*sz  cx*cz-sx*sy*sz -cy*sx
	//       -cx*cz*sy+sx*sz  cz*sx+cx*sy*sz  cx*cy

	fPAngle = asinf(m[0][2]);

	if (fPAngle < V_HALFPI + SMALL_EPSILON)
	{
		if (fPAngle > -V_HALFPI - SMALL_EPSILON)
		{
			fYAngle = atan2f(-m[1][2], m[2][2]);
			fRAngle = atan2f(-m[0][1], m[0][0]);
			return true;
		}
		else
		{
			return false;
		}
	}
	else
	{
		return false;
	}
}

inline void v3dxMatrix3::fromEulerAnglesXYZ(float fYAngle, float fPAngle, float fRAngle)
{
	float fYawSin = sinf(fYAngle);
	float fYawCos  = cosf(fYAngle);
	float fPitchSin = sinf(fPAngle);
	float fPitchCos = cosf(fPAngle);
	float fRollSin = sinf(fRAngle);
	float fRollCos = cosf(fRAngle);

	v3dxMatrix3 kXMat(1.0,0.0,0.0,0.0,fPitchCos,-fPitchSin,0.0,fPitchSin,fPitchCos);
	v3dxMatrix3 kYMat(fYawCos,0.0,fYawSin,0.0,1.0,0.0,-fYawSin,0.0,fYawCos);
	v3dxMatrix3 kZMat(fRollCos,-fRollSin,0.0,fRollSin,fRollCos,0.0,0.0,0.0,1.0);

	*this = kXMat*(kYMat*kZMat);

}
