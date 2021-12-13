#pragma once
#include "../ICommandList.h"
#include "VKPreHead.h"
#include "Utility/IGeometryMesh.h"

NS_BEGIN

class IVKRenderContext;
class IVKComputeShader;
class IVKVertexBuffer;
class IVKCommandList : public ICommandList
{
public:
	IVKCommandList();
	~IVKCommandList();

	virtual bool BeginCommand() override;
	virtual void EndCommand() override;

	virtual bool BeginRenderPass(IFrameBuffers* pFrameBuffer, const IRenderPassClears* passClears, const char* debugName) override;
	virtual void EndRenderPass() override;

	virtual void Commit(IRenderContext* pRHICtx) override;

	virtual void SetRasterizerState(IRasterizerState* State) override;
	virtual void SetDepthStencilState(IDepthStencilState* State) override;
	TR_FUNCTION(SV_NoStarToRef = blendFactor)
	virtual void SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask) override;

	virtual void SetComputeShader(IComputeShader* ComputerShader) override;
	virtual void CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture) override;
	virtual void CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT* pUAVInitialCounts) override;
	virtual void CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer) override;
	virtual void CSDispatch(UINT x, UINT y, UINT z) override;
	virtual void CSDispatchIndirect(IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs) override;
	virtual void SetScissorRect(IScissorRect* sr) override;
	virtual void SetVertexBuffer(UINT32 StreamIndex, IVertexBuffer* VertexBuffer, UINT32 Offset, UINT Stride) override;
	virtual void SetIndexBuffer(IIndexBuffer* IndexBuffer) override;
	virtual void DrawPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 NumPrimitives, UINT32 NumInstances) override;
	virtual void DrawIndexedPrimitive(EPrimitiveType PrimitiveType, UINT32 BaseVertexIndex, UINT32 StartIndex, UINT32 NumPrimitives, UINT32 NumInstances) override;
	virtual void DrawIndexedInstancedIndirect(EPrimitiveType PrimitiveType, IGpuBuffer* pBufferForArgs, UINT32 AlignedByteOffsetForArgs) override;
	virtual void IASetInputLayout(IInputLayout* pInputLayout) override;
	virtual void VSSetShader(IVertexShader* pVertexShader, void** ppClassInstances, UINT NumClassInstances) override;
	virtual void PSSetShader(IPixelShader* pPixelShader, void** ppClassInstances, UINT NumClassInstances) override;
	virtual void SetViewport(IViewPort* vp) override;
	virtual void VSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer) override;
	virtual void PSSetConstantBuffer(UINT32 Index, IConstantBuffer* CBuffer) override;
	virtual void VSSetShaderResource(UINT32 Index, IShaderResourceView* pSRV) override;
	virtual void PSSetShaderResource(UINT32 Index, IShaderResourceView* pSRV) override;
	virtual void VSSetSampler(UINT32 Index, ISamplerState* Sampler) override;
	virtual void PSSetSampler(UINT32 Index, ISamplerState* Sampler) override;
	virtual void SetRenderPipeline(IRenderPipeline* pipeline, EPrimitiveType dpType) override;
	virtual vBOOL CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers) override;

	bool Init(IVKRenderContext* rc, const ICommandListDesc* desc);
public:
	VkCommandBuffer							mCommandBuffer;
	VkFence									mFence;
	AutoRef<IFrameBuffers>					mFrameBuffer;
	AutoRef<IScissorRect>					mScissors;
	
	AutoRef<IVKVertexBuffer>				mCurVB;

	bool									mIsRecording;
	bool									mPassRendering;
	bool									mCanCommit;

	VkViewport								mVkViewport;

	typedef void (FVkCmdExecute)(IVKCommandList* rc);
	std::vector<std::function<FVkCmdExecute>>	Commited;
	VSLLock									ComitLocker;
	void PostCommited(const std::function<FVkCmdExecute>& exec) {
		VAutoVSLLock locker(ComitLocker);
		Commited.push_back(exec);
	}
	void ExecCommited();
};

NS_END