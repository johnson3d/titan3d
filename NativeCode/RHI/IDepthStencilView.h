#pragma once
#include "IRenderResource.h"
#include "ITextureBase.h"

NS_BEGIN

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IDepthStencilViewDesc
{
	/*IDepthStencilViewDesc()
	{
		SetDefault();
	}*/
	TR_FUNCTION()
	void SetDefault()
	{
		Width = 0;
		Height = 0;
		Format = PXF_D24_UNORM_S8_UINT;
		CPUAccess = 0;
		mCanBeSampled = TRUE;
		mUseStencil = FALSE;
		m_pTexture2D = nullptr;
	}

	UINT					Width;
	UINT					Height;
	EPixelFormat			Format;
	UINT					CPUAccess;
	vBOOL					mCanBeSampled;
	vBOOL					mUseStencil;
	ITexture2D*				m_pTexture2D;
};

class IShaderResourceView;
class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IDepthStencilView : public IRenderResource
{
public:
	IDepthStencilView();
	~IDepthStencilView();

	TR_FUNCTION()
	ITexture2D* GetTexture2D()
	{
		return m_refTexture2D;
	}
public:
	IDepthStencilViewDesc			mDesc;
	AutoRef<ITexture2D>	m_refTexture2D;
};

NS_END