#pragma once
#include "IRenderResource.h"

NS_BEGIN

#define MAX_MRT_NUM 8

class IRenderTargetView;
class IDepthStencilView;

struct IFrameBuffersDesc
{
	IFrameBuffersDesc()
	{
		IsSwapChainBuffer = FALSE;
		UseDSV = TRUE;
	}
	vBOOL		IsSwapChainBuffer;
	vBOOL		UseDSV;
};

StructBegin(IFrameBuffersDesc, EngineNS)
	StructMember(IsSwapChainBuffer);
	StructMember(UseDSV);
StructEnd(void)





class IFrameBuffers : public IRenderResource
{
public:
	IFrameBuffers();
	~IFrameBuffers();
	virtual void BindRenderTargetView(UINT index, IRenderTargetView* rt);
	virtual void BindDepthStencilView(IDepthStencilView* ds);
	IRenderTargetView* GetRenderTargetView(UINT index) {
		return mRenderTargets[index];
	}
	IDepthStencilView* GetDepthStencilView() {
		return m_pDepthStencilView;
	}
public:
	IFrameBuffersDesc					mDesc;
	IRenderTargetView*					mRenderTargets[MAX_MRT_NUM];
	IDepthStencilView*					m_pDepthStencilView;
};

enum FrameBufferLoadAction
{
	LoadActionDontCare = 0,
	LoadActionLoad = 1,
	LoadActionClear = 2
};

enum FrameBufferStoreAction
{
	StoreActionDontCare = 0,
	StoreActionStore = 1,
	StoreActionMultisampleResolve = 2,
	StoreActionStoreAndMultisampleResolve = 3,
	StoreActionUnknown = 4
};

struct FrameBufferClearColor
{
	float r;
	float g;
	float b;
	float a;
};

struct RenderPassDesc
{
	FrameBufferLoadAction mFBLoadAction_Color;
	FrameBufferStoreAction mFBStoreAction_Color;
	FrameBufferClearColor mFBClearColorRT0;
	FrameBufferClearColor mFBClearColorRT1;
	FrameBufferClearColor mFBClearColorRT2;
	FrameBufferClearColor mFBClearColorRT3;
	FrameBufferClearColor mFBClearColorRT4;
	FrameBufferClearColor mFBClearColorRT5;
	FrameBufferClearColor mFBClearColorRT6;
	FrameBufferClearColor mFBClearColorRT7;

	FrameBufferLoadAction mFBLoadAction_Depth;
	FrameBufferStoreAction mFBStoreAction_Depth;
	float mDepthClearValue;

	FrameBufferLoadAction mFBLoadAction_Stencil;
	FrameBufferStoreAction mFBStoreAction_Stencil;
	UINT32 mStencilClearValue;

};

NS_END