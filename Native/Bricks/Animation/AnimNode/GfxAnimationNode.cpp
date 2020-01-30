#include "GfxAnimationNode.h"
#include "../Skeleton/GfxBone.h"
#include "../GfxBoneAnim.h"
#include "../GfxSkeletonAction.h"


#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxAnimationNode, EngineNS::VIUnknown);

GfxAnimationNode::GfxAnimationNode()
	: mFrameCount(0)
	, mDuration(0)
	, mFps(30)
	, mCurrentTime(0)
	, mPlayRate(1.0f)
{
}

GfxAnimationNode::~GfxAnimationNode()
{
}

vBOOL GfxAnimationNode::LoadXnd(IRenderContext* rc, const char* name, XNDNode* node, bool isLoad)
{
	return TRUE;
}

void EngineNS::GfxAnimationNode::Save2Xnd(XNDNode* node)
{

}

void GfxAnimationNode::SetAnimationPose(GfxAnimationPose* pose)
{
	mAnimationPose.StrongRef(pose);
}
NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI4(vBOOL, EngineNS, GfxAnimationNode, LoadXnd, IRenderContext*, const char*, XNDNode*, bool);
	CSharpAPI1(EngineNS, GfxAnimationNode, Save2Xnd, XNDNode*);
	CSharpReturnAPI0(GfxAnimationPose*, EngineNS, GfxAnimationNode, GetAnimationPose);
	CSharpAPI1(EngineNS, GfxAnimationNode, SetAnimationPose, GfxAnimationPose*);
	CSharpAPI1(EngineNS, GfxAnimationNode, SetPlayRate, float);
	CSharpAPI1(EngineNS, GfxAnimationNode, SetCurrentTime, UINT);
	CSharpReturnAPI0(UINT, EngineNS, GfxAnimationNode, GetDuration);
	CSharpReturnAPI0(UINT, EngineNS, GfxAnimationNode, GetFrameCount);
	CSharpReturnAPI0(float, EngineNS, GfxAnimationNode, GetFps);
	CSharpAPI1(EngineNS, GfxAnimationNode, Update, vTimeTick);
}