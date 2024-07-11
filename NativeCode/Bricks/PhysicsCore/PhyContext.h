#pragma once
#include "PhyEntity.h"

NS_BEGIN

class PhySceneDesc;
class PhyScene;
class PhyActor;
class PhyMaterial;
class PhyShape;
class PhyTriMesh;
class PhyConvexMesh;
class PhyHeightfield;
struct PhyHeightFieldSample;

class TR_CLASS() 
	PhyContext : public PhyEntity
{
public:
	ENGINE_RTTI(PhyContext);
	physx::PxPhysics*		mContext;
	physx::PxFoundation*	mFoundation;
	physx::PxCooking*		mCooking;
	physx::PxPvd*			mPvd;
public:
	PhyContext();
	vBOOL Init(UINT featureFlags = 0xFFFFFFFF);
	PhySceneDesc* CreateSceneDesc();
	PhyScene* CreateScene(const PhySceneDesc* desc);
	PhyActor* CreateActor(EPhyActorType type, const v3dxVector3* p, const v3dxQuaternion* q);
	PhyActor* CreateActor(EPhyActorType type, const physx::PxTransform* pose);
	PhyMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution);

	PhyShape* CreateShapePlane(PhyMaterial* material);
	PhyShape* CreateShapeBox(PhyMaterial* material, const v3dxVector3* halfExtent);
	PhyShape* CreateShapeSphere(PhyMaterial* material, float radius);
	PhyShape* CreateShapeCapsule(PhyMaterial* material, float radius, float halfHeight);
	PhyShape* CreateShapeConvex(PhyMaterial* material, PhyConvexMesh* mesh, const v3dxVector3* scale, const v3dxQuaternion* scaleRot);
	PhyShape* CreateShapeTriMesh(PhyMaterial** material, int NumOfMtl, PhyTriMesh * mesh, const v3dxVector3* scale, const v3dxQuaternion* scaleRot);
	PhyShape* CreateShapeHeightfield(PhyMaterial** material, int NumOfMtl, PhyHeightfield* heightfield, float heightScale, const v3dxVector3* scale);

	PhyHeightfield* CookHeightfield(int nbColumns, int nbRows, PhyHeightFieldSample* pData, float convexEdgeThreshold = 0.0f, bool bNoBoundaryEdge = true);

	PhyConvexMesh* CookConvexMesh(NxRHI::FMeshDataProvider* mesh);
	PhyTriMesh* CookTriMesh(NxRHI::FMeshDataProvider* mesh, IBlobObject* uvblob, IBlobObject* faceblob, IBlobObject* posblob);
};

NS_END