#include "VKPreHead.h"
#include "../../Core/thread/vfxthread.h"
#include "IVKRenderContext.h"

#define new VNEW

#if PLATFORM_WIN
#pragma comment(lib, "vulkan-1.lib")
#pragma comment(lib, "shaderc_combined.lib") 
#pragma comment(lib, "VkLayer_utils.lib") 
#else
#endif


NS_BEGIN

uint32_t VK_FindMemoryType(IVKRenderContext* rc, uint32_t typeFilter, VkMemoryPropertyFlags properties)
{
	VkPhysicalDeviceMemoryProperties memProperties;
	vkGetPhysicalDeviceMemoryProperties(rc->mPhysicalDevice, &memProperties);

	for (uint32_t i = 0; i < memProperties.memoryTypeCount; i++)
	{
		if ((typeFilter & (1 << i)) && (memProperties.memoryTypes[i].propertyFlags & properties) == properties)
		{
			return i;
		}
	}

	return 0xFFFFFFFF;
}

bool VK_CreateBuffer(IVKRenderContext* rc, VkDeviceSize size, VkBufferUsageFlags usage, VkMemoryPropertyFlags properties, VkBuffer& buffer, VkDeviceMemory& bufferMemory)
{
	VkBufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.size = size;
	bufferInfo.usage = usage;
	bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

	if (vkCreateBuffer(rc->mLogicalDevice, &bufferInfo, nullptr, &buffer) != VK_SUCCESS) {
		return false;
	}

	VkMemoryRequirements memRequirements;
	vkGetBufferMemoryRequirements(rc->mLogicalDevice, buffer, &memRequirements);

	VkMemoryAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
	allocInfo.allocationSize = memRequirements.size;
	allocInfo.memoryTypeIndex = VK_FindMemoryType(rc, memRequirements.memoryTypeBits, properties);

	if (vkAllocateMemory(rc->mLogicalDevice, &allocInfo, nullptr, &bufferMemory) != VK_SUCCESS) {
		return false;
	}

	vkBindBufferMemory(rc->mLogicalDevice, buffer, bufferMemory, 0);
	return true;
}

VkCommandBuffer beginSingleTimeCommands() {
	return nullptr;
	/*VkCommandBufferAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
	allocInfo.commandPool = commandPool;
	allocInfo.commandBufferCount = 1;

	VkCommandBuffer commandBuffer;
	vkAllocateCommandBuffers(device, &allocInfo, &commandBuffer);

	VkCommandBufferBeginInfo beginInfo = {};
	beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;

	vkBeginCommandBuffer(commandBuffer, &beginInfo);

	return commandBuffer;*/
}

void endSingleTimeCommands(VkCommandBuffer commandBuffer) {
	/*vkEndCommandBuffer(commandBuffer);

	VkSubmitInfo submitInfo = {};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.commandBufferCount = 1;
	submitInfo.pCommandBuffers = &commandBuffer;

	vkQueueSubmit(graphicsQueue, 1, &submitInfo, VK_NULL_HANDLE);
	vkQueueWaitIdle(graphicsQueue);

	vkFreeCommandBuffers(device, commandPool, 1, &commandBuffer);*/
}

void VK_CopyBuffer(IVKRenderContext* rc, VkBuffer srcBuffer, VkBuffer dstBuffer, VkDeviceSize size) {
	VkCommandBuffer commandBuffer = beginSingleTimeCommands();

	VkBufferCopy copyRegion = {};
	copyRegion.size = size;
	vkCmdCopyBuffer(commandBuffer, srcBuffer, dstBuffer, 1, &copyRegion);

	endSingleTimeCommands(commandBuffer);
}

NS_END

using namespace EngineNS;

extern "C"
{
	
}