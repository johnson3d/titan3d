#include "VKEffect.h"
#include "VKGpuDevice.h"
#include "VKCommandList.h"
#include "VKShader.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	template<>
	struct AuxGpuResourceDestroyer<VkPipelineLayout>
	{
		static void Destroy(VkPipelineLayout obj, IGpuDevice* device1)
		{
			auto device = (VKGpuDevice*)device1;
			vkDestroyPipelineLayout(device->mDevice, obj, device->GetVkAllocCallBacks());
		}
	};
	/*static void FillRangeVK(std::vector<VkDescriptorSetLayoutBinding>* pOutRanges, FShaderBinder* pVSBinder, FShaderBinder* pPSBinder, VkDescriptorType type)
	{
		if (pVSBinder != nullptr && pPSBinder == nullptr)
		{
			VkDescriptorSetLayoutBinding binding{};
			binding.binding = pVSBinder->Slot;
			binding.descriptorCount = 1;
			binding.descriptorType = type;
			binding.pImmutableSamplers = nullptr;
			binding.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;
			pOutRanges->push_back(binding);
		}
		else if (pVSBinder == nullptr && pPSBinder != nullptr)
		{
			VkDescriptorSetLayoutBinding binding{};
			binding.binding = pPSBinder->Slot;
			binding.descriptorCount = 1;
			binding.descriptorType = type;
			binding.pImmutableSamplers = nullptr;
			binding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;
			pOutRanges->push_back(binding);
		}
		else if (pVSBinder->Slot == pPSBinder->Slot && pVSBinder->Space == pPSBinder->Space)
		{
			VkDescriptorSetLayoutBinding binding{};
			binding.binding = pPSBinder->Slot;
			binding.descriptorCount = 1;
			binding.descriptorType = type;
			binding.pImmutableSamplers = nullptr;
			binding.stageFlags = VK_SHADER_STAGE_VERTEX_BIT | VK_SHADER_STAGE_FRAGMENT_BIT;
			pOutRanges->push_back(binding);
		}
		else
		{
			{
				VkDescriptorSetLayoutBinding binding{};
				binding.binding = pVSBinder->Slot;
				binding.descriptorCount = 1;
				binding.descriptorType = type;
				binding.pImmutableSamplers = nullptr;
				binding.stageFlags = VK_SHADER_STAGE_VERTEX_BIT;
				pOutRanges->push_back(binding);
			}

			{
				VkDescriptorSetLayoutBinding binding{};
				binding.binding = pPSBinder->Slot;
				binding.descriptorCount = 1;
				binding.descriptorType = type;
				binding.pImmutableSamplers = nullptr;
				binding.stageFlags = VK_SHADER_STAGE_FRAGMENT_BIT;
				pOutRanges->push_back(binding);
			}
		}
	}*/
	
	VKShaderEffect::VKShaderEffect()
	{
		
	}
	VKShaderEffect::~VKShaderEffect()
	{
		auto device = mDeviceRef.GetPtr();
		if (device == nullptr)
			return;

		if (mPipelineLayout != nullptr)
		{
			device->DelayDestroy(mPipelineLayout);
			mPipelineLayout = nullptr;
		}
	}
	void VKShaderEffect::BuildState(IGpuDevice* device1)
	{
		auto device = (VKGpuDevice*)device1;
		mDeviceRef.FromObject(device);
		//vkAllocateDescriptorSets
		//vkUpdateDescriptorSets
		
		std::vector<VkDescriptorSetLayout> validSetLayout;

		auto layout = mVertexShader.UnsafeConvertTo<VKShader>()->mLayout;
		if (layout!=nullptr)
		{
			validSetLayout.push_back(layout);
		}
		layout = mPixelShader.UnsafeConvertTo<VKShader>()->mLayout;
		if (layout != nullptr)
		{
			validSetLayout.push_back(layout);
		}

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
		pipelineLayoutInfo.setLayoutCount = (UINT)validSetLayout.size(); //BDS_Number;
		pipelineLayoutInfo.pSetLayouts = &validSetLayout[0];//mDescriptorSetLayout;
		if (vkCreatePipelineLayout(device->mDevice, &pipelineLayoutInfo, device->GetVkAllocCallBacks(), &mPipelineLayout) != VK_SUCCESS)
		{
			ASSERT(false);
		}
	}

	void VKShaderEffect::Commit(ICommandList* cmdlist, IGraphicDraw* drawcall)
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

		if (mPipelineLayout != nullptr)
		{
			device->DelayDestroy(mPipelineLayout);
			mPipelineLayout = nullptr;
		}
	}

	void VKComputeEffect::BuildState(IGpuDevice* device1)
	{
		auto device = (VKGpuDevice*)device1;
		mDeviceRef.FromObject(device);
		
		auto pShader = mComputeShader.UnsafeConvertTo<VKShader>();

		VkPipelineLayoutCreateInfo pipelineLayoutInfo{};
		pipelineLayoutInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
		pipelineLayoutInfo.setLayoutCount = 1; //BDS_Number;
		pipelineLayoutInfo.pSetLayouts = &pShader->mLayout;//mDescriptorSetLayout;
		if (vkCreatePipelineLayout(device->mDevice, &pipelineLayoutInfo, device->GetVkAllocCallBacks(), &mPipelineLayout) != VK_SUCCESS)
		{
			ASSERT(false);
		}

		VkPipelineShaderStageCreateInfo shaderStages{};
		shaderStages.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
		shaderStages.stage = VK_SHADER_STAGE_COMPUTE_BIT;
		shaderStages.module = pShader->mShader;
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