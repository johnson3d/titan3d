#pragma once
#include "NvCommon.h"
#include "../PhyMesh.h"

NS_BEGIN

class XndAttribute;
class NvPhyContext;

class NvPhyTriMesh : public PhyTriMesh
{
public:
	physx::PxTriangleMesh*		mMesh;
	TR_MEMBER(SV_NoBind)
	IBlobObject					mCookedData;
	NvPhyTriMesh()
	{
		mMesh = nullptr;
	}
	~NvPhyTriMesh();
	void Cleanup();
	bool CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size);
	IBlobObject* GetCookedData() {
		return &mCookedData;
	}
	NxRHI::FMeshDataProvider* CreateMeshProvider();
};

class TR_CLASS()
	NvPhyConvexMesh : public PhyConvexMesh
{
public:
	physx::PxConvexMesh*		mMesh;
	IBlobObject* mCookedData;
	NvPhyConvexMesh()
	{
		mMesh = nullptr;
		mCookedData = nullptr;
	}
	~NvPhyConvexMesh();
};

NS_END