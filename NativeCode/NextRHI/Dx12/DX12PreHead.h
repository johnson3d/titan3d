#pragma once

#include "../NxGpuDevice.h"
#include "../NxRHIDefine.h"

#pragma warning(push)
#pragma warning(disable:4005)
#include <d3d12.h>
#include <d3d12Shader.h>
#include <d3dcompiler.h>
#include <dxgi1_4.h>
#pragma warning(pop)

NS_BEGIN

namespace NxRHI
{
	inline DXGI_FORMAT FormatToDX12Format(EPixelFormat pixel_fmt)
	{
		switch (pixel_fmt)
		{
		case PXF_UNKNOWN:
			return DXGI_FORMAT_UNKNOWN;
		case PXF_R16_FLOAT:
			return DXGI_FORMAT_R16_FLOAT;
		case PXF_R16_UINT:
			return DXGI_FORMAT_R16_UINT;
		case PXF_R16_SINT:
			return DXGI_FORMAT_R16_SINT;
		case PXF_R16_UNORM:
			return DXGI_FORMAT_R16_UNORM;
		case PXF_R16_SNORM:
			return DXGI_FORMAT_R16_SNORM;
		case PXF_R32_UINT:
			return DXGI_FORMAT_R32_UINT;
		case PXF_R32_SINT:
			return DXGI_FORMAT_R32_SINT;
		case PXF_R32_FLOAT:
			return DXGI_FORMAT_R32_FLOAT;
		case PXF_R8G8B8A8_SINT:
			return DXGI_FORMAT_R8G8B8A8_SINT;
		case PXF_R8G8B8A8_UINT:
			return DXGI_FORMAT_R8G8B8A8_UINT;
		case PXF_R8G8B8A8_UNORM:
			return DXGI_FORMAT_R8G8B8A8_UNORM;
		case PXF_R8G8B8A8_SNORM:
			return DXGI_FORMAT_R8G8B8A8_SNORM;
		case PXF_R16G16_UINT:
			return DXGI_FORMAT_R16G16_UINT;
		case PXF_R16G16_SINT:
			return DXGI_FORMAT_R16G16_SINT;
		case PXF_R16G16_FLOAT:
			return DXGI_FORMAT_R16G16_FLOAT;
		case PXF_R16G16_UNORM:
			return DXGI_FORMAT_R16G16_UNORM;
		case PXF_R16G16_SNORM:
			return DXGI_FORMAT_R16G16_SNORM;
		case PXF_R16G16B16A16_UINT:
			return DXGI_FORMAT_R16G16B16A16_UINT;
		case PXF_R16G16B16A16_SINT:
			return DXGI_FORMAT_R16G16B16A16_SINT;
		case PXF_R16G16B16A16_FLOAT:
			return DXGI_FORMAT_R16G16B16A16_FLOAT;
		case PXF_R16G16B16A16_UNORM:
			return DXGI_FORMAT_R16G16B16A16_UNORM;
		case PXF_R16G16B16A16_SNORM:
			return DXGI_FORMAT_R16G16B16A16_SNORM;
		case PXF_R32G32B32A32_UINT:
			return DXGI_FORMAT_R32G32B32A32_UINT;
		case PXF_R32G32B32A32_SINT:
			return DXGI_FORMAT_R32G32B32A32_SINT;
		case PXF_R32G32B32A32_FLOAT:
			return DXGI_FORMAT_R32G32B32A32_FLOAT;
		case PXF_R32G32B32_UINT:
			return DXGI_FORMAT_R32G32B32_UINT;
		case PXF_R32G32B32_SINT:
			return DXGI_FORMAT_R32G32B32_SINT;
		case PXF_R32G32B32_FLOAT:
			return DXGI_FORMAT_R32G32B32_FLOAT;
		case PXF_R32G32_UINT:
			return DXGI_FORMAT_R32G32_UINT;
		case PXF_R32G32_SINT:
			return DXGI_FORMAT_R32G32_SINT;
		case PXF_R32G32_FLOAT:
			return DXGI_FORMAT_R32G32_FLOAT;
		case PXF_D24_UNORM_S8_UINT:
			return DXGI_FORMAT_D24_UNORM_S8_UINT;
		case PXF_D32_FLOAT:
			return DXGI_FORMAT_D32_FLOAT;
		case PXF_D32_FLOAT_S8X24_UINT:
			return DXGI_FORMAT_D32_FLOAT_S8X24_UINT;
		case PXF_D16_UNORM:
			return DXGI_FORMAT_D16_UNORM;
		case PXF_B8G8R8A8_UNORM:
			return DXGI_FORMAT_B8G8R8A8_UNORM;
		case PXF_R11G11B10_FLOAT:
			return DXGI_FORMAT_R11G11B10_FLOAT;
		case PXF_R8G8_UNORM:
			return DXGI_FORMAT_R8G8_UNORM;
		case PXF_R8_UNORM:
			return DXGI_FORMAT_R8_UNORM;
		case PXF_R32_TYPELESS:
			return DXGI_FORMAT_R32_TYPELESS;
		case PXF_R32G32B32A32_TYPELESS:
			return DXGI_FORMAT_R32G32B32A32_TYPELESS;
		case PXF_R32G32B32_TYPELESS:
			return DXGI_FORMAT_R32G32B32_TYPELESS;
		case PXF_R16G16B16A16_TYPELESS:
			return DXGI_FORMAT_R16G16B16A16_TYPELESS;
		case PXF_R32G32_TYPELESS:
			return DXGI_FORMAT_R32G32_TYPELESS;
		case PXF_R32G8X24_TYPELESS:
			return DXGI_FORMAT_R32G8X24_TYPELESS;
		case PXF_R10G10B10A2_TYPELESS:
			return DXGI_FORMAT_R10G10B10A2_TYPELESS;
		case PXF_R10G10B10A2_UNORM:
			return DXGI_FORMAT_R10G10B10A2_UNORM;
		case PXF_R10G10B10A2_UINT:
			return DXGI_FORMAT_R10G10B10A2_UINT;
		case PXF_R8G8B8A8_TYPELESS:
			return DXGI_FORMAT_R8G8B8A8_TYPELESS;
		case PXF_R8G8B8A8_UNORM_SRGB:
			return DXGI_FORMAT_R8G8B8A8_UNORM_SRGB;
		case PXF_R16G16_TYPELESS:
			return DXGI_FORMAT_R16G16_TYPELESS;
		case PXF_R24G8_TYPELESS:
			return DXGI_FORMAT_R24G8_TYPELESS;
		case PXF_R24_UNORM_X8_TYPELESS:
			return DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
		case PXF_X24_TYPELESS_G8_UINT:
			return DXGI_FORMAT_X24_TYPELESS_G8_UINT;
		case PXF_R8G8_TYPELESS:
			return DXGI_FORMAT_R8G8_TYPELESS;
		case PXF_R8G8_UINT:
			return DXGI_FORMAT_R8G8_UINT;
		case PXF_R8G8_SNORM:
			return DXGI_FORMAT_R8G8_SNORM;
		case PXF_R8G8_SINT:
			return DXGI_FORMAT_R8G8_SINT;
		case PXF_R16_TYPELESS:
			return DXGI_FORMAT_R16_TYPELESS;
		case PXF_R8_TYPELESS:
			return DXGI_FORMAT_R8_TYPELESS;
		case PXF_R8_UINT:
			return DXGI_FORMAT_R8_UINT;
		case PXF_R8_SNORM:
			return DXGI_FORMAT_R8_SNORM;
		case PXF_R8_SINT:
			return DXGI_FORMAT_R8_SINT;
		case PXF_A8_UNORM:
			return DXGI_FORMAT_A8_UNORM;
		case PXF_B8G8R8X8_UNORM:
			return DXGI_FORMAT_B8G8R8X8_UNORM;
		case PXF_B8G8R8A8_TYPELESS:
			return DXGI_FORMAT_B8G8R8A8_TYPELESS;
		case PXF_B8G8R8A8_UNORM_SRGB:
			return DXGI_FORMAT_B8G8R8A8_UNORM_SRGB;
		case PXF_B8G8R8X8_TYPELESS:
			return DXGI_FORMAT_B8G8R8X8_TYPELESS;
		case PXF_B8G8R8X8_UNORM_SRGB:
			return DXGI_FORMAT_B8G8R8X8_UNORM_SRGB;
		case PXF_B5G6R5_UNORM:
			return DXGI_FORMAT_B5G6R5_UNORM;
		case PXF_B4G4R4A4_UNORM:
			return DXGI_FORMAT_B4G4R4A4_UNORM;
		default:
			break;
		}
		return DXGI_FORMAT_UNKNOWN;
	}

	class FDX12GpuMemoryBatch;
	class FDX12GpuMemory : public FGpuMemory
	{
	public:
		TObjectHandle<FDX12GpuMemoryBatch>	Batch;
		D3D12_CPU_DESCRIPTOR_HANDLE			Handle;
		FDX12GpuMemory*						Next = nullptr;
		ID3D12DescriptorHeap* GetDX12Heap();

		virtual void FreeMemory() override;
	};
	class DX12AllocHeapManager;
	class FDX12GpuMemoryBatch : public VIUnknown
	{
	public:
		~FDX12GpuMemoryBatch();
		DX12AllocHeapManager*			mManager;
		AutoRef<ID3D12DescriptorHeap>	mHeap;
		FDX12GpuMemory* Init(ID3D12Device* device, DX12AllocHeapManager* manager);
	};

	class DX12AllocHeapManager : public VIUnknownBase
	{
	public:
		DX12AllocHeapManager(ID3D12Device* device, D3D12_DESCRIPTOR_HEAP_TYPE type, D3D12_DESCRIPTOR_HEAP_FLAGS flags, UINT nodeMask, UINT num = 8)
		{
			mHeapDesc.Type = type;
			mHeapDesc.NodeMask = nodeMask;
			mHeapDesc.Flags = flags;
			mHeapDesc.NumDescriptors = num;
			mDescriptorSize = device->GetDescriptorHandleIncrementSize(type);
		}
		~DX12AllocHeapManager();
		void Lock()
		{
			mLocker.Lock();
		}
		void Unlock()
		{
			mLocker.Unlock();
		}
		AutoRef<FDX12GpuMemory> Alloc(ID3D12Device* device);
		void Free(FDX12GpuMemory* handle);
	public:
		VSLLock							mLocker;
		
		UINT							mDescriptorSize = 1;
		D3D12_DESCRIPTOR_HEAP_DESC		mHeapDesc;
		std::vector<AutoRef<FDX12GpuMemoryBatch>>	mHandleBatches;
		FDX12GpuMemory*					mFreePoint = nullptr;
	};

	class DX12TableHeapPool;
	class DX12TableHeapManager;
	class DX12TableHeap : public VIUnknownBase
	{
	public:
		void InitTableHeap(ID3D12Device* device, DX12TableHeapManager* manager, UINT num);
		AutoRef<ID3D12DescriptorHeap>	mHeap;
		DX12TableHeapPool*				mPool = nullptr;
		D3D12_CPU_DESCRIPTOR_HANDLE	GetHandle(int index);
		void FreeTableHeap();
	};
	class DX12TableHeapPool : public VIUnknownBase
	{
	public:
		AutoRef<DX12TableHeap> Alloc(ID3D12Device* device, UINT num);
		void Free(const AutoRef<DX12TableHeap>& th);
	public:
		TObjectHandle<DX12TableHeapManager>	mManagerRef;
		int								mNum = 0;
		std::stack<AutoRef<DX12TableHeap>>	mTableHeaps;
	};
	class DX12TableHeapManager : public VIUnknown
	{
	public:
		AutoRef<DX12TableHeap> Alloc(ID3D12Device* device, UINT num);
		void Free(const AutoRef<DX12TableHeap>& th);
	public:
		D3D12_DESCRIPTOR_HEAP_TYPE	mHeapType = D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV;
		int					mDescriptorStride = 0;
		int					mGrowNum = 64;
		VSLLock				mLocker;
		std::map<int, AutoRef<DX12TableHeapPool>>	Pools;
	};

	class DX12CommandAllocatorManager : public VIUnknownBase
	{
	public:
		AutoRef<ID3D12CommandAllocator> Alloc(ID3D12Device* device);
		void Free(const AutoRef<ID3D12CommandAllocator>& allocator, UINT64 waitValue, AutoRef<IFence>& fence);
		void TickRecycle();
	public:
		VSLLock				mLocker;
		std::queue<AutoRef<ID3D12CommandAllocator>>		CmdAllocators;
		struct FWaitRecycle
		{
			UINT64							WaitValue = 0;
			AutoRef<IFence>					Fence;
			AutoRef<ID3D12CommandAllocator> Allocator;
		};
		std::vector<FWaitRecycle>	Recycles;
	};

	struct DX12GpuHeap : public IGpuHeap
	{
		//AutoRef<ID3D12Heap>			mDxHeap;
		AutoRef<ID3D12Resource>		mGpuResource;
		virtual UINT64 GetGPUVirtualAddress() override
		{
			return mGpuResource->GetGPUVirtualAddress();
		}
		virtual void* GetHWBuffer() override {
			return mGpuResource;
		}
	};
	struct FDX12DefaultGpuMemory : public FGpuMemory
	{
		~FDX12DefaultGpuMemory();
		DX12GpuHeap* GetDX12GpuHeap() {
			return (DX12GpuHeap*)GpuHeap;
		}
		virtual void FreeMemory();
	};

	class DX12GpuDefaultMemAllocator : public IGpuMemAllocator
	{
	public:
		D3D12_RESOURCE_DESC			mResDesc{};
		D3D12_HEAP_PROPERTIES		mHeapProperties{};
		D3D12_RESOURCE_STATES		mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;

		static AutoRef<FGpuMemory> Alloc(IGpuDevice* device, const D3D12_RESOURCE_DESC* resDesc, const D3D12_HEAP_PROPERTIES* heapDesc, D3D12_RESOURCE_STATES resState);
		virtual AutoRef<FGpuMemory> Alloc(IGpuDevice* device, UINT64 size);
		virtual void Free(FGpuMemory* memory);
	};

	class DX12GpuPooledMemAllocator : public IGpuPooledMemAllocator
	{
	public:
		D3D12_RESOURCE_DESC			mResDesc{};
		D3D12_HEAP_PROPERTIES		mHeapProperties{};
		D3D12_RESOURCE_STATES		mResState = D3D12_RESOURCE_STATES::D3D12_RESOURCE_STATE_GENERIC_READ;
		virtual UINT GetBatchCount(UINT64 size) {
			return (UINT)(mResDesc.Width / size);
		}
		virtual IGpuHeap* CreateGpuHeap(IGpuDevice* device, UINT64 size, UINT count) override;
	};
}

NS_END