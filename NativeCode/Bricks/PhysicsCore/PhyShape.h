#pragma once
#include "PhyEntity.h"

NS_BEGIN

class PhyScene;
class PhyActor;
struct PhyFilterData;
class PhyMaterial;

enum TR_ENUM(SV_EnumNoFlags = true)
	EPhysShapeType
{
	PST_Plane,
	PST_Sphere,
	PST_Box,
	PST_Convex,
	PST_TriangleMesh,
	PST_HeightField,
	PST_Capsule,
	PST_Unknown,
};

enum TR_ENUM()
	EPhysShapeFlag
{
	eSIMULATION_SHAPE = (1 << 0),
	eSCENE_QUERY_SHAPE = (1 << 1),
	eTRIGGER_SHAPE = (1 << 2),
	eVISUALIZATION = (1 << 3),
	ePARTICLE_DRAIN = (1 << 4)
};

class TR_CLASS()
	PhyShape : public PhyEntity
{
public:
	ENGINE_RTTI(PhyShape)

public:
	TObjectHandle<PhyActor>			mActor;
	EPhysShapeType					mType;
	physx::PxShape*					mShape;

	int								mTrianglesRemapNumber;
	uint32_t*						mTrianglesRemap;

	//physx::PxTriangleMesh*			mTriangleMesh;
public:
	PhyShape();
	~PhyShape();
	virtual void Cleanup() override;
	void BindPhysX();
	vBOOL AddToActor(PhyActor* actor, const physx::PxTransform* relativePose);
	bool AddToActor(PhyActor * actor, const v3dxVector3 * p, const v3dxQuaternion * q)
	{
		physx::PxTransform tm;
		tm.p.x = p->x;
		tm.p.y = p->y;
		tm.p.z = p->z;

		tm.q.x = q->x;
		tm.q.y = q->y;
		tm.q.z = q->z;
		tm.q.w = q->w;

		return AddToActor(actor, &tm) ? true : false;
	}
	void RemoveFromActor();
	void SetLocalPose(const physx::PxTransform* relativePose);
	void SetLocalPose(const v3dxVector3* p, const v3dxQuaternion* q)
	{
		physx::PxTransform tm;
		tm.p.x = p->x;
		tm.p.y = p->y;
		tm.p.z = p->z;

		tm.q.x = q->x;
		tm.q.y = q->y;
		tm.q.z = q->z;
		tm.q.w = q->w;
		SetLocalPose(&tm);
	}
	void GetLocalPose(physx::PxTransform* relativePose);
	void GetLocalPose(v3dxVector3* p, v3dxQuaternion* q)
	{
		physx::PxTransform tm;
		GetLocalPose(&tm);
		p->x = tm.p.x;
		p->y = tm.p.y;
		p->z = tm.p.z;

		q->x = tm.q.x;
		q->y = tm.q.y;
		q->z = tm.q.z;
		q->w = tm.q.w;
	}
	void SetQueryFilterData(const PhyFilterData* filterData);
	void SetSimulationFilterData(const PhyFilterData* filterData);
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