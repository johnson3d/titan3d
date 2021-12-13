
#include "INullSwapChain.h"
#include "INullRenderSystem.h"
#include "INullRenderContext.h"
#include "INullShaderResourceView.h"

#define new VNEW

NS_BEGIN

INullSwapChain::INullSwapChain()
{
}


INullSwapChain::~INullSwapChain()
{

}

bool INullSwapChain::Init(INullRenderContext* rc, const ISwapChainDesc* desc)
{
	mDesc = *desc;
	mRenderContext.FromObject(rc);
	return true;
}

void INullSwapChain::BindCurrent()
{

}

void INullSwapChain::Present(UINT SyncInterval, UINT Flags)
{

}

void INullSwapChain::OnLost()
{

}

vBOOL INullSwapChain::OnRestore(const ISwapChainDesc* desc)
{
	return TRUE;
}

NS_END