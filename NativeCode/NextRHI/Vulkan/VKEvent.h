#pragma once
#include "../NxEvent.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	class VKEvent : public IEvent
	{
	public:
		VKEvent(const char* name)
		{
			//mHandle = CreateEventEx(nullptr, name, 0, EVENT_ALL_ACCESS);
		}
		~VKEvent()
		{
			/*if (mHandle != nullptr)
			{
				CloseHandle(mHandle);
				mHandle = nullptr;
			}*/
		}
		bool Init(VKGpuDevice* pDevice, const FEventDesc& desc, const char* name);
		virtual void SetEvent() override
		{
			//::SetEvent(mHandle);
		}
		virtual void ResetEvent() override
		{
			//::ResetEvent(mHandle);
		}
		virtual bool Wait(UINT timeOut = INFINITE) override
		{
			//vkCmdWaitEvents()
			/*if (WaitForSingleObject(mHandle, timeOut) != WAIT_OBJECT_0)
				return false;*/
			return true;
		}
		VkEvent mHandle;
	};
	class VKFence : public IFence
	{
	public:
		VKFence();
		~VKFence();

		bool Init(VKGpuDevice * pDevice, const FFenceDesc & desc, const char* name);
		virtual UINT64 GetCompletedValue() override;
		virtual void CpuSignal(UINT64 value) override;
		virtual void Signal(ICmdQueue* queue, UINT64 value) override;
		virtual bool Wait(UINT64 value, UINT timeOut = INFINITE) override;
		virtual void SetDebugName(const char* name) override;

		virtual bool IsBinary() const{
			return false;
		}
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		AutoRef<VKEvent>	mEvent;
		VkSemaphore			mSemaphore = (VkSemaphore)nullptr;
	};
	class VKBinaryFence : public VKFence
	{
	public:
		VKBinaryFence(VKGpuDevice* pDevice);
		~VKBinaryFence();
		virtual bool IsBinary() const override{
			return true;
		}
		virtual UINT64 GetCompletedValue() override {
			return 0;
		}
		virtual bool Wait(UINT64 value, UINT timeOut = INFINITE) override;
	};
	class VKGpuToHostFence : public VIUnknownBase
	{
	public:
		VKGpuToHostFence(VKGpuDevice* pDevice, bool signal);
		~VKGpuToHostFence();
		void Wait();
		void Reset();
		bool IsSignaled();
	public:
		TObjectHandle<VKGpuDevice>	mDeviceRef;
		VkFence				mFence = (VkFence)nullptr;
	};
}

NS_END