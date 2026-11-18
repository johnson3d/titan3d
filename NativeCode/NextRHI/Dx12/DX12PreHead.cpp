#include "DX12PreHead.h"
#include "DX12Event.h"
#include "DX12GpuDevice.h"
#include "DX12Buffer.h"
#include "DX12GpuState.h"
#include "DX12CommandList.h"
#include "DX12Drawcall.h"
#include "../../Base/vfxsampcounter.h"

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
	int DX12CommandAllocatorManager::GetNumOfRecyclesRefs()
	{
		int count = 0;
		for (auto& i : Recycles)
		{
			count += (int)i.Allocator->mRefBuffers.size();
		}
		return count;
	}
	AutoRef<DX12CmdRecorder> DX12CommandAllocatorManager::Alloc(DX12GpuDevice* device, DX12CommandList* cmdlist)
	{
		VAutoVSLLock lk(mLocker);
		if (CmdAllocators.size() == 0)
		{
			for (int i = 0; i < 10; i++)
			{
				AutoRef<DX12CmdRecorder> tmp = MakeWeakRef(new DX12CmdRecorder(device, ECmdRecorderType::CRT_All));
				auto hr = device->mDevice->CreateCommandAllocator(D3D12_COMMAND_LIST_TYPE_DIRECT, IID_PPV_ARGS(tmp->mAllocator.GetAddressOf()));
				ASSERT(hr == S_OK);

				CmdAllocators.push(tmp);
			}
		}
		auto result = CmdAllocators.front();
		ASSERT(result->GetDrawcallNumber() == 0);
		result->mCmdlist = cmdlist;
		result->ResetGpuDraws();

		CmdAllocators.pop();
		return result;
	}
	void DX12CommandAllocatorManager::Free(const AutoRef<DX12CmdRecorder>& allocator, UINT64 waitValue, AutoRef<IFence>& fence)
	{
		ASSERT(waitValue > 0);
		VAutoVSLLock lk(mLocker);
		ASSERT(allocator != nullptr);
		if (allocator->GetDrawcallNumber() == 0)
		{
			allocator->ResetGpuDraws();
			allocator->mCmdlist = nullptr;
			CmdAllocators.push(allocator);
			return;
		}
		FWaitRecycle tmp;
		tmp.Allocator = allocator;
		tmp.WaitValue = waitValue;
		tmp.Fence = fence;
		tmp.WaitFrameCount = 5;
		Recycles.push_back(tmp);
	}
	void DX12CommandAllocatorManager::UnsafeDirectFree(const AutoRef<DX12CmdRecorder>& allocator)
	{//never used
		ASSERT(allocator != nullptr);
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
			if (value >= waitValue)
			{
				ASSERT(i->Allocator->GetDrawcallNumber() != 0);
				i->Allocator->ResetGpuDraws();
				CmdAllocators.push(i->Allocator);
				i = Recycles.erase(i);
			}
			else
			{
				i->WaitFrameCount--;
				if (i->WaitFrameCount == 0)
				{
					VFX_LTRACE(ELTT_Warning, "DX12CmdAllocator always alive %d / %d\r\n", value, waitValue);
				}
				i++;	
			}
		}
	}
	void DX12CommandAllocatorManager::FinalCleanup()
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
		//ASSERT(Recycles.size() == 0);
		CmdAllocators = std::queue<AutoRef<DX12CmdRecorder>>();
	}

	void DX12GpuHeap::SetDebugName(const char* name)
	{
		std::wstring n = StringHelper::strtowstr(name);
		mGpuResource->SetName(n.c_str());
		DX12ResourceDebugMapper::Get()->SetDebugMapper(mGpuResource, name);
	}
	FDX12DefaultGpuMemory::~FDX12DefaultGpuMemory()
	{
		Safe_Release(GpuHeap);
	}
	void FDX12DefaultGpuMemory::FreeMemory()
	{
		
	}
	AutoRef<FGpuMemory> DX12DefaultGpuMemAllocator::Alloc(const char* file, int line, IGpuDevice* device, const D3D12_RESOURCE_DESC* resDesc, const D3D12_HEAP_PROPERTIES* heapDesc, D3D12_RESOURCE_STATES resState, const char* name)
	{
#pragma push_macro("new")
#undef new
		auto result = MakeWeakRef(new(file, line) FDX12DefaultGpuMemory());
		result->GpuHeap = new(file, line) DX12GpuHeap();
#pragma pop_macro("new")
		result->Size = (resDesc->Width * resDesc->Height) * GetPixelByteWidth(DXFormatToFormat(resDesc->Format));
		result->Offset = 0;

		TotalAllocSize += result->Size;
		auto hr = ((DX12GpuDevice*)device)->mDevice->CreateCommittedResource(heapDesc, D3D12_HEAP_FLAG_NONE,
			resDesc, resState, nullptr, IID_PPV_ARGS(result->GetDX12GpuHeap()->mGpuResource.GetAddressOf()));
		if (hr == DXGI_ERROR_DEVICE_REMOVED)
		{
			((DX12GpuDevice*)device)->OnDeviceRemoved();
		}
		if (hr != S_OK)
		{
			return nullptr;
		}

		if (name != nullptr)
		{
			std::wstring n = StringHelper::strtowstr(name);
			result->GetDX12GpuHeap()->mGpuResource->SetName(n.c_str());
		}
		if (device->Desc.IsAftermath)
		{
			DX12ResourceDebugMapper::Get()->SetDebugMapper(result->GetDX12GpuHeap()->mGpuResource, name);
		}
		return result;
	}
	AutoRef<FGpuMemory> DX12DefaultGpuMemAllocator::Alloc(IGpuDevice* device, UINT64 size, const char* name)
	{
		TotalAllocSize += size;//(resDesc.Width * resDesc.Height) * GetPixelByteWidth(DXFormatToFormat(resDesc->Format));
		auto result = MakeWeakRef(new FDX12DefaultGpuMemory());
		result->GpuHeap = new DX12GpuHeap();
		result->Offset = 0;
		result->Size = size;
		auto resDesc = mResDesc;
		resDesc.Width = size;
		((DX12GpuDevice*)device)->mDevice->CreateCommittedResource(&mHeapProperties, D3D12_HEAP_FLAG_NONE,
			&resDesc, mResState, nullptr, IID_PPV_ARGS(result->GetDX12GpuHeap()->mGpuResource.GetAddressOf()));

		if (name != nullptr)
		{
			std::wstring n = StringHelper::strtowstr(name);
			result->GetDX12GpuHeap()->mGpuResource->SetName(n.c_str());
		}
		if (device->Desc.IsAftermath)
		{
			DX12ResourceDebugMapper::Get()->SetDebugMapper(result->GetDX12GpuHeap()->mGpuResource, name);
		}
		return result;
	}
	
	void DX12DefaultGpuMemAllocator::Free(FGpuMemory* memory)
	{
		TotalFreeSize -= memory->Size;
		memory->GpuHeap->Release();
		memory->GpuHeap = nullptr;
		memory->Offset = -1;
	}
	
	IGpuHeap* DX12PagedGpuMemAllocator::CreateGpuHeap(IGpuDevice* device, UINT64 size, UINT count, const char* name)
	{
		auto result = new DX12GpuHeap();
		
		//((DX12GpuDevice*)device)->mDevice->CreateHeap(&mHeapDesc, IID_PPV_ARGS(result->mDxHeap.GetAddressOf()));

		((DX12GpuDevice*)device)->mDevice->CreateCommittedResource(&mHeapProperties, D3D12_HEAP_FLAG_NONE,
			&mResDesc, mResState, nullptr, IID_PPV_ARGS(result->mGpuResource.GetAddressOf()));

		if (name != nullptr)
		{
			std::wstring n = StringHelper::strtowstr(name);
			result->mGpuResource->SetName(n.c_str());
		}
		if (device->Desc.IsAftermath)
		{
			DX12ResourceDebugMapper::Get()->SetDebugMapper(result->mGpuResource, name);
		}

		return result;
	}

	/// DX12StagingMemAllocator-----------------------------------------------------------
	DX12StagingPage::~DX12StagingPage()
	{
		if (pMapped != nullptr && mGpuResource != nullptr)
		{
			mGpuResource->Unmap(0, nullptr);
			pMapped = nullptr;
		}
	}
	void FDX12StagingGpuMemory::FreeMemory()
	{
		if (Page == nullptr)
			return;
		auto allocator = HostAllocator.GetPtr();
		if (allocator != nullptr)
		{
			allocator->Free(this);
		}
		// 页已被 TickShrink 从池中移除时, 这里是最后一个引用, 资源随之释放
		Page = nullptr;
		GpuHeap = nullptr;
	}
	DX12StagingPage* DX12StagingMemAllocator::CreatePage(IGpuDevice* device, UINT64 chunkSize)
	{
		auto pageSize = chunkSize * ChunksPerPage;
		if (pageSize < MinPageSize)
			pageSize = MinPageSize;
		if (pageSize > MaxPageSize)
			pageSize = MaxPageSize;
		if (pageSize < chunkSize)
			pageSize = chunkSize;

		D3D12_HEAP_PROPERTIES properties{};
		properties.Type = D3D12_HEAP_TYPE_UPLOAD;
		properties.CPUPageProperty = D3D12_CPU_PAGE_PROPERTY_UNKNOWN;
		properties.MemoryPoolPreference = D3D12_MEMORY_POOL_UNKNOWN;
		D3D12_RESOURCE_DESC resDesc{};
		resDesc.Dimension = D3D12_RESOURCE_DIMENSION_BUFFER;
		resDesc.Alignment = D3D12_DEFAULT_RESOURCE_PLACEMENT_ALIGNMENT;
		resDesc.Width = pageSize;
		resDesc.Height = 1;
		resDesc.DepthOrArraySize = 1;
		resDesc.MipLevels = 1;
		resDesc.Format = DXGI_FORMAT_UNKNOWN;
		resDesc.SampleDesc.Count = 1;
		resDesc.SampleDesc.Quality = 0;
		resDesc.Layout = D3D12_TEXTURE_LAYOUT_ROW_MAJOR;
		resDesc.Flags = D3D12_RESOURCE_FLAG_NONE;

		auto dxDevice = (DX12GpuDevice*)device;
		auto page = new DX12StagingPage();
		auto hr = dxDevice->mDevice->CreateCommittedResource(&properties, D3D12_HEAP_FLAG_NONE,
			&resDesc, D3D12_RESOURCE_STATE_GENERIC_READ, nullptr, IID_PPV_ARGS(page->mGpuResource.GetAddressOf()));
		if (hr == DXGI_ERROR_DEVICE_REMOVED)
		{
			dxDevice->OnDeviceRemoved();
		}
		if (hr != S_OK)
		{
			page->Release();
			return nullptr;
		}
		// UPLOAD heap 允许常驻映射, CPU 只写不读: readRange 为空
		D3D12_RANGE readRange{ 0, 0 };
		void* pData = nullptr;
		if (page->mGpuResource->Map(0, &readRange, &pData) != S_OK)
		{
			page->Release();
			return nullptr;
		}
		page->pMapped = (BYTE*)pData;
		page->mGpuResource->SetName(L"StagingPool");
		if (device->Desc.IsAftermath)
		{
			DX12ResourceDebugMapper::Get()->SetDebugMapper(page->mGpuResource, "StagingPool");
		}

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
	AutoRef<FGpuMemory> DX12StagingMemAllocator::Alloc(IGpuDevice* device, UINT64 size, const char* name)
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
		DX12StagingPage* page = nullptr;
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

		auto result = MakeWeakRef(new FDX12StagingGpuMemory());
		result->HostAllocator.FromObject(this);
		result->Page = page;
		result->GpuHeap = page;
		result->ChunkIndex = page->FreeChunks.back();
		page->FreeChunks.pop_back();
		result->Offset = result->ChunkIndex * chunkSize;
		result->Size = chunkSize;
		TotalAllocSize += chunkSize;
		return result;
	}
	void DX12StagingMemAllocator::Free(FGpuMemory* memory)
	{
		auto mem = (FDX12StagingGpuMemory*)memory;
		VAutoVSLLock lk(mLocker);
		auto page = mem->Page.GetPtr();
		ASSERT(page->FreeChunks.size() < page->ChunkCount);
		page->FreeChunks.push_back(mem->ChunkIndex);
		if (page->IsIdle())
			page->IdleSinceTick = TickIndex;
		TotalFreeSize += mem->Size;
	}
	FGpuMemHolder* DX12StagingMemAllocator::AllocGpuMem(IGpuDevice* device, UINT64 size, const char* name)
	{
		auto mem = Alloc(device, size, name);
		if (mem == nullptr)
			return nullptr;
		auto result = new FGpuMemHolder();
		// Allocator 留空: holder 可能活得比 allocator 久 (设备销毁时 recorder 尚未回收), 归还走 FreeMemory 的弱引用
		result->GpuMem = mem;
		return result;
	}
	void DX12StagingMemAllocator::TickShrink()
	{
		std::vector<AutoRef<DX12StagingPage>> released;//出锁后再释放资源
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
				std::vector<AutoRef<DX12StagingPage>>* oldestPages = nullptr;
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

	/// DX12DescriptorSetPagedObject-----------------------------------------------------------
	void DX12PagedHeap::PushDescriptorCopy(FCopyDescriptors& descriptors, DX12PagedHeap* dest, UINT destIndex)
	{
		descriptors.Sizes.push_back(1);
		descriptors.Dest.push_back(dest->GetCpuAddress(destIndex));
		descriptors.Src.push_back(this->GetCpuAddress(0));
	}
	void DX12PagedHeap::PushDescriptorCopy(FCopyDescriptors& descriptors, FDX12DescriptorHeap& dest, UINT destIndex)
	{
		descriptors.Sizes.push_back(1);
		descriptors.Dest.push_back(dest.GetCpuAddress(destIndex));
		descriptors.Src.push_back(this->GetCpuAddress(0));

		dest.Set(destIndex);
	}
	void DX12PagedHeap::BindToHeap(DX12GpuDevice* device, DX12PagedHeap* dest, UINT destIndex, UINT srcIndex, D3D12_DESCRIPTOR_HEAP_TYPE HeapType)
	{
		//ASSERT(destIndex < dest->RefResources.size());
		//dest->RefResources[destIndex] = this->RefResources[srcIndex];
		AUTO_SAMP("DX12PagedHeap.BindToHeap");
		device->mDevice->CopyDescriptorsSimple(1, dest->GetCpuAddress(destIndex),
			this->GetCpuAddress(srcIndex), HeapType);
	}
	void DX12PagedHeap::BindToHeap(DX12GpuDevice* device, FDX12DescriptorHeap& dest, UINT destIndex, UINT srcIndex, D3D12_DESCRIPTOR_HEAP_TYPE HeapType)
	{
		//ASSERT(destIndex < dest->RefResources.size());
		//dest->RefResources[destIndex] = this->RefResources[srcIndex];
		AUTO_SAMP("DX12PagedHeap.BindToHeap");
		device->mDevice->CopyDescriptorsSimple(1, dest.GetCpuAddress(destIndex),
			this->GetCpuAddress(srcIndex), HeapType);
	}
	D3D12_GPU_DESCRIPTOR_HANDLE	DX12PagedHeap::GetGpuAddress(int index)
	{
		auto pManager = (DX12HeapAllocator*)GetAllocator();
		ASSERT(pManager);
		//ASSERT(index < pManager->Creator.GetPageSize());
		D3D12_GPU_DESCRIPTOR_HANDLE result = RealObject->GetGPUDescriptorHandleForHeapStart();
		result.ptr += this->OffsetInPage + pManager->mDescriptorStride * index;
		ASSERT(result.ptr != 0);
		return result;
	}
	D3D12_CPU_DESCRIPTOR_HANDLE	DX12PagedHeap::GetCpuAddress(int index)
	{
		auto pManager = (DX12HeapAllocator*)GetAllocator();
		if (pManager == nullptr)
			return D3D12_CPU_DESCRIPTOR_HANDLE{};
		D3D12_CPU_DESCRIPTOR_HANDLE result = RealObject->GetCPUDescriptorHandleForHeapStart();
		result.ptr += this->OffsetInPage + pManager->mDescriptorStride * index;
		return result;
	}
	DX12HeapCreator::PageType* DX12HeapCreator::CreatePage(UINT pageSize)
	{
		auto device = mDeviceRef.GetPtr();
		auto result = new DX12HeapCreator::PageType();
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
	DX12HeapCreator::PagedObjectType* DX12HeapCreator::CreatePagedObject(
		DX12HeapCreator::PageType* page, UINT index, const char* file, int line)
	{
		//auto device = mDeviceRef.GetPtr();
		auto pAllocator = page->Allocator.GetCastPtr<DX12HeapAllocator>();

#pragma push_macro("new")
#undef new
		auto result = new(file, line) DX12PagedHeap();
#pragma pop_macro("new")
		result->OffsetInPage = pAllocator->mDescriptorStride * mDesc.NumDescriptors * index;
		result->RealObject = page->mGpuHeap;
		//result->RefResources.resize(this->mDesc.NumDescriptors);
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
	void DX12HeapCreator::OnAlloc(DX12HeapCreator::AllocatorType* pAllocator,
		DX12HeapCreator::PagedObjectType* obj)
	{
		//auto name = DebugName + L"_" + std::to_wstring(SerialId++);
		//obj->RealObject->SetName(name.c_str());
	}
	void DX12HeapCreator::OnFree(DX12HeapCreator::AllocatorType* pAllocator,
		DX12HeapCreator::PagedObjectType* obj)
	{
		/*if (IsDescriptorSet == false)
			return;*/
		//obj->RealObject->SetName(DebugName.c_str());
		
		/*auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		auto pDescriptorObj = (DX12PagedHeap*)obj;
		
		for (UINT i = 0; i < mDesc.NumDescriptors; i++)
		{
			AutoRef<DX12PagedHeap> nullDescriptor;
			
			if (pDescriptorObj->RefResources[i] != nullptr)
				nullDescriptor = (DX12PagedHeap*)pDescriptorObj->RefResources[i]->GetNull();
			if (nullDescriptor == nullptr)
				continue;
			if (pDescriptorObj == nullDescriptor)
				continue;

			nullDescriptor->BindToHeap(device, pDescriptorObj, i, 0, HeapType);
		}*/
	}
	void DX12HeapCreator::FinalCleanup(MemAlloc::FPage<ObjectType>* page)
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		auto pPage = (DX12DescriptorSetPage*)page;
	}
	DX12HeapHolder* DX12HeapAllocatorManager::AllocDX12Heap(DX12GpuDevice* pDevice,
		UINT numOfDescriptor, D3D12_DESCRIPTOR_HEAP_TYPE type, const char* file, int line)
	{
		//todo:Alloc Like Vulkan DescriptorPool!
		//AUTO_SAMP("DX12HeapAllocatorManager.AllocDX12Heap");
		UINT64 key = (((UINT64)type) << 32) | numOfDescriptor;
		AutoRef<DX12HeapAllocator> allocator;
		auto iter = mAllocators.find(key);
		if (iter == mAllocators.end())
		{
			allocator = MakeWeakRef(new DX12HeapAllocator());
			allocator->Creator.Type = DX12HeapCreator::EDescriptorType::Graphics;
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

		auto result = allocator->AllocDX12Heap(file, line);
		result->NumOfDescriptor = numOfDescriptor;
		return result;

		/*auto result = new DX12HeapHolder();
		result->Heap = allocator->Alloc<DX12PagedDescriptorHeap>();
		return result;*/
	}

	void DX12ResourceDebugMapper::SetDebugMapper(ID3D12Resource* res, const char* name)
	{
		VAutoVSLLock lk(mLocker);
		AutoRef<DX12ResourceDebugInfo> tmp;
		auto iter = mMapper.find(res);
		if (iter == mMapper.end())
		{
			tmp = MakeWeakRef(new DX12ResourceDebugInfo());
			mMapper.insert(std::make_pair(res, tmp));

			GFSDK_Aftermath_ResourceHandle afh{};
			GFSDK_Aftermath_DX12_RegisterResource(res, &afh);
		}
		else
		{
			tmp = iter->second;
		}
		
		tmp->Name = name;
	}
	DX12ResourceDebugMapper* DX12ResourceDebugMapper::Get()
	{
		static DX12ResourceDebugMapper obj;
		return &obj;
	}
	DX12PagedHeap* DX12PagedHeap::GetNullHeap(DX12GpuDevice* device, EShaderBindType type)
	{
		switch (type)
		{
		case EngineNS::NxRHI::SBT_CBV:
			return device->mNullCBV->mView->Heap;
		case EngineNS::NxRHI::SBT_SRV:
			return device->mNullSRV->mView->Heap;
		case EngineNS::NxRHI::SBT_UAV:
			return device->mNullUAV->mView->Heap;
		case EngineNS::NxRHI::SBT_Sampler:
			return device->mNullSampler->mView;
		default:
			return nullptr;
		}
	}
}

NS_END