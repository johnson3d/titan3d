#pragma once
#include "NvCommon.h"
#include "../PhyShape.h"

NS_BEGIN

class NvPhyScene;
class NvPhyActor;
struct FPhyFilterData;
class NvPhyMaterial;

class NvPhyShape : public PhyShape
{
public:
	ENGINE_RTTI(NvPhyShape)

public:
	TWeakRefHandle<PhyActor>			mActor;
	EPhysShapeType					mType;
	physx::PxShape*					mShape;

	int								mTrianglesRemapNumber;
	uint32_t*						mTrianglesRemap;

	//physx::PxTriangleMesh*			mTriangleMesh;
public:
	NvPhyShape();
	~NvPhyShape();
	virtual void Cleanup() override;
	void BindPhysX();
	vBOOL AddToActor(PhyActor* actor, const physx::PxTransform* relativePose);
	bool AddToActor(PhyActor * actor, const v3dxVector3 * p, const v3dxQuaternion * q)
	{
		physx::PxTransform tm;
		tm.p.x = p->X;
		tm.p.y = p->Y;
		tm.p.z = p->Z;

		tm.q.x = q->X;
		tm.q.y = q->Y;
		tm.q.z = q->Z;
		tm.q.w = q->W;

		return AddToActor(actor, &tm) ? true : false;
	}
	void RemoveFromActor();
	void SetLocalPose(const physx::PxTransform* relativePose);
	void SetLocalPose(const v3dxVector3* p, const v3dxQuaternion* q)
	{
		physx::PxTransform tm;
		tm.p.x = p->X;
		tm.p.y = p->Y;
		tm.p.z = p->Z;

		tm.q.x = q->X;
		tm.q.y = q->Y;
		tm.q.z = q->Z;
		tm.q.w = q->W;
		SetLocalPose(&tm);
	}
	void GetLocalPose(physx::PxTransform* relativePose);
	void GetLocalPose(v3dxVector3* p, v3dxQuaternion* q)
	{
		physx::PxTransform tm;
		GetLocalPose(&tm);
		p->X = tm.p.x;
		p->Y = tm.p.y;
		p->Z = tm.p.z;

		q->X = tm.q.x;
		q->Y = tm.q.y;
		q->Z = tm.q.z;
		q->W = tm.q.w;
	}
	void SetQueryFilterData(const FPhyFilterData* filterData);
	void SetSimulationFilterData(const FPhyFilterData* filterData);
	void SetFlag(EPhysShapeFlag flag, bool value);
	bool HaveFlag(EPhysShapeFlag flag);

	void GetMaterials(PhyMaterial** materials, int count);
	void SetMaterials(PhyMaterial** materials, int count);

	bool IfGetBox(v3dxVector3* halfExtent);
	bool IfSetBox(const v3dxVector3* halfExtent);
	bool IfGetSphere(float* radius);
	bool IfSetSphere(float radius);
	bool IfGetCapsule(float* radius, float* halfHeight);
	bool IfSetCapsule(float radius, float halfHeight);
	bool IfGetTriMeshScaling(v3dxVector3* scale, v3dxQuaternion* scaleRot);
	bool IfSetTriMeshScaling(const v3dxVector3* scale, const v3dxQuaternion* scaleRot);

	TR_MEMBER(SV_NoBind = true)
	NxRHI::FMeshPrimitives* IfGetTriMesh(NxRHI::IGpuDevice* rc);
	TR_MEMBER(SV_NoBind = true)
	NxRHI::FMeshPrimitives* IfGetConvexMesh(NxRHI::IGpuDevice* rc);

	int GetTrianglesRemap(int index);
};

NS_END