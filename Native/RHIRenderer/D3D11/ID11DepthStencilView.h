#pragma once
#include "../IDepthStencilView.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11DepthStencilView : public IDepthStencilView
{
public:
	ID11DepthStencilView();
	~ID11DepthStencilView();
public:
	//D3D11_TEXTURE2D_DESC			mTextureDesc;
	//ID3D11Texture2D*				mTexture;
	//D3D11_DEPTH_STENCIL_VIEW_DESC	mViewDesc;
	ID3D11DepthStencilView*			m_pDX11DSV;
public:
	bool Init(ID11RenderContext* pCtx, const IDepthStencilViewDesc* pDesc);
};

NS_END