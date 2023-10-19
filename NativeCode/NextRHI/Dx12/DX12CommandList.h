#pragma once
#include "../NxCommandList.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12CmdQueue;
	class DX12RenderTargetView;
	class DX12CmdRecorder : public ICmdRecorder
	{
	public:
		AutoRef<ID3D12CommandAllocator>		mAllocator;
		virtual void ResetGpuDraws() override;
	};
	class DX12CommandList : public ICommandList
	{
	public:
		DX12CommandList();
		~DX12CommandList();
		bool Init(DX12GpuDevice* device);
		virtual ICmdRecorder* BeginCommand() override;
		virtual void EndCommand() override;
		virtual bool IsRecording() const override {
			return mIsRecording;
		}
		virtual void SetDebugName(const char* name) override;
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
		virtual void IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset = 0, IBuffer* countBuffer = nullptr) override;
		virtual void Dispatch(UINT x, UINT y, UINT z) override;
		virtual void IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset = 0) override;
		virtual void SetMemoryBarrier(EPipelineStage srcStage, EPipelineStage dstStage, EBarrierAccess srcAccess, EBarrierAccess dstAccess) override;
		virtual void SetBufferBarrier(IBuffer* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess) override;
		virtual void SetTextureBarrier(ITexture* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess) override;

		/*virtual UINT64 SignalFence(IFence* fence, UINT64 value, IEvent* evt = nullptr) override;
		virtual void WaitGpuFence(IFence* fence, UINT64 value) override;*/

		virtual void CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size) override;
		virtual void CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* src, UINT srcSubRes, const FSubresourceBox* box) override;
		virtual void CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint) override;
		virtual void CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* src, UINT subRes) override;

		virtual void WriteBufferUINT32(UINT Count, FBufferWriter* BufferWriters) override;

		virtual void BeginEvent(const char* info) override;
		void BeginEvent(std::wstring& info);
		virtual void EndEvent() override;
	public:
		void Commit(DX12CmdQueue* cmdQueue, EQueueType type);
		inline DX12GpuDevice* GetDX12Device()
		{
			return (DX12GpuDevice*)mDevice.GetNakedPtr();
		}
		AutoRef<ID3D12GraphicsCommandList>	mContext;
		AutoRef<ID3D12GraphicsCommandList2> mContext2;
		AutoRef<ID3D12DebugCommandList1>	mDebugCommandList1;
		
		//AutoRef<ID3D12CommandSignature>	mCurrentIndirectDrawIndexSig;
		//AutoRef<ID3D12CommandSignature>	mCurrentIndirectDispatchSig;
		UINT								mCurrentIndirectOffset = 0;
		AutoRef<ID3D12CommandSignature>		mCurrentCmdSig;

		std::wstring				mDebugNameW;
		bool						mIsRecording = false;
	private:
		DX12CmdRecorder* GetDX12CmdRecorder()
		{
			if (mCmdRecorder == nullptr)
			{
				return nullptr;
			}
			return mCmdRecorder.UnsafeConvertTo<DX12CmdRecorder>();
		}
	};

	class DX12GpuScope : public IGpuScope
	{
	public:
		~DX12GpuScope();
		bool Init(DX12GpuDevice* device);

		virtual bool IsFinished() override;
		virtual UINT64 GetDeltaTime() override;
		virtual void Begin(ICommandList* cmdlist) override;
		virtual void End(ICommandList* cmdlist) override;

		virtual const char* GetName() override {
			return mName.c_str();
		}
		virtual void SetName(const char* name) override;
	public:
		TWeakRefHandle<DX12GpuDevice>	mDeviceRef;
		std::string					mName;
		AutoRef<ID3D12QueryHeap>	mQueryHeap;
		AutoRef<FGpuMemHolder>	mResultBuffer;
		//AutoRef<IFence>				mFence;
	};
}

NS_END