#include "stdafx.h"
#include "CLRProfilerCallbackImpl.h"
#include <mscoree.h>  
#include "basehdr.h"
#include "regutil.hpp"

extern const GUID __declspec(selectany) CLSID_PROFILER =
{ 0xa347c588, 0x436, 0x471c, { 0x99, 0x1a, 0x31, 0xc7, 0x5, 0x7, 0xaa, 0x9 } };

#include "classfactory.hpp"

#pragma comment(lib, "mscoree.lib") 
#pragma comment(lib, "corguids.lib") 


STDAPI DllUnregisterServer();

BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved)
{
	// save off the instance handle for later use
	switch (dwReason)
	{
	case DLL_PROCESS_ATTACH:
		g_hInst = hInstance;
		DisableThreadLibraryCalls(hInstance);
		break;
	case DLL_PROCESS_DETACH:
		break;
	default:
		break;
	}
	return TRUE;
} // DllMain

STDAPI DllGetClassObject(REFCLSID rclsid, REFIID riid, LPVOID FAR *ppv)
{
	CClassFactory *pClassFactory;
	const COCLASS_REGISTER *pCoClass;
	HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;

	// scan for the right one
	for (pCoClass = g_CoClasses; pCoClass->pClsid != NULL; pCoClass++)
	{
		if (*pCoClass->pClsid == rclsid)
		{
			pClassFactory = new CClassFactory(pCoClass);
			if (pClassFactory != NULL)
			{
				hr = pClassFactory->QueryInterface(riid, ppv);

				pClassFactory->Release();
				break;
			}
			else
			{
				hr = E_OUTOFMEMORY;
				break;
			}
		}
	} // for

	return hr;

} // DllGetClassObject

STDAPI_(char *) GetGUIDAsString()
{
	return (char*)PROFILER_GUID;
} // GetGUIDAsString

STDAPI DllRegisterServer()
{
	HRESULT hr = S_OK;
	char  rcModule[_MAX_PATH];
	const COCLASS_REGISTER *pCoClass;


	DllUnregisterServer();
	GetModuleFileNameA(g_hInst, rcModule, _MAX_PATH);

	// for each item in the coclass list, register it
	for (pCoClass = g_CoClasses; (SUCCEEDED(hr) && (pCoClass->pClsid != NULL)); pCoClass++)
	{
		// register the class with default values
		hr = REGUTIL::RegisterCOMClass(*pCoClass->pClsid,
			g_szCoclassDesc,
			g_szProgIDPrefix,
			g_iVersion,
			pCoClass->szProgID,
			g_szThreadingModel,
			rcModule);
	} // for


	if (FAILED(hr))
		DllUnregisterServer();

	return hr;

} // DllRegisterServer

STDAPI DllUnregisterServer()
{
	const COCLASS_REGISTER *pCoClass;


	// for each item in the coclass list, unregister it
	for (pCoClass = g_CoClasses; pCoClass->pClsid != NULL; pCoClass++)
	{
		REGUTIL::UnregisterCOMClass(*pCoClass->pClsid,
			g_szProgIDPrefix,
			g_iVersion,
			pCoClass->szProgID);
	} // for


	return S_OK;

} // DllUnregisterServer

CLRProfiler gCLRProfiler;
HRESULT EnableDebugPrivilege()
{
	HRESULT hr;
	HANDLE hProcessToken = NULL;
	LPBYTE pbTokenInformation = NULL;
	TOKEN_PRIVILEGES * pPrivileges = NULL;
	BOOL fFoundDebugPrivilege = FALSE;
	LUID_AND_ATTRIBUTES * pLuidAndAttrs = NULL;
	DWORD cbTokenInformation = 0;

	if (!OpenProcessToken(
		GetCurrentProcess(),
		TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY,
		&hProcessToken))
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		goto Cleanup;
	}

	DWORD cbTokenInformationOut;
	if (!GetTokenInformation(
		hProcessToken,
		TokenPrivileges,
		NULL,               // TokenInformation
		0,                  // TokenInformationLength,
		&cbTokenInformationOut))
	{
		DWORD dwLastError = GetLastError();
		if (dwLastError != ERROR_INSUFFICIENT_BUFFER)
		{
			hr = HRESULT_FROM_WIN32(dwLastError);
			goto Cleanup;
		}
	}

	cbTokenInformation = cbTokenInformationOut;
	pbTokenInformation = new BYTE[cbTokenInformation];
	if (pbTokenInformation == NULL)
	{
		hr = E_OUTOFMEMORY;
		goto Cleanup;
	}

	if (!GetTokenInformation(
		hProcessToken,
		TokenPrivileges,
		pbTokenInformation,
		cbTokenInformation,
		&cbTokenInformationOut))
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		goto Cleanup;
	}

	pPrivileges = (TOKEN_PRIVILEGES *)pbTokenInformation;
	fFoundDebugPrivilege = FALSE;
	pLuidAndAttrs = NULL;

	for (DWORD i = 0; i < pPrivileges->PrivilegeCount; i++)
	{
		pLuidAndAttrs = &(pPrivileges->Privileges[i]);
		WCHAR wszPrivilegeName[100];
		DWORD cchPrivilegeName = 100;
		if (!LookupPrivilegeName(
			NULL,       // lpSystemName
			&(pLuidAndAttrs->Luid),
			&(wszPrivilegeName[0]),
			&cchPrivilegeName))
		{
			hr = HRESULT_FROM_WIN32(GetLastError());
			goto Cleanup;
		}

		if (wcscmp(wszPrivilegeName, SE_DEBUG_NAME) == 0)
		{
			fFoundDebugPrivilege = TRUE;
			break;
		}
	}

	if (!fFoundDebugPrivilege)
	{
		//Unable to find SeDebugPrivilege; user may not be able to profile higher integrity proceses. 
		//return silently and give it a try.
		//if the attach failed, let the customer know they can run CLRProfiler as administrator and try again. 
		hr = E_FAIL;
		goto Cleanup;
	}

	if ((pLuidAndAttrs->Attributes & SE_PRIVILEGE_ENABLED) != 0)
	{
		// Privilege already enabled.  Nothing to do.
		hr = S_OK;
		// Log(L"SeDebugPrivilege is already enabled.\n");
		goto Cleanup;
	}

	// Log(L"SeDebugPrivilege available but disabled.  Attempting to enable it...\n");
	pLuidAndAttrs->Attributes |= SE_PRIVILEGE_ENABLED;
	if (!AdjustTokenPrivileges(
		hProcessToken,
		FALSE,              // DisableAllPrivileges,
		pPrivileges,
		cbTokenInformationOut,
		NULL,               // PreviousState,
		NULL                // ReturnLength
	))
	{
		hr = HRESULT_FROM_WIN32(GetLastError());
		goto Cleanup;
	}

	hr = S_OK;

Cleanup:
	if (hProcessToken != NULL)
	{
		CloseHandle(hProcessToken);
		hProcessToken = NULL;
	}

	if (pbTokenInformation != NULL)
	{
		delete[] pbTokenInformation;
		pbTokenInformation = NULL;
	}

	return hr;
}

HRESULT CLRProfiler::CallAttachProfiler(HANDLE hProcess, DWORD dwProfileeProcID, DWORD dwMillisecondsTimeout,
	GUID profilerCLSID, LPCWSTR wszProfilerPath)
{
	hProcess = ::GetCurrentProcess();
	dwProfileeProcID = ::GetCurrentProcessId();
	//CLSIDFromString(PROFILER_GUID_WCHAR, &profilerCLSID);
	CoInitializeEx(0, COINIT_APARTMENTTHREADED);

	DllRegisterServer();

	// This can be a data type of your own choosing for sending configuration data 
	// to your profiler:
	//MyProfilerConfigData profConfig;
	//PopulateMyProfilerConfigData(&profConfig);
	LPVOID pvClientData = this;
	DWORD cbClientData = sizeof(CLRProfiler);
	
	ICLRMetaHost * pMetaHost = NULL;
	IEnumUnknown * pEnum = NULL;
	IUnknown * pUnk = NULL;
	ICLRRuntimeInfo * pRuntime = NULL;
	ICLRProfiling * pProfiling = NULL;
	HRESULT hr = E_FAIL;
	
	hr = CLRCreateInstance(CLSID_CLRMetaHost, IID_ICLRMetaHost, (LPVOID *)&pMetaHost);
	if (FAILED(hr))
		goto Cleanup;

	EnableDebugPrivilege();

	hr = pMetaHost->EnumerateLoadedRuntimes(hProcess, &pEnum);
	if (FAILED(hr))
		goto Cleanup;

	hProcess = nullptr;
	hr = pMetaHost->GetRuntime(L"v4.0.30319", IID_ICLRRuntimeInfo, (LPVOID*)&pRuntime);
	if (hr == S_OK) 
	{
		hr = pRuntime->GetInterface(CLSID_CLRProfiling, IID_ICLRProfiling, (LPVOID *)&pProfiling);
		if (SUCCEEDED(hr))
		{
			hr = pProfiling->AttachProfiler(
				dwProfileeProcID,
				dwMillisecondsTimeout,
				&profilerCLSID,//"8C29BC4E-1F57-461a-9B51-1200C32E6F1F"
				wszProfilerPath,
				pvClientData,
				cbClientData
			);
			if (FAILED(hr))
			{
				if (hr == ERROR_TIMEOUT)//ERROR_TIMEOUT 
				{
					OutputDebugStringW(L"CLRProfiler timed out to attach to the process.\nPlease check the event log to find out whether the attach succeeded or failed.");
				}
				else if (hr == COR_E_UNAUTHORIZEDACCESS)//0x80070005
				{
					OutputDebugStringW(L"CLRProfiler failed to attach to the process with error code 0x80070005(COR_E_UNAUTHORIZEDACCESS).\n"
						L"This may occur if the target process(%d) does not have access to ProfilerOBJ.dll or the directory in which ProfilerOBJ.dll is located.\n"
						L"Please check event log for more details.");
				}
				else if (hr == CORPROF_E_CONCURRENT_GC_NOT_PROFILABLE)
				{
					OutputDebugStringW(L"Profiler initialization failed because the target process is running with concurrent GC enabled. Either\n"
						L"  1) turn off concurrent GC in the application's configuration file before launching the application, or\n"
						L"  2) simply start the application from CLRProfiler rather than trying to attach CLRProfiler after the application has already started.");
				}
				else
				{
					OutputDebugStringW(L"Attach Profiler Failed 0x%x, please check the event log for more details.");
				}

			}
			pProfiling->Release();
			pProfiling = NULL;
		}
	}

	while (pEnum->Next(1, &pUnk, NULL) == S_OK)
	{
		hr = pUnk->QueryInterface(IID_ICLRRuntimeInfo, (LPVOID *)&pRuntime);
		if (FAILED(hr))
		{
			pUnk->Release();
			pUnk = NULL;
			continue;
		}

		WCHAR wszVersion[30];
		DWORD cchVersion = sizeof(wszVersion) / sizeof(wszVersion[0]);
		hr = pRuntime->GetVersionString(wszVersion, &cchVersion);
		if (SUCCEEDED(hr) &&
			(cchVersion >= 3) &&
			((wszVersion[0] == L'v') || (wszVersion[0] == L'V')) &&
			((wszVersion[1] >= L'4') || (wszVersion[2] != L'.')))
		{

			hr = pRuntime->GetInterface(CLSID_CLRProfiling, IID_ICLRProfiling, (LPVOID *)&pProfiling);
			if (SUCCEEDED(hr))
			{
				hr = pProfiling->AttachProfiler(
					dwProfileeProcID,
					dwMillisecondsTimeout,
					&profilerCLSID,//"8C29BC4E-1F57-461a-9B51-1200C32E6F1F"
					wszProfilerPath,
					pvClientData,
					cbClientData
				);
				if (FAILED(hr))
				{
					if (hr == ERROR_TIMEOUT)//ERROR_TIMEOUT 
					{
						OutputDebugStringW(L"CLRProfiler timed out to attach to the process.\nPlease check the event log to find out whether the attach succeeded or failed.");
					}
					else if (hr == COR_E_UNAUTHORIZEDACCESS)//0x80070005
					{
						OutputDebugStringW(L"CLRProfiler failed to attach to the process with error code 0x80070005(COR_E_UNAUTHORIZEDACCESS).\n"
							L"This may occur if the target process(%d) does not have access to ProfilerOBJ.dll or the directory in which ProfilerOBJ.dll is located.\n"
							L"Please check event log for more details.");
					}
					else if (hr == CORPROF_E_CONCURRENT_GC_NOT_PROFILABLE)
					{
						OutputDebugStringW(L"Profiler initialization failed because the target process is running with concurrent GC enabled. Either\n"
							L"  1) turn off concurrent GC in the application's configuration file before launching the application, or\n"
							L"  2) simply start the application from CLRProfiler rather than trying to attach CLRProfiler after the application has already started.");
					}
					else
					{
						OutputDebugStringW(L"Attach Profiler Failed 0x%x, please check the event log for more details.");
					}

				}
				pProfiling->Release();
				pProfiling = NULL;
				break;
			}
		}

		pRuntime->Release();
		pRuntime = NULL;
		pUnk->Release();
		pUnk = NULL;
	}

Cleanup:
	if (pProfiling != NULL)
	{
		pProfiling->Release();
		pProfiling = NULL;
	}
	if (pRuntime != NULL)
	{
		pRuntime->Release();
		pRuntime = NULL;
	}

	if (pUnk != NULL)
	{
		pUnk->Release();
		pUnk = NULL;
	}

	if (pEnum != NULL)
	{
		pEnum->Release();
		pEnum = NULL;
	}

	if (pMetaHost != NULL)
	{
		pMetaHost->Release();
		pMetaHost = NULL;
	}

	return hr;
}

HRESULT CLRProfilerCallbackImpl::CreateObject(REFIID riid, void **ppInterface)
{
	HRESULT hr = E_NOINTERFACE;


	*ppInterface = NULL;
	if ((riid == IID_IUnknown)
		|| (riid == IID_ICorProfilerCallback)
		|| (riid == IID_ICorProfilerCallback2)
		|| (riid == IID_ICorProfilerCallback3))
	{
		CLRProfilerCallbackImpl *pProfilerCallback;

		pProfilerCallback = new CLRProfilerCallbackImpl();
		if (pProfilerCallback != NULL)
		{
			hr = S_OK;
			pProfilerCallback->AddRef();
			*ppInterface = static_cast<ICorProfilerCallback *>(pProfilerCallback);
		}
		else
			hr = E_OUTOFMEMORY;
	}

	return hr;
}

HRESULT CLRProfilerCallbackImpl::QueryInterface(REFIID riid, void **ppInterface)
{
	if (riid == IID_IUnknown)
		*ppInterface = static_cast<IUnknown *>(this);
	else if (riid == IID_ICorProfilerCallback)
		*ppInterface = static_cast<ICorProfilerCallback *>(this);
	else if (riid == IID_ICorProfilerCallback2)
		*ppInterface = static_cast<ICorProfilerCallback2 *>(this);
	else if (riid == IID_ICorProfilerCallback3)
		*ppInterface = static_cast<ICorProfilerCallback3 *>(this);
	else
	{
		*ppInterface = NULL;
		return E_NOINTERFACE;
	}

	reinterpret_cast<IUnknown *>(*ppInterface)->AddRef();

	return S_OK;

} // ProfilerCallback::QueryInterface 

ULONG CLRProfilerCallbackImpl::AddRef()
{
	return InterlockedIncrement(&m_refCount);
} // ProfilerCallback::AddRef

ULONG CLRProfilerCallbackImpl::Release()
{
	long refCount;


	refCount = InterlockedDecrement(&m_refCount);
	if (refCount == 0)
		delete this;


	return refCount;

} // ProfilerCallback::Release

CLRProfilerCallbackImpl::CLRProfilerCallbackImpl() 
{
	m_refCount = 0;
}

CLRProfilerCallbackImpl::~CLRProfilerCallbackImpl() 
{

}

STDMETHODIMP CLRProfilerCallbackImpl::Initialize(IUnknown* pICorProfilerInfoUnk) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::Shutdown()
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::AppDomainCreationStarted(AppDomainID appDomainID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::AppDomainCreationFinished(AppDomainID appDomainID, HRESULT hrStatus) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::AppDomainShutdownStarted(AppDomainID appDomainID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::AppDomainShutdownFinished(AppDomainID appDomainID, HRESULT hrStatus) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::AssemblyLoadStarted(AssemblyID assemblyID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::AssemblyLoadFinished(AssemblyID assemblyID, HRESULT hrStatus) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::AssemblyUnloadStarted(AssemblyID assemblyID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::AssemblyUnloadFinished(AssemblyID assemblyID, HRESULT hrStatus) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ModuleLoadStarted(ModuleID moduleID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ModuleLoadFinished(ModuleID moduleID, HRESULT hrStatus) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ModuleUnloadStarted(ModuleID moduleID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ModuleUnloadFinished(ModuleID moduleID, HRESULT hrStatus) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ModuleAttachedToAssembly(ModuleID moduleID, AssemblyID assemblyID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ClassLoadStarted(ClassID classID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ClassLoadFinished(ClassID classID, HRESULT hrStatus)
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ClassUnloadStarted(ClassID classID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ClassUnloadFinished(ClassID classID, HRESULT hrStatus) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::FunctionUnloadStarted(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::JITCompilationStarted(FunctionID functionID, BOOL fIsSafeToBlock) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::JITCompilationFinished(FunctionID functionID, HRESULT hrStatus, BOOL fIsSafeToBlock) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::JITCachedFunctionSearchStarted(FunctionID functionID, BOOL* pbUseCachedFunction) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::JITCachedFunctionSearchFinished(FunctionID functionID, COR_PRF_JIT_CACHE result) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::JITFunctionPitched(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::JITInlining(FunctionID callerID, FunctionID calleeID, BOOL* pfShouldInline) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ThreadCreated(ThreadID threadID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ThreadDestroyed(ThreadID threadID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ThreadAssignedToOSThread(ThreadID managedThreadID, DWORD osThreadID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RemotingClientInvocationStarted() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RemotingClientSendingMessage(GUID* pCookie, BOOL fIsAsync) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RemotingClientReceivingReply(GUID* pCookie, BOOL fIsAsync) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RemotingClientInvocationFinished() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RemotingServerReceivingMessage(GUID* pCookie, BOOL fIsAsync) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RemotingServerInvocationStarted() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RemotingServerInvocationReturned() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RemotingServerSendingReply(GUID* pCookie, BOOL fIsAsync) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::UnmanagedToManagedTransition(FunctionID functionID, COR_PRF_TRANSITION_REASON reason) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ManagedToUnmanagedTransition(FunctionID functionID, COR_PRF_TRANSITION_REASON reason) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RuntimeSuspendFinished()
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RuntimeSuspendAborted()
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RuntimeResumeStarted() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RuntimeResumeFinished() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RuntimeThreadSuspended(ThreadID threadid) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RuntimeThreadResumed(ThreadID threadid) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::MovedReferences(ULONG cmovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[]) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ObjectAllocated(ObjectID objectID, ClassID classID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ObjectsAllocatedByClass(ULONG classCount, ClassID classIDs[], ULONG objects[]) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ObjectReferences(ObjectID objectID, ClassID classID, ULONG cObjectRefs, ObjectID objectRefIDs[]) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RootReferences(ULONG cRootRefs, ObjectID rootRefIDs[])
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionThrown(ObjectID thrownObjectID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionSearchFunctionEnter(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionSearchFunctionLeave() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionSearchFilterEnter(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionSearchFilterLeave() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionSearchCatcherFound(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionCLRCatcherFound() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionCLRCatcherExecute() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionOSHandlerEnter(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionOSHandlerLeave(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionUnwindFunctionEnter(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionUnwindFunctionLeave() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionUnwindFinallyEnter(FunctionID functionID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionUnwindFinallyLeave() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionCatcherEnter(FunctionID functionID, ObjectID objectID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ExceptionCatcherLeave() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::COMClassicVTableCreated(ClassID wrappedClassID, REFGUID implementedIID, void* pVTable, ULONG cSlots) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::COMClassicVTableDestroyed(ClassID wrappedClassID, REFGUID implementedIID, void* pVTable) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::ThreadNameChanged(ThreadID threadId, ULONG cchName, WCHAR name[])
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::GarbageCollectionStarted(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], ULONG cObjectIDRangeLength[]) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::GarbageCollectionFinished() 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::RootReferences2(ULONG cRootRefs, ObjectID rootRefIds[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIds[]) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::HandleCreated(GCHandleID handleId, ObjectID initialObjectId) 
{
   return S_OK;
}

STDMETHODIMP CLRProfilerCallbackImpl::HandleDestroyed(GCHandleID handleId) 
{
   return S_OK;
}


STDMETHODIMP CLRProfilerCallbackImpl::InitializeForAttach(IUnknown* pCorProfilerInfoUnk, void* pvClientData, UINT cbClientData) 
{ 
   return S_OK; 
}


STDMETHODIMP CLRProfilerCallbackImpl::ProfilerAttachComplete() 
{ 
   return S_OK; 
}


STDMETHODIMP CLRProfilerCallbackImpl::ProfilerDetachSucceeded() 
{
   return S_OK; 
}

extern "C"
{
	__declspec(dllexport) int SDK_CLRProfiler_CallAttachProfiler(HANDLE hProcess, DWORD dwProfileeProcID, DWORD dwMillisecondsTimeout,
		GUID profilerCLSID, LPCWSTR wszProfilerPath)
	{
		return S_OK==gCLRProfiler.CallAttachProfiler(hProcess, dwProfileeProcID, dwMillisecondsTimeout, profilerCLSID, wszProfilerPath);
	}
}