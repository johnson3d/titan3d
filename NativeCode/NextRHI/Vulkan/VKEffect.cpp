#include "VKEffect.h"
#include "VKGpuDevice.h"
#include "VKCommandList.h"
#include "VKShader.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	static void FillRangeVK(std::vector<VkDescriptorSetLayoutBinding>* pOutRanges, const FShaderBinder* pBinder, VkDescriptorType type, VkShaderStageFlagBits shaderStage)
	{
		if (pBinder == nullptr)
			return;
		VkDescriptorSetLayoutBinding binding{};
		binding.binding = pBinder->Slot;
		binding.descriptorCount = 1;
		binding.descriptorType = type;
		binding.pImmutableSamplers = nullptr;
		binding.stageFlags = shaderStage;
		pOutRanges->push_back(binding);
	}

	VkDescriptorType VKDescriptorSetLayoutBuilder::GetDescriptorType(const FShaderBinder* binder)
	{
		VkDescriptorType type = VK_DESCRIPTOR_TYPE_MAX_ENUM;
		switch (binder->Type)
		{
		case EShaderBindType::SBT_CBV:
			type = VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
			break;
		case EShaderBindType::SBT_Sampler:
			type = VK_DESCRIPTOR_TYPE_SAMPLER;
			break;
		case EShaderBindType::SBT_SRV:
			if (binder->IsStructuredBuffer)
				type = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
			else
				type = VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
			break;
		case EShaderBindType::SBT_UAV:
			if (binder->IsStructuredBuffer)
				type = VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
			else
				type = VK_DESCRIPTOR_TYPE_STORAGE_IMAGE;
			break;
		default:
			ASSERT(false);
			break;
		}
		return type;
	}

	VkDescriptorSetLayout VKDescriptorSetLayoutBuilder::Build(VKGpuDevice* device, VKGraphicsEffect* effect, std::vector<VkDescriptorSetLayoutBinding>& bindings)
	{
		auto& binders = effect->mBinders;
		for (auto& i : binders)
		{
			FEffectBinder* binder = i.second;
			auto pBinder = binder->GetShaderBinder();
			
			auto type = GetDescriptorType(pBinder);
			if (binder->VSBinder)
				FillRangeVK(&bindings, binder->VSBinder, type, VkShaderStageFlagBits::VK_SHADER_STAGE_VERTEX_BIT);
			if (binder->VSBinder)
				FillRangeVK(&bindings, binder->PSBinder, type, VkShaderStageFlagBits::VK_SHADER_STAGE_FRAGMENT_BIT);
			if (binder->ASBinder)
				FillRangeVK(&bindings, binder->VSBinder, type, VkShaderStageFlagBits::VK_SHADER_STAGE_TASK_BIT_NV);
			if (binder->MSBinder)
				FillRangeVK(&bindings, binder->VSBinder, type, VkShaderStageFlagBits::VK_SHADER_STAGE_MESH_BIT_NV);
		}

		VkDescriptorSetLayoutCreateInfo layoutInfo{};
		layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
		//layoutInfo.flags = VK_DESCRIPTOR_SET_LAYOUT_CREATE_PER_STAGE_BIT_NV;
		layoutInfo.bindingCount = (UINT)bindings.size();
		if (layoutInfo.bindingCount != 0)
		{
			layoutInfo.pBindings = &bindings[0];
		}
		VkDescriptorSetLayout result = nullptr;
		if (vkCreateDescriptorSetLayout(device->mDevice, &layoutInfo, device->GetVkAllocCallBacks(), &result) != VK_SUCCESS)
		{
			ASSERT(false);
			return nullptr;
		}
		return result;
	}

	VkDescriptorSetLayout VKDescriptorSetLayoutBuilder::Build(VKGpuDevice* device, VKComputeEffect* effect, std::vector<VkDescriptorSetLayoutBinding>& bindings)
	{
		for (auto& i : effect->mComputeShader->Reflector->CBuffers)
		{
			auto type = GetDescriptorType(i);
			FillRangeVK(&bindings, i, type, VkShaderStageFlagBits::VK_SHADER_STAGE_COMPUTE_BIT);
		}
		for (auto& i : effect->mComputeShader->Reflector->Srvs)
		{
			auto type = GetDescriptorType(i);
			FillRangeVK(&bindings, i, type, VkShaderStageFlagBits::VK_SHADER_STAGE_COMPUTE_BIT);
		}
		for (auto& i : effect->mComputeShader->Reflector->Uavs)
		{
			auto type = GetDescriptorType(i);
			FillRangeVK(&bindings, i, type, VkShaderStageFlagBits::VK_SHADER_STAGE_COMPUTE_BIT);
		}
		for (auto& i : effect->mComputeShader->Reflector->Samplers)
		{
			auto type = GetDescriptorType(i);
			FillRangeVK(&bindings, i, type, VkShaderStageFlagBits::VK_SHADER_STAGE_COMPUTE_BIT);
		}
		VkDescriptorSetLayoutCreateInfo layoutInfo{};
		layoutInfo.sType = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
		//layoutInfo.flags = VK_DESCRIPTOR_SET_LAYOUT_CREATE_PER_STAGE_BIT_NV;
		layoutInfo.bindingCount = (UINT)bindings.size();
		if (layoutInfo.bindingCount != 0)
		{
			layoutInfo.pBindings = &bindings[0];
		}
		VkDescriptorSetLayout result = nullptr;
		if (vkCreateDescriptorSetLayout(device->mDevice, &layoutInfo, device->GetVkAllocCallBacks(), &result) != VK_SUCCESS)
		{
			ASSERT(false);
			return nullptr;
		}
		return result;
	}
	
	VKGraphicsEffect::VKGraphicsEffect()
	{
		
	}
	VKGraphicsEffect::~VKGraphicsEffect()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		if (mLayout)
		{
			vkDestroyDescriptorSetLayout(device->mDevice, mLayout, device->GetVkAllocCallBacks());
			mLayout = nullptr;
		}
		if (mPipelineLayout != nullptr)
		{
			vkDestroyPipelineLayout(device->mDevice, mPipelineLayout, device->GetVkAllocCallBacks());
			mPipelineLayout = nullptr;
		}
	}
	void VKGraphicsEffect::BuildState(IGpuDevice* device1)
	{
		auto device = (VKGpuDevice*)device1;
		mDeviceRef.FromObject(device);
		//vkAllocateDescriptorSets
		//vkUpdateDescriptorSets
		
		mBindings.clear();
		mLayout = VKDescriptorSetLayoutBuilder::Build(device, this, mBindings);

		VkPipelineShaderStageCreateInfo mVSCreateInfo{};
		memset(&mVSCreateInfo, 0, sizeof(mVSCreateInfo));
		mVSCreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		mVSCreateInfo.stage = VK_SHADER_STAGE_VERTEX_BIT;
		auto pShader = mVertexShader.UnsafeConvertTo<VKShader>();
		mVSCreateInfo.module = pShader->mShader;
		mVSCreateInfo.pName = pShader->Desc->FunctionName.c_str();// "VS_Main";

		VkPipelineShaderStageCreateInfo mPSCreateInfo{};
		memset(&mPSCreateInfo, 0, sizeof(mPSCreateInfo));
		mPSCreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		mPSCreateInfo.stage = VK_SHADER_STAGE_FRAGMENT_BIT;
		pShader = mPixelShader.UnsafeConvertTo<VKShader>();
		mPSCreateInfo.module = pShader->mShader;
		mPSCreateInfo.pName = pShader->Desc->FunctionName.c_str();// "PS_Main";

		VkPipelineLayoutCreateInfo pipelineLayoutInfo{};
		pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
		pipelineLayoutInfo.setLayoutCount = (UINT)1; //BDS_Number;
		pipelineLayoutInfo.pSetLayouts = &mLayout;//mDescriptorSetLayout;
		if (vkCreatePipelineLayout(device->mDevice, &pipelineLayoutInfo, device->GetVkAllocCallBacks(), &mPipelineLayout) != VK_SUCCESS)
		{
			ASSERT(false);
		}
	}

	void VKGraphicsEffect::Commit(ICommandList* cmdlist, IGraphicDraw* drawcall)
	{
		//vkCmdBindDescriptorSets(vkCmd->mCommandBuffer, VK_PIPELINE_BIND_POINT_GRAPHICS, layout, 0, (UINT)IVKDrawCall::ESetStage::STS_NUM, mCurVkSetState->mDescriptorSet, 0, nullptr);

		/*auto dx12Cmd = (VKCommandList*)cmdlist;
		ASSERT(dx12Cmd->mCurrentTableRecycle != nullptr);
		auto device = dx12Cmd->GetDX12Device();

		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;
		
		dx12Cmd->mContext->SetGraphicsRootSignature(mSignature);
		if (mSrvTableSize > 0)
		{
			dx12Cmd->mCurrentSrvTable = device->mSrvTableHeapManager->Alloc(device->mDevice, mSrvTableSize);
			dx12Cmd->mCurrentTableRecycle->mAllocTableHeaps.push_back(dx12Cmd->mCurrentSrvTable);
			descriptorHeaps[NumOfHeaps++] = dx12Cmd->mCurrentSrvTable->mHeap;
		}
		else
		{
			dx12Cmd->mCurrentSrvTable = nullptr;
		}

		if (mSamplerTableSize > 0)
		{
			dx12Cmd->mCurrentSamplerTable = device->mSamplerTableHeapManager->Alloc(device->mDevice, mSamplerTableSize);
			dx12Cmd->mCurrentTableRecycle->mAllocTableHeaps.push_back(dx12Cmd->mCurrentSamplerTable);
			descriptorHeaps[NumOfHeaps++] = dx12Cmd->mCurrentSamplerTable->mHeap;
		}
		else
		{
			dx12Cmd->mCurrentSamplerTable = nullptr;
		}

		dx12Cmd->mContext->SetGraphicsRootSignature(mSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);
		
		if (mSrvTableSize > 0)
		{
			dx12Cmd->mContext->SetGraphicsRootDescriptorTable(mSrvTableSizeIndex, dx12Cmd->mCurrentSrvTable->mHeap->GetGPUDescriptorHandleForHeapStart());
		}
		if (mSamplerTableSize > 0)
		{
			dx12Cmd->mContext->SetGraphicsRootDescriptorTable(mSamplerTableSizeIndex, dx12Cmd->mCurrentSamplerTable->mHeap->GetGPUDescriptorHandleForHeapStart());
		}*/
	}

	VKComputeEffect::VKComputeEffect()
	{

	}

	VKComputeEffect::~VKComputeEffect()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		if (mLayout)
		{
			vkDestroyDescriptorSetLayout(device->mDevice, mLayout, device->GetVkAllocCallBacks());
			mLayout = nullptr;
		}

		if (mPipelineLayout != nullptr)
		{
			vkDestroyPipelineLayout(device->mDevice, mPipelineLayout, device->GetVkAllocCallBacks());
			mPipelineLayout = nullptr;
		}
	}

	void VKComputeEffect::BuildState(IGpuDevice* device1)
	{
		auto device = (VKGpuDevice*)device1;
		mDeviceRef.FromObject(device);
		
		mBindings.clear();
		mLayout = VKDescriptorSetLayoutBuilder::Build(device, this, mBindings);
		VkPipelineLayoutCreateInfo pipelineLayoutInfo{};
		pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
		pipelineLayoutInfo.setLayoutCount = 1; //BDS_Number;
		pipelineLayoutInfo.pSetLayouts = &mLayout;//mDescriptorSetLayout;
		if (vkCreatePipelineLayout(device->mDevice, &pipelineLayoutInfo, device->GetVkAllocCallBacks(), &mPipelineLayout) != VK_SUCCESS)
		{
			ASSERT(false);
		}

		VkPipelineShaderStageCreateInfo shaderStages{};
		shaderStages.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		shaderStages.stage = VK_SHADER_STAGE_COMPUTE_BIT;
		shaderStages.module = mComputeShader.UnsafeConvertTo<VKShader>()->mShader;
		shaderStages.pName = mComputeShader->Desc->FunctionName.c_str();

		VkComputePipelineCreateInfo pipelineInfo{};
		pipelineInfo.sType = VK_STRUCTURE_TYPE_COMPUTE_PIPELINE_CREATE_INFO;
		pipelineInfo.stage = shaderStages;
		pipelineInfo.layout = mPipelineLayout;
		pipelineInfo.basePipelineHandle = VK_NULL_HANDLE;
		pipelineInfo.basePipelineIndex = 0;

		if (vkCreateComputePipelines(device->mDevice, VK_NULL_HANDLE, 1, &pipelineInfo,
			device->GetVkAllocCallBacks(), &mComputePipeline) != VK_SUCCESS)
		{
			ASSERT(false);
		}
	}
	
	void VKComputeEffect::Commit(ICommandList* cmdlist)
	{
		/*auto dx12Cmd = (VKCommandList*)cmdlist;
		ASSERT(dx12Cmd->mCurrentTableRecycle != nullptr);
		auto device = dx12Cmd->GetDX12Device();

		ID3D12DescriptorHeap* descriptorHeaps[4] = {};
		int NumOfHeaps = 0;

		if (mSrvTableSize > 0)
		{
			dx12Cmd->mCurrentComputeSrvTable = device->mSrvTableHeapManager->Alloc(device->mDevice, mSrvTableSize);
			dx12Cmd->mCurrentTableRecycle->mAllocTableHeaps.push_back(dx12Cmd->mCurrentComputeSrvTable);
			descriptorHeaps[NumOfHeaps++] = dx12Cmd->mCurrentComputeSrvTable->mHeap;
		}
		else
		{
			dx12Cmd->mCurrentComputeSrvTable = nullptr;
		}

		if (mSamplerTableSize > 0)
		{
			dx12Cmd->mCurrentComputeSamplerTable = device->mSamplerTableHeapManager->Alloc(device->mDevice, mSamplerTableSize);
			dx12Cmd->mCurrentTableRecycle->mAllocTableHeaps.push_back(dx12Cmd->mCurrentComputeSamplerTable);
			descriptorHeaps[NumOfHeaps++] = dx12Cmd->mCurrentComputeSamplerTable->mHeap;
		}
		else
		{
			dx12Cmd->mCurrentComputeSamplerTable = nullptr;
		}

		dx12Cmd->mContext->SetPipelineState(mPipelineState);
		dx12Cmd->mContext->SetComputeRootSignature(mSignature);
		dx12Cmd->mContext->SetDescriptorHeaps(NumOfHeaps, descriptorHeaps);

		if (mSrvTableSize > 0)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(mSrvTableSizeIndex, dx12Cmd->mCurrentComputeSrvTable->mHeap->GetGPUDescriptorHandleForHeapStart());
		}
		if (mSamplerTableSize > 0)
		{
			dx12Cmd->mContext->SetComputeRootDescriptorTable(mSamplerTableSizeIndex, dx12Cmd->mCurrentComputeSamplerTable->mHeap->GetGPUDescriptorHandleForHeapStart());
		}*/
	}
}

NS_END