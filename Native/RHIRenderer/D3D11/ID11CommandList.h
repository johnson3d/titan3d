#pragma once
#include "../ICommandList.h"
#include "D11PreHead.h"

NS_BEGIN

class ID11RenderContext;

class ID11CommandList : public ICommandList
{
public:
	ID11CommandList();
	~ID11CommandList();

	virtual void BeginCommand() override;
	
	virtual void EndCommand() override;

	/*virtual void SetRenderTargets(IFrameBuffers* FrameBuffers) override;

	virtual void ClearMRT(const std::pair<BYTE, DWORD>* ClearColors, int ColorNum,
		vBOOL bClearDepth, float Depth, vBOOL bClearStencil, UINT32 Stencil) override;*/
	
	virtual void BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer) override;
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
	virtual vBOOL CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers) override;
public:
	ID3D11DeviceContext*		mDeferredContext;
	ID3D11CommandList*			mCmdList;
	std::vector<ID3D11RenderTargetView*>	 mDX11RTVArray;
	ID3D11DepthStencilView*		mDSView;
public:
	bool Init(ID11RenderContext* rc, const ICommandListDesc* desc);
	bool InitD11Point(ID11RenderContext* rc, ID3D11DeviceContext* context);


};

NS_END