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
	void BindStageResourceToDescriptSets(VKGpuDevice* device,
		const FShaderBinder* pBinder, IGpuResource* resource, VkDescriptorSet pDescriptorSet, std::vector<VkWriteDescriptorSet>& dsWriteSets, std::vector<FDescriptorSetInfo>& dsSetInfos, int& index)
	{
		index++;
		/*if (resource == nullptr)
			return;*/
		dsSetInfos[index] = FDescriptorSetInfo{};
		VkDescriptorImageInfo& imageInfo = dsSetInfos[index].imageInfo;
		VkDescriptorBufferInfo& bufferInfo = dsSetInfos[index].bufferInfo;
		if (pBinder != nullptr)
		{
			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = pDescriptorSet;

			descriptorWrite.dstBinding = pBinder->Slot;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			switch (pBinder->Type)
			{
				case EShaderBindType::SBT_CBV:
				{
					if (resource)
					{
						auto pBuffer = ((VKCbView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
						bufferInfo.buffer = pBuffer->mBuffer;
						bufferInfo.range = pBuffer->Desc.Size;
					}
					else
					{
						bufferInfo.buffer = nullptr;
						bufferInfo.range = VK_WHOLE_SIZE;
					}
					
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
					descriptorWrite.pBufferInfo = &bufferInfo;
					break;
				}
				case EShaderBindType::SBT_SRV:
				{
					if (pBinder->IsStructuredBuffer)
					{
						if (resource)
						{
							VKBuffer* pBuffer = ((VKSrView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
							bufferInfo.buffer = pBuffer->mBuffer;
							bufferInfo.range = pBuffer->Desc.Size;
						}
						else
						{
							bufferInfo.buffer = nullptr;
							bufferInfo.range = VK_WHOLE_SIZE;
						}
						bufferInfo.offset = 0;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
						descriptorWrite.pBufferInfo = &bufferInfo;
					}
					else
					{
						imageInfo.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
						if (resource)
						{
							imageInfo.imageView = ((VKSrView*)resource)->mImageView;
						}
						else
							imageInfo.imageView = nullptr;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
						descriptorWrite.pImageInfo = &imageInfo;
					}
					break;
				}
				case EShaderBindType::SBT_UAV:
				{
					if (pBinder->IsStructuredBuffer)
					{
						if (resource)
						{
							VKBuffer* pBuffer = ((VKUaView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
							bufferInfo.buffer = pBuffer->mBuffer;
							bufferInfo.range = pBuffer->Desc.Size;
						}
						else
						{
							bufferInfo.buffer = nullptr;
							bufferInfo.range = VK_WHOLE_SIZE;
						}
						bufferInfo.offset = 0;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
						descriptorWrite.pBufferInfo = &bufferInfo;
					}
					else
					{
						//ASSERT(false);
						imageInfo.imageLayout = VK_IMAGE_LAYOUT_GENERAL;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_IMAGE;
						if (resource)
						{
							imageInfo.imageView = ((VKUaView*)resource)->mImageView;
						}
						else
							imageInfo.imageView = nullptr;
						descriptorWrite.pImageInfo = &imageInfo;
					}
					break;
				}
				case EShaderBindType::SBT_Sampler:
				{
					//tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
					//tmp.imageView = ((VKSrView*)resource)->mImageView;
					if (resource)
						imageInfo.sampler = ((VKSampler*)resource)->mSamplder;
					else
						imageInfo.sampler = device->mNullSampler->mSamplder;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
					descriptorWrite.pImageInfo = &imageInfo;
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
		auto pool = device->mDescriptorPoolManager->GetCurrentFramePool();
		auto effect = this->ShaderEffect.UnsafeConvertTo<VKGraphicsEffect>();
		VkDescriptorSet ds = pool->AllocDescriptorSet(effect->mLayout);
		mDsWriteSets.clear();
		mDescriptorSetInfos.resize(effect->mBindings.size());
		int index = 0;
		for (auto& i : BindResources)
		{
			auto binder = i.first;
			auto resource = i.second.Resource;
			switch (binder->BindType)
			{
				case SBT_CBV:
				{
					if (binder->VSBinder)
					{
						vkCmd->SetCBV(EShaderType::SDT_VertexShader, binder->VSBinder, (ICbView*)resource);
					}
					if (binder->PSBinder)
					{
						vkCmd->SetCBV(EShaderType::SDT_PixelShader, binder->VSBinder, (ICbView*)resource);
					}
				}
				break;
				case SBT_SRV:
				{
					if (binder->VSBinder)
					{
						vkCmd->SetSrv(EShaderType::SDT_VertexShader, binder->VSBinder, (ISrView*)resource);
					}
					if (binder->PSBinder)
					{
						vkCmd->SetSrv(EShaderType::SDT_PixelShader, binder->VSBinder, (ISrView*)resource);
					}
				}
				break;
				case SBT_UAV:
				{
					if (binder->VSBinder)
					{
						vkCmd->SetUav(EShaderType::SDT_VertexShader, binder->VSBinder, (IUaView*)resource);
					}
					if (binder->PSBinder)
					{
						vkCmd->SetUav(EShaderType::SDT_PixelShader, binder->VSBinder, (IUaView*)resource);
					}
				}
				break;
				case SBT_Sampler:
				{
					if (binder->VSBinder)
					{
						vkCmd->SetSampler(EShaderType::SDT_VertexShader, binder->VSBinder, (ISampler*)resource);
					}
					if (binder->PSBinder)
					{
						vkCmd->SetSampler(EShaderType::SDT_PixelShader, binder->VSBinder, (ISampler*)resource);
					}
				}
				break;
				default:
					break;
			}

			if (binder->VSBinder)
			{
				BindStageResourceToDescriptSets(device, binder->VSBinder, resource, ds, mDsWriteSets, mDescriptorSetInfos, index);
			}
			if (binder->PSBinder)
			{
				BindStageResourceToDescriptSets(device, binder->PSBinder, resource, ds, mDsWriteSets, mDescriptorSetInfos, index);
			}
		}
		if (mDsWriteSets.size() > 0)
		{
			vkUpdateDescriptorSets(device->mDevice, (UINT)mDsWriteSets.size(), &mDsWriteSets[0], 0, nullptr);
		}
		mDsWriteSets.clear();

		vkCmdBindDescriptorSets(vkCmd->GetVKCmdRecorder()->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, effect->mPipelineLayout, 0, 1, &ds, 0, nullptr);
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
		vkCmdBindVertexBuffers(((VKCommandList*)cmdlist)->GetVKCmdRecorder()->mCommandBuffer, 0, VST_Number, vkVBuffers, vkOffsets);
		cmdlist->SetIndexBuffer(Mesh->IndexBuffer, Mesh->IsIndex32);

		if (AttachVB != nullptr)
		{
			AttachVB->Commit(cmdlist);
		}

		auto vkCmd = (VKCommandList*)cmdlist;
		UpdateDescriptorSets(vkCmd);

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

	void VKComputeDraw::UpdateDescriptorSets(VKCommandList* vkCmd)
	{
		auto device = mDeviceRef.GetPtr();
		auto pool = device->mDescriptorPoolManager->GetCurrentFramePool();
		auto effect = this->mEffect.UnsafeConvertTo<VKComputeEffect>();
		auto ds = pool->AllocDescriptorSet(effect->mLayout);
		
		mDsWriteSets.clear();
		mDescriptorSetInfos.resize(effect->mBindings.size());
		int index = 0;
		for (auto& i : BindResources)
		{
			auto binder = i.first;
			auto resource = i.second.Resource;
			switch (i.first->Type)
			{
				case SBT_CBV:
				{
					vkCmd->SetCBV(EShaderType::SDT_ComputeShader, binder, (ICbView*)resource);
				}
				break;
				case SBT_SRV:
				{
					vkCmd->SetSrv(EShaderType::SDT_ComputeShader, binder, (ISrView*)resource);
				}
				break;
				case SBT_UAV:
				{
					vkCmd->SetUav(EShaderType::SDT_ComputeShader, binder, (IUaView*)resource);
				}
				break;
				case SBT_Sampler:
				{
					vkCmd->SetSampler(EShaderType::SDT_ComputeShader, binder, (ISampler*)resource);
				}
				break;
				default:
					break;
			}
			BindStageResourceToDescriptSets(device, binder, resource, ds, mDsWriteSets, mDescriptorSetInfos, index);
		}
		if (mDsWriteSets.size() > 0)
		{
			vkUpdateDescriptorSets(device->mDevice, (UINT)mDsWriteSets.size(), &mDsWriteSets[0], 0, nullptr);
		}
		mDsWriteSets.clear();
		vkCmdBindDescriptorSets(vkCmd->GetVKCmdRecorder()->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, effect->mPipelineLayout, 0, 1, &ds, 0, nullptr);
	}
	void VKComputeDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		if (mEffect == nullptr)
			return;

		auto vkCmd = (VKCommandList*)cmdlist;

		cmdlist->SetComputePipeline(mEffect);
		
		UpdateDescriptorSets(vkCmd);

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
