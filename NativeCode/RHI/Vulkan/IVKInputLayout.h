#pragma once
#include "../IInputLayout.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKInputLayout : public IInputLayout
{
public:
	IVKInputLayout();
	~IVKInputLayout();
	bool Init(IVKRenderContext* rc, const IInputLayoutDesc* desc);
public:
	std::vector<VkVertexInputBindingDescription>		BindingDescriptions;
	std::vector<VkVertexInputAttributeDescription>		AttributeDescriptions;
	VkPipelineVertexInputStateCreateInfo				CreateInfo;

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
		return &BindingDescriptions[BindingDescriptions.size()-1];
	}
};

NS_END