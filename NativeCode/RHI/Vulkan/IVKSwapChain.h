#pragma once
#include "../ISwapChain.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKShaderResourceView;
class IVKTexture2D;
class IVKRenderTargetView;
class IVKSwapChain : public ISwapChain
{
	const int MAX_FRAMES_IN_FLIGHT = 2;
public:
	IVKSwapChain();
	~IVKSwapChain();
	void Cleanup();

	virtual UINT GetBackBufferNum() override;
	virtual ITexture2D* GetBackBuffer(UINT index) override;
	virtual void BindCurrent() override;
	virtual void Present(UINT SyncInterval, UINT Flags) override;

	virtual void OnLost() override;
	virtual vBOOL OnRestore(const ISwapChainDesc* desc) override;

	inline IVKShaderResourceView* GetBackSRV(UINT index) {
		return mSwapChainTextures[index];
	}
public:
	VkSurfaceKHR						mSurface;
	VkSwapchainKHR						mSwapChain;	
	std::vector<IVKShaderResourceView*>	mSwapChainTextures;
	std::vector<VkSemaphore>			mAvailableSemaphores;
	UINT								mCurrentFrame;
	UINT								mCurrentImageIndex;

	TObjectHandle<IVKRenderContext>		mRenderContext;
	bool Init(IVKRenderContext* rc, const ISwapChainDesc* desc);

	bool CheckSwapSurfaceFormat(const VkSurfaceFormatKHR& format, const std::vector<VkSurfaceFormatKHR>& availableFormats);
};

NS_END