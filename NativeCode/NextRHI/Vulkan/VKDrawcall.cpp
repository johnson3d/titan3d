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
	void BindStageResourceToDescriptSets(VKGpuDevice* device,
		const FShaderBinder* pBinder, IGpuResource* resource, VKDescriptorSetHolder* pDescriptorSet, std::vector<VkWriteDescriptorSet>& dsWriteSets, std::vector<FDescriptorSetInfo>& dsSetInfos, int index)
	{
		/*if (resource == nullptr)
			return;*/
		pDescriptorSet->UseResource(resource);
		dsSetInfos[index] = FDescriptorSetInfo{};
		VkDescriptorImageInfo& imageInfo = dsSetInfos[index].imageInfo;
		VkDescriptorBufferInfo& bufferInfo = dsSetInfos[index].bufferInfo;
		if (pBinder != nullptr)
		{
			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = pDescriptorSet->DescriptorSet->RealObject;

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
							pDescriptorSet->UseResource(((VKSrView*)resource)->Buffer);
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
	void VKGraphicDraw::BindResourceToDescriptSets(VKGpuDevice* device, 
						const FEffectBinder* binder, IGpuResource* resource, std::vector<VkWriteDescriptorSet>& dsWriteSets, int index)
	{
		BindStageResourceToDescriptSets(device, binder->VSBinder, resource, mDescriptorSetVS, dsWriteSets, mDescriptorSetInfos, index);
		
		BindStageResourceToDescriptSets(device, binder->PSBinder, resource, mDescriptorSetPS, dsWriteSets, mDescriptorSetInfos, index);
	}
	
	void VKGraphicDraw::UpdateDescriptorSets(VKCommandList* vkCmd)
	{
		auto device = mDeviceRef.GetPtr();

		mDescriptorSetVS = MakeWeakRef(ShaderEffect->mVertexShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.AllocDecriptorSet());
		mDescriptorSetPS = MakeWeakRef(ShaderEffect->mPixelShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.AllocDecriptorSet());
		vkCmd->GetCmdRecorder()->UseResource(mDescriptorSetVS);
		vkCmd->GetCmdRecorder()->UseResource(mDescriptorSetPS);

		
		mDsWriteSets.clear();
		mDescriptorSetInfos.resize(BindResources.size());
		int index = 0;
		for (auto& i : BindResources)
		{
			switch (i.first->BindType)
			{
				case SBT_CBV:
				{
					IGpuResource* t = i.second.Resource;
					if (i.first->VSBinder)
					{
						vkCmd->SetCBV(EShaderType::SDT_VertexShader, i.first->VSBinder, (ICbView*)t);
					}
					else if (i.first->PSBinder)
					{
						vkCmd->SetCBV(EShaderType::SDT_PixelShader, i.first->VSBinder, (ICbView*)t);
					}
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second.Resource;
					if (i.first->VSBinder)
					{
						vkCmd->SetSrv(EShaderType::SDT_VertexShader, i.first->VSBinder, (ISrView*)t);
					}
					else if (i.first->PSBinder)
					{
						vkCmd->SetSrv(EShaderType::SDT_PixelShader, i.first->VSBinder, (ISrView*)t);
					}
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second.Resource;
					if (i.first->VSBinder)
					{
						vkCmd->SetUav(EShaderType::SDT_VertexShader, i.first->VSBinder, (IUaView*)t);
					}
					else if (i.first->PSBinder)
					{
						vkCmd->SetUav(EShaderType::SDT_PixelShader, i.first->VSBinder, (IUaView*)t);
					}
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second.Resource;
					if (i.first->VSBinder)
					{
						vkCmd->SetSampler(EShaderType::SDT_VertexShader, i.first->VSBinder, (ISampler*)t);
					}
					else if (i.first->PSBinder)
					{
						vkCmd->SetSampler(EShaderType::SDT_PixelShader, i.first->VSBinder, (ISampler*)t);
					}
				}
				break;
				default:
					break;
			}
			BindResourceToDescriptSets(device, i.first, i.second.Resource, mDsWriteSets, index++);
		}
		if (mDsWriteSets.size() > 0)
		{
			vkUpdateDescriptorSets(device->mDevice, (UINT)mDsWriteSets.size(), &mDsWriteSets[0], 0, nullptr);
		}
		mDsWriteSets.clear();
	}
	void VKGraphicDraw::BindDescriptorSets(VKCommandList* vkCmd)
	{
		auto effect = (VKGraphicsEffect*)GetGraphicsEffect();
		VkDescriptorSet dsSets[2] = { mDescriptorSetVS->DescriptorSet->RealObject, mDescriptorSetPS->DescriptorSet->RealObject };
		vkCmdBindDescriptorSets(vkCmd->GetVKCmdRecorder()->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, effect->mPipelineLayout, 0, 2, dsSets, 0, nullptr);
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
		const FShaderBinder* binder, IGpuResource* resource, std::vector<VkWriteDescriptorSet>& dsWriteSets, int index)
	{
		BindStageResourceToDescriptSets(device, binder, resource, mDescriptorSetCS, dsWriteSets, mDescriptorSetInfos, index);
	}

	void VKComputeDraw::UpdateDescriptorSets(VKCommandList* vkCmd)
	{
		auto device = mDeviceRef.GetPtr();
		mDescriptorSetCS = MakeWeakRef(mEffect->mComputeShader.UnsafeConvertTo<VKShader>()->mDescriptorSetAllocator.AllocDecriptorSet());
		vkCmd->GetCmdRecorder()->UseResource(mDescriptorSetCS);

		mDsWriteSets.clear();
		mDescriptorSetInfos.resize(BindResources.size());
		int index = 0;
		for (auto& i : BindResources)
		{
			switch (i.first->Type)
			{
				case SBT_CBV:
				{
					IGpuResource* t = i.second.Resource;
					vkCmd->SetCBV(EShaderType::SDT_ComputeShader, i.first, (ICbView*)t);
				}
				break;
				case SBT_SRV:
				{
					IGpuResource* t = i.second.Resource;
					vkCmd->SetSrv(EShaderType::SDT_ComputeShader, i.first, (ISrView*)t);
				}
				break;
				case SBT_UAV:
				{
					IGpuResource* t = i.second.Resource;
					vkCmd->SetUav(EShaderType::SDT_ComputeShader, i.first, (IUaView*)t);
				}
				break;
				case SBT_Sampler:
				{
					IGpuResource* t = i.second.Resource;
					vkCmd->SetSampler(EShaderType::SDT_ComputeShader, i.first, (ISampler*)t);
				}
				break;
				default:
					break;
			}
			BindResourceToDescriptSets(device, i.first, i.second.Resource, mDsWriteSets, index++);
		}
		if (mDsWriteSets.size() > 0)
		{
			vkUpdateDescriptorSets(device->mDevice, (UINT)mDsWriteSets.size(), &mDsWriteSets[0], 0, nullptr);
		}
		mDsWriteSets.clear();
	}
	void VKComputeDraw::BindDescriptorSets(VKCommandList* vkCmd)
	{
		auto vkEffect = mEffect.UnsafeConvertTo<VKComputeEffect>();
		VkDescriptorSet dsSets = mDescriptorSetCS->DescriptorSet->RealObject;
		vkCmdBindDescriptorSets(vkCmd->GetVKCmdRecorder()->mCommandBuffer, VK_PIPELINE_BIND_POINT_COMPUTE, vkEffect->mPipelineLayout, 0, 1, &dsSets, 0, nullptr);
	}
	void VKComputeDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		if (mEffect == nullptr)
			return;

		auto vkCmd = (VKCommandList*)cmdlist;

		cmdlist->SetComputePipeline(mEffect);
		
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
