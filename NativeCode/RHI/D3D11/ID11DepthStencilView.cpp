#include "ID11DepthStencilView.h"
#include "ID11RenderContext.h"
#include "ID11ShaderResourceView.h"
#include "ID11Texture2D.h"

#define new VNEW

NS_BEGIN

ID11DepthStencilView::ID11DepthStencilView()
{
	//mTexture = nullptr;
	m_pDX11DSV = nullptr;
}

ID11DepthStencilView::~ID11DepthStencilView()
{
	//Safe_Release(mTexture);
	Safe_Release(m_pDX11DSV);
}

bool ID11DepthStencilView::Init(ID11RenderContext* rc, const IDepthStencilViewDesc* pDesc)
{
	auto dxPixelFormat = FormatToDXFormat(pDesc->Format);
	
	m_refTexture2D.StrongRef(pDesc->m_pTexture2D);
	/*memset(&mTextureDesc, 0, sizeof(mTextureDesc));
	mTextureDesc.Width = desc->Width;
	mTextureDesc.Height = desc->Height;
	mTextureDesc.MipLevels = 1;
	mTextureDesc.ArraySize = 1;
	switch (dxPixelFormat)
	{
	case DXGI_FORMAT_D24_UNORM_S8_UINT:
		mTextureDesc.Format = DXGI_FORMAT_R24G8_TYPELESS;
		break;
	case DXGI_FORMAT_D32_FLOAT:
		mTextureDesc.Format = DXGI_FORMAT_R32_TYPELESS;
		break;
	case DXGI_FORMAT_D16_UNORM:
		mTextureDesc.Format = DXGI_FORMAT_R16_TYPELESS;
		break;
	default:
		mTextureDesc.Format = DXGI_FORMAT_R32_TYPELESS;
		break;
	}

	mTextureDesc.SampleDesc.Count = 1;
	mTextureDesc.SampleDesc.Quality = 0;
	mTextureDesc.Usage = D3D11_USAGE_DEFAULT;
	mTextureDesc.BindFlags = D3D11_BIND_DEPTH_STENCIL | D3D11_BIND_SHADER_RESOURCE;
	mTextureDesc.CPUAccessFlags = 0;
	mTextureDesc.MiscFlags = 0;
	auto hr = rc->mDevice->CreateTexture2D(&mTextureDesc, NULL, &mTexture);
	if (FAILED(hr))
		return false;*/
	D3D11_DEPTH_STENCIL_VIEW_DESC	DSVDesc;
	memset(&DSVDesc, 0, sizeof(DSVDesc));
	switch (dxPixelFormat)
	{
	case DXGI_FORMAT_D24_UNORM_S8_UINT:
	case DXGI_FORMAT_D32_FLOAT:
	case DXGI_FORMAT_D16_UNORM:
		DSVDesc.Format = dxPixelFormat;
		break;
	default:
		DSVDesc.Format = DXGI_FORMAT_D32_FLOAT;
		break;
	}
	DSVDesc.ViewDimension = D3D11_DSV_DIMENSION_TEXTURE2D;
	DSVDesc.Texture2D.MipSlice = 0;
	
	auto hr = rc->mDevice->CreateDepthStencilView(((ID11Texture2D*)pDesc->m_pTexture2D)->m_pDX11Texture2D, &DSVDesc, &m_pDX11DSV);
	if (FAILED(hr))
	{
		return false;
	}
	
	/*auto d11SRV = new ID11ShaderResourceView();
	memset(&d11SRV->mDX11SRVDesc, 0, sizeof(d11SRV->mDX11SRVDesc));
	switch (dxPixelFormat)
	{
	case DXGI_FORMAT_D24_UNORM_S8_UINT:
		d11SRV->mDX11SRVDesc.Format = DXGI_FORMAT_R24_UNORM_X8_TYPELESS;
		break;
	case DXGI_FORMAT_D32_FLOAT:
		d11SRV->mDX11SRVDesc.Format = DXGI_FORMAT_R32_FLOAT;
		break;
	case DXGI_FORMAT_D16_UNORM:
		d11SRV->mDX11SRVDesc.Format = DXGI_FORMAT_R16_UNORM;
		break;
	default:
		d11SRV->mDX11SRVDesc.Format = DXGI_FORMAT_R32_FLOAT;
		break;
	}
	d11SRV->mDX11SRVDesc.ViewDimension = D3D11_SRV_DIMENSION_TEXTURE2D;
	d11SRV->mDX11SRVDesc.Texture2D.MipLevels = 1;
	hr = rc->mDevice->CreateShaderResourceView(mTexture, &d11SRV->mDX11SRVDesc, &d11SRV->m_pDX11SRV);
	if (FAILED(hr))
		return false;*/
	/*AutoRef<ID11Texture2D> texture = new ID11Texture2D();
	texture->InitD11Texture2D(mTexture);
	d11SRV->SetDepthStencilTexture(texture);
	m_refSRV_DS = d11SRV;
	d11SRV->GetResourceState()->SetStreamState(SS_Valid);*/

#ifdef _DEBUG
	m_pDX11DSV->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	static UINT UniqueId = 0;
	auto debuginfo = VStringA_FormatV("DSView_%u", UniqueId++);
	m_pDX11DSV->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
#endif

	return true;
}

NS_END