#include "Nv5PhyHeightfield.h"
#include "Nv5PhyContext.h"
#include "../../../NextRHI/NxRHI.h"
#include "../../../Base/xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

using namespace NxRHI;

Nv5PhyHeightfield::~Nv5PhyHeightfield()
{
	Cleanup();
}

void Nv5PhyHeightfield::Cleanup()
{
	if (mHeightField != nullptr)
	{
		mHeightField->release();
		mHeightField = nullptr;
	}
}

bool Nv5PhyHeightfield::CreateFromCookedData(PhyContext* ctx, void* cookedData, UINT size)
{
	Cleanup();

	mCookedData.ReSize(0);
	mCookedData.PushData(cookedData, size);
	physx::PxDefaultMemoryInputData readBuffer((physx::PxU8*)cookedData, size);
	mHeightField = ((Nv5PhyContext*)ctx)->mContext->createHeightField(readBuffer);
	if (mHeightField == nullptr)
		return false;

	return true;
}

FMeshDataProvider* Nv5PhyHeightfield::CreateMeshProvider()
{
	return nullptr;
}

bool Nv5PhyHeightfield::ModifySamples(UINT startCol, UINT startRow,
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
