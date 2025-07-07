#include "NvPhyHeightfield.h"
#include "NvPhyContext.h"
#include "../../../NextRHI/NxRHI.h"
#include "../../../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

using namespace NxRHI;

NvPhyHeightfield::~NvPhyHeightfield()
{
	Cleanup();
}

void NvPhyHeightfield::Cleanup()
{
	if (mHeightField != nullptr)
	{
		mHeightField->release();
		mHeightField = nullptr;
	}
}

bool NvPhyHeightfield::CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size)
{
	Cleanup();

	mCookedData.ReSize(0);
	mCookedData.PushData(cookedData, size);
	physx::PxDefaultMemoryInputData readBuffer((physx::PxU8*)cookedData, size);
	mHeightField = ((NvPhyContext*)ctx)->mContext->createHeightField(readBuffer);
	if (mHeightField == nullptr)
		return false;

	return true;
}

FMeshDataProvider* NvPhyHeightfield::CreateMeshProvider()
{
	return nullptr;
}

bool NvPhyHeightfield::ModifySamples(UINT startCol, UINT startRow,
	UINT nbColumns, UINT nbRows, void* pData, UINT dataStride, float convexEdgeThreshold,
	bool shrinkBounds)
{
	physx::PxHeightFieldDesc desc;
	desc.nbColumns = nbColumns;
	desc.nbRows = nbRows;
	desc.convexEdgeThreshold = convexEdgeThreshold;
	desc.samples.data = pData;
	desc.samples.stride = dataStride;
	return mHeightField->modifySamples(startCol, startRow, desc, shrinkBounds);
}

NS_END
