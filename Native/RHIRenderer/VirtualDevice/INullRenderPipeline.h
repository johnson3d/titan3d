#pragma once
#include "../IRenderPipeline.h"

NS_BEGIN

class INullRenderPipeline : public IRenderPipeline
{
public:
	INullRenderPipeline();
	~INullRenderPipeline();

	virtual void SetRasterizerState(ICommandList* cmd, IRasterizerState* Layout) override;
	virtual void SetDepthStencilState(ICommandList* cmd, IDepthStencilState* State) override;
	virtual void SetBlendState(ICommandList* cmd, IBlendState* State) override;
};

NS_END