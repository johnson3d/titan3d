#pragma once

#include "../NxRHIDefine.h"
#include "../NxGpuDevice.h"
#include "../NxBuffer.h"
#include "../DXCommon.h"

#include "../../Base/allocator/PagedAllocator.h"

#pragma warning(push)
#pragma warning(disable:4005)
#include <d3d12.h>
#include <d3d12Shader.h>
#include <d3dcompiler.h>
#include <dxgi1_4.h>
#include "d3dx12.h"
#pragma warning(pop)

NS_BEGIN

namespace NxRHI
{
	inline UINT Align(UINT size, UINT alignment)
	{
		return (size + (alignment - 1)) & ~(alignment - 1);
	}
	class DX12GpuDevice;
	class DX12CmdRecorder;
	class DX12CommandList;

	inline DXGI_FORMAT FormatToDX12Format(EPixelFormat pixel_fmt)
	{
		return FormatToDXFormat(pixel_fmt);
	}
	inline EPixelFormat DX12FormatToFormat(DXGI_FORMAT fmt)
	{
		return DXFormatToFormat(fmt);
	}
	class TR_CLASS()
		DX12CommandAllocatorManager : public VIUnknown
	{
	public:
		AutoRef<DX12CmdRecorder> Alloc(DX12GpuDevice* device, DX12CommandList* cmdlist);
		void Free(const AutoRef<DX12CmdRecorder>& allocator, UINT64 waitValue, AutoRef<IFence>& fence);
		void TickRecycle();
		void FinalCleanup();
		void UnsafeDirectFree(const AutoRef<DX12CmdRecorder>& allocator);
		int GetNumOfAllocators() {
			return (int)CmdAllocators.size();
		}
		int GetNumOfRecycles() {
			return (int)Recycles.size();
		}
		int GetNumOfRecyclesRefs();
	public:
		VSLLock				mLocker;
		std::queue<AutoRef<DX12CmdRecorder>>		CmdAllocators;
		struct FWaitRecycle
		{
			UINT64							WaitValue = 0;
			AutoRef<IFence>					Fence;
			AutoRef<DX12CmdRecorder>		Allocator;
			int								WaitFrameCount = 0;
		};
		std::vector<FWaitRecycle>	Recycles;
	};

	struct DX12GpuHeap : public IGpuHeap
	{
		//AutoRef<ID3D12Heap>			mDxHeap;
		AutoRef<ID3D12Resource>		mGpuResource;
		virtual UINT64 GetGPUVirtualAddress() override
		{
			auto result = mGpuResource->GetGPUVirtualAddress();
			ASSERT(result != 0);
			return result;
		}
		virtual void* GetHWBuffer() override {
			return mGpuResource;
		}
		void SetDebugName(const char* name);
	};
	struct FDX12DefaultGpuMemory : public FGpuMemory
	{
		~FDX12DefaultGpuMemory();
		DX12GpuHeap* GetDX12GpuHeap() {
			return (DX12GpuHeap*)GpuHeap;
		}
		virtual void FreeMemory();
	};

	class TR_CLASS()
		DX12DefaultGpuMemAllocator : public IGpuMemAllocator
	{
	public:
		D3D12_RESOURCE_DESC			mResDesc{};
		D3D12_HEAP_PROPERTIES		mHeapProperties{};
		D3D12_RESOURCE_STATES		mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;

		AutoRef<FGpuMemory> Alloc(IGpuDevice* device, const D3D12_RESOURCE_DESC* resDesc, const D3D12_HEAP_PROPERTIES* heapDesc, D3D12_RESOURCE_STATES resState, const char* debugName);
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size, const char* name);
		virtual void Free(FGpuMemory* memory);

		FGpuMemHolder* AllocGpuMem(IGpuDevice* device, const D3D12_RESOURCE_DESC* resDesc, const D3D12_HEAP_PROPERTIES* heapDesc, D3D12_RESOURCE_STATES resState, const char* debugName) {
			auto result = new FGpuMemHolder();
			result->Allocator = this;
			result->GpuMem = Alloc(device, resDesc, heapDesc, resState, debugName);
			return result;
		}
		UINT64 GetAllocSize() {
			return TotalAllocSize;
		}
		UINT64 GetFreeSize() {
			return TotalFreeSize;
		}
	};

	class TR_CLASS()
		DX12PagedGpuMemAllocator : public IPagedGpuMemAllocator
	{
	public:
		D3D12_RESOURCE_DESC			mResDesc{};
		D3D12_HEAP_PROPERTIES		mHeapProperties{};
		D3D12_RESOURCE_STATES		mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;
		virtual UINT GetBatchCount(UINT64 size) {
			return (UINT)(mResDesc.Width / size);
		}
		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size, UINT count, const char* name) override;

		int GetPoolsCount()
		{
			return IPagedGpuMemAllocator::GetPoolsCount();
		}
		UINT64 GetTotalPoolSize()
		{
			return IPagedGpuMemAllocator::GetTotalPoolSize();
		}
	};

	///-----------------------------------------------------------
	///DX12DescriptorSetPagedObject
	struct FDX12DescriptorHeap;
	struct FCopyDescriptors
	{
		std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> Src;
		std::vector<D3D12_CPU_DESCRIPTOR_HANDLE> Dest;
		std::vector<UINT> Sizes;
		void Reset()
		{
			Src.clear();
			Dest.clear();
			Sizes.clear();
		}
	};

	struct DX12PagedHeap : public MemAlloc::FPagedObject<AutoRef<ID3D12DescriptorHeap>>
	{
		static DX12PagedHeap* GetNullHeap(DX12GpuDevice* device, EShaderBindType type);
		//DX12ShaderEffect*		ShaderEffect = nullptr;
		D3D12_GPU_DESCRIPTOR_HANDLE	GetGpuAddress(int index = 0);
		D3D12_CPU_DESCRIPTOR_HANDLE	GetCpuAddress(int index);
		void PushDescriptorCopy(FCopyDescriptors& descriptors, DX12PagedHeap* dest, UINT destIndex);
		void BindToHeap(DX12GpuDevice* device, DX12PagedHeap* dest, UINT destIndex, UINT srcIndex, D3D12_DESCRIPTOR_HEAP_TYPE HeapType);

		void PushDescriptorCopy(FCopyDescriptors& descriptors, FDX12DescriptorHeap& dest, UINT destIndex);
		void BindToHeap(DX12GpuDevice* device, FDX12DescriptorHeap& dest, UINT destIndex, UINT srcIndex, D3D12_DESCRIPTOR_HEAP_TYPE HeapType);
		//D3D12_DESCRIPTOR_HEAP_TYPE	HeapType = D3D12_DESCRIPTOR_HEAP_TYPE::D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
		SIZE_T						OffsetInPage = 0;

		//std::vector<AutoRef<IGpuResource>>	RefResources;
	};
	struct DX12HeapHolder : public IGpuResource
	{
		int NumOfDescriptor = 0;
		AutoRef<DX12PagedHeap>	Heap;
		~DX12HeapHolder()
		{
			if (Heap != nullptr)
			{
				Heap->Free();
				/*for (auto& i : Heap->RefResources)
				{
					i = nullptr;
				}*/

				Heap = nullptr;
			}
		}
		D3D12_GPU_DESCRIPTOR_HANDLE	GetGpuAddress(int index = 0)
		{
			return Heap->GetGpuAddress(index);
		}
		D3D12_CPU_DESCRIPTOR_HANDLE	GetCpuAddress(int index)
		{
			return Heap->GetCpuAddress(index);
		}
		void BindToHeap(DX12GpuDevice* device, DX12PagedHeap* dest, UINT destIndex, UINT srcIndex, D3D12_DESCRIPTOR_HEAP_TYPE HeapType)
		{
			return Heap->BindToHeap(device, dest, destIndex, srcIndex, HeapType);
		}
	};

	struct DX12HeapCreator
	{
		struct DX12DescriptorSetPage : public MemAlloc::FPage<AutoRef<ID3D12DescriptorHeap>>
		{
			AutoRef<ID3D12DescriptorHeap>		mGpuHeap;
		};

		DX12HeapCreator()
		{
			
		}
		TWeakRefHandle<DX12GpuDevice>		mDeviceRef;

		enum EDescriptorType
		{
			Graphics,
			Compute,
		};
		EDescriptorType Type = EDescriptorType::Graphics;
		bool			IsDescriptorSet = false;
		
		D3D12_DESCRIPTOR_HEAP_TYPE			HeapType = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
		D3D12_DESCRIPTOR_HEAP_DESC			mDesc{};
		//bool								IsSampler = false;
		std::wstring						DebugName;
		UINT								SerialId = 0;
		using ObjectType = AutoRef<ID3D12DescriptorHeap>;
		using PagedObjectType = MemAlloc::FPagedObject<ObjectType>;
		using PageType = DX12DescriptorSetPage;// MemAlloc::FPage<ObjectType>;
		using AllocatorType = MemAlloc::FAllocatorBase<ObjectType>;

		UINT GetPageSize() const{
			return PageSize;
		}
		UINT PageSize = 128;

		PageType* CreatePage(UINT pageSize);
		PagedObjectType* CreatePagedObject(PageType* page, UINT index);
		void OnAlloc(AllocatorType* pAllocator, PagedObjectType* obj);
		void OnFree(AllocatorType* pAllocator, PagedObjectType* obj);
		void FinalCleanup(MemAlloc::FPage<ObjectType>* page);
	};

	struct TR_CLASS()
		DX12HeapAllocator : public MemAlloc::FPagedObjectAllocator<DX12HeapCreator::ObjectType, DX12HeapCreator>
	{
		UINT			mDescriptorStride = 0;
		DX12HeapHolder* AllocDX12Heap()
		{
			auto result = new DX12HeapHolder();
			result->Heap = this->Alloc<DX12PagedHeap>();
			return result;
		}
		int GetPageCount() {
			return (int)Pages.size();
		}
		int GetTotalSize() {
			return (int)Pages.size() * Creator.GetPageSize();
		}
		int GetAliveCount() {
			return this->LiveCount;
		}
	};	
	
	TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
	typedef void(*FDX12HeapAllocatorVisitor)(UINT64 key, EngineNS::NxRHI::DX12HeapAllocator* value);

	class TR_CLASS()
		DX12HeapAllocatorManager : public IWeakRefObject
	{
		std::map<UINT64, AutoRef<DX12HeapAllocator>>		mAllocators;
	public:
		DX12HeapHolder* AllocDX12Heap(DX12GpuDevice* pDevice, UINT numOfDescriptor, D3D12_DESCRIPTOR_HEAP_TYPE type);
		int GetAllocatorCount() {
			return (int)mAllocators.size();
		}
		void IterateAllocator(FDX12HeapAllocatorVisitor fn)
		{
			if (fn == nullptr)
				return;
			for (auto& i : mAllocators)
			{
				fn(i.first, i.second);
			}
		}
	};
	///-----------------------------------------------------------

	struct FDX12DescriptorHeap
	{
		int Start = 0;
		int Stride = 0;
		int Num = 0;
		UINT DescriptorMask = 0;
		UINT DescriptorState = 0;
		ID3D12DescriptorHeap* Heap = nullptr;
		inline void Set(int index)
		{
			if (Num > 32)
				return;
			DescriptorState |= (1 << index);
		}
		inline bool IsSet(int index)
		{
			if (Num > 32)
				return false;
			return (DescriptorState & (1 << index)) != 0;
		}
		inline void CheckCompletion()
		{
			ASSERT(DescriptorState == DescriptorMask);
		}
		D3D12_GPU_DESCRIPTOR_HANDLE GetGpuAddress(int index)
		{
			auto result = Heap->GetGPUDescriptorHandleForHeapStart();
			result.ptr += (Start + index) * Stride;
			return result;
		}
		D3D12_CPU_DESCRIPTOR_HANDLE GetCpuAddress(int index)
		{
			auto result = Heap->GetCPUDescriptorHandleForHeapStart();
			result.ptr += (Start + index) * Stride;
			return result;
		}
	};

	class DX12ResourceDebugMapper
	{
	public:
		struct DX12ResourceDebugInfo : public VIUnknown
		{
			std::string Name;
		};
		std::map<ID3D12Resource*, AutoRef<DX12ResourceDebugInfo>>	mMapper;
		VSLLock									mLocker;
	public:
		static DX12ResourceDebugMapper* Get();
		void SetDebugMapper(ID3D12Resource* res, const char* name);

		DX12ResourceDebugInfo* FindDebugInfo(ID3D12Resource* res) {
			auto iter = mMapper.find(res);
			if (iter == mMapper.end())
			{
				return nullptr;
			}
			else
			{
				return iter->second;
			}
		}
	};

}

NS_END