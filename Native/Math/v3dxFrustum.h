/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxfrustum.h
	Created Time:		30:6:2002   16:33
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/

#ifndef __v3dxFrustum__H__
#define __v3dxFrustum__H__

#include "v3dxMath.h"
#include "v3dxVector3.h"
#include "v3dxPlane3.h"
#include "v3dxOBB.h"
#include "v3dxSphere.h"

#pragma pack(push,4)

/*
corner arrange
	| 0 | 1 |
	| 2 | 3 |
	number = 4
*/
enum ENUM_FRUSTUM_CORNER
{
	ENUM_FRUSTUMCN_LEFTTOP = 0,
	ENUM_FRUSTUMCN_RIGHTTOP,
	ENUM_FRUSTUMCN_RIGHTBOTTOM,
	ENUM_FRUSTUMCN_LEFTBOTTOM,
	ENUM_FRUSTUMCN_NUMBER
};

/*
frustum plane
	|--1--|
	4     2
	|--3--|
*/
enum ENUM_FRUSTUM_PLANE
{
	ENUM_FRUSTUMPL_TOP = 0,
	ENUM_FRUSTUMPL_RIGHT,
	ENUM_FRUSTUMPL_BOTTOM,
	ENUM_FRUSTUMPL_LEFT,
	ENUM_FRUSTUMPL_NEAR,
	ENUM_FRUSTUMPL_FAR,
	ENUM_FRUSTUMPL_NUMBER
};

enum CONTAIN_TYPE{
	CONTAIN_TEST_NOTIMPLEMENT	= -2,
	CONTAIN_TEST_INNER			= 1,
	CONTAIN_TEST_OUTER			= -1,
	CONTAIN_TEST_REFER			= 0,
};

class v3dxFrustum
{
public:
	inline WORD WhichOutSide(const v3dxVector3* pvPos)
	{
		WORD _sides = 0;
		int _OutTimes = 0;
		for (int i = 0; i < ENUM_FRUSTUMPL_NUMBER; i++)
			//for( int i=0 ; i<ENUM_FRUSTUMPL_NUMBER-2 ; i++)
		{
			if (m_aPlane[i].classify(*pvPos) >= 0.f)
			{
				_sides = _sides | (1 << i);
				_OutTimes++;
			}
			// һ��������ͬʱ��������������������棬���Բ���Ҫ���������ˣ�ֱ�ӷ���
			if (_OutTimes >= 3)
				//if(_OutTimes>=2)
				return _sides;
		}
		return _sides;
	}

	 vBOOL isContain(const v3dVector3_t & center) const;
	 vBOOL isContain(const v3dVector3_t & center,float fRadius) const;
	 vBOOL isContain(const v3dxBox3 & box) const;

	 bool fastTestOBB(const v3dxOBB* pOBB, const v3dxMatrix4* pTM) const;
	 bool fastTestOBB2(const v3dxBox3* pBox, const v3dxMatrix4* pTM) const;

	 int whichContainTypeFast(const v3dxBox3* pBBox, vBOOL testInner) const;
	 int whichContainTypeFast( const v3dxPoly3* pPoly , const v3dxMatrix4* pTM=NULL ) const;//���������
	 int whichContainType(v3dxVector3* verts, int num, const v3dxVector3* center, vBOOL testInner) const;//���������

	 HRESULT build(const v3dxVector3* pvTipPt,const v3dxVector3 avDir[ENUM_FRUSTUMCN_NUMBER],
		const v3dxVector3* pvDirection=NULL,FLOAT fHeight = FLT_MAX*0.5f);
	 void buildFrustum(const v3dVector3_t vecFrustum[8]);
	 void buildFrustum( const v3dxVector3* pvTipPt , 
		const v3dxVector3* pv1,
		const v3dxVector3* pv2,
		const v3dxVector3* pv3,
		const v3dxVector3* pv4,
		const v3dxPlane3* pNearPlane,
		const v3dxPlane3* pFarPlane );
	 void buildFrustum(const v3dxMatrix4* InvViewProj);
	 void buildFrustum(const v3dxVector3* vecPoints);

	 int _checkBeContained(const v3dxBox3* box);
	 int _checkContain(const v3dxBox3* box) const;


	 void CopyFrustum(v3dxFrustum* dest) const {
		 dest->m_vTipPt = m_vTipPt;
		 for (int i = 0; i < 8; i++)
		 {
			 dest->m_vecFrustum[i] = m_vecFrustum[i];
		 }
		 for (int i = 0; i < ENUM_FRUSTUMPL_NUMBER; i++)
		 {
			 dest->m_aPlane[i] = m_aPlane[i];
		 }
	 }
public:
	v3dxVector3* getTipPos() const {
		return (v3dxVector3*)&m_vTipPt;
	}
	v3dxPlane3* getEdgePlane() const{
		return (v3dxPlane3*)m_aPlane;
	}
	v3dxPlane3* getFarPlane() const{
		return (v3dxPlane3*)&m_aPlane[ENUM_FRUSTUMPL_FAR];
	}

public:
	v3dxVector3				m_vTipPt;
	
	v3dxVector3				m_vecFrustum[8];
	v3dxPlane3				m_aPlane[ENUM_FRUSTUMPL_NUMBER];
};

#pragma pack(pop)

#endif//#ifndef __v3dxFrustum__H__