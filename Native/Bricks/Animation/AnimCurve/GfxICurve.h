#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../AnimationDataTypes.h"

NS_BEGIN

class GfxICurve : public VIUnknown
{
public:
	RTTI_DEF(GfxICurve, 0x8d7ad4e45cf0b1d0, true);
	GfxICurve();
	~GfxICurve();
public:
	void EvaluateNative(float curveT, CurveResult* result);

	virtual void Evaluate(float curveT, CurveResult& result);
	virtual void EvaluateClamp(float curveT, CurveResult& result);
	virtual UINT GetKeyCount() { return 0; }
	virtual bool IsValid() { return false; }
	CurveType GetCurveType() { return mType; }

	virtual vBOOL LoadXnd(IRenderContext* rc, XNDNode* node) { return false; }
	virtual void Save2Xnd(XNDNode* node) {}
protected:
	CurveType mType;
};

NS_END


