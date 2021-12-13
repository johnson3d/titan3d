#include "IVKTexture2D.h"
#include "IVKRenderContext.h"
#include "IVKCommandList.h"

#define new VNEW

NS_BEGIN

IVKTexture2D::IVKTexture2D()
{
	mVkImage = nullptr;
	mImageMemory = nullptr;
	mCurLayout = VkImageLayout::VK_IMAGE_LAYOUT_UNDEFINED;
}

IVKTexture2D::~IVKTexture2D()
{
	auto rc = mContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mDestroyImage && mVkImage != nullptr)
	{	
		PostVkExecute([image = mVkImage, ImageMemory = mImageMemory](IVKRenderContext* rc)
			{
				vkDestroyImage(rc->mLogicalDevice, image, rc->GetVkAllocCallBacks());
				VK_FreeGpuMemory(rc, ImageMemory);
			});		
		mVkImage = nullptr;
	}
	mImageMemory = nullptr;
}

bool IVKTexture2D::Init(IVKRenderContext* rc, const ITexture2DDesc* desc)
{
	mContext.FromObject(rc);
	mDestroyImage = true;
	mTextureDesc = *desc;

	VkImageCreateInfo imageInfo = {};
	imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
	imageInfo.imageType = VK_IMAGE_TYPE_2D;
	imageInfo.extent.width = mTextureDesc.Width;
	imageInfo.extent.height = mTextureDesc.Height;
	imageInfo.extent.depth = 1;
	imageInfo.mipLevels = mTextureDesc.MipLevels;
	imageInfo.arrayLayers = mTextureDesc.ArraySize;
	imageInfo.format = Format2VKFormat(mTextureDesc.Format);
	if (VK_FORMAT_UNDEFINED == rc->FindSupportedFormat({ imageInfo.format, },
		VK_IMAGE_TILING_OPTIMAL,
		VK_FORMAT_FEATURE_SAMPLED_IMAGE_BIT))
	{
		imageInfo.format = Format2VKFormat(mTextureDesc.Format);
	}
	imageInfo.tiling = VK_IMAGE_TILING_OPTIMAL;
	imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
	if (desc->BindFlags & BF_RENDER_TARGET)
	{
		if (desc->CPUAccess == 0)
		{
			imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;//VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
		}
		imageInfo.usage = VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
	}
	else if (desc->BindFlags & BF_DEPTH_STENCIL)
	{
		imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;//VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
		imageInfo.usage = VK_IMAGE_USAGE_SAMPLED_BIT | VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT;

		if (imageInfo.format == VkFormat::VK_FORMAT_R16_UNORM)
		{
			imageInfo.format = VkFormat::VK_FORMAT_D16_UNORM;
		}
	}
	else if (desc->BindFlags & BF_SHADER_RES)
	{
		if (desc->CPUAccess == CAS_READ)
		{
			imageInfo.initialLayout = VK_IMAGE_LAYOUT_READ_ONLY_OPTIMAL_KHR;
		}
		else if (desc->CPUAccess == CAS_WRITE)
		{
			//imageInfo.initialLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
			imageInfo.usage |= VK_IMAGE_USAGE_TRANSFER_DST_BIT;
		}
		if (desc->InitData != nullptr)
		{
			imageInfo.usage |= VK_IMAGE_USAGE_TRANSFER_DST_BIT;
		}
		imageInfo.usage |= VK_IMAGE_USAGE_SAMPLED_BIT;
		if (desc->BindFlags & BF_UNORDERED_ACCESS)
		{
			imageInfo.usage |= VK_IMAGE_USAGE_STORAGE_BIT;
		}
		imageInfo.usage |= VK_IMAGE_USAGE_TRANSFER_DST_BIT;
	}
	else
	{
		imageInfo.usage = VK_IMAGE_USAGE_SAMPLED_BIT;
	}
	imageInfo.samples = VK_SAMPLE_COUNT_1_BIT;
	imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
	imageInfo.flags |= VK_IMAGE_CREATE_MUTABLE_FORMAT_BIT;
	if (VK_SUCCESS != vkCreateImage(rc->mLogicalDevice, &imageInfo, rc->GetVkAllocCallBacks(), &mVkImage))
		return false;

	mCurLayout = imageInfo.initialLayout;

	VkMemoryRequirements memRequirements;
	vkGetImageMemoryRequirements(rc->mLogicalDevice, mVkImage, &memRequirements);

	VkMemoryAllocateInfo allocInfo{};
	allocInfo.sType = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;

	auto memSize = ((memRequirements.size + memRequirements.alignment - 1) / memRequirements.alignment) * memRequirements.alignment;
	allocInfo.allocationSize = memSize;
	allocInfo.memoryTypeIndex = VK_FindMemoryType(rc, memRequirements.memoryTypeBits,
		VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);
		//VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);//VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT | 

	mImageMemory = VK_AllocGpuMemory(rc, &allocInfo);
	if (mImageMemory == nullptr)
	{
		return false;
	}

	vkBindImageMemory(rc->mLogicalDevice, mVkImage, mImageMemory->GetDeviceMemory(), mImageMemory->Offset);

	if (desc->InitData != nullptr)
	{
		auto vkCmd = VK_BeginSingleTimeCommands(rc);
		TransitionImageLayout(rc, mVkImage, 0, mTextureDesc.MipLevels, imageInfo.format, imageInfo.initialLayout, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, vkCmd);

		auto CurWidth = desc->Width;
		auto CurHeight = desc->Height;
		std::vector<VkBuffer> tmpBuffers;
		std::vector<VKGpuMemory*> tmpMemory;		
		for (UINT i = 0; i < desc->MipLevels; i++)
		{
			auto& mipData = desc->InitData[i];

			auto byteSize = mipData.SysMemPitch * CurHeight;
			VkBuffer stagingBuffer;
			VKGpuMemory* stagingBufferMemory;
			VK_CreateBuffer(rc, byteSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT, VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, stagingBuffer, stagingBufferMemory);

			void* data;
			vkMapMemory(rc->mLogicalDevice, stagingBufferMemory->GetDeviceMemory(), stagingBufferMemory->Offset, byteSize, 0, &data);
			memcpy(data, desc->InitData->pSysMem, byteSize);
			vkUnmapMemory(rc->mLogicalDevice, stagingBufferMemory->GetDeviceMemory());
			
			VK_CopyBufferToImage(rc, stagingBuffer, mVkImage, i, CurWidth, CurHeight, vkCmd);

			tmpBuffers.push_back(stagingBuffer);
			tmpMemory.push_back(stagingBufferMemory);
			
			if (CurHeight >= 2)
				CurHeight = CurHeight / 2;
			if (CurWidth >= 2)
				CurWidth = CurWidth / 2;
		}
		
		TransitionImageLayout(rc, mVkImage, 0, mTextureDesc.MipLevels, imageInfo.format, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, vkCmd);
		VK_EndSingleTimeCommands(rc, vkCmd);

		for (auto i : tmpBuffers)
		{
			vkDestroyBuffer(rc->mLogicalDevice, i, rc->GetVkAllocCallBacks());
		}
		tmpBuffers.clear();
		for (auto i : tmpMemory)
		{
			VK_FreeGpuMemory(rc, i);
		}
		tmpMemory.clear();
	}
	else
	{
		auto vkCmd = VK_BeginSingleTimeCommands(rc);
		TransitionImageToLayout(rc, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, vkCmd);
		VK_EndSingleTimeCommands(rc, vkCmd);
	}
	return mVkImage != nullptr;
}

bool IVKTexture2D::InitVkImage(IVKRenderContext* rc, VkImage pTex2d, const ITexture2DDesc* desc)
{
	mTextureDesc = *desc;
	mContext.FromObject(rc);
	mVkImage = pTex2d;
	mDestroyImage = false;

	VkMemoryRequirements memRequirements;
	vkGetImageMemoryRequirements(rc->mLogicalDevice, mVkImage, &memRequirements);

	//D3D11_TEXTURE2D_DESC desc;
	//pTex2d->GetDesc(&desc);
	//mDesc.Width = desc.Width;
	//mDesc.Height = desc.Height;
	//mDesc.MipLevels = desc.MipLevels;
	//mDesc.ArraySize = desc.ArraySize;
	//mDesc.Format = VkFormat2Format(desc.Format);
	//mDesc.BindFlags = desc.BindFlags;
	//mDesc.CPUAccess = desc.CPUAccessFlags;// CAS_WRITE | CAS_READ;
	//mDesc.InitData = nullptr;
	//ASSERT(mDesc.Format != PXF_UNKNOWN);

	
	return true;
}

//VkCommandBuffer IVKTexture2D::BeginSingleTimeCommands(IVKRenderContext* rc)
//{
//	VkCommandBufferAllocateInfo allocInfo{};
//	allocInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
//	allocInfo.level = VK_COMMAND_BUFFER_LEVEL_PRIMARY;
//	allocInfo.commandPool = rc->mCommandPool;
//	allocInfo.commandBufferCount = 1;
//
//	VkCommandBuffer commandBuffer;
//	vkAllocateCommandBuffers(rc->mLogicalDevice, &allocInfo, &commandBuffer);
//
//	VkCommandBufferBeginInfo beginInfo{};
//	beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
//	beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
//
//	vkBeginCommandBuffer(commandBuffer, &beginInfo);
//
//	return commandBuffer;
//}
//
//void IVKTexture2D::EndSingleTimeCommands(IVKRenderContext* rc, VkCommandBuffer commandBuffer) 
//{
//	vkEndCommandBuffer(commandBuffer);
//
//	VkSubmitInfo submitInfo{};
//	submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;
//	submitInfo.commandBufferCount = 1;
//	submitInfo.pCommandBuffers = &commandBuffer;
//
//	vkQueueSubmit(rc->mGraphicsQueue, 1, &submitInfo, VK_NULL_HANDLE);
//	vkQueueWaitIdle(rc->mGraphicsQueue);
//
//	vkFreeCommandBuffers(rc->mLogicalDevice, rc->mCommandPool, 1, &commandBuffer);
//}

void IVKTexture2D::TransitionImageToLayout(IVKRenderContext* rc, VkImageLayout newLayout, VkCommandBuffer cmdBuffer)
{
	if (mCurLayout == newLayout)
		return;

	TransitionImageLayout(rc, mVkImage, 0, mTextureDesc.MipLevels, Format2VKFormat(mTextureDesc.Format), mCurLayout, newLayout, cmdBuffer);
}

void IVKTexture2D::TransitionImageLayout(IVKRenderContext* rc, VkImage image, UINT mipLevel, UINT mipCount, 
			VkFormat format, VkImageLayout oldLayout, VkImageLayout newLayout, VkCommandBuffer commandBuffer)
{
	bool IsImmTransition = false;
	if (commandBuffer == nullptr)
	{
		IsImmTransition = true;
		commandBuffer = VK_BeginSingleTimeCommands(rc);
	}

	VkImageMemoryBarrier barrier{};
	barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
	barrier.oldLayout = oldLayout;
	barrier.newLayout = newLayout;
	barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
	barrier.image = image;
	if (mTextureDesc.BindFlags & BF_DEPTH_STENCIL)
	{
		switch (format)
		{
		case VK_FORMAT_D16_UNORM:
			barrier.subresourceRange.aspectMask |= (VK_IMAGE_ASPECT_DEPTH_BIT);
			break;
		case VK_FORMAT_X8_D24_UNORM_PACK32:
			barrier.subresourceRange.aspectMask |= (VK_IMAGE_ASPECT_DEPTH_BIT);
			break;
		case VK_FORMAT_D32_SFLOAT:
			barrier.subresourceRange.aspectMask |= (VK_IMAGE_ASPECT_DEPTH_BIT);
			break;
		case VK_FORMAT_S8_UINT:
			barrier.subresourceRange.aspectMask |= (VK_IMAGE_ASPECT_STENCIL_BIT);
			break;
		case VK_FORMAT_D16_UNORM_S8_UINT:
			barrier.subresourceRange.aspectMask |= (VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT);
			break;
		case VK_FORMAT_D24_UNORM_S8_UINT:
			barrier.subresourceRange.aspectMask |= (VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT);
			break;
		case VK_FORMAT_D32_SFLOAT_S8_UINT:
			barrier.subresourceRange.aspectMask |= (VK_IMAGE_ASPECT_DEPTH_BIT | VK_IMAGE_ASPECT_STENCIL_BIT);
			break;
		default:
			barrier.subresourceRange.aspectMask |= (VK_IMAGE_ASPECT_DEPTH_BIT);
			break;
		}
	}
	else
	{
		barrier.subresourceRange.aspectMask |= VK_IMAGE_ASPECT_COLOR_BIT;
	}
	barrier.subresourceRange.baseArrayLayer = 0;
	barrier.subresourceRange.layerCount = mTextureDesc.ArraySize;

	barrier.subresourceRange.baseMipLevel = mipLevel;
	barrier.subresourceRange.levelCount = mipCount;

	VkPipelineStageFlags sourceStage = 0;
	VkPipelineStageFlags destinationStage = 0;

	if (oldLayout == VK_IMAGE_LAYOUT_UNDEFINED && newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL) {
		barrier.srcAccessMask = 0;
		barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;

		sourceStage = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		destinationStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL) {
		barrier.srcAccessMask = 0;
		barrier.dstAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;

		sourceStage = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		destinationStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL) {
		barrier.srcAccessMask = VK_ACCESS_TRANSFER_WRITE_BIT;
		barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;

		sourceStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
		destinationStage = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_UNDEFINED && newLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		barrier.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		sourceStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
		destinationStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_UNDEFINED && newLayout == VK_IMAGE_LAYOUT_PRESENT_SRC_KHR)
	{
		barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		barrier.dstAccessMask = VK_ACCESS_TRANSFER_READ_BIT;
		sourceStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
		destinationStage = VK_PIPELINE_STAGE_TRANSFER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
		barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
		sourceStage = VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT;
		destinationStage = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
		sourceStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
		destinationStage = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
		barrier.dstAccessMask = VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
		sourceStage = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
		destinationStage = VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL && newLayout == VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
		barrier.dstAccessMask = VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
		sourceStage = VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
		destinationStage = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_UNDEFINED && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
		barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
		sourceStage = VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
		destinationStage = VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
	}
	else if (oldLayout == VK_IMAGE_LAYOUT_PREINITIALIZED && newLayout == VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL)
	{
		barrier.srcAccessMask = VK_ACCESS_SHADER_READ_BIT;
		barrier.dstAccessMask = VK_ACCESS_SHADER_READ_BIT;
		sourceStage = VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
		destinationStage = VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
	}
	else
	{
		ASSERT(false);
		return;
	}

	vkCmdPipelineBarrier(
		commandBuffer,
		sourceStage, destinationStage,
		0,
		0, nullptr,
		0, nullptr,
		1, &barrier
	);

	mCurLayout = newLayout;

	if (IsImmTransition)
		VK_EndSingleTimeCommands(rc, commandBuffer);
}

vBOOL IVKTexture2D::MapMipmap(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch)
{
	//auto rc = cmd->GetContext().UnsafeConvertTo<ID11RenderContext>();
	//auto pContext = rc->mHardwareContext;

	//rc->mHWContextLocker.Lock();
	////auto pContext = ((ID11CommandList*)cmd)->mDeferredContext;
	//D3D11_MAPPED_SUBRESOURCE mapped;
	//if (pContext->Map(m_pDX11Texture2D, 0, D3D11_MAP_READ, 0, &mapped) != S_OK)
	//{
	//	rc->mHWContextLocker.Unlock();
	//	return FALSE;
	//}
	//*ppData = mapped.pData;
	//*pRowPitch = mapped.RowPitch;
	//*pDepthPitch = mapped.DepthPitch;
	return TRUE;
}

void IVKTexture2D::UnmapMipmap(ICommandList* cmd, int MipLevel)
{
	//auto rc = cmd->GetContext().UnsafeConvertTo<ID11RenderContext>();
	//auto pContext = rc->mHardwareContext;
	////auto pContext = ((ID11CommandList*)cmd)->mDeferredContext;
	//pContext->Unmap(m_pDX11Texture2D, 0);
	//rc->mHWContextLocker.Unlock();
}

void IVKTexture2D::UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch)
{
	auto pContext = mContext.GetPtr();

	VkBuffer stagingBuffer;
	VKGpuMemory* stagingBufferMemory;
	VkDeviceSize imageSize = Pitch * height;// * GetPixelByteWidth(mTextureDesc.Format);///???
	VK_CreateBuffer(pContext, imageSize, VK_BUFFER_USAGE_TRANSFER_SRC_BIT, VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, stagingBuffer, stagingBufferMemory);
	
	void* data;
	vkMapMemory(pContext->mLogicalDevice, stagingBufferMemory->GetDeviceMemory(), stagingBufferMemory->Offset, imageSize, 0, &data);
	memcpy(data, pData, static_cast<size_t>(imageSize));
	vkUnmapMemory(pContext->mLogicalDevice, stagingBufferMemory->GetDeviceMemory());

	//IVKCommandList* vkCmd = (IVKCommandList*)cmd;
	VkBufferImageCopy region{};
	region.bufferOffset = 0;
	region.bufferRowLength = 0;
	region.bufferImageHeight = 0;
	region.imageSubresource.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT;
	region.imageSubresource.mipLevel = level;	
	region.imageSubresource.baseArrayLayer = 0;
	region.imageSubresource.layerCount = 1;
	region.imageOffset = { 0, 0, 0 };
	region.imageExtent = 
	{
		width,
		height,
		1
	};

	auto vkCmd = VK_BeginSingleTimeCommands(pContext);
	this->TransitionImageToLayout(pContext, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, vkCmd);
	vkCmdCopyBufferToImage(vkCmd, stagingBuffer, mVkImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &region);
	this->TransitionImageToLayout(pContext, VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL, vkCmd);
	VK_EndSingleTimeCommands(pContext, vkCmd);

	vkDestroyBuffer(pContext->mLogicalDevice, stagingBuffer, nullptr);
	VK_FreeGpuMemory(pContext, stagingBufferMemory);
}

NS_END