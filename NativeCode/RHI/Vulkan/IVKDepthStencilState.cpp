#include "IVKDepthStencilState.h"

#define new VNEW

NS_BEGIN

IVKDepthStencilState::IVKDepthStencilState()
{
}


IVKDepthStencilState::~IVKDepthStencilState()
{
	
}

bool IVKDepthStencilState::Init(IVKRenderContext* rc, const IDepthStencilStateDesc* desc)
{
	mDesc = *desc;
	memset(&CreateInfo, 0, sizeof(CreateInfo));
	CreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO;
	CreateInfo.depthTestEnable = desc->DepthEnable;
	CreateInfo.depthWriteEnable = (desc->DepthWriteMask == DSWM_ZERO) ? FALSE : TRUE;
	CreateInfo.depthCompareOp = CompareOp2VKCompareOp(desc->DepthFunc);
	CreateInfo.depthBoundsTestEnable = VK_FALSE;
	CreateInfo.stencilTestEnable = desc->StencilEnable;
	CreateInfo.front.failOp = StencilOp2VKStencilOp(desc->FrontFace.StencilFailOp);
	CreateInfo.front.passOp = StencilOp2VKStencilOp(desc->FrontFace.StencilPassOp);
	CreateInfo.front.depthFailOp = StencilOp2VKStencilOp(desc->FrontFace.StencilDepthFailOp);
	CreateInfo.front.compareOp = CompareOp2VKCompareOp(desc->FrontFace.StencilFunc);
	
	CreateInfo.back.failOp = StencilOp2VKStencilOp(desc->BackFace.StencilFailOp);
	CreateInfo.back.passOp = StencilOp2VKStencilOp(desc->BackFace.StencilPassOp);
	CreateInfo.back.depthFailOp = StencilOp2VKStencilOp(desc->BackFace.StencilDepthFailOp);
	CreateInfo.back.compareOp = CompareOp2VKCompareOp(desc->BackFace.StencilFunc);
	return true;
}

NS_END

