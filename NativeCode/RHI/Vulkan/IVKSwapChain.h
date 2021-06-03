#pragma once
#include "../ISwapChain.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKShaderResourceView;
class IVKSwapChain : public ISwapChain
{
public:
	IVKSwapChain();
	~IVKSwapChain();

	virtual ITexture2D* GetTexture2D() override;
	virtual void BindCurrent() override;
	virtual void Present(UINT SyncInterval, UINT Flags) override;

	virtual void OnLost() override;
	virtual vBOOL OnRestore(const ISwapChainDesc* desc) override;

public:
	VkSurfaceKHR						mSurface;
	VkSwapchainKHR						mSwapChain;
	std::vector<IVKShaderResourceView*>	mSwapChainTextures;

	TObjectHandle<IVKRenderContext>		mRenderContext;
	bool Init(IVKRenderContext* rc, const ISwapChainDesc* desc);

	bool CheckSwapSurfaceFormat(const VkSurfaceFormatKHR& format, const std::vector<VkSurfaceFormatKHR>& availableFormats);
};

NS_END