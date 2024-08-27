#pragma once
#include "NxGpuDevice.h"
#include "NxBuffer.h"
#include "../Math/v3dxColor4.h"

NS_BEGIN

namespace NxRHI
{
	const int C_MAX_MRT_NUM = 8;

	class IRenderTargetView;
	class IDepthStencilView;
	enum TR_ENUM()
		EFrameBufferLoadAction
	{
		LoadActionDontCare = 0,
			LoadActionLoad = 1,
			LoadActionClear = 2
	};

	enum TR_ENUM()
		EFrameBufferStoreAction
	{
		StoreActionDontCare = 0,
			StoreActionStore = 1,
			StoreActionMultisampleResolve = 2,
			StoreActionStoreAndMultisampleResolve = 3,
			StoreActionUnknown = 4
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FAttachmentDesc
	{
		FAttachmentDesc()
		{
			SetDefault();
		}
		void SetDefault()
		{
			Format = EPixelFormat::PXF_B8G8R8A8_UNORM;
			Samples = 1;
			IsSwapChain = FALSE;
			Unused = 0;
			LoadAction = EFrameBufferLoadAction::LoadActionDontCare;
			StoreAction = EFrameBufferStoreAction::StoreActionStore;

			StencilLoadAction = EFrameBufferLoadAction::LoadActionDontCare;
			StencilStoreAction = EFrameBufferStoreAction::StoreActionStore;
		}
		EPixelFormat			Format;
		UINT					Samples;
		vBOOL					IsSwapChain;
		UINT					Unused;

		EFrameBufferLoadAction	LoadAction;
		EFrameBufferStoreAction	StoreAction;
		EFrameBufferLoadAction	StencilLoadAction;
		EFrameBufferStoreAction	StencilStoreAction;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FViewInstanceLocation
	{
		void SetDefault()
		{
			ViewportArrayIndex = 0;
			RenderTargetArrayIndex = 0;
		}
		UINT ViewportArrayIndex;
		UINT RenderTargetArrayIndex;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FViewInstanceDesc
	{
		void SetDefault()
		{
			ViewInstanceCount = 0;
			pViewInstanceLocations = nullptr;
		}
		UINT ViewInstanceCount;
		const FViewInstanceLocation* pViewInstanceLocations;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FRenderPassDesc
	{
		FRenderPassDesc()
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

			ViewInstanceDesc.SetDefault();
		}
		UINT NumOfMRT;
		FAttachmentDesc AttachmentMRTs[C_MAX_MRT_NUM];
		FAttachmentDesc AttachmentDepthStencil;

		FViewInstanceDesc ViewInstanceDesc{};
	};

	enum TR_ENUM()
		ERenderPassClearFlags
	{
		CLEAR_NONE = 0,
		CLEAR_DEPTH = 1,
		CLEAR_STENCIL = (1 << 1),
		CLEAR_RT0 = (1 << 2),
		CLEAR_RT1 = (1 << 3),
		CLEAR_RT2 = (1 << 4),
		CLEAR_RT3 = (1 << 5),
		CLEAR_RT4 = (1 << 6),
		CLEAR_RT5 = (1 << 7),
		CLEAR_RT6 = (1 << 8),
		CLEAR_RT7 = (1 << 9),

		CLEAR_ALL = 0xffffffff,
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FRenderPassClears
	{
		FRenderPassClears()
		{
			SetDefault();
		}
		void SetDefault()
		{
			for (int i = 0; i < 8; i++)
			{
				ClearColor[i] = v3dxColor4(0, 0, 0, 0);
			}
			DepthClearValue = 1.0f;
			StencilClearValue = 0;
			ClearFlags = ERenderPassClearFlags::CLEAR_ALL;
		}
		void SetClearColor(UINT index, const v3dxColor4 * color) {
			ClearColor[index] = *color;
		}
		ERenderPassClearFlags	ClearFlags = ERenderPassClearFlags::CLEAR_ALL;
		v3dxColor4				ClearColor[C_MAX_MRT_NUM];
		float					DepthClearValue;
		UINT					StencilClearValue;
	};

	class TR_CLASS()
		IRenderPass : public IGpuResource
	{
	public:
		ENGINE_RTTI(IRenderPass);
		FRenderPassDesc			Desc;

		void SetViewInstanceLocations()
		{
			ViewInstanceLocations.clear();
			for (size_t i = 0; i < Desc.ViewInstanceDesc.ViewInstanceCount; i++)
			{
				ViewInstanceLocations.push_back(Desc.ViewInstanceDesc.pViewInstanceLocations[i]);
			}
			Desc.ViewInstanceDesc.pViewInstanceLocations = ViewInstanceLocations.data();
		}
		std::vector<FViewInstanceLocation> ViewInstanceLocations;
	};


	struct TR_CLASS(SV_LayoutStruct = 8)
		FFrameBuffersDesc
	{
		FFrameBuffersDesc()
		{
			RenderPass = nullptr;
		}
		IRenderPass* RenderPass;
		bool HasSwapchain() const
		{
			for (UINT i = 0; i < RenderPass->Desc.NumOfMRT; i++)
			{
				if (RenderPass->Desc.AttachmentMRTs[i].IsSwapChain)
					return true;
			}
			return false;
		}
	};
	class TR_CLASS()
		IFrameBuffers : public IGpuResource
	{
	public:
		ENGINE_RTTI(IFrameBuffers);

		virtual void BindRenderTargetView(UINT index, IRenderTargetView * rt);
		virtual void BindDepthStencilView(IDepthStencilView * ds);
		virtual void FlushModify() = 0;

		IRenderPass* GetRenderPass() {
			return mRenderPass;
		}
		IRenderTargetView* GetRtv(UINT index) {
			return mRenderTargets[index];
		}
		IDepthStencilView* GetDsv() {
			return mDepthStencilView;
		}
	public:
		AutoRef<IRenderPass>			mRenderPass;
		AutoRef<IRenderTargetView>		mRenderTargets[C_MAX_MRT_NUM];
		AutoRef<IDepthStencilView>		mDepthStencilView;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FViewPort
	{
	public:
		FViewPort()
		{
			TopLeftX = 0;
			TopLeftY = 0;
			Width = 0;
			Height = 0;
			MinDepth = 0;
			MaxDepth = 1.0F;
		}
		TR_DECL(FViewPort);

		float TopLeftX;
		float TopLeftY;
		float Width;
		float Height;
		float MinDepth;
		float MaxDepth;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FScissorRect
	{
		FScissorRect()
		{
			MinX = 0;
			MinY = 0;
			MaxX = 1;
			MaxY = 1;
		}
		TR_DECL(FScissorRect);
		int MinX;
		int MinY;
		int MaxX;
		int MaxY;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FSwapChainDesc
	{
		void SetDefault()
		{
			Width = 1;
			Height = 1;
			Format = PXF_R8G8B8A8_UNORM;
			SampleDesc.Count = 1;
			SampleDesc.Quality = 0;
			BufferCount = 1;
			OutputWindow = nullptr;
			Windowed = TRUE;
		}
		UINT Width;
		UINT Height;
		EPixelFormat Format;
		FSamplerMode SampleDesc;
		//DXGI_USAGE BufferUsage;
		UINT BufferCount;
		void* OutputWindow;
		vBOOL Windowed;
		//DXGI_SWAP_EFFECT SwapEffect;
		//UINT Flags;
	};
	class TR_CLASS()
		ISwapChain : public IGpuResource
	{
	public:
		ENGINE_RTTI(ISwapChain);

		virtual UINT GetBackBufferCount() const = 0;
		virtual ITexture* GetBackBuffer(UINT index) = 0;
		virtual IRenderTargetView* GetBackRTV(UINT index) = 0;
		virtual UINT GetCurrentBackBuffer() = 0;
		virtual void BeginFrame() {}
		virtual void Present(IGpuDevice* device, UINT SyncInterval, UINT Flags) = 0;
		virtual bool Resize(IGpuDevice * device, UINT w, UINT h) = 0;
	public:
		FSwapChainDesc			Desc;
	};
}

NS_END