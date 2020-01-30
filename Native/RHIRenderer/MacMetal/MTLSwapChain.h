#pragma once
#include "../ISwapChain.h"
#include "MTLRenderContext.h"
#include "MTLTexture2D.h"

NS_BEGIN

class MtlContext;

class MtlSwapChain : public ISwapChain
{
public:
	MtlSwapChain();
	~MtlSwapChain();

	virtual ITexture2D* GetTexture2D() override;
	virtual void BindCurrent() override;
	virtual void Present(UINT SyncInterval, UINT Flags) override;

	virtual void OnLost() override;
	virtual vBOOL OnRestore(const ISwapChainDesc* desc) override;

public:
	bool Init(MtlContext* pCtx, const ISwapChainDesc* pDesc);

public:
	CAMetalLayer* m_pMtlLayer;
	MtlTexture2D* m_pSwapChainBuffer;
	
};

NS_END