#pragma once
#include "../IRasterizerState.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRasterizerState : public IRasterizerState
{
public:
	IVKRasterizerState();
	~IVKRasterizerState();
	bool Init(IVKRenderContext* rc, const IRasterizerStateDesc* desc);
public:
	VkPipelineRasterizationStateCreateInfo		mCreateInfo;
};

NS_END