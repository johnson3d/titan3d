#pragma once
#include "PhyEntity.h"

NS_BEGIN

class PhySceneDesc;
class PhyScene;
class PhyActor;
class PhyMaterial;
class PhyShape;

class PhyContext : public PhyEntity
{
public:
	RTTI_DEF(PhyContext, 0x0d6aae375befae7b, true);
	physx::PxPhysics*		mContext;
	physx::PxFoundation*	mFoundation;
	physx::PxCooking*		mCooking;
	physx::PxPvd*			mPvd;
public:
	PhyContext();
	vBOOL Init(UINT32 featureFlags = 0xFFFFFFFF);
	PhySceneDesc* CreateSceneDesc();
	PhyScene* CreateScene(const PhySceneDesc* desc);
	PhyActor* CreateActor(EPhyActorType type, const physx::PxTransform* pose);
	PhyMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution);

	PhyShape* CreateShapePlane(PhyMaterial* material);
	PhyShape* CreateShapeBox(PhyMaterial* material, float width, float height, float length);
	PhyShape* CreateShapeSphere(PhyMaterial* material, float radius);
	PhyShape* CreateShapeCapsule(PhyMaterial* material, float radius, float halfHeight);
	PhyShape* CreateShapeConvex(PhyMaterial* material, IBlobObject* blob, const v3dxVector3* scale, const v3dxQuaternion* scaleRot);
	PhyShape* CreateShapeTriMesh(PhyMaterial* material, IBlobObject* blob, const v3dxVector3* scale, const v3dxQuaternion* scaleRot);

	vBOOL CookConvexMesh(IRenderContext* rc, IGeometryMesh* mesh, IBlobObject* blob);
	vBOOL CookTriMesh(IRenderContext* rc, IGeometryMesh* mesh, IBlobObject* blob, IBlobObject* uvblob, IBlobObject* faceblob, IBlobObject* posblob);
};

NS_END