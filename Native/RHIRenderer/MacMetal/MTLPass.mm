#include "MTLPass.h"
#include "MTLCommandList.h"
#include "MTLRenderPipeline.h"
#include "MTLConstantBuffer.h"
#include "MTLVertexBuffer.h"
#include "MTLSamplerState.h"
#include "MTLShaderResourceView.h"

#define new VNEW

NS_BEGIN

MtlMeshDrawPass::MtlMeshDrawPass()
{
	m_pMtlRenderPipelineState = nil;
	m_refIndexBuffer = nullptr;
}

MtlMeshDrawPass::~MtlMeshDrawPass()
{
}

void MtlMeshDrawPass::SetViewport(ICommandList* pCmdList, IViewPort* pVP)
{
	MTLViewport vp = { pVP->TopLeftX, pVP->TopLeftY, pVP->Width, pVP->Height, 0.0f, 1.0f };
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	[refCmdList->m_pRenderCmdEncoder setViewport : vp];
}

void MtlMeshDrawPass::SetScissorRect(ICommandList* pCmdList, IScissorRect* sr)
{
	/*if (pSR->bEnable == true)
	{
		MTLScissorRect sr = {(NSUInteger)pSR->MinX, (NSUInteger)pSR->MinY, (NSUInteger)(pSR->MaxX - pSR->MinX), (NSUInteger)(pSR->MaxY - pSR->MinY)};
		MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
		[refCmdList->m_pRenderCmdEncoder setScissorRect : sr];
	}*/
}

void MtlMeshDrawPass::SetPipeline(ICommandList* pCmdList, IRenderPipeline* pPipelineState)
{
	MtlPipelineState* refPipelineState = (MtlPipelineState*)pPipelineState;
	refPipelineState->SetPipelineState(pCmdList);
}

void MtlMeshDrawPass::SetVertexBuffer(ICommandList* pCmdList, UINT32 StreamIndex, IVertexBuffer* pVertexBuffer, UINT32 Offset, UINT Stride)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	if (pVertexBuffer == nullptr)
	{
		[refCmdList->m_pRenderCmdEncoder setVertexBuffer: refCmdList->m_refContext->m_pOneByteBuffer  offset: Offset atIndex : MaxConstBufferNum + StreamIndex];
	}
	else
	{
		MtlVertexBuffer* refVB = (MtlVertexBuffer*)pVertexBuffer;
		[refCmdList->m_pRenderCmdEncoder setVertexBuffer : refVB->m_pVtxBuffer offset: Offset atIndex : MaxConstBufferNum + StreamIndex];
	}
}

void MtlMeshDrawPass::SetIndexBuffer(ICommandList* pCmdList, IIndexBuffer* pIdxBuffer)
{
	m_refIndexBuffer = (MtlIndexBuffer*)pIdxBuffer;
}

void MtlMeshDrawPass::VSSetConstantBuffer(ICommandList* pCmdList, UINT32 index, IConstantBuffer* pCBuffer)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	MtlConstBuffer* refConstBuffer = (MtlConstBuffer*)pCBuffer;
	[refCmdList->m_pRenderCmdEncoder setVertexBuffer : refConstBuffer->m_pConstBuffer offset : 0 atIndex : (NSUInteger)index];
}

void MtlMeshDrawPass::PSSetConstantBuffer(ICommandList* pCmdList, UINT32 index, IConstantBuffer* pCBuffer)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	MtlConstBuffer* refConstBuffer = (MtlConstBuffer*)pCBuffer;
	[refCmdList->m_pRenderCmdEncoder setFragmentBuffer : refConstBuffer->m_pConstBuffer offset : 0 atIndex : (NSUInteger)index];
}

void MtlMeshDrawPass::VSSetShaderResource(ICommandList* pCmdList, UINT32 Index, IShaderResourceView* pSRV)
{
	if (pSRV == nullptr)
	{
		return;
	}
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	pSRV->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
	if (pSRV->GetResourceState()->GetStreamState() != SS_Valid)
	{
		[refCmdList->m_pRenderCmdEncoder setVertexTexture: refCmdList->m_refContext->m_pOnePixelTex2D atIndex: Index];
	}
	else
	{
		MtlShaderResView* refSRV = (MtlShaderResView*)pSRV;
		[refCmdList->m_pRenderCmdEncoder setVertexTexture : ((MtlTexture2D*)(ITexture2D*)refSRV->m_refTexture2D)->m_pMtlTexture2D atIndex : Index];
	}
}

void MtlMeshDrawPass::PSSetShaderResource(ICommandList* pCmdList, UINT32 Index, IShaderResourceView* pSRV)
{
	if (pSRV == nullptr)
	{
		return;
	}
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	pSRV->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
	if (pSRV->GetResourceState()->GetStreamState() != SS_Valid)
	{
		[refCmdList->m_pRenderCmdEncoder setFragmentTexture : refCmdList->m_refContext->m_pOnePixelTex2D atIndex : Index];
	}
	else
	{
		MtlShaderResView* refSRV = (MtlShaderResView*)pSRV;
		[refCmdList->m_pRenderCmdEncoder setFragmentTexture : ((MtlTexture2D*)(ITexture2D*)refSRV->m_refTexture2D)->m_pMtlTexture2D atIndex : Index];
	}
}

//we can use setVertexSamplerStates to set all the samplers in one RHI call;improve this later;
void MtlMeshDrawPass::VSSetSampler(ICommandList* pCmdList, UINT32 Index, ISamplerState* pSamplerState)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	MtlSamplerState* refSamplerState = (MtlSamplerState*)pSamplerState;
	[refCmdList->m_pRenderCmdEncoder setVertexSamplerState: refSamplerState->m_pSamplerState  atIndex: Index];
}

//we can use setFragmentSamplerStates to set all the samplers in one RHI call;improve this later;
void MtlMeshDrawPass::PSSetSampler(ICommandList* pCmdList, UINT32 Index, ISamplerState* pSamplerState)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	MtlSamplerState* refSamplerState = (MtlSamplerState*)pSamplerState;
	[refCmdList->m_pRenderCmdEncoder setFragmentSamplerState: refSamplerState->m_pSamplerState  atIndex: Index];
}

static const MTLPrimitiveType MtlPrimitiveTypeArray[6] =
{
	MTLPrimitiveTypePoint,
	MTLPrimitiveTypeLine,
	MTLPrimitiveTypeLineStrip,
	MTLPrimitiveTypeTriangle,
	MTLPrimitiveTypeTriangleStrip,
	MTLPrimitiveTypeTriangle
};

MTLPrimitiveType TranslatePrimitiveType_RHI2Mtl(EPrimitiveType primitive_type)
{
	UINT32 idx = (UINT32)primitive_type - 1;
	return MtlPrimitiveTypeArray[idx];
}

void MtlMeshDrawPass::DrawPrimitive(ICommandList* pCmdList, EPrimitiveType PrimitiveType, UINT32 VtxStartIndex, UINT32 PrimitiveCount, UINT32 NumInstance)
{
	//we need to add pass dirty  mechanism here in the future;
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	
	if (m_pMtlRenderPipelineState == nil)
	{
		NSError* pErrorInfo = nil;
		m_pMtlRenderPipelineState = [refCmdList->m_refContext->m_pDevice newRenderPipelineStateWithDescriptor: refCmdList->m_pMtlRenderPipelineDesc error:&pErrorInfo];

		AssertRHI(pErrorInfo == nil);
	}

	[refCmdList->m_pRenderCmdEncoder setRenderPipelineState: m_pMtlRenderPipelineState];

	MTLPrimitiveType mtl_primitive_type = TranslatePrimitiveType_RHI2Mtl(PrimitiveType);
	UINT32 VtxCount = 3 * PrimitiveCount; //we only support and need triangle primitive;

	[refCmdList->m_pRenderCmdEncoder drawPrimitives: mtl_primitive_type vertexStart: VtxStartIndex vertexCount: VtxCount instanceCount: NumInstance];
}

void MtlMeshDrawPass::DrawIndexedPrimitive(ICommandList* pCmdList, EPrimitiveType PrimitiveType, UINT32 VtxStartIndex, UINT32 IdxStartIndex, UINT32 NumPrimitive, UINT32 NumInstance)
{
	//we need to add pass dirty  mechanism here in the future;
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;

	if (m_pMtlRenderPipelineState == nil)
	{
		NSError* pErrorInfo = nil;
		m_pMtlRenderPipelineState = [refCmdList->m_refContext->m_pDevice newRenderPipelineStateWithDescriptor : refCmdList->m_pMtlRenderPipelineDesc error : &pErrorInfo];

		AssertRHI(pErrorInfo == nil);
	}

	[refCmdList->m_pRenderCmdEncoder setRenderPipelineState : m_pMtlRenderPipelineState];

	MTLPrimitiveType mtl_primitive_type = TranslatePrimitiveType_RHI2Mtl(PrimitiveType);
	
	[refCmdList->m_pRenderCmdEncoder drawIndexedPrimitives: mtl_primitive_type indexCount: m_refIndexBuffer->mMtlIndexCount indexType: m_refIndexBuffer->mMtlIndexType 
		indexBuffer: m_refIndexBuffer->m_pIndexBuffer indexBufferOffset: IdxStartIndex instanceCount: NumInstance];
}

NS_END