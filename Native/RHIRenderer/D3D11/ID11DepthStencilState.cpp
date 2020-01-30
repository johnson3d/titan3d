#include "ID11DepthStencilState.h"
#include "ID11RenderContext.h"

#define new VNEW

NS_BEGIN

ID11DepthStencilState::ID11DepthStencilState()
{
	mState = nullptr;
}


ID11DepthStencilState::~ID11DepthStencilState()
{
	Safe_Release(mState);
}

bool ID11DepthStencilState::Init(ID11RenderContext* rc, const IDepthStencilStateDesc* desc)
{
	mDesc = *desc;
	//memcpy(&mDesc, desc, sizeof(D3D11_DEPTH_STENCIL_DESC));
	auto hr = rc->mDevice->CreateDepthStencilState((D3D11_DEPTH_STENCIL_DESC*)&mDesc, &mState);
	if (FAILED(hr))
		return false;
	return true;
}

NS_END