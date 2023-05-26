#pragma once

#include "../../Base/IUnknown.h"
#include "../../Base/BlobObject.h"
#include "../../NextRHI/NxGpuDevice.h"

NS_BEGIN

namespace GpuDump
{
	class TR_CLASS()
		NvAftermath : public IWeakReference
	{
	public:
		static void InitDump(NxRHI::ERhiType type);
		static void RegSpirv(void* pCode, UINT size);
	};
};

NS_END