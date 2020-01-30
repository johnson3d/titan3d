#include "IGLRenderPipeline.h"
#include "IGLRasterizerState.h"
#include "IGLDepthStencilState.h"
#include "IGLBlendState.h"
#include "IGLCommandList.h"

#define new VNEW

NS_BEGIN

IGLRenderPipeline::IGLRenderPipeline()
{
	
}

IGLRenderPipeline::~IGLRenderPipeline()
{
	Cleanup();
}

void IGLRenderPipeline::Cleanup()
{

}

void IGLRenderPipeline::SetRasterizerState(ICommandList* cmd, IRasterizerState* State)
{
	((IGLRasterizerState*)State)->ApplyStates( ((IGLCommandList*)cmd)->mCmdList );
}

void IGLRenderPipeline::SetDepthStencilState(ICommandList* cmd, IDepthStencilState* State)
{
	((IGLDepthStencilState*)State)->ApplyStates( ((IGLCommandList*)cmd)->mCmdList );
}

void IGLRenderPipeline::SetBlendState(ICommandList* cmd, IBlendState* State)
{
	((IGLBlendState*)State)->ApplyStates(((IGLCommandList*)cmd)->mCmdList );
}

void IGLRenderPipeline::ApplyState(ICommandList* cmd)
{
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

bool IGLRenderPipeline::Init(IGLRenderContext* rc, const IRenderPipelineDesc* desc)
{
	return true;
}

NS_END