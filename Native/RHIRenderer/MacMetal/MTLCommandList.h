#pragma once
#include "../ICommandList.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlCmdList : public ICommandList
{
public:
	MtlCmdList();
	~MtlCmdList();

	virtual void BeginCommand() override;
	virtual void EndCommand() override;
	virtual void BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer) override;
	virtual void EndRenderPass() override;

	virtual void Commit(IRenderContext* pRHICtx) override;
	
public:
	bool Init(MtlContext* pCtx);


public:
	id<MTLCommandBuffer> m_pCmdBuffer;
	id<MTLRenderCommandEncoder> m_pRenderCmdEncoder;
	MtlContext* m_refContext;
	MTLRenderPipelineDescriptor* m_pMtlRenderPipelineDesc; 
	MTLRenderPassDescriptor* m_pMtlRenderPassDesc;
	

};

NS_END