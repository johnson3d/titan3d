#include "ID11Fence.h"

#define new VNEW

NS_BEGIN

ID11Fence::ID11Fence()
{
	mQuery = nullptr;
	mContext = nullptr;
}

ID11Fence::~ID11Fence()
{
	Safe_Release(mQuery);
}

bool ID11Fence::Init(ID3D11Device* pDevice)
{
	D3D11_QUERY_DESC desc;
	desc.Query = D3D11_QUERY_EVENT;
	desc.MiscFlags = 0;
	auto hr = pDevice->CreateQuery(&desc, &mQuery);
	if (hr != S_OK)
		return false;
	return true;
}

void ID11Fence::Wait()
{

}

void ID11Fence::Reset()
{
	if (mContext == nullptr)
	{
		return;
	}
	BOOL ok = FALSE;
	while (mContext->GetData(mQuery, &ok, sizeof(BOOL), 0) != S_OK)
	{
		
	}
	mContext = nullptr;
}

bool ID11Fence::IsCompletion()
{
	if (mContext == nullptr)
	{
		return false;
	}
	BOOL ok = FALSE;
	auto hr = mContext->GetData(mQuery, &ok, sizeof(BOOL), 0);
	if (hr == S_OK)
	{
		mContext = nullptr;
		return true;
	}
	return false;
}

NS_END
