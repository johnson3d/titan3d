#include "GfxSkeletonControl.h"
#include "../Pose/GfxAnimationPose.h"
#include "../../../Math/v3dxMath.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxSkeletonControl, EngineNS::VIUnknown);

GfxSkeletonControl::GfxSkeletonControl()
	:mAlpha(1.0f)
	,mEnable(TRUE)
{

}

GfxSkeletonControl::~GfxSkeletonControl()
{

}

void GfxSkeletonControl::SetAnimationPose(GfxAnimationPose* pose)
{
	mAnimationPose.StrongRef(pose);
}

GfxBone* GfxSkeletonControl::GetBone(const char* name)
{
	if (!mAnimationPose)
		return nullptr;
	return mAnimationPose->FindBone(name); 
}

EngineNS::GfxBone* EngineNS::GfxSkeletonControl::GetBone(UINT nameHash)
{
	if (!mAnimationPose)
		return nullptr;
	return mAnimationPose->FindBone(nameHash);
}

void GfxSkeletonControl::Update(vTimeTick time)
{

}




NS_END

using namespace EngineNS;
extern "C"
{
	CSharpReturnAPI0(float, EngineNS, GfxSkeletonControl, GetAlpha);
	CSharpAPI1(EngineNS, GfxSkeletonControl, SetAlpha, float);
	CSharpReturnAPI0(vBOOL, EngineNS, GfxSkeletonControl, GetEnable);
	CSharpAPI1(EngineNS, GfxSkeletonControl, SetEnable, vBOOL);
	CSharpReturnAPI0(GfxAnimationPose*, EngineNS, GfxSkeletonControl, GetAnimationPose);
	CSharpAPI1(EngineNS, GfxSkeletonControl, SetAnimationPose, GfxAnimationPose*);
	CSharpAPI1(EngineNS, GfxSkeletonControl, Update, vTimeTick);
}