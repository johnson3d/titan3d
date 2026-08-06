#include "VKDrawcall.h"
#include "VKEffect.h"
#include "VKGpuDevice.h"
#include "VKBuffer.h"
#include "VKGpuState.h"
#include "VKCommandList.h"
#include "VKFrameBuffers.h"
#include "VKShader.h"
#include "VKDescriptorSet.h"

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
	VkDescriptorSet BindStageResourceToDescriptSets(VKGpuDevice* device, VkDescriptorSet pDescriptorSet, 
		const FShaderBinder* pBinder, IGpuResource* resource, std::vector<VkWriteDescriptorSet>& dsWriteSets, std::vector<FDescriptorSetInfo>& dsSetInfos, int& index)
	{
		/*if (resource == nullptr)
			return;*/

		dsSetInfos[index] = FDescriptorSetInfo{};
		VkDescriptorImageInfo& imageInfo = dsSetInfos[index].imageInfo;
		VkDescriptorBufferInfo& bufferInfo = dsSetInfos[index].bufferInfo;
		FDescriptorSetInfo::FRtasInfo& rtasInfo = dsSetInfos[index].rtasInfo;
		index++;

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
					if (pBinder->ResourceType == EShaderBindResourceType::SBRT_AccelerationStructure)
					{
						//TLAS binding, align with DX12 D3D12_SRV_DIMENSION_RAYTRACING_ACCELERATION_STRUCTURE
						rtasInfo.asInfo = {};
						rtasInfo.asInfo.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET_ACCELERATION_STRUCTURE_KHR;
						rtasInfo.asInfo.accelerationStructureCount = 1;
						if (resource != nullptr)
						{
							auto pBuffer = ((VKSrView*)resource)->Buffer.UnsafeConvertTo<VKBuffer>();
							rtasInfo.asHandle = pBuffer->mAccelerationStructure;
						}
						else
						{
							rtasInfo.asHandle = VK_NULL_HANDLE;
						}
						rtasInfo.asInfo.pAccelerationStructures = &rtasInfo.asHandle;
						descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_ACCELERATION_STRUCTURE_KHR;
						descriptorWrite.pNext = &rtasInfo.asInfo;
					}
					else if (pBinder->ResourceType == EShaderBindResourceType::SBRT_Buffer)
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
							imageInfo.imageView = ((VKSrView*)resource)->mView->mImageView;
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
					if (pBinder->ResourceType == EShaderBindResourceType::SBRT_Buffer)
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
							imageInfo.imageView = ((VKUaView*)resource)->mView->mImageView;
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
		return pDescriptorSet;
	}

	void PushDS(std::vector<VkDescriptorSet>& sets, VkDescriptorSet ds)
	{
		for(auto i : sets)
		{
			if (i == ds)
				return; // already in the list
		}
		sets.push_back(ds);
	}
	
	void VKGraphicDraw::UpdateDescriptorSets(VKCommandList* vkCmd)
	{
		auto device = mDeviceRef.GetPtr();
		auto effect = this->ShaderEffect.UnsafeConvertTo<VKGraphicsEffect>();
		const auto& layouts = effect->mLayouts;
		mDsWriteSets.clear();
		mDescriptorSetInfos.resize(layouts[0]->mBindings.size());
		mDescriptorSets.clear();
		auto pPool = (VKDescriptorPool*)device->GetDescriptorPoolManager()->GetCurrentFramePool();
		VkDescriptorSet pDescriptorSet = pPool->AllocDescriptorSet(layouts[0]->mLayout);
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
						BindStageResourceToDescriptSets(device, pDescriptorSet, binder->VSBinder, resource, mDsWriteSets, mDescriptorSetInfos, index);
					}
					if (binder->PSBinder)
					{
						BindStageResourceToDescriptSets(device, pDescriptorSet, binder->PSBinder, resource, mDsWriteSets, mDescriptorSetInfos, index);
					}
				}
				break;
				case SBT_SRV:
				{
					if (resource)
					{
						vkCmd->GetCmdRecorder()->UseResource(((VKSrView*)resource)->mView);
					}
					if (binder->VSBinder)
					{
						BindStageResourceToDescriptSets(device, pDescriptorSet, binder->VSBinder, resource, mDsWriteSets, mDescriptorSetInfos, index);
					}
					if (binder->PSBinder)
					{
						BindStageResourceToDescriptSets(device, pDescriptorSet, binder->PSBinder, resource, mDsWriteSets, mDescriptorSetInfos, index);
					}
				}
				break;
				case SBT_UAV:
				{
					if (resource)
					{
						vkCmd->GetCmdRecorder()->UseResource(((VKUaView*)resource)->mView);
					}
					if (binder->VSBinder)
					{
						BindStageResourceToDescriptSets(device, pDescriptorSet, binder->VSBinder, resource, mDsWriteSets, mDescriptorSetInfos, index);
					}
					if (binder->PSBinder)
					{
						BindStageResourceToDescriptSets(device, pDescriptorSet, binder->PSBinder, resource, mDsWriteSets, mDescriptorSetInfos, index);
					}
				}
				break;
				case SBT_Sampler:
				{
					if (binder->VSBinder)
					{
						BindStageResourceToDescriptSets(device, pDescriptorSet, binder->VSBinder, resource, mDsWriteSets, mDescriptorSetInfos, index);
					}
					if (binder->PSBinder)
					{
						BindStageResourceToDescriptSets(device, pDescriptorSet, binder->PSBinder, resource, mDsWriteSets, mDescriptorSetInfos, index);
					}
				}
				break;
				default:
					break;
			}
		}
		if (mDsWriteSets.size() > 0)
		{
			vkUpdateDescriptorSets(device->mDevice, (UINT)mDsWriteSets.size(), &mDsWriteSets[0], 0, nullptr);
		}
		mDsWriteSets.clear();

		vkCmdBindDescriptorSets(vkCmd->GetVKCmdRecorder()->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, effect->mPipelineLayout, 0, (UINT)1, &pDescriptorSet, 0, nullptr);
		mDescriptorSets.clear();
	}
	void VKGraphicDraw::BuildDrawcall(ICommandList* cmdlist)
	{
		auto vkCmd = (VKCommandList*)cmdlist;
		for (auto& i : BindResources)
		{
			auto binder = i.first;
			auto resource = i.second.Resource;
			auto shaderBinder = binder->GetShaderBinder();
			switch (binder->BindType)
			{
				case SBT_CBV:
				{
					vkCmd->SetCBV(EShaderType::SDT_Unknown, shaderBinder, (ICbView*)resource);
				}
				break;
				case SBT_SRV:
				{
					vkCmd->SetSrv(EShaderType::SDT_Unknown, shaderBinder, (ISrView*)resource);
				}
				break;
				case SBT_UAV:
				{
					vkCmd->SetUav(EShaderType::SDT_Unknown, shaderBinder, (IUaView*)resource);
				}
				break;
				case SBT_Sampler:
				{
					vkCmd->SetSampler(EShaderType::SDT_Unknown, shaderBinder, (ISampler*)resource);
				}
				break;
				default:
					break;
			}
		}
		if (IndirectDrawArgsBuffer)
			FTransitionScope::TryAutoTransition(cmdlist, IndirectDrawArgsBuffer, GRS_UavIndirect, true);
	}
	void VKGraphicDraw::Commit(ICommandList* cmdlist, bool bRefResource)
	{
		//IGraphicDraw::Commit(cmdlist);

		if (Mesh == nullptr || ShaderEffect == nullptr)
			return;

		auto device = cmdlist->mDevice.GetCastPtr<VKGpuDevice>();

		if (ScissorRect != nullptr)
		{
			cmdlist->SetScissor(1, &ScissorRect->ScissorRect);
		}

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

		FMeshAtomDesc* pDrawDesc;
		if (AtomDesc == nullptr)
			pDrawDesc = Mesh->GetAtomDesc(MeshAtom, MeshLOD);
		else
			pDrawDesc = &AtomDesc->AtomDesc;
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
		auto effect = this->mEffect.UnsafeConvertTo<VKComputeEffect>();
		const auto& layouts = effect->mLayouts;
		auto pPool = (VKDescriptorPool*)device->GetDescriptorPoolManager()->GetCurrentFramePool();
		VkDescriptorSet pDescriptorSet = pPool->AllocDescriptorSet(layouts[0]->mLayout);
		mDsWriteSets.clear();
		mDescriptorSetInfos.resize(layouts[0]->mBindings.size());
		mDescriptorSets.clear();
		int index = 0;
		for (auto& i : BindResources)
		{
			auto binder = i.first;
			auto resource = i.second.Resource;
			switch (i.first->Type)
			{
				case SBT_CBV:
				{
					//vkCmd->SetCBV(EShaderType::SDT_ComputeShader, binder, (ICbView*)resource);
					BindStageResourceToDescriptSets(device, pDescriptorSet, binder, resource, mDsWriteSets, mDescriptorSetInfos, index);
				}
				break;
				case SBT_SRV:
				{
					//vkCmd->SetSrv(EShaderType::SDT_ComputeShader, binder, (ISrView*)resource);
					BindStageResourceToDescriptSets(device, pDescriptorSet, binder, resource, mDsWriteSets, mDescriptorSetInfos, index);
				}
				break;
				case SBT_UAV:
				{
					//vkCmd->SetUav(EShaderType::SDT_ComputeShader, binder, (IUaView*)resource);
					BindStageResourceToDescriptSets(device, pDescriptorSet, binder, resource, mDsWriteSets, mDescriptorSetInfos, index);
				}
				break;
				case SBT_Sampler:
				{
					//vkCmd->SetSampler(EShaderType::SDT_ComputeShader, binder, (ISampler*)resource);
					BindStageResourceToDescriptSets(device, pDescriptorSet, binder, resource, mDsWriteSets, mDescriptorSetInfos, index);
				}
				break;
				default:
					break;
			}
		}
		if (mDsWriteSets.size() > 0)
		{
			vkUpdateDescriptorSets(device->mDevice, (UINT)mDsWriteSets.size(), &mDsWriteSets[0], 0, nullptr);
		}
		mDsWriteSets.clear();

		vkCmdBindDescriptorSets(vkCmd->GetVKCmdRecorder()->mCommandBuffer, VK_PIPELINE_BIND_POINT_COMPUTE, effect->mPipelineLayout, 0, (UINT)1, &pDescriptorSet, 0, nullptr);
		mDescriptorSets.clear();
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
