#pragma once
#include "IRenderResource.h"
#include "../Math/v3dxColor4.h"
#include "ISwapChain.h"

NS_BEGIN

#define MAX_MRT_NUM 8

class IRenderTargetView;
class IDepthStencilView;
class ISwapChain;
class IRenderPass;


struct TR_CLASS(SV_LayoutStruct = 8)
	IIAttachmentDesc
{
	IIAttachmentDesc()
	{
		SetDefault();
	}
	void SetDefault()
	{
		Format = EPixelFormat::PXF_B8G8R8A8_UNORM;
		Samples = 1;
		IsSwapChain = FALSE;
		Unused = 0;
		LoadAction = FrameBufferLoadAction::LoadActionDontCare;
		StoreAction = FrameBufferStoreAction::StoreActionStore;

		StencilLoadAction = FrameBufferLoadAction::LoadActionDontCare;
		StencilStoreAction = FrameBufferStoreAction::StoreActionStore;
	}
	EPixelFormat			Format;
	UINT					Samples;
	vBOOL					IsSwapChain;
	UINT					Unused;

	FrameBufferLoadAction	LoadAction;
	FrameBufferStoreAction	StoreAction;
	FrameBufferLoadAction	StencilLoadAction;
	FrameBufferStoreAction	StencilStoreAction;
};

struct TR_CLASS(SV_LayoutStruct = 8)
	IRenderPassDesc
{
	IRenderPassDesc()
	{
		SetDefault();
	}
	void SetDefault()
	{
		NumOfMRT = 0;
		for (int i = 0; i < 8; i++)
		{
			AttachmentMRTs[i].SetDefault();
		}
		AttachmentDepthStencil.Format = EPixelFormat::PXF_UNKNOWN;
	}
	UINT NumOfMRT;
	IIAttachmentDesc AttachmentMRTs[8];
	IIAttachmentDesc AttachmentDepthStencil;
};

struct TR_CLASS(SV_LayoutStruct = 8)
	IRenderPassClears
{
	IRenderPassClears()
	{
		SetDefault();
	}
	void SetDefault()
	{
		for (int i = 0; i < 8; i++)
		{
			ClearColor[i] = v3dxColor4(1, 0, 0, 0);
		}
		DepthClearValue = 1.0f;
		StencilClearValue = 0;
	}
	void SetClearColor(UINT index, const v3dxColor4 * color) {
		ClearColor[index] = *color;
	}
	v3dxColor4				ClearColor[8];
	float					DepthClearValue;
	UINT					StencilClearValue;
};

class TR_CLASS()
	IRenderPass : public IRenderResource
{
public:
	IRenderPassDesc			mDesc;
};


struct TR_CLASS(SV_LayoutStruct = 8)
IFrameBuffersDesc
{
	IFrameBuffersDesc()
	{
		RenderPass = nullptr;
	}
	IRenderPass*	RenderPass;
	bool HasSwapchain() const
	{
		for (UINT i = 0; i < RenderPass->mDesc.NumOfMRT; i++)
		{
			if (RenderPass->mDesc.AttachmentMRTs[i].IsSwapChain)
				return true;
		}
		return false;
	}
};

class TR_CLASS()
IFrameBuffers : public IRenderResource
{
public:
	IFrameBuffers();
	~IFrameBuffers();
	virtual void BindSwapChain(UINT index, ISwapChain * swapchain)
	{
		mSwapChainIndex = index;
		mSwapChain.StrongRef(swapchain);
	}
	virtual void BindRenderTargetView(UINT index, IRenderTargetView* rt);
	virtual void BindDepthStencilView(IDepthStencilView* ds);
	IRenderTargetView* GetRenderTargetView(UINT index) {
		return mRenderTargets[index];
	}
	IDepthStencilView* GetDepthStencilView() {
		return m_pDepthStencilView;
	}	
	virtual bool UpdateFrameBuffers(IRenderContext* rc, float width, float height) { return true; }
public:
	IFrameBuffersDesc					mDesc;
	AutoRef<IRenderPass>				mRenderPass;
	AutoRef<ISwapChain>					mSwapChain;
	INT									mSwapChainIndex;
	IRenderTargetView*					mRenderTargets[MAX_MRT_NUM];
	IDepthStencilView*					m_pDepthStencilView;
};

NS_END