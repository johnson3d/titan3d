#pragma once
#include "NvCommon.h"
#include "../PhyScene.h"
#include "../../../Math/v3dxRayCast.h"
using namespace physx;

NS_BEGIN

class NvPhyContext;
class NvPhyController;
class NvPhyMaterial;
class NvPhyBoxControllerDesc;
class NvPhyCapsuleControllerDesc;
class NvPhyShape;
class NvPhyActor;
class NvPhyObstacleContext;

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
	void* Handle;
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
		if (_CustomSimulationFilterShader != nullptr)
		{
			if ((&pairFlags) == nullptr)
			{
				VFX_LTRACE(ELTT_Physics, "CorePxSimulationFilterShader pairFlags == null\r\n");
			}
			return (physx::PxFilterFlags)_CustomSimulationFilterShader(attributes0, (FPhyFilterData*)&filterData0, attributes1, (FPhyFilterData*)&filterData1, &pairFlags, constantBlock, constantBlockSize);
		}
		pairFlags = physx::PxPairFlag::eCONTACT_DEFAULT| physx::PxPairFlag::eTRIGGER_DEFAULT;
		return physx::PxFilterFlags();
	}

	static FSimulationFilterShader _CustomSimulationFilterShader;
};

class NvPhySceneDesc : public PhySceneDesc
{
	friend NvPhyContext;
public:

	ENGINE_RTTI(PhySceneDesc);
	NvPhySceneDesc();
	~NvPhySceneDesc();
	void Init();
	physx::PxSceneDesc* GetDesc() {
		return mDesc;
	}
	void SetFlags(EPhySceneFlag flags) {
		mDesc->flags = (physx::PxSceneFlag::Enum)flags;
	}
	EPhySceneFlag GetFlags() {
		return (EPhySceneFlag)((physx::PxU32)mDesc->flags);
	}
	void SetContactDataBlocks(physx::PxU32 nb) {
		mDesc->nbContactDataBlocks = nb;
	}
	physx::PxU32 GetContactDataBlocks() {
		return mDesc->nbContactDataBlocks;
	}
	void SetGravity(const v3dxVector3* gravity) {
		mDesc->gravity = *(physx::PxVec3*)gravity;
	}
	void GetGravity(v3dxVector3* gravity) {
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
	void SetOnTrigger(FonTrigger onTrigger) {
		mSimulationEventCallback._onTrigger = onTrigger;
	}
	void SetOnContact(FonContact onContact) {
		mSimulationEventCallback._onContact = onContact;
	}
	
protected:
	PxSceneDesc*	mDesc;
	PhySimulationEventCallback mSimulationEventCallback;
	PhySimulationFilterShader SimulationFilterShader;
};


class NvPhyScene : public PhyScene
{
public:
	ENGINE_RTTI(NvPhyScene);

	NvPhyScene();
	~NvPhyScene();
	virtual void Cleanup() override;
	void BindPhysX();

	void LockRead() {
		mScene->lockRead();
	}
	void UnlockRead() {
		mScene->unlockRead();
	}
	void LockWrite() {
		mScene->lockWrite();
	}
	void UnlockWrite() {
		mScene->unlockWrite();
	}
	void* UpdateActorTransforms(UINT* activeActorCount);
	PhyActor* GetActor(void* updatedActors, UINT index);

	void Simulate(physx::PxReal elapsedTime,
		void* scratchMemBlock = 0, physx::PxU32 scratchMemBlockSize = 0, bool controlSimulation = true)
	{
		physx::PxSceneWriteLock writeLock(*mScene);
		mScene->simulate(elapsedTime, CompletionTask, scratchMemBlock, scratchMemBlockSize, controlSimulation);
	}
	vBOOL FetchResults(bool block = false, physx::PxU32* errorState = 0)
	{
		physx::PxSceneWriteLock writeLock(*mScene);
		return mScene->fetchResults(block, errorState) ? 1 : 0;
	}
	vBOOL Raycast(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult);
	vBOOL Sweep(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult);
	vBOOL Overlap(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, OUT VHitResult* hitResult);
	vBOOL RaycastWithFilter(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, FPhyQueryFilterData* queryFilterData,OUT VHitResult* hitResult);
	vBOOL SweepWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, FPhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult);
	vBOOL OverlapWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, FPhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult);
	PhyController* CreateBoxController(const PhyBoxControllerDesc* desc);
	PhyController* CreateCapsuleController(const PhyCapsuleControllerDesc* desc);
	int GetNbControllers() {
		return ControllerManager->getNbControllers();
	}
	PhyController* GetController(UINT index);
	PhyObstacleContext* CreateObstacleContext();
public:
	physx::PxScene*			mScene;
	physx::PxControllerManager* ControllerManager;
	physx::PxObstacleContext*	ObstacleContext;
	physx::PxBaseTask*		CompletionTask;
};

NS_END