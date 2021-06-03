#pragma once
#include "IRenderResource.h"
#include "../Math/v3dxColor4.h"

NS_BEGIN

#define MAX_MRT_NUM 8

class IRenderTargetView;
class IDepthStencilView;

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
	TR_FUNCTION()
	virtual void BindRenderTargetView(UINT index, IRenderTargetView* rt);
	TR_FUNCTION()
	virtual void BindDepthStencilView(IDepthStencilView* ds);
	TR_FUNCTION()
	IRenderTargetView* GetRenderTargetView(UINT index) {
		return mRenderTargets[index];
	}
	TR_FUNCTION()
	IDepthStencilView* GetDepthStencilView() {
		return m_pDepthStencilView;
	}
public:
	IFrameBuffersDesc					mDesc;
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

struct TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct = 8)
RenderPassDesc
{
	FrameBufferLoadAction mFBLoadAction_Color;
	FrameBufferStoreAction mFBStoreAction_Color;
	TR_MEMBER(SV_ReturnConverter=v3dVector4_t)
	v3dxColor4 mFBClearColorRT0;
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4 mFBClearColorRT1;
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4 mFBClearColorRT2;
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4 mFBClearColorRT3;
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4 mFBClearColorRT4;
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4 mFBClearColorRT5;
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4 mFBClearColorRT6;
	TR_MEMBER(SV_ReturnConverter = v3dVector4_t)
	v3dxColor4 mFBClearColorRT7;

	FrameBufferLoadAction mFBLoadAction_Depth;
	FrameBufferStoreAction mFBStoreAction_Depth;
	float mDepthClearValue;

	FrameBufferLoadAction mFBLoadAction_Stencil;
	FrameBufferStoreAction mFBStoreAction_Stencil;
	UINT mStencilClearValue;

};

NS_END