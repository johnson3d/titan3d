#pragma once
#include "../VKPreHead.h"
#include "../../NxRHIDefine.h"

NS_BEGIN

namespace NxRHI
{
	class VKFence;

	class VKDescriptorPoolWrapper : public IGpuResource
	{
	public:
		VKGpuDevice* mDeviceRef = nullptr;
		int							MaxOfDesriptor = 10000;
		int 						NumOfSampler = 5000;
		int 						NumOfSampledImage = 10000;
		int 						NumOfStorageImage = 10000;
		int 						NumOfUniformBuffer = 10000;
		int 						NumOfStorageBuffer = 10000;
		VkDescriptorPool			mDescriptorPool = VK_NULL_HANDLE;

		void Initialize(VKGpuDevice* device);
		VkDescriptorSet Alloc(VkDescriptorSetLayout layout);
	};

	class VKDesriptorPoolManager;
	class VKDescriptorSetFrame : public VIUnknown
	{
	private:
		VKGpuDevice* mDeviceRef;
		VKDesriptorPoolManager* mManagerRef;
		std::vector<AutoRef<VKDescriptorPoolWrapper>> mDescriptorPools;
		int mCurrentFreePoolIndex{};
		int GetCurrentPoolIndex();
		int IncreaseCurrentPoolIndex();
	public:
		void Initialize(VKGpuDevice* device, VKDesriptorPoolManager* manager);
		void Reset();
		VkDescriptorSet AllocDescriptorSet(VkDescriptorSetLayout layout);
		
		UINT64 mFrameFenceValue = 0;
	};

	class VKDesriptorPoolManager : public VIUnknown
	{
		VKGpuDevice* mDeviceRef;
		std::stack<AutoRef<VKDescriptorSetFrame>> mFreeFramePools;
		std::vector<AutoRef<VKDescriptorSetFrame>> mUsingFramePools;
		AutoRef<VKDescriptorSetFrame> mCurrentFramePool;
		AutoRef<VKFence> mFrameFence;
		AutoRef<VKDescriptorSetFrame> Pop();
		void TickRecycle();
	public:
		void Initialize(VKGpuDevice* device);
		
		VKDescriptorSetFrame* GetCurrentFramePool()
		{
			return mCurrentFramePool;
		}
		void BeginFrame();
		
		void EndFrame();
	};
}

NS_END