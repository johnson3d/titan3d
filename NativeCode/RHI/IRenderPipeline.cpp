#include "IRenderPipeline.h"
#include "IRenderContext.h"
#include "IVertexShader.h"
#include "IPixelShader.h"
#include "IInputLayout.h"
#include "IRasterizerState.h"
#include "IDepthStencilState.h"
#include "IBlendState.h"
#include "IShaderProgram.h"
#include "IFrameBuffers.h"

#define new VNEW

NS_BEGIN

IRenderPipeline::IRenderPipeline()
{
	mSampleMask = 0xffffffff;
	mIsDirty = true;
}

IRenderPipeline::~IRenderPipeline()
{
	
}

void IRenderPipeline::BindRenderPass(IRenderPass* State)
{
	if (mRenderPass == State)
		return;
	mRenderPass.StrongRef(State);
	mIsDirty = true;
}

void IRenderPipeline::BindRasterizerState(IRasterizerState* State)
{
	if (mRasterizerState == State)
		return;
	mRasterizerState.StrongRef(State);
	mIsDirty = true;
}

void IRenderPipeline::BindDepthStencilState(IDepthStencilState* State)
{
	if (mDepthStencilState == State)
		return;
	mDepthStencilState.StrongRef(State);
	mIsDirty = true;
}

void IRenderPipeline::BindBlendState(IBlendState* State)
{
	if (mBlendState == State)
		return;
	mBlendState.StrongRef(State);
	mIsDirty = true;
}

void IRenderPipeline::BindGpuProgram(IShaderProgram* pGpuProgram)
{
	if (mGpuProgram == pGpuProgram)
		return;
	mGpuProgram.StrongRef(pGpuProgram);
	mIsDirty = true;
}

NS_END
