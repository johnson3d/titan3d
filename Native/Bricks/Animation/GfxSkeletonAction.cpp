#include "GfxSkeletonAction.h"
#include "GfxBoneAnim.h"
#include "Skeleton/GfxSkeleton.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxSkeletonAction, EngineNS::VIUnknown);

GfxSkeletonAction::GfxSkeletonAction():mFps(30)
{
}

GfxSkeletonAction::~GfxSkeletonAction()
{
	InvalidateResource();
}

vBOOL EngineNS::GfxSkeletonAction::Init(IRenderContext* rc, const char* name)
{
	mName = name;
	return TRUE;
}
void GfxSkeletonAction::InvalidateResource()
{
	for (auto i : mBonesAnim)
	{
		Safe_Release(i);
	}
	mBonesAnim.clear();
}

vBOOL GfxSkeletonAction::RestoreResource()
{
	if (!mSrcNode)
		return FALSE;
	XNDNode* boneNode = mSrcNode->GetChild(vT("BonesData"));
	if (boneNode)
	{
		XNDNode::XNDAttribVector& bone_attrs = boneNode->GetAttribVector();
		ASSERT(bone_attrs.size() == mBonesAnim.size());
		for (size_t i = 0; i < bone_attrs.size(); i++)
		{
			bone_attrs[i]->BeginRead(__FILE__, __LINE__);
			mBonesAnim[i]->LoadData(bone_attrs[i]);
			bone_attrs[i]->EndRead();
		}
	}
	return TRUE;
}

vBOOL GfxSkeletonAction::LoadXnd(IRenderContext* rc, const char* name, XNDNode* node, bool isLoad)
{
	mSrcNode.StrongRef(node);
	mName = name;
	mFrameCount = 0;
	mDuration = 0;
	auto temp = node->GetAttrib("FPS");
	if (temp)
	{
		temp->BeginRead(__FILE__, __LINE__);
		temp->Read(mFps);
		temp->EndRead();
	}
	temp = node->GetAttrib("Duration");
	if (temp)
	{
		temp->BeginRead(__FILE__, __LINE__);
		temp->Read(mDuration);
		temp->EndRead();
	}

	auto pAttr = node->GetAttrib("Notify");
	if (pAttr)
	{
		pAttr->BeginRead(__FILE__, __LINE__);
		mNotifyTime.Load(*pAttr);
		pAttr->EndRead();
	}
	auto boneNode = node->GetChild("BonesHead");
	if (boneNode)
	{
		pAttr = node->GetAttrib("KeyData");
		if (pAttr)
		{
			pAttr->BeginRead(__FILE__, __LINE__);
			std::string vmasaveName;
			pAttr->ReadText(vmasaveName);
			pAttr->EndRead();

			auto& bone_attrs = boneNode->GetAttribVector();
			for (size_t i = 0; i < bone_attrs.size(); i++)
			{
				auto anim = new GfxBoneAnim();
				bone_attrs[i]->BeginRead(__FILE__, __LINE__);
				anim->LoadHead(bone_attrs[i]);
				bone_attrs[i]->EndRead();
				mBonesAnim.push_back(anim);
				mBonesAnimMap.insert(std::make_pair(anim->mBoneData->NameHash, anim));
			}
		}
		else
		{
			node->TryReleaseHolder();
			return FALSE;
		}
	}
	/*else
	{
		pAttr = node->GetAttrib("KeyData");
		if (pAttr)
		{
			pAttr->BeginRead();

			std::string vmasaveName;
			pAttr->ReadString(vmasaveName);

			UINT uBoneNumber;
			pAttr->Read(&uBoneNumber, sizeof(uBoneNumber));
			ASSERT(uBoneNumber > 0);
			for (UINT i = 0; i < uBoneNumber; i++)
			{
				auto anim = new GfxBoneAnim();
				anim->LoadHead(pAttr);
				anim->LoadData(pAttr);

				mBonesAnim.push_back(anim);
				mBonesAnimMap.insert(std::make_pair(anim->mBoneData->NameHash, anim));
			}

			pAttr->EndRead();
		}
		else
		{
			node->TryReleaseHolder();
			return FALSE;
		}
	}*/

	if (isLoad)
	{
		RestoreResource();
		this->GetResourceState()->SetStreamState(SS_Valid);
	}
	else
	{
		node->TryReleaseHolder();
		this->GetResourceState()->SetStreamState(SS_Invalid);
	}
	return TRUE;
}

vBOOL EngineNS::GfxSkeletonAction::MakeTPoseActionFromSkeleton(GfxSkeleton* skeleton, const char* name, bool isLoad)
{

	auto bones = skeleton->mBoneTable->mBones;
	for (int i = 0; i < bones.size(); ++i)
	{
		auto anim = new GfxBoneAnim();
		anim->mBoneData = bones[i]->mSharedData;
		v3dxMatrix4 parentInvMat = v3dxMatrix4::IDENTITY;;
		auto parentBone = skeleton->mBoneTable->FindBone(bones[i]->mSharedData->ParentHash);
		if (parentBone)
		{
			parentInvMat = parentBone->mSharedData->InvInitMatrix;
		}
		auto mat = bones[i]->mSharedData->InitMatrix;
		mat = mat * parentInvMat;
		v3dxVector3 scale, pos;
		v3dxQuaternion rot;
		mat.Decompose(scale,pos,rot);
		TimeKeys key;
		key.__SetDurationFrame(0, 1);
		key.CreateTimes(1);
		anim->mPosTimeKeys = key;
		anim->mPosFrames.push_back(pos);
		anim->mRotateTimeKeys = key;
		anim->mRotateFrames.push_back(rot);
		anim->mScaleTimeKeys = key;
		anim->mScaleFrames.push_back(scale);
		mBonesAnim.push_back(anim);
		mBonesAnimMap.insert(std::make_pair(anim->mBoneData->NameHash, anim));
	}
	return TRUE;
}

void GfxSkeletonAction::Save2Xnd(XNDNode* node)
{
	auto temp = node->AddAttrib("FPS");
	if (temp)
	{
		temp->BeginWrite();
		temp->Write(mFps);
		temp->EndWrite();
	}
	temp = node->AddAttrib("Duration");
	if (temp)
	{
		temp->BeginWrite();
		temp->Write(mDuration);
		temp->EndWrite();
	}
	auto pAttr = node->AddAttrib("Notify");
	if (pAttr)
	{
		pAttr->BeginWrite();
		mNotifyTime.Save(*pAttr);
		pAttr->EndWrite();
	}
	auto boneNode = node->AddNode("BonesHead", 0, 0);
	auto dataNode = node->AddNode("BonesData", 0, 0);

	pAttr = node->AddAttrib("KeyData");
	if (pAttr)
	{
		pAttr->BeginWrite();
		std::string vmasaveName;
		pAttr->WriteText(vmasaveName.c_str());
		pAttr->EndWrite();

		for (auto i : mBonesAnim)
		{
			auto attrHead = boneNode->AddAttrib("");
			auto attrData = dataNode->AddAttrib("");

			attrHead->BeginWrite();
			attrData->BeginWrite();
			i->Save2Xnd(attrHead, attrData);
			attrHead->EndWrite();
			attrData->EndWrite();
		}
	}
}

void GfxSkeletonAction::FixBoneTree(GfxSkeleton* skeleton)
{
	for (auto i : mBonesAnim)
	{
		auto pBone = skeleton->mBoneTable->FindBone(i->mBoneData->NameHash);
		if (pBone == nullptr)
			continue;

		i->mBoneData->SetParent(pBone->GetBoneDesc()->Parent.c_str());
		i->mBoneData->InitMatrix = pBone->GetBoneDesc()->InitMatrix;
		i->mBoneData->InvInitMatrix = pBone->GetBoneDesc()->InvInitMatrix;
		i->mBoneData->InvPos = pBone->GetBoneDesc()->InvPos;
		i->mBoneData->InvQuat = pBone->GetBoneDesc()->InvQuat;
		i->mBoneData->InvScale = pBone->GetBoneDesc()->InvScale;

	}
}

void GfxSkeletonAction::FixBoneAnimPose(GfxSkeleton* skeleton)
{
	for (auto i : mBonesAnim)
	{
		auto pBone = skeleton->mBoneTable->FindBone(i->mBoneData->NameHash);
		if(!pBone)
			continue;
		auto boneMat = pBone->GetBoneDesc()->InitMatrix;
		//pBone->GetBoneDesc()->InvInitMatrix = v3dxMatrix4::IDENTITY;
		//pBone->GetBoneDesc()->InvPos = v3dxVector3::ZERO;
		//pBone->GetBoneDesc()->InvQuat = v3dxQuaternion::IDENTITY;
		//pBone->GetBoneDesc()->InvScale = v3dxVector3::UNIT_SCALE;
		auto pParentBone = skeleton->mBoneTable->FindBone(i->mBoneData->ParentHash);
		if (pParentBone)
		{
			boneMat = boneMat * pParentBone->GetBoneDesc()->InvInitMatrix;
		}

		//v3dxVector3 pos;
		//boneMat.ExtractionTrans(pos);
		//v3dxQuaternion rot;
		//boneMat.ExtractionRotation(rot);

		GfxBoneAnim* anim = i;
		for (int j = 0; j < anim->mPosFrames.size(); ++j)
		{
			v3dxMatrix4 animMat;
			animMat.makeTrans(anim->mPosFrames[j], v3dxVector3::UNIT_SCALE, anim->mRotateFrames[j]);
			auto finalMat = animMat * boneMat;

			v3dxVector3 pos;
			finalMat.ExtractionTrans(pos);
			v3dxQuaternion rot;
			finalMat.ExtractionRotation(rot);
			anim->mPosFrames[j] = pos;
			//anim->mRotateFrames[j] = rot;

			//v3dxQuaternion animRot = anim->mRotateFrames[j];
			//animRot.inverse();

			//auto animPos = animRot * anim->mPosFrames[j];

			//anim->mPosFrames[j] = pos + animRot *anim->mPosFrames[j];
			////anim->mRotateFrames[j] = rot * anim->mRotateFrames[j];
			//anim->mRotateFrames[j] = anim->mRotateFrames[j] * rot;
		}
	}
}
void GfxSkeletonAction::GetAnimaPose(vTimeTick time, GfxAnimationPose* outPose, vBOOL withMotionData)
{
	ASSERT(outPose);
	for (auto i : outPose->mBones)
	{
		auto boneAnim = mBonesAnimMap[i->mSharedData->NameHash];
		if (boneAnim == nullptr)
		{
			auto parentBone = outPose->FindBone(i->mSharedData->ParentHash);
			auto mat = i->mSharedData->InitMatrix;
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

			i->Transform.Position = pos;
			i->Transform.Rotation = rot;
			i->Transform.Scale = scale;
		}
		else
		{
			boneAnim->GetTransform(time, i->Transform);
			if (withMotionData)
				boneAnim->GetMotionData(time, i->MotionData);
			if (!boneAnim->IsHavePosFrames())
			{
				auto parentBone = outPose->FindBone(i->mSharedData->ParentHash);
				auto mat = i->mSharedData->InitMatrix;
				if (parentBone != NULL)
				{
					v3dxMatrix4 parentInvMat = v3dxMatrix4::IDENTITY;;
					parentInvMat = parentBone->mSharedData->InvInitMatrix;
					mat = mat * parentInvMat;
				}
				v3dxVector3 pos, scale;
				v3dxQuaternion rot;
				mat.ExtractionTrans(pos);
				i->Transform.Position = pos;
			 }
		}
	}
	auto root = outPose->GetRoot();
	if (root != nullptr)
	{
		root->AbsTransform = root->Transform;
		root->LinkBone(outPose);
	}
	else
	{
		ASSERT(FALSE);
	}
	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->GrantChildren)
		{
			auto grantBone = outPose->GetBone(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
		}
	}
}
void GfxSkeletonAction::GetAnimaPoseWithoutMotionDatas(vTimeTick time, GfxAnimationPose* outPose)
{
	ASSERT(outPose);
	for (auto i : outPose->mBones)
	{
		auto boneAnim = mBonesAnimMap[i->mSharedData->NameHash];
		if (boneAnim == nullptr)
		{
			auto parentBone = outPose->FindBone(i->mSharedData->ParentHash);
			auto mat = i->mSharedData->InitMatrix;
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

			i->Transform.Position = pos;
			i->Transform.Rotation = rot;
			i->Transform.Scale = scale;
		}
		else
		{
			if (!boneAnim->IsHavePosFrames())
			{
				auto parentBone = outPose->FindBone(i->mSharedData->ParentHash);
				auto mat = i->mSharedData->InitMatrix;
				if (parentBone != NULL)
				{
					v3dxMatrix4 parentInvMat = v3dxMatrix4::IDENTITY;;
					parentInvMat = parentBone->mSharedData->InvInitMatrix;
					mat = mat * parentInvMat;
				}
				v3dxVector3 pos, scale;
				v3dxQuaternion rot;
				mat.ExtractionTrans(pos);
				i->Transform.Position = pos;
			}
			else
			{
				boneAnim->GetTransform(time, i->Transform);
				//boneAnim->GetMotionData(time, i->MotionData);
			}
		}
	}
	auto root = outPose->GetRoot();
	if (root != nullptr)
	{
		root->AbsTransform = root->Transform;
		root->LinkBone(outPose);
	}
	else
	{
		ASSERT(FALSE);
	}
	for (auto bone : outPose->mBones)
	{
		for (auto j : bone->GrantChildren)
		{
			auto grantBone = outPose->GetBone(j);
			ASSERT(grantBone != nullptr);
			if (grantBone == nullptr)
				continue;
			//grantBone->AbsTransform.Rotation.slerp(bone->AbsTransform.Rotation, grantBone->mSharedData->GrantWeight);
			grantBone->Transform.Rotation.slerp(bone->Transform.Rotation, grantBone->mSharedData->GrantWeight);
		}
	}
}

void EngineNS::GfxSkeletonAction::GetMotionDatas(vTimeTick time, UINT boneHashName,GfxMotionState* motionData)
{
	auto boneAnim = mBonesAnimMap[boneHashName];
	if (boneAnim != nullptr && boneAnim->IsHavePosFrames())
	{
		boneAnim->GetMotionData(time, *motionData);
	}
}

void EngineNS::GfxSkeletonAction::CalculateFrameCountAndDuration()
{
	UINT duration = 0;
	UINT frameCount = 0;
	for (int i = 0;i<mBonesAnim.size();++i)
	{
		auto& anim = mBonesAnim[i];
		if (duration < anim->mPosTimeKeys.GetDuration())
		{
			duration = anim->mPosTimeKeys.GetDuration();
		}
		if (frameCount < anim->mPosTimeKeys.GetFrameCount())
		{
			frameCount = anim->mPosTimeKeys.GetFrameCount();
		}
		if (duration < anim->mRotateTimeKeys.GetDuration())
		{
			duration = anim->mRotateTimeKeys.GetDuration();
		}
		if (frameCount < anim->mRotateTimeKeys.GetFrameCount())
		{
			frameCount = anim->mRotateTimeKeys.GetFrameCount();
		}
		if (duration < anim->mScaleTimeKeys.GetDuration())
		{
			duration = anim->mScaleTimeKeys.GetDuration();
		}
		if (frameCount < anim->mScaleTimeKeys.GetFrameCount())
		{
			frameCount = anim->mScaleTimeKeys.GetFrameCount();
		}
	}

	mDuration = duration;
	mFrameCount = frameCount;
}


NS_END

using namespace EngineNS;



extern "C"
{
	CSharpReturnAPI2(vBOOL, EngineNS, GfxSkeletonAction, Init, IRenderContext*, const char*);
	CSharpReturnAPI4(vBOOL, EngineNS, GfxSkeletonAction, LoadXnd, IRenderContext*, const char*, XNDNode*, bool);
	CSharpReturnAPI3(vBOOL, EngineNS, GfxSkeletonAction, MakeTPoseActionFromSkeleton, GfxSkeleton*, const char*, bool);
	CSharpReturnAPI0(const char*, EngineNS, GfxSkeletonAction, GetName);
	CSharpReturnAPI0(UINT, EngineNS, GfxSkeletonAction, GetBoneNumber);
	CSharpReturnAPI1(GfxBoneAnim*, EngineNS, GfxSkeletonAction, GetBoneAnum, UINT);
	CSharpReturnAPI0(UINT, EngineNS, GfxSkeletonAction, GetDuration);
	CSharpReturnAPI0(UINT, EngineNS, GfxSkeletonAction, GetFrameCount);
	CSharpReturnAPI0(float, EngineNS, GfxSkeletonAction, GetFps);
	CSharpReturnAPI1(GfxBoneAnim*, EngineNS, GfxSkeletonAction, FindBoneAnimByHashId, int);
	CSharpAPI1(EngineNS, GfxSkeletonAction, FixBoneTree, GfxSkeleton*);
	CSharpAPI1(EngineNS, GfxSkeletonAction, FixBoneAnimPose, GfxSkeleton*);
	CSharpAPI3(EngineNS, GfxSkeletonAction, GetAnimaPose, vTimeTick, GfxAnimationPose*,vBOOL);
	CSharpAPI3(EngineNS, GfxSkeletonAction, GetMotionDatas, vTimeTick, UINT, GfxMotionState*);
	CSharpAPI1(EngineNS, GfxSkeletonAction, Save2Xnd, XNDNode*);
}