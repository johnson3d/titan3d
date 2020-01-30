#pragma once
#include "PhyEntity.h"
#include "../../Math/v3dxRayCast.h"


NS_BEGIN

class PhyContext;
class PhyController;
class PhyMaterial;
class PhyShape;
enum PhyQueryFlag
{
	eSTATIC = (1 << 0),	//!< Traverse static shapes
	eDYNAMIC = (1 << 1),	//!< Traverse dynamic shapes
	ePREFILTER = (1 << 2),	//!< Run the pre-intersection-test filter (see #PxQueryFilterCallback::preFilter())
	ePOSTFILTER = (1 << 3),	//!< Run the post-intersection-test filter (see #PxQueryFilterCallback::postFilter())
	eANY_HIT = (1 << 4),	//!< Abort traversal as soon as any hit is found and return it via callback.block.
									//!< Helps query performance. Both eTOUCH and eBLOCK hitTypes are considered hits with this flag.
	eNO_BLOCK = (1 << 5),	//!< All hits are reported as touching. Overrides eBLOCK returned from user filters with eTOUCH.
																	//!< This is also an optimization hint that may improve query performance.
	eRESERVED = (1 << 15)	//!< Reserved for internal use
};
struct PhyQueryFilterData
{
public:
	PX_INLINE PhyQueryFilterData() : flag((PhyQueryFlag)(PhyQueryFlag::eDYNAMIC | PhyQueryFlag::eSTATIC)){}

	/** \brief constructor to set both filter data and filter flags */
	PX_INLINE PhyQueryFilterData(const physx::PxFilterData& fd, PhyQueryFlag f) : data(fd), flag(f){}

	/** \brief constructor to set filter flags only */
	PX_INLINE PhyQueryFilterData(PhyQueryFlag f) : flag(f){}
	physx::PxFilterData	data;		//!< Filter data associated with the scene query
	PhyQueryFlag	flag;		//!< Filter flags (see #PxQueryFlags)
};
struct PhyTriggerPair
{
	PX_INLINE PhyTriggerPair() {}

	void*				triggerShape;	//!< The shape that has been marked as a trigger.
	void*			triggerActor;	//!< The actor to which triggerShape is attached
	void*				otherShape;		//!< The shape causing the trigger event. \deprecated (see #PxSimulationEventCallback::onTrigger()) If collision between trigger shapes is enabled, then this member might point to a trigger shape as well.
	void*			otherActor;		//!< The actor to which otherShape is attached
	physx::PxPairFlag::Enum		status;			//!< Type of trigger event (eNOTIFY_TOUCH_FOUND or eNOTIFY_TOUCH_LOST). eNOTIFY_TOUCH_PERSISTS events are not supported.
	physx::PxTriggerPairFlags		flags;			//!< Additional information on the pair (see #PxTriggerPairFlag)
};

struct PhyContactPair
{
public:
	PhyContactPair() {}
	void*				shapes[2];
	const physx::PxU8* contactPatches;
	const physx::PxU8* contactPoints;
	const physx::PxReal*			contactImpulses;
	physx::PxU32					requiredBufferSize;
	physx::PxU8					contactCount;
	physx::PxU8					patchCount;
	physx::PxU16					contactStreamSize;
	physx::PxContactPairFlags		flags;
	physx::PxPairFlags				events;
	physx::PxU32					internalData[2];	// For internal use only
	//PX_INLINE PxU32			extractContacts(PxContactPairPoint* userBuffer, PxU32 bufferSize) const;
	//PX_INLINE void				bufferContacts(PxContactPair* newPair, PxU8* bufferMemory) const;

	const physx::PxU32*		getInternalFaceIndices() const { return reinterpret_cast<const physx::PxU32*>(contactImpulses + contactCount); };
};
struct PhyContactPairHeader
{
public:
	PhyContactPairHeader() {}

	void*				actors[2];
	const BYTE*					extraDataStream;
	UINT16						extraDataStreamSize;
	physx::PxContactPairHeaderFlags	flags;
	const struct physx::PxContactPair*	pairs;
	UINT						nbPairs;
};

class PhySceneDesc : public VIUnknown
{
	friend PhyContext;
public:
	struct ContactReportCallback : public physx::PxSimulationEventCallback
	{
		typedef void(WINAPI *FonContact)(void* selft, const physx::PxContactPairHeader* pairHeader, const physx::PxContactPair* pairs, physx::PxU32 nbPairs);
		typedef void(WINAPI *FonTrigger)(void* selft, PhyTriggerPair* pairs, physx::PxU32 count);
		typedef void(WINAPI *FonConstraintBreak)(void* selft, physx::PxConstraintInfo*, physx::PxU32);
		typedef void(WINAPI *FonWake)(void* selft, physx::PxActor**, physx::PxU32);
		typedef void(WINAPI *FonSleep)(void* selft, physx::PxActor**, physx::PxU32);
		typedef void(WINAPI *FonAdvance)(void* selft, const physx::PxRigidBody *const *, const physx::PxTransform *, const physx::PxU32);

		//typedef physx::PxSimulationFilterShader FPxSimulationFilterShader;
		//typedef physx::PxFilterFlags(WINAPI*FPxSimulationFilterShader)(//void* self, 
		typedef USHORT(WINAPI*FPxSimulationFilterShader)(//void* self, 这里不能是physx::PxFilterFlags，因为c#callback不能写出返回构造处理
			physx::PxFilterObjectAttributes attributes0, physx::PxFilterData* filterData0,
			physx::PxFilterObjectAttributes attributes1, physx::PxFilterData* filterData1,
			physx::PxPairFlags* pairFlags, const void* constantBlock, physx::PxU32 constantBlockSize);

		void* Handle;
		FonContact _onContact;
		FonTrigger _onTrigger;
		FonConstraintBreak _onConstraintBreak;
		FonWake _onWake;
		FonSleep _onSleep;
		FonAdvance _onAdvance;
		static FPxSimulationFilterShader _PxSimulationFilterShader;

		static physx::PxFilterFlags CorePxSimulationFilterShader(physx::PxFilterObjectAttributes attributes0, physx::PxFilterData filterData0,
			physx::PxFilterObjectAttributes attributes1, physx::PxFilterData filterData1,
			physx::PxPairFlags& pairFlags, const void* constantBlock, physx::PxU32 constantBlockSize)
		{
			if (_PxSimulationFilterShader != nullptr)
			{
				if ((&pairFlags) == nullptr)
				{
					VFX_LTRACE(ELTT_Physics, "CorePxSimulationFilterShader pairFlags == null\r\n");
				}
				return (physx::PxFilterFlags)_PxSimulationFilterShader(attributes0, &filterData0, attributes1, &filterData1, &pairFlags, constantBlock, constantBlockSize);
			}
			return physx::PxFilterFlag::eDEFAULT;
		}

		ContactReportCallback()
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
				_onContact(Handle, &pairHeader, pairs, nbPairs);
				pUsed->actors[0] = saved_actor0;
				pUsed->actors[1] = saved_actor1;
			}
		}
		virtual void onTrigger(physx::PxTriggerPair* pairs, physx::PxU32 count) override
		{
			if (_onTrigger != nullptr)
			{
				PhyTriggerPair* phyPairs = new PhyTriggerPair[count];
				for (UINT i = 0; i < count; ++i)
				{
					phyPairs[i].otherActor = pairs[i].otherActor->userData;
					phyPairs[i].otherShape = pairs[i].otherShape->userData;
					phyPairs[i].triggerActor = pairs[i].triggerActor->userData;
					phyPairs[i].triggerShape = pairs[i].triggerShape->userData;
					phyPairs[i].status = pairs[i].status;
					phyPairs[i].flags = pairs[i].flags;
				}
				_onTrigger(Handle, phyPairs, count);
				delete[] phyPairs;
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
		virtual void onAdvance(const physx::PxRigidBody *const * body, const physx::PxTransform * trans, const physx::PxU32 count) override
		{
			if (_onAdvance != nullptr)
				_onAdvance(Handle, body, trans, count);
		}
	};
	RTTI_DEF(PhySceneDesc, 0x821fb6ef5bf21426, false);
	PhySceneDesc();
	~PhySceneDesc();
	void Init();
	physx::PxSceneDesc* GetDesc() {
		return mDesc;
	}
	void SetFlags(physx::PxU32 flags) {
		mDesc->flags = (physx::PxSceneFlag::Enum)flags;
	}
	physx::PxU32 GetFlags() {
		return mDesc->flags;
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
	void SetContactCallBack(void* handle, ContactReportCallback::FonContact onContact,
		ContactReportCallback::FonTrigger onTrigger,
		ContactReportCallback::FonConstraintBreak onConstraintBreak,
		ContactReportCallback::FonWake onWake,
		ContactReportCallback::FonSleep onSleep,
		ContactReportCallback::FonAdvance onAdvance,
		ContactReportCallback::FPxSimulationFilterShader pxSimulationFilterShader);
protected:
	physx::PxSceneDesc*	mDesc;
	ContactReportCallback ContactCB;
};

class PhyControllerDesc : public VIUnknown
{
public:
	physx::PxControllerDesc*		mDesc;
	physx::PxFilterData* mFilterData;
	void SetMaterial(PhyMaterial* mtl);
	void SetQueryFilterData(physx::PxFilterData* data);
	void SetHitReportCallback()
	{

	}
	void SetBehaviorCallback()
	{

	}
};

class PhyBoxControllerDesc : public PhyControllerDesc
{
public:
	RTTI_DEF(PhyBoxControllerDesc, 0x990b86bc5c346130, true);
	PhyBoxControllerDesc()
	{
		mDesc = &mBoxDesc;
	}
	physx::PxBoxControllerDesc		mBoxDesc;
	v3dxVector3 GetExtent() {
		v3dxVector3 v;
		v.x = mBoxDesc.halfSideExtent;
		v.y = mBoxDesc.halfHeight;
		v.z = mBoxDesc.halfForwardExtent;
		return v;
	}
	void SetExtent(const v3dxVector3* v) {
		mBoxDesc.halfSideExtent = v->x;
		mBoxDesc.halfHeight = v->y;
		mBoxDesc.halfForwardExtent = v->z;
	}
};

class PhyCapsuleControllerDesc : public PhyControllerDesc
{
public:
	RTTI_DEF(PhyCapsuleControllerDesc, 0xc907e31d5c346136, true);
	PhyCapsuleControllerDesc()
	{
		mDesc = &mCapsuleDesc;
	}
	physx::PxCapsuleControllerDesc	mCapsuleDesc;

	float GetCapsuleRadius() {
		return mCapsuleDesc.radius;
	}
	void SetCapsuleRadius(float v) {
		mCapsuleDesc.radius = v;
	}
	float GetCapsuleHeight() {
		return mCapsuleDesc.height;
	}
	void SetCapsuleHeight(float v) {
		mCapsuleDesc.height = v;
	}
	physx::PxCapsuleClimbingMode::Enum GetCapsuleClimbingMode() {
		return mCapsuleDesc.climbingMode;
	}
	void SetCapsuleClimbingMode(physx::PxCapsuleClimbingMode::Enum v) {
		mCapsuleDesc.climbingMode = v;
	}
};

class PhyScene : public PhyEntity
{
public:
	RTTI_DEF(PhyScene, 0x45c92b0b5befac5d, true);

	PhyScene();
	~PhyScene();
	virtual void Cleanup() override;
	void BindPhysX();

	void UpdateActorTransforms();

	void Simulate(physx::PxReal elapsedTime, physx::PxBaseTask* completionTask = NULL,
		void* scratchMemBlock = 0, physx::PxU32 scratchMemBlockSize = 0, vBOOL controlSimulation = true)
	{
		physx::PxSceneWriteLock writeLock(*mScene);
		mScene->simulate(elapsedTime, completionTask, scratchMemBlock, scratchMemBlockSize, controlSimulation ? true : false);
	}
	vBOOL FetchResults(vBOOL block = false, physx::PxU32* errorState = 0)
	{
		physx::PxSceneWriteLock writeLock(*mScene);
		return mScene->fetchResults(block ? true : false, errorState) ? 1 : 0;
	}
	vBOOL Raycast(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult);
	vBOOL Sweep(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult);
	vBOOL Overlap(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, OUT VHitResult* hitResult);
	vBOOL RaycastWithFilter(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, PhyQueryFilterData* queryFilterData,OUT VHitResult* hitResult);
	vBOOL SweepWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, PhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult);
	vBOOL OverlapWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, PhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult);
	PhyController* CreateBoxController(const PhyBoxControllerDesc* desc);
	PhyController* CreateCapsuleController(const PhyCapsuleControllerDesc* desc);
public:
	physx::PxScene*			mScene;
	physx::PxControllerManager* ControllerManager;
	physx::PxObstacleContext*	ObstacleContext;
};

NS_END