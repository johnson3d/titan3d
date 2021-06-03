#include "ID11RenderSystem.h"
#include "ID11RenderContext.h"
#include <dwmapi.h>

//#include <dxgitype.h>
//#include <dxgi1_2.h>

#define new VNEW

extern void Unicode2Ansi(const wchar_t* src, std::string& tar)
{
	int wlen = (int)wcslen(src);
	//预转换，得到所需空间的大小，这次用的函数和上面名字相反
	int ansiLen = ::WideCharToMultiByte(CP_ACP, NULL, src, wlen, NULL, 0, NULL, NULL);
	//同上，分配空间要给'\0'留个空间
	tar.resize(ansiLen);
	//unicode版对应的strlen是wcslen
	::WideCharToMultiByte(CP_ACP, NULL, src, wlen, &tar[0], ansiLen, NULL, NULL);
}

extern void Unicode2Ansi(const wchar_t* src, char* tar)
{
	int wlen = (int)wcslen(src);
	int ansiLen = ::WideCharToMultiByte(CP_ACP, NULL, src, wlen, NULL, 0, NULL, NULL);
	::WideCharToMultiByte(CP_ACP, NULL, src, wlen, tar, ansiLen, NULL, NULL);
}

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
	memset(desc->DeviceName, 0, sizeof(desc->DeviceName));
	Unicode2Ansi(dxDesc.Description, desc->DeviceName);
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