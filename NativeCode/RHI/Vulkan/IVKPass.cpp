#include "IVKPass.h"
#include "IVKCommandList.h"
#include "IVKRenderPipeline.h"
#include "IVKRenderContext.h"
#include "IVKConstantBuffer.h"
#include "IVKShaderResourceView.h"
#include "IVKSamplerState.h"
#include "IVKVertexBuffer.h"
#include "IVKIndexBuffer.h"
#include "IVKGpuBuffer.h"
#include "IVKFrameBuffers.h"

#include "../../Base/vfxsampcounter.h"
#include "../Utility/IMeshPrimitives.h"
#include "../ShaderReflector.h"

#define new VNEW

NS_BEGIN

void IVKDrawCall::VDescriptorSets::Init(IVKRenderContext* rc, IVKDrawCall* dr)
{
	auto pProgram = (IVKShaderProgram*)dr->mPipelineState->GetGpuProgram();
	auto backup = pProgram->mLayoutBindings;
	const auto& layoutBindings = pProgram->mLayoutBindings;
	const auto& setLayouts = pProgram->mDescriptorSetLayout;

	for (auto& i : layoutBindings)
	{
		auto stage = i.first;

		VkDescriptorSetAllocateInfo allocInfo{};
		allocInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
		allocInfo.descriptorPool = rc->mDescriptorPool->AllocDescriptor(pProgram->mAllocInfo[stage]);
		allocInfo.descriptorSetCount = 1;
		allocInfo.pSetLayouts = &setLayouts[stage];

		if (vkAllocateDescriptorSets(rc->mLogicalDevice, &allocInfo, &mDescriptorSet[stage]) != VK_SUCCESS)
		{
			return;
		}
	}
	if (dr->mShaderCBufferBinder != nullptr)
	{
		for (auto& i : dr->mShaderCBufferBinder->VSResources)
		{
			auto vkCB = i.second.UnsafeConvertTo<IVKConstantBuffer>();
			if (vkCB == nullptr || mDescriptorSet[STS_VS] == nullptr)
				continue;

			VkDescriptorBufferInfo tmp{};
			tmp.buffer = vkCB->mBuffer;
			tmp.offset = 0;
			tmp.range = vkCB->Desc.Size;

			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = mDescriptorSet[STS_VS];
			descriptorWrite.dstBinding = i.first;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
			descriptorWrite.descriptorCount = 1;
			descriptorWrite.pBufferInfo = &tmp;

			auto& bindings = backup[STS_VS];
			if (bindings.size() > 0)
			{
				vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);

				bool bFind = false;
				for (auto j = bindings.begin(); j != bindings.end(); j++)
				{
					if ((*j).binding == i.first)
					{
						bindings.erase(j);
						bFind = true;
						break;
					}
				}
				ASSERT(bFind);
			}
		}
		for (auto& i : dr->mShaderCBufferBinder->PSResources)
		{
			auto vkCB = i.second.UnsafeConvertTo<IVKConstantBuffer>();
			if (vkCB == nullptr || mDescriptorSet[STS_PS] == nullptr)
				continue;
			VkDescriptorBufferInfo tmp{};
			tmp.buffer = vkCB->mBuffer;
			tmp.offset = 0;
			tmp.range = vkCB->Desc.Size;

			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = mDescriptorSet[STS_PS];
			descriptorWrite.dstBinding = i.first;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
			descriptorWrite.descriptorCount = 1;
			descriptorWrite.pBufferInfo = &tmp;

			auto& bindings = backup[STS_PS];
			if (bindings.size() > 0)
			{
				vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);

				bool bFind = false;
				for (auto j = bindings.begin(); j != bindings.end(); j++)
				{
					if ((*j).binding == i.first)
					{
						bindings.erase(j);
						bFind = true;
						break;
					}
				}
				ASSERT(bFind);
			}
		}
		dr->mShaderCBufferBinder->IsDirty = false;
	}
	if (dr->mShaderSrvBinder != nullptr)
	{
		for (auto& i : dr->mShaderSrvBinder->VSResources)
		{
			auto vkCB = i.second.UnsafeConvertTo<IVKShaderResourceView>();
			if (vkCB == nullptr || mDescriptorSet[STS_VS] == nullptr)
				continue;

			auto idx = dr->GetReflector()->FindShaderBinderBySlot(EShaderBindType::SBT_Srv, EShaderType::EST_VertexShader, i.first);
			auto pBinder = (ShaderRViewBindInfo*)dr->GetReflector()->GetShaderBinder(EShaderBindType::SBT_Srv, idx);

			VkDescriptorImageInfo tmp{};			
			VkDescriptorBufferInfo tmpStructureBuffer{};
			
			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = mDescriptorSet[STS_VS];
			descriptorWrite.dstBinding = i.first;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;
			
			if (pBinder->BufferType == EGpuBufferType::GBT_UavBuffer)
			{
				auto pGpuBuffer = vkCB->mBuffer.UnsafeConvertTo<IVKGpuBuffer>();
				tmpStructureBuffer.buffer = pGpuBuffer->mBuffer;
				tmpStructureBuffer.offset = 0;
				tmpStructureBuffer.range = pGpuBuffer->mBufferDesc.ByteWidth;

				descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
				descriptorWrite.pBufferInfo = &tmpStructureBuffer;

				/*AutoRef<IVKGpuBuffer> refObj;
				refObj.StrongRef(pGpuBuffer);
				mUsedBuffers.push_back(refObj);*/
			}
			else if (pBinder->BufferType == EGpuBufferType::GBT_TBufferBuffer)
			{
				/*auto pGpuBuffer = vkCB->mBuffer.UnsafeConvertTo<IVKGpuBuffer>();
				tmpStructureBuffer.buffer = pGpuBuffer->mBuffer;
				tmpStructureBuffer.offset = 0;
				tmpStructureBuffer.range = pGpuBuffer->mBufferDesc.ByteWidth;*/

				descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
				descriptorWrite.pTexelBufferView = &vkCB->mBufferView;
			}
			else if (pBinder->BufferType == EGpuBufferType::GBT_Unknown)
			{
				tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
				tmp.imageView = vkCB->mImageView;
				//tmp.sampler = textureSampler;

				descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
				descriptorWrite.pImageInfo = &tmp;

				/*AutoRef<IVKShaderResourceView> refObj;
				refObj.StrongRef(vkCB);
				mUsedSrvs.push_back(refObj);*/
			}
			
			auto& bindings = backup[STS_VS];
			if (bindings.size() > 0)
			{
				vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);

				bool bFind = false;
				for (auto j = bindings.begin(); j != bindings.end(); j++)
				{
					if ((*j).binding == i.first)
					{
						bindings.erase(j);
						bFind = true;
						break;
					}
				}
				ASSERT(bFind);
			}
		}
		for (auto& i : dr->mShaderSrvBinder->PSResources)
		{
			auto vkCB = i.second.UnsafeConvertTo<IVKShaderResourceView>();
			if (vkCB == nullptr || mDescriptorSet[STS_PS] == nullptr)
				continue;

			auto idx = dr->GetReflector()->FindShaderBinderBySlot(EShaderBindType::SBT_Srv, EShaderType::EST_PixelShader, i.first);
			auto pBinder = (ShaderRViewBindInfo*)dr->GetReflector()->GetShaderBinder(EShaderBindType::SBT_Srv, idx);

			VkDescriptorImageInfo tmp{};
			VkDescriptorBufferInfo tmpStructureBuffer{};

			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = mDescriptorSet[STS_PS];
			descriptorWrite.dstBinding = i.first;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorCount = 1;

			if (pBinder->BufferType == EGpuBufferType::GBT_UavBuffer)
			{
				auto pGpuBuffer = vkCB->mBuffer.UnsafeConvertTo<IVKGpuBuffer>();
				tmpStructureBuffer.buffer = pGpuBuffer->mBuffer;
				tmpStructureBuffer.offset = 0;
				tmpStructureBuffer.range = pGpuBuffer->mBufferDesc.ByteWidth;

				descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
				descriptorWrite.pBufferInfo = &tmpStructureBuffer;

				/*AutoRef<IVKGpuBuffer> refObj;
				refObj.StrongRef(pGpuBuffer);
				mUsedBuffers.push_back(refObj);*/
			}
			else if (pBinder->BufferType == EGpuBufferType::GBT_TBufferBuffer)
			{
				/*auto pGpuBuffer = vkCB->mBuffer.UnsafeConvertTo<IVKGpuBuffer>();
				tmpStructureBuffer.buffer = pGpuBuffer->mBuffer;
				tmpStructureBuffer.offset = 0;
				tmpStructureBuffer.range = pGpuBuffer->mBufferDesc.ByteWidth;*/

				descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
				descriptorWrite.pTexelBufferView = &vkCB->mBufferView;
			}
			else if (pBinder->BufferType == EGpuBufferType::GBT_Unknown)
			{
				tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
				tmp.imageView = vkCB->mImageView;
				//tmp.sampler = textureSampler;

				descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
				descriptorWrite.pImageInfo = &tmp;

				/*AutoRef<IVKShaderResourceView> refObj;
				refObj.StrongRef(vkCB);
				mUsedSrvs.push_back(refObj);*/
			}
			
			auto& bindings = backup[STS_PS];
			if (bindings.size() > 0)
			{
				vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);

				bool bFind = false;
				for (auto j = bindings.begin(); j != bindings.end(); j++)
				{
					if ((*j).binding == i.first)
					{
						bindings.erase(j);
						bFind = true;
						break;
					}
				}
				ASSERT(bFind);
			}
		}
		dr->mShaderSrvBinder->IsDirty = false;
	}
	if (dr->mShaderSamplerBinder != nullptr)
	{
		for (auto& i : dr->mShaderSamplerBinder->VSResources)
		{
			auto vkCB = i.second.UnsafeConvertTo<IVKSamplerState>();
			if (vkCB == nullptr || mDescriptorSet[STS_VS] == nullptr)
				continue;

			VkDescriptorImageInfo tmp{};
			tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
			tmp.sampler = vkCB->TextureSampler;
			tmp.imageView = nullptr;

			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = mDescriptorSet[STS_VS];
			descriptorWrite.dstBinding = i.first;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
			descriptorWrite.descriptorCount = 1;
			descriptorWrite.pImageInfo = &tmp;

			auto& bindings = backup[STS_VS];
			if (bindings.size() > 0)
			{
				vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);

				bool bFind = false;
				for (auto j = bindings.begin(); j != bindings.end(); j++)
				{
					if ((*j).binding == i.first)
					{
						bindings.erase(j);
						bFind = true;
						break;
					}
				}
				ASSERT(bFind);
			}
		}
		for (auto& i : dr->mShaderSamplerBinder->PSResources)
		{
			auto vkCB = i.second.UnsafeConvertTo<IVKSamplerState>();
			if (vkCB == nullptr || mDescriptorSet[STS_PS] == nullptr)
				continue;

			VkDescriptorImageInfo tmp{};
			tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
			tmp.sampler = vkCB->TextureSampler;
			tmp.imageView = nullptr;

			VkWriteDescriptorSet descriptorWrite = {};
			descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
			descriptorWrite.dstSet = mDescriptorSet[STS_PS];
			descriptorWrite.dstBinding = i.first;
			descriptorWrite.dstArrayElement = 0;
			descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
			descriptorWrite.descriptorCount = 1;
			descriptorWrite.pImageInfo = &tmp;

			auto& bindings = backup[STS_PS];
			if (bindings.size() > 0)
			{
				vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);

				bool bFind = false;
				auto& bindings = backup[STS_PS];
				for (auto j = bindings.begin(); j != bindings.end(); j++)
				{
					if ((*j).binding == i.first)
					{
						bindings.erase(j);
						bFind = true;
						break;
					}
				}
				ASSERT(bFind);
			}
		}
		dr->mShaderSamplerBinder->IsDirty = false;
	}

	if (mDescriptorSet[STS_VS] != nullptr)
	{
		auto& bindings = backup[STS_VS];		
		for (auto& i : bindings)
		{
			VkDescriptorImageInfo tmp{};
			VkDescriptorBufferInfo tmpStructureBuffer{};
			switch (i.descriptorType)
			{
				case VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE:
				{
					VkWriteDescriptorSet descriptorWrite = {};
					descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
					descriptorWrite.dstSet = mDescriptorSet[STS_VS];
					descriptorWrite.dstBinding = i.binding;
					descriptorWrite.dstArrayElement = 0;
					descriptorWrite.descriptorCount = 1;

					tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
					tmp.imageView = rc->mNullRsv->mImageView;
					//tmp.sampler = textureSampler;

					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
					descriptorWrite.pImageInfo = &tmp;
					vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);
				}
				break;
				case VK_DESCRIPTOR_TYPE_SAMPLER:
				{
					tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
					tmp.sampler = rc->mNullSampler->TextureSampler;
					tmp.imageView = nullptr;

					VkWriteDescriptorSet descriptorWrite = {};
					descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
					descriptorWrite.dstSet = mDescriptorSet[STS_VS];
					descriptorWrite.dstBinding = i.binding;
					descriptorWrite.dstArrayElement = 0;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
					descriptorWrite.descriptorCount = 1;
					descriptorWrite.pImageInfo = &tmp;

					vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);
				}
				break;
				case VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER:
				{
					ASSERT(false);
				}
				break;
				case VK_DESCRIPTOR_TYPE_STORAGE_BUFFER:
				{
					ASSERT(false);
				}
				break;
				case VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER:
				{
					ASSERT(false);
				}
				break;
				default:
				{
					ASSERT(false);
					break;
				}
			}
		}
	}
	if (mDescriptorSet[STS_PS] != nullptr)
	{
		auto& bindings = backup[STS_PS];
		for (auto& i : bindings)
		{
			VkDescriptorImageInfo tmp{};
			VkDescriptorBufferInfo tmpStructureBuffer{};
			switch (i.descriptorType)
			{
				case VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE:
				{
					VkWriteDescriptorSet descriptorWrite = {};
					descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
					descriptorWrite.dstSet = mDescriptorSet[STS_PS];
					descriptorWrite.dstBinding = i.binding;
					descriptorWrite.dstArrayElement = 0;
					descriptorWrite.descriptorCount = 1;

					tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
					tmp.imageView = rc->mNullRsv->mImageView;
					//tmp.sampler = textureSampler;

					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
					descriptorWrite.pImageInfo = &tmp;
					vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);
				}
				break;
				case VK_DESCRIPTOR_TYPE_SAMPLER:
				{
					tmp.imageLayout = VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL;
					tmp.sampler = rc->mNullSampler->TextureSampler;
					tmp.imageView = nullptr;

					VkWriteDescriptorSet descriptorWrite = {};
					descriptorWrite.sType = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
					descriptorWrite.dstSet = mDescriptorSet[STS_PS];
					descriptorWrite.dstBinding = i.binding;
					descriptorWrite.dstArrayElement = 0;
					descriptorWrite.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
					descriptorWrite.descriptorCount = 1;
					descriptorWrite.pImageInfo = &tmp;

					vkUpdateDescriptorSets(rc->mLogicalDevice, 1, &descriptorWrite, 0, nullptr);
				}
				break;
				case VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER:
				{
					ASSERT(false);
				}
				break;
				case VK_DESCRIPTOR_TYPE_STORAGE_BUFFER:
				{
					ASSERT(false);
				}
				break;
				case VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER:
				{
					ASSERT(false);
				}
				break;
				default:
				{
					ASSERT(false);
				}
				break;
			}
		}
	}
}

void IVKDrawCall::VDescriptorSets::Cleanup(IVKRenderContext* rc, IVKShaderProgram* program)
{
	PostVkExecute([DescriptorSet = mDescriptorSet, program = program](IVKRenderContext* rc)
		{
			auto& allocInfo = program->mAllocInfo;
			for (int i = 0; i < STS_NUM; i++)
			{
				if (DescriptorSet[i] != nullptr)
				{
					vkFreeDescriptorSets(rc->mLogicalDevice, rc->mDescriptorPool->FreeDescriptor(allocInfo[i]), 1, &DescriptorSet[i]);
					DescriptorSet[i] = nullptr;
				}
			}
		});	
}

IVKDrawCall::IVKDrawCall()
{
	mCurVkSetState = nullptr;	
	mStateManager.StrongRef(new VDescriptorSetsManager());
}

IVKDrawCall::~IVKDrawCall()
{
	auto pProgram = (IVKShaderProgram*)this->GetPipeline()->GetGpuProgram();
	PostVkExecute([StateManager = mStateManager, pProgram = pProgram](IVKRenderContext* rc)
	{
		for (auto& i : StateManager->mCachedStates)
		{
			i.second->Cleanup(rc, pProgram);
			delete i.second;
		}
		StateManager->mCachedStates.clear();
	});
}

void IVKDrawCall::UpdateLayoutSet(IVKRenderContext* rc)
{
	auto str = GetShaderResourcesHash();
	auto hash = HashHelper::CalcHash64(str.c_str(), (int)str.length());
	if (mCurVkSetState == nullptr || mCurVkSetStateHash.Int64Value != hash.Int64Value ||
		(mShaderCBufferBinder != nullptr && mShaderCBufferBinder->IsDirty) ||
		(mShaderSrvBinder != nullptr && mShaderSrvBinder->IsDirty) ||
		(mShaderSamplerBinder != nullptr && mShaderSamplerBinder->IsDirty))
	{
		auto iter = mStateManager->mCachedStates.find(hash.Int64Value);
		if (iter == mStateManager->mCachedStates.end())
		{
			//todo: reset or remove some DescriptorSetStates when cache is too huge
			auto state = new VDescriptorSets();
			state->Init(rc, this);
			mStateManager->mCachedStates.insert(std::make_pair(hash.Int64Value, state));
			mCurVkSetState = state;
		}
		else
		{
			mCurVkSetState = iter->second;
		}
	}
}

void IVKDrawCall::BuildPass(ICommandList* cmd, vBOOL bImmCBuffer)
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	
	auto vkCmd = (IVKCommandList*)cmd;
	GetPipeline()->BindRenderPass(vkCmd->mFrameBuffer->mRenderPass);

	auto pProgram = (IVKShaderProgram*)this->mPipelineState->GetGpuProgram();
	auto layout = pProgram->mPipelineLayout; 
	
	if (mShaderSrvBinder != nullptr)
	{
		for (auto& i : mShaderSrvBinder->VSResources)
		{
			if (i.second == nullptr)
				continue;
			
			i.second->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
		}
		for (auto& i : mShaderSrvBinder->PSResources)
		{
			if (i.second == nullptr)
				continue;

			i.second->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
		}
	}

	UpdateLayoutSet(rc);
	mCurVkSetState->mCurrentFrame = rc->GetCurrentFrame();

	if (mCurVkSetState->mDescriptorSet[IVKDrawCall::ESetStage::STS_VS] != nullptr &&
		mCurVkSetState->mDescriptorSet[IVKDrawCall::ESetStage::STS_PS] != nullptr)
	{
		vkCmdBindDescriptorSets(vkCmd->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, layout, 0, (UINT)IVKDrawCall::ESetStage::STS_NUM, mCurVkSetState->mDescriptorSet, 0, nullptr);
	}
	else if (mCurVkSetState->mDescriptorSet[IVKDrawCall::ESetStage::STS_VS] == nullptr &&
		mCurVkSetState->mDescriptorSet[IVKDrawCall::ESetStage::STS_PS] == nullptr)
	{
		//int xxx = 0;
	}
	else if (mCurVkSetState->mDescriptorSet[IVKDrawCall::ESetStage::STS_PS] == nullptr)
	{
		vkCmdBindDescriptorSets(vkCmd->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, layout, 0, (UINT)1, mCurVkSetState->mDescriptorSet, 0, nullptr);
	}
	else if (mCurVkSetState->mDescriptorSet[IVKDrawCall::ESetStage::STS_VS] == nullptr)
	{
		vkCmdBindDescriptorSets(vkCmd->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, layout, 1, (UINT)1, &mCurVkSetState->mDescriptorSet[1], 0, nullptr);
	}

	//IDrawCall::BuildPassDefault(cmd, bImmCBuffer);

	const DrawPrimitiveDesc* dpDesc = nullptr;
	{
		AUTO_SAMP("Native.IPass.BuildPass.Geometry");
		vBOOL applyOK = ApplyGeomtry(cmd, bImmCBuffer);
		if (applyOK == FALSE)
			return;
		dpDesc = MeshPrimitives->GetAtom(AtomIndex, LodLevel);
		if (dpDesc == nullptr)
			return;
	}

	auto vs = pProgram->GetVertexShader();
	if (vs && mShaderCBufferBinder != nullptr)
	{
		for (const auto& i : mShaderCBufferBinder->VSResources)
		{
			if (i.second == nullptr)
				continue;
			i.second->UpdateDrawPass(cmd, bImmCBuffer);
		}
	}
	auto ps = pProgram->GetPixelShader();
	if (ps && mShaderCBufferBinder != nullptr)
	{
		for (const auto& i : mShaderCBufferBinder->PSResources)
		{
			if (i.second == nullptr)
				continue;
			i.second->UpdateDrawPass(cmd, bImmCBuffer);
		}
	}

	SetPipeline(cmd, mPipelineState, dpDesc->PrimitiveType);
	mPipelineState->mIsDirty = false;

	{
		if (AttachVBs != nullptr)
		{
			NumInstances = AttachVBs->mNumInstances;
		}
		else
		{
			NumInstances = 1;
		}

		if (cmd->OnPassBuilt != nullptr)
		{
			cmd->OnPassBuilt(cmd, this);
		}

		if (cmd->mPipelineStat != nullptr)
		{
			cmd->mPipelineStat->mDrawCall++;
			cmd->mPipelineStat->mDrawTriangle += NumInstances * dpDesc->NumPrimitives;
		}

		if (IndirectDrawArgsBuffer)
		{
			this->DrawIndexedInstancedIndirect(cmd, dpDesc->PrimitiveType, IndirectDrawArgsBuffer, IndirectDrawOffsetForArgs);
		}
		else
		{
			if (dpDesc->IsIndexDraw())
			{
				this->DrawIndexedPrimitive(cmd, dpDesc->PrimitiveType, dpDesc->BaseVertexIndex, dpDesc->StartIndex, dpDesc->NumPrimitives, NumInstances);
			}
			else
			{
				this->DrawPrimitive(cmd, dpDesc->PrimitiveType, dpDesc->BaseVertexIndex, dpDesc->NumPrimitives, NumInstances);
			}
		}
	}
}

vBOOL IVKDrawCall::ApplyGeomtry(ICommandList* cmd, vBOOL bImmCBuffer)
{
	return IDrawCall::ApplyGeomtry(cmd, bImmCBuffer);
}

void IVKDrawCall::SetViewport(ICommandList* cmd, IViewPort* vp)
{
	cmd->SetViewport(vp);
}

void IVKDrawCall::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
{
	cmd->SetScissorRect(sr);
}

void IVKDrawCall::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline, EPrimitiveType dpType)
{
	((IVKRenderPipeline*)pipeline)->OnSetPipeline((IVKCommandList*)cmd, dpType);
}

void IVKDrawCall::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{
	cmd->SetVertexBuffer(StreamIndex, VertexBuffer, Offset, Stride);
}

void IVKDrawCall::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
{
	cmd->SetIndexBuffer(IndexBuffer);
}

void IVKDrawCall::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	cmd->VSSetConstantBuffer(Index, CBuffer);
}

void IVKDrawCall::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	cmd->PSSetConstantBuffer(Index, CBuffer);
}

void IVKDrawCall::VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	cmd->VSSetShaderResource(Index, Texture);
}

void IVKDrawCall::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	cmd->PSSetShaderResource(Index, Texture);
}

void IVKDrawCall::VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{
	cmd->VSSetSampler(Index, Sampler);
}

void IVKDrawCall::PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{
	cmd->PSSetSampler(Index, Sampler);
}

void IVKDrawCall::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	cmd->DrawPrimitive(PrimitiveType, BaseVertexIndex, NumPrimitives, NumInstances);
}

void IVKDrawCall::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	cmd->DrawIndexedPrimitive(PrimitiveType, BaseVertexIndex, StartIndex, NumPrimitives, NumInstances);
}

void IVKDrawCall::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	cmd->DrawIndexedInstancedIndirect(PrimitiveType, pBufferForArgs, AlignedByteOffsetForArgs);
}

bool IVKDrawCall::Init(IVKRenderContext* rc, const IDrawCallDesc* desc)
{
	mRenderContext.FromObject(rc);
	return true;
}

void IVKComputeDrawcall::BuildPass(ICommandList* cmd)
{

}

bool IVKComputeDrawcall::Init(IVKRenderContext* rc)
{
	return true;
}

NS_END