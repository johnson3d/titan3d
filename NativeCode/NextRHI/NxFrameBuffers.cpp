#include "NxFrameBuffers.h"
#include "NxBuffer.h"
#include "NxDrawcall.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	void IFrameBuffers::BindRenderTargetView(UINT index, IRenderTargetView* rt)
	{
		mRenderTargets[index] = rt;
	}
	void IFrameBuffers::BindDepthStencilView(IDepthStencilView* ds)
	{
		mDepthStencilView = ds;
	}

	IRenderPass::IRenderPass()
	{
		BeginBarriers = MakeWeakRef(new IBarriersDraw());
		BeginCopyDraws = MakeWeakRef(new IRenderPassCopyDraw());
	}
	IRenderPass::~IRenderPass()
	{
		BeginBarriers = nullptr;
		BeginCopyDraws = nullptr;
	}
	void IRenderPass::PushBeginCopyDraw(ICopyDraw* draw)
	{
		BeginCopyDraws->PushCopyDraw(draw);
	}
	void IRenderPass::PushBeginBarrier(IGpuBufferDataBase* buffer, const FSubresourceRange& range, EGpuResourceState state)
	{
		BeginBarriers->PushBarrier(buffer, range, state);
	}

	void ISwapChain::BeginBackFrameBuffers(ICommandList* cmd, IFrameBuffers* fb)
	{
		auto rtv = fb ? fb->GetRtv(0) : nullptr;
		if (rtv == nullptr || rtv->GetTexture() == nullptr)
			return;
		rtv->GetTexture()->TransitionTo(cmd, EGpuResourceState::GRS_RenderTarget);
	}
	void ISwapChain::EndBackFrameBuffers(ICommandList* cmd, IFrameBuffers* fb)
	{
		auto rtv = fb ? fb->GetRtv(0) : nullptr;
		if (rtv == nullptr || rtv->GetTexture() == nullptr)
			return;
		rtv->GetTexture()->TransitionTo(cmd, EGpuResourceState::GRS_Present);
	}
}

NS_END