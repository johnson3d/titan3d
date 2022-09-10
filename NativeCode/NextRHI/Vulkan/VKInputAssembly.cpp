#include "VKInputAssembly.h"
#include "VKGpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	VKInputLayout::VKInputLayout()
	{

	}
	VKInputLayout::~VKInputLayout()
	{
	}
	bool VKInputLayout::Init(VKGpuDevice* device, FInputLayoutDesc* desc)
	{
		mDesc = desc;
		
		desc->ShaderDesc = nullptr;

		mInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;

		for (const auto& i : desc->Layouts)
		{
			//todo: count same slot elements
			VkVertexInputBindingDescription bd;
			bd.binding = i.InputSlot;
			bd.stride = GetPixelByteWidth(i.Format);
			bd.inputRate = i.IsInstanceData ? VK_VERTEX_INPUT_RATE_INSTANCE : VK_VERTEX_INPUT_RATE_VERTEX;
			FindBindingDesc(i.InputSlot, bd);
		}

		for (const auto& i : desc->Layouts)
		{
			VkVertexInputAttributeDescription ad;
			ad.binding = i.InputSlot;
			ad.format = Format2VKFormat(i.Format);
			ad.location = InputLayoutSemanticToVK(i.SemanticName, i.SemanticIndex);//reflect from SpirV
			ad.offset = i.AlignedByteOffset;
			AttributeDescriptions.push_back(ad);
		}

		mInfo.vertexBindingDescriptionCount = (uint32_t)BindingDescriptions.size();
		mInfo.vertexAttributeDescriptionCount = (uint32_t)AttributeDescriptions.size();
		mInfo.pVertexBindingDescriptions = &BindingDescriptions[0];
		mInfo.pVertexAttributeDescriptions = &AttributeDescriptions[0];
		return true;
	}
	/*void VKInputLayout::GetDX12Elements(std::vector<D3D12_INPUT_ELEMENT_DESC>& mDx12Elements)
	{
		mDx12Elements.clear();
		for (const auto& i : mDesc->Layouts)
		{
			D3D12_INPUT_ELEMENT_DESC tmp;
			tmp.SemanticName = i.SemanticName.c_str();
			tmp.SemanticIndex = i.SemanticIndex;
			tmp.Format = FormatToVKFormat(i.Format);
			tmp.InputSlot = i.InputSlot;
			tmp.AlignedByteOffset = i.AlignedByteOffset;
			tmp.InputSlotClass = i.IsInstanceData ? D3D12_INPUT_CLASSIFICATION_PER_INSTANCE_DATA : D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA;
			tmp.InstanceDataStepRate = i.InstanceDataStepRate;
			mDx12Elements.push_back(tmp);
		}
	}*/
}

NS_END
