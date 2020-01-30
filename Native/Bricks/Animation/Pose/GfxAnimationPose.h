#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Skeleton/GfxBone.h"

NS_BEGIN
class GfxAnimationPose : public VIUnknown
{
public:
	RTTI_DEF(GfxAnimationPose, 0xb33b079c5cc17691, true);
	GfxAnimationPose();
	~GfxAnimationPose();
	/*void BlendWithTargetPose(GfxAnimationPose* targetPose, float targetWeight);
	void FromTwoPosesBlend(GfxAnimationPose* aPose, GfxAnimationPose* bPose, float bWeight);
	void AdditiveCharacterSpacePose(GfxAnimationPose* basePose, GfxAnimationPose* additivePose, float alpha);
	void MinusCharacterSpacePose(GfxAnimationPose* minusPose, GfxAnimationPose* minuendPose);
	void ZeroPose();
	vBOOL IsZeroPose();
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
	GfxAnimationPose* Clone() const;
	inline GfxBone* GetRoot() const {
		return GetBone(mRoot);
	}
	vBOOL SetRoot(const char* name);
	void SetRootByIndex(UINT index) {
		mRoot = index;
	}
	v3dxVector3 ExtractRootMotion(vBOOL OnlyPosition);
	v3dxVector3 ExtractRootMotionPosition(vBOOL ingoreY);
	void GenerateHierarchy();
	void CalculatePose();
	void InitDefaultPose();*/
public:
	UINT							mRoot;
	std::vector<GfxBone*>			mBones;
	std::map<UINT, GfxBone*>		mBoneMap;
public:
	//void Cleanup();
	//void ClearLink();
	//void UpdateBonesMap();
};

NS_END