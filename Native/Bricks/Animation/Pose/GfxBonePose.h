#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Skeleton/GfxBone.h"
#include "../AnimationDataTypes.h"
NS_BEGIN
class GfxBonePose : public VIUnknown
{
public:
	RTTI_DEF(GfxBonePose, 0x0809f3515d5a385f, true);
	GfxBonePose();
	~GfxBonePose();
public:
	void GetTransform(GfxBoneTransform* transform) const {
		*transform = Transform;
	}
	void SetTransform(GfxBoneTransform* transform) {
		Transform = *transform;
	}
	void GetMotionData(GfxMotionState* motionData) const {
		*motionData = MotionData;
	}
	void SetMotionData(GfxMotionState* motionData) {
		MotionData = *motionData;
	}
	void SetReferenceBone(GfxBone* bone) {
		ReferenceBone.StrongRef(bone);
	}
public:
	AutoRef<GfxBone> ReferenceBone;
	GfxBoneTransform Transform;
	GfxMotionState	MotionData;
};

NS_END