#include "IBone.h"
#include "../../../Base/CoreRtti.h"

#define new VNEW

namespace EngineNS
{
	template<> AuxRttiStruct<v3dxIndexInSkeleton>		AuxRttiStruct<v3dxIndexInSkeleton>::Instance;

	IBone* IBone::Create(const IBoneDesc& desc)
	{
		return new IBone(desc);
	}

	IBone::IBone(const IBoneDesc& desc): ParentIndex(-1), Index(-1)
	{
		Desc = desc;
	}

}