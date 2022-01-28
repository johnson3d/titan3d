#include "ID11ComputeShader.h"
#include "ID11RenderContext.h"

#define new VNEW

NS_BEGIN

ID11ComputeShader::ID11ComputeShader()
{
	mShader = nullptr;
}


ID11ComputeShader::~ID11ComputeShader()
{
	Safe_Release(mShader);
}

bool ID11ComputeShader::Init(ID11RenderContext* rc, const IShaderDesc* desc)
{
	auto hr = rc->mDevice->CreateComputeShader(&desc->GetCodes()[0], desc->GetCodes().size(), NULL, &mShader);
	if (FAILED(hr))
		return false;

	mDesc = (IShaderDesc*)desc;
#ifdef _DEBUG
	mShader->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	static UINT UniqueId = 0;
	auto debuginfo = VStringA_FormatV("CS_%u", UniqueId++);
	mShader->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
#endif
	return true;
}

NS_END
