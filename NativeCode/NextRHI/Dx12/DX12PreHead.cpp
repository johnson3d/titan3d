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
	/// <summary>
	/// 
	/// </summary>
	//const int NumHeapDescriptor = 2;//cbv,srv,uav:(vs,ps)|sampler(vs,ps)
	
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