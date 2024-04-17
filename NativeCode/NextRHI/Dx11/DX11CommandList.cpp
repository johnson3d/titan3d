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
	void DX11CmdRecorder::ResetGpuDraws()
	{
		ICmdRecorder::ResetGpuDraws();
		mCmdList = nullptr;
	}
	DX11CommandList::DX11CommandList()
	{
		mContext = nullptr;
		mContext4 = nullptr;
	}
	DX11CommandList::~DX11CommandList()
	{
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
	ICmdRecorder* DX11CommandList::BeginCommand()
	{
		mIsRecording = true;
		if (mCmdRecorder == nullptr)
		{
			mCmdRecorder = MakeWeakRef(new DX11CmdRecorder());
		}
		mCmdRecorder->ResetGpuDraws();
		mPrimitiveNum = 0;
		return mCmdRecorder;
	}
	void DX11CommandList::EndCommand()
	{
		ICommandList::EndCommand();

		mContext->FinishCommandList(0, GetDX11CmdRecorder()->mCmdList.GetAddressOf());
		mIsRecording = false;
	}
	void DX11CommandList::Commit(ID3D11DeviceContext* imContex)
	{
		if (GetDX11CmdRecorder() == nullptr/* || GetDX11CmdRecorder()->GetDrawcallNumber() == 0*/)
		{
			return;
		}
		BeginEvent(mDebugName.c_str());
		imContex->ExecuteCommandList(GetDX11CmdRecorder()->mCmdList, 0);
		EndEvent();
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
					if (dxFB->mDX11RTVArray[RTVIdx] != nullptr)
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
		if (mIsRecording)
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
		if (mIsRecording)
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
		//buffer->FlushDirty();
		//buffer->Buffer->FlushDirty(this);

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
		if (view == nullptr)
			return;
		view->GetResourceState()->SetAccessFrame(IWeakReference::EngineCurrentFrame);
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
				ID3D11UnorderedAccessView* pSrv = nullptr;
				if (d11View != nullptr)
				{
					pSrv = d11View->mView;
				}
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
		Offset = Offset + (UINT)buffer->Desc.Offset;
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
			mContext->IASetIndexBuffer(buffer->Buffer.UnsafeConvertTo<DX11Buffer>()->mBuffer, fmt, (UINT)buffer->Desc.Offset);
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
	void DX11CommandList::IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset, IBuffer* countBuffer)
	{
		UINT dpCount = 0;
		mContext->IASetPrimitiveTopology(PrimitiveTypeToDX(topology, 0, &dpCount));

		ASSERT(countBuffer == nullptr);
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
		box.top = 0;
		box.bottom = 1;
		box.front = 0;
		box.back = 1;
		mContext->CopySubresourceRegion((ID3D11Resource*)target->GetHWBuffer(), 0, (UINT)DstOffset, 0, 0, (ID3D11Resource*)src->GetHWBuffer(), 0, &box);
	}
	void DX11CommandList::CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* src, UINT srcSubRes, const FSubresourceBox* box)
	{
		mContext->CopySubresourceRegion((ID3D11Resource*)target->GetHWBuffer(), tarSubRes, DstX, DstY, DstZ, (ID3D11Resource*)src->GetHWBuffer(), srcSubRes, (D3D11_BOX*)box);
	}
	void DX11CommandList::CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint)
	{
		auto pDevice = GetDX11Device();
		auto cmd = ((DX11CmdQueue*)pDevice->GetCmdQueue())->mHardwareContext;

		AutoRef<NxRHI::IBuffer> copyBuffer;
		if ((src->Desc.CpuAccess & ECpuAccess::CAS_READ)&&
			src->Desc.Usage == EGpuUsage::USAGE_STAGING)
		{
			copyBuffer = src;
		}
		else
		{
			auto copyDesc = src->Desc;
			copyDesc.CpuAccess = ECpuAccess::CAS_READ;
			copyDesc.Usage = EGpuUsage::USAGE_STAGING;
			copyDesc.MiscFlags = (EResourceMiscFlag)0;

			copyBuffer = MakeWeakRef(pDevice->CreateBuffer(&copyDesc));
			auto hwCmd = ((DX11CmdQueue*)pDevice->GetCmdQueue())->mHardwareContext;
			hwCmd->mContext->CopyResource((ID3D11Resource*)copyBuffer->GetHWBuffer(), (ID3D11Resource*)src->GetHWBuffer());
		}
		
		auto pixelStride = GetPixelByteWidth(footprint->Format);
		FMappedSubResource lockBuffer{};
		FMappedSubResource lockTexture{};
		if (copyBuffer->Map(0, &lockBuffer, true))
		{
			NxRHI::FSubresourceBox box;
			box.Left = footprint->X;
			box.Top = footprint->Y;
			box.Front = footprint->Z;
			box.Right = footprint->X + footprint->Width;
			box.Bottom = footprint->Y + footprint->Height;
			box.Back = footprint->Z + footprint->Depth;
			target->UpdateGpuData(this, subRes, lockBuffer.pData, footprint);
			/*if (target->Map(cmd, subRes, &lockTexture, false))
			{
				for (UINT z = 0; z < footprint->Depth; z++)
				{
					auto pSliceStart = (BYTE*)lockTexture.pData + lockTexture.DepthPitch * (z + footprint->Z);
					auto pBufferStart = (BYTE*)lockBuffer.pData + footprint->RowPitch * footprint->Height * z;
					for (UINT y = 0; y < footprint->Height; y++)
					{
						auto pRowStart = pSliceStart +
							(y + footprint->Y) * lockTexture.RowPitch +
							footprint->X * pixelStride;
						memcpy(pRowStart, pBufferStart, pixelStride * footprint->Width);

						pBufferStart += footprint->RowPitch;
					}
				}

				target->Unmap(cmd, subRes);
			}*/
			copyBuffer->Unmap(0);
		}
	}
	void DX11CommandList::CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* source, UINT subRes)
	{
		auto copyDesc = source->Desc;
		copyDesc.CpuAccess = ECpuAccess::CAS_READ;
		copyDesc.Usage = EGpuUsage::USAGE_STAGING;
		copyDesc.BindFlags = EBufferType(0);
		copyDesc.InitData = nullptr;
		
		auto pDevice = ((DX11Texture*)source)->mDeviceRef.GetPtr();
		auto copyTexture = MakeWeakRef(pDevice->CreateTexture(&copyDesc));

		auto cmd = ((DX11CmdQueue*)pDevice->GetCmdQueue())->mHardwareContext;
		cmd->mContext->CopyResource((ID3D11Resource*)copyTexture->GetHWBuffer(), (ID3D11Resource*)source->GetHWBuffer());
		/*if (IsImmContext == false)
		{
			pDevice->GetCmdQueue()->ExecuteCommandList(this, EQueueType::QU_Default);
		}*/
		
		auto pixelStride = GetPixelByteWidth(footprint->Format);
		FMappedSubResource lockTexture{};
		if (copyTexture->Map(subRes, &lockTexture, true))
		{
			/*for (UINT z = 0; z < footprint->Depth; z++)
			{
				auto pSliceStart = (BYTE*)lockTexture.pData + lockTexture.DepthPitch * (z + footprint->Z);
				for (UINT y = 0; y < footprint->Height; y++)
				{
					auto pRowStart = pSliceStart + lockTexture.RowPitch * (y + footprint->Y);
					for (UINT x = 0; x < footprint->Width; x++)
					{
						if (((DWORD*)pRowStart)[x] != 0)
						{
							int xxx = ((DWORD*)pRowStart)[x];
						}
					}
				}
			}*/
			FMappedSubResource lockBuffer{};
			if (target->Map(0, &lockBuffer, false))
			{
				for (UINT z = 0; z < footprint->Depth; z++)
				{
					auto pSliceStart = (BYTE*)lockTexture.pData + lockTexture.DepthPitch * (z + footprint->Z);
					auto pWrite = (BYTE*)lockBuffer.pData + footprint->RowPitch * footprint->Height * z;
					for (UINT y = 0; y < footprint->Height; y++)
					{
						auto pRowStart = pSliceStart +
							(y + footprint->Y) * lockTexture.RowPitch +
							footprint->X * pixelStride;
						memcpy(pWrite, pRowStart, pixelStride * footprint->Width);

						pWrite += footprint->RowPitch;
					}
				}

				/*for (UINT z = 0; z < footprint->Depth; z++)
				{
					auto pSliceStart = (BYTE*)lockBuffer.pData + footprint->RowPitch * footprint->Height * z;
					for (UINT y = 0; y < footprint->Height; y++)
					{
						auto pRowStart = pSliceStart + lockBuffer.RowPitch * y;
						for (UINT x = 0; x < footprint->Width; x++)
						{
							if (((DWORD*)pRowStart)[x] != 0)
							{
								int xxx = ((DWORD*)pRowStart)[x];
							}
						}
					}
				}*/

				target->Unmap(0);
			}
			copyTexture->Unmap(subRes);
		}
		/*D3D11_BOX box;
		box.left = footprint->X;
		box.top = footprint->Y;
		box.front = footprint->Z;
		box.right = box.left + footprint->Width;
		box.bottom = box.top + footprint->Height;
		box.back = box.front + footprint->Depth;
		mContext->CopySubresourceRegion((ID3D11Resource*)copyTexture->GetHWBuffer(), subRes, 0, 0, 0, (ID3D11Resource*)source->GetHWBuffer(), subRes, &box);*/
	}

	void DX11CommandList::WriteBufferUINT32(UINT Count, FBufferWriter* BufferWriters)
	{
		if (Count == 0)
			return;
		FBufferDesc bfDesc{};
		bfDesc.SetDefault();
		bfDesc.Usage = EGpuUsage::USAGE_STAGING;
		bfDesc.CpuAccess = ECpuAccess::CAS_WRITE;
		bfDesc.Type = EBufferType::BFT_NONE;
		bfDesc.Size = Count * sizeof(UINT);
		bfDesc.RowPitch = bfDesc.Size;
		bfDesc.DepthPitch = bfDesc.Size;
		auto ptr = (UINT*)alloca(sizeof(UINT) * Count); 
		for (UINT i = 0; i < Count; i++)
		{
			ptr[i] = BufferWriters[i].Value;
		}

		bfDesc.InitData = ptr;
		auto copyBuffer = MakeWeakRef(GetDX11Device()->CreateBuffer(&bfDesc));
		/*FMappedSubResource mapped{};
		if (copyBuffer->Map(0, &mapped, false))
		{
			auto ptr = (UINT*)mapped.pData;
			for (UINT i = 0; i < Count; i++)
			{
				ptr[i] = BufferWriters[i].Value;
			}
			copyBuffer->Unmap(0);
		}*/
		for (UINT i = 0; i < Count; i++)
		{
			CopyBufferRegion(BufferWriters[i].Buffer, BufferWriters[i].Offset, copyBuffer, i * sizeof(UINT), sizeof(UINT));
		}
	}

	//////////////////////////////////////////////////////////////////////////
	DX11GpuScope::~DX11GpuScope()
	{
		
	}
	bool DX11GpuScope::Init(DX11GpuDevice* device)
	{
		mDeviceRef.FromObject(device);

		D3D11_QUERY_DESC queryDesc{};
		queryDesc.Query = D3D11_QUERY::D3D11_QUERY_TIMESTAMP;
		
		device->mDevice->CreateQuery(&queryDesc, mQueryStart.GetAddressOf());
		device->mDevice->CreateQuery(&queryDesc, mQueryEnd.GetAddressOf());

		queryDesc.Query = D3D11_QUERY::D3D11_QUERY_TIMESTAMP_DISJOINT;
		device->mDevice->CreateQuery(&queryDesc, mQueryJoint.GetAddressOf());
		return true;
	}

	bool DX11GpuScope::IsFinished()
	{
		return true;
	}
	UINT64 DX11GpuScope::GetDeltaTime()
	{
		/*auto device = mDeviceRef.GetPtr();
		UINT64 start = 0,end = 0;
		D3D11_QUERY_DATA_TIMESTAMP_DISJOINT disjoint = { 1, TRUE };
		auto context = ((DX11CmdQueue*)device->GetCmdQueue())->mHardwareContext->mContext;
		HRESULT hr = S_OK;
		while ((hr = context->GetData(mQueryJoint, &disjoint, sizeof(disjoint), 0)) == S_FALSE)
		{
		}
		device->GetCmdQueue()->mDefaultQueueFrequence = disjoint.Frequency;
		while ((hr = context->GetData(mQueryStart, &start, sizeof(UINT64), 0)) == S_FALSE)
		{
		}
		while ((hr = context->GetData(mQueryEnd, &end, sizeof(UINT64), 0)) == S_FALSE)
		{
		}
		return end - start;*/
		return 0;
	}
	void DX11GpuScope::Begin(ICommandList* cmdlist)
	{
		auto cmd = (DX11CommandList*)cmdlist;
		cmd->mContext->Begin(mQueryJoint);
		cmd->mContext->End(mQueryStart);
	}
	void DX11GpuScope::End(ICommandList* cmdlist)
	{
		auto cmd = (DX11CommandList*)cmdlist;
		cmd->mContext->End(mQueryEnd);
		cmd->mContext->End(mQueryJoint);
	}
	void DX11GpuScope::SetName(const char* name)
	{
		mName = name;
		//std::wstring n = StringHelper::strtowstr(name);
		//mQueryHeap->SetName(n.c_str());
	}
}

NS_END