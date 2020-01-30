#include "GfxCCDIK.h"
#include "../Pose/GfxAnimationPose.h"
#include "../../../Math/v3dxMath.h"
#include "../../../Math/v3dxMath.h"
#include "../../../Core/vfxSampCounter.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxCCDIK, EngineNS::VIUnknown);

GfxCCDIK::GfxCCDIK()
	:mTargetBone(NULL)
	, mEndEffecterBone(NULL)
	, mDepth(1)
	, mIteration(15)
	, mLimitAngle(180)
{

}

EngineNS::GfxCCDIK::~GfxCCDIK()
{

}

vBOOL EngineNS::GfxCCDIK::SetTargetBone(GfxBone* bone)
{
	auto refBone = mAnimationPose->FindBone(bone->mSharedData->NameHash);
	if (!refBone)
		return FALSE;
	mTargetBoneName = refBone->mSharedData->Name.c_str();
	mTargetBone.StrongRef(refBone);
	return TRUE;
}

vBOOL EngineNS::GfxCCDIK::SetEndEffecterBone(GfxBone* bone)
{
	auto refBone = mAnimationPose->FindBone(bone->mSharedData->NameHash);
	if (!refBone)
		return FALSE;
	mEndEffecterBoneName = refBone->mSharedData->Name.c_str();
	mEndEffecterBone.StrongRef(refBone);
	return TRUE;
}

vBOOL EngineNS::GfxCCDIK::SetTargetBoneByNameHash(UINT nameHash)
{
	auto refBone = mAnimationPose->FindBone(nameHash);
	if (!refBone)
		return FALSE;
	mTargetBoneName = refBone->mSharedData->Name.c_str();
	mTargetBone.StrongRef(refBone);
	return TRUE;
}

vBOOL EngineNS::GfxCCDIK::SetEndEffecterBoneByNameHash(UINT nameHash)
{
	auto refBone = mAnimationPose->FindBone(nameHash);
	if (!refBone)
		return FALSE;
	mEndEffecterBoneName = refBone->mSharedData->Name.c_str();
	mEndEffecterBone.StrongRef(refBone);
	return TRUE;
}


vBOOL EngineNS::GfxCCDIK::SetTargetBoneByIndex(UINT index)
{
	auto refBone = mAnimationPose->GetBone(index);
	if (!refBone)
		return FALSE;
	mTargetBoneName = refBone->mSharedData->Name.c_str();
	mTargetBone.StrongRef(refBone);
	return TRUE;
}

vBOOL EngineNS::GfxCCDIK::SetEndEffecterBoneByIndex(UINT index)
{
	auto refBone = mAnimationPose->GetBone(index);
	if (!refBone)
		return FALSE;
	mEndEffecterBoneName = refBone->mSharedData->Name.c_str();
	mEndEffecterBone.StrongRef(refBone);
	return TRUE;
}

vBOOL EngineNS::GfxCCDIK::SetEndEffecterBoneByName(const char* name)
{
	mEndEffecterBoneName = name;
	auto bone = mAnimationPose->FindBone(mEndEffecterBoneName.c_str());
	if (!bone)
		return FALSE;
	mEndEffecterBone.StrongRef(bone);
	return TRUE;
}


vBOOL EngineNS::GfxCCDIK::SetTargetBoneByName(const char* name)
{
	mTargetBoneName = name;
	auto bone = mAnimationPose->FindBone(mTargetBoneName.c_str());
	if (!bone)
		return FALSE;
	mTargetBone.StrongRef(bone);
	return TRUE;
}



void EngineNS::GfxCCDIK::SetDepth(UINT val)
{
	mDepth = val;
	auto bone = GetBone(mEndEffecterBoneName.c_str());
	BuildBoneChain(bone, mDepth);
}

void EngineNS::GfxCCDIK::BuildBoneChain(GfxBone* bone, UINT currentDepth)
{
	if (!bone)
		return;
	if (currentDepth < 1)
		return;
	auto parentBone = GetBone(bone->mSharedData->ParentHash);
	if (!parentBone)
		return;
	mBoneChain.push_back(parentBone);
	currentDepth--;
	BuildBoneChain(parentBone, currentDepth);
}

void EngineNS::GfxCCDIK::Solve(GfxBone* joint,
	v3dxVector3&  target,
	GfxBone* end,
	float limitAngle,
	int iterNum)
{
	auto j2e = end->AbsTransform.Position - joint->AbsTransform.Position;
	auto j2t = target - joint->AbsTransform.Position;
	v3dxQuaternion invRot = joint->AbsTransform.Rotation.inverse();
	auto localJ2E = invRot * j2e;
	auto localJ2T = invRot * j2t;
	localJ2E.normalize();
	localJ2T.normalize();

	float deltaAngle = Math::ACos(localJ2T.dotProduct(localJ2E));
	if (std::isnan(deltaAngle) || deltaAngle < 0.0001f)
		return;

	deltaAngle = Math::clamp(-limitAngle, deltaAngle, limitAngle);

	auto axis = localJ2E.crossProduct(localJ2T);
	v3dxQuaternion detaRotation;
	detaRotation.FromAngleAxis(deltaAngle, axis);
	v3dxVector3 rot;
	v3dxYawPitchRollQuaternionRotation(detaRotation, &rot);
	//auto locRot = joint->Transform.Rotation * detaRotation;
	auto locRot = detaRotation * joint->Transform.Rotation;
	//joint->Transform.Position = mTargetPosition;
	auto constraint = joint->mSharedData->MotionConstraint;
	if (constraint.ConstraintType & ConstraintType::Rotation)
	{
		v3dxYawPitchRollQuaternionRotation(locRot, &rot);
		rot.x = Math::clamp(rot.x, constraint.MinRotation.x, constraint.MaxRotation.x);
		rot.y = Math::clamp(rot.y, constraint.MinRotation.y, constraint.MaxRotation.y);
		rot.z = Math::clamp(rot.z, constraint.MinRotation.z, constraint.MaxRotation.z);
		v3dxQuaternionRotationYawPitchRoll(&locRot, rot.y, rot.x, rot.z);
	}
	joint->Transform.Rotation.slerp(locRot, mAlpha);
	//auto parentBone = mAnimationPose->FindBone(joint->mSharedData->ParentHash);
	//if (parentBone)
	//{
	//	joint->AbsTransform.Rotation = (locRot * parentBone->AbsTransform.Rotation);
	//}
	//else
	//{
	//	joint->AbsTransform.Rotation = locRot;
	//}
}

void EngineNS::GfxCCDIK::CalculatePose(UINT currentBoneIndex)
{
	//第一根骨
	auto bone = mBoneChain[currentBoneIndex];
	auto parB = mAnimationPose->FindBone(bone->mSharedData->ParentHash);
	if (parB)
		GfxBoneTransform::Transform(&bone->AbsTransform, &bone->Transform, &parB->AbsTransform);
	else
		bone->AbsTransform = bone->Transform;
	//中间骨
	for (int i = currentBoneIndex; i > 0; --i)
	{
		auto parentBone = mBoneChain[i];
		auto bone = mBoneChain[i - 1];
		GfxBoneTransform::Transform(&bone->AbsTransform, &bone->Transform, &parentBone->AbsTransform);
	}
	//endEffecter骨
	GfxBoneTransform::Transform(&mEndEffecterBone->AbsTransform, &mEndEffecterBone->Transform, &mBoneChain[0]->AbsTransform);
}

void GfxCCDIK::Update(vTimeTick time)
{
	AUTO_SAMP("Native.GfxCCDIK.Update");
	if (!mEnable)
		return;
	if (!mEndEffecterBone)
		return;
	if (mTargetBone)
		mTargetPosition = mTargetBone->AbsTransform.Position;
	for (UINT i = 0; i < mIteration; ++i)
	{
		for (int j = 0; j < mBoneChain.size(); ++j)
		{
			auto joint = mBoneChain[j];
			Solve(joint, mTargetPosition, mEndEffecterBone, mLimitAngle, i);
			CalculatePose(j);
		}
		if ((mTargetPosition - mEndEffecterBone->AbsTransform.Position).getLengthSq() < 0.00001f)
			break;
	}
	//mEndEffecterBone->LinkBone(mAnimationPose);
	for (auto bone : mBoneChain)
	{
		for (auto j : bone->GrantChildren)
		{
			auto grantBone = mAnimationPose->GetBone(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
			//grantBone->AbsTransform.Position = (bone->AbsTransform.Position - grantBone->AbsTransform.Position) * grantBone->mSharedData->GrantWeight;
		}
	}
	mBoneChain[mBoneChain.size() - 1]->LinkBone(mAnimationPose);

}




NS_END

using namespace EngineNS;




extern "C"
{
	CSharpReturnAPI1(vBOOL, EngineNS, GfxCCDIK, SetEndEffecterBoneByName, const char*);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxCCDIK, SetTargetBoneByName, const char*);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxCCDIK, SetEndEffecterBoneByNameHash, UINT);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxCCDIK, SetTargetBoneByNameHash, UINT);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxCCDIK, SetEndEffecterBoneByIndex, UINT);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxCCDIK, SetTargetBoneByIndex, UINT);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxCCDIK, SetEndEffecterBone, GfxBone*);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxCCDIK, SetTargetBone, GfxBone*);
	CSharpAPI1(EngineNS, GfxCCDIK, SetTargetPosition, v3dxVector3);
	CSharpAPI1(EngineNS, GfxCCDIK, SetDepth, UINT);
	CSharpAPI1(EngineNS, GfxCCDIK, SetIteration, UINT);
	CSharpAPI1(EngineNS, GfxCCDIK, SetLimitAngle, float);
}
