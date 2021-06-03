#include "MTLSwapChain.h"

#define new VNEW

NS_BEGIN

MtlSwapChain::MtlSwapChain()
{
	m_pMtlLayer = nil;
	m_pSwapChainBuffer = nullptr;
}


MtlSwapChain::~MtlSwapChain()
{
	Safe_Release(m_pSwapChainBuffer);
}

ITexture2D* MtlSwapChain::GetTexture2D()
{
	return m_pSwapChainBuffer;
}

void MtlSwapChain::BindCurrent()
{
}

void MtlSwapChain::Present(UINT SyncInterval, UINT Flags)
{

}

void MtlSwapChain::OnLost()
{
}

vBOOL MtlSwapChain::OnRestore(const ISwapChainDesc* pDesc)
{
	Safe_Release(m_pSwapChainBuffer);

	m_pMtlLayer = (__bridge CAMetalLayer*)pDesc->WindowHandle;
	m_pSwapChainBuffer = new MtlTexture2D();
	m_pSwapChainBuffer->m_pMtlTexture2D = nil;
	return TRUE;
}

bool MtlSwapChain::Init(MtlContext* pCtx, const ISwapChainDesc* pDesc)
{
	m_pMtlLayer = (__bridge CAMetalLayer*)pDesc->WindowHandle;
	m_pSwapChainBuffer = new MtlTexture2D();
	m_pSwapChainBuffer->m_pMtlTexture2D = nil;
	return true;
}

NS_END