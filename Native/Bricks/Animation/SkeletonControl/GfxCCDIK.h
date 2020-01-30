#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Pose/GfxAnimationPose.h"
#include "GfxSkeletonControl.h"

NS_BEGIN
class GfxCCDIK : public GfxSkeletonControl
{
public:
	RTTI_DEF(GfxCCDIK, 0x3bd2fc6c5bd670b4, true);
	GfxCCDIK();
	~GfxCCDIK();

	virtual void Update(vTimeTick time) override;
	vBOOL SetEndEffecterBoneByName(const char* name);
	vBOOL SetTargetBoneByName(const char* name);
	vBOOL SetEndEffecterBoneByNameHash(UINT nameHash);
	vBOOL SetTargetBoneByNameHash(UINT nameHash);
	vBOOL SetEndEffecterBone(GfxBone* bone);
	vBOOL SetTargetBone(GfxBone* bone);
	vBOOL SetEndEffecterBoneByIndex(UINT index);
	vBOOL SetTargetBoneByIndex(UINT index);
	void SetTargetPosition(v3dxVector3 position) {
		mTargetPosition = position;
	}
	UINT GetDepth() const { return mDepth; }
	void SetDepth(UINT val);
	UINT GetIteration() const { return mIteration; }
	void SetIteration(UINT val) { mIteration = val; }
	float GetLimitAngle() const { return mLimitAngle; }
	void SetLimitAngle(float val) { mLimitAngle = val; }
public:
	std::string						mEndEffecterBoneName;
	std::string						mTargetBoneName;
	v3dxVector3						mTargetPosition;
protected:
	void BuildBoneChain(GfxBone* bone, UINT currentDepth);
	void Solve(GfxBone* joint, // 想象成肘部
		v3dxVector3& target, // 目标位置
		GfxBone* end, // 末端效应器
		float limitAngle,// 单位限制角度
		int iterNum);// 迭代次数
	void CalculatePose(UINT currentBoneIndex);
private:
	UINT      mIteration;
	UINT      mDepth;
	float     mLimitAngle;

	AutoRef<GfxBone> mEndEffecterBone;
	AutoRef<GfxBone> mTargetBone;
	std::vector<GfxBone*> mBoneChain;
};
NS_END