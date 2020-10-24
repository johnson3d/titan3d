#include "GfxSkeletonPose.h"
#include "GfxBonePose.h"
#define new VNEW

NS_BEGIN
RTTI_IMPL(EngineNS::GfxSkeletonPose, EngineNS::VIUnknown);
GfxSkeletonPose::GfxSkeletonPose()
{
}


GfxSkeletonPose::~GfxSkeletonPose()
{

}

void GfxSkeletonPose::AddBonePose(GfxBonePose* bonePose)
{
	mBones.push_back(bonePose);
	mBoneMap.insert(std::make_pair(bonePose->ReferenceBone->mSharedData->NameHash, bonePose));
}
GfxSkeletonPose* GfxSkeletonPose::Clone()
{
	GfxSkeletonPose* pose = new GfxSkeletonPose();
	pose->ReferenceSkeleton = ReferenceSkeleton;
	for (int i = 0; i < mBones.size(); ++i)
	{
		GfxBonePose* bonePose = new GfxBonePose();
		bonePose->Transform = mBones[i]->Transform;
		bonePose->MotionData = mBones[i]->MotionData;
		bonePose->ReferenceBone = mBones[i]->ReferenceBone;
		pose->AddBonePose(bonePose);
	}
	return pose;
}
NS_END

using namespace EngineNS;

extern "C"
{
	Cpp2CS1(EngineNS, GfxSkeletonPose, AddBonePose);
	Cpp2CS1(EngineNS, GfxSkeletonPose, SetReferenceSkeleton);

}

