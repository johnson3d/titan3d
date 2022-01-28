#pragma once
#include "../ICommandList.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;
class ID11Fence;

class ID11CommandList : public ICommandList
{
public:
	ID11CommandList();
	~ID11CommandList();

	virtual bool BeginCommand() override;
	
	virtual void EndCommand() override;

	/*virtual void SetRenderTargets(IFrameBuffers* FrameBuffers) override;

	virtual void ClearMRT(const std::pair<BYTE, DWORD>* ClearColors, int ColorNum,
		vBOOL bClearDepth, float Depth, vBOOL bClearStencil, UINT32 Stencil) override;*/
	
	virtual bool BeginRenderPass(IFrameBuffers* pFrameBuffer, const IRenderPassClears* passClears, const char* debugName) override;
	//virtual void BuildRenderPass(vBOOL bImmCBuffer, UINT* limitter, IPass** ppPass) override;
	virtual void EndRenderPass() override;
	
	virtual void Commit(IRenderContext* pRHICtx) override;

	virtual void SetRasterizerState(IRasterizerState* State) override;
	virtual void SetDepthStencilState(IDepthStencilState* State) override;
	virtual void SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask) override;

	virtual void SetComputeShader(IComputeShader* ComputerShader) override;
	virtual void CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture) override;
	virtual void CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT *pUAVInitialCounts) override;
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

	void BeginEvent(const wchar_t* info);
	virtual void BeginEvent(const char* info) override;	
	virtual void EndEvent() override;
	virtual void Signal(IFence* fence, int value) override;
public:
	ID3D11DeviceContext*		mDeferredContext;
	ID3D11CommandList*			mCmdList;
	std::vector<ID3D11RenderTargetView*>	 mDX11RTVArray;
	ID3D11DepthStencilView*		mDSView;
	std::vector<AutoRef<ID11Fence>>	mSignals;
public:
	bool Init(ID11RenderContext* rc, const ICommandListDesc* desc);
	bool InitD11Point(ID11RenderContext* rc, ID3D11DeviceContext* context);


};

NS_END