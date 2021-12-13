#pragma once
#include "../IDepthStencilView.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKTexture2D;
class IVKShaderResourceView;
class IVKDepthStencilView : public IDepthStencilView
{
public:
	IVKDepthStencilView();
	~IVKDepthStencilView();
	bool Init(IVKRenderContext* rc, const IDepthStencilViewDesc* desc);
public:
	AutoRef<IVKShaderResourceView>		mTextureSRV;
};

NS_END