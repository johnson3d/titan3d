#ifdef PLATFORM_WIN
#define VK_USE_PLATFORM_WIN32_KHR
#endif

#include "IVKSwapChain.h"
#include "IVKRenderSystem.h"
#include "IVKRenderContext.h"
#include "IVKShaderResourceView.h"
#include "IVKTexture2D.h"

#define new VNEW

NS_BEGIN

IVKSwapChain::IVKSwapChain()
{
	mSwapChain = nullptr;
	mSurface = nullptr;
	mCurrentFrame = 0;
	mCurrentImageIndex = 0;
}

IVKSwapChain::~IVKSwapChain()
{
	Cleanup();
}

void IVKSwapChain::Cleanup()
{
	for (size_t i = 0; i < mSwapChainTextures.size(); i++)
	{
		mSwapChainTextures[i]->Release();
	}
	mSwapChainTextures.clear();
	auto pRC = mRenderContext.GetPtr();
	if (pRC == nullptr)
		return;
	if (mSwapChain != nullptr)
	{
		vkDestroySwapchainKHR(pRC->mLogicalDevice, mSwapChain, nullptr);
		mSwapChain = nullptr;
	}
	if (mSurface != nullptr)
	{
		auto pSys = pRC->mRenderSystem.GetPtr();
		if (pSys != nullptr)
		{
			vkDestroySurfaceKHR(pSys->mVKInstance, mSurface, nullptr);
		}
		mSurface = nullptr;
	}
	for (size_t i = 0; i < mAvailableSemaphores.size(); i++)
	{
		vkDestroySemaphore(pRC->mLogicalDevice, mAvailableSemaphores[i], pRC->GetVkAllocCallBacks());
	}
	mAvailableSemaphores.clear();
}

UINT IVKSwapChain::GetBackBufferNum()
{
	return (UINT)mSwapChainTextures.size();
}

ITexture2D* IVKSwapChain::GetBackBuffer(UINT index)
{
	return (ITexture2D*)mSwapChainTextures[index]->GetGpuBuffer();
}

bool IVKSwapChain::CheckSwapSurfaceFormat(const VkSurfaceFormatKHR& format,const std::vector<VkSurfaceFormatKHR>& availableFormats)
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

bool IVKSwapChain::Init(IVKRenderContext* rc, const ISwapChainDesc* desc)
{
	mDesc = *desc;
	mRenderContext.FromObject(rc);
	auto pSys = rc->mRenderSystem.GetPtr();
#ifdef PLATFORM_WIN
	{
		VkWin32SurfaceCreateInfoKHR createInfo_surf = {};
		createInfo_surf.sType = VK_STRUCTURE_TYPE_WIN32_SURFACE_CREATE_INFO_KHR;
		createInfo_surf.pNext = nullptr;
		createInfo_surf.hinstance = nullptr;
		createInfo_surf.hwnd = (HWND)desc->WindowHandle;
		vkCreateWin32SurfaceKHR(pSys->mVKInstance, &createInfo_surf, nullptr, &mSurface);
	}
#endif

	auto scs = pSys->QuerySwapChainSupport(rc->mPhysicalDevice, mSurface);

	VkSurfaceFormatKHR surfaceFormat;
	surfaceFormat.format = Format2VKFormat(desc->Format);
	surfaceFormat.colorSpace = ColorSpace2VKFormat(desc->ColorSpace);

	if (false == CheckSwapSurfaceFormat(surfaceFormat, scs.formats))
		return false;

	// TODO: chooseSwapPresentMode
	VkPresentModeKHR presentMode = VK_PRESENT_MODE_MAILBOX_KHR;

	// TODO: chooseSwapExtent
	VkExtent2D extent;
	extent.width = desc->Width;
	extent.height = desc->Height;

	uint32_t imageCount = scs.capabilities.minImageCount + 1;
	if (scs.capabilities.maxImageCount > 0 && imageCount > scs.capabilities.maxImageCount) {
		imageCount = scs.capabilities.maxImageCount;
	}

	VkSwapchainCreateInfoKHR createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR;
	createInfo.surface = mSurface;

	createInfo.minImageCount = imageCount;
	createInfo.imageFormat = surfaceFormat.format;
	createInfo.imageColorSpace = surfaceFormat.colorSpace;
	createInfo.imageExtent = extent;
	createInfo.imageArrayLayers = 1;
	createInfo.imageUsage = VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;

	auto indices = pSys->FindQueueFamilies(rc->mPhysicalDevice, mSurface);
	uint32_t queueFamilyIndices[] = { indices.graphicsFamily, indices.presentFamily };

	if (indices.graphicsFamily != indices.presentFamily) {
		createInfo.imageSharingMode = VK_SHARING_MODE_CONCURRENT;
		createInfo.queueFamilyIndexCount = 2;
		createInfo.pQueueFamilyIndices = queueFamilyIndices;
	}
	else {
		createInfo.imageSharingMode = VK_SHARING_MODE_EXCLUSIVE;
	}

	createInfo.preTransform = scs.capabilities.currentTransform;
	createInfo.compositeAlpha = VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR;
	createInfo.presentMode = presentMode;
	createInfo.clipped = VK_TRUE;

	createInfo.oldSwapchain = VK_NULL_HANDLE;

	if (vkCreateSwapchainKHR(rc->mLogicalDevice, &createInfo, rc->GetVkAllocCallBacks(), &mSwapChain) != VK_SUCCESS) 
	{
		return false;
	}

	std::vector<VkImage> swapChainImages;
	{	
		vkGetSwapchainImagesKHR(rc->mLogicalDevice, mSwapChain, &imageCount, nullptr);	
		swapChainImages.resize(imageCount);
		vkGetSwapchainImagesKHR(rc->mLogicalDevice, mSwapChain, &imageCount, swapChainImages.data());
	}

	for (size_t i = 0; i < swapChainImages.size(); i++)
	{
		auto pTex = new IVKTexture2D();
		ITexture2DDesc rtDesc;
		rtDesc.SetDefault();
		rtDesc.Format = desc->Format;
		rtDesc.Width = desc->Width;
		rtDesc.Height = desc->Height;
		rtDesc.MipLevels = 1;
		rtDesc.ArraySize = 1;
		pTex->InitVkImage(rc, swapChainImages[i], &rtDesc);
		//pTex->TransitionImageLayout(rc, swapChainImages[i], surfaceFormat.format, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_PRESENT_SRC_KHR);//VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL
		pTex->TransitionImageLayout(rc, swapChainImages[i], 0, 1, surfaceFormat.format, VK_IMAGE_LAYOUT_UNDEFINED, VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL);
		auto pSrv = new IVKShaderResourceView();
		IShaderResourceViewDesc srvdesc;
		srvdesc.SetTexture2D();
		srvdesc.mGpuBuffer = pTex;
		srvdesc.Format = desc->Format;
		srvdesc.Texture2D.MipLevels = 1;
		if (pSrv->Init(rc, &srvdesc) == false)
		{
			ASSERT(false);
		}
		pTex->Release();
		mSwapChainTextures.push_back(pSrv);
	}

	mAvailableSemaphores.resize(MAX_FRAMES_IN_FLIGHT);
	VkSemaphoreCreateInfo semaphoreInfo{};
	semaphoreInfo.sType = VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO;
	for (size_t i = 0; i < MAX_FRAMES_IN_FLIGHT; i++) 
	{
		if (vkCreateSemaphore(rc->mLogicalDevice, &semaphoreInfo, rc->GetVkAllocCallBacks(), &mAvailableSemaphores[i]) != VK_SUCCESS ) 
		{
			return false;
		}
	}
	mCurrentFrame = 0;
	vkAcquireNextImageKHR(rc->mLogicalDevice, mSwapChain, UINT64_MAX, mAvailableSemaphores[mCurrentFrame], VK_NULL_HANDLE, &mCurrentImageIndex);
	return true;
}

void IVKSwapChain::BindCurrent()
{

}

void IVKSwapChain::Present(UINT SyncInterval, UINT Flags)
{
	auto context = mRenderContext.GetPtr();
	//vkAcquireNextImageKHR(context->mLogicalDevice, mSwapChain, UINT64_MAX, mAvailableSemaphores[mCurrentFrame], VK_NULL_HANDLE, &mCurrentImageIndex);
	VkPresentInfoKHR presentInfo{};
	presentInfo.sType = VK_STRUCTURE_TYPE_PRESENT_INFO_KHR;

	VkSemaphore waitSemaphores[] = { mAvailableSemaphores[mCurrentFrame] };
	presentInfo.waitSemaphoreCount = 1;
	presentInfo.pWaitSemaphores = waitSemaphores;

	VkSwapchainKHR swapChains[] = { mSwapChain };
	presentInfo.swapchainCount = 1;
	presentInfo.pSwapchains = swapChains;

	presentInfo.pImageIndices = &mCurrentImageIndex;
	auto result = vkQueuePresentKHR(context->mPresentQueue, &presentInfo);
	if (result == VK_ERROR_OUT_OF_DATE_KHR || result == VK_SUBOPTIMAL_KHR)
	{
		
	}
	mCurrentFrame = (mCurrentFrame + 1) % MAX_FRAMES_IN_FLIGHT;	
	result = vkAcquireNextImageKHR(context->mLogicalDevice, mSwapChain, UINT64_MAX, mAvailableSemaphores[mCurrentFrame], VK_NULL_HANDLE, &mCurrentImageIndex);
}

void IVKSwapChain::OnLost()
{

}

vBOOL IVKSwapChain::OnRestore(const ISwapChainDesc* desc)
{
	if (mRenderContext.GetPtr() == nullptr)
		return FALSE;
	mDesc = *desc;
	
	Cleanup();
	return Init(mRenderContext.GetPtr(), desc) ? 1 : 0;
}

NS_END