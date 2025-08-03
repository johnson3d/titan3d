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
	void IRenderPass::PushBeginBarrier(IGpuBufferData* buffer, EGpuResourceState state)
	{
		BeginBarriers->PushBarrier(buffer, state);
	}
}

NS_END