#include "INullCommandList.h"
#include "INullPass.h"
#include "INullRenderContext.h"

#define new VNEW

NS_BEGIN

INullCommandList::INullCommandList()
{
}

INullCommandList::~INullCommandList()
{
	
}

void INullCommandList::BeginCommand()
{
	ICommandList::BeginCommand();
}

void INullCommandList::EndCommand()
{
	ICommandList::EndCommand();
}

void INullCommandList::BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer)
{

}

void INullCommandList::EndRenderPass()
{

}

void INullCommandList::Commit(IRenderContext* pRHICtx)
{

}

void INullCommandList::SetRasterizerState(IRasterizerState* State)
{

}

void INullCommandList::SetDepthStencilState(IDepthStencilState* State)
{

}

void INullCommandList::SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask)
{

}

void INullCommandList::SetComputeShader(IComputeShader* ComputerShader)
{

}

void INullCommandList::CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
{

}

void INullCommandList::CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT* pUAVInitialCounts)
{

}

void INullCommandList::CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer)
{

}

void INullCommandList::CSDispatch(UINT x, UINT y, UINT z)
{

}

vBOOL INullCommandList::CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers)
{
	return FALSE;
}

NS_END