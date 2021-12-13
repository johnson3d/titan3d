#include "ID11FrameBuffers.h"
#include "ID11RenderTargetView.h"
#include "ID11DepthStencilView.h"
#include "ID11RenderContext.h"

#define new VNEW

NS_BEGIN

ID11FrameBuffers::ID11FrameBuffers()
{
}

ID11FrameBuffers::~ID11FrameBuffers()
{
	
}

void ID11FrameBuffers::BindSwapChain(UINT index, ISwapChain* swapchain)
{
	mSwapChainIndex = index;
	mSwapChain.StrongRef(swapchain);

	ID11RenderTargetView* pRTV = new ID11RenderTargetView();
	auto pTexture = swapchain->GetBackBuffer(0);
	IRenderTargetViewDesc rtvDesc;
	rtvDesc.SetTexture2D();
	rtvDesc.Format = pTexture->mTextureDesc.Format;
	rtvDesc.Width = pTexture->mTextureDesc.Width;
	rtvDesc.Height = pTexture->mTextureDesc.Height;
	rtvDesc.mGpuBuffer = pTexture;
	pRTV->Init(mRenderContext.GetPtr(), &rtvDesc);
	Safe_Release(mRenderTargets[index]);
	mRenderTargets[index] = pRTV;
}

bool ID11FrameBuffers::Init(ID11RenderContext* rc, const IFrameBuffersDesc* desc)
{
	mRenderContext.FromObject(rc);
	mDesc = *desc;
	mRenderPass.StrongRef(desc->RenderPass);
	return true;
}

NS_END