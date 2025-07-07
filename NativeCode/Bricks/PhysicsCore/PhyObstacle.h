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
public:
	virtual v3dVector3_t GetPos() = 0;
	virtual v3dxQuaternion* GetQuat() = 0;
	virtual void* GetInnerObstacle() = 0;
};

class TR_CLASS()
	PhyBoxObstacle : public PhyObstacle
{
public:
	virtual void SetHalfExtent(const v3dxVector3* halfExtent) = 0;
	virtual const v3dxVector3* GetHalfExtent() = 0;
};

class TR_CLASS()
	PhyCapsuleObstacle : public PhyObstacle
{
public:
	virtual void SetRadius(float v) = 0;
	virtual float GetRadius() = 0;
	virtual void SetHalfHeight(float v) = 0;
	virtual float GetHalfHeight() = 0;
};

class TR_CLASS()
	PhyObstacleContext : public PhyEntity
{
public:
	virtual void AddObstacle(PhyObstacle* obstacle) = 0;
	virtual void RemoveObstacle(PhyObstacle* obstacle) = 0;

	virtual UINT GetNbObstacles() = 0;
	virtual PhyObstacle* GetObstacle(UINT i) = 0;
};

NS_END