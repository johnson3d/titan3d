#pragma once
#include "../IRenderPipeline.h"
#include "../IFrameBuffers.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlPipelineState : public IRenderPipeline
{
public:
	MtlPipelineState();
	~MtlPipelineState();

	virtual void SetRasterizerState(ICommandList* pCmdList, IRasterizerState* pRasterState) override;
	virtual void SetDepthStencilState(ICommandList* pCmdList, IDepthStencilState* pDepthStencilState) override;
	virtual void SetBlendState(ICommandList* pCmdList, IBlendState* pBlendState) override;

public:
	bool Init(MtlContext* pCtx, const IRenderPipelineDesc* pDesc);
	void SetPipelineState(ICommandList* pCmdList);
};

NS_END