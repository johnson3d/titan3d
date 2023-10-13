#pragma once
#include "../NxEvent.h"
#include "DX11PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX11GpuDevice;
	class DX11Event : public IEvent
	{
	public:
		DX11Event(const char* name)
		{
			mHandle = CreateEventExA(nullptr, name, 0, EVENT_ALL_ACCESS);
		}
		~DX11Event()
		{
			if (mHandle != nullptr)
			{
				CloseHandle(mHandle);
				mHandle = nullptr;
			}
		}
		bool Init(DX11GpuDevice* pDevice, const FEventDesc& desc, const char* name);
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
	class DX11Fence : public IFence
	{
	public:
		DX11Fence();
		~DX11Fence();

		bool Init(DX11GpuDevice * pDevice, const FFenceDesc & desc, const char* name);
		virtual UINT64 GetCompletedValue() override;
		virtual void CpuSignal(UINT64 value) override;
		virtual void Signal(ICmdQueue* queue, UINT64 value, EQueueType type) override;
		virtual bool Wait(UINT64 value, UINT timeOut = INFINITE) override;
	public:
		AutoRef<DX11Event>	mEvent;
		ID3D11Fence*		mFence;
	};
}

NS_END