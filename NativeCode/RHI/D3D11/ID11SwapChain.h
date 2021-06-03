#pragma once
#include "../ISwapChain.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11Texture2D;
class ID11RenderContext;

class ID11SwapChain : public ISwapChain
{
public:
	ID11SwapChain();
	~ID11SwapChain();

	virtual ITexture2D* GetTexture2D() override;
	virtual void BindCurrent() override;
	virtual void Present(UINT SyncInterval, UINT Flags) override;

	virtual void OnLost() override;
	virtual vBOOL OnRestore(const ISwapChainDesc* desc) override;
public:
	TObjectHandle<ID11RenderContext> HostContext;
	DXGI_SWAP_CHAIN_DESC	 mSwapChainDesc;
	IDXGISwapChain*			mSwapChain;
	ID11Texture2D*			m_pSwapChainBuffer;
public:
	bool Init(ID11RenderContext* rc, const ISwapChainDesc* desc);
	DXGI_MODE_DESC SetupDXGI_MODE_DESC(UINT w, UINT h, EPixelFormat format) const;
};

NS_END