#include "NxFrameBuffers.h"
#include "NxBuffer.h"

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
}

NS_END