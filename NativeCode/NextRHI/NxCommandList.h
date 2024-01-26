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
	class ICommandList;

	struct TR_CLASS(SV_LayoutStruct = 8)
		FIndirectDrawArgument
	{
		UINT VertexCountPerInstance;
		UINT InstanceCount;
		UINT StartIndex;
		UINT StartVertex;
		UINT StartInstance;
	};
	struct TR_CLASS(SV_LayoutStruct = 8)
		FIndirectDispatchArgument
	{
		UINT X;
		UINT Y;
		UINT Z;
	};

	struct TR_CLASS(SV_LayoutStruct = 8)
		FBufferWriter
	{
		IBuffer* Buffer;
		UINT Offset;
		UINT Value;
	};

	enum TR_ENUM()
		EPipelineStage 
	{
		PPLS_NONE,
			PPLS_TOP_OF_PIPE = 0x00000001,
			PPLS_DRAW_INDIRECT = 0x00000002,
			PPLS_VERTEX_INPUT = 0x00000004,
			PPLS_VERTEX_SHADER = 0x00000008,
			PPLS_TESSELLATION_CONTROL_SHADER = 0x00000010,
			PPLS_TESSELLATION_EVALUATION_SHADER = 0x00000020,
			PPLS_GEOMETRY_SHADER = 0x00000040,
			PPLS_FRAGMENT_SHADER = 0x00000080,
			PPLS_EARLY_FRAGMENT_TESTS = 0x00000100,
			PPLS_LATE_FRAGMENT_TESTS = 0x00000200,
			PPLS_COLOR_ATTACHMENT_OUTPUT = 0x00000400,
			PPLS_COMPUTE_SHADER = 0x00000800,
			PPLS_TRANSFER = 0x00001000,
			PPLS_BOTTOM_OF_PIPE = 0x00002000,
			PPLS_HOST = 0x00004000,
			PPLS_ALL_GRAPHICS = 0x00008000,
			PPLS_ALL_COMMANDS = 0x00010000,
			PPLS_TRANSFORM_FEEDBACK = 0x01000000,
			PPLS_CONDITIONAL_RENDERING = 0x00040000,
			PPLS_ACCELERATION_STRUCTURE_BUILD = 0x02000000,
			PPLS_RAY_TRACING_SHADER = 0x00200000,
			PPLS_TASK_SHADER = 0x00080000,
			PPLS_MESH_SHADER = 0x00100000,
			PPLS_FRAGMENT_DENSITY_PROCESS = 0x00800000,
			PPLS_FRAGMENT_SHADING_RATE_ATTACHMENT = 0x00400000,
			PPLS_FLAG_BITS_MAX_ENUM = 0x7FFFFFFF
	};

	class TR_CLASS()
		ICmdRecorder : public VIUnknown
	{
	public:
		~ICmdRecorder();
		std::vector<AutoRef<IGpuDraw>>			mDrawcallArray;
		std::vector<AutoRef<IGpuResource>>		mRefBuffers;
		UINT									mDirectDrawNum = 0;
		UINT									mPrimitiveNum = 0;

		VSLLock									mLocker;
	public:
		inline size_t GetDrawcallNumber() const {
			return mDrawcallArray.size() + mDirectDrawNum;
		}
		inline void UseResource(IGpuResource* res) {
			res->AddCmdRefCount();
			mRefBuffers.push_back(res);
		}
		void PushGpuDraw(IGpuDraw * draw);
		virtual void ResetGpuDraws();
		void FlushDraws(ICommandList* cmdlist);
	};

	class TR_CLASS()
		ICommandList : public IWeakReference
	{
	public:
		ENGINE_RTTI(ICommandList);
		virtual ICmdRecorder* BeginCommand() = 0;
		virtual void EndCommand() = 0;
		virtual bool IsRecording() const = 0;
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

		void InheritPass(ICommandList* cmdlist);

		virtual void Draw(EPrimitiveType topology, UINT BaseVertex, UINT DrawCount, UINT Instance = 1) = 0;
		virtual void DrawIndexed(EPrimitiveType topology, UINT BaseVertex, UINT StartIndex, UINT DrawCount, UINT Instance = 1) = 0;
		virtual void IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset = 0, IBuffer* countBuffer = nullptr) = 0;
		virtual void Dispatch(UINT x, UINT y, UINT z) = 0;
		virtual void IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset = 0) = 0;
		virtual void SetMemoryBarrier(EPipelineStage srcStage, EPipelineStage dstStage, EBarrierAccess srcAccess, EBarrierAccess dstAccess) = 0;
		virtual void SetBufferBarrier(IBuffer* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess) = 0;
		virtual void SetTextureBarrier(ITexture* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess) = 0;

		/*virtual UINT64 SignalFence(IFence * fence, UINT64 value, IEvent* evt = nullptr) = 0;
		virtual void WaitGpuFence(IFence * fence, UINT64 value) = 0;*/

		virtual void CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size) = 0;
		virtual void CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* src, UINT srcSubRes, const FSubresourceBox* box) = 0;
		virtual void CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint) = 0;
		virtual void CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* src, UINT subRes) = 0;

		virtual void WriteBufferUINT32(UINT Count, FBufferWriter* BufferWriters);

		virtual void BeginEvent(const char* info) = 0;
		inline void BeginEvent(VNameString info)
		{
			BeginEvent(info.c_str());
		}
		virtual void EndEvent() = 0;

		ICmdRecorder* GetCmdRecorder() {
			return mCmdRecorder;
		}
		bool PushGpuDrawImpl(IGpuDraw * draw, bool bCheck);
		bool PushGpuDraw(IGpuDraw* draw)
		{
			return PushGpuDrawImpl(draw, true);
		}
		void PushGpuDrawUntilSuccessed(IGpuDraw* draw)
		{
			while (PushGpuDrawImpl(draw, false) == false)
			{

			}
		}
		void DirectGpuDraw(IGpuDraw* draw);
		/*void ResetGpuDraws()
		{
			ASSERT(mCmdRecorder != nullptr);
			mCmdRecorder->ResetGpuDraws();
		}*/
		void FlushDraws()
		{
			if (mCmdRecorder != nullptr)
				mCmdRecorder->FlushDraws(this);
		}
		void AppendDraws(ICmdRecorder* pCmdRecorder);

		virtual void SetDebugName(const char* name) {
			mDebugName = name;
		}
		const char* GetDebugName() const{
			return mDebugName.c_str();
		}
		IGpuPipeline* GetDefaultPipeline() {
			return mPipelineDesc;
		}
		void SetDefaultPipeline(IGpuPipeline* pipeline) {
			mPipelineDesc = pipeline;
		}
		UINT GetDrawcallNumber() const{
			if (mCmdRecorder == nullptr)
				return 0;
			return (UINT)mCmdRecorder->mDrawcallArray.size();
		}
		UINT GetPrimitiveNumber() const {
			return mPrimitiveNum;
		}
		IGpuDevice* GetGpuDevice() {
			return mDevice.GetNakedPtr();
		}
		IFence* GetCommitFence() {
			return mCommitFence;
		}
	public:
		TWeakRefHandle<IGpuDevice>			mDevice;
		AutoRef<ICmdRecorder>				mCmdRecorder;
		std::string							mDebugName;
		UINT								mPrimitiveNum = 0;
		
		AutoRef<IGpuPipeline>				mPipelineDesc;
		AutoRef<IFrameBuffers>				mCurrentFrameBuffers;

		AutoRef<IFence>						mCommitFence;
	};

	class TR_CLASS()
		IGpuScope : public VIUnknown
	{
	public:
		virtual bool IsFinished() = 0;
		virtual UINT64 GetDeltaTime() = 0;
		virtual void Begin(ICommandList * cmdlist) = 0;
		virtual void End(ICommandList * cmdlist) = 0;
		virtual const char* GetName() = 0;
		virtual void SetName(const char* name) = 0;
	public:
	};

	class FTransientCmd
	{
		IGpuDevice* mDevice = nullptr;
		ICommandList* mCmdList = nullptr;
		EQueueType mType = EQueueType::QU_Default;
	public:
		FTransientCmd(IGpuDevice* device, EQueueType type, const char* debugName)
		{
			mDevice = device;
			mCmdList = mDevice->GetCmdQueue()->GetIdleCmdlist();
			mCmdList->SetDebugName(debugName);
			mCmdList->BeginCommand();
			mCmdList->BeginEvent(debugName);
		}
		~FTransientCmd()
		{
			mCmdList->FlushDraws();
			mCmdList->EndEvent();
			mCmdList->EndCommand();
			mDevice->GetCmdQueue()->ExecuteCommandListSingle(mCmdList, mType);
			mDevice->GetCmdQueue()->ReleaseIdleCmdlist(mCmdList);
			mCmdList = nullptr;
		}
		inline ICommandList* GetCmdList() {
			return mCmdList;
		}
	};
}

NS_END