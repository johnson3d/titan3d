#pragma once
#include "../NxCommandList.h"
#include "DX12PreHead.h"

NS_BEGIN

namespace NxRHI
{
	class DX12GpuDevice;
	class DX12CmdQueue;
	class DX12RenderTargetView;
	class DX12CommandList : public ICommandList
	{
	public:
		DX12CommandList();
		~DX12CommandList();
		bool Init(DX12GpuDevice* device);
		virtual bool BeginCommand() override;
		virtual void EndCommand() override;
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
		void Commit(DX12CmdQueue* cmdQueue);
		inline DX12GpuDevice* GetDX12Device()
		{
			return (DX12GpuDevice*)mDevice.GetPtr();
		}
		AutoRef<ID3D12CommandAllocator>		mAllocator;
		AutoRef<ID3D12GraphicsCommandList>	mContext;
		
		//AutoRef<ID3D12CommandSignature>	mCurrentIndirectDrawIndexSig;
		//AutoRef<ID3D12CommandSignature>	mCurrentIndirectDispatchSig;

		bool						mIsRecording = false;

		std::vector<AutoRef<DX12RenderTargetView>>	mCurRtvs;
	};
}

NS_END