#include "ID11RenderTargetView.h"
#include "ID11RenderContext.h"
#include "ID11Texture2D.h"
#include "ID11ShaderResourceView.h"

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

bool ID11RenderTargetView::Init(ID11RenderContext* pCtx, const IRenderTargetViewDesc* pRTVDesc)
{
	m_refTexture2D.StrongRef(pRTVDesc->m_pTexture2D);
	if (pRTVDesc->m_pTexture2D != nullptr)
	{
		auto hr = pCtx->mDevice->CreateRenderTargetView(((ID11Texture2D*)pRTVDesc->m_pTexture2D)->m_pDX11Texture2D, NULL, &m_pDX11RTV);
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