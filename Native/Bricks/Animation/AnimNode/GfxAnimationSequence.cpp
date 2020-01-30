#include "GfxAnimationSequence.h"
#include "../Skeleton/GfxBone.h"
#include "../GfxBoneAnim.h"
#include "../GfxSkeletonAction.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxAnimationSequence, EngineNS::VIUnknown);

GfxAnimationSequence::GfxAnimationSequence():mSkeletonAction(NULL)
{
}

GfxAnimationSequence::~GfxAnimationSequence()
{
}

vBOOL GfxAnimationSequence::LoadXnd(IRenderContext* rc, const char* name, XNDNode* node, bool isLoad)
{
	GfxAnimationNode::LoadXnd(rc, name, node, isLoad);
	auto attr = node->GetAttrib("AnimSeg");
	if (attr != nullptr)
	{
		attr->BeginRead(__FILE__, __LINE__);
		//attr->ReadStringAsRName(mSkeletonActionName);
		attr->EndRead();
	}
	return TRUE;
}

void GfxAnimationSequence::Save2Xnd(XNDNode* node)
{
	GfxAnimationNode::Save2Xnd(node);
	auto attr = node->AddAttrib("AnimSeg");
	if (attr != nullptr)
	{
		attr->BeginWrite();
		//attr->WriteStringAsRName(mSkeletonActionName);
		attr->EndWrite();
	}
}

void GfxAnimationSequence::SetSkeletonAction(GfxSkeletonAction* action)
{
	if (action)
	{
		mSkeletonAction.StrongRef(action);
		if (action->mFrameCount == 0 || action->mDuration == 0)
		{
			action->CalculateFrameCountAndDuration();
		}
		SetDuration(action->mDuration);
		SetFrameCount(action->mFrameCount);
	}
}

void GfxAnimationSequence::Update(vTimeTick time)
{
	if (!mSkeletonAction)
		return;
	//mSkeletonAction->GetAnimaPose(mCurrentTime, mAnimationPose);
	//mCurrentTime += (vTimeTick)((float)time * mPlayRate);
	//mCurrentTime = mCurrentTime % (mDuration + 1);
}

NS_END
using namespace EngineNS;

extern "C"
{
	CSharpAPI1(EngineNS, GfxAnimationSequence, SetSkeletonAction, GfxSkeletonAction*);
	CSharpReturnAPI0(GfxSkeletonAction*, EngineNS, GfxAnimationSequence, GetSkeletonAction);
}