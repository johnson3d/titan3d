#include "vfxThreadDispatcher.h"

#define new VNEW

NS_BEGIN

static FContextTickableManager gFContextTickableManager;
FContextTickableManager* FContextTickableManager::GetInstance()
{
	return &gFContextTickableManager;
}

void FContextTickableManager::ThreadTick()
{
	for (size_t i = 0; i < mTickables.size(); i++)
	{
		mTickables[i]();
	} 
}

NS_END
