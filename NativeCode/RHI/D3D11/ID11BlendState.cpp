#include "ID11BlendState.h"
#include "ID11RenderContext.h"

#define new VNEW

NS_BEGIN

ID11BlendState::ID11BlendState()
{
	mState = nullptr;
}


ID11BlendState::~ID11BlendState()
{
	Safe_Release(mState);
}

bool ID11BlendState::Init(ID11RenderContext* rc, const IBlendStateDesc* desc)
{
	mDesc = *desc;
	D3D11_BLEND_DESC dxDesc;
	dxDesc.AlphaToCoverageEnable = desc->AlphaToCoverageEnable;
	dxDesc.IndependentBlendEnable = desc->IndependentBlendEnable;
	for (int i = 0; i < 8; i++)
	{
		memcpy(&dxDesc.RenderTarget[i], &desc->RenderTarget[i], sizeof(RenderTargetBlendDesc));
	}
	auto hr = rc->mDevice->CreateBlendState(&dxDesc, &mState);
	if (hr != S_OK)
		return false;
	return true;
}

NS_END