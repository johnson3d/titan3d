#pragma once
#include "../ISamplerState.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;

class IGLSamplerState : public ISamplerState
{
public:
	IGLSamplerState();
	~IGLSamplerState();
	virtual void Cleanup() override;
public:
	std::shared_ptr<GLSdk::GLBufferId>			mSampler;
public:
	bool Init(IGLRenderContext* rc, const ISamplerStateDesc* desc);
};

NS_END