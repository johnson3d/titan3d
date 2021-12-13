#pragma once
#include "../ISwapChain.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class IGLSwapChain : public ISwapChain
{
public:
	IGLSwapChain();
	~IGLSwapChain();
	virtual void Cleanup() override;

	virtual UINT GetBackBufferNum() override {
		return 1;
	}
	virtual ITexture2D* GetBackBuffer(UINT index) override {
		return nullptr;
	}
	virtual void BindCurrent() override;
	virtual void Present(UINT SyncInterval, UINT Flags) override;

	virtual void OnLost() override;
	virtual vBOOL OnRestore(const ISwapChainDesc* desc) override;
public:
	ISwapChainDesc			mDesc;
	TObjectHandle<IGLRenderContext> mRenderContext;
#if defined(PLATFORM_WIN)
	HDC					mDC;
#elif defined(PLATFORM_DROID)
	EGLConfig			mConfig;
	EGLSurface			mEglSurface;
#elif defined(PLATFORM_IOS)

#endif
public:
	bool Init(IGLRenderContext* rc, const ISwapChainDesc* desc);
};

NS_END