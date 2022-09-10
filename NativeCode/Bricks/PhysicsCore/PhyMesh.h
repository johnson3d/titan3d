#pragma once
#include "PhyEntity.h"

NS_BEGIN

class XndAttribute;
class PhyContext;

class TR_CLASS()
	PhyTriMesh : public VIUnknown
{
public:
	physx::PxTriangleMesh*		mMesh;
	TR_MEMBER(SV_NoBind)
	IBlobObject					mCookedData;
	PhyTriMesh()
	{
		mMesh = nullptr;
	}
	~PhyTriMesh();
	void Cleanup();
	bool CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size);
	IBlobObject* GetCookedData() {
		return &mCookedData;
	}
	NxRHI::FMeshDataProvider* CreateMeshProvider();
};

class TR_CLASS()
	PhyConvexMesh : public VIUnknown
{
public:
	physx::PxConvexMesh*		mMesh;
	IBlobObject* mCookedData;
	PhyConvexMesh()
	{
		mMesh = nullptr;
		mCookedData = nullptr;
	}
	~PhyConvexMesh();
};

NS_END