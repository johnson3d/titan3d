#include "VKEvent.h"
#include "VKGpuDevice.h"
#include "VKCommandList.h"

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
	template<>
	struct AuxGpuResourceDestroyer<VkFence>
	{
		static void Destroy(VkFence obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyFence(device->mDevice, obj, device->GetVkAllocCallBacks());
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
		ExpectValue = desc.InitValue;

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
		auto hr = VKGpuSystem::vkGetSemaphoreCounterValue(device->mDevice, mSemaphore, &count);
		if (count == UINT64_MAX)
		{
			ASSERT(false);
		}
		ASSERT(hr == VK_SUCCESS);
		return count;
	}
	void VKFence::CpuSignal(UINT64 value)
	{
		auto device = mDeviceRef.GetPtr();
		VkSemaphoreSignalInfo info{};
		info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_SIGNAL_INFO;
		info.semaphore = mSemaphore;
		info.value = value;
		VKGpuSystem::vkSignalSemaphore(device->mDevice, &info);
	}
	void VKFence::Signal(ICmdQueue* queue1, UINT64 value, EQueueType type)
	{
		auto queue = (VKCmdQueue*)queue1;
		ASSERT(value != UINT64_MAX);
		auto signalFence = this;

		VkSubmitInfo submitInfo{};
		submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
		submitInfo.commandBufferCount = 1;
		submitInfo.pCommandBuffers = &queue->mDummyCmdList->mCommandBuffer->RealObject;

		submitInfo.waitSemaphoreCount = 0;
		submitInfo.pWaitSemaphores = nullptr;
		VkPipelineStageFlags waitStage = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT |
			VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT |
			VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;
		submitInfo.pWaitDstStageMask = &waitStage;
		submitInfo.signalSemaphoreCount = 1;
		VkSemaphore signalSmp[1]{};
		signalSmp[0] = signalFence->mSemaphore;
		submitInfo.pSignalSemaphores = signalSmp;

		VkTimelineSemaphoreSubmitInfo timelineInfo{};
		submitInfo.pNext = &timelineInfo;
		timelineInfo.sType = VK_STRUCTURE_TYPE_TIMELINE_SEMAPHORE_SUBMIT_INFO;

		timelineInfo.waitSemaphoreValueCount = 0;
		timelineInfo.pWaitSemaphoreValues = nullptr;

		UINT64 signalValue[1]{};
		signalValue[0] = value;
		timelineInfo.signalSemaphoreValueCount = 1;
		timelineInfo.pSignalSemaphoreValues = signalValue;

		auto hr = vkQueueSubmit(queue->mGraphicsQueue, 1, &submitInfo, nullptr);
		ASSERT(hr == VK_SUCCESS);

		return;
	}
	UINT64 VKFence::Wait(UINT64 value, UINT timeOut)
	{
		//VkSubmitInfo submitInfo{};
		//submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
		//submitInfo.commandBufferCount = 1;
		//submitInfo.pCommandBuffers = &mDummyCmdList->mCommandBuffer->RealObject;

		//VkPipelineStageFlags waitStage = VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;/*VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT |
		//	VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT |
		//	VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;*/
		//VkSemaphore waitSmp[1]{};
		//waitSmp[0] = waitFence->mSemaphore;
		//submitInfo.waitSemaphoreCount = 1;
		//submitInfo.pWaitSemaphores = waitSmp;
		//VkPipelineStageFlags waitStages[1];
		//waitStages[0] = waitStage;
		//submitInfo.pWaitDstStageMask = waitStages;

		//submitInfo.signalSemaphoreCount = 0;
		//submitInfo.pSignalSemaphores = nullptr;

		//VkTimelineSemaphoreSubmitInfo timelineInfo{};
		//submitInfo.pNext = &timelineInfo;
		//timelineInfo.sType = VK_STRUCTURE_TYPE_TIMELINE_SEMAPHORE_SUBMIT_INFO;

		//UINT64 waitValue[1]{};
		//waitValue[0] = value;
		//timelineInfo.waitSemaphoreValueCount = 2;
		//timelineInfo.pWaitSemaphoreValues = waitValue;

		//timelineInfo.signalSemaphoreValueCount = 0;
		//timelineInfo.pSignalSemaphoreValues = nullptr;

		//auto hr = vkQueueSubmit(mGraphicsQueue, 1, &submitInfo, nullptr);
		//ASSERT(hr == VK_SUCCESS);

		//temp code
		auto device = mDeviceRef.GetPtr();
		VkSemaphoreWaitInfo info{};
		info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_WAIT_INFO;
		info.flags = 0;// VK_SEMAPHORE_WAIT_ANY_BIT;
		info.semaphoreCount = 1;
		info.pSemaphores = &mSemaphore;
		info.pValues = &value;
		auto hr = VKGpuSystem::vkWaitSemaphores(device->mDevice, &info, UINT64_MAX);
		ASSERT(hr == VK_SUCCESS);

		//auto completed = GetCompletedValue();
		//if (completed < value)
		//{
		//	auto device = mDeviceRef.GetPtr();
		//	VkSemaphoreWaitInfo info{};
		//	info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_WAIT_INFO;
		//	info.flags = 0;// VK_SEMAPHORE_WAIT_ANY_BIT;
		//	info.semaphoreCount = 1;
		//	info.pSemaphores = &mSemaphore;
		//	info.pValues = &value;
		//	VKGpuSystem::vkWaitSemaphores(device->mDevice, &info, UINT64_MAX);
		//	//completed = GetCompletedValue();
		//}

		return value;
	}
	void VKFence::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT, mSemaphore, name);
	}
	/// ====================================
	VKBinaryFence::VKBinaryFence(VKGpuDevice* device)
	{
		mDeviceRef.FromObject(device);

		VkSemaphoreCreateInfo info{};
		info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
		if (vkCreateSemaphore(device->mDevice, &info, device->GetVkAllocCallBacks(), &mSemaphore) != VK_SUCCESS)
		{
			ASSERT(false);
		}
	}
	VKBinaryFence::~VKBinaryFence()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		device->DelayDestroy(mSemaphore);
		mSemaphore = nullptr;
	}
	UINT64 VKBinaryFence::Wait(UINT64 value, UINT timeOut)
	{
		ASSERT(false);
		return 0;
	}
	/// ====================================
	VKGpuToHostFence::VKGpuToHostFence(VKGpuDevice* device, bool signal)
	{
		mDeviceRef.FromObject(device);

		VkFenceCreateInfo info{};
		info.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
		if (signal)
			info.flags = VK_FENCE_CREATE_SIGNALED_BIT;
		if (vkCreateFence(device->mDevice, &info, device->GetVkAllocCallBacks(), &mFence) != VK_SUCCESS)
		{
			ASSERT(false);
		}
	}
	VKGpuToHostFence::~VKGpuToHostFence()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		device->DelayDestroy(mFence);
		mFence = nullptr;
	}
	void VKGpuToHostFence::Reset()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		auto hr = vkResetFences(device->mDevice, 1, &mFence);
		if (hr != VK_SUCCESS)
		{
			ASSERT(false);
		}
	}
	bool VKGpuToHostFence::IsSignaled()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return false;
		auto hr = vkGetFenceStatus(device->mDevice, mFence);
		return hr == VK_SUCCESS;
	}
	void VKGpuToHostFence::Wait()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		auto hr = vkWaitForFences(device->mDevice, 1, &mFence, VK_TRUE, UINT64_MAX);
		if (hr != VK_SUCCESS)
		{
			ASSERT(false);
		}
	}
}

NS_END
