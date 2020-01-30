#include "MTLRenderPipeline.h"
#include "MTLRasterizerState.h"
#include "MTLDepthStencilState.h"
#include "MTLBlendState.h"
#include "MTLCommandList.h"


#define new VNEW

NS_BEGIN

MtlPipelineState::MtlPipelineState()
{
	
}

MtlPipelineState::~MtlPipelineState()
{
}

void MtlPipelineState::SetRasterizerState(ICommandList* pCmdList, IRasterizerState* pRasterState)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	MtlRasterState* refRasterState = (MtlRasterState*)pRasterState;
	[refCmdList->m_pRenderCmdEncoder setTriangleFillMode : refRasterState->mFillMode];
	[refCmdList->m_pRenderCmdEncoder setCullMode : refRasterState->mCullMode];
	[refCmdList->m_pRenderCmdEncoder setFrontFacingWinding : refRasterState->mFrontFaceWinding];
	[refCmdList->m_pRenderCmdEncoder setDepthBias : refRasterState->mDepthBias slopeScale : refRasterState->mSlopeScaledDepthBias clamp : refRasterState->mDepthBiasClamp];
}

void MtlPipelineState::SetDepthStencilState(ICommandList* pCmdList, IDepthStencilState* pDepthStencilState)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	MtlDepthStencilState* refDepthStencilState = (MtlDepthStencilState*)pDepthStencilState;
	[refCmdList->m_pRenderCmdEncoder setDepthStencilState : refDepthStencilState->m_pDepthStencilState];
	[refCmdList->m_pRenderCmdEncoder setStencilReferenceValue : refDepthStencilState->mStencilRefValue];
}

void MtlPipelineState::SetBlendState(ICommandList* pCmdList, IBlendState* pBlendState)
{
	MtlCmdList* refCmdList = (MtlCmdList*)pCmdList;
	MtlBlendState* pMtlBlendState = (MtlBlendState*)pBlendState;
	for (UINT32 idx = 0; idx < MAX_MRT_NUM; idx++)
	{
		refCmdList->m_pMtlRenderPipelineDesc.colorAttachments[idx].blendingEnabled = pMtlBlendState->mMtlBlendStateDescArray[idx].mEnable;
		refCmdList->m_pMtlRenderPipelineDesc.colorAttachments[idx].rgbBlendOperation = pMtlBlendState->mMtlBlendStateDescArray[idx].mBlendOperationRGB;
		refCmdList->m_pMtlRenderPipelineDesc.colorAttachments[idx].alphaBlendOperation = pMtlBlendState->mMtlBlendStateDescArray[idx].mBlendOperationAlpha;
		refCmdList->m_pMtlRenderPipelineDesc.colorAttachments[idx].sourceRGBBlendFactor = pMtlBlendState->mMtlBlendStateDescArray[idx].mBlendFactorSrcRGB;
		refCmdList->m_pMtlRenderPipelineDesc.colorAttachments[idx].sourceAlphaBlendFactor = pMtlBlendState->mMtlBlendStateDescArray[idx].mBlendFactorSrcAlpha;
		refCmdList->m_pMtlRenderPipelineDesc.colorAttachments[idx].destinationRGBBlendFactor = pMtlBlendState->mMtlBlendStateDescArray[idx].mBlendFactorDstRGB;
		refCmdList->m_pMtlRenderPipelineDesc.colorAttachments[idx].destinationAlphaBlendFactor = pMtlBlendState->mMtlBlendStateDescArray[idx].mBlendFactorDstAlpha;
		refCmdList->m_pMtlRenderPipelineDesc.colorAttachments[idx].writeMask = pMtlBlendState->mMtlBlendStateDescArray[idx].mWriteMask;
	}
}

bool MtlPipelineState::Init(MtlContext* pCtx, const IRenderPipelineDesc* pDesc)
{
	return true;
}

void MtlPipelineState::SetPipelineState(ICommandList* pCmdList)
{
	SetRasterizerState(pCmdList, mRasterizerState);
	SetDepthStencilState(pCmdList, mDepthStencilState);
	SetBlendState(pCmdList, mBlendState);

}

NS_END