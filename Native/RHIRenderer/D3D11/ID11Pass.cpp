#include "ID11Pass.h"
#include "ID11CommandList.h"
#include "ID11RenderPipeline.h"
#include "ID11CommandList.h"
#include "ID11VertexBuffer.h"
#include "ID11IndexBuffer.h"
#include "ID11ConstantBuffer.h"
#include "ID11ShaderResourceView.h"
#include "ID11SamplerState.h"
#include "ID11RenderContext.h"
#include "ID11InputLayout.h"
#include "ID11UnorderedAccessView.h"
#include "../../Core/vfxSampCounter.h"
#include "../../Graphics/Mesh/GfxMeshPrimitives.h"

#define new VNEW

NS_BEGIN

ID11Pass::ID11Pass()
{
	 
}

ID11Pass::~ID11Pass()
{
}

void ID11Pass::SetViewport(ICommandList* cmd, IViewPort* vp)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	D3D11_VIEWPORT d11vp;
	d11vp.Width = vp->Width;
	d11vp.Height = vp->Height;
	d11vp.MinDepth = vp->MinDepth;
	d11vp.MaxDepth = vp->MaxDepth;
	d11vp.TopLeftX = vp->TopLeftX;
	d11vp.TopLeftY = vp->TopLeftY;

	auto d11cmd = (ID11CommandList*)cmd;
	d11cmd->mDeferredContext->RSSetViewports(1, &d11vp);
}

void ID11Pass::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11cmd = (ID11CommandList*)cmd;
	if (sr != nullptr)
	{
		d11cmd->mDeferredContext->RSSetScissorRects((UINT)sr->Rects.size(), (D3D11_RECT*)&sr->Rects[0]);
	}
	else
	{
		d11cmd->mDeferredContext->RSSetScissorRects(0, nullptr);
	}
}

//����Ⱦ״̬
void ID11Pass::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	((ID11RenderPipeline*)pipeline)->ApplyState(cmd);
}

void ID11Pass::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	auto d11cmd = (ID11CommandList*)cmd;
	if (VertexBuffer == nullptr)
	{
		auto tmp = (ID3D11Buffer*)VertexBuffer;
		d11cmd->mDeferredContext->IASetVertexBuffers(StreamIndex, 1, &tmp, &Stride, &Offset);
	}
	else
	{
		d11cmd->mDeferredContext->IASetVertexBuffers(StreamIndex, 1, &((ID11VertexBuffer*)VertexBuffer)->mBuffer, &Stride, &Offset);
	}
}

void ID11Pass::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11cmd = (ID11CommandList*)cmd;
	DXGI_FORMAT fmt = DXGI_FORMAT_R16_UINT;
	if (IndexBuffer->mDesc.Type == IBT_Int32)
	{
		fmt = DXGI_FORMAT_R32_UINT;
	}
	d11cmd->mDeferredContext->IASetIndexBuffer(((ID11IndexBuffer*)IndexBuffer)->mBuffer, fmt, 0);
}

void ID11Pass::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11cmd = (ID11CommandList*)cmd;
	d11cmd->mDeferredContext->VSSetConstantBuffers(Index, 1, &((ID11ConstantBuffer*)CBuffer)->mBuffer);
}

void ID11Pass::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11cmd = (ID11CommandList*)cmd;
	d11cmd->mDeferredContext->PSSetConstantBuffers(Index, 1, &((ID11ConstantBuffer*)CBuffer)->mBuffer);
}

void ID11Pass::VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* pSRV)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	if (pSRV == nullptr)
		return;

	pSRV->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
	if (pSRV->GetResourceState()->GetStreamState() != SS_Valid)
		return;

	auto d11cmd = (ID11CommandList*)cmd;
	ID3D11ShaderResourceView* pSrv = ((ID11ShaderResourceView*)pSRV)->m_pDX11SRV;
	d11cmd->mDeferredContext->VSSetShaderResources(Index, 1, &pSrv);
}

void ID11Pass::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* pSRV)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	
	if (pSRV == nullptr)
		return;

	pSRV->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
	/*if (pSRV->GetResourceState()->GetStreamState() != SS_Valid)
		return;*/

	auto d11cmd = (ID11CommandList*)cmd;
	ID3D11ShaderResourceView* pSrv = ((ID11ShaderResourceView*)pSRV)->m_pDX11SRV;
	/*if(pSrv==nullptr)
		d11cmd->mDeferredContext->PSSetShaderResources(Index, 1, &pSrv);
	else
		d11cmd->mDeferredContext->PSSetShaderResources(Index, 1, &pSrv);*/

	if (pSrv != nullptr)
		d11cmd->mDeferredContext->PSSetShaderResources(Index, 1, &pSrv);
}

void ID11Pass::VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11cmd = (ID11CommandList*)cmd;
	d11cmd->mDeferredContext->VSSetSamplers(Index, 1, &((ID11SamplerState*)Sampler)->mSampler);
}

void ID11Pass::PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11cmd = (ID11CommandList*)cmd;
	if(Sampler!=nullptr)
		d11cmd->mDeferredContext->PSSetSamplers(Index, 1, &((ID11SamplerState*)Sampler)->mSampler);
}

void ID11Pass::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	auto d11cmd = (ID11CommandList*)cmd;
	UINT dpCount = 0;
	d11cmd->mDeferredContext->IASetPrimitiveTopology(PrimitiveTypeToDX(PrimitiveType, NumPrimitives, &dpCount));

	AUTO_SAMP("Native.IPass.BuildPass.DrawPrimitive");
	if(NumInstances==1)
		d11cmd->mDeferredContext->Draw(dpCount, BaseVertexIndex);
	else
		d11cmd->mDeferredContext->DrawInstanced(dpCount, NumInstances, BaseVertexIndex, 0);
}

void ID11Pass::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	
	auto d11cmd = (ID11CommandList*)cmd;
	UINT dpCount = 0;
	d11cmd->mDeferredContext->IASetPrimitiveTopology(PrimitiveTypeToDX(PrimitiveType, NumPrimitives, &dpCount));

	AUTO_SAMP("Native.IPass.BuildPass.DrawPrimitive");
#if _DEBUG
	if (S_OK != ID11RenderContext::DefaultRenderContext->CheckContext(d11cmd->mDeferredContext))
	{
		ID3D11InputLayout* curILT;
		d11cmd->mDeferredContext->IAGetInputLayout(&curILT);
		if (curILT==nullptr)
		{
			auto d11Layout = (ID11InputLayout*)this->GetGpuProgram()->GetInputLayout();
			auto d3d11obj = d11Layout->GetInnerLayout();
			d11cmd->mDeferredContext->IASetInputLayout(d3d11obj);
		}
	}
#endif
	if (NumInstances == 1)
		d11cmd->mDeferredContext->DrawIndexed(dpCount, StartIndex, BaseVertexIndex);
	else
		d11cmd->mDeferredContext->DrawIndexedInstanced(dpCount, NumInstances, StartIndex, BaseVertexIndex, 0);
}

void ID11Pass::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	auto d11cmd = (ID11CommandList*)cmd;
	UINT dpCount = 0;
	d11cmd->mDeferredContext->IASetPrimitiveTopology(PrimitiveTypeToDX(PrimitiveType, 0, &dpCount));

	d11cmd->mDeferredContext->DrawIndexedInstancedIndirect(((ID11GpuBuffer*)pBufferForArgs)->mBuffer, AlignedByteOffsetForArgs);
}

NS_END