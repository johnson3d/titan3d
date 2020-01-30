#include "INullRenderPipeline.h"

#define new VNEW

NS_BEGIN

INullRenderPipeline::INullRenderPipeline()
{
	
}

INullRenderPipeline::~INullRenderPipeline()
{
}

void INullRenderPipeline::SetRasterizerState(ICommandList* cmd, IRasterizerState* Layout)
{

}

void INullRenderPipeline::SetDepthStencilState(ICommandList* cmd, IDepthStencilState* State)
{

}

void INullRenderPipeline::SetBlendState(ICommandList* cmd, IBlendState* State)
{

}

NS_END