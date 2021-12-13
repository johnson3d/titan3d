#include "ID11RenderPipeline.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "ID11VertexShader.h"
#include "ID11PixelShader.h"
#include "ID11InputLayout.h"
#include "ID11RasterizerState.h"
#include "ID11DepthStencilState.h"
#include "ID11BlendState.h"
#include "ID11ShaderProgram.h"

#define new VNEW

NS_BEGIN

ID11RenderPipeline::ID11RenderPipeline()
{
	
}

ID11RenderPipeline::~ID11RenderPipeline()
{
}

void ID11RenderPipeline::SetRasterizerState(ICommandList* cmd, IRasterizerState* State)
{
	auto d11Cmd = (ID11CommandList*)cmd;
	d11Cmd->mDeferredContext->RSSetState(((ID11RasterizerState*)State)->mState);
}

void ID11RenderPipeline::SetDepthStencilState(ICommandList* cmd, IDepthStencilState* State)
{
	auto d11Cmd = (ID11CommandList*)cmd;
	d11Cmd->mDeferredContext->OMSetDepthStencilState(((ID11DepthStencilState*)State)->mState, State->GetDescPtr()->StencilRef);
}

void ID11RenderPipeline::SetBlendState(ICommandList* cmd, IBlendState* State)
{
	auto d11Cmd = (ID11CommandList*)cmd;
	v3dxColor4 color(0,0,0,0);
	d11Cmd->mDeferredContext->OMSetBlendState(((ID11BlendState*)State)->mState, (const float*)&color, mSampleMask);
}

bool ID11RenderPipeline::Init(ID11RenderContext* rc, const IRenderPipelineDesc* desc)
{
	BindRenderPass(desc->RenderPass);
	BindBlendState(desc->Blend);
	BindDepthStencilState(desc->DepthStencil);
	BindGpuProgram(desc->GpuProgram);
	BindRasterizerState(desc->Rasterizer);
	return true;
}

void ID11RenderPipeline::ApplyState(ICommandList* cmd)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	if (cmd->mCurrentState.TrySet_RasterizerState(mRasterizerState))
	{
		if (mRasterizerState != nullptr)
			SetRasterizerState(cmd, mRasterizerState);
	}
	if (cmd->mCurrentState.TrySet_DepthStencilState(mDepthStencilState))
	{
		if (mDepthStencilState != nullptr)
			SetDepthStencilState(cmd, mDepthStencilState);
	}
	if (cmd->mCurrentState.TrySet_BlendState(mBlendState))
	{
		if (mBlendState != nullptr)
			SetBlendState(cmd, mBlendState);
	}
}

NS_END