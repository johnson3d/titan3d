#include "VKBuffer.h"
#include "VKCommandList.h"
#include "VKGpuDevice.h"
#include "VKEvent.h"
#include "VKDrawcall.h"
#include "../NxGeomMesh.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void VKMemoryViewWrapper::Initialize(VKGpuDevice* device)
	{
		mDeviceRef.FromObject(device);
	}
	void VKMemoryViewWrapper::FreeView()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		if (mIsBufferView)
		{
			if (mBufferView != nullptr)
			{
				vkDestroyBufferView(device->mDevice, mBufferView, device->GetVkAllocCallBacks());
				mBufferView = nullptr;
			}
		}
		else
		{
			if (mImageView != nullptr)
			{
				vkDestroyImageView(device->mDevice, mImageView, device->GetVkAllocCallBacks());
				mImageView = nullptr;
			}
		}
	}
	void VKMemoryViewWrapper::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		if (mIsBufferView)
			VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_BUFFER_VIEW, mBufferView, name);
		else
			VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_IMAGE_VIEW, mImageView, name);
	}

	VKBuffer::VKBuffer()
	{
	}

	VKBuffer::~VKBuffer()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		
		if (mBuffer)
		{
			vmaDestroyBuffer(device->mVmaAllocator, mBuffer, mAllocation);
			mBuffer = nullptr;
			mAllocation = nullptr;
		}
	}

	bool VKBuffer::Init(VKGpuDevice* device, const FBufferDesc& desc)
	{
		Desc = desc;
		if (Desc.RowPitch == 0)
			Desc.RowPitch = desc.Size;
		if (Desc.DepthPitch == 0)
			Desc.DepthPitch = desc.Size;
		Desc.InitData = nullptr;
		mDeviceRef.FromObject(device);

		UINT memFlags = 0;
		VkBufferCreateInfo bufferInfo = {};
		bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
		bufferInfo.size = desc.Size;

		VmaAllocationCreateInfo vmaInfo{};

		if (desc.Type & EBufferType::BFT_CBuffer)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;
			memFlags = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
		}
		if (desc.Type & EBufferType::BFT_UAV)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_STORAGE_TEXEL_BUFFER_BIT;
			memFlags = VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT;
			if (desc.MiscFlags & EResourceMiscFlag::RM_BUFFER_ALLOW_RAW_VIEWS)
			{
				auto pAlignment = device->GetGpuResourceAlignment();
				if (pAlignment->RawSrvUavAlignment > 0 && bufferInfo.size % pAlignment->RawSrvUavAlignment)
				{
					bufferInfo.size = (bufferInfo.size / pAlignment->RawSrvUavAlignment + 1) * pAlignment->RawSrvUavAlignment;
				}
			}
		}
		if (desc.Type & EBufferType::BFT_SRV)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT | VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
		}
		if (desc.Type & EBufferType::BFT_Vertex)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT | VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
		}
		if (desc.Type & EBufferType::BFT_Index)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDEX_BUFFER_BIT | VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
		}
		if (desc.Type & EBufferType::BFT_IndirectArgs)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDIRECT_BUFFER_BIT | VK_BUFFER_USAGE_STORAGE_TEXEL_BUFFER_BIT;
		}
		if (desc.Type & EBufferType::BFT_RTV)
		{
			ASSERT(false);
		}
		if (desc.Type & EBufferType::BFT_DSV)
		{
			ASSERT(false);
		}
		if (desc.Type & EBufferType::BFT_RTAS)
		{
			//storage of BLAS/TLAS, align with DX12 D3D12_RESOURCE_STATE_RAYTRACING_ACCELERATION_STRUCTURE
			bufferInfo.usage |= VK_BUFFER_USAGE_ACCELERATION_STRUCTURE_STORAGE_BIT_KHR;
		}
		if (device->mVulkanExt.IsRayQuery)
		{
			//BLAS build reads VB/IB, TLAS build reads instance buffer, all of them are addressed by VkDeviceAddress
			bufferInfo.usage |= VK_BUFFER_USAGE_SHADER_DEVICE_ADDRESS_BIT;
			if (desc.Type & (EBufferType::BFT_Vertex | EBufferType::BFT_Index | EBufferType::BFT_SRV))
			{
				bufferInfo.usage |= VK_BUFFER_USAGE_ACCELERATION_STRUCTURE_BUILD_INPUT_READ_ONLY_BIT_KHR;
			}
		}
		
		bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
		if (desc.Usage == EGpuUsage::USAGE_STAGING)
		{
			if (desc.CpuAccess == ECpuAccess::CAS_WRITE)
			{
				bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_SRC_BIT;
				vmaInfo.usage = VMA_MEMORY_USAGE_CPU_TO_GPU;
			}
			if (desc.CpuAccess == ECpuAccess::CAS_READ)
			{
				bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT;
				vmaInfo.usage = VMA_MEMORY_USAGE_GPU_TO_CPU;
			}
			memFlags |= (VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_CACHED_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);
			
		}
		else if (desc.Usage == EGpuUsage::USAGE_DYNAMIC)
		{
			memFlags |= (VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);
			vmaInfo.usage = VMA_MEMORY_USAGE_CPU_TO_GPU;
			vmaInfo.flags = VMA_ALLOCATION_CREATE_MAPPED_BIT;
		}
		else if(desc.Usage == EGpuUsage::USAGE_DEFAULT)
		{
			if (desc.MiscFlags & EResourceMiscFlag::RM_READ_ONLY)
			{
				//bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT;
				ASSERT(false);
			}
			else
			{
				bufferInfo.usage |= (VK_BUFFER_USAGE_TRANSFER_SRC_BIT | VK_BUFFER_USAGE_TRANSFER_DST_BIT);
			}
			memFlags |= VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT;
			vmaInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;
		}

		if (Desc.MiscFlags & EResourceMiscFlag::RM_COPY_SRC)
		{
			bufferInfo.usage |= VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
		}

		if (vmaCreateBuffer(device->mVmaAllocator, &bufferInfo, &vmaInfo, &mBuffer, &mAllocation, &mAllocationInfo) != VK_SUCCESS)
			return false;
		/*if (vkCreateBuffer(device->mDevice, &bufferInfo, device->GetVkAllocCallBacks(), &mBuffer) != VK_SUCCESS)
		{
			return false;
		}*/

		FTransientCmd tsCmd(device, QU_Transfer, "TextureInit");
		auto cmd = (VKCommandList*)tsCmd.GetCmdList();
		if (Desc.Type & EBufferType::BFT_RTAS)
		{
			GpuState = EGpuResourceState::GRS_GenericRead;
		}
		else if (Desc.Type & EBufferType::BFT_SRV)
		{
			FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_GenericRead, false);
		}
		else if (Desc.Type & EBufferType::BFT_UAV)
		{
			FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_Uav, false);
		}
		else if (Desc.Type & EBufferType::BFT_RTV)
		{
			FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_RenderTarget, false);
		}
		else if (Desc.Type & EBufferType::BFT_DSV)
		{
			FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_DepthStencil, false);
		}
		else
		{
			FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_GenericRead, false);
		}
		//GpuState = VKImageLayoutToGpuState(imageInfo.initialLayout);
		if (desc.InitData != nullptr)
		{
			if (desc.Usage == EGpuUsage::USAGE_DEFAULT)
			{
				auto copyDesc = this->Desc;
				copyDesc.Usage = EGpuUsage::USAGE_STAGING;
				copyDesc.Type = EBufferType::BFT_NONE;
				copyDesc.Size = desc.Size;
				copyDesc.InitData = desc.InitData;
				copyDesc.CpuAccess = ECpuAccess::CAS_WRITE;
				auto bf = MakeWeakRef(device->CreateBuffer(&copyDesc, __FILE__, __LINE__));

				FTransientCmd tsCmd(device, EQueueType::QU_Transfer, "VKBuffer.Init");
				auto cmd = tsCmd.GetCmdList();
				AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw(__FILE__, __LINE__));
				cpDraw->BindBufferDest(this);
				cpDraw->BindBufferSrc(bf);
				cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Buffer;
				cpDraw->FootPrint.Format = EPixelFormat::PXF_UNKNOWN;
				cpDraw->FootPrint.X = 0;
				cpDraw->FootPrint.Y = 0;
				cpDraw->FootPrint.Z = 0;
				cpDraw->FootPrint.Width = desc.Size;
				cpDraw->FootPrint.Height = 1;
				cpDraw->FootPrint.Depth = 1;
				cpDraw->FootPrint.RowPitch = desc.RowPitch;
				cpDraw->FootPrint.TotalSize = desc.Size;
				cpDraw->DstX = 0;

				cmd->PushGpuDraw(cpDraw.GetPtr());
			}
			else
			{
				FMappedSubResource subRes{};
				if (this->Map(0, &subRes, false))
				{
					//for texture upload the whole subresource data size is DepthPitch(RowPitch is bytes per row)
					auto size = std::max(desc.InitData->RowPitch, desc.InitData->DepthPitch);
					size = std::min(size, desc.Size);
					memcpy(subRes.pData, desc.InitData->pData, size);
					this->Unmap(0);
				}
			}
		}

		//ASSERT(GpuState != EGpuResourceState::GRS_Undefine);
		return true;
	}

	UINT ShaderStagesToVKStage(EShaderType stages)
	{
		UINT result = (VkPipelineStageFlagBits)0;
		if (stages & EShaderType::SDT_PixelShader)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
		if (stages & EShaderType::SDT_VertexShader)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_VERTEX_SHADER_BIT;
		if (stages & EShaderType::SDT_ComputeShader)
			result |= VkPipelineStageFlagBits::VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT;
		return result;
	}
	
	void VKBuffer::TransitionTo(ICommandList* cmd, EGpuResourceState state)
	{
		//ASSERT(state != EGpuResourceState::GRS_Undefine);
		if (state == GpuState)
			return;
		
		cmd->SetBufferBarrier(this, EPipelineStage::PPLS_ALL_COMMANDS, EPipelineStage::PPLS_ALL_COMMANDS, GpuState, state);
		GpuState = state;
	}
	void VKBuffer::UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		if (Desc.Usage == EGpuUsage::USAGE_DEFAULT)
		{
			auto device = mDeviceRef.GetPtr();

			auto copyDesc = this->Desc;
			copyDesc.Usage = EGpuUsage::USAGE_STAGING;
			copyDesc.Type = EBufferType::BFT_NONE;
			copyDesc.Size = footPrint->TotalSize;
			FMappedSubResource initData{};
			initData.pData = pData;
			initData.RowPitch = footPrint->TotalSize;
			copyDesc.InitData = &initData;
			copyDesc.CpuAccess = ECpuAccess::CAS_WRITE;

			auto bf = MakeWeakRef(device->CreateBuffer(&copyDesc, __FILE__, __LINE__));

			FTransientCmd tsCmd(device, EQueueType::QU_Transfer, "UpdateGpuData");
			auto cmd = tsCmd.GetCmdList();
			{
				AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw(__FILE__, __LINE__));
				cpDraw->BindBufferDest(this);
				cpDraw->BindBufferSrc(bf);
				cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Buffer;
				cpDraw->FootPrint.Format = EPixelFormat::PXF_UNKNOWN;
				cpDraw->FootPrint.X = 0;
				cpDraw->FootPrint.Y = 0;
				cpDraw->FootPrint.Z = 0;
				cpDraw->FootPrint.Width = footPrint->Width;
				cpDraw->FootPrint.Height = footPrint->Height;
				cpDraw->FootPrint.Depth = footPrint->Depth;
				cpDraw->FootPrint.RowPitch = footPrint->RowPitch;
				cpDraw->FootPrint.TotalSize = footPrint->RowPitch * footPrint->Height;
				cpDraw->DstX = footPrint->X;

				cmd->PushGpuDraw(cpDraw.GetPtr());
			}
		}
		else
		{
			FMappedSubResource mapped{};
			if (this->Map(subRes, &mapped, false))
			{
				memcpy(mapped.pData, pData, footPrint->RowPitch);
				this->Unmap(subRes);
			}
		}
	}
	void VKBuffer::UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		if (cmd != nullptr && Desc.Usage == EGpuUsage::USAGE_DEFAULT)
		{
			auto device = mDeviceRef.GetPtr();
			auto copyDesc = this->Desc;
			copyDesc.Usage = EGpuUsage::USAGE_STAGING;
			copyDesc.Type = EBufferType::BFT_NONE;
			copyDesc.Size = footPrint->TotalSize;
			FMappedSubResource initData{};
			initData.pData = pData;
			initData.RowPitch = copyDesc.Size;
			copyDesc.InitData = &initData;
			copyDesc.CpuAccess = ECpuAccess::CAS_WRITE;

			auto bf = MakeWeakRef(device->CreateBuffer(&copyDesc, __FILE__, __LINE__));

			//cmd->CopyBufferRegion(this, footPrint->GetOffset(), bf, 0, footPrint->TotalSize);
			{
				AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw(__FILE__, __LINE__));
				cpDraw->BindBufferDest(this);
				cpDraw->BindBufferSrc(bf);
				cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Buffer;
				cpDraw->FootPrint.Format = footPrint->Format;
				cpDraw->FootPrint.X = 0;
				cpDraw->FootPrint.Y = 0;
				cpDraw->FootPrint.Z = 0;
				cpDraw->FootPrint.Width = footPrint->Width;
				cpDraw->FootPrint.Height = footPrint->Height;
				cpDraw->FootPrint.Depth = footPrint->Depth;
				cpDraw->FootPrint.RowPitch = footPrint->RowPitch;
				cpDraw->FootPrint.TotalSize = footPrint->RowPitch * footPrint->Height;
				cpDraw->DstX = footPrint->X;

				cmd->PushGpuDraw(cpDraw.GetPtr());
			}
		}
		else
		{
			FMappedSubResource mapped{};
			if (this->Map(subRes, &mapped, false))
			{
				memcpy(mapped.pData, pData, footPrint->RowPitch);
				this->Unmap(subRes);
			}
			else
			{
				ASSERT(false);
			}
		}
	}

	bool VKBuffer::Map(UINT index, FMappedSubResource* res, bool forRead)
	{
		auto device = mDeviceRef.GetPtr();
		void* data;
		//auto hr = vkMapMemory(device->mDevice, (VkDeviceMemory)mGpuMemory->GetHWBuffer(), mGpuMemory->Offset, Desc.Size, (VkMemoryMapFlags)0, &data);
		auto hr = vmaMapMemory(device->mVmaAllocator, mAllocation, &data);
		if (VK_SUCCESS != hr)
			return false;
		
		res->pData = data;
		res->RowPitch = Desc.RowPitch;
		res->DepthPitch = Desc.DepthPitch;
		return true;
	}

	void VKBuffer::Unmap(UINT index)
	{
		auto device = mDeviceRef.GetPtr();
		//vkUnmapMemory(device->mDevice, (VkDeviceMemory)mGpuMemory->GetHWBuffer());

		VkMemoryPropertyFlags memFlags;
		vmaGetMemoryTypeProperties(device->mVmaAllocator, mAllocationInfo.memoryType, &memFlags);
		if (!(memFlags & VK_MEMORY_PROPERTY_HOST_COHERENT_BIT))
		{
			vmaFlushAllocation(device->mVmaAllocator, mAllocation, 0, Desc.Size);
		}
		vmaUnmapMemory(device->mVmaAllocator, mAllocation);
	}

	void VKBuffer::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_BUFFER, (void*)mBuffer, name);
	}
	
	VKTexture::VKTexture()
	{
	}

	VKTexture::~VKTexture()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		
		if (mImage)
		{
			//vkDestroyImage(device->mDevice, mImage, device->GetVkAllocCallBacks());
			vmaDestroyImage(device->mVmaAllocator, mImage, mAllocation);
			mImage = nullptr;
		}
	}
	
	VkImageLayout VKTexture::GetImageLayout()
	{
		return GpuStateToVKImageLayout(GpuState);
	}
	VkImageAspectFlagBits VKTexture::GetImageAspect()
	{
		return (VkImageAspectFlagBits)FormatToVKImageAspectFlags(Desc.Format, true, true);
	}
	bool VKTexture::Init(VKGpuDevice* device, const FTextureDesc& desc)
	{
		Desc = desc;
		mDeviceRef.FromObject(device);
		
		VkImageCreateInfo imageInfo = {};
		VmaAllocationCreateInfo vmaInfo{};
		imageInfo.sType = VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO;
		switch (this->GetDimension())
		{
		case 1:
			imageInfo.imageType = VK_IMAGE_TYPE_1D;
			break;
		case 2:
			imageInfo.imageType = VK_IMAGE_TYPE_2D;
			break;
		case 3:
			imageInfo.imageType = VK_IMAGE_TYPE_3D;
			break;
		}

		if (Desc.MiscFlags & EResourceMiscFlag::RM_TEXTURECUBE)
		{
			imageInfo.flags |= VK_IMAGE_CREATE_CUBE_COMPATIBLE_BIT;
			imageInfo.imageType = VK_IMAGE_TYPE_2D;
		}
		
		imageInfo.extent.width = Desc.Width;
		if (Desc.Height == 0)
			imageInfo.extent.height = 1;
		else
			imageInfo.extent.height = Desc.Height;
		if (Desc.Depth == 0)
			imageInfo.extent.depth = 1;
		else
			imageInfo.extent.depth = Desc.Depth;
		imageInfo.mipLevels = Desc.MipLevels;
		imageInfo.arrayLayers = Desc.ArraySize;
		imageInfo.format = Format2VKFormat(Desc.Format);
		/*if (VK_FORMAT_UNDEFINED == device->FindSupportedFormat({ imageInfo.format, },
			VK_IMAGE_TILING_OPTIMAL,
			VK_FORMAT_FEATURE_SAMPLED_IMAGE_BIT))
		{
			imageInfo.format = Format2VKFormat(mTextureDesc.Format);
		}*/
		imageInfo.tiling = VK_IMAGE_TILING_OPTIMAL;
		//imageInfo.tiling = VK_IMAGE_TILING_LINEAR;
		imageInfo.initialLayout = VK_IMAGE_LAYOUT_UNDEFINED;
		if (desc.MiscFlags & EResourceMiscFlag::RM_READ_ONLY)
		{
			ASSERT(false);
		}
		else
		{
			imageInfo.usage |= VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK_IMAGE_USAGE_TRANSFER_SRC_BIT;//temp for SRC
		}
		if (Desc.MiscFlags & EResourceMiscFlag::RM_COPY_SRC)
		{
			imageInfo.usage |= VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
		}

		if (Desc.BindFlags & EBufferType::BFT_SRV)
		{
			//imageInfo.initialLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
			imageInfo.usage |= VK_IMAGE_USAGE_SAMPLED_BIT;
			//GpuState = EGpuResourceState::GRS_GenericRead;
		}
		if (Desc.BindFlags & EBufferType::BFT_UAV)
		{
			//imageInfo.initialLayout = VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL;
			imageInfo.usage |= VK_IMAGE_USAGE_STORAGE_BIT;
			//GpuState = EGpuResourceState::GRS_Uav;
		}
		if (Desc.BindFlags & EBufferType::BFT_RTV)
		{
			//imageInfo.initialLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
			imageInfo.usage |= VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT;
			imageInfo.usage |= VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
			//imageInfo.initialLayout = VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
			//GpuState = EGpuResourceState::GRS_RenderTarget;
		}
		if (Desc.BindFlags & EBufferType::BFT_DSV)
		{
			//imageInfo.initialLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;//VK_IMAGE_LAYOUT_UNDEFINED;
			imageInfo.usage |= VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT;
			//imageInfo.initialLayout = VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
			//GpuState = EGpuResourceState::GRS_DepthStencil;

			/*if (imageInfo.format == VkFormat::VK_FORMAT_R16_UNORM)
			{
				imageInfo.format = VkFormat::VK_FORMAT_D16_UNORM;
			}*/
		}
		
		imageInfo.samples = (VkSampleCountFlagBits)desc.SamplerDesc.Count;//VK_SAMPLE_COUNT_1_BIT;//
		imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
		imageInfo.flags |= VK_IMAGE_CREATE_MUTABLE_FORMAT_BIT;

 		VkMemoryPropertyFlags memFlags = 0;
		if (Desc.Usage == EGpuUsage::USAGE_DEFAULT)
		{
			memFlags = VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT;
			vmaInfo.usage = VMA_MEMORY_USAGE_GPU_ONLY;
		}
		else if (Desc.Usage == EGpuUsage::USAGE_DYNAMIC)
		{
			memFlags = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
			vmaInfo.usage = VMA_MEMORY_USAGE_CPU_TO_GPU;
			vmaInfo.flags = VMA_ALLOCATION_CREATE_MAPPED_BIT;
		}
		else if (Desc.Usage == EGpuUsage::USAGE_STAGING)
		{
			memFlags = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
			if (Desc.CpuAccess == CAS_READ)
			{
				imageInfo.initialLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
				imageInfo.usage = VK_IMAGE_USAGE_TRANSFER_DST_BIT;
				vmaInfo.usage = VMA_MEMORY_USAGE_GPU_TO_CPU;
			}
			else if (Desc.CpuAccess == CAS_WRITE)
			{
				imageInfo.initialLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
				imageInfo.usage = VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
				vmaInfo.usage = VMA_MEMORY_USAGE_CPU_TO_GPU;
			}
			else
			{
				imageInfo.initialLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
			}
		}
		
		if (VK_SUCCESS != vmaCreateImage(device->mVmaAllocator, &imageInfo, &vmaInfo, &mImage, &mAllocation, &mAllocationInfo))
			return false;
		/*if (VK_SUCCESS != vkCreateImage(device->mDevice, &imageInfo, device->GetVkAllocCallBacks(), &mImage))
			return false;

		VkMemoryRequirements memRequirements;
		vkGetImageMemoryRequirements(device->mDevice, mImage, &memRequirements);

		auto memSize = ((memRequirements.size + memRequirements.alignment - 1) / memRequirements.alignment) * memRequirements.alignment;
		auto typeIndex = device->FindMemoryType(memRequirements.memoryTypeBits, memFlags);
		mGpuMemory = device->mDefaultBufferAllocator->Alloc(device, typeIndex, memSize, "Texture");
		if (mGpuMemory == nullptr)
		{
			return false;
		}

		vkBindImageMemory(device->mDevice, mImage, (VkDeviceMemory)mGpuMemory->GetHWBuffer(), mGpuMemory->Offset);*/

		GpuState = VKImageLayoutToGpuState(imageInfo.initialLayout);
		
		if (desc.InitData != nullptr)
		{
			FTransientCmd tsCmd(device, QU_Transfer, "TextureInit");
			auto cmd = (VKCommandList*)tsCmd.GetCmdList();
			if (Desc.BindFlags & EBufferType::BFT_SRV)
			{
				FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_GenericRead, false);
			}
			else if (Desc.BindFlags & EBufferType::BFT_UAV)
			{
				FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_Uav, false);
			}
			else if (Desc.BindFlags & EBufferType::BFT_RTV)
			{
				FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_RenderTarget, false);
			}
			else if (Desc.BindFlags & EBufferType::BFT_DSV)
			{
				FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_DepthStencil, false);
			}
			else
			{
				ASSERT(false);
			}
			for (UINT i = 0; i < desc.ArraySize; i++)
			{
				UINT w = Desc.Width;
				UINT h = Desc.Height;
				UINT d = (Desc.Depth == 0) ? 1 : Desc.Depth;
				for (UINT k = 0; k < desc.MipLevels; k++)
				{
					UINT j = i * Desc.MipLevels + k;
					/*VkImageSubresource subresource = {
						.aspectMask = VK_IMAGE_ASPECT_COLOR_BIT,
						.mipLevel = k,
						.arrayLayer = i
					};
					VkSubresourceLayout layout;
					vkGetImageSubresourceLayout(device->mDevice, mImage, &subresource, &layout);*/
					FBufferDesc copyDesc{};
					copyDesc.SetDefault();
					copyDesc.Usage = EGpuUsage::USAGE_STAGING;
					copyDesc.Type = EBufferType::BFT_NONE;
					//3D texture: InitData.DepthPitch is the size of one slice, upload all slices(align with DX12 CreateUploadResource)
					copyDesc.Size = desc.InitData[j].DepthPitch * d;
					FMappedSubResource sliceInitData = desc.InitData[j];
					sliceInitData.DepthPitch = copyDesc.Size;
					copyDesc.InitData = &sliceInitData;
					copyDesc.CpuAccess = ECpuAccess::CAS_WRITE;

					auto bf = MakeWeakRef(device->CreateBuffer(&copyDesc, __FILE__, __LINE__));
					FTransitionScope::TryAutoTransition(cmd, bf, EGpuResourceState::GRS_CopySrc, false);

					AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw(__FILE__, __LINE__));
					cpDraw->BindTextureDest(this);
					cpDraw->BindBufferSrc(bf);
					cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Texture;
					cpDraw->DestSubResource = j;
					cpDraw->FootPrint.Format = desc.Format;
					cpDraw->FootPrint.X = 0;
					cpDraw->FootPrint.Y = 0;
					cpDraw->FootPrint.Z = 0;
					cpDraw->FootPrint.Width = w;
					cpDraw->FootPrint.Height = h;
					cpDraw->FootPrint.Depth = d;
					cpDraw->FootPrint.RowPitch = desc.InitData[j].RowPitch;
					cpDraw->FootPrint.TotalSize = copyDesc.Size; //desc.InitData[k].RowPitch * footPrint.Footprint.Height;
					//device->mPostCmdRecorder->PushGpuDraw(cpDraw);
					cmd->PushGpuDraw(cpDraw.GetPtr());

					w = w / 2;
					h = h / 2;
					d = d / 2;
					if (w == 0)
						w = 1;
					if (h == 0)
						h = 1;
					if (d == 0)
						d = 1;
				}
			}
		}
		else
		{
			if (GpuState == EGpuResourceState::GRS_Undefine)
			{
				FTransientCmd tsCmd(device, EQueueType::QU_Transfer, "Buffer.Update");
				auto cmd = (VKCommandList*)tsCmd.GetCmdList();
				if (Desc.BindFlags & EBufferType::BFT_SRV)
				{
					FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_GenericRead, false);
				}
				else if (Desc.BindFlags & EBufferType::BFT_UAV)
				{
					FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_Uav, false);
				}
				else if (Desc.BindFlags & EBufferType::BFT_RTV)
				{
					FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_RenderTarget, false);
				}
				else if (Desc.BindFlags & EBufferType::BFT_DSV)
				{
					FTransitionScope::TryAutoTransition(cmd, this, EGpuResourceState::GRS_DepthStencil, false);
				}
				else
				{
					ASSERT(false);
				}
			}
		}

		ASSERT(GpuState != EGpuResourceState::GRS_Undefine);
			
		return true;
	}
	IGpuBufferData* VKTexture::CreateBufferData(IGpuDevice* device1, UINT mipIndex, ECpuAccess cpuAccess, FSubResourceFootPrint* outFootPrint)
	{
		auto device = (VKGpuDevice*)device1;
		VkMemoryRequirements memRequirements;
		vkGetImageMemoryRequirements(device->mDevice, mImage, &memRequirements);

		FBufferDesc desc{};
		desc.CpuAccess = cpuAccess;
		desc.Type = EBufferType::BFT_SRV;
		if (cpuAccess & ECpuAccess::CAS_READ)
		{
			desc.Usage = EGpuUsage::USAGE_STAGING;
			desc.Type = EBufferType::BFT_NONE;
		}
		else if (cpuAccess & ECpuAccess::CAS_WRITE)
			desc.Usage = EGpuUsage::USAGE_DYNAMIC;
		else
			desc.Usage = EGpuUsage::USAGE_DEFAULT;

		desc.StructureStride = GetPixelByteWidth(Desc.Format);
		desc.RowPitch = desc.StructureStride * Desc.Width;
		auto alignment = (UINT)memRequirements.alignment;
		if (desc.RowPitch % alignment > 0)
		{
			desc.RowPitch = (desc.RowPitch / alignment + 1) * alignment;
		}
		desc.DepthPitch = desc.RowPitch * Desc.Height;
		desc.Size = desc.DepthPitch;

		auto result = device->CreateBuffer(&desc, __FILE__, __LINE__);

		outFootPrint->X = 0;
		outFootPrint->Y = 0;
		outFootPrint->Z = 0;
		outFootPrint->Width = Desc.Width;
		outFootPrint->Height = Desc.Height;
		if (outFootPrint->Height == 0)
			outFootPrint->Height = 1;
		outFootPrint->Depth = Desc.Depth;
		if (outFootPrint->Depth == 0)
			outFootPrint->Depth = 1;
		outFootPrint->Format = Desc.Format;
		outFootPrint->RowPitch = desc.RowPitch;

		return result;
	}
	bool VKTexture::GetFootprint(FSubResourceFootPrint* fp, UINT64* rowSize, UINT64* totalSize, UINT subRes, UINT64 offset)
	{
		//keep the same semantic as DX12Texture::GetFootprint(GetCopyableFootprints)
		auto device = mDeviceRef.GetPtr();
		UINT mipLevels = Desc.MipLevels > 0 ? Desc.MipLevels : 1;
		UINT mip = subRes % mipLevels;
		UINT w = Desc.Width >> mip;
		if (w == 0)
			w = 1;
		UINT h = Desc.Height >> mip;
		if (h == 0)
			h = 1;
		UINT d = Desc.Depth >> mip;
		if (d == 0)
			d = 1;

		UINT pixelWidth = GetPixelByteWidth(Desc.Format);
		UINT rowPitch = pixelWidth * w;
		VkMemoryRequirements memRequirements;
		vkGetImageMemoryRequirements(device->mDevice, mImage, &memRequirements);
		auto alignment = (UINT)memRequirements.alignment;
		if (alignment > 0 && rowPitch % alignment > 0)
		{
			rowPitch = (rowPitch / alignment + 1) * alignment;
		}

		fp->Format = Desc.Format;
		fp->X = 0;
		fp->Y = 0;
		fp->Z = 0;
		fp->Width = w;
		fp->Height = h;
		fp->Depth = d;
		fp->RowPitch = rowPitch;
		fp->TotalSize = rowPitch * h;

		if (rowSize != nullptr)
			*rowSize = (UINT64)pixelWidth * w;
		if (totalSize != nullptr)
			*totalSize = (UINT64)rowPitch * h * d;

		return true;
	}
	bool VKTexture::Map(UINT subRes, FMappedSubResource* res, bool forRead)
	{
		auto device = mDeviceRef.GetPtr();

		VkImageSubresource imgSubRes{};
		if (Desc.BindFlags & EBufferType::BFT_SRV)
			imgSubRes.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, false);
		else
			imgSubRes.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, true);
		
		imgSubRes.mipLevel = subRes % Desc.MipLevels;
		imgSubRes.arrayLayer = subRes / Desc.MipLevels;
		VkSubresourceLayout subLayout{};
		vkGetImageSubresourceLayout(device->mDevice, mImage, &imgSubRes, &subLayout);

		void* data;
		//auto hr = vkMapMemory(device->mDevice, (VkDeviceMemory)mGpuMemory->GetHWBuffer(), mGpuMemory->Offset, subLayout.size, (VkMemoryMapFlags)0, &data);
		auto hr = vmaMapMemory(device->mVmaAllocator, mAllocation, &data);
		if (hr != VK_SUCCESS)
		{
			return false;
		}
		res->pData = (BYTE*)data + subLayout.offset;
		res->RowPitch = (UINT)subLayout.rowPitch;
		res->DepthPitch = (UINT)subLayout.depthPitch;
		return true;
	}
	void VKTexture::Unmap(UINT subRes)
	{
		//UINT subRes = mipIndex + Desc.MipLevels * arrayIndex;
		//mGpuResource->Unmap(subRes, nullptr);
		auto device = mDeviceRef.GetPtr();

		VkImageSubresource imgSubRes{};
		if (Desc.BindFlags & EBufferType::BFT_SRV)
			imgSubRes.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, false);
		else
			imgSubRes.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, true);
		imgSubRes.mipLevel = subRes % Desc.MipLevels;
		imgSubRes.arrayLayer = subRes / Desc.MipLevels;
		VkSubresourceLayout subLayout{};
		vkGetImageSubresourceLayout(device->mDevice, mImage, &imgSubRes, &subLayout);

		//vkUnmapMemory(device->mDevice, (VkDeviceMemory)mGpuMemory->GetHWBuffer());
		VkMemoryPropertyFlags memFlags;
		vmaGetMemoryTypeProperties(device->mVmaAllocator, mAllocationInfo.memoryType, &memFlags);
		if (!(memFlags & VK_MEMORY_PROPERTY_HOST_COHERENT_BIT))
		{
			vmaFlushAllocation(device->mVmaAllocator, mAllocation, subLayout.offset, subLayout.depthPitch);
		}
		vmaUnmapMemory(device->mVmaAllocator, mAllocation);
	}
	void VKTexture::UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		auto device = mDeviceRef.GetPtr();

		FBufferDesc copyDesc{};
		copyDesc.SetDefault();
		copyDesc.Usage = EGpuUsage::USAGE_STAGING;
		copyDesc.Type = EBufferType::BFT_NONE;
		copyDesc.Size = footPrint->TotalSize;
		FMappedSubResource initData{};
		initData.pData = pData;
		initData.RowPitch = footPrint->TotalSize;
		copyDesc.InitData = &initData;
		copyDesc.CpuAccess = ECpuAccess::CAS_WRITE;

		auto bf = MakeWeakRef(device->CreateBuffer(&copyDesc, __FILE__, __LINE__));

		AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw(__FILE__, __LINE__));
		cpDraw->BindTextureDest(this);
		cpDraw->BindBufferSrc(bf);
		cpDraw->DestSubResource = subRes;
		cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Texture;
		cpDraw->FootPrint.Format = footPrint->Format;
		// 透传目标偏移, 支持局部区域上传 (全量上传的调用方 X/Y/Z 为 0, 行为不变)。
		cpDraw->FootPrint.X = footPrint->X;
		cpDraw->FootPrint.Y = footPrint->Y;
		cpDraw->FootPrint.Z = footPrint->Z;
		cpDraw->FootPrint.Width = footPrint->Width;
		cpDraw->FootPrint.Height = footPrint->Height;
		cpDraw->FootPrint.Depth = footPrint->Depth;
		cpDraw->FootPrint.RowPitch = footPrint->RowPitch;
		cpDraw->FootPrint.TotalSize = footPrint->RowPitch * footPrint->Height;

		cmd->PushGpuDraw(cpDraw.GetPtr());
	}
	void VKTexture::UpdateGpuData(UINT subRes, void* pData, const FSubResourceFootPrint* footPrint)
	{
		if (Desc.Usage == EGpuUsage::USAGE_DEFAULT)
		{
			auto device = mDeviceRef.GetPtr();

			FBufferDesc copyDesc{};
			copyDesc.SetDefault();
			copyDesc.Usage = EGpuUsage::USAGE_STAGING;
			copyDesc.Type = EBufferType::BFT_NONE;
			copyDesc.Size = footPrint->TotalSize;
			FMappedSubResource initData{};
			initData.pData = pData;
			initData.RowPitch = footPrint->TotalSize;
			copyDesc.InitData = &initData;
			copyDesc.CpuAccess = ECpuAccess::CAS_WRITE;

			auto bf = MakeWeakRef(device->CreateBuffer(&copyDesc, __FILE__, __LINE__));

			AutoRef<ICopyDraw> cpDraw = MakeWeakRef(device->CreateCopyDraw(__FILE__, __LINE__));
			cpDraw->BindTextureDest(this);
			cpDraw->BindBufferSrc(bf);
			cpDraw->DestSubResource = subRes;
			cpDraw->Mode = ECopyDrawMode::CDM_Buffer2Texture;
			cpDraw->FootPrint.Format = footPrint->Format;
			// 透传目标偏移, 支持局部区域上传 (全量上传的调用方 X/Y/Z 为 0, 行为不变)。
			cpDraw->FootPrint.X = footPrint->X;
			cpDraw->FootPrint.Y = footPrint->Y;
			cpDraw->FootPrint.Z = footPrint->Z;
			cpDraw->FootPrint.Width = footPrint->Width;
			cpDraw->FootPrint.Height = footPrint->Height;
			cpDraw->FootPrint.Depth = footPrint->Depth;
			cpDraw->FootPrint.RowPitch = footPrint->RowPitch;
			cpDraw->FootPrint.TotalSize = footPrint->RowPitch * footPrint->Height;

			FTransientCmd tsCmd(device, EQueueType::QU_Transfer, "Texture.UpdateGpuData");
			auto cmd = tsCmd.GetCmdList();
			cmd->PushGpuDraw(cpDraw.GetPtr());
		}
		else //if (Desc.Usage == EGpuUsage::USAGE_DYNAMIC || Desc.Usage == EGpuUsage::USAGE_STAGING)
		{
			FMappedSubResource mapped{};
			if (this->Map(subRes, &mapped, false))
			{
				for (int i = 0; i < (int)footPrint->Height; i++)
				{
					//copy row by row
				}
				memcpy(mapped.pData, pData, footPrint->RowPitch);
				this->Unmap(subRes);
			}
		}
	}
	void VKTexture::TransitionTo(ICommandList* cmd, EGpuResourceState state)
	{
		ASSERT(state != EGpuResourceState::GRS_Undefine);
		if (state == GpuState)
			return;

		cmd->SetTextureBarrier(this, 0, Desc.MipLevels, EPipelineStage::PPLS_ALL_COMMANDS, EPipelineStage::PPLS_ALL_COMMANDS, GpuState, state);
		//VkImageMemoryBarrier barrier{};
		//barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
		//barrier.oldLayout = GpuStateToVKImageLayout(GpuState);
		//barrier.newLayout = GpuStateToVKImageLayout(state);
		////barrier.srcAccessMask
		//
		//if (barrier.oldLayout != barrier.newLayout)
		//{
		//	barrier.oldLayout = VK_IMAGE_LAYOUT_UNDEFINED;
		//	barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
		//	barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
		//	barrier.image = mImage;

		//	if (Desc.BindFlags & EBufferType::BFT_SRV)
		//		barrier.subresourceRange.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, false);
		//	else
		//		barrier.subresourceRange.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, true);
		//	barrier.subresourceRange.baseArrayLayer = 0;
		//	barrier.subresourceRange.layerCount = Desc.ArraySize;
		//	barrier.subresourceRange.baseMipLevel = 0;//All
		//	barrier.subresourceRange.levelCount = Desc.MipLevels;

		//	auto vkCmd = (VKCommandList*)cmd;

		//	VkPipelineStageFlagBits srcStages, dstStages;
		//	GpuResourceStateToVKAccessAndPipeline(GpuState, barrier.srcAccessMask, srcStages);
		//	GpuResourceStateToVKAccessAndPipeline(state, barrier.dstAccessMask, dstStages);

		//	vkCmdPipelineBarrier(
		//		vkCmd->mCommandBuffer->RealObject,
		//		srcStages, //VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
		//		dstStages, //VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,//VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
		//		0,
		//		0, nullptr,
		//		0, nullptr,
		//		1, &barrier
		//	);
		//}

		GpuState = state;
	}

	void VKTexture::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_IMAGE, mImage, name);
	}

	VKCbView::VKCbView()
	{

	}
	VKCbView::~VKCbView()
	{
		/*auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		device->DelayDestroy(mView);
		mView = nullptr;*/
	}
	bool VKCbView::Init(VKGpuDevice* device, IBuffer* pBuffer, const FCbvDesc& desc)
	{
		mDeviceRef.FromObject(device);
		ShaderBinder = desc.ShaderBinder;

		UINT alignedSize = desc.BufferSize;
		if (ShaderBinder != nullptr)
		{
			alignedSize = ShaderBinder->Size;
		}
		auto pAlignment = device->GetGpuResourceAlignment();
		if (alignedSize % pAlignment->CBufferAlignment)
		{
			alignedSize = (alignedSize / pAlignment->CBufferAlignment + 1) * pAlignment->CBufferAlignment;
		}
		//alignedSize = (alignedSize + 255) & (~255);

		if (pBuffer == nullptr)
		{
			FBufferDesc bfDesc{};
			bfDesc.SetDefault();
			bfDesc.Size = alignedSize;
			bfDesc.StructureStride = 0;
			bfDesc.InitData = nullptr;
			bfDesc.Type = EBufferType::BFT_CBuffer;
			bfDesc.Usage = EGpuUsage::USAGE_DYNAMIC;
			bfDesc.CpuAccess = ECpuAccess::CAS_WRITE;

			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc, __FILE__, __LINE__));
			ASSERT(Buffer != nullptr);
		}
		else
		{
			Buffer = pBuffer;
		}
		
		//auto bf = Buffer.UnsafeConvertTo<VKBuffer>();
		//VkBufferViewCreateInfo createInfo = {};
		//createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
		//createInfo.buffer = bf->mBuffer;
		////createInfo.flags = 0;//reserved
		//createInfo.format = VkFormat::VK_FORMAT_R8_UNORM; //Format2VKFormat(Desc.Format);
		//createInfo.offset = 0;//bf->mGpuMemory->Offset;
		//createInfo.range = alignedSize;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
		//vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView);

		return true;
	}
	VKVbView::VKVbView()
	{

	}
	VKVbView::~VKVbView()
	{

	}
	bool VKVbView::Init(VKGpuDevice* device, IBuffer* pBuffer, const FVbvDesc* desc)
	{
		Desc = *desc;
		Desc.InitData = nullptr;

		/*UINT alignedSize = desc->Size;
		auto pAlignment = device->GetGpuResourceAlignment();
		if (alignedSize % pAlignment->VbIbAlignment)
		{
			alignedSize = (alignedSize / pAlignment->VbIbAlignment + 1) * pAlignment->VbIbAlignment;
		}*/

		if (pBuffer == nullptr)
		{
			FBufferDesc bfDesc{};
			bfDesc.SetDefault();
			bfDesc.Size = desc->Size;
			bfDesc.StructureStride = desc->Stride;
			bfDesc.InitData = desc->InitData;
			bfDesc.Type = EBufferType::BFT_Vertex;
			bfDesc.Usage = desc->Usage;
			bfDesc.CpuAccess = desc->CpuAccess;
			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc, __FILE__, __LINE__));
			ASSERT(Buffer != nullptr);
		}
		else
		{
			Buffer = pBuffer;
		}

		//auto bf = Buffer.UnsafeConvertTo<VKBuffer>();
		//VkBufferViewCreateInfo createInfo = {};
		//createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
		//createInfo.buffer = bf->mBuffer;
		////createInfo.flags = 0;//reserved
		//createInfo.format = VkFormat::VK_FORMAT_R8_UNORM; //Format2VKFormat(Desc.Format);
		//createInfo.offset = 0;
		//createInfo.range = VK_WHOLE_SIZE;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
		//vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView);
		return true;
	}

	VKIbView::VKIbView()
	{

	}
	VKIbView::~VKIbView()
	{

	}
	bool VKIbView::Init(VKGpuDevice* device, IBuffer* pBuffer, const FIbvDesc* desc)
	{
		Desc = *desc;
		Desc.InitData = nullptr;

		UINT alignedSize = desc->Size;
		auto pAlignment = device->GetGpuResourceAlignment();
		if (alignedSize % pAlignment->VbIbAlignment)
		{
			alignedSize = (alignedSize / pAlignment->VbIbAlignment + 1) * pAlignment->VbIbAlignment;
		}

		if (pBuffer == nullptr)
		{
			FBufferDesc bfDesc{};
			bfDesc.SetDefault();
			bfDesc.Size = desc->Size;
			bfDesc.StructureStride = desc->Stride;
			bfDesc.InitData = desc->InitData;
			bfDesc.Type = EBufferType::BFT_Index;
			bfDesc.Usage = desc->Usage;
			bfDesc.CpuAccess = desc->CpuAccess;
			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc, __FILE__, __LINE__));
			ASSERT(Buffer != nullptr);
		}
		else
		{
			Buffer = pBuffer;
		}

		//auto bf = Buffer.UnsafeConvertTo<VKBuffer>();
		//VkBufferViewCreateInfo createInfo = {};
		//createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
		//createInfo.buffer = bf->mBuffer;
		////createInfo.flags = 0;//reserved
		//createInfo.format = VkFormat::VK_FORMAT_R8_UNORM; //Format2VKFormat(Desc.Format);
		//createInfo.offset = 0;
		//createInfo.range = VK_WHOLE_SIZE;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
		//vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView);
		return true;
	}

	VKSrView::VKSrView()
	{
		//mBufferView = nullptr;
	}

	VKSrView::~VKSrView()
	{
		mView = nullptr;
	}
	
	VkImageViewType SrvTypeToVK(ESrvType type)
	{
		switch (type)
		{
			case EngineNS::NxRHI::ST_BufferSRV:
				break;
			case EngineNS::NxRHI::ST_Texture1D:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_1D;
			case EngineNS::NxRHI::ST_Texture1DArray:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_1D_ARRAY;
			case EngineNS::NxRHI::ST_Texture2D:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_2D;
			case EngineNS::NxRHI::ST_Texture2DArray:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_2D_ARRAY;
			case EngineNS::NxRHI::ST_Texture2DMS:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_2D;
			case EngineNS::NxRHI::ST_Texture2DMSArray:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_2D_ARRAY;
			case EngineNS::NxRHI::ST_Texture3D:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_3D;
			case EngineNS::NxRHI::ST_TextureCube:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_CUBE;
			case EngineNS::NxRHI::ST_TextureCubeArray:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_CUBE_ARRAY;
			default:
				break;
		}
		ASSERT(false);
		return VkImageViewType::VK_IMAGE_VIEW_TYPE_MAX_ENUM;
	}

	void VKSrView::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		if (mView->mIsBufferView)
			VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_BUFFER_VIEW, mView->mBufferView, name);
		else
			VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_IMAGE_VIEW, mView->mImageView, name);
	}

	bool VKSrView::Init(VKGpuDevice* device, IGpuBufferData* pBuffer, const FSrvDesc& desc)
	{
		Desc = desc;
		Buffer = pBuffer;
		mDeviceRef.FromObject(device);

		mView = MakeWeakRef(new VKMemoryViewWrapper());
		mView->Initialize(device);

		if (Desc.Type == ESrvType::ST_BufferSRV || Desc.Type == ESrvType::ST_RTAS)
		{
			mView->AsBufferView((VKBuffer*)pBuffer);
			UINT alignedSize = ((VKBuffer*)pBuffer)->Desc.Size;
			VkBufferViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
			createInfo.buffer = (VkBuffer)Buffer->GetHWBuffer();

			createInfo.offset = desc.Buffer.FirstElement;// bf->mGpuMemory->Offset;
			createInfo.range = alignedSize;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
			if (desc.Format == EPixelFormat::PXF_UNKNOWN)
			{
				createInfo.format = VkFormat::VK_FORMAT_UNDEFINED; //Format2VKFormat(Desc.Format);
				//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;//flags must be 0
			}
			else
			{
				createInfo.format = Format2VKFormat(Desc.Format);
				//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
				if (VK_SUCCESS != vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView->mBufferView))
					return false;
			}
		}
		else
		{
			mView->AsTextureView((VKTexture*)pBuffer);
			auto pTexture = (VKTexture*)pBuffer;
			auto pImage = pTexture->mImage;
			VkImageViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
			createInfo.image = pImage;
			createInfo.viewType = SrvTypeToVK(desc.Type);
			createInfo.format = Format2VKFormat(desc.Format);
			createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.subresourceRange.aspectMask = FormatToVKImageAspectFlags(desc.Format, true, false);
			createInfo.subresourceRange.baseArrayLayer = 0;
			switch (desc.Type)
			{
				case ESrvType::ST_BufferSRV:
				case ESrvType::ST_RTAS:
					ASSERT(false && "Buffer SRV should not be created here");
					break;
				case ESrvType::ST_TextureCube:
					createInfo.subresourceRange.levelCount = desc.TextureCube.MipLevels;
					createInfo.subresourceRange.layerCount = 6;
					break;
				case ESrvType::ST_TextureCubeArray:
					createInfo.subresourceRange.levelCount = desc.TextureCubeArray.MipLevels;
					createInfo.subresourceRange.layerCount = 6 * desc.TextureCubeArray.NumCubes;
					break;
				case ESrvType::ST_Texture1D:
					createInfo.subresourceRange.levelCount = desc.Texture1D.MipLevels;
					createInfo.subresourceRange.layerCount = 1;
					break;
				case ESrvType::ST_Texture2D:
					createInfo.subresourceRange.levelCount = desc.Texture2D.MipLevels;
					createInfo.subresourceRange.layerCount = 1;
					break;
				case ESrvType::ST_Texture3D:
					createInfo.subresourceRange.levelCount = desc.Texture3D.MipLevels;
					createInfo.subresourceRange.layerCount = 1;
					break;
				case ESrvType::ST_Texture1DArray:
					createInfo.subresourceRange.levelCount = desc.Texture1DArray.MipLevels;
					createInfo.subresourceRange.layerCount = desc.Texture1DArray.ArraySize;
					break;
				case ESrvType::ST_Texture2DArray:
					createInfo.subresourceRange.levelCount = desc.Texture2DArray.MipLevels;
					createInfo.subresourceRange.layerCount = desc.Texture2DArray.ArraySize;
					break;
				case ESrvType::ST_Texture2DMS:
					ASSERT(pTexture->Desc.SamplerDesc.Count != 1);
					createInfo.subresourceRange.levelCount = 1;
					createInfo.subresourceRange.layerCount = 1;
					break;
				case ESrvType::ST_Texture2DMSArray:
					ASSERT(pTexture->Desc.SamplerDesc.Count != 1);
					createInfo.subresourceRange.levelCount = 1;
					createInfo.subresourceRange.layerCount = desc.Texture2DMSArray.ArraySize;
					break;
			}
			if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView->mImageView) != VK_SUCCESS)
			{
				return false;
			}
		}
		
		return true;
	}
	bool VKSrView::UpdateBuffer(IGpuDevice* device1, IGpuBufferData* pBuffer)
	{
		if (Buffer == pBuffer)
			return true;

		auto device = (VKGpuDevice*)device1;

		//device->DelayDestroy(mView);
		Buffer = pBuffer;

		mView = MakeWeakRef(new VKMemoryViewWrapper());
		mView->Initialize(device);

		switch (Desc.Type)
		{
			case ESrvType::ST_Texture1D:
				Desc.Texture1D.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_Texture1DArray:
				Desc.Texture1DArray.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_Texture2D:
				Desc.Texture2D.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_Texture2DArray:
				Desc.Texture2DArray.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_Texture3D:
				Desc.Texture3D.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_TextureCube:
				Desc.TextureCube.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			case ESrvType::ST_TextureCubeArray:
				Desc.TextureCubeArray.MipLevels = Buffer.UnsafeConvertTo<ITexture>()->Desc.MipLevels;
				break;
			default:
				break;
		}

		auto& desc = Desc;
		if (Desc.Type == ESrvType::ST_BufferSRV)
		{
			mView->AsBufferView((VKBuffer*)pBuffer);
			UINT alignedSize = ((VKBuffer*)pBuffer)->Desc.Size;
			VkBufferViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
			createInfo.buffer = (VkBuffer)Buffer->GetHWBuffer();

			createInfo.offset = desc.Buffer.FirstElement;// bf->mGpuMemory->Offset;
			createInfo.range = alignedSize;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
			if (desc.Format == EPixelFormat::PXF_UNKNOWN)
			{
				createInfo.format = VkFormat::VK_FORMAT_UNDEFINED; //Format2VKFormat(Desc.Format);
				//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;//flags must be 0
			}
			else
			{
				createInfo.format = Format2VKFormat(Desc.Format);
				//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
				if (VK_SUCCESS != vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView->mBufferView))
					return false;
			}
		}
		else
		{
			mView->AsTextureView((VKTexture*)pBuffer);
			auto pTexture = (VKTexture*)pBuffer;
			auto pImage = pTexture->mImage;
			VkImageViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
			createInfo.image = pImage;
			createInfo.viewType = SrvTypeToVK(desc.Type);
			createInfo.format = Format2VKFormat(desc.Format);
			createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.subresourceRange.aspectMask = FormatToVKImageAspectFlags(desc.Format, true, false);
			createInfo.subresourceRange.baseArrayLayer = 0;
			switch (desc.Type)
			{
			case ESrvType::ST_BufferSRV:
			case ESrvType::ST_RTAS:
				ASSERT(false && "Buffer SRV should not be created here");
				break;
			case ESrvType::ST_TextureCube:
				createInfo.subresourceRange.levelCount = desc.TextureCube.MipLevels;
				createInfo.subresourceRange.layerCount = 6;
				break;
			case ESrvType::ST_TextureCubeArray:
				createInfo.subresourceRange.levelCount = desc.TextureCubeArray.MipLevels;
				createInfo.subresourceRange.layerCount = 6 * desc.TextureCubeArray.NumCubes;
				break;
			case ESrvType::ST_Texture1D:
				createInfo.subresourceRange.levelCount = desc.Texture1D.MipLevels;
				createInfo.subresourceRange.layerCount = 1;
				break;
			case ESrvType::ST_Texture2D:
				createInfo.subresourceRange.levelCount = desc.Texture2D.MipLevels;
				createInfo.subresourceRange.layerCount = 1;
				break;
			case ESrvType::ST_Texture3D:
				createInfo.subresourceRange.levelCount = desc.Texture3D.MipLevels;
				createInfo.subresourceRange.layerCount = 1;
				break;
			case ESrvType::ST_Texture1DArray:
				createInfo.subresourceRange.levelCount = desc.Texture1DArray.MipLevels;
				createInfo.subresourceRange.layerCount = desc.Texture1DArray.ArraySize;
				break;
			case ESrvType::ST_Texture2DArray:
				createInfo.subresourceRange.levelCount = desc.Texture2DArray.MipLevels;
				createInfo.subresourceRange.layerCount = desc.Texture2DArray.ArraySize;
				break;
			case ESrvType::ST_Texture2DMS:
				ASSERT(pTexture->Desc.SamplerDesc.Count != 1);
				createInfo.subresourceRange.levelCount = 1;
				createInfo.subresourceRange.layerCount = 1;
				break;
			case ESrvType::ST_Texture2DMSArray:
				ASSERT(pTexture->Desc.SamplerDesc.Count != 1);
				createInfo.subresourceRange.levelCount = 1;
				createInfo.subresourceRange.layerCount = desc.Texture2DMSArray.ArraySize;
				break;
			}
			if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView->mImageView) != VK_SUCCESS)
			{
				return false;
			}
		}
		return true;
	}

	VKUaView::VKUaView()
	{
		mView = nullptr;
	}

	VKUaView::~VKUaView()
	{
		mView = nullptr;
	}

	VkImageViewType UavTypeToVK(EDimensionUAV type)
	{
		switch (type)
		{
			case EngineNS::NxRHI::UAV_DIMENSION_UNKNOWN:
				break;
			case EngineNS::NxRHI::UAV_DIMENSION_BUFFER:
				break;
			case EngineNS::NxRHI::UAV_DIMENSION_TEXTURE1D:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_1D;
			case EngineNS::NxRHI::UAV_DIMENSION_TEXTURE1DARRAY:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_1D_ARRAY;
			case EngineNS::NxRHI::UAV_DIMENSION_TEXTURE2D:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_2D;
			case EngineNS::NxRHI::UAV_DIMENSION_TEXTURE2DARRAY:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_2D_ARRAY;
			case EngineNS::NxRHI::UAV_DIMENSION_TEXTURE3D:
				return VkImageViewType::VK_IMAGE_VIEW_TYPE_3D;
			default:
				break;
		}
		ASSERT(false);
		return VkImageViewType::VK_IMAGE_VIEW_TYPE_MAX_ENUM;
	}
	void VKUaView::SetDebugName(const char* name)
	{
		mView->SetDebugName(name);
	}
	bool VKUaView::Init(VKGpuDevice* device, IGpuBufferData* pBuffer, const FUavDesc& desc)
	{
		Desc = desc;
		Buffer = pBuffer;
		mDeviceRef.FromObject(device);
		
		mView = MakeWeakRef(new VKMemoryViewWrapper());
		mView->Initialize(device);

		if (Desc.ViewDimension == EDimensionUAV::UAV_DIMENSION_BUFFER)
		{
			mView->AsBufferView((VKBuffer*)pBuffer);
			UINT alignedSize = ((VKBuffer*)pBuffer)->Desc.Size;
			VkBufferViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
			createInfo.buffer = (VkBuffer)Buffer->GetHWBuffer();

			createInfo.offset = desc.Buffer.FirstElement;// bf->mGpuMemory->Offset;
			createInfo.range = alignedSize;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
			if (desc.Format == EPixelFormat::PXF_UNKNOWN)
			{
				createInfo.format = VkFormat::VK_FORMAT_UNDEFINED; //Format2VKFormat(Desc.Format);
				//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;//flags must be 0
			}
			else
			{
				createInfo.format = Format2VKFormat(Desc.Format);
				//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
				if (VK_SUCCESS != vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView->mBufferView))
					return false;
			}
		}
		else
		{
			mView->AsTextureView((VKTexture*)pBuffer);
			auto pTexture = (VKTexture*)pBuffer;
			auto pImage = pTexture->mImage;
			VkImageViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
			createInfo.image = pImage;
			createInfo.viewType = UavTypeToVK(Desc.ViewDimension);
			createInfo.format = Format2VKFormat(Desc.Format);
			createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.subresourceRange.aspectMask = FormatToVKImageAspectFlags(desc.Format, true, false);
			createInfo.subresourceRange.baseArrayLayer = 0;
			switch (Desc.ViewDimension)
			{
			case EDimensionUAV::UAV_DIMENSION_BUFFER:
			case EDimensionUAV::UAV_DIMENSION_UNKNOWN:
				ASSERT(false && "Buffer SRV should not be created here");
				break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE1D:
				createInfo.subresourceRange.baseMipLevel = desc.Texture1D.MipSlice;
				createInfo.subresourceRange.levelCount = 1;
				createInfo.subresourceRange.layerCount = 1;
				break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE2D:
				createInfo.subresourceRange.baseMipLevel = desc.Texture2D.MipSlice;
				createInfo.subresourceRange.levelCount = 1;
				createInfo.subresourceRange.layerCount = 1;
				break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE3D:
				createInfo.subresourceRange.baseMipLevel = desc.Texture3D.MipSlice;
				createInfo.subresourceRange.levelCount = 1;
				createInfo.subresourceRange.layerCount = 1;
				break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE1DARRAY:
				createInfo.subresourceRange.baseMipLevel = desc.Texture1DArray.MipSlice;
				createInfo.subresourceRange.levelCount = 1;
				createInfo.subresourceRange.layerCount = desc.Texture1DArray.ArraySize;
				break;
			case EDimensionUAV::UAV_DIMENSION_TEXTURE2DARRAY:
				createInfo.subresourceRange.baseMipLevel = desc.Texture2DArray.MipSlice;
				createInfo.subresourceRange.levelCount = 1;
				createInfo.subresourceRange.layerCount = desc.Texture2DArray.ArraySize;
				break;
			}
			if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView->mImageView) != VK_SUCCESS)
			{
				return false;
			}
		}
		return true;
	}
	
	VKRenderTargetView::VKRenderTargetView()
	{
		mView = nullptr;
	}

	VKRenderTargetView::~VKRenderTargetView()
	{
		mView = nullptr;
	}

	void VKRenderTargetView::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_IMAGE_VIEW, mView, name);
	}
	bool VKRenderTargetView::Init(VKGpuDevice* device, ITexture* pBuffer, const FRtvDesc* desc)
	{
		if (desc != nullptr)
			Desc = *desc;
		GpuResource = pBuffer;
		mDeviceRef.FromObject(device);
		
		auto pTexture = (VKTexture*)pBuffer;
		auto pImage = pTexture->mImage;
		VkImageViewCreateInfo createInfo = {};
		createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
		createInfo.image = pImage;
		createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
		createInfo.format = Format2VKFormat(desc->Format);
		createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.subresourceRange.aspectMask = FormatToVKImageAspectFlags(desc->Format, true, true);

		createInfo.subresourceRange.baseMipLevel = 0;
		createInfo.subresourceRange.levelCount = 1;
		createInfo.subresourceRange.baseArrayLayer = 0;
		createInfo.subresourceRange.layerCount = 1;
		
		mView = MakeWeakRef(new VKMemoryViewWrapper());
		mView->Initialize(device);
		mView->AsTextureView(pBuffer);
		if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView->mImageView) != VK_SUCCESS)
		{
			return false;
		}

		return true;
	}

	VKDepthStencilView::VKDepthStencilView()
	{
		mView = nullptr;
	}

	VKDepthStencilView::~VKDepthStencilView()
	{
		mView = nullptr;
	}

	void VKDepthStencilView::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_OBJECT_TYPE_IMAGE_VIEW, mView, name);
	}
	bool VKDepthStencilView::Init(VKGpuDevice* device, ITexture* pBuffer, const FDsvDesc& desc)
	{
		Desc = desc;
		GpuResource = pBuffer;
		mDeviceRef.FromObject(device);

		auto pTexture = (VKTexture*)pBuffer;
		auto pImage = pTexture->mImage;
		VkImageViewCreateInfo createInfo = {};
		createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
		createInfo.image = pImage;
		createInfo.viewType = VK_IMAGE_VIEW_TYPE_2D;
		createInfo.format = Format2VKFormat(desc.Format);
		createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
		createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;

		createInfo.subresourceRange.aspectMask = FormatToVKImageAspectFlags(desc.Format, true, true);
		createInfo.subresourceRange.baseMipLevel = 0;
		createInfo.subresourceRange.levelCount = 1;
		createInfo.subresourceRange.baseArrayLayer = 0;
		createInfo.subresourceRange.layerCount = 1;

		mView = MakeWeakRef(new VKMemoryViewWrapper());
		mView->Initialize(device);
		mView->AsTextureView(pBuffer);
		if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView->mImageView) != VK_SUCCESS)
		{
			return false;
		}

		return true;
	}

	UINT64 VKBuffer::GetGPUVirtualAddress()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr || mBuffer == nullptr)
			return 0;
		VkBufferDeviceAddressInfo info{};
		info.sType = VK_STRUCTURE_TYPE_BUFFER_DEVICE_ADDRESS_INFO;
		info.buffer = mBuffer;
		return (UINT64)vkGetBufferDeviceAddress(device->mDevice, &info);
	}

	//align with DX12AccelerationStructure::Init
	VKAccelerationStructure::~VKAccelerationStructure()
	{
		auto device = mDeviceRef.GetPtr();
		if (device != nullptr && mAccelerationStructure != VK_NULL_HANDLE && device->fn_vkDestroyAccelerationStructureKHR != nullptr)
		{
			device->fn_vkDestroyAccelerationStructureKHR(device->mDevice, mAccelerationStructure, device->GetVkAllocCallBacks());
			mAccelerationStructure = VK_NULL_HANDLE;
		}
	}
	bool VKAccelerationStructure::Init(VKGpuDevice* device, const FAccelerationStructureDesc* desc)
	{
		mDeviceRef.FromObject(device);
		if (device->mVulkanExt.IsRayQuery == false)
			return false;

		mMeshes.resize(desc->GeometryCount);
		std::vector<VkAccelerationStructureGeometryKHR> geometries(desc->GeometryCount);
		std::vector<VkAccelerationStructureBuildRangeInfoKHR> ranges(desc->GeometryCount);
		std::vector<UINT> primCounts(desc->GeometryCount);
		for (UINT i = 0; i < desc->GeometryCount; i++)
		{
			mMeshes[i] = desc->Geometries[i];

			auto mesh = mMeshes[i].GetPtr();
			auto ib = mesh->GetGeomtryMesh()->IndexBuffer->Buffer.UnsafeConvertTo<VKBuffer>();
			auto vb = mesh->GetGeomtryMesh()->VertexArray->GetVB(EVertexStreamType::VST_Position)->Buffer.UnsafeConvertTo<VKBuffer>();
			bool isIndex32 = mesh->GetGeomtryMesh()->IsIndex32;
			auto indexCount = (UINT)(ib->Desc.Size / (isIndex32 ? sizeof(UINT) : sizeof(USHORT)));
			auto vertexCount = (UINT)(vb->Desc.Size / sizeof(v3dxVector3));

			auto& geomDesc = geometries[i];
			geomDesc.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_KHR;
			geomDesc.geometryType = VK_GEOMETRY_TYPE_TRIANGLES_KHR;
			if (desc->GeomFlags != nullptr)
				geomDesc.flags = (VkGeometryFlagsKHR)desc->GeomFlags[i];//ERayTracingGeomFlags values match VkGeometryFlagBitsKHR
			else
				geomDesc.flags = 0;
			auto& tri = geomDesc.geometry.triangles;
			tri.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_TRIANGLES_DATA_KHR;
			tri.vertexFormat = VK_FORMAT_R32G32B32_SFLOAT;
			tri.vertexData.deviceAddress = vb->GetGPUVirtualAddress();
			tri.vertexStride = sizeof(v3dxVector3);
			tri.maxVertex = vertexCount > 0 ? vertexCount - 1 : 0;
			tri.indexType = isIndex32 ? VK_INDEX_TYPE_UINT32 : VK_INDEX_TYPE_UINT16;
			tri.indexData.deviceAddress = ib->GetGPUVirtualAddress();
			tri.transformData.deviceAddress = 0;

			ranges[i] = {};
			ranges[i].primitiveCount = indexCount / 3;
			primCounts[i] = indexCount / 3;
		}

		VkAccelerationStructureBuildGeometryInfoKHR buildInfo{};
		buildInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_GEOMETRY_INFO_KHR;
		buildInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL_KHR;
		buildInfo.flags = VK_BUILD_ACCELERATION_STRUCTURE_PREFER_FAST_TRACE_BIT_KHR;
		buildInfo.mode = VK_BUILD_ACCELERATION_STRUCTURE_MODE_BUILD_KHR;
		buildInfo.geometryCount = (UINT)geometries.size();
		buildInfo.pGeometries = geometries.data();

		VkAccelerationStructureBuildSizesInfoKHR sizeInfo{};
		sizeInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_SIZES_INFO_KHR;
		device->fn_vkGetAccelerationStructureBuildSizesKHR(device->mDevice, VK_ACCELERATION_STRUCTURE_BUILD_TYPE_DEVICE_KHR, &buildInfo, primCounts.data(), &sizeInfo);
		mScratchSize = (UINT)sizeInfo.buildScratchSize;

		FBufferDesc bfDesc{};
		bfDesc.SetDefault(false, (EBufferType)(EBufferType::BFT_RTAS | EBufferType::BFT_UAV));
		bfDesc.Size = (UINT)sizeInfo.accelerationStructureSize;
		bfDesc.RowPitch = bfDesc.Size;
		bfDesc.DepthPitch = bfDesc.Size;
		mGpuBuffer = MakeWeakRef(device->CreateBuffer(&bfDesc, __FILE__, __LINE__));
		if (mGpuBuffer == nullptr)
			return false;

		VkAccelerationStructureCreateInfoKHR asCreateInfo{};
		asCreateInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_INFO_KHR;
		asCreateInfo.buffer = (VkBuffer)mGpuBuffer->GetHWBuffer();
		asCreateInfo.offset = 0;
		asCreateInfo.size = sizeInfo.accelerationStructureSize;
		asCreateInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_BOTTOM_LEVEL_KHR;
		if (device->fn_vkCreateAccelerationStructureKHR(device->mDevice, &asCreateInfo, device->GetVkAllocCallBacks(), &mAccelerationStructure) != VK_SUCCESS)
			return false;
		mGpuBuffer.UnsafeConvertTo<VKBuffer>()->mAccelerationStructure = mAccelerationStructure;

		bfDesc.SetDefault(false, EBufferType::BFT_UAV);
		bfDesc.Size = mScratchSize;
		bfDesc.RowPitch = bfDesc.Size;
		bfDesc.DepthPitch = bfDesc.Size;
		auto pScratchBuffer = MakeWeakRef(device->CreateBuffer(&bfDesc, __FILE__, __LINE__));

		buildInfo.dstAccelerationStructure = mAccelerationStructure;
		buildInfo.scratchData.deviceAddress = pScratchBuffer.UnsafeConvertTo<VKBuffer>()->GetGPUVirtualAddress();
		{
			FTransientCmd cmd(device, EQueueType::QU_Default, "BuildBLAStructure");
			auto cmdlist = (VKCommandList*)cmd.GetCmdList();
			cmdlist->GetCmdRecorder()->UseResource(this);
			cmdlist->GetCmdRecorder()->UseResource(pScratchBuffer);
			const VkAccelerationStructureBuildRangeInfoKHR* pRanges = ranges.data();
			device->fn_vkCmdBuildAccelerationStructuresKHR(cmdlist->GetVKCmdRecorder()->mCommandBuffer, 1, &buildInfo, &pRanges);
		}

		VkAccelerationStructureDeviceAddressInfoKHR addrInfo{};
		addrInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_DEVICE_ADDRESS_INFO_KHR;
		addrInfo.accelerationStructure = mAccelerationStructure;
		mDeviceAddress = device->fn_vkGetAccelerationStructureDeviceAddressKHR(device->mDevice, &addrInfo);
		return true;
	}
	bool VKAStructureInstance::Init(VKGpuDevice* device, const FAStructureInstanceDesc* desc, IAccelerationStructure* pAStructrure)
	{
		mDesc = *desc;
		mAStructure = pAStructrure;

		return true;
	}
	static void MakeVKInstanceDesc(VkAccelerationStructureInstanceKHR& instDesc, IAStructureInstance* i)
	{
		instDesc = {};
		instDesc.accelerationStructureReference = ((VKAccelerationStructure*)i->mAStructure.GetPtr())->mDeviceAddress;
		instDesc.flags = i->mDesc.Flags;//ERayTracingInstanceFlags values match VkGeometryInstanceFlagBitsKHR
		instDesc.instanceShaderBindingTableRecordOffset = i->mDesc.InstanceContributionToHitGroupIndex;
		instDesc.instanceCustomIndex = i->mDesc.InstanceID;
		instDesc.mask = i->mDesc.InstanceMask;
		v3dxMatrix4 tMat;
		v3dxMatrix4Transpose(&tMat, &i->mDesc.Matrix);
		memcpy(&instDesc.transform.matrix[0], &tMat.m11, sizeof(float) * 4);
		memcpy(&instDesc.transform.matrix[1], &tMat.m21, sizeof(float) * 4);
		memcpy(&instDesc.transform.matrix[2], &tMat.m31, sizeof(float) * 4);
	}
	//align with DX12TopAccelerationStructure
	VKTopAccelerationStructure::~VKTopAccelerationStructure()
	{
		auto device = mDeviceRef.GetPtr();
		if (device != nullptr && mAccelerationStructure != VK_NULL_HANDLE && device->fn_vkDestroyAccelerationStructureKHR != nullptr)
		{
			device->fn_vkDestroyAccelerationStructureKHR(device->mDevice, mAccelerationStructure, device->GetVkAllocCallBacks());
			mAccelerationStructure = VK_NULL_HANDLE;
		}
	}
	bool VKTopAccelerationStructure::Init(VKGpuDevice* device, const FTopAccelerationStructureDesc* desc)
	{
		mDeviceRef.FromObject(device);
		if (device->mVulkanExt.IsRayQuery == false)
			return false;

		VkAccelerationStructureGeometryKHR geomDesc{};
		geomDesc.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_KHR;
		geomDesc.geometryType = VK_GEOMETRY_TYPE_INSTANCES_KHR;
		geomDesc.geometry.instances.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_INSTANCES_DATA_KHR;
		geomDesc.geometry.instances.arrayOfPointers = VK_FALSE;

		VkAccelerationStructureBuildGeometryInfoKHR buildInfo{};
		buildInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_GEOMETRY_INFO_KHR;
		buildInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL_KHR;
		buildInfo.flags = VK_BUILD_ACCELERATION_STRUCTURE_PREFER_FAST_TRACE_BIT_KHR;
		buildInfo.mode = VK_BUILD_ACCELERATION_STRUCTURE_MODE_BUILD_KHR;
		buildInfo.geometryCount = 1;
		buildInfo.pGeometries = &geomDesc;

		//align with DX12TopAccelerationStructure::Init: NumDescs = 1
		UINT maxInstanceCount = 1;
		VkAccelerationStructureBuildSizesInfoKHR sizeInfo{};
		sizeInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_SIZES_INFO_KHR;
		device->fn_vkGetAccelerationStructureBuildSizesKHR(device->mDevice, VK_ACCELERATION_STRUCTURE_BUILD_TYPE_DEVICE_KHR, &buildInfo, &maxInstanceCount, &sizeInfo);
		mScratchSize = (UINT)sizeInfo.buildScratchSize;

		FBufferDesc bfDesc{};
		bfDesc.SetDefault(true, (EBufferType)(EBufferType::BFT_UAV | EBufferType::BFT_SRV | EBufferType::BFT_RTAS));
		bfDesc.Size = (UINT)sizeInfo.accelerationStructureSize;
		bfDesc.Size = ((bfDesc.Size + (sizeof(UINT) - 1)) / sizeof(UINT)) * sizeof(UINT);
		bfDesc.RowPitch = bfDesc.Size;
		bfDesc.DepthPitch = bfDesc.Size;
		mGpuBuffer = MakeWeakRef(device->CreateBuffer(&bfDesc, __FILE__, __LINE__));
		if (mGpuBuffer == nullptr)
			return false;

		VkAccelerationStructureCreateInfoKHR asCreateInfo{};
		asCreateInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_CREATE_INFO_KHR;
		asCreateInfo.buffer = (VkBuffer)mGpuBuffer->GetHWBuffer();
		asCreateInfo.offset = 0;
		asCreateInfo.size = sizeInfo.accelerationStructureSize;
		asCreateInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL_KHR;
		if (device->fn_vkCreateAccelerationStructureKHR(device->mDevice, &asCreateInfo, device->GetVkAllocCallBacks(), &mAccelerationStructure) != VK_SUCCESS)
			return false;
		mGpuBuffer.UnsafeConvertTo<VKBuffer>()->mAccelerationStructure = mAccelerationStructure;

		FSrvDesc srvDesc{};
		srvDesc.SetRTAS();
		mGpuBufferSRV = MakeWeakRef(device->CreateSRV(mGpuBuffer, &srvDesc, __FILE__, __LINE__));

		FUavDesc uavDesc{};
		uavDesc.SetBuffer(true);
		uavDesc.Buffer.NumElements = bfDesc.Size / sizeof(UINT);
		mGpuBufferUAV = MakeWeakRef(device->CreateUAV(mGpuBuffer, &uavDesc, __FILE__, __LINE__));

		bfDesc.SetDefault(false, (EBufferType)(EBufferType::BFT_UAV));
		bfDesc.Size = mScratchSize;
		bfDesc.RowPitch = bfDesc.Size;
		bfDesc.DepthPitch = bfDesc.Size;
		mGpuScratchBuffer = MakeWeakRef(device->CreateBuffer(&bfDesc, __FILE__, __LINE__));

		return true;
	}
	bool VKTopAccelerationStructure::IsBuild(VKGpuDevice* device)
	{
		if (mInstDescs.size() != mBottomASInstances.size())
		{
			mInstDescs.clear();
			for (auto& i : mBottomASInstances)
			{
				VkAccelerationStructureInstanceKHR instDesc;
				MakeVKInstanceDesc(instDesc, i);
				mInstDescs.push_back(instDesc);
			}
			mInstanceHash = Hash128::GetHash128((const char*)mInstDescs.data(), (UINT)(sizeof(VkAccelerationStructureInstanceKHR) * mInstDescs.size()));

			FBufferDesc bfDesc{};
			bfDesc.SetDefault(false, BFT_SRV);
			bfDesc.CpuAccess = ECpuAccess::CAS_WRITE;
			bfDesc.Usage = EGpuUsage::USAGE_STAGING;
			bfDesc.Size = (UINT)(sizeof(VkAccelerationStructureInstanceKHR) * mBottomASInstances.size());
			bfDesc.RowPitch = bfDesc.Size;
			bfDesc.DepthPitch = bfDesc.Size;
			FMappedSubResource initData{};
			initData.pData = mInstDescs.data();
			initData.RowPitch = bfDesc.Size;
			bfDesc.InitData = &initData;
			auto buffer = MakeWeakRef(device->CreateBuffer(&bfDesc, __FILE__, __LINE__));
			mInstanceGpuBuffer = MakeWeakRef(new FUploadBuffer(buffer));
			return true;
		}
		else
		{
			auto saved = mInstanceHash;
			mInstDescs.clear();
			for (auto& i : mBottomASInstances)
			{
				VkAccelerationStructureInstanceKHR instDesc;
				MakeVKInstanceDesc(instDesc, i);
				mInstDescs.push_back(instDesc);
			}
			mInstanceHash = Hash128::GetHash128((const char*)mInstDescs.data(), (UINT)(sizeof(VkAccelerationStructureInstanceKHR) * mInstDescs.size()));
			auto result = saved != mInstanceHash;
			if (result)
			{
				UINT size = (UINT)(sizeof(VkAccelerationStructureInstanceKHR) * mBottomASInstances.size());
				memcpy(mInstanceGpuBuffer->GetPtr(), mInstDescs.data(), size);
			}
			return result;
		}
	}
	bool VKTopAccelerationStructure::BuildAcclerationStruture()
	{
		auto device = mDeviceRef.GetPtr();
		if (IsBuild(device) == false)
			return true;

		VkAccelerationStructureGeometryKHR geomDesc{};
		geomDesc.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_KHR;
		geomDesc.geometryType = VK_GEOMETRY_TYPE_INSTANCES_KHR;
		geomDesc.geometry.instances.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_GEOMETRY_INSTANCES_DATA_KHR;
		geomDesc.geometry.instances.arrayOfPointers = VK_FALSE;
		geomDesc.geometry.instances.data.deviceAddress = ((VKBuffer*)mInstanceGpuBuffer->GetBuffer())->GetGPUVirtualAddress();

		VkAccelerationStructureBuildGeometryInfoKHR buildInfo{};
		buildInfo.sType = VK_STRUCTURE_TYPE_ACCELERATION_STRUCTURE_BUILD_GEOMETRY_INFO_KHR;
		buildInfo.type = VK_ACCELERATION_STRUCTURE_TYPE_TOP_LEVEL_KHR;
		buildInfo.flags = VK_BUILD_ACCELERATION_STRUCTURE_PREFER_FAST_TRACE_BIT_KHR;
		buildInfo.mode = VK_BUILD_ACCELERATION_STRUCTURE_MODE_BUILD_KHR;
		buildInfo.geometryCount = 1;
		buildInfo.pGeometries = &geomDesc;
		buildInfo.dstAccelerationStructure = mAccelerationStructure;
		buildInfo.scratchData.deviceAddress = mGpuScratchBuffer.UnsafeConvertTo<VKBuffer>()->GetGPUVirtualAddress();

		{
			FTransientCmd cmd(device, EQueueType::QU_Default, "BuildTLAStructure");
			auto cmdlist = (VKCommandList*)cmd.GetCmdList();
			cmdlist->GetCmdRecorder()->UseResource(this);
			cmdlist->GetCmdRecorder()->UseResource(mInstanceGpuBuffer->GetBuffer());
			VkAccelerationStructureBuildRangeInfoKHR range{};
			range.primitiveCount = (UINT)mBottomASInstances.size();
			const VkAccelerationStructureBuildRangeInfoKHR* pRange = &range;
			device->fn_vkCmdBuildAccelerationStructuresKHR(cmdlist->GetVKCmdRecorder()->mCommandBuffer, 1, &buildInfo, &pRange);
			//DX12 clones dest into mSourceGpuBuffer here, the transient cmd is flushed synchronously on VK so the clone is unnecessary
		}

		return true;
	}
}
NS_END