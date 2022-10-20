#include "VKFrameBuffers.h"
#include "VKBuffer.h"
#include "VKGpuDevice.h"
#include "VKEvent.h"
#include "VKCommandList.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	template<>
	struct AuxGpuResourceDestroyer<VkFramebuffer>
	{
		static void Destroy(VkFramebuffer obj, IGpuDevice* device1)
		{
			VKGpuDevice* device = (VKGpuDevice*)device1;
			vkDestroyFramebuffer(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<VkSurfaceKHR>
	{
		static void Destroy(VkSurfaceKHR obj, IGpuDevice* device1)
		{
			VKGpuDevice* device = (VKGpuDevice*)device1;
			vkDestroySurfaceKHR(device->GetVkInstance(), obj, device->GetVkAllocCallBacks());
		}
	}; 
	template<>
	struct AuxGpuResourceDestroyer<VkSwapchainKHR>
	{
		static void Destroy(VkSwapchainKHR obj, IGpuDevice* device1)
		{
			VKGpuDevice* device = (VKGpuDevice*)device1;
			vkDestroySwapchainKHR(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	}; 
	template<>
	struct AuxGpuResourceDestroyer<VkRenderPass>
	{
		static void Destroy(VkRenderPass obj, IGpuDevice* device1)
		{
			VKGpuDevice* device = (VKGpuDevice*)device1;
			vkDestroyRenderPass(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<VkSemaphore>
	{
		static void Destroy(VkSemaphore obj, IGpuDevice* device1)
		{
			VKGpuDevice* device = (VKGpuDevice*)device1;
			vkDestroySemaphore(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<VkFence>
	{
		static void Destroy(VkFence obj, IGpuDevice* device1)
		{
			VKGpuDevice* device = (VKGpuDevice*)device1;
			vkDestroyFence(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};

	inline VkAttachmentLoadOp FrameBufferLoadAction2VK(EFrameBufferLoadAction action)
	{
		switch (action)
		{
			case EFrameBufferLoadAction::LoadActionDontCare:
				return VkAttachmentLoadOp::VK_ATTACHMENT_LOAD_OP_DONT_CARE;
			case EFrameBufferLoadAction::LoadActionLoad:
				return VkAttachmentLoadOp::VK_ATTACHMENT_LOAD_OP_LOAD;
			case EFrameBufferLoadAction::LoadActionClear:
				return VkAttachmentLoadOp::VK_ATTACHMENT_LOAD_OP_CLEAR;
			default:
				return VkAttachmentLoadOp::VK_ATTACHMENT_LOAD_OP_DONT_CARE;
		}
	}
	inline VkAttachmentStoreOp FrameBufferStoreAction2VK(EFrameBufferStoreAction action)
	{
		switch (action)
		{
			case EFrameBufferStoreAction::StoreActionDontCare:
				return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_DONT_CARE;
			case EFrameBufferStoreAction::StoreActionStore:
				return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_STORE;
			case EFrameBufferStoreAction::StoreActionMultisampleResolve:
				return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_MAX_ENUM;
			case EFrameBufferStoreAction::StoreActionStoreAndMultisampleResolve:
				return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_MAX_ENUM;
			case EFrameBufferStoreAction::StoreActionUnknown:
				return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_MAX_ENUM;
			default:
				return VkAttachmentStoreOp::VK_ATTACHMENT_STORE_OP_MAX_ENUM;
		}
	}
	VKRenderPass::VKRenderPass()
	{

	}
	VKRenderPass::~VKRenderPass()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		device->DelayDestroy(mRenderPass);
		mRenderPass = nullptr;
	}
	bool VKRenderPass::Init(VKGpuDevice* device, const FRenderPassDesc& desc)
	{
		Desc = desc;
		mDeviceRef.FromObject(device);

		std::vector<VkAttachmentDescription> attachments;
		std::vector<VkAttachmentReference> attachmentRefs;
		VkAttachmentReference depthAttachmentRef{};
		for (UINT i = 0; i < desc.NumOfMRT; i++)
		{
			VkAttachmentDescription colorAttachment{};

			colorAttachment.format = Format2VKFormat(desc.AttachmentMRTs[i].Format);
			colorAttachment.samples = VK_SAMPLE_COUNT_1_BIT;
			colorAttachment.loadOp = FrameBufferLoadAction2VK(desc.AttachmentMRTs[i].LoadAction);
			colorAttachment.storeOp = FrameBufferStoreAction2VK(desc.AttachmentMRTs[i].StoreAction);
			colorAttachment.stencilLoadOp = FrameBufferLoadAction2VK(desc.AttachmentMRTs[i].StencilLoadAction);
			colorAttachment.stencilStoreOp = FrameBufferStoreAction2VK(desc.AttachmentMRTs[i].StencilStoreAction);
			colorAttachment.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
			if (desc.AttachmentMRTs[i].IsSwapChain)
			{
				colorAttachment.finalLayout = VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;
			}
			else
			{
				colorAttachment.finalLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;//VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;
			}
			attachments.push_back(colorAttachment);

			VkAttachmentReference colorAttachmentRef{};
			colorAttachmentRef.attachment = i;
			colorAttachmentRef.layout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
			attachmentRefs.push_back(colorAttachmentRef);
		}

		if (desc.AttachmentDepthStencil.Format != PXF_UNKNOWN)
		{
			VkAttachmentDescription depthAttachment{};
			depthAttachment.format = Format2VKFormat(desc.AttachmentDepthStencil.Format);
			depthAttachment.samples = VK_SAMPLE_COUNT_1_BIT;
			depthAttachment.loadOp = FrameBufferLoadAction2VK(desc.AttachmentDepthStencil.LoadAction);;
			depthAttachment.storeOp = FrameBufferStoreAction2VK(desc.AttachmentDepthStencil.StoreAction);
			depthAttachment.stencilLoadOp = FrameBufferLoadAction2VK(desc.AttachmentDepthStencil.StencilLoadAction);;
			depthAttachment.stencilStoreOp = FrameBufferStoreAction2VK(desc.AttachmentDepthStencil.StencilStoreAction);;
			depthAttachment.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
			depthAttachment.finalLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

			depthAttachmentRef.attachment = (UINT)attachments.size();
			depthAttachmentRef.layout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;

			attachments.push_back(depthAttachment);
		}

		VkSubpassDescription subpass{};
		subpass.pipelineBindPoint = VK_PIPELINE_BIND_POINT_GRAPHICS;
		subpass.colorAttachmentCount = desc.NumOfMRT;
		if (desc.NumOfMRT > 0)
			subpass.pColorAttachments = &attachmentRefs[0];
		else
			subpass.pColorAttachments = nullptr;
		if (desc.AttachmentDepthStencil.Format != PXF_UNKNOWN)
		{
			subpass.pDepthStencilAttachment = &depthAttachmentRef;
		}
		//subpass.inputAttachmentCount TBDR GPU optimizer
		//subpass.pInputAttachments
		//https://zhuanlan.zhihu.com/p/131392827
		//https://stackoverflow.com/questions/66461389/vksubpassdependency-required-for-depth-attachment-but-not-for-color-attachment

		VkSubpassDependency dependency[2]{};
		dependency[0].dependencyFlags = VK_DEPENDENCY_BY_REGION_BIT;
		dependency[0].srcSubpass = VK_SUBPASS_EXTERNAL;
		dependency[0].dstSubpass = 0;
		dependency[0].srcStageMask = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;// VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT; //VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT | VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
		dependency[0].dstStageMask = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;// VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;// VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT | VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
		dependency[0].srcAccessMask = 0;// VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;		
		dependency[0].dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;// VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;//VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		
		dependency[1].dependencyFlags = VK_DEPENDENCY_BY_REGION_BIT;
		dependency[1].srcSubpass = VK_SUBPASS_EXTERNAL;
		dependency[1].dstSubpass = 0;
		dependency[1].srcStageMask = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;// VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT; //VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT | VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
		dependency[1].dstStageMask = VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;// VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;// VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT | VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT;
		dependency[1].srcAccessMask = 0;// VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;		
		dependency[1].dstAccessMask = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;// VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT | VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;//VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;

		VkRenderPassCreateInfo renderPassInfo{};
		renderPassInfo.sType = VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO;
		renderPassInfo.attachmentCount = static_cast<uint32_t>(attachments.size());
		renderPassInfo.pAttachments = &attachments[0];
		renderPassInfo.subpassCount = 1;
		renderPassInfo.pSubpasses = &subpass;
		if (desc.AttachmentDepthStencil.Format != PXF_UNKNOWN)
		{
			if (desc.NumOfMRT == 0)
			{
				renderPassInfo.dependencyCount = 1;
				renderPassInfo.pDependencies = &dependency[1];
			}
			else
			{
				renderPassInfo.dependencyCount = 2;
				renderPassInfo.pDependencies = dependency;
			}
		}
		else
		{
			renderPassInfo.dependencyCount = 1;
			renderPassInfo.pDependencies = dependency;
		}
		

		if (vkCreateRenderPass(device->mDevice, &renderPassInfo, device->GetVkAllocCallBacks(), &mRenderPass) != VK_SUCCESS)
			return false;
		return true;

	}
	VKFrameBuffers::VKFrameBuffers()
	{

	}
	VKFrameBuffers::~VKFrameBuffers()
	{
		DestroyFrameBuffer();
	}
	void VKFrameBuffers::DestroyFrameBuffer()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		device->DelayDestroy(mFrameBuffer);
		mFrameBuffer = nullptr;
	}
	void VKFrameBuffers::FlushModify()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		
		DestroyFrameBuffer();

		auto NumRTV = mRenderPass->Desc.NumOfMRT;
		std::vector<VkImageView> attachments;

		UINT width, height;
		if (NumRTV > 0)
		{
			width = mRenderTargets[0]->Desc.Width;
			height = mRenderTargets[0]->Desc.Height;
		}
		else if (mRenderPass->Desc.AttachmentDepthStencil.Format != PXF_UNKNOWN && mDepthStencilView != nullptr)
		{
			width = mDepthStencilView->Desc.Width;
			height = mDepthStencilView->Desc.Height;
		}
		else
		{
			ASSERT(false);
		}
		for (UINT RTVIdx = 0; RTVIdx < NumRTV; RTVIdx++)
		{
			auto refRTV = mRenderTargets[RTVIdx];
			if (refRTV == nullptr)
			{
				break;
			}
			ASSERT(refRTV->GetHWBuffer() != nullptr);
			auto dxRtv = (VkImageView)refRTV->GetHWBuffer();
			attachments.push_back(dxRtv);
		}

		if (mRenderPass->Desc.AttachmentDepthStencil.Format != PXF_UNKNOWN && mDepthStencilView != nullptr)
		{
			auto dxDsv = (VkImageView)mDepthStencilView->GetHWBuffer();
			attachments.push_back(dxDsv);
		}

		VkFramebufferCreateInfo framebufferInfo{};
		framebufferInfo.sType = VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO;
		framebufferInfo.renderPass = mRenderPass.UnsafeConvertTo<VKRenderPass>()->mRenderPass;
		framebufferInfo.attachmentCount = (UINT)attachments.size(); //mRenderPass->Desc.NumOfMRT;
		framebufferInfo.pAttachments = &attachments[0];
		framebufferInfo.width = width;
		framebufferInfo.height = height;
		framebufferInfo.layers = 1;

		if (vkCreateFramebuffer(device->mDevice, &framebufferInfo, device->GetVkAllocCallBacks(), &mFrameBuffer) != VK_SUCCESS)
		{
			ASSERT(false);
			return;
		}
	}

	VKSwapChain::FBackBuffer::FBackBuffer()
	{

	}
	void VKSwapChain::FBackBuffer::CleanupVK(VKGpuDevice* device)
	{
		if (AcquireSemaphore != nullptr)
		{
			device->DelayDestroy(AcquireSemaphore);
			AcquireSemaphore = nullptr;
		}
		if (RenderFinishSemaphore != nullptr)
		{
			device->DelayDestroy(RenderFinishSemaphore);
			RenderFinishSemaphore = nullptr;
		}
	}

	VKSwapChain::VKSwapChain()
	{
		
	}

	VKSwapChain::~VKSwapChain()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		
		for (auto i : BackBuffers)
		{
			i->CleanupVK(device);
		}
		BackBuffers.clear();

		device->DelayDestroy(mSwapChain);
		mSurface = nullptr;

		device->DelayDestroy(mSurface);
		mSurface = nullptr;

		//device->DelayDestroy(AcquireImageFence);
		//AcquireImageFence = nullptr;
	}

	bool VKSwapChain::Init(VKGpuDevice* device, const FSwapChainDesc& desc)
	{
		Desc = desc;
		Desc.BufferCount = 3;
		mDeviceRef.FromObject(device);

#ifdef PLATFORM_WIN
		{
			VkWin32SurfaceCreateInfoKHR createInfo_surf = {};
			createInfo_surf.sType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR;
			createInfo_surf.pNext = nullptr;
			createInfo_surf.hinstance = nullptr;
			createInfo_surf.hwnd = (HWND)desc.OutputWindow;
			vkCreateWin32SurfaceKHR(device->GetVkInstance(), &createInfo_surf, nullptr, &mSurface);
		}
#endif
		vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device->mPhysicalDevice, mSurface, &mCapabilities);
		
		/*VkFenceCreateInfo vkfcInfo{};
		vkfcInfo.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
		vkCreateFence(device->mDevice, &vkfcInfo, device->GetVkAllocCallBacks(), &AcquireImageFence);*/

		FFenceDesc fcdesc{};
		fcdesc.InitValue = 0;
		PresentFence = MakeWeakRef(device->CreateFence(&fcdesc, "SwapChain Fence"));
		return Create(device, Desc.Width, Desc.Height);
	}
	bool CheckSwapSurfaceFormat(const VkSurfaceFormatKHR& format, const std::vector<VkSurfaceFormatKHR>& availableFormats)
	{
		for (const auto& availableFormat : availableFormats)
		{
			if (availableFormat.format == format.format && availableFormat.colorSpace == format.colorSpace)
			{
				return true;
			}
		}

		return false;
	}
	struct SwapChainSupportDetails {
		VkSurfaceCapabilitiesKHR capabilities;
		std::vector<VkSurfaceFormatKHR> formats;
		std::vector<VkPresentModeKHR> presentModes;
	};
	void QuerySwapChainSupport(VkPhysicalDevice device, VkSurfaceKHR surface, SwapChainSupportDetails& details)
	{
		vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device, surface, &details.capabilities);

		uint32_t formatCount;
		vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &formatCount, nullptr);

		if (formatCount != 0) {
			details.formats.resize(formatCount);
			vkGetPhysicalDeviceSurfaceFormatsKHR(device, surface, &formatCount, details.formats.data());
		}

		uint32_t presentModeCount;
		vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &presentModeCount, nullptr);

		if (presentModeCount != 0) {
			details.presentModes.resize(presentModeCount);
			vkGetPhysicalDeviceSurfacePresentModesKHR(device, surface, &presentModeCount, details.presentModes.data());
		}
	}
	struct QueueFamilyIndices
	{
		QueueFamilyIndices()
		{
			graphicsFamily = 0xFFFFFFFF;
			presentFamily = 0xFFFFFFFF;
		}
		uint32_t graphicsFamily;
		uint32_t presentFamily;

		bool isComplete() {
			return graphicsFamily != 0xFFFFFFFF && presentFamily != 0xFFFFFFFF;
		}
	};
	QueueFamilyIndices FindQueueFamilies(VkPhysicalDevice device, VkSurfaceKHR surface)
	{
		QueueFamilyIndices indices;

		uint32_t queueFamilyCount = 0;
		vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, nullptr);

		std::vector<VkQueueFamilyProperties> queueFamilies(queueFamilyCount);
		vkGetPhysicalDeviceQueueFamilyProperties(device, &queueFamilyCount, queueFamilies.data());

		int i = 0;
		for (const auto& queueFamily : queueFamilies) {
			if (queueFamily.queueFlags & VK_QUEUE_GRAPHICS_BIT) {
				indices.graphicsFamily = i;
			}

			VkBool32 presentSupport = false;
			vkGetPhysicalDeviceSurfaceSupportKHR(device, i, surface, &presentSupport);

			if (presentSupport) {
				indices.presentFamily = i;
			}

			if (indices.isComplete()) {
				break;
			}

			i++;
		}

		return indices;
	}
	bool VKSwapChain::Resize(IGpuDevice* device1, UINT w, UINT h)
	{
		if (Desc.Width == w && Desc.Height == h)
			return true;

		VKGpuDevice* device = (VKGpuDevice*)device1;
		
		vkDestroySwapchainKHR(device->mDevice, mSwapChain, device->GetVkAllocCallBacks());
		mSwapChain = nullptr;

		//device->DelayDestroy(mSurface);
		vkDestroySurfaceKHR(device->GetVkInstance(), mSurface, device->GetVkAllocCallBacks());
		mSurface = nullptr;

#ifdef PLATFORM_WIN
		{
			VkWin32SurfaceCreateInfoKHR createInfo_surf = {};
			createInfo_surf.sType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR;
			createInfo_surf.pNext = nullptr;
			createInfo_surf.hinstance = nullptr;
			createInfo_surf.hwnd = (HWND)Desc.OutputWindow;
			vkCreateWin32SurfaceKHR(device->GetVkInstance(), &createInfo_surf, nullptr, &mSurface);
		}
#endif
		vkGetPhysicalDeviceSurfaceCapabilitiesKHR(device->mPhysicalDevice, mSurface, &mCapabilities);

		Desc.Width = w;
		Desc.Height = h;
		ASSERT(BackBuffers.size() == Desc.BufferCount);

		VkSurfaceFormatKHR surfaceFormat;
		surfaceFormat.format = Format2VKFormat(Desc.Format);
		surfaceFormat.colorSpace = VK_COLOR_SPACE_SRGB_NONLINEAR_KHR;// ColorSpace2VKFormat(desc.ColorSpace);

		SwapChainSupportDetails scs;
		QuerySwapChainSupport(device->mPhysicalDevice, mSurface, scs);
		if (false == CheckSwapSurfaceFormat(surfaceFormat, scs.formats))
			return false;

		VkPresentModeKHR presentMode = VK_PRESENT_MODE_MAILBOX_KHR;

		VkExtent2D extent;
		extent.width = Desc.Width;
		extent.height = Desc.Height;

		VkSwapchainCreateInfoKHR createInfo = {};
		createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
		createInfo.surface = mSurface;

		createInfo.minImageCount = Desc.BufferCount;
		createInfo.imageFormat = surfaceFormat.format;
		createInfo.imageColorSpace = surfaceFormat.colorSpace;
		createInfo.imageExtent = extent;
		createInfo.imageArrayLayers = 1;
		createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

		auto queueFamilies = FindQueueFamilies(device->mPhysicalDevice, mSurface);
		uint32_t queueFamilyIndices[] = { device->mCmdQueue->mGraphicsQueueIndex, device->mCmdQueue->mPresentQueueIndex };
		queueFamilyIndices[0] = queueFamilies.graphicsFamily;
		queueFamilyIndices[1] = queueFamilies.presentFamily;
		
		if (device->mCmdQueue->GraphicsEqualPresentQueue())
		{
			createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
		}
		else
		{
			createInfo.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
			createInfo.queueFamilyIndexCount = 2;
			createInfo.pQueueFamilyIndices = queueFamilyIndices;
		}

		createInfo.preTransform = mCapabilities.currentTransform;
		createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
		createInfo.presentMode = presentMode;
		createInfo.clipped = VK_TRUE;

		createInfo.oldSwapchain = mSwapChain;//VK_NULL_HANDLE;//

		if (vkCreateSwapchainKHR(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mSwapChain) != VK_SUCCESS)
		{
			return false;
		}

		std::vector<VkImage> swapChainImages;
		{
			swapChainImages.resize(Desc.BufferCount);
			vkGetSwapchainImagesKHR(((VKGpuDevice*)device)->mDevice, mSwapChain, &Desc.BufferCount, swapChainImages.data());
		}

		auto cmd = (VKCommandList*)device->GetCmdQueue()->GetIdleCmdlist(EQueueCmdlist::QCL_Transient);
		cmd->BeginCommand();
		for (UINT i = 0; i < Desc.BufferCount; i++)
		{
			ASSERT(BackBuffers[i]->Texture);
			ASSERT(BackBuffers[i]->AcquireSemaphore);
			ASSERT(BackBuffers[i]->RenderFinishSemaphore);

			auto pDx12Texture = (VKTexture*)GetBackBuffer(i);
			pDx12Texture->mDeviceRef.FromObject(device);
			if (pDx12Texture != nullptr)
			{
				pDx12Texture->Desc.Width = Desc.Width;
				pDx12Texture->Desc.Height = Desc.Height;
				pDx12Texture->mImage = swapChainImages[i];
				pDx12Texture->GpuState = EGpuResourceState::GRS_Undefine;
				pDx12Texture->TransitionTo(cmd, EGpuResourceState::GRS_Present);
			}
			else
			{
				ASSERT(false);
			}
			BackBuffers[i]->CreateRtvAndSrv(device);
		}
		cmd->EndCommand();
		device->GetCmdQueue()->ExecuteCommandList(cmd);
		device->GetCmdQueue()->ReleaseIdleCmdlist(cmd, EQueueCmdlist::QCL_Transient);

		//CurrentBackBuffer = 0;

		return true;
	}
	bool VKSwapChain::Create(IGpuDevice* device1, UINT w, UINT h)
	{
		Desc.Width = w;
		Desc.Height = h;
		if (BackBuffers.size() != Desc.BufferCount)
		{
			BackBuffers.clear();
			BackBuffers.resize(Desc.BufferCount);
			for (UINT i = 0; i < Desc.BufferCount; i++)
			{
				BackBuffers[i] = MakeWeakRef(new FBackBuffer());
			}
		}

		VKGpuDevice* device = (VKGpuDevice*)device1;
		VkSurfaceFormatKHR surfaceFormat;
		surfaceFormat.format = Format2VKFormat(Desc.Format);
		surfaceFormat.colorSpace = VK_COLOR_SPACE_SRGB_NONLINEAR_KHR;// ColorSpace2VKFormat(desc.ColorSpace);

		SwapChainSupportDetails scs;
		QuerySwapChainSupport(device->mPhysicalDevice, mSurface, scs);
		if (false == CheckSwapSurfaceFormat(surfaceFormat, scs.formats))
			return false;

		VkPresentModeKHR presentMode = VkPresentModeKHR::VK_PRESENT_MODE_FIFO_KHR;

		VkExtent2D extent;
		extent.width = Desc.Width;
		extent.height = Desc.Height;

		VkSwapchainCreateInfoKHR createInfo = {};
		createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
		createInfo.surface = mSurface;

		createInfo.minImageCount = Desc.BufferCount;
		createInfo.imageFormat = surfaceFormat.format;
		createInfo.imageColorSpace = surfaceFormat.colorSpace;
		createInfo.imageExtent = extent;
		createInfo.imageArrayLayers = 1;
		createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

		auto queueFamilies = FindQueueFamilies(device->mPhysicalDevice, mSurface);
		uint32_t queueFamilyIndices[] = { device->mCmdQueue->mGraphicsQueueIndex, device->mCmdQueue->mPresentQueueIndex };
		queueFamilyIndices[0] = queueFamilies.graphicsFamily;
		queueFamilyIndices[1] = queueFamilies.presentFamily;
		if (device->mCmdQueue->GraphicsEqualPresentQueue())
		{
			createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
			/*createInfo.queueFamilyIndexCount = 1;
			createInfo.pQueueFamilyIndices = queueFamilyIndices;*/
		}
		else
		{
			createInfo.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
			createInfo.queueFamilyIndexCount = 2;
			createInfo.pQueueFamilyIndices = queueFamilyIndices;
		}

		createInfo.preTransform = mCapabilities.currentTransform;
		createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
		createInfo.presentMode = presentMode;
		createInfo.clipped = VK_TRUE;

		createInfo.oldSwapchain = VK_NULL_HANDLE;

		if (vkCreateSwapchainKHR(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mSwapChain) != VK_SUCCESS)
		{
			return false;
		}

		std::vector<VkImage> swapChainImages;
		{
			swapChainImages.resize(Desc.BufferCount);
			vkGetSwapchainImagesKHR(((VKGpuDevice*)device)->mDevice, mSwapChain, &Desc.BufferCount, swapChainImages.data());
		}

		auto cmd = (VKCommandList*)device->GetCmdQueue()->GetIdleCmdlist(EQueueCmdlist::QCL_Transient);
		cmd->BeginCommand();
		for (UINT i = 0; i < Desc.BufferCount; i++)
		{
			if (BackBuffers[i]->Texture == nullptr)
			{
				BackBuffers[i]->Texture = MakeWeakRef(new VKTexture());
			}
			if (BackBuffers[i]->RenderFinishSemaphore == nullptr)
			{
				/*FFenceDesc fcDesc{};
				BackBuffers[i].PresentFence = MakeWeakRef((VKFence*)device->CreateFence(&fcDesc, "Swapchain present fence"));*/

				VkSemaphoreCreateInfo info{};
				info.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
				info.flags = 0;

				BackBuffers[i]->AcquireSemaphore = MakeWeakRef(new VKBinaryFence(device));
				BackBuffers[i]->RenderFinishSemaphore = MakeWeakRef(new VKBinaryFence(device));
				BackBuffers[i]->RenderFinishFence = MakeWeakRef(new VKGpuToHostFence(device, true));
			}
			auto pDx12Texture = (VKTexture*)GetBackBuffer(i);
			pDx12Texture->mDeviceRef.FromObject(device);
			if (pDx12Texture != nullptr)
			{
				pDx12Texture->Desc.Format = Desc.Format;
				pDx12Texture->Desc.Width = Desc.Width;
				pDx12Texture->Desc.Height = Desc.Height;
				pDx12Texture->mImage = swapChainImages[i];
				//pDx12Texture->GpuState = EGpuResourceState::GRS_Present;
				pDx12Texture->TransitionTo(cmd, EGpuResourceState::GRS_Present);
			}
			else
			{
				ASSERT(false);
			}
			BackBuffers[i]->CreateRtvAndSrv(device);
		}
		cmd->EndCommand();
		device->GetCmdQueue()->ExecuteCommandList(cmd);
		device->GetCmdQueue()->ReleaseIdleCmdlist(cmd, EQueueCmdlist::QCL_Transient);

		//CurrentBackBuffer = 0;

		return true;
	}
	ITexture* VKSwapChain::GetBackBuffer(UINT index)
	{
		return BackBuffers[index]->Texture;
	}
	IRenderTargetView* VKSwapChain::GetBackRTV(UINT index)
	{
		return BackBuffers[index]->Rtv;
	}
	UINT VKSwapChain::GetCurrentBackBuffer()
	{
		//CurrentBackBuffer = CurrentBackBuffer % BackBuffers.size();
		return CurrentBackBuffer;
	}
	void VKSwapChain::BeginFrame()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		auto semaphore = BackBuffers[CurrentFrame]->AcquireSemaphore;
		auto result = vkAcquireNextImageKHR(device->mDevice, mSwapChain, UINT64_MAX, semaphore->mSemaphore, VK_NULL_HANDLE, &CurrentBackBuffer);
		ASSERT(result == VK_SUCCESS);
		auto rfinishFence = BackBuffers[CurrentBackBuffer]->RenderFinishFence;
		
		rfinishFence->Wait();
		rfinishFence->Reset();
		
		auto cmdQueue = (VKCmdQueue*)device->GetCmdQueue();
		cmdQueue->WaitFence(semaphore, 0);// semaphore->GetAspectValue());
	}
	void VKSwapChain::Present(IGpuDevice* device, UINT SyncInterval, UINT Flags)
	{
		auto cmdQueue = (VKCmdQueue*)device->GetCmdQueue();

		auto rfinishSemaphore = BackBuffers[CurrentFrame]->RenderFinishSemaphore;
		auto rfinishFence = BackBuffers[CurrentBackBuffer]->RenderFinishFence;
		
		//cmdQueue->SignalFence(semaphore, 0);// semaphore->GetAspectValue());
		
		//rfinishFence->Reset();
		auto aspect1 = cmdQueue->mQueueExecuteFence->GetAspectValue();

		cmdQueue->QueueSignal(rfinishSemaphore, 0, rfinishFence->mFence);
		
		cmdQueue->Flush();//temp code
		
		VkPresentInfoKHR presentInfo{};
		presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;

		//auto& curBackBuffer = BackBuffers[CurrentFrame];
		
		VkSwapchainKHR swapChains[] = { mSwapChain };
		presentInfo.swapchainCount = 1;
		presentInfo.pSwapchains = swapChains;
		presentInfo.pImageIndices = &CurrentBackBuffer;
		presentInfo.waitSemaphoreCount = 1;
		presentInfo.pWaitSemaphores = &rfinishSemaphore->mSemaphore;
		auto result = vkQueuePresentKHR(cmdQueue->mPresentQueue, &presentInfo);
		ASSERT(result == VK_SUCCESS);
		
		CurrentFrame = (CurrentFrame + 1) % (UINT)BackBuffers.size();

		//device->mFrameFence->WaitToAspect();
		device->GetCmdQueue()->IncreaseSignal(PresentFence);
	}
	void VKSwapChain::FBackBuffer::CreateRtvAndSrv(IGpuDevice* device)
	{
		FRtvDesc rtvDesc{};
		rtvDesc.SetTexture2D();
		rtvDesc.Width = Texture->Desc.Width;
		rtvDesc.Height = Texture->Desc.Height;
		rtvDesc.Format = Texture->Desc.Format;
		rtvDesc.Texture2D.MipSlice = 0;
		Rtv = MakeWeakRef(device->CreateRTV(Texture, &rtvDesc));
	}
}

NS_END