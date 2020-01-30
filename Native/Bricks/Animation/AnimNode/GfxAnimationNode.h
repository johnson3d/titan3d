#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Skeleton/GfxSkeleton.h"
#include "../Pose/GfxAnimationPose.h"

NS_BEGIN

class GfxSkeletonAction;

///Base class of AnimationSegement,Blend Space,Aim Space 
class GfxAnimationNode : public VIUnknown
{
public:
	RTTI_DEF(GfxAnimationNode, 0x127733145bcacd60, true);
	GfxAnimationNode();
	~GfxAnimationNode();

	virtual vBOOL LoadXnd(IRenderContext* rc, const char* name, XNDNode* node, bool isLoad);
	virtual void Save2Xnd(XNDNode* node);
	GfxAnimationPose* GetAnimationPose() {
		return mAnimationPose;
	}
	void SetAnimationPose(GfxAnimationPose* pose);
	float GetPlayRate() const { return mPlayRate; }
	void SetPlayRate(float val) { mPlayRate = val; }
	UINT GetCurrentTime() const { return mCurrentTime; }
	void SetCurrentTime(UINT val) { mCurrentTime = val; }
	UINT GetDuration() const { return mDuration; }
	void SetDuration(UINT val) { mDuration = val; }
	UINT GetFrameCount() const { return mFrameCount; }
	void SetFrameCount(UINT val) { mFrameCount = val; }
	float GetFps() const { return mFps; }
	void SetFps(float val) { mFps = val; }
	virtual void Update(vTimeTick time) {};

public:
	AutoRef<GfxAnimationPose>		mAnimationPose;
	UINT							mFrameCount;
	UINT							mDuration; //∫¡√Î
	float							mFps;
	UINT							mCurrentTime; //∫¡√Î
	float							mPlayRate;

};

NS_END