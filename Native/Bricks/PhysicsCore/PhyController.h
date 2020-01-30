#pragma once
#include "PhyEntity.h"

NS_BEGIN

class PhyScene;
class PhyController : public PhyEntity
{
	physx::PxController*		mController;
	TObjectHandle<PhyScene>		mScene;
public:
	RTTI_DEF(PhyController, 0xf34234975c341640, false);

	PhyController(PhyScene* scene, physx::PxController* ctr);
	~PhyController();
	virtual void Cleanup() override;
	void BindPhysX();
	physx::PxControllerCollisionFlags Move(const v3dxVector3* disp, float minDist, float elapsedTime, const physx::PxFilterData* filterData, physx::PxU16 filterFlags);
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