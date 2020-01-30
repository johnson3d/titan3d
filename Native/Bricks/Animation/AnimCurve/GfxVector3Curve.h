#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxCurveTpl.h"
#include "GfxICurve.h"

NS_BEGIN
class GfxVector3Curve : public GfxICurve
{
public:
	RTTI_DEF(GfxVector3Curve, 0x455862fa5cf0b246, true);
	GfxVector3Curve();
	~GfxVector3Curve();
public:
	virtual void Evaluate(float curveT, CurveResult& result) override;
	virtual void EvaluateClamp(float curveT, CurveResult& result) override;
	virtual UINT GetKeyCount() override { return std::max<int>(mXCurve ? mXCurve->GetKeyCount() : 0, std::max<int>(mYCurve ? mYCurve->GetKeyCount() : 0, mZCurve ? mZCurve->GetKeyCount() : 0)); }

	virtual vBOOL LoadXnd(IRenderContext* rc, XNDNode* node) override;
	virtual void Save2Xnd(XNDNode* node) override;

	void SetCurve(int index, GfxCurveTpl<float>* curve) 
	{ 
		switch (index)
		{
		case 0:
			mXCurve.StrongRef(curve);
		case 1:
			mYCurve.StrongRef(curve);
		case 2:
			mZCurve.StrongRef(curve);
		default:
			break;
		}
	}
public:
	//GfxCurveTpl<float>* mCurves[3];
	AutoRef<GfxCurveTpl<float>> mXCurve;
	AutoRef<GfxCurveTpl<float>> mYCurve;
	AutoRef<GfxCurveTpl<float>> mZCurve;
};

NS_END