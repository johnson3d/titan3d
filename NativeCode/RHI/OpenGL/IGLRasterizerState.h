#pragma once
#include "../IRasterizerState.h"

NS_BEGIN

class GLSdk;
class IGLRenderContext;
class IGLRasterizerState : public IRasterizerState
{
public:
	IGLRasterizerState();
	~IGLRasterizerState();

	virtual void Cleanup() override;
public:
public:
	bool Init(IGLRenderContext* rc, const IRasterizerStateDesc* desc);

	void ApplyStates(GLSdk* sdk);
};

NS_END