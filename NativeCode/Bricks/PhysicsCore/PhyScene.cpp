#include "PhyScene.h"
#include "PhyActor.h"
#include "PhyController.h"
#include "PhyMaterial.h"
#include "PhyShape.h"
#include "PhyObstacle.h"
#include "PhyController.h"
#include "PxSceneDesc.h"

#define new VNEW

using namespace physx;

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::PhySceneDesc);
ENGINE_RTTI_IMPL(EngineNS::PhyScene);

FSimulationFilterShader PhySimulationFilterShader::_CustomSimulationFilterShader = nullptr;

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
	mDesc->simulationEventCallback = &SimulationEventCallback;
	mDesc->filterShader = PhySimulationFilterShader::DefaultSimulationFilterShader;
	mDesc->flags |= PxSceneFlag::eREQUIRE_RW_LOCK;
	if (!mDesc->cpuDispatcher)
	{
		physx::PxDefaultCpuDispatcher* dispatcher = physx::PxDefaultCpuDispatcherCreate(1);
		if (!dispatcher)
		{
			VFX_LTRACE(ELevelTraceType::ELTT_Error, vT("PxDefaultCpuDispatcherCreate(Ver = %d) failed!\r\n"), PX_PHYSICS_VERSION);
		}
		mDesc->cpuDispatcher = dispatcher;
	}

	if (!mDesc->cpuDispatcher)
	{
		PxDefaultCpuDispatcher* mCpuDispatcher = PxDefaultCpuDispatcherCreate(1);
		if (!mCpuDispatcher)
			VFX_LTRACE(ELevelTraceType::ELTT_Error, vT("PxDefaultCpuDispatcherCreate(Ver = %d) failed!\r\n"), PX_PHYSICS_VERSION);
		mDesc->cpuDispatcher = mCpuDispatcher;
	}
#if defined PLATFORM_WIN
	/*if (!mDesc.gpuDispatcher && cudaManager)
	{
		sceneDesc.gpuDispatcher = cudaManager->getGpuDispatcher();
	}*/
#endif
}

void PhySceneDesc::SetSimulationEventCallback(void* handle, FonContact onContact,
	FonTrigger onTrigger,
	FonConstraintBreak onConstraintBreak,
	FonWake onWake,
	FonSleep onSleep,
	FonAdvance onAdvance)
{
	SimulationEventCallback.Handle = handle;
	SimulationEventCallback._onContact = onContact;
	SimulationEventCallback._onTrigger = onTrigger;
	SimulationEventCallback._onConstraintBreak = onConstraintBreak;
	SimulationEventCallback._onWake = onWake;
	SimulationEventCallback._onSleep = onSleep;
	SimulationEventCallback._onAdvance = onAdvance;
	
	//mDesc->filterShader = &ContactReportCallback::CorePxSimulationFilterShader;
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
	CompletionTask = nullptr;
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

void* PhyScene::UpdateActorTransforms(UINT* activeActorCount)
{
	return mScene->getActiveActors(*activeActorCount);
	/*for (PxU32 i = 0; i < *activeActorCount; ++i)
	{
		if (activeActors[i]->userData != NULL)
		{
			PhyEntity* entity = static_cast<PhyEntity*>(activeActors[i]->userData);
			if (entity->EntityType == PhyEntityType::Phy_Actor)
			{
				PhyActor* actor = (PhyActor*)entity;
				actor->UpdateTransform();
			}
		}
	}*/
}

PhyActor* PhyScene::GetActor(void* updatedActors, UINT index)
{
	physx::PxActor** activeActors= (physx::PxActor**)updatedActors;
	if (activeActors[index]->userData != nullptr)
	{
		PhyEntity* entity = static_cast<PhyEntity*>(activeActors[index]->userData);
		if (entity->EntityType == PhyEntityType::Phy_Actor)
		{
			PhyActor* actor = (PhyActor*)entity;
			return actor;
		}
	}
	return nullptr;
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
					hitResult->ExtData = pPhyActor->mCSharpHandle;
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
	PxTransform transform(position->X, position->Y, position->Z);

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
					hitResult->ExtData = pPhyActor->mCSharpHandle;
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
	PxQuat quat(rotation->X, rotation->Y, rotation->Z, rotation->W);
	PxTransform transform(position->X, position->Y, position->Z, quat);
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
					hitResult->ExtData = pPhyActor->mCSharpHandle;
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
	mFilterData = *(PhyFilterData*)data;
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
		shapeList[i]->setQueryFilterData(*(physx::PxFilterData*)&desc->mFilterData);
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
		shapeList[i]->setQueryFilterData(*(physx::PxFilterData*)&desc->mFilterData);
	}
	PhyController* ret = new PhyController(this, ctr);
	ret->BindPhysX();
	Safe_DeleteArray(shapeList);

	return ret;
}

PhyController* PhyScene::GetController(UINT index)
{
	auto crl = ControllerManager->getController(index);
	return (PhyController*)crl->getUserData();
}

PhyObstacleContext* PhyScene::CreateObstacleContext()
{
	PhyObstacleContext* ret = new PhyObstacleContext();
	ret->mContext = ControllerManager->createObstacleContext();	
	return ret;
}

NS_END


