#pragma once
#include "../IFrameBuffers.h"

NS_BEGIN

class ID11RenderContext;
class ID11FrameBuffers : public IFrameBuffers
{
public:
	ID11FrameBuffers();
	~ID11FrameBuffers();

	virtual void BindSwapChain(UINT index, ISwapChain* swapchain) override;
public:
	bool Init(ID11RenderContext* rc, const IFrameBuffersDesc* desc);

	TObjectHandle<ID11RenderContext>		mRenderContext;
};

class ID11RenderPass : public IRenderPass
{
public:
	ID11RenderPass()
	{

	}
	~ID11RenderPass()
	{

	}
	bool Init(ID11RenderContext* rc, const IRenderPassDesc* desc)
	{
		mDesc = *desc;
		return true;
	}
};

NS_END