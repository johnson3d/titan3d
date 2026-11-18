#define VMA_IMPLEMENTATION 

#include "VKPreHead.h"
#include "VKEvent.h"
#include "VKGpuDevice.h"

#define new VNEW

#if PLATFORM_WIN
#pragma comment(lib, "vulkan-1.lib")
//#pragma comment(lib, "shaderc_combined.lib") 
//#pragma comment(lib, "VkLayer_utils.lib") 
#else
#endif

NS_BEGIN

namespace NxRHI
{
	/// <summary>
	/// 
	/// </summary>
	FVKDefaultGpuMemory::~FVKDefaultGpuMemory()
	{
		Safe_Release(GpuHeap);
	}
	void FVKDefaultGpuMemory::FreeMemory()
	{
		auto vkHeap = (VKGpuHeap*)GpuHeap;
		if (vkHeap && vkHeap->Memory != nullptr)
		{
			vkFreeMemory(mDevice->mDevice, vkHeap->Memory, mDevice->GetVkAllocCallBacks());
			vkHeap->Memory = nullptr;
		}
	}
	
	AutoRef<FGpuMemory> VKGpuDefaultMemAllocator::Alloc(IGpuDevice* device1, UINT typeIndex, UINT64 size, const char* name)
	{
		auto device = (VKGpuDevice*)device1;
		auto result = MakeWeakRef(new FVKDefaultGpuMemory());
		result->Offset = 0;
		result->mDevice = device;
		
		VKGpuHeap* heap = new VKGpuHeap();
		
		VkMemoryAllocateInfo ainfo{};
		ainfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
		ainfo.memoryTypeIndex = typeIndex;
		ainfo.allocationSize = size;
		VkDeviceMemory mem;
		if (vkAllocateMemory(device->mDevice, &ainfo, device->GetVkAllocCallBacks(), &mem) != VK_SUCCESS)
		{
			ASSERT(false);
			return nullptr;
		}
		heap->Memory = mem;

		result->GpuHeap = heap;
		return result;
	}

	AutoRef<FGpuMemory> VKGpuDefaultMemAllocator::Alloc(IGpuDevice* device1, UINT64 size, const char* name)
	{
		return Alloc(device1, mMemTypeIndex, size, name);
	}
	
	void VKGpuDefaultMemAllocator::Free(FGpuMemory* memory)
	{
		memory->GpuHeap->Release();
		memory->GpuHeap = nullptr;
		memory->Offset = -1;
	}
	
	IGpuHeap* VKGpuPooledMemAllocator::CreateGpuHeap(IGpuDevice* device1, UINT64 size, UINT count, const char* name)
	{
		auto device = (VKGpuDevice*)device1;
		auto result = new VKGpuHeap();
		
		VkMemoryAllocateInfo ainfo{};
		ainfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
		ainfo.memoryTypeIndex = mMemTypeIndex;
		ainfo.allocationSize = size * count;
		VkDeviceMemory mem;
		if (vkAllocateMemory(device->mDevice, &ainfo, device->GetVkAllocCallBacks(), &mem) != VK_SUCCESS)
		{
			ASSERT(false);
			return nullptr;
		}
		result->Memory = mem;

		return result;
	}

	IGpuHeap* VKGpuLinearMemAllocator::CreateGpuHeap(IGpuDevice* device1, UINT64 size, const char* name)
	{
		auto device = (VKGpuDevice*)device1;
		auto result = new VKGpuHeap();

		VkMemoryAllocateInfo ainfo{};
		ainfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
		ainfo.memoryTypeIndex = mMemTypeIndex;
		ainfo.allocationSize = size;
		VkDeviceMemory mem;
		if (vkAllocateMemory(device->mDevice, &ainfo, device->GetVkAllocCallBacks(), &mem) != VK_SUCCESS)
		{
			ASSERT(false);
			return nullptr;
		}
		result->Memory = mem;

		return result;
	}

	/// VKStagingMemAllocator-----------------------------------------------------------
	VKStagingPage::~VKStagingPage()
	{
		auto device = mDeviceRef.GetPtr();
		// 设备已销毁时 VMA 已整体释放, 不能再调用 vmaDestroyBuffer
		if (device != nullptr && device->mVmaAllocator != nullptr && Buffer != nullptr)
		{
			vmaDestroyBuffer(device->mVmaAllocator, Buffer, Allocation);
		}
		Buffer = nullptr;
		Allocation = nullptr;
		pMapped = nullptr;
	}
	FVKStagingChunk::~FVKStagingChunk()
	{
		if (Page == nullptr)
			return;
		auto allocator = HostAllocator.GetPtr();
		if (allocator != nullptr)
		{
			allocator->Free(this);
		}
		// 页已被 TickShrink 从池中移除时, 这里是最后一个引用, VkBuffer 随之释放
		Page = nullptr;
	}
	void FVKStagingChunk::FlushWrite(UINT64 size)
	{
		if (Page->IsCoherent)
			return;
		auto device = Page->mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		// vmaFlushAllocation 内部会按 nonCoherentAtomSize 对齐范围
		vmaFlushAllocation(device->mVmaAllocator, Page->Allocation, Offset, size);
	}
	VKStagingPage* VKStagingMemAllocator::CreatePage(VKGpuDevice* device, UINT64 chunkSize)
	{
		auto pageSize = chunkSize * ChunksPerPage;
		if (pageSize < MinPageSize)
			pageSize = MinPageSize;
		if (pageSize > MaxPageSize)
			pageSize = MaxPageSize;
		if (pageSize < chunkSize)
			pageSize = chunkSize;

		VkBufferCreateInfo bufferInfo{};
		bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
		bufferInfo.size = pageSize;
		bufferInfo.usage = VK_BUFFER_USAGE_TRANSFER_SRC_BIT;
		bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

		VmaAllocationCreateInfo vmaInfo{};
		vmaInfo.usage = VMA_MEMORY_USAGE_CPU_TO_GPU;
		vmaInfo.flags = VMA_ALLOCATION_CREATE_MAPPED_BIT;//常驻映射

		auto page = new VKStagingPage();
		page->mDeviceRef.FromObject(device);
		VmaAllocationInfo allocInfo{};
		if (vmaCreateBuffer(device->mVmaAllocator, &bufferInfo, &vmaInfo, &page->Buffer, &page->Allocation, &allocInfo) != VK_SUCCESS
			|| allocInfo.pMappedData == nullptr)
		{
			page->Release();
			return nullptr;
		}
		page->pMapped = (BYTE*)allocInfo.pMappedData;
		VkMemoryPropertyFlags memFlags = 0;
		vmaGetMemoryTypeProperties(device->mVmaAllocator, allocInfo.memoryType, &memFlags);
		page->IsCoherent = (memFlags & VK_MEMORY_PROPERTY_HOST_COHERENT_BIT) != 0;
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_BUFFER, (void*)page->Buffer, "StagingPool");

		page->ChunkSize = chunkSize;
		page->ChunkCount = (UINT)(pageSize / chunkSize);
		page->FreeChunks.resize(page->ChunkCount);
		for (UINT i = 0; i < page->ChunkCount; i++)
		{//倒序压入, pop_back 从低地址取
			page->FreeChunks[i] = page->ChunkCount - 1 - i;
		}
		page->IdleSinceTick = TickIndex;
		TotalPageSize += pageSize;
		return page;
	}
	AutoRef<FVKStagingChunk> VKStagingMemAllocator::Alloc(VKGpuDevice* device, UINT64 size)
	{
		if (size == 0 || size > MaxChunkSize)
			return nullptr;
		UINT sizeClass = 0;
		UINT64 chunkSize = MinChunkSize;
		while (chunkSize < size)
		{
			chunkSize <<= 1;
			sizeClass++;
		}

		VAutoVSLLock lk(mLocker);
		auto& pages = SizeClasses[sizeClass];
		VKStagingPage* page = nullptr;
		for (auto i = pages.rbegin(); i != pages.rend(); i++)
		{
			if ((*i)->FreeChunks.size() > 0)
			{
				page = (*i).GetPtr();
				break;
			}
		}
		if (page == nullptr)
		{
			page = CreatePage(device, chunkSize);
			if (page == nullptr)
				return nullptr;
			pages.push_back(MakeWeakRef(page));
		}

		auto result = MakeWeakRef(new FVKStagingChunk());
		result->HostAllocator.FromObject(this);
		result->Page = page;
		result->ChunkIndex = page->FreeChunks.back();
		page->FreeChunks.pop_back();
		result->Offset = result->ChunkIndex * chunkSize;
		result->Size = chunkSize;
		return result;
	}
	void VKStagingMemAllocator::Free(FVKStagingChunk* chunk)
	{
		VAutoVSLLock lk(mLocker);
		auto page = chunk->Page.GetPtr();
		ASSERT(page->FreeChunks.size() < page->ChunkCount);
		page->FreeChunks.push_back(chunk->ChunkIndex);
		if (page->IsIdle())
			page->IdleSinceTick = TickIndex;
	}
	void VKStagingMemAllocator::TickShrink()
	{
		std::vector<AutoRef<VKStagingPage>> released;//出锁后再释放资源
		{
			VAutoVSLLock lk(mLocker);
			TickIndex++;

			for (auto& pages : SizeClasses)
			{
				for (auto i = pages.begin(); i != pages.end(); )
				{
					if ((*i)->IsIdle() && TickIndex - (*i)->IdleSinceTick >= IdleReleaseTicks)
					{
						TotalPageSize -= (*i)->GetPageSize();
						released.push_back(*i);
						i = pages.erase(i);
					}
					else
					{
						i++;
					}
				}
			}

			while (TotalPageSize > BudgetSize)
			{//超预算: 按闲置时长从久到新回收空页, 在用的页不动
				std::vector<AutoRef<VKStagingPage>>* oldestPages = nullptr;
				size_t oldestIndex = 0;
				for (auto& pages : SizeClasses)
				{
					for (size_t i = 0; i < pages.size(); i++)
					{
						if (pages[i]->IsIdle() == false)
							continue;
						if (oldestPages == nullptr || pages[i]->IdleSinceTick < (*oldestPages)[oldestIndex]->IdleSinceTick)
						{
							oldestPages = &pages;
							oldestIndex = i;
						}
					}
				}
				if (oldestPages == nullptr)
					break;
				TotalPageSize -= (*oldestPages)[oldestIndex]->GetPageSize();
				released.push_back((*oldestPages)[oldestIndex]);
				oldestPages->erase(oldestPages->begin() + oldestIndex);
			}
		}
		released.clear();
	}
}

NS_END