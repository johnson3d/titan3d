#pragma once
#include "NvCommon.h"
#include "../PhyController.h"


NS_BEGIN

class NvPhyScene;
class NvPhyActor;
class NvPhyMaterial;

class NvPhyBoxControllerDesc : public PhyBoxControllerDesc
{
public:
	ENGINE_RTTI(NvPhyBoxControllerDesc);
	physx::PxBoxControllerDesc		mBoxDesc;
	virtual void SetMaterial(PhyMaterial* mtl) override;
	v3dxVector3 GetExtent() {
		v3dxVector3 v;
		v.X = mBoxDesc.halfSideExtent;
		v.Y = mBoxDesc.halfHeight;
		v.Z = mBoxDesc.halfForwardExtent;
		return v;
	}
	void SetExtent(const v3dxVector3 * v) {
		mBoxDesc.halfSideExtent = v->X;
		mBoxDesc.halfHeight = v->Y;
		mBoxDesc.halfForwardExtent = v->Z;
	}
};

class NvPhyCapsuleControllerDesc : public PhyCapsuleControllerDesc
{
public:
	ENGINE_RTTI(NvPhyCapsuleControllerDesc);
	NvPhyCapsuleControllerDesc();
	physx::PxCapsuleControllerDesc	mCapsuleDesc;

	virtual void SetMaterial(PhyMaterial* mtl) override;
	float GetCapsuleRadius() {
		return mCapsuleDesc.radius;
	}
	void SetCapsuleRadius(float v) {
		mCapsuleDesc.radius = v;
	}
	float GetCapsuleHeight() {
		return mCapsuleDesc.height;
	}
	void SetCapsuleHeight(float v) {
		mCapsuleDesc.height = v;
	}
	physx::PxCapsuleClimbingMode::Enum GetCapsuleClimbingMode() {
		return mCapsuleDesc.climbingMode;
	}
	void SetCapsuleClimbingMode(physx::PxCapsuleClimbingMode::Enum v) {
		mCapsuleDesc.climbingMode = v;
	}
};

class NvPhyController : public PhyController
{
	NvPhyActor*					mActor;
	physx::PxController*		mController;
	TWeakRefHandle<PhyScene>		mScene;
public:
	ENGINE_RTTI(NvPhyController);

	NvPhyController(PhyScene* scene, physx::PxController* ctr);
	~NvPhyController();
	virtual void Cleanup() override;
	void BindPhysX();
	
	PhyActor* GetReadOnlyActor();
	EPhyControllerCollisionFlag Move(const v3dxVector3* disp, float minDist, float elapsedTime, const FPhyFilterData* filterData, EPhyQueryFlag filterFlags);
	void SetPosition(const v3dxVector3* position);
	v3dxVector3 GetPosition();
	void SetFootPosition(const v3dxVector3* position);
	v3dxVector3 GetFootPosition();
	float GetContactOffset();
	void SetContactOffset(float offset);
	float GetSlopeLimit();
	void SetSlopeLimit(float slopeLimit);
	void SetQueryFilterData(const FPhyFilterData * filterData);
	void SetSimulationFilterData(const FPhyFilterData * filterData);
};

NS_END