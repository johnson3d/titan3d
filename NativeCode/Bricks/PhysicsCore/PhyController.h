#pragma once
#include "PhyEntity.h"


NS_BEGIN

class PhyScene;
class PhyActor;
class PhyMaterial;

enum TR_ENUM()
EPhyControllerCollisionFlag
{
	eCOLLISION_SIDES = (1 << 0),	//!< Character is colliding to the sides.
	eCOLLISION_UP = (1 << 1),	//!< Character has collision above.
	eCOLLISION_DOWN = (1 << 2)	//!< Character has collision below.
};


class TR_CLASS()
	PhyControllerDesc : public IWeakReference
{
public:
	PhyControllerDesc()
	{

	}
	physx::PxControllerDesc* mDesc;
	PhyFilterData mFilterData;
	void SetMaterial(PhyMaterial * mtl);
	void SetQueryFilterData(physx::PxFilterData * data);
	void SetHitReportCallback()
	{

	}
	void SetBehaviorCallback()
	{

	}
};

class TR_CLASS()
	PhyBoxControllerDesc : public PhyControllerDesc
{
public:
	ENGINE_RTTI(PhyBoxControllerDesc);
	PhyBoxControllerDesc()
	{
		mDesc = &mBoxDesc;
	}
	physx::PxBoxControllerDesc		mBoxDesc;
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

class TR_CLASS()
	PhyCapsuleControllerDesc : public PhyControllerDesc
{
public:
	ENGINE_RTTI(PhyCapsuleControllerDesc);
	PhyCapsuleControllerDesc();
	physx::PxCapsuleControllerDesc	mCapsuleDesc;

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

class TR_CLASS() 
	PhyController : public PhyEntity
{
	PhyActor*					mActor;
	physx::PxController*		mController;
	TWeakRefHandle<PhyScene>		mScene;
public:
	ENGINE_RTTI(PhyController);

	PhyController(PhyScene* scene, physx::PxController* ctr);
	~PhyController();
	virtual void Cleanup() override;
	void BindPhysX();
	
	PhyActor* GetReadOnlyActor();
	EPhyControllerCollisionFlag Move(const v3dxVector3* disp, float minDist, float elapsedTime, const PhyFilterData* filterData, PhyQueryFlag filterFlags);
	void SetPosition(const v3dxVector3* position);
	v3dxVector3 GetPosition();
	void SetFootPosition(const v3dxVector3* position);
	v3dxVector3 GetFootPosition();
	float GetContactOffset();
	void SetContactOffset(float offset);
	float GetSlopeLimit();
	void SetSlopeLimit(float slopeLimit);
	void SetQueryFilterData(const PhyFilterData * filterData);
	void SetSimulationFilterData(const PhyFilterData * filterData);
};

NS_END