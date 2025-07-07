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
	TWeakRefHandle<PhyActor>		mActor;
	EPhysShapeType					mType;

	int								mTrianglesRemapNumber;
	unsigned int*					mTrianglesRemap;

public:
	virtual void BindPhysX() = 0;
	virtual bool AddToActor(PhyActor* actor, const v3dxVector3* p, const v3dxQuaternion* q) = 0;
	virtual void RemoveFromActor() = 0;
	virtual void SetLocalPose(const v3dxVector3* p, const v3dxQuaternion* q) = 0;
	virtual void GetLocalPose(v3dxVector3* p, v3dxQuaternion* q) = 0;
	virtual void SetQueryFilterData(const PhyFilterData* filterData) = 0;
	virtual void SetSimulationFilterData(const PhyFilterData* filterData) = 0;
	virtual void SetFlag(EPhysShapeFlag flag, bool value) = 0;
	virtual bool HaveFlag(EPhysShapeFlag flag) = 0;

	virtual void GetMaterials(PhyMaterial** materials, int count) = 0;
	virtual void SetMaterials(PhyMaterial** materials, int count) = 0;

	virtual bool IfGetBox(v3dxVector3* halfExtent) = 0;
	virtual bool IfSetBox(const v3dxVector3* halfExtent) = 0;
	virtual bool IfGetSphere(float* radius) = 0;
	virtual bool IfSetSphere(float radius) = 0;
	virtual bool IfGetCapsule(float* radius, float* halfHeight) = 0;
	virtual bool IfSetCapsule(float radius, float halfHeight) = 0;
	virtual bool IfGetTriMeshScaling(v3dxVector3* scale, v3dxQuaternion* scaleRot) = 0;
	virtual bool IfSetTriMeshScaling(const v3dxVector3* scale, const v3dxQuaternion* scaleRot) = 0;

	TR_MEMBER(SV_NoBind = true)
	virtual NxRHI::FMeshPrimitives* IfGetTriMesh(NxRHI::IGpuDevice* rc) = 0;
	TR_MEMBER(SV_NoBind = true)
	virtual NxRHI::FMeshPrimitives* IfGetConvexMesh(NxRHI::IGpuDevice* rc) = 0;

	virtual int GetTrianglesRemap(int index) = 0;
};

NS_END