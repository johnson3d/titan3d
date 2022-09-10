#include "DX12PreHead.h"
#include "DX12Event.h"
#include "DX12GpuDevice.h"

#define new VNEW

#pragma comment(lib,"DXGI.lib")
#pragma comment(lib,"D3D12.lib")
#pragma comment(lib,"dxguid.lib")

NS_BEGIN

namespace NxRHI
{
	ID3D12DescriptorHeap* FDX12GpuMemory::GetDX12Heap()
	{
		auto p = Batch.GetPtr();
		if (p == nullptr)
			return nullptr;
		return p->mHeap;
	}
	void FDX12GpuMemory::FreeMemory()
	{
		auto p = Batch.GetPtr();
		if (p == nullptr)
			return;
		p->mManager->Free(this);
	}

	FDX12GpuMemory* FDX12GpuMemoryBatch::Init(ID3D12Device* device, DX12AllocHeapManager* manager)
	{
		mManager = manager;
		auto hr = device->CreateDescriptorHeap(
			&manager->mHeapDesc, IID_PPV_ARGS(mHeap.GetAddressOf()));
		if (hr != S_OK)
		{
			ASSERT(false);
			return nullptr;
		}

		UINT BatchSize = manager->mHeapDesc.NumDescriptors;
		auto offset = mHeap->GetCPUDescriptorHandleForHeapStart().ptr;
		FDX12GpuMemory* prev = nullptr;
		FDX12GpuMemory* first = nullptr;
		for (UINT i = 0; i < BatchSize; i++)
		{
			auto cur = new FDX12GpuMemory();
			cur->Batch.FromObject((FDX12GpuMemoryBatch*)this);
			cur->Handle.ptr = offset;
			offset += manager->mDescriptorSize;
			if (first == nullptr)
				first = cur;
			if (prev != nullptr)
			{
				prev->Next = cur;
			}
			prev = cur;
		}

		return first;
	}
	FDX12GpuMemoryBatch::~FDX12GpuMemoryBatch()
	{
		
	}
	AutoRef<FDX12GpuMemory> DX12AllocHeapManager::Alloc(ID3D12Device* device)
	{
		VAutoVSLLock lk(mLocker);
		if (mFreePoint == nullptr)
		{
			auto pBatch = MakeWeakRef(new FDX12GpuMemoryBatch());
			mHandleBatches.push_back(pBatch);
			mFreePoint = pBatch->Init(device, this);
		}
		auto result = mFreePoint;
		mFreePoint = mFreePoint->Next;
		result->Next = nullptr;
		return MakeWeakRef(result);
	}
	void DX12AllocHeapManager::Free(FDX12GpuMemory* handle)
	{
		auto p = handle->Batch.GetPtr();
		p->mManager->Lock();
		ASSERT(handle->Next == nullptr);
		handle->Next = mFreePoint;
		mFreePoint = handle;
		p->mManager->Unlock();
		handle->AddRef();
	}
	DX12AllocHeapManager::~DX12AllocHeapManager()
	{
		while (mFreePoint != nullptr)
		{
			auto save = mFreePoint;
			mFreePoint = mFreePoint->Next;
			save->Release();
		}
		mHandleBatches.clear();
	}




	/// <summary>
	/// 
	/// </summary>
	//const int NumHeapDescriptor = 2;//cbv,srv,uav:(vs,ps)|sampler(vs,ps)
	void DX12TableHeap::InitTableHeap(ID3D12Device* device, DX12TableHeapManager* manager, UINT num)
	{
		D3D12_DESCRIPTOR_HEAP_DESC srvHeapDesc = {};
		srvHeapDesc.NumDescriptors = num;// NumHeapDescriptor;
		srvHeapDesc.Type = manager->mHeapType;
		srvHeapDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
		auto hr = device->CreateDescriptorHeap(&srvHeapDesc, IID_PPV_ARGS(mHeap.GetAddressOf()));
		ASSERT(hr == S_OK);
	}
	void DX12TableHeap::FreeTableHeap()
	{
		auto pManager = mPool->mManagerRef.GetPtr();
		if (pManager == nullptr)
			return;
		pManager->Free(this);
	}
	D3D12_CPU_DESCRIPTOR_HANDLE	DX12TableHeap::GetHandle(int index)
	{
		auto pManager = mPool->mManagerRef.GetPtr();
		if (pManager == nullptr)
			return D3D12_CPU_DESCRIPTOR_HANDLE{};
		if (index >= mPool->mNum)
			return D3D12_CPU_DESCRIPTOR_HANDLE{};
		D3D12_CPU_DESCRIPTOR_HANDLE result = mHeap->GetCPUDescriptorHandleForHeapStart();
		result.ptr += pManager->mDescriptorStride * index;
		return result;
	}
	AutoRef<DX12TableHeap> DX12TableHeapPool::Alloc(ID3D12Device* device, UINT num) 
	{
		auto pManager = mManagerRef.GetPtr();
		if (pManager == nullptr)
			return nullptr;
		if (mTableHeaps.empty())
		{
			for (int i = 0; i < pManager->mGrowNum; i++)
			{
				auto t = MakeWeakRef(new DX12TableHeap());
				t->mPool = this;
				t->InitTableHeap(device, pManager, num);
				mTableHeaps.push(t);
			}
		}
		auto result = mTableHeaps.top();
		mTableHeaps.pop();
		return result;
	}
	void DX12TableHeapPool::Free(const AutoRef<DX12TableHeap>& th)
	{
		mTableHeaps.push(th);
	}

	AutoRef<DX12TableHeap> DX12TableHeapManager::Alloc(ID3D12Device* device, UINT num)
	{
		VAutoVSLLock lk(mLocker);
		AutoRef<DX12TableHeapPool> pool;
		auto iter = Pools.find(num);
		if (iter == Pools.end())
		{
			pool = MakeWeakRef(new DX12TableHeapPool());
			pool->mManagerRef.FromObject(this);
			pool->mNum = num;
			Pools.insert(std::make_pair(num, pool));
		}
		else
		{
			pool = iter->second;
		}

		return pool->Alloc(device, num);
	}
	void DX12TableHeapManager::Free(const AutoRef<DX12TableHeap>& th)
	{
		VAutoVSLLock lk(mLocker);
		AutoRef<DX12TableHeapPool> pool;
		auto iter = Pools.find(th->mPool->mNum);
		if (iter == Pools.end())
		{
			ASSERT(false);
			return;
		}
		else
		{
			pool = iter->second;
		}
		pool->Free(th);
	}

	AutoRef<ID3D12CommandAllocator> DX12CommandAllocatorManager::Alloc(ID3D12Device* device)
	{
		VAutoVSLLock lk(mLocker);
		if (CmdAllocators.size() == 0)
		{
			for (int i = 0; i < 10; i++)
			{
				AutoRef<ID3D12CommandAllocator> tmp;
				auto hr = device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(tmp.GetAddressOf()));
				ASSERT(hr == S_OK);

				CmdAllocators.push(tmp);
			}
		}
		auto result = CmdAllocators.front();
		CmdAllocators.pop();
		return result;
	}
	void DX12CommandAllocatorManager::Free(const AutoRef<ID3D12CommandAllocator>& allocator, UINT64 waitValue, AutoRef<IFence>& fence) 
	{
		VAutoVSLLock lk(mLocker);
		FWaitRecycle tmp;
		tmp.Allocator = allocator;
		tmp.WaitValue = waitValue;
		tmp.Fence = fence;
		Recycles.push_back(tmp);
	}
	void DX12CommandAllocatorManager::TickRecycle()
	{
		VAutoVSLLock lk(mLocker);
		for (auto i = Recycles.begin(); i != Recycles.end(); )
		{
			if (i->Fence->GetCompletedValue() >= i->WaitValue)
			{
				CmdAllocators.push(i->Allocator);
				i = Recycles.erase(i);
			}
			else
			{
				i++;
			}
		}
	}

	FDX12DefaultGpuMemory::~FDX12DefaultGpuMemory()
	{
		Safe_Release(GpuHeap);
	}
	void FDX12DefaultGpuMemory::FreeMemory()
	{
		
	}
	AutoRef<FGpuMemory> DX12GpuDefaultMemAllocator::Alloc(IGpuDevice* device, const D3D12_RESOURCE_DESC* resDesc, const D3D12_HEAP_PROPERTIES* heapDesc, D3D12_RESOURCE_STATES resState)
	{
		auto result = MakeWeakRef(new FDX12DefaultGpuMemory());
		result->GpuHeap = new DX12GpuHeap();
		result->Offset = 0;
		((DX12GpuDevice*)device)->mDevice->CreateCommittedResource(heapDesc, D3D12_HEAP_FLAG_NONE,
			resDesc, resState, nullptr, IID_PPV_ARGS(result->GetDX12GpuHeap()->mGpuResource.GetAddressOf()));
		return result;
	}
	AutoRef<FGpuMemory> DX12GpuDefaultMemAllocator::Alloc(IGpuDevice* device, UINT64 size)
	{
		auto result = MakeWeakRef(new FDX12DefaultGpuMemory());
		result->GpuHeap = new DX12GpuHeap();
		result->Offset = 0;
		auto resDesc = mResDesc;
		resDesc.Width = size;
		((DX12GpuDevice*)device)->mDevice->CreateCommittedResource(&mHeapProperties, D3D12_HEAP_FLAG_NONE,
			&resDesc, mResState, nullptr, IID_PPV_ARGS(result->GetDX12GpuHeap()->mGpuResource.GetAddressOf()));
		return result;
	}
	
	void DX12GpuDefaultMemAllocator::Free(FGpuMemory* memory)
	{
		memory->GpuHeap->Release();
		memory->GpuHeap = nullptr;
		memory->Offset = -1;
	}
	
	IGpuHeap* DX12GpuPooledMemAllocator::CreateGpuHeap(IGpuDevice* device, UINT64 size, UINT count)
	{
		auto result = new DX12GpuHeap();
		
		//((DX12GpuDevice*)device)->mDevice->CreateHeap(&mHeapDesc, IID_PPV_ARGS(result->mDxHeap.GetAddressOf()));

		((DX12GpuDevice*)device)->mDevice->CreateCommittedResource(&mHeapProperties, D3D12_HEAP_FLAG_NONE,
			&mResDesc, mResState, nullptr, IID_PPV_ARGS(result->mGpuResource.GetAddressOf()));
		return result;
	}
}

NS_END