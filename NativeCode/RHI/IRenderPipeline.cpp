#include "IRenderPipeline.h"
#include "IVertexShader.h"
#include "IPixelShader.h"
#include "IInputLayout.h"
#include "IRasterizerState.h"
#include "IDepthStencilState.h"
#include "IBlendState.h"
#include "IShaderProgram.h"

#define new VNEW

NS_BEGIN

IRenderPipeline::IRenderPipeline()
{
	mSampleMask = 0xffffffff;
	memset(&mBlendFactor, 0, sizeof(mBlendFactor));
}

IRenderPipeline::~IRenderPipeline()
{
	
}

void IRenderPipeline::BindRasterizerState(IRasterizerState* State)
{
	mRasterizerState.StrongRef(State);
}

void IRenderPipeline::BindDepthStencilState(IDepthStencilState* State)
{
	mDepthStencilState.StrongRef(State);
}

void IRenderPipeline::BindBlendState(IBlendState* State)
{
	mBlendState.StrongRef(State);
}

void IRenderPipeline::BindGpuProgram(IShaderProgram* pGpuProgram)
{
	mGpuProgram.StrongRef(pGpuProgram);
}

NS_END
