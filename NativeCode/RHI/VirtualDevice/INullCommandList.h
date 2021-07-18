#pragma once
#include "../ICommandList.h"
#include "NullPreHead.h"

NS_BEGIN

class INullRenderContext;
class INullCommandList : public ICommandList
{
public:
	INullCommandList();
	~INullCommandList();

	virtual void BeginCommand() override;
	virtual void EndCommand() override;

	virtual void BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer, const char* debugName) override;
	//virtual void BuildRenderPass(vBOOL bImmCBuffer, UINT* limitter, IPass** ppPass) override;
	virtual void EndRenderPass() override;

	virtual void Commit(IRenderContext* pRHICtx) override;

	virtual void SetRasterizerState(IRasterizerState* State) override;
	virtual void SetDepthStencilState(IDepthStencilState* State) override;
	virtual void SetBlendState(IBlendState* State, float* blendFactor, UINT samplerMask) override;

	virtual void SetComputeShader(IComputeShader* ComputerShader) override;
	virtual void CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture) override;
	virtual void CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT* pUAVInitialCounts) override;
	virtual void CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer) override;
	virtual void CSDispatch(UINT x, UINT y, UINT z) override;
	virtual vBOOL CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers) override;
};

NS_END