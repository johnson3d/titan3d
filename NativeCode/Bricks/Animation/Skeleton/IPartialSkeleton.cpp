#include "IPartialSkeleton.h"

namespace EngineNS
{

	void IPartialSkeleton::Cleanup()
	{

	}

	void IPartialSkeleton::GenerateHierarchy()
	{

	}
	bool IPartialSkeleton::SetRoot(VNameString name)
	{
		ASSERT(false);
		return false;
	}
	int IPartialSkeleton::AddBone(IBone* pBone) 
	{
		ASSERT(false);
		return 0;
	}
	bool IPartialSkeleton::RemoveBone(UINT nameHash)
	{
		ASSERT(false);
		return false;
	}
}