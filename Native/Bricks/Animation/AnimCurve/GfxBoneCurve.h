#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxVector3Curve.h"
#include "GfxQuaternionCurve.h"
#include "../Skeleton/GfxBone.h"
#include "GfxMotionStateCurve.h"
#include "GfxICurve.h"

NS_BEGIN

class GfxBoneCurve : public GfxICurve
{
	friend class GfxSkeletonAnimation;
	friend class GfxBoneAnim;
	friend class GfxAsset_AnimationCreater;
public:
	RTTI_DEF(GfxBoneCurve, 0xe6267c765cf0b288, true);
	GfxBoneCurve();
	~GfxBoneCurve();
public:
	virtual void Evaluate(float curveT, CurveResult& result) override;
	void EvaluateMotionState(float curveT, GfxMotionState* motionData);
	GfxVector3Curve* GetPosCurve() { return mPosCurve; }
	GfxQuaternionCurve* GetQuaternionCurve() { return mRotationCurve; }
	GfxVector3Curve* GetScaleCurve() { return mScaleCurve; }
	GfxMotionStateCurve* GetMotionStateCurve() { return mMotionStateCurve; }
	virtual UINT GetKeyCount() override { return std::max<int>(mPosCurve ? mPosCurve->GetKeyCount() : 0, std::max<int>(mRotationCurve ? mRotationCurve->GetKeyCount() : 0, mScaleCurve ? mScaleCurve->GetKeyCount() : 0)); }

	virtual vBOOL LoadXnd(IRenderContext* rc, XNDNode* node) override;
	virtual void Save2Xnd(XNDNode* node) override;
protected:
	AutoRef<GfxBoneDesc>		mBoneData;
	AutoRef<GfxVector3Curve> mPosCurve;
	AutoRef<GfxQuaternionCurve> mRotationCurve;
	AutoRef<GfxVector3Curve> mScaleCurve;
	AutoRef<GfxMotionStateCurve> mMotionStateCurve;
};

NS_END