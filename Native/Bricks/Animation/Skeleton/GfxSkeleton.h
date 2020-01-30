#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "GfxBone.h"

NS_BEGIN
class GfxSkeletonPose;
class GfxBone;
class GfxSkeleton : public VIUnknown
{
public:
	RTTI_DEF(GfxSkeleton, 0x66e0a6cd45c2d57c, true);
	GfxSkeleton();
	~GfxSkeleton();

	UINT GetBoneNumber() {
		return (UINT)mBones.size();
	}
	GfxSkeleton* CloneSkeleton();	
	virtual void Cleanup() override;
	inline GfxBone* GetBone(UINT index) const {
		if (index >= (UINT)mBones.size())
			return NULL;
		return mBones[index];
	}
	inline UINT GetBoneNumber() const {
		return (UINT)mBones.size();
	}
	GfxBone* FindBone(const char* name) const {
		auto iter = mBoneMap.find(GfxBoneDesc::Name2Hash(name));
		if (iter == mBoneMap.end())
			return nullptr;
		return iter->second;
	}
	GfxBone* FindBoneByNameHash(UINT boneNameHashId) const {
		return FindBone(boneNameHashId);
	}
	GfxBone* FindBone(UINT boneNameHashId) const {
		auto iter = mBoneMap.find(boneNameHashId);
		if (iter == mBoneMap.end())
			return nullptr;
		return iter->second;
	}
	GfxBone* NewBone(GfxBoneDesc* desc);
	UINT AddBone(GfxBone* pBone);
	vBOOL RemoveBone(UINT nameHash);
	inline GfxBone* GetRoot() const {
		return GetBone(mRoot);
	}
	vBOOL SetRoot(const char* name);
	void SetRootByIndex(UINT index) {
		mRoot = index;
	}
	void GenerateHierarchy();
	GfxSkeletonPose* CreateSkeletonPose();
public:
	UINT							mRoot;
	std::vector<GfxBone*>			mBones;
	std::map<UINT, GfxBone*>		mBoneMap;
	
};

NS_END