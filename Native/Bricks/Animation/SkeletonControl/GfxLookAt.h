#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../Pose/GfxAnimationPose.h"
#include "GfxSkeletonControl.h"

NS_BEGIN
class GfxLookAt : public GfxSkeletonControl
{
public:
	RTTI_DEF(GfxLookAt, 0x7eb58a075bd15ad6, true);
	GfxLookAt();
	~GfxLookAt();

	virtual void Update(vTimeTick time) override;
	void SetModifyBoneName(const char* name);
	void SetTargetBoneName(const char* name);
	void SetTargetPosition(v3dxVector3 position) {
		mTargetPosition = position;
	}
	void SetLookAtAxis(v3dxVector3 val) { mLookAtAxis = val; }
public:
	std::string						mModifyBoneName;
	std::string						mTargetBoneName;
	v3dxVector3						mTargetPosition;
	v3dxVector3						mLookAtAxis;

private:
	AutoRef<GfxBone> mModifyBone;
	AutoRef<GfxBone> mTargetBone;
};
NS_END