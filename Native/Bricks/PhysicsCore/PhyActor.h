#pragma once
#include "PhyEntity.h"

NS_BEGIN

class PhyScene;
class PhyShape;
class PhyActor : public PhyEntity
{
public:
	RTTI_DEF(PhyActor, 0x5860484d5befad18, true)

public:
	TObjectHandle<PhyScene>			mScene;
	v3dxVector3						mPosition;
	v3dxQuaternion					mRotation;

	EPhyActorType					mActorType;
	physx::PxActor*					mActor;
public:
	PhyActor();
	~PhyActor();
	virtual void Cleanup() override;
	void BindPhysX();
	vBOOL AddToScene(PhyScene* scene);
	virtual void UpdateTransform(const physx::PxActiveTransform* transform);

	vBOOL SetPose2Physics(const physx::PxTransform* transform, vBOOL autowake);
	vBOOL AttachShape(PhyShape* shape, const physx::PxTransform* relativePose);
	void DetachShape(PhyShape* shape, vBOOL wakeOnLostTouch);

	vBOOL SetRigidBodyFlag(UINT flag, vBOOL value);
	vBOOL SetActorFlag(UINT flag, vBOOL value);

	VDef_ReadOnly(Position);
	VDef_ReadOnly(Rotation);
};

NS_END