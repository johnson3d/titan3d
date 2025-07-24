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
		BeginCopyDraws->CopyDraws.push_back(draw);
	}
	void IRenderPass::PushBeginBarrier(IGpuBufferData* buffer, EGpuResourceState state)
	{
		//ASSERT(state != EGpuResourceState::GRS_CopyDst);
		FBarrierDesc tmp(buffer, state);
		for (const auto& i : BeginBarriers->Barriers)
		{
			if (i.Buffer == buffer)
			{
				if (i.ToState != state)
				{
					ASSERT(false);
				}
				else
				{
					return;
				}
			}
		}
		BeginBarriers->Barriers.push_back(tmp);
	}
}

NS_END