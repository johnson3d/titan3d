#pragma once
#include "../NxFrameBuffers.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12Texture;
	class DX12RenderTargetView;
	class DX12SrView;
	class DX12FrameBuffers : public IFrameBuffers
	{
	public:
		DX12FrameBuffers();
		~DX12FrameBuffers();
		virtual void FlushModify() override;
	public:
		std::vector<D3D12_CPU_DESCRIPTOR_HANDLE>	 mDX11RTVArray;
	}; 

	class DX12SwapChain : public ISwapChain
	{
	public:
		DX12SwapChain();
		~DX12SwapChain();
		virtual void* GetHWBuffer() override {
			return mView;
		}
		bool Init(DX12GpuDevice* device, const FSwapChainDesc& desc);
		virtual UINT GetBackBufferCount() const override
		{
			return (UINT)BackBuffers.size();
		}
		virtual ITexture* GetBackBuffer(UINT index) override;
		virtual IRenderTargetView* GetBackRTV(UINT index) override;
		virtual UINT GetCurrentBackBuffer() override;
		virtual void BeginFrame() override;
		virtual void Present(IGpuDevice* device, UINT SyncInterval, UINT Flags) override;
		virtual bool Resize(IGpuDevice* device, UINT w, UINT h) override;

		bool Create(IGpuDevice* device, UINT w, UINT h);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		IDXGISwapChain* mView = nullptr;
		AutoRef<IDXGISwapChain3> mView3 = nullptr;
		struct FBackBuffer
		{
			AutoRef<ITexture>	Texture;
			AutoRef<IRenderTargetView>	Rtv;
			UINT64				FenceValue = 0;
			void CreateRtvAndSrv(IGpuDevice* device, UINT index);
		};
		std::vector<FBackBuffer>		BackBuffers;
		UINT							CurrentBackBuffer = 0;

		AutoRef<IFence>			FramePresentFence;
	};
}

NS_END