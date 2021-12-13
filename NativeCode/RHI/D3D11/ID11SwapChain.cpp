#include "ID11SwapChain.h"
#include "ID11RenderContext.h"
#include "ID11RenderSystem.h"
#include "ID11Texture2D.h"
#include "ID11RenderTargetView.h"
#include "../../Base/vfxSampCounter.h"

#define new VNEW

NS_BEGIN

ID11SwapChain::ID11SwapChain()
{
	mSwapChain = nullptr;
	mBufferCount = 0;
	memset(&mSwapChainDesc, 0, sizeof(mSwapChainDesc));
}

ID11SwapChain::~ID11SwapChain()
{
	Safe_Release(mSwapChain);
}

UINT ID11SwapChain::GetBackBufferNum()
{
	return 1;
}

ITexture2D* ID11SwapChain::GetBackBuffer(UINT index)
{
	return mBackBuffer;
}

void ID11SwapChain::BindCurrent()
{

}

void ID11SwapChain::Present(UINT SyncInterval, UINT Flags)
{
	AUTO_SAMP("Native.ISwapChain.Present");
	
	BindCurrent();
	if(mSwapChain!=nullptr)
		mSwapChain->Present(SyncInterval, Flags);
}

void ID11SwapChain::OnLost()
{

}

vBOOL ID11SwapChain::OnRestore(const ISwapChainDesc* desc)
{
	if (HostContext.GetPtr() == nullptr)
		return FALSE;
	mDesc = *desc;
	
	Safe_Release(mSwapChain);

	return Init(HostContext.GetPtr(), desc)?1:0;
}

DXGI_MODE_DESC ID11SwapChain::SetupDXGI_MODE_DESC(UINT w, UINT h, EPixelFormat format) const
{
	DXGI_MODE_DESC Ret;

	Ret.Width = w;
	Ret.Height = h;
	Ret.RefreshRate.Numerator = 0;	
	Ret.RefreshRate.Denominator = 1;	
	Ret.Format = FormatToDXFormat(format);
	Ret.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
	Ret.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;

	return Ret;
}

bool ID11SwapChain::Init(ID11RenderContext* rc, const ISwapChainDesc* desc)
{
	mDesc = *desc;
	HostContext.FromObject(rc);

	ZeroMemory(&mSwapChainDesc, sizeof(mSwapChainDesc));
	
	mSwapChainDesc.BufferDesc = SetupDXGI_MODE_DESC(desc->Width, desc->Height, desc->Format);

	mSwapChainDesc.SampleDesc.Count = 1;
	mSwapChainDesc.SampleDesc.Quality = 0;
	mSwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT | DXGI_USAGE_SHADER_INPUT;
	// 1:single buffering, 2:double buffering, 3:triple buffering
	mSwapChainDesc.BufferCount = 1;
	mSwapChainDesc.OutputWindow = (HWND)desc->WindowHandle;
	mSwapChainDesc.Windowed = TRUE;
	mSwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	mSwapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;

	auto hr = rc->mSystem->m_pDXGIFactory->CreateSwapChain(rc->mDevice, &mSwapChainDesc, &mSwapChain);
	if (FAILED(hr))
		return false;

	ID3D11Texture2D* pBackBuffer = NULL;
	
	DXGI_SWAP_CHAIN_DESC scDesc;
	mSwapChain->GetDesc(&scDesc);
	mBufferCount = scDesc.BufferCount;
	hr = mSwapChain->GetBuffer(0, IID_ID3D11Texture2D, (void**)&pBackBuffer);
	if (hr == S_OK)
	{
		AutoRef<ID11Texture2D> pTexture(new ID11Texture2D());
		pTexture->InitD11Texture2D(pBackBuffer);
		pBackBuffer->Release();
		mBackBuffer = pTexture;

		//auto pRTV = new ID11RenderTargetView();
		//IRenderTargetViewDesc rtvDesc;
		//rtvDesc.SetTexture2D();
		//rtvDesc.mGpuBuffer = pTexture;
		//rtvDesc.Format = pTexture->mTextureDesc.Format;
		//rtvDesc.Width = pTexture->mTextureDesc.Width;
		//rtvDesc.Height = pTexture->mTextureDesc.Height;
		////rtvDesc.Texture2D.MipSlice = pTexture->mTextureDesc.MipLevels;
		//pRTV->Init(rc, &rtvDesc);
		//mSwapChainRTV.WeakRef(pRTV);
	}

#ifdef _DEBUG
	mSwapChain->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	static UINT UniqueId = 0;
	auto debuginfo = VStringA_FormatV("SwapChain_%u", UniqueId++);
	mSwapChain->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.size(), debuginfo.c_str());
#endif

	return true;
}

NS_END