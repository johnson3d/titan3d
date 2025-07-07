#pragma once
#include "NvCommon.h"
#include "../PhyHeightfield.h"

NS_BEGIN

class NvPhyContext;

class NvPhyHeightfield : public PhyHeightfield
{
public:
	physx::PxHeightField*		mHeightField;
	TR_MEMBER(SV_NoBind)
	IBlobObject					mCookedData;
	NvPhyHeightfield()
	{
		mHeightField = nullptr;
	}
	~NvPhyHeightfield();
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