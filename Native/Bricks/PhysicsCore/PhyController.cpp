#include "PhyController.h"
#include "PhyScene.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::PhyController, EngineNS::PhyEntity);

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
} ControllerFilter;

struct vPhysXCharacterControllerCallBack_QueryFilter : public physx::PxQueryFilterCallback
{
	virtual physx::PxQueryHitType::Enum preFilter(const physx::PxFilterData& filterData, const physx::PxShape* shape, const physx::PxRigidActor* actor, physx::PxSceneQueryFlags& queryFlags)
	{
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
} QueryFilter;

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
		return physx::PxControllerBehaviorFlag::eCCT_CAN_RIDE_ON_OBJECT;
	}

	virtual physx::PxControllerBehaviorFlags getBehaviorFlags(const physx::PxController& controller)
	{
		return physx::PxControllerBehaviorFlag::eCCT_CAN_RIDE_ON_OBJECT;
	}

	virtual physx::PxControllerBehaviorFlags getBehaviorFlags(const physx::PxObstacle& obstacle)
	{
		return physx::PxControllerBehaviorFlag::eCCT_CAN_RIDE_ON_OBJECT;
	}
}Behavior;


PhyController::PhyController(PhyScene* scene, physx::PxController* ctr)
{
	mController = ctr;
	mScene.FromObject(scene);
	EntityType = Phy_Controller;
}

PhyController::~PhyController()
{
	Cleanup();
}

void PhyController::Cleanup()
{
	if (mController != nullptr)
	{
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

physx::PxControllerCollisionFlags PhyController::Move(const v3dxVector3* disp, float minDist, float elapsedTime,
	const physx::PxFilterData* filterData, physx::PxU16 filterFlags)
{
	auto pScene = mScene.GetPtr();

	if (pScene == nullptr)
		return physx::PxControllerCollisionFlag::Enum::eCOLLISION_SIDES;

	physx::PxControllerFilters cFilter;
	cFilter.mCCTFilterCallback = &ControllerFilter;
	cFilter.mFilterCallback = &QueryFilter;
	cFilter.mFilterData = filterData;
	cFilter.mFilterFlags = (physx::PxQueryFlag::Enum)filterFlags;

	physx::PxSceneWriteLock Lock(*pScene->mScene);
	return mController->move(*(const physx::PxVec3*)disp, minDist, elapsedTime, cFilter, pScene->ObstacleContext);
}

void PhyController::SetPosition(const v3dxVector3* position)
{
	physx::PxExtendedVec3 vec;
	vec.x = position->x;
	vec.y = position->y;
	vec.z = position->z;

	auto pScene = mScene.GetPtr();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setPosition(vec);
}

v3dxVector3 PhyController::GetPosition()
{
	v3dxVector3 result;
	const physx::PxExtendedVec3& vec = mController->getPosition();
	result.x = (float)vec.x;
	result.y = (float)vec.y;
	result.z = (float)vec.z;
	return result;
}
void PhyController::SetFootPosition(const v3dxVector3* position)
{
	physx::PxExtendedVec3 vec;
	vec.x = position->x;
	vec.y = position->y;
	vec.z = position->z;

	auto pScene = mScene.GetPtr();
	physx::PxSceneWriteLock Lock(*pScene->mScene);
	mController->setFootPosition(vec);
}

v3dxVector3 PhyController::GetFootPosition()
{
	v3dxVector3 result;
	const physx::PxExtendedVec3& vec = mController->getFootPosition();
	result.x = (float)vec.x;
	result.y = (float)vec.y;
	result.z = (float)vec.z;
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
void PhyController::SetQueryFilterData(physx::PxFilterData* filterData)
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
		shapeList[i]->setQueryFilterData(*filterData);
	}
	if (shapeCount > 32)
	{
		delete[] shapeList;
	}
}
NS_END

using namespace EngineNS;

template <>
struct Type2TypeConverter<physx::PxControllerCollisionFlags>
{
	typedef physx::PxU8	TarType;
};

template <>
struct Type2TypeConverter<v3dxVector3>
{
	typedef v3dVector3_t	TarType;
};

extern "C"
{
	Cpp2CS5(EngineNS, PhyController, Move);
	Cpp2CS1(EngineNS, PhyController, SetPosition);
	Cpp2CS0(EngineNS, PhyController, GetPosition);
	Cpp2CS1(EngineNS, PhyController, SetFootPosition);
	Cpp2CS0(EngineNS, PhyController, GetFootPosition);
	Cpp2CS0(EngineNS, PhyController, GetContactOffset);
	Cpp2CS1(EngineNS, PhyController, SetContactOffset);
	Cpp2CS0(EngineNS, PhyController, GetSlopeLimit);
	Cpp2CS1(EngineNS, PhyController, SetSlopeLimit);
	Cpp2CS1(EngineNS, PhyController, SetQueryFilterData);
}