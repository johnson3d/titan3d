#pragma once
#include "PhyEntity.h"

NS_BEGIN

class XndAttribute;
class PhyContext;

class TR_CLASS()
	PhyTriMesh : public IWeakRefObject
{
public:
	TR_MEMBER(SV_NoBind)
	IBlobObject					mCookedData;
	virtual bool CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size) = 0;
	IBlobObject* GetCookedData() {
		return &mCookedData;
	}
	virtual NxRHI::FMeshDataProvider* CreateMeshProvider() = 0;
};

class TR_CLASS()
	PhyConvexMesh : public IWeakRefObject
{
public:
	IBlobObject* mCookedData;
};

NS_END