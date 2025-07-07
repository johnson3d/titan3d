#pragma once
#include "PhyEntity.h"
#include "../../Math/v3dxRayCast.h"

NS_BEGIN

class PhyContext;
class PhyController;
class PhyMaterial;
class PhyBoxControllerDesc;
class PhyCapsuleControllerDesc;
class PhyShape;
class PhyActor;
class PhyObstacleContext;

enum TR_ENUM()
	PhyPairFlag
{
	eSOLVE_CONTACT = (1 << 0),
	eMODIFY_CONTACTS = (1 << 1),
	eNOTIFY_TOUCH_FOUND = (1 << 2),
	eNOTIFY_TOUCH_PERSISTS = (1 << 3),
	eNOTIFY_TOUCH_LOST = (1 << 4),
	eNOTIFY_TOUCH_CCD = (1 << 5),
	eNOTIFY_THRESHOLD_FORCE_FOUND = (1 << 6),
	eNOTIFY_THRESHOLD_FORCE_PERSISTS = (1 << 7),
	eNOTIFY_THRESHOLD_FORCE_LOST = (1 << 8),
	eNOTIFY_CONTACT_POINTS = (1 << 9),
	eDETECT_DISCRETE_CONTACT = (1 << 10),
	eDETECT_CCD_CONTACT = (1 << 11),
	ePRE_SOLVER_VELOCITY = (1 << 12),
	ePOST_SOLVER_VELOCITY = (1 << 13),
	eCONTACT_EVENT_POSE = (1 << 14),
	//eNEXT_FREE = (1 << 15),        //!< For internal use only.

	eCONTACT_DEFAULT = eSOLVE_CONTACT | eDETECT_DISCRETE_CONTACT,

	eTRIGGER_DEFAULT = eNOTIFY_TOUCH_FOUND | eNOTIFY_TOUCH_LOST | eDETECT_DISCRETE_CONTACT
};

struct FFilterData
{
	FFilterData()
	{
		word0 = word1 = word2 = word3 = 0;
	}

	FFilterData(const FFilterData& fd) : word0(fd.word0), word1(fd.word1), word2(fd.word2), word3(fd.word3) {}

	FFilterData(UINT w0, UINT w1, UINT w2, UINT w3) : word0(w0), word1(w1), word2(w2), word3(w3) {}

	void setToDefault()
	{
		*this = FFilterData();
	}
	void operator = (const FFilterData& fd)
	{
		word0 = fd.word0;
		word1 = fd.word1;
		word2 = fd.word2;
		word3 = fd.word3;
	}
	bool operator == (const FFilterData& a) const
	{
		return a.word0 == word0 && a.word1 == word1 && a.word2 == word2 && a.word3 == word3;
	}
	bool operator != (const FFilterData& a) const
	{
		return !(a == *this);
	}

	UINT word0;
	UINT word1;
	UINT word2;
	UINT word3;
};

struct TR_CLASS(SV_LayoutStruct = 8)
	PhyQueryFilterData
{
public:
	PhyQueryFilterData() : flag((PhyQueryFlag)(PhyQueryFlag::eDYNAMIC | PhyQueryFlag::eSTATIC)){}

	/** \brief constructor to set both filter data and filter flags */
	PhyQueryFilterData(const FFilterData& fd, PhyQueryFlag f) : data(fd), flag(f){}

	/** \brief constructor to set filter flags only */
	PhyQueryFilterData(PhyQueryFlag f) : flag(f){}
	FFilterData		data;		//!< Filter data associated with the scene query
	PhyQueryFlag	flag;		//!< Filter flags (see #PxQueryFlags)
};

enum ETriggerPairFlag
{
	eREMOVED_SHAPE_TRIGGER = (1 << 0),					//!< The trigger shape has been removed from the actor/scene.
		eREMOVED_SHAPE_OTHER = (1 << 1),					//!< The shape causing the trigger event has been removed from the actor/scene.
		//eNEXT_FREE = (1 << 2)					//!< For internal use only.
};

struct TR_CLASS(SV_LayoutStruct = 8)
	PhyTriggerPair
{
	PhyTriggerPair() {}

	void*				triggerShape;	//!< The shape that has been marked as a trigger.
	void*			triggerActor;	//!< The actor to which triggerShape is attached
	void*				otherShape;		//!< The shape causing the trigger event. \deprecated (see #PxSimulationEventCallback::onTrigger()) If collision between trigger shapes is enabled, then this member might point to a trigger shape as well.
	void*			otherActor;		//!< The actor to which otherShape is attached
	PhyPairFlag		status;			//!< Type of trigger event (eNOTIFY_TOUCH_FOUND or eNOTIFY_TOUCH_LOST). eNOTIFY_TOUCH_PERSISTS events are not supported.
	ETriggerPairFlag		flags;			//!< Additional information on the pair (see #PxTriggerPairFlag)
};

struct TR_CLASS(SV_LayoutStruct = 8)
	PhyContactPair
{
public:
	PhyContactPair() {}
	void*				shapes[2];
	const UINT8* contactPatches;
	const UINT8* contactPoints;
	const RealType*			contactImpulses;
	UINT					requiredBufferSize;
	UINT8					contactCount;
	UINT8					patchCount;
	UINT16					contactStreamSize;
	UINT					flags;
	UINT					events;
	UINT					internalData[2];	// For internal use only
	//PX_INLINE UINT			extractContacts(PxContactPairPoint* userBuffer, UINT bufferSize) const;
	//PX_INLINE void				bufferContacts(PxContactPair* newPair, PxU8* bufferMemory) const;

	const UINT*		getInternalFaceIndices() const { return reinterpret_cast<const UINT*>(contactImpulses + contactCount); };
};

struct TR_CLASS(SV_LayoutStruct = 8)
	PhyContactPairHeader
{
public:
	PhyContactPairHeader() {}

	void*						actors[2];
	const BYTE*					extraDataStream;
	UINT16						extraDataStreamSize;
	UINT						flags;
	PhyContactPair*				pairs;
	UINT						nbPairs;
};

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(* FonTrigger)(void* self, PhyTriggerPair* pairs, UINT count);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(* FonContact)(void* selft, const PhyContactPairHeader* pairHeader, const PhyContactPair* pairs, UINT nbPairs);



enum TR_ENUM()
EPhySceneFlag
{
	eENABLE_ACTIVE_ACTORS = (1 << 0),
	eENABLE_CCD = (1 << 1),
	eDISABLE_CCD_RESWEEP = (1 << 2),
	eADAPTIVE_FORCE = (1 << 3),
	eENABLE_PCM = (1 << 6),
	eDISABLE_CONTACT_REPORT_BUFFER_RESIZE = (1 << 7),
	eDISABLE_CONTACT_CACHE = (1 << 8),
	eREQUIRE_RW_LOCK = (1 << 9),
	eENABLE_STABILIZATION = (1 << 10),
	eENABLE_AVERAGE_POINT = (1 << 11),
	eEXCLUDE_KINEMATICS_FROM_ACTIVE_ACTORS = (1 << 12),
	eENABLE_GPU_DYNAMICS = (1 << 13),
	eENABLE_ENHANCED_DETERMINISM = (1 << 14),
	eENABLE_FRICTION_EVERY_ITERATION = (1 << 15),
	eMUTABLE_FLAGS = eENABLE_ACTIVE_ACTORS | eEXCLUDE_KINEMATICS_FROM_ACTIVE_ACTORS
};

class TR_CLASS() 
	PhySceneDesc : public IWeakRefObject
{
	friend PhyContext;
public:

	ENGINE_RTTI(PhySceneDesc);
	virtual void Init() = 0;
	virtual void SetFlags(EPhySceneFlag flags) = 0;
	virtual EPhySceneFlag GetFlags() = 0;
	virtual void SetContactDataBlocks(UINT nb) = 0;
	virtual UINT GetContactDataBlocks() = 0;
	virtual void SetGravity(const v3dxVector3* gravity) = 0;
	virtual void GetGravity(v3dxVector3* gravity) = 0;

	virtual void SetOnTrigger(FonTrigger onTrigger) = 0;
	virtual void SetOnContact(FonContact onContact) = 0;
};


class TR_CLASS()
	PhyScene : public PhyEntity
{
public:
	ENGINE_RTTI(PhyScene);

	virtual void BindPhysX() = 0;

	virtual void LockRead() = 0;
	virtual void UnlockRead() = 0;
	virtual void LockWrite() = 0;
	virtual void UnlockWrite() = 0;
	virtual void* UpdateActorTransforms(UINT* activeActorCount) = 0;
	virtual PhyActor* GetActor(void* updatedActors, UINT index) = 0;

	virtual void Simulate(RealType elapsedTime,
		void* scratchMemBlock = 0, UINT scratchMemBlockSize = 0, bool controlSimulation = true) = 0;
	virtual vBOOL FetchResults(bool block = false, UINT* errorState = 0) = 0;
	virtual vBOOL Raycast(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult) = 0;
	virtual vBOOL Sweep(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, OUT VHitResult* hitResult) = 0;
	virtual vBOOL Overlap(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, OUT VHitResult* hitResult) = 0;
	virtual vBOOL RaycastWithFilter(const v3dxVector3* origin, const v3dxVector3* unitDir, float maxDistance, PhyQueryFilterData* queryFilterData,OUT VHitResult* hitResult) = 0;
	virtual vBOOL SweepWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxVector3* unitDir, float maxDistance, PhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult) = 0;
	virtual vBOOL OverlapWithFilter(const PhyShape* shape, const v3dxVector3* position, const v3dxQuaternion* rotation, PhyQueryFilterData* queryFilterData, OUT VHitResult* hitResult) = 0;
	virtual PhyController* CreateBoxController(const PhyBoxControllerDesc* desc) = 0;
	virtual PhyController* CreateCapsuleController(const PhyCapsuleControllerDesc* desc) = 0;
	virtual int GetNbControllers() = 0;
	virtual PhyController* GetController(UINT index) = 0;
	virtual PhyObstacleContext* CreateObstacleContext() = 0;
};

NS_END