#include "MTLDepthStencilView.h"

NS_BEGIN

MtlDepthStencilView::MtlDepthStencilView()
{
	m_pMtlStencilBuffer = nil;
}

MtlDepthStencilView::~MtlDepthStencilView()
{
}

bool MtlDepthStencilView::Init(MtlContext* pCtx, const IDepthStencilViewDesc* pDesc)
{
	mDesc = *pDesc;
	m_refTexture2D.StrongRef(pDesc->m_pTexture2D);
	if (pDesc->mUseStencil == TRUE)
	{
		MTLTextureDescriptor* pMtlTexDesc = [MTLTextureDescriptor new];
		pMtlTexDesc.textureType = MTLTextureType2D;
		pMtlTexDesc.width = pDesc->Width;
		pMtlTexDesc.height = pDesc->Height;
		pMtlTexDesc.depth = 1;
		pMtlTexDesc.arrayLength = 1;
		pMtlTexDesc.sampleCount = 1;
		pMtlTexDesc.mipmapLevelCount = 1;
		pMtlTexDesc.pixelFormat = MTLPixelFormatStencil8;
		pMtlTexDesc.usage = MTLTextureUsageRenderTarget | MTLTextureUsageShaderRead;
		m_pMtlStencilBuffer = [pCtx->m_pDevice newTextureWithDescriptor : pMtlTexDesc];

		[pMtlTexDesc release];
		pMtlTexDesc = nil;
	}
	return true;
}

NS_END