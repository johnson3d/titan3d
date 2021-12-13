#include "IVKShaderResourceView.h"
#include "IVKRenderContext.h"
#include "IVKTexture2D.h"
#include "IVKGpuBuffer.h"

#define new VNEW

NS_BEGIN

IVKShaderResourceView::IVKShaderResourceView()
{
	mImageView = nullptr;
}


IVKShaderResourceView::~IVKShaderResourceView()
{
	switch (mSrvDesc.Type)
	{
		case ST_Texture2D:
		{
			if (mImageView != nullptr)
			{
				PostVkExecute([view = mImageView](IVKRenderContext* rc)
					{
						vkDestroyImageView(rc->mLogicalDevice, view, rc->GetVkAllocCallBacks());
					});
				mImageView = nullptr;
			}						
		}
		break;
		case ST_BufferSRV:
		{
			if (mBufferView != nullptr)
			{
				PostVkExecute([view = mBufferView](IVKRenderContext* rc)
					{
						vkDestroyBufferView(rc->mLogicalDevice, view, rc->GetVkAllocCallBacks());
					});

				mBufferView = nullptr;
			}
		}
		break;
	}	
}

bool IVKShaderResourceView::Init(IVKRenderContext* rc, const IShaderResourceViewDesc* desc)
{
	this->mSrvDesc = *desc;
	mRenderContext.FromObject(rc);
	mBuffer.StrongRef(desc->mGpuBuffer);

	switch (desc->Type)
	{
		case ST_Texture2D:
		{
			auto pTexture = ((IVKTexture2D*)desc->mGpuBuffer);
			auto pImage = ((IVKTexture2D*)desc->mGpuBuffer)->mVkImage;
			VkImageViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
			createInfo.image = pImage;
			createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
			createInfo.format = Format2VKFormat(desc->Format);
			createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
			switch (desc->Format)
			{
				case EPixelFormat::PXF_D16_UNORM:
				case EPixelFormat::PXF_D32_FLOAT:
					{
						createInfo.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_DEPTH_BIT;
					}
					break;
				case EPixelFormat::PXF_D24_UNORM_S8_UINT:				
				case EPixelFormat::PXF_D32_FLOAT_S8X24_UINT:
					{
						createInfo.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_DEPTH_BIT;
						//createInfo.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_STENCIL_BIT;
						//createInfo.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_COLOR_BIT;
					}
					break;
				default:
					{
						createInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
					}
					break;
			}
			
			createInfo.subresourceRange.baseMipLevel = 0;
			createInfo.subresourceRange.levelCount = 1;
			createInfo.subresourceRange.baseArrayLayer = 0;
			createInfo.subresourceRange.layerCount = 1;

			if (vkCreateImageView(rc->mLogicalDevice, &createInfo, rc->GetVkAllocCallBacks(), &mImageView) != VK_SUCCESS)
			{
				return false;
			}
			mTxDesc.Width = pTexture->mTextureDesc.Width;
			mTxDesc.Height = pTexture->mTextureDesc.Height;
			return true;
		}
		case ST_BufferSRV:
		{
			//https://zhuanlan.zhihu.com/p/46731936
			if (desc->Format != EPixelFormat::PXF_UNKNOWN)
			{
				auto pBuffer = ((IVKGpuBuffer*)desc->mGpuBuffer);
				VkBufferViewCreateInfo createInfo = {};
				createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
				createInfo.buffer = pBuffer->mBuffer;
				//createInfo.flags = 0;//reserved
				createInfo.format = Format2VKFormat(desc->Format);
				createInfo.offset = desc->Buffer.ElementOffset;
				createInfo.range = pBuffer->mBufferDesc.ByteWidth;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
				if (vkCreateBufferView(rc->mLogicalDevice, &createInfo, rc->GetVkAllocCallBacks(), &mBufferView) != VK_SUCCESS)
				{
					return false;
				}
			}
			else
			{
				//as IVKGpuBuffer: mBuffer.StrongRef(desc->mGpuBuffer);
			}
			return true;
		}
		default:
			break;
	}

	return false;
}

void* IVKShaderResourceView::GetAPIObject()
{
	if (mImageView != nullptr)
		return mImageView;
	return mBuffer.UnsafeConvertTo<IVKGpuBuffer>()->mBuffer;
	/*if (mSrvDesc.Type == ST_BufferSRV)
	{
		if (mSrvDesc.Format == PXF_UNKNOWN)
			return mBuffer.UnsafeConvertTo<IVKGpuBuffer>()->mBuffer;
	}
	return mImageView;*/
}

bool IVKShaderResourceView::UpdateBuffer(IRenderContext* rc1, const IGpuBuffer* buffer)
{
	mBuffer.StrongRef((IGpuBuffer*)buffer);

	ASSERT(buffer != nullptr);

	IVKRenderContext* rc = (IVKRenderContext*)rc1;

	if (mSrvDesc.Type == ST_Texture2D)
	{
		auto pTexture = ((IVKTexture2D*)buffer);
		auto pImage = pTexture->mVkImage;
		VkImageViewCreateInfo createInfo = {};
		createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
		createInfo.image = pImage;
		createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
		createInfo.format = Format2VKFormat(mSrvDesc.Format);
		createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
		switch (mSrvDesc.Format)
		{
		case EPixelFormat::PXF_D16_UNORM:
		case EPixelFormat::PXF_D32_FLOAT:
		{
			createInfo.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_DEPTH_BIT;
		}
		break;
		case EPixelFormat::PXF_D24_UNORM_S8_UINT:
		case EPixelFormat::PXF_D32_FLOAT_S8X24_UINT:
		{
			createInfo.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_DEPTH_BIT;
			//createInfo.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_STENCIL_BIT;
			//createInfo.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_COLOR_BIT;
		}
		break;
		default:
		{
			createInfo.subresourceRange.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
		}
		break;
		}

		createInfo.subresourceRange.baseMipLevel = 0;
		createInfo.subresourceRange.levelCount = 1;
		createInfo.subresourceRange.baseArrayLayer = 0;
		createInfo.subresourceRange.layerCount = 1;

		if (mImageView != nullptr)
		{
			PostVkExecute([view = mImageView](IVKRenderContext* rc)
				{
					vkDestroyImageView(rc->mLogicalDevice, view, rc->GetVkAllocCallBacks());
				});
		}
		if (vkCreateImageView(rc->mLogicalDevice, &createInfo, rc->GetVkAllocCallBacks(), &mImageView) != VK_SUCCESS)
		{
			return false;
		}
		mTxDesc.Width = pTexture->mTextureDesc.Width;
		mTxDesc.Height = pTexture->mTextureDesc.Height;
		return true;
	}
	else
	{
		if (mSrvDesc.Format != EPixelFormat::PXF_UNKNOWN)
		{
			auto pBuffer = ((IVKGpuBuffer*)buffer);
			VkBufferViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
			createInfo.buffer = pBuffer->mBuffer;
			//createInfo.flags = 0;//reserved
			createInfo.format = Format2VKFormat(mSrvDesc.Format);
			createInfo.offset = mSrvDesc.Buffer.ElementOffset;
			createInfo.range = pBuffer->mBufferDesc.ByteWidth;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;

			if (mBufferView != nullptr)
			{
				PostVkExecute([view = mBufferView](IVKRenderContext* rc)
					{
						vkDestroyBufferView(rc->mLogicalDevice, view, rc->GetVkAllocCallBacks());
					});
			}
			if (vkCreateBufferView(rc->mLogicalDevice, &createInfo, rc->GetVkAllocCallBacks(), &mBufferView) != VK_SUCCESS)
			{
				return false;
			}
		}
		else
		{
			//as IVKGpuBuffer: mBuffer.StrongRef(desc->mGpuBuffer);
		}
	}
	return true;
}

NS_END