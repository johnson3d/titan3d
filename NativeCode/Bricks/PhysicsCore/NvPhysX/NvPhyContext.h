#pragma once
#include "NvCommon.h"
#include "../PhyContext.h"

NS_BEGIN

class NvPhyContext : public PhyContext
{
public:
	ENGINE_RTTI(NvPhyContext);
	physx::PxPhysics*		mContext;
	physx::PxFoundation*	mFoundation;
	physx::PxCooking*		mCooking;
	physx::PxPvd*			mPvd;
public:
	NvPhyContext();
	virtual vBOOL Init(UINT featureFlags = 0xFFFFFFFF) override;
	virtual PhySceneDesc* CreateSceneDesc() override;
	virtual PhyScene* CreateScene(const PhySceneDesc* desc) override;
	virtual PhyActor* CreateActor(EPhyActorType type, const v3dxVector3* p, const v3dxQuaternion* q) override;
	PhyActor* CreateActor(EPhyActorType type, const physx::PxTransform* pose);
	virtual PhyMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution) override;

	virtual PhyShape* CreateShapePlane(PhyMaterial* material) override;
	virtual PhyShape* CreateShapeBox(PhyMaterial* material, const v3dxVector3* halfExtent) override;
	virtual PhyShape* CreateShapeSphere(PhyMaterial* material, float radius) override;
	virtual PhyShape* CreateShapeCapsule(PhyMaterial* material, float radius, float halfHeight) override;
	virtual PhyShape* CreateShapeConvex(PhyMaterial* material, PhyConvexMesh* mesh, const v3dxVector3* scale, const v3dxQuaternion* scaleRot) override;
	virtual PhyShape* CreateShapeTriMesh(PhyMaterial** material, int NumOfMtl, PhyTriMesh * mesh, const v3dxVector3* scale, const v3dxQuaternion* scaleRot) override;
	virtual PhyShape* CreateShapeHeightfield(PhyMaterial** material, int NumOfMtl, PhyHeightfield* heightfield, float heightScale, const v3dxVector3* scale) override;

	virtual PhyHeightfield* CookHeightfield(int nbColumns, int nbRows, PhyHeightFieldSample* pData, float convexEdgeThreshold = 0.0f, bool bNoBoundaryEdge = true) override;

	virtual PhyConvexMesh* CookConvexMesh(NxRHI::FMeshDataProvider* mesh) override;
	virtual PhyTriMesh* CookTriMesh(NxRHI::FMeshDataProvider* mesh, IBlobObject* uvblob, IBlobObject* faceblob, IBlobObject* posblob) override;

	virtual PhyBoxControllerDesc* CreateBoxControllerDesc() override;
	virtual PhyCapsuleControllerDesc* CreateCapsuleControllerDesc()override;

	virtual PhyTriMesh* CreateTriMesh() override;
	virtual PhyConvexMesh* CreateConvexMesh() override;
};

NS_END