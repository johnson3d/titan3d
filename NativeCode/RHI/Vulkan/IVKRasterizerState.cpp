#include "IVKRasterizerState.h"

#define new VNEW

NS_BEGIN

IVKRasterizerState::IVKRasterizerState()
{
}


IVKRasterizerState::~IVKRasterizerState()
{
}

bool IVKRasterizerState::Init(IVKRenderContext* rc, const IRasterizerStateDesc* desc)
{
	memset(&mCreateInfo, 0, sizeof(mCreateInfo));

	mCreateInfo.sType = VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO;
	mCreateInfo.rasterizerDiscardEnable = VK_FALSE;
	mCreateInfo.polygonMode = FillMode2VKFillMode(desc->FillMode);
	mCreateInfo.lineWidth = 1.0f;
	mCreateInfo.cullMode = CullMode2VKCullMode(desc->CullMode);
	mCreateInfo.frontFace = desc->FrontCounterClockwise ? VK_FRONT_FACE_COUNTER_CLOCKWISE : VK_FRONT_FACE_CLOCKWISE;
	
	mCreateInfo.depthClampEnable = desc->DepthBiasClamp == 0 ? FALSE : TRUE;
	mCreateInfo.depthBiasEnable = desc->DepthBias;
	mCreateInfo.depthBiasClamp = desc->DepthBiasClamp;
	mCreateInfo.depthBiasSlopeFactor = desc->SlopeScaledDepthBias;
	
	return true;
}

NS_END