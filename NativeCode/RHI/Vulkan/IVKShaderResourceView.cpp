#include "IVKShaderResourceView.h"
#include "IVKRenderContext.h"

#define new VNEW

NS_BEGIN

IVKShaderResourceView::IVKShaderResourceView()
{
	mImageView = nullptr;
	mImage = nullptr;
}


IVKShaderResourceView::~IVKShaderResourceView()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mImageView != nullptr)
	{
		vkDestroyImageView(rc->mLogicalDevice, mImageView, nullptr);
		mImageView = nullptr;
	}
	if (mImage != nullptr)
	{
		vkDestroyImage(rc->mLogicalDevice, mImage, nullptr);
		mImage = nullptr;
	}
}

bool IVKShaderResourceView::Init(IVKRenderContext* rc, const IShaderResourceViewDesc* desc)
{
	mRenderContext.FromObject(rc);

	return true;
}

bool IVKShaderResourceView::Init(IVKRenderContext* rc, VkImage pBuffer, const ISRVDesc* desc)
{
	mRenderContext.FromObject(rc);
	mImage = pBuffer;

	VkImageViewCreateInfo createInfo = {};
	createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
	createInfo.image = mImage;
	createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
	createInfo.format = Format2VKFormat(desc->Format);
	createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
	createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
	createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
	createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
	createInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	createInfo.subresourceRange.baseMipLevel = 0;
	createInfo.subresourceRange.levelCount = 1;
	createInfo.subresourceRange.baseArrayLayer = 0;
	createInfo.subresourceRange.layerCount = 1;

	if (vkCreateImageView(rc->mLogicalDevice, &createInfo, nullptr, &mImageView) != VK_SUCCESS) 
	{
		return false;
	}

	return true;
}

NS_END