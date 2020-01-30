#include "IFrameBuffers.h"
#include "IRenderTargetView.h"
#include "IDepthStencilView.h"

#define new VNEW

NS_BEGIN

StructImpl(IFrameBuffersDesc)

IFrameBuffers::IFrameBuffers()
{
	memset(mRenderTargets, 0, sizeof(mRenderTargets));
	m_pDepthStencilView = nullptr;
}

IFrameBuffers::~IFrameBuffers()
{
	for (int i = 0; i < 8; i++)
	{
		Safe_Release(mRenderTargets[i]);
	}
	Safe_Release(m_pDepthStencilView);
}

void IFrameBuffers::BindRenderTargetView(UINT index, IRenderTargetView* rt)
{
	if(rt)
		rt->AddRef();
	Safe_Release(mRenderTargets[index]);
	mRenderTargets[index] = rt;
}

void IFrameBuffers::BindDepthStencilView(IDepthStencilView* ds)
{
	if (ds)
		ds->AddRef();
	Safe_Release(m_pDepthStencilView);
	m_pDepthStencilView = ds;
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpAPI2(EngineNS, IFrameBuffers, BindRenderTargetView, UINT, IRenderTargetView*);
	CSharpAPI1(EngineNS, IFrameBuffers, BindDepthStencilView, IDepthStencilView*);
	CSharpReturnAPI1(IRenderTargetView*, EngineNS, IFrameBuffers, GetRenderTargetView, UINT);
	CSharpReturnAPI0(IDepthStencilView*, EngineNS, IFrameBuffers, GetDepthStencilView);
}