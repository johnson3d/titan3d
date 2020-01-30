#include "MTLTexture2D.h"
#include "MTLTextureCommon.h"

NS_BEGIN

MtlTexture2D::MtlTexture2D()
{
	m_pMtlTexture2D = nil;
}

MtlTexture2D::~MtlTexture2D()
{

}


bool MtlTexture2D::Init(MtlContext* pCtx, const ITexture2DDesc* pDesc)
{
	mDesc.Width = pDesc->Width;
	mDesc.Height = pDesc->Height;
	mDesc.Format = pDesc->Format;
	mDesc.MipLevels = pDesc->MipLevels;
	mDesc.BindFlags = pDesc->BindFlags;
	
	MTLTextureDescriptor* pMtlTexDesc = [MTLTextureDescriptor new];
	pMtlTexDesc.textureType = MTLTextureType2D;
	pMtlTexDesc.width = pDesc->Width;
	pMtlTexDesc.height = pDesc->Height;
	pMtlTexDesc.depth = 1;
	pMtlTexDesc.arrayLength = 1;
	pMtlTexDesc.sampleCount = 1;
	pMtlTexDesc.mipmapLevelCount = pDesc->MipLevels;
	pMtlTexDesc.pixelFormat = TranslatePixelFormat_RHI2Mtl(pDesc->Format);
	
	if ((pDesc->BindFlags & 0x60) == 0)
	{
		pMtlTexDesc.usage = MTLTextureUsageShaderRead;
	}
	else
	{
		pMtlTexDesc.usage = MTLTextureUsageRenderTarget | MTLTextureUsageShaderRead;
	}
	
	m_pMtlTexture2D = [pCtx->m_pDevice newTextureWithDescriptor: pMtlTexDesc];
	
	[pMtlTexDesc release];
	pMtlTexDesc = nil;
	return true;
}

NS_END