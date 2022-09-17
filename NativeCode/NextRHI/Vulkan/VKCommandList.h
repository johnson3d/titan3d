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

	enum EPagedCmdBufferState
	{
		PCBS_Free,
		PCBS_Recording,
		PCBS_Commiting,
		PCBS_WaitFree,
	};

	struct VKCommandBufferPagedObject : public MemAlloc::FPagedObject<VkCommandBuffer>
	{
		EPagedCmdBufferState		mState = EPagedCmdBufferState::PCBS_Free;
		UINT64						mTargetValue = 0;
	};
	struct VKCommandBufferPage : public MemAlloc::FPage<VkCommandBuffer>
	{
		~VKCommandBufferPage();
		VkCommandPool					mCommandPool = nullptr;
	};
	struct VKCommandBufferCreator
	{
		TObjectHandle<VKGpuDevice>		mDeviceRef;
		
		MemAlloc::FPage<VkCommandBuffer>* CreatePage(UINT pageSize);
		MemAlloc::FPagedObject<VkCommandBuffer>* CreatePagedObject(MemAlloc::FPage<VkCommandBuffer>* page, UINT index);
		void OnFree(MemAlloc::FPagedObject<VkCommandBuffer>* obj);
		void FinalCleanup(MemAlloc::FPage<VkCommandBuffer>* page);
	};

	struct VKCommandbufferAllocator : public MemAlloc::FPagedObjectAllocator<VkCommandBuffer, VKCommandBufferCreator>
	{
		struct FCmdBufferHolder
		{
			AutoRef<VKCommandBufferPagedObject>	CmdBuffer;
			AutoRef<IFence>						Fence;
		};
		VSLLock									mLocker;
		std::vector<FCmdBufferHolder>			mWaitFrees;
		~VKCommandbufferAllocator();
		void TickForRecycle(VKGpuDevice* device);
		void PushRecycle(const AutoRef<IFence>& fence, AutoRef<VKCommandBufferPagedObject>& buffer);
	};

	class VKCmdBufferManager : public VThreadDispatcher<VKCommandbufferAllocator>
	{
		VKGpuDevice*				mDevice = nullptr;
	public:
		void Initialize(VKGpuDevice* device);
		virtual void InitContext(VKCommandbufferAllocator* context) override;
	};

	class VKCommandList : public ICommandList
	{
	public:
		VKCommandList();
		~VKCommandList();
		bool Init(VKGpuDevice* device);
		virtual bool BeginCommand() override;
		virtual void EndCommand() override;
		bool BeginCommand(VkCommandBufferUsageFlagBits flags);
		void EndCommand(bool bRecycle);
		virtual void SetShader(IShader* shader) override;
		virtual void SetCBV(EShaderType type, const FShaderBinder* binder, ICbView* buffer) override;
		virtual void SetSrv(EShaderType type, const FShaderBinder* binder, ISrView* view) override;
		virtual void SetUav(EShaderType type, const FShaderBinder* binder, IUaView* view) override;
		virtual void SetSampler(EShaderType type, const FShaderBinder* binder, ISampler* sampler) override;
		virtual void SetVertexBuffer(UINT slot, IVbView* buffer, UINT32 Offset, UINT Stride) override;
		virtual void SetIndexBuffer(IIbView* buffer, bool IsBit32) override;
		virtual void SetGraphicsPipeline(const IGpuDrawState* drawState) override;
		virtual void SetComputePipeline(const IComputeEffect* drawState) override;
		virtual void SetInputLayout(IInputLayout* layout) override;

		virtual bool BeginPass(IFrameBuffers* fb, const FRenderPassClears* passClears, const char* name) override;
		virtual void SetViewport(UINT Num, const FViewPort* pViewports) override;
		virtual void SetScissor(UINT Num, const FScissorRect* pScissor) override;
		virtual void EndPass() override;

		virtual void Draw(EPrimitiveType topology, UINT BaseVertex, UINT DrawCount, UINT Instance = 1) override;
		virtual void DrawIndexed(EPrimitiveType topology, UINT BaseVertex, UINT StartIndex, UINT DrawCount, UINT Instance = 1) override;
		virtual void IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset = 0) override;
		virtual void Dispatch(UINT x, UINT y, UINT z) override;
		virtual void IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset = 0) override;

		/*virtual UINT64 SignalFence(IFence* fence, UINT64 value, IEvent* evt = nullptr) override;
		virtual void WaitGpuFence(IFence* fence, UINT64 value) override;*/

		virtual void CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size) override;
		virtual void CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* src, UINT srcSubRes, const FSubresourceBox* box) override;
		virtual void CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint) override;
		virtual void CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* src, UINT subRes) override;

		virtual void Flush() override;
		virtual void BeginEvent(const char* info) override;
		virtual void EndEvent() override;
	public:
		void UseCurrentViewports();
		void UseCurrentScissors();
		inline VKGpuDevice* GetVKDevice()
		{
			return (VKGpuDevice*)mDevice.GetPtr();
		}
		AutoRef<VKCommandBufferPagedObject>			mCommandBuffer;
		
		bool						mIsRecording = false;

		std::vector<std::pair<EGpuResourceState, AutoRef<VKRenderTargetView>>>	mCurRtvs;
		std::pair<EGpuResourceState, AutoRef<VKDepthStencilView>>	mCurDsv;

		std::vector<VkViewport>						mCurrentViewports;
		std::vector<VkRect2D>						mCurrentScissorRects;
	};
}

NS_END