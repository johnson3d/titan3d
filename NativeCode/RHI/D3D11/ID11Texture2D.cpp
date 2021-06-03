#include "ID11Texture2D.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"

#define new VNEW

NS_BEGIN

ID11Texture2D::ID11Texture2D()
{
	m_pDX11Texture2D = nullptr;
}

ID11Texture2D::~ID11Texture2D()
{
	Safe_Release(m_pDX11Texture2D);
}

bool ID11Texture2D::Init(ID11RenderContext* rc, const ITexture2DDesc* desc)
{
	mDesc = *desc;
	if (desc->MipLevels == 0)
		return false;
	D3D11_TEXTURE2D_DESC Tex2dDesc;
	ZeroMemory(&Tex2dDesc, sizeof(Tex2dDesc));
	Tex2dDesc.Width = desc->Width;
	Tex2dDesc.Height = desc->Height;
	Tex2dDesc.MipLevels = desc->MipLevels;
	Tex2dDesc.ArraySize = desc->ArraySize;
	Tex2dDesc.Format = FormatToDXFormat(desc->Format);
	Tex2dDesc.SampleDesc.Count = 1;
	Tex2dDesc.SampleDesc.Quality = 0;
	Tex2dDesc.Usage = D3D11_USAGE_DEFAULT;
	Tex2dDesc.BindFlags = desc->BindFlags;
	Tex2dDesc.CPUAccessFlags = desc->CPUAccess;
	Tex2dDesc.MiscFlags = 0;
	
	switch (Tex2dDesc.Format)
	{
	case DXGI_FORMAT_D24_UNORM_S8_UINT:
		Tex2dDesc.Format = DXGI_FORMAT_R24G8_TYPELESS;
		break;
	case DXGI_FORMAT_D32_FLOAT:
		Tex2dDesc.Format = DXGI_FORMAT_R32_TYPELESS;
		break;
	case DXGI_FORMAT_D16_UNORM:
		Tex2dDesc.Format = DXGI_FORMAT_R16_TYPELESS;
		break;
	}


	if (desc->InitData != nullptr)
	{
		rc->mDevice->CreateTexture2D(&Tex2dDesc, (D3D11_SUBRESOURCE_DATA*)desc->InitData, &m_pDX11Texture2D);
	}
	else
	{
		rc->mDevice->CreateTexture2D(&Tex2dDesc, nullptr, &m_pDX11Texture2D);
	}
	return true;
}

bool ID11Texture2D::InitD11Texture2D(ID3D11Texture2D* pTex2d)
{
	if (pTex2d)
	{
		pTex2d->AddRef();
	}
	Safe_Release(m_pDX11Texture2D);
	m_pDX11Texture2D = pTex2d;

	D3D11_TEXTURE2D_DESC desc;
	pTex2d->GetDesc(&desc);
	mDesc.Width = desc.Width;
	mDesc.Height = desc.Height;
	mDesc.MipLevels = desc.MipLevels;
	mDesc.ArraySize = desc.ArraySize;
	mDesc.Format = DXFormatToFormat(desc.Format);
	mDesc.BindFlags = desc.BindFlags;
	mDesc.CPUAccess = desc.CPUAccessFlags;// CAS_WRITE | CAS_READ;
	mDesc.InitData = nullptr;
	ASSERT(mDesc.Format != PXF_UNKNOWN);
	return true;
}

vBOOL ID11Texture2D::Map(ICommandList* cmd, int MipLevel, void** ppData, UINT* pRowPitch, UINT* pDepthPitch)
{
	auto rc = cmd->GetContext().UnsafeConvertTo<ID11RenderContext>();
	auto pContext = rc->mHardwareContext;

	rc->mHWContextLocker.Lock();
	//auto pContext = ((ID11CommandList*)cmd)->mDeferredContext;
	D3D11_MAPPED_SUBRESOURCE mapped;
	if (pContext->Map(m_pDX11Texture2D, 0, D3D11_MAP_READ, 0, &mapped) != S_OK)
	{
		rc->mHWContextLocker.Unlock();
		return FALSE;
	}
	*ppData = mapped.pData;
	*pRowPitch = mapped.RowPitch;
	*pDepthPitch = mapped.DepthPitch;
	return TRUE;
}

void ID11Texture2D::Unmap(ICommandList* cmd, int MipLevel)
{
	auto rc = cmd->GetContext().UnsafeConvertTo<ID11RenderContext>();
	auto pContext = rc->mHardwareContext;
	//auto pContext = ((ID11CommandList*)cmd)->mDeferredContext;
	pContext->Unmap(m_pDX11Texture2D, 0);
	rc->mHWContextLocker.Unlock();
}

void ID11Texture2D::UpdateMipData(ICommandList* cmd, UINT level, void* pData, UINT width, UINT height, UINT Pitch)
{
	auto pContext = ((ID11CommandList*)cmd)->mDeferredContext;

	D3D11_BOX box;
	box.left = 0;
	box.top = 0;
	box.right = width;
	box.bottom = height;
	box.front = 1;
	box.back = 0;
	pContext->UpdateSubresource(m_pDX11Texture2D, level, nullptr, pData, Pitch, Pitch*height);
}

NS_END