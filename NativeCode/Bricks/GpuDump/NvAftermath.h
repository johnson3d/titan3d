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
		static NxRHI::ERhiType GetAfterMathRhiType();
		static void InitDump(NxRHI::ERhiType type);
		static void DeviceCreated(NxRHI::ERhiType type, NxRHI::IGpuDevice * device);
		static void RegByteCode(const char* name, void* pCode, UINT size);

		static void OnDredDump(NxRHI::IGpuDevice* device, const char* dir);
	};
};

NS_END