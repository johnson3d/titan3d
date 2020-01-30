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

extern "C"
{
	CSharpAPI1(EngineNS, PhySceneDesc, SetGravity, const v3dxVector3*);
	CSharpAPI1(EngineNS, PhySceneDesc, GetGravity, v3dxVector3*);
	CSharpAPI1(EngineNS, PhySceneDesc, SetFlags, physx::PxU32);
	CSharpReturnAPI0(physx::PxU32, EngineNS, PhySceneDesc, GetFlags);
	CSharpAPI1(EngineNS, PhySceneDesc, SetContactDataBlocks, physx::PxU32);
	CSharpReturnAPI0(physx::PxU32, EngineNS, PhySceneDesc, GetContactDataBlocks);
	CSharpAPI8(EngineNS, PhySceneDesc, SetContactCallBack, void*,
		PhySceneDesc::ContactReportCallback::FonContact,
		PhySceneDesc::ContactReportCallback::FonTrigger,
		PhySceneDesc::ContactReportCallback::FonConstraintBreak,
		PhySceneDesc::ContactReportCallback::FonWake,
		PhySceneDesc::ContactReportCallback::FonSleep,
		PhySceneDesc::ContactReportCallback::FonAdvance,
		PhySceneDesc::ContactReportCallback::FPxSimulationFilterShader);

	CSharpAPI5(EngineNS, PhyScene, Simulate, PxReal, physx::PxBaseTask*, void*, PxU32, bool);
	CSharpAPI2(EngineNS, PhyScene, FetchResults, vBOOL, PxU32*);
	CSharpAPI0(EngineNS, PhyScene, UpdateActorTransforms);
	CSharpReturnAPI4(vBOOL, EngineNS, PhyScene, Raycast, const v3dxVector3*, const v3dxVector3*, float, VHitResult*);
	CSharpReturnAPI5(vBOOL, EngineNS, PhyScene, Sweep, const PhyShape*, const v3dxVector3*, const v3dxVector3*, float, VHitResult*);
	CSharpReturnAPI4(vBOOL, EngineNS, PhyScene, Overlap, const PhyShape*, const v3dxVector3*, const v3dxQuaternion*, VHitResult*);
	CSharpReturnAPI5(vBOOL, EngineNS, PhyScene, RaycastWithFilter, const v3dxVector3*, const v3dxVector3*, float, PhyQueryFilterData*, VHitResult*);
	CSharpReturnAPI6(vBOOL, EngineNS, PhyScene, SweepWithFilter, const PhyShape*, const v3dxVector3*, const v3dxVector3*,  float, PhyQueryFilterData*, VHitResult*);
	CSharpReturnAPI5(vBOOL, EngineNS, PhyScene, OverlapWithFilter, const PhyShape*, const v3dxVector3*, const v3dxQuaternion*, PhyQueryFilterData*, VHitResult*);

	CSharpReturnAPI1(PhyController*, EngineNS, PhyScene, CreateBoxController, const PhyBoxControllerDesc*);
	CSharpReturnAPI1(PhyController*, EngineNS, PhyScene, CreateCapsuleController, const PhyCapsuleControllerDesc*);

	CSharpAPI1(EngineNS, PhyControllerDesc, SetMaterial, PhyMaterial*);
	CSharpAPI1(EngineNS, PhyControllerDesc, SetQueryFilterData, physx::PxFilterData*);

	CSharpReturnAPI0(v3dVector3_t, EngineNS, PhyBoxControllerDesc, GetExtent);
	CSharpAPI1(EngineNS, PhyBoxControllerDesc, SetExtent, const v3dxVector3*);

	CSharpReturnAPI0(float, EngineNS, PhyCapsuleControllerDesc, GetCapsuleRadius);
	CSharpAPI1(EngineNS, PhyCapsuleControllerDesc, SetCapsuleRadius, float);
	CSharpReturnAPI0(float, EngineNS, PhyCapsuleControllerDesc, GetCapsuleHeight);
	CSharpAPI1(EngineNS, PhyCapsuleControllerDesc, SetCapsuleHeight, float);
	CSharpReturnAPI0(physx::PxCapsuleClimbingMode::Enum, EngineNS, PhyCapsuleControllerDesc, GetCapsuleClimbingMode);
	CSharpAPI1(EngineNS, PhyCapsuleControllerDesc, SetCapsuleClimbingMode, physx::PxCapsuleClimbingMode::Enum);
}