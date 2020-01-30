#pragma once
#include "../IDepthStencilView.h"
#include "GLPreHead.h"
#include "IGLTexture2D.h"

NS_BEGIN

class IGLRenderContext;
class IGLDepthStencilView : public IDepthStencilView
{
public:
	IGLDepthStencilView();
	~IGLDepthStencilView();
	virtual void Cleanup() override;

public:
	bool Init(IGLRenderContext* rc, const IDepthStencilViewDesc* desc);
//private:
//	IGLTexture2D* m_pTex2d;
//	IGLShaderResourceView* m_pSRV;

};

NS_END