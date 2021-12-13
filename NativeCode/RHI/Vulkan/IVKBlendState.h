#pragma once

#include "../IBlendState.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;

class IVKBlendState : public IBlendState
{
public:
	IVKBlendState();
	~IVKBlendState();
	bool Init(IVKRenderContext* rc, const IBlendStateDesc* desc);
public:
	VkPipelineColorBlendAttachmentState ColorBlendAttachment[8];
	VkPipelineColorBlendStateCreateInfo ColorBlending;
};

NS_END