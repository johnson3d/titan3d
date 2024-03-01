#include "PhyController.h"
#include "PhyScene.h"
#include "PhyActor.h"
#include "PhyMaterial.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::PhyBoxControllerDesc);
ENGINE_RTTI_IMPL(EngineNS::PhyCapsuleControllerDesc);
ENGINE_RTTI_IMPL(EngineNS::PhyController);

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
} ControllerFilterCallback;

struct vPhysXCharacterControllerCallBack_QueryFilter : public physx::PxQueryFilterCallback
{
	virtual physx::PxQueryHitType::Enum preFilter(const physx::PxFilterData& filterData, const physx::PxShape* shape, const physx::PxRigidActor* actor, physx::PxSceneQueryFlags& queryFlags)
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
	virtual	physx::PxQueryHitType::Enum postFilter(const physx::PxFilterData& filterData, const physx::PxSceneQueryHit& hit)
	{
		return physx::PxQueryHitType::eBLOCK;
	}
} QueryFilterCallback;

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
}HitReport;
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
}Behavior;

PhyCapsuleControllerDesc::PhyCapsuleControllerDesc()
{
	mDesc = &mCapsuleDesc;
	mDesc->behaviorCallback = &Behavior;
	mDesc->reportCallback = &HitReport;
}

PhyController::PhyController(PhyScene* scene, physx::PxController* ctr)
{
	mController = ctr;
	mScene.FromObject(scene);
	EntityType = Phy_Controller;

	mActor = new PhyActor();
	mActor->mActor = ctr->getActor();
}

PhyController::~PhyController()
{
	Cleanup();
}

PhyActor* PhyController::GetReadOnlyActor() 
{
	if (mActor == nullptr || mActor->mActor == nullptr)
		return nullptr;
	return mActor;
}

void PhyController::Cleanup()
{
	if (mController != nullptr)
	{
		mActor->mActor = nullptr;
		Safe_Release(mActor);
		//destroy pxActor
		auto pScene = mScene.GetPtr();
		if (pScene != nullptr)
		{
			physx::PxSceneWriteLock Lock(*pScene->mScene);
			mController->release();
		}
		mController = nullptr;
	}
}

void PhyController::BindPhysX()
{
	ASSERT(mController);
	mController->setUserData(this);
	mController->getActor()->userData = this;
}

EPhyControllerCollisionFlag PhyController::Move(const v3dxVector3* disp, float minDist, float elapsedTime,
	const PhyFilterData* filterData, PhyQueryFlag filterFlags)
{
	auto pScene = mScene.GetPtr();

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

void PhyController::SetPosition(const v3dxVector3* position)
{
	physx::PxExtendedVec3 vec;
	vec.x = position->X;
	vec.y = position->Y;
	vec.z = position->Z;

	auto pScene = mScene.GetPtr();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setPosition(vec);
}

v3dxVector3 PhyController::GetPosition()
{
	v3dxVector3 result;
	const physx::PxExtendedVec3& vec = mController->getPosition();
	result.X = (float)vec.x;
	result.Y = (float)vec.y;
	result.Z = (float)vec.z;
	return result;
}
void PhyController::SetFootPosition(const v3dxVector3* position)
{
	physx::PxExtendedVec3 vec;
	vec.x = position->X;
	vec.y = position->Y;
	vec.z = position->Z;

	auto pScene = mScene.GetPtr();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setFootPosition(vec);
}

v3dxVector3 PhyController::GetFootPosition()
{
	v3dxVector3 result;
	const physx::PxExtendedVec3& vec = mController->getFootPosition();
	result.X = (float)vec.x;
	result.Y = (float)vec.y;
	result.Z = (float)vec.z;
	return result;
}
float PhyController::GetContactOffset()
{
	return mController->getContactOffset();
}

void PhyController::SetContactOffset(float offset)
{
	auto pScene = mScene.GetPtr();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setContactOffset(offset);
}

float PhyController::GetSlopeLimit()
{
	return mController->getSlopeLimit();
}

void PhyController::SetSlopeLimit(float slopeLimit)
{
	auto pScene = mScene.GetPtr();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setSlopeLimit(slopeLimit);
}
void PhyController::SetQueryFilterData(const PhyFilterData* filterData)
{
	auto pScene = mScene.GetPtr();
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
void PhyController::SetSimulationFilterData(const PhyFilterData* filterData)
{
	auto pScene = mScene.GetPtr();
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


