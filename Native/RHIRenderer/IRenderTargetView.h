#pragma once
#include "IRenderResource.h"

NS_BEGIN

class ITexture2D;
struct IRenderTargetViewDesc
{
	IRenderTargetViewDesc()
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

class IRenderTargetView : public IRenderResource
{
public:
	AutoRef<ITexture2D>		m_refTexture2D;
public:
	IRenderTargetView();
	~IRenderTargetView();

	ITexture2D* GetTexture2D() 
	{
		return m_refTexture2D;
	}
};

NS_END