#pragma once
#include "../NxDescriptorSet.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKFence;

	class VkDescriptorPoolWrapper : public IGpuResource
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

	class VKGpuDevice;
	class VKDesriptorPoolManager;
	class VKDescriptorPool : public IDescriptorPool
	{
	private:
		VKGpuDevice* mDeviceRef;
		VKDesriptorPoolManager* mManagerRef;
		std::vector<AutoRef<VkDescriptorPoolWrapper>> mDescriptorPools;
		int mCurrentFreePoolIndex{};
		int GetCurrentPoolIndex();
		int IncreaseCurrentPoolIndex();
	public:
		void Initialize(VKGpuDevice* device, VKDesriptorPoolManager* manager);
		VkDescriptorSet AllocDescriptorSet(VkDescriptorSetLayout layout);
		virtual void Reset() override;
	};

	class VKDesriptorPoolManager : public NxDesriptorPoolManager
	{
		VKGpuDevice* mDeviceRef;
	public:
		virtual void Initialize(IGpuDevice* device) override;
		virtual IDescriptorPool* CreateDescriptorPool() override;
	};
}

NS_END