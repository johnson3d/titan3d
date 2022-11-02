#include "DX11CommandList.h"
#include "DX11GpuDevice.h"
#include "DX11Shader.h"
#include "DX11Buffer.h"
#include "DX11GpuState.h"
#include "DX11Event.h"
#include "DX11InputAssembly.h"
#include "DX11FrameBuffers.h"
#include "../NxDrawcall.h"
#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	DX11CommandList::DX11CommandList()
	{
		mCmdList = nullptr;
		mContext = nullptr;
		mContext4 = nullptr;
	}
	DX11CommandList::~DX11CommandList()
	{
		Safe_Release(mCmdList);
		Safe_Release(mContext4);
		Safe_Release(mContext);
	}
	bool DX11CommandList::Init(DX11GpuDevice* device)
	{
		mDevice.FromObject(device);
		auto hr = device->mDevice->CreateDeferredContext(0, &mContext);
		if (FAILED(hr))
			return false;
		mContext->QueryInterface(IID_ID3D11DeviceContext4, (void**)&mContext4);		

		FFenceDesc fcDesc{};
		mCommitFence = MakeWeakRef(device->CreateFence(&fcDesc, "CmdList Commit Fence"));
		return true;
	}
	bool DX11CommandList::Init(DX11GpuDevice* device, ID3D11DeviceContext* context)
	{
		mDevice.FromObject(device);
		mContext = context;
		context->AddRef();
		mContext->QueryInterface(IID_ID3D11DeviceContext4, (void**)&mContext4);
		return true;
	}
	bool DX11CommandList::BeginCommand()
	{
		IsRecording = true;
		Safe_Release(mCmdList);
		return true;
	}
	void DX11CommandList::EndCommand()
	{
		mContext->FinishCommandList(0, &mCmdList);
		IsRecording = false;
	}
	bool DX11CommandList::BeginPass(IFrameBuffers* fb, const FRenderPassClears* passClears, const char* name)
	{
		mDebugName = name;
		mCurrentFrameBuffers = fb;
		auto dxFB = ((DX11FrameBuffers*)fb);
		auto RTVArraySize = (UINT)dxFB->mDX11RTVArray.size();
		if (RTVArraySize > 0)
		{
			if (dxFB->mDepthStencilView != nullptr)
				mContext->OMSetRenderTargets((UINT)dxFB->mDX11RTVArray.size(), &dxFB->mDX11RTVArray[0], (ID3D11DepthStencilView*)dxFB->mDepthStencilView->GetHWBuffer());
			else
				mContext->OMSetRenderTargets((UINT)dxFB->mDX11RTVArray.size(), &dxFB->mDX11RTVArray[0], nullptr);
		}
		else
		{
			if (dxFB->mDepthStencilView != nullptr)
			{
				mContext->OMSetRenderTargets(0, nullptr, (ID3D11DepthStencilView*)dxFB->mDepthStencilView->GetHWBuffer());
			}
			else
			{
				mContext->OMSetRenderTargets(0, nullptr, nullptr);
			}
		}

		if (passClears == nullptr)
		{
			return true;
		}
		auto pRenderPassDesc = &fb->mRenderPass->Desc;

		for (UINT RTVIdx = 0; RTVIdx < RTVArraySize; RTVIdx++)
		{
			UINT flags = ((UINT)passClears->ClearFlags) & (1 << (RTVIdx + 2));
			if (flags != 0)
			{
				if (pRenderPassDesc->AttachmentMRTs[RTVIdx].LoadAction == EFrameBufferLoadAction::LoadActionClear)
				{
					mContext->ClearRenderTargetView(dxFB->mDX11RTVArray[RTVIdx], (const float*)&passClears->ClearColor[RTVIdx]);
				}
			}
		}

		if (dxFB->mDepthStencilView != nullptr)
		{
			DWORD flag = 0;
			UINT clrFlg = (UINT)(passClears->ClearFlags) & ERenderPassClearFlags::CLEAR_DEPTH;
			if (clrFlg != 0 && pRenderPassDesc->AttachmentDepthStencil.LoadAction == EFrameBufferLoadAction::LoadActionClear)
			{
				flag |= D3D11_CLEAR_DEPTH;
			}

			clrFlg = (UINT)(passClears->ClearFlags) & ERenderPassClearFlags::CLEAR_STENCIL;
			if (clrFlg != 0 && pRenderPassDesc->AttachmentDepthStencil.StencilLoadAction == EFrameBufferLoadAction::LoadActionClear)
			{
				flag |= D3D11_CLEAR_STENCIL;
			}

			if (flag != 0)
			{
				mContext->ClearDepthStencilView((ID3D11DepthStencilView*)dxFB->mDepthStencilView->GetHWBuffer(), flag, passClears->DepthClearValue, passClears->StencilClearValue);
			}
		}

		return true;
	}
	void DX11CommandList::SetViewport(UINT Num, const FViewPort* pViewports)
	{
		mContext->RSSetViewports(Num, (const D3D11_VIEWPORT*)pViewports);
	}
	void DX11CommandList::SetScissor(UINT Num, const FScissorRect* pScissor)
	{
		mContext->RSSetScissorRects(Num, (const D3D11_RECT*)pScissor);
	}
	void DX11CommandList::EndPass()
	{
		mCurrentFrameBuffers = nullptr;
	}
	void DX11CommandList::BeginEvent(const char* info)
	{
		if (IsRecording)
			return;
		auto anno = GetDX11Device()->mDefinedAnnotation;
		if (anno != nullptr)
		{
			//std::wstring_convert<std:: codecvt_utf8_utf16<wchar_t>> converter;
			////std::string str = converter.to_bytes(L"Hello world");
			//std::wstring wstr = converter.from_bytes(info);
			std::string str = info;
			std::wstring wstr(str.begin(), str.end());
			anno->BeginEvent(wstr.c_str());
		}
	}
	void DX11CommandList::EndEvent()
	{
		if (IsRecording)
			return;
		auto anno = GetDX11Device()->mDefinedAnnotation;
		if (anno != nullptr)
		{
			anno->EndEvent();
		}
	}
	void DX11CommandList::SetShader(IShader* shader)
	{
		switch (shader->Desc->Type)
		{
			case EShaderType::SDT_ComputeShader:
			{
				auto d11CSShader = (DX11Shader*)shader;
				mContext->CSSetShader(d11CSShader->mComputeShader, nullptr, 0);
			}
			break;
			default:
				break;
		}
	}
	void DX11CommandList::SetCBV(EShaderType type, const FShaderBinder* binder, ICbView* buffer)
	{
		//ASSERT(binder->Space == type);
		buffer->FlushDirty(this);
		switch (type)
		{
			case EShaderType::SDT_ComputeShader:
			{
				auto d11View = buffer->Buffer.UnsafeConvertTo<DX11Buffer>();
				auto pSrv = d11View->mBuffer;
				mContext->CSSetConstantBuffers(binder->Slot, 1, &pSrv);
			}
			break;
			case EShaderType::SDT_VertexShader:
			{
				auto d11View = buffer->Buffer.UnsafeConvertTo<DX11Buffer>();
				auto pSrv = d11View->mBuffer;
				mContext->VSSetConstantBuffers(binder->Slot, 1, &pSrv);
			}
			break;
			case EShaderType::SDT_PixelShader:
			{
				auto d11View = buffer->Buffer.UnsafeConvertTo<DX11Buffer>();
				auto pSrv = d11View->mBuffer;
				mContext->PSSetConstantBuffers(binder->Slot, 1, &pSrv);
			}
			break;
		}
	}
	void DX11CommandList::SetSrv(EShaderType type, const FShaderBinder* binder, ISrView* view)
	{
		view->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
		switch (type)
		{
			case EShaderType::SDT_ComputeShader:
			{
				auto d11View = (DX11SrView*)view;
				ID3D11ShaderResourceView* pSrv = d11View->mView;
				mContext->CSSetShaderResources(binder->Slot, 1, &pSrv);
			}
			break;
			case EShaderType::SDT_VertexShader:
			{
				auto d11View = (DX11SrView*)view;
				ID3D11ShaderResourceView* pSrv = d11View->mView;
				mContext->VSSetShaderResources(binder->Slot, 1, &pSrv);
			}
			break;
			case EShaderType::SDT_PixelShader:
			{
				auto d11View = (DX11SrView*)view;
				ID3D11ShaderResourceView* pSrv = d11View->mView;
				mContext->PSSetShaderResources(binder->Slot, 1, &pSrv);
			}
			break;
		}
	}
	void DX11CommandList::SetUav(EShaderType type, const FShaderBinder* binder, IUaView* view)
	{
		UINT nUavInitialCounts = 1;
		switch (type)
		{
			case EShaderType::SDT_ComputeShader:
			{
				auto d11View = (DX11UaView*)view;
				auto pSrv = d11View->mView;
				mContext->CSSetUnorderedAccessViews(binder->Slot, 1, &pSrv, &nUavInitialCounts);
			}
			break;
			case EShaderType::SDT_VertexShader:
			{

			}
			break;
			case EShaderType::SDT_PixelShader:
			{

			}
			break;
		}
	}
	void DX11CommandList::SetSampler(EShaderType type, const FShaderBinder* binder, ISampler* sampler)
	{
		switch (type)
		{
		case EShaderType::SDT_ComputeShader:
		{
			auto d11Sampler = (DX11Sampler*)sampler;
			mContext->CSSetSamplers(binder->Slot, 1, &d11Sampler->mState);
		}
		break;
		case EShaderType::SDT_VertexShader:
		{
			auto d11Sampler = (DX11Sampler*)sampler;
			mContext->VSSetSamplers(binder->Slot, 1, &d11Sampler->mState);
		}
		break;
		case EShaderType::SDT_PixelShader:
		{
			auto d11Sampler = (DX11Sampler*)sampler;
			mContext->PSSetSamplers(binder->Slot, 1, &d11Sampler->mState);
		}
		break;
		}
	}
	void DX11CommandList::SetVertexBuffer(UINT slot, IVbView* buffer, UINT32 Offset, UINT Stride)
	{
		if (buffer == nullptr)
		{
			auto tmp = (ID3D11Buffer*)nullptr;
			mContext->IASetVertexBuffers(slot, 1, &tmp, &Stride, &Offset);
		}
		else
		{
			mContext->IASetVertexBuffers(slot, 1, &buffer->Buffer.UnsafeConvertTo<DX11Buffer>()->mBuffer, &Stride, &Offset);
		}
	}
	void DX11CommandList::SetIndexBuffer(IIbView* buffer, bool IsBit32)
	{
		DXGI_FORMAT fmt = DXGI_FORMAT_R16_UINT;
		if (IsBit32)
		{
			fmt = DXGI_FORMAT_R32_UINT;
		}
		if (buffer == nullptr)
		{
			mContext->IASetIndexBuffer(nullptr, fmt, 0);
		}
		else
		{
			mContext->IASetIndexBuffer(buffer->Buffer.UnsafeConvertTo<DX11Buffer>()->mBuffer, fmt, 0);
		}
	}
	void DX11CommandList::SetGraphicsPipeline(const IGpuDrawState* drawState)
	{
		auto dx11State = (DX11GpuPipeline*)drawState->Pipeline.GetPtr();
		if (drawState->ShaderEffect->mInputLayout != nullptr)
			SetInputLayout(drawState->ShaderEffect->mInputLayout);
		auto vs = ((DX11Shader*)drawState->ShaderEffect->GetVS());
		if (vs != nullptr)
			mContext->VSSetShader(vs->mVertexShader, nullptr, 0);
		auto ps = ((DX11Shader*)drawState->ShaderEffect->GetPS());
		if (ps != nullptr)
			mContext->PSSetShader(ps->mPixelShader, nullptr, 0);
		mContext->RSSetState(dx11State->mRasterState);
		mContext->OMSetDepthStencilState(dx11State->mDepthStencilState, drawState->Pipeline->Desc.StencilRef);
		mContext->OMSetBlendState(dx11State->mBlendState, drawState->Pipeline->Desc.BlendFactors, drawState->Pipeline->Desc.SampleMask);
	}
	void DX11CommandList::SetComputePipeline(const IComputeEffect* drawState)
	{

	}
	void DX11CommandList::SetInputLayout(IInputLayout* layout)
	{
		mContext->IASetInputLayout(((DX11InputLayout*)layout)->mLayout);
	}
	inline D3D_PRIMITIVE_TOPOLOGY PrimitiveTypeToDX(EPrimitiveType type, UINT NumPrimitives, UINT* pCount)
	{
		switch (type)
		{
		case EPT_LineStrip:
			*pCount = NumPrimitives;
			return D3D11_PRIMITIVE_TOPOLOGY_LINESTRIP;
		case EPT_LineList:
			*pCount = NumPrimitives * 2;
			return D3D11_PRIMITIVE_TOPOLOGY_LINELIST;
		case EPT_TriangleList:
			*pCount = NumPrimitives * 3;
			return D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
		case EPT_TriangleStrip:
			*pCount = NumPrimitives + 2;
			return D3D11_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP;
		}
		return D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST;
	}

	void DX11CommandList::Draw(EPrimitiveType topology, UINT BaseVertex, UINT DrawCount, UINT Instance)
	{
		UINT dpCount = 0;
		mContext->IASetPrimitiveTopology(PrimitiveTypeToDX(topology, DrawCount, &dpCount));
		if (Instance == 1)
			mContext->Draw(dpCount, BaseVertex);
		else
			mContext->DrawInstanced(dpCount, Instance, BaseVertex, 0);
	}
	void DX11CommandList::DrawIndexed(EPrimitiveType topology, UINT BaseVertex, UINT StartIndex, UINT DrawCount, UINT Instance)
	{
		UINT dpCount = 0;
		mContext->IASetPrimitiveTopology(PrimitiveTypeToDX(topology, DrawCount, &dpCount));
		if (Instance == 1)
			mContext->DrawIndexed(dpCount, StartIndex, BaseVertex);
		else
			mContext->DrawIndexedInstanced(dpCount, Instance, StartIndex, BaseVertex, 0);
	}
	void DX11CommandList::IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset)
	{
		UINT dpCount = 0;
		mContext->IASetPrimitiveTopology(PrimitiveTypeToDX(topology, 0, &dpCount));

		mContext->DrawIndexedInstancedIndirect(((DX11Buffer*)indirectArg)->mBuffer, indirectArgOffset);
	}
	void DX11CommandList::Dispatch(UINT x, UINT y, UINT z)
	{
		mContext->Dispatch(x, y, z);
	}
	void DX11CommandList::IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset)
	{
		mContext->DispatchIndirect(((DX11Buffer*)indirectArg)->mBuffer, indirectArgOffset);
	}
	void DX11CommandList::SetMemoryBarrier(EPipelineStage srcStage, EPipelineStage dstStage, EBarrierAccess srcAccess, EBarrierAccess dstAccess)
	{

	}
	void DX11CommandList::SetBufferBarrier(IBuffer* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess)
	{

	}
	void DX11CommandList::SetTextureBarrier(ITexture* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess)
	{

	}
	/*UINT64 DX11CommandList::SignalFence(IFence* fence, UINT64 value, IEvent* evt)
	{
		ASSERT(mContext4);
		if (mContext4 == nullptr)
			return -1;
		auto dxFence = ((DX11Fence*)fence);
		if (evt != nullptr)
		{
			dxFence->mFence->SetEventOnCompletion(value, ((DX11Event*)evt)->mHandle);
		}
		else
		{
			dxFence->mFence->SetEventOnCompletion(value, dxFence->mEvent->mHandle);
		}
		mContext4->Signal(dxFence->mFence, value);
		return value;
	}
	void DX11CommandList::WaitGpuFence(IFence* fence, UINT64 value)
	{
		ASSERT(mContext4);
		if (mContext4 == nullptr)
			return;
		auto dxFence = ((DX11Fence*)fence);
		mContext4->Wait(dxFence->mFence, value);
	}*/
	void DX11CommandList::CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size)
	{
		if (target->GetRtti() != src->GetRtti())
		{
			ASSERT(false);
			return;
		}
		if (Size == 0 && DstOffset == 0 && SrcOffset == 0)
		{
			mContext->CopyResource((ID3D11Resource*)target->GetHWBuffer(), (ID3D11Resource*)src->GetHWBuffer());
			return;
		}
		D3D11_BOX box{};
		box.left = (UINT)SrcOffset;
		box.right = (UINT)(SrcOffset + Size);
		mContext->CopySubresourceRegion((ID3D11Resource*)target->GetHWBuffer(), 0, (UINT)DstOffset, 0, 0, (ID3D11Resource*)src->GetHWBuffer(), 0, &box);
	}
	void DX11CommandList::CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* src, UINT srcSubRes, const FSubresourceBox* box)
	{
		mContext->CopySubresourceRegion((ID3D11Resource*)target->GetHWBuffer(), tarSubRes, DstX, DstY, DstZ, (ID3D11Resource*)src->GetHWBuffer(), srcSubRes, (D3D11_BOX*)box);
	}
	void DX11CommandList::CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint)
	{
		ASSERT(false);
	}
	void DX11CommandList::CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* source, UINT subRes)
	{
		D3D11_BOX box;
		box.left = footprint->X;
		box.top = footprint->Y;
		box.front = footprint->Z;
		box.right = box.left + footprint->Width;
		box.bottom = box.top + footprint->Height;
		box.back = box.front + footprint->Depth;
		mContext->CopySubresourceRegion((ID3D11Resource*)target->GetHWBuffer(), subRes, footprint->X, footprint->Y, footprint->Z, (ID3D11Resource*)source->GetHWBuffer(), subRes, &box);
	}
	void DX11CommandList::Flush()
	{
		mContext->Flush();
	}
	void DX11CommandList::Commit(ID3D11DeviceContext* imContex)
	{
		if (mCmdList == nullptr)
		{
			return;
		}
		BeginEvent(mDebugName.c_str());
		imContex->ExecuteCommandList(mCmdList, 0);
		Safe_Release(mCmdList);
		EndEvent();
	}
}

NS_END