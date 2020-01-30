#include "GfxAnimationPose.h"
#include "../GfxBoneAnim.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxAnimationPose, EngineNS::VIUnknown);

GfxAnimationPose::GfxAnimationPose()
{
	mRoot = -1;
}

GfxAnimationPose::~GfxAnimationPose()
{
	Cleanup();
}
//
//void GfxAnimationPose::BlendWithTargetPose(GfxAnimationPose* targetPose, float targetWeight)
//{
//	for (auto iter = mBoneMap.begin(); iter != mBoneMap.end(); iter++)
//	{
//		auto nameHash = iter->first;
//		GfxBone* sourceBone = iter->second;
//		GfxBone* targetBone = targetPose->FindBoneByNameHash(nameHash);
//		if (targetBone)
//		{
//			v3dxVec3Lerp(&sourceBone->Transform.Position, &sourceBone->Transform.Position, &targetBone->Transform.Position, targetWeight);
//			v3dxQuaternionSlerp(&sourceBone->Transform.Rotation, &sourceBone->Transform.Rotation, &targetBone->Transform.Rotation, targetWeight);
//		}
//	}
//	auto root = GetRoot();
//	if (root != nullptr)
//	{
//		root->AbsTransform = root->Transform;
//		root->LinkBone(this);
//	}
//	else
//	{
//		ASSERT(FALSE);
//	}
//	for (auto bone : mBones)
//	{
//		for (auto j : bone->GrantChildren)
//		{
//			auto grantBone = GetBone(j);
//			ASSERT(grantBone != nullptr);
//			if (grantBone == nullptr)
//				continue;
//			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
//			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
//		}
//	}
//}
//
//void GfxAnimationPose::FromTwoPosesBlend(GfxAnimationPose* aPose, GfxAnimationPose* bPose, float bWeight)
//{
//	for (auto iter = mBoneMap.begin(); iter != mBoneMap.end(); iter++)
//	{
//		auto nameHash = iter->first;
//		GfxBone* myBone = iter->second;
//		GfxBone* aBone = aPose->FindBoneByNameHash(nameHash);
//		GfxBone* bBone = bPose->FindBoneByNameHash(nameHash);
//		if (aBone && bBone)
//		{
//			v3dxVec3Lerp(&myBone->Transform.Position, &aBone->Transform.Position, &bBone->Transform.Position, bWeight);
//			v3dxQuaternionSlerp(&myBone->Transform.Rotation, &aBone->Transform.Rotation, &bBone->Transform.Rotation, bWeight);
//		}
//		else
//		{
//			if (aBone)
//			{
//				myBone->Transform = aBone->Transform;
//			}
//			if (bBone)
//			{
//				myBone->Transform = bBone->Transform;
//			}
//		}
//	}
//	auto root = GetRoot();
//	if (root != nullptr)
//	{
//		root->AbsTransform = root->Transform;
//		root->LinkBone(this);
//	}
//	else
//	{
//		ASSERT(FALSE);
//	}
//	for (auto bone : mBones)
//	{
//		for (auto j : bone->GrantChildren)
//		{
//			auto grantBone = GetBone(j);
//			ASSERT(grantBone != nullptr);
//			if (grantBone == nullptr)
//				continue;
//			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
//			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
//		}
//	}
//}
//
//void EngineNS::GfxAnimationPose::AdditiveCharacterSpacePose(GfxAnimationPose* basePose, GfxAnimationPose* additivePose, float alpha)
//{
//	for (auto iter = mBoneMap.begin(); iter != mBoneMap.end(); iter++)
//	{
//		auto nameHash = iter->first;
//		GfxBone* myBone = iter->second;
//		GfxBone* baseBone = basePose->FindBoneByNameHash(nameHash);
//		GfxBone* additiveBone = additivePose->FindBoneByNameHash(nameHash);
//		if (baseBone && additiveBone)
//		{
//			auto quat = additiveBone->Transform.Rotation * baseBone->Transform.Rotation;
//			auto pos = baseBone->Transform.Position + additiveBone->Transform.Position;
//			v3dxVec3Lerp(&myBone->Transform.Position, &baseBone->Transform.Position, &pos, alpha);
//			v3dxQuaternionSlerp(&myBone->Transform.Rotation, &baseBone->Transform.Rotation, &quat, alpha);
//			
//		}
//		else
//		{
//			if (baseBone)
//			{
//				myBone->Transform = baseBone->Transform;
//			}
//			if (additiveBone)
//			{
//				myBone->Transform = additiveBone->Transform;
//			}
//		}
//	}
//	auto root = GetRoot();
//	if (root != nullptr)
//	{
//		root->AbsTransform = root->Transform;
//		root->LinkBone(this);
//	}
//	else
//	{
//		ASSERT(FALSE);
//	}
//	for (auto bone : mBones)
//	{
//		for (auto j : bone->GrantChildren)
//		{
//			auto grantBone = GetBone(j);
//			ASSERT(grantBone != nullptr);
//			if (grantBone == nullptr)
//				continue;
//			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
//			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
//		}
//	}
//}
//
//void EngineNS::GfxAnimationPose::MinusCharacterSpacePose(GfxAnimationPose* minusPose, GfxAnimationPose* minuendPose)
//{
//	for (auto iter = mBoneMap.begin(); iter != mBoneMap.end(); iter++)
//	{
//		auto nameHash = iter->first;
//		GfxBone* myBone = iter->second;
//		GfxBone* minusBone = minusPose->FindBoneByNameHash(nameHash);
//		GfxBone* minuendBone = minuendPose->FindBoneByNameHash(nameHash);
//		if (minusBone && minuendBone)
//		{
//			//v3dxVec3Lerp(&myBone->Transform.Position, &minusBone->Transform.Position, &minuendBone->Transform.Position, bWeight);
//			//v3dxQuaternionSlerp(&myBone->Transform.Rotation, &minusBone->Transform.Rotation, &minuendBone->Transform.Rotation, bWeight);
//			myBone->Transform.Position = minuendBone->Transform.Position - minusBone->Transform.Position;
//			myBone->Transform.Rotation = minuendBone->Transform.Rotation * minusBone->Transform.Rotation.inverse();
//		}
//		else
//		{
//			if (minusBone)
//			{
//				myBone->Transform = minusBone->Transform;
//			}
//			if (minuendBone)
//			{
//				myBone->Transform = minuendBone->Transform;
//			}
//		}
//	}
//	auto root = GetRoot();
//	if (root != nullptr)
//	{
//		root->AbsTransform = root->Transform;
//		root->LinkBone(this);
//	}
//	else
//	{
//		ASSERT(FALSE);
//	}
//	for (auto bone : mBones)
//	{
//		for (auto j : bone->GrantChildren)
//		{
//			auto grantBone = GetBone(j);
//			ASSERT(grantBone != nullptr);
//			if (grantBone == nullptr)
//				continue;
//			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
//			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
//		}
//	}
//}
//
//void GfxAnimationPose::ZeroPose()
//{
//	for (int i = 0; i < mBones.size(); ++i)
//	{
//		auto& bone = mBones[i];
//		bone->Transform = GfxBoneTransform();
//		bone->AbsTransform = GfxBoneTransform();
//	}
//}
//
//vBOOL GfxAnimationPose::IsZeroPose()
//{
//	for (int i = 0; i < mBones.size(); ++i)
//	{
//		auto& bone = mBones[i];
//		//ÔÝÊ±²»¿¼ÂÇScale
//		if (!bone->Transform.Position.isZeroLength() || !bone->Transform.Rotation.IsIdentity())
//		{
//			return FALSE;
//		}
//	}
//	return TRUE;
//}
//
//void GfxAnimationPose::Cleanup()
//{
//	for (auto i : mBones)
//	{
//		Safe_Release(i);
//	}
//	mBones.clear();
//	mBoneMap.clear();
//}
//
//v3dxVector3 GfxAnimationPose::ExtractRootMotion(vBOOL OnlyPosition)
//{
//	auto root = GetRoot();
//	auto rootTransform = root->Transform;
//	if (OnlyPosition == TRUE)
//	{
//		rootTransform.Position = v3dxVector3::ZERO;
//	}
//	else
//	{
//		rootTransform = GfxBoneTransform();
//	}
//	if (root != nullptr)
//	{
//		root->Transform = GfxBoneTransform();
//		root->AbsTransform = rootTransform;
//		root->LinkBone(this);
//	}
//	else
//	{
//		ASSERT(FALSE);
//	}
//	for (auto bone : mBones)
//	{
//		for (auto j : bone->GrantChildren)
//		{
//			auto grantBone = GetBone(j);
//			ASSERT(grantBone != nullptr);
//			if (grantBone == nullptr)
//				continue;
//			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
//			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
//		}
//	}
//	return rootTransform.Position;
//}
//v3dxVector3 GfxAnimationPose::ExtractRootMotionPosition(vBOOL ingoreY)
//{
//	auto root = GetRoot();
//	auto rootTransform = root->Transform;
//	rootTransform.Position.x = 0;
//	rootTransform.Position.z = 0;
//	if (!ingoreY)
//		rootTransform.Position.y = 0;
//	if (root != nullptr)
//	{
//		root->Transform = GfxBoneTransform();
//		root->AbsTransform = rootTransform;
//		root->LinkBone(this);
//	}
//	else
//	{
//		ASSERT(FALSE);
//	}
//	for (auto bone : mBones)
//	{
//		for (auto j : bone->GrantChildren)
//		{
//			auto grantBone = GetBone(j);
//			ASSERT(grantBone != nullptr);
//			if (grantBone == nullptr)
//				continue;
//			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
//			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
//		}
//	}
//	return rootTransform.Position;
//}
//
//vBOOL GfxAnimationPose::SetRoot(const char* name)
//{
//	auto bone = FindBone(GfxBoneDesc::Name2Hash(name));
//	if (bone == nullptr)
//		return FALSE;
//	mRoot = bone->IndexInTable;
//	return TRUE;
//}
//
//GfxBone* GfxAnimationPose::NewBone(GfxBoneDesc* desc)
//{
//	auto bone = new GfxBone(desc);
//	AddBone(bone);
//	return bone;
//}
//
//UINT GfxAnimationPose::AddBone(GfxBone* pBone)
//{
//	auto iter = mBoneMap.find(pBone->mSharedData->NameHash);
//	if (iter != mBoneMap.end())
//	{
//		return -1;
//	}
//	pBone->IndexInTable = (UINT)mBones.size();
//	pBone->AddRef();
//	mBones.push_back(pBone);
//	mBoneMap.insert(std::make_pair(pBone->mSharedData->NameHash, pBone));
//	return (UINT)(mBones.size() - 1);
//}
//
//vBOOL EngineNS::GfxAnimationPose::RemoveBone(UINT nameHash)
//{
//	auto mapIter = mBoneMap.find(nameHash);
//	if (mapIter == mBoneMap.end())
//	{
//		return FALSE;
//	}
//
//	for (auto i = mBones.begin(); i != mBones.end(); i++)
//	{
//		if ((*i)->GetBoneDesc()->NameHash == nameHash)
//		{
//			mBones.erase(i);
//			break;
//		}
//	}
//	for (int i = 0; i < mBones.size(); ++i)
//	{
//		mBones[i]->IndexInTable = i;
//	}
//	(*mapIter).second->Release();
//	mBoneMap.erase(mapIter);
//	return TRUE;
//}
//
//void GfxAnimationPose::GenerateHierarchy()
//{
//	for (size_t i = 0; i < mBones.size(); i++)
//	{
//		mBones[i]->Children.clear();
//	}
//	//Build mFullBones BoneTree
//	for (size_t i = 0; i < mBones.size(); i++)
//	{
//		auto bone = mBones[i];
//		if (bone->mSharedData->Parent == "" || !FindBone(bone->mSharedData->ParentHash))
//		{
//			if (mRoot == i)
//				continue;
//			if (mRoot == -1)
//			{
//				mRoot = (UINT)i;
//			}
//			else
//			{
//				auto parent = GetRoot();
//				if (parent != nullptr)
//					parent->Children.push_back((UINT)i);
//			}
//		}
//		else
//		{
//			auto parent = FindBone(bone->mSharedData->ParentHash);
//			if (parent != nullptr)
//				parent->Children.push_back((UINT)i);
//			auto grantParent = FindBone(bone->mSharedData->GrantParentHash);
//			if (grantParent != nullptr)
//				grantParent->GrantChildren.push_back((UINT)i);
//		}
//	}
//}
//
//void EngineNS::GfxAnimationPose::CalculatePose()
//{
//	auto root = GetRoot();
//	if (root != nullptr)
//	{
//		root->AbsTransform = root->Transform;
//		root->LinkBone(this);
//	}
//	else
//	{
//		ASSERT(FALSE);
//	}
//	for (auto bone : mBones)
//	{
//		for (auto j : bone->GrantChildren)
//		{
//			auto grantBone = GetBone(j);
//			ASSERT(grantBone != nullptr);
//			if (grantBone == nullptr)
//				continue;
//			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
//			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
//		}
//	}
//}
//
//void GfxAnimationPose::InitDefaultPose()
//{
//	GenerateHierarchy();
//	for (auto i : mBones)
//	{
//		auto pHash = i->mSharedData->ParentHash;
//		auto parentBone = FindBone(pHash);
//		auto mat = i->mSharedData->InitMatrix;
//		if (parentBone != NULL)
//		{
//			v3dxMatrix4 parentInvMat = v3dxMatrix4::IDENTITY;;
//			parentInvMat = parentBone->mSharedData->InvInitMatrix;
//			mat = mat * parentInvMat;
//		}
//		v3dxVector3 pos, scale;
//		v3dxQuaternion rot;
//		mat.ExtractionTrans(pos);
//		mat.ExtractionRotation(rot);
//		mat.ExtractionScale(scale);
//
//		GfxBoneTransform transform;
//		transform.Position = pos;
//		transform.Rotation = rot;
//		transform.Scale = scale;
//
//		i->Transform = transform;
//	}
//	auto root = GetRoot();
//	if (root != nullptr)
//	{
//		root->AbsTransform = root->Transform;
//		root->LinkBone(this);
//	}
//	else
//	{
//		ASSERT(FALSE);
//	}
//	for (auto bone : mBones)
//	{
//		for (auto j : bone->GrantChildren)
//		{
//			auto grantBone = GetBone(j);
//			ASSERT(grantBone != nullptr);
//			if (grantBone == nullptr)
//				continue;
//			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
//			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
//		}
//	}
//}
//GfxAnimationPose* GfxAnimationPose::Clone() const
//{
//	GfxAnimationPose* cloneObj = new GfxAnimationPose();
//	for (auto i : mBones)
//	{
//		AutoRef<GfxBone> bone = new GfxBone(i->mSharedData);
//		bone->BoneAnim = i->BoneAnim;
//		bone->IndexInTable = i->IndexInTable;
//		bone->Children = i->Children;
//		bone->GrantChildren = i->GrantChildren;
//		bone->Transform = i->Transform;
//		bone->AbsTransform = i->AbsTransform;
//		cloneObj->AddBone(bone);
//	}
//	cloneObj->mRoot = mRoot;
//	return cloneObj;
//}

NS_END

using namespace EngineNS;



extern "C"
{
	//CSharpAPI0(EngineNS, GfxAnimationPose, ZeroPose);
	//CSharpReturnAPI0(vBOOL, EngineNS, GfxAnimationPose, IsZeroPose);
	//CSharpAPI0(EngineNS, GfxAnimationPose, InitDefaultPose);
	//CSharpAPI2(EngineNS, GfxAnimationPose, BlendWithTargetPose, GfxAnimationPose*, float);
	//CSharpAPI3(EngineNS, GfxAnimationPose, FromTwoPosesBlend, GfxAnimationPose*, GfxAnimationPose*, float);
	//CSharpAPI3(EngineNS, GfxAnimationPose, AdditiveCharacterSpacePose, GfxAnimationPose*, GfxAnimationPose*, float);
	//CSharpAPI2(EngineNS, GfxAnimationPose, MinusCharacterSpacePose, GfxAnimationPose*, GfxAnimationPose*);
	//CSharpReturnAPI0(GfxBone*, EngineNS, GfxAnimationPose, GetRoot);
	//CSharpReturnAPI1(vBOOL, EngineNS, GfxAnimationPose, SetRoot, const char*);
	//CSharpAPI1(EngineNS, GfxAnimationPose, SetRootByIndex, UINT);
	//CSharpReturnAPI1(v3dVector3_t, EngineNS, GfxAnimationPose, ExtractRootMotion, vBOOL);
	//CSharpReturnAPI1(v3dVector3_t, EngineNS, GfxAnimationPose, ExtractRootMotionPosition, vBOOL);
	//CSharpReturnAPI1(GfxBone*, EngineNS, GfxAnimationPose, FindBone, const char*);
	//CSharpReturnAPI1(GfxBone*, EngineNS, GfxAnimationPose, FindBoneByNameHash, UINT);
	//CSharpReturnAPI0(UINT, EngineNS, GfxAnimationPose, GetBoneNumber);
	//CSharpReturnAPI1(GfxBone*, EngineNS, GfxAnimationPose, GetBone, UINT);
	//CSharpReturnAPI1(GfxBone*, EngineNS, GfxAnimationPose, NewBone, GfxBoneDesc*);
	//CSharpReturnAPI1(UINT, EngineNS, GfxAnimationPose, AddBone, GfxBone*);
	//CSharpReturnAPI1(vBOOL, EngineNS, GfxAnimationPose, RemoveBone, UINT);
	//CSharpAPI0(EngineNS, GfxAnimationPose, GenerateHierarchy);
	//CSharpAPI0(EngineNS, GfxAnimationPose, CalculatePose);
	//CSharpReturnAPI0(GfxAnimationPose*, EngineNS, GfxAnimationPose, Clone);
}