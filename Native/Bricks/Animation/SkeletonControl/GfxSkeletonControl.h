#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Pose/GfxAnimationPose.h"

NS_BEGIN
class GfxSkeletonControl : public VIUnknown
{
public:
	RTTI_DEF(GfxSkeletonControl, 0xb8e503175bd16b5c, true);
	GfxSkeletonControl();
	~GfxSkeletonControl();

	GfxAnimationPose* GetAnimationPose() {
		return mAnimationPose;
	}
	float GetAlpha() const { return mAlpha; }
	void SetAlpha(float val) { mAlpha = val; }
	vBOOL GetEnable() const { return mEnable; }
	void SetEnable(vBOOL val) { mEnable = val; }
	void SetAnimationPose(GfxAnimationPose* pose);
	GfxBone* GetBone(const char* name);
	GfxBone* GetBone(UINT nameHash);
	virtual void Update(vTimeTick time);
public:
	AutoRef<GfxAnimationPose>		mAnimationPose;
protected:
	float							mAlpha;
	vBOOL							mEnable;
};
NS_END