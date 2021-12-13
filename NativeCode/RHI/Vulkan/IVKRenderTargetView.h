#pragma once
#include "../IRenderTargetView.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKTexture2D;
class IVKShaderResourceView;
class IVKRenderTargetView : public IRenderTargetView
{
public:
	IVKRenderTargetView();
	~IVKRenderTargetView();
	bool Init(IVKRenderContext* rc, const IRenderTargetViewDesc* desc);
public:
	AutoRef<IVKShaderResourceView>		mTextureSRV;
};

NS_END