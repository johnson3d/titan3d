#pragma once
#include "NvCommon.h"
#include "../PhyActor.h"

NS_BEGIN

class NvPhyScene;
class NvPhyShape;
class NvPhyActor : public PhyActor
{
public:
	ENGINE_RTTI(NvPhyActor)
public:
	TWeakRefHandle<NvPhyScene>		mScene;
	v3dxVector3						mPosition;
	v3dxQuaternion					mRotation;

	EPhyActorType					mActorType;
	physx::PxActor*					mActor;
public:
	NvPhyActor();
	~NvPhyActor();
	inline NvPhyScene* GetScene() const {
		return mScene.GetPtr();
	}
	inline const v3dxVector3* GetPostion() const {
		return &mPosition;
	}
	inline const v3dxQuaternion* GetRotation() const {
		return &mRotation;
	}
	virtual void Cleanup() override;
	void BindPhysX();
	bool AddToScene(PhyScene* scene);
	bool RemoveFromScene(PhyScene* scene);
	virtual void UpdateTransform();

	bool SetPose2Physics(const v3dxVector3* p, const v3dxQuaternion* q, bool autowake)
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
	bool AttachShape(PhyShape* shape, const v3dxVector3* p, const v3dxQuaternion* q)
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
	void DetachShape(PhyShape* shape, bool wakeOnLostTouch);

	bool SetRigidBodyFlag(EPhyRigidBodyFlag flag, bool value);
	bool SetActorFlag(EPhyActorFlag flag, bool value);

	void SetMass(float mass)
	{
		if (mActorType == EPhyActorType::PAT_Dynamic)
		{
			((physx::PxRigidDynamic*)mActor)->setMass(mass);
		}
	}
	float GetMass() const 
	{
		if (mActorType == EPhyActorType::PAT_Dynamic)
		{
			return ((physx::PxRigidDynamic*)mActor)->getMass();
		}
		return 0;
	}
	void SetMassSpaceInertiaTensor(const v3dxVector3* m)
	{
		if (mActorType == EPhyActorType::PAT_Dynamic)
		{
			((physx::PxRigidDynamic*)mActor)->setMassSpaceInertiaTensor(*(physx::PxVec3*)m);
		}
	}
	v3dxVector3 GetMassSpaceInertiaTensor() const
	{
		if (mActorType == EPhyActorType::PAT_Dynamic)
		{
			auto v3 = ((physx::PxRigidDynamic*)mActor)->getMassSpaceInertiaTensor();
			return *(v3dxVector3*)&v3;
		}
		return v3dxVector3::ZERO;
	}
	float GetMinCCDAdvanceCoefficient();
	void SetMinCCDAdvanceCoefficient(float advanceCoefficient);
};

NS_END