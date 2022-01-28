#pragma once
#include "PhyEntity.h"

NS_BEGIN

enum TR_ENUM()
	EPhyRigidBodyFlag
{
	PRF_eKINEMATIC = (1 << 0),		//!< Enable kinematic mode for the body.
	PRF_eUSE_KINEMATIC_TARGET_FOR_SCENE_QUERIES = (1 << 1),
	PRF_eENABLE_CCD = (1 << 2),		//!< Enable CCD for the body.
	PRF_eENABLE_CCD_FRICTION = (1 << 3),
	PRF_eENABLE_POSE_INTEGRATION_PREVIEW = (1 << 4),
	PRF_eENABLE_SPECULATIVE_CCD = (1 << 5),
	PRF_eENABLE_CCD_MAX_CONTACT_IMPULSE = (1 << 6),
	PRF_eRETAIN_ACCELERATIONS = (1 << 7),
	PRF_eFORCE_KINE_KINE_NOTIFICATIONS = (1 << 8),
	PRF_eFORCE_STATIC_KINE_NOTIFICATIONS = (1 << 9),
	PRF_eRESERVED = (1 << 15),
};

enum TR_ENUM()
	EPhyActorFlag
{
	PAF_eVISUALIZATION = (1 << 0),
	PAF_eDISABLE_GRAVITY = (1 << 1),
	PAF_eSEND_SLEEP_NOTIFIES = (1 << 2),
	PAF_eDISABLE_SIMULATION = (1 << 3),
};

class PhyScene;
class PhyShape;
class TR_CLASS()
	PhyActor : public PhyEntity
{
public:
	ENGINE_RTTI(PhyActor)
public:
	TObjectHandle<PhyScene>			mScene;
	v3dxVector3						mPosition;
	v3dxQuaternion					mRotation;

	EPhyActorType					mActorType;
	physx::PxActor*					mActor;
public:
	PhyActor();
	~PhyActor();
	inline PhyScene* GetScene() const {
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
	virtual void UpdateTransform();

	bool SetPose2Physics(const v3dxVector3* p, const v3dxQuaternion* q, bool autowake)
	{
		physx::PxTransform trf;
		trf.p.x = p->x;
		trf.p.y = p->y;
		trf.p.z = p->z;
		trf.q.x = q->x;
		trf.q.y = q->y;
		trf.q.z = q->z;
		trf.q.w = q->w;
		return SetPose2Physics(&trf, autowake ? TRUE : FALSE);
	}
	bool SetPose2Physics(const physx::PxTransform* transform, bool autowake);
	bool AttachShape(PhyShape* shape, const v3dxVector3* p, const v3dxQuaternion* q)
	{
		physx::PxTransform trf;
		trf.p.x = p->x;
		trf.p.y = p->y;
		trf.p.z = p->z;
		trf.q.x = q->x;
		trf.q.y = q->y;
		trf.q.z = q->z;
		trf.q.w = q->w;
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