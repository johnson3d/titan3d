#pragma once
#include "Nv5Common.h"
#include "../PhyMesh.h"

NS_BEGIN

class XndAttribute;
class NvPhyContext;

class Nv5PhyTriMesh : public PhyTriMesh
{
public:
	physx::PxTriangleMesh*		mMesh;
	TR_MEMBER(SV_NoBind)
	IBlobObject					mCookedData;
	Nv5PhyTriMesh()
	{
		mMesh = nullptr;
	}
	~Nv5PhyTriMesh();
	void Cleanup();
	bool CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size);
	IBlobObject* GetCookedData() {
		return &mCookedData;
	}
	NxRHI::FMeshDataProvider* CreateMeshProvider();
};

class Nv5PhyConvexMesh : public PhyConvexMesh
{
public:
	physx::PxConvexMesh*		mMesh;
	IBlobObject* mCookedData;
	Nv5PhyConvexMesh()
	{
		mMesh = nullptr;
		mCookedData = nullptr;
	}
	~Nv5PhyConvexMesh();
};

NS_END