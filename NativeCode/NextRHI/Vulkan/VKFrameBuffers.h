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
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;
		VkRenderPass		mRenderPass = nullptr;
	};
	class VKFrameBuffers : public IFrameBuffers
	{
	public:
		VKFrameBuffers();
		~VKFrameBuffers();
		virtual void FlushModify() override;
	public:
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;

		class FrameBufferWrapper : public IGpuResource
		{
		public:
			void Initialize(VKGpuDevice* device, VKFrameBuffers* fb);
			~FrameBufferWrapper();
			VKGpuDevice*		mDevice = nullptr;
			VkFramebuffer		mFrameBuffer = nullptr;
			
			TWeakRefHandle<IRenderTargetView>		mRenderTargets[C_MAX_MRT_NUM];
			TWeakRefHandle<IDepthStencilView>		mDepthStencilView;
		};
		
		AutoRef<FrameBufferWrapper>		mFrameBuffer;
		//mVulkanExt.IsDynamicRendering: use VK_KHR_dynamic_rendering extension, VkFramebuffer is not necessary,we can call vkCmdBeginRenderingKHR like dx12
	}; 

	class VKFrameBufferCache : public VIUnknown
	{
	public:
		struct FFrameBufferList : public VIUnknown
		{
			std::vector<AutoRef<VKFrameBuffers::FrameBufferWrapper>> FramBuffers;
			VKFrameBuffers::FrameBufferWrapper* GetOrCreate(VKFrameBuffers* fb);
		};
		std::map<VKRenderPass*, AutoRef<FFrameBufferList>> mCache;

		VKFrameBuffers::FrameBufferWrapper* GetOrCreate(VKFrameBuffers* fb);
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
		TWeakRefHandle<VKGpuDevice>		mDeviceRef;
		VkSurfaceKHR					mSurface = nullptr;
		VkSwapchainKHR					mSwapChain = nullptr;
		VkSurfaceCapabilitiesKHR		mCapabilities{};
		struct FBackBuffer : public IWeakRefObject
		{
			AutoRef<VKTexture>	Texture;
			AutoRef<VKRenderTargetView>	Rtv;
			UINT64				FenceValue = 0;
			void CreateRtvAndSrv(IGpuDevice* device, UINT index);
		};
		std::vector<FBackBuffer>		BackBuffers;
		VkFence							AcquireFence = VK_NULL_HANDLE;
		UINT							CurrentBackBuffer = 0;
		UINT							CurrentFrame = 0;
		AutoRef<IFence>					FramePresentFence;
	};
}

NS_END