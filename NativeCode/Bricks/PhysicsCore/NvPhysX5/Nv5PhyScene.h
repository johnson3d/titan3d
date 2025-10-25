#pragma once
#include "Nv5Common.h"
#include "../PhyScene.h"
#include "../../../Math/v3dxRayCast.h"
using namespace physx;

NS_BEGIN

class Nv5PhyContext;
class Nv5PhyController;
class Nv5PhyMaterial;
class Nv5PhyBoxControllerDesc;
class Nv5PhyCapsuleControllerDesc;
class Nv5PhyShape;
class Nv5PhyActor;
class Nv5PhyObstacleContext;

//TODO: Need to impl
typedef void(*FonConstraintBreak)(void* selft, physx::PxConstraintInfo*, UINT);
typedef void(*FonWake)(void* selft, physx::PxActor**, UINT);
typedef void(*FonSleep)(void* selft, physx::PxActor**, UINT);
typedef void(*FonAdvance)(void* selft, const physx::PxRigidBody* const*, const physx::PxTransform*, const UINT);

//typedef physx::PxSimulationFilterShader FPxSimulationFilterShader;
//typedef physx::PxFilterFlags(WINAPI*FPxSimulationFilterShader)(//void* self, 
typedef USHORT(*FSimulationFilterShader)(//void* self, 
	UINT attributes0, FPhyFilterData* filterData0,
	UINT attributes1, FPhyFilterData* filterData1,
	physx::PxPairFlags* pairFlags, const void* constantBlock, UINT constantBlockSize);


class PhySimulationEventCallback : public physx::PxSimulationEventCallback
{
public:
	void* Handle = nullptr;
	FonContact _onContact = nullptr;
	FonTrigger _onTrigger;
	FonConstraintBreak _onConstraintBreak;
	FonWake _onWake;
	FonSleep _onSleep;
	FonAdvance _onAdvance;
	
	PhySimulationEventCallback()
	{
		_onContact = nullptr;
		_onTrigger = nullptr;
		_onConstraintBreak = nullptr;
		_onWake = nullptr;
		_onSleep = nullptr;
		_onAdvance = nullptr;
	}

	virtual void onContact(const physx::PxContactPairHeader& pairHeader, const physx::PxContactPair* pairs, physx::PxU32 nbPairs) override
	{
		if (_onContact != nullptr)
		{
			auto saved_actor0 = pairHeader.actors[0];
			auto saved_actor1 = pairHeader.actors[1];
			physx::PxContactPairHeader* pUsed = (physx::PxContactPairHeader*)&pairHeader;
			pUsed->actors[0] = (physx::PxRigidActor*)pairHeader.actors[0]->userData;
			pUsed->actors[1] = (physx::PxRigidActor*)pairHeader.actors[1]->userData;
			_onContact(Handle, (FPhyContactPairHeader*)&pairHeader, (FPhyContactPair*)pairs, nbPairs);
			pUsed->actors[0] = saved_actor0;
			pUsed->actors[1] = saved_actor1;
		}
	}
	virtual void onTrigger(physx::PxTriggerPair* pairs, physx::PxU32 count) override
	{
		if (_onTrigger != nullptr)
		{
			FPhyTriggerPair* phyPairs = (FPhyTriggerPair*)alloca(sizeof(FPhyTriggerPair) * count);
			if (phyPairs != nullptr)
			{
				//PhyTriggerPair* phyPairs = new PhyTriggerPair[count];
				for (UINT i = 0; i < count; ++i)
				{
					phyPairs[i].otherActor = pairs[i].otherActor->userData;
					phyPairs[i].otherShape = pairs[i].otherShape->userData;
					phyPairs[i].triggerActor = pairs[i].triggerActor->userData;
					phyPairs[i].triggerShape = pairs[i].triggerShape->userData;
					phyPairs[i].status = (EPhyPairFlag)pairs[i].status;
					phyPairs[i].flags = (ETriggerPairFlag)((uint32_t)pairs[i].flags);
				}
				_onTrigger(Handle, phyPairs, count);
				//delete[] phyPairs;
			}
		}
	}
	virtual void onConstraintBreak(physx::PxConstraintInfo* constrait, physx::PxU32 count) override
	{
		if (_onConstraintBreak != nullptr)
			_onConstraintBreak(Handle, constrait, count);
	}
	virtual void onWake(physx::PxActor** actor, physx::PxU32 count) override
	{
		if (_onWake != nullptr)
			_onWake(Handle, actor, count);
	}
	virtual void onSleep(physx::PxActor** actor, physx::PxU32 count) override
	{
		if (_onSleep != nullptr)
			_onSleep(Handle, actor, count);
	}
	virtual void onAdvance(const physx::PxRigidBody* const* body, const physx::PxTransform* trans, const physx::PxU32 count) override
	{
		if (_onAdvance != nullptr)
			_onAdvance(Handle, body, trans, count);
	}
};

struct PhySimulationFilterShader
{
	static physx::PxFilterFlags DefaultSimulationFilterShader(PxFilterObjectAttributes attributes0, PxFilterData filterData0,
		PxFilterObjectAttributes attributes1, PxFilterData filterData1,
		PxPairFlags& pairFlags, const void* constantBlock, PxU32 constantBlockSize)
	{
		if (_CustomSimulationFilterShader5 != nullptr)
		{
			if ((&pairFlags) == nullptr)
			{
				VFX_LTRACE(ELTT_Physics, "CorePxSimulationFilterShader pairFlags == null\r\n");
			}
			return (physx::PxFilterFlags)_CustomSimulationFilterShader5(attributes0, (FPhyFilterData*)&filterData0, attributes1, (FPhyFilterData*)&filterData1, &pairFlags, constantBlock, constantBlockSize);
		}
		pairFlags = physx::PxPairFlag::eCONTACT_DEFAULT| physx::PxPairFlag::eTRIGGER_DEFAULT;
		return physx::PxFilterFlags();
	}

	static FSimulationFilterShader _CustomSimulationFilterShader5;
};

class Nv5PhySceneDesc : public PhySceneDesc
{
	friend Nv5PhyContext;
public:
	ENGINE_RTTI(Nv5PhySceneDesc);
	Nv5PhySceneDesc();
	~Nv5PhySceneDesc();
	virtual void Init() override;
	physx::PxSceneDesc* GetDesc() {
		return mDesc;
	}
	virtual void SetFlags(EPhySceneFlag flags) override{
		mDesc->flags = (physx::PxSceneFlag::Enum)flags;
	}
	virtual EPhySceneFlag GetFlags() override{
		return (EPhySceneFlag)((physx::PxU32)mDesc->flags);
	}
	virtual void SetContactDataBlocks(physx::PxU32 nb) override{
		mDesc->nbContactDataBlocks = nb;
	}
	virtual physx::PxU32 GetContactDataBlocks() override{
		return mDesc->nbContactDataBlocks;
	}
	virtual void SetGravity(const v3dxVector3* gravity) override{
		mDesc->gravity = *(physx::PxVec3*)gravity;
	}
	virtual void GetGravity(v3dxVector3* gravity) override{
		*gravity = *(v3dxVector3*)(&mDesc->gravity);
	}
	void SetSimulationEventCallback(void* handle,
		FonContact onContact,
		FonTrigger onTrigger,
		FonConstraintBreak onConstraintBreak,
		FonWake onWake,
		FonSleep onSleep,
		FonAdvance onAdvance);

	void SetHandle(void* handle) {
		mSimulationEventCallback.Handle = handle;
	}
	virtual void SetOnTrigger(FonTrigger onTrigger) override{
		mSimulationEventCallback._onTrigger = onTrigger;
	}
	virtual void SetOnContact(FonContact onContact) override{
		mSimulationEventCallback._onContact = onContact;
	}
	
protected:
	PxSceneDesc*	mDesc;
	PhySimulationEventCallback mSimulationEventCallback;
	PhySimulationFilterShader SimulationFilterShader;
};


class Nv5PhyScene : public PhyScene
{
public:
	ENGINE_RTTI(Nv5PhyScene);

	Nv5PhyScene();
	~Nv5PhyScene();
	virtual void Cleanup() override;
	virtual void BindPhysX() override;

	virtual void LockRead() override {
		mScene->lockRead();
	}
	virtual void UnlockRead() override {
		mScene->unlockRead();
	}
	virtual void LockWrite() override {
		mScene->lockWrite();
	}
	virtual void UnlockWrite() override {
		mScene->unlockWrite();
	}
	virtual void* UpdateActorTransforms(UINT* activeActorCount) override;
	virtual PhyActor* GetActor(void* updatedActors, UINT index) override;

	virtual void Simulate(physx::PxReal elapsedTime,
		void* scratchMemBlock = 0, physx::PxU32 scratchMemBlockSize = 0, bool controlSimulation = true) override
	{
		physx::PxSceneWriteLock writeLock(*mScene);
		mScene->simulate(elapsedTime, CompletionTask, scratchMemBlock, scratchMemBlockSize, controlSimulation);
	}
	virtual vBOOL FetchResults(bool block = false, physx::PxU32* errorState = 0) override
	{
		physx::PxSceneWriteLock writeLock(*mScene);
		return mScene->fetchResults(block, errorState) ? 1 : 0;
	}
	virtual vBOOL Raycast(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult) override;
	virtual vBOOL Sweep(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult) override;
	virtual vBOOL Overlap(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, OUT VHitResult* hitResult) override;
	virtual vBOOL RaycastWithFilter(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, FPhyQueryFilterData* queryFilterData,OUT VHitResult* hitResult) override;
	virtual vBOOL SweepWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, FPhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult) override;
	virtual vBOOL OverlapWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, FPhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult) override;
	virtual PhyController* CreateBoxController(const PhyBoxControllerDesc* desc) override;
	virtual PhyController* CreateCapsuleController(const PhyCapsuleControllerDesc* desc) override;
	virtual int GetNbControllers() override{
		return ControllerManager->getNbControllers();
	}
	virtual PhyController* GetController(UINT index) override;
	virtual PhyObstacleContext* CreateObstacleContext() override;
public:
	physx::PxScene*			mScene;
	physx::PxControllerManager* ControllerManager;
	physx::PxObstacleContext*	ObstacleContext;
	physx::PxBaseTask*		CompletionTask;
};

NS_END