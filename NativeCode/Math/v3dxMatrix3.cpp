#include "v3dxMatrix3.h"
#include "v3dxQuaternion.h"

#if defined(PLATFORM_WIN)
#pragma warning(disable:4244)
#pragma warning(disable:4305)
#endif

#define new VNEW

/////////////////////////////////////////////////////////////////////
// static variable initialize
const v3dxMatrix3 v3dxMatrix3::ZERO(0.f,0.f,0.f,0.f,0.f,0.f,0.f,0.f,0.f);
const v3dxMatrix3 v3dxMatrix3::IDENTITY(1.f,0.f,0.f,0.f,1.f,0.f,0.f,0.f,1.f);
const float v3dxMatrix3::ms_fSvdEpsilon = 1e-04;
const int v3dxMatrix3::ms_iSvdMaxIterations = 32;


//v3dxMatrix3::v3dxMatrix3(const v3dxQuaternion &quat)
//{
//}

void v3dxMatrix3::operator= (const v3dxQuaternion& quat)
{
	quat.toRotationMatrix(*this);
}

//-----------------------------------------------------------------------
void v3dxMatrix3::singularValueDecomposition (v3dxMatrix3& kL, v3dxVector3& kS,
										  v3dxMatrix3& kR) const
{
	// temas: currently unused
	//const int iMax = 16;
	int iRow, iCol;

	v3dxMatrix3 kA = *this;
	Bidiagonalize(kA,kL,kR);

	for (int i = 0; i < ms_iSvdMaxIterations; i++)
	{
		float fTmp, fTmp0, fTmp1;
		float fSin0, fCos0, fTan0;
		float fSin1, fCos1, fTan1;

		bool bTest1 = (Math::Abs(kA[0][1]) <=
			ms_fSvdEpsilon*(Math::Abs(kA[0][0])+Math::Abs(kA[1][1])));
		bool bTest2 = (Math::Abs(kA[1][2]) <=
			ms_fSvdEpsilon*(Math::Abs(kA[1][1])+Math::Abs(kA[2][2])));
		if ( bTest1 )
		{
			if ( bTest2 )
			{
				kS[0] = kA[0][0];
				kS[1] = kA[1][1];
				kS[2] = kA[2][2];
				break;
			}
			else
			{
				// 2x2 closed form factorization
				fTmp = (kA[1][1]*kA[1][1] - kA[2][2]*kA[2][2] +
					kA[1][2]*kA[1][2])/(kA[1][2]*kA[2][2]);
				fTan0 = 0.5*(fTmp+Math::Sqrt(fTmp*fTmp + 4.0));
				fCos0 = 1.0/Math::Sqrt(1.0+fTan0*fTan0);
				fSin0 = fTan0*fCos0;

				for (iCol = 0; iCol < 3; iCol++)
				{
					fTmp0 = kL[iCol][1];
					fTmp1 = kL[iCol][2];
					kL[iCol][1] = fCos0*fTmp0-fSin0*fTmp1;
					kL[iCol][2] = fSin0*fTmp0+fCos0*fTmp1;
				}

				fTan1 = (kA[1][2]-kA[2][2]*fTan0)/kA[1][1];
				fCos1 = 1.0/Math::Sqrt(1.0+fTan1*fTan1);
				fSin1 = -fTan1*fCos1;

				for (iRow = 0; iRow < 3; iRow++)
				{
					fTmp0 = kR[1][iRow];
					fTmp1 = kR[2][iRow];
					kR[1][iRow] = fCos1*fTmp0-fSin1*fTmp1;
					kR[2][iRow] = fSin1*fTmp0+fCos1*fTmp1;
				}

				kS[0] = kA[0][0];
				kS[1] = fCos0*fCos1*kA[1][1] -
					fSin1*(fCos0*kA[1][2]-fSin0*kA[2][2]);
				kS[2] = fSin0*fSin1*kA[1][1] +
					fCos1*(fSin0*kA[1][2]+fCos0*kA[2][2]);
				break;
			}
		}
		else
		{
			if ( bTest2 )
			{
				// 2x2 closed form factorization
				fTmp = (kA[0][0]*kA[0][0] + kA[1][1]*kA[1][1] -
					kA[0][1]*kA[0][1])/(kA[0][1]*kA[1][1]);
				fTan0 = 0.5*(-fTmp+Math::Sqrt(fTmp*fTmp + 4.0));
				fCos0 = 1.0/Math::Sqrt(1.0+fTan0*fTan0);
				fSin0 = fTan0*fCos0;

				for (iCol = 0; iCol < 3; iCol++)
				{
					fTmp0 = kL[iCol][0];
					fTmp1 = kL[iCol][1];
					kL[iCol][0] = fCos0*fTmp0-fSin0*fTmp1;
					kL[iCol][1] = fSin0*fTmp0+fCos0*fTmp1;
				}

				fTan1 = (kA[0][1]-kA[1][1]*fTan0)/kA[0][0];
				fCos1 = 1.0/Math::Sqrt(1.0+fTan1*fTan1);
				fSin1 = -fTan1*fCos1;

				for (iRow = 0; iRow < 3; iRow++)
				{
					fTmp0 = kR[0][iRow];
					fTmp1 = kR[1][iRow];
					kR[0][iRow] = fCos1*fTmp0-fSin1*fTmp1;
					kR[1][iRow] = fSin1*fTmp0+fCos1*fTmp1;
				}

				kS[0] = fCos0*fCos1*kA[0][0] -
					fSin1*(fCos0*kA[0][1]-fSin0*kA[1][1]);
				kS[1] = fSin0*fSin1*kA[0][0] +
					fCos1*(fSin0*kA[0][1]+fCos0*kA[1][1]);
				kS[2] = kA[2][2];
				break;
			}
			else
			{
				GolubKahanStep(kA,kL,kR);
			}
		}
	}

	// positize diagonal
	for (iRow = 0; iRow < 3; iRow++)
	{
		if ( kS[iRow] < 0.0 )
		{
			kS[iRow] = -kS[iRow];
			for (iCol = 0; iCol < 3; iCol++)
				kR[iRow][iCol] = -kR[iRow][iCol];
		}
	}
}
//-----------------------------------------------------------------------
void v3dxMatrix3::singularValueComposition (const v3dxMatrix3& kL,
										const v3dxVector3& kS, const v3dxMatrix3& kR)
{
	int iRow, iCol;
	v3dxMatrix3 kTmp;

	// product S*R
	for (iRow = 0; iRow < 3; iRow++)
	{
		for (iCol = 0; iCol < 3; iCol++)
			kTmp[iRow][iCol] = kS[iRow]*kR[iRow][iCol];
	}

	// product L*S*R
	for (iRow = 0; iRow < 3; iRow++)
	{
		for (iCol = 0; iCol < 3; iCol++)
		{
			m[iRow][iCol] = 0.0;
			for (int iMid = 0; iMid < 3; iMid++)
				m[iRow][iCol] += kL[iRow][iMid]*kTmp[iMid][iCol];
		}
	}
}
//-----------------------------------------------------------------------
void v3dxMatrix3::orthonormalize ()
{
	// Algorithm uses Gram-Schmidt orthogonalization.  If 'this' matrix is
	// M = [m0|m1|m2], then orthonormal output matrix is Q = [q0|q1|q2],
	//
	//   q0 = m0/|m0|
	//   q1 = (m1-(q0*m1)q0)/|m1-(q0*m1)q0|
	//   q2 = (m2-(q0*m2)q0-(q1*m2)q1)/|m2-(q0*m2)q0-(q1*m2)q1|
	//
	// where |V| indicates length of vector V and A*B indicates dot
	// product of vectors A and B.

	// compute q0
	float fInvLength = 1.0/Math::Sqrt(m[0][0]*m[0][0]
	+ m[1][0]*m[1][0] +
		m[2][0]*m[2][0]);

	m[0][0] *= fInvLength;
	m[1][0] *= fInvLength;
	m[2][0] *= fInvLength;

	// compute q1
	float fDot0 =
		m[0][0]*m[0][1] +
		m[1][0]*m[1][1] +
		m[2][0]*m[2][1];

	m[0][1] -= fDot0*m[0][0];
	m[1][1] -= fDot0*m[1][0];
	m[2][1] -= fDot0*m[2][0];

	fInvLength = 1.0/Math::Sqrt(m[0][1]*m[0][1] +
		m[1][1]*m[1][1] +
		m[2][1]*m[2][1]);

	m[0][1] *= fInvLength;
	m[1][1] *= fInvLength;
	m[2][1] *= fInvLength;

	// compute q2
	float fDot1 =
		m[0][1]*m[0][2] +
		m[1][1]*m[1][2] +
		m[2][1]*m[2][2];

	fDot0 =
		m[0][0]*m[0][2] +
		m[1][0]*m[1][2] +
		m[2][0]*m[2][2];

	m[0][2] -= fDot0*m[0][0] + fDot1*m[0][1];
	m[1][2] -= fDot0*m[1][0] + fDot1*m[1][1];
	m[2][2] -= fDot0*m[2][0] + fDot1*m[2][1];

	fInvLength = 1.0/Math::Sqrt(m[0][2]*m[0][2] +
		m[1][2]*m[1][2] +
		m[2][2]*m[2][2]);

	m[0][2] *= fInvLength;
	m[1][2] *= fInvLength;
	m[2][2] *= fInvLength;
}
//-----------------------------------------------------------------------
void v3dxMatrix3::QDUDecomposition (v3dxMatrix3& kQ,
								v3dxVector3& kD, v3dxVector3& kU) const
{
	// Factor M = QR = QDU where Q is orthogonal, D is diagonal,
	// and U is upper triangular with ones on its diagonal.  Algorithm uses
	// Gram-Schmidt orthogonalization (the QR algorithm).
	//
	// If M = [ m0 | m1 | m2 ] and Q = [ q0 | q1 | q2 ], then
	//
	//   q0 = m0/|m0|
	//   q1 = (m1-(q0*m1)q0)/|m1-(q0*m1)q0|
	//   q2 = (m2-(q0*m2)q0-(q1*m2)q1)/|m2-(q0*m2)q0-(q1*m2)q1|
	//
	// where |V| indicates length of vector V and A*B indicates dot
	// product of vectors A and B.  The matrix R has entries
	//
	//   r00 = q0*m0  r01 = q0*m1  r02 = q0*m2
	//   r10 = 0      r11 = q1*m1  r12 = q1*m2
	//   r20 = 0      r21 = 0      r22 = q2*m2
	//
	// so D = diag(r00,r11,r22) and U has entries u01 = r01/r00,
	// u02 = r02/r00, and u12 = r12/r11.

	// Q = rotation
	// D = scaling
	// U = shear

	// D stores the three diagonal entries r00, r11, r22
	// U stores the entries U[0] = u01, U[1] = u02, U[2] = u12

	// build orthogonal matrix Q
	float fInvLength = 1.0/Math::Sqrt(m[0][0]*m[0][0]
	+ m[1][0]*m[1][0] +
		m[2][0]*m[2][0]);
	kQ[0][0] = m[0][0]*fInvLength;
	kQ[1][0] = m[1][0]*fInvLength;
	kQ[2][0] = m[2][0]*fInvLength;

	float fDot = kQ[0][0]*m[0][1] + kQ[1][0]*m[1][1] +
		kQ[2][0]*m[2][1];
	kQ[0][1] = m[0][1]-fDot*kQ[0][0];
	kQ[1][1] = m[1][1]-fDot*kQ[1][0];
	kQ[2][1] = m[2][1]-fDot*kQ[2][0];
	fInvLength = 1.0/Math::Sqrt(kQ[0][1]*kQ[0][1] + kQ[1][1]*kQ[1][1] +
		kQ[2][1]*kQ[2][1]);
	kQ[0][1] *= fInvLength;
	kQ[1][1] *= fInvLength;
	kQ[2][1] *= fInvLength;

	fDot = kQ[0][0]*m[0][2] + kQ[1][0]*m[1][2] +
		kQ[2][0]*m[2][2];
	kQ[0][2] = m[0][2]-fDot*kQ[0][0];
	kQ[1][2] = m[1][2]-fDot*kQ[1][0];
	kQ[2][2] = m[2][2]-fDot*kQ[2][0];
	fDot = kQ[0][1]*m[0][2] + kQ[1][1]*m[1][2] +
		kQ[2][1]*m[2][2];
	kQ[0][2] -= fDot*kQ[0][1];
	kQ[1][2] -= fDot*kQ[1][1];
	kQ[2][2] -= fDot*kQ[2][1];
	fInvLength = 1.0/Math::Sqrt(kQ[0][2]*kQ[0][2] + kQ[1][2]*kQ[1][2] +
		kQ[2][2]*kQ[2][2]);
	kQ[0][2] *= fInvLength;
	kQ[1][2] *= fInvLength;
	kQ[2][2] *= fInvLength;

	// guarantee that orthogonal matrix has determinant 1 (no reflections)
	float fDet = kQ[0][0]*kQ[1][1]*kQ[2][2] + kQ[0][1]*kQ[1][2]*kQ[2][0] +
		kQ[0][2]*kQ[1][0]*kQ[2][1] - kQ[0][2]*kQ[1][1]*kQ[2][0] -
		kQ[0][1]*kQ[1][0]*kQ[2][2] - kQ[0][0]*kQ[1][2]*kQ[2][1];

	if ( fDet < 0.0 )
	{
		for (int iRow = 0; iRow < 3; iRow++)
			for (int iCol = 0; iCol < 3; iCol++)
				kQ[iRow][iCol] = -kQ[iRow][iCol];
	}

	// build "right" matrix R
	v3dxMatrix3 kR;
	kR[0][0] = kQ[0][0]*m[0][0] + kQ[1][0]*m[1][0] +
		kQ[2][0]*m[2][0];
	kR[0][1] = kQ[0][0]*m[0][1] + kQ[1][0]*m[1][1] +
		kQ[2][0]*m[2][1];
	kR[1][1] = kQ[0][1]*m[0][1] + kQ[1][1]*m[1][1] +
		kQ[2][1]*m[2][1];
	kR[0][2] = kQ[0][0]*m[0][2] + kQ[1][0]*m[1][2] +
		kQ[2][0]*m[2][2];
	kR[1][2] = kQ[0][1]*m[0][2] + kQ[1][1]*m[1][2] +
		kQ[2][1]*m[2][2];
	kR[2][2] = kQ[0][2]*m[0][2] + kQ[1][2]*m[1][2] +
		kQ[2][2]*m[2][2];

	// the scaling component
	kD[0] = kR[0][0];
	kD[1] = kR[1][1];
	kD[2] = kR[2][2];

	// the shear component
	float fInvD0 = 1.0/kD[0];
	kU[0] = kR[0][1]*fInvD0;
	kU[1] = kR[0][2]*fInvD0;
	kU[2] = kR[1][2]/kD[1];
}
//-----------------------------------------------------------------------
void v3dxMatrix3::Bidiagonalize (v3dxMatrix3& kA, v3dxMatrix3& kL,
							 v3dxMatrix3& kR)
{
	float afV[3], afW[3];
	float fLength, fSign, fT1, fInvT1, fT2;
	bool bIdentity;

	// map first column to (*,0,0)
	fLength = Math::Sqrt(kA[0][0]*kA[0][0] + kA[1][0]*kA[1][0] +
		kA[2][0]*kA[2][0]);
	if ( fLength > 0.0 )
	{
		fSign = (kA[0][0] > 0.0 ? 1.0 : -1.0);
		fT1 = kA[0][0] + fSign*fLength;
		fInvT1 = 1.0/fT1;
		afV[1] = kA[1][0]*fInvT1;
		afV[2] = kA[2][0]*fInvT1;

		fT2 = -2.0/(1.0+afV[1]*afV[1]+afV[2]*afV[2]);
		afW[0] = fT2*(kA[0][0]+kA[1][0]*afV[1]+kA[2][0]*afV[2]);
		afW[1] = fT2*(kA[0][1]+kA[1][1]*afV[1]+kA[2][1]*afV[2]);
		afW[2] = fT2*(kA[0][2]+kA[1][2]*afV[1]+kA[2][2]*afV[2]);
		kA[0][0] += afW[0];
		kA[0][1] += afW[1];
		kA[0][2] += afW[2];
		kA[1][1] += afV[1]*afW[1];
		kA[1][2] += afV[1]*afW[2];
		kA[2][1] += afV[2]*afW[1];
		kA[2][2] += afV[2]*afW[2];

		kL[0][0] = 1.0+fT2;
		kL[0][1] = kL[1][0] = fT2*afV[1];
		kL[0][2] = kL[2][0] = fT2*afV[2];
		kL[1][1] = 1.0+fT2*afV[1]*afV[1];
		kL[1][2] = kL[2][1] = fT2*afV[1]*afV[2];
		kL[2][2] = 1.0+fT2*afV[2]*afV[2];
		bIdentity = false;
	}
	else
	{
		kL = v3dxMatrix3::IDENTITY;
		bIdentity = true;
	}

	// map first row to (*,*,0)
	fLength = Math::Sqrt(kA[0][1]*kA[0][1]+kA[0][2]*kA[0][2]);
	if ( fLength > 0.0 )
	{
		fSign = (kA[0][1] > 0.0 ? 1.0 : -1.0);
		fT1 = kA[0][1] + fSign*fLength;
		afV[2] = kA[0][2]/fT1;

		fT2 = -2.0/(1.0+afV[2]*afV[2]);
		afW[0] = fT2*(kA[0][1]+kA[0][2]*afV[2]);
		afW[1] = fT2*(kA[1][1]+kA[1][2]*afV[2]);
		afW[2] = fT2*(kA[2][1]+kA[2][2]*afV[2]);
		kA[0][1] += afW[0];
		kA[1][1] += afW[1];
		kA[1][2] += afW[1]*afV[2];
		kA[2][1] += afW[2];
		kA[2][2] += afW[2]*afV[2];

		kR[0][0] = 1.0;
		kR[0][1] = kR[1][0] = 0.0;
		kR[0][2] = kR[2][0] = 0.0;
		kR[1][1] = 1.0+fT2;
		kR[1][2] = kR[2][1] = fT2*afV[2];
		kR[2][2] = 1.0+fT2*afV[2]*afV[2];
	}
	else
	{
		kR = v3dxMatrix3::IDENTITY;
	}

	// map second column to (*,*,0)
	fLength = Math::Sqrt(kA[1][1]*kA[1][1]+kA[2][1]*kA[2][1]);
	if ( fLength > 0.0 )
	{
		fSign = (kA[1][1] > 0.0 ? 1.0 : -1.0);
		fT1 = kA[1][1] + fSign*fLength;
		afV[2] = kA[2][1]/fT1;

		fT2 = -2.0/(1.0+afV[2]*afV[2]);
		afW[1] = fT2*(kA[1][1]+kA[2][1]*afV[2]);
		afW[2] = fT2*(kA[1][2]+kA[2][2]*afV[2]);
		kA[1][1] += afW[1];
		kA[1][2] += afW[2];
		kA[2][2] += afV[2]*afW[2];

		float fA = 1.0+fT2;
		float fB = fT2*afV[2];
		float fC = 1.0+fB*afV[2];

		if ( bIdentity )
		{
			kL[0][0] = 1.0;
			kL[0][1] = kL[1][0] = 0.0;
			kL[0][2] = kL[2][0] = 0.0;
			kL[1][1] = fA;
			kL[1][2] = kL[2][1] = fB;
			kL[2][2] = fC;
		}
		else
		{
			for (int iRow = 0; iRow < 3; iRow++)
			{
				float fTmp0 = kL[iRow][1];
				float fTmp1 = kL[iRow][2];
				kL[iRow][1] = fA*fTmp0+fB*fTmp1;
				kL[iRow][2] = fB*fTmp0+fC*fTmp1;
			}
		}
	}
}
//-----------------------------------------------------------------------
void v3dxMatrix3::GolubKahanStep (v3dxMatrix3& kA, v3dxMatrix3& kL,
							  v3dxMatrix3& kR)
{
	float fT11 = kA[0][1]*kA[0][1]+kA[1][1]*kA[1][1];
	float fT22 = kA[1][2]*kA[1][2]+kA[2][2]*kA[2][2];
	float fT12 = kA[1][1]*kA[1][2];
	float fTrace = fT11+fT22;
	float fDiff = fT11-fT22;
	float fDiscr = Math::Sqrt(fDiff*fDiff+4.0*fT12*fT12);
	float fRoot1 = 0.5*(fTrace+fDiscr);
	float fRoot2 = 0.5*(fTrace-fDiscr);

	// adjust right
	float fY = kA[0][0] - (Math::Abs(fRoot1-fT22) <=
		Math::Abs(fRoot2-fT22) ? fRoot1 : fRoot2);
	float fZ = kA[0][1];
	float fInvLength = 1.0/Math::Sqrt(fY*fY+fZ*fZ);
	float fSin = fZ*fInvLength;
	float fCos = -fY*fInvLength;

	float fTmp0 = kA[0][0];
	float fTmp1 = kA[0][1];
	kA[0][0] = fCos*fTmp0-fSin*fTmp1;
	kA[0][1] = fSin*fTmp0+fCos*fTmp1;
	kA[1][0] = -fSin*kA[1][1];
	kA[1][1] *= fCos;

	int iRow;
	for (iRow = 0; iRow < 3; iRow++)
	{
		fTmp0 = kR[0][iRow];
		fTmp1 = kR[1][iRow];
		kR[0][iRow] = fCos*fTmp0-fSin*fTmp1;
		kR[1][iRow] = fSin*fTmp0+fCos*fTmp1;
	}

	// adjust left
	fY = kA[0][0];
	fZ = kA[1][0];
	fInvLength = 1.0/Math::Sqrt(fY*fY+fZ*fZ);
	fSin = fZ*fInvLength;
	fCos = -fY*fInvLength;

	kA[0][0] = fCos*kA[0][0]-fSin*kA[1][0];
	fTmp0 = kA[0][1];
	fTmp1 = kA[1][1];
	kA[0][1] = fCos*fTmp0-fSin*fTmp1;
	kA[1][1] = fSin*fTmp0+fCos*fTmp1;
	kA[0][2] = -fSin*kA[1][2];
	kA[1][2] *= fCos;

	int iCol;
	for (iCol = 0; iCol < 3; iCol++)
	{
		fTmp0 = kL[iCol][0];
		fTmp1 = kL[iCol][1];
		kL[iCol][0] = fCos*fTmp0-fSin*fTmp1;
		kL[iCol][1] = fSin*fTmp0+fCos*fTmp1;
	}

	// adjust right
	fY = kA[0][1];
	fZ = kA[0][2];
	fInvLength = 1.0/Math::Sqrt(fY*fY+fZ*fZ);
	fSin = fZ*fInvLength;
	fCos = -fY*fInvLength;

	kA[0][1] = fCos*kA[0][1]-fSin*kA[0][2];
	fTmp0 = kA[1][1];
	fTmp1 = kA[1][2];
	kA[1][1] = fCos*fTmp0-fSin*fTmp1;
	kA[1][2] = fSin*fTmp0+fCos*fTmp1;
	kA[2][1] = -fSin*kA[2][2];
	kA[2][2] *= fCos;

	for (iRow = 0; iRow < 3; iRow++)
	{
		fTmp0 = kR[1][iRow];
		fTmp1 = kR[2][iRow];
		kR[1][iRow] = fCos*fTmp0-fSin*fTmp1;
		kR[2][iRow] = fSin*fTmp0+fCos*fTmp1;
	}

	// adjust left
	fY = kA[1][1];
	fZ = kA[2][1];
	fInvLength = 1.0/Math::Sqrt(fY*fY+fZ*fZ);
	fSin = fZ*fInvLength;
	fCos = -fY*fInvLength;

	kA[1][1] = fCos*kA[1][1]-fSin*kA[2][1];
	fTmp0 = kA[1][2];
	fTmp1 = kA[2][2];
	kA[1][2] = fCos*fTmp0-fSin*fTmp1;
	kA[2][2] = fSin*fTmp0+fCos*fTmp1;

	for (iCol = 0; iCol < 3; iCol++)
	{
		fTmp0 = kL[iCol][1];
		fTmp1 = kL[iCol][2];
		kL[iCol][1] = fCos*fTmp0-fSin*fTmp1;
		kL[iCol][2] = fSin*fTmp0+fCos*fTmp1;
	}
}


//-----------------------------------------------------------------------
float v3dxMatrix3::MaxCubicRoot (float afCoeff[3])
{
	// Spectral norm is for A^T*A, so characteristic polynomial
	// P(x) = c[0]+c[1]*x+c[2]*x^2+x^3 has three positive real roots.
	// This yields the assertions c[0] < 0 and c[2]*c[2] >= 3*c[1].

	// quick out for uniform scale (triple root)
	const float fOneThird = 1.0/3.0;
	const float fEpsilon = 1e-06;
	float fDiscr = afCoeff[2]*afCoeff[2] - 3.0*afCoeff[1];
	if ( fDiscr <= fEpsilon )
		return -fOneThird*afCoeff[2];

	// Compute an upper bound on roots of P(x).  This assumes that A^T*A
	// has been scaled by its largest entry.
	float fX = 1.0;
	float fPoly = afCoeff[0]+fX*(afCoeff[1]+fX*(afCoeff[2]+fX));
	if ( fPoly < 0.0 )
	{
		// uses a matrix norm to find an upper bound on maximum root
		fX = Math::Abs(afCoeff[0]);
		float fTmp = 1.0+Math::Abs(afCoeff[1]);
		if ( fTmp > fX )
			fX = fTmp;
		fTmp = 1.0+Math::Abs(afCoeff[2]);
		if ( fTmp > fX )
			fX = fTmp;
	}

	// Newton's method to find root
	float fTwoC2 = 2.0*afCoeff[2];
	for (int i = 0; i < 16; i++)
	{
		fPoly = afCoeff[0]+fX*(afCoeff[1]+fX*(afCoeff[2]+fX));
		if ( Math::Abs(fPoly) <= fEpsilon )
			return fX;

		float fDeriv = afCoeff[1]+fX*(fTwoC2+3.0*fX);
		fX -= fPoly/fDeriv;
	}

	return fX;
}

//-----------------------------------------------------------------------
float v3dxMatrix3::spectralNorm () const
{
	v3dxMatrix3 kP;
	int iRow, iCol;
	float fPmax = 0.0;
	for (iRow = 0; iRow < 3; iRow++)
	{
		for (iCol = 0; iCol < 3; iCol++)
		{
			kP[iRow][iCol] = 0.0;
			for (int iMid = 0; iMid < 3; iMid++)
			{
				kP[iRow][iCol] +=
					m[iMid][iRow]*m[iMid][iCol];
			}
			if ( kP[iRow][iCol] > fPmax )
				fPmax = kP[iRow][iCol];
		}
	}

	float fInvPmax = 1.0/fPmax;
	for (iRow = 0; iRow < 3; iRow++)
	{
		for (iCol = 0; iCol < 3; iCol++)
			kP[iRow][iCol] *= fInvPmax;
	}

	float afCoeff[3];
	afCoeff[0] = -(kP[0][0]*(kP[1][1]*kP[2][2]-kP[1][2]*kP[2][1]) +
		kP[0][1]*(kP[2][0]*kP[1][2]-kP[1][0]*kP[2][2]) +
		kP[0][2]*(kP[1][0]*kP[2][1]-kP[2][0]*kP[1][1]));
	afCoeff[1] = kP[0][0]*kP[1][1]-kP[0][1]*kP[1][0] +
		kP[0][0]*kP[2][2]-kP[0][2]*kP[2][0] +
		kP[1][1]*kP[2][2]-kP[1][2]*kP[2][1];
	afCoeff[2] = -(kP[0][0]+kP[1][1]+kP[2][2]);

	float fRoot = MaxCubicRoot(afCoeff);
	float fNorm = Math::Sqrt(fPmax*fRoot);
	return fNorm;
}


//-----------------------------------------------------------------------
void v3dxMatrix3::Tridiagonal (float afDiag[3], float afSubDiag[3])
{
	// Householder reduction T = Q^t M Q
	//   Input:
	//     mat, symmetric 3x3 matrix M
	//   Output:
	//     mat, orthogonal matrix Q
	//     diag, diagonal entries of T
	//     subd, subdiagonal entries of T (T is symmetric)

	float fA = m[0][0];
	float fB = m[0][1];
	float fC = m[0][2];
	float fD = m[1][1];
	float fE = m[1][2];
	float fF = m[2][2];

	afDiag[0] = fA;
	afSubDiag[2] = 0.0;
	if ( Math::Abs(fC) >= EPSILON )
	{
		float fLength = Math::Sqrt(fB*fB+fC*fC);
		float fInvLength = 1.0/fLength;
		fB *= fInvLength;
		fC *= fInvLength;
		float fQ = 2.0*fB*fE+fC*(fF-fD);
		afDiag[1] = fD+fC*fQ;
		afDiag[2] = fF-fC*fQ;
		afSubDiag[0] = fLength;
		afSubDiag[1] = fE-fB*fQ;
		m[0][0] = 1.0;
		m[0][1] = 0.0;
		m[0][2] = 0.0;
		m[1][0] = 0.0;
		m[1][1] = fB;
		m[1][2] = fC;
		m[2][0] = 0.0;
		m[2][1] = fC;
		m[2][2] = -fB;
	}
	else
	{
		afDiag[1] = fD;
		afDiag[2] = fF;
		afSubDiag[0] = fB;
		afSubDiag[1] = fE;
		m[0][0] = 1.0;
		m[0][1] = 0.0;
		m[0][2] = 0.0;
		m[1][0] = 0.0;
		m[1][1] = 1.0;
		m[1][2] = 0.0;
		m[2][0] = 0.0;
		m[2][1] = 0.0;
		m[2][2] = 1.0;
	}
}
//-----------------------------------------------------------------------
bool v3dxMatrix3::QLAlgorithm (float afDiag[3], float afSubDiag[3])
{
	// QL iteration with implicit shifting to reduce matrix from tridiagonal
	// to diagonal

	for (int i0 = 0; i0 < 3; i0++)
	{
		const int iMaxIter = 32;
		int iIter;
		for (iIter = 0; iIter < iMaxIter; iIter++)
		{
			int i1;
			for (i1 = i0; i1 <= 1; i1++)
			{
				float fSum = Math::Abs(afDiag[i1]) +
					Math::Abs(afDiag[i1+1]);
				if ( Math::Abs(afSubDiag[i1]) + fSum == fSum )
					break;
			}
			if ( i1 == i0 )
				break;

			float fTmp0 = (afDiag[i0+1]-afDiag[i0])/(2.0*afSubDiag[i0]);
			float fTmp1 = Math::Sqrt(fTmp0*fTmp0+1.0);
			if ( fTmp0 < 0.0 )
				fTmp0 = afDiag[i1]-afDiag[i0]+afSubDiag[i0]/(fTmp0-fTmp1);
			else
				fTmp0 = afDiag[i1]-afDiag[i0]+afSubDiag[i0]/(fTmp0+fTmp1);
			float fSin = 1.0;
			float fCos = 1.0;
			float fTmp2 = 0.0;
			for (int i2 = i1-1; i2 >= i0; i2--)
			{
				float fTmp3 = fSin*afSubDiag[i2];
				float fTmp4 = fCos*afSubDiag[i2];
				if ( Math::Abs(fTmp3) >= Math::Abs(fTmp0) )
				{
					fCos = fTmp0/fTmp3;
					fTmp1 = Math::Sqrt(fCos*fCos+1.0);
					afSubDiag[i2+1] = fTmp3*fTmp1;
					fSin = 1.0/fTmp1;
					fCos *= fSin;
				}
				else
				{
					fSin = fTmp3/fTmp0;
					fTmp1 = Math::Sqrt(fSin*fSin+1.0);
					afSubDiag[i2+1] = fTmp0*fTmp1;
					fCos = 1.0/fTmp1;
					fSin *= fCos;
				}
				fTmp0 = afDiag[i2+1]-fTmp2;
				fTmp1 = (afDiag[i2]-fTmp0)*fSin+2.0*fTmp4*fCos;
				fTmp2 = fSin*fTmp1;
				afDiag[i2+1] = fTmp0+fTmp2;
				fTmp0 = fCos*fTmp1-fTmp4;

				for (int iRow = 0; iRow < 3; iRow++)
				{
					fTmp3 = m[iRow][i2+1];
					m[iRow][i2+1] = fSin*m[iRow][i2] +
						fCos*fTmp3;
					m[iRow][i2] = fCos*m[iRow][i2] -
						fSin*fTmp3;
				}
			}
			afDiag[i0] -= fTmp2;
			afSubDiag[i0] = fTmp0;
			afSubDiag[i1] = 0.0;
		}

		if ( iIter == iMaxIter )
		{
			// should not get here under normal circumstances
			return false;
		}
	}

	return true;
}
