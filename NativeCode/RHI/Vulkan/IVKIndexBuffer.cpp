#include "IVKIndexBuffer.h"
#include "IVKRenderContext.h"

#define new VNEW

NS_BEGIN

IVKIndexBuffer::IVKIndexBuffer()
{
}


IVKIndexBuffer::~IVKIndexBuffer()
{
}

void IVKIndexBuffer::GetBufferData(IRenderContext* rc, IBlobObject* data)
{

}

void IVKIndexBuffer::UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size)
{
	/*auto rc = (IVKRenderContext*)rc1;

	VkBuffer							mStagingBuffer;
	VkDeviceMemory						mStagingBufferMemory;

	VK_CreateBuffer(rc, size, VK_BUFFER_USAGE_TRANSFER_SRC_BIT, VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, mStagingBuffer, mStagingBufferMemory);

	void* data;
	vkMapMemory(rc->mLogicalDevice, mStagingBufferMemory, 0, size, 0, &data);
	memcpy(data, ptr, (size_t)size);
	vkUnmapMemory(rc->mLogicalDevice, mStagingBufferMemory);

	VK_CopyBuffer(rc, mStagingBuffer, mBuffer, size);

	if (mStagingBuffer != nullptr)
	{
		vkDestroyBuffer(rc->mLogicalDevice, mStagingBuffer, nullptr);
		mStagingBuffer = nullptr;
	}
	if (mStagingBufferMemory != nullptr)
	{
		vkFreeMemory(rc->mLogicalDevice, mStagingBufferMemory, nullptr);
		mStagingBufferMemory = nullptr;
	}*/
}

bool IVKIndexBuffer::Init(IVKRenderContext* rc, const IIndexBufferDesc* desc)
{
	mDesc = *desc;
	VkDeviceSize bufferSize = desc->ByteWidth;

	VK_CreateBuffer(rc, bufferSize, VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDEX_BUFFER_BIT, VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT, mBuffer, mMemory);

	if (desc->InitData != nullptr)
	{
		UpdateGPUBuffData(rc->GetImmCommandList(), desc->InitData, desc->ByteWidth);
	}

	return true;
}

NS_END