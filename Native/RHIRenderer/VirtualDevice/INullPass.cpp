#include "INullPass.h"
#include "INullCommandList.h"
#include "INullRenderPipeline.h"
#include "INullRenderContext.h"
#include "INullConstantBuffer.h"

#define new VNEW

NS_BEGIN

INullPass::INullPass()
{
}

INullPass::~INullPass()
{
}

void INullPass::BuildPass(ICommandList* cmd, vBOOL bImmCBuffer)
{
	//////////////////////////////////////////////////////////////////////////
	
	//////////////////////////////////////////////////////////////////////////
	
}

void INullPass::SetViewport(ICommandList* cmd, IViewPort* vp)
{

}

void INullPass::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
{

}

//����Ⱦ״̬
void INullPass::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline)
{
	
}

void INullPass::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{

}

void INullPass::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
{

}

void INullPass::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	
}

void INullPass::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	
}

void INullPass::VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	
}

void INullPass::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	
}

void INullPass::VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{

}

void INullPass::PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{

}

void INullPass::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{

}

void INullPass::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{

}

void INullPass::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{

}

NS_END