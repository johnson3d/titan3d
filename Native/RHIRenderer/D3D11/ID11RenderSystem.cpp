#include "ID11RenderSystem.h"
#include "ID11RenderContext.h"
#include <dwmapi.h>
//#include <dxgitype.h>
//#include <dxgi1_2.h>

#define new VNEW

NS_BEGIN

ID11RenderSystem::ID11RenderSystem()
{
	m_pDXGIFactory = nullptr;
	mDeviceNumber = 0;
}


ID11RenderSystem::~ID11RenderSystem()
{
	Safe_Release(m_pDXGIFactory);
}

UINT32 ID11RenderSystem::GetContextNumber()
{
	return mDeviceNumber;
}

vBOOL ID11RenderSystem::GetContextDesc(UINT32 index, IRenderContextDesc* desc)
{
	IDXGIAdapter* pAdapter = nullptr;
	if (m_pDXGIFactory->EnumAdapters(index, &pAdapter) == DXGI_ERROR_NOT_FOUND)
	{
		return false;
	}
	DXGI_ADAPTER_DESC dxDesc;
	pAdapter->GetDesc(&dxDesc);
	auto strUni = VStringA_Unicode2Ansi(dxDesc.Description);
	strcpy(desc->DeviceName, strUni.c_str());
	desc->AdapterId = index;
	desc->VendorId = dxDesc.VendorId;
	desc->DeviceId = dxDesc.DeviceId;
	desc->Revision = dxDesc.Revision;
	desc->VideoMemory = dxDesc.DedicatedVideoMemory;
	desc->SysMemory = dxDesc.DedicatedSystemMemory;
	desc->SharedMemory = dxDesc.SharedSystemMemory;
	return TRUE;
}

IRenderContext* ID11RenderSystem::CreateContext(const IRenderContextDesc* desc)
{
	auto rc = new ID11RenderContext();
	if (rc->Init(this, desc) == false)
	{
		rc->Release();
		return nullptr;
	}
	return rc;
}

void ID11RenderSystem::SafeCreateDXGIFactory(IDXGIFactory** ppDXGIFactory)
{
	HMODULE  hmDXGI_DLL= LoadLibraryA("dxgi.dll");

	typedef HRESULT(WINAPI* FnpCreateDXGIFactory)(REFIID ridd, void** ppFactory);

	FnpCreateDXGIFactory fnpCreateDXGIFactory = (FnpCreateDXGIFactory)GetProcAddress(hmDXGI_DLL, "CreateDXGIFactory1");

	if (fnpCreateDXGIFactory == NULL)
	{
		fnpCreateDXGIFactory = (FnpCreateDXGIFactory)GetProcAddress(hmDXGI_DLL, "CreateDXGIFactory");
		if (fnpCreateDXGIFactory == NULL)
		{
			fnpCreateDXGIFactory = CreateDXGIFactory1;
		}
	}

	fnpCreateDXGIFactory(__uuidof(IDXGIFactory), (void**)ppDXGIFactory);

	
}

bool ID11RenderSystem::Init(const IRenderSystemDesc* desc)
{
	SafeCreateDXGIFactory(&m_pDXGIFactory);

	IDXGIAdapter* pAdapter = nullptr;
	
	bool bNotFound = false;
	do
	{
		if (m_pDXGIFactory->EnumAdapters(mDeviceNumber, &pAdapter) != DXGI_ERROR_NOT_FOUND)
		{
			mDeviceNumber++;
		}
		else
		{
			bNotFound = true;
		}
	} while (bNotFound == false);
	return true;
}

NS_END