#include "DX11FrameBuffers.h"
#include "DX11Buffer.h"
#include "DX11GpuDevice.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	DX11FrameBuffers::DX11FrameBuffers()
	{

	}
	DX11FrameBuffers::~DX11FrameBuffers()
	{
		for (auto i : mDX11RTVArray)
		{
			Safe_Release(i);
		}
		mDX11RTVArray.clear();
	}
	void DX11FrameBuffers::FlushModify()
	{
		for (auto i : mDX11RTVArray)
		{
			Safe_Release(i);
		}
		mDX11RTVArray.clear();
		auto NumRTV = mRenderPass->Desc.NumOfMRT;
		mDX11RTVArray.resize(NumRTV);

		UINT RTVIdx = 0;
		for (RTVIdx = 0; RTVIdx < NumRTV; RTVIdx++)
		{
			auto refRTV = mRenderTargets[RTVIdx];
			if (refRTV == nullptr || refRTV->GetHWBuffer() == nullptr)
			{
				break;
			}
			auto dxRtv = (ID3D11RenderTargetView*)refRTV->GetHWBuffer();
			dxRtv->AddRef();
			mDX11RTVArray[RTVIdx] = dxRtv;
		}
	}

	DX11SwapChain::DX11SwapChain()
	{
		mView = nullptr;
	}

	DX11SwapChain::~DX11SwapChain()
	{
		BackBuffers.clear();
		Safe_Release(mView);
	}

	static DXGI_MODE_DESC SetupDXGI_MODE_DESC(UINT w, UINT h, EPixelFormat format)
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
	bool DX11SwapChain::Init(DX11GpuDevice* device, const FSwapChainDesc& desc)
	{
		Desc = desc;
		
		return Resize(device, Desc.Width, Desc.Height);
	}
	bool DX11SwapChain::Resize(IGpuDevice* device1, UINT w, UINT h)
	{
		Desc.Width = w;
		Desc.Height = h;
		if (BackBuffers.size() != Desc.BufferCount)
		{
			BackBuffers.clear();
			BackBuffers.resize(Desc.BufferCount);
		}
		Safe_Release(mView);

		DX11GpuDevice* device = (DX11GpuDevice*)device1;
		DXGI_SWAP_CHAIN_DESC	mSwapChainDesc;
		ZeroMemory(&mSwapChainDesc, sizeof(mSwapChainDesc));

		mSwapChainDesc.BufferDesc = SetupDXGI_MODE_DESC(Desc.Width, Desc.Height, Desc.Format);

		mSwapChainDesc.SampleDesc.Count = Desc.SampleDesc.Count;
		mSwapChainDesc.SampleDesc.Quality = Desc.SampleDesc.Quality;
		mSwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT | DXGI_USAGE_SHADER_INPUT;
		// 1:single buffering, 2:double buffering, 3:triple buffering
		mSwapChainDesc.BufferCount = Desc.BufferCount;
		mSwapChainDesc.OutputWindow = (HWND)Desc.OutputWindow;
		mSwapChainDesc.Windowed = Desc.Windowed;
		mSwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_SEQUENTIAL;
		mSwapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;

		auto hr = device-> mDXGIFactory->CreateSwapChain(device->mDevice, &mSwapChainDesc, &mView);
		if (FAILED(hr))
			return false;

		for (UINT i = 0; i < Desc.BufferCount; i++)
		{
			if (BackBuffers[i].Texture == nullptr)
			{
				auto pTex = new DX11Texture();
				pTex->mDeviceRef.FromObject(device);
				BackBuffers[i].Texture = MakeWeakRef(pTex);
			}
			auto pDx11Texture = (DX11Texture*)GetBackBuffer(i);			
			ID3D11Texture2D* pBackBuffer = nullptr;
			hr = mView->GetBuffer(i, IID_ID3D11Texture2D, (void**)&pBackBuffer);
			if (hr == S_OK)
			{
				pDx11Texture->Desc.Format = Desc.Format;
				pDx11Texture->Desc.Width = Desc.Width;
				pDx11Texture->Desc.Height = Desc.Height;
				pDx11Texture->Desc.BindFlags = (EBufferType)(EBufferType::BFT_SRV | EBufferType::BFT_RTV);
				Safe_Release(pDx11Texture->mTexture2D);
				pDx11Texture->mTexture2D = pBackBuffer;
				pDx11Texture->GpuState = EGpuResourceState::GRS_Present;
			}
			else
			{
				ASSERT(false);
			}
			BackBuffers[i].CreateRtvAndSrv(device);
		}
		return true;
	}
	ITexture* DX11SwapChain::GetBackBuffer(UINT index)
	{
		return BackBuffers[index].Texture;
	}
	IRenderTargetView* DX11SwapChain::GetBackRTV(UINT index)
	{
		return BackBuffers[index].Rtv;
	}
	UINT DX11SwapChain::GetCurrentBackBuffer()
	{
		return 0;
	}
	void DX11SwapChain::Present(IGpuDevice* device, UINT SyncInterval, UINT Flags)
	{
		auto hr = mView->Present(SyncInterval, Flags);
		ASSERT(hr == S_OK);
	}
	void DX11SwapChain::FBackBuffer::CreateRtvAndSrv(IGpuDevice* device)
	{
		FRtvDesc rtvDesc{};
		rtvDesc.SetTexture2D();
		rtvDesc.Width = Texture->Desc.Width;
		rtvDesc.Height = Texture->Desc.Height;
		rtvDesc.Format = Texture->Desc.Format;
		rtvDesc.Texture2D.MipSlice = 0;
		Rtv = MakeWeakRef(device->CreateRTV(Texture, &rtvDesc));
	}
}

NS_END