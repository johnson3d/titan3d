#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Skeleton/GfxBone.h"
#include "../Pose/GfxSkeletonPose.h"

NS_BEGIN
class GfxAnimationRuntime : public VIUnknown
{
public:
	RTTI_DEF(GfxAnimationRuntime, 0xf84a635d5d4a5972, true);
	GfxAnimationRuntime();
	~GfxAnimationRuntime();
	static void BlendPose(GfxSkeletonPose* outPose, GfxSkeletonPose* aPose, GfxSkeletonPose* bPose, float weight);
	static void AddPose(GfxSkeletonPose* outPose,GfxSkeletonPose* basePose, GfxSkeletonPose* additivePose, float alpha);
	static void MinusPose(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose);
	static void MinusPoseMeshSpace(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose);
	static void CopyPose(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose);

	static void FastBlendPose(GfxSkeletonPose* outPose, GfxSkeletonPose* aPose, GfxSkeletonPose* bPose, float weight);
	static void FastAddPose(GfxSkeletonPose* outPose, GfxSkeletonPose* basePose, GfxSkeletonPose* additivePose, float alpha);
	static void FastMinusPose(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose);
	static void FastMinusPoseMeshSpace(GfxSkeletonPose* outPose, GfxSkeletonPose* minusPose, GfxSkeletonPose* minuendPose);
	static void FastCopyPose(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose);

	static void ZeroPose(GfxSkeletonPose* pose);
	static void ZeroTransition(GfxSkeletonPose* pose);
	static vBOOL IsZeroPose(GfxSkeletonPose* pose);
	static void ConvertToMeshSpace(GfxSkeletonPose* pose);
	static void ConvertToLocalSpace(GfxSkeletonPose* pose);
	//static void ConvertToLocalSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose);
	static void ConvertRotationToMeshSpace(GfxSkeletonPose* pose);
	static void ConvertRotationToLocalSpace(GfxSkeletonPose* pose);

    /////////////////////
	static void CopyPoseAndConvertMeshSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose);
	static void CopyPoseAndConvertRotationToMeshSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose);
	static void CopyPoseAndConvertRotationToLocalSpace(GfxSkeletonPose* desPose, GfxSkeletonPose* srcPose);
public:

};

NS_END