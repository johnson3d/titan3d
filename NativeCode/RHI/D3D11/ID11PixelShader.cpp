#include "ID11PixelShader.h"
#include "ID11RenderContext.h"

#define new VNEW

NS_BEGIN

ID11PixelShader::ID11PixelShader()
{
	mShader = nullptr;
}


ID11PixelShader::~ID11PixelShader()
{
	Safe_Release(mShader);
}

bool ID11PixelShader::Init(ID11RenderContext* rc, const IShaderDesc* desc)
{
	auto hr = rc->mDevice->CreatePixelShader(&desc->GetCodes()[0], desc->GetCodes().size(), NULL, &mShader);
	if (FAILED(hr))
		return false;

	mDesc = (IShaderDesc*)desc;
	mDesc->AddRef();

	return true;
}

void ID11PixelShader::SetDebugName(const char* name)
{
	mShader->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	mShader->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen(name), name);
}

NS_END