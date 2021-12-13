#include "IVKShaderProgram.h"
#include "IVKVertexShader.h"
#include "IVKPixelShader.h"
#include "IVKInputLayout.h"
#include "IVKRenderContext.h"
#include "IVKConstantBuffer.h"
#include "../IShader.h"
#include "../ShaderReflector.h"

#define new VNEW

NS_BEGIN

IVKShaderProgram::IVKShaderProgram()
{
	mPipelineLayout = nullptr;
}

IVKShaderProgram::~IVKShaderProgram()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mPipelineLayout != nullptr)
	{
		vkDestroyPipelineLayout(rc->mLogicalDevice, mPipelineLayout, rc->GetVkAllocCallBacks());
		mPipelineLayout = nullptr;
	}
}

void IVKShaderProgram::PushBinding(UINT descriptorSet, const VkDescriptorSetLayoutBinding& binding)
{
	ASSERT(descriptorSet!=-1);
	auto iter = mLayoutBindings.find(descriptorSet);
	if (iter == mLayoutBindings.end())
	{
		std::vector<VkDescriptorSetLayoutBinding> bindings;
		bindings.push_back(binding);
		mLayoutBindings.insert(std::make_pair(descriptorSet, bindings));
		return;
	}
	mLayoutBindings[descriptorSet].push_back(binding);
}

vBOOL IVKShaderProgram::LinkShaders(IRenderContext* rc)
{
	mRenderContext.FromObject(rc);

	mReflector->Reset();
	mReflector->MergeShaderStage(EST_VertexShader, mVertexShader->GetReflector());
	mReflector->MergeShaderStage(EST_PixelShader, mPixelShader->GetReflector());

	int MaxLayoutSet = -1;
	{
		for (auto& i : mVertexShader->GetReflector()->mCBDescArray)
		{
			VkDescriptorSetLayoutBinding uboLayoutBinding{};
			uboLayoutBinding.binding = i.VSBindPoint;
			uboLayoutBinding.descriptorCount = 1;
			uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
			uboLayoutBinding.pImmutableSamplers = nullptr;
			uboLayoutBinding.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;

			PushBinding(i.DescriptorSet, uboLayoutBinding);
			if (MaxLayoutSet < (int)i.DescriptorSet)
			{
				MaxLayoutSet = i.DescriptorSet;
			}
		}
		for (auto& i : mVertexShader->GetReflector()->mUavBindArray)
		{
			VkDescriptorSetLayoutBinding uboLayoutBinding{};
			uboLayoutBinding.binding = i.VSBindPoint;
			uboLayoutBinding.descriptorCount = 1;
			uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
			uboLayoutBinding.pImmutableSamplers = nullptr;
			uboLayoutBinding.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;

			PushBinding(i.DescriptorSet, uboLayoutBinding);
			if (MaxLayoutSet < (int)i.DescriptorSet)
			{
				MaxLayoutSet = i.DescriptorSet;
			}
		}
		for (auto& i : mVertexShader->GetReflector()->mSrvBindArray)
		{
			VkDescriptorSetLayoutBinding uboLayoutBinding{};
			uboLayoutBinding.binding = i.VSBindPoint;
			uboLayoutBinding.descriptorCount = 1;
			
			if (i.BufferType == EGpuBufferType::GBT_UavBuffer)
			{
				uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
			}
			else if (i.BufferType == EGpuBufferType::GBT_TBufferBuffer)
			{
				uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
			}
			else if (i.BufferType == EGpuBufferType::GBT_Unknown)
			{
				uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
			}
			else
			{
				ASSERT(false);
			}
			uboLayoutBinding.pImmutableSamplers = nullptr;
			uboLayoutBinding.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;

			PushBinding(i.DescriptorSet, uboLayoutBinding);
			if (MaxLayoutSet < (int)i.DescriptorSet)
			{
				MaxLayoutSet = i.DescriptorSet;
			}
		}
		for (auto& i : mVertexShader->GetReflector()->mSamplerBindArray)
		{
			VkDescriptorSetLayoutBinding uboLayoutBinding{};
			uboLayoutBinding.binding = i.VSBindPoint;
			uboLayoutBinding.descriptorCount = 1;
			uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
			uboLayoutBinding.pImmutableSamplers = nullptr;
			uboLayoutBinding.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;

			PushBinding(i.DescriptorSet, uboLayoutBinding);
			if (MaxLayoutSet < (int)i.DescriptorSet)
			{
				MaxLayoutSet = i.DescriptorSet;
			}
		}
	}
	{
		for (auto& i : mPixelShader->GetReflector()->mCBDescArray)
		{
			VkDescriptorSetLayoutBinding uboLayoutBinding{};
			uboLayoutBinding.binding = i.PSBindPoint;
			uboLayoutBinding.descriptorCount = 1;
			uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
			uboLayoutBinding.pImmutableSamplers = nullptr;
			uboLayoutBinding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;

			PushBinding(i.DescriptorSet, uboLayoutBinding);
			if (MaxLayoutSet < (int)i.DescriptorSet)
			{
				MaxLayoutSet = i.DescriptorSet;
			}
		}
		for (auto& i : mPixelShader->GetReflector()->mUavBindArray)
		{
			VkDescriptorSetLayoutBinding uboLayoutBinding{};
			uboLayoutBinding.binding = i.PSBindPoint;
			uboLayoutBinding.descriptorCount = 1;
			uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
			uboLayoutBinding.pImmutableSamplers = nullptr;
			uboLayoutBinding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;

			PushBinding(i.DescriptorSet, uboLayoutBinding);
			if (MaxLayoutSet < (int)i.DescriptorSet)
			{
				MaxLayoutSet = i.DescriptorSet;
			}
		}
		for (auto& i : mPixelShader->GetReflector()->mSrvBindArray)
		{
			VkDescriptorSetLayoutBinding uboLayoutBinding{};
			uboLayoutBinding.binding = i.PSBindPoint;
			uboLayoutBinding.descriptorCount = 1;
			if (i.BufferType == EGpuBufferType::GBT_UavBuffer)
			{
				uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
			}
			else if (i.BufferType == EGpuBufferType::GBT_TBufferBuffer)
			{
				uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER;
			}
			else if (i.BufferType == EGpuBufferType::GBT_Unknown)
			{
				uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
			}
			else
			{
				ASSERT(false);
			}
			uboLayoutBinding.pImmutableSamplers = nullptr;
			uboLayoutBinding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;

			PushBinding(i.DescriptorSet, uboLayoutBinding);
			if (MaxLayoutSet < (int)i.DescriptorSet)
			{
				MaxLayoutSet = i.DescriptorSet;
			}
		}
		for (auto& i : mPixelShader->GetReflector()->mSamplerBindArray)
		{
			VkDescriptorSetLayoutBinding uboLayoutBinding{};
			uboLayoutBinding.binding = i.PSBindPoint;
			uboLayoutBinding.descriptorCount = 1;
			uboLayoutBinding.descriptorType = VK_DESCRIPTOR_TYPE_SAMPLER;
			uboLayoutBinding.pImmutableSamplers = nullptr;
			uboLayoutBinding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;

			PushBinding(i.DescriptorSet, uboLayoutBinding);
			if (MaxLayoutSet < (int)i.DescriptorSet)
			{
				MaxLayoutSet = i.DescriptorSet;
			}
		}
	}

	ASSERT(MaxLayoutSet>=0);
	mDescriptorSetLayout.resize(MaxLayoutSet + 1);
	mAllocInfo.resize(MaxLayoutSet + 1);
	for (const auto& i : mLayoutBindings)
	{
		VkDescriptorSetLayoutCreateInfo layoutInfo{};
		layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
		layoutInfo.bindingCount = (UINT)i.second.size();
		layoutInfo.pBindings = &i.second[0];

		VkDescriptorSetLayout setlayout;
		if (vkCreateDescriptorSetLayout(((IVKRenderContext*)rc)->mLogicalDevice, &layoutInfo,
			((IVKRenderContext*)rc)->GetVkAllocCallBacks(), &setlayout) != VK_SUCCESS)
		{
			return FALSE;
		}
		mDescriptorSetLayout[i.first] = setlayout;
		for (auto& j : i.second)
		{
			switch (j.descriptorType)
			{
				case VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER:
				{
					mAllocInfo[i.first].UniformNum++;
				}
				break;
				case VK_DESCRIPTOR_TYPE_STORAGE_BUFFER:
				{
					mAllocInfo[i.first].StorageNum++;
				}
				break;
				case VK_DESCRIPTOR_TYPE_SAMPLER:
				{
					mAllocInfo[i.first].SamplerNum++;
				}
				break;
				case VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE:
				{
					mAllocInfo[i.first].TextureNum++;
				}
				break;
				case VK_DESCRIPTOR_TYPE_STORAGE_TEXEL_BUFFER:
				{
					mAllocInfo[i.first].StorageTexelNum++;
				}
				break;
				default:
					break;
			}
		}		
	}
	for (size_t i = 0; i < mDescriptorSetLayout.size(); i++)
	{
		if (mDescriptorSetLayout[i] == nullptr)
		{
			VkDescriptorSetLayoutCreateInfo layoutInfo{};
			layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
			layoutInfo.bindingCount = 0;
			layoutInfo.pBindings = nullptr;

			VkDescriptorSetLayout setlayout;
			if (vkCreateDescriptorSetLayout(((IVKRenderContext*)rc)->mLogicalDevice, &layoutInfo,
				((IVKRenderContext*)rc)->GetVkAllocCallBacks(), &setlayout) != VK_SUCCESS)
			{
				return FALSE;
			}
			mDescriptorSetLayout[i] = setlayout;
		}
	}

	std::vector<VkDescriptorSetLayout>	validSetLayout;
	for (size_t i = 0; i < mDescriptorSetLayout.size(); i++)
	{
		if (mDescriptorSetLayout[i] != nullptr)
		{
			validSetLayout.push_back(mDescriptorSetLayout[i]);
		}
	}
	memset(&mVSCreateInfo, 0, sizeof(mVSCreateInfo));
	mVSCreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
	mVSCreateInfo.stage = VK_SHADER_STAGE_VERTEX_BIT;
	mVSCreateInfo.module = mVertexShader.UnsafeConvertTo<IVKVertexShader>()->mShader;
	mVSCreateInfo.pName = "VS_Main";

	memset(&mPSCreateInfo, 0, sizeof(mPSCreateInfo));
	mPSCreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
	mPSCreateInfo.stage = VK_SHADER_STAGE_FRAGMENT_BIT;
	mPSCreateInfo.module = mPixelShader.UnsafeConvertTo<IVKPixelShader>()->mShader;
	mPSCreateInfo.pName = "PS_Main";
	
	VkPipelineLayoutCreateInfo pipelineLayoutInfo{};
	pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
	pipelineLayoutInfo.setLayoutCount = (UINT)validSetLayout.size(); //BDS_Number;
	pipelineLayoutInfo.pSetLayouts = &validSetLayout[0];//mDescriptorSetLayout;
	if (vkCreatePipelineLayout(((IVKRenderContext*)rc)->mLogicalDevice, &pipelineLayoutInfo, ((IVKRenderContext*)rc)->GetVkAllocCallBacks(), &mPipelineLayout) != VK_SUCCESS)
	{
		return FALSE;
	}

	return TRUE;
}

void IVKShaderProgram::ApplyShaders(ICommandList* cmd)
{

}

bool IVKShaderProgram::Init(IVKRenderContext* rc, const IShaderProgramDesc* desc)
{
	BindInputLayout(desc->InputLayout);
	BindVertexShader(desc->VertexShader);
	BindPixelShader(desc->PixelShader);
	return true;
}

NS_END