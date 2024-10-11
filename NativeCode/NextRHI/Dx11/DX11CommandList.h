#pragma once
#include "../NxCommandList.h"
#include "DX11PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX11CmdRecorder : public ICmdRecorder
	{
	public:
		AutoRef<ID3D11CommandList> mCmdList;

		virtual void ResetGpuDraws() override;
	};
	class DX11GpuDevice;
	class DX11CommandList : public ICommandList
	{
	public:
		DX11CommandList();
		~DX11CommandList();
		bool Init(DX11GpuDevice* device);
		bool Init(DX11GpuDevice* device, ID3D11DeviceContext* context);
		virtual ICmdRecorder* BeginCommand() override;
		virtual void EndCommand() override;
		virtual bool IsRecording() const override {
			return mIsRecording;
		}
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
		virtual void CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* source, UINT subRes) override;

		virtual void WriteBufferUINT32(UINT Count, FBufferWriter* BufferWriters) override;

		virtual void BeginEvent(const char* info) override;
		virtual void EndEvent() override;
	public:
		void Commit(ID3D11DeviceContext* imContex);
		inline DX11GpuDevice* GetDX11Device()
		{
			return (DX11GpuDevice*)mDevice.GetNakedPtr();
		}
		ID3D11DeviceContext* mContext;
		ID3D11DeviceContext4* mContext4;
		bool mIsRecording = false;
		bool IsImmContext = false;
	private:
		DX11CmdRecorder* GetDX11CmdRecorder()
		{
			if (mCmdRecorder == nullptr)
			{
				return nullptr;
			}
			return mCmdRecorder.UnsafeConvertTo<DX11CmdRecorder>();
		}
	};

	class DX11GpuScope : public IGpuScope
	{
	public:
		~DX11GpuScope();
		bool Init(DX11GpuDevice* device);

		virtual bool IsFinished() override;
		virtual UINT64 GetDeltaTime() override;
		virtual void Begin(ICommandList* cmdlist) override;
		virtual void End(ICommandList* cmdlist) override;

		virtual const char* GetName() override {
			return mName.c_str();
		}
		virtual void SetName(const char* name) override;
	public:
		TWeakRefHandle<DX11GpuDevice>	mDeviceRef;
		std::string					mName;
		AutoRef<ID3D11Query>		mQueryStart;
		AutoRef<ID3D11Query>		mQueryEnd;
		//AutoRef<IFence>				mFence;
		UINT64						mDeltaTime = 0;
	};
}

NS_END