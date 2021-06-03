#pragma once
#include "../IDepthStencilView.h"
#include "MTLRenderContext.h"

NS_BEGIN

class MtlContext;

class MtlDepthStencilView : public IDepthStencilView
{
public:
	MtlDepthStencilView();
	~MtlDepthStencilView();

public:
	bool Init(MtlContext* pCtx, const IDepthStencilViewDesc* pDesc);
public:
	id<MTLTexture> m_pMtlStencilBuffer;
};

NS_END