#pragma once
#include "../NxFrameBuffers.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	class VKTexture;
	class VKRenderTargetView;
	class VKSrView;
	class VKFence;
	class VKBinaryFence;
	class VKGpuToHostFence;
	class VKRenderPass : public IRenderPass
	{
	public:
		VKRenderPass();
		~VKRenderPass();
		bool Init(VKGpuDevice* device, const FRenderPassDesc& desc);
	public:
		TObjectHandle<VKGpuDevice>		mDeviceRef;
		VkRenderPass		mRenderPass = nullptr;
	};
	class VKFrameBuffers : public IFrameBuffers
	{
	public:
		VKFrameBuffers();
		~VKFrameBuffers();
		virtual void FlushModify() override;

		void DestroyFrameBuffer();
	public:
		TObjectHandle<VKGpuDevice>		mDeviceRef;
		VkFramebuffer		mFrameBuffer = nullptr;
	}; 

	class VKSwapChain : public ISwapChain
	{
	public:
		VKSwapChain();
		~VKSwapChain();
		virtual void* GetHWBuffer() override {
			return mSwapChain;
		}
		bool Init(VKGpuDevice* device, const FSwapChainDesc& desc);
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
		TObjectHandle<VKGpuDevice>		mDeviceRef;
		VkSurfaceKHR					mSurface = nullptr;
		VkSwapchainKHR					mSwapChain = nullptr;
		VkSurfaceCapabilitiesKHR		mCapabilities{};
		struct FBackBuffer : public VIUnknown
		{
			FBackBuffer();
			void Cleanup(VKGpuDevice* device);
			AutoRef<VKBinaryFence>		AcquireSemaphore;
			AutoRef<VKBinaryFence>		RenderFinishSemaphore;
			AutoRef<VKGpuToHostFence>	RenderFinishFence;
			AutoRef<ITexture>			Texture;
			AutoRef<IRenderTargetView>	Rtv;
			void CreateRtvAndSrv(IGpuDevice* device);
		};
		std::vector<AutoRef<FBackBuffer>>		BackBuffers;
		UINT							CurrentBackBuffer = 0;
		UINT							CurrentFrame = 0;
		AutoRef<IFence>					PresentFence;
	};
}

NS_END