#include "ID11VertexBuffer.h"
#include "ID11RenderContext.h"
#include "ID11CommandList.h"
#include "ID11GpuBuffer.h"

#define new VNEW

NS_BEGIN

ID11VertexBuffer::ID11VertexBuffer()
{
	mBuffer = nullptr;
}

ID11VertexBuffer::~ID11VertexBuffer()
{
	Safe_Release(mBuffer);
}

void ID11VertexBuffer::GetBufferData(IRenderContext* rc, IBlobObject* data)
{
	auto d11rc = (ID11RenderContext*)rc;
	
	D3D11_BUFFER_DESC bd;
	memset(&bd, 0, sizeof(bd));
	bd.Usage = D3D11_USAGE_STAGING;
	bd.ByteWidth = mDesc.ByteWidth;
	bd.BindFlags = 0;// D3D11_BIND_VERTEX_BUFFER;
	bd.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE | D3D11_CPU_ACCESS_READ;
	D3D11_SUBRESOURCE_DATA InitData;
	memset(&InitData, 0, sizeof(InitData));
	InitData.pSysMem = new BYTE[mDesc.ByteWidth];

	ID3D11Buffer*			memBuffer;

	auto hr = d11rc->mDevice->CreateBuffer(&bd, &InitData, &memBuffer);
	delete[](BYTE*)InitData.pSysMem;
	if (FAILED(hr))
		return;
	
	InitData.pSysMem = nullptr;

	auto pContext = d11rc->mHardwareContext;
	d11rc->mHWContextLocker.Lock();
	pContext->CopyResource(memBuffer, mBuffer);
	D3D11_MAPPED_SUBRESOURCE mappedRes;
	if (S_OK == pContext->Map(memBuffer, 0, D3D11_MAP_READ, 0, &mappedRes))
	{
		data->ReSize(mDesc.ByteWidth);
		memcpy(data->GetData(), mappedRes.pData, mDesc.ByteWidth);

		pContext->Unmap(memBuffer, 0);
	}
	d11rc->mHWContextLocker.Unlock();
	memBuffer->Release();
}

void ID11VertexBuffer::UpdateGPUBuffData(ICommandList* cmd, void* ptr, UINT size)
{
	if (mDesc.ByteWidth < size)
	{
		ASSERT(false);
		return;
	}

	auto pContext = ((ID11CommandList*)cmd)->mDeferredContext;
	if (size < mDesc.ByteWidth && mDesc.CPUAccess & CAS_WRITE)
	{
		D3D11_MAPPED_SUBRESOURCE mapData;
		if (S_OK == pContext->Map(mBuffer, 0, D3D11_MAP_WRITE_DISCARD, 0, &mapData))
		{
			memcpy(mapData.pData, ptr, size);
			pContext->Unmap(mBuffer, 0);
		}
	}
	else
	{
		pContext->UpdateSubresource(mBuffer, 0, nullptr, ptr, 0, 0);
	}
}

bool ID11VertexBuffer::Init(ID11RenderContext* rc, const IVertexBufferDesc* desc)
{
	D3D11_BUFFER_DESC bd;
	memset(&bd, 0, sizeof(bd));
	bd.ByteWidth = desc->ByteWidth;
	bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
	bd.CPUAccessFlags = desc->CPUAccess;
	if (desc->CPUAccess != 0)
	{
		bd.Usage = D3D11_USAGE_DYNAMIC;
		bd.CPUAccessFlags = D3D11_CPU_ACCESS_WRITE;
	}
	else
	{
		bd.Usage = D3D11_USAGE_DEFAULT;
		bd.CPUAccessFlags = 0;
	}
	D3D11_SUBRESOURCE_DATA InitData;
	memset(&InitData, 0, sizeof(InitData));
	if (desc->InitData!=nullptr)
	{
		InitData.pSysMem = desc->InitData;
	}
	else
	{
		InitData.pSysMem = new BYTE[desc->ByteWidth];
	}
	mDesc = *desc;
	mDesc.InitData = nullptr;
	auto hr = rc->mDevice->CreateBuffer(&bd, &InitData, &mBuffer);
	if (desc->InitData == nullptr)
	{
		Safe_DeleteArray(InitData.pSysMem);
	}
	if (FAILED(hr))
	{
		if (DXGI_ERROR_DEVICE_REMOVED == hr)
		{
			auto hr2 = rc->mDevice->GetDeviceRemovedReason();
			switch (hr)
			{
			case DXGI_ERROR_DEVICE_HUNG:
				VFX_LTRACE(ELTT_Graphics, "VertexBuffer Create failed:%d: DEVICE_HUNG\r\n", hr);
				break;
			case DXGI_ERROR_DEVICE_REMOVED:
				VFX_LTRACE(ELTT_Graphics, "VertexBuffer Create failed:%d: DEVICE_REMOVED\r\n", hr);
				break;
			case DXGI_ERROR_DEVICE_RESET:
				VFX_LTRACE(ELTT_Graphics, "VertexBuffer Create failed:%d: DEVICE_RESET\r\n", hr);
				break;
			case DXGI_ERROR_DRIVER_INTERNAL_ERROR:
				VFX_LTRACE(ELTT_Graphics, "VertexBuffer Create failed:%d: INTERNAL_ERROR\r\n", hr);
				break;
			case DXGI_ERROR_INVALID_CALL:
				VFX_LTRACE(ELTT_Graphics, "VertexBuffer Create failed:%d: INVALID_CALL\r\n", hr);
				break;
			case S_OK:
				VFX_LTRACE(ELTT_Graphics, "VertexBuffer Create failed:%d: OK\r\n", hr);
				break;
			default:
				VFX_LTRACE(ELTT_Graphics, "VertexBuffer Create failed:%d: unknown\r\n", hr);
				break;
			}
		}
		
		return false;
	}

#ifdef _DEBUG
	mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	static UINT UniqueId = 0;
	auto debuginfo = VStringA_FormatV("VB_%u", UniqueId++);
	mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
#endif
	return true;
}

bool ID11VertexBuffer::Init(ID11RenderContext* rc, const IVertexBufferDesc* desc, const IGpuBuffer* pBuffer)
{
	mDesc = *desc;
	
	mBuffer = ((ID11GpuBuffer*)pBuffer)->mBuffer;
	mBuffer->AddRef();

#ifdef _DEBUG
	mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
	static UINT UniqueId = 0;
	auto debuginfo = VStringA_FormatV("VB_%u", UniqueId++);
	mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
#endif
	return true;
}

UINT GVBNameId = 0;

void ID11VertexBuffer::SetDebugInfo(const char* info)
{
	UINT size = (UINT)mDebugInfo.length();
	if (mBuffer != nullptr)
	{
		mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, 0, NULL);
		static UINT UniqueId = 0;
		auto debuginfo = VStringA_FormatV("%s->%u", info, UniqueId++);
		auto hr = mBuffer->SetPrivateData(WKPDID_D3DDebugObjectName, (UINT)debuginfo.length(), debuginfo.c_str());
		if (hr != S_OK)
		{
			_vfxTraceA("VB DebugName: %s\r\n", info);
		}
	}
}

NS_END