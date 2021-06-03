#include "ID11UnorderedAccessView.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"

#define new VNEW

NS_BEGIN

vBOOL ID11GpuBuffer::GetBufferData(IRenderContext* rc, IBlobObject* blob)
{
	ID3D11Buffer* CopyBuffer;

	D3D11_BUFFER_DESC desc;
	ZeroMemory(&desc, sizeof(desc));
	mBuffer->GetDesc(&desc);
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
	desc.Usage = D3D11_USAGE_STAGING;
	desc.BindFlags = 0;
	desc.MiscFlags = 0;
	auto dxCtx = (ID11RenderContext*)rc;
	auto hr = dxCtx->mDevice->CreateBuffer(&desc, NULL, &CopyBuffer);
	if (hr != S_OK)
	{
		return FALSE;
	}

	auto pContext = dxCtx->mImmCmdList->mDeferredContext;
	pContext->CopyResource(CopyBuffer, mBuffer);

	D3D11_MAPPED_SUBRESOURCE MappedResource;
	hr = pContext->Map(CopyBuffer, 0, D3D11_MAP_READ, 0, &MappedResource);
	if (hr != S_OK)
	{
		return FALSE;
	}

	blob->PushData(MappedResource.pData, desc.ByteWidth);

	pContext->Unmap(CopyBuffer, 0);
	Safe_Release(CopyBuffer);

	return TRUE;
}

vBOOL ID11GpuBuffer::Map(IRenderContext* rc,
	UINT Subresource,
	EGpuMAP MapType,
	UINT MapFlags,
	IMappedSubResource* mapRes)
{
	Safe_Release(mCopyBuffer);

	D3D11_BUFFER_DESC desc;
	ZeroMemory(&desc, sizeof(desc));
	mBuffer->GetDesc(&desc);
	desc.CPUAccessFlags = D3D11_CPU_ACCESS_READ;
	desc.Usage = D3D11_USAGE_STAGING;
	desc.BindFlags = 0;
	desc.MiscFlags = 0;
	auto dxCtx = (ID11RenderContext*)rc;
	auto hr = dxCtx->mDevice->CreateBuffer(&desc, NULL, &mCopyBuffer);
	if (hr!=S_OK)
	{
		return FALSE;
	}

	auto pContext = dxCtx->mHardwareContext;
	dxCtx->mHWContextLocker.Lock();
	//auto pContext = dxCtx->mImmCmdList->mDeferredContext;
	pContext->CopyResource(mCopyBuffer, mBuffer);

	hr = pContext->Map(mCopyBuffer, Subresource, (D3D11_MAP)MapType, MapFlags, (D3D11_MAPPED_SUBRESOURCE*)mapRes);
	if (hr != S_OK)
	{
		dxCtx->mHWContextLocker.Unlock();
		return FALSE;
	}

	return TRUE;
}

void ID11GpuBuffer::Unmap(IRenderContext* rc, UINT Subresource)
{
	auto dxCtx = (ID11RenderContext*)rc;
	auto pContext = dxCtx->mImmCmdList->mDeferredContext;

	pContext->Unmap(mCopyBuffer, Subresource);
	Safe_Release(mCopyBuffer);
	dxCtx->mHWContextLocker.Unlock();
}

vBOOL ID11GpuBuffer::UpdateBufferData(ICommandList* cmd, void* data, UINT size)
{
	if (size > mDesc.ByteWidth)
		return FALSE;

	auto pContext = ((ID11CommandList*)cmd)->mDeferredContext;
	if (size < mDesc.ByteWidth && mDesc.CPUAccessFlags & CAS_WRITE)
	{
		D3D11_MAPPED_SUBRESOURCE mapRes;
		auto hr = pContext->Map(mBuffer, 0, (D3D11_MAP)D3D11_MAP_WRITE_DISCARD, 0, &mapRes);
		if (hr != S_OK)
		{
			return FALSE;
		}

		memcpy(mapRes.pData, data, size);

		pContext->Unmap(mBuffer, 0);
	}
	else
	{
		pContext->UpdateSubresource(mBuffer, 0, nullptr, data, 0, 0);
	}
	
	return TRUE;
}

//////////////////////////////////////////////////////////////////////////

ID11GpuBuffer::ID11GpuBuffer()
{
	mBuffer = nullptr;
	mCopyBuffer = nullptr;
}

ID11GpuBuffer::~ID11GpuBuffer()
{
	Safe_Release(mBuffer);
	Safe_Release(mCopyBuffer);
}

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
	D3D11_UNORDERED_ACCESS_VIEW_DESC tmp = *(const D3D11_UNORDERED_ACCESS_VIEW_DESC*)desc;
	tmp.Format = FormatToDXFormat(desc->Format);
	auto hr = rc->mDevice->CreateUnorderedAccessView(pBuffer, &tmp, &mView);
	if (FAILED(hr))
		return false;

	//mView->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)strlen("ShaderResource"), "ShaderResource");
	return true;
}

NS_END

