#pragma once
#include "../NxFrameBuffers.h"
#include "DX11PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX11GpuDevice;
	class DX11FrameBuffers : public IFrameBuffers
	{
	public:
		DX11FrameBuffers();
		~DX11FrameBuffers();
		virtual void FlushModify() override;
	public:
		std::vector<ID3D11RenderTargetView*>	 mDX11RTVArray;
	}; 

	class DX11SwapChain : public ISwapChain
	{
	public:
		DX11SwapChain();
		~DX11SwapChain();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX11GpuDevice* device, const FSwapChainDesc& desc);
		virtual UINT GetBackBufferCount() const override
		{
			return (UINT)BackBuffers.size();
		}
		virtual ITexture* GetBackBuffer(UINT index) override;
		virtual IRenderTargetView* GetBackRTV(UINT index) override;
		virtual UINT GetCurrentBackBuffer() override;
		virtual void Present(IGpuDevice* device, UINT SyncInterval, UINT Flags) override;
		virtual bool Resize(IGpuDevice* device, UINT w, UINT h) override;
	public:
		IDXGISwapChain* mView = nullptr;
		struct FBackBuffer
		{
			AutoRef<ITexture>	Texture;
			AutoRef<IRenderTargetView>	Rtv;
			void CreateRtvAndSrv(IGpuDevice* device);
		};
		std::vector<FBackBuffer>		BackBuffers;
	};
}

NS_END