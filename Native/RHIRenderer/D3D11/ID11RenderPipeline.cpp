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
	d11Cmd->mDeferredContext->OMSetBlendState(((ID11BlendState*)State)->mState, mBlendFactor, mSampleMask);
}

bool ID11RenderPipeline::Init(ID11RenderContext* rc, const IRenderPipelineDesc* desc)
{
	return true;
}

void ID11RenderPipeline::ApplyState(ICommandList* cmd)
{
	/*if (cmd->IsDoing() == false)
		return;*/

	if (cmd->mCurrentState.TrySet_RasterizerState(mRasterizerState))
	{
		SetRasterizerState(cmd, mRasterizerState);
	}
	if (cmd->mCurrentState.TrySet_DepthStencilState(mDepthStencilState))
	{
		SetDepthStencilState(cmd, mDepthStencilState);
	}
	if (cmd->mCurrentState.TrySet_BlendState(mBlendState))
	{
		SetBlendState(cmd, mBlendState);
	}
}

NS_END