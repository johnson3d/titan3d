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
	Cpp2CS2(EngineNS, IFrameBuffers, BindRenderTargetView);
	Cpp2CS1(EngineNS, IFrameBuffers, BindDepthStencilView);
	Cpp2CS1(EngineNS, IFrameBuffers, GetRenderTargetView);
	Cpp2CS0(EngineNS, IFrameBuffers, GetDepthStencilView);
}