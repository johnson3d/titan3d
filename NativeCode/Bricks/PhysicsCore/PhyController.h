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
	PhyControllerDesc : public IWeakRefObject
{
public:
	PhyControllerDesc()
	{

	}
	PhyFilterData mFilterData;
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
	
	virtual void SetMaterial(PhyMaterial* mtl) = 0;
	virtual v3dxVector3 GetExtent() = 0;
	virtual void SetExtent(const v3dxVector3* v) = 0;
};

class TR_CLASS()
	PhyCapsuleControllerDesc : public PhyControllerDesc
{
public:
	ENGINE_RTTI(PhyCapsuleControllerDesc);
	
	virtual void SetMaterial(PhyMaterial* mtl) = 0;
	virtual float GetCapsuleRadius() = 0;
	virtual void SetCapsuleRadius(float v) = 0;
	virtual float GetCapsuleHeight() = 0;
	virtual void SetCapsuleHeight(float v) = 0;
};

class TR_CLASS() 
	PhyController : public PhyEntity
{
public:
	ENGINE_RTTI(PhyController);

	virtual void BindPhysX() = 0;
	
	virtual PhyActor* GetReadOnlyActor() = 0;
	virtual EPhyControllerCollisionFlag Move(const v3dxVector3* disp, float minDist, float elapsedTime, const PhyFilterData* filterData, PhyQueryFlag filterFlags) = 0;
	virtual void SetPosition(const v3dxVector3* position) = 0;
	virtual v3dxVector3 GetPosition() = 0;
	virtual void SetFootPosition(const v3dxVector3* position) = 0;
	virtual v3dxVector3 GetFootPosition() = 0;
	virtual float GetContactOffset() = 0;
	virtual void SetContactOffset(float offset) = 0;
	virtual float GetSlopeLimit() = 0;
	virtual void SetSlopeLimit(float slopeLimit) = 0;
	virtual void SetQueryFilterData(const PhyFilterData * filterData) = 0;
	virtual void SetSimulationFilterData(const PhyFilterData * filterData) = 0;
};

NS_END