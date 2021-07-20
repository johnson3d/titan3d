/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxmath.cpp
	Created Time:		30:6:2002   16:35
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/
#include "v3dxMath.h"

#include "v3dxPoly3.h"
#include "v3dxSegment3.h"
#include "v3dxBox3.h"
#include "v3dxMatrix4.h"
#include "v3dxPlane3.h"
#include "v3dxLine.h"
#include <algorithm>

#define new VNEW

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wunused-value"
#pragma clang diagnostic ignored "-Warray-bounds"
#endif

using namespace TPL_HELP;
const USHORT Math::USHORT_INFINITY = (USHORT)65535;
const float Math::POS_INFINITY = std::numeric_limits<float>::infinity();
const float Math::NEG_INFINITY = -std::numeric_limits<float>::infinity();
const float Math::V3_PI = float(4.0 * atan(1.0));
const float Math::TWO_PI = float(2.0 * Math::V3_PI);
const float Math::HALF_PI = float(0.5 * Math::V3_PI);
const float Math::fDeg2Rad = Math::V3_PI / float(180.0);
const float Math::fRad2Deg = float(180.0) / Math::V3_PI;

int Math::mTrigTableSize;
Math::AngleUnit Math::msAngleUnit;

float  Math::mTrigTableFactor;
float *Math::mSinTable = NULL;
float *Math::mTanTable = NULL;

Math GMath(360);
//-----------------------------------------------------------------------
Math::Math(unsigned int trigTableSize)
{
	msAngleUnit = AU_DEGREE;

	mTrigTableSize = trigTableSize;
	mTrigTableFactor = mTrigTableSize / Math::TWO_PI;

	mSinTable = (FLOAT *)malloc(mTrigTableSize * sizeof(FLOAT));
	mTanTable = (FLOAT *)malloc(mTrigTableSize * sizeof(FLOAT));

	buildTrigTables();

	// Init random number generator
	srand((unsigned)time(0));
}

//-----------------------------------------------------------------------
Math::~Math()
{
	free(mSinTable);
	free(mTanTable);
}

//-----------------------------------------------------------------------
void Math::buildTrigTables(void)
{
	// Build trig lookup tables
	// Could get away with building only PI sized Sin table but simpler this 
	// way. Who cares, it'll ony use an extra 8k of memory anyway and I like 
	// simplicity.
	float angle;
	for (int i = 0; i < mTrigTableSize; ++i)
	{
		angle = Math::TWO_PI * i / mTrigTableSize;
		mSinTable[i] = sin(angle);
		mTanTable[i] = tan(angle);
	}
}
//-----------------------------------------------------------------------	
float Math::SinTable(float fValue)
{
	// Convert range to index values, wrap if required
	int idx;
	if (fValue >= 0)
	{
		idx = int(fValue * mTrigTableFactor) % mTrigTableSize;
	}
	else
	{
		idx = mTrigTableSize - (int(-fValue * mTrigTableFactor) % mTrigTableSize) - 1;
	}

	return mSinTable[idx];
}
//-----------------------------------------------------------------------
float Math::TanTable(float fValue)
{
	// Convert range to index values, wrap if required
	int idx = int(fValue *= mTrigTableFactor) % mTrigTableSize;
	return mTanTable[idx];
}

//-----------------------------------------------------------------------
int Math::ISign(int iValue)
{
	return (iValue > 0 ? +1 : (iValue < 0 ? -1 : 0));
}
//-----------------------------------------------------------------------
float Math::ACos(float fValue)
{
	return acos(fValue);
}
//-----------------------------------------------------------------------
float Math::ASin(float fValue)
{
	return asin(fValue);
}
//-----------------------------------------------------------------------
//-----------------------------------------------------------------------
float Math::Sign(float fValue)
{
	if (fValue > 0.0)
		return 1.0;

	if (fValue < 0.0)
		return -1.0;

	return 0.0;
}
//-----------------------------------------------------------------------
float Math::InvSqrt(float fValue)
{
	long i;
	float y, r;

	y = fValue * 0.5f;
	i = *reinterpret_cast<long *>(&fValue);
	i = 0x5f3759df - (i >> 1);
	r = *reinterpret_cast<float *>(&i);
	r = r * (1.5f - r * r * y);
	return r;
}
//-----------------------------------------------------------------------
float Math::UnitRandom()
{
	auto sr = rand();
	return ((float)(sr % RAND_MAX)) / RAND_MAX;
}

//-----------------------------------------------------------------------
float Math::RangeRandom(float fLow, float fHigh)
{
	return (fHigh - fLow)*UnitRandom() + fLow;
}

//-----------------------------------------------------------------------
float Math::SymmetricRandom()
{
	return 2.0f * UnitRandom() - 1.0f;
}
//-----------------------------------------------------------------------
void Math::setAngleUnit(Math::AngleUnit unit)
{
	msAngleUnit = unit;
}
//-----------------------------------------------------------------------
Math::AngleUnit Math::getAngleUnit(void)
{
	return msAngleUnit;
}
//-----------------------------------------------------------------------
float Math::AngleUnitsToRadians(float angleunits)
{
	static float fDeg2Rad = V3_PI / float(180.0);

	if (msAngleUnit == AU_DEGREE)
		return angleunits * fDeg2Rad;
	else
		return angleunits;
}

//-----------------------------------------------------------------------
float Math::RadiansToAngleUnits(float radians)
{
	static float fRad2Deg = float(180.0) / V3_PI;

	if (msAngleUnit == AU_DEGREE)
		return radians * fRad2Deg;
	else
		return radians;
}

//-----------------------------------------------------------------------
bool Math::pointInTri2D(float px, float py, float ax, float ay, float bx, float by, float cx, float cy)
{
	float v1x, v2x, v1y, v2y;
	bool bClockwise;

	v1x = bx - ax;
	v1y = by - ay;

	v2x = px - bx;
	v2y = py - by;

	// For the sake of readability
#define Clockwise ( v1x * v2y - v1y * v2x >= 0.0 )

	bClockwise = Clockwise;

	v1x = cx - bx;
	v1y = cy - by;

	v2x = px - cx;
	v2y = py - cy;

	if (Clockwise != bClockwise)
		return false;

	v1x = ax - cx;
	v1y = ay - cy;

	v2x = px - ax;
	v2y = py - ay;

	if (Clockwise != bClockwise)
		return false;

	return true;

	// Clean up the #defines
#undef Clockwise
}
//-----------------------------------------------------------------------
bool Math::RealEqual(float a, float b, float tolerance)
{
	if ((b < (a + tolerance)) && (b >(a - tolerance)))
		return true;
	else
		return false;
}

inline bool CompareApproximately(float f0, float f1, float epsilon = 0.000001F)
{
	float dist = (f0 - f1);
	dist = Math::Abs(dist);
	return dist < epsilon;
}

inline void QuaternionToAxisAngle(const v3dxQuaternion& q, v3dxVector3* axis, float* targetAngle)
{
	*targetAngle = 2.0f* acos(q.w);
	if (CompareApproximately(*targetAngle, 0.0F))
	{
		*axis = v3dxVector3::UNIT_X;
		return;
	}

	float div = 1.0f / sqrt(1.0f - Math::Sqr(q.w));
	axis->setValue(q.x*div, q.y*div, q.z*div);
}

inline float Dot(const v3dxQuaternion& q1, const v3dxQuaternion& q2)
{
	return (q1.x*q2.x + q1.y*q2.y + q1.z*q2.z + q1.w*q2.w);
}

inline float SqrMagnitude(const v3dxQuaternion& q)
{
	return Dot(q, q);
}

inline float Magnitude(const v3dxQuaternion& q)
{
	return sqrtf(SqrMagnitude(q));
}

inline v3dxQuaternion Normalize(const v3dxQuaternion& q)
{
	return q / Magnitude(q);
}

extern "C"
{
	VFX_API v3dMatrix4_t* v3dxMatrixMultiply(v3dMatrix4_t* pOut, const v3dMatrix4_t* mat1, const v3dMatrix4_t* mat2)
	{
		return v3dxMatrix4Mul(pOut, mat1, mat2);
	}
	VFX_API v3dMatrix4_t* v3dxMatrix4Inverse(v3dMatrix4_t* out, const v3dMatrix4_t* mat, float* pDet)
	{
#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dMatrix4_t*)D3DXMatrixInverse((D3DXMATRIX *)out, pDet, (CONST D3DXMATRIX *)mat);//v3dxMatrix4Inverse_CXX( out , mat , pDet );
#else
		return v3dxMatrix4Inverse_CXX(out, mat, pDet);
#endif
	}
	VFX_API v3dMatrix4_t* v3dxMatrix4Transpose_extern(v3dMatrix4_t* pOut, const v3dMatrix4_t* pMat)
	{
		return v3dxMatrix4Transpose(pOut, pMat);
	}
	//-------------------------------------------------------------------------------------------------
	VFX_API v3dMatrix4_t* v3dxMatrix4PerspectiveEx(v3dMatrix4_t* pOut,
								const float near_plane,
								const float far_plane,
								const float fov_horiz,
								const float fov_vert)
	{
		float    h, w, Q;
		w = (float)(cosf(fov_horiz*0.5f));
		h = (float)(cosf(fov_vert*0.5f));
		Q = far_plane/(far_plane - near_plane);

		pOut->m11 = w;		 pOut->m12 = 0.f;	  pOut->m13 = 0.f;		  pOut->m14 = 0.f;
		pOut->m21 = 0.f;	 pOut->m22 = h;		  pOut->m23 = 0.f;		  pOut->m24 = 0.f;
		pOut->m31 = 0.f;	 pOut->m32 = 0.f;	  pOut->m33 = Q;		  pOut->m34 = 1.f;
		pOut->m41 = 0.f;	 pOut->m42 = 0.f;	  pOut->m43 = -Q*near_plane; pOut->m44 = 0.f;
		return pOut;
	}   

	VFX_API v3dMatrix4_t* v3dxMatrix4Perspective(v3dMatrix4_t* pOut,
								  float fovy,
								  float Aspect,
								  float zn,
								  float zf)
	{
		/*
		h = cos(fov/2) / sin(fov/2);
		w = h / aspect;

		2*zn/w  0       0              0
		0       2*zn/h  0              0
		0       0       zf/(zf-zn)     1
		0       0       zn*zf/(zn-zf)  0
		*/
		float Ys = 1.0f / tanf(fovy*0.5f);
		float Xs = Ys / Aspect;
		float m33 = float(double(zf) / (double(zf) - double(zn)));
		float m43 = float(double(zn) * double(zf) / (double(zn) - double(zf)));
	
		pOut->m11 = Xs;	 pOut->m12 = 0.f;	  pOut->m13 = 0.f;		  pOut->m14 = 0.f;
		pOut->m21 = 0.f;	 pOut->m22 = Ys;	  pOut->m23 = 0.f;		  pOut->m24 = 0.f;
		pOut->m31 = 0.f;	 pOut->m32 = 0.f;	  pOut->m33 = m33;   pOut->m34 = 1.0f;
		pOut->m41 = 0.f;	 pOut->m42 = 0.f;	  pOut->m43 = m43;   pOut->m44 = 0.f;

		return pOut;
	}

	//ƽ��ͶӰ
	VFX_API v3dMatrix4_t* v3dxMatrix4Ortho( v3dMatrix4_t* pOut,
								  float w,
								  float h,
								  float zn,
								  float zf)
	{
		/*
		2/w  0    0           0
		0    2/h  0           0
		0    0    1/(zf-zn)   0
		0    0    zn/(zn-zf)  1
		*/
		pOut->m11 = 2.f/w;	 pOut->m12 = 0.f;	  pOut->m13 = 0.f;		  pOut->m14 = 0.f;
		pOut->m21 = 0.f;	 pOut->m22 = 2.f/h;	  pOut->m23 = 0.f;		  pOut->m24 = 0.f;
		pOut->m31 = 0.f;	 pOut->m32 = 0.f;	  pOut->m33 = 1.f/(zf-zn);pOut->m34 = 0.f;
		pOut->m41 = 0.f;	 pOut->m42 = 0.f;	  pOut->m43 = zn/(zn-zf); pOut->m44 = 1.f;
		return pOut;
	}

	
	VFX_API v3dMatrix4_t* v3dxMatrix4OrthoForDirLightShadow(v3dMatrix4_t* pOut, float w, float h, float zn, float zf, float TexelOffsetNdcX, float TexelOffsetNdcY)
	{
		pOut->m11 = 2.f / w;						pOut->m12 = 0.f;							pOut->m13 = 0.f;					 pOut->m14 = 0.f;
		pOut->m21 = 0.f;							pOut->m22 = 2.f / h;						pOut->m23 = 0.f;					 pOut->m24 = 0.f;
		pOut->m31 = 0.f;							pOut->m32 = 0.f;						     pOut->m33 = 1.f / (zf - zn);	 pOut->m34 = 0.f;
		pOut->m41 = TexelOffsetNdcX;	pOut->m42 = TexelOffsetNdcY;   pOut->m43 = zn / (zn - zf);   pOut->m44 = 1.f;
		return pOut;
	}

	

	VFX_API void v3dxMatrixOrthoOffCenterLH(v3dMatrix4_t* pOut, float MinX, float MaxX, float MinY, float MaxY,
		float ZNear, float ZFar)
	{
		/* 2 / (r - l)			0							0						0
			0						2 / (t - b)				0						0
			0						0							1 / (zf - zn)		0
			(l + r) / (l - r)	(t + b) / (b - t)		zn / (zn - zf)		1
		*/
		pOut->m11 = 2.0f / (MaxX - MinX);						pOut->m12 = 0.0f;													pOut->m13 = 0.0f;									pOut->m14 = 0.0f;
		pOut->m21 = 0.0f;													pOut->m22 = 2.0f / (MaxY - MinY);						pOut->m23 = 0.0f;									pOut->m24 = 0.0f;
		pOut->m31 = 0.0f;													pOut->m32 = 0.0f;													pOut->m33 = 1.0f / (ZFar - ZNear);		pOut->m34 = 0.0f;
		pOut->m41 = (MinX + MaxX) / (MinX - MaxX);		pOut->m42 = (MinY + MaxY) / (MinY - MaxY);		pOut->m43 = ZNear / (ZNear - ZFar);	pOut->m44 = 1.0f;
	
	}


	VFX_API int v3dxWhichSide3D (const v3dVector3_t* p,
									 const v3dVector3_t* v1, const v3dVector3_t* v2 , const v3dVector3_t* vSpire )
	{
		v3dxPlane3 plane;
		plane.set( *vSpire , *v1 , *v2 );
		float s = plane.classify( *p );
		if( fabs(s)<EPSILON )
			return 0;
		else if (s > 0) 
			return 1;
		else// if (s < 0) 
			return -1;
	}

	VFX_API int v3dxWhichSide3D_v2 (const v3dVector3_t* p,
									const v3dVector3_t* v1, const v3dVector3_t* v2)
	{
		v3dxPlane3 plane;
		plane.set( v3dxVector3::ZERO , *v1 , *v2 );
		float s = plane.classify( *p );
		if( fabs(s)<EPSILON )
			return 0;
		else if (s > 0) 
			return 1;
		else// if (s < 0) 
			return -1;
		//// V1 % V2 �õ� V1-V2-0 �淨����������p-0��������õ��н�����
		//v3dVector3_t vPlaneNormal;
		//v3dxVec3Cross( &vPlaneNormal , v1 , v2);
		////float s = p->x*( v1->y*v2->z - v1->z*v2->y ) + p->y*( v1->z*v2->x - v1->x*v2->z ) + 
		////		  p->z*( v1->x*v2->y - v1->y*v2->x);
		//float s = v3dxVec3Dot( &vPlaneNormal , p );
		//if (s < 0) 
		//	return 1;
		//else if (s > 0) 
		//	return -1;
		//else 
		//	return 0;
	}

	/**
	   ���߾���ƽ��
	*/
	VFX_API float v3dxPointLineDistance(const v3dVector3_t* p,
							  const v3dVector3_t* l1, const v3dVector3_t* l2,v3dVector3_t* outClosePoint )
	{
		v3dVector3_t W;
		v3dxVec3Sub(&W,p,l1);
		v3dVector3_t L;
		v3dxVec3Sub(&L,l2,l1);
		float fRatio = v3dxVec3Dot(&W,&L) / v3dxVec3Dot(&L,&L);
		v3dVector3_t vtL;
		v3dxVec3Mul(&vtL,fRatio,&L);
		v3dVector3_t p2l ;
		v3dxVec3Sub( &p2l , &W , &vtL);
		//v3dVector3_t p2l = W - L * (W*L)/(L*L);//(W*L)/(L*L)����ʸ��ͼ���Ҵ�������֪�������������Ǵ��㵽lp�ľ�����l2-l1����ı�
					//L*������������������������������������Եõ����ߵ�����
		return v3dxVec3Length( &p2l );//������������
	}

	VFX_API float v3dxPointPlaneDistance(const v3dVector3_t* p, const v3dxPlane3* plane)
	{
		//plane.normalize();
		return v3dxVec3Dot(p,&plane->m_vNormal)+plane->m_fDD;
	}
	/**
	   ����3������ɵ����
	   �������ص�����������������2����ƽ��
	*/
	VFX_API double Area3 (const v3dVector3_t* a, const v3dVector3_t* b,
								 const v3dVector3_t* c)
	{
		//�˴�����������ļ��������Ӧ��
		//û����2�����Գ�������ƽ���ı�������������������������⣬
		//����˵���н��Ǹ��Ƕ�
		//�������������Ҫ��abs * 0.5
		v3dVector3_t v1;
		v3dxVec3Sub( &v1 , b , a );
		v3dVector3_t v2;
		v3dxVec3Sub( &v2 , c , a );
		return ((v1.y * v2.z + v1.z * v2.x + v1.x * v2.y) -
				(v1.y * v2.x + v1.x * v2.z + v1.z * v2.y));
	}

	VFX_API float v3dxCalAngleXZ(const v3dVector3_t* vect)//��Y��
	{
		v3dVector2_t vtProjection;
		vtProjection.x=vect->x;
		vtProjection.y=vect->z;
		v3dxVec2Normalize(&vtProjection,&vtProjection);
		if(vect->z>0.0f)
		{
			return acosf(vtProjection.x);
		}
		else
		{
			return 2*V_PI-acosf(vtProjection.x);
		}
	}

	VFX_API float CalAngleYZ(const v3dVector3_t* vect)//��X��
	{
		v3dVector2_t vtProjection;
		vtProjection.x=vect->y;
		vtProjection.y=-vect->z;
		v3dxVec2Normalize(&vtProjection,&vtProjection);
		if(vect->z<0.0f)
		{
			return acosf(vtProjection.x);
		}
		else
		{
			return 2*V_PI-acosf(vtProjection.x);
		}
	}

	VFX_API float CalAngleXY(const v3dVector3_t* vect)//��Z��
	{
		v3dVector2_t vtProjection;
		vtProjection.x=vect->y;
		vtProjection.y=-vect->x;
		v3dxVec2Normalize(&vtProjection,&vtProjection);
		if(vect->x<0.0f)
		{
			return acosf(vtProjection.x);
		}
		else
		{
			return 2*V_PI-acosf(vtProjection.x);
		}
	}

	VFX_API float v3dxArea3 (const v3dVector3_t *a, const v3dVector3_t *b,
								 const v3dVector3_t *c)
	{
		//�˴�����������ļ��������Ӧ��
		//û����2�����Գ�������ƽ���ı�������������������������⣬
		//����˵���н��Ǹ��Ƕ�
		//�������������Ҫ��abs * 0.5
		v3dxVector3 v1;
		v3dxVec3Sub( &v1 , b , a );
		v3dxVector3 v2;
		v3dxVec3Sub( &v2 , c , a );
		return ((v1.y * v2.z + v1.z * v2.x + v1.x * v2.y) -
				(v1.y * v2.x + v1.x * v2.z + v1.z * v2.y));
	}

	//�����߶κ������εĽ���
	VFX_API vBOOL v3dxLineIntersectPlane(
						 FLOAT *pfT,
						 v3dxVector3 *pvPoint,
						 const v3dxVector3 *pvFrom,
						 const v3dxVector3 *pvLength,					 
						 const v3dxVector3 *pvA,
						 const v3dxVector3 *pvB,
						 const v3dxVector3 *pvC,
						 vBOOL bDoubleFace
						 )
	{
		v3dxVector3 vNormal;

		v3dxVector3 vAB, vBC;
		vAB = *pvB - *pvA;
		vBC = *pvC - *pvB;
		v3dxVec3Cross(&vNormal, &vAB, &vBC);

		v3dxVector3 vPoint;
	
		FLOAT fFromT, fFromA;
	
		fFromT = v3dxVec3Dot(pvLength, &vNormal);
		if( !bDoubleFace && fFromT < 0.f )
			return FALSE; //����
	
		v3dxVector3 vFromA;
		vFromA = *pvA - *pvFrom;
		fFromA = v3dxVec3Dot( &vFromA, &vNormal );
		*pfT = fFromA / fFromT;
	
		if (*pfT < 0.f || *pfT > 1.f)
			return FALSE; // out of range.
	
		*pvPoint = *pvFrom + (*pvLength)*(*pfT);
	
		return TRUE;
	}

	VFX_API vBOOL v3dxLineIntersectTriangle(
							 float *pfT,
							 v3dxVector3 *pvPoint,
							 const v3dxVector3 *pvFrom,
							 const v3dxVector3 *pvLength,
							 const v3dxVector3 *pvA,
							 const v3dxVector3 *pvB,
							 const v3dxVector3 *pvC,
							 const v3dxBox3 *pLineBox,
							 const v3dxBox3 *pTriBox,
							 OUT v3dxVector3 *pvNormal, // = NULL
							 float fPrecision // = 0.0f
							 )
	{
		v3dxBox3 TriBox;
		if( !pTriBox )
		{
			TriBox.OptimalVertex( *pvA );
			TriBox.OptimalVertex( *pvB );
			TriBox.OptimalVertex( *pvC );
			pTriBox = &TriBox;
		}

		v3dxBox3 LineBox;
		if (!pLineBox) {
			v3dxVector3 vTo = *pvFrom + *pvLength;

			LineBox.OptimalVertex( *pvFrom);
			LineBox.OptimalVertex( vTo );
			pLineBox = &LineBox;
		}

		// Do fast reject.
		if (pLineBox->Min().x > pTriBox->Max().x
			|| pLineBox->Min().y > pTriBox->Max().y
			|| pLineBox->Min().z > pTriBox->Max().z
			|| pLineBox->Max().x < pTriBox->Min().x
			|| pLineBox->Max().y < pTriBox->Min().y
			|| pLineBox->Max().z < pTriBox->Min().z)
			return FALSE;

		// Generate normal if not inputed.
	
		v3dVector3_t vAB, vBC;
		v3dxVec3Sub( &vAB , pvB , pvA );
		v3dxVec3Sub( &vBC , pvC , pvB );

		v3dxVector3 vNormal;
		v3dxVec3Cross(&vNormal, &vAB, &vBC);
		if (pvNormal) {
			*pvNormal = *(v3dxVector3*)(&vNormal);
			pvNormal->normalize();
		}
		else{
			pvNormal = (v3dxVector3*)(&vNormal);
			pvNormal->normalize();
		}

		FLOAT fFromT, fFromA;

		fFromT = v3dxVec3Dot(pvLength, pvNormal);
		if( fFromT > 0.f )
			return FALSE;//����

		v3dVector3_t vFromA;
		v3dxVec3Sub( &vFromA , pvA , pvFrom );
		fFromA = v3dxVec3Dot( &vFromA , pvNormal );
		*pfT = fFromA / fFromT;
	
		if (*pfT < 0.f || *pfT > 1.f)
			return FALSE; // out of range.

		*pvPoint = *pvFrom + (*pvLength)*(*pfT);

		enum {PROJECT_X, PROJECT_Y, PROJECT_Z} enProjDir;
	
		float sx, sy, sz;
		sx = vAB.y*vBC.z - vAB.z*vBC.y;
		sy = vAB.x*vBC.z - vAB.z*vBC.x;
		sz = vAB.x*vBC.y - vAB.y*vBC.x;
		sx = sx < 0.f ? -sx : sx;
		sy = sy < 0.f ? -sy : sy;
		sz = sz < 0.f ? -sz : sz;
	
		if (sx >= sy && sx >= sz) 
			enProjDir = PROJECT_X;
		else if (sy >= sx && sy >= sz) 
			enProjDir = PROJECT_Y;
		else 
			enProjDir = PROJECT_Z;

		float x0, y0, x1, y1, x2, y2;
		switch(enProjDir)
		{
		case PROJECT_Z:
			x0 = pvA->x - pvPoint->x; y0 = pvA->y - pvPoint->y;
			x1 = pvB->x - pvPoint->x; y1 = pvB->y - pvPoint->y;
			x2 = pvC->x - pvPoint->x; y2 = pvC->y - pvPoint->y;
			break;
		case PROJECT_X:
			x0 = pvA->z - pvPoint->z; y0 = pvA->y - pvPoint->y;
			x1 = pvB->z - pvPoint->z; y1 = pvB->y - pvPoint->y;
			x2 = pvC->z - pvPoint->z; y2 = pvC->y - pvPoint->y;
			break;
		default: // case PROJECT_Y:
			y0 = pvA->x - pvPoint->x; x0 = pvA->z - pvPoint->z;
			y1 = pvB->x - pvPoint->x; x1 = pvB->z - pvPoint->z;
			y2 = pvC->x - pvPoint->x; x2 = pvC->z - pvPoint->z;
		}
	
		float t1, t2, t3;
		t1 = x0*y1-x1*y0;
		t2 = x2*y0-x0*y2;
		t3 = x1*y2-x2*y1;
		return (t1 > -fPrecision && t2 >= -fPrecision && t3 >= -fPrecision)
			|| (t1 <= fPrecision && t2 <= fPrecision && t3 <= fPrecision);
	}

	VFX_API vBOOL v3dxLineIntersectTriangleEx(
		float *pfT,
		v3dxVector3 *pvPoint,
		v3dxVector3 *pvNormal,
		const v3dxVector3 *pvFrom,
		const v3dxVector3 *pvLength,
		const v3dxVector3 *pvA,
		const v3dxVector3 *pvB,
		const v3dxVector3 *pvC,
		const v3dxBox3 *pLineBox,
		const v3dxBox3 *pTriBox,
		float fPrecision // = 0.0f
		)
	{
		v3dxBox3 TriBox;
		if( !pTriBox )
		{
			TriBox.OptimalVertex( *pvA );
			TriBox.OptimalVertex( *pvB );
			TriBox.OptimalVertex( *pvC );
			pTriBox = &TriBox;
		}

		v3dxBox3 LineBox;
		if (!pLineBox) {
			v3dxVector3 vTo = *pvFrom + *pvLength;

			LineBox.OptimalVertex( *pvFrom);
			LineBox.OptimalVertex( vTo );
			pLineBox = &LineBox;
		}

		// Do fast reject.
		if (pLineBox->Min().x > pTriBox->Max().x
			|| pLineBox->Min().y > pTriBox->Max().y
			|| pLineBox->Min().z > pTriBox->Max().z
			|| pLineBox->Max().x < pTriBox->Min().x
			|| pLineBox->Max().y < pTriBox->Min().y
			|| pLineBox->Max().z < pTriBox->Min().z)
			return FALSE;

		// Generate normal if not inputed.

		v3dVector3_t vAB, vBC;
		v3dxVec3Sub( &vAB , pvB , pvA );
		v3dxVec3Sub( &vBC , pvC , pvB );

		v3dxVector3 vNormal;
		v3dxVec3Cross(&vNormal, &vAB, &vBC);
		vNormal.normalize();

		if (pvNormal) {
			*pvNormal = vNormal;
		}
		else
		{
			pvNormal = &vNormal;
		}

		FLOAT fFromT, fFromA;

		fFromT = v3dxVec3Dot(pvLength, pvNormal);
		if( fFromT > 0.f )
			return FALSE;//����

		v3dVector3_t vFromA;
		v3dxVec3Sub( &vFromA , pvA , pvFrom );
		fFromA = v3dxVec3Dot( &vFromA , pvNormal );
		*pfT = fFromA / fFromT;

		if (*pfT < 0.f || *pfT > 1.f)
			return FALSE; // out of range.

		*pvPoint = *pvFrom + (*pvLength)*(*pfT);

		enum {PROJECT_X, PROJECT_Y, PROJECT_Z} enProjDir;

		float sx, sy, sz;
		sx = vAB.y*vBC.z - vAB.z*vBC.y;
		sy = vAB.x*vBC.z - vAB.z*vBC.x;
		sz = vAB.x*vBC.y - vAB.y*vBC.x;
		sx = sx < 0.f ? -sx : sx;
		sy = sy < 0.f ? -sy : sy;
		sz = sz < 0.f ? -sz : sz;

		if (sx >= sy && sx >= sz) 
			enProjDir = PROJECT_X;
		else if (sy >= sx && sy >= sz) 
			enProjDir = PROJECT_Y;
		else 
			enProjDir = PROJECT_Z;

		float x0, y0, x1, y1, x2, y2;
		switch(enProjDir)
		{
		case PROJECT_Z:
			x0 = pvA->x - pvPoint->x; y0 = pvA->y - pvPoint->y;
			x1 = pvB->x - pvPoint->x; y1 = pvB->y - pvPoint->y;
			x2 = pvC->x - pvPoint->x; y2 = pvC->y - pvPoint->y;
			break;
		case PROJECT_X:
			x0 = pvA->z - pvPoint->z; y0 = pvA->y - pvPoint->y;
			x1 = pvB->z - pvPoint->z; y1 = pvB->y - pvPoint->y;
			x2 = pvC->z - pvPoint->z; y2 = pvC->y - pvPoint->y;
			break;
		default: // case PROJECT_Y:
			y0 = pvA->x - pvPoint->x; x0 = pvA->z - pvPoint->z;
			y1 = pvB->x - pvPoint->x; x1 = pvB->z - pvPoint->z;
			y2 = pvC->x - pvPoint->x; x2 = pvC->z - pvPoint->z;
		}

		float t1, t2, t3;
		t1 = x0*y1-x1*y0;
		t2 = x2*y0-x0*y2;
		t3 = x1*y2-x2*y1;
		return (t1 > -fPrecision && t2 >= -fPrecision && t3 >= -fPrecision)
			|| (t1 <= fPrecision && t2 <= fPrecision && t3 <= fPrecision);
	}

	VFX_API vBOOL v3dxLineIntersectBox3( float *pfT_n,
							 v3dxVector3 *pvPoint_n,
							 float *pfT_f,
							 v3dxVector3 *pvPoint_f,
							 const v3dxVector3 *pvFrom,
							 const v3dxVector3 *pvDir,
							 const v3dxBox3* pBox
							 )
	{
		v3dxVector3 vDir;
		v3dxVec3Normalize( &vDir , pvDir );
		const v3dxVector3& vMin = pBox->Min();
		const v3dxVector3& vMax = pBox->Max();
	
		int nIntersect = 0;
		struct InSectPos{
			v3dxVector3 vPos;
			float		f;
		};
		InSectPos vRet[2];

		//Proj_x
		v3dxVector3 vP1,vP2;
		float fT1 = (vMin.x - pvFrom->x)/vDir.x;
		float fT2 = (vMax.x - pvFrom->x)/vDir.x;
		vP1.x = vMin.x;
		vP1.y = pvFrom->y + vDir.y*fT1;
		vP1.z = pvFrom->z + vDir.z*fT1;

		vP2.x = vMax.x;
		vP2.y = pvFrom->y + vDir.y*fT2;
		vP2.z = pvFrom->z + vDir.z*fT2;

		if( ( vP1.y > vMax.y && vP2.y > vMax.y )||( vP1.y < vMin.y && vP2.y < vMin.y ) ||
			( vP1.z > vMax.z && vP2.z > vMax.z )||( vP1.z < vMin.z && vP2.z < vMin.z ) )
		{
			return FALSE;
		}
		else
		{
			if( vP1.y < vMax.y && vP1.y > vMin.y && vP1.z < vMax.z && vP1.z > vMin.z )
			{
				vRet[nIntersect].vPos = vP1;
				vRet[nIntersect].f = fT1;
				nIntersect++;
			}
			if( vP2.y < vMax.y && vP2.y > vMin.y && vP2.z < vMax.z && vP2.z > vMin.z )
			{
				vRet[nIntersect].vPos = vP2;
				vRet[nIntersect].f = fT2;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}
				return TRUE;
			}
		}

		//Proj_y
		fT1 = (vMin.y - pvFrom->y)/vDir.y;
		fT2 = (vMax.y - pvFrom->y)/vDir.y;
		vP1.x = pvFrom->x + vDir.x*fT1;
		vP1.y = vMin.y;
		vP1.z = pvFrom->z + vDir.z*fT1;

		vP2.x = pvFrom->x + vDir.x*fT2;
		vP2.y = vMax.y;
		vP2.z = pvFrom->z + vDir.z*fT2;

		if( ( vP1.x > vMax.x && vP2.x > vMax.x )||( vP1.x < vMin.x && vP2.x < vMin.x ) ||
			( vP1.z > vMax.z && vP2.z > vMax.z )||( vP1.z < vMin.z && vP2.z < vMin.z ) )
		{
			return FALSE;
		}
		else
		{
			if( vP1.x < vMax.x && vP1.x > vMin.x && vP1.z < vMax.z && vP1.z > vMin.z )
			{
				vRet[nIntersect].vPos = vP1;
				vRet[nIntersect].f = fT1;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}
				return TRUE;
			}
			if( vP2.x < vMax.x && vP2.x > vMin.x && vP2.z < vMax.z && vP2.z > vMin.z )
			{
				vRet[nIntersect].vPos = vP2;
				vRet[nIntersect].f = fT2;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}
				return TRUE;
			}
		}

		//Proj_z
		fT1 = (vMin.z - pvFrom->z)/vDir.z;
		fT2 = (vMax.z - pvFrom->z)/vDir.z;
		vP1.x = pvFrom->x + vDir.x*fT1;
		vP1.y = pvFrom->y + vDir.y*fT1;
		vP1.z = vMin.z;

		vP2.x = pvFrom->x + vDir.x*fT2;
		vP2.y = pvFrom->y + vDir.y*fT2;
		vP2.z = vMax.z;

		if( ( vP1.x > vMax.x && vP2.x > vMax.x )||( vP1.x < vMin.x && vP2.x < vMin.x ) ||
			( vP1.y > vMax.y && vP2.y > vMax.y )||( vP1.y < vMin.y && vP2.y < vMin.y ) )
		{
			return FALSE;
		}
		else
		{
			if( vP1.x < vMax.x && vP1.x > vMin.x && vP1.y < vMax.y && vP1.y > vMin.y )
			{
				vRet[nIntersect].vPos = vP1;
				vRet[nIntersect].f = fT1;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}
				return TRUE;
			}
			if( vP2.x < vMax.x && vP2.x > vMin.x && vP2.y < vMax.y && vP2.y > vMin.y )
			{
				vRet[nIntersect].vPos = vP2;
				vRet[nIntersect].f = fT2;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}
				return TRUE;
			}
		}

		return FALSE;
	}



	VFX_API vBOOL v3dxLineIntersectBox3_v2( float *pfT_n,
										 v3dxVector3 *pvPoint_n,
										 float *pfT_f,
										 v3dxVector3 *pvPoint_f,
										 v3dxVector3 *pvNormal,
										 const v3dxVector3 *pvFrom,
										 const v3dxVector3 *pvDir,
										 const v3dxBox3* pBox
										 )
	{
		v3dxVector3 vDir;
		v3dxVec3Normalize( &vDir , pvDir );
		const v3dxVector3& vMin = pBox->Min();
		const v3dxVector3& vMax = pBox->Max();

		int nIntersect = 0;
		struct InSectPos{
			v3dxVector3 vPos;
			float		f;
		};
		InSectPos vRet[2];

		//Proj_x
		v3dxVector3 vP1,vP2;
		float fT1 = (vMin.x - pvFrom->x)/vDir.x;
		float fT2 = (vMax.x - pvFrom->x)/vDir.x;
		vP1.x = vMin.x;
		vP1.y = pvFrom->y + vDir.y*fT1;
		vP1.z = pvFrom->z + vDir.z*fT1;

		vP2.x = vMax.x;
		vP2.y = pvFrom->y + vDir.y*fT2;
		vP2.z = pvFrom->z + vDir.z*fT2;

		if( ( vP1.y > vMax.y && vP2.y > vMax.y )||( vP1.y < vMin.y && vP2.y < vMin.y ) ||
			( vP1.z > vMax.z && vP2.z > vMax.z )||( vP1.z < vMin.z && vP2.z < vMin.z ) )
		{
			return FALSE;
		}
		else
		{
			if( vP1.y < vMax.y && vP1.y > vMin.y && vP1.z < vMax.z && vP1.z > vMin.z )
			{
				vRet[nIntersect].vPos = vP1;
				vRet[nIntersect].f = fT1;
				nIntersect++;
			}
			if( vP2.y < vMax.y && vP2.y > vMin.y && vP2.z < vMax.z && vP2.z > vMin.z )
			{
				vRet[nIntersect].vPos = vP2;
				vRet[nIntersect].f = fT2;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}

				// normal 
				if( pvPoint_n->x < pvPoint_f->x )
				{
					*pvNormal = -v3dxVector3::UNIT_X;
				}
				else
				{
					*pvNormal = v3dxVector3::UNIT_X;
				}

				return TRUE;
			}
		}

		//Proj_y
		fT1 = (vMin.y - pvFrom->y)/vDir.y;
		fT2 = (vMax.y - pvFrom->y)/vDir.y;
		vP1.x = pvFrom->x + vDir.x*fT1;
		vP1.y = vMin.y;
		vP1.z = pvFrom->z + vDir.z*fT1;

		vP2.x = pvFrom->x + vDir.x*fT2;
		vP2.y = vMax.y;
		vP2.z = pvFrom->z + vDir.z*fT2;

		if( ( vP1.x > vMax.x && vP2.x > vMax.x )||( vP1.x < vMin.x && vP2.x < vMin.x ) ||
			( vP1.z > vMax.z && vP2.z > vMax.z )||( vP1.z < vMin.z && vP2.z < vMin.z ) )
		{
			return FALSE;
		}
		else
		{
			if( vP1.x < vMax.x && vP1.x > vMin.x && vP1.z < vMax.z && vP1.z > vMin.z )
			{
				vRet[nIntersect].vPos = vP1;
				vRet[nIntersect].f = fT1;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}


				// normal 
				if( pvPoint_n->y < pvPoint_f->y )
				{
					*pvNormal = -v3dxVector3::UNIT_Y;
				}
				else
				{
					*pvNormal = v3dxVector3::UNIT_Y;
				}



				return TRUE;
			}
			if( vP2.x < vMax.x && vP2.x > vMin.x && vP2.z < vMax.z && vP2.z > vMin.z )
			{
				vRet[nIntersect].vPos = vP2;
				vRet[nIntersect].f = fT2;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}

				// normal 
				if( pvPoint_n->y < pvPoint_f->y )
				{
					*pvNormal = -v3dxVector3::UNIT_Y;
				}
				else
				{
					*pvNormal = v3dxVector3::UNIT_Y;
				}


				return TRUE;
			}
		}

		//Proj_z
		fT1 = (vMin.z - pvFrom->z)/vDir.z;
		fT2 = (vMax.z - pvFrom->z)/vDir.z;
		vP1.x = pvFrom->x + vDir.x*fT1;
		vP1.y = pvFrom->y + vDir.y*fT1;
		vP1.z = vMin.z;

		vP2.x = pvFrom->x + vDir.x*fT2;
		vP2.y = pvFrom->y + vDir.y*fT2;
		vP2.z = vMax.z;

		if( ( vP1.x > vMax.x && vP2.x > vMax.x )||( vP1.x < vMin.x && vP2.x < vMin.x ) ||
			( vP1.y > vMax.y && vP2.y > vMax.y )||( vP1.y < vMin.y && vP2.y < vMin.y ) )
		{
			return FALSE;
		}
		else
		{
			if( vP1.x < vMax.x && vP1.x > vMin.x && vP1.y < vMax.y && vP1.y > vMin.y )
			{
				vRet[nIntersect].vPos = vP1;
				vRet[nIntersect].f = fT1;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}


				// normal 
				if( pvPoint_n->z < pvPoint_f->z )
				{
					*pvNormal = -v3dxVector3::UNIT_Z;
				}
				else
				{
					*pvNormal = v3dxVector3::UNIT_Z;
				}

				return TRUE;
			}
			if( vP2.x < vMax.x && vP2.x > vMin.x && vP2.y < vMax.y && vP2.y > vMin.y )
			{
				vRet[nIntersect].vPos = vP2;
				vRet[nIntersect].f = fT2;
				nIntersect++;
			}
			if( nIntersect==2 )
			{
				if( vRet[0].f > vRet[1].f )
				{
					*pvPoint_n = vRet[1].vPos;
					*pfT_n = vRet[1].f;
					*pvPoint_f = vRet[0].vPos;
					*pfT_f = vRet[0].f;
				}
				else
				{
					*pvPoint_n = vRet[0].vPos;
					*pfT_n = vRet[0].f;
					*pvPoint_f = vRet[1].vPos;
					*pfT_f = vRet[1].f;
				}

				// normal 
				if( pvPoint_n->z < pvPoint_f->z )
				{
					*pvNormal = -v3dxVector3::UNIT_Z;
				}
				else
				{
					*pvNormal = v3dxVector3::UNIT_Z;
				}

				return TRUE;
			}
		}

		return FALSE;
	}


	/**
		�����߶κ������εĽ��㣬ʵ�õ�witchside����
	*/
	VFX_API vBOOL v3dxSegIntersectTriangleD( const v3dxVector3& tr1,
  							const v3dxVector3& tr2, 
							const v3dxVector3& tr3,
							const v3dxSegment3& seg, 
							v3dxVector3& isect)
	{
		v3dxPlane3 plane(tr1,tr2,tr3);
		float dist;
		if( v3dxLineIntersectPlane( &dist , &isect , &seg.getStart() , &seg.getLength() , 
			&tr1 , &tr2 , &tr3 , TRUE ) == FALSE )
			return FALSE;
	
		if ( v3dxWhichSide3D_v2 ( &isect, &tr3, &tr1 ) > 0 ) 
			return FALSE;
		if ( v3dxWhichSide3D_v2( &isect, &tr1, &tr2 ) > 0 )
			return FALSE;
		if ( v3dxWhichSide3D_v2( &isect, &tr2, &tr3 ) > 0 )
			return FALSE;
		return TRUE;
	}

	/**
		���һ��3D�߶κ�һ��ƽ���ཻ�������һ�����㣬����true
		���㷵�ص�isect��
		��u������ľ��뷵�ص�dist��������뾭���˹�һ����
		����0.5��ʾ��u��v���е�
	*/
	VFX_API vBOOL v3dxLineIntersectPlane_v1( const v3dxVector3& u,
								 const v3dxVector3& v,
								 float A, float B, float C, float D,
								 v3dxVector3& isect, float& dist)
	{
		float x,y,z, denom;

		x = v.x-u.x;  y = v.y-u.y;  z = v.z-u.z;//x,y,z������vu����
		denom = A*x + B*y + C*z;//ֱ����ƽ�淨�������|UV|*|NORM|*cos(alpha)��
								//��ΪA,B,C�����˵�λ������|NORM|=1��
								//denom����|UV|*cos(alpha)�����˵����ֱ�����ƽ�����б�̶�
								//denomҲ��ֱ����ƽ����ͶӰ���ȵ��Ǹ������εĶԱߣ��ٺ٣���ͼ�����ף�
								//Ҳ���Ǵ��߷����ͶӰ����
		if (std::abs (denom) < SMALL_EPSILON) 
			return FALSE; // sinֵ��С��˵��ֱ��ƽ����ƽ��

		dist = -(A*u.x + B*u.y + C*u.z + D) / denom;//ǰ���������U��ƽ����룬�����������ֵ�ͳ�����
		if (dist < -SMALL_EPSILON || dist > 1+SMALL_EPSILON) //��ֵ̫С��̫���ǿ������ཻ
			return FALSE;

		isect.x = u.x + dist*x;  isect.y = u.y + dist*y;  isect.z = u.z + dist*z;//����ֱ�߷��̼��㽻���
		return TRUE;
	}

	VFX_API vBOOL v3dxLineIntersectPlane_v2( const v3dxVector3& u,
								 const v3dxVector3& v,
								 const v3dxPlane3& p, 
								 v3dxVector3& isect, 
								 float& dist)
	{
		float x,y,z, denom;

		x = v.x-u.x;  y = v.y-u.y;  z = v.z-u.z;
		denom = p.m_vNormal.x*x + p.m_vNormal.y*y + p.m_vNormal.z*z;
		if (std::abs(denom) < SMALL_EPSILON)
			return FALSE; // ƽ��

		dist = -(p.m_vNormal.dotProduct(u) + p.m_fDD) / denom;//u�㵽ƽ��ľ��� ���� ����ƽ��н����� �õ� ֱ���Ͻ��㵹U��ľ���
		if (dist < -SMALL_EPSILON || dist > 1+SMALL_EPSILON) 
			return FALSE;

		isect.x = u.x + dist*x;  isect.y = u.y + dist*y;  isect.z = u.z + dist*z;
		return TRUE;
	}

	/**
		��������poly��ƽ��plane�Ľ��㣬���û�н��㣬����false���еĻ�segment
		���ǽ��ߣ���segment��start��end��ͬ����ʾֻ��һ������
	*/
	VFX_API vBOOL v3dxPlaneIntersectPolygon( const v3dxPlane3& plane, v3dxPoly3* poly,
  							v3dxSegment3& seg)
	{
		v3dxVector3& v1 = seg.getStart ();
		v3dxVector3& v2 = seg.getEnd ();
		int i, i1;
		float c, c1;
		v3dxVector3 isect;
		float dist;
		i1 = poly->getNumVertices()-1;//�õ�����εı���
		c1 = plane.classify ((*poly)[i1]);//�������㵽ƽ�����
		vBOOL found_v1 = FALSE;
		vBOOL found_v2 = FALSE;
		for (i = 0 ; i < poly->getNumVertices () ; i++)
		{
			c = plane.classify ((*poly)[i]);
			if ((c < 0 && c1 > 0) || (c1 < 0 && c > 0))//�ֱ���ƽ�������
			{
				v3dxLineIntersectPlane_v2( (*poly)[i1], (*poly)[i],
      					plane, isect, dist );//���������߶���ƽ��Ľ���
				if (!found_v1) 
				{ 
					v1 = isect; 
					found_v1 = TRUE; //�ҵ���һ������
				}
				else 
				{ 
					v2 = isect; 
					found_v2 = TRUE; //�ҵ��ڶ������㣬͹��������2������
					break; 
				}
			}
			i1 = i;
			c1 = c;
		}
		if (!found_v1) 
			return FALSE;//û��һ������
		if (!found_v2) 
			v2 = v1;//ֻ��һ������������������Ŷ����
		return TRUE;
	}

	VFX_API void	v3dxAABBVertexEdges(const v3dxVector3& aabb_min, const v3dxVector3& aabb_max, v3dxVector3 v[8], int e[12][2])
	{
		// ��4
		v[0] = v3dxVector3( aabb_min.x, aabb_max.y, aabb_min.z);
		v[1] = v3dxVector3( aabb_max.x, aabb_max.y, aabb_min.z);
		v[2] = v3dxVector3( aabb_max.x, aabb_max.y, aabb_max.z);
		v[3] = v3dxVector3( aabb_min.x, aabb_max.y, aabb_max.z);
		// ��4����Ӧ��
		v[4] = v3dxVector3( aabb_min.x, aabb_min.y, aabb_min.z);
		v[5] = v3dxVector3( aabb_max.x, aabb_min.y, aabb_min.z);
		v[6] = v3dxVector3( aabb_max.x, aabb_min.y, aabb_max.z);
		v[7] = v3dxVector3( aabb_min.x, aabb_min.y, aabb_max.z);

		// ��4
		e[0][0] = 0;	e[0][1] = 1;
		e[1][0] = 1;	e[1][1] = 2;
		e[2][0] = 2;	e[2][1] = 3;
		e[3][0] = 3;	e[3][1] = 0;
		// ��4
		e[4][0] = 4;	e[4][1] = 5;
		e[5][0] = 5;	e[5][1] = 6;
		e[6][0] = 6;	e[6][1] = 7;
		e[7][0] = 7;	e[7][1] = 4;
		// ��4
		e[8][0] = 0;	e[8][1] = 4;
		e[9][0] = 1;	e[9][1] = 5;
		e[10][0]= 2;	e[10][1]= 6;
		e[11][0]= 3;	e[11][1]= 7;
	}

	VFX_API void	v3dxAABBInnerPlanes(const v3dxVector3& aabb_min, const v3dxVector3& aabb_max, v3dxPlane3 p[6])
	{
		/*v3dxVector3	v[8];
		int		e[12][2];
		v3dxAABBVertexEdges(aabb_min, aabb_max, v, e);
		p[0].set(v[1], v[0], v[5]);
		p[1].set(v[2], v[0], v[6]);
		p[2].set(v[3], v[2], v[7]);
		p[3].set(v[0], v[3], v[8]);
		p[4].set(v[0], v[1], v[2]);
		p[5].set(v[5], v[8], v[7]);*/
	}

	VFX_API void	v3dxZPolyCWInnerPlanes(v3dxVector3* pPoly, int Length, float MaxY, float MinY, std::vector<v3dxPlane3>& OutResult)
	{
		if (Length < 3)
		{
			// С�������ܽ���Polygon
			return;
		}
		// �Ϸ���
		v3dxVector3* pVerticesUp = new v3dxVector3[Length];
		for (int i = 0 ; i < Length ; ++i)
		{
			pVerticesUp[i] = pPoly[i];
			pVerticesUp[i].y = MaxY;
		}
		// �·���
		v3dxVector3* pVerticesDown = new v3dxVector3[Length];
		for (int i = 0 ; i < Length ; ++i)
		{
			pVerticesDown[i] = pPoly[i];
			pVerticesDown[i].y = MinY;
		}
		// ������Polygon������+2�������£�
		OutResult.reserve(Length+2);
		OutResult.resize(Length+2);
		for (int i = 0 ; i < Length-1 ; ++i)
		{
			OutResult[i].set(pVerticesUp[i], pVerticesUp[i+1], pVerticesDown[i]);
		}
		// ���
		OutResult[Length-1].set(pVerticesUp[Length-1], pVerticesUp[0], pVerticesDown[Length-1]);
		// ����
		OutResult[Length].set(pVerticesUp[0], pVerticesUp[1], pVerticesUp[2]);
		OutResult[Length+1].set(pVerticesDown[0], pVerticesDown[1], pVerticesDown[2]);

		delete[] pVerticesUp;
		delete[] pVerticesDown;
	}

	//	This function uses the following formula to compute the returned matrix.
	//	P = normalize(Plane);
	//	-2 * P.a * P.a + 1  -2 * P.b * P.a      -2 * P.c * P.a        0
	//	-2 * P.a * P.b      -2 * P.b * P.b + 1  -2 * P.c * P.b        0
	//	-2 * P.a * P.c      -2 * P.b * P.c      -2 * P.c * P.c + 1    0
	//	-2 * P.a * P.d      -2 * P.b * P.d      -2 * P.c * P.d        1
	VFX_API void v3dxReflect(v3dxMatrix4* m,const v3dxPlane3* plane)
	{
		v3dxMatrixReflect( m , plane );
	} 

	//	This function uses the following formula to compute the returned matrix.
	//	P = normalize(Plane);
	//	L = Light;
	//	d = dot(P, L)
	//	P.a * L.x + d  P.a * L.y      P.a * L.z      P.a * L.w  
	//	P.b * L.x      P.b * L.y + d  P.b * L.z      P.b * L.w  
	//	P.c * L.x      P.c * L.y      P.c * L.z + d  P.c * L.w  
	//	P.d * L.x      P.d * L.y      P.d * L.z      P.d * L.w + d

	VFX_API void v3dxShadow(v3dMatrix4_t* m,const v3dVector4_t* l, const v3dxPlane3* plane)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		D3DXMatrixShadow( (D3DXMATRIX*)m , (D3DXVECTOR4*)l , (D3DXPLANE*)plane );
	#else
		v3dxPlane3 p = *plane;
		//v3dxPlane3 p;
		//v3dxVec4Normalize((v3dColor4_t *)&p,(v3dColor4_t *)plane);
		p.normalize();
		float d = v3dxVec4Dot((v3dVector4_t *)&p,l);
		m->m11 = p.m_vNormal.x * l->x + d; m->m12 = p.m_vNormal.x * l->y;    m->m13 = p.m_vNormal.x * l->z;    m->m14 = p.m_vNormal.x * l->w;
		m->m21 = p.m_vNormal.y * l->x    ;m->m22 = p.m_vNormal.y * l->y + d;m->m23 = p.m_vNormal.y * l->z;    m->m24 = p.m_vNormal.y * l->w;
		m->m31 = p.m_vNormal.z * l->x    ;m->m32 = p.m_vNormal.z * l->y;    m->m33 = p.m_vNormal.z * l->z + d;m->m34 = p.m_vNormal.z * l->w;
		m->m41 = p.m_fDD * l->x    ;m->m42 = p.m_fDD * l->y;    m->m43 = p.m_fDD * l->z;    m->m44 = p.m_fDD * l->w + d;
	#endif
	}

	//ƽ�� N * p + D = 0
	//ֱ�� p(t) = Q + t * V
	//=> N * p(t) + D = 0
	//=> N * Q + t * N * V + D = 0
	//=> t = -(N * Q + D) / (N * V)
	//=> ����p(t) = Q + t * V�õ�����p(t)
	VFX_API vBOOL IntersectPlaneLine(v3dVector3_t & o,const v3dxPlane3 & p,const v3dxLine3 & l)
	{
		float NV = v3dxVec3Dot(&p.m_vNormal,&l.m_direct);
		if(NV == 0)
			return FALSE;
		float t = -(p.D() + v3dxVec3Dot(&p.m_vNormal,&l.m_point)) / NV;
		v3dxVec3Mul(&o,t,&l.m_direct);
		v3dxVec3Add(&o,&o,&l.m_point);
		return TRUE;
	}

	VFX_API vBOOL IntersectPlane3(v3dVector3_t & q,const v3dxPlane3 & p1,const v3dxPlane3 & p2,const v3dxPlane3 & p3)
	{
		v3dxMatrix4 m(
			p1.A(),p1.B(),p1.C(),0.0f,
			p2.A(),p2.B(),p2.C(),0.0f,
			p3.A(),p3.B(),p3.C(),0.0f,
			0.0f,0.0f,0.0f,1.0f);

		if(NULL == v3dxMatrix4Inverse(&m, &m,NULL))
			return FALSE;
		q.x = - m.m11 * p1.D() - m.m12 * p2.D() - m.m13 * p3.D();
		q.y = - m.m21 * p1.D() - m.m22 * p2.D() - m.m23 * p3.D();
		q.z = - m.m31 * p1.D() - m.m32 * p2.D() - m.m33 * p3.D();

		return TRUE;
	}

	VFX_API vBOOL IntersectPlane2(v3dxLine3 & l,const v3dxPlane3 & p1,const v3dxPlane3 & p2)
	{
		v3dxPlane3 t;
		v3dxVec3Cross(&t.m_vNormal,&p1.m_vNormal,&p2.m_vNormal);
		if(t.A() == 0 && t.B() == 0 && t.C() == 0)
			return FALSE;

		t.m_fDD = 0.0f;
		IntersectPlane3(l.m_point,p1,p2,t);
		l.m_direct = t.m_vNormal;

		return TRUE;
	}

	VFX_API vBOOL VerticelLine(v3dxLine3 & l,const v3dxLine3 & l1,const v3dxLine3 & l2)
	{
		//���߷���
		v3dxVec3Cross(&l.m_direct,&l1.m_direct,&l2.m_direct);
		if(l.m_direct.x == 0 && l.m_direct.y == 0 && l.m_direct.z == 0)
			return FALSE;

		//���ߺ�l1���Թ���һ��ƽ��
		v3dxVector3 n;
		v3dxVec3Cross(&n,&l.m_direct,&l1.m_direct);
		v3dxPlane3 p(n,l1.m_point);

		//���l2�ʹ���Ľ�����Ϊ�ߵ����
		return IntersectPlaneLine(l.m_point,p,l2);
	}

	VFX_API FLOAT DistanceLine2(const v3dxLine3* l1, const v3dxLine3* l2, v3dxVector3* outPointOnL1, v3dxVector3* outPointOnL2)
	{
		//���߷���
		v3dxVector3 dir;
		v3dxVec3Cross(&dir,&l1->m_direct,&l2->m_direct);
		if( dir.getLengthSq()<EPSILON )  // Cheng �������ƺ���Ӧ���ж���ȣ�����޸�
			/*		if(dir.x == 0 && dir.y == 0 && dir.z == 0)*/
		{//����ֱ��ƽ��
			v3dxPlane3 p(l1->m_direct,l1->m_point);
			v3dxVector3 v;
			IntersectPlaneLine(v,p,*l2);
			return v3dxVec3Length(&v);
		}
		else
		{
			//���ߺ�l1���Թ���һ��ƽ��
			v3dxVector3 v1;
			v3dxVec3Cross(&v1,&dir,&l1->m_direct);
			v3dxPlane3 p1(v1,l1->m_point);
			IntersectPlaneLine(v1,p1,*l2); // Cheng Ӧ����l2������l1
			if( outPointOnL2 )    // Cheng Ϊ��ӵĽӿ����ֵ
				*outPointOnL2 = v1;

			//���ߺ�l2���Թ���һ��ƽ��
			v3dxVector3 v2;
			v3dxVec3Cross(&v2,&dir,&l2->m_direct);
			v3dxPlane3 p2(v2,l2->m_point);
			IntersectPlaneLine(v2,p2,*l1);
			if( outPointOnL1 )
				*outPointOnL1 = v2;

			v3dxVec3Sub(&v1,&v2,&v1);
			return v3dxVec3Length(&v1);
		}
	}

	//inline v3dxQuaternion Lerp(const v3dxQuaternion& q1, const v3dxQuaternion& q2, float t)
	//{
	//	v3dxQuaternion tmpQuat;
	//	// if (dot < 0), q1 and q2 are more than 360 deg apart.
	//	// The problem is that quaternions are 720deg of freedom.
	//	// so we - all components when lerping
	//	if (Dot(q1, q2) < 0.0F)
	//	{
	//		tmpQuat.set(q1.x + t * (-q2.x - q1.x),
	//			q1.y + t * (-q2.y - q1.y),
	//			q1.z + t * (-q2.z - q1.z),
	//			q1.w + t * (-q2.w - q1.w));
	//	}
	//	else
	//	{
	//		tmpQuat.set(q1.x + t * (q2.x - q1.x),
	//			q1.y + t * (q2.y - q1.y),
	//			q1.z + t * (q2.z - q1.z),
	//			q1.w + t * (q2.w - q1.w));
	//	}
	//	return Normalize(tmpQuat);
	//}

	VFX_API v3dxQuaternion* v3dxQuaternionSlerp(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ1, CONST v3dxQuaternion *pQ2, float t)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxQuaternion*)D3DXQuaternionSlerp((D3DXQUATERNION*)pOut, (const D3DXQUATERNION*)pQ1, (const D3DXQUATERNION*)pQ2, t);
	#else
		const v3dxQuaternion& q1 = *pQ1;
		const v3dxQuaternion& q2 = *pQ2;
		
		float opposite;
		float inverse;
		float dot = (q1.x * q2.x) + (q1.y * q2.y) + (q1.z * q2.z) + (q1.w * q2.w);
		bool flag = false;

		if (dot < 0.0f)
		{
			flag = true;
			dot = -dot;
		}

		if (dot > 0.999999f)
		{
			inverse = 1.0f - t;
			opposite = flag ? -t : t;
		}
		else
		{
			float acos = (float)(Math::ACos(dot));
			float invSin = (float)((1.0f / Math::Sin(acos)));

			inverse = ((float)(Math::Sin((1.0f - t) * acos))) * invSin;
			opposite = flag ? (((float)(-Math::Sin(t * acos))) * invSin) : (((float)(Math::Sin(t * acos))) * invSin);
		}

		pOut->x = (inverse * q1.x) + (opposite * q2.x);
		pOut->y = (inverse * q1.y) + (opposite * q2.y);
		pOut->z = (inverse * q1.z) + (opposite * q2.z);
		pOut->w = (inverse * q1.w) + (opposite * q2.w);

		return pOut;

	#endif
	}

	VFX_API v3dxQuaternion* v3dxQuaternionRotationAxis(v3dxQuaternion *pOut, CONST v3dxVector3 *axis, float Angle)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxQuaternion*)D3DXQuaternionRotationAxis((D3DXQUATERNION *)pOut, (const D3DXVECTOR3*)axis, Angle);
	#else
		v3dxVector3 tempAxis = *axis;
		tempAxis.normalize();
		float halfAngle = Angle * 0.5F;
		float s = sin(halfAngle);

		pOut->w = cos(halfAngle);
		pOut->x = s * tempAxis.x;
		pOut->y = s * tempAxis.y;
		pOut->z = s * tempAxis.z;
		return pOut;
	#endif
	}

	VFX_API v3dxVector3* v3dxYawPitchRollQuaternionRotation(v3dxQuaternion pIn, v3dxVector3* pOutOLAngle)
	{
		v3dxVector3 &pOlAngle = *pOutOLAngle;
		float fYaw = 0.0f;
		float fPitch = 0.0f;
		float fRoll = 0.0f;

		float fX = pIn.x;
		float fY = pIn.y;
		float fZ = pIn.z;
		float fW = pIn.w;

		fYaw = (float)(atan2(2 * (fW * fY + fZ * fX), 1 - 2 * (fX * fX + fY * fY)));
		float fValue = 2 * (fW * fX - fY * fZ);
		fValue = ((fValue) > (1.0f) ? (1.0f) : ((fValue) < (-1.0f) ? (-1.0f) : fValue));
		fPitch = (float)(asin(fValue));
		fRoll = (float)(atan2(2 * (fW * fZ + fX * fY), 1 - 2 * (fZ * fZ + fX * fX)));

		pOlAngle.y = fYaw;
		pOlAngle.x = fPitch;
		pOlAngle.z = fRoll;

		return pOutOLAngle;
	}


	VFX_API v3dxQuaternion* v3dxQuaternionRotationYawPitchRoll(v3dxQuaternion *pOut, float p_y, float p_x, float p_z)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxQuaternion*)D3DXQuaternionRotationYawPitchRoll((D3DXQUATERNION *)pOut, p_y, p_x, p_z);
	#else
		v3dxQuaternion &result = *pOut;
		float yaw = p_y;
		float pitch = p_x;
		float roll = p_z;

		float halfRoll = roll * 0.5f;
		float sinRoll = (float)(Math::Sin((halfRoll)));
		float cosRoll = (float)(Math::Cos((halfRoll)));
		float halfPitch = pitch * 0.5f;
		float sinPitch = (float)(Math::Sin((halfPitch)));
		float cosPitch = (float)(Math::Cos((halfPitch)));
		float halfYaw = yaw * 0.5f;
		float sinYaw = (float)(Math::Sin((halfYaw)));
		float cosYaw = (float)(Math::Cos((halfYaw)));

		result.x = (cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll);
		result.y = (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll);
		result.z = (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll);
		result.w = (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll);

		return pOut;

	#endif
	}

	//////////////////////////////////////////////////////////////////////////
	VFX_API void v3dxQuaternionToAxisAngle(const v3dxQuaternion *pQ, v3dxVector3 *pAxis, float *pAngle)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		D3DXQuaternionToAxisAngle((D3DXQUATERNION *)pQ, (D3DXVECTOR3*)pAxis, pAngle);
	#else
		QuaternionToAxisAngle(*pQ, pAxis, pAngle);
	#endif
	}

	VFX_API v3dxMatrix4* v3dxMatrixRotationQuaternion(v3dxMatrix4 *pOut, CONST v3dxQuaternion *pQ)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxMatrix4*)D3DXMatrixRotationQuaternion((D3DXMATRIX *)pOut, (const D3DXQUATERNION*)pQ);
	#else
		float fTx = 2.0f*pQ->x;
		float fTy = 2.0f*pQ->y;
		float fTz = 2.0f*pQ->z;
		float fTwx = fTx*pQ->w;
		float fTwy = fTy*pQ->w;
		float fTwz = fTz*pQ->w;
		float fTxx = fTx*pQ->x;
		float fTxy = fTy*pQ->x;
		float fTxz = fTz*pQ->x;
		float fTyy = fTy*pQ->y;
		float fTyz = fTz*pQ->y;
		float fTzz = fTz*pQ->z;

		pOut->m[0][0] = 1.0f - (fTyy + fTzz);
		pOut->m[0][1] = fTxy + fTwz;
		pOut->m[0][2] = fTxz - fTwy;
		pOut->m[1][0] = fTxy - fTwz;
		pOut->m[1][1] = 1.0f - (fTxx + fTzz);
		pOut->m[1][2] = fTyz + fTwx;
		pOut->m[2][0] = fTxz + fTwy;
		pOut->m[2][1] = fTyz - fTwx;
		pOut->m[2][2] = 1.0f - (fTxx + fTyy);

		pOut->m[0][3] = 0;
		pOut->m[1][3] = 0;
		pOut->m[2][3] = 0;
		pOut->m[3][0] = 0;
		pOut->m[3][1] = 0;
		pOut->m[3][2] = 0;
		pOut->m[3][3] = 1;

		return pOut;
	#endif
	}

	VFX_API void v3dxMatrix4NoRotation(OUT v3dxMatrix4 *pOut, const v3dxMatrix4 *pSrc)
	{
		v3dxVector3 trans, scale;
		pSrc->ExtractionTrans(trans);
		pSrc->ExtractionScale(scale);
		pOut->m[0][0] = scale.x;
		pOut->m[0][1] = scale.x;
		pOut->m[0][2] = scale.x;
		pOut->m[0][3] = 0;

		pOut->m[1][0] = scale.y;
		pOut->m[1][1] = scale.y;
		pOut->m[1][2] = scale.y;
		pOut->m[1][3] = 0;

		pOut->m[2][0] = scale.z;
		pOut->m[2][1] = scale.z;
		pOut->m[2][2] = scale.z;
		pOut->m[2][3] = 0;

		pOut->m[3][0] = trans.x;
		pOut->m[3][1] = trans.y;
		pOut->m[3][2] = trans.z;
		pOut->m[3][3] = 1;
	}

	VFX_API HRESULT v3dxMatrixDecompose(v3dxVector3 *pOutScale, v3dxQuaternion *pOutRotation, v3dxVector3 *pOutTranslation, const v3dxMatrix4 *pM)
	{
//#ifdef USE_DX
//		return D3DXMatrixDecompose((D3DXVECTOR3*)pOutScale, (D3DXQUATERNION*)pOutRotation, (D3DXVECTOR3*)pOutTranslation, (const D3DXMATRIX*)pM);
//#else
//		return E_FAIL;
//#endif
	//#if defined(USE_DXMATH) && defined(USE_DX)
	#if defined WIN	// ������Windows��ʹ��DX�����㱣֤�༭������������������ƫ�ƽ�����ת�����ŵ�������Լ����㷨������
		return D3DXMatrixDecompose((D3DXVECTOR3*)pOutScale, (D3DXQUATERNION*)pOutRotation, (D3DXVECTOR3*)pOutTranslation, (const D3DXMATRIX*)pM);
	#else
		auto saveMT = *pM;
		saveMT.ExtractionTrans(*pOutTranslation);
		saveMT.m41 = 0;
		saveMT.m42 = 0;
		saveMT.m43 = 0;

		saveMT.ExtractionScale(*pOutScale);

		saveMT.m[0][0] /= pOutScale->x;
		saveMT.m[0][1] /= pOutScale->x;
		saveMT.m[0][2] /= pOutScale->x;
		
		saveMT.m[1][0] /= pOutScale->y;
		saveMT.m[1][1] /= pOutScale->y;
		saveMT.m[1][2] /= pOutScale->y;
		
		saveMT.m[2][0] /= pOutScale->z;
		saveMT.m[2][1] /= pOutScale->z;
		saveMT.m[2][2] /= pOutScale->z;

		saveMT.ExtractionRotation(*pOutRotation);
		
		return S_OK;
	#endif
	}

	VFX_API v3dxMatrix4* v3dxMatrixTransformationOrigin(v3dxMatrix4 *pOut, CONST v3dxVector3 *pScaling,
		CONST v3dxQuaternion *pRotation,
		CONST v3dxVector3 *pTranslation)
	{
		pOut->identity();

		if (pRotation != nullptr)
		{
			pRotation->toRotationMatrix(*pOut);
		}

		if (pScaling != nullptr)
		{
			pOut->m[0][0] *= pScaling->x;
			pOut->m[0][1] *= pScaling->x;
			pOut->m[0][2] *= pScaling->x;

			pOut->m[1][0] *= pScaling->y;
			pOut->m[1][1] *= pScaling->y;
			pOut->m[1][2] *= pScaling->y;

			pOut->m[2][0] *= pScaling->z;
			pOut->m[2][1] *= pScaling->z;
			pOut->m[2][2] *= pScaling->z;
		}
		if (pTranslation != nullptr)
		{
			pOut->m[3][0] = pTranslation->x;
			pOut->m[3][1] = pTranslation->y;
			pOut->m[3][2] = pTranslation->z;
		}
		return pOut;
	}

	VFX_API v3dxMatrix4* v3dxMatrixTransformation(v3dxMatrix4 *pOut, CONST v3dxVector3 *pScalingCenter,
		CONST v3dxQuaternion *pScalingRotation, CONST v3dxVector3 *pScaling,
		CONST v3dxVector3 *pRotationCenter, CONST v3dxQuaternion *pRotation,
		CONST v3dxVector3 *pTranslation)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxMatrix4*)D3DXMatrixTransformation((D3DXMATRIX*)pOut, (const D3DXVECTOR3*)pScalingCenter, (const D3DXQUATERNION*)pScalingRotation, (const D3DXVECTOR3*)pScaling,
			(const D3DXVECTOR3*)pRotationCenter, (const D3DXQUATERNION*)pRotation, (const D3DXVECTOR3*)pTranslation);
	#else
		pOut->identity();
		v3dxMatrix4 matRC;
		v3dxMatrix4 matSC;
		v3dxMatrix4 matSR;
		v3dxMatrix4 matR;
		v3dxMatrix4 matS;
		v3dxMatrix4 matT;

		v3dxMatrix4Translation(&matRC, pRotationCenter->x, pRotationCenter->y, pRotationCenter->z);
		v3dxMatrix4Translation(&matSC, pScalingCenter->x, pScalingCenter->y, pScalingCenter->z);
		v3dxMatrix4Translation(&matT, pTranslation->x, pTranslation->y, pTranslation->z);
		
		pScalingRotation->toRotationMatrix(matSR);
		pRotation->toRotationMatrix(matR);

		v3dxMatrix4Scale(&matS, pScaling->x, pScaling->y, pScaling->z);
		
		v3dxMatrix4 matSCInv;
		v3dxMatrix4 matSRInv;
		v3dxMatrix4 matRCInv;
		v3dxMatrix4Inverse(&matSCInv, &matSC, NULL);
		v3dxMatrix4Inverse(&matSRInv, &matSR, NULL);
		v3dxMatrix4Inverse(&matRCInv, &matRC, NULL);

		*pOut = matSCInv * matSRInv * matS * matSR * matSC * matRCInv * matR * matRC * matT;

		return pOut;
	#endif
	}

	VFX_API v3dxQuaternion* v3dxQuaternionMultiply(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ1, CONST v3dxQuaternion *pQ2)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxQuaternion*)D3DXQuaternionMultiply((D3DXQUATERNION*)pOut, (D3DXQUATERNION*)pQ1, (D3DXQUATERNION*)pQ2);
	#else
		const v3dxQuaternion &l = *pQ1;
		const v3dxQuaternion &r = *pQ2;
		/**pOut = v3dxQuaternion(
			l.w*r.x + l.x*r.w + l.y*r.z - l.z*r.y,
			l.w*r.y + l.y*r.w + l.z*r.x - l.x*r.z,
			l.w*r.z + l.z*r.w + l.x*r.y - l.y*r.x,
			l.w*r.w - l.x*r.x - l.y*r.y - l.z*r.z
			);*/


		float lx = l.x;
		float ly = l.y;
		float lz = l.z;
		float lw = l.w;
		float rx = r.x;
		float ry = r.y;
		float rz = r.z;
		float rw = r.w;

		pOut->x = (rx * lw + lx * rw + ry * lz) - (rz * ly);
		pOut->y = (ry * lw + ly * rw + rz * lx) - (rx * lz);
		pOut->z = (rz * lw + lz * rw + rx * ly) - (ry * lx);
		pOut->w = (rw * lw) - (rx *lx + ry * ly + rz * lz);

		return pOut;
	#endif
	}

	VFX_API v3dxMatrix4* v3dxMatrixReflect(v3dxMatrix4 *m, CONST v3dxPlane3 *plane)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxMatrix4*)D3DXMatrixReflect((D3DXMATRIX*)m, (D3DXPLANE*)plane);
	#else
		v3dxPlane3 p;
		v3dxVec4Normalize((v3dVector4_t *)&p,(v3dVector4_t *)plane);
		m->m11 = -2.0f * p.m_vNormal.x * p.m_vNormal.x + 1.0f;m->m12 = -2.0f * p.m_vNormal.y * p.m_vNormal.x;       m->m13 = -2.0f * p.m_vNormal.z * p.m_vNormal.x;       m->m14 = 0.0f;
		m->m21 = -2.0f * p.m_vNormal.x * p.m_vNormal.y       ;m->m22 = -2.0f * p.m_vNormal.y * p.m_vNormal.y + 1.0f;m->m23 = -2.0f * p.m_vNormal.z * p.m_vNormal.y;       m->m24 = 0.0f;
		m->m31 = -2.0f * p.m_vNormal.x * p.m_vNormal.z       ;m->m32 = -2.0f * p.m_vNormal.y * p.m_vNormal.z;       m->m33 = -2.0f * p.m_vNormal.z * p.m_vNormal.z + 1.0f;m->m34 = 0.0f;
		m->m41 = -2.0f * p.m_vNormal.x * p.m_fDD       ;m->m42 = -2.0f * p.m_vNormal.y * p.m_fDD;       m->m43 = -2.0f * p.m_vNormal.z * p.m_fDD;       m->m44 = 1.0f;
		return m;
	#endif
	}

	VFX_API v3dVector4_t* v3dxVec3TransformArray(v3dVector4_t *pOut, UINT OutStride, CONST v3dVector3_t *pV, UINT VStride, CONST v3dxMatrix4 *pM, UINT n)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dVector4_t*)D3DXVec3TransformArray((D3DXVECTOR4*)pOut, OutStride, (D3DXVECTOR3*)pV, VStride, (D3DXMATRIX*)pM, n);
	#else
		BYTE* outByte = (BYTE*)pOut;
		BYTE* inByte = (BYTE*)pV;
		for (UINT i = 0;i < n;i++)
		{
			v3dxVec3TransformFull((v3dVector4_t*)(outByte + OutStride*i), (v3dVector3_t*)(inByte + VStride), pM);
		}
		return pOut;
	#endif
	}
	VFX_API v3dVector3_t* v3dxVec3TransformNormalArray(v3dVector3_t* pOut, UINT OutStride, CONST v3dVector3_t *pV, UINT VStride, CONST v3dxMatrix4 *pM, UINT n)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dVector3_t*)D3DXVec3TransformNormalArray((D3DXVECTOR3*)pOut, OutStride, (D3DXVECTOR3*)pV, VStride, (D3DXMATRIX*)pM, n);
	#else
		BYTE* outByte = (BYTE*)pOut;
		BYTE* inByte = (BYTE*)pV;
		for (UINT i = 0;i < n;i++)
		{
			v3dxVec3TransformNormal((v3dVector3_t*)(outByte + OutStride*i), (v3dVector3_t*)(inByte + VStride), pM);
		}
		return pOut;
	#endif
	}

	VFX_API v3dxMatrix4* v3dxMatrixAffineTransformation(v3dxMatrix4 *pOut, FLOAT Scaling, CONST v3dVector3_t *pRotationCenter, CONST v3dVector4_t *pRotation, CONST v3dVector3_t *pTranslation)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxMatrix4*)D3DXMatrixAffineTransformation((D3DXMATRIX*)pOut, Scaling, (const D3DXVECTOR3*)pRotationCenter, (const D3DXQUATERNION*)pRotation, (const D3DXVECTOR3*)pTranslation);
	#else
		// Build affine transformation matrix.  NULL arguments are treated as identity.
		// Mout = Ms * Mrc-1 * Mr * Mrc * Mt
		return pOut;
	#endif
	}

	VFX_API v3dVector3_t* v3dxPlaneIntersectLine(v3dVector3_t *pOut, CONST v3dxPlane3 *pP, CONST v3dVector3_t *pV1, CONST v3dVector3_t *pV2)
	{
	#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dVector3_t*)D3DXPlaneIntersectLine((D3DXVECTOR3*)pOut, (const D3DXPLANE*)pP, (const D3DXVECTOR3*)pV1, (const D3DXVECTOR3*)pV2);
	#else
		float dist;
		auto ret = v3dxLineIntersectPlane_v2(*(v3dxVector3*)pV1, *(v3dxVector3*)pV2, *pP, *(v3dxVector3*)pOut, dist);
		if (ret == TRUE)
			return pOut;
		return NULL;
	#endif
	}

	VFX_API v3dxPlane3* v3dxPlaneScale
		(v3dxPlane3 *pOut, CONST v3dxPlane3 *pP, FLOAT s)
	{
		pOut->m_vNormal.x = pP->m_vNormal.x * s;
		pOut->m_vNormal.y = pP->m_vNormal.y * s;
		pOut->m_vNormal.z = pP->m_vNormal.z * s;
		pOut->m_fDD = pP->m_fDD * s;
		return pOut;
	}

	// Barycentric interpolation.
	// Slerp(Slerp(Q1, Q2, f+g), Slerp(Q1, Q3, f+g), g/(f+g))
	VFX_API v3dxQuaternion* v3dxQuaternionBaryCentric(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ1,
		CONST v3dxQuaternion *pQ2, CONST v3dxQuaternion *pQ3,
		FLOAT f, FLOAT g)
	{
#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxQuaternion*)D3DXQuaternionBaryCentric((D3DXQUATERNION *)pOut, (CONST D3DXQUATERNION *)pQ1,
			(CONST D3DXQUATERNION *)pQ2, (CONST D3DXQUATERNION*)pQ3, f, g);
#else
		v3dxQuaternion t1,t2;
		v3dxQuaternion::slerp(t1, f + g, *pQ1, *pQ2);
		v3dxQuaternion::slerp(t2, f + g, *pQ1, *pQ3);

		v3dxQuaternion::slerp(*pOut, g/(f + g), t1, t2);
		
		return pOut;
#endif
	}

	VFX_API v3dxQuaternion* v3dxQuaternionExp(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ)
	{
#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxQuaternion*)D3DXQuaternionExp((D3DXQUATERNION *)pOut, (CONST D3DXQUATERNION *)pQ);
#else
		*pOut = pQ->exp();
		return pOut;
#endif
	}

	VFX_API v3dxQuaternion* v3dxQuaternionLn(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ)
	{
#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dxQuaternion*)D3DXQuaternionLn((D3DXQUATERNION*)pOut, (CONST D3DXQUATERNION*)pQ);
#else
		*pOut = pQ->log();
		return pOut;
#endif
	}

	VFX_API v3dxQuaternion* v3dxQuaternionRotationMatrix(v3dxQuaternion *pOut, CONST v3dxMatrix4 *pM)
	{
#if defined(USE_DXMATH) && defined(USE_DX)		
		return (v3dxQuaternion*)D3DXQuaternionRotationMatrix((D3DXQUATERNION*)pOut, (CONST D3DXMATRIX *)pM);
#else
		pOut->fromRotationMatrix(*pM);
		return pOut;
#endif
	}

	VFX_API v3dxQuaternion* v3dxQuaternionSquad(v3dxQuaternion *pOut, CONST v3dxQuaternion *pQ1, CONST v3dxQuaternion *pA, CONST v3dxQuaternion *pB, CONST v3dxQuaternion *pC, FLOAT t)
	{
#if defined(USE_DXMATH) && defined(USE_DX)		
		return (v3dxQuaternion*)D3DXQuaternionSquad((D3DXQUATERNION*)pOut, (CONST D3DXQUATERNION*)pQ1, (CONST D3DXQUATERNION*)pA, (CONST D3DXQUATERNION*)pB, (CONST D3DXQUATERNION*)pC, t);
#else
		// Slerp(Slerp(Q1, C, t), Slerp(A, B, t), 2t(1-t))
		v3dxQuaternion t1, t2;
		v3dxQuaternion::slerp(t1, t, *pQ1, *pC);
		v3dxQuaternion::slerp(t2, t, *pA, *pB);
		v3dxQuaternion::slerp(*pOut, 2*t*(1-t), t1, t2);
		return pOut;
#endif
	}

	VFX_API v3dMatrix4_t* v3dxMatrixLookAtLH(v3dMatrix4_t* pOut, const v3dVector3_t* pvPos,
		const v3dVector3_t* pvAt, const v3dVector3_t* pvUp) 
	{
#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dMatrix4_t*)D3DXMatrixLookAtLH((D3DXMATRIX*)pOut, (D3DXVECTOR3*)pvPos, (D3DXVECTOR3*)pvAt, (D3DXVECTOR3*)pvUp);
#else
		v3dVector3_t vDir, vRight, vUp;
		v3dxVec3Sub(&vDir, pvAt, pvPos);
		v3dxVec3Normalize(&vDir, &vDir);

		v3dxVec3Cross(&vRight, pvUp, &vDir);
		v3dxVec3Normalize(&vRight, &vRight);
		v3dxVec3Cross(&vUp, &vDir, &vRight);

		return v3dxMatrix4View(pOut, pvPos, &vDir, &vUp, &vRight);
#endif
	}

	VFX_API v3dVector3_t* v3dxVec3TransformCoordArray(v3dVector3_t *pOut, UINT OutStride, CONST v3dVector3_t *pV, UINT VStride, CONST v3dxMatrix4 *pM, UINT n)
	{
#if defined(USE_DXMATH) && defined(USE_DX)
		return (v3dVector3_t*)D3DXVec3TransformCoordArray((D3DXVECTOR3*)pOut, OutStride, (CONST D3DXVECTOR3*)pV, VStride, (CONST D3DXMATRIX*)pM, n);
#else
		BYTE* outBytes = (BYTE*)pOut;
		BYTE* inBytes = (BYTE*)pV;
		for (UINT i = 0;i < n;i++)
		{
			v3dxVec3TransformCoord((v3dVector3_t*)(outBytes + OutStride), (v3dVector3_t*)(inBytes + VStride), pM);
		}
		return pOut;
#endif
	}

	VFX_API HRESULT v3dxComputeBoundingSphere(
		CONST v3dVector3_t *pFirstPosition,  // pointer to first position
		DWORD NumVertices,
		DWORD dwStride,                     // count in bytes to subsequent position vectors
		v3dVector3_t *pCenter,
		FLOAT *pRadius)
	{
#if defined(USE_DXMATH) && defined(USE_DX)
		return D3DXComputeBoundingSphere(
			(CONST D3DXVECTOR3 *)pFirstPosition,  // pointer to first position
			NumVertices,
			dwStride,                     // count in bytes to subsequent position vectors
			(D3DXVECTOR3 *)pCenter,
			pRadius);
#else
		return E_FAIL;
#endif
	}

	inline float PerpDotProduct2D(const v3dxVector3* u, const v3dxVector3* v)
	{//���� u ������ v �� PertDotProduct ���Ϊ��u �ķ�����������v �ĵ�˻������������������߶���ɵ�ƽ���ı��ε������||u||.||v||.sina��
		//Hill (1994):��ֱu������Ϊ(u.z,-u.x)���������dot v��������Ĺ�ʽ
		return u->z * v->x - u->x * v->z;
	}

	VFX_API vBOOL v3dIntersectSegSeg(const v3dxVector3* ap, const v3dxVector3* aq,
		const v3dxVector3* bp, const v3dxVector3* bq,
		float* s, float* t)
	{
		v3dxVector3 u, v, w;
		v3dxVec3Sub(&u, aq, ap);
		v3dxVec3Sub(&v, bq, bp);
		v3dxVec3Sub(&w, ap, bp);
		float d = PerpDotProduct2D(&u, &v);//u,v����������ƽ���ı������d
		if (fabsf(d) < 1e-6f) //������Ϊ0��˵���غ���
			return FALSE;
		d = 1.0f / d;
		//����u,v,w���������ͷ��֣��ֱ�����,u,v���������鹲�׵�ƽ���ı���
		*s = PerpDotProduct2D(&v, &w) * d;//v,w������ƽ���ı��κ�uv������ƽ���ı�������ȣ���Ϊ����v�����Ծ��Ǹ߱ȣ��߱ȸ�������������ɳ©ģ�ͣ���֪�������ǽ�����v�ϵı���s
		*t = PerpDotProduct2D(&u, &w) * d;//ͬ�����ƣ��������������u�ϱ���t
		if (*t < 0 || *t > 1)
			return FALSE;
		return TRUE;
	}

	bool _IntersectTri(const v3dxVector3& ray_origin, const v3dxVector3& ray_direction, const v3dxVector3& v0, const v3dxVector3& v1, const v3dxVector3& v2, float& t, float& u, float& v)
	{
		v3dxVector3 edge1 = v1 - v0;
		v3dxVector3 edge2 = v2 - v0;
		v3dxVector3 pvec = ray_direction.crossProduct(edge2);
		float det = edge1.dotProduct(pvec);
		if (det > -EPSILON && det < EPSILON)
			return false;
		float invDet = 1 / det;
		v3dxVector3 tvec = ray_origin - v0;
		u = tvec.dotProduct(pvec) * invDet;
		if (u < 0 || u > 1)
			return false;
		v3dxVector3 qvec = tvec.crossProduct(edge1);
		v = ray_direction.dotProduct(qvec) * invDet;
		if (v < 0 || u + v > 1)
			return false;
		t = edge2.dotProduct(qvec) * invDet;
		if (t < 0)
			return false;
		return true;
	}

	VFX_API vBOOL v3dxIntersectTri(
		CONST v3dxVector3 *p0,           // Triangle vertex 0 position
		CONST v3dxVector3 *p1,           // Triangle vertex 1 position
		CONST v3dxVector3 *p2,           // Triangle vertex 2 position
		CONST v3dxVector3 *pRayPos,      // Ray origin
		CONST v3dxVector3 *pRayDir,      // Ray direction
		FLOAT *pU,                       // Barycentric Hit Coordinates
		FLOAT *pV,                       // Barycentric Hit Coordinates
		FLOAT *pDist)
	{
#if defined(USE_DXMATH) && defined(USE_DX)
		return D3DXIntersectTri(
			(CONST D3DXVECTOR3 *)p0,           // Triangle vertex 0 position
			(CONST D3DXVECTOR3 *)p1,           // Triangle vertex 1 position
			(CONST D3DXVECTOR3 *)p2,           // Triangle vertex 2 position
			(CONST D3DXVECTOR3 *)pRayPos,      // Ray origin
			(CONST D3DXVECTOR3 *)pRayDir,      // Ray direction
			pU,                       // Barycentric Hit Coordinates
			pV,                       // Barycentric Hit Coordinates
			pDist);
		//v3dxVector3 vn = p0 + u * (p1 - p0) + v * (p2 - p1);
#else
		return _IntersectTri(*pRayPos, *pRayDir, *p0, *p1, *p2, *pDist, *pU, *pV) ? 1 : 0;
#endif
	}

	VFX_API void v3dxMatrix4Mul_CSharp(v3dMatrix4_t* pOut, const v3dMatrix4_t* mat1, const v3dMatrix4_t* mat2)
	{
		v3dxMatrix4Mul(pOut, mat1, mat2);
	}

	VFX_API v3dxPlane3* WINAPI v3dxPlaneTransform(v3dxPlane3* pout,
		const v3dxPlane3* pplane,
		const v3dxMatrix4* pm
	)
	{//https://doxygen.reactos.org/de/d57/dll_2directx_2wine_2d3dx9__36_2math_8c.html#abd38e717074a81bef44bcfe057f96f0c
		const v3dxPlane3& plane = *pplane;

		pout->m_vNormal.x = pm->m[0][0] * plane.m_vNormal.x + pm->m[1][0] * plane.m_vNormal.y + pm->m[2][0] * plane.m_vNormal.z + pm->m[3][0] * plane.m_fDD;
		pout->m_vNormal.y = pm->m[0][1] * plane.m_vNormal.x + pm->m[1][1] * plane.m_vNormal.y + pm->m[2][1] * plane.m_vNormal.z + pm->m[3][1] * plane.m_fDD;
		pout->m_vNormal.z = pm->m[0][2] * plane.m_vNormal.x + pm->m[1][2] * plane.m_vNormal.y + pm->m[2][2] * plane.m_vNormal.z + pm->m[3][2] * plane.m_fDD;
		pout->m_fDD = pm->m[0][3] * plane.m_vNormal.x + pm->m[1][3] * plane.m_vNormal.y + pm->m[2][3] * plane.m_vNormal.z + pm->m[3][3] * plane.m_fDD;
		pout->normalize();
		return pout;
	}
}