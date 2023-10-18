#pragma once
#include "../NxBuffer.h"
#include "DX11PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX11GpuDevice;
	class DX11CommandList;
	class DX11Buffer : public IBuffer
	{
	public:
		DX11Buffer();
		~DX11Buffer();
		bool Init(DX11GpuDevice* device, const FBufferDesc& desc);

		virtual void* GetHWBuffer() override{
			return mBuffer;
		}
		virtual bool Map(UINT index, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(UINT index) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void SetDebugName(const char* name) override;
	public:
		ID3D11Buffer* mBuffer;
		TWeakRefHandle<DX11GpuDevice>	mDeviceRef;
		DX11CommandList*				mMappingCmdList = nullptr;
	};

	class DX11Texture : public ITexture
	{
	public:
		DX11Texture();
		~DX11Texture();
		bool Init(DX11GpuDevice* device, const FTextureDesc& desc);
		bool Init(DX11GpuDevice* device, void* pSharedObject);
		virtual void* GetHWBuffer() override {
			return mTexture1D;
		}
		virtual void* GetSharedHandle() override;
		virtual bool Map(UINT subRes, FMappedSubResource* res, bool forRead) override;
		virtual void Unmap(UINT subRes) override;
		virtual void UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint) override;
		virtual void SetDebugName(const char* name) override;
		virtual IGpuBufferData* CreateBufferData(IGpuDevice* device, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint) override;
	public:
		union
		{
			ID3D11Texture1D* mTexture1D;
			ID3D11Texture2D* mTexture2D;
			ID3D11Texture3D* mTexture3D;
		};
		TWeakRefHandle<DX11GpuDevice>	mDeviceRef;
		DX11CommandList*				mMappingCmdList = nullptr;
	};

	class DX11CbView : public ICbView
	{
	public:
		bool Init(DX11GpuDevice* device, IBuffer* pBuffer, const FCbvDesc& desc);
	};

	class DX11VbView : public IVbView
	{
	public:
		bool Init(DX11GpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc);
	};

	class DX11IbView : public IIbView
	{
	public:
		bool Init(DX11GpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc);
	};

	class DX11SrView : public ISrView
	{
	public:
		DX11SrView();
		~DX11SrView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX11GpuDevice* device, IGpuBufferData* pBffer, const FSrvDesc& desc);
		virtual bool UpdateBuffer(IGpuDevice* device, IGpuBufferData* buffer) override;
	public:
		ID3D11ShaderResourceView* mView = nullptr;
	};

	class DX11UaView : public IUaView
	{
	public:
		DX11UaView();
		~DX11UaView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX11GpuDevice* device, IGpuBufferData* pBuffer, const FUavDesc& desc);
	public:
		ID3D11UnorderedAccessView* mView = nullptr;
	};

	class DX11RenderTargetView : public IRenderTargetView
	{
	public:
		DX11RenderTargetView();
		~DX11RenderTargetView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX11GpuDevice* device, ITexture* pBuffer, const FRtvDesc& desc);
	public:
		ID3D11RenderTargetView* mView = nullptr;
	};

	class DX11DepthStencilView : public IDepthStencilView
	{
	public:
		DX11DepthStencilView();
		~DX11DepthStencilView();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX11GpuDevice* device, ITexture* pBuffer, const FDsvDesc& desc);
	public:
		ID3D11DepthStencilView* mView = nullptr;
	};
}

NS_END
