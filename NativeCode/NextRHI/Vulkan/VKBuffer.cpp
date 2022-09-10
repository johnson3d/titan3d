#include "VKBuffer.h"
#include "VKCommandList.h"
#include "VKGpuDevice.h"
#include "VKEvent.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	template<>
	struct AuxGpuResourceDestroyer<VkBuffer>
	{
		static void Destroy(VkBuffer obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyBuffer(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<AutoRef<VKBuffer>>
	{
		static void Destroy(AutoRef<VKBuffer> obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
		}
	}; 
	template<>
	struct AuxGpuResourceDestroyer<VkImage>
	{
		static void Destroy(VkImage obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyImage(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<VkBufferView>
	{
		static void Destroy(VkBufferView obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyBufferView(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<VkImageView>
	{
		static void Destroy(VkImageView obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyImageView(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	template<>
	struct AuxGpuResourceDestroyer<AutoRef<FGpuMemory>>
	{
		static void Destroy(AutoRef<FGpuMemory> obj, IGpuDevice* device1)
		{
			obj->FreeMemory();
		}
	};
	VKBuffer::VKBuffer()
	{
	}

	VKBuffer::~VKBuffer()
	{
		if (mGpuMemory == nullptr)
			return;
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		
		device->DelayDestroy(mBuffer);
		mBuffer = nullptr;

		device->DelayDestroy(mGpuMemory);
		mGpuMemory = nullptr;
	}

	AutoRef<VKBuffer> CreateUploadBuffer(VKGpuDevice* device, void* pData, UINT64 totalSize, UINT size)
	{
		auto result = MakeWeakRef(new VKBuffer());
		
		VkBuffer buffer;
		VkBufferCreateInfo bufferInfo = {};
		bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
		bufferInfo.size = size;
		bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
		bufferInfo.usage = VK_BUFFER_USAGE_TRANSFER_SRC_BIT;
		if (vkCreateBuffer(device->mDevice, &bufferInfo, device->GetVkAllocCallBacks(), &buffer) != VK_SUCCESS)
		{
			return nullptr;
		}
		VkMemoryRequirements memRequires{};
		vkGetBufferMemoryRequirements(device->mDevice, buffer, &memRequires);
		result->Desc.CpuAccess = ECpuAccess::CAS_WRITE;
		result->Desc.Type = BFT_NONE;
		result->Desc.Size = size;
		result->Desc.RowPitch = (UINT)totalSize;
		result->Desc.DepthPitch = (UINT)totalSize;
		result->Desc.StructureStride = 0;
		result->Desc.Usage = EGpuUsage::USAGE_DYNAMIC;
		result->mDeviceRef.FromObject(device);

		result->mBuffer = buffer;

		auto typeIndex = device->FindMemoryType(memRequires.memoryTypeBits, VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);

		result->mGpuMemory = device->mDefaultBufferAllocator->Alloc(device, typeIndex, memRequires.size);
		vkBindBufferMemory(device->mDevice, result->mBuffer, (VkDeviceMemory)result->mGpuMemory->GetHWBuffer(), result->mGpuMemory->Offset);

		FMappedSubResource subRes{};
		if (result->Map(nullptr, 0, &subRes, false))
		{
			memcpy(subRes.pData, pData, size);
			result->Unmap(nullptr, 0);
		}

		return result;
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

		/*UINT alignedSize = desc.Size;
		auto pAlignment = device->GetGpuResourceAlignment();
		if (alignedSize % pAlignment->VbIbAlignment)
		{
			alignedSize = (alignedSize / pAlignment->VbIbAlignment + 1) * pAlignment->VbIbAlignment;
		}*/
		
		UINT memFlags = 0;
		VkBufferCreateInfo bufferInfo = {};
		bufferInfo.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
		bufferInfo.size = desc.Size;

		IGpuMemAllocator* allocator = nullptr;
		if (desc.Type & EBufferType::BFT_CBuffer)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;
			memFlags = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
			
			allocator = device->mCBufferAllocator;
		}
		if (desc.Type & EBufferType::BFT_UAV)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_STORAGE_TEXEL_BUFFER_BIT;
			memFlags = VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT;

			allocator = device->mSsboAllocator;
		}
		if (desc.Type & EBufferType::BFT_SRV)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT | VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
			allocator = device->mUploadBufferAllocator;
		}
		if (desc.Type & EBufferType::BFT_Vertex)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_VERTEX_BUFFER_BIT | VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
			allocator = device->mVbIbAllocator;
		}
		if (desc.Type & EBufferType::BFT_Index)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDEX_BUFFER_BIT | VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
			allocator = device->mVbIbAllocator;
		}
		if (desc.Type & EBufferType::BFT_IndirectArgs)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_INDIRECT_BUFFER_BIT | VK_BUFFER_USAGE_STORAGE_TEXEL_BUFFER_BIT;
			allocator = device->mSsboAllocator;
		}
		if (desc.Type & EBufferType::BFT_RTV)
		{
			ASSERT(false);
		}
		if (desc.Type & EBufferType::BFT_DSV)
		{
			ASSERT(false);
		}
		
		bufferInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;

		if (desc.Usage == EGpuUsage::USAGE_STAGING)
		{
			bufferInfo.usage |= VK_BUFFER_USAGE_TRANSFER_DST_BIT;
		}

		if (vkCreateBuffer(device->mDevice, &bufferInfo, device->GetVkAllocCallBacks(), &mBuffer) != VK_SUCCESS)
		{
			return false;
		}

		if (desc.Usage == EGpuUsage::USAGE_DYNAMIC)
		{
			memFlags = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
			if (desc.Type != EBufferType::BFT_CBuffer)
				allocator = device->mUploadBufferAllocator;
			//GpuState = EGpuResourceState::GRS_GenericRead;
		}
		else if (desc.Usage == EGpuUsage::USAGE_STAGING)
		{
			memFlags = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
			if (allocator == nullptr)
				allocator = device->mReadBackAllocator;
			//GpuState = EGpuResourceState::GRS_CopyDst;
		}
		/*else
		{
			if (allocator == nullptr)
				allocator = device->mStaticBufferAllocator;
		}*/

		if (allocator == nullptr)
		{
			allocator = device->mDefaultBufferAllocator;
		}

		VkMemoryRequirements memRequirements;
		vkGetBufferMemoryRequirements(device->mDevice, mBuffer, &memRequirements);
		auto memSize = memRequirements.size;//((memRequirements.size + memRequirements.alignment - 1) / memRequirements.alignment) * memRequirements.alignment;
		auto memoryTypeIndex = device->FindMemoryType(memRequirements.memoryTypeBits, memFlags);
		//ASSERT(device->mCBufferAllocator->mMemTypeIndex == memoryTypeIndex);
		mGpuMemory = allocator->Alloc(device, memSize);

		vkBindBufferMemory(device->mDevice, mBuffer, (VkDeviceMemory)mGpuMemory->GetHWBuffer(), mGpuMemory->Offset);

		//GpuState = VKImageLayoutToGpuState(imageInfo.initialLayout);
		if (desc.InitData != nullptr)
		{
			if (desc.Usage == EGpuUsage::USAGE_DYNAMIC)
			{
				FMappedSubResource subRes{};
				if (this->Map(nullptr, 0, &subRes, false))
				{
					memcpy(subRes.pData, desc.InitData, desc.Size);
					this->Unmap(nullptr, 0);
				}
			}
			else
			{
				auto bf = CreateUploadBuffer(device, desc.InitData, memSize, desc.Size);
				auto cmd = device->GetCmdQueue()->GetIdleCmdlist(EQueueCmdlist::QCL_Transient);
				cmd->BeginCommand();
				auto saved = this->GpuState;
				this->TransitionTo(cmd, EGpuResourceState::GRS_CopyDst);
				cmd->CopyBufferRegion(this, 0, bf, 0, Desc.Size);
				this->TransitionTo(cmd, saved);
				cmd->EndCommand();
				device->GetCmdQueue()->ExecuteCommandList(cmd);
				device->GetCmdQueue()->ReleaseIdleCmdlist(cmd, EQueueCmdlist::QCL_Transient);
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
	void GpuResourceStateToVKAccessAndPipeline(EGpuResourceState state, VkAccessFlags& outAccessFlags, VkPipelineStageFlagBits& outPipelineStages)
	{//https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkAccessFlagBits.html
		outPipelineStages = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
		switch (state)
		{
		case EngineNS::NxRHI::GRS_Undefine:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_NONE;
			outPipelineStages = VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT;
			return;
		case EngineNS::NxRHI::GRS_Present:
		case EngineNS::NxRHI::GRS_SrvPS:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT;
			outPipelineStages = VkPipelineStageFlagBits::VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT;
			return;
		case EngineNS::NxRHI::GRS_GenericRead:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT | VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT);
			return;
		case EngineNS::NxRHI::GRS_Uav:
			outAccessFlags = (VkAccessFlagBits)(VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT | VkAccessFlagBits::VK_ACCESS_SHADER_WRITE_BIT);
			outPipelineStages = VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT;
			return;
		case EngineNS::NxRHI::GRS_UavIndirect:
			outAccessFlags = (VkAccessFlagBits)(VkAccessFlagBits::VK_ACCESS_INDIRECT_COMMAND_READ_BIT);
			outPipelineStages = VK_PIPELINE_STAGE_DRAW_INDIRECT_BIT;
			return;
		case EngineNS::NxRHI::GRS_RenderTarget:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT;
			outPipelineStages = VK_PIPELINE_STAGE_COLOR_ATTACHMENT_OUTPUT_BIT;
			return;
		case EngineNS::NxRHI::GRS_DepthStencil:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT | VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT);
			return;
		case EngineNS::NxRHI::GRS_DepthRead:
		case EngineNS::NxRHI::GRS_StencilRead:
		case EngineNS::NxRHI::GRS_DepthStencilRead:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_EARLY_FRAGMENT_TESTS_BIT | VK_PIPELINE_STAGE_LATE_FRAGMENT_TESTS_BIT);
			return;
		case EngineNS::NxRHI::GRS_CopySrc:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_TRANSFER_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_TRANSFER_BIT | VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR);
			return;
		case EngineNS::NxRHI::GRS_CopyDst:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_TRANSFER_WRITE_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_TRANSFER_BIT | VK_PIPELINE_STAGE_ACCELERATION_STRUCTURE_BUILD_BIT_KHR);
			return;
		default:
			outAccessFlags = VkAccessFlagBits::VK_ACCESS_SHADER_READ_BIT;
			outPipelineStages = (VkPipelineStageFlagBits)(VK_PIPELINE_STAGE_COMPUTE_SHADER_BIT | VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT);
			return;
		}
	}
	void VKBuffer::TransitionTo(ICommandList* cmd, EGpuResourceState state)
	{
		//ASSERT(state != EGpuResourceState::GRS_Undefine);
		if (state == GpuState)
			return;
		
		VkBufferMemoryBarrier barrier{};
		barrier.sType = VK_STRUCTURE_TYPE_BUFFER_MEMORY_BARRIER;
		barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
		barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
		barrier.buffer = mBuffer;
		barrier.offset = 0;
		barrier.size = Desc.Size;

		VkPipelineStageFlagBits srcStages,dstStages;
		GpuResourceStateToVKAccessAndPipeline(GpuState, barrier.srcAccessMask, srcStages);
		GpuResourceStateToVKAccessAndPipeline(state, barrier.dstAccessMask, dstStages);

		auto vkCmd = (VKCommandList*)cmd;

		vkCmdPipelineBarrier(
			vkCmd->mCommandBuffer->RealObject,
			srcStages,
			dstStages,//VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,//VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
			0,
			0, nullptr,
			1, &barrier,
			0, nullptr
		);
		GpuState = state;
	}

	void VKBuffer::Flush2Device(ICommandList* cmd, void* pBuffer, UINT Size)
	{
		if (Size > Desc.Size || pBuffer == NULL)
			return;
		ASSERT(Size == Desc.Size);

		FMappedSubResource mapped{};
		if (this->Map(cmd, 0, &mapped, false))
		{
			memcpy(mapped.pData, pBuffer, Size);
			this->Unmap(cmd, 0);
		}
	}

	void VKBuffer::UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch)
	{
		if (Desc.Usage == EGpuUsage::USAGE_DEFAULT)
		{
			auto copyDesc = this->Desc;
		}
		else
		{
			FMappedSubResource mapped{};
			if (this->Map(cmd, subRes, &mapped, false))
			{
				memcpy(mapped.pData, pData, rowPitch);
				this->Unmap(cmd, subRes);
			}
		}
	}

	void VKBuffer::UpdateGpuData(ICommandList* cmd, UINT offset, void* pData, UINT size)
	{
		if (Desc.Usage == EGpuUsage::USAGE_DEFAULT)
		{
			auto copyDesc = this->Desc;
			copyDesc.Usage = EGpuUsage::USAGE_STAGING;
			copyDesc.Size = size;
			copyDesc.InitData = pData;
			copyDesc.CpuAccess = ECpuAccess::CAS_WRITE;

			auto device = mDeviceRef.GetPtr();
			auto bf = MakeWeakRef(device->CreateBuffer(&copyDesc));

			cmd->CopyBufferRegion(this, offset, bf, 0, size);
			device->DelayDestroy(bf);
			/*auto fence = ((VKCommandList*)cmd)->mCommitFence;
			auto tarValue = fence->GetCompletedValue() + 1;

			device->PushPostEvent([fence, bf, tarValue](IGpuDevice* pDevice)->bool
				{
					if (fence->GetCompletedValue() >= tarValue)
					{
						pDevice->DelayDestroy(bf);
						return true;
					}
					return false;
				});*/
		}
		else
		{
			FMappedSubResource mapped{};
			if (this->Map(cmd, 0, &mapped, false))
			{
				memcpy(mapped.pData, pData, size);
				this->Unmap(cmd, 0);
			}
		}
	}

	bool VKBuffer::Map(ICommandList* cmd, UINT index, FMappedSubResource* res, bool forRead)
	{
		auto device = mDeviceRef.GetPtr();
		void* data;
		auto hr = vkMapMemory(device->mDevice, (VkDeviceMemory)mGpuMemory->GetHWBuffer(), mGpuMemory->Offset, Desc.Size, (VkMemoryMapFlags)0, &data);
		if (VK_SUCCESS != hr)
			return false;
		
		res->pData = data;
		res->RowPitch = Desc.RowPitch;
		res->DepthPitch = Desc.DepthPitch;
		return true;
	}

	void VKBuffer::Unmap(ICommandList* cmd, UINT index)
	{
		auto device = mDeviceRef.GetPtr();
		vkUnmapMemory(device->mDevice, (VkDeviceMemory)mGpuMemory->GetHWBuffer());
	}

	void VKBuffer::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT, mBuffer, name);
	}
	
	VkImageAspectFlags FormatToVKImageAspectFlags(EPixelFormat format, bool sampledDepth, bool sampledStencil)
	{
		VkImageAspectFlags result = (VkImageAspectFlags)0;
		switch (format)
		{
			case EPixelFormat::PXF_D16_UNORM:
			case EPixelFormat::PXF_D32_FLOAT:
			case EPixelFormat::PXF_D32_FLOAT_S8X24_UINT:
			{
				result |= VK_IMAGE_ASPECT_DEPTH_BIT;
			}
			break;
			case EPixelFormat::PXF_R24G8_TYPELESS:
			case EPixelFormat::PXF_D24_UNORM_S8_UINT:
			{
				if (sampledDepth)
					result |= VK_IMAGE_ASPECT_DEPTH_BIT;
				/*if (sampledStencil)
					result |= VK_IMAGE_ASPECT_STENCIL_BIT;*/
			}
			break;
			default:
			{
				result = VK_IMAGE_ASPECT_COLOR_BIT;
			}
			break;
		}
		return result;
	}

	VKTexture::VKTexture()
	{
	}

	VKTexture::~VKTexture()
	{
		if (mGpuMemory == nullptr)
			return;
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		
		device->DelayDestroy(mImage);
		mImage = nullptr;

		device->DelayDestroy(mGpuMemory);
		mGpuMemory = nullptr;
	}
	AutoRef<VKTexture> CreateUploadTexure(VKGpuDevice* device, const FTextureDesc& srcDesc)
	{
		FTextureDesc desc = srcDesc;
		desc.BindFlags = EBufferType::BFT_NONE;
		desc.Usage = EGpuUsage::USAGE_STAGING;
		desc.CpuAccess = ECpuAccess::CAS_WRITE;
		desc.InitData = nullptr;

		auto result = MakeWeakRef(new VKTexture());
		result->Init(device, desc);

		for (UINT i = 0; i < desc.MipLevels; i++)
		{
			auto& data = srcDesc.InitData[i];
			if (result->Map(nullptr, i, &data, false))
			{
				result->Unmap(nullptr, i);
			}
		}
		
		return result;
	}
	std::vector<AutoRef<VKBuffer>> CreateUploadTexureBuffers(VKGpuDevice* device, VKTexture* texture, const FTextureDesc& desc)
	{
		std::vector<AutoRef<VKBuffer>> result;
		//FBufferDesc bfDesc{};
		//bfDesc.SetDefault();
		//bfDesc.CpuAccess = ECpuAccess::CAS_WRITE;
		//bfDesc.Usage = EGpuUsage::USAGE_STAGING;

		//VkImageSubresource imgSubRes{};
		//imgSubRes.aspectMask = FormatToVKImageAspectFlags(desc.Format);

		for (UINT i = 0; i < desc.ArraySize; i++)
		{
			//UINT height = desc.Height;
			for (UINT j = 0; j < desc.MipLevels; j++)
			{	
				//imgSubRes.arrayLayer = i;
				//imgSubRes.mipLevel = j;
				//VkSubresourceLayout layout{};
				//vkGetImageSubresourceLayout(device->mDevice, texture->mImage, &imgSubRes, &layout);
				//bfDesc.InitData = desc.InitData[i * desc.MipLevels + j].pData;
				auto pData = desc.InitData[i * desc.MipLevels + j].pData;
				//UINT size = desc.InitData[i * desc.MipLevels + j].RowPitch * height;
				//ASSERT(size <= layout.size);
				//bfDesc.Size = size;
				//bfDesc.RowPitch = (UINT)layout.rowPitch;
				//bfDesc.DepthPitch = (UINT)layout.depthPitch;
				//auto tmp = MakeWeakRef((VKBuffer*)device->CreateBuffer(&bfDesc));

				UINT size = desc.InitData[i * desc.MipLevels + j].DepthPitch;
				//ASSERT(desc.InitData[i * desc.MipLevels + j].RowPitch * height == size);
				auto tmp = CreateUploadBuffer(device, pData, size, size);
				result.push_back(tmp);

				//height = height / 2;
				//if (height == 0)
					//height = 1;
			}
		}
		return result;
	}
	EGpuResourceState VKImageLayoutToGpuState(VkImageLayout layout)
	{
		switch (layout)
		{
		case VK_IMAGE_LAYOUT_UNDEFINED:
			return EGpuResourceState::GRS_Undefine;
		case VK_IMAGE_LAYOUT_GENERAL:
			return EGpuResourceState::GRS_GenericRead;
		case VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL:
			return EGpuResourceState::GRS_RenderTarget;
		case VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL:
			return EGpuResourceState::GRS_DepthStencil;
		case VK_IMAGE_LAYOUT_DEPTH_STENCIL_READ_ONLY_OPTIMAL:
			return EGpuResourceState::GRS_DepthStencilRead;
		case VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL:
			return EGpuResourceState::GRS_GenericRead;
		case VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL:
			return EGpuResourceState::GRS_CopySrc;
		case VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL:
			return EGpuResourceState::GRS_CopyDst;
		case VK_IMAGE_LAYOUT_PREINITIALIZED:
			return EGpuResourceState::GRS_Undefine;
		case VK_IMAGE_LAYOUT_DEPTH_READ_ONLY_STENCIL_ATTACHMENT_OPTIMAL:
			return EGpuResourceState::GRS_Undefine;
		case VK_IMAGE_LAYOUT_DEPTH_ATTACHMENT_STENCIL_READ_ONLY_OPTIMAL:
			return EGpuResourceState::GRS_DepthStencil;
		case VK_IMAGE_LAYOUT_DEPTH_ATTACHMENT_OPTIMAL:
			return EGpuResourceState::GRS_DepthStencil;
		case VK_IMAGE_LAYOUT_DEPTH_READ_ONLY_OPTIMAL:
			return EGpuResourceState::GRS_DepthRead;
		case VK_IMAGE_LAYOUT_STENCIL_ATTACHMENT_OPTIMAL:
			return EGpuResourceState::GRS_DepthStencil;
		case VK_IMAGE_LAYOUT_STENCIL_READ_ONLY_OPTIMAL:
			return EGpuResourceState::GRS_StencilRead;
		case VK_IMAGE_LAYOUT_READ_ONLY_OPTIMAL:
			return EGpuResourceState::GRS_GenericRead;
		case VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL:
			return EGpuResourceState::GRS_Uav;
		case VK_IMAGE_LAYOUT_PRESENT_SRC_KHR:
			return EGpuResourceState::GRS_Present;
		case VK_IMAGE_LAYOUT_SHARED_PRESENT_KHR:
			return EGpuResourceState::GRS_Present;
		case VK_IMAGE_LAYOUT_FRAGMENT_DENSITY_MAP_OPTIMAL_EXT:
			break;
		case VK_IMAGE_LAYOUT_FRAGMENT_SHADING_RATE_ATTACHMENT_OPTIMAL_KHR:
			break;
		case VK_IMAGE_LAYOUT_MAX_ENUM:
			break;
		default:
			break;
		}
		return EGpuResourceState::GRS_Undefine;
	}
	VkImageLayout GpuStateToVKImageLayout(EGpuResourceState state)
	{
		switch (state)
		{
		case EngineNS::NxRHI::GRS_Undefine:
			return VK_IMAGE_LAYOUT_UNDEFINED;
		case EngineNS::NxRHI::GRS_SrvPS:
			return VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
		case EngineNS::NxRHI::GRS_GenericRead:
			return VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
		case EngineNS::NxRHI::GRS_Uav:
			return VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL;
		case EngineNS::NxRHI::GRS_RenderTarget:
			return VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL;
		case EngineNS::NxRHI::GRS_DepthStencil:
			return VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL;
		case EngineNS::NxRHI::GRS_DepthRead:
			return VK_IMAGE_LAYOUT_DEPTH_READ_ONLY_OPTIMAL;
		case EngineNS::NxRHI::GRS_StencilRead:
			return VK_IMAGE_LAYOUT_STENCIL_READ_ONLY_OPTIMAL;
		case EngineNS::NxRHI::GRS_DepthStencilRead:
			return VK_IMAGE_LAYOUT_DEPTH_STENCIL_READ_ONLY_OPTIMAL;
		case EngineNS::NxRHI::GRS_CopySrc:
			return VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
		case EngineNS::NxRHI::GRS_CopyDst:
			return VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
		case EngineNS::NxRHI::GRS_Present:
			return VK_IMAGE_LAYOUT_PRESENT_SRC_KHR;
		default:
			break;
		}
		return VkImageLayout::VK_IMAGE_LAYOUT_GENERAL;
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
		imageInfo.usage |= VK_IMAGE_USAGE_TRANSFER_DST_BIT;

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
		
		imageInfo.samples = VK_SAMPLE_COUNT_1_BIT;//desc.SamplerDesc.Count
		imageInfo.sharingMode = VK_SHARING_MODE_EXCLUSIVE;
		imageInfo.flags |= VK_IMAGE_CREATE_MUTABLE_FORMAT_BIT;

 		VkMemoryPropertyFlags memFlags = 0;
		if (Desc.Usage == EGpuUsage::USAGE_DEFAULT)
		{
			memFlags = VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT;
		}
		else if (Desc.Usage == EGpuUsage::USAGE_DYNAMIC)
		{
			memFlags = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
		}
		else if (Desc.Usage == EGpuUsage::USAGE_STAGING)
		{
			memFlags = VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK_MEMORY_PROPERTY_HOST_COHERENT_BIT;
			if (Desc.CpuAccess == CAS_READ)
			{
				imageInfo.initialLayout = VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL;
				imageInfo.usage = VK_IMAGE_USAGE_TRANSFER_DST_BIT;
			}
			else if (Desc.CpuAccess == CAS_WRITE)
			{
				imageInfo.initialLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
				imageInfo.usage = VK_IMAGE_USAGE_TRANSFER_SRC_BIT;
			}
			else
			{
				imageInfo.initialLayout = VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL;
			}
		}
		
		if (VK_SUCCESS != vkCreateImage(device->mDevice, &imageInfo, device->GetVkAllocCallBacks(), &mImage))
			return false;

		VkMemoryRequirements memRequirements;
		vkGetImageMemoryRequirements(device->mDevice, mImage, &memRequirements);

		auto memSize = ((memRequirements.size + memRequirements.alignment - 1) / memRequirements.alignment) * memRequirements.alignment;
		auto typeIndex = device->FindMemoryType(memRequirements.memoryTypeBits, memFlags);
		mGpuMemory = device->mDefaultBufferAllocator->Alloc(device, typeIndex, memSize);
		if (mGpuMemory == nullptr)
		{
			return false;
		}

		vkBindImageMemory(device->mDevice, mImage, (VkDeviceMemory)mGpuMemory->GetHWBuffer(), mGpuMemory->Offset);

		GpuState = VKImageLayoutToGpuState(imageInfo.initialLayout);
		
		if (desc.InitData != nullptr)
		{
			if (desc.Usage == EGpuUsage::USAGE_DYNAMIC)
			{
				for (UINT i = 0; i < desc.MipLevels; i++)
				{
					FMappedSubResource mapped{};
					if (this->Map(nullptr, i, &mapped, false))
					{
						memcpy(mapped.pData, &desc.InitData[i], desc.InitData[i].DepthPitch);
						this->Unmap(nullptr, i);
					}
				}
			}
			else
			{
				auto bf = CreateUploadTexureBuffers(device, this , desc);
				if (bf.size() > 0)
				{
					auto width = desc.Width;
					auto height = desc.Height;
					if (height == 0)
						height = 1;
					auto depth = desc.Depth;
					if (depth == 0)
						depth = 1;
					UINT layer = 0;
					for (size_t i = 0; i < bf.size(); i++)
					{
						VkBufferImageCopy region{};
						region.imageSubresource.baseArrayLayer = (UINT)i / desc.MipLevels;
						if (region.imageSubresource.baseArrayLayer != layer)
						{
							width = desc.Width;
							auto height = desc.Height;
							if (height == 0)
								height = 1;
							auto depth = desc.Depth;
							if (depth == 0)
								depth = 1;
							layer = region.imageSubresource.baseArrayLayer;
						}
						region.imageSubresource.mipLevel = (UINT)i % desc.MipLevels;
						region.bufferOffset = 0;
						region.bufferRowLength = 0;// desc.InitData[i].RowPitch;
						region.bufferImageHeight = 0;// height;
						if (Desc.BindFlags & EBufferType::BFT_SRV)
							region.imageSubresource.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, true);
						else
							region.imageSubresource.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, true);
						region.imageSubresource.layerCount = 1;// Desc.ArraySize;
						region.imageOffset = { 0, 0, 0 };
						region.imageExtent =
						{
							width,
							height,
							depth
						};

						height = height / 2;
						if (height == 0)
							height = 1;
						width = width / 2;
						if (width == 0)
							width = 1;
						depth = depth / 2;
						if (depth == 0)
							depth = 1;

						auto cmd = (VKCommandList*)device->GetCmdQueue()->GetIdleCmdlist(EQueueCmdlist::QCL_Transient);
						cmd->BeginCommand();
						
						this->TransitionTo(cmd, EGpuResourceState::GRS_CopyDst);
						VkBuffer srcBuffer = bf[i]->mBuffer;
						vkCmdCopyBufferToImage(cmd->mCommandBuffer->RealObject, srcBuffer, mImage, VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 1, &region);
						this->TransitionTo(cmd, EGpuResourceState::GRS_GenericRead);

						cmd->EndCommand();
						device->GetCmdQueue()->ExecuteCommandList(cmd);
						device->GetCmdQueue()->ReleaseIdleCmdlist(cmd, EQueueCmdlist::QCL_Transient);
					}
				}
			}
		}
		else
		{
			if (GpuState == EGpuResourceState::GRS_Undefine)
			{
				auto cmd = (VKCommandList*)device->GetCmdQueue()->GetIdleCmdlist(EQueueCmdlist::QCL_Transient);
				cmd->BeginCommand();
				if (Desc.BindFlags & EBufferType::BFT_SRV)
				{
					/*if (Desc.BindFlags & EBufferType::BFT_DSV)
					{
						this->TransitionTo(cmd, EGpuResourceState::GRS_DepthRead);
					}
					else
					{
						this->TransitionTo(cmd, EGpuResourceState::GRS_GenericRead);
					}*/
					this->TransitionTo(cmd, EGpuResourceState::GRS_GenericRead);
				}
				else if (Desc.BindFlags & EBufferType::BFT_RTV)
				{
					this->TransitionTo(cmd, EGpuResourceState::GRS_RenderTarget);
				}
				else if (Desc.BindFlags & EBufferType::BFT_DSV)
				{
					this->TransitionTo(cmd, EGpuResourceState::GRS_DepthStencil);
				}
				cmd->EndCommand();
				device->GetCmdQueue()->ExecuteCommandList(cmd);
				device->GetCmdQueue()->ReleaseIdleCmdlist(cmd, EQueueCmdlist::QCL_Transient);
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

		auto result = device->CreateBuffer(&desc);

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
	bool VKTexture::Map(ICommandList* cmd, UINT subRes, FMappedSubResource* res, bool forRead)
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
		auto hr = vkMapMemory(device->mDevice, (VkDeviceMemory)mGpuMemory->GetHWBuffer(), mGpuMemory->Offset, subLayout.size, (VkMemoryMapFlags)0, &data);
		if (hr != VK_SUCCESS)
		{
			return false;
		}
		res->pData = (BYTE*)data + subLayout.offset;
		res->RowPitch = (UINT)subLayout.rowPitch;
		res->DepthPitch = (UINT)subLayout.depthPitch;
		return true;
	}
	void VKTexture::Unmap(ICommandList* cmd, UINT subRes)
	{
		//UINT subRes = mipIndex + Desc.MipLevels * arrayIndex;
		//mGpuResource->Unmap(subRes, nullptr);
		auto device = mDeviceRef.GetPtr();

		vkUnmapMemory(device->mDevice, (VkDeviceMemory)mGpuMemory->GetHWBuffer());
	}

	void VKTexture::UpdateGpuData(ICommandList* cmd, UINT subRes, void* pData, const FSubresourceBox* box, UINT rowPitch, UINT depthPitch)
	{
		////UINT subRes = mipIndex + Desc.MipLevels * arrayIndex;
		//auto refCmdList = (VKCommandList*)cmd;
		////refCmdList->mContext->UpdateSubresource(mTexture1D, subRes, (D3D11_BOX*)box, pData, rowPitch, depthPitch);
		//D3D12_RANGE range{};
		//void* pTarData = nullptr;
		//if (mGpuResource->Map(0, &range, &pTarData) == S_OK)
		//{
		//	memcpy(pTarData, pData, rowPitch);
		//	mGpuResource->Unmap(0, nullptr);
		//}
	}
	void VKTexture::TransitionTo(ICommandList* cmd, EGpuResourceState state)
	{
		ASSERT(state != EGpuResourceState::GRS_Undefine);
		if (state == GpuState)
			return;

		VkImageMemoryBarrier barrier{};
		barrier.sType = VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER;
		barrier.oldLayout = GpuStateToVKImageLayout(GpuState);
		barrier.newLayout = GpuStateToVKImageLayout(state);
		//barrier.srcAccessMask
		
		if (barrier.oldLayout != barrier.newLayout)
		{
			barrier.oldLayout = VK_IMAGE_LAYOUT_UNDEFINED;
			barrier.srcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
			barrier.dstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED;
			barrier.image = mImage;

			if (Desc.BindFlags & EBufferType::BFT_SRV)
				barrier.subresourceRange.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, false);
			else
				barrier.subresourceRange.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, true);
			barrier.subresourceRange.baseArrayLayer = 0;
			barrier.subresourceRange.layerCount = Desc.ArraySize;
			barrier.subresourceRange.baseMipLevel = 0;//All
			barrier.subresourceRange.levelCount = Desc.MipLevels;

			auto vkCmd = (VKCommandList*)cmd;

			VkPipelineStageFlagBits srcStages, dstStages;
			GpuResourceStateToVKAccessAndPipeline(GpuState, barrier.srcAccessMask, srcStages);
			GpuResourceStateToVKAccessAndPipeline(state, barrier.dstAccessMask, dstStages);

			vkCmdPipelineBarrier(
				vkCmd->mCommandBuffer->RealObject,
				srcStages, //VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,
				dstStages, //VK_PIPELINE_STAGE_TOP_OF_PIPE_BIT,//VK_PIPELINE_STAGE_VERTEX_SHADER_BIT | VK_PIPELINE_STAGE_FRAGMENT_SHADER_BIT,
				0,
				0, nullptr,
				0, nullptr,
				1, &barrier
			);
		}

		GpuState = state;
	}

	void VKTexture::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT, mImage, name);
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

		UINT alignedSize = ShaderBinder->Size;
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

			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc));
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
			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc));
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
			Buffer = MakeWeakRef(device->CreateBuffer(&bfDesc));
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
		mImageView = nullptr;
	}

	VKSrView::~VKSrView()
	{
		FreeView();
	}

	void VKSrView::FreeView()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		if (Desc.Type == ESrvType::ST_BufferSRV)
		{
			if (Desc.Format == EPixelFormat::PXF_UNKNOWN)
			{

			}
		}
		else
		{
			device->DelayDestroy(mImageView);
			mImageView = nullptr;
		}
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
				break;
			case EngineNS::NxRHI::ST_Texture2DMSArray:
				break;
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
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT, mImageView, name);
	}

	bool VKSrView::Init(VKGpuDevice* device, IGpuBufferData* pBuffer, const FSrvDesc& desc)
	{
		Desc = desc;
		Buffer = pBuffer;
		mDeviceRef.FromObject(device);

		if (Desc.Type == ESrvType::ST_BufferSRV)
		{
			//UINT alignedSize = ((VKBuffer*)pBuffer)->Desc.Size;
			//auto bf = Buffer.UnsafeConvertTo<VKBuffer>();
			//VkBufferViewCreateInfo createInfo = {};
			//createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
			//createInfo.buffer = (VkBuffer)Buffer->GetHWBuffer();

			//createInfo.offset = desc.Buffer.FirstElement;// bf->mGpuMemory->Offset;
			//createInfo.range = alignedSize;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
			//if (desc.Format == EPixelFormat::PXF_UNKNOWN)
			//{
			//	createInfo.format = VkFormat::VK_FORMAT_UNDEFINED; //Format2VKFormat(Desc.Format);
			//	//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;//flags must be 0
			//}
			//else
			//{
			//	createInfo.format = Format2VKFormat(Desc.Format);
			//	//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
			//	if (VK_SUCCESS != vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mBufferView))
			//		return false;
			//}
		}
		else
		{
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
			createInfo.subresourceRange.baseMipLevel = 0;
			createInfo.subresourceRange.levelCount = 1;
			createInfo.subresourceRange.baseArrayLayer = 0;
			createInfo.subresourceRange.layerCount = 1;
			if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mImageView) != VK_SUCCESS)
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

		Buffer = pBuffer;
		mFingerPrint++;

		FreeView();

		auto device = (VKGpuDevice*)device1;
		
		if (Desc.Type == ESrvType::ST_BufferSRV)
		{
			UINT alignedSize = ((VKBuffer*)pBuffer)->Desc.Size;
			auto bf = Buffer.UnsafeConvertTo<VKBuffer>();
			VkBufferViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
			createInfo.buffer = (VkBuffer)Buffer->GetHWBuffer();

			createInfo.offset = Desc.Buffer.FirstElement;// bf->mGpuMemory->Offset;
			createInfo.range = alignedSize;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
			if (Desc.Format == EPixelFormat::PXF_UNKNOWN)
			{
				createInfo.format = VkFormat::VK_FORMAT_UNDEFINED; //Format2VKFormat(Desc.Format);
				//createInfo.flags = VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;//flags must be 0
			}
			else
			{
				//createInfo.format = Format2VKFormat(Desc.Format);
				////createInfo.flags = VK_BUFFER_USAGE_UNIFORM_TEXEL_BUFFER_BIT;
				//if (VK_SUCCESS != vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mBufferView))
				//	return false;
			}
		}
		else
		{
			auto pTexture = (VKTexture*)pBuffer;
			auto pImage = pTexture->mImage;
			VkImageViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
			createInfo.image = pImage;
			createInfo.viewType = SrvTypeToVK(Desc.Type);
			createInfo.format = Format2VKFormat(Desc.Format);
			createInfo.components.r = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.g = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.b = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.components.a = VK_COMPONENT_SWIZZLE_IDENTITY;
			createInfo.subresourceRange.aspectMask = FormatToVKImageAspectFlags(Desc.Format, true, false);
			createInfo.subresourceRange.baseMipLevel = 0;
			createInfo.subresourceRange.levelCount = 1;
			createInfo.subresourceRange.baseArrayLayer = 0;
			createInfo.subresourceRange.layerCount = 1;
			if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mImageView) != VK_SUCCESS)
			{
				return false;
			}
		}
		return true;
	}

	VKUaView::VKUaView()
	{
		
	}

	VKUaView::~VKUaView()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		if (Desc.ViewDimension == EDimensionUAV::UAV_DIMENSION_BUFFER)
		{
			/*device->DelayDestroy(mBufferView);
			mBufferView = nullptr;*/
		}
		else
		{
			device->DelayDestroy(mImageView);
			mImageView = nullptr;
		}
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
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT, mImageView, name);
	}
	bool VKUaView::Init(VKGpuDevice* device, IGpuBufferData* pBuffer, const FUavDesc& desc)
	{
		Desc = desc;
		Buffer = pBuffer;
		mDeviceRef.FromObject(device);

		if (Desc.ViewDimension == EDimensionUAV::UAV_DIMENSION_BUFFER)
		{
			UINT alignedSize = ((VKBuffer*)pBuffer)->Desc.Size;
			auto bf = Buffer.UnsafeConvertTo<VKBuffer>();
			VkBufferViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_BUFFER_VIEW_CREATE_INFO;
			createInfo.buffer = (VkBuffer)Buffer->GetHWBuffer();
			
			createInfo.offset = desc.Buffer.FirstElement;// bf->mGpuMemory->Offset;
			createInfo.range = alignedSize;//VK_WHOLE_SIZE;//desc->Buffer.ElementWidth;
			if (desc.Format == EPixelFormat::PXF_UNKNOWN)
			{
				createInfo.format = VkFormat::VK_FORMAT_UNDEFINED; //Format2VKFormat(Desc.Format);
				createInfo.flags = VK_BUFFER_USAGE_STORAGE_BUFFER_BIT;//reserved
			}
			else
			{
				//createInfo.format = Format2VKFormat(Desc.Format);
				//createInfo.flags = VK_BUFFER_USAGE_STORAGE_TEXEL_BUFFER_BIT;//reserved
				//if (VK_SUCCESS != vkCreateBufferView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mBufferView))
				//	return false;
			}
		}
		else
		{
			auto pTexture = (VKTexture*)pBuffer;
			auto pImage = pTexture->mImage;
			VkImageViewCreateInfo createInfo = {};
			createInfo.sType = VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO;
			createInfo.image = pImage;
			createInfo.flags = 0;//VK_IMAGE_VIEW_CREATE_FRAGMENT_DENSITY_MAP_DYNAMIC_BIT_EXT
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
			if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mImageView) != VK_SUCCESS)
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
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		device->DelayDestroy(mView);
		mView = nullptr;
	}

	void VKRenderTargetView::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT, mView, name);
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
		if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView) != VK_SUCCESS)
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
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		device->DelayDestroy(mView);
		mView = nullptr;
	}

	void VKDepthStencilView::SetDebugName(const char* name)
	{
		auto device = mDeviceRef.GetPtr();
		VKGpuSystem::SetVkObjectDebugName(device->mDevice, VK_DEBUG_REPORT_OBJECT_TYPE_PIPELINE_EXT, mView, name);
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
		if (vkCreateImageView(device->mDevice, &createInfo, device->GetVkAllocCallBacks(), &mView) != VK_SUCCESS)
		{
			return false;
		}

		return true;
	}
}
NS_END