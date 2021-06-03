#pragma once

#include "../IBlendState.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;

class IGLBlendState : public IBlendState
{
public:
	IGLBlendState();
	~IGLBlendState();

public:
public:
	bool Init(IGLRenderContext* rc, const IBlendStateDesc* desc);

	void ApplyStates(GLSdk* sdk);
};

NS_END