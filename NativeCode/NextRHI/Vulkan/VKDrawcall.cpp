#include "VKDrawcall.h"
#include "VKEffect.h"
#include "VKGpuDevice.h"
#include "VKBuffer.h"
#include "VKGpuState.h"
#include "VKCommandList.h"
#include "VKFrameBuffers.h"
#include "VKShader.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	VKGraphicDraw::~VKGraphicDraw()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;
		mDescriptorSetVS = nullptr;
		mDescriptorSetPS = nullptr;
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
	void VKGraphicDraw::OnBindResource(const FEffectBinder* binder, FBindResource& resource)
	{
		IsDirty = true;
	}
	void VKGraphicDraw::BindResourceToDescriptSets(VKGpuDevice* device, 
						const FEffectBinder* binder, IGpuResource* resource, std::vector<VkWriteDescriptorSet>& dsWriteSets)
	{
		VkDescriptorImageInfo tmpVS{};
		VkDescriptorBufferInfo tmpStructureBufferVS{};
		VkDescriptorImageInfo tmpPS{};
		VkDescriptorBufferInfo tmpStructureBufferPS{};
		
		auto pBinder = binder->VSBinder;
		if (pBinder != nullptr)
		{
			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = mDescriptorSetVS->DescriptorSet->RealObject;

			descriptorWrite.dstBinding = pBinder->Slot;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			switch (binder->BindType)
			{
				case EShaderBindType::SBT_CBV:
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
			descriptorWrite.dstSet = mDescriptorSetPS->DescriptorSet->RealObject;

			descriptorWrite.dstBinding = pBinder->Slot;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			switch (binder->BindType)
			{
				case EShaderBindType::SBT_CBV:
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
	}
	void VKGraphicDraw::UpdateDescriptorSets(VKCommandList* vkCmd)
	{
		auto device = mDeviceRef.GetPtr();

		mDescriptorSetVS = ShaderEffect->mVertexShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.AllocDecriptorSet();
		mDescriptorSetPS = ShaderEffect->mPixelShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.AllocDecriptorSet();
		vkCmd->GetCmdRecorder()->UseResource(mDescriptorSetVS);
		vkCmd->GetCmdRecorder()->UseResource(mDescriptorSetPS);

		std::vector<VkWriteDescriptorSet> dsWriteSets;
		for (auto& i : BindResources)
		{
			BindResourceToDescriptSets(device, i.first, i.second.Resource, dsWriteSets);
		}
		vkUpdateDescriptorSets(device->mDevice, (UINT)dsWriteSets.size(), &dsWriteSets[0], 0, nullptr);
	}
	void VKGraphicDraw::BindDescriptorSets(VKCommandList* vkCmd)
	{
		auto effect = (VKGraphicsEffect*)GetGraphicsEffect();
		VkDescriptorSet dsSets[2] = { mDescriptorSetVS->DescriptorSet->RealObject, mDescriptorSetPS->DescriptorSet->RealObject };
		vkCmdBindDescriptorSets(vkCmd->mCommandBuffer->RealObject, VK_PIPELINE_BIND_POINT_GRAPHICS, effect->mPipelineLayout, 0, 2, dsSets, 0, nullptr);
	}
	void VKGraphicDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		//IGraphicDraw::Commit(cmdlist);

		if (Mesh == nullptr || ShaderEffect == nullptr)
			return;

		auto device = cmdlist->mDevice.GetCastPtr<VKGpuDevice>();

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

		auto effect = (VKGraphicsEffect*)GetGraphicsEffect();
		//effect->Commit(cmdlist, this);
		for (auto& i : BindResources)
		{
			switch (i.first->BindType)
			{
				case SBT_CBV:
				{
					IGpuResource* t = i.second.Resource;
					effect->BindCBV(cmdlist, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second.Resource;
					effect->BindSrv(cmdlist, i.first, (ISrView*)t);
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second.Resource;
					effect->BindUav(cmdlist, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second.Resource;
					effect->BindSampler(cmdlist, i.first, (ISampler*)t);
				}
				break;
				default:
					break;
			}
		}

		auto vkCmd = (VKCommandList*)cmdlist;
		UpdateDescriptorSets(vkCmd);
		BindDescriptorSets(vkCmd);

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
	void VKComputeDraw::OnBindResource(const FShaderBinder* binder, FBindResource& resource)
	{
		IsDirty = true;
	}
	void VKComputeDraw::BindResourceToDescriptSets(VKGpuDevice* device,
		const FShaderBinder* binder, IGpuResource* resource, std::vector<VkWriteDescriptorSet>& dsWriteSets)
	{
		VkDescriptorImageInfo tmpVS{};
		VkDescriptorBufferInfo tmpStructureBufferVS{};
		auto pBinder = binder;
		if (pBinder != nullptr)
		{
			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = mDescriptorSetCS->DescriptorSet->RealObject;

			descriptorWrite.dstBinding = pBinder->Slot;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			switch (binder->Type)
			{
				case EShaderBindType::SBT_CBV:
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
	}

	void VKComputeDraw::UpdateDescriptorSets(VKCommandList* vkCmd)
	{
		auto device = mDeviceRef.GetPtr();
		mDescriptorSetCS = mEffect->mComputeShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.AllocDecriptorSet();
		vkCmd->GetCmdRecorder()->UseResource(mDescriptorSetCS);

		std::vector<VkWriteDescriptorSet> dsWriteSets;
		for (auto& i : BindResources)
		{
			BindResourceToDescriptSets(device, i.first, i.second.Resource, dsWriteSets);
		}
		vkUpdateDescriptorSets(device->mDevice, (UINT)dsWriteSets.size(), &dsWriteSets[0], 0, nullptr);
	}
	void VKComputeDraw::BindDescriptorSets(VKCommandList* vkCmd)
	{
		auto vkEffect = mEffect.UnsafeConvertTo<VKComputeEffect>();
		VkDescriptorSet dsSets = mDescriptorSetCS->DescriptorSet->RealObject;
		vkCmdBindDescriptorSets(vkCmd->mCommandBuffer->RealObject, VK_PIPELINE_BIND_POINT_COMPUTE, vkEffect->mPipelineLayout, 0, 1, &dsSets, 0, nullptr);
	}
	void VKComputeDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		if (mEffect == nullptr)
			return;

		auto vkCmd = (VKCommandList*)cmdlist;

		cmdlist->SetComputePipeline(mEffect);
		
		//mEffect->Commit(cmdlist);
		for (auto& i : BindResources)
		{
			switch (i.first->Type)
			{
				case SBT_CBV:
				{
					IGpuResource* t = i.second.Resource;
					cmdlist->SetCBV(EShaderType::SDT_ComputeShader, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second.Resource;
					cmdlist->SetSrv(EShaderType::SDT_ComputeShader, i.first, (ISrView*)t);
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second.Resource;
					cmdlist->SetUav(EShaderType::SDT_ComputeShader, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second.Resource;
					cmdlist->SetSampler(EShaderType::SDT_ComputeShader, i.first, (ISampler*)t);
				}
				break;
				default:
					break;
			}
		}

		UpdateDescriptorSets(vkCmd);
		BindDescriptorSets(vkCmd);

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
