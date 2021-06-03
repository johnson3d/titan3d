#pragma once
#include "../ICommandList.h"
#include "GLSdk.h"

NS_BEGIN

class IGLRenderContext;
class IGLFrameBuffers;
class IGLCommandList : public ICommandList
{
public:
	IGLCommandList();
	~IGLCommandList();

	virtual void Cleanup() override;

	virtual void BeginCommand() override;
	virtual void EndCommand() override;

	/*virtual void SetRenderTargets(IFrameBuffers* FrameBuffers) override;

	virtual void ClearMRT(const std::pair<BYTE, DWORD>* ClearColors, int ColorNum,
		vBOOL bClearDepth, float Depth, vBOOL bClearStencil, UINT32 Stencil) override;*/

	virtual void BeginRenderPass(RenderPassDesc* pRenderPassDesc, IFrameBuffers* pFrameBuffer) override;
	//virtual void BuildRenderPass(vBOOL bImmCBuffer, UINT* limitter, IPass** ppPass) override;
	virtual void EndRenderPass() override;

	virtual void Blit2DefaultFrameBuffer(IFrameBuffers* FrameBuffers, int dstWidth, int dstHeight) override;

	virtual void Commit(IRenderContext* pRHICtx) override;

	virtual void SetComputeShader(IComputeShader* ComputerShader) override;
	virtual void CSSetShaderResource(UINT32 Index, IShaderResourceView* Texture) override;
	virtual void CSSetUnorderedAccessView(UINT32 Index, IUnorderedAccessView* view, const UINT *pUAVInitialCounts) override;
	virtual void CSSetConstantBuffer(UINT32 Index, IConstantBuffer* cbuffer) override;
	virtual void CSDispatch(UINT x, UINT y, UINT z) override;
	virtual vBOOL CreateReadableTexture2D(ITexture2D** ppTexture, IShaderResourceView* src, IFrameBuffers* pFrameBuffers) override;
public:
	bool Init(IGLRenderContext* rc, const ICommandListDesc* desc, GLSdk* sdk);
	AutoRef<GLSdk>		mCmdList;

	std::shared_ptr<GLSdk::GLBufferId>		mSavedFrameBuffer;
};

NS_END