#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxICurve.h"
#include "GfxCurveTpl.h"

NS_BEGIN
class GfxFloatCurve : public GfxICurve
{
public:
	RTTI_DEF(GfxFloatCurve, 0xc172a8e95cf0b21d, true);
	GfxFloatCurve();
	~GfxFloatCurve();
public:
	virtual void Evaluate(float curveT, CurveResult& result) override;
	virtual void EvaluateClamp(float curveT, CurveResult& result) override;
	virtual UINT GetKeyCount() override { return mCurve.GetKeyCount(); }

	virtual vBOOL LoadXnd(IRenderContext* rc, XNDNode* node) override;
	virtual void Save2Xnd(XNDNode* node) override;
public:
	GfxCurveTpl<float> mCurve;
};

NS_END