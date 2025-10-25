#pragma once
#include "Nv5Common.h"
#include "../PhyObstacle.h"

NS_BEGIN

class Nv5PhyBoxObstacle : public PhyBoxObstacle
{
protected:
	physx::PxBoxObstacle		PxBox;
	virtual void* GetInnerObstacle() {
		return &PxBox;
	}
public:
	Nv5PhyBoxObstacle() {
		PxBox.mUserData = this;
	}
	void SetHalfExtent(const v3dxVector3* halfExtent)
	{
		PxBox.mHalfExtents = *(physx::PxVec3*)halfExtent;
	}
	const v3dxVector3* GetHalfExtent() {
		return (const v3dxVector3*)(&PxBox.mHalfExtents);
	}
};

class Nv5PhyCapsuleObstacle : public PhyCapsuleObstacle
{
protected:
	physx::PxCapsuleObstacle		PxCapsule;
	virtual void* GetInnerObstacle() {
		return &PxCapsule;
	}
public:
	Nv5PhyCapsuleObstacle() {
		PxCapsule.mUserData = this;
	}
	void SetRadius(float v) {
		PxCapsule.mRadius = v;
	}
	float GetRadius() {
		return PxCapsule.mRadius;
	}
	void SetHalfHeight(float v) {
		PxCapsule.mHalfHeight = v;
	}
	float GetHalfHeight() {
		return PxCapsule.mHalfHeight;
	}
};

class Nv5PhyObstacleContext : public PhyObstacleContext
{
public:
	physx::PxObstacleContext*		mContext;
public:
	Nv5PhyObstacleContext();
	~Nv5PhyObstacleContext();

	void AddObstacle(PhyObstacle* obstacle);
	void RemoveObstacle(PhyObstacle* obstacle);

	UINT GetNbObstacles();
	PhyObstacle* GetObstacle(UINT i);
};

NS_END