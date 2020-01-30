#include "GfxLookAt.h"
#include "../Pose/GfxAnimationPose.h"
#include "../../../Math/v3dxMath.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::GfxLookAt, EngineNS::VIUnknown);

GfxLookAt::GfxLookAt()
	:mTargetBone(NULL)
	,mModifyBone(NULL)
	,mLookAtAxis(0,0,1)
{

}

void EngineNS::GfxLookAt::SetTargetBoneName(const char* name)
{
	mTargetBoneName = name;
	auto bone = GetBone(mTargetBoneName.c_str());
	if (bone)
		mTargetBone.StrongRef(bone);
}

void EngineNS::GfxLookAt::SetModifyBoneName(const char* name)
{
	mModifyBoneName = name;
	auto bone = GetBone(mModifyBoneName.c_str());
	if (bone)
		mModifyBone.StrongRef(bone);
}

EngineNS::GfxLookAt::~GfxLookAt()
{

}

void EngineNS::GfxLookAt::Update(vTimeTick time)
{
	if (!mEnable)
		return;
	if(!mModifyBone)
		mModifyBone.StrongRef(GetBone(mModifyBoneName.c_str()));
	if(!mTargetBone)
		mTargetBone.StrongRef(GetBone(mTargetBoneName.c_str()));
	v3dxVector3 targetPosition;
	if (mTargetBone)
		targetPosition = mTargetBone->AbsTransform.Position;
	else
		targetPosition = mTargetPosition;
	auto dir = targetPosition - mModifyBone->AbsTransform.Position;
	auto oriDir = mModifyBone->AbsTransform.Rotation * mLookAtAxis;
	dir.normalize();
	oriDir.normalize();
	auto axis = oriDir.crossProduct(dir);
	auto cosAng = oriDir.dotProduct(dir);
	auto angle = Math::ACos(cosAng);
	v3dxQuaternion quat;
	quat.FromAngleAxis(angle, axis);
	mModifyBone->AbsTransform.Rotation.slerp(quat * mModifyBone->AbsTransform.Rotation, mAlpha);
}




NS_END

using namespace EngineNS;
extern "C"
{
	CSharpAPI1(EngineNS, GfxLookAt, SetModifyBoneName, const char*);
	CSharpAPI1(EngineNS, GfxLookAt, SetTargetBoneName, const char*);
	CSharpAPI1(EngineNS, GfxLookAt, SetTargetPosition, v3dxVector3);
	CSharpAPI1(EngineNS, GfxLookAt, SetLookAtAxis, v3dxVector3);
}