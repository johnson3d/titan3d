#pragma once
#include "../IFrameBuffers.h"
#include "VKPreHead.h"

NS_BEGIN

class IVKRenderContext;
class IVKCommandList;
class IVKShaderResourceView;
class IVKFrameBuffers : public IFrameBuffers
{
public:
	IVKFrameBuffers();
	~IVKFrameBuffers();

	bool Init(IVKRenderContext* rc, const IFrameBuffersDesc* desc);
	virtual void BindSwapChain(UINT index, ISwapChain* swapchain) override;
	virtual bool UpdateFrameBuffers(IRenderContext* rc, float width, float height) override;

	VkFramebuffer GetFrameBuffer();

	bool UpdateFrameBuffersWithSwapChain(IVKRenderContext* rc, IRenderPass* renderPass, UINT iBackBuffer, float width, float height);
	void OnBeginPass(IVKRenderContext* rc, IVKCommandList* cmdlist);
	void OnEndPass(IVKRenderContext* rc, IVKCommandList* cmdlist);
public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	//VkFramebuffer			mFrameBuffer;
	std::vector<AutoRef<IVKShaderResourceView>>		mReferSRV;
	std::vector<VkFramebufferCreateInfo>	mFramebufferInfos;
	std::vector<VkFramebuffer>			mFrameBuffers;
	UINT								Width;
	UINT								Height;
};

class IVKRenderPass : public IRenderPass
{
public:
	IVKRenderPass();
	~IVKRenderPass();
	bool Init(IVKRenderContext* rc, const IRenderPassDesc* desc);

public:
	TObjectHandle<IVKRenderContext>		mRenderContext;
	VkRenderPass						mRenderPass;
};

NS_END