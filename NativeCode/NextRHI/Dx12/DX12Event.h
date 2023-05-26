#pragma once
#include "../NxEvent.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12Event : public IEvent
	{
	public:
		DX12Event(const char* name)
		{
			mHandle = CreateEventExA(nullptr, name, 0, EVENT_ALL_ACCESS);
		}
		~DX12Event()
		{
			if (mHandle != nullptr)
			{
				CloseHandle(mHandle);
				mHandle = nullptr;
			}
		}
		bool Init(DX12GpuDevice* pDevice, const FEventDesc& desc, const char* name);
		virtual void SetEvent() override
		{
			::SetEvent(mHandle);
		}
		virtual void ResetEvent() override
		{
			::ResetEvent(mHandle);
		}
		virtual bool Wait(UINT timeOut = INFINITE) override
		{
			if (WaitForSingleObject(mHandle, timeOut) != WAIT_OBJECT_0)
				return false;
			return true;
		}
		HANDLE mHandle;
	};
	class DX12Fence : public IFence
	{
	public:
		DX12Fence();
		~DX12Fence();

		bool Init(DX12GpuDevice * pDevice, const FFenceDesc & desc, const char* name);
		virtual UINT64 GetCompletedValue() override;
		virtual void CpuSignal(UINT64 value) override;
		virtual void Signal(ICmdQueue* queue, UINT64 value, EQueueType type) override;
		virtual bool Wait(UINT64 value, UINT timeOut = INFINITE) override;
		virtual void SetDebugName(const char* name) override;
	public:
		AutoRef<DX12Event>	mEvent;
		ID3D12Fence*		mFence;
	};
}

NS_END