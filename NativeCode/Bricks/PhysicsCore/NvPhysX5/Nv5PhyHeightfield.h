#pragma once
#include "Nv5Common.h"
#include "../PhyHeightfield.h"

NS_BEGIN

class Nv5PhyContext;

class Nv5PhyHeightfield : public PhyHeightfield
{
public:
	physx::PxHeightField*		mHeightField;
	TR_MEMBER(SV_NoBind)
	IBlobObject					mCookedData;
	Nv5PhyHeightfield()
	{
		mHeightField = nullptr;
	}
	~Nv5PhyHeightfield();
	void Cleanup();
	bool CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size);
	IBlobObject* GetCookedData() {
		return &mCookedData;
	}
	NxRHI::FMeshDataProvider* CreateMeshProvider();

	bool ModifySamples(UINT startCol, UINT startRow, 
			UINT nbCols, UINT nbRows, void* pData, UINT dataStride, float convexEdgeThreshold,
			bool shrinkBounds = false);
};

NS_END