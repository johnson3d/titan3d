#include "ID11CommandList.h"
#include "ID11Pass.h"
#include "ID11RenderContext.h"
#include "ID11RenderTargetView.h"
#include "ID11DepthStencilView.h"
#include "ID11ShaderResourceView.h"
#include "ID11UnorderedAccessView.h"
#include "ID11ConstantBuffer.h"
#include "ID11FrameBuffers.h"
#include "ID11ComputeShader.h"
#include "ID11SwapChain.h"
#include "ID11RasterizerState.h"
#include "ID11DepthStencilState.h"
#include "ID11BlendState.h"
#include "ID11Texture2D.h"
#include "ID11SamplerState.h"
#include "ID11VertexBuffer.h"
#include "ID11IndexBuffer.h"
#include "ID11InputLayout.h"
#include "ID11ShaderProgram.h"
#include "ID11VertexShader.h"
#include "ID11PixelShader.h"
#include "ID11RenderPipeline.h"
#include "ID11Fence.h"
#include "../Utility/GraphicsProfiler.h"
#include "../../Base/vfxsampcounter.h"

#define new VNEW

NS_BEGIN




ID11CommandList::ID11CommandList()
{
	mCmdList = nullptr;
	mDeferredContext = nullptr;
	mDSView = nullptr;
}


ID11CommandList::~ID11CommandList()
{
	for (auto i : mDX11RTVArray)
	{
		i->Release();
	}
	mDX11RTVArray.clear();
	Safe_Release(mDSView);
	Safe_Release(mCmdList);
	Safe_Release(mDeferredContext);

}

void ID11CommandList::BeginCommand()
{
	//this->mCurrentState.Reset();
	ICommandList::BeginCommand();
}

void ID11CommandList::EndCommand()
{
	Safe_Release(mCmdList);
	mDeferredContext->FinishCommandList(FALSE, &mCmdList);
#ifdef _DEBUG
	if (mDebugName.length() > 0)
	{
		mCmdList->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)mDebugName.length(), mDebugName.c_str());
	}
#endif

	ICommandList::EndCommand();
}

void ID11CommandList::BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer)
{
	if (mProfiler != nullptr && mProfiler->mNoPixelWrite)
	{
		pFrameBuffer = mProfiler->mOnePixelFB;
	}

	UINT32 RTVIdx = 0;
	UINT32 RTVArraySize = (UINT32)mDX11RTVArray.size();

	for (RTVIdx = 0; RTVIdx < RTVArraySize; RTVIdx++)
	{
		mDX11RTVArray[RTVIdx]->Release();
	}
	mDX11RTVArray.clear();

	for (RTVIdx = 0; RTVIdx < MAX_MRT_NUM; RTVIdx++)
	{
		auto refRTV = (ID11RenderTargetView*)pFrameBuffer->mRenderTargets[RTVIdx];
		if (refRTV == nullptr || refRTV->m_pDX11RTV == nullptr)
		{
			break;
		}
		refRTV->m_pDX11RTV->AddRef();
		mDX11RTVArray.push_back(refRTV->m_pDX11RTV);
	}
	RTVArraySize = (UINT32)mDX11RTVArray.size();
	Safe_Release(mDSView);

	if (RTVArraySize == 0)
	{
		ID3D11RenderTargetView* pNullRTV = NULL;
		if (pFrameBuffer->m_pDepthStencilView == nullptr)
		{
			mDeferredContext->OMSetRenderTargets(1, &pNullRTV, NULL);
		}
		else
		{
			mDSView = ((ID11DepthStencilView*)pFrameBuffer->m_pDepthStencilView)->m_pDX11DSV;
			if (mDSView)
			{
				mDSView->AddRef();
			}
			mDeferredContext->OMSetRenderTargets(1, &pNullRTV, mDSView);
		}
	}
	else
	{
		if (pFrameBuffer->m_pDepthStencilView == NULL)
		{
			mDeferredContext->OMSetRenderTargets(RTVArraySize, &mDX11RTVArray[0], NULL);
		}
		else
		{
			mDSView = ((ID11DepthStencilView*)pFrameBuffer->m_pDepthStencilView)->m_pDX11DSV;
			if (mDSView)
			{
				mDSView->AddRef();
			}
			mDeferredContext->OMSetRenderTargets(RTVArraySize, &mDX11RTVArray[0], mDSView);
		}
	}

	

	v3dxColor4 RTVClearColorArray[MAX_MRT_NUM] = {
		pRenderPassDesc->mFBClearColorRT0,
		pRenderPassDesc->mFBClearColorRT1,
		pRenderPassDesc->mFBClearColorRT2,
		pRenderPassDesc->mFBClearColorRT3,
		pRenderPassDesc->mFBClearColorRT4,
		pRenderPassDesc->mFBClearColorRT5,
		pRenderPassDesc->mFBClearColorRT6,
		pRenderPassDesc->mFBClearColorRT7
	};
	float ClearColor[4];
	for (RTVIdx = 0; RTVIdx < RTVArraySize; RTVIdx++)
	{
		if (pRenderPassDesc->mFBLoadAction_Color == FrameBufferLoadAction::LoadActionClear)
		{
			ClearColor[0] = RTVClearColorArray[RTVIdx].r;
			ClearColor[1] = RTVClearColorArray[RTVIdx].g;
			ClearColor[2] = RTVClearColorArray[RTVIdx].b;
			ClearColor[3] = RTVClearColorArray[RTVIdx].a;

			mDeferredContext->ClearRenderTargetView(mDX11RTVArray[RTVIdx], ClearColor);
		}
	}

	if (mDSView != nullptr)
	{
		DWORD flag = 0;
		if (pRenderPassDesc->mFBLoadAction_Depth == FrameBufferLoadAction::LoadActionClear)
		{
			flag |= D3D11_CLEAR_DEPTH;	
		}

		if (pRenderPassDesc->mFBLoadAction_Stencil == FrameBufferLoadAction::LoadActionClear)
		{
			flag |= D3D11_CLEAR_STENCIL;
		}

		mDeferredContext->ClearDepthStencilView(mDSView, flag, pRenderPassDesc->mDepthClearValue, pRenderPassDesc->mStencilClearValue);
	}
}

void ID11CommandList::EndRenderPass()
{
	mDrawCall = 0;
	mDrawTriangle = 0;
	ICommandList::EndRenderPass();
}


void ID11CommandList::Commit(IRenderContext* pRHICtx)
{
	if (mCmdList == nullptr || pRHICtx == NULL)
	{
		return;
	}
		
	{
		VAutoLock(((ID11RenderContext*)pRHICtx)->mHWContextLocker);
		auto context = ((ID11RenderContext*)pRHICtx)->mHardwareContext;
		
		context->ExecuteCommandList(mCmdList, FALSE);

		for (auto& i : mSignals)
		{
			context->End(i->mQuery);
			i->mContext = context;
		}
		mSignals.clear();
	}
	
	this->mCurrentState.Reset();
}

void ID11CommandList::SetRasterizerState(IRasterizerState* State)
{
	mDeferredContext->RSSetState(((ID11RasterizerState*)State)->mState);
}

void ID11CommandList::SetDepthStencilState(IDepthStencilState* State)
{
	mDeferredContext->OMSetDepthStencilState(((ID11DepthStencilState*)State)->mState, State->GetDescPtr()->StencilRef);
}

void ID11CommandList::SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask)
{
	mDeferredContext->OMSetBlendState(((ID11BlendState*)State)->mState, blendFactor, samplerMask);
}

bool ID11CommandList::Init(ID11RenderContext* rc, const ICommandListDesc* desc)
{
	auto hr = rc->mDevice->CreateDeferredContext(0, &mDeferredContext);
	if (FAILED(hr))
		return false;
	
	mRHIContext.FromObject(rc);

	return true;
}

bool ID11CommandList::InitD11Point(ID11RenderContext* rc, ID3D11DeviceContext* context)
{
	mRHIContext.FromObject(rc);

	context->AddRef();
	mDeferredContext = context;

	return true;
}

void ID11CommandList::SetComputeShader(IComputeShader* ComputerShader)
{
	if (ComputerShader == nullptr)
	{
		mDeferredContext->CSSetShader(nullptr, nullptr, 0);
		return;
	}
	auto d11CSShader = (ID11ComputeShader*)ComputerShader;
	mDeferredContext->CSSetShader(d11CSShader->mShader, nullptr, 0);
}

void ID11CommandList::CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
{
	auto d11View = (ID11ShaderResourceView*)Texture;
	ID3D11ShaderResourceView* pSrv = d11View->m_pDX11SRV;
	mDeferredContext->CSSetShaderResources(Index, 1, &pSrv);
}

void ID11CommandList::CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT *pUAVInitialCounts)
{
	auto d11View = (ID11UnorderedAccessView*)view;
	if (view == nullptr)
	{
		ID3D11UnorderedAccessView* nuneSrv = nullptr;
		mDeferredContext->CSSetUnorderedAccessViews(Index, 1, &nuneSrv, pUAVInitialCounts);
		return;
	}
	mDeferredContext->CSSetUnorderedAccessViews(Index, 1, &d11View->mView, pUAVInitialCounts);
}

void ID11CommandList::CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer)
{
	auto d11CB = (ID11ConstantBuffer*)cbuffer;
	mDeferredContext->CSSetConstantBuffers(Index, 1, &d11CB->mBuffer);
}

void ID11CommandList::CSDispatch(UINT x, UINT y, UINT z)
{
	mDeferredContext->Dispatch(x, y, z);
}

void ID11CommandList::PSSetShaderResource(UINT32 Index, IShaderResourceView* Texture)
{
	if (Texture == nullptr)
		return;

	Texture->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);

	ID3D11ShaderResourceView* pSrv = ((ID11ShaderResourceView*)Texture)->m_pDX11SRV;

	if (pSrv != nullptr)
		mDeferredContext->PSSetShaderResources(Index, 1, &pSrv);
}

void ID11CommandList::PSSetSampler(UINT32 Index, ISamplerState* Sampler)
{
	if (Sampler != nullptr)
		mDeferredContext->PSSetSamplers(Index, 1, &((ID11SamplerState*)Sampler)->mSampler);
}

void ID11CommandList::SetScissorRect(IScissorRect* sr)
{
	if (sr != nullptr && sr->Rects.size() > 0)
	{
		mDeferredContext->RSSetScissorRects((UINT)sr->Rects.size(), (D3D11_RECT*)&sr->Rects[0]);
	}
	else
	{
		mDeferredContext->RSSetScissorRects(0, nullptr);
	}
}

void ID11CommandList::SetVertexBuffer(UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride)
{
	if (VertexBuffer == nullptr)
	{
		auto tmp = (ID3D11Buffer*)VertexBuffer;
		mDeferredContext->IASetVertexBuffers(StreamIndex, 1, &tmp, &Stride, &Offset);
	}
	else
	{
		mDeferredContext->IASetVertexBuffers(StreamIndex, 1, &((ID11VertexBuffer*)VertexBuffer)->mBuffer, &Stride, &Offset);
	}
}

void ID11CommandList::SetIndexBuffer(IIndexBuffer* IndexBuffer)
{
	DXGI_FORMAT fmt = DXGI_FORMAT_R16_UINT;
	if (IndexBuffer->mDesc.Type == IBT_Int32)
	{
		fmt = DXGI_FORMAT_R32_UINT;
	}
	mDeferredContext->IASetIndexBuffer(((ID11IndexBuffer*)IndexBuffer)->mBuffer, fmt, 0);
}

void ID11CommandList::DrawPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	UINT dpCount = 0;
	mDeferredContext->IASetPrimitiveTopology(PrimitiveTypeToDX(PrimitiveType, NumPrimitives, &dpCount));

	AUTO_SAMP("Native.IPass.BuildPass.DrawPrimitive");
	if (NumInstances == 1)
		mDeferredContext->Draw(dpCount, BaseVertexIndex);
	else
		mDeferredContext->DrawInstanced(dpCount, NumInstances, BaseVertexIndex, 0);
}

void ID11CommandList::DrawIndexedPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances)
{
	UINT dpCount = 0;
	mDeferredContext->IASetPrimitiveTopology(PrimitiveTypeToDX(PrimitiveType, NumPrimitives, &dpCount));

	AUTO_SAMP("Native.IPass.BuildPass.DrawPrimitive");
//#if _DEBUG
//	if (S_OK != ID11RenderContext::DefaultRenderContext->CheckContext(mDeferredContext))
//	{
//		ID3D11InputLayout* curILT;
//		mDeferredContext->IAGetInputLayout(&curILT);
//		if (curILT == nullptr)
//		{
//			auto d11Layout = (ID11InputLayout*)this->mCurrentState.InputLayout->GetInputLayout();
//			auto d3d11obj = d11Layout->GetInnerLayout();
//			mDeferredContext->IASetInputLayout(d3d11obj);
//		}
//	}
//#endif
	if (NumInstances == 1)
		mDeferredContext->DrawIndexed(dpCount, StartIndex, BaseVertexIndex);
	else
		mDeferredContext->DrawIndexedInstanced(dpCount, NumInstances, StartIndex, BaseVertexIndex, 0);
}
void ID11CommandList::DrawIndexedInstancedIndirect(EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs)
{
	UINT dpCount = 0;
	mDeferredContext->IASetPrimitiveTopology(PrimitiveTypeToDX(PrimitiveType, 0, &dpCount));

	mDeferredContext->DrawIndexedInstancedIndirect(((ID11GpuBuffer*)pBufferForArgs)->mBuffer, AlignedByteOffsetForArgs);
}
void ID11CommandList::IASetInputLayout(IInputLayout* pInputLayout)
{
	mDeferredContext->IASetInputLayout(((ID11InputLayout*)pInputLayout)->GetInnerLayout());
}
void ID11CommandList::VSSetShader(IVertexShader* pVertexShader, void** ppClassInstances, UINT NumClassInstances)
{
	mDeferredContext->VSSetShader(((ID11VertexShader*)pVertexShader)->mShader, nullptr, 0);
}
void ID11CommandList::PSSetShader(IPixelShader* pPixelShader, void** ppClassInstances, UINT NumClassInstances)
{
	if (pPixelShader == nullptr)
	{
		ID3D11PixelShader* p = nullptr;
		mDeferredContext->PSSetShader(p, nullptr, 0);
	}
	else
	{
		mDeferredContext->PSSetShader(((ID11PixelShader*)pPixelShader)->mShader, nullptr, 0);
	}
}
void ID11CommandList::SetViewport(IViewPort* vp)
{
	D3D11_VIEWPORT d11vp;
	d11vp.Width = vp->Width;
	d11vp.Height = vp->Height;
	d11vp.MinDepth = vp->MinDepth;
	d11vp.MaxDepth = vp->MaxDepth;
	d11vp.TopLeftX = vp->TopLeftX;
	d11vp.TopLeftY = vp->TopLeftY;

	mDeferredContext->RSSetViewports(1, &d11vp);
}
void ID11CommandList::VSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer)
{
	//CBuffer->UpdateDrawPass(this, TRUE);
	mDeferredContext->VSSetConstantBuffers(Index, 1, &((ID11ConstantBuffer*)CBuffer)->mBuffer);
}
void ID11CommandList::PSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer)
{
	//CBuffer->UpdateDrawPass(this, TRUE);
	mDeferredContext->PSSetConstantBuffers(Index, 1, &((ID11ConstantBuffer*)CBuffer)->mBuffer);
}
void ID11CommandList::SetRenderPipeline(IRenderPipeline* pipeline)
{
	((ID11RenderPipeline*)pipeline)->ApplyState(this);
	pipeline->GetGpuProgram()->ApplyShaders(this);
}
vBOOL ID11CommandList::CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers)
{
	auto d11Src = (ID11ShaderResourceView*)src;
	ID3D11Texture2D* pSrcTexture;
	d11Src->m_pDX11SRV->GetResource((ID3D11Resource**)&pSrcTexture);
	if (pSrcTexture == nullptr)
		return FALSE;

	D3D11_TEXTURE2D_DESC desc;
	pSrcTexture->GetDesc(&desc);

	auto pD11Texture = (ID11Texture2D*)(*ppTexture);
	bool needCreateTexture = false;
	if (pD11Texture == nullptr)
	{
		needCreateTexture = true;
	}
	else
	{
		if (pD11Texture->mDesc.Width != desc.Width || 
			pD11Texture->mDesc.Height != desc.Height)
		{
			needCreateTexture = true;
		}
	}

	ID3D11Texture2D* memTexture = nullptr;
	if (needCreateTexture)
	{
		desc.BindFlags = 0;
		desc.Usage = D3D11_USAGE_STAGING;
		desc.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;

		auto rc = this->GetContext().UnsafeConvertTo<ID11RenderContext>();
		auto pDevice = rc->mDevice;
		auto hr = pDevice->CreateTexture2D(&desc, nullptr, &memTexture);
		if (FAILED(hr))
		{
			pSrcTexture->Release();
			return FALSE;
		}
	}
	else
	{
		memTexture = pD11Texture->m_pDX11Texture2D;
	}

	mDeferredContext->CopyResource(memTexture, pSrcTexture);

	if (needCreateTexture)
	{
		Safe_Release(pD11Texture);
		pD11Texture = new ID11Texture2D();
		pD11Texture->InitD11Texture2D(memTexture);
		memTexture->Release();

		*ppTexture = pD11Texture;
	}
	
	pSrcTexture->Release();
	
	return TRUE;
}

void ID11CommandList::Signal(IFence* fence, int value)
{
	AutoRef<ID11Fence> tmp;
	tmp.StrongRef((ID11Fence*)fence);
	mSignals.push_back(tmp);
}

NS_END