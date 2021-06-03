#include "INullPass.h"
#include "INullCommandList.h"
#include "INullRenderPipeline.h"
#include "INullRenderContext.h"
#include "INullConstantBuffer.h"

#define new VNEW

NS_BEGIN

INullDrawCall::INullDrawCall()
{
}

INullDrawCall::~INullDrawCall()
{
}

void INullDrawCall::BuildPass(ICommandList* cmd, vBOOL bImmCBuffer)
{
	//////////////////////////////////////////////////////////////////////////
	
	//////////////////////////////////////////////////////////////////////////
	
}

void INullDrawCall::SetViewport(ICommandList* cmd, IViewPort* vp)
{

}

void INullDrawCall::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
{

}

void INullDrawCall::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline)
{
	
}

void INullDrawCall::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{

}

void INullDrawCall::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
{

}

void INullDrawCall::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	
}

void INullDrawCall::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	
}

void INullDrawCall::VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	
}

void INullDrawCall::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* Texture)
{
	
}

void INullDrawCall::VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{

}

void INullDrawCall::PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{

}

void INullDrawCall::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{

}

void INullDrawCall::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{

}

void INullDrawCall::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{

}

NS_END