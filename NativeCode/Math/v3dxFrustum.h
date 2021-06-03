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
enum TR_ENUM()
ENUM_FRUSTUM_CORNER
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
enum TR_ENUM()
ENUM_FRUSTUM_PLANE
{
	ENUM_FRUSTUMPL_TOP = 0,
	ENUM_FRUSTUMPL_RIGHT,
	ENUM_FRUSTUMPL_BOTTOM,
	ENUM_FRUSTUMPL_LEFT,
	ENUM_FRUSTUMPL_NEAR,
	ENUM_FRUSTUMPL_FAR,
	ENUM_FRUSTUMPL_NUMBER
};

enum TR_ENUM()
CONTAIN_TYPE{
	CONTAIN_TEST_NOTIMPLEMENT	= -2,
	CONTAIN_TEST_INNER			= 1,
	CONTAIN_TEST_OUTER			= -1,
	CONTAIN_TEST_REFER			= 0,
};

class TR_CLASS(SV_LayoutStruct = 8)
v3dxFrustum
{
public:
	TR_FUNCTION()
	inline WORD WhichOutSide(const v3dxVector3* pvPos)
	{
		WORD _sides = 0;
		int _OutTimes = 0;
		for (int i = 0; i < ENUM_FRUSTUMPL_NUMBER; i++)
		{
			if (m_aPlane[i].classify(*pvPos) >= 0.f)
			{
				_sides = _sides | (1 << i);
				_OutTimes++;
			}
			if (_OutTimes >= 3)
				//if(_OutTimes>=2)
				return _sides;
		}
		return _sides;
	}

	TR_FUNCTION()
	vBOOL isContain(const v3dxVector3 & center) const;
	TR_FUNCTION()
	vBOOL isContain(const v3dxVector3 & center, float fRadius) const;
	TR_FUNCTION()
	vBOOL isContain(const v3dxBox3 & box) const;

	TR_DISCARD()
	bool fastTestOBB(const v3dxOBB* pOBB, const v3dxMatrix4* pTM) const;
	TR_FUNCTION()
	bool fastTestOBB2(const v3dxBox3* pBox, const v3dxMatrix4* pTM) const;

	TR_DISCARD()
	CONTAIN_TYPE whichContainTypeFast(const v3dxBox3* pBBox, vBOOL testInner) const;
	TR_DISCARD()
	CONTAIN_TYPE whichContainTypeFast(const v3dxPoly3* pPoly, const v3dxMatrix4* pTM = NULL) const;
	TR_DISCARD()
	CONTAIN_TYPE whichContainType(v3dxVector3* verts, int num, const v3dxVector3* center, vBOOL testInner) const;

	CONTAIN_TYPE whichContainTypeFast(const v3dxBox3* pBBox, const v3dxMatrix4* pTMInverse, vBOOL testInner) const;

	TR_DISCARD()
	HRESULT build(const v3dxVector3* pvTipPt, const v3dxVector3 avDir[ENUM_FRUSTUMCN_NUMBER],
		const v3dxVector3* pvDirection = NULL, FLOAT fHeight = FLT_MAX * 0.5f);
	TR_DISCARD()
	void buildFrustum(const v3dVector3_t vecFrustum[8]);
	TR_DISCARD()
	void buildFrustum(const v3dxVector3* pvTipPt,
		const v3dxVector3* pv1,
		const v3dxVector3* pv2,
		const v3dxVector3* pv3,
		const v3dxVector3* pv4,
		const v3dxPlane3* pNearPlane,
		const v3dxPlane3* pFarPlane);

	TR_FUNCTION()
	void BuildFrustum(const v3dxMatrix4* InvViewProj);
	TR_DISCARD()
	void BuildFrustum(const v3dxVector3* vecPoints);

	TR_DISCARD()
	int _checkBeContained(const v3dxBox3* box);
	TR_DISCARD()
	int _checkContain(const v3dxBox3* box) const;

	TR_FUNCTION()
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
	TR_FUNCTION()
	v3dxVector3* getTipPos() const {
		return (v3dxVector3*)&m_vTipPt;
	}
	TR_FUNCTION()
	v3dxPlane3* getEdgePlane() const{
		return (v3dxPlane3*)m_aPlane;
	}
	TR_FUNCTION()
	v3dxPlane3* getFarPlane() const{
		return (v3dxPlane3*)&m_aPlane[ENUM_FRUSTUMPL_FAR];
	}

public:
	TR_MEMBER(SV_ReturnConverter = v3dVector3_t)
	v3dxVector3				m_vTipPt;
	
	v3dxVector3				m_vecFrustum[8];
	v3dxPlane3				m_aPlane[6];
};

#pragma pack(pop)

#endif//#ifndef __v3dxFrustum__H__