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

#ifdef _DEBUG
	mShader->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	static UINT UniqueId = 0;
	auto debuginfo = VStringA_FormatV("PS_%u", UniqueId++);
	mShader->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
#endif
	
	return true;
}

NS_END