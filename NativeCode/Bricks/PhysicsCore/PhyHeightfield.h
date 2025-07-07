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
	PhyHeightfield : public IWeakRefObject
{
public:
	TR_MEMBER(SV_NoBind)
	IBlobObject					mCookedData;
	virtual bool CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size) = 0;
	IBlobObject* GetCookedData() {
		return &mCookedData;
	}
	virtual NxRHI::FMeshDataProvider* CreateMeshProvider() = 0;

	virtual bool ModifySamples(UINT startCol, UINT startRow,
			UINT nbCols, UINT nbRows, void* pData, UINT dataStride, float convexEdgeThreshold,
			bool shrinkBounds = false) = 0;
};

NS_END