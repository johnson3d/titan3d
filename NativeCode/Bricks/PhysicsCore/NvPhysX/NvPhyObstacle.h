#pragma once
#include "NvCommon.h"
#include "../PhyObstacle.h"

NS_BEGIN

class NvPhyBoxObstacle : public PhyBoxObstacle
{
protected:
	physx::PxBoxObstacle		PxBox;
	virtual void* GetInnerObstacle() {
		return &PxBox;
	}
public:
	NvPhyBoxObstacle() {
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

class NvPhyCapsuleObstacle : public PhyCapsuleObstacle
{
protected:
	physx::PxCapsuleObstacle		PxCapsule;
	virtual void* GetInnerObstacle() {
		return &PxCapsule;
	}
public:
	NvPhyCapsuleObstacle() {
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

class NvPhyObstacleContext : public PhyObstacleContext
{
public:
	physx::PxObstacleContext*		mContext;
public:
	NvPhyObstacleContext();
	~NvPhyObstacleContext();

	void AddObstacle(PhyObstacle* obstacle);
	void RemoveObstacle(PhyObstacle* obstacle);

	UINT GetNbObstacles();
	PhyObstacle* GetObstacle(UINT i);
};

NS_END