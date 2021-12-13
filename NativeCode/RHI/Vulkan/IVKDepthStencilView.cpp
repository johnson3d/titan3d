#include "IVKDepthStencilView.h"
#include "IVKRenderContext.h"
#include "IVKTexture2D.h"
#include "IVKShaderResourceView.h"

#define new VNEW

NS_BEGIN

IVKDepthStencilView::IVKDepthStencilView()
{
}


IVKDepthStencilView::~IVKDepthStencilView()
{
}

bool IVKDepthStencilView::Init(IVKRenderContext* rc, const IDepthStencilViewDesc* desc)
{
	mDesc = *desc;

	IShaderResourceViewDesc srvDesc;
	srvDesc.SetTexture2D();
	srvDesc.mGpuBuffer = desc->m_pTexture2D;
	srvDesc.Format = desc->Format;
	srvDesc.ViewDimension = SRV_DIMENSION::SRV_DIMENSION_TEXTURE2D;

	srvDesc.Texture2D.MipLevels = 1;
	auto pSRV = new IVKShaderResourceView();
	pSRV->Init(rc, &srvDesc);
	mTextureSRV.WeakRef(pSRV);
	m_refTexture2D.StrongRef(desc->m_pTexture2D);
	return true;
}
NS_END