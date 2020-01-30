#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Skeleton/GfxBone.h"
#include "GfxBonePose.h"
#include "../Skeleton/GfxSkeleton.h"
NS_BEGIN
class GfxSkeletonPose : public VIUnknown
{
public:
	RTTI_DEF(GfxSkeletonPose, 0x62adecb75d5a3856, true);
	GfxSkeletonPose();
	~GfxSkeletonPose();
public:
	inline GfxBonePose* GetBonePose(UINT index) const {
		if (index >= (UINT)mBones.size())
			return NULL;
		return mBones[index];
	}
	inline UINT GetBonePoseNumber() const {
		return (UINT)mBones.size();
	}
	GfxBonePose* FindBonePose(const char* name) const {
		auto iter = mBoneMap.find(GfxBoneDesc::Name2Hash(name));
		if (iter == mBoneMap.end())
			return nullptr;
		return iter->second;
	}
	GfxBonePose* FindBonePoseByHash(UINT boneNameHashId) const {
		return FindBonePose(boneNameHashId);
	}
	GfxBonePose* FindBonePose(UINT boneNameHashId) const {
		auto iter = mBoneMap.find(boneNameHashId);
		if (iter == mBoneMap.end())
			return nullptr;
		return (iter->second);
	}
	GfxBonePose* GetRootBonePose()
	{
		return GetBonePose(ReferenceSkeleton->mRoot);
	}
	void AddBonePose(GfxBonePose* bonePose);
	void SetReferenceSkeleton(GfxSkeleton* skeleton) { ReferenceSkeleton.StrongRef(skeleton); }
	GfxSkeletonPose* Clone();
public:
	std::vector<GfxBonePose*>			mBones;
	std::map<UINT, GfxBonePose*>		mBoneMap;
	AutoRef<GfxSkeleton> ReferenceSkeleton;
};

NS_END