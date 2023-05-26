#include "DX12FrameBuffers.h"
#include "DX12Buffer.h"
#include "DX12GpuDevice.h"
#include "DX12Event.h"
#include "DX12Effect.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	DX12FrameBuffers::DX12FrameBuffers()
	{

	}
	DX12FrameBuffers::~DX12FrameBuffers()
	{
		mDX11RTVArray.clear();
	}
	void DX12FrameBuffers::FlushModify()
	{
		auto NumRTV = mRenderPass->Desc.NumOfMRT;
		mDX11RTVArray.resize(NumRTV);

		UINT32 RTVIdx = 0;
		for (RTVIdx = 0; RTVIdx < NumRTV; RTVIdx++)
		{
			auto refRTV = mRenderTargets[RTVIdx];
			if (refRTV == nullptr)
			{
				break;
			}
			ASSERT(refRTV->GetHWBuffer() != nullptr);
			auto dxRtv = (DX12DescriptorSetPagedObject*)refRTV->GetHWBuffer();
			mDX11RTVArray[RTVIdx] = dxRtv->GetCpuAddress(0);
		}
	}

	DX12SwapChain::DX12SwapChain()
	{
		mView = nullptr;
	}

	DX12SwapChain::~DX12SwapChain()
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
		Ret.Format = FormatToDX12Format(format);
		Ret.ScanlineOrdering = DXGI_MODE_SCANLINE_ORDER_UNSPECIFIED;
		Ret.Scaling = DXGI_MODE_SCALING_UNSPECIFIED;

		return Ret;
	}
	bool DX12SwapChain::Init(DX12GpuDevice* device, const FSwapChainDesc& desc)
	{
		Desc = desc;
		Desc.BufferCount = 3;
		
		FFenceDesc fcdesc{};
		fcdesc.InitValue = 0;
		PresentFence = MakeWeakRef(device->CreateFence(&fcdesc, "SwapChain Fence"));
		CurrentFenceTargetValue = 0;
		return Create(device, Desc.Width, Desc.Height);
	}
	bool DX12SwapChain::Resize(IGpuDevice* device, UINT w, UINT h)
	{
		//return true;
		//return Create(device, w, h);
		device->GetCmdQueue()->Flush((EQueueType)(EQueueType::QU_Default | EQueueType::QU_Compute | EQueueType::QU_Transfer));
		for (auto& i : BackBuffers)
		{
			auto pDxTexture = i.Texture.UnsafeConvertTo<DX12Texture>();
			pDxTexture->mGpuResource = nullptr;
		}
		Desc.Width = w;
		Desc.Height = h;

		auto hr = mView->ResizeBuffers(Desc.BufferCount, w, h, FormatToDX12Format(Desc.Format), DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH);
		if (hr != S_OK)
			return false;

		for (UINT i = 0; i < Desc.BufferCount; i++)
		{
			auto pDx12Texture = (DX12Texture*)GetBackBuffer(i);
			ID3D12Resource* pBackBuffer = nullptr;
			hr = mView->GetBuffer(i, IID_PPV_ARGS(&pBackBuffer));
			if (hr == S_OK)
			{
				pDx12Texture->Desc.Width = Desc.Width;
				pDx12Texture->Desc.Height = Desc.Height;
				pDx12Texture->mGpuResource = MakeWeakRef(pBackBuffer);
				pDx12Texture->GpuState = EGpuResourceState::GRS_Present;
			}
			else
			{
				ASSERT(false);
			}
			BackBuffers[i].CreateRtvAndSrv(device);
		}

		if (mView3 != nullptr)
			CurrentBackBuffer = mView3->GetCurrentBackBufferIndex();
		else
			CurrentBackBuffer = 0;
		return true;
	}
	bool DX12SwapChain::Create(IGpuDevice* device1, UINT w, UINT h)
	{
		Desc.Width = w;
		Desc.Height = h;
		if (BackBuffers.size() != Desc.BufferCount)
		{
			BackBuffers.clear();
			BackBuffers.resize(Desc.BufferCount);
		}
		Safe_Release(mView);

		DX12GpuDevice* device = (DX12GpuDevice*)device1;
		DXGI_SWAP_CHAIN_DESC	mSwapChainDesc{};
		ZeroMemory(&mSwapChainDesc, sizeof(mSwapChainDesc));

		mSwapChainDesc.BufferDesc = SetupDXGI_MODE_DESC(Desc.Width, Desc.Height, Desc.Format);
		mSwapChainDesc.BufferCount = Desc.BufferCount;

		mSwapChainDesc.SampleDesc.Count = Desc.SampleDesc.Count;
		mSwapChainDesc.SampleDesc.Quality = Desc.SampleDesc.Quality;
		mSwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;// | DXGI_USAGE_SHADER_INPUT;
		// 1:single buffering, 2:double buffering, 3:triple buffering
		mSwapChainDesc.OutputWindow = (HWND)Desc.OutputWindow;
		mSwapChainDesc.Windowed = Desc.Windowed;
		mSwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_FLIP_DISCARD;
		mSwapChainDesc.Flags = DXGI_SWAP_CHAIN_FLAG_ALLOW_MODE_SWITCH;

		auto hr = device->mGpuSystem.GetPtr()->mDXGIFactory->CreateSwapChain(device->mCmdQueue->mCmdQueue, &mSwapChainDesc, (IDXGISwapChain**)&mView);
		if (FAILED(hr))
			return false;
		mView->QueryInterface(IID_IDXGISwapChain3, (void**)mView3.GetAddressOf());

		for (UINT i = 0; i < Desc.BufferCount; i++)
		{
			if (BackBuffers[i].Texture == nullptr)
			{
				BackBuffers[i].Texture = MakeWeakRef(new DX12Texture());
			}
			auto pDx12Texture = (DX12Texture*)GetBackBuffer(i);
			pDx12Texture->mDeviceRef.FromObject(device);
			ID3D12Resource* pBackBuffer = nullptr;
			hr = mView->GetBuffer(i, IID_PPV_ARGS(&pBackBuffer));
			if (hr == S_OK)
			{
				pDx12Texture->Desc.Format = Desc.Format;
				pDx12Texture->Desc.Width = Desc.Width;
				pDx12Texture->Desc.Height = Desc.Height;
				pDx12Texture->mGpuResource = MakeWeakRef(pBackBuffer);
				pDx12Texture->GpuState = EGpuResourceState::GRS_Present;
			}
			else
			{
				ASSERT(false);
			}
			BackBuffers[i].CreateRtvAndSrv(device);
		}
		CurrentBackBuffer = 0;
		return true;
	}
	ITexture* DX12SwapChain::GetBackBuffer(UINT index)
	{
		return BackBuffers[index].Texture;
	}
	IRenderTargetView* DX12SwapChain::GetBackRTV(UINT index)
	{
		return BackBuffers[index].Rtv;
	}
	UINT DX12SwapChain::GetCurrentBackBuffer()
	{
		CurrentBackBuffer = CurrentBackBuffer % BackBuffers.size();
		return CurrentBackBuffer;
	}
	void DX12SwapChain::BeginFrame()
	{
		if (PresentFence != nullptr)
		{
			//PresentFence->Wait(CurrentFenceTargetValue);
			PresentFence->WaitToExpect();
		}
	}
	void DX12SwapChain::Present(IGpuDevice* device, UINT SyncInterval, UINT Flags)
	{
		/*auto pTexture = (DX12Texture*)GetBackBuffer(GetCurrentBackBuffer());
		
		mCommandList->ResourceBarrier(1, &CD3DX12_RESOURCE_BARRIER::Transition(CurrentBackBuffer(),
			D3D12_RESOURCE_STATE_PRESENT, D3D12_RESOURCE_STATE_RENDER_TARGET));*/

		auto hr = mView->Present(SyncInterval, Flags);
		if (hr != S_OK)
		{
			VFX_LTRACE(ELTT_Graphics, "SwapChain::Present");
			hr = ((DX12GpuDevice*)device)->mDevice->GetDeviceRemovedReason();
		}
		
		if (mView3 != nullptr)
			CurrentBackBuffer = mView3->GetCurrentBackBufferIndex();
		else
			CurrentBackBuffer = (CurrentBackBuffer + 1) % (UINT)BackBuffers.size();
		
		device->GetCmdQueue()->IncreaseSignal(PresentFence, EQueueType::QU_Default);
	}
	void DX12SwapChain::FBackBuffer::CreateRtvAndSrv(IGpuDevice* device)
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