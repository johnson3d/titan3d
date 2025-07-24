#include "VKDescriptorSet.h"
#include "VKGpuDevice.h"
#include "VKShader.h"

NS_BEGIN

namespace NxRHI
{
	void VkDescriptorPoolWrapper::Initialize(VKGpuDevice* device)
	{
		mDeviceRef = device;

		std::vector<VkDescriptorPoolSize> psz;

		VkDescriptorPoolSize tmp;
		tmp.type = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
		tmp.descriptorCount = NumOfUniformBuffer;
		psz.push_back(tmp);
		tmp.type = VK_DESCRIPTOR_TYPE_SAMPLER;
		tmp.descriptorCount = NumOfSampler;
		psz.push_back(tmp);
		tmp.type = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
		tmp.descriptorCount = NumOfSampledImage;
		psz.push_back(tmp);
		tmp.type = VK_DESCRIPTOR_TYPE_STORAGE_IMAGE;
		tmp.descriptorCount = NumOfStorageImage;
		psz.push_back(tmp);
		tmp.type = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
		tmp.descriptorCount = NumOfStorageBuffer;
		psz.push_back(tmp);
		VkDescriptorPoolCreateInfo poolInfo = {};
		poolInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
		poolInfo.poolSizeCount = (UINT)psz.size();
		poolInfo.pPoolSizes = psz.data();

		poolInfo.maxSets = MaxOfDesriptor;

		if (vkCreateDescriptorPool(device->mDevice, &poolInfo, device->GetVkAllocCallBacks(), &mDescriptorPool) != VK_SUCCESS)
		{
			return;
		}
	}
	VkDescriptorSet VkDescriptorPoolWrapper::Alloc(VkDescriptorSetLayout layout)
	{
		VkDescriptorSetAllocateInfo allocInfo{};
		allocInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
		allocInfo.descriptorPool = mDescriptorPool;
		allocInfo.descriptorSetCount = 1;
		allocInfo.pSetLayouts = &layout;

		VkDescriptorSet ds;
		if (vkAllocateDescriptorSets(mDeviceRef->mDevice, &allocInfo, &ds) != VK_SUCCESS)
		{
			return nullptr;
		}
		return ds;
	}

	void VKDescriptorPool::Initialize(VKGpuDevice* device, VKDesriptorPoolManager* manager)
	{
		mDeviceRef = device;
		mManagerRef = manager;
	}
	void VKDescriptorPool::Reset()
	{
		for (auto& pool : mDescriptorPools)
		{
			vkResetDescriptorPool(mDeviceRef->mDevice, pool->mDescriptorPool, 0);
		}
		mCurrentFreePoolIndex = 0;
		mFrameFenceValue = UINT64_MAX;
	}
	int VKDescriptorPool::GetCurrentPoolIndex()
	{
		size_t curIndex = mCurrentFreePoolIndex;
		auto& curPools = mDescriptorPools;
		if (curPools.size() <= curIndex)
		{
			auto num = curIndex + 1 - curPools.size();
			for (size_t i = 0; i < num; i++)
			{
				VkDescriptorPoolWrapper* newPool = new VkDescriptorPoolWrapper();
				newPool->Initialize(mDeviceRef); // Assuming 1024 is the page size
				curPools.push_back(MakeWeakRef(newPool));
			}
		}
		return (int)curIndex;
	}
	int VKDescriptorPool::IncreaseCurrentPoolIndex()
	{
		mCurrentFreePoolIndex++;
		return GetCurrentPoolIndex();
	}
	VkDescriptorSet VKDescriptorPool::AllocDescriptorSet(VkDescriptorSetLayout layout)
	{
		size_t curIndex = GetCurrentPoolIndex();
		auto& curPools = mDescriptorPools;
		auto result = curPools[curIndex]->Alloc(layout);
		if (result != nullptr)
			return result;

		curIndex = IncreaseCurrentPoolIndex();
		return curPools[curIndex]->Alloc(layout);
	}
	void VKDesriptorPoolManager::Initialize(IGpuDevice* device)
	{
		mDeviceRef = (VKGpuDevice*)device;
		NxDesriptorPoolManager::Initialize(device);
	}
	IDescriptorPool* VKDesriptorPoolManager::CreateDescriptorPool()
	{
		auto result = new VKDescriptorPool();
		result->Initialize(mDeviceRef, this);
		return result;
	}
}

NS_END