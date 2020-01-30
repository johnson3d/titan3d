#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxCurveTpl.h"
#include "GfxCurveTpl.cpp"
#include "GfxICurve.h"

NS_BEGIN
class GfxQuaternionCurve : public GfxICurve
{
public:
	RTTI_DEF(GfxQuaternionCurve, 0xb852425c5cf0b26c, true);
	GfxQuaternionCurve();
	~GfxQuaternionCurve();
public:
	virtual void Evaluate(float curveT, CurveResult& result) override;
	virtual void EvaluateClamp(float curveT, CurveResult& result) override;
	virtual UINT GetKeyCount() override { return mCurve.GetKeyCount(); } // std::max<int>(mCurves[0].GetKeyCount(), std::max<int>(mCurves[1].GetKeyCount(), std::max<int>(mCurves[2].GetKeyCount(),mCurves[3].GetKeyCount()))); }
	CurveKeyTpl<v3dxQuaternion>& GetKey(int index) { return mCurve.GetKey(index); }
	bool IsValid() override { return mCurve.IsValid(); }

	virtual vBOOL LoadXnd(IRenderContext* rc, XNDNode* node) override;
	virtual void Save2Xnd(XNDNode* node) override;
public:
	//GfxCurve mCurves[4];
	GfxCurveTpl<v3dxQuaternion> mCurve;
};

NS_END