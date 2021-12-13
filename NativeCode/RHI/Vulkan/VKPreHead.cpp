#include "VKPreHead.h"
#include "IVKRenderContext.h"

#define new VNEW

#if PLATFORM_WIN
#pragma comment(lib, "vulkan-1.lib")
//#pragma comment(lib, "shaderc_combined.lib") 
//#pragma comment(lib, "VkLayer_utils.lib") 
#else
#endif


NS_BEGIN

std::vector<std::pair<int, std::function<FVkExecute>>>	PostVkExecutes;
VSLLock VkExeLocker;

void PostVkExecute(const std::function<FVkExecute>& exec, int delayFrame)
{
	VAutoVSLLock locker(VkExeLocker);
	PostVkExecutes.push_back(std::make_pair(delayFrame,exec));
}

void VkExecuteAll(IVKRenderContext* rc, bool force)
{
	std::vector<std::function<FVkExecute>>	CopyVkExecutes;
	{
		VAutoVSLLock locker(VkExeLocker);
		for (auto i = PostVkExecutes.begin(); i != PostVkExecutes.end(); )
		{
			auto& cur = *i;
			if (cur.first == 0 || force)
			{
				CopyVkExecutes.push_back(cur.second);
				i = PostVkExecutes.erase(i);
			}
			else
			{
				cur.first--;
				i++;
			}
		}
	}
	for (auto i = CopyVkExecutes.begin(); i != CopyVkExecutes.end(); i++)
	{
		auto& cur = *i;
		cur(rc);
	}
	CopyVkExecutes.clear();
}

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

bool VKGpuMemoryType::Init(IVKRenderContext* rc, UINT typeIndex, UINT64 cellSize)
{
	TypeIndex = typeIndex;
	if (cellSize <= GpuMemoryBlockSize)
	{
		CellSize = cellSize;
		CellCount = (UINT)(GpuMemoryBlockSize / cellSize);
	}
	else
	{
		CellSize = cellSize;
		CellCount = 1;
	}
	
	return true;
}

VKGpuMemory* VKGpuMemoryType::Alloc(IVKRenderContext* rc)
{
	MemCount++;
	if (FreePoint != nullptr)
	{
		auto result = FreePoint;
		FreePoint = FreePoint->Next;
		return result;
	}

	VkMemoryAllocateInfo ainfo{};
	ainfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
	ainfo.memoryTypeIndex = TypeIndex;
	ainfo.allocationSize = CellSize * CellCount;
	VkDeviceMemory mem;
	if (vkAllocateMemory(rc->mLogicalDevice, &ainfo, rc->GetVkAllocCallBacks(), &mem) != VK_SUCCESS)
	{
		ASSERT(false);
		return nullptr;
	}
	auto pNewBatch = new VkMemoryBatch();
	pNewBatch->Memory = mem;
	pNewBatch->MemoryType = this;
	MemoryBatches.push_back(pNewBatch);

	auto& newCells = pNewBatch->Cells;
	newCells.resize(CellCount);	
	for (UINT i = 0; i < CellCount; i++)
	{
		newCells[i].HostBatch = pNewBatch;
		newCells[i].Offset = i * CellSize;
		newCells[i].Size = 0;
		if (i < CellCount - 1)
		{
			newCells[i].Next = &newCells[i + 1];
		}
		else
		{
			newCells[i].Next = nullptr;
		}
	}
	FreePoint = &newCells[0];

	auto result = FreePoint;
	FreePoint = FreePoint->Next;
	return result;
}

class IVKGpuMemoryManager
{
	std::atomic<UINT>	MemCount;

	VSLLock mLocker;
	struct VKTypedMemory
	{
		std::map<UINT64, VKGpuMemoryType*>	SizedBlocks;
	};
	std::map<UINT, VKTypedMemory*>			TypedMems;
	VKGpuMemoryType* GetMemoryType(IVKRenderContext* rc, UINT typeIndex, UINT64 size)
	{
		VKTypedMemory* pTypedMemory = nullptr;
		auto typeIter = TypedMems.find(typeIndex);
		if (typeIter == TypedMems.end())
		{
			pTypedMemory = new VKTypedMemory();
			TypedMems.insert(std::make_pair(typeIndex, pTypedMemory));
		}
		else
		{
			pTypedMemory = typeIter->second;
		}
		auto blockIter = pTypedMemory->SizedBlocks.find(size);
		if (blockIter != pTypedMemory->SizedBlocks.end())
		{
			return blockIter->second;
		}
		auto pResult = new VKGpuMemoryType();
		pResult->Init(rc, typeIndex, size);
		pTypedMemory->SizedBlocks.insert(std::make_pair(size, pResult));
		return pResult;
	}
public:
	static IVKGpuMemoryManager* GetInstance() {
		static IVKGpuMemoryManager obj;
		return &obj;
	}
	IVKGpuMemoryManager()
	{
		MemCount = 0;
	}
	VKGpuMemory* AllocMemory(IVKRenderContext* rc, const VkMemoryAllocateInfo* info)
	{
		VAutoVSLLock locker(mLocker);
		auto pType = GetMemoryType(rc, info->memoryTypeIndex, info->allocationSize);
		auto result = pType->Alloc(rc);
		result->Next = nullptr;
		MemCount++;
		return result;
	}
	void FreeMemory(IVKRenderContext* rc, VKGpuMemory* memory)
	{
		VAutoVSLLock locker(mLocker);
		memory->HostBatch->MemoryType->MemCount--;
		memory->Next = memory->HostBatch->MemoryType->FreePoint;
		memory->HostBatch->MemoryType->FreePoint = memory;
		MemCount--;
	}
};

VKGpuMemory* VK_AllocGpuMemory(IVKRenderContext* rc, const VkMemoryAllocateInfo* info)
{
	//return IVKGpuMemoryManager::GetInstance()->AllocGpuMemory(rc, info);
	return IVKGpuMemoryManager::GetInstance()->AllocMemory(rc, info);
}

void VK_FreeGpuMemory(IVKRenderContext* rc, VKGpuMemory* memory)
{
	//return IVKGpuMemoryManager::GetInstance()->FreeGpuMemory(rc, memory);
	IVKGpuMemoryManager::GetInstance()->FreeMemory(rc, memory);
}

bool VK_CreateBuffer(IVKRenderContext* rc, VkDeviceSize size, VkBufferUsageFlags usage, VkMemoryPropertyFlags properties, VkBuffer& buffer, VKGpuMemory*& bufferMemory)
{
	VkBufferCreateInfo bufferInfo = {};
	bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
	bufferInfo.size = size;
	bufferInfo.usage = usage;
	bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

	if (vkCreateBuffer(rc->mLogicalDevice, &bufferInfo, rc->GetVkAllocCallBacks(), &buffer) != VK_SUCCESS) {
		return false;
	}

	VkMemoryRequirements memRequirements;
	vkGetBufferMemoryRequirements(rc->mLogicalDevice, buffer, &memRequirements);

	VkMemoryAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;

	auto memSize = ((memRequirements.size + memRequirements.alignment - 1) / memRequirements.alignment) * memRequirements.alignment;
	allocInfo.allocationSize = memSize;
	allocInfo.memoryTypeIndex = VK_FindMemoryType(rc, memRequirements.memoryTypeBits, properties);

	bufferMemory = VK_AllocGpuMemory(rc, &allocInfo);
	if (bufferMemory == nullptr) 
	{
		return false;
	}

	vkBindBufferMemory(rc->mLogicalDevice, buffer, bufferMemory->GetDeviceMemory(), bufferMemory->Offset);
	return true;
}

VSLLock gVKSingleTimeLocker;
VkCommandBuffer VK_BeginSingleTimeCommands(IVKRenderContext* rc) 
{
	gVKSingleTimeLocker.Lock();

	VkCommandBufferAllocateInfo allocInfo = {};
	allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
	allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
	allocInfo.commandPool = rc->mCommandPoolForSingleTime;
	allocInfo.commandBufferCount = 1;

	VkCommandBuffer commandBuffer;
	vkAllocateCommandBuffers(rc->mLogicalDevice, &allocInfo, &commandBuffer);

	VkCommandBufferBeginInfo beginInfo = {};
	beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
	beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;

	vkBeginCommandBuffer(commandBuffer, &beginInfo);

	return commandBuffer;
}

void VK_EndSingleTimeCommands(IVKRenderContext* rc, VkCommandBuffer commandBuffer) 
{
	vkEndCommandBuffer(commandBuffer);

	VkSubmitInfo submitInfo = {};
	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
	submitInfo.commandBufferCount = 1;
	submitInfo.pCommandBuffers = &commandBuffer;

	vkQueueSubmit(rc->mGraphicsQueueForSingleTime, 1, &submitInfo, VK_NULL_HANDLE);
	vkQueueWaitIdle(rc->mGraphicsQueueForSingleTime);

	vkFreeCommandBuffers(rc->mLogicalDevice, rc->mCommandPoolForSingleTime, 1, &commandBuffer);
	
	gVKSingleTimeLocker.Unlock();
}

void VK_CopyBuffer(IVKRenderContext* rc, VkBuffer srcBuffer, VkBuffer dstBuffer, VkDeviceSize size, VkCommandBuffer commandBuffer)
{
	//VkCommandBuffer commandBuffer = VK_BeginSingleTimeCommands(rc);

	VkBufferCopy copyRegion = {};
	copyRegion.size = size;
	vkCmdCopyBuffer(commandBuffer, srcBuffer, dstBuffer, 1, &copyRegion);

	//VK_EndSingleTimeCommands(rc, commandBuffer);
}

void VK_CopyBufferToImage(IVKRenderContext* rc, VkBuffer buffer, VkImage image, UINT mipLevel, uint32_t width, uint32_t height, VkCommandBuffer commandBuffer)
{
	//VkCommandBuffer commandBuffer = VK_BeginSingleTimeCommands(rc);

	VkBufferImageCopy region{};
	region.bufferOffset = 0;
	region.bufferRowLength = 0;
	region.bufferImageHeight = 0;
	region.imageSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	region.imageSubresource.mipLevel = mipLevel;
	region.imageSubresource.baseArrayLayer = 0;
	region.imageSubresource.layerCount = 1;
	region.imageOffset = { 0, 0, 0 };
	region.imageExtent = {
		width,
		height,
		1
	};

	vkCmdCopyBufferToImage(commandBuffer, buffer, image, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &region);

	//VK_EndSingleTimeCommands(rc, commandBuffer);
}

NS_END
