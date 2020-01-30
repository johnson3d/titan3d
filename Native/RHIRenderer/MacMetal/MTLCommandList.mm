#include "MTLCommandList.h"
#include "MTLSwapChain.h"
#include "MTLFrameBuffers.h"
#include "MTLTextureCommon.h"
#include "MTLRenderTargetView.h"
#include "MTLDepthStencilView.h"


#define new VNEW

NS_BEGIN

MtlCmdList::MtlCmdList()
{
	m_pCmdBuffer = nil;
	m_pRenderCmdEncoder = nil;
	m_refContext = nullptr;
	m_pMtlRenderPipelineDesc = nil;
	m_pMtlRenderPassDesc = nil;
}

MtlCmdList::~MtlCmdList()
{
	[m_pMtlRenderPipelineDesc release];
	m_pMtlRenderPipelineDesc = nil;
	[m_pMtlRenderPassDesc release];
	m_pMtlRenderPassDesc = nil;
}

void MtlCmdList::BeginCommand()
{
	m_pCmdBuffer = [m_refContext->m_pCmdQueue commandBuffer];
	
	ICommandList::BeginCommand();
}

void MtlCmdList::EndCommand()
{
	ICommandList::EndCommand();
}

MTLLoadAction TranslateLoadAction_RHI2Mtl(FrameBufferLoadAction load_action)
{
	static const MTLLoadAction LoadActionArray[3] = 
	{
		MTLLoadActionDontCare,
		MTLLoadActionLoad,
		MTLLoadActionClear
	};

	UINT32 idx = (UINT32)load_action;
	return LoadActionArray[idx];
}

MTLStoreAction TranslateStoreAction_RHI2Mtl(FrameBufferStoreAction store_action)
{
	static const MTLStoreAction StoreActionArray[5] = 
	{
		MTLStoreActionDontCare,
		MTLStoreActionStore,
		MTLStoreActionMultisampleResolve,
		MTLStoreActionStoreAndMultisampleResolve,
		MTLStoreActionUnknown
	};
	UINT32 idx = (UINT32)store_action;
	return StoreActionArray[idx];
}


void MtlCmdList::BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer)
{
	if (pFrameBuffer->mDesc.IsSwapChainBuffer == TRUE)
	{
		m_refContext->m_refMtlDrawable = [m_refContext->m_refMtlLayer nextDrawable];
		
		if (m_refContext->m_refMtlDrawable == nil || m_refContext->m_refMtlDrawable.texture == nil)
		{
			m_pMtlRenderPassDesc.colorAttachments[0].texture = m_refContext->m_pOnePixelTex2D;
		}
		else
		{
			m_pMtlRenderPassDesc.colorAttachments[0].texture = m_refContext->m_refMtlDrawable.texture;
		}
		//m_pMtlRenderPassDesc.colorAttachments[0].texture = refMtlTexture2D == nil ? m_refContext->m_pOnePixelTex2D : refMtlTexture2D;
		m_pMtlRenderPassDesc.colorAttachments[0].loadAction = TranslateLoadAction_RHI2Mtl(pRenderPassDesc->mFBLoadAction_Color);
		m_pMtlRenderPassDesc.colorAttachments[0].storeAction = TranslateStoreAction_RHI2Mtl(pRenderPassDesc->mFBStoreAction_Color);
		m_pMtlRenderPassDesc.colorAttachments[0].clearColor = MTLClearColorMake(pRenderPassDesc->mFBClearColorRT0.r, pRenderPassDesc->mFBClearColorRT0.g,
			pRenderPassDesc->mFBClearColorRT0.b, pRenderPassDesc->mFBClearColorRT0.a);

		m_pMtlRenderPipelineDesc.colorAttachments[0].pixelFormat = MTLPixelFormatBGRA8Unorm;

		for (UINT32 idx = 1; idx < MAX_MRT_NUM; idx++)
		{
			m_pMtlRenderPassDesc.colorAttachments[idx].texture = nil;
			m_pMtlRenderPipelineDesc.colorAttachments[idx].pixelFormat = MTLPixelFormatInvalid;
		}
	}
	else
	{
		FrameBufferClearColor RTVClearColorArray[MAX_MRT_NUM] = 
		{
			pRenderPassDesc->mFBClearColorRT0,
			pRenderPassDesc->mFBClearColorRT1,
			pRenderPassDesc->mFBClearColorRT2,
			pRenderPassDesc->mFBClearColorRT3,
			pRenderPassDesc->mFBClearColorRT4,
			pRenderPassDesc->mFBClearColorRT5,
			pRenderPassDesc->mFBClearColorRT6,
			pRenderPassDesc->mFBClearColorRT7
		};
		
		for (UINT32 RTVIdx = 0; RTVIdx < MAX_MRT_NUM; RTVIdx++)
		{
			MtlRTV* refRTV = (MtlRTV*)pFrameBuffer->mRenderTargets[RTVIdx];
			if (refRTV == nullptr)
			{
				m_pMtlRenderPassDesc.colorAttachments[RTVIdx].texture = nil;
				m_pMtlRenderPipelineDesc.colorAttachments[RTVIdx].pixelFormat = MTLPixelFormatInvalid;
				continue;
			}
			id<MTLTexture> refMtlTex2d = ((MtlTexture2D*)((ITexture2D*)(refRTV->m_refTexture2D)))->m_pMtlTexture2D;
			m_pMtlRenderPassDesc.colorAttachments[RTVIdx].texture = refMtlTex2d;
			m_pMtlRenderPassDesc.colorAttachments[RTVIdx].loadAction = TranslateLoadAction_RHI2Mtl(pRenderPassDesc->mFBLoadAction_Color);
			m_pMtlRenderPassDesc.colorAttachments[RTVIdx].storeAction = TranslateStoreAction_RHI2Mtl(pRenderPassDesc->mFBStoreAction_Color);
			m_pMtlRenderPassDesc.colorAttachments[RTVIdx].clearColor = MTLClearColorMake(RTVClearColorArray[RTVIdx].r, RTVClearColorArray[RTVIdx].g,
				RTVClearColorArray[RTVIdx].b, RTVClearColorArray[RTVIdx].a);

			m_pMtlRenderPipelineDesc.colorAttachments[RTVIdx].pixelFormat = TranslatePixelFormat_RHI2Mtl(((MtlTexture2D*)(ITexture2D*)refRTV->m_refTexture2D)->mDesc.Format);
		}
	}
	
	MtlDepthStencilView* refDSV = (MtlDepthStencilView*)pFrameBuffer->m_pDepthStencilView;
	if (refDSV != nullptr)
	{
		m_pMtlRenderPassDesc.depthAttachment.texture = ((MtlTexture2D*)(ITexture2D*)refDSV->m_refTexture2D)->m_pMtlTexture2D;
		m_pMtlRenderPassDesc.depthAttachment.loadAction = TranslateLoadAction_RHI2Mtl(pRenderPassDesc->mFBLoadAction_Depth);
		m_pMtlRenderPassDesc.depthAttachment.storeAction = TranslateStoreAction_RHI2Mtl(pRenderPassDesc->mFBStoreAction_Depth);
		m_pMtlRenderPassDesc.depthAttachment.clearDepth = pRenderPassDesc->mDepthClearValue;

		m_pMtlRenderPipelineDesc.depthAttachmentPixelFormat = MTLPixelFormatDepth32Float;
		
		if (refDSV->mDesc.mUseStencil == TRUE)
		{
			m_pMtlRenderPassDesc.stencilAttachment.texture = refDSV->m_pMtlStencilBuffer;
			m_pMtlRenderPassDesc.stencilAttachment.loadAction = TranslateLoadAction_RHI2Mtl(pRenderPassDesc->mFBLoadAction_Stencil);
			m_pMtlRenderPassDesc.stencilAttachment.storeAction = TranslateStoreAction_RHI2Mtl(pRenderPassDesc->mFBStoreAction_Stencil);
			m_pMtlRenderPassDesc.stencilAttachment.clearStencil = pRenderPassDesc->mStencilClearValue;

			m_pMtlRenderPipelineDesc.stencilAttachmentPixelFormat = MTLPixelFormatStencil8;
		}
		else
		{
			m_pMtlRenderPassDesc.stencilAttachment.texture = nil;
			m_pMtlRenderPipelineDesc.stencilAttachmentPixelFormat = MTLPixelFormatInvalid;
		}
	}
	else
	{
		m_pMtlRenderPassDesc.depthAttachment.texture = nil;
		m_pMtlRenderPipelineDesc.depthAttachmentPixelFormat = MTLPixelFormatInvalid;
	}

	m_pRenderCmdEncoder = [m_pCmdBuffer renderCommandEncoderWithDescriptor : m_pMtlRenderPassDesc];
}

void MtlCmdList::EndRenderPass()
{
	[m_pRenderCmdEncoder endEncoding];
	ICommandList::EndRenderPass();
}

void MtlCmdList::Commit(IRenderContext* pRHICtx)
{
	if (m_pCmdBuffer == nil)
	{
		return;
	}

	MtlSwapChain* refSwapChain = (MtlSwapChain*)pSwapChain;
	if (refSwapChain != nullptr && m_refContext->m_refMtlDrawable != nil)
	{
		[m_pCmdBuffer presentDrawable: m_refContext->m_refMtlDrawable];
	}

	[m_pCmdBuffer enqueue];
	[m_pCmdBuffer commit];
	m_pCmdBuffer = nil;

	if (refSwapChain != nullptr)
	{
		//m_refContext->m_refMtlDrawable = [[refSwapChain->m_pMtlLayer nextDrawable] retain];
		m_refContext->m_refMtlLayer = refSwapChain->m_pMtlLayer;
	}
}

bool MtlCmdList::Init(MtlContext* pCtx)
{	
	m_refContext = pCtx;
	m_pMtlRenderPipelineDesc = [[MTLRenderPipelineDescriptor alloc] init];
	m_pMtlRenderPassDesc = [[MTLRenderPassDescriptor alloc] init];
	return true;
}

NS_END