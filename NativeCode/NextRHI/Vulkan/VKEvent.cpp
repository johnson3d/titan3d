#include "VKEvent.h"
#include "VKGpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	template<>
	struct AuxGpuResourceDestroyer<VkSemaphore>
	{
		static void Destroy(VkSemaphore obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroySemaphore(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	bool VKEvent::Init(VKGpuDevice* pDevice, const FEventDesc& desc, const char* name)
	{
		Name = name;
		return true;
	}

	VKFence::VKFence()
	{
	}
	VKFence::~VKFence()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		device->DelayDestroy(mSemaphore);
		mSemaphore = nullptr;
	}
	bool VKFence::Init(VKGpuDevice* pDevice, const FFenceDesc& desc, const char* name)
	{
		Desc = desc;
		Name = name;
		mDeviceRef.FromObject(pDevice);

		mEvent = MakeWeakRef(new VKEvent(Name.c_str()));
		/*if (S_OK != pDevice->mDevice->CreateFence(desc.InitValue, (D3D12_FENCE_FLAGS)desc.Type, __uuidof(ID3D12Fence), (void**)&mFence))
		{
			return false;
		}*/
		VkSemaphoreCreateInfo info{};
		info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
		//info.flags = VK_FENCE_CREATE_SIGNALED_BIT;

		VkSemaphoreTypeCreateInfo timelineCreateInfo{};
		timelineCreateInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_TYPE_CREATE_INFO;
		timelineCreateInfo.semaphoreType = VkSemaphoreType::VK_SEMAPHORE_TYPE_TIMELINE;
		timelineCreateInfo.initialValue = desc.InitValue;
		AspectValue = desc.InitValue;

		info.pNext = &timelineCreateInfo;

		if (vkCreateSemaphore(pDevice->mDevice, &info, pDevice->GetVkAllocCallBacks(), &mSemaphore) != VK_SUCCESS)
		{
			return false;
		}

		SetDebugName(name);
		return true;
	}
	UINT64 VKFence::GetCompletedValue()
	{
		UINT64 count = 0;
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
		{
			ASSERT(false);
			return 0;
		}
		auto hr = vkGetSemaphoreCounterValue(device->mDevice, mSemaphore, &count);
		if (count == UINT64_MAX)
		{
			device->DelayDestroy(mSemaphore);
			mSemaphore = nullptr;

			VkSemaphoreCreateInfo info{};
			info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
			//info.flags = VK_FENCE_CREATE_SIGNALED_BIT;

			VkSemaphoreTypeCreateInfo timelineCreateInfo{};
			timelineCreateInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_TYPE_CREATE_INFO;
			timelineCreateInfo.semaphoreType = VkSemaphoreType::VK_SEMAPHORE_TYPE_TIMELINE;
			timelineCreateInfo.initialValue = 0;

			info.pNext = &timelineCreateInfo;

			if (vkCreateSemaphore(device->mDevice, &info, device->GetVkAllocCallBacks(), &mSemaphore) != VK_SUCCESS)
			{
				ASSERT(false);
			}
			return device->GetCmdQueue()->SignalFence(this, AspectValue + 1);
		}
		ASSERT(hr == VK_SUCCESS);
		return count;
	}
	void VKFence::SetEvent()
	{
		mEvent->SetEvent();
	}
	void VKFence::ResetEvent()
	{
		mEvent->ResetEvent();
	}
	void VKFence::CpuSignal(UINT64 value)
	{
		auto device = mDeviceRef.GetPtr();
		VkSemaphoreSignalInfo info{};
		info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_SIGNAL_INFO;
		info.semaphore = mSemaphore;
		info.value = value;
		vkSignalSemaphore(device->mDevice, &info);
	}
	void VKFence::Signal(ICmdQueue* queue, UINT64 value)
	{
		ASSERT(value < UINT64_MAX / 2);
		if (AspectValue >= value)
		{
			ASSERT(false);
			return;
		}
		AspectValue = value;
		
		VkTimelineSemaphoreSubmitInfo timelineInfo;
		timelineInfo.sType = VK_STRUCTURE_TYPE_TIMELINE_SEMAPHORE_SUBMIT_INFO;
		timelineInfo.pNext = NULL;
		timelineInfo.waitSemaphoreValueCount = 0;
		timelineInfo.signalSemaphoreValueCount = 1;
		timelineInfo.pSignalSemaphoreValues = &value;

		VkSubmitInfo submitInfo{};
		submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
		submitInfo.pNext = &timelineInfo;
		submitInfo.commandBufferCount = 0;
		submitInfo.waitSemaphoreCount = 0;
		submitInfo.signalSemaphoreCount = 1;
		submitInfo.pSignalSemaphores = &mSemaphore;
		VAutoVSLLock lk(((VKCmdQueue*)queue)->mGraphicsQueueLocker);
		vkQueueSubmit(((VKCmdQueue*)queue)->mGraphicsQueue, 1, &submitInfo, nullptr);
	}
	bool VKFence::Wait(UINT64 value, UINT timeOut)
	{
		if (GetCompletedValue() < value)
		{
			auto device = mDeviceRef.GetPtr();
			VkSemaphoreWaitInfo info{};
			info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_WAIT_INFO;
			info.flags = 0;// VK_SEMAPHORE_WAIT_ANY_BIT;
			info.semaphoreCount = 1;
			info.pSemaphores = &mSemaphore;
			info.pValues = &value;
			vkWaitSemaphores(device->mDevice, &info, UINT64_MAX);
		}

		return true;
	}
	void VKFence::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT, mSemaphore, name);
	}
}

NS_END
