#include "DX12PreHead.h"
#include "DX12Event.h"
#include "DX12GpuDevice.h"
#include "DX12CommandList.h"
#include "DX12Drawcall.h"

#include "../../3rd/native/NVAftermath/include/GFSDK_Aftermath.h"
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
	
	AutoRef<DX12CmdRecorder> DX12CommandAllocatorManager::Alloc(ID3D12Device* device)
	{
		VAutoVSLLock lk(mLocker);
		if (CmdAllocators.size() == 0)
		{
			for (int i = 0; i < 10; i++)
			{
				AutoRef<DX12CmdRecorder> tmp = MakeWeakRef(new DX12CmdRecorder());
				auto hr = device->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(tmp->mAllocator.GetAddressOf()));
				ASSERT(hr == S_OK);

				CmdAllocators.push(tmp);
			}
		}
		auto result = CmdAllocators.front();
		CmdAllocators.pop();
		return result;
	}
	void DX12CommandAllocatorManager::Free(const AutoRef<DX12CmdRecorder>& allocator, UINT64 waitValue, AutoRef<IFence>& fence)
	{
		ASSERT(waitValue > 0);
		VAutoVSLLock lk(mLocker);
		FWaitRecycle tmp;
		tmp.Allocator = allocator;
		tmp.WaitValue = waitValue;
		tmp.Fence = fence;
		Recycles.push_back(tmp);
	}
	void DX12CommandAllocatorManager::UnsafeDirectFree(const AutoRef<DX12CmdRecorder>& allocator)
	{
		VAutoVSLLock lk(mLocker);
		CmdAllocators.push(allocator);
	}
	void DX12CommandAllocatorManager::TickRecycle()
	{
		VAutoVSLLock lk(mLocker);
		for (auto i = Recycles.begin(); i != Recycles.end(); )
		{
			auto value = i->Fence->GetCompletedValue();
			auto waitValue = i->WaitValue;
			ASSERT(waitValue > 0);
			//fuck off: d12 or driver reference the resource but didn't AddRef
			if (value > waitValue)
			{
				i->Allocator->mAllocator->Reset();
				i->Allocator->ResetGpuDraws();
				auto cmdlist = (DX12CommandList*)i->Allocator->mCmdList.GetPtr();
				if (cmdlist != nullptr && cmdlist->mCmdRecorder == nullptr)
				{
					cmdlist->mContext->Reset(i->Allocator->mAllocator, nullptr);
					cmdlist->mContext->Close();
				}
				i->Allocator->mCmdList.FromObject(nullptr);
				CmdAllocators.push(i->Allocator);
				i = Recycles.erase(i);
			}
			else
			{
				i++;
			}
		}
	}
	void DX12CommandAllocatorManager::Finalize()
	{
		VAutoVSLLock lk(mLocker);
		for (auto i = Recycles.begin(); i != Recycles.end(); )
		{
			auto value = i->Fence->GetCompletedValue();
			auto waitValue = i->WaitValue;
			ASSERT(waitValue > 0);
			//fuck off: d12 or driver reference the resource but didn't AddRef
			if (value >= waitValue)
			{
				i->Allocator->mAllocator->Reset();
				i->Allocator->ResetGpuDraws();

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
	AutoRef<FGpuMemory> DX12GpuDefaultMemAllocator::Alloc(IGpuDevice* device, const D3D12_RESOURCE_DESC* resDesc, const D3D12_HEAP_PROPERTIES* heapDesc, D3D12_RESOURCE_STATES resState, const wchar_t* debugName)
	{
		auto result = MakeWeakRef(new FDX12DefaultGpuMemory());
		result->GpuHeap = new DX12GpuHeap();
		result->Offset = 0;
		auto hr = ((DX12GpuDevice*)device)->mDevice->CreateCommittedResource(heapDesc, D3D12_HEAP_FLAG_NONE,
			resDesc, resState, nullptr, IID_PPV_ARGS(result->GetDX12GpuHeap()->mGpuResource.GetAddressOf()));
		if (hr == DXGI_ERROR_DEVICE_REMOVED)
		{
			((DX12GpuDevice*)device)->OnDeviceRemoved();
		}

		result->GetDX12GpuHeap()->mGpuResource->SetName(debugName);
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

		result->mGpuResource->SetName(L"Memory:Pooled");
		return result;
	}

	/// DX12DescriptorSetPagedObject-----------------------------------------------------------
	D3D12_GPU_DESCRIPTOR_HANDLE	DX12DescriptorSetPagedObject::GetGpuAddress(int index)
	{
		auto pManager = (DX12DescriptorSetAllocator*)GetAllocator();
		ASSERT(pManager);
		D3D12_GPU_DESCRIPTOR_HANDLE result = RealObject->GetGPUDescriptorHandleForHeapStart();
		result.ptr += this->OffsetInPage + pManager->mDescriptorStride * index;
		ASSERT(result.ptr != 0);
		return result;
	}
	D3D12_CPU_DESCRIPTOR_HANDLE	DX12DescriptorSetPagedObject::GetCpuAddress(int index)
	{
		auto pManager = (DX12DescriptorSetAllocator*)GetAllocator();
		if (pManager == nullptr)
			return D3D12_CPU_DESCRIPTOR_HANDLE{};
		D3D12_CPU_DESCRIPTOR_HANDLE result = RealObject->GetCPUDescriptorHandleForHeapStart();
		result.ptr += this->OffsetInPage + pManager->mDescriptorStride * index;
		return result;
	}
	DX12DescriptorSetCreator::PageType* DX12DescriptorSetCreator::CreatePage(UINT pageSize)
	{
		auto device = mDeviceRef.GetPtr();
		auto result = new DX12DescriptorSetCreator::PageType();
		auto desc = mDesc;
		desc.NumDescriptors = mDesc.NumDescriptors * pageSize;
		auto hr = device->mDevice->CreateDescriptorHeap(&desc, IID_ID3D12DescriptorHeap, (void**)result->mGpuHeap.GetAddressOf());
		if (hr != S_OK)
		{
			hr = device->mDevice->GetDeviceRemovedReason();
			device->OnDeviceRemoved();
			ASSERT(false);
			result->Release();
			return nullptr;
		}
		return result;
	}
	DX12DescriptorSetCreator::PagedObjectType* DX12DescriptorSetCreator::CreatePagedObject(
		DX12DescriptorSetCreator::PageType* page, UINT index)
	{
		//auto device = mDeviceRef.GetPtr();
		auto pAllocator = (DX12DescriptorSetAllocator*)page->Allocator.GetPtr();

		auto result = new DX12DescriptorSetPagedObject();
		result->OffsetInPage = pAllocator->mDescriptorStride * mDesc.NumDescriptors * index;
		result->RealObject = page->mGpuHeap;
		/*auto hr = device->mDevice->CreateDescriptorHeap(&mDesc, IID_ID3D12DescriptorHeap, (void**)result->RealObject.GetAddressOf());
		if (hr != S_OK)
		{
			ASSERT(false);
			result->Release();
			return nullptr;
		}
		result->RealObject->SetName(DebugName.c_str());*/
		//result->ShaderEffect = mShaderEffect;

		/*for (auto& i : result->ShaderEffect->mBinders)
		{
			if (i.second->BindType == EShaderBindType::SBT_CBuffer)
			{
				auto pVSBinder = (FShaderBinder*)i.second->VSBinder;
				FillRange(&usb, pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_CBV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_SRV)
			{
				auto pVSBinder = (FShaderBinder*)i.second->VSBinder;
				FillRange(&usb, pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SRV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_UAV)
			{
				auto pVSBinder = (FShaderBinder*)i.second->VSBinder;
				FillRange(&usb, pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_UAV);
			}
			else if (i.second->BindType == EShaderBindType::SBT_Sampler)
			{
				auto pVSBinder = (FShaderBinder*)i.second->VSBinder;
				FillRange(&sampler, pVSBinder, D3D12_DESCRIPTOR_RANGE_TYPE_SAMPLER);
			}
		}*/

		return result;
	}
	void DX12DescriptorSetCreator::OnAlloc(DX12DescriptorSetCreator::AllocatorType* pAllocator,
		DX12DescriptorSetCreator::PagedObjectType* obj)
	{
		//auto name = DebugName + L"_" + std::to_wstring(SerialId++);
		//obj->RealObject->SetName(name.c_str());
	}
	void DX12DescriptorSetCreator::OnFree(DX12DescriptorSetCreator::AllocatorType* pAllocator,
		DX12DescriptorSetCreator::PagedObjectType* obj)
	{
		if (IsDescriptorSet == false)
			return;
		//obj->RealObject->SetName(DebugName.c_str());
		
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		auto pDescriptorObj = (DX12DescriptorSetPagedObject*)obj;
		AutoRef<DX12DescriptorSetPagedObject> nullDescriptor;
		switch (HeapType)
		{
			case D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV:
				nullDescriptor = device->mNullCBV_SRV_UAV;
				break;
			case D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER:
				nullDescriptor = device->mNullSampler;
				break;
			case D3D12_DESCRIPTOR_HEAP_TYPE_RTV:
				nullDescriptor = device->mNullRTV;
				break;
			case D3D12_DESCRIPTOR_HEAP_TYPE_DSV:
				nullDescriptor = device->mNullDSV;
				break;
			default:
				break;
		}
		if (nullDescriptor == nullptr)
			return;
		if (pDescriptorObj == nullDescriptor)
			return;
		
		for (int i = 0; i < (int)mDesc.NumDescriptors; i++)
		{
			/*auto dst = pDescriptorObj->GetCpuAddress(i);
			auto src = nullDescriptor->GetCpuAddress(0);
			device->mDevice->CopyDescriptorsSimple(1, dst, src, HeapType);*/
		}
	}
	void DX12DescriptorSetCreator::FinalCleanup(MemAlloc::FPage<ObjectType>* page)
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		auto pPage = (DX12DescriptorSetPage*)page;
	}
	AutoRef<DX12DescriptorSetPagedObject> DX12DescriptorAllocatorManager::AllocDescriptorSet(DX12GpuDevice* pDevice,
		UINT numOfDescriptor, D3D12_DESCRIPTOR_HEAP_TYPE type)
	{
		UINT64 key = (((UINT64)type) << 32) | numOfDescriptor;
		AutoRef<DX12DescriptorSetAllocator> allocator;
		auto iter = mAllocators.find(key);
		if (iter == mAllocators.end())
		{
			allocator = MakeWeakRef(new DX12DescriptorSetAllocator());
			allocator->Creator.Type = DX12DescriptorSetCreator::EDescriptorType::Graphics;
			allocator->Creator.IsDescriptorSet = true;
			allocator->Creator.DebugName = L"DescriptorSet";
			allocator->Creator.mDesc.Type = type;
			/*if (type == D3D12_DESCRIPTOR_HEAP_TYPE_RTV || type == D3D12_DESCRIPTOR_HEAP_TYPE_DSV)
			{
				allocator->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_NONE;
			}
			else
			{
				allocator->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			}*/
			allocator->Creator.mDesc.Flags = D3D12_DESCRIPTOR_HEAP_FLAG_SHADER_VISIBLE;
			allocator->Creator.mDesc.NumDescriptors = numOfDescriptor;
			allocator->Creator.mDeviceRef.FromObject(pDevice);
			allocator->mDescriptorStride = pDevice->mDevice->GetDescriptorHandleIncrementSize(type);
			mAllocators.insert(std::make_pair(key, allocator));
		}
		else
		{
			allocator = iter->second;
		}

		return allocator->Alloc<DX12DescriptorSetPagedObject>();
	}

	void DX12ResourceDebugMapper::SetDebugMapper(ID3D12Resource* res, const char* name)
	{
		AutoRef<DX12ResourceDebugInfo> tmp;
		auto iter = mMapper.find(res);
		if (iter == mMapper.end())
		{
			tmp = MakeWeakRef(new DX12ResourceDebugInfo());
			mMapper.insert(std::make_pair(res, tmp));
		}
		else
		{
			tmp = iter->second;
		}
		tmp->Name = name;
		GFSDK_Aftermath_ResourceHandle afh{};
		GFSDK_Aftermath_DX12_RegisterResource(res, &afh);
	}
	DX12ResourceDebugMapper* DX12ResourceDebugMapper::Get()
	{
		static DX12ResourceDebugMapper obj;
		return &obj;
	}
}

NS_END