#include "IGLRenderTargetView.h"
#include "IGLRenderContext.h"
#include "IGLTexture2D.h"
#include "IGLShaderResourceView.h"

#define new VNEW

NS_BEGIN

IGLRenderTargetView::IGLRenderTargetView()
{
}

IGLRenderTargetView::~IGLRenderTargetView()
{
}

void IGLRenderTargetView::Cleanup()
{
}

bool IGLRenderTargetView::Init(IGLRenderContext* rc, const IRenderTargetViewDesc* pDesc)
{
	m_refTexture2D.StrongRef(pDesc->m_pTexture2D);

	return true;
}

NS_END