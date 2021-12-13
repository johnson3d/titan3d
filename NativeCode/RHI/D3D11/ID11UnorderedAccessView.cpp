#include "ID11UnorderedAccessView.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "ID11GpuBuffer.h"

#define new VNEW

NS_BEGIN

ID11UnorderedAccessView::ID11UnorderedAccessView()
{
	mView = nullptr;
}


ID11UnorderedAccessView::~ID11UnorderedAccessView()
{
	Safe_Release(mView);
}

bool ID11UnorderedAccessView::Init(ID11RenderContext* rc, ID3D11Buffer* pBuffer, const IUnorderedAccessViewDesc* desc)
{
	mDesc = *desc;
	D3D11_UNORDERED_ACCESS_VIEW_DESC tmp = *(const D3D11_UNORDERED_ACCESS_VIEW_DESC*)desc;
	tmp.Format = FormatToDXFormat(desc->Format);
	auto hr = rc->mDevice->CreateUnorderedAccessView(pBuffer, &tmp, &mView);
	if (FAILED(hr))
		return false;

	//mView->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen("ShaderResource"), "ShaderResource");
	return true;
}

NS_END

