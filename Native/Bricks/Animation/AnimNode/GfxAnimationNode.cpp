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
	Cpp2CS4(EngineNS, GfxAnimationNode, LoadXnd);
	Cpp2CS1(EngineNS, GfxAnimationNode, Save2Xnd);
	Cpp2CS0(EngineNS, GfxAnimationNode, GetAnimationPose);
	Cpp2CS1(EngineNS, GfxAnimationNode, SetAnimationPose);
	Cpp2CS1(EngineNS, GfxAnimationNode, SetPlayRate);
	Cpp2CS1(EngineNS, GfxAnimationNode, SetCurrentTime);
	Cpp2CS0(EngineNS, GfxAnimationNode, GetDuration);
	Cpp2CS0(EngineNS, GfxAnimationNode, GetFrameCount);
	Cpp2CS0(EngineNS, GfxAnimationNode, GetFps);
	Cpp2CS1(EngineNS, GfxAnimationNode, Update);
}