#pragma once
#include "../NxFrameBuffers.h"
#include "NullPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class NullGpuDevice;
	class NullFrameBuffers : public IFrameBuffers
	{
	public:
		NullFrameBuffers();
		~NullFrameBuffers();
		virtual void FlushModify() override;
	public:
	}; 

	class NullSwapChain : public ISwapChain
	{
	public:
		NullSwapChain();
		~NullSwapChain();
		virtual void* GetHWBuffer() override {
			return nullptr;
		}
		bool Init(NullGpuDevice* device, const FSwapChainDesc& desc);
		virtual UINT GetBackBufferCount() const override
		{
			return 0;
		}
		virtual ITexture* GetBackBuffer(UINT index) override;
		virtual IRenderTargetView* GetBackRTV(UINT index) override;
		virtual UINT GetCurrentBackBuffer() override;
		virtual void Present(IGpuDevice* device, UINT SyncInterval, UINT Flags) override;
		virtual bool Resize(IGpuDevice* device, UINT w, UINT h) override;
	public:
	};
}

NS_END