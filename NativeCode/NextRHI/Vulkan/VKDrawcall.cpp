#include "VKDrawcall.h"
#include "VKEffect.h"
#include "VKGpuDevice.h"
#include "VKBuffer.h"
#include "VKGpuState.h"
#include "VKCommandList.h"
#include "VKFrameBuffers.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	template<>
	struct AuxGpuResourceDestroyer<AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>>
	{
		static void Destroy(AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>> obj, IGpuDevice* device1)
		{
			//auto device = (VKGpuDevice*)device1;
			//vkDestroyBuffer(device->mDevice, obj, device->GetVkAllocCallBacks());
			obj->Free();
		}
	};
	VKGraphicDraw::~VKGraphicDraw()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		if (mDescriptorSetVS != nullptr)
		{
			device->DelayDestroy(mDescriptorSetVS);
			mDescriptorSetVS = nullptr;
		}
		if (mDescriptorSetPS != nullptr)
		{
			device->DelayDestroy(mDescriptorSetPS);
			mDescriptorSetPS = nullptr;
		}
	}
	void VKGraphicDraw::OnGpuDrawStateUpdated()
	{
		//auto effect = ShaderEffect.UnsafeConvertTo<VKShaderEffect>();
		/*mDescriptorSetVS = ShaderEffect->mVertexShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.Alloc();
		mDescriptorSetPS = ShaderEffect->mPixelShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.Alloc();

		for (auto& i : BindResources)
		{
			OnBindResource(i.first, i.second);
		}*/
		IsDirty = true;
	}
	void VKGraphicDraw::OnBindResource(const FEffectBinder* binder, IGpuResource* resource)
	{
		IsDirty = true;
	}
	void VKGraphicDraw::BindResourceToDescriptSets(VKGpuDevice* device, AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>& dsSetVS,
						AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>& dsSetPS, 
						const FEffectBinder* binder, IGpuResource* resource)
	{
		VkDescriptorImageInfo tmpVS{};
		VkDescriptorBufferInfo tmpStructureBufferVS{};
		VkDescriptorImageInfo tmpPS{};
		VkDescriptorBufferInfo tmpStructureBufferPS{};
		std::vector<VkWriteDescriptorSet>	dsWriteSets;
		auto pBinder = binder->VSBinder;
		if (pBinder != nullptr)
		{
			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = dsSetVS->RealObject;

			descriptorWrite.dstBinding = pBinder->Slot;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			switch (binder->BindType)
			{
				case EShaderBindType::SBT_CBuffer:
				{
					auto pBuffer = ((VKCbView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
					tmpStructureBufferVS.buffer = pBuffer->mBuffer;
					tmpStructureBufferVS.offset = 0;
					tmpStructureBufferVS.range = pBuffer->Desc.Size;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
					descriptorWrite.pBufferInfo = &tmpStructureBufferVS;
					break;
				}
				case EShaderBindType::SBT_SRV:
				{
					if (pBinder->IsStructuredBuffer)
					{
						auto pBuffer = ((VKSrView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
						tmpStructureBufferVS.buffer = pBuffer->mBuffer;
						tmpStructureBufferVS.offset = 0;
						tmpStructureBufferVS.range = pBuffer->Desc.Size;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
						descriptorWrite.pBufferInfo = &tmpStructureBufferVS;
					}
					else
					{
						tmpVS.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
						tmpVS.imageView = ((VKSrView*)resource)->mImageView;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
						descriptorWrite.pImageInfo = &tmpVS;
					}
					break;
				}
				case EShaderBindType::SBT_UAV:
				{
					if (pBinder->IsStructuredBuffer)
					{
						auto pBuffer = ((VKUaView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
						tmpStructureBufferVS.buffer = pBuffer->mBuffer;
						tmpStructureBufferVS.offset = 0;
						tmpStructureBufferVS.range = pBuffer->Desc.Size;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
						descriptorWrite.pBufferInfo = &tmpStructureBufferVS;
					}
					else
					{
						ASSERT(false);
						tmpVS.imageLayout = VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL;
						tmpVS.imageView = ((VKSrView*)resource)->mImageView;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
						descriptorWrite.pImageInfo = &tmpVS;
					}
					break;
				}
				case EShaderBindType::SBT_Sampler:
				{
					//tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
					//tmp.imageView = ((VKSrView*)resource)->mImageView;
					tmpVS.sampler = ((VKSampler*)resource)->mView;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
					descriptorWrite.pImageInfo = &tmpVS;
					break;
				}
				default:
					break;
			}

			dsWriteSets.push_back(descriptorWrite);
		}
		pBinder = binder->PSBinder;
		if (pBinder != nullptr)
		{
			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = dsSetPS->RealObject;

			descriptorWrite.dstBinding = pBinder->Slot;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			switch (binder->BindType)
			{
				case EShaderBindType::SBT_CBuffer:
				{
					auto pBuffer = ((VKCbView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
					tmpStructureBufferPS.buffer = pBuffer->mBuffer;
					tmpStructureBufferPS.offset = 0;
					tmpStructureBufferPS.range = pBuffer->Desc.Size;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
					descriptorWrite.pBufferInfo = &tmpStructureBufferPS;
					break;
				}
				case EShaderBindType::SBT_SRV:
				{
					if (pBinder->IsStructuredBuffer)
					{
						auto pBuffer = ((VKSrView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
						tmpStructureBufferPS.buffer = pBuffer->mBuffer;
						tmpStructureBufferPS.offset = 0;
						tmpStructureBufferPS.range = pBuffer->Desc.Size;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
						descriptorWrite.pBufferInfo = &tmpStructureBufferPS;
					}
					else
					{
						tmpPS.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
						tmpPS.imageView = ((VKSrView*)resource)->mImageView;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
						descriptorWrite.pImageInfo = &tmpPS;
					}
					break;
				}
				case EShaderBindType::SBT_UAV:
				{
					if (pBinder->IsStructuredBuffer)
					{
						auto pBuffer = ((VKUaView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
						tmpStructureBufferPS.buffer = pBuffer->mBuffer;
						tmpStructureBufferPS.offset = 0;
						tmpStructureBufferPS.range = pBuffer->Desc.Size;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
						descriptorWrite.pBufferInfo = &tmpStructureBufferPS;
					}
					else
					{
						ASSERT(false);
						tmpPS.imageLayout = VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL;
						tmpPS.imageView = ((VKSrView*)resource)->mImageView;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
						descriptorWrite.pImageInfo = &tmpPS;
					}
					break;
				}
				case EShaderBindType::SBT_Sampler:
				{
					//tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
					//tmp.imageView = ((VKSrView*)resource)->mImageView;
					tmpPS.sampler = ((VKSampler*)resource)->mView;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
					descriptorWrite.pImageInfo = &tmpPS;
					break;
				}
				default:
					break;
			}

			dsWriteSets.push_back(descriptorWrite);
		}
		
		vkUpdateDescriptorSets(device->mDevice, (UINT)dsWriteSets.size(), &dsWriteSets[0], 0, nullptr);
	}
	void VKGraphicDraw::RebuildDescriptorSets()
	{
		if (IsDirty == false)
		{
			UINT finger = 0;
			for (auto& i : BindResources)
			{
				finger += i.second->GetFingerPrint();
			}
			if (finger == FingerPrient)
				return;
			FingerPrient = finger;
		}
			
		IsDirty = false;
		auto device = mDeviceRef.GetPtr();
		auto vs = ShaderEffect->mVertexShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.Alloc();
		auto ps = ShaderEffect->mPixelShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.Alloc();

		for (auto& i : BindResources)
		{
			BindResourceToDescriptSets(device, vs, ps, i.first, i.second);
		}
		if (mDescriptorSetVS != nullptr)
		{
			device->DelayDestroy(mDescriptorSetVS);
			mDescriptorSetVS = nullptr;
		}
		if (mDescriptorSetPS != nullptr)
		{
			device->DelayDestroy(mDescriptorSetPS);
			mDescriptorSetPS = nullptr;
		}
		mDescriptorSetVS = vs;
		mDescriptorSetPS = ps;
	}
	void VKGraphicDraw::Commit(ICommandList* cmdlist)
	{
		//IGraphicDraw::Commit(cmdlist);

		if (Mesh == nullptr || ShaderEffect == nullptr)
			return;

		auto device = (VKGpuDevice*)cmdlist->mDevice.GetPtr();

		UpdateGpuDrawState(cmdlist->GetGpuDevice(), cmdlist, cmdlist->mCurrentFrameBuffers->mRenderPass);
		cmdlist->SetGraphicsPipeline(GpuDrawState);

		((VKCommandList*)cmdlist)->UseCurrentViewports();
		((VKCommandList*)cmdlist)->UseCurrentScissors();
		
		//Mesh->Commit(cmdlist);
		VkBuffer vkVBuffers[VST_Number]{};
		VkDeviceSize vkOffsets[VST_Number]{};
		FVertexArray* pVertexArray = Mesh->VertexArray;
		for (int i = 0; i < VST_Number; i++)
		{
			if (pVertexArray->VertexBuffers[i] == nullptr)
			{
				vkVBuffers[i] = device->mNullVB->mBuffer;
			}
			else
			{
				vkVBuffers[i] = pVertexArray->VertexBuffers[i]->Buffer.UnsafeConvertTo<VKBuffer>()->mBuffer;
			}
		}
		vkCmdBindVertexBuffers(((VKCommandList*)cmdlist)->mCommandBuffer->RealObject, 0, VST_Number, vkVBuffers, vkOffsets);
		cmdlist->SetIndexBuffer(Mesh->IndexBuffer, Mesh->IsIndex32);

		if (AttachVB != nullptr)
		{
			AttachVB->Commit(cmdlist);
		}

		auto effect = (VKShaderEffect*)GetShaderEffect();

		effect->Commit(cmdlist, this);

		for (auto& i : BindResources)
		{
			switch (i.first->BindType)
			{
				case SBT_CBuffer:
				{
					IGpuResource* t = i.second;
					effect->BindCBuffer(cmdlist, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second;
					effect->BindSrv(cmdlist, i.first, (ISrView*)t);
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second;
					effect->BindUav(cmdlist, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second;
					effect->BindSampler(cmdlist, i.first, (ISampler*)t);
				}
				break;
				default:
					break;
			}
		}

		RebuildDescriptorSets();

		VkDescriptorSet dsSets[2] = { mDescriptorSetVS->RealObject, mDescriptorSetPS->RealObject };
		auto vkCmd = (VKCommandList*)cmdlist;
		vkCmdBindDescriptorSets(vkCmd->mCommandBuffer->RealObject, VK_PIPELINE_BIND_POINT_GRAPHICS, effect->mPipelineLayout, 0, 2, dsSets, 0, nullptr);

		auto pDrawDesc = Mesh->GetAtomDesc(MeshAtom, MeshLOD);
		ASSERT(pDrawDesc);
		if (IndirectDrawArgsBuffer)
		{
			cmdlist->IndirectDrawIndexed(pDrawDesc->PrimitiveType, IndirectDrawArgsBuffer, IndirectDrawOffsetForArgs);
		}
		else
		{
			if (pDrawDesc->IsIndexDraw())
			{
				cmdlist->DrawIndexed(pDrawDesc->PrimitiveType, pDrawDesc->BaseVertexIndex, pDrawDesc->StartIndex, pDrawDesc->NumPrimitives, DrawInstance);
			}
			else
			{
				cmdlist->Draw(pDrawDesc->PrimitiveType, pDrawDesc->BaseVertexIndex, pDrawDesc->NumPrimitives, DrawInstance);
			}
		}
	}

	////////////////////////////////////////////////////////
	void VKComputeDraw::OnBindResource(const FShaderBinder* binder, IGpuResource* resource)
	{
		IsDirty = true;
	}
	void CSBindResourceToDescriptSets(VKGpuDevice* device, AutoRef<MemAlloc::FPagedObject<VkDescriptorSet>>& dsSetVS,
		const FShaderBinder* binder, IGpuResource* resource)
	{
		VkDescriptorImageInfo tmpVS{};
		VkDescriptorBufferInfo tmpStructureBufferVS{};
		std::vector<VkWriteDescriptorSet>	dsWriteSets;
		auto pBinder = binder;
		if (pBinder != nullptr)
		{
			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = dsSetVS->RealObject;

			descriptorWrite.dstBinding = pBinder->Slot;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			switch (binder->Type)
			{
				case EShaderBindType::SBT_CBuffer:
				{
					auto pBuffer = ((VKCbView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
					tmpStructureBufferVS.buffer = pBuffer->mBuffer;
					tmpStructureBufferVS.offset = 0;
					tmpStructureBufferVS.range = pBuffer->Desc.Size;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
					descriptorWrite.pBufferInfo = &tmpStructureBufferVS;
					break;
				}
				case EShaderBindType::SBT_SRV:
				{
					if (pBinder->IsStructuredBuffer)
					{
						auto pBuffer = ((VKSrView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
						tmpStructureBufferVS.buffer = pBuffer->mBuffer;
						tmpStructureBufferVS.offset = 0;
						tmpStructureBufferVS.range = pBuffer->Desc.Size;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
						descriptorWrite.pBufferInfo = &tmpStructureBufferVS;
					}
					else
					{
						/*auto pTexture = ((VKSrView*)resource)->Buffer.UnsafeConvertTo<VKTexture>();
						if (pTexture->Desc.BindFlags & EBufferType::BFT_DSV)
						{
							tmpVS.imageLayout = VK_IMAGE_LAYOUT_DEPTH_READ_ONLY_OPTIMAL;
						}
						else
						{
							tmpVS.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
						}*/
						tmpVS.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
						
						tmpVS.imageView = ((VKSrView*)resource)->mImageView;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
						descriptorWrite.pImageInfo = &tmpVS;
					}
					break;
				}
				case EShaderBindType::SBT_UAV:
				{
					if (pBinder->IsStructuredBuffer)
					{
						auto pBuffer = ((VKUaView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
						tmpStructureBufferVS.buffer = pBuffer->mBuffer;
						tmpStructureBufferVS.offset = 0;
						tmpStructureBufferVS.range = pBuffer->Desc.Size;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
						descriptorWrite.pBufferInfo = &tmpStructureBufferVS;
					}
					else
					{
						ASSERT(false);
						tmpVS.imageLayout = VK_IMAGE_LAYOUT_ATTACHMENT_OPTIMAL;
						tmpVS.imageView = ((VKSrView*)resource)->mImageView;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
						descriptorWrite.pImageInfo = &tmpVS;
					}
					break;
				}
				case EShaderBindType::SBT_Sampler:
				{
					//tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
					//tmp.imageView = ((VKSrView*)resource)->mImageView;
					tmpVS.sampler = ((VKSampler*)resource)->mView;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
					descriptorWrite.pImageInfo = &tmpVS;
					break;
				}
				default:
					break;
			}

			dsWriteSets.push_back(descriptorWrite);
		}
		
		vkUpdateDescriptorSets(device->mDevice, (UINT)dsWriteSets.size(), &dsWriteSets[0], 0, nullptr);
	}
	void VKComputeDraw::RebuildDescriptorSets()
	{
		if (IsDirty == false)
		{
			UINT finger = 0;
			for (auto& i : BindResources)
			{
				finger += i.second->GetFingerPrint();
			}
			if (finger == FingerPrient)
				return;
			FingerPrient = finger;
		}

		IsDirty = false;
		auto device = mDeviceRef.GetPtr();
		auto cs = mEffect->mComputeShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.Alloc();

		for (auto& i : BindResources)
		{
			CSBindResourceToDescriptSets(device, cs, i.first, i.second);
		}
		if (mDescriptorSetCS != nullptr)
		{
			device->DelayDestroy(mDescriptorSetCS);
			mDescriptorSetCS = nullptr;
		}
		mDescriptorSetCS = cs;
	}
	void VKComputeDraw::Commit(ICommandList* cmdlist)
	{
		if (mEffect == nullptr)
			return;

		//cmdlist->SetShader(mEffect->mComputeShader);
		cmdlist->SetComputePipeline(mEffect);
		
		mEffect->Commit(cmdlist);
		for (auto& i : BindResources)
		{
			switch (i.first->Type)
			{
				case SBT_CBuffer:
				{
					IGpuResource* t = i.second;
					cmdlist->SetCBV(EShaderType::SDT_ComputeShader, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second;
					cmdlist->SetSrv(EShaderType::SDT_ComputeShader, i.first, (ISrView*)t);
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second;
					cmdlist->SetUav(EShaderType::SDT_ComputeShader, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second;
					cmdlist->SetSampler(EShaderType::SDT_ComputeShader, i.first, (ISampler*)t);
				}
				break;
				default:
					break;
			}
		}

		RebuildDescriptorSets();

		auto vkEffect = mEffect.UnsafeConvertTo<VKComputeEffect>();
		VkDescriptorSet dsSets = mDescriptorSetCS->RealObject;
		auto vkCmd = (VKCommandList*)cmdlist;
		vkCmdBindDescriptorSets(vkCmd->mCommandBuffer->RealObject, VK_PIPELINE_BIND_POINT_COMPUTE, vkEffect->mPipelineLayout, 0, 1, &dsSets, 0, nullptr);

		if (IndirectDispatchArgsBuffer != nullptr)
		{
			cmdlist->IndirectDispatch(IndirectDispatchArgsBuffer, 0);
		}
		else
		{
			cmdlist->Dispatch(mDispatchX, mDispatchY, mDispatchZ);
		}
	}
}

NS_END
