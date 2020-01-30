#include "ID11SwapChain.h"
#include "ID11RenderContext.h"
#include "ID11RenderSystem.h"
#include "ID11Texture2D.h"
#include "../../Core/vfxSampCounter.h"

#define new VNEW

NS_BEGIN

ID11SwapChain::ID11SwapChain()
{
	mSwapChain = nullptr;
	memset(&mSwapChainDesc, 0, sizeof(mSwapChainDesc));
	m_pSwapChainBuffer = nullptr;
}

ID11SwapChain::~ID11SwapChain()
{
	Safe_Release(m_pSwapChainBuffer);
	Safe_Release(mSwapChain);
}

ITexture2D* ID11SwapChain::GetTexture2D()
{
	return m_pSwapChainBuffer;
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
	
	Safe_Release(m_pSwapChainBuffer);
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
	
	hr = mSwapChain->GetBuffer(0, IID_ID3D11Texture2D, (void**)&pBackBuffer);
	if (hr == S_OK)
	{
		m_pSwapChainBuffer = new ID11Texture2D();
		m_pSwapChainBuffer->InitD11Texture2D(pBackBuffer);
		pBackBuffer->Release();
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