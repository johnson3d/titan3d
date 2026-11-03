#pragma once
#include "../NxBuffer.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	struct DX12PagedHeap;
	class DX12Buffer : public IBuffer
	{
	public:
		DX12Buffer();
		~DX12Buffer();
		bool Init(const char* file, int line, DX12GpuDevice* device, const FBufferDesc& desc);

		virtual void* GetHWBuffer() override{
			return ((DX12GpuHeap*)mGpuMemory->GpuMem->GpuHeap)->mGpuResource;
		}
		virtual bool Map(UINT index, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(UINT index) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) override;
		virtual void SetDebugName(const char* name) override;
		D3D12_GPU_VIRTUAL_ADDRESS GetGPUVirtualAddress()
		{
			return mGpuMemory->GetGPUVirtualAddress();
		}
	public:
		AutoRef<FGpuMemHolder>		mGpuMemory;
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
	};

	class DX12Texture : public ITexture
	{
	public:
		DX12Texture();
		~DX12Texture();
		bool Init(DX12GpuDevice* device, const FTextureDesc& desc);
		virtual void* GetHWBuffer() override {
			return (void*)mGpuResource;
		}
		virtual bool Map(UINT subRes, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(UINT subRes) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) override;
		virtual void SetDebugName(const char* name) override;
		virtual IGpuBufferData* CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint) override;
		virtual bool GetFootprint(FSubResourceFootPrint* fp, UINT64* rowSize, UINT64* totalSize, UINT subRes, UINT64 offset) override;
	public:
		TWeakRefHandle<DX12GpuDevice> mDeviceRef;
		D3D12_RESOURCE_DESC mDX12ResourceDesc{};
		AutoRef<ID3D12Resource>		mGpuResource;
	};

	class DX12CbView : public ICbView
	{
	public:
		DX12CbView();
		~DX12CbView();
		virtual void* GetHWBuffer() override {
			return mView->Heap;
		}
		D3D12_GPU_VIRTUAL_ADDRESS GetBufferVirtualAddress();
		bool Init(DX12GpuDevice* device, IBuffer* pBuffer, const FCbvDesc& desc, const char* file = nullptr, int line = 0);
	public:
		TWeakRefHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<DX12HeapHolder>		mView;
	};

	class DX12VbView : public IVbView
	{
		DX12GpuDevice* mDevice = nullptr;
	public:
		virtual void DeleteThis() override;
		bool Init(DX12GpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc, const char* file = nullptr, int line = 0);
		virtual void SetDebugName(const char* name) override;
	};

	class DX12IbView : public IIbView
	{
		DX12GpuDevice* mDevice = nullptr;
	public:
		virtual void DeleteThis() override;
		bool Init(DX12GpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc, const char* file = nullptr, int line = 0);
		virtual void SetDebugName(const char* name) override;
	};

	class DX12SrView : public ISrView
	{
	public:
		DX12SrView();
		~DX12SrView();
		virtual void* GetHWBuffer() override {
			return mView->Heap;
		}
		bool Init(DX12GpuDevice* device, IGpuBufferData* pBffer, const FSrvDesc& desc, const char* file = nullptr, int line = 0);
		virtual bool UpdateBuffer(IGpuDevice* device, IGpuBufferData* buffer) override;
		virtual UINT GetFingerPrint() const override {
			return mFingerPrint;
		}
		virtual void SetFingerPrint(UINT fp) {
			mFingerPrint = fp;
		}
		virtual void SetDebugName(const char* name) {
			mDebugName = name;
			auto mDebugNameW = StringHelper::strtowstr(mDebugName);
			mView->Heap->RealObject->SetName(mDebugNameW.c_str());
		}
		const char* GetDebugName() const {
			return mDebugName.c_str();
		}
	public:
		TWeakRefHandle<DX12GpuDevice> mDeviceRef;
		
		std::string 				mDebugName;
		AutoRef<DX12HeapHolder>		mView;
		UINT						mFingerPrint = 0;
		
	};

	class DX12UaView : public IUaView
	{
	public:
		DX12UaView();
		~DX12UaView();
		virtual void* GetHWBuffer() override {
			return mView->Heap;
		}
		bool Init(DX12GpuDevice* device, IGpuBufferData* pBuffer, const FUavDesc& desc, const char* file = nullptr, int line = 0);
	public:
		TWeakRefHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<DX12HeapHolder>		mView;
	};

	class DX12RenderTargetView : public IRenderTargetView
	{
	public:
		DX12RenderTargetView();
		~DX12RenderTargetView();
		virtual void* GetHWBuffer() override {
			return mView->Heap;
		}
		bool Init(DX12GpuDevice* device, ITexture* pBuffer, const FRtvDesc* desc, const char* file = nullptr, int line = 0);
	public:
		TWeakRefHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<DX12HeapHolder>		mView;
	};

	class DX12DepthStencilView : public IDepthStencilView
	{
	public:
		DX12DepthStencilView();
		~DX12DepthStencilView();
		virtual void* GetHWBuffer() override {
			return mView->Heap;
		}
		bool Init(DX12GpuDevice* device, ITexture* pBuffer, const FDsvDesc& desc, const char* file = nullptr, int line = 0);
	public:
		TWeakRefHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<DX12HeapHolder>		mView;
	};

	class DX12AccelerationStructure : public IAccelerationStructure
	{
	public:
		bool Init(DX12GpuDevice* device, const FAccelerationStructureDesc* desc, const char* file = nullptr, int line = 0);

		std::vector<AutoRef<FMeshPrimitives>>		mMeshes;
		std::vector<D3D12_RAYTRACING_GEOMETRY_DESC>	mGeometryDesc;

		D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_DESC mBuildDesc{};
		// 创建归属：内部 buffer 都是替调用者创建的，台账要归到 CreateAccelerationStructure 的调用点。
		const char*						mCreateFile = nullptr;
		int								mCreateLine = 0;
	};

	class DX12AStructureInstance : public IAStructureInstance
	{
	public:
		bool Init(DX12GpuDevice* device, const FAStructureInstanceDesc* desc, IAccelerationStructure* pAStructrure);
	};

	class DX12TopAccelerationStructure : public ITopAccelerationStructure
	{
	public:
		bool Init(DX12GpuDevice* device, const FTopAccelerationStructureDesc* desc, const char* file = nullptr, int line = 0);
		virtual bool BuildAcclerationStruture() override;

		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		// 创建归属：IsBuild/BuildAcclerationStruture 里的惰性分配沿用该位置。
		const char*						mCreateFile = nullptr;
		int								mCreateLine = 0;
		Hash128							mInstanceHash;
		std::vector<D3D12_RAYTRACING_INSTANCE_DESC>		mInstDescs;
		D3D12_BUILD_RAYTRACING_ACCELERATION_STRUCTURE_DESC mBuildDesc{};
		AutoRef<IBuffer>				mSourceGpuBuffer;
	private:
		bool IsBuild(DX12GpuDevice* device);
	};

	class DX12Bindless : public IBindless
	{
	public:
		TWeakRefHandle<DX12GpuDevice> mDeviceRef;
		FDX12DescriptorHeap			mHeap;
		UINT						mStartIndex;
		virtual void OnBind(UINT index, IGpuResource* resource) override;
	};
}

NS_END
