#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "AnimCurve/GfxBoneCurve.h"
#include "GfxSkeletonAction.h"
NS_BEGIN
class GfxSkeletonAnimation :public VIUnknown
{
public:
	RTTI_DEF(GfxSkeletonAnimation, 0xbbc17a4c5ce50bfc, true);
	GfxSkeletonAnimation();
	~GfxSkeletonAnimation();
public:
	void EvaluatePose(float time, GfxAnimationPose* outPose, vBOOL evaluateMotionState);
	void InitBySkeletonActioin(GfxSkeletonAction* action);
public:
	std::vector<GfxBoneCurve*>		mBoneCurves;
	std::map<int, GfxBoneCurve*>	mBoneCurveMap;
};

NS_END