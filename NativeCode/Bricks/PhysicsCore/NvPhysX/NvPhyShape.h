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
	virtual void BindPhysX() override;
	vBOOL AddToActor(PhyActor* actor, const physx::PxTransform* relativePose);
	virtual bool AddToActor(PhyActor * actor, const v3dxVector3 * p, const v3dxQuaternion * q) override
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
	virtual void RemoveFromActor() override;
	void SetLocalPose(const physx::PxTransform* relativePose);
	virtual void SetLocalPose(const v3dxVector3* p, const v3dxQuaternion* q) override
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
	virtual void GetLocalPose(v3dxVector3* p, v3dxQuaternion* q) override
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
	virtual void SetQueryFilterData(const FPhyFilterData* filterData) override;
	virtual void SetSimulationFilterData(const FPhyFilterData* filterData) override;
	virtual void SetFlag(EPhysShapeFlag flag, bool value) override;
	virtual bool HaveFlag(EPhysShapeFlag flag) override;

	virtual void GetMaterials(PhyMaterial** materials, int count) override;
	virtual void SetMaterials(PhyMaterial** materials, int count) override;

	virtual bool IfGetBox(v3dxVector3* halfExtent) override;
	virtual bool IfSetBox(const v3dxVector3* halfExtent) override;
	virtual bool IfGetSphere(float* radius) override;
	virtual bool IfSetSphere(float radius) override;
	virtual bool IfGetCapsule(float* radius, float* halfHeight) override;
	virtual bool IfSetCapsule(float radius, float halfHeight) override;
	virtual bool IfGetTriMeshScaling(v3dxVector3* scale, v3dxQuaternion* scaleRot) override;
	virtual bool IfSetTriMeshScaling(const v3dxVector3* scale, const v3dxQuaternion* scaleRot) override;

	TR_MEMBER(SV_NoBind = true)
		virtual NxRHI::FMeshPrimitives* IfGetTriMesh(NxRHI::IGpuDevice* rc) override;
	TR_MEMBER(SV_NoBind = true)
		virtual NxRHI::FMeshPrimitives* IfGetConvexMesh(NxRHI::IGpuDevice* rc) override;

	virtual int GetTrianglesRemap(int index) override;
};

NS_END