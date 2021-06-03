#pragma once
#include "IRenderResource.h"

NS_BEGIN

class ITexture2D;
struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IRenderTargetViewDesc
{
	IRenderTargetViewDesc()
	{
		SetDefault();
	}
	TR_FUNCTION()
	void SetDefault()
	{
		m_pTexture2D = nullptr;
		mCanBeSampled = TRUE;
		Width = 0;
		Height = 0;
		Format = PXF_R8G8B8A8_UNORM;
	}
	ITexture2D*		m_pTexture2D;
	vBOOL						mCanBeSampled;
	UINT						Width;
	UINT						Height;
	EPixelFormat				Format;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IRenderTargetView : public IRenderResource
{
public:
	AutoRef<ITexture2D>		m_refTexture2D;
public:
	IRenderTargetView();
	~IRenderTargetView();

	TR_FUNCTION()
	ITexture2D* GetTexture2D() 
	{
		return m_refTexture2D;
	}
};

NS_END