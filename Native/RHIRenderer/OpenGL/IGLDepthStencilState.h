#pragma once
#include "../IDepthStencilState.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLDepthStencilState : public IDepthStencilState
{
public:
	IGLDepthStencilState();
	~IGLDepthStencilState();

	virtual void Cleanup() override;
public:
	bool Init(IGLRenderContext* rc, const IDepthStencilStateDesc* desc);

	void ApplyStates(GLSdk* sdk);
};

NS_END