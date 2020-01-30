#pragma once
#include "../IPass.h"
#include "MTLRenderContext.h"
#include "MTLIndexBuffer.h"

NS_BEGIN

class MtlCmdList;

class MtlMeshDrawPass : public IPass
{
public:
	MtlMeshDrawPass();
	~MtlMeshDrawPass();

	virtual void SetViewport(ICommandList* pCmdList, IViewPort* pViewport) override;
	virtual void SetScissorRect(ICommandList* pCmdList, IScissorRect* sr) override;

	virtual void SetPipeline(ICommandList* pCmdList, IRenderPipeline* pRenderPipeline) override;

	virtual void SetVertexBuffer(ICommandList* pCmdList, UINT32 StreamIndex, IVertexBuffer* pVtxBuffer, UINT32 Offset, UINT Stride) override;
	virtual void SetIndexBuffer(ICommandList* pCmdList, IIndexBuffer* pIdxBuffer) override;

	virtual void VSSetConstantBuffer(ICommandList* pCmdList, UINT32 index, IConstantBuffer* pConstBuffer) override;
	virtual void PSSetConstantBuffer(ICommandList* pCmdList, UINT32 index, IConstantBuffer* pConstBuffer) override;
	virtual void VSSetShaderResource(ICommandList* pCmdList, UINT32 index, IShaderResourceView* pSRV) override;
	virtual void PSSetShaderResource(ICommandList* pCmdList, UINT32 index, IShaderResourceView* pSRV) override;
	virtual void VSSetSampler(ICommandList* pCmdList, UINT32 index, ISamplerState* pSamplerState) override;
	virtual void PSSetSampler(ICommandList* pCmdList, UINT32 index, ISamplerState* pSamplerState) override;
	
	virtual void DrawPrimitive(ICommandList* pCmdList, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances) override;
	virtual void DrawIndexedPrimitive(ICommandList* pCmdList, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances) override;

private:
	id<MTLRenderPipelineState> m_pMtlRenderPipelineState;
	MtlIndexBuffer* m_refIndexBuffer;
};

NS_END