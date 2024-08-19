#pragma once
#include "../Base/IUnknown.h"
#include "../Base/thread/vfxcritical.h"
#include "../Base/thread/vfxthread.h"
#include "NxEvent.h"
#include "NxRHIDefine.h"

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
	class IGraphicsEffect;
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
	class IActionDraw;
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
	class IGpuScope;
	class FVertexArray;
	class FGeomMesh;

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
		vBOOL		GpuBaseValidation = FALSE;
		void*		WindowHandle = nullptr;
		VNameString	RenderDocPath;
	};
	class TR_CLASS()
		IGpuSystem : public IWeakReference
	{
	public:
		static IGpuSystem* CreateGpuSystem(ERhiType type, const FGpuSystemDesc* desc);
		virtual bool InitGpuSystem(ERhiType type, const FGpuSystemDesc * desc) { return true; }
		virtual IGpuDevice* CreateDevice(const FGpuDeviceDesc * desc) = 0;
		virtual int GetNumOfGpuDevice() const{
			return 1;
		}
		virtual void GetDeviceDesc(int index, FGpuDeviceDesc* desc) const {

		}
	public:
		ERhiType		Type = ERhiType::RHI_D3D11;
		FGpuSystemDesc	Desc{};
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FGpuDeviceDesc
	{
		void SetDefault()
		{
			RhiType = ERhiType::RHI_D3D11;
			memset(Name, 0, sizeof(Name));
			VendorId = 0;
			AdapterId = 0;
			DeviceHandle = nullptr;
			DeviceContextHandle = nullptr;
			CreateDebugLayer = true;
			GpuDump = true;
		}
		bool IsNVIDIA() const{
			return 0x10DE == VendorId;
		}
		bool IsIntel() const {
			return 0x8086 == VendorId;
		}
		bool IsAMD() const {
			return 0x1002 == VendorId;
		}
		ERhiType	RhiType = ERhiType::RHI_D3D11;
		char	Name[256]{};
		int		VendorId;
		int		AdapterId = 0;
		UINT64	DedicatedVideoMemory = 0;
		void*	DeviceHandle = nullptr;
		void*	DeviceContextHandle = nullptr;
		bool	CreateDebugLayer = true;
		bool	GpuDump = true;

		const char* GetName() const {
			return Name;
		}
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FGpuDeviceCaps
	{
		bool	IsSupportFence = false;
		bool	IsSupportSSBO_VS = true;
		bool	IsSupoortBufferToTexture = false;
		bool	Unused = false;
		UINT			MaxViewInstanceCount = 0;
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
		UINT RoundupTexturePitch(UINT x) const
		{
			return TexturePitchAlignment * ((x + (TexturePitchAlignment - 1)) / TexturePitchAlignment);
		}
	};
	template<class _DestroyType>
	struct AuxGpuResourceDestroyer
	{
		static void Destroy(_DestroyType obj, IGpuDevice* device)
		{

		}
	};
	class TR_CLASS()
		IGpuDevice : public IWeakReference
	{
	protected:
		bool mIsTryFinalize = false;
		bool mIsFinalized = false;
	public:
		ENGINE_RTTI(IGpuDevice);
		IGpuDevice();
		virtual bool InitDevice(IGpuSystem * pGpuSystem, const FGpuDeviceDesc * desc) = 0;
		virtual void TryFinalizeDevice(IGpuSystem * pGpuSystem) {
			mIsTryFinalize = true;
			mIsFinalized = true;
		}
		bool IsFinalized() const{
			return mIsFinalized;
		}
		virtual IBuffer* CreateBuffer(const FBufferDesc * desc) = 0;
		virtual ITexture* CreateTexture(const FTextureDesc * desc) = 0;
		virtual ITexture* CreateTexture(void* pSharedObject) { return nullptr; }
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
		virtual IGraphicsEffect* CreateShaderEffect() = 0;
		virtual IComputeEffect* CreateComputeEffect() = 0;
		virtual IFence* CreateFence(const FFenceDesc* desc, const char* name) = 0;
		virtual IEvent* CreateGpuEvent(const FEventDesc * desc, const char* name) = 0;
		virtual ICmdQueue* GetCmdQueue() = 0;

		virtual IGraphicDraw* CreateGraphicDraw();
		virtual IComputeDraw* CreateComputeDraw();
		virtual ICopyDraw* CreateCopyDraw();
		virtual IActionDraw* CreateActionDraw();

		virtual FVertexArray* CreateVertexArray();
		virtual FGeomMesh* CreateGeomMesh();
		virtual IGpuScope* CreateGpuScope() = 0;
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

		virtual void SetBreakOnID(int id, bool open) = 0;

		inline IFence* GetFrameFence() {
			return mFrameFence;
		}
	public:
		FGpuDeviceDesc		Desc;
		FGpuDeviceCaps		mCaps;
		FGpuResourceAlignment	mGpuResourceAlignment{};
		
		AutoRef<FGpuPipelineManager>	mPipelineManager;
		AutoRef<IFence>					mFrameFence;
		
		bool				EnableImmExecute = false;
		void*				mDeviceThreadId = nullptr;
		bool				IsSyncStage = false;
		inline void CheckDeviceThread()
		{
#if DEBUG
			ASSERT(IsSyncStage || mDeviceThreadId == vfxThread::GetCurrentThreadId());
#endif
		}
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
			auto targetValue = mFrameFence->GetExpectValue() + delayFrame;
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
		void WaitFrameFence(int beforeFrame = 3);
	};
	class TR_CLASS()
		ICmdQueue : public IWeakReference
	{
	public:
		ENGINE_RTTI(ICmdQueue);

		void ExecuteCommandListSingle(ICommandList * pCmdlist, EQueueType type)
		{
			ExecuteCommandList(1, &pCmdlist, 0, nullptr, type);
		}
		//NumOfWait = 0: No wait
		//NumOfWait = 0xFFFFFFFF: Prev Queued Cmdlist
		virtual void ExecuteCommandList(UINT NumOfExe, ICommandList** Cmdlist, UINT NumOfWait, ICommandList** ppWaitCmdlists, EQueueType type) = 0;
		virtual ICommandList* GetIdleCmdlist() = 0;
		virtual void ReleaseIdleCmdlist(ICommandList* cmd) = 0;
		virtual UINT64 Flush(EQueueType type) = 0;
		
		inline UINT64 IncreaseSignal(IFence* fence, EQueueType type)
		{
			return fence->IncreaseExpect(this, 1, type);
		}
	public:
		VCritical						mGraphicsQueueLocker;
		
		UINT64							mDefaultQueueFrequence = 0;
		UINT64							mComputeQueueFrequence = 0;
		UINT64							mTransferQueueFrequence = 0;
	};
}

NS_END