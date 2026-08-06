#pragma once
#include "../NxCommandList.h"
#include "VKPreHead.h"
#include "../../Base/allocator/PagedAllocator.h"
#include "../../Base/thread/vfxThreadDispatcher.h"

NS_BEGIN

namespace NxRHI
{
	class VKGpuDevice;
	class VKCmdQueue;
	class VKRenderTargetView;
	class VKDepthStencilView;
	class VKCmdRecorder;
	class VKCommandList;

	enum EPagedCmdBufferState
	{
		PCBS_Free,
		PCBS_Recording,
		PCBS_Commiting,
		PCBS_WaitFree,
	};
	
	class VKThreadCmdBufferManager
	{	
	public:
		void Initialize(VKGpuDevice* device);
		AutoRef<VKCmdRecorder> Alloc(VKCommandList* cmdlist);
		void Free(const AutoRef<VKCmdRecorder>& allocator, UINT64 waitValue, AutoRef<IFence>& fence);
		void TickRecycle();
		void FinalCleanup();

		void UnsafeDirectFree(const AutoRef<VKCmdRecorder>& allocator);
	public:
		std::string mThreadName;
		VKThreadCmdBufferManager** mThreadStaticAddr = nullptr;
		VKGpuDevice*		mDevice = nullptr;
		//VkCommandPool is not thread safe, so we need to make instance for each thread
		VkCommandPool		mCmdPool = (VkCommandPool)nullptr;
		VSLLock				mLocker;
		std::queue<AutoRef<VKCmdRecorder>>		CmdAllocators;
		struct FWaitRecycle
		{
			UINT64							WaitValue = 0;
			AutoRef<IFence>					Fence;
			AutoRef<VKCmdRecorder>			Allocator;
			int								WaitFrameCount = 0;
		};
		std::vector<FWaitRecycle>	Recycles;
	};

	class VKCmdBufferManager : public VIUnknown
	{
		thread_local static VKThreadCmdBufferManager* mThreadManager;
		std::vector<VKThreadCmdBufferManager*> mAllManagers;
	public:
		void Initialize(VKGpuDevice* device)
		{
			mDevice = device;
		}
		AutoRef<VKCmdRecorder> Alloc(VKCommandList* cmdlist);
		void Free(const AutoRef<VKCmdRecorder>& allocator, UINT64 waitValue, AutoRef<IFence>& fence);
		void TickRecycle();
		bool FinalCleanup();
	public:
		VKGpuDevice* mDevice = nullptr;
	};

	class VKCmdRecorder : public ICmdRecorder
	{
	public:
		VKCmdRecorder(IGpuDevice* device, ECmdRecorderType type)
			: ICmdRecorder(device, type)
		{

		}
		VKThreadCmdBufferManager*			mManager;
		VkCommandBuffer						mCommandBuffer;
		AutoRef<VKCommandList>				mCmdlist;
		bool								mIsRecording = false;
		virtual void ResetGpuDraws() override;
		void FinalCleanup(VKThreadCmdBufferManager* manager);

		void Free(UINT64 waitValue, AutoRef<IFence>& fence)
		{
			mManager->Free(this, waitValue, fence);
		}
	};
	class VKCommandList : public ICommandList
	{
	public:
		VKCommandList();
		~VKCommandList();
		bool Init(VKGpuDevice* device);
		virtual ICmdRecorder* BeginCommand() override;
		virtual void EndCommand() override;
		virtual bool IsRecording() const override {
			return mCmdListState == ECmdListState::Recording;
		}
		void Commit(VKCmdQueue* queue, EQueueType type);
		virtual void SetShader(IShader* shader) override;
		virtual void SetCBV(EShaderType type, const FShaderBinder* binder, ICbView* buffer) override;
		virtual void SetSrv(EShaderType type, const FShaderBinder* binder, ISrView* view) override;
		virtual void SetUav(EShaderType type, const FShaderBinder* binder, IUaView* view) override;
		virtual void SetSampler(EShaderType type, const FShaderBinder* binder, ISampler* sampler) override;
		virtual void SetVertexBuffer(UINT slot, IVbView* buffer, UINT Offset, UINT Stride) override;
		virtual void SetIndexBuffer(IIbView* buffer, bool IsBit32) override;
		virtual void SetGraphicsPipeline(const IGpuDrawState* drawState) override;
		virtual void SetComputePipeline(const IComputeEffect* drawState) override;
		virtual void SetInputLayout(IInputLayout* layout) override;

		virtual void SetViewInstanceMask(UINT Mask) override;

		virtual bool BeginPass(IFrameBuffers* fb, const FRenderPassClears* passClears, const char* name) override;
		virtual void SetViewport(UINT Num, const FViewPort* pViewports) override;
		virtual void SetScissor(UINT Num, const FScissorRect* pScissor) override;
		virtual void EndPass() override;

		virtual void Draw(EPrimitiveType topology, UINT BaseVertex, UINT DrawCount, UINT Instance = 1) override;
		virtual void IndirectDraw(EPrimitiveType topology, IBuffer* indirectArg, UINT AlignedByteOffsetForArgs, IBuffer* countBuffer) override;
		virtual void DrawIndexed(EPrimitiveType topology, UINT BaseVertex, UINT StartIndex, UINT DrawCount, UINT Instance = 1) override;
		virtual void IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset = 0, IBuffer* countBuffer = nullptr) override;
		virtual void Dispatch(UINT x, UINT y, UINT z) override;
		virtual void IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset = 0) override;
		virtual void DispatchMesh(UINT x, UINT y, UINT z) override;
		virtual void IndirectDispatchMesh(IBuffer* indirectArg, UINT indirectArgOffset = 0) override;
		virtual void SetMemoryBarrier(EPipelineStage srcStage, EPipelineStage dstStage, EBarrierAccess srcAccess, EBarrierAccess dstAccess) override;
		virtual void SetBufferBarrier(IBuffer* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess) override;
		virtual void SetTextureBarrier(ITexture* pResource, UINT subResource, UINT levelCount, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess) override;

		/*virtual UINT64 SignalFence(IFence* fence, UINT64 value, IEvent* evt = nullptr) override;
		virtual void WaitGpuFence(IFence* fence, UINT64 value) override;*/

		virtual void CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size) override;
		virtual void CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* src, UINT srcSubRes, const FSubresourceBox* box) override;
		virtual void CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint) override;
		virtual void CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* src, UINT subRes) override;

		virtual void WriteBufferUINT32(UINT Count, FBufferWriter* BufferWriters) override;

		virtual void BeginEvent(const char* info, DWORD color = 0) override;
		virtual void EndEvent() override;
	public:
		void UseCurrentViewports();
		void UseCurrentScissors();
		
		std::vector<std::pair<EGpuResourceState, AutoRef<VKRenderTargetView>>>	mCurRtvs;
		std::pair<EGpuResourceState, AutoRef<VKDepthStencilView>>	mCurDsv;

		std::vector<VkViewport>						mCurrentViewports;
		std::vector<VkRect2D>						mCurrentScissorRects;

		UINT						mViewInstanceMask = 0;
		ECmdListState				mCmdListState = ECmdListState::None;
	public:
		inline VKGpuDevice* GetVKDevice()
		{
			return (VKGpuDevice*)mDevice.GetNakedPtr();
		}
		VKCmdRecorder* GetVKCmdRecorder()
		{
			if (mCmdRecorder == nullptr)
			{
				return nullptr;
			}
			return mCmdRecorder.UnsafeConvertTo<VKCmdRecorder>();
		}
	private:
		bool BeginRendering(IFrameBuffers* fb, const FRenderPassClears* passClears, const char* name);
		void EndRendering();
		class VKCmdBeginRenderingDraw : public IGpuDraw
		{
		public:
			std::vector<VkRenderingAttachmentInfo> mColorAttachments;
			VkRenderingAttachmentInfo mDepthAttachment;
			VkRenderingAttachmentInfo mStencilAttachment;
			VkRenderingInfo mRenderingInfo = {};

			virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;
			virtual UINT GetPrimitiveNum() override
			{
				return 0;
			}
			virtual void ResetResources() override
			{

			}
		};
		class VKCmdBeginRenderPassDraw : public IGpuDraw
		{
		public:
			VkRenderPassBeginInfo mRenderPassInfo;

			virtual void Commit(ICommandList* cmdlist, bool bRefResource) override;
			virtual UINT GetPrimitiveNum() override
			{
				return 0;
			}
			virtual void ResetResources() override
			{

			}
		};
	};
}

NS_END