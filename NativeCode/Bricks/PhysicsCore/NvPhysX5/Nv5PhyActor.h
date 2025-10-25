#pragma once
#include "Nv5Common.h"
#include "../PhyActor.h"

NS_BEGIN

class Nv5PhyScene;
class Nv5PhyShape;
class Nv5PhyActor : public PhyActor
{
public:
	ENGINE_RTTI(Nv5PhyActor)
public:
	TWeakRefHandle<Nv5PhyScene>		mScene;
	v3dxVector3						mPosition;
	v3dxQuaternion					mRotation;

	EPhyActorType					mActorType;
	physx::PxActor*					mActor;
public:
	Nv5PhyActor();
	~Nv5PhyActor();
	inline Nv5PhyScene* GetScene() const {
		return mScene.GetPtr();
	}
	inline const v3dxVector3* GetPostion() const {
		return &mPosition;
	}
	inline const v3dxQuaternion* GetRotation() const {
		return &mRotation;
	}
	virtual void Cleanup() override;
	virtual void BindPhysX() override;
	virtual bool AddToScene(PhyScene* scene) override;
	virtual bool RemoveFromScene(PhyScene* scene) override;
	virtual void UpdateTransform() override;

	virtual bool SetPose2Physics(const v3dxVector3* p, const v3dxQuaternion* q, bool autowake) override
	{
		physx::PxTransform trf;
		trf.p.x = p->X;
		trf.p.y = p->Y;
		trf.p.z = p->Z;
		trf.q.x = q->X;
		trf.q.y = q->Y;
		trf.q.z = q->Z;
		trf.q.w = q->W;
		return SetPose2Physics(&trf, autowake ? TRUE : FALSE);
	}
	bool SetPose2Physics(const physx::PxTransform* transform, bool autowake);
	virtual bool AttachShape(PhyShape* shape, const v3dxVector3* p, const v3dxQuaternion* q) override
	{
		physx::PxTransform trf;
		trf.p.x = p->X;
		trf.p.y = p->Y;
		trf.p.z = p->Z;
		trf.q.x = q->X;
		trf.q.y = q->Y;
		trf.q.z = q->Z;
		trf.q.w = q->W;
		return AttachShape(shape, &trf);
	}
	bool AttachShape(PhyShape* shape, const physx::PxTransform* relativePose);
	virtual void DetachShape(PhyShape* shape, bool wakeOnLostTouch) override;

	virtual bool SetRigidBodyFlag(EPhyRigidBodyFlag flag, bool value) override;
	virtual bool SetActorFlag(EPhyActorFlag flag, bool value) override;

	virtual void SetMass(float mass) override
	{
		if (mActorType == EPhyActorType::PAT_Dynamic)
		{
			((physx::PxRigidDynamic*)mActor)->setMass(mass);
		}
	}
	virtual float GetMass() const override
	{
		if (mActorType == EPhyActorType::PAT_Dynamic)
		{
			return ((physx::PxRigidDynamic*)mActor)->getMass();
		}
		return 0;
	}
	virtual void SetMassSpaceInertiaTensor(const v3dxVector3* m) override
	{
		if (mActorType == EPhyActorType::PAT_Dynamic)
		{
			((physx::PxRigidDynamic*)mActor)->setMassSpaceInertiaTensor(*(physx::PxVec3*)m);
		}
	}
	virtual v3dxVector3 GetMassSpaceInertiaTensor() const override
	{
		if (mActorType == EPhyActorType::PAT_Dynamic)
		{
			auto v3 = ((physx::PxRigidDynamic*)mActor)->getMassSpaceInertiaTensor();
			return *(v3dxVector3*)&v3;
		}
		return v3dxVector3::ZERO;
	}
	virtual float GetMinCCDAdvanceCoefficient() override;
	virtual void SetMinCCDAdvanceCoefficient(float advanceCoefficient) override;
};

NS_END