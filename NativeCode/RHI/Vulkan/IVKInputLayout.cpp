#include "IVKInputLayout.h"

#define new VNEW

NS_BEGIN

IVKInputLayout::IVKInputLayout()
{
}

IVKInputLayout::~IVKInputLayout()
{
}

bool IVKInputLayout::Init(IVKRenderContext* rc, const IInputLayoutDesc* desc)
{
	mDesc.StrongRef((IInputLayoutDesc*)desc);
	memset(&CreateInfo, 0, sizeof(CreateInfo));
	CreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO;

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
		ad.location = HLSLBinding2KhronosBindingLocation(i.SemanticName, i.SemanticIndex);//reflect from SpirV
		ad.offset = i.AlignedByteOffset;
		AttributeDescriptions.push_back(ad);
	}
	
	CreateInfo.vertexBindingDescriptionCount = (uint32_t)BindingDescriptions.size();
	CreateInfo.vertexAttributeDescriptionCount = (uint32_t)AttributeDescriptions.size();
	CreateInfo.pVertexBindingDescriptions = &BindingDescriptions[0];
	CreateInfo.pVertexAttributeDescriptions = &AttributeDescriptions[0];
	return true;
}

NS_END