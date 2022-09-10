#pragma once
#include "../NxInputAssembly.h"
#include "VKPreHead.h"

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	class VKInputLayout : public IInputLayout
	{
	public:
		VKInputLayout();
		~VKInputLayout();
		bool Init(VKGpuDevice* device, FInputLayoutDesc* desc);
	public:
		//void GetDX12Elements(std::vector<D3D12_INPUT_ELEMENT_DESC>& mDx12Elements);
		std::vector<VkVertexInputBindingDescription>		BindingDescriptions;
		std::vector<VkVertexInputAttributeDescription>		AttributeDescriptions;
		VkPipelineVertexInputStateCreateInfo	mInfo{};

		VkVertexInputBindingDescription* FindBindingDesc(UINT slot, const VkVertexInputBindingDescription& bd) {
			for (auto& i : BindingDescriptions)
			{
				if (i.binding == slot)
				{
					i.stride += bd.stride;
					return &i;
				}
			}
			BindingDescriptions.push_back(bd);
			return &BindingDescriptions[BindingDescriptions.size() - 1];
		}
	};
}

NS_END

