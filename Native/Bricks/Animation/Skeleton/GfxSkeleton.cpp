#include "GfxSkeleton.h"
#include "GfxBone.h"
#include "../Pose/GfxSkeletonPose.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxSkeleton, EngineNS::VIUnknown);

GfxSkeleton::GfxSkeleton()
{

}

GfxSkeleton::~GfxSkeleton()
{

}
GfxSkeleton* GfxSkeleton::CloneSkeleton()
{
	GfxSkeleton* result = new GfxSkeleton();
	int nBone = GetBoneNumber();
	for (INT i = 0; i < nBone; i++)
	{
		auto bone = mBones[i];
		AutoRef<GfxBone> sktBone = new GfxBone(bone->GetBoneDesc());
		result->AddBone(sktBone);
	}
	return result;
}

void GfxSkeleton::Cleanup()
{
	for (auto i : mBones)
	{
		Safe_Release(i);
	}
	mBones.clear();
	mBoneMap.clear();
}

vBOOL GfxSkeleton::SetRoot(const char* name)
{
	auto bone = FindBone(GfxBoneDesc::Name2Hash(name));
	if (bone == nullptr)
		return FALSE;
	mRoot = bone->IndexInTable;
	return TRUE;
}

GfxBone* GfxSkeleton::NewBone(GfxBoneDesc* desc)
{
	auto bone = new GfxBone(desc);
	AddBone(bone);
	return bone;
}

UINT GfxSkeleton::AddBone(GfxBone* pBone)
{
	auto iter = mBoneMap.find(pBone->mSharedData->NameHash);
	if (iter != mBoneMap.end())
	{
		return -1;
	}
	pBone->IndexInTable = (UINT)mBones.size();
	mBones.push_back(pBone);
	mBoneMap.insert(std::make_pair(pBone->mSharedData->NameHash, pBone));
	return (UINT)(mBones.size() - 1);
}

vBOOL EngineNS::GfxSkeleton::RemoveBone(UINT nameHash)
{
	auto mapIter = mBoneMap.find(nameHash);
	if (mapIter == mBoneMap.end())
	{
		return FALSE;
	}

	for (auto i = mBones.begin(); i != mBones.end(); i++)
	{
		if ((*i)->GetBoneDesc()->NameHash == nameHash)
		{
			mBones.erase(i);
			break;
		}
	}
	for (int i = 0; i < mBones.size(); ++i)
	{
		mBones[i]->IndexInTable = i;
	}
	(*mapIter).second->Release();
	mBoneMap.erase(mapIter);
	return TRUE;
}

void GfxSkeleton::GenerateHierarchy()
{
	for (size_t i = 0; i < mBones.size(); i++)
	{
		mBones[i]->Children.clear();
	}
	//Build mFullBones BoneTree
	for (size_t i = 0; i < mBones.size(); i++)
	{
		auto bone = mBones[i];
		if (bone->mSharedData->Parent == "")
		{
			if (mRoot == i)
				continue;
			if (mRoot == -1)
			{
				mRoot = (UINT)i;
			}
			else
			{
				auto parent = GetRoot();
				if (parent != nullptr)
					parent->Children.push_back((UINT)i);
			}
		}
		else
		{
			auto parent = FindBone(bone->mSharedData->ParentHash);
			if (parent != nullptr)
				parent->Children.push_back((UINT)i);
			auto grantParent = FindBone(bone->mSharedData->GrantParentHash);
			if (grantParent != nullptr)
				grantParent->GrantChildren.push_back((UINT)i);
		}
	}
}
GfxSkeletonPose* GfxSkeleton::CreateSkeletonPose()
{
	GfxSkeletonPose* pose = new GfxSkeletonPose();
	pose->ReferenceSkeleton = this;
	for (int i = 0; i < mBones.size(); ++i)
	{

		auto pHash = mBones[i]->mSharedData->ParentHash;
		auto parentBone = FindBone(pHash);
		auto mat = mBones[i]->mSharedData->InitMatrix;
		if (parentBone != NULL)
		{
			v3dxMatrix4 parentInvMat = v3dxMatrix4::IDENTITY;;
			parentInvMat = parentBone->mSharedData->InvInitMatrix;
			mat = mat * parentInvMat;
		}
		v3dxVector3 pos, scale;
		v3dxQuaternion rot;
		mat.ExtractionTrans(pos);
		mat.ExtractionRotation(rot);
		mat.ExtractionScale(scale);

		GfxBoneTransform transform;
		transform.Position = pos;
		transform.Rotation = rot;
		transform.Scale = scale;

		GfxBonePose* bonePose = new GfxBonePose();
		bonePose->Transform = transform;
		bonePose->ReferenceBone = mBones[i];
		pose->mBones.push_back(bonePose);
		pose->mBoneMap.insert(std::make_pair(mBones[i]->mSharedData->NameHash, bonePose));
	}
	return pose;
}


NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI0(UINT, EngineNS, GfxSkeleton, GetBoneNumber);
	CSharpReturnAPI0(GfxSkeleton*, EngineNS, GfxSkeleton, CloneSkeleton);
	CSharpReturnAPI0(GfxBone*, EngineNS, GfxSkeleton, GetRoot);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxSkeleton, SetRoot, const char*);
	CSharpAPI1(EngineNS, GfxSkeleton, SetRootByIndex, UINT);
	CSharpReturnAPI1(GfxBone*, EngineNS, GfxSkeleton, FindBone, const char*);
	CSharpReturnAPI1(GfxBone*, EngineNS, GfxSkeleton, FindBoneByNameHash, UINT);
	CSharpReturnAPI1(GfxBone*, EngineNS, GfxSkeleton, GetBone, UINT);
	CSharpReturnAPI1(GfxBone*, EngineNS, GfxSkeleton, NewBone, GfxBoneDesc*);
	CSharpReturnAPI1(UINT, EngineNS, GfxSkeleton, AddBone, GfxBone*);
	CSharpReturnAPI1(vBOOL, EngineNS, GfxSkeleton, RemoveBone, UINT);
	CSharpAPI0(EngineNS, GfxSkeleton, GenerateHierarchy);
}
