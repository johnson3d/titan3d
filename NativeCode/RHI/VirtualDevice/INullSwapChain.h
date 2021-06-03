#pragma once
#include "../ISwapChain.h"
#include "NullPreHead.h"

NS_BEGIN

class INullRenderContext;
class INullShaderResourceView;
class INullSwapChain : public ISwapChain
{
public:
	INullSwapChain();
	~INullSwapChain();

	virtual ITexture2D* GetTexture2D() override;
	virtual void BindCurrent() override;
	virtual void Present(UINT SyncInterval, UINT Flags) override;

	virtual void OnLost() override;
	virtual vBOOL OnRestore(const ISwapChainDesc* desc) override;

public:
	TObjectHandle<INullRenderContext>		mRenderContext;
	bool Init(INullRenderContext* rc, const ISwapChainDesc* desc);
};

NS_END