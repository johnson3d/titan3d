#include "v3dxSpline.h"
#include "v3dxMatrix4.h"
#include "v3dxMath.h"
#include <algorithm>

#define new VNEW
//V3D_BEGIN

v3dxMatrix4 c_matHermite = v3dxMatrix4( 2.f,-2.f, 1.f, 1.f,
								 -3.f, 3.f,-2.f,-1.f,
								 0.f, 0.f, 1.f, 0.f,
								 1.f, 0.f, 0.f, 0.f);

#define CalcVel(vAim,vSrc,fZoom) ((vAim - vSrc).getNormal() * fZoom)
// cubic curve defined by 2 positions and 2 velocities
v3dxVector3 c_GetPositionOnCubic(const v3dxVector3 &startPos, const v3dxVector3 &startVel, const v3dxVector3 &endPos, const v3dxVector3 &endVel, float time)
{
	v3dxMatrix4 m( startPos.x, startPos.y, startPos.z, 0,
		endPos.x,	endPos.y,	endPos.z,   0,
		startVel.x,	startVel.y,	startVel.z, 0,
		endVel.x,	endVel.y,	endVel.z,   1);
	m = c_matHermite * m;

	v3dxVector3 timeVector = v3dxVector3(time*time*time, time*time, time);
	v3dxVec3TransformCoord(&timeVector, &timeVector, &m);
	return timeVector;
}

void v3dxSpline::insertNode(int idx, const v3dxVector3 &pos)
{
	int nNodeCnt = (int)m_aNodeArray.size();
	if ( idx < 0 ) idx = 0;
	if ( idx > nNodeCnt ) idx = nNodeCnt;
	sSubPoint sp;
	if ( idx < nNodeCnt )
		m_aNodeArray.insert(m_aNodeArray.begin() + idx, sp);
	else
		m_aNodeArray.push_back(sp);
	nNodeCnt = (int)m_aNodeArray.size();
	setPosition(idx, pos);
	buildVelocity(idx);
}

void v3dxSpline::deleteNode(int idx)
{
	if ( idx >= 0 && idx < (int)m_aNodeArray.size() )
	{
		m_fMaxDistance -= m_aNodeArray[idx].fDist;
		m_aNodeArray.erase(m_aNodeArray.begin()+idx);
		int nCnt = (int)m_aNodeArray.size();
		if ( nCnt > 0 )
		{
			if ( idx == 0 )
				m_aNodeArray[idx].vVel1 = v3dxVector3::ZERO;
			if ( idx == nCnt )
				m_aNodeArray[nCnt-1].vVel2 = v3dxVector3::ZERO;
		}
		//if ( idx > 0 )
		//	setPosition(idx-1, m_aNodeArray[idx-1].vPos);
		//if ( idx >= 0 )
		//	setPosition(idx, m_aNodeArray[idx].vPos);
	}
}

void v3dxSpline::setPosition(int idx, const v3dxVector3 &pos)
{
	int nNodeCnt = (int)m_aNodeArray.size();
	if ( idx >= 0 && idx < nNodeCnt )
	{
		sSubPoint& sTheNode = m_aNodeArray[idx];
		sTheNode.vPos = pos;
		if ( idx > 0 )
			m_aNodeArray[idx-1].fDist = (m_aNodeArray[idx-1].vPos - pos).getLength();
		if ( idx < nNodeCnt - 1 )
			sTheNode.fDist = (sTheNode.vPos - m_aNodeArray[idx+1].vPos).getLength();
		m_fMaxDistance = 0;
		for ( int i = 0; i < nNodeCnt; ++i )
			m_fMaxDistance += m_aNodeArray[i].fDist;

		if ( idx <= 1 || idx >= nNodeCnt - 2 )
			enableTangentOnTail(m_bTangentOnTail);
	}
}

void v3dxSpline::enableTangentOnTail(vBOOL b)
{
	m_bTangentOnTail = b;
	int nNodeCnt = (int)m_aNodeArray.size();
	if ( !m_bTangentOnTail && nNodeCnt )
	{
		buildVelocity(0);
		if ( nNodeCnt > 1 )
			buildVelocity(nNodeCnt-1);
	}
}

void v3dxSpline::setBeginVel(int idx, const v3dxVector3 &vel, vBOOL bWithEnd/* = TRUE*/)
{
	if ( idx >= 0 && idx < (int)m_aNodeArray.size() )
	{
		v3dxVector3 vPreVel = m_aNodeArray[idx].vVel1;
		if ( vPreVel == vel )
			return;
		if ( vel.getLength() < 0.0001f )
			m_aNodeArray[idx].vVel1 = m_aNodeArray[idx].vVel1.getNormal();
		else
		{
			m_aNodeArray[idx].vVel1 = vel;
			if ( bWithEnd )
			{
				v3dxVector3 vCrs = vel.crossProduct(vPreVel);
				float fSrcLen2 = vPreVel.getLengthSq();
				float fAimLen2 = vel.getLengthSq();
				if ( fSrcLen2 != 0 && fAimLen2 != 0 && vCrs.getLength() > 0.001f )
				{
					float f3rdLen2 = (vPreVel - vel).getLengthSq();
					float fAngle = acosf((fSrcLen2 + fAimLen2 - f3rdLen2) / (2*sqrtf(fSrcLen2*fAimLen2)));
					if ( fAngle != 0 )
					{
						v3dxMatrix4 matRot;
						v3dxMatrix4RotationAxis(&matRot, &vCrs, -fAngle);
						v3dxVec3TransformCoord(&m_aNodeArray[idx].vVel2, &m_aNodeArray[idx].vVel2, &matRot);
					}
				}
			}
		}
	}
}

void v3dxSpline::setEndVel(int idx, const v3dxVector3 &vel, vBOOL bWithBegin/* = TRUE*/)
{
	if ( idx >= 0 && idx < (int)m_aNodeArray.size() )
	{
		v3dxVector3 vPreVel = m_aNodeArray[idx].vVel2;
		if ( vPreVel == vel )
			return;
		if ( vel.getLength() < 0.0001f )
			m_aNodeArray[idx].vVel2 = m_aNodeArray[idx].vVel2.getNormal();
		else
		{
			m_aNodeArray[idx].vVel2 = vel;
			if ( bWithBegin )
			{
				v3dxVector3 vCrs = vel.crossProduct(vPreVel);
				float fSrcLen2 = vPreVel.getLengthSq();
				float fAimLen2 = vel.getLengthSq();
				if ( fSrcLen2 != 0 && fAimLen2 != 0 && vCrs.getLength() > 0.001f )
				{
					float f3rdLen2 = (vPreVel - vel).getLengthSq();
					float fAngle = acosf((fSrcLen2 + fAimLen2 - f3rdLen2) / (2*sqrtf(fSrcLen2*fAimLen2)));
					if ( fAngle != 0 )
					{
						v3dxMatrix4 matRot;
						v3dxMatrix4RotationAxis(&matRot, &vCrs, -fAngle);
						v3dxVec3TransformCoord(&m_aNodeArray[idx].vVel1, &m_aNodeArray[idx].vVel1, &matRot);
					}
				}
			}
		}
	}
}

void v3dxSpline::buildVelocities()
{
	for ( int i = 0; i < (int)m_aNodeArray.size(); ++i )
		buildVelocity(i);
}

void v3dxSpline::buildVelocity(int idx)
{
	int nNodeCnt = (int)m_aNodeArray.size();
	if (idx < 0 || idx >= nNodeCnt) return;

	sSubPoint& sNode = m_aNodeArray[idx];
	if ( idx == 0 )
	{
		sNode.vVel1 = v3dxVector3::ZERO;
		if ( nNodeCnt > 1 )
			sNode.vVel2 = CalcVel(m_aNodeArray[1].vPos, sNode.vPos, sNode.fDist);
		if ( nNodeCnt == 2 )
		{
			m_aNodeArray[1].vVel1 = CalcVel(sNode.vPos, m_aNodeArray[1].vPos, sNode.fDist);
			m_aNodeArray[1].vVel2 = v3dxVector3::ZERO;
		}
	}
	else if ( idx == nNodeCnt - 1 )
	{
		if ( idx == 1 )
		{
			m_aNodeArray[0].vVel1 = v3dxVector3::ZERO;
			m_aNodeArray[0].vVel2 = CalcVel(sNode.vPos, m_aNodeArray[0].vPos, m_aNodeArray[0].fDist);
		}
		if ( idx > 0 )
		{
			sNode.vVel1 = CalcVel(m_aNodeArray[idx-1].vPos, sNode.vPos, m_aNodeArray[idx-1].fDist);
			sNode.vVel2 = v3dxVector3::ZERO;
		}
	}
	else
	{
		v3dxVector3 v1 = m_aNodeArray[idx-1].vPos - sNode.vPos;
		v3dxVector3 v2 = m_aNodeArray[idx+1].vPos - sNode.vPos;
		v3dxVector3 vTg = (v2.getNormal() - v1.getNormal()).getNormal();
		sNode.vVel2 = vTg * sNode.fDist;
		sNode.vVel1 = -1 * vTg * sNode.fDist;
		if ( idx == 1 )
		{
			m_aNodeArray[0].vVel1 = v3dxVector3::ZERO;
			m_aNodeArray[0].vVel2 = CalcVel(sNode.vPos, m_aNodeArray[0].vPos, m_aNodeArray[0].fDist);
		}
		if ( idx == nNodeCnt - 2 )
		{
			m_aNodeArray[nNodeCnt-1].vVel1 = CalcVel(sNode.vPos, m_aNodeArray[nNodeCnt-1].vPos, sNode.fDist);
			m_aNodeArray[nNodeCnt-1].vVel2 = v3dxVector3::ZERO;
		}
	}
}

// spline access function. time is 0 -> 1
v3dxVector3 v3dxSpline::getPosition(float fTime, int* pIndex/* = NULL*/, v3dxVector3* pVel/* = NULL*/)
{
	float fDist = fTime * m_fMaxDistance;
	float fCurDist = 0.f;
	int nNodeCnt = (int)m_aNodeArray.size();
	int i = 0;
	while (i < nNodeCnt && fCurDist + m_aNodeArray[i].fDist <= fDist )
	{
		fCurDist += m_aNodeArray[i].fDist;
		i++;
	}
	if ( i < nNodeCnt - 1 )
	{
		float fNextFar = m_aNodeArray[i].fDist;
		float t = fNextFar ? ((fDist - fCurDist) / fNextFar) : 0;
		v3dxVector3 startVel = m_aNodeArray[i].vVel2;
		v3dxVector3 endVel = -1 * m_aNodeArray[i+1].vVel1;
		if ( pIndex ) *pIndex = i;
		if ( pVel ) *pVel = startVel;
		return c_GetPositionOnCubic(m_aNodeArray[i].vPos, startVel, m_aNodeArray[i+1].vPos, endVel, t);
	}
	else if ( nNodeCnt > 1 )
	{
		if ( pIndex ) *pIndex = nNodeCnt-1;
		if ( pVel ) *pVel = m_aNodeArray[nNodeCnt-2].vVel2;
		return m_aNodeArray[nNodeCnt-1].vPos;
	}
	return v3dxVector3::ZERO;
}

void v3dxSpline::genLineNodes(std::vector<v3dxVector3>* aArray, float fDistStep /*= 1.f*/, vBOOL bGetLessNode /*= TRUE*/)
{
	aArray->clear();
	if ( fDistStep <= 0 )
		fDistStep = m_fMaxDistance / 100.f;
	for ( float d = 0; d <= m_fMaxDistance; d += fDistStep )
	{
		float t = d / m_fMaxDistance;
		v3dxVector3 vPos = getPosition(t);
		int nNowSize = (int)aArray->size();
		if ( nNowSize < 2 || d == m_fMaxDistance )
			aArray->push_back(vPos);
		else
		{
			if ( !bGetLessNode )
				aArray->push_back(vPos);
			else
			{
				v3dxVector3 vPrePos1 = (*aArray)[nNowSize-2];
				v3dxVector3 vPrePos2 = (*aArray)[nNowSize-1];
				v3dxVector3 vInv1 = vPrePos2 - vPrePos1;
				v3dxVector3 vInv2 = vPos - vPrePos2;
				float fVal = vInv1.crossProduct(vInv2).getLengthSq();
				if ( fVal > 1e-3f * fDistStep )
					aArray->push_back(vPos);
			}
		}
		if ( d < m_fMaxDistance - m_fMaxDistance * 0.0001f && d + fDistStep > m_fMaxDistance )
			d = m_fMaxDistance - fDistStep;
	}
}

v3dxSpline& v3dxSpline::operator = (const v3dxSpline& r)
{
	m_aNodeArray.clear();
	for ( int i = 0; i < (int)r.m_aNodeArray.size(); ++i )
		m_aNodeArray.push_back(r.m_aNodeArray[i]);
	m_fMaxDistance = r.m_fMaxDistance;
	m_bTangentOnTail = r.m_bTangentOnTail;
	return *this;
}

//////////////////////////////////////////////////////////////////////////
v3dxCurve2::v3dxCurve2()
:m_fValBgn(0),m_fValEnd(0)
,m_fValMin(-10),m_fValMax(10)
,m_fHoriLength(1),m_fValRand(0)
,m_bRandomOnHead(TRUE),m_bStraightMode(FALSE)
{
	m_bTangentOnTail = TRUE;
	insertNode(0,v3dxVector3(0,m_fValBgn,0));
	insertNode(1,v3dxVector3(m_fHoriLength,m_fValEnd,0));
	buildVelocities();
}

v3dxCurve2::v3dxCurve2(float fVal1, float fVal2, float fAspect /*= 0*/, float fMin /*= FLT_MIN*/, float fMax /*= FLT_MAX*/):
m_fValBgn(fVal1),m_fValEnd(fVal2),m_fValRand(0),m_bRandomOnHead(TRUE),m_bStraightMode(FALSE)
{
	m_bTangentOnTail = TRUE;
	if (fMin != FLT_MIN)
		m_fValMin = fMin;
	else
	{
		float val = fabsf(TPL_HELP::vfxMAX(fVal1, fVal2));
		if (val == 0)
			m_fValMin = -10;
		else
			m_fValMin = -val;
	}
	if (fMax != FLT_MAX)
		m_fValMax = fMax;
	else
	{
		float val = fabsf(TPL_HELP::vfxMAX(fVal1, fVal2));
		if (val == 0)
			m_fValMax = 10;
		else
			m_fValMax = val;
	}
	setViewAspect(fAspect);
	insertNode(0,v3dxVector3(0,m_fValBgn,0));
	insertNode(1,v3dxVector3(m_fHoriLength,m_fValEnd,0));
	buildVelocities();
}

void v3dxCurve2::setViewAspect(float fAspect)
{// w / h
	float fSrcLen = m_fHoriLength;

	if ( fAspect > 0.f )
	{
		m_fHoriLength = getValRange() * fAspect;
	}
	else
	{
		m_fHoriLength = 1;
	}

	float fZoom = m_fHoriLength / fSrcLen;
	if(fZoom != 1.f)
	{
		for ( int i = 0; i < (int)m_aNodeArray.size(); ++i )
		{
			v3dxVector3 v = m_aNodeArray[i].vPos;
			v.x *= fZoom;
			setPosition(i, v);
			m_aNodeArray[i].vVel1.x *= fZoom;
			m_aNodeArray[i].vVel2.x *= fZoom;
		}
	}
}

void v3dxCurve2::enableStraightMode(vBOOL b)
{
	if ( b )
	{
		m_fValEnd = m_fValBgn;
		removeAll();
	}
	m_bStraightMode = b;
}

void v3dxCurve2::insertNode(int idx, const v3dxVector3 &pos)
{
	if ( idx == 0 ) idx = 1;
	v3dxVector3 okpos = pos;
	okpos.x = okpos.x < 0 ? 0 : okpos.x;
	okpos.x = okpos.x > m_fHoriLength ? m_fHoriLength : okpos.x;
	okpos.z = 0;
	okpos.y = okpos.y < m_fValMin ? m_fValMin : okpos.y;
	okpos.y = okpos.y > m_fValMax ? m_fValMax : okpos.y;
	v3dxSpline::insertNode(idx, okpos);
	m_bStraightMode = FALSE;
}

void v3dxCurve2::deleteNode(int idx)
{
	if ( idx != 0 && idx != getNodeCount() - 1 )
		v3dxSpline::deleteNode(idx);
}

void v3dxCurve2::setPosition(int idx, const v3dxVector3 &pos)
{
	v3dxVector3 okpos = pos;
	okpos.z = 0;
	if ( idx == 0 )
		okpos.x = 0;
	else if ( idx == getNodeCount() - 1 )
		okpos.x = m_fHoriLength;
	okpos.y = okpos.y < m_fValMin ? m_fValMin : okpos.y;
	okpos.y = okpos.y > m_fValMax ? m_fValMax : okpos.y;
	if ( !m_bStraightMode )
	{
		v3dxSpline::setPosition(idx, okpos);
		if ( idx == 0 )
			m_fValBgn = okpos.y;
		else if ( idx == (int)m_aNodeArray.size() - 1 )
			m_fValEnd = okpos.y;
	}
	else
	{
		v3dxSpline::setPosition(0, v3dxVector3(0, okpos.y, 0));
		v3dxSpline::setPosition(1, v3dxVector3(m_fHoriLength, okpos.y, 0));
		m_fValBgn = m_fValEnd = okpos.y;
	}
}

v3dxVector3 v3dxCurve2::getPosition(float fTime, int* pIndex /*= NULL*/, v3dxVector3* pVel /*= NULL*/)
{
	if ( m_bStraightMode || fTime == 0 )
		return v3dxVector3(0, getValBegin(), 0);
	return v3dxSpline::getPosition(fTime, pIndex, pVel);
}

void v3dxCurve2::genLineNodes(std::vector<v3dxVector3>* aArray, float fDistStep /*= 1.f*/, vBOOL bGetLessNode /*= TRUE*/)
{
	if ( !m_bStraightMode )
		v3dxSpline::genLineNodes(aArray, fDistStep, bGetLessNode);
	else
	{
		aArray->clear();
		assert(m_aNodeArray.size()==2);
		for ( int i = 0; i < (int)m_aNodeArray.size(); ++i )
			aArray->push_back(m_aNodeArray[i].vPos);
	}
}

float v3dxCurve2::getValue(float fTime)
{
	if ( m_bStraightMode || fTime == 0 )
		return getValBegin();
	return getPosition(fTime).y;
}

void v3dxCurve2::removeAll()
{
	v3dxSpline::removeAll();
	insertNode(0,v3dxVector3(0,m_fValBgn,0));
	insertNode(1,v3dxVector3(m_fHoriLength,m_fValEnd,0));
	buildVelocities();
}

void v3dxCurve2::setValue(float fBgn, float fEnd, float fRand)
{
	setValBegin(fBgn);
	setValEnd(fEnd);
	setValRand(fRand);
}

void v3dxCurve2::setValBegin(float v)
{
	v = v < m_fValMin ? m_fValMin : v;
	v = v > m_fValMax ? m_fValMax : v;
	m_fValBgn = v;
	setPosition(0, v3dxVector3(0,m_fValBgn,0));
}

void v3dxCurve2::setValEnd(float v)
{
	v = v < m_fValMin ? m_fValMin : v;
	v = v > m_fValMax ? m_fValMax : v;
	m_fValEnd = v;
	setPosition((int)m_aNodeArray.size()-1, v3dxVector3(m_fHoriLength,m_fValEnd,0));
}

void v3dxCurve2::setValRange(float fMin, float fMax)
{
	fMax = fMax < fMin ? fMin : fMax;
	m_fValMin = fMin;
	m_fValMax = fMax;
	for ( int i = 0; i < (int)m_aNodeArray.size(); ++i )
	{
		v3dxVector3 pos = m_aNodeArray[i].vPos;
		if ( pos.y < fMin )
		{
			pos.y = fMin;
			v3dxSpline::setPosition(i, pos);
		}
		else if ( pos.y > fMax )
		{
			pos.y = fMax;
			v3dxSpline::setPosition(i, pos);
		}
	}
}

v3dxCurve2& v3dxCurve2::operator = (const v3dxCurve2& r)
{
	m_aNodeArray.clear();
	for ( int i = 0; i < (int)r.m_aNodeArray.size(); ++i )
		m_aNodeArray.push_back(r.m_aNodeArray[i]);
	m_fMaxDistance = r.m_fMaxDistance;
	m_bTangentOnTail = r.m_bTangentOnTail;
	m_fValBgn = r.m_fValBgn;
	m_fValEnd = r.m_fValEnd;
	m_fValMin = r.m_fValMin;
	m_fValMax = r.m_fValMax;
	m_fHoriLength = r.m_fHoriLength;
	return *this;
}

/*#include <tchar.h>
#define CURVESTR_PREFIX_W	_T("T:%d;R:%d,%.3f;S:%d;H:%.3f;A:%.3f;B:%.3f;")
#define CURVESTR_PREFIX_R	_T("T:%d;R:%d,%f;S:%d;H:%f;A:%f;B:%f;")
#define CURVESTR_NODE_W		_T("(%.3f,%.3f)[%.3f,%.3f,%.3f][%.3f,%.3f,%.3f]")
#define CURVESTR_NODE_R		_T("(%f,%f)[%f,%f,%f][%f,%f,%f]")
void v3dxCurve2::setValueByString(LPCSTR str)
{
	VString strVal(str);
	int t = strVal.Find(_T("{"), 0);
	if ( t > 0 )
	{
		VString strPre = strVal.Left(t);
		_stscanf(strPre, CURVESTR_PREFIX_R, &m_bTangentOnTail, &m_bRandomOnHead, &m_fValRand, &m_bStraightMode, &m_fHoriLength, &m_fValMin, &m_fValMax);
		VString strNodes = strVal.Mid(t+1, strVal.length()-t-2);
		VString strNode;
		t = 0;
		int idx = 0;
		v3dxSpline::removeAll();
		while(1)
		{
			int s = t;
			t = strNodes.Find(_T(";"), s+1);
			if ( t >= 0 )
				strNode = strNodes.Mid(s, t-s);
			else
				strNode = strNodes.Mid(s+1, strNodes.length()-s-1);
			sSubPoint sp;
			float a,b,vx1,vy1,vz1,vx2,vy2,vz2;
			_stscanf(strNode, CURVESTR_NODE_R, &a, &b, &vx1, &vy1, &vz1, &vx2, &vy2, &vz2);
			sp.vPos.x = a; sp.vPos.y = b;
			sp.vVel1.x = vx1; sp.vVel1.x = vy1; sp.vVel1.x = vz1;
			sp.vVel2.x = vx2; sp.vVel2.x = vy2; sp.vVel2.x = vz2;
			m_aNodeArray.push_back(sp);
			if ( t < 0 ) break;
		}
		int nCnt = (int)m_aNodeArray.size();
		m_fMaxDistance = 0.f;
		for ( int i = 0; i < nCnt - 1; ++i )
		{
			m_aNodeArray[i].fDist = (m_aNodeArray[i+1].vPos - m_aNodeArray[i].vPos).getLength();
			m_fMaxDistance += m_aNodeArray[i].fDist;
			if ( i == 0 )
				m_fValBgn = m_aNodeArray[i].vPos.y;
			if ( i == nCnt - 2 )
				m_fValEnd = m_aNodeArray[nCnt - 1].vPos.y;
		}
	}
}

VString v3dxCurve2::getValueString()
{
	// bTan;bRdHead;bStaight;fHorLen;fMin;fMax;{vPos;vVel1;vVel2}
	VString strVal(_T(""));
	TCHAR ch[MAX_PATH];
	_stprintf_s(ch, CURVESTR_PREFIX_W, m_bTangentOnTail, m_bRandomOnHead, m_fValRand, m_bStraightMode, m_fHoriLength, m_fValMin, m_fValMax);
	strVal += VString(ch) + VString(_T("{"));
	int nCnt = (int)m_aNodeArray.size();
	for ( int i = 0; i < nCnt; ++i )
	{
		sSubPoint& sp = m_aNodeArray[i];
		_stprintf_s(ch, CURVESTR_NODE_W, sp.vPos.x, sp.vPos.y, sp.vVel1.x, sp.vVel1.y, sp.vVel1.z, sp.vVel2.x, sp.vVel2.y, sp.vVel2.z);
		strVal += VString(ch);
		if ( i < nCnt - 1 )
			strVal += VString(_T(";"));
		else
			strVal += VString(_T("}"));
	}
	return strVal;
}
//V3D_END*/

extern "C"
{
	 v3dxCurve2* v3dxCurve2_New(float fBgn, float fEnd, float fAspect, float fMin, float fMax)
	{
		return new v3dxCurve2(fBgn, fEnd, fAspect, fMin, fMax);
	}

	 void v3dxCurve2_Delete(v3dxCurve2* cur2)
	{
		 EngineNS::Safe_Delete(cur2);
	}

	 void v3dxCurve2_InsertNode(v3dxCurve2* cur2, int idx, v3dxVector3* pos)
	{
		if(cur2 == NULL)
			return;

		cur2->insertNode(idx, *pos);
	}

	 void v3dxCurve2_DeleteNode(v3dxCurve2* cur2, int idx)
	{
		if(cur2 == NULL)
			return;

		cur2->deleteNode(idx);
	}

	 void v3dxCurve2_SetPosition(v3dxCurve2* cur2, int idx, v3dxVector3* pos)
	{
		if(cur2 == NULL)
			return;

		cur2->setPosition(idx, *pos);
	}

	 float v3dxCurve2_GetValue(v3dxCurve2* cur2, float fTime)
	{
		if(cur2 == NULL)
			return 0;

		return cur2->getValue(fTime);
	}

	 void v3dxCurve2_SetValBegin(v3dxCurve2* cur2, float v)
	{
		if(cur2 == NULL)
			return;

		return cur2->setValBegin(v);
	}

	 void v3dxCurve2_SetValEnd(v3dxCurve2* cur2, float v)
	{
		if(cur2 == NULL)
			return;

		return cur2->setValEnd(v);
	}

	 int v3dxCurve2_GetNodeCount(v3dxCurve2* cur2)
	{
		if(cur2 == NULL)
			return 0;

		return cur2->getNodeCount();
	}

	 void v3dxCurve2_GetNodePos(v3dxCurve2* cur2, int index, v3dxVector3* pos)
	{
		if(cur2 == NULL)
			return;

		v3dxSpline::sSubPoint* pt = cur2->getNode(index);
		if(pt == NULL)
			return;

		*pos = pt->vPos;
	}

	 void v3dxCurve2_SetNodeBeginVel(v3dxCurve2* cur2, int index, v3dxVector3* pos, bool bWithEnd)
	{
		if(cur2 == NULL)
			return;

		cur2->setBeginVel(index, *pos, bWithEnd);
	}

	 void v3dxCurve2_GetNodeBeginVel(v3dxCurve2* cur2, int index, v3dxVector3* pos)
	{
		if(cur2 == NULL)
			return;

		v3dxSpline::sSubPoint* pt = cur2->getNode(index);
		if(pt == NULL)
			return;

		*pos = pt->vVel1;
	}

	 void v3dxCurve2_SetNodeEndVel(v3dxCurve2* cur2, int index, v3dxVector3* pos, bool bWithBegin)
	{
		if(cur2 == NULL)
			return;

		cur2->setEndVel(index, *pos, bWithBegin);
	}

	 void v3dxCurve2_GetNodeEndVel(v3dxCurve2* cur2, int index, v3dxVector3* pos)
	{
		if(cur2 == NULL)
			return;

		v3dxSpline::sSubPoint* pt = cur2->getNode(index);
		if(pt == NULL)
			return;

		*pos = pt->vVel2;
	}

	 void v3dxCurve2_BuildVelocity(v3dxCurve2* cur2, int index)
	{
		if(cur2 == NULL)
			return;

		cur2->buildVelocity(index);
	}
};