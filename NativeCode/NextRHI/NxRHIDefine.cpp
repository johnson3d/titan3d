#include "NxRHIDefine.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void FPooledGpuMemory::FreeMemory()
	{
		auto pool = HostPool.GetPtr();
		if (pool != nullptr)
		{
			auto allocator = pool->HostAllocator.GetPtr();
			if (allocator != nullptr)
			{
				allocator->Free(this);
			}
		}
	}
	IGpuPooledMemAllocator::~IGpuPooledMemAllocator()
	{
		VAutoVSLLock lk(mLocker);
		Pools.clear();
		mGpuHeaps.clear();
	}
	AutoRef<FGpuMemory> IGpuPooledMemAllocator::Alloc(IGpuDevice* device, UINT64 size)
	{
		VAutoVSLLock lk(mLocker);

		AutoRef<FGpuHeapSizedPool> pool;
		auto iter = Pools.find(size);
		if (iter == Pools.end())
		{
			pool = MakeWeakRef(new FGpuHeapSizedPool());
			pool->HostAllocator.FromObject(this);
			pool->ChunkSize = size;
			Pools.insert(std::make_pair(size, pool));
		}
		else
		{
			pool = iter->second;
		}
		return MakeWeakRef(pool->Alloc(device, this, size));
	}
	void IGpuPooledMemAllocator::Free(FGpuMemory* memory)
	{
		VAutoVSLLock lk(mLocker);
		auto pool = ((FPooledGpuMemory*)memory)->HostPool.GetPtr();
		if (pool == nullptr)
			return;
		pool->Free(memory);
	}
	FGpuHeapSizedPool::~FGpuHeapSizedPool()
	{
		while (FreePoint != nullptr)
		{
			auto save = FreePoint;
			FreePoint = FreePoint->Next;
			save->Release();
		}
	}
	FGpuMemory* FGpuHeapSizedPool::Alloc(IGpuDevice* device, IGpuPooledMemAllocator* allocator, UINT64 size)
	{
		ASSERT(ChunkSize == size);
		UINT BatchCount = allocator->GetBatchCount(ChunkSize);
		if (FreePoint == nullptr)
		{
			auto heap = MakeWeakRef(allocator->CreateGpuHeap(device, ChunkSize, BatchCount));
			allocator->mGpuHeaps.push_back(heap);
			FPooledGpuMemory* first = nullptr;
			FPooledGpuMemory* cur = nullptr;
			FPooledGpuMemory* prev = nullptr;
			UINT64 curOffset = 0;
			for (UINT i = 0; i < BatchCount; i++)
			{
				cur = new FPooledGpuMemory();
				cur->HostPool.FromObject(this);
				cur->GpuHeap = heap;
				cur->Offset = curOffset;
				//cur->Size = size;
				curOffset += ChunkSize;
				if (first == nullptr)
					first = cur;
				if (prev != nullptr)
				{
					prev->Next = cur;
				}
				prev = cur;
			}
			FreePoint = first;
		}
		auto result = FreePoint;
		FreePoint = FreePoint->Next;
		result->Next = nullptr;
		return result;
	}
	void FGpuHeapSizedPool::Free(FGpuMemory* memorybase)
	{
		FPooledGpuMemory* memory = (FPooledGpuMemory*)memorybase;
		ASSERT(this == memory->HostPool.GetPtr());
		memory->Next = FreePoint;
		FreePoint = memory;
		memory->AddRef();
	}
	/// ===================================================================================================================
	void FLinearGpuMemory::FreeMemory()
	{
		auto pool = HostPool.GetPtr();
		if (pool == nullptr)
			return;
		auto allocator = pool->HostAllocator.GetPtr();
		if (allocator == nullptr)
			return;

		allocator->Free(this);
	}
	FLinearGpuMemory* FLinearGpuHeapPool::Alloc(IGpuDevice* device, UINT64 size)
	{
		UINT64 delta = UINT64_MAX;
		size_t slt = -1;
		for (size_t i = 0; i < FreeRanges.size(); i++)
		{
			auto sz = FreeRanges[i].GetSize();
			if (sz < size)
				continue;
			else if (sz == size)
			{
				slt = i;
				break;
			}
			else
			{
				if (sz - size < delta)
				{
					slt = i;
					delta = sz - size;
				}
			}
		}
		if (slt == -1)
		{
			return nullptr;
		}
		if (FreeRanges[slt].GetSize() == size)
		{
			auto result = new FLinearGpuMemory();
			result->HostPool.FromObject(this);
			result->GpuHeap = this->GpuHeap;
			result->AddressRange = FreeRanges[slt];
			result->Offset = result->AddressRange.Begin;
			FreeRanges.erase(FreeRanges.begin() + slt);
			return result;
		}
		else
		{
			auto result = new FLinearGpuMemory();
			result->HostPool.FromObject(this);
			result->GpuHeap = this->GpuHeap;
			result->AddressRange.Begin = FreeRanges[slt].Begin;
			result->AddressRange.End = result->AddressRange.Begin + size;
			result->Offset = result->AddressRange.Begin;
			FreeRanges[slt].Begin += size;
			return result;
		}
		return nullptr;
	}
	void FLinearGpuHeapPool::Free(FLinearGpuMemory* memory)
	{
		size_t mergedAsHead = -1;
		size_t mergedAsTail = -1;
		for (size_t i = 0; i < FreeRanges.size(); i++)
		{
			if (FreeRanges[i].End == memory->AddressRange.Begin)
			{
				mergedAsTail = i;
			}
			if (FreeRanges[i].Begin == memory->AddressRange.End)
			{
				mergedAsHead = i;
			}
			if (mergedAsHead != -1 && mergedAsTail != -1)
				break;
		}
		if (mergedAsHead != -1 && mergedAsTail != -1)
		{
			FreeRanges[mergedAsTail].End = FreeRanges[mergedAsHead].End;
			FreeRanges.erase(FreeRanges.begin() + mergedAsHead);
		}
		else if (mergedAsHead != -1)
		{
			FreeRanges[mergedAsHead].Begin = memory->AddressRange.Begin;
		}
		else if (mergedAsTail != -1)
		{
			FreeRanges[mergedAsTail].End = memory->AddressRange.End;
		}
		else
		{
			FreeRanges.push_back(memory->AddressRange);
		}
	}
	AutoRef<FGpuMemory> IGpuLinearMemAllocator::Alloc(IGpuDevice* device, UINT64 size)
	{
		VAutoVSLLock lk(mLocker);

		for (auto& i : Pools)
		{
			auto result = i->Alloc(device, size);
			if (result!=nullptr)
			{
				return result;
			}
		}
		auto pool = MakeWeakRef(new FLinearGpuHeapPool());
		pool->HostAllocator.FromObject(this);
		pool->GpuHeap = MakeWeakRef(this->CreateGpuHeap(device, PoolSize));
		FAddressRange range;
		range.Begin = 0;
		range.End = PoolSize;
		pool->FreeRanges.push_back(range);
		Pools.push_back(pool);
		return pool->Alloc(device, size);
	}
	void IGpuLinearMemAllocator::Free(FGpuMemory* memory)
	{
		VAutoVSLLock lk(mLocker);
		auto pLinearMem = (FLinearGpuMemory*)memory;
		auto pool = pLinearMem->HostPool.GetPtr();
		if (pool == nullptr)
			return;

		pool->Free(pLinearMem);
		pLinearMem->GpuHeap = nullptr;
	}

	void IGpuLinearMemAllocator::TestAlloc()
	{
		IGpuLinearMemAllocator allocator{};
		int AllocTimes = 100;
		int MaxSize = 256;
		int ReAllocTimes = 20;
		allocator.PoolSize = MaxSize * AllocTimes * 2;
		std::vector<AutoRef<FGpuMemory>> alives;
		for (int i = 0; i < AllocTimes; i++)
		{
			auto size = rand() % MaxSize;
			alives.push_back(allocator.Alloc(nullptr, size));
		}
		while(true)
		{
			for (int i = 0; i < ReAllocTimes; i++)
			{
				auto index = rand() % alives.size();
				alives[index]->FreeMemory();
				alives.erase(alives.begin() + index);
			}

			for (int i = 0; i < ReAllocTimes; i++)
			{
				auto size = rand() % MaxSize;
				alives.push_back(allocator.Alloc(nullptr, size));
			}

			auto poolNum = (UINT)allocator.Pools.size();
			UINT freeRangeNum = 0;
			for (auto& i : allocator.Pools)
			{
				freeRangeNum += (UINT)i->FreeRanges.size();
			}
			VFX_LTRACE(ELTT_Graphics, "%d:%d\r\n", poolNum, freeRangeNum);
		}
	}

	struct IGpuLinearMemAllocator_Test
	{
		IGpuLinearMemAllocator_Test()
		{
			IGpuLinearMemAllocator::TestAlloc();
		}
	};// dummyObj;
}

NS_END
