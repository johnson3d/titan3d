#include "v3dxBezier.h"
#include "v3dxQuaternion.h"
#include "../CSharpAPI.h"

#define new VNEW

NS_BEGIN

v3dxBezier::v3dxBezier(vBOOL Is2D) :
	m_fMaxLength(0)
{
	mIs2D = Is2D;
}


v3dxBezier::~v3dxBezier(void)
{
	//m_nodeArray.clear();
	//m_fMaxLength = 0;
	ClearNodes();
}


void v3dxBezier::CalculateMaxLength()
{
	if (mIs2D)
	{
		CalculateMaxLength2D();
		return;
	}
	m_fMaxLength = 0;

	if(m_nodeArray.size() < 2)
		return;

	for(size_t i=1; i<m_nodeArray.size(); ++i)
	{
		m_fMaxLength += (m_nodeArray[i]->vPos - m_nodeArray[i-1]->vPos).getLength();
	}
}

void v3dxBezier::CalculateMaxLength2D()
{
	m_fMaxLength = 0;

	if (m_nodeArray.size() < 2)
		return;

	m_fMaxLength = m_nodeArray[m_nodeArray.size() - 1]->vPos.x - m_nodeArray[0]->vPos.x;
}

void v3dxBezier::AddNode(const v3dxVector3* pos, const v3dxVector3* ctrlPos1, const v3dxVector3* ctrlPos2)
{
	auto sp = new sBezierPoint();
	sp->vPos = *pos;
	sp->vCtrlPos1 = *ctrlPos1;
	sp->vCtrlPos2 = *ctrlPos2;
	m_nodeArray.push_back(sp);
	CalculateMaxLength();
}
void v3dxBezier::InsertNode(int idx, const v3dxVector3* pos, const v3dxVector3* ctrlPos1, const v3dxVector3* ctrlPos2)
{
	int nNodesCount = (int)m_nodeArray.size();
	if(idx < 0) idx = 0;
	if(idx >= nNodesCount) 
		idx = nNodesCount;
	sBezierPoint* sp = new sBezierPoint();
	sp->vPos = *pos;
	sp->vCtrlPos1 = *ctrlPos1;
	sp->vCtrlPos2 = *ctrlPos2;
	if(idx < nNodesCount)
		m_nodeArray.insert(m_nodeArray.begin() + idx, sp);
	else
		m_nodeArray.push_back(sp);

	CalculateMaxLength();
}

void v3dxBezier::DeleteNode(int idx)
{
	if(idx >= 0 && idx < (int)m_nodeArray.size())
	{
		Safe_Delete(m_nodeArray[idx]);
		m_nodeArray.erase(m_nodeArray.begin() + idx);
	}

	CalculateMaxLength();
}

void v3dxBezier::ClearNodes()
{
	for (size_t i = 0; i<m_nodeArray.size(); i++)
	{
		Safe_Delete(m_nodeArray[i]);
	}
	m_nodeArray.clear();
	m_fMaxLength = 0;
}

// fTime 0-1
v3dxVector3 v3dxBezier::GetValue(float fTime)
{
	if(fTime == 0)
	{
		if(m_nodeArray.size() > 0)
			return m_nodeArray[0]->vPos;
		return v3dxVector3::ZERO;
	}

	float fDis = fTime * m_fMaxLength;

	size_t idx=0;
	float fTempDis = 0;
	for(idx=1; idx<m_nodeArray.size(); idx++)
	{
		fTempDis += (m_nodeArray[idx]->vPos - m_nodeArray[idx-1]->vPos).getLength();
		if(fTempDis > fDis)
			break;
	}
	
	if(idx >= m_nodeArray.size())
	{
		if(m_nodeArray.size() > 0)
			return m_nodeArray[m_nodeArray.size() - 1]->vPos;
		return v3dxVector3::ZERO;
	}

	sBezierPoint& pt0 = *m_nodeArray[idx-1];
	sBezierPoint& pt1 = *m_nodeArray[idx];
	float pt0Dis = 0;
	for(size_t i=1; i<=idx-1; i++)
	{
		pt0Dis += (m_nodeArray[i]->vPos - m_nodeArray[i-1]->vPos).getLength();
	}
	float t = (fDis - pt0Dis) / ((pt1.vPos - pt0.vPos).getLength());
	float yt = 1 - t;

	v3dxVector3 retValue;
	retValue.x = pt0.vPos.x * yt * yt * yt +
				 3 * (pt0.vCtrlPos2.x + pt0.vPos.x) * yt * yt * t +
				 3 * (pt1.vCtrlPos1.x + pt1.vPos.x) * yt * t * t +
				 pt1.vPos.x * t * t * t;
	retValue.y = pt0.vPos.y * yt * yt * yt +
				 3 * (pt0.vCtrlPos2.y + pt0.vPos.y) * yt * yt * t +
				 3 * (pt1.vCtrlPos1.y + pt1.vPos.y) * yt * t * t +
				 pt1.vPos.y * t * t * t;
	retValue.z = pt0.vPos.z * yt * yt * yt +
				 3 * (pt0.vCtrlPos2.z + pt0.vPos.z) * yt * yt * t +
				 3 * (pt1.vCtrlPos1.z + pt1.vPos.z) * yt * t * t +
				 pt1.vPos.z * t * t * t;

	return retValue;
}

float v3dxBezier::GetValueY_2D(float fTime)
{
	float fDis = fTime * m_fMaxLength;
	size_t idx = 0;
	for (idx = 1; idx < m_nodeArray.size(); idx++)
	{
		if (m_nodeArray[idx]->vPos.x > fDis)
			break;
	}

	if (idx >= m_nodeArray.size())
	{
		if (m_nodeArray.size() > 0)
			return m_nodeArray[m_nodeArray.size() - 1]->vPos.y;
		return 0;
	}

	sBezierPoint& pt0 = *m_nodeArray[idx - 1];
	sBezierPoint& pt1 = *m_nodeArray[idx];

	float t = (fDis - pt0.vPos.x) / (pt1.vPos.x - pt0.vPos.x);
	float yt = 1 - t;

	float y = pt0.vPos.y * yt * yt * yt +
		3 * (pt0.vCtrlPos2.y + pt0.vPos.y) * yt * yt * t +
		3 * (pt1.vCtrlPos1.y + pt1.vPos.y) * yt * t * t +
		pt1.vPos.y * t * t * t;

	return y;
}

vBOOL v3dxBezier::IsInRangeX(float value)
{
	float begin, end;
	GetRangeX(&begin, &end);

	if(value >= begin && value <= end)
		return true;

	return false;
}
void v3dxBezier::GetRangeX(float* begin, float* end)
{
	*begin = FLT_MAX;
	*end = -FLT_MAX;

	for(size_t i=0; i<m_nodeArray.size(); i++)
	{
		if(*begin > m_nodeArray[i]->vPos.x)
			*begin = m_nodeArray[i]->vPos.x;

		if(*end < m_nodeArray[i]->vPos.x)
			*end = m_nodeArray[i]->vPos.x;
	}
}
vBOOL v3dxBezier::IsInRangeY(float value)
{
	float begin, end;
	GetRangeY(&begin, &end);

	if(value >= begin && value <= end)
		return true;

	return false;
}
void v3dxBezier::GetRangeY(float* begin, float* end)
{
	*begin = FLT_MAX;
	*end = -FLT_MAX;

	for(size_t i=0; i<m_nodeArray.size(); i++)
	{
		if(*begin > m_nodeArray[i]->vPos.y)
			*begin = m_nodeArray[i]->vPos.y;

		if(*end < m_nodeArray[i]->vPos.y)
			*end = m_nodeArray[i]->vPos.y;
	}
}
vBOOL v3dxBezier::IsInRangeZ(float value)
{
	float begin, end;
	GetRangeZ(&begin, &end);

	if(value >= begin && value <= end)
		return true;

	return false;
}
void v3dxBezier::GetRangeZ(float* begin, float* end)
{
	*begin = FLT_MAX;
	*end = -FLT_MAX;

	for(size_t i=0; i<m_nodeArray.size(); i++)
	{
		if(*begin > m_nodeArray[i]->vPos.z)
			*begin = m_nodeArray[i]->vPos.z;

		if(*end < m_nodeArray[i]->vPos.z)
			*end = m_nodeArray[i]->vPos.z;
	}
}

void v3dxBezier::SetPosition(int idx, const v3dxVector3* pos)
{
	sBezierPoint* pt = GetNode(idx);
	if(pt != NULL)
		pt->vPos = *pos;

	CalculateMaxLength();
}

void v3dxBezier::SetControlPos1(int idx, const v3dxVector3* pos, vBOOL bWithPos2)
{
	sBezierPoint* pt = GetNode(idx);
	if(pt != NULL)
	{
		if(bWithPos2)
		{
			v3dxVector3 ctrlPosOld = pt->vCtrlPos1;
			ctrlPosOld.normalize();
			v3dxVector3 ctrlPosNew = *pos;
			ctrlPosNew.normalize();

			v3dxQuaternion quat = ctrlPosOld.getRotationTo(ctrlPosNew);
			v3dxMatrix4 mat;
			quat.toRotationMatrix(mat);
			v3dxVec3TransformCoord(&(pt->vCtrlPos2), &(pt->vCtrlPos2), &mat);
		}

		pt->vCtrlPos1 = *pos;
	}
}
void v3dxBezier::SetControlPos2(int idx, const v3dxVector3* pos, vBOOL bWithPos1)
{
	sBezierPoint* pt = GetNode(idx);
	if(pt != NULL)
	{
		if(bWithPos1)
		{
			v3dxVector3 ctrlPosOld = pt->vCtrlPos2;
			ctrlPosOld.normalize();
			v3dxVector3 ctrlPosNew = *pos;
			ctrlPosNew.normalize();

			v3dxQuaternion quat = ctrlPosOld.getRotationTo(ctrlPosNew);
			v3dxMatrix4 mat;
			quat.toRotationMatrix(mat);
			v3dxVec3TransformCoord(&(pt->vCtrlPos1), &(pt->vCtrlPos1), &mat);
		}

		pt->vCtrlPos2 = *pos;
	}
}

vBOOL v3dxBezier::CopyTo(v3dxBezier* cloneTo) const
{
	if (cloneTo == NULL)
		return FALSE;

	cloneTo->m_fMaxLength = m_fMaxLength;
	if (m_nodeArray.size() > 0)
	{
		cloneTo->m_nodeArray.resize(m_nodeArray.size());
		for (size_t i = 0; i < m_nodeArray.size(); ++i)
		{
			cloneTo->m_nodeArray[i] = new sBezierPoint();
			cloneTo->m_nodeArray[i]->vPos = m_nodeArray[i]->vPos;
			cloneTo->m_nodeArray[i]->vCtrlPos1 = m_nodeArray[i]->vCtrlPos1;
			cloneTo->m_nodeArray[i]->vCtrlPos2 = m_nodeArray[i]->vCtrlPos2;
		}
	}

	return TRUE;
}


v3dScalarVariable::v3dScalarVariable()
	: v3dVariable(Constant)
{
	mConstant = 0;
	mValueBegin = 0;
	mValueEnd = 0;
	mChangeToMask = 0xFFFFFFFF;
	mBezier2D = NULL;
	mConstantRange = NULL;
}
v3dScalarVariable::v3dScalarVariable(float value)
	: v3dVariable(Constant)
{
	mConstant = value;
	mValueBegin = 0;
	mValueEnd = 0;
	mBezier2D = NULL;
	mConstantRange = NULL;
	mChangeToMask = 0xFFFFFFFF;
}
v3dScalarVariable::v3dScalarVariable(float begin, float end)
	: v3dVariable(ConstantRange)
{
	mBezier2D = NULL;
	mConstantRange = new v3dxScalarRange(begin, end);
	mValueBegin = begin;
	mValueEnd = end;
	mChangeToMask = 0xFFFFFFFF;
}

v3dScalarVariable::v3dScalarVariable(const v3dxBezier& cv)
	: v3dVariable(Curve)
{
	mChangeToMask = 0xFFFFFFFF;
	mConstantRange = new v3dxScalarRange(mValueBegin, mValueEnd);
	mBezier2D = new v3dxBezier();
	cv.CopyTo(mBezier2D);
	if (mBezier2D->GetNodesCount() >= 2)
	{
		mBezier2D->GetRangeY(&mValueBegin, &mValueEnd);
	}
	else
	{
		mValueBegin = 0;
		mValueEnd = 0;
	}
}

v3dScalarVariable::~v3dScalarVariable()
{
	//if (mType == ConstantRange)
	Safe_Delete(mConstantRange);
	//else if (mType == Curve)
	Safe_Release(mBezier2D);
}

v3dxBezier* v3dScalarVariable::GetBezier()
{
	return mBezier2D;
}

float v3dScalarVariable::getConstant()
{
	if (mType == Constant)
		return mConstant;
	return FLT_MAX;
}
v3dxScalarRange *v3dScalarVariable::GetConstantRange()
{
	if (mType == ConstantRange)
		return mConstantRange;
	return NULL;
}

v3dxBezier *v3dScalarVariable::getCurve()
{
	if (mType == Curve)
		return mBezier2D;
	return NULL;
}

float v3dScalarVariable::getValue(float slider)
{
	if (mType == Constant)
		return mConstant;
	else if (mType == ConstantRange)
		return mConstantRange->getValue(slider);
	else if (mType == Curve)
		return mBezier2D->GetValueY_2D(slider) * (mValueEnd - mValueBegin) + mValueBegin;
	return FLT_MAX;
}
float v3dScalarVariable::getRandomValue()
{
	if (mType == Constant)
		return mConstant;
	else if (mType == ConstantRange)
		return mConstantRange->rand();
	else if (mType == Curve)
		return mBezier2D->GetValueY_2D(Math::UnitRandom()) * (mValueEnd - mValueBegin) + mValueBegin;
	return FLT_MAX;
}
bool v3dScalarVariable::isInRange(float value)
{
	if (mType == Constant)
		return mConstant == value;
	else if (mType == ConstantRange)
		return mConstantRange->isInRange(value);
	else if (mType == Curve)
		return mBezier2D->IsInRangeX(value);
	//return value <= mBezier2D->getValMax() && value >= mBezier2D->getValMin();
	return false;
}
void v3dScalarVariable::setValue(float value)
{
	//changeType(Constant);
	mConstant = value;
}
//void v3dScalarVariable::setValue(float begin, float end, bool curve)
//{
//	if (curve)
//	{
//		//changeType(Curve);
//		//mBezier2D->setValBegin(begin);
//		//mBezier2D->setValBegin(end);
//		mValueBegin = begin;
//		mValueEnd = end;
//	}
//	else
//	{
//		//changeType(ConstantRange);
//		mConstantRange->setRange(begin, end);
//	}
//}
void v3dScalarVariable::setValueBegin(float value)
{
	mValueBegin = value;
	switch (mType)
	{
	case v3dVariable::ConstantRange:
		mConstantRange->setBegin(value);
		break;

	case v3dVariable::Curve:
		break;
	}
}
void v3dScalarVariable::setValueEnd(float value)
{
	mValueEnd = value;
	switch (mType)
	{
	case v3dVariable::ConstantRange:
		mConstantRange->setEnd(value);
		break;

	case v3dVariable::Curve:
		break;
	}
}
void v3dScalarVariable::changeType(Type type)
{
	if (mType == type)
		return;

	if (!CanChangeToType(type))
		return;

	Safe_Delete(mConstantRange);
	Safe_Release(mBezier2D);

	if (type == ConstantRange)
		mConstantRange = new v3dxScalarRange();
	else if (type == Curve)
	{
		mBezier2D = new v3dxBezier();
		v3dxVector3 pos = v3dxVector3(0, 0.5f, 0);
		v3dxVector3 cPos1 = v3dxVector3(-0.1f, 0, 0);
		v3dxVector3 cPos2 = v3dxVector3(0.1f, 0, 0);
		mBezier2D->InsertNode(0, &pos, &cPos1, &cPos2);

		pos = v3dxVector3(1, 0.5f, 0);
		mBezier2D->InsertNode(1, &pos, &cPos1, &cPos2);
	}
	else
		mConstant = 0;
	mType = type;
}
void v3dScalarVariable::SetChangeToTypeEnable(Type type, bool enable)
{
	if (enable)
		mChangeToMask |= (1 << (int)type);
	else
		mChangeToMask &= ~(1 << (int)type);
}
bool v3dScalarVariable::CanChangeToType(Type type)
{
	return ((mChangeToMask & (1 << (int)type)) == (1 << (int)type));
}

v3dScalarVariable *v3dScalarVariable::Clone()
{
	v3dScalarVariable *pNew = new v3dScalarVariable();
	pNew->mType = mType;
	pNew->mConstant = mConstant;
	pNew->mValueBegin = mValueBegin;
	pNew->mValueEnd = mValueEnd;
	pNew->mChangeToMask = mChangeToMask;
	if (mBezier2D)
	{
		Safe_Release(pNew->mBezier2D);
		pNew->mBezier2D = new v3dxBezier();
		mBezier2D->CopyTo(pNew->mBezier2D);
	}
	if (pNew->mConstantRange)
	{
		Safe_Delete(pNew->mConstantRange);
		pNew->mConstantRange = new v3dxScalarRange(mConstantRange->mBegin, mConstantRange->mEnd);
	}

	return pNew;
}

void v3dScalarVariable::CopyFrom(const v3dScalarVariable *src)
{
	mType = src->mType;
	mConstant = src->mConstant;
	mValueBegin = src->mValueBegin;
	mValueEnd = src->mValueEnd;
	mChangeToMask = src->mChangeToMask;
	Safe_Release(mBezier2D);
	if (src->mBezier2D)
	{
		mBezier2D = new v3dxBezier();
		src->mBezier2D->CopyTo(mBezier2D);
	}
	Safe_Delete(mConstantRange);
	if (src->mConstantRange)
	{
		mConstantRange = new v3dxScalarRange(src->mConstantRange->mBegin, src->mConstantRange->mEnd);
	}
}

//////////////////////////////////////////////////////////////////////////

v3dColorVariable::v3dColorVariable()
	: v3dVariable(Constant)
	, mConstant(0xffffffff)
{
}
v3dColorVariable::v3dColorVariable(const v3dxColor4& value)
	: v3dVariable(Constant)
	, mConstant(value.getD3DVal())
{
}
v3dColorVariable::v3dColorVariable(const v3dxColor4& begin, const v3dxColor4& end)
	: v3dVariable(ConstantRange)
	, mConstantRange(new v3dxColorRange(begin.getD3DVal(), end.getD3DVal()))
{
}
v3dColorVariable::~v3dColorVariable()
{
	if (mType == ConstantRange)
		Safe_Delete(mConstantRange);
}
v3dxColor4 v3dColorVariable::getValue(float slider)
{
	if (mType == Constant)
		return mConstant;
	else if (mType == ConstantRange)
		return mConstantRange->getValue(slider);
	return v3dxColor4(1, 1, 1, 1);
}
v3dxColor4 v3dColorVariable::getRandomValue()
{
	if (mType == Constant)
		return mConstant;
	else if (mType == ConstantRange)
		return mConstantRange->getValue(Math::UnitRandom());
	return v3dxColor4(1, 1, 1, 1);
}
void v3dColorVariable::CopyFrom(const v3dColorVariable *src)
{
	if (mType == ConstantRange)
		Safe_Delete(mConstantRange);

	mType = src->mType;
	if (mType == Constant)
	{
		mConstant = src->mConstant;
	}
	else if (mType == ConstantRange)
	{
		mConstantRange = new v3dxColorRange();
		mConstantRange->mBegin = src->mConstantRange->mBegin;
		mConstantRange->mEnd = src->mConstantRange->mEnd;
	}
}
void v3dColorVariable::setValue(const v3dxColor4 &clr)
{
	changeType(Constant);
	mConstant = clr.getD3DVal();
}
void v3dColorVariable::setValue(const v3dxColor4 &bgn, const v3dxColor4 &end)
{
	changeType(ConstantRange);
	mConstantRange->setRange(bgn.getD3DVal(), end.getD3DVal());
}
void v3dColorVariable::changeType(v3dVariable::Type type)
{
	if (mType == type)
		return;
	if (mType == ConstantRange)
		Safe_Delete(mConstantRange);
	if (type == ConstantRange)
		mConstantRange = new v3dxColorRange();
	else
		mConstant = 0xFFFFFFFF;
	mType = type;
}
v3dxColor4 v3dColorVariable::getValue()
{
	if (mType == Constant)
		return mConstant;
	return getValueBegin();
}
v3dxColor4 v3dColorVariable::getValueBegin()
{
	if (mType == ConstantRange)
		return mConstantRange->getMin();
	return mConstant;
}
v3dxColor4 v3dColorVariable::getValueEnd()
{
	if (mType == ConstantRange)
		return mConstantRange->getMax();
	return mConstant;
}

//////////////////////////////////////////////////////////////////////////

v3dFloat4Variable::v3dFloat4Variable()
	: v3dVariable(Constant)
{
	mConstant.x = mConstant.y = mConstant.z = mConstant.w = 1;
}
v3dFloat4Variable::v3dFloat4Variable(const v3dVector4_t& value)
	: v3dVariable(Constant)
	, mConstant(value)
{
}
v3dFloat4Variable::v3dFloat4Variable(const v3dVector4_t& begin, const v3dVector4_t& end)
	: v3dVariable(ConstantRange)
{
	mRange.mBegin = begin;
	mRange.mEnd = end;
}
v3dFloat4Variable::~v3dFloat4Variable()
{
}
v3dVector4_t v3dFloat4Variable::getValue()
{
	if (mType == Constant)
		return mConstant;
	return getValueBegin();
}
v3dVector4_t v3dFloat4Variable::getValueBegin()
{
	if (mType == ConstantRange)
		return mRange.mBegin;
	return mConstant;
}
v3dVector4_t v3dFloat4Variable::getValueEnd()
{
	if (mType == ConstantRange)
		return mRange.mEnd;
	return mConstant;
}
v3dVector4_t v3dFloat4Variable::getValue(float slider)
{
	v3dVector4_t v;
	if (mType == Constant)
		v = mConstant;
	else if (mType == ConstantRange) {
		v.x = mRange.mBegin.x + (mRange.mEnd.x - mRange.mBegin.x) * slider;
		v.y = mRange.mBegin.y + (mRange.mEnd.y - mRange.mBegin.y) * slider;
		v.z = mRange.mBegin.z + (mRange.mEnd.z - mRange.mBegin.z) * slider;
		v.w = mRange.mBegin.w + (mRange.mEnd.w - mRange.mBegin.w) * slider;
	}
	else {
		v.x = v.y = v.z = v.w = 1;
	}
	return v;
}
v3dVector4_t v3dFloat4Variable::getRandomValue()
{
	return getValue(Math::UnitRandom());
}
void v3dFloat4Variable::CopyFrom(const v3dFloat4Variable *src)
{
	mType = src->mType;
	if (mType == Constant)
	{
		mConstant = src->mConstant;
	}
	else if (mType == ConstantRange)
	{
		mRange.mBegin = src->mRange.mBegin;
		mRange.mEnd = src->mRange.mEnd;
	}
}
void v3dFloat4Variable::setValue(const v3dVector4_t &val)
{
	mType = Constant;
	mConstant = val;
}
void v3dFloat4Variable::setValue(const v3dVector4_t &bgn, const v3dVector4_t &end)
{
	mType = ConstantRange;
	mRange.mBegin = bgn;
	mRange.mEnd = end;
}

NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API v3dxBezier* SDK_v3dxBezier_New(vBOOL is2D)
	{
		return new v3dxBezier(is2D);
	}
	CSharpAPI3(EngineNS, v3dxBezier, AddNode, const v3dxVector3*, const v3dxVector3*, const v3dxVector3*);
	CSharpAPI4(EngineNS, v3dxBezier, InsertNode, int, const v3dxVector3*, const v3dxVector3*, const v3dxVector3*);
	CSharpAPI1(EngineNS, v3dxBezier, DeleteNode, int);
	CSharpReturnAPI0(vBOOL, EngineNS, v3dxBezier, GetIs2D);
	CSharpAPI2(EngineNS, v3dxBezier, GetValue, float, v3dxVector3*);
	CSharpAPI2(EngineNS, v3dxBezier, GetPosition, int, v3dxVector3*);
	CSharpAPI2(EngineNS, v3dxBezier, SetPosition, int, v3dxVector3*);
	CSharpAPI2(EngineNS, v3dxBezier, GetControlPos1, int, v3dxVector3*);
	CSharpAPI3(EngineNS, v3dxBezier, SetControlPos1, int, v3dxVector3*, vBOOL);
	CSharpAPI2(EngineNS, v3dxBezier, GetControlPos2, int, v3dxVector3*);
	CSharpAPI3(EngineNS, v3dxBezier, SetControlPos2, int, v3dxVector3*, vBOOL);
	CSharpReturnAPI0(int, EngineNS, v3dxBezier, GetNodesCount);
	CSharpReturnAPI1(vBOOL, EngineNS, v3dxBezier, CopyTo, v3dxBezier*);
	CSharpAPI0(EngineNS, v3dxBezier, ClearNodes);
	CSharpReturnAPI1(vBOOL, EngineNS, v3dxBezier, IsInRangeX, float);
	CSharpAPI2(EngineNS, v3dxBezier, GetRangeX, float*, float*);
	CSharpReturnAPI1(vBOOL, EngineNS, v3dxBezier, IsInRangeY, float);
	CSharpAPI2(EngineNS, v3dxBezier, GetRangeY, float*, float*);
	CSharpReturnAPI1(vBOOL, EngineNS, v3dxBezier, IsInRangeZ, float);
	CSharpAPI2(EngineNS, v3dxBezier, GetRangeZ, float*, float*);
};