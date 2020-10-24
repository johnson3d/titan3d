#include "PhyScene.h"
#include "PhyActor.h"
#include "PhyController.h"
#include "PhyMaterial.h"
#include "PhyShape.h"

#define new VNEW

using namespace physx;

NS_BEGIN

RTTI_IMPL(EngineNS::PhyBoxControllerDesc, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::PhyCapsuleControllerDesc, EngineNS::VIUnknown);

RTTI_IMPL(EngineNS::PhySceneDesc, EngineNS::VIUnknown);
RTTI_IMPL(EngineNS::PhyScene, EngineNS::PhyEntity);

PhySceneDesc::ContactReportCallback::FPxSimulationFilterShader PhySceneDesc::ContactReportCallback::_PxSimulationFilterShader = nullptr;

PhySceneDesc::PhySceneDesc()
{
	mDesc = nullptr;
}

PhySceneDesc::~PhySceneDesc()
{
	Safe_Delete(mDesc);
}

void PhySceneDesc::Init()
{
	mDesc->simulationEventCallback = &ContactCB;
	if (!mDesc->cpuDispatcher)
	{
		physx::PxDefaultCpuDispatcher* dispatcher = physx::PxDefaultCpuDispatcherCreate(1);
		if (!dispatcher)
		{
			VFX_LTRACE(3, vT("PxDefaultCpuDispatcherCreate(Ver = %d) failed!\r\n"), PX_PHYSICS_VERSION);
		}
		mDesc->cpuDispatcher = dispatcher;
	}

	static physx::PxSimulationFilterShader defaultFilterShader = physx::PxDefaultSimulationFilterShader;
	if (!mDesc->filterShader)
	{
		mDesc->filterShader = defaultFilterShader;
	}

	if (!mDesc->cpuDispatcher)
	{
		PxDefaultCpuDispatcher* mCpuDispatcher = PxDefaultCpuDispatcherCreate(1);
		if (!mCpuDispatcher)
			VFX_LTRACE(3, vT("PxDefaultCpuDispatcherCreate(Ver = %d) failed!\r\n"), PX_PHYSICS_VERSION);
		mDesc->cpuDispatcher = mCpuDispatcher;
	}
#if defined PLATFORM_WIN
	/*if (!mDesc.gpuDispatcher && cudaManager)
	{
		sceneDesc.gpuDispatcher = cudaManager->getGpuDispatcher();
	}*/
#endif
}

void PhySceneDesc::SetContactCallBack(void* handle, ContactReportCallback::FonContact onContact,
	ContactReportCallback::FonTrigger onTrigger,
	ContactReportCallback::FonConstraintBreak onConstraintBreak,
	ContactReportCallback::FonWake onWake,
	ContactReportCallback::FonSleep onSleep,
	ContactReportCallback::FonAdvance onAdvance,
	ContactReportCallback::FPxSimulationFilterShader pxSimulationFilterShader)
{
	ContactCB.Handle = handle;
	ContactCB._onContact = onContact;
	ContactCB._onTrigger = onTrigger;
	ContactCB._onConstraintBreak = onConstraintBreak;
	ContactCB._onWake = onWake;
	ContactCB._onSleep = onSleep;
	ContactCB._onAdvance = onAdvance;
	ContactCB._PxSimulationFilterShader = pxSimulationFilterShader;
	mDesc->filterShader = &ContactReportCallback::CorePxSimulationFilterShader;
	/*mDesc->filterShader = (physx::PxSimulationFilterShader)pxSimulationFilterShader;
	{
		physx::PxFilterObjectAttributes attributes0 = 0;
		physx::PxFilterData filterData0;
		physx::PxFilterObjectAttributes attributes1 = 0;
		physx::PxFilterData filterData1;
		physx::PxPairFlags pairFlags;
		const void* constantBlock = nullptr;
		physx::PxU32 constantBlockSize = 0;
		pxSimulationFilterShader(attributes0, filterData0, attributes1, filterData1, pairFlags, constantBlock, constantBlockSize);
		mDesc->filterShader(attributes0, filterData0, attributes1, filterData1, pairFlags, constantBlock, constantBlockSize);
	}*/
}

PhyScene::PhyScene()
{
	mScene = nullptr;
	ControllerManager = nullptr;
	ObstacleContext = nullptr;
	EntityType = Phy_Scene;
}

PhyScene::~PhyScene()
{
	Cleanup();
}

void PhyScene::Cleanup()
{
	if (mScene != nullptr)
	{
		{
			physx::PxSceneWriteLock Lock(*mScene);
			ControllerManager->release();
			ControllerManager = nullptr;
		}

		mScene->userData = nullptr;
		mScene->release();
		mScene = nullptr;
	}
}

void PhyScene::BindPhysX()
{
	ASSERT(mScene);
	mScene->userData = this;

	physx::PxSceneWriteLock Lock(*mScene);
	ControllerManager = PxCreateControllerManager(*mScene);
}

void PhyScene::UpdateActorTransforms()
{
	PxSceneWriteLock writeLock(*mScene);
	PxU32 activeTransformsCount;
	const PxActiveTransform* activeTransforms = mScene->getActiveTransforms(activeTransformsCount);
	for (PxU32 i = 0; i < activeTransformsCount; ++i)
	{
		if (activeTransforms[i].userData != NULL)
		{
			PhyEntity* entity = static_cast<PhyEntity*>(activeTransforms[i].userData);
			if (entity->EntityType == PhyEntityType::Phy_Actor)
			{
				PhyActor* actor = (PhyActor*)entity;
				actor->UpdateTransform(&activeTransforms[i]);
			}
		}
	}
}

vBOOL PhyScene::Raycast(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult)
{
	PhyQueryFilterData queryFilterData;
	return RaycastWithFilter(origin, unitDir, maxDistance, &queryFilterData, OUT hitResult);
}
vBOOL PhyScene::RaycastWithFilter(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, PhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult)
{
	PxRaycastBuffer hit;
	PxVec3& pxOrgin = *(PxVec3*)origin;
	PxVec3& pxUnitDir = *(PxVec3*)unitDir;
	PxQueryFilterData data;/*(PxQueryFlags)filterData*/;
	data.data = queryFilterData->data;
	data.flags = (PxQueryFlags)queryFilterData->flag;
	PxHitFlags flags = PxHitFlags((physx::PxU16)hitResult->HitFlags);
	{
		vBOOL status;
		{
			PxSceneReadLock scopedLock(*mScene);
			//PxSceneWriteLock scopedLock(*m_pPxScene);
			//status = mScene->raycast(pxOrgin, pxUnitDir, maxDistance, hit);
			status = mScene->raycast(pxOrgin, pxUnitDir, maxDistance, hit, flags, data);
		}
		if (status)
		{
			hitResult->Position = *(v3dxVector3*)&hit.block.position;
			hitResult->Normal = *(v3dxVector3*)&hit.block.normal;
			hitResult->Distance = hit.block.distance;

			hitResult->FaceId = hit.block.faceIndex;
			hitResult->U = hit.block.u;
			hitResult->V = hit.block.v;
			if (hit.block.actor != nullptr)
			{
				auto pPhyActor = (PhyEntity*)hit.block.actor->userData;
				if (pPhyActor && pPhyActor->EntityType == PhyEntityType::Phy_Actor)
					hitResult->ExtData = pPhyActor->GetCSharpHandle();
				else
					hitResult->ExtData = nullptr;
			}
			else
				hitResult->ExtData = nullptr;
			return TRUE;
		}
	}
	return FALSE;
}
vBOOL PhyScene::Sweep(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult)
{
	PhyQueryFilterData queryFilterData;
	return SweepWithFilter(shape, position, unitDir, maxDistance, &queryFilterData, OUT hitResult);
}
vBOOL PhyScene::SweepWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, PhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult)
{
	PxSweepBuffer hit;
	PxGeometry* geo = &shape->mShape->getGeometry().any();
	PxTransform transform(position->x, position->y, position->z);

	PxVec3& pxUnitDir = *(PxVec3*)unitDir;
	PxQueryFilterData data;/*(PxQueryFlags)filterData*/;
	data.data = queryFilterData->data;
	data.flags = (PxQueryFlags)queryFilterData->flag;
	PxHitFlags hitFlags = PxHitFlags((physx::PxU16)hitResult->HitFlags);
	{
		vBOOL status;
		{
			PxSceneReadLock scopedLock(*mScene);
			//PxSceneWriteLock scopedLock(*m_pPxScene);
			status = mScene->sweep(*geo, transform, pxUnitDir, maxDistance, hit, hitFlags, data);
		}
		if (status)
		{
			hitResult->Position = *(v3dxVector3*)&hit.block.position;
			hitResult->Normal = *(v3dxVector3*)&hit.block.normal;
			hitResult->Distance = hit.block.distance;

			//hitResult->U = hit.block.u;
			//hitResult->V = hit.block.v;
			if (hit.block.actor != nullptr)
			{
				auto pPhyActor = (PhyEntity*)hit.block.actor->userData;
				if (pPhyActor && pPhyActor->EntityType == PhyEntityType::Phy_Actor)
					hitResult->ExtData = pPhyActor->GetCSharpHandle();
				else
					hitResult->ExtData = nullptr;
			}
			else
				hitResult->ExtData = nullptr;
			return TRUE;
		}
	}
	return FALSE;
}

vBOOL PhyScene::Overlap(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, OUT VHitResult* hitResult)
{
	PhyQueryFilterData queryFilterData;
	return OverlapWithFilter(shape, position, rotation, &queryFilterData, OUT hitResult);
}
vBOOL PhyScene::OverlapWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, PhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult)
{
	PxOverlapBuffer hit;
	PxGeometry* geo = &shape->mShape->getGeometry().any();
	PxQuat quat(rotation->x, rotation->y, rotation->z, rotation->w);
	PxTransform transform(position->x, position->y, position->z, quat);
	PxQueryFilterData data;/*(PxQueryFlags)filterData*/;
	data.data = queryFilterData->data;
	data.flags = (PxQueryFlags)queryFilterData->flag;
	{
		vBOOL status;
		{
			PxSceneReadLock scopedLock(*mScene);
			status = mScene->overlap(*geo, transform, hit, data);
		}
		if (status)
		{
			if (hit.block.actor != nullptr)
			{
				auto pPhyActor = (PhyEntity*)hit.block.actor->userData;
				if (pPhyActor && pPhyActor->EntityType == PhyEntityType::Phy_Actor)
					hitResult->ExtData = pPhyActor->GetCSharpHandle();
				else
					hitResult->ExtData = nullptr;
			}
			else
				hitResult->ExtData = nullptr;
			return TRUE;
		}
	}
	return FALSE;
}

void PhyControllerDesc::SetMaterial(PhyMaterial* mtl)
{
	mDesc->material = mtl->mMaterial;
}

void PhyControllerDesc::SetQueryFilterData(physx::PxFilterData* data)
{
	mFilterData = data;
}

PhyController* PhyScene::CreateBoxController(const PhyBoxControllerDesc* desc)
{
	physx::PxSceneWriteLock Lock(*mScene);
	auto ctr = ControllerManager->createController(desc->mBoxDesc);
	auto shapeCount = ctr->getActor()->getNbShapes();
	physx::PxShape** shapeList = new physx::PxShape*[shapeCount];
	ctr->getActor()->getShapes(shapeList, sizeof(physx::PxShape)*shapeCount);
	for (UINT i = 0; i < shapeCount; ++i)
	{
		shapeList[i]->setQueryFilterData(*desc->mFilterData);
	}
	PhyController* ret = new PhyController(this, ctr);
	ret->BindPhysX();
	Safe_DeleteArray(shapeList);
	return ret;
}

PhyController* PhyScene::CreateCapsuleController(const PhyCapsuleControllerDesc* desc)
{
	physx::PxSceneWriteLock Lock(*mScene);
	auto ctr = ControllerManager->createController(desc->mCapsuleDesc);
	auto shapeCount = ctr->getActor()->getNbShapes();
	physx::PxShape** shapeList = new physx::PxShape*[shapeCount];
	ctr->getActor()->getShapes(shapeList, sizeof(physx::PxShape)*shapeCount);
	for (UINT i = 0; i < shapeCount; ++i)
	{
		shapeList[i]->setQueryFilterData(*desc->mFilterData);
	}
	PhyController* ret = new PhyController(this, ctr);
	ret->BindPhysX();
	Safe_DeleteArray(shapeList);
	return ret;
}

NS_END

using namespace EngineNS;

template <>
struct Type2TypeConverter<v3dxVector3>
{
	typedef v3dVector3_t		TarType;
};

extern "C"
{
	Cpp2CS1(EngineNS, PhySceneDesc, SetGravity);
	Cpp2CS1(EngineNS, PhySceneDesc, GetGravity);
	Cpp2CS1(EngineNS, PhySceneDesc, SetFlags);
	Cpp2CS0(EngineNS, PhySceneDesc, GetFlags);
	Cpp2CS1(EngineNS, PhySceneDesc, SetContactDataBlocks);
	Cpp2CS0(EngineNS, PhySceneDesc, GetContactDataBlocks);
	Cpp2CS8(EngineNS, PhySceneDesc, SetContactCallBack);

	Cpp2CS5(EngineNS, PhyScene, Simulate);
	Cpp2CS2(EngineNS, PhyScene, FetchResults);
	Cpp2CS0(EngineNS, PhyScene, UpdateActorTransforms);
	Cpp2CS4(EngineNS, PhyScene, Raycast);
	Cpp2CS5(EngineNS, PhyScene, Sweep);
	Cpp2CS4(EngineNS, PhyScene, Overlap);
	Cpp2CS5(EngineNS, PhyScene, RaycastWithFilter);
	Cpp2CS6(EngineNS, PhyScene, SweepWithFilter);
	Cpp2CS5(EngineNS, PhyScene, OverlapWithFilter);

	Cpp2CS1(EngineNS, PhyScene, CreateBoxController);
	Cpp2CS1(EngineNS, PhyScene, CreateCapsuleController);

	Cpp2CS1(EngineNS, PhyControllerDesc, SetMaterial);
	Cpp2CS1(EngineNS, PhyControllerDesc, SetQueryFilterData);

	Cpp2CS0(EngineNS, PhyBoxControllerDesc, GetExtent);
	Cpp2CS1(EngineNS, PhyBoxControllerDesc, SetExtent);

	Cpp2CS0(EngineNS, PhyCapsuleControllerDesc, GetCapsuleRadius);
	Cpp2CS1(EngineNS, PhyCapsuleControllerDesc, SetCapsuleRadius);
	Cpp2CS0(EngineNS, PhyCapsuleControllerDesc, GetCapsuleHeight);
	Cpp2CS1(EngineNS, PhyCapsuleControllerDesc, SetCapsuleHeight);
	Cpp2CS0(EngineNS, PhyCapsuleControllerDesc, GetCapsuleClimbingMode);
	Cpp2CS1(EngineNS, PhyCapsuleControllerDesc, SetCapsuleClimbingMode);
}