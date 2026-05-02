#pragma once

#include "../../Base/IUnknown.h"
#include "../../Base/BlobObject.h"
#include "../../NextRHI/NxGpuDevice.h"

NS_BEGIN

namespace GpuDump
{
	class TR_CLASS()
		NvAftermath : public IWeakRefObject
	{
	public:
		static NxRHI::ERhiType GetAfterMathRhiType();
		static void InitDump(NxRHI::ERhiType type);
		static void DeviceCreated(NxRHI::ERhiType type, NxRHI::IGpuDevice * device);
		static void RegByteCode(const char* name, void* pCode, UINT size);

		static void OnDredDump(NxRHI::IGpuDevice* device, const char* dir);

		// Set the root directory where all Aftermath output files (crash dumps, shader debug info,
		// shader binaries, JSON, etc.) will be written. The directory is created if it does not
		// exist. Trailing slashes are normalized. Pass an empty string to fall back to the current
		// working directory (default behavior).
		// Should be called once during engine initialization, before InitDump.
		static void SetOutputRoot(const char* root);

		// Get the currently configured output root (always ends with '/' if non-empty).
		static const std::string& GetOutputRoot();
	};
};

NS_END