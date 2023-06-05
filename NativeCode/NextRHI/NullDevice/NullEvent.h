#pragma once
#include "../NxEvent.h"
#include "NullPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class NullGpuDevice;
	class NullEvent : public IEvent
	{
	public:
		NullEvent(const char* name)
		{
			
		}
		~NullEvent()
		{
			
		}
		bool Init(NullGpuDevice* pDevice, const FEventDesc& desc, const char* name);
		virtual void SetEvent() override
		{
			
		}
		virtual void ResetEvent() override
		{
			
		}
		virtual bool Wait(UINT timeOut = INFINITE) override
		{
			return true;
		}
	};
	class NullFence : public IFence
	{
	public:
		NullFence();
		~NullFence();

		bool Init(NullGpuDevice * pDevice, const FFenceDesc & desc, const char* name);
		virtual UINT64 GetCompletedValue() override;
		virtual void CpuSignal(UINT64 value) override;
		virtual void Signal(ICmdQueue* queue, UINT64 value, EQueueType type) override;
		virtual UINT64 Wait(UINT64 value, UINT timeOut = INFINITE) override;
	public:
	};
}

NS_END