#pragma once
#include "../ISamplerState.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKSamplerState : public ISamplerState
{
public:
	IVKSamplerState();
	~IVKSamplerState();
	bool Init(IVKRenderContext* rc, const ISamplerStateDesc* desc);
public:
	VkSampler			TextureSampler;
};

NS_END