#pragma once
#include "IRenderResource.h"
#include "ITextureBase.h"

NS_BEGIN

struct TR_CLASS(SV_LayoutStruct = 8)
IDepthStencilViewDesc
{
	IDepthStencilViewDesc()
	{
		SetDefault();
	}
	void SetDefault()
	{
		Width = 0;
		Height = 0;
		Format = PXF_D24_UNORM_S8_UINT;
		CPUAccess = 0;
		mCanBeSampled = TRUE;
		mUseStencil = FALSE;
		Texture2D = nullptr;
		MipLevel = 1;
	}

	UINT					Width;
	UINT					Height;
	EPixelFormat			Format;
	UINT					CPUAccess;
	vBOOL					mCanBeSampled;
	vBOOL					mUseStencil;
	UINT					MipLevel;
	IGpuBuffer*				Texture2D;
};

class IShaderResourceView;
class TR_CLASS()
IDepthStencilView : public IRenderResource
{
public:
	IDepthStencilView();
	~IDepthStencilView();

	ITexture2D* GetTexture2D()
	{
		return m_refTexture2D;
	}
public:
	IDepthStencilViewDesc			mDesc;
	AutoRef<ITexture2D>	m_refTexture2D;
};

NS_END