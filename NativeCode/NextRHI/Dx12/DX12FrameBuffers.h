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
			return mSwapChain;
		}
		bool Init(DX12GpuDevice* device, const FSwapChainDesc& desc, const char* file = nullptr, int line = 0);
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

		bool Create(IGpuDevice* device, UINT w, UINT h, const char* file = nullptr, int line = 0);
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		IDXGISwapChain* mSwapChain = nullptr;
		AutoRef<IDXGISwapChain3> mSwapChain3 = nullptr;
		struct FBackBuffer
		{
			AutoRef<ITexture>	Texture;
			AutoRef<IRenderTargetView>	Rtv;
			UINT64				FenceValue = 0;
			// back buffer 的 RTV/Texture 是替调用者创建的，file/line 必须由 CreateSwapChain 一路传下来，
			// 这样泄漏台账里才能看到是谁要的 backbuffer，而不是只有 RHI 内部位置。
			void CreateRtvAndSrv(IGpuDevice* device, UINT index, const char* file = nullptr, int line = 0);
		};
		std::vector<FBackBuffer>		BackBuffers;
		UINT							CurrentBackBuffer = 0;
		// 创建归属：Resize 时会重建 backbuffer RTV，仍延用最初 CreateSwapChain 的调用位置。
		const char*						mCreateFile = nullptr;
		int								mCreateLine = 0;

		AutoRef<IFence>			FramePresentFence;
	};
}

NS_END