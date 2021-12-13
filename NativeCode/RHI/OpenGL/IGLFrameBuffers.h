#pragma once
#include "../IFrameBuffers.h"
#include "GLPreHead.h"

NS_BEGIN

class IGLRenderContext;
class GLSdk;
class IGLFrameBuffers : public IFrameBuffers
{
public:
	IGLFrameBuffers();
	~IGLFrameBuffers();
	virtual void Cleanup() override;

	virtual void BindRenderTargetView(UINT index, IRenderTargetView* rt) override;
	virtual void BindDepthStencilView(IDepthStencilView* ds) override;
private:
	bool				mNeedUpdate;
public:
	std::shared_ptr<GLSdk::GLBufferId>	mFrameBufferId;
public:
	bool Init(IGLRenderContext* rc, const IFrameBuffersDesc* desc);
	void ApplyBuffers(GLSdk* sdk);
};

class IGLRenderPass : public IRenderPass
{
public:
	IGLRenderPass()
	{

	}
	~IGLRenderPass()
	{

	}
	bool Init(IGLRenderContext* rc, const IRenderPassDesc* desc)
	{
		mDesc = *desc;
		return true;
	}
};

NS_END