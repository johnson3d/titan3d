#pragma once
#include "IRenderResource.h"
#include "../Math/v3dxColor4.h"
#include "ISwapChain.h"

NS_BEGIN

#define MAX_MRT_NUM 8

class IRenderTargetView;
class IDepthStencilView;
class ISwapChain;

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
IFrameBuffersDesc
{
	IFrameBuffersDesc()
	{
		IsSwapChainBuffer = FALSE;
		UseDSV = TRUE;
	}
	vBOOL		IsSwapChainBuffer;
	vBOOL		UseDSV;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
IFrameBuffers : public IRenderResource
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
	void SetSwapChain(ISwapChain* swapchain) {
		mSwapChain.StrongRef(swapchain);
	}
public:
	IFrameBuffersDesc					mDesc;
	AutoRef<ISwapChain>					mSwapChain;
	IRenderTargetView*					mRenderTargets[MAX_MRT_NUM];
	IDepthStencilView*					m_pDepthStencilView;
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
FrameBufferLoadAction
{
	LoadActionDontCare = 0,
	LoadActionLoad = 1,
	LoadActionClear = 2
};

enum TR_ENUM(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
FrameBufferStoreAction
{
	StoreActionDontCare = 0,
	StoreActionStore = 1,
	StoreActionMultisampleResolve = 2,
	StoreActionStoreAndMultisampleResolve = 3,
	StoreActionUnknown = 4
};

struct TR_CLASS(SV_LayoutStruct = 8)
RenderPassDesc
{
	FrameBufferLoadAction mFBLoadAction_Color;
	FrameBufferStoreAction mFBStoreAction_Color;
	v3dxColor4 mFBClearColorRT0;
	v3dxColor4 mFBClearColorRT1;
	v3dxColor4 mFBClearColorRT2;
	v3dxColor4 mFBClearColorRT3;
	v3dxColor4 mFBClearColorRT4;
	v3dxColor4 mFBClearColorRT5;
	v3dxColor4 mFBClearColorRT6;
	v3dxColor4 mFBClearColorRT7;

	FrameBufferLoadAction mFBLoadAction_Depth;
	FrameBufferStoreAction mFBStoreAction_Depth;
	float mDepthClearValue;

	FrameBufferLoadAction mFBLoadAction_Stencil;
	FrameBufferStoreAction mFBStoreAction_Stencil;
	UINT mStencilClearValue;

};

NS_END