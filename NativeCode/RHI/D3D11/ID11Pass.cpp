#include "ID11Pass.h"
#include "ID11CommandList.h"
#include "ID11RenderPipeline.h"
#include "ID11ComputeShader.h"
#include "ID11CommandList.h"
#include "ID11VertexBuffer.h"
#include "ID11IndexBuffer.h"
#include "ID11ConstantBuffer.h"
#include "ID11ShaderResourceView.h"
#include "ID11SamplerState.h"
#include "ID11RenderContext.h"
#include "ID11InputLayout.h"
#include "ID11UnorderedAccessView.h"
#include "ID11ShaderProgram.h"
#include "ID11GpuBuffer.h"
#include "ID11Texture2D.h"
#include "../../Base/vfxSampCounter.h"

#define new VNEW

NS_BEGIN

ID11DrawCall::ID11DrawCall()
{
	 
}

ID11DrawCall::~ID11DrawCall()
{
}

void ID11DrawCall::SetViewport(ICommandList* cmd, IViewPort* vp)
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

void ID11DrawCall::SetScissorRect(ICommandList* cmd, IScissorRect* sr)
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

void ID11DrawCall::SetPipeline(ICommandList* cmd, IRenderPipeline* pipeline, EPrimitiveType dpType)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	((ID11RenderPipeline*)pipeline)->ApplyState(cmd);
}

void ID11DrawCall::SetVertexBuffer(ICommandList* cmd, UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
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

void ID11DrawCall::SetIndexBuffer(ICommandList* cmd, IIndexBuffer* IndexBuffer)
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

void ID11DrawCall::VSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11cmd = (ID11CommandList*)cmd;
	d11cmd->mDeferredContext->VSSetConstantBuffers(Index, 1, &((ID11ConstantBuffer*)CBuffer)->mBuffer);
}

void ID11DrawCall::PSSetConstantBuffer(ICommandList* cmd, UINT32 Index, IConstantBuffer* CBuffer)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	auto d11cmd = (ID11CommandList*)cmd;
	d11cmd->mDeferredContext->PSSetConstantBuffers(Index, 1, &((ID11ConstantBuffer*)CBuffer)->mBuffer);
}

void ID11DrawCall::VSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* pSRV)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	cmd->VSSetShaderResource(Index, pSRV);
}

void ID11DrawCall::PSSetShaderResource(ICommandList* cmd, UINT32 Index, IShaderResourceView* pSRV)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	
	cmd->PSSetShaderResource(Index, pSRV);
}

void ID11DrawCall::VSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	cmd->VSSetSampler(Index, Sampler);
}

void ID11DrawCall::PSSetSampler(ICommandList* cmd, UINT32 Index, ISamplerState* Sampler)
{
	/*if (cmd->IsDoing() == false)
		return;*/
	cmd->PSSetSampler(Index, Sampler);
}

void ID11DrawCall::DrawPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
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

void ID11DrawCall::DrawIndexedPrimitive(ICommandList* cmd, EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
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
			auto d11Layout = (ID11InputLayout*)this->GetPipeline()->GetGpuProgram()->GetInputLayout();
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

void ID11DrawCall::DrawIndexedInstancedIndirect(ICommandList* cmd, EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	auto d11cmd = (ID11CommandList*)cmd;
	UINT dpCount = 0;
	d11cmd->mDeferredContext->IASetPrimitiveTopology(PrimitiveTypeToDX(PrimitiveType, 0, &dpCount));

	d11cmd->mDeferredContext->DrawIndexedInstancedIndirect(((ID11GpuBuffer*)pBufferForArgs)->mBuffer, AlignedByteOffsetForArgs);
}

void ID11ComputeDrawcall::BuildPass(ICommandList* cmd)
{
	auto d11cmd = (ID11CommandList*)cmd;

	d11cmd->mDeferredContext->CSSetShader(mComputeShader.UnsafeConvertTo<ID11ComputeShader>()->mShader, nullptr, 0);

	if (mShaderCBufferBinder != nullptr)
	{
		for (auto& i : mShaderCBufferBinder->VSResources)
		{
			auto d11buffer = i.second.UnsafeConvertTo<ID11ConstantBuffer>();			
			d11buffer->UpdateDrawPass(cmd, 1);
			d11cmd->mDeferredContext->CSSetConstantBuffers(i.first, 1, &d11buffer->mBuffer);
		}
	}
	if (mShaderSrvBinder != nullptr)
	{
		for (auto& i : mShaderSrvBinder->VSResources)
		{
			auto d11buffer = i.second.UnsafeConvertTo<ID11ShaderResourceView>();
			ID3D11ShaderResourceView* pSrv = d11buffer->m_pDX11SRV;
			d11cmd->mDeferredContext->CSSetShaderResources(i.first, 1, &pSrv);
		}
	}
	if (mShaderUavBinder != nullptr)
	{
		UInt32 nUavInitialCounts = 1;
		for (auto& i : mShaderUavBinder->VSResources)
		{
			auto d11buffer = i.second.UnsafeConvertTo<ID11UnorderedAccessView>();
			d11cmd->mDeferredContext->CSSetUnorderedAccessViews(i.first, 1, &d11buffer->mView, &nUavInitialCounts);
		}
	}
	auto indirectBuffer = IndirectDrawArgsBuffer.UnsafeConvertTo<ID11GpuBuffer>();
	if (indirectBuffer != nullptr)
	{
		d11cmd->mDeferredContext->DispatchIndirect(indirectBuffer->mBuffer, IndirectDrawArgsOffset);
	}
	else
	{
		d11cmd->mDeferredContext->Dispatch(mDispatchX, mDispatchY, mDispatchZ);
	}
}

void ID11CopyDrawcall::BuildPass(ICommandList* cmd)
{
	auto d11cmd = (ID11CommandList*)cmd;
	D3D11_BOX box;
	switch (Type)
	{
	case EngineNS::ICopyDrawcall::CPTP_Unkown:
		break;
	case EngineNS::ICopyDrawcall::CPTP_Buffer:
		{
			//d11cmd->mDeferredContext->CopyResource(((ID11GpuBuffer*)BufferDesc.Target.GetPtr())->mBuffer, ((ID11GpuBuffer*)BufferDesc.Source.GetPtr())->mBuffer);
			box.left = BufferDesc.SrcOffset;
			box.top = 0;
			box.front = 0;
			box.right = BufferDesc.SrcOffset + BufferDesc.Size;
			box.bottom = 1;
			box.back = 1; 
			d11cmd->mDeferredContext->CopySubresourceRegion(((ID11GpuBuffer*)BufferDesc.Target.GetPtr())->mBuffer, 0, BufferDesc.TarOffset, 0, 0,
				((ID11GpuBuffer*)BufferDesc.Source.GetPtr())->mBuffer, 0, &box);
		}
		break;
	case EngineNS::ICopyDrawcall::CPTP_Texture2D:
		{
			//d11cmd->mDeferredContext->CopyResource(((ID11GpuBuffer*)BufferDesc.Target.GetPtr())->mBuffer, ((ID11GpuBuffer*)BufferDesc.Source.GetPtr())->mBuffer);
			box.left = Texture2DDesc.SrcX;
			box.top = Texture2DDesc.SrcY;
			box.front = 0;
			box.right = Texture2DDesc.SrcX + Texture2DDesc.Width;
			box.bottom = Texture2DDesc.SrcY + Texture2DDesc.Height;
			box.back = 1;
			d11cmd->mDeferredContext->CopySubresourceRegion(((ID11Texture2D*)Texture2DDesc.Target.GetPtr())->m_pDX11Texture2D, 0, Texture2DDesc.TarX, Texture2DDesc.TarY, 0,
				((ID11Texture2D*)Texture2DDesc.Source.GetPtr())->m_pDX11Texture2D, 0, &box);
		}
		break;
	default:
		break;
	}
}

NS_END