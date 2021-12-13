#pragma once
#include "PhyEntity.h"

NS_BEGIN

class TR_CLASS()
	PhyObstacle : public PhyEntity
{
public:
	UINT			Handle;
	PhyObstacle()
	{
		Handle = 0;
	}
	virtual physx::PxObstacle* GetPxObstacle() {
		return nullptr;
	}
public:
	v3dVector3_t GetPos() {
		v3dVector3_t result;
		auto px = GetPxObstacle();
		result.x = (float)px->mPos.x;
		result.y = (float)px->mPos.y;
		result.z = (float)px->mPos.z;
		return result;
	}
	v3dxQuaternion* GetQuat() {
		auto px = GetPxObstacle();
		return (v3dxQuaternion*)(&px->mRot);
	}
};

class TR_CLASS()
	PhyBoxObstacle : public PhyObstacle
{
protected:
	physx::PxBoxObstacle		PxBox;
	virtual physx::PxObstacle* GetPxObstacle() {
		return &PxBox;
	}
public:
	PhyBoxObstacle() {
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

class TR_CLASS()
	PhyCapsuleObstacle : public PhyObstacle
{
protected:
	physx::PxCapsuleObstacle		PxCapsule;
	virtual physx::PxObstacle* GetPxObstacle() {
		return &PxCapsule;
	}
public:
	PhyCapsuleObstacle() {
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

class TR_CLASS()
	PhyObstacleContext : public PhyEntity
{
public:
	physx::PxObstacleContext*		mContext;
public:
	PhyObstacleContext();
	~PhyObstacleContext();

	void AddObstacle(PhyObstacle* obstacle);
	void RemoveObstacle(PhyObstacle* obstacle);

	UINT GetNbObstacles();
	PhyObstacle* GetObstacle(UINT i);
};

NS_END