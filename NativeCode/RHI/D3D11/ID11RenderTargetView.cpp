#include "ID11RenderTargetView.h"
#include "ID11RenderContext.h"
#include "ID11Texture2D.h"
#include "ID11ShaderResourceView.h"
#include "ID11GpuBuffer.h"

#define new VNEW

NS_BEGIN

ID11RenderTargetView::ID11RenderTargetView()
{
	m_pDX11RTV = nullptr;
}

ID11RenderTargetView::~ID11RenderTargetView()
{
	Safe_Release(m_pDX11RTV);
}

void RtvDesc2DX(D3D11_RENDER_TARGET_VIEW_DESC* tar, const IRenderTargetViewDesc* src)
{
	memset(tar, 0, sizeof(D3D11_RENDER_TARGET_VIEW_DESC));
	tar->Format = FormatToDXFormat(src->Format);
	tar->ViewDimension = *(D3D11_RTV_DIMENSION*)&src->ViewDimension;
	switch (src->Type)
	{
	case RTV_BufferSRV:
	{
		tar->Buffer = *(D3D11_BUFFER_RTV*)&src->Buffer;
	}
	break;
	case RTV_Texture1D:
	{
		tar->Texture1D.MipSlice = src->Texture1D.MipSlice;
	}
	break;
	case RTV_Texture1DArray:
	{
		tar->Texture1DArray.MipSlice = src->Texture1DArray.MipSlice;
		tar->Texture1DArray.ArraySize = src->Texture1DArray.ArraySize;
		tar->Texture1DArray.FirstArraySlice = src->Texture1DArray.FirstArraySlice;
	}
	break;
	case RTV_Texture2D:
	{
		tar->Texture2D.MipSlice = src->Texture2D.MipSlice;
	}
	break;
	case RTV_Texture2DArray:
	{
		tar->Texture2DArray.MipSlice = src->Texture2DArray.MipSlice;
		tar->Texture2DArray.ArraySize = src->Texture2DArray.ArraySize;
		tar->Texture2DArray.FirstArraySlice = src->Texture2DArray.FirstArraySlice;
	}
	break;
	case RTV_Texture2DMS:
	{

	}
	break;
	case RTV_Texture2DMSArray:
	{

	}
	break;
	case RTV_Texture3D:
	{

	}
	break;
	default:
		break;
	}

	switch (tar->Format)
	{
	case DXGI_FORMAT_R24G8_TYPELESS:
		tar->Format = DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
		break;
	case DXGI_FORMAT_R32_TYPELESS:
		tar->Format = DXGI_FORMAT_R32_FLOAT;
		break;
	case DXGI_FORMAT_R16_TYPELESS:
		tar->Format = DXGI_FORMAT_R16_UNORM;
		break;
	}
}

bool ID11RenderTargetView::Init(ID11RenderContext* pCtx, const IRenderTargetViewDesc* pRTVDesc)
{
	RefGpuBuffer.StrongRef(pRTVDesc->mGpuBuffer);

	D3D11_RENDER_TARGET_VIEW_DESC	mDesc;
	RtvDesc2DX(&mDesc, pRTVDesc);

	if (pRTVDesc->mGpuBuffer != nullptr)
	{
		auto hr = pCtx->mDevice->CreateRenderTargetView((ID3D11Resource*)pRTVDesc->mGpuBuffer->GetHWBuffer(), &mDesc, &m_pDX11RTV);
		if (FAILED(hr))
		{
			return false;
		}
		
#ifdef _DEBUG
		m_pDX11RTV->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
		static UINT UniqueId = 0;
		auto debuginfo = VStringA_FormatV("RTV_%u", UniqueId++);
		m_pDX11RTV->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.size(), debuginfo.c_str());
#endif
	}

	/*D3D11_RENDER_TARGET_VIEW_DESC d11Desc;
	memset(&d11Desc, 0, sizeof(d11Desc));
	d11Desc.Format = FormatToDXFormat(desc->Format);
	d11Desc.Texture2D.MipSlice = 0;
	d11Desc.ViewDimension = D3D11_RTV_DIMENSION::D3D11_RTV_DIMENSION_TEXTURE2D;*/
	
	return true;
}

NS_END