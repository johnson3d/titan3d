#include "v3dxOBB.h"

#define new VNEW

//////////////////////////////////////////////////////////////////////////

inline float MatrixHelper_Get(const v3dxMatrix4& matrix, int row, int col) 
{
	switch(row << 4 | col) 
	{
	case 0x00: { return matrix.m11; }
	case 0x01: { return matrix.m12; }
	case 0x02: { return matrix.m13; }
	case 0x03: { return matrix.m14; }

	case 0x10: { return matrix.m21; }
	case 0x11: { return matrix.m22; }
	case 0x12: { return matrix.m23; }
	case 0x13: { return matrix.m24; }

	case 0x20: { return matrix.m31; }
	case 0x21: { return matrix.m32; }
	case 0x22: { return matrix.m33; }
	case 0x23: { return matrix.m34; }

	case 0x30: { return matrix.m41; }
	case 0x31: { return matrix.m42; }
	case 0x32: { return matrix.m43; }
	case 0x33: { return matrix.m44; }

	default: 
		{
			return 0.f;
		}
	}
}

/// <summary>Checks an AABB for intersection with an OBB</summary>
/// <param name="aabbExtents">Extents of the AABB</param>
/// <param name="obbTransform">Orientation and position of the OBB</param>
/// <param name="obbExtents">Extents of the OBB</param>
/// <returns>True if the two boxes are overlapping</returns>
/// <remarks>
///   <para>
///     This method is a helper method for the other intersection checks. It assumes the
///     AABB is sitting right in the center of the coordinate frame. In other words,
///     that the OBB has been transformed into the AABB's local coordinate frame.
///   </para>
///   <para>
///     Idea taken from the "Simple Intersection Tests for Games" article
///     on gamasutra by Gomez. The algorithm uses the separating axis test for
///     all 15 potential separating axes. If a separating axis can not be found,
///     the two boxes are overlapping.
///     (http://www.gamasutra.com/features/19991018/Gomez_1.htm)
///   </para>
/// </remarks>
bool CheckContact( const v3dxVector3& aabbExtents, const v3dxMatrix4& obbTransform, const v3dxVector3& obbExtents ) 
{
	double ra, rb, t;

	// A's basis vectors
	for(int i = 0; i < 3; ++i) 
	{
		ra = aabbExtents[i];
		rb = obbExtents.x * fabs(MatrixHelper_Get( obbTransform, i, 0)) +
			obbExtents.y * fabs(MatrixHelper_Get( obbTransform, i, 1)) +
			obbExtents.z * fabs(MatrixHelper_Get( obbTransform, i, 2));

		t = fabs(MatrixHelper_Get(obbTransform, 3, i));

		if(t > ra + rb)
			return false;
	}

	// B's basis vectors
	for(int k = 0; k < 3; ++k) 
	{
		ra = aabbExtents.x * fabs(MatrixHelper_Get( obbTransform, 0, k)) +
			aabbExtents.y * fabs(MatrixHelper_Get( obbTransform, 1, k)) +
			aabbExtents.z * fabs(MatrixHelper_Get( obbTransform, 2, k));

		rb = obbExtents[k];

		t = fabs( 
			obbTransform.m41 * MatrixHelper_Get( obbTransform, 0, k) +
			obbTransform.m42 * MatrixHelper_Get( obbTransform, 1, k) +
			obbTransform.m43 * MatrixHelper_Get( obbTransform, 2, k)
			);

		if(t > ra + rb)
			return false;
	}

	// L = A0 x B0
	ra =
		aabbExtents.y * fabs(obbTransform.m31) +
		aabbExtents.z * fabs(obbTransform.m21);
	rb =
		obbExtents.y * fabs(obbTransform.m13) +
		obbExtents.z * fabs(obbTransform.m12);
	t = fabs(
		obbTransform.m43 * obbTransform.m21 - obbTransform.m42 * obbTransform.m31
		);
	if(t > ra + rb)
		return false;

	// L = A0 x B1
	ra =
		aabbExtents.y * fabs(obbTransform.m32) +
		aabbExtents.z * fabs(obbTransform.m22);
	rb =
		obbExtents.x * fabs(obbTransform.m13) +
		obbExtents.z * fabs(obbTransform.m11);
	t = fabs(
		obbTransform.m43 * obbTransform.m22 - obbTransform.m42 * obbTransform.m32
		);
	if(t > ra + rb)
		return false;

	// L = A0 x B2
	ra =
		aabbExtents.y * fabs(obbTransform.m33) +
		aabbExtents.z * fabs(obbTransform.m23);
	rb =
		obbExtents.x * fabs(obbTransform.m12) +
		obbExtents.y * fabs(obbTransform.m11);
	t = fabs(
		obbTransform.m43 * obbTransform.m23 - obbTransform.m42 * obbTransform.m33
		);
	if(t > ra + rb)
		return false;

	// L = A1 x B0
	ra =
		aabbExtents.x * fabs(obbTransform.m31) +
		aabbExtents.z * fabs(obbTransform.m11);
	rb =
		obbExtents.y * fabs(obbTransform.m23) +
		obbExtents.z * fabs(obbTransform.m22);
	t = fabs(
		obbTransform.m41 * obbTransform.m31 - obbTransform.m43 * obbTransform.m11
		);
	if(t > ra + rb)
		return false;

	// L = A1 x B1
	ra =
		aabbExtents.x * fabs(obbTransform.m32) +
		aabbExtents.z * fabs(obbTransform.m12);
	rb =
		obbExtents.x * fabs(obbTransform.m23) +
		obbExtents.z * fabs(obbTransform.m21);
	t = fabs(
		obbTransform.m41 * obbTransform.m32 - obbTransform.m43 * obbTransform.m12
		);
	if(t > ra + rb)
		return false;

	// L = A1 x B2
	ra =
		aabbExtents.x * fabs(obbTransform.m33) +
		aabbExtents.z * fabs(obbTransform.m13);
	rb =
		obbExtents.x * fabs(obbTransform.m22) +
		obbExtents.y * fabs(obbTransform.m21);
	t = fabs(
		obbTransform.m41 * obbTransform.m33 - obbTransform.m43 * obbTransform.m13
		);
	if(t > ra + rb)
		return false;

	// L = A2 x B0
	ra =
		aabbExtents.x * fabs(obbTransform.m21) +
		aabbExtents.y * fabs(obbTransform.m11);
	rb =
		obbExtents.y * fabs(obbTransform.m33) +
		obbExtents.z * fabs(obbTransform.m32);
	t = fabs(
		obbTransform.m42 * obbTransform.m11 - obbTransform.m41 * obbTransform.m21
		);
	if(t > ra + rb)
		return false;

	// L = A2 x B1
	ra =
		aabbExtents.x * fabs(obbTransform.m22) +
		aabbExtents.y * fabs(obbTransform.m12);
	rb =
		obbExtents.x * fabs(obbTransform.m33) +
		obbExtents.z * fabs(obbTransform.m31);
	t = fabs(
		obbTransform.m42 * obbTransform.m12 - obbTransform.m41 * obbTransform.m22
		);
	if(t > ra + rb)
		return false;

	// L = A2 x B2
	ra =
		aabbExtents.x * fabs(obbTransform.m23) +
		aabbExtents.y * fabs(obbTransform.m13);
	rb =
		obbExtents.x * fabs(obbTransform.m32) +
		obbExtents.y * fabs(obbTransform.m31);
	t = fabs(
		obbTransform.m42 * obbTransform.m13 - obbTransform.m41 * obbTransform.m23
		);
	if(t > ra + rb)
		return false;

	// No separating axis found, the two boxes overlap
	return true;
}

//////////////////////////////////////////////////////////////////////////

v3dxMatrix4 v3dTransUtility::GetRelativeTM( const v3dxMatrix4& ToTM , const v3dxMatrix4& FromTM )
{
	return FromTM*ToTM.inverse();
}

v3dxMatrix4 v3dTransUtility::GetAbsTM( const v3dxMatrix4& BaseTM , const v3dxMatrix4& RelativeTM )
{
	return RelativeTM*BaseTM;
}

v3dxVector3 v3dTransUtility::GetRelativePos( const v3dxMatrix4& BaseTM , const v3dxVector3& AbsPos )
{
	v3dxVector3 vRet;
	auto itm = BaseTM.inverse();
	v3dxVec3TransformCoord( &vRet ,&AbsPos , &itm);
	return vRet;
}

v3dxVector3 v3dTransUtility::GetRelativeNormal( const v3dxMatrix4& BaseTM , const v3dxVector3& AbsNormal )
{
	v3dxVector3 vRet;
	auto itm = BaseTM.inverse();
	v3dxVec3TransformNormal( &vRet ,&AbsNormal , &itm);
	return vRet;
}

//////////////////////////////////////////////////////////////////////////

typedef float REAL;

template <class Type> class Eigen
{
public:
	void DecrSortEigenStuff(void)
	{
		Tridiagonal(); //diagonalize the matrix.
		QLAlgorithm(); //
		DecreasingSort();
		GuaranteeRotation();
	}

	void Tridiagonal(void)
	{
		Type fM00 = mElement[0][0];
		Type fM01 = mElement[0][1];
		Type fM02 = mElement[0][2];
		Type fM11 = mElement[1][1];
		Type fM12 = mElement[1][2];
		Type fM22 = mElement[2][2];

		m_afDiag[0] = fM00;
		m_afSubd[2] = 0;
		if (fM02 != (Type)0.0)
		{
			Type fLength = sqrt(fM01*fM01+fM02*fM02);
			Type fInvLength = ((Type)1.0)/fLength;
			fM01 *= fInvLength;
			fM02 *= fInvLength;
			Type fQ = ((Type)2.0)*fM01*fM12+fM02*(fM22-fM11);
			m_afDiag[1] = fM11+fM02*fQ;
			m_afDiag[2] = fM22-fM02*fQ;
			m_afSubd[0] = fLength;
			m_afSubd[1] = fM12-fM01*fQ;
			mElement[0][0] = (Type)1.0;
			mElement[0][1] = (Type)0.0;
			mElement[0][2] = (Type)0.0;
			mElement[1][0] = (Type)0.0;
			mElement[1][1] = fM01;
			mElement[1][2] = fM02;
			mElement[2][0] = (Type)0.0;
			mElement[2][1] = fM02;
			mElement[2][2] = -fM01;
			m_bIsRotation = false;
		}
		else
		{
			m_afDiag[1] = fM11;
			m_afDiag[2] = fM22;
			m_afSubd[0] = fM01;
			m_afSubd[1] = fM12;
			mElement[0][0] = (Type)1.0;
			mElement[0][1] = (Type)0.0;
			mElement[0][2] = (Type)0.0;
			mElement[1][0] = (Type)0.0;
			mElement[1][1] = (Type)1.0;
			mElement[1][2] = (Type)0.0;
			mElement[2][0] = (Type)0.0;
			mElement[2][1] = (Type)0.0;
			mElement[2][2] = (Type)1.0;
			m_bIsRotation = true;
		}
	}

	bool QLAlgorithm(void)
	{
		const int iMaxIter = 32;

		for (int i0 = 0; i0 <3; i0++)
		{
			int i1;
			for (i1 = 0; i1 < iMaxIter; i1++)
			{
				int i2;
				for (i2 = i0; i2 <= (3-2); i2++)
				{
					Type fTmp = fabs(m_afDiag[i2]) + fabs(m_afDiag[i2+1]);
					if ( fabs(m_afSubd[i2]) + fTmp == fTmp )
						break;
				}
				if (i2 == i0)
				{
					break;
				}

				Type fG = (m_afDiag[i0+1] - m_afDiag[i0])/(((Type)2.0) * m_afSubd[i0]);
				Type fR = sqrt(fG*fG+(Type)1.0);
				if (fG < (Type)0.0)
				{
					fG = m_afDiag[i2]-m_afDiag[i0]+m_afSubd[i0]/(fG-fR);
				}
				else
				{
					fG = m_afDiag[i2]-m_afDiag[i0]+m_afSubd[i0]/(fG+fR);
				}
				Type fSin = (Type)1.0, fCos = (Type)1.0, fP = (Type)0.0;
				for (int i3 = i2-1; i3 >= i0; i3--)
				{
					Type fF = fSin*m_afSubd[i3];
					Type fB = fCos*m_afSubd[i3];
					if (fabs(fF) >= fabs(fG))
					{
						fCos = fG/fF;
						fR = sqrt(fCos*fCos+(Type)1.0);
						m_afSubd[i3+1] = fF*fR;
						fSin = ((Type)1.0)/fR;
						fCos *= fSin;
					}
					else
					{
						fSin = fF/fG;
						fR = sqrt(fSin*fSin+(Type)1.0);
						m_afSubd[i3+1] = fG*fR;
						fCos = ((Type)1.0)/fR;
						fSin *= fCos;
					}
					fG = m_afDiag[i3+1]-fP;
					fR = (m_afDiag[i3]-fG)*fSin+((Type)2.0)*fB*fCos;
					fP = fSin*fR;
					m_afDiag[i3+1] = fG+fP;
					fG = fCos*fR-fB;
					for (int i4 = 0; i4 < 3; i4++)
					{
						fF = mElement[i4][i3+1];
						mElement[i4][i3+1] = fSin*mElement[i4][i3]+fCos*fF;
						mElement[i4][i3] = fCos*mElement[i4][i3]-fSin*fF;
					}
				}
				m_afDiag[i0] -= fP;
				m_afSubd[i0] = fG;
				m_afSubd[i2] = (Type)0.0;
			}
			if (i1 == iMaxIter)
			{
				return false;
			}
		}
		return true;
	}

	void DecreasingSort(void)
	{
		//sort eigenvalues in decreasing order, e[0] >= ... >= e[iSize-1]
		for (int i0 = 0, i1; i0 <= 3-2; i0++)
		{
			// locate maximum eigenvalue
			i1 = i0;
			Type fMax = m_afDiag[i1];
			int i2;
			for (i2 = i0+1; i2 < 3; i2++)
			{
				if (m_afDiag[i2] > fMax)
				{
					i1 = i2;
					fMax = m_afDiag[i1];
				}
			}

			if (i1 != i0)
			{
				// swap eigenvalues
				m_afDiag[i1] = m_afDiag[i0];
				m_afDiag[i0] = fMax;
				// swap eigenvectors
				for (i2 = 0; i2 < 3; i2++)
				{
					Type fTmp = mElement[i2][i0];
					mElement[i2][i0] = mElement[i2][i1];
					mElement[i2][i1] = fTmp;
					m_bIsRotation = !m_bIsRotation;
				}
			}
		}
	}


	void GuaranteeRotation(void)
	{
		if (!m_bIsRotation)
		{
			// change sign on the first column
			for (int iRow = 0; iRow <3; iRow++)
			{
				mElement[iRow][0] = -mElement[iRow][0];
			}
		}
	}

	Type mElement[3][3];
	Type m_afDiag[3];
	Type m_afSubd[3];
	bool m_bIsRotation;
};

bool  v3dxOBB::ComputeBestObbMatrix(size_t vcount,     // number of input data points
								const float *points,     // starting address of points array.
								size_t vstride,    // stride between input points.
								const float *weights,    // *optional point weighting values.
								size_t wstride,    // weight stride for each vertex.
								float *matrix )
{
	bool ret = false;

	REAL kOrigin[3] = { 0, 0, 0 };

	REAL wtotal = 0;

	{
		const char *source  = (const char *) points;
		const char *wsource = (const char *) weights;

		for (size_t i=0; i<vcount; i++)
		{

			const REAL *p = (const REAL *) source;

			REAL w = 1;

			if ( wsource )
			{
				const REAL *ws = (const REAL *) wsource;
				w = *ws; //
				wsource+=wstride;
			}

			kOrigin[0]+=p[0]*w;
			kOrigin[1]+=p[1]*w;
			kOrigin[2]+=p[2]*w;

			wtotal+=w;

			source+=vstride;
		}
	}

	REAL recip = 1.0f / wtotal; // reciprocol of total weighting

	kOrigin[0]*=recip;
	kOrigin[1]*=recip;
	kOrigin[2]*=recip;


	REAL fSumXX=0;
	REAL fSumXY=0;
	REAL fSumXZ=0;

	REAL fSumYY=0;
	REAL fSumYZ=0;
	REAL fSumZZ=0;


	{
		const char *source  = (const char *) points;
		const char *wsource = (const char *) weights;

		for (size_t i=0; i<vcount; i++)
		{

			const REAL *p = (const REAL *) source;

			REAL w = 1;

			if ( wsource )
			{
				const REAL *ws = (const REAL *) wsource;
				w = *ws; //
				wsource+=wstride;
			}

			REAL kDiff[3];

			kDiff[0] = w*(p[0] - kOrigin[0]); // apply vertex weighting!
			kDiff[1] = w*(p[1] - kOrigin[1]);
			kDiff[2] = w*(p[2] - kOrigin[2]);

			fSumXX+= kDiff[0] * kDiff[0]; // sume of the squares of the differences.
			fSumXY+= kDiff[0] * kDiff[1]; // sume of the squares of the differences.
			fSumXZ+= kDiff[0] * kDiff[2]; // sume of the squares of the differences.

			fSumYY+= kDiff[1] * kDiff[1];
			fSumYZ+= kDiff[1] * kDiff[2];
			fSumZZ+= kDiff[2] * kDiff[2];


			source+=vstride;
		}
	}

	fSumXX *= recip;
	fSumXY *= recip;
	fSumXZ *= recip;
	fSumYY *= recip;
	fSumYZ *= recip;
	fSumZZ *= recip;

	// setup the eigensolver
	Eigen<REAL> kES;

	kES.mElement[0][0] = fSumXX;
	kES.mElement[0][1] = fSumXY;
	kES.mElement[0][2] = fSumXZ;

	kES.mElement[1][0] = fSumXY;
	kES.mElement[1][1] = fSumYY;
	kES.mElement[1][2] = fSumYZ;

	kES.mElement[2][0] = fSumXZ;
	kES.mElement[2][1] = fSumYZ;
	kES.mElement[2][2] = fSumZZ;

	// compute eigenstuff, smallest eigenvalue is in last position
	kES.DecrSortEigenStuff();

	memset( matrix,0,sizeof(REAL)*16 );
	matrix[0+0] = kES.mElement[0][0];
	matrix[1*4+0] = kES.mElement[0][1];
	matrix[2*4+0] = kES.mElement[0][2];

	matrix[0+1] = kES.mElement[1][0];
	matrix[1*4+1] = kES.mElement[1][1];
	matrix[2*4+1] = kES.mElement[1][2];

	matrix[0+2] = kES.mElement[2][0];
	matrix[1*4+2] = kES.mElement[2][1];
	matrix[2*4+2] = kES.mElement[2][2];

	matrix[15] = 1.f;

	ret = true;

	return ret;
}

bool v3dxOBB::IsOverlap( const v3dxOBB& obb , const v3dxMatrix4& tar_tm ) const
{
	return CheckContact( m_vExtent , tar_tm , obb.m_vExtent );
}

bool v3dxOBB::IsIntersect( float *pfT_n,
						  v3dxVector3 *pvPoint_n,
						  float *pfT_f,
						  v3dxVector3 *pvPoint_f,
						  const v3dxVector3 *pvFrom,
						  const v3dxVector3 *pvDir )
{
	v3dxBox3 box3;
	box3.minbox = -m_vExtent;
	box3.maxbox = m_vExtent;

	return v3dxLineIntersectBox3( pfT_n,
		pvPoint_n,
		pfT_f,
		pvPoint_f,
		pvFrom,
		pvDir,
		&box3
		) ? true : false;
}

vBOOL v3dxOBB::IsFastOutRef( const v3dxOBB& obb , const v3dxMatrix4& tar_tm )
{
	v3dxVector3 boxCorner[BOX3_CORNER_NUMBER];
	for( int i=0 ; i<BOX3_CORNER_NUMBER ; i++ )
	{
		boxCorner[i] = obb.GetCorner(i);
		v3dxVec3TransformCoord( &boxCorner[i] , &boxCorner[i] , &tar_tm );
	}

	//GetXPlane1
	vBOOL bOut = TRUE;
	for( int i=0 ; i<BOX3_CORNER_NUMBER ; i++ )
	{
		if( boxCorner[i].x<-m_vExtent.x )
		{
			continue;
		}
		else
		{
			bOut = FALSE;
			break;
		}
	}
	if( bOut!=FALSE )
		return TRUE;

	//GetXPlane2
	bOut = TRUE;
	for( int i=0 ; i<BOX3_CORNER_NUMBER ; i++ )
	{
		if( boxCorner[i].x>m_vExtent.x )
		{
			continue;
		}
		else
		{
			bOut = FALSE;
			break;
		}
	}
	if( bOut!=FALSE )
		return TRUE;


	//GetYPlane1
	bOut = TRUE;
	for( int i=0 ; i<BOX3_CORNER_NUMBER ; i++ )
	{
		if( boxCorner[i].y<-m_vExtent.y )
		{
			continue;
		}
		else
		{
			bOut = FALSE;
			break;
		}
	}
	if( bOut!=FALSE )
		return TRUE;

	//GetXPlane2
	bOut = TRUE;
	for( int i=0 ; i<BOX3_CORNER_NUMBER ; i++ )
	{
		if( boxCorner[i].y>m_vExtent.y )
		{
			continue;
		}
		else
		{
			bOut = FALSE;
			break;
		}
	}
	if( bOut!=FALSE )
		return TRUE;

	//GetZPlane1
	bOut = TRUE;
	for( int i=0 ; i<BOX3_CORNER_NUMBER ; i++ )
	{
		if( boxCorner[i].z<-m_vExtent.z )
		{
			continue;
		}
		else
		{
			bOut = FALSE;
			break;
		}
	}
	if( bOut!=FALSE )
		return TRUE;

	//GetZPlane2
	bOut = TRUE;
	for( int i=0 ; i<BOX3_CORNER_NUMBER ; i++ )
	{
		if( boxCorner[i].z>m_vExtent.z )
		{
			continue;
		}
		else
		{
			bOut = FALSE;
			break;
		}
	}
	return bOut;
}

extern "C"
{
	 v3dxOBB* v3dxOBB_New(float extX, float extY, float extZ)
	{
		auto retValue = new v3dxOBB();
		retValue->m_vExtent = v3dxVector3(extX, extY, extZ);
		return retValue;
	}
	 void v3dxOBB_Delete(v3dxOBB* obb)
	{
		 EngineNS::Safe_Delete(obb);
	}
	 vBOOL v3dxOBB_IsOverLap(v3dxOBB* obb, v3dxOBB* obb2, v3dxMatrix4* obb2Mat)
	{
		if (obb == NULL)
			return FALSE;
		return obb->IsOverlap(*obb2, *obb2Mat) ? TRUE : FALSE;
	}
}