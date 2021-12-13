#pragma once
#include "../IDepthStencilState.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKDepthStencilState : public IDepthStencilState
{
public:
	IVKDepthStencilState();
	~IVKDepthStencilState();
	bool Init(IVKRenderContext*rc, const IDepthStencilStateDesc* desc);
public:
	VkPipelineDepthStencilStateCreateInfo		CreateInfo;
};

NS_END