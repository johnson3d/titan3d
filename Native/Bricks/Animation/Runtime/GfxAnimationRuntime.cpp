#include "GfxAnimationRuntime.h"
#include "../Pose/GfxSkeletonPose.h"
#include "../Pose/GfxBonePose.h"
#include "../Skeleton/GfxBone.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxAnimationRuntime, EngineNS::VIUnknown);

GfxAnimationRuntime::GfxAnimationRuntime()
{

}

GfxAnimationRuntime::~GfxAnimationRuntime()
{

}

void GfxAnimationRuntime::BlendPose(GfxSkeletonPose* outPose, GfxSkeletonPose* aPose, GfxSkeletonPose* bPose, float weight)
{
	for (auto iter = outPose->mBoneMap.begin(); iter != outPose->mBoneMap.end(); iter++)
	{
		auto nameHash = iter->first;
		GfxBonePose* myBone = iter->second;
		GfxBonePose* aBone = aPose->FindBonePoseByHash(nameHash);
		GfxBonePose* bBone = bPose->FindBonePoseByHash(nameHash);
		if (aBone && bBone)
		{
			v3dxVec3Lerp(&myBone->Transform.Position, &aBone->Transform.Position, &bBone->Transform.Position, weight);
			v3dxQuaternionSlerp(&myBone->Transform.Rotation, &aBone->Transform.Rotation, &bBone->Transform.Rotation, weight);
		}
		else
		{
			if (aBone)
			{
				myBone->Transform = aBone->Transform;
			}
			if (bBone)
			{
				myBone->Transform = bBone->Transform;
			}
		}
	}
	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->ReferenceBone->GrantChildren)
		{
			auto grantBone = outPose->GetBonePose(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->ReferenceBone->mSharedData->GrantWeight);
		}
	}
}

void GfxAnimationRuntime::AddPose(GfxSkeletonPose* outPose, GfxSkeletonPose* basePose, GfxSkeletonPose* additivePose, float alpha)
{
	for (auto iter = outPose->mBoneMap.begin(); iter != outPose->mBoneMap.end(); iter++)
	{
		auto nameHash = iter->first;
		GfxBonePose* myBone = iter->second;
		GfxBonePose* baseBone = basePose->FindBonePoseByHash(nameHash);
		GfxBonePose* additiveBone = additivePose->FindBonePoseByHash(nameHash);
		if (baseBone && additiveBone)
		{
			auto quat = additiveBone->Transform.Rotation * baseBone->Transform.Rotation;
			auto pos = baseBone->Transform.Position + additiveBone->Transform.Position;
			v3dxVec3Lerp(&myBone->Transform.Position, &baseBone->Transform.Position, &pos, alpha);
			v3dxQuaternionSlerp(&myBone->Transform.Rotation, &baseBone->Transform.Rotation, &quat, alpha);
		}
		else
		{
			if (baseBone)
			{
				myBone->Transform = baseBone->Transform;
			}
			if (additiveBone)
			{
				myBone->Transform = additiveBone->Transform;
			}
		}
		myBone->Transform.Rotation.normalize();
	}
	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->ReferenceBone->GrantChildren)
		{
			auto grantBone = outPose->GetBonePose(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->ReferenceBone->mSharedData->GrantWeight);
		}
	}
}

void GfxAnimationRuntime::MinusPose(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
{
	for (auto iter = outPose->mBoneMap.begin(); iter != outPose->mBoneMap.end(); iter++)
	{
		auto nameHash = iter->first;
		GfxBonePose* myBone = iter->second;
		GfxBonePose* minusBone = minusPose->FindBonePoseByHash(nameHash);
		GfxBonePose* minuendBone = minuendPose->FindBonePoseByHash(nameHash);
		if (minusBone && minuendBone)
		{
			myBone->Transform.Position = minuendBone->Transform.Position - minusBone->Transform.Position;
			myBone->Transform.Rotation = minuendBone->Transform.Rotation * minusBone->Transform.Rotation.inverse();
		}
		else
		{
			if (minusBone)
			{
				myBone->Transform = minusBone->Transform;
			}
			if (minuendBone)
			{
				myBone->Transform = minuendBone->Transform;
			}
		}
		myBone->Transform.Rotation.normalize();
	}

	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->ReferenceBone->GrantChildren)
		{
			auto grantBone = outPose->GetBonePose(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->ReferenceBone->mSharedData->GrantWeight);
		}
	}
}

void GfxAnimationRuntime::FastBlendPose(GfxSkeletonPose* outPose, GfxSkeletonPose* aPose, GfxSkeletonPose* bPose, float weight)
{
	for (int i = 0; i < outPose->mBones.size(); ++i)
	{
		GfxBonePose* myBone = outPose->mBones[i];
		GfxBonePose* aBone = aPose->mBones[i];
		GfxBonePose* bBone = bPose->mBones[i];
		if (aBone && bBone)
		{
			v3dxVec3Lerp(&myBone->Transform.Position, &aBone->Transform.Position, &bBone->Transform.Position, weight);
			v3dxQuaternionSlerp(&myBone->Transform.Rotation, &aBone->Transform.Rotation, &bBone->Transform.Rotation, weight);
		}
		else
		{
			if (aBone)
			{
				myBone->Transform = aBone->Transform;
			}
			if (bBone)
			{
				myBone->Transform = bBone->Transform;
			}
		}
	}
	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->ReferenceBone->GrantChildren)
		{
			auto grantBone = outPose->GetBonePose(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->ReferenceBone->mSharedData->GrantWeight);
		}
	}
}
void GfxAnimationRuntime::FastAddPose(GfxSkeletonPose* outPose, GfxSkeletonPose* basePose, GfxSkeletonPose* additivePose, float alpha)
{
	for (int i = 0; i < outPose->mBones.size(); ++i)
	{
		GfxBonePose* myBone = outPose->mBones[i];
		GfxBonePose* baseBone = basePose->mBones[i];
		GfxBonePose* additiveBone = additivePose->mBones[i];
		if (baseBone && additiveBone)
		{
			auto quat = additiveBone->Transform.Rotation * baseBone->Transform.Rotation;
			auto pos = baseBone->Transform.Position + additiveBone->Transform.Position;
			v3dxVec3Lerp(&myBone->Transform.Position, &baseBone->Transform.Position, &pos, alpha);
			v3dxQuaternionSlerp(&myBone->Transform.Rotation, &baseBone->Transform.Rotation, &quat, alpha);
		}
		else
		{
			if (baseBone)
			{
				myBone->Transform = baseBone->Transform;
			}
			if (additiveBone)
			{
				myBone->Transform = additiveBone->Transform;
			}
		}
		myBone->Transform.Rotation.normalize();
	}
	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->ReferenceBone->GrantChildren)
		{
			auto grantBone = outPose->GetBonePose(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->ReferenceBone->mSharedData->GrantWeight);
		}
	}
}
void GfxAnimationRuntime::FastMinusPose(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
{
	for (int i = 0; i < outPose->mBones.size(); ++i)
	{
		GfxBonePose* myBone = outPose->mBones[i];
		GfxBonePose* minusBone = minusPose->mBones[i];
		GfxBonePose* minuendBone = minuendPose->mBones[i];
		if (minusBone && minuendBone)
		{
			myBone->Transform.Position = minuendBone->Transform.Position - minusBone->Transform.Position;
			myBone->Transform.Rotation = minuendBone->Transform.Rotation * minusBone->Transform.Rotation.inverse();
		}
		else
		{
			if (minusBone)
			{
				myBone->Transform = minusBone->Transform;
			}
			if (minuendBone)
			{
				myBone->Transform = minuendBone->Transform;
			}
		}
		myBone->Transform.Rotation.normalize();
	}

	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->ReferenceBone->GrantChildren)
		{
			auto grantBone = outPose->GetBonePose(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->ReferenceBone->mSharedData->GrantWeight);
		}
	}
}

GfxBoneTransform ConvertBoneToMeshSpace(GfxBonePose* bonePose, GfxSkeletonPose* skeleton)
{
	GfxBoneTransform trans = bonePose->Transform;
	//parent must be meshSpace already
	GfxBonePose* parent = skeleton->FindBonePoseByHash(bonePose->ReferenceBone->mSharedData->ParentHash);
	if (parent != nullptr)
	{
		GfxBoneTransform::Transform(&bonePose->Transform, &bonePose->Transform, &parent->Transform);
		return bonePose->Transform;
	}
	return trans;
}
void MinusBoneMeshSpace(GfxBonePose* bonePose, GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
{
	GfxBonePose* minusBone = minusPose->FindBonePoseByHash(bonePose->ReferenceBone->mSharedData->NameHash);
	GfxBonePose* minuendBone = minuendPose->FindBonePoseByHash(bonePose->ReferenceBone->mSharedData->NameHash);
	auto minusTrans = ConvertBoneToMeshSpace(minusBone, minusPose);
	auto minuendTrans = ConvertBoneToMeshSpace(minuendBone, minuendPose);

	bonePose->Transform.Position = minuendTrans.Position - minusTrans.Position;
	bonePose->Transform.Rotation = minuendTrans.Rotation * minusTrans.Rotation.inverse();
	//bonePose->Transform.Rotation = minusTrans.Rotation * minuendTrans.Rotation.inverse();
	minuendBone->Transform = minusBone->Transform;
	//minusBone->Transform = minuendBone->Transform;

	for (auto i : bonePose->ReferenceBone->Children)
	{
		auto bone = outPose->GetBonePose(i);
		ASSERT(bone != nullptr);
		if (bone == nullptr)
			continue;
		MinusBoneMeshSpace(bone, outPose, minusPose, minuendPose);
	}
}
void FastMinusBoneMeshSpace(GfxBonePose* bonePose, GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
{
	GfxBonePose* minusBone = minusPose->mBones[bonePose->ReferenceBone->IndexInTable];
	GfxBonePose* minuendBone = minuendPose->mBones[bonePose->ReferenceBone->IndexInTable];
	auto minusTrans = ConvertBoneToMeshSpace(minusBone, minusPose);
	auto minuendTrans = ConvertBoneToMeshSpace(minuendBone, minuendPose);

	bonePose->Transform.Position = minuendTrans.Position - minusTrans.Position;
	bonePose->Transform.Rotation = minuendTrans.Rotation * minusTrans.Rotation.inverse();
	//bonePose->Transform.Rotation = minusTrans.Rotation * minuendTrans.Rotation.inverse();
	minuendBone->Transform = minusBone->Transform;
	//minusBone->Transform = minuendBone->Transform;

	for (auto i : bonePose->ReferenceBone->Children)
	{
		auto bone = outPose->GetBonePose(i);
		ASSERT(bone != nullptr);
		if (bone == nullptr)
			continue;
		MinusBoneMeshSpace(bone, outPose, minusPose, minuendPose);
	}
}
void GfxAnimationRuntime::MinusPoseMeshSpace(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
{
	if (outPose->GetBonePoseNumber() == 0)
		return;
	auto root = outPose->GetRootBonePose();
	if (root != nullptr)
	{
		MinusBoneMeshSpace(root, outPose, minusPose, minuendPose);
	}
}
void GfxAnimationRuntime::FastMinusPoseMeshSpace(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
{
	if (outPose->GetBonePoseNumber() == 0)
		return;
	auto root = outPose->GetRootBonePose();
	if (root != nullptr)
	{
		FastMinusBoneMeshSpace(root, outPose, minusPose, minuendPose);
	}
}

void GfxAnimationRuntime::ZeroPose(GfxSkeletonPose* pose)
{
	for (int i = 0; i < pose->mBones.size(); ++i)
	{
		auto& bone = pose->mBones[i];
		bone->Transform = GfxBoneTransform();
	}
}

void GfxAnimationRuntime::ZeroTransition(GfxSkeletonPose* pose)
{
	for (int i = 0; i < pose->mBones.size(); ++i)
	{
		auto& bone = pose->mBones[i];
		bone->Transform.Position = v3dxVector3::ZERO;
	}
}

vBOOL GfxAnimationRuntime::IsZeroPose(GfxSkeletonPose* pose)
{
	if (pose == nullptr)
		return FALSE;
	for (int i = 0; i < pose->mBones.size(); ++i)
	{
		auto& bone = pose->mBones[i];
		//ÔÝÊ±²»¿¼ÂÇScale
		if (!bone->Transform.Position.isZeroLength() || !bone->Transform.Rotation.IsIdentity())
		{
			return FALSE;
		}
	}
	return TRUE;
}

void GfxAnimationRuntime::CopyPose(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
{
	for (auto iter = desPose->mBoneMap.begin(); iter != desPose->mBoneMap.end(); iter++)
	{
		auto nameHash = iter->first;
		GfxBonePose* myBone = iter->second;
		GfxBonePose* aBone = srcPose->FindBonePoseByHash(nameHash);
		if (aBone)
		{
			myBone->Transform = aBone->Transform;
		}
	}
}

void EngineNS::GfxAnimationRuntime::FastCopyPose(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
{
	int count = (int)(desPose->mBones.size() > srcPose->mBones.size() ? desPose->mBones.size() : srcPose->mBones.size());
	for (int i = 0; i < count; ++i)
	{

		desPose->mBones[i]->Transform = srcPose->mBones[i]->Transform;

	}
}

void ConvertToMeshSpaceTransformRecursively(GfxBonePose* parent, GfxSkeletonPose* skeleton)
{
	for (auto i : parent->ReferenceBone->Children)
	{
		auto bone = skeleton->GetBonePose(i);
		ASSERT(bone != nullptr);
		if (bone == nullptr)
			continue;
		GfxBoneTransform::Transform(&bone->Transform, &bone->Transform, &parent->Transform);
		ConvertToMeshSpaceTransformRecursively(bone, skeleton);
	}
}
void GfxAnimationRuntime::ConvertToMeshSpace(GfxSkeletonPose* pose)
{
	if (pose->GetBonePoseNumber() == 0)
		return;
	auto root = pose->GetRootBonePose();
	if (root != nullptr)
	{
		ConvertToMeshSpaceTransformRecursively(root, pose);
	}
	else
	{
		ASSERT(FALSE);
	}
}

void GfxAnimationRuntime::CopyPoseAndConvertMeshSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
{
	if (desPose->GetBonePoseNumber() == 0 || srcPose->GetBonePoseNumber() == 0)
		return;
	CopyPose(desPose, srcPose);
	auto root = desPose->GetRootBonePose();
	if (root != nullptr)
	{
		ConvertToMeshSpaceTransformRecursively(root, desPose);
	}
	else
	{
		ASSERT(FALSE);
	}
}

void GfxAnimationRuntime::ConvertToLocalSpace(GfxSkeletonPose* pose)
{

}
void ConvertToMeshSpaceRotationRecursively(GfxBonePose* parent, GfxSkeletonPose* skeleton)
{
	for (auto i : parent->ReferenceBone->Children)
	{
		auto bone = skeleton->GetBonePose(i);
		ASSERT(bone != nullptr);
		if (bone == nullptr)
			continue;
		//bone->Transform.Rotation = bone->Transform.Rotation * parent->Transform.Rotation;
		bone->Transform.Rotation = bone->Transform.Rotation * parent->Transform.Rotation;
		ConvertToMeshSpaceRotationRecursively(bone, skeleton);
	}
}
void ConvertToLoaclSpaceRotationRecursively(GfxBonePose* parent, GfxSkeletonPose* skeleton)
{
	for (auto i : parent->ReferenceBone->Children)
	{
		auto bone = skeleton->GetBonePose(i);
		ASSERT(bone != nullptr);
		if (bone == nullptr)
			continue;
		ConvertToLoaclSpaceRotationRecursively(bone, skeleton);
		//bone->Transform.Rotation = bone->Transform.Rotation * parent->Transform.Rotation.inverse();
		bone->Transform.Rotation = bone->Transform.Rotation * parent->Transform.Rotation.inverse();
	}
}
void EngineNS::GfxAnimationRuntime::ConvertRotationToMeshSpace(GfxSkeletonPose* pose)
{
	auto root = pose->GetRootBonePose();
	if (root != nullptr)
	{
		ConvertToMeshSpaceRotationRecursively(root, pose);
	}
	else
	{
		ASSERT(FALSE);
	}
}
void EngineNS::GfxAnimationRuntime::ConvertRotationToLocalSpace(GfxSkeletonPose* pose)
{
	auto root = pose->GetRootBonePose();
	if (root != nullptr)
	{
		ConvertToLoaclSpaceRotationRecursively(root, pose);
	}
	else
	{
		ASSERT(FALSE);
	}
}
void GfxAnimationRuntime::CopyPoseAndConvertRotationToLocalSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
{
	if (desPose->GetBonePoseNumber() == 0 || srcPose->GetBonePoseNumber() == 0)
		return;
	CopyPose(desPose, srcPose);
	auto root = desPose->GetRootBonePose();
	if (root != nullptr)
	{
		ConvertToLoaclSpaceRotationRecursively(root, desPose);
	}
	else
	{
		ASSERT(FALSE);
	}
}
void GfxAnimationRuntime::CopyPoseAndConvertRotationToMeshSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
{
	if (desPose->GetBonePoseNumber() == 0 || srcPose->GetBonePoseNumber() == 0)
		return;
	CopyPose(desPose, srcPose);
	auto root = desPose->GetRootBonePose();
	if (root != nullptr)
	{
		ConvertToMeshSpaceRotationRecursively(root, desPose);
	}
	else
	{
		ASSERT(FALSE);
	}
}


NS_END

using namespace EngineNS;
extern "C"
{
	VFX_API vBOOL SDK_GfxAnimationRuntime_IsZeroPose(GfxSkeletonPose* pose)
	{
		return GfxAnimationRuntime::IsZeroPose(pose);
	}
	VFX_API void SDK_GfxAnimationRuntime_ZeroPose(GfxSkeletonPose* pose)
	{
		GfxAnimationRuntime::ZeroPose(pose);
	}
	VFX_API void SDK_GfxAnimationRuntime_ZeroTransition(GfxSkeletonPose* pose)
	{
		GfxAnimationRuntime::ZeroTransition(pose);
	}
	VFX_API void SDK_GfxAnimationRuntime_BlendPose(GfxSkeletonPose* outPose, GfxSkeletonPose* aPose, GfxSkeletonPose* bPose, float weight)
	{
		GfxAnimationRuntime::BlendPose(outPose, aPose, bPose, weight);
	}
	VFX_API void SDK_GfxAnimationRuntime_AddPose(GfxSkeletonPose* outPose, GfxSkeletonPose* basePose, GfxSkeletonPose* additivePose, float alpha)
	{
		GfxAnimationRuntime::AddPose(outPose, basePose, additivePose, alpha);
	}
	VFX_API void SDK_GfxAnimationRuntime_MinusPose(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
	{
		GfxAnimationRuntime::MinusPose(outPose, minusPose, minuendPose);
	}
	VFX_API void SDK_GfxAnimationRuntime_MinusPoseMeshSpace(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
	{
		GfxAnimationRuntime::MinusPoseMeshSpace(outPose, minusPose, minuendPose);
	}
	VFX_API void SDK_GfxAnimationRuntime_FastBlendPose(GfxSkeletonPose* outPose, GfxSkeletonPose* aPose, GfxSkeletonPose* bPose, float weight)
	{
		GfxAnimationRuntime::FastBlendPose(outPose, aPose, bPose, weight);
	}
	VFX_API void SDK_GfxAnimationRuntime_FastAddPose(GfxSkeletonPose* outPose, GfxSkeletonPose* basePose, GfxSkeletonPose* additivePose, float alpha)
	{
		GfxAnimationRuntime::FastAddPose(outPose, basePose, additivePose, alpha);
	}
	VFX_API void SDK_GfxAnimationRuntime_FastMinusPose(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
	{
		GfxAnimationRuntime::FastMinusPose(outPose, minusPose, minuendPose);
	}
	VFX_API void SDK_GfxAnimationRuntime_FastMinusPoseMeshSpace(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose)
	{
		GfxAnimationRuntime::FastMinusPoseMeshSpace(outPose, minusPose, minuendPose);
	}
	VFX_API void SDK_GfxAnimationRuntime_CopyPose(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
	{
		GfxAnimationRuntime::CopyPose(desPose, srcPose);
	}
	VFX_API void SDK_GfxAnimationRuntime_FastCopyPose(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
	{
		GfxAnimationRuntime::FastCopyPose(desPose, srcPose);
	}
	VFX_API void SDK_GfxAnimationRuntime_CopyPoseAndConvertMeshSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
	{
		GfxAnimationRuntime::CopyPoseAndConvertMeshSpace(desPose, srcPose);
	}
	VFX_API void SDK_GfxAnimationRuntime_ConvertToMeshSpace(GfxSkeletonPose* pose)
	{
		GfxAnimationRuntime::ConvertToMeshSpace(pose);
	}
	VFX_API void SDK_GfxAnimationRuntime_ConvertToLocalSpace(GfxSkeletonPose* pose)
	{
		GfxAnimationRuntime::ConvertToLocalSpace(pose);
	}
	VFX_API void SDK_GfxAnimationRuntime_ConvertRotationToMeshSpace(GfxSkeletonPose* pose)
	{
		GfxAnimationRuntime::ConvertRotationToMeshSpace(pose);
	}
	VFX_API void SDK_GfxAnimationRuntime_ConvertRotationToLocalSpace(GfxSkeletonPose* pose)
	{
		GfxAnimationRuntime::ConvertRotationToLocalSpace(pose);
	}
	VFX_API void SDK_GfxAnimationRuntime_CopyPoseAndConvertRotationToMeshSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
	{
		GfxAnimationRuntime::CopyPoseAndConvertRotationToMeshSpace(desPose, srcPose);
	}
	VFX_API void SDK_GfxAnimationRuntime_CopyPoseAndConvertRotationToLocalSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose)
	{
		GfxAnimationRuntime::CopyPoseAndConvertRotationToLocalSpace(desPose, srcPose);
	}
}