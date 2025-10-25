#include "Nv5PhyController.h"
#include "Nv5PhyScene.h"
#include "Nv5PhyActor.h"
#include "Nv5PhyMaterial.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::Nv5PhyBoxControllerDesc);
ENGINE_RTTI_IMPL(EngineNS::Nv5PhyCapsuleControllerDesc);
ENGINE_RTTI_IMPL(EngineNS::Nv5PhyController);

struct vPhysXCharacterControllerCallBack_ControllerFilter : public physx::PxControllerFilterCallback
{
	virtual bool filter(const physx::PxController& a, const physx::PxController& b)
	{
		physx::PxShape* aShape = NULL;
		physx::PxU32 nb = a.getActor()->getShapes(&aShape, 1);
		PX_ASSERT(nb == 1);
		physx::PxShape* bShape = NULL;
		nb = b.getActor()->getShapes(&bShape, 1);
		PX_ASSERT(nb == 1);

		auto aData = aShape->getQueryFilterData();
		auto bData = bShape->getQueryFilterData();
		if ((aData.word0 & bData.word1) == aData.word0 && (aData.word1 & bData.word0) == bData.word0)
			return true;
		else
			return false;
	}
} static ControllerFilterCallback;

struct vPhysXCharacterControllerCallBack_QueryFilter : public physx::PxQueryFilterCallback
{
	virtual PxQueryHitType::Enum preFilter(const PxFilterData& filterData, const PxShape* shape, const PxRigidActor* actor, PxHitFlags& queryFlags) override
	{
		return physx::PxQueryHitType::eBLOCK;
		if (shape->getFlags()&physx::PxShapeFlag::eTRIGGER_SHAPE)
			return physx::PxQueryHitType::eTOUCH;
		auto shapeData = shape->getQueryFilterData();
		if ((filterData.word0 & shapeData.word1) == filterData.word0 && (filterData.word1&shapeData.word0) == shapeData.word0)
			return physx::PxQueryHitType::eBLOCK;
		else
			return  physx::PxQueryHitType::eNONE;
	}
	virtual PxQueryHitType::Enum postFilter(const PxFilterData& filterData, const PxQueryHit& hit, const PxShape* shape, const PxRigidActor* actor) override
	{
		return physx::PxQueryHitType::eBLOCK;
	}
} static QueryFilterCallback;

struct vPhysXCharacterControllerCallBack_HitReport : public physx::PxUserControllerHitReport
{
	virtual void onShapeHit(const physx::PxControllerShapeHit& hit)
	{

	}

	virtual void onControllerHit(const physx::PxControllersHit& hit)
	{

	}

	virtual void onObstacleHit(const physx::PxControllerObstacleHit& hit)
	{

	}
} static HitReport;
struct vPhysXCharacterControllerCallBack_Behavior : public physx::PxControllerBehaviorCallback
{
	virtual physx::PxControllerBehaviorFlags getBehaviorFlags(const physx::PxShape& shape, const physx::PxActor& actor)
	{
		return physx::PxControllerBehaviorFlags(0);
	}

	virtual physx::PxControllerBehaviorFlags getBehaviorFlags(const physx::PxController& controller)
	{
		return physx::PxControllerBehaviorFlags(0);
	}

	virtual physx::PxControllerBehaviorFlags getBehaviorFlags(const physx::PxObstacle& obstacle)
	{
		return physx::PxControllerBehaviorFlags(0);
	}
}static Behavior;

void Nv5PhyBoxControllerDesc::SetMaterial(PhyMaterial* mtl)
{
	if (mtl == nullptr)
		mBoxDesc.material = nullptr;
	else
		mBoxDesc.material = static_cast<Nv5PhyMaterial*>(mtl)->mMaterial;
}

void Nv5PhyCapsuleControllerDesc::SetMaterial(PhyMaterial* mtl)
{
	if (mtl == nullptr)
		mCapsuleDesc.material = nullptr;
	else
		mCapsuleDesc.material = static_cast<Nv5PhyMaterial*>(mtl)->mMaterial;
}

Nv5PhyCapsuleControllerDesc::Nv5PhyCapsuleControllerDesc()
{
	mCapsuleDesc.behaviorCallback = &Behavior;
	mCapsuleDesc.reportCallback = &HitReport;
}

Nv5PhyController::Nv5PhyController(PhyScene* scene, physx::PxController* ctr)
{
	mController = ctr;
	ASSERT(mController);
	mScene.FromObject(scene);
	EntityType = Phy_Controller;

	mActor = new Nv5PhyActor();
	mActor->mActor = ctr->getActor();
}

Nv5PhyController::~Nv5PhyController()
{
	Cleanup();
}

PhyActor* Nv5PhyController::GetReadOnlyActor() 
{
	if (mActor == nullptr || mActor->mActor == nullptr)
		return nullptr;
	return mActor;
}

void Nv5PhyController::Cleanup()
{
	if (mController != nullptr)
	{
		mActor->mActor->userData = nullptr;
		mActor->mActor = nullptr;
		Safe_Release(mActor);
		//destroy pxActor
		auto pScene = mScene.GetCastPtr<Nv5PhyScene>();
		if (pScene != nullptr)
		{
			physx::PxSceneWriteLock Lock(*pScene->mScene);
			mController->setUserData(nullptr);
			mController->release();
		}
		mController = nullptr;
	}
}

void Nv5PhyController::BindPhysX()
{
	ASSERT(mController);
	mController->setUserData(this);
	mController->getActor()->userData = this;
}

EPhyControllerCollisionFlag Nv5PhyController::Move(const v3dxVector3* disp, float minDist, float elapsedTime,
	const FPhyFilterData* filterData, EPhyQueryFlag filterFlags)
{
	auto pScene = mScene.GetCastPtr<Nv5PhyScene>();

	if (pScene == nullptr)
		return (EPhyControllerCollisionFlag)physx::PxControllerCollisionFlag::Enum::eCOLLISION_SIDES;

	physx::PxControllerFilters cFilter;
	cFilter.mCCTFilterCallback = &ControllerFilterCallback;
	cFilter.mFilterCallback = &QueryFilterCallback;
	cFilter.mFilterData = (const physx::PxFilterData*)filterData;
	cFilter.mFilterFlags = (physx::PxQueryFlag::Enum)filterFlags;

	physx::PxSceneWriteLock Lock(*pScene->mScene);
	auto ret = (uint8_t)mController->move(*(const physx::PxVec3*)disp, minDist, elapsedTime, cFilter, pScene->ObstacleContext);
	return (EPhyControllerCollisionFlag)(ret);
}

void Nv5PhyController::SetPosition(const v3dxVector3* position)
{
	physx::PxExtendedVec3 vec;
	vec.x = position->X;
	vec.y = position->Y;
	vec.z = position->Z;

	auto pScene = mScene.GetCastPtr<Nv5PhyScene>();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setPosition(vec);
}

v3dxVector3 Nv5PhyController::GetPosition()
{
	v3dxVector3 result;
	const physx::PxExtendedVec3& vec = mController->getPosition();
	result.X = (float)vec.x;
	result.Y = (float)vec.y;
	result.Z = (float)vec.z;
	return result;
}
void Nv5PhyController::SetFootPosition(const v3dxVector3* position)
{
	physx::PxExtendedVec3 vec;
	vec.x = position->X;
	vec.y = position->Y;
	vec.z = position->Z;

	auto pScene = mScene.GetCastPtr<Nv5PhyScene>();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setFootPosition(vec);
}

v3dxVector3 Nv5PhyController::GetFootPosition()
{
	v3dxVector3 result;
	const physx::PxExtendedVec3& vec = mController->getFootPosition();
	result.X = (float)vec.x;
	result.Y = (float)vec.y;
	result.Z = (float)vec.z;
	return result;
}
float Nv5PhyController::GetContactOffset()
{
	return mController->getContactOffset();
}

void Nv5PhyController::SetContactOffset(float offset)
{
	auto pScene = mScene.GetCastPtr<Nv5PhyScene>();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setContactOffset(offset);
}

float Nv5PhyController::GetSlopeLimit()
{
	return mController->getSlopeLimit();
}

void Nv5PhyController::SetSlopeLimit(float slopeLimit)
{
	auto pScene = mScene.GetCastPtr<Nv5PhyScene>();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setSlopeLimit(slopeLimit);
}
void Nv5PhyController::SetQueryFilterData(const FPhyFilterData* filterData)
{
	auto pScene = mScene.GetCastPtr<Nv5PhyScene>();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	auto shapeCount = mController->getActor()->getNbShapes();
	physx::PxShape* shapeList_array[32];
	physx::PxShape** shapeList = nullptr;
	if (shapeCount > 32)
	{
		shapeList = new physx::PxShape * [shapeCount];
	}
	else
	{
		shapeList = shapeList_array;
	}
	mController->getActor()->getShapes(shapeList, sizeof(physx::PxShape) * shapeCount);
	for (UINT i = 0; i < shapeCount; ++i)
	{
		shapeList[i]->setQueryFilterData(*(physx::PxFilterData*)filterData);
	}
	if (shapeCount > 32)
	{
		delete[] shapeList;
	}
}
void Nv5PhyController::SetSimulationFilterData(const FPhyFilterData* filterData)
{
	auto pScene = mScene.GetCastPtr<Nv5PhyScene>();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	auto shapeCount = mController->getActor()->getNbShapes();
	physx::PxShape* shapeList_array[32];
	physx::PxShape** shapeList = nullptr;
	if (shapeCount > 32)
	{
		shapeList = new physx::PxShape * [shapeCount];
	}
	else
	{
		shapeList = shapeList_array;
	}
	mController->getActor()->getShapes(shapeList, sizeof(physx::PxShape) * shapeCount);
	for (UINT i = 0; i < shapeCount; ++i)
	{
		shapeList[i]->setSimulationFilterData(*(physx::PxFilterData*)filterData);
	}
	if (shapeCount > 32)
	{
		delete[] shapeList;
	}
}
NS_END


