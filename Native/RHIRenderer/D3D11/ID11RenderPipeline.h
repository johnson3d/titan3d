#pragma once
#include "../IRenderPipeline.h"

NS_BEGIN

class ID11RenderContext;
class ID11RenderPipeline : public IRenderPipeline
{
public:
	ID11RenderPipeline();
	~ID11RenderPipeline();

	virtual void SetRasterizerState(ICommandList* cmd, IRasterizerState* State) override;
	virtual void SetDepthStencilState(ICommandList* cmd, IDepthStencilState* State) override;
	virtual void SetBlendState(ICommandList* cmd, IBlendState* State) override;
public:
public:
	bool Init(ID11RenderContext* rc, const IRenderPipelineDesc* desc);
	void ApplyState(ICommandList* cmd);
};

NS_END