#pragma once
#include "../Base/IUnknown.h"
#include "../Base/thread/vfxcritical.h"
#include "NxEvent.h"

NS_BEGIN

namespace NxRHI
{
	class IGpuDevice;
	struct FBufferDesc;
	class IBuffer;
	class ICommandList;
	struct FCbvDesc;
	class ICbView;
	struct FVbvDesc;
	class IVbView;
	struct FIbvDesc;
	class IIbView;
	struct FUavDesc;
	class IUaView;
	struct FSrvDesc;
	class ISrView;
	struct FSamplerDesc;
	class ISampler;
	struct FGpuPipelineDesc;
	class IShaderEffect;
	class IGpuPipeline;
	class IGpuDrawState;
	struct FInputLayoutDesc;
	class IInputLayout;
	struct FShaderDesc;
	class IShader;
	struct FFenceDesc;
	class IFence;
	struct FEventDesc;
	class IEvent;
	class IGraphicDraw;
	class IComputeDraw;
	class ICopyDraw;
	struct FTextureDesc;
	class ITexture;
	class IGpuResource;
	struct FRtvDesc;
	class IRenderTargetView;
	struct FDsvDesc;
	class IDepthStencilView;
	struct FSwapChainDesc;
	class ISwapChain;
	struct FRenderPassDesc;
	class IRenderPass;
	class IFrameBuffers;
	struct FGpuDeviceDesc;
	class IGpuDevice;
	class FGpuPipelineManager;
	class ICmdQueue;
	class IComputeEffect;
	class IGpuBufferData;

	enum TR_ENUM(SV_EnumNoFlags)
		ERhiType
	{
		RHI_D3D11,
			RHI_D3D12,
			RHI_VK,
			RHI_GL,
			RHI_Metal,
			RHI_VirtualDevice,
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FGpuSystemDesc
	{
		vBOOL		UseRenderDoc = TRUE;
		vBOOL		CreateDebugLayer = FALSE;
		void*		WindowHandle = nullptr;
	};
	class TR_CLASS()
		IGpuSystem : public VIUnknown
	{
	public:
		static IGpuSystem* CreateGpuSystem(ERhiType type, const FGpuSystemDesc* desc);
		virtual bool InitGpuSystem(ERhiType type, const FGpuSystemDesc * desc) { return true; }
		virtual IGpuDevice* CreateDevice(const FGpuDeviceDesc * desc) = 0;
		virtual int GetNumOfGpuDevice() const{
			return 1;
		}
	public:
		ERhiType		Type = ERhiType::RHI_D3D11;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FGpuDeviceDesc
	{
		void SetDefault()
		{
			RhiType = ERhiType::RHI_D3D11;
			AdapterId = 0;
			DeviceHandle = nullptr;
			DeviceContextHandle = nullptr;
			CreateDebugLayer = true;
		}
		ERhiType	RhiType = ERhiType::RHI_D3D11;
		int		AdapterId = 0;
		void*	DeviceHandle = nullptr;
		void*	DeviceContextHandle = nullptr;
		bool	CreateDebugLayer = true;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FGpuDeviceCaps
	{
		bool	IsSupportFence = false;
		bool	IsSupportSSBO_VS = true;
		bool	IsSupoortBufferToTexture = false;
		bool	Unused = false;
		UINT			NumOfSwapchainFormats = 0;
		EPixelFormat	SwapchainFormats[16] = {};
		bool IsSupportSwapchainFormat(EPixelFormat format) const
		{
			for (UINT i = 0; i < NumOfSwapchainFormats; i++)
			{
				if (SwapchainFormats[i] == format)
				{
					return true;
				}
			}
			return false;
		}
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FGpuResourceAlignment
	{
		UINT CBufferAlignment = 1;
		UINT VbIbAlignment = 1;
		UINT SsbAlignment = 1;
		UINT TexturePitchAlignment = 1;
		UINT TextureAlignment = 1;
		UINT MsaaAlignment = 1;
		UINT RawSrvUavAlignment = 1;
		UINT UavCounterAlignment = 1;
	};
	template<class _DestroyType>
	struct AuxGpuResourceDestroyer
	{
		static void Destroy(_DestroyType obj, IGpuDevice* device)
		{

		}
	};
	class TR_CLASS()
		IGpuDevice : public VIUnknown
	{
	public:
		ENGINE_RTTI(IGpuDevice);
		IGpuDevice();
		virtual bool InitDevice(IGpuSystem * pGpuSystem, const FGpuDeviceDesc * desc) = 0;
		virtual IBuffer* CreateBuffer(const FBufferDesc * desc) = 0;
		virtual ITexture* CreateTexture(const FTextureDesc * desc) = 0;
		virtual ICbView* CreateCBV(IBuffer* pBuffer, const FCbvDesc * desc) = 0;
		virtual IVbView* CreateVBV(IBuffer* pBuffer, const FVbvDesc * desc) = 0;
		virtual IIbView* CreateIBV(IBuffer* pBuffer, const FIbvDesc * desc) = 0;
		virtual ISrView* CreateSRV(IGpuBufferData * pBuffer, const FSrvDesc * desc) = 0;
		virtual IUaView* CreateUAV(IGpuBufferData * pBuffer, const FUavDesc * desc) = 0;
		virtual IRenderTargetView* CreateRTV(ITexture * pBuffer, const FRtvDesc* desc) = 0;
		virtual IDepthStencilView* CreateDSV(ITexture * pBuffer, const FDsvDesc* desc) = 0;
		virtual ISampler* CreateSampler(const FSamplerDesc * desc) = 0;
		virtual ISwapChain* CreateSwapChain(const FSwapChainDesc * desc) = 0;
		virtual IRenderPass* CreateRenderPass(const FRenderPassDesc * desc) = 0;
		virtual IFrameBuffers* CreateFrameBuffers(IRenderPass* rpass) = 0;
		
		virtual IGpuPipeline* CreatePipeline(const FGpuPipelineDesc * desc) = 0;
		virtual IGpuDrawState* CreateGpuDrawState() = 0;
		virtual IInputLayout* CreateInputLayout(FInputLayoutDesc* desc) = 0;
		virtual ICommandList* CreateCommandList() = 0;
		virtual IShader* CreateShader(FShaderDesc* desc) = 0;
		virtual IShaderEffect* CreateShaderEffect() = 0;
		virtual IComputeEffect* CreateComputeEffect() = 0;
		virtual IFence* CreateFence(const FFenceDesc* desc, const char* name) = 0;
		virtual IEvent* CreateGpuEvent(const FEventDesc * desc, const char* name) = 0;
		virtual ICmdQueue* GetCmdQueue() = 0;

		virtual IGraphicDraw* CreateGraphicDraw();
		virtual IComputeDraw* CreateComputeDraw();
		virtual ICopyDraw* CreateCopyDraw();

		inline const FGpuResourceAlignment* GetGpuResourceAlignment() const{
			return &mGpuResourceAlignment;
		}
		inline const FGpuDeviceCaps* GetGpuDeviceCaps() const {
			return &mCaps;
		}
		inline bool IsSupportSwapchainFormat(EPixelFormat format) const {
			return mCaps.IsSupportSwapchainFormat(format);
		}
		FGpuPipelineManager* GetGpuPipelineManager();
	public:
		FGpuDeviceDesc		Desc;
		FGpuDeviceCaps		mCaps;
		FGpuResourceAlignment	mGpuResourceAlignment{};
		
		AutoRef<FGpuPipelineManager>	mPipelineManager;
		AutoRef<IFence>					mFrameFence;
	public:
		typedef bool (FGpuPostEvent)(IGpuDevice* device, UINT64 completed);
		std::vector<std::function<FGpuPostEvent>>	mPostEvents;
		std::vector<std::function<FGpuPostEvent>>	mTickingPostEvents;
		VSLLock mPostEventLocker;
		void PushPostEvent(const std::function<FGpuPostEvent>& evt)
		{
			VAutoVSLLock lk(mPostEventLocker);
			mPostEvents.push_back(evt);
		}
		template<class _DestroyType>
		void DelayDestroy(_DestroyType obj, UINT delayFrame = 4)
		{
			auto targetValue = mFrameFence->GetAspectValue() + delayFrame;
			this->PushPostEvent([targetValue, obj](IGpuDevice* pDevice, UINT64 completed)->bool
				{
					if (completed >= targetValue)
					{
						AuxGpuResourceDestroyer<_DestroyType>::Destroy(obj, pDevice);
						return true;
					}
					return false;
				});
		}
		virtual void TickPostEvents();
	};
	enum TR_ENUM()
		EQueueCmdlist
	{
		QCL_Read = 0,
			QCL_Transient,
			QCL_FramePost,
	};
	class TR_CLASS()
		ICmdQueue : public VIUnknown
	{
	public:
		ENGINE_RTTI(ICmdQueue);

		void ExecuteCommandList(ICommandList * pCmdlist)
		{
			ExecuteCommandList(1, &pCmdlist);
		}
		virtual void ExecuteCommandList(UINT num, ICommandList * *ppCmdlist) = 0;
		virtual UINT64 SignalFence(IFence * fence, UINT64 value) = 0;
		virtual ICommandList* GetIdleCmdlist(EQueueCmdlist type) = 0;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd, EQueueCmdlist type) = 0;
		virtual void Flush() = 0;
		
		inline UINT64 IncreaseSignal(IFence* fence)
		{
			UINT64 target = fence->GetAspectValue() + 1;
			return SignalFence(fence, target);
		}
	public:
		AutoRef<IFence>					mQueueFence;
	};
}

NS_END