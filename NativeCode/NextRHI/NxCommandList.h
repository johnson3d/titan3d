#pragma once
#include "NxGpuDevice.h"
#include "NxShader.h"
#include "NxGeomMesh.h"
#include "NxGpuState.h"

NS_BEGIN

namespace NxRHI
{
	struct FShaderBinder;
	class IGpuDevice;
	class ISrView;
	class IUaView;
	class ISampler;
	class IFence;
	class IEvent;
	class IGpuPipeline;
	class IInputLayout;
	struct FRenderPassClears;
	class IFrameBuffers;
	class IGpuDraw;
	struct FViewPort;
	struct FScissorRect;
	struct FSubResourceFootPrint;

	class TR_CLASS()
		ICommandList : public VIUnknown
	{
	public:
		ENGINE_RTTI(ICommandList);
		virtual bool BeginCommand() = 0;
		virtual void EndCommand() = 0;
		virtual void SetShader(IShader* shader) = 0;
		virtual void SetCBV(EShaderType type, const FShaderBinder* binder, ICbView * buffer) = 0;
		virtual void SetSrv(EShaderType type, const FShaderBinder* binder, ISrView* view) = 0;
		virtual void SetUav(EShaderType type, const FShaderBinder* binder, IUaView* view) = 0;
		virtual void SetSampler(EShaderType type, const FShaderBinder * binder, ISampler* sampler) = 0;
		virtual void SetVertexBuffer(UINT slot, IVbView * buffer, UINT32 Offset, UINT Stride) = 0;
		virtual void SetIndexBuffer(IIbView * buffer, bool IsBit32 = false) = 0;
		virtual void SetGraphicsPipeline(const IGpuDrawState* drawState) = 0;
		virtual void SetComputePipeline(const IComputeEffect * drawState) = 0;
		virtual void SetInputLayout(IInputLayout* layout) = 0;
		
		virtual bool BeginPass(IFrameBuffers* fb, const FRenderPassClears * passClears, const char* name) = 0;
		virtual void SetViewport(UINT Num, const FViewPort* pViewports) = 0;
		inline void SetViewport(const FViewPort & viewports)
		{
			SetViewport(1, &viewports);
		}
		virtual void SetScissor(UINT Num, const FScissorRect* pScissor) = 0;
		inline void SetScissor(const FScissorRect& scissor)
		{
			SetScissor(1, &scissor);
		}
		virtual void EndPass() = 0;

		virtual void Draw(EPrimitiveType topology, UINT BaseVertex, UINT DrawCount, UINT Instance = 1) = 0;
		virtual void DrawIndexed(EPrimitiveType topology, UINT BaseVertex, UINT StartIndex, UINT DrawCount, UINT Instance = 1) = 0;
		virtual void IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset = 0) = 0;
		virtual void Dispatch(UINT x, UINT y, UINT z) = 0;
		virtual void IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset = 0) = 0;
		
		/*virtual UINT64 SignalFence(IFence * fence, UINT64 value, IEvent* evt = nullptr) = 0;
		virtual void WaitGpuFence(IFence * fence, UINT64 value) = 0;*/

		virtual void CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size) = 0;
		virtual void CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* src, UINT srcSubRes, const FSubresourceBox* box) = 0;
		virtual void CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint) = 0;
		virtual void CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* src, UINT subRes) = 0;

		virtual void Flush() = 0;
		virtual void BeginEvent(const char* info) = 0;
		virtual void EndEvent() = 0;

		void PushGpuDraw(IGpuDraw * draw);
		void FlushDraws();
		void ResetGpuDraws();

		void SetDebugName(const char* name) {
			mDebugName = name;
		}
		IGpuPipeline* GetDefaultPipeline() {
			return mPipelineDesc;
		}
		void SetDefaultPipeline(IGpuPipeline* pipeline) {
			mPipelineDesc = pipeline;
		}
		UINT GetDrawcallNumber() const{
			return (UINT)mDrawcallArray.size();
		}
		IGpuDevice* GetGpuDevice() {
			return mDevice.GetPtr();
		}
	public:
		TObjectHandle<IGpuDevice>			mDevice;
		std::vector<AutoRef<IGpuDraw>>		mDrawcallArray;
		std::string							mDebugName;
		
		AutoRef<IGpuPipeline>				mPipelineDesc;
		AutoRef<IFrameBuffers>				mCurrentFrameBuffers;
	};
}

NS_END