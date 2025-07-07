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
class PhyBoxControllerDesc;
class PhyCapsuleControllerDesc;

enum TR_ENUM()
	EPhysicsContextType
{
	NvPhysX,
};

class TR_CLASS() 
	PhyContext : public PhyEntity
{
public:
	ENGINE_RTTI(PhyContext);
public:
	static PhyContext* CreateContext(EPhysicsContextType type = EPhysicsContextType::NvPhysX);
	virtual vBOOL Init(UINT featureFlags = 0xFFFFFFFF) = 0;
	virtual PhySceneDesc* CreateSceneDesc() = 0;
	virtual PhyScene* CreateScene(const PhySceneDesc* desc) = 0;
	virtual PhyActor* CreateActor(EPhyActorType type, const v3dxVector3* p, const v3dxQuaternion* q) = 0;
	virtual PhyMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution) = 0;

	virtual PhyShape* CreateShapePlane(PhyMaterial* material) = 0;
	virtual PhyShape* CreateShapeBox(PhyMaterial* material, const v3dxVector3* halfExtent) = 0;
	virtual PhyShape* CreateShapeSphere(PhyMaterial* material, float radius) = 0;
	virtual PhyShape* CreateShapeCapsule(PhyMaterial* material, float radius, float halfHeight) = 0;
	virtual PhyShape* CreateShapeConvex(PhyMaterial* material, PhyConvexMesh* mesh, const v3dxVector3* scale, const v3dxQuaternion* scaleRot) = 0;
	virtual PhyShape* CreateShapeTriMesh(PhyMaterial** material, int NumOfMtl, PhyTriMesh * mesh, const v3dxVector3* scale, const v3dxQuaternion* scaleRot) = 0;
	virtual PhyShape* CreateShapeHeightfield(PhyMaterial** material, int NumOfMtl, PhyHeightfield* heightfield, float heightScale, const v3dxVector3* scale) = 0;

	virtual PhyHeightfield* CookHeightfield(int nbColumns, int nbRows, PhyHeightFieldSample* pData, float convexEdgeThreshold = 0.0f, bool bNoBoundaryEdge = true) = 0;

	virtual PhyConvexMesh* CookConvexMesh(NxRHI::FMeshDataProvider* mesh) = 0;
	virtual PhyTriMesh* CookTriMesh(NxRHI::FMeshDataProvider* mesh, IBlobObject* uvblob, IBlobObject* faceblob, IBlobObject* posblob) = 0;

	virtual PhyBoxControllerDesc* CreateBoxControllerDesc() = 0;
	virtual PhyCapsuleControllerDesc* CreateCapsuleControllerDesc() = 0;
	virtual PhyTriMesh* CreateTriMesh() = 0;
	virtual PhyConvexMesh* CreateConvexMesh() = 0;
};

NS_END