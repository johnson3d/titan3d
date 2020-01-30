#ifdef PLATFORM_WIN
#define VK_USE_PLATFORM_WIN32_KHR
#endif

#include "IVKSwapChain.h"
#include "IVKRenderSystem.h"
#include "IVKRenderContext.h"
#include "IVKShaderResourceView.h"

#define new VNEW

NS_BEGIN

IVKSwapChain::IVKSwapChain()
{
	mSwapChain = nullptr;
	mSurface = nullptr;
}


IVKSwapChain::~IVKSwapChain()
{
	for (size_t i = 0; i < mSwapChainTextures.size(); i++)
	{
		mSwapChainTextures[i]->mImage = nullptr;
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
}

ITexture2D* IVKSwapChain::GetTexture2D()
{
	return nullptr;
}

bool IVKSwapChain::CheckSwapSurfaceFormat(const VkSurfaceFormatKHR& format,const std::vector<VkSurfaceFormatKHR>& availableFormats)
{
	for (const auto& availableFormat : availableFormats) {
		if (availableFormat.format == format.format && availableFormat.colorSpace == format.colorSpace) {
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

	VkPresentModeKHR presentMode = VK_PRESENT_MODE_MAILBOX_KHR;
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

	if (vkCreateSwapchainKHR(rc->mLogicalDevice, &createInfo, nullptr, &mSwapChain) != VK_SUCCESS) 
	{
		return false;
	}

	vkGetSwapchainImagesKHR(rc->mLogicalDevice, mSwapChain, &imageCount, nullptr);

	std::vector<VkImage> swapChainImages;
	swapChainImages.resize(imageCount);
	vkGetSwapchainImagesKHR(rc->mLogicalDevice, mSwapChain, &imageCount, swapChainImages.data());

	for (size_t i = 0; i < swapChainImages.size(); i++)
	{
		auto pTexture = new IVKShaderResourceView();
		ISRVDesc srvdesc;
		srvdesc.Format = desc->Format;
		srvdesc.Texture2D.MipLevels = 1;
		srvdesc.ViewDimension = RESOURCE_DIMENSION_TEXTURE2D;
		pTexture->Init(rc, swapChainImages[i], &srvdesc);
		mSwapChainTextures.push_back(pTexture);
	}
	return true;
}

void IVKSwapChain::BindCurrent()
{

}

void IVKSwapChain::Present(UINT SyncInterval, UINT Flags)
{

}

void IVKSwapChain::OnLost()
{

}

vBOOL IVKSwapChain::OnRestore(const ISwapChainDesc* desc)
{
	return TRUE;
}

NS_END