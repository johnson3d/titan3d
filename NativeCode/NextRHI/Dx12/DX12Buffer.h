#pragma once
#include "../NxBuffer.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12Buffer : public IBuffer
	{
	public:
		DX12Buffer();
		~DX12Buffer();
		bool Init(DX12GpuDevice* device, const FBufferDesc& desc);

		virtual void* GetHWBuffer() override{
			return ((DX12GpuHeap*)mGpuMemory->GpuHeap)->mGpuResource;
		}
		virtual void Flush2Device(ICommandList* cmd, void* pBuffer, UINT Size) override;
		virtual bool Map(ICommandList* cmd, UINT index, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(ICommandList* cmd, UINT index) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT offset, void* pData, UINT size) override;
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) override;
		virtual void SetDebugName(const char* name) override;

		D3D12_GPU_VIRTUAL_ADDRESS GetGPUVirtualAddress()
		{
			return mGpuMemory->GetGPUVirtualAddress();
		}
	public:
		AutoRef<FGpuMemory>				mGpuMemory;
		TObjectHandle<DX12GpuDevice>	mDeviceRef;
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
		virtual bool Map(ICommandList* cmd, UINT subRes, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(ICommandList* cmd, UINT subRes) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch) override;
		virtual void TransitionTo(ICommandList* cmd, EGpuResourceState state) override;
		virtual void SetDebugName(const char* name) override;
		virtual IGpuBufferData* CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint) override;
	public:
		TObjectHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<ID3D12Resource>		mGpuResource;
	};

	class DX12CbView : public ICbView
	{
	public:
		DX12CbView();
		~DX12CbView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX12GpuDevice* device, IBuffer* pBuffer, const FCbvDesc& desc);
	public:
		TObjectHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<FDX12GpuMemory> mView;
	};

	class DX12VbView : public IVbView
	{
	public:
		bool Init(DX12GpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc);
	};

	class DX12IbView : public IIbView
	{
	public:
		bool Init(DX12GpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc);
	};

	class DX12SrView : public ISrView
	{
	public:
		DX12SrView();
		~DX12SrView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX12GpuDevice* device, IGpuBufferData* pBffer, const FSrvDesc& desc);
		virtual bool UpdateBuffer(IGpuDevice* device, IGpuBufferData* buffer) override;
	public:
		TObjectHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<FDX12GpuMemory> mView;
	};

	class DX12UaView : public IUaView
	{
	public:
		DX12UaView();
		~DX12UaView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX12GpuDevice* device, IGpuBufferData* pBuffer, const FUavDesc& desc);
	public:
		TObjectHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<FDX12GpuMemory> mView;
	};

	class DX12RenderTargetView : public IRenderTargetView
	{
	public:
		DX12RenderTargetView();
		~DX12RenderTargetView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX12GpuDevice* device, ITexture* pBuffer, const FRtvDesc* desc);
	public:
		TObjectHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<FDX12GpuMemory> mView;
	};

	class DX12DepthStencilView : public IDepthStencilView
	{
	public:
		DX12DepthStencilView();
		~DX12DepthStencilView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX12GpuDevice* device, ITexture* pBuffer, const FDsvDesc& desc);
	public:
		TObjectHandle<DX12GpuDevice> mDeviceRef;
		AutoRef<FDX12GpuMemory> mView;
	};
}

NS_END
