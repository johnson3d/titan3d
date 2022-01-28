#pragma once
#include "PhyEntity.h"

NS_BEGIN

enum TR_ENUM()
EPhyControllerCollisionFlag
{
	eCOLLISION_SIDES = (1 << 0),	//!< Character is colliding to the sides.
	eCOLLISION_UP = (1 << 1),	//!< Character has collision above.
	eCOLLISION_DOWN = (1 << 2)	//!< Character has collision below.
};

class PhyScene;
class PhyActor;
class TR_CLASS() 
	PhyController : public PhyEntity
{
	PhyActor*					mActor;
	physx::PxController*		mController;
	TObjectHandle<PhyScene>		mScene;
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
	void SetQueryFilterData(physx::PxFilterData* filterData);
};

NS_END