#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxVector3Curve.h"
#include "../Skeleton/GfxBone.h"

NS_BEGIN
class GfxMotionStateCurve : public VIUnknown
{
	friend class GfxAsset_AnimationCreater;
public:
	GfxMotionStateCurve();
	~GfxMotionStateCurve();
public:
	GfxMotionState Evaluate(float curveT) const;
	vBOOL LoadXnd(IRenderContext* rc, XNDNode* node);
	void Save2Xnd(XNDNode* node);
protected:
	AutoRef<GfxVector3Curve> mPosCurve;
	AutoRef<GfxVector3Curve> mVelocityCurve;
};

NS_END