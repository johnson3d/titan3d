#include "ID11RasterizerState.h"
#include "ID11RenderContext.h"

#define new VNEW

NS_BEGIN

ID11RasterizerState::ID11RasterizerState()
{
	mState = nullptr;
}

ID11RasterizerState::~ID11RasterizerState()
{
	Safe_Release(mState);
}

bool ID11RasterizerState::Init(ID11RenderContext* rc, const IRasterizerStateDesc* desc)
{
	mDesc = *desc;
	mD11Desc.FillMode = (D3D11_FILL_MODE)desc->FillMode;
	mD11Desc.CullMode = (D3D11_CULL_MODE)desc->CullMode;
	mD11Desc.FrontCounterClockwise = desc->FrontCounterClockwise;
	mD11Desc.DepthBias = desc->DepthBias;
	mD11Desc.DepthBiasClamp = desc->DepthBiasClamp;
	mD11Desc.SlopeScaledDepthBias = desc->SlopeScaledDepthBias;
	mD11Desc.DepthClipEnable = desc->DepthClipEnable;
	mD11Desc.ScissorEnable = desc->ScissorEnable;
	mD11Desc.MultisampleEnable = desc->MultisampleEnable;
	mD11Desc.AntialiasedLineEnable = desc->AntialiasedLineEnable;
	rc->mDevice->CreateRasterizerState(&mD11Desc, &mState);
	return true;
}

NS_END