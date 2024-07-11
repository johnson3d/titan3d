/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxquaternion.cpp
	Created Time:		30:6:2002   16:34
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/
#include "v3dxQuaternion.h"
#include "v3dxMatrix3.h"
#include "v3dxVector3.h"

#define new VNEW

#if defined(PLATFORM_WIN)
#pragma warning(disable:4244)
#pragma warning(disable:4305)
#else
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wlogical-op-parentheses"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif

float v3dxQuaternion::ms_fEpsilon = 1e-03;
v3dxQuaternion v3dxQuaternion::IDENTITY(0.f, 0.f, 0.f, 1.f);
v3dxQuaternion v3dxQuaternion::ZERO(0.f, 0.f, 0.f, 0.f);

using namespace TPL_HELP; 



struct FTestQuat
{
	FTestQuat()
	{
		for (int i = 0; i < 360; i++)
		{
			for (int j = 0; j < 360; j++)
			{
				for (int k = 0; k < 360; k++)
				{
					v3dxQuaternion tmp;
					v3dxRotator a0;
					a0.Yaw = ((float)i) * V_DEG_TO_RAD;
					a0.Pitch = ((float)j) * V_DEG_TO_RAD;
					a0.Roll = ((float)k) * V_DEG_TO_RAD;
					tmp.eulerRadianAnglesToQuat(a0.Yaw, a0.Pitch, a0.Roll);
					v3dxRotator a;
					auto det = tmp.ToEuler(a);
					
					v3dxRotator ca;
					ca.Yaw = a.Yaw - a0.Yaw;
					ca.Pitch = a.Pitch - a0.Pitch;
					ca.Roll = a.Roll - a0.Roll;
					ca.Yaw = Math::ClampAngleRad(ca.Yaw);
					ca.Pitch = Math::ClampAngleRad(ca.Pitch);
					ca.Roll = Math::ClampAngleRad(ca.Roll);

					const float Epsl = 0.02f;
					const float TPI = 6.283;
					const float CV = TPI - Epsl;
					if ((ca.Yaw > Epsl && ca.Yaw < CV) ||
						(ca.Pitch > Epsl && ca.Pitch < CV) ||
						(ca.Roll > Epsl && ca.Roll < CV))
					//if (i != x || j != y || k != z)
					{
						if (det > 1.0 - 0.05f || det < -1.0f + 0.05f)
						{
							//int xxx = 0;
						}
						else
						{
							//int xxx = 0;
						}
					}
				}
			}
		}
	}
};

//static FTestQuat ttt;

//-----------------------------------------------------------------------
void v3dxQuaternion::slerp(v3dxQuaternion& temp, float t, const v3dxQuaternion& rkP,
	const v3dxQuaternion& to, bool shortestPath)
{
	v3dxQuaternionSlerp(&temp,
		&rkP,
		&to,
		t);
}

void v3dxQuaternion::slerp(const v3dxQuaternion &to, float t)
{
	v3dxQuaternionSlerp(this,
		this,
		&to,
		t);
}

float v3dxQuaternion::AngularDistance(const v3dxQuaternion& lhs, const v3dxQuaternion& rhs)
{
	float dot = lhs.dot(rhs);
	if (dot < 0.0f)
		dot = -dot;
	return Math::ACos(TPL_HELP::vfxMIN(1.0F, dot)) * 2.0f;
}

void v3dxQuaternion::FromAngleAxis(const float& rfAngle, const v3dxVector3& rkAxis)
{
	axisRadianAngleToQuat(rkAxis, rfAngle);
}
void v3dxQuaternion::ToAngleAxis(float &rfAngle, v3dxVector3& rkAxis) const
{
	getAxisRadianAngle(rkAxis, rfAngle);
}

void v3dxQuaternion::getAxisRadianAngle(v3dxVector3 &v3, float &fAngle) const
{
	v3dxQuaternionToAxisAngle(this, &v3, &fAngle);
}

void v3dxQuaternion::fromRotationMatrix (const v3dxMatrix3& kRot)
{
	v3dxQuaternion &result = *this;
	float scale = kRot.m11 + kRot.m22 + kRot.m33;

	if (scale > 0.0f)
	{
		float sqrt = (float)(Math::Sqrt(((double)(scale + 1.0f))));

		result.W = sqrt * 0.5f;
		sqrt = 0.5f / sqrt;

		result.X = (kRot.m23 - kRot.m32) * sqrt;
		result.Y = (kRot.m31 - kRot.m13) * sqrt;
		result.Z = (kRot.m12 - kRot.m21) * sqrt;

		return;
	}

	if ((kRot.m11 >= kRot.m22) && (kRot.m11 >= kRot.m33))
	{
		float sqrt = (float)(Math::Sqrt((double)(1.0f + kRot.m11 - kRot.m22 - kRot.m33)));
		float half = 0.5f / sqrt;

		result.X = 0.5f * sqrt;
		result.Y = (kRot.m12 + kRot.m21) * half;
		result.Z = (kRot.m13 + kRot.m31) * half;
		result.W = (kRot.m23 - kRot.m32) * half;

		return;
	}

	if (kRot.m22 > kRot.m33)
	{
		float sqrt = (float)(Math::Sqrt((double)(1.0f + kRot.m22 - kRot.m11 - kRot.m33)));
		float half = 0.5f / sqrt;

		result.X = (kRot.m21 + kRot.m12) * half;
		result.Y = 0.5f * sqrt;
		result.Z = (kRot.m32 + kRot.m23) * half;
		result.W = (kRot.m31 - kRot.m13) * half;

		return;
	}

	float sqrt = (float)(Math::Sqrt((double)(1.0f + kRot.m33 - kRot.m11 - kRot.m22)));
	float half = 0.5f / sqrt;

	result.X = (kRot.m31 + kRot.m13) * half;
	result.Y = (kRot.m32 + kRot.m23) * half;
	result.Z = 0.5f * sqrt;
	result.W = (kRot.m12 - kRot.m21) * half;
}

void v3dxQuaternion::fromRotationMatrix (const v3dxMatrix4& kRot)
{
#if defined USE_DX && defined(USE_DXMATH)
	D3DXQuaternionRotationMatrix((D3DXQUATERNION *)this, (const D3DXMATRIX*)&kRot);
#else

	v3dxQuaternion &result = *this;
	float scale = kRot.m11 + kRot.m22 + kRot.m33;

	if( scale > 0.0f )
	{
		float sqrt = (float)( Math::Sqrt(( (double)(scale + 1.0f) ) ));

		result.W = sqrt * 0.5f;
		sqrt = 0.5f / sqrt;

		result.X = (kRot.m23 - kRot.m32) * sqrt;
		result.Y = (kRot.m31 - kRot.m13) * sqrt;
		result.Z = (kRot.m12 - kRot.m21) * sqrt;

		return;
	}

	if( (kRot.m11 >= kRot.m22) && (kRot.m11 >= kRot.m33) )
	{
		float sqrt = (float)( Math::Sqrt( (double)(1.0f + kRot.m11 - kRot.m22 - kRot.m33) ) );
		float half = 0.5f / sqrt;

		result.X = 0.5f * sqrt;
		result.Y = (kRot.m12 + kRot.m21) * half;
		result.Z = (kRot.m13 + kRot.m31) * half;
		result.W = (kRot.m23 - kRot.m32) * half;

		return ;
	}

	if( kRot.m22 > kRot.m33 )
	{
		float sqrt = (float)( Math::Sqrt( (double)(1.0f + kRot.m22 - kRot.m11 - kRot.m33) ) );
		float half = 0.5f / sqrt;

		result.X = (kRot.m21 + kRot.m12) * half;
		result.Y = 0.5f * sqrt;
		result.Z = (kRot.m32 + kRot.m23) * half;
		result.W = (kRot.m31 - kRot.m13) * half;

		return ;
	}

	float sqrt = (float)( Math::Sqrt( (double)(1.0f + kRot.m33 - kRot.m11 - kRot.m22) ) );
	float half = 0.5f / sqrt;

	result.X = (kRot.m31 + kRot.m13) * half;
	result.Y = (kRot.m32 + kRot.m23) * half;
	result.Z = 0.5f * sqrt;
	result.W = (kRot.m12 - kRot.m21) * half;

	// Algorithm in Ken Shoemake's article in 1987 SIGGRAPH course notes
	// article "Quaternion Calculus and Fast Animation".

	//float fTrace = kRot[0][0]+kRot[1][1]+kRot[2][2];
	//float s;

	//if ( fTrace > 0.0 )
	//{
	//	// |w| > 1/2, may as well choose w > 1/2
	//	s = Math::Sqrt(fTrace + 1.0);  // 2w
	//	w = 0.5*s;
	//	s = 0.5/s;  // 1/(4w)
	//	x = (kRot[1][2]-kRot[2][1])*s;
	//	y = (kRot[2][0]-kRot[0][2])*s;
	//	z = (kRot[0][1]-kRot[1][0])*s;
	//}
	//else
	//{
	//	if ( (kRot[1][1] > kRot[0][0]) && (kRot[2][2] <= kRot[1][1]) )
	//	{
	//		s = Math::Sqrt(kRot[1][1] - (kRot[2][2] + kRot[0][0]) + 1.f);

	//		y = s * 0.5;

	//		if (s!=0)
	//		{
	//			s = 0.5 / s;
	//		}

	//		w = (kRot[2][0]-kRot[0][2]) * s;
	//		z = (kRot[2][1]+kRot[1][2]) * s;
	//		x = (kRot[0][1]+kRot[1][0]) * s;
	//	}
	//	else
	//	{
	//		if ( (kRot[1][1] <= kRot[0][0]) && (kRot[2][2] > kRot[0][0]) || (kRot[2][2] > kRot[1][1]) )
	//		{
	//			s = Math::Sqrt((kRot[2][2] - (kRot[0][0] + kRot[1][1])) + 1.f);

	//			z = s * 0.5;

	//			if (s!= 0)
	//			{
	//				s = 0.5 / s;
	//			}

	//			w = (kRot[0][1]-kRot[1][0]) * s;
	//			x = (kRot[0][2]+kRot[2][0]) * s;
	//			y = (kRot[1][2]+kRot[2][1]) * s;
	//		}
	//		else
	//		{
	//			s = Math::Sqrt((kRot[0][0] - (kRot[1][1] + kRot[2][2])) + 1.f);

	//			x = s * 0.5;

	//			if (s!=0)
	//			{
	//				s = 0.5 / s;
	//			}

	//			w = (kRot[1][2]-kRot[2][1]) * s;
	//			y = (kRot[1][0]+kRot[0][1]) * s;
	//			z = (kRot[2][0]+kRot[0][2]) * s;
	//		}
	//	}
	//}
#endif
}

void v3dxQuaternion::fromRotationDMatrix(const v3dDMatrix4_t& kRot)
{
	v3dxQuaternion& result = *this;
	auto scale = kRot.m11 + kRot.m22 + kRot.m33;

	if (scale > 0.0)
	{
		auto sqrt = Math::D_Sqrt(scale + 1.0);

		result.W = sqrt * 0.5;
		sqrt = 0.5 / sqrt;

		result.X = (kRot.m23 - kRot.m32) * sqrt;
		result.Y = (kRot.m31 - kRot.m13) * sqrt;
		result.Z = (kRot.m12 - kRot.m21) * sqrt;

		return;
	}

	if ((kRot.m11 >= kRot.m22) && (kRot.m11 >= kRot.m33))
	{
		auto sqrt = Math::D_Sqrt(1.0 + kRot.m11 - kRot.m22 - kRot.m33);
		auto half = 0.5 / sqrt;

		result.X = 0.5 * sqrt;
		result.Y = (kRot.m12 + kRot.m21) * half;
		result.Z = (kRot.m13 + kRot.m31) * half;
		result.W = (kRot.m23 - kRot.m32) * half;

		return;
	}

	if (kRot.m22 > kRot.m33)
	{
		auto sqrt = Math::D_Sqrt(1.0 + kRot.m22 - kRot.m11 - kRot.m33);
		auto half = 0.5 / sqrt;

		result.X = (kRot.m21 + kRot.m12) * half;
		result.Y = 0.5f * sqrt;
		result.Z = (kRot.m32 + kRot.m23) * half;
		result.W = (kRot.m31 - kRot.m13) * half;

		return;
	}

	auto sqrt = Math::D_Sqrt(1.0 + kRot.m33 - kRot.m11 - kRot.m22);
	auto half = 0.5 / sqrt;

	result.X = (kRot.m31 + kRot.m13) * half;
	result.Y = (kRot.m32 + kRot.m23) * half;
	result.Z = 0.5f * sqrt;
	result.W = (kRot.m12 - kRot.m21) * half;
}

void v3dxQuaternion::toRotationMatrix(v3dxMatrix3& Matrix3) const
{
	float fTx = 2.0f*X;
	float fTy = 2.0f*Y;
	float fTz = 2.0f*Z;
	float fTwx = fTx*W;
	float fTwy = fTy*W;
	float fTwz = fTz*W;
	float fTxx = fTx*X;
	float fTxy = fTy*X;
	float fTxz = fTz*X;
	float fTyy = fTy*Y;
	float fTyz = fTz*Y;
	float fTzz = fTz*Z;

	Matrix3[0][0] = 1.0f - (fTyy + fTzz);
	Matrix3[0][1] = fTxy + fTwz;
	Matrix3[0][2] = fTxz - fTwy;
	Matrix3[1][0] = fTxy - fTwz;
	Matrix3[1][1] = 1.0f - (fTxx + fTzz);
	Matrix3[1][2] = fTyz + fTwx;
	Matrix3[2][0] = fTxz + fTwy;
	Matrix3[2][1] = fTyz - fTwx;
	Matrix3[2][2] = 1.0f - (fTxx + fTyy);
}

void v3dxQuaternion::toRotationMatrix(v3dxMatrix4& Matrix4) const
{
	v3dxMatrixRotationQuaternion(&Matrix4, this);
}
// void v3dxQuaternion::toRotationMatrix(v3dxMatrix4 &Matrix4) const 
//{
//	float fTx  = 2.0f*x;
//	float fTy  = 2.0f*y;
//	float fTz  = 2.0f*z;
//	float fTwx = fTx*w;
//	float fTwy = fTy*w;
//	float fTwz = fTz*w;
//	float fTxx = fTx*x;
//	float fTxy = fTy*x;
//	float fTxz = fTz*x;
//	float fTyy = fTy*y;
//	float fTyz = fTz*y;
//	float fTzz = fTz*z;
//
//	Matrix4[0][0] = 1.0f-(fTyy+fTzz);
//	Matrix4[0][1] = fTxy+fTwz;
//	Matrix4[0][2] = fTxz-fTwy;
//	Matrix4[1][0] = fTxy-fTwz;
//	Matrix4[1][1] = 1.0f-(fTxx+fTzz);
//	Matrix4[1][2] = fTyz+fTwx;
//	Matrix4[2][0] = fTxz+fTwy;
//	Matrix4[2][1] = fTyz-fTwx;
//	Matrix4[2][2] = 1.0f-(fTxx+fTyy);
//}

////-----------------------------------------------------------------------
// void v3dxQuaternion::fromAxes (const v3dxVector3* akAxis)
//{
//	v3dxMatrix3 kRot;
//
//	for (int iCol = 0; iCol < 3; iCol++)
//	{
//		kRot[0][iCol] = akAxis[iCol].x;
//		kRot[1][iCol] = akAxis[iCol].y;
//		kRot[2][iCol] = akAxis[iCol].z;
//	}
//
//	fromRotationMatrix(kRot);
//}

//-----------------------------------------------------------------------
 void v3dxQuaternion::fromAxes (const v3dxVector3& xAxis, const v3dxVector3& yAxis, const v3dxVector3& zAxis)
{
	v3dxMatrix3 kRot;

	kRot[0][0] = xAxis.X;
	kRot[1][0] = xAxis.Y;
	kRot[2][0] = xAxis.Z;

	kRot[0][1] = yAxis.X;
	kRot[1][1] = yAxis.Y;
	kRot[2][1] = yAxis.Z;

	kRot[0][2] = zAxis.X;
	kRot[1][2] = zAxis.Y;
	kRot[2][2] = zAxis.Z;

	fromRotationMatrix(kRot);

}

// void v3dxQuaternion::toAxis(v3dxVector3 *pvAxis) const
//{
//	v3dxMatrix3 matRot;
//	toRotationMatrix(matRot);
//
//	for (int nCol=0; nCol<3; nCol++)
//	{
//		pvAxis[nCol][0] = matRot[0][nCol];
//		pvAxis[nCol][1] = matRot[1][nCol];
//		pvAxis[nCol][2] = matRot[2][nCol];
//	}
//}

 void v3dxQuaternion::toAxis(v3dxVector3 &vAxisX, v3dxVector3 &vAxisY, v3dxVector3 &vAxisZ) const
{
	v3dxMatrix3 matRot;
	toRotationMatrix(matRot);

	vAxisX[0] = matRot[0][0];
	vAxisX[1] = matRot[1][0];
	vAxisX[2] = matRot[2][0];

	vAxisY[0] = matRot[0][1];
	vAxisY[1] = matRot[1][1];
	vAxisY[2] = matRot[2][1];

	vAxisZ[0] = matRot[0][2];
	vAxisZ[1] = matRot[1][2];
	vAxisZ[2] = matRot[2][2];
}


////-----------------------------------------------------------------------
// void v3dxQuaternion::FromAngleAxis (const float& rfAngle,
//									const v3dxVector3& rkAxis)
//{
//	// assert:  axis[] is unit length
//	//
//	// The quaternion representing the rotation is
//	//   q = cos(A/2)+sin(A/2)*(x*i+y*j+z*k)
//
//	float fHalfAngle = 0.5*rfAngle;
//	float fSin = Math::Sin(fHalfAngle);
//	w = Math::Cos(fHalfAngle);
//	x = fSin*rkAxis.x;
//	y = fSin*rkAxis.y;
//	z = fSin*rkAxis.z;
//}

////-----------------------------------------------------------------------

// void v3dxQuaternion::ToAngleAxis (float& rfAngle, v3dxVector3& rkAxis) const
//{
//	// The quaternion representing the rotation is
//	//   q = cos(A/2)+sin(A/2)*(x*i+y*j+z*k)
//
//	float fSqrLength = x*x+y*y+z*z;
//	if ( fSqrLength > 0.0 )
//	{
//		rfAngle = 2.0*Math::ACos(w);
//		float fInvLength = 1.0/Math::Sqrt(fSqrLength);
//		rkAxis.x = x*fInvLength;
//		rkAxis.y = y*fInvLength;
//		rkAxis.z = z*fInvLength;
//	}
//	else
//	{
//		// angle is 0 (mod 2*pi), so any axis will do
//		rfAngle = 0.0;
//		rkAxis.x = 1.0;
//		rkAxis.y = 0.0;
//		rkAxis.z = 0.0;
//	}
//}

//-----------------------------------------------------------------------
 v3dxVector3 v3dxQuaternion::xAxis(void)
{
	float fTx  = 2.0*X;
	float fTy  = 2.0*Y;
	float fTz  = 2.0*Z;
	float fTwy = fTy*W;
	float fTwz = fTz*W;
	float fTxy = fTy*X;
	float fTxz = fTz*X;
	float fTyy = fTy*Y;
	float fTzz = fTz*Z;

	return v3dxVector3(1.0-(fTyy+fTzz), fTxy+fTwz, fTxz-fTwy);
}
//-----------------------------------------------------------------------
 v3dxVector3 v3dxQuaternion::yAxis(void)
{
	float fTx  = 2.0*X;
	float fTy  = 2.0*Y;
	float fTz  = 2.0*Z;
	float fTwx = fTx*W;
	float fTwz = fTz*W;
	float fTxx = fTx*X;
	float fTxy = fTy*X;
	float fTyz = fTz*Y;
	float fTzz = fTz*Z;

	return v3dxVector3(fTxy-fTwz, 1.0-(fTxx+fTzz), fTyz+fTwx);
}
//-----------------------------------------------------------------------
 v3dxVector3 v3dxQuaternion::zAxis(void)
{
	float fTx  = 2.0*X;
	float fTy  = 2.0*Y;
	float fTz  = 2.0*Z;
	float fTwx = fTx*W;
	float fTwy = fTy*W;
	float fTxx = fTx*X;
	float fTxz = fTz*X;
	float fTyy = fTy*Y;
	float fTyz = fTz*Y;

	return v3dxVector3(fTxz+fTwy, fTyz-fTwx, 1.0-(fTxx+fTyy));
}

//-----------------------------------------------------------------------
 v3dxQuaternion v3dxQuaternion::exp () const
{
	// If q = A*(x*i+y*j+z*k) where (x,y,z) is unit length, then
	// exp(q) = cos(A)+sin(A)*(x*i+y*j+z*k).  If sin(A) is near zero,
	// use exp(q) = cos(A)+A*(x*i+y*j+z*k) since A/sin(A) has limit 1.

	float fAngle = Math::Sqrt(X*X+Y*Y+Z*Z);
	float fSin = Math::Sin(fAngle);

	v3dxQuaternion kResult;
	kResult.W = Math::Cos(fAngle);

	if ( Math::Abs(fSin) >= ms_fEpsilon )
	{
		float fCoeff = fSin/fAngle;
		kResult.X = fCoeff*X;
		kResult.Y = fCoeff*Y;
		kResult.Z = fCoeff*Z;
	}
	else
	{
		kResult.X = X;
		kResult.Y = Y;
		kResult.Z = Z;
	}

	return kResult;
}
//-----------------------------------------------------------------------
 v3dxQuaternion v3dxQuaternion::log () const
{
	// If q = cos(A)+sin(A)*(x*i+y*j+z*k) where (x,y,z) is unit length, then
	// log(q) = A*(x*i+y*j+z*k).  If sin(A) is near zero, use log(q) =
	// sin(A)*(x*i+y*j+z*k) since sin(A)/A has limit 1.

	v3dxQuaternion kResult;
	kResult.W = 0.0;

	if ( Math::Abs(W) < 1.0 )
	{
		float fAngle = Math::ACos(W);
		float fSin = Math::Sin(fAngle);
		if ( Math::Abs(fSin) >= ms_fEpsilon )
		{
			float fCoeff = fAngle/fSin;
			kResult.X = fCoeff*X;
			kResult.Y = fCoeff*Y;
			kResult.Z = fCoeff*Z;
			return kResult;
		}
	}

	kResult.X = X;
	kResult.Y = Y;
	kResult.Z = Z;

	return kResult;
}
//-----------------------------------------------------------------------
// v3dxQuaternion v3dxQuaternion::slerp (float fT, const v3dxQuaternion& rkP,
//									  const v3dxQuaternion& rkQ, bool shortestPath)
//{
//	float fCos = rkP.dot(rkQ);
//	float fAngle = Math::ACos(fCos);
//
//	if ( Math::Abs(fAngle) < ms_fEpsilon )
//		return rkP;
//
//	float fSin = Math::Sin(fAngle);
//	float fInvSin = 1.0/fSin;
//	float fCoeff0 = Math::Sin((1.0-fT)*fAngle)*fInvSin;
//	float fCoeff1 = Math::Sin(fT*fAngle)*fInvSin;
//	// Do we need to invert rotation?
//	if (fCos < 0.0f && shortestPath)
//	{
//		fCoeff0 = -fCoeff0;
//		// taking the complement requires renormalisation
//		v3dxQuaternion t(fCoeff0*rkP + fCoeff1*rkQ);
//		t.normalize();
//		return t;
//	}
//	else
//	{
//		return fCoeff0*rkP + fCoeff1*rkQ;
//	}
//
//}
//-----------------------------------------------------------------------
 v3dxQuaternion v3dxQuaternion::slerpExtraSpins (float fT,
												const v3dxQuaternion& rkP, const v3dxQuaternion& rkQ, int iExtraSpins)
{
	float fCos = rkP.dot(rkQ);
	float fAngle = Math::ACos(fCos);

	if ( Math::Abs(fAngle) < ms_fEpsilon )
		return rkP;

	float fSin = Math::Sin(fAngle);
	float fPhase = Math::V3_PI*iExtraSpins*fT;
	float fInvSin = 1.0/fSin;
	float fCoeff0 = Math::Sin((1.0-fT)*fAngle - fPhase)*fInvSin;
	float fCoeff1 = Math::Sin(fT*fAngle + fPhase)*fInvSin;
	return fCoeff0*rkP + fCoeff1*rkQ;
}

//-----------------------------------------------------------------------
 void v3dxQuaternion::intermediate (const v3dxQuaternion& rkQ0,
								   const v3dxQuaternion& rkQ1, const v3dxQuaternion& rkQ2,
								   v3dxQuaternion& rkA, v3dxQuaternion& rkB)
{
	// assert:  q0, q1, q2 are unit quaternions

	v3dxQuaternion kQ0inv = rkQ0.unitInverse();
	v3dxQuaternion kQ1inv = rkQ1.unitInverse();
	v3dxQuaternion rkP0 = kQ0inv*rkQ1;
	v3dxQuaternion rkP1 = kQ1inv*rkQ2;
	v3dxQuaternion kArg = 0.25*(rkP0.log()-rkP1.log());
	v3dxQuaternion kMinusArg = -kArg;

	rkA = rkQ1*kArg.exp();
	rkB = rkQ1*kMinusArg.exp();
}

////-----------------------------------------------------------------------
// v3dxQuaternion v3dxQuaternion::squad (float fT,
//									  const v3dxQuaternion& rkP, const v3dxQuaternion& rkA,
//									  const v3dxQuaternion& rkB, const v3dxQuaternion& rkQ)
//{
//	float fSlerpT = 2.0*fT*(1.0-fT);
//	v3dxQuaternion kSlerpP = slerp(fT,rkP,rkQ);
//	v3dxQuaternion kSlerpQ = slerp(fT,rkA,rkB);
//	return slerp(fSlerpT,kSlerpP,kSlerpQ);
//}


 v3dxQuaternion v3dxQuaternion::inverse() const
{
	float fNorm = W*W+X*X+Y*Y+Z*Z;
	if ( fNorm > 0.0f )
	{
		float fInvNorm = 1.0f/fNorm;
		return v3dxQuaternion(-X*fInvNorm,-Y*fInvNorm,-Z*fInvNorm, W*fInvNorm);
	}
	else
	{
		// return an invalid result to flag the error
		return ZERO;
	}
}

 template<typename T> inline bool vfxEq(const T& v1, const T& v2, const T& Epsilon) {
	 return (std::abs(v1 - v2) <= Epsilon);
 }

// Does the quaternion normalized ?
 bool v3dxQuaternion::isNormalize() const
{
	// the magnitude of quanternion
	float fMagnitude = magnitude(); 

	if (fMagnitude == 0.f) return false; // avoid the div zero error

	if ( vfxEq(fMagnitude, 1.f, 0.0001f) )
		return true;

	return false;
}

 template<typename T> inline T vfxLimit(const T& v, const T& minv, const T& maxv) {
	 if (v <= minv) {
		 return minv;
	 }
	 else if (v >= maxv) {
		 return maxv;
	 }
	 else {
		 return v;
	 }
 }

 void v3dxQuaternion::normalize()
{
	//if (isNormalize()) return;

	float fMagnitude = magnitude();

	W /= fMagnitude;
	X /= fMagnitude;
	Y /= fMagnitude;
	Z /= fMagnitude;

	//limit v3dxQuaternion's element at range -1.0f ~ 1.-0f
	vfxLimit(W, -1.0f, 1.0f);
	vfxLimit(X, -1.0f, 1.0f);
	vfxLimit(Y, -1.0f, 1.0f);
	vfxLimit(Z, -1.0f, 1.0f);
}

//inline Vector3f RotateVectorByQuat(const Quaternionf& lhs, const Vector3f& rhs)
//{
//	//	Matrix3x3f m;
//	//	QuaternionToMatrix (lhs, &m);
//	//	Vector3f restest = m.MultiplyVector3 (rhs);
//	float x = lhs.x * 2.0F;
//	float y = lhs.y * 2.0F;
//	float z = lhs.z * 2.0F;
//	float xx = lhs.x * x;
//	float yy = lhs.y * y;
//	float zz = lhs.z * z;
//	float xy = lhs.x * y;
//	float xz = lhs.x * z;
//	float yz = lhs.y * z;
//	float wx = lhs.w * x;
//	float wy = lhs.w * y;
//	float wz = lhs.w * z;
//
//	Vector3f res;
//	res.x = (1.0f - (yy + zz)) * rhs.x + (xy - wz)          * rhs.y + (xz + wy)          * rhs.z;
//	res.y = (xy + wz)          * rhs.x + (1.0f - (xx + zz)) * rhs.y + (yz - wx)          * rhs.z;
//	res.z = (xz - wy)          * rhs.x + (yz + wx)          * rhs.y + (1.0f - (xx + yy)) * rhs.z;
//
//	//	AssertIf (!CompareApproximately (restest, res));
//	return res;
//}


v3dxVector3 v3dxQuaternion::operator *(const v3dxVector3 &v3) const
{

	// Given a vector u = (x0,y0,z0) and a unit length quaternion
	// q = <w,x,y,z>, the vector v = (x1,y1,z1) which represents the
	// rotation of u by q is v = q*u*q^{-1} where * indicates quaternion
	// multiplication and where u is treated as the quaternion <0,x0,y0,z0>.
	// Note that q^{-1} = <w,-x,-y,-z>, so no real work is required to
	// invert q.  Now
	//
	//   q*u*q^{-1} = q*<0,x0,y0,z0>*q^{-1}
	//     = q*(x0*i+y0*j+z0*k)*q^{-1}
	//     = x0*(q*i*q^{-1})+y0*(q*j*q^{-1})+z0*(q*k*q^{-1})
	//
	// As 3-vectors, q*i*q^{-1}, q*j*q^{-1}, and 2*k*q^{-1} are the columns
	// of the rotation matrix computed in Quaternion::ToRotationMatrix.
	// The vector v is obtained as the product of that rotation matrix with
	// vector u.  As such, the quaternion representation of a rotation
	// matrix requires less space than the matrix and more time to compute
	// the rotated vector.  Typical space-time tradeoff...


	static bool bDefault = true;
	if(bDefault)
	{
		v3dxVector3 uv, uuv; 
		v3dxVector3 qvec(X, Y, Z);
		v3dxVec3Cross( &uv, &qvec, &v3 );
		v3dxVec3Cross( &uuv, &qvec, &uv );
		uv *= (2.0f * W); 
		uuv *= 2.0f; 

		return v3 + uv + uuv;
	}
	else
	{
		v3dxMatrix3 kRot;
		toRotationMatrix(kRot);
		return kRot*v3;
	}

	//const v3dxQuaternion& lhs = *this;
	//const v3dxVector3& rhs= v3;
	////	Matrix3x3f m;
	////	QuaternionToMatrix (lhs, &m);
	////	Vector3f restest = m.MultiplyVector3 (rhs);
	//float x = lhs.x * 2.0F;
	//float y = lhs.y * 2.0F;
	//float z = lhs.z * 2.0F;
	//float xx = lhs.x * x;
	//float yy = lhs.y * y;
	//float zz = lhs.z * z;
	//float xy = lhs.x * y;
	//float xz = lhs.x * z;
	//float yz = lhs.y * z;
	//float wx = lhs.w * x;
	//float wy = lhs.w * y;
	//float wz = lhs.w * z;

	//v3dxVector3 res;
	//res.x = (1.0f - (yy + zz)) * rhs.x + (xy - wz)          * rhs.y + (xz + wy)          * rhs.z;
	//res.y = (xy + wz)          * rhs.x + (1.0f - (xx + zz)) * rhs.y + (yz - wx)          * rhs.z;
	//res.z = (xz - wy)          * rhs.x + (yz + wx)          * rhs.y + (1.0f - (xx + yy)) * rhs.z;

	////	AssertIf (!CompareApproximately (restest, res));
	//return res;
}

// Covert method ==================================================================
/*
Qx = [ cos(a/2), (sin(a/2), 0, 0)]
Qy = [ cos(b/2), (0, sin(b/2), 0)]
Qz = [ cos(c/2), (0, 0, sin(c/2))]
*/
// void v3dxQuaternion::eulerRadianAnglesToQuat(float p_x, float p_y, float p_z)
//{
//	v3dxQuaternion Q_x,Q_y,Q_z;
//
//	Q_x = v3dxQuaternion((float)cos(p_x/2), (float)sin(p_x/2), 0, 0);
//	Q_y = v3dxQuaternion((float)cos(p_y/2), 0, (float)sin(p_y/2), 0);
//	Q_z = v3dxQuaternion((float)cos(p_z/2), 0, 0, (float)sin(p_z/2));
//
//	*this = Q_x*Q_y*Q_z;
//}

// void v3dxQuaternion::eularToMe(float fRadX, float fRadY, float fRadZ)
//{
//	double	ex, ey, ez;		// temp half euler angles
//	double	cr, cp, cy, sr, sp, sy, cpcy, spsy;		// temp vars in roll,pitch yaw
//
//	//	ex = DEGTORAD(x) / 2.0;	// convert to rads and half them
//	//	ey = DEGTORAD(y) / 2.0;
//	//	ez = DEGTORAD(z) / 2.0;
//
//	ex = fRadX / 2.0f;	// convert to rads and half them
//	ey = fRadY / 2.0f;
//	ez = fRadZ / 2.0f;
//
//	cr = cos(ex);
//	cp = cos(ey);
//	cy = cos(ez);
//
//	sr = sin(ex);
//	sp = sin(ey);
//	sy = sin(ez);
//
//	cpcy = cp * cy;
//	spsy = sp * sy;
//
//	w = float(cr * cpcy + sr * spsy);
//
//	x = float(sr * cpcy - cr * spsy);
//	y = float(cr * sp * cy + sr * cp * sy);
//	z = float(cr * cp * sy - sr * sp * cy);
//
//	return;
//}
// Slerp =========================================================================
// void v3dxQuaternion::slerp(const v3dxQuaternion &quat, float fSlerp)
//{
//	float norm;
//	norm = x * quat[0] + y * quat[1] + z * quat[2] + w * quat[3];
//
//	bool bFlip;
//	bFlip = false;
//
//	if(norm < 0.0f)
//	{
//		norm = -norm;
//		bFlip = true;
//	}
//
//	float inv_d;
//	if(1.0f - norm < 0.000001f)
//	{
//		inv_d = 1.0f - fSlerp;
//	}
//	else
//	{
//		float theta;
//		theta = acosf(norm);
//
//		float s;
//		s = 1.0f / sinf(theta);
//
//		inv_d = sinf((1.0f - fSlerp) * theta) * s;
//		fSlerp = sinf(fSlerp * theta) * s;
//	}
//
//	if(bFlip)
//	{
//		fSlerp = -fSlerp;
//	}
//
//	set(	inv_d * w + fSlerp * quat[3],
//			inv_d * x + fSlerp * quat[0],
//			inv_d * y + fSlerp * quat[1],
//			inv_d * z + fSlerp * quat[2]
//		);
//
//		///////////////////////////
//		/*	float omega,cosom,sinom,Scale0,Scale1;
//
//		v3dxQuaternion QL = p_Q;
//
//		cosom =	(m_x * p_Q[0]) + (m_y * p_Q[1]) + (m_z * p_Q[2]) + (m_w * p_Q[3]);
//
//		if (cosom < 0)
//		{
//		cosom = -cosom;
//		QL.Set(-p_Q[0],-p_Q[1],-p_Q[2],-p_Q[3]);
//		}
//		else
//		{
//		// no changes 
//		}
//
//
//		if ( (1.0f - cosom) > ZEUS3D_EPSILON )
//		{
//		omega  = (float) acos( cosom );
//		sinom  = (float) sin( omega );
//		Scale0 = (float) sin( (1.0f-fSlerp) * omega) / sinom;
//		Scale1 = (float) sin( fSlerp*omega) / sinom;
//		}
//		else
//		{
//		// has numerical difficulties around cosom == 0
//		// in this case degenerate to linear interpolation
//
//		Scale0 = 1.0f - p_fSlerp;
//		Scale1 = p_fSlerp;
//		}
//
//
//		Set(	Scale0 * m_w + Scale1 * QL[3],
//		Scale0 * m_x + Scale1 * QL[0],
//		Scale0 * m_y + Scale1 * QL[1],
//		Scale0 * m_z + Scale1 * QL[2]
//		);
//		*/
//}


float v3dxQuaternion::ToEuler(v3dxRotator& result)
{
	return v3dxYawPitchRollQuaternionRotation(this, &result);
}

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif