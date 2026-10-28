#include "../../Base/debug/vfxdebug.h"

#pragma push_macro("interface")
#undef interface
#define LEANCLR_STATIC
#include "alloc/general_allocation.h"
#pragma pop_macro("interface")

#include <cstdint>
#include <cstring>
#include <algorithm>

// LeanCLR 的 GeneralAllocation(含 MemPool、GC arena、HashMap/HashSet 节点)统一走引擎分配器，
// 并打上 c_vfxMemoryNoLeakReport：VM 没有真正的 shutdown，这些内存是进程级常驻的，不该进引擎泄露报告。
//
// 回调必须在 LeanCLR 任何静态对象构造之前装好：大量全局 HashMap 在默认构造时就会分配，
// 若先用默认 std::malloc 分配、换回调后再走引擎 free 释放会直接破坏堆。
// init_seg(lib) 让本 TU 的动态初始化排在用户代码(.CRT$XCU)之前；仅 MSVC，LeanCLR 目前只在 Windows 编译。
#pragma warning(disable : 4073)
#pragma init_seg(lib)

namespace
{
	// 每块前置头：记录引擎分配的原始地址与用户请求大小，供 free/realloc 使用
	struct FLeanClrBlockHeader
	{
		void* RawPtr;
		size_t Size;
	};

	constexpr size_t c_LeanClrDefaultAlignment = 16;
	static_assert(sizeof(FLeanClrBlockHeader) <= c_LeanClrDefaultAlignment, "header must fit in default alignment");

	inline FLeanClrBlockHeader* GetBlockHeader(void* ptr)
	{
		return reinterpret_cast<FLeanClrBlockHeader*>(ptr) - 1;
	}

	void* LeanClrAlignedMalloc(size_t size, size_t alignment)
	{
		if (alignment < c_LeanClrDefaultAlignment)
			alignment = c_LeanClrDefaultAlignment;
		ASSERT((alignment & (alignment - 1)) == 0);

		const size_t overhead = sizeof(FLeanClrBlockHeader) + alignment - 1;
		if (size > SIZE_MAX - overhead)
			return nullptr;

		void* raw = _vfxMemoryNew(size + overhead, c_vfxMemoryNoLeakReport, __LINE__);
		if (raw == nullptr)
			return nullptr;

		const uintptr_t aligned = ((uintptr_t)raw + sizeof(FLeanClrBlockHeader) + alignment - 1) & ~(uintptr_t)(alignment - 1);
		auto header = GetBlockHeader((void*)aligned);
		header->RawPtr = raw;
		header->Size = size;
		return (void*)aligned;
	}

	void LeanClrFree(void* ptr)
	{
		if (ptr == nullptr)
			return;
		_vfxMemoryDelete(GetBlockHeader(ptr)->RawPtr, nullptr, 0);
	}

	void* LeanClrAlignedRealloc(void* ptr, size_t size, size_t alignment)
	{
		if (ptr == nullptr)
			return LeanClrAlignedMalloc(size, alignment);
		if (size == 0)
		{
			LeanClrFree(ptr);
			return nullptr;
		}

		void* newPtr = LeanClrAlignedMalloc(size, alignment);
		if (newPtr == nullptr)
			return nullptr;
		memcpy(newPtr, ptr, (std::min)(GetBlockHeader(ptr)->Size, size));
		LeanClrFree(ptr);
		return newPtr;
	}

	void* LeanClrMalloc(size_t size)
	{
		return LeanClrAlignedMalloc(size, c_LeanClrDefaultAlignment);
	}

	void* LeanClrCalloc(size_t nmemb, size_t size)
	{
		if (size != 0 && nmemb > SIZE_MAX / size)
			return nullptr;
		const size_t total = nmemb * size;
		void* ptr = LeanClrMalloc(total);
		if (ptr != nullptr)
			memset(ptr, 0, total);
		return ptr;
	}

	void* LeanClrRealloc(void* ptr, size_t size)
	{
		return LeanClrAlignedRealloc(ptr, size, c_LeanClrDefaultAlignment);
	}

	struct FLeanClrMemoryInstaller
	{
		FLeanClrMemoryInstaller()
		{
			leanclr::alloc::MemoryCallbacks callbacks = {};
			callbacks.malloc = &LeanClrMalloc;
			callbacks.aligned_malloc = &LeanClrAlignedMalloc;
			callbacks.free = &LeanClrFree;
			callbacks.aligned_free = &LeanClrFree;
			callbacks.calloc = &LeanClrCalloc;
			callbacks.realloc = &LeanClrRealloc;
			callbacks.aligned_realloc = &LeanClrAlignedRealloc;
			leanclr::alloc::GeneralAllocation::set_memory_callbacks(callbacks);
		}
	};

	FLeanClrMemoryInstaller GLeanClrMemoryInstaller;
}
