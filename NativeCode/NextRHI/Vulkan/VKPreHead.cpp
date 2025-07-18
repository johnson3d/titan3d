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
}

NS_END