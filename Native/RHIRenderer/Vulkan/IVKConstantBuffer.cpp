#include "IVKConstantBuffer.h"
#include "IVKRenderContext.h"

#define new VNEW

NS_BEGIN

IVKConstantBuffer::IVKConstantBuffer()
{
	mBuffer = nullptr;
	mMemory = nullptr;
	mLayout = nullptr;
}

IVKConstantBuffer::~IVKConstantBuffer()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mBuffer != nullptr)
	{
		vkDestroyBuffer(rc->mLogicalDevice, mBuffer, nullptr);
		mBuffer = nullptr;
	}
	if (mMemory != nullptr)
	{
		vkFreeMemory(rc->mLogicalDevice, mMemory, nullptr);
		mMemory = nullptr;
	}
	if (mLayout != nullptr)
	{
		vkDestroyDescriptorSetLayout(rc->mLogicalDevice, mLayout, nullptr);
		mLayout = nullptr;
	}
}

bool IVKConstantBuffer::UpdateContent(ICommandList* cmd, void* pBuffer, UINT Size)
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return false;
	ASSERT(Size <= Desc.Size);
	void* data;
	if (vkMapMemory(rc->mLogicalDevice, mMemory, 0, Desc.Size, 0, &data)!= VK_SUCCESS)
	{
		return false;
	}
	memcpy(data, pBuffer, Size);
	vkUnmapMemory(rc->mLogicalDevice, mMemory);
	return true;
}

bool IVKConstantBuffer::Init(IVKRenderContext* rc, const IConstantBufferDesc* desc)
{
	Desc = *desc;

	VkDescriptorSetLayoutBinding uboLayoutBinding = {};
	uboLayoutBinding.binding = desc->VSBindPoint;
	uboLayoutBinding.descriptorCount = 1;
	uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
	uboLayoutBinding.pImmutableSamplers = nullptr;
	uboLayoutBinding.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;

	VkDescriptorSetLayoutCreateInfo layoutInfo = {};
	layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
	layoutInfo.bindingCount = 1;
	layoutInfo.pBindings = &uboLayoutBinding;

	if (vkCreateDescriptorSetLayout(rc->mLogicalDevice, &layoutInfo, nullptr, &mLayout) != VK_SUCCESS) {
		return  false;
	}

	return VK_CreateBuffer(rc, desc->Size, VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT, VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, mBuffer, mMemory);
}

NS_END