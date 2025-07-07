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
	TWeakRefHandle<PhyScene>		mScene;
	v3dxVector3						mPosition;
	v3dxQuaternion					mRotation;

	EPhyActorType					mActorType;
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
	virtual void BindPhysX() = 0;
	virtual bool AddToScene(PhyScene* scene) = 0;
	virtual bool RemoveFromScene(PhyScene* scene) = 0;
	virtual void UpdateTransform() = 0;

	virtual bool SetPose2Physics(const v3dxVector3* p, const v3dxQuaternion* q, bool autowake) = 0;
	virtual bool AttachShape(PhyShape* shape, const v3dxVector3* p, const v3dxQuaternion* q) = 0;
	virtual void DetachShape(PhyShape* shape, bool wakeOnLostTouch) = 0;

	virtual bool SetRigidBodyFlag(EPhyRigidBodyFlag flag, bool value) = 0;
	virtual bool SetActorFlag(EPhyActorFlag flag, bool value) = 0;

	virtual void SetMass(float mass) = 0;
	virtual float GetMass() const = 0;
	virtual void SetMassSpaceInertiaTensor(const v3dxVector3* m) = 0;
	virtual v3dxVector3 GetMassSpaceInertiaTensor() const = 0;
	virtual float GetMinCCDAdvanceCoefficient() = 0;
	virtual void SetMinCCDAdvanceCoefficient(float advanceCoefficient) = 0;
};

NS_END