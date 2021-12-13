#include "IVKBlendState.h"

#define new VNEW

NS_BEGIN

IVKBlendState::IVKBlendState()
{
}


IVKBlendState::~IVKBlendState()
{
}

bool IVKBlendState::Init(IVKRenderContext* rc, const IBlendStateDesc* desc)
{
	mDesc = *desc;

	memset(&ColorBlending, 0, sizeof(ColorBlending));
	memset(ColorBlendAttachment, 0, sizeof(ColorBlendAttachment));
	//desc->NumOfRT
	for (int i = 0; i < 8; i++)
	{
		ColorBlendAttachment[i].colorWriteMask = desc->RenderTarget[i].RenderTargetWriteMask;//VK_COLOR_COMPONENT_R_BIT | VK_COLOR_COMPONENT_G_BIT | VK_COLOR_COMPONENT_B_BIT | VK_COLOR_COMPONENT_A_BIT;
		ColorBlendAttachment[i].blendEnable = desc->RenderTarget[i].BlendEnable;
		
		ColorBlendAttachment[i].colorBlendOp = BlendOp2VKBlendOp(desc->RenderTarget[i].BlendOp);
		ColorBlendAttachment[i].srcColorBlendFactor = BlendFactor2VKBlendFactor(desc->RenderTarget[i].SrcBlend);
		ColorBlendAttachment[i].dstColorBlendFactor = BlendFactor2VKBlendFactor(desc->RenderTarget[i].DestBlend);

		ColorBlendAttachment[i].alphaBlendOp = BlendOp2VKBlendOp(desc->RenderTarget[i].BlendOpAlpha);
		ColorBlendAttachment[i].srcAlphaBlendFactor = BlendFactor2VKBlendFactor(desc->RenderTarget[i].SrcBlendAlpha);
		ColorBlendAttachment[i].dstAlphaBlendFactor = BlendFactor2VKBlendFactor(desc->RenderTarget[i].DestBlendAlpha);
	}
	
	ColorBlending.sType = VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO;
	ColorBlending.logicOpEnable = VK_FALSE;
	ColorBlending.logicOp = VK_LOGIC_OP_COPY;
	ColorBlending.attachmentCount = 1;//desc->NumOfRT;
	ColorBlending.pAttachments = ColorBlendAttachment;
	ColorBlending.blendConstants[0] = 0.0f;//blendfactor...
	ColorBlending.blendConstants[1] = 0.0f;
	ColorBlending.blendConstants[2] = 0.0f;
	ColorBlending.blendConstants[3] = 0.0f;
	return true;
}

NS_END