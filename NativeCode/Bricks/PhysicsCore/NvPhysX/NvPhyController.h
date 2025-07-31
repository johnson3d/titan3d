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
	virtual v3dxVector3 GetExtent()  override {
		v3dxVector3 v;
		v.X = mBoxDesc.halfSideExtent;
		v.Y = mBoxDesc.halfHeight;
		v.Z = mBoxDesc.halfForwardExtent;
		return v;
	}
	virtual void SetExtent(const v3dxVector3 * v) override {
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
	virtual float GetCapsuleRadius() override {
		return mCapsuleDesc.radius;
	}
	virtual void SetCapsuleRadius(float v) override {
		mCapsuleDesc.radius = v;
	}
	virtual float GetCapsuleHeight() override {
		return mCapsuleDesc.height;
	}
	virtual void SetCapsuleHeight(float v) override {
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
	NvPhyActor*					mActor = nullptr;
	physx::PxController*		mController = nullptr;
	TWeakRefHandle<PhyScene>		mScene;
public:
	ENGINE_RTTI(NvPhyController);

	NvPhyController(PhyScene* scene, physx::PxController* ctr);
	~NvPhyController();
	virtual void Cleanup() override;
	virtual void BindPhysX() override;
	
	virtual PhyActor* GetReadOnlyActor() override;
	virtual EPhyControllerCollisionFlag Move(const v3dxVector3* disp, float minDist, float elapsedTime, const FPhyFilterData* filterData, EPhyQueryFlag filterFlags) override;
	virtual void SetPosition(const v3dxVector3* position) override;
	virtual v3dxVector3 GetPosition() override;
	virtual void SetFootPosition(const v3dxVector3* position) override;
	virtual v3dxVector3 GetFootPosition() override;
	virtual float GetContactOffset() override;
	virtual void SetContactOffset(float offset) override;
	virtual float GetSlopeLimit() override;
	virtual void SetSlopeLimit(float slopeLimit) override;
	virtual void SetQueryFilterData(const FPhyFilterData * filterData) override;
	virtual void SetSimulationFilterData(const FPhyFilterData * filterData) override;
};

NS_END