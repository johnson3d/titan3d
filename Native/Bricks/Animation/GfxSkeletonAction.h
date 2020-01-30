#pragma once
#include "Skeleton/GfxBone.h"
#include "Pose/GfxAnimationPose.h"

NS_BEGIN

class GfxSkeleton;
class GfxSkeletonAction : public VIUnknown
{
public:
	RTTI_DEF(GfxSkeletonAction, 0x4b0267115b222988, true);
	GfxSkeletonAction();
	~GfxSkeletonAction();
	virtual IResourceState* GetResourceState() override {
		return &mResourceState;
	}
	vBOOL Init(IRenderContext* rc, const char* name);
	virtual void InvalidateResource() override;
	virtual vBOOL RestoreResource() override;
	vBOOL LoadXnd(IRenderContext* rc, const char* name, XNDNode* node, bool isLoad);
	vBOOL MakeTPoseActionFromSkeleton(GfxSkeleton* skeleton, const char* name, bool isLoad);
	void Save2Xnd(XNDNode* node);

	const char* GetName() const{
		return mName.c_str();
	}
	UINT GetBoneNumber() const {
		return (UINT)mBonesAnim.size();
	}
	GfxBoneAnim* GetBoneAnum(UINT index) {
		if (index >= (UINT)mBonesAnim.size())
			return nullptr;
		return mBonesAnim[index];
	}
	GfxBoneAnim* FindBoneAnimByHashId(int hash) {
		auto iter = mBonesAnimMap.find(hash);
		if (iter == mBonesAnimMap.end())
			return nullptr;
		return iter->second;
	}
	void FixBoneTree(GfxSkeleton* skeleton);
	void FixBoneAnimPose(GfxSkeleton* skeleton);
	void GetAnimaPose(vTimeTick time, GfxAnimationPose* outPose,vBOOL withMotionData);
	void GetAnimaPoseWithoutMotionDatas(vTimeTick time, GfxAnimationPose* outPose);
	void GetMotionDatas(vTimeTick time, UINT boneHashName, GfxMotionState* motionData);
	void GetAnimaPoseMotioinData(vTimeTick time, GfxAnimationPose* outPose);
	void CalculateFrameCountAndDuration();
	UINT GetDuration() { return mDuration; }
	UINT GetFrameCount() { return mFrameCount; }
	float GetFps() { return mFps; }
public:
	AutoRef<XNDNode>		mSrcNode;
	IResourceState			mResourceState;
	VStringA				mName;
	TimeKeys				mNotifyTime;
	UINT					mDuration;
	UINT					mFrameCount;
	float					mFps;
	std::vector<GfxBoneAnim*>		mBonesAnim;
	std::map<int, GfxBoneAnim*>		mBonesAnimMap;
};

NS_END
