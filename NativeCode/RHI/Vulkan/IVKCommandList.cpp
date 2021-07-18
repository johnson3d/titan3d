#include "IVKCommandList.h"
#include "IVKPass.h"
#include "IVKRenderContext.h"

#define new VNEW

NS_BEGIN

IVKCommandList::IVKCommandList()
{
	mCommandBuffer = nullptr;
}

IVKCommandList::~IVKCommandList()
{
	auto rc = mRenderContext.GetPtr();
	if (rc == nullptr)
		return;
	if (mCommandBuffer != nullptr)
	{
		vkFreeCommandBuffers(rc->mLogicalDevice, rc->mCommandPool, 1, &mCommandBuffer);
		mCommandBuffer = nullptr;
	}
}

void IVKCommandList::BeginCommand()
{
	ICommandList::BeginCommand();
}

void IVKCommandList::EndCommand()
{
	ICommandList::EndCommand();
}

void IVKCommandList::BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer, const char* debugName)
{

}

void IVKCommandList::EndRenderPass()
{

}

void IVKCommandList::Commit(IRenderContext* pRHICtx)
{

}

void IVKCommandList::SetRasterizerState(IRasterizerState* State)
{

}

void IVKCommandList::SetDepthStencilState(IDepthStencilState* State)
{

}

void IVKCommandList::SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask)
{

}

void IVKCommandList::SetComputeShader(IComputeShader* ComputerShader)
{

}

void IVKCommandList::CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
{

}

void IVKCommandList::CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT* pUAVInitialCounts)
{

}

void IVKCommandList::CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer)
{

}

void IVKCommandList::CSDispatch(UINT x, UINT y, UINT z)
{

}

vBOOL IVKCommandList::CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers)
{
	return FALSE;
}

NS_END