#include "ID11VertexShader.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "ID11ConstantBuffer.h"

#define new VNEW

NS_BEGIN

ID11VertexShader::ID11VertexShader()
{
	mShader = nullptr;
}

ID11VertexShader::~ID11VertexShader()
{
	Safe_Release(mShader);
}

bool ID11VertexShader::Init(ID11RenderContext* rc, const IShaderDesc* desc)
{
	auto hr = rc->mDevice->CreateVertexShader(&desc->GetCodes()[0], desc->GetCodes().size(), NULL, &mShader);
	if (FAILED(hr))
		return false;
	
	mDesc = (IShaderDesc*)desc;

	return true;
}

void ID11VertexShader::SetDebugName(const char* name)
{
	mShader->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	mShader->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen(name), name);
}

NS_END