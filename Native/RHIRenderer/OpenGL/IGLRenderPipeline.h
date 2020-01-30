#pragma once
#include "../IRenderPipeline.h"

NS_BEGIN

class IGLRenderContext;

class IGLRenderPipeline : public IRenderPipeline
{
public:
	IGLRenderPipeline();
	~IGLRenderPipeline();
	virtual void Cleanup() override;

	virtual void SetRasterizerState(ICommandList* cmd, IRasterizerState* State) override;
	virtual void SetDepthStencilState(ICommandList* cmd, IDepthStencilState* State) override;
	virtual void SetBlendState(ICommandList* cmd, IBlendState* State) override;
public:
public:
	bool Init(IGLRenderContext* rc, const IRenderPipelineDesc* desc);

	void ApplyState(ICommandList* cmd);
};

NS_END