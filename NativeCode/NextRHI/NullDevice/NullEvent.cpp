#include "NullEvent.h"
#include "NullGpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	bool NullEvent::Init(NullGpuDevice* pDevice, const FEventDesc& desc, const char* name)
	{
		Name = name;
		return true;
	}

	NullFence::NullFence()
	{
	}
	NullFence::~NullFence()
	{
	}
	bool NullFence::Init(NullGpuDevice* pDevice, const FFenceDesc& desc, const char* name)
	{
		Desc = desc;
		Name = name;

		return true;
	}
	UINT64 NullFence::GetCompletedValue()
	{
		return 0;
	}
	void NullFence::CpuSignal(UINT64 value)
	{

	}
	void NullFence::Signal(ICmdQueue* queue, UINT64 value)
	{

	}
	bool NullFence::Wait(UINT64 value, UINT timeOut)
	{
		return true;
	}
}

NS_END
