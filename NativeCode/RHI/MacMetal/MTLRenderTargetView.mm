#include "MTLRenderTargetView.h"

#define new VNEW

NS_BEGIN

MtlRTV::MtlRTV()
{
}

MtlRTV::~MtlRTV()
{
}

bool MtlRTV::Init(MtlContext* pCtx, const IRenderTargetViewDesc* pDesc)
{
	m_refTexture2D.StrongRef(pDesc->m_pTexture2D);
	return true;
}

NS_END