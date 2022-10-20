#include "NullCommandList.h"
#include "NullGpuDevice.h"
#include "NullShader.h"
#include "NullBuffer.h"
#include "NullGpuState.h"
#include "NullEvent.h"
#include "NullInputAssembly.h"
#include "NullFrameBuffers.h"
#include "../NxDrawcall.h"

#define new VNEW

NS_BEGIN

namespace NxRHI
{
	NullCommandList::NullCommandList()
	{
		
	}
	NullCommandList::~NullCommandList()
	{
		
	}
	bool NullCommandList::Init(NullGpuDevice* device)
	{
		return true;
	}
	bool NullCommandList::BeginCommand()
	{
		return true;
	}
	void NullCommandList::EndCommand()
	{
		
	}
	bool NullCommandList::BeginPass(IFrameBuffers* fb, const FRenderPassClears* passClears, const char* name)
	{
		mDebugName = name;
		
		return true;
	}
	void NullCommandList::SetViewport(UINT Num, const FViewPort* pViewports)
	{
		
	}
	void NullCommandList::SetScissor(UINT Num, const FScissorRect* pScissor)
	{
		
	}
	void NullCommandList::EndPass()
	{
	}
	void NullCommandList::BeginEvent(const char* info)
	{
		
	}
	void NullCommandList::EndEvent()
	{
		
	}
	void NullCommandList::SetShader(IShader* shader)
	{
		
	}
	void NullCommandList::SetCBV(EShaderType type, const FShaderBinder* binder, ICbView* buffer)
	{	
	}
	void NullCommandList::SetSrv(EShaderType type, const FShaderBinder* binder, ISrView* view)
	{
		view->GetResourceState()->SetAccessTime(VIUnknown::EngineTime);
	}
	void NullCommandList::SetUav(EShaderType type, const FShaderBinder* binder, IUaView* view)
	{
		
	}
	void NullCommandList::SetSampler(EShaderType type, const FShaderBinder* binder, ISampler* sampler)
	{
		
	}
	void NullCommandList::SetVertexBuffer(UINT slot, IVbView* buffer, UINT32 Offset, UINT Stride)
	{
		
	}
	void NullCommandList::SetIndexBuffer(IIbView* buffer, bool IsBit32)
	{
		
	}
	void NullCommandList::SetGraphicsPipeline(const IGpuDrawState* drawState)
	{
		
	}
	void NullCommandList::SetComputePipeline(const IComputeEffect* drawState)
	{

	}
	void NullCommandList::SetInputLayout(IInputLayout* layout)
	{
		
	}
	void NullCommandList::Draw(EPrimitiveType topology, UINT BaseVertex, UINT DrawCount, UINT Instance)
	{
		
	}
	void NullCommandList::DrawIndexed(EPrimitiveType topology, UINT BaseVertex, UINT StartIndex, UINT DrawCount, UINT Instance)
	{
		
	}
	void NullCommandList::IndirectDrawIndexed(EPrimitiveType topology, IBuffer* indirectArg, UINT indirectArgOffset)
	{
		
	}
	void NullCommandList::Dispatch(UINT x, UINT y, UINT z)
	{
		
	}
	void NullCommandList::IndirectDispatch(IBuffer* indirectArg, UINT indirectArgOffset)
	{
		
	}
	void NullCommandList::SetMemoryBarrier(EPipelineStage srcStage, EPipelineStage dstStage, EBarrierAccess srcAccess, EBarrierAccess dstAccess)
	{

	}
	void NullCommandList::SetBufferBarrier(IBuffer* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess)
	{

	}
	void NullCommandList::SetTextureBarrier(ITexture* pResource, EPipelineStage srcStage, EPipelineStage dstStage, EGpuResourceState srcAccess, EGpuResourceState dstAccess)
	{

	}
	/*UINT64 NullCommandList::SignalFence(IFence* fence, UINT64 value, IEvent* evt)
	{
		return value;
	}
	void NullCommandList::WaitGpuFence(IFence* fence, UINT64 value)
	{
		
	}*/
	void NullCommandList::CopyBufferRegion(IBuffer* target, UINT64 DstOffset, IBuffer* src, UINT64 SrcOffset, UINT64 Size)
	{

	}
	void NullCommandList::CopyTextureRegion(ITexture* target, UINT tarSubRes, UINT DstX, UINT DstY, UINT DstZ, ITexture* src, UINT srcSubRes, const FSubresourceBox* box)
	{

	}
	void NullCommandList::CopyBufferToTexture(ITexture* target, UINT subRes, IBuffer* src, const FSubResourceFootPrint* footprint)
	{

	}
	void NullCommandList::CopyTextureToBuffer(IBuffer* target, const FSubResourceFootPrint* footprint, ITexture* src, UINT subRes)
	{

	}
	void NullCommandList::Flush()
	{
		
	}
}

NS_END