#pragma once
#include "PhyEntity.h"

NS_BEGIN

class PhyContext;

#pragma pack(push)
#pragma pack(4)
struct TR_CLASS(SV_LayoutStruct = 4)
	PhyHeightFieldSample
{
	SHORT			height;
	BYTE			materialIndex0;
	BYTE			materialIndex1;
};
#pragma pack(pop)

class TR_CLASS()
	PhyHeightfield : public VIUnknown
{
public:
	physx::PxHeightField*		mHeightField;
	TR_MEMBER(SV_NoBind)
	IBlobObject					mCookedData;
	PhyHeightfield()
	{
		mHeightField = nullptr;
	}
	~PhyHeightfield();
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