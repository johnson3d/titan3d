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
#include "../GraphicsProfiler.h"
#include "../../Core/vfxSampCounter.h"

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
		mCmdList->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
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

	

	FrameBufferClearColor RTVClearColorArray[MAX_MRT_NUM] = {
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
		((ID11RenderContext*)pRHICtx)->mHardwareContext->ExecuteCommandList(mCmdList, FALSE);
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

NS_END