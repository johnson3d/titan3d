#include "ClrProfiler.h"
#include <mscoree.h>  
#include <assert.h>
#include "ClrLogger.h"

#include "../../base/debug/vfxnew.h"

#define new VNEW

class __declspec(uuid("805A308B-061C-47F3-9B30-F785C3186E81")) CoreProfiler;

extern "C" HRESULT __stdcall DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv) 
{
	if (rclsid == __uuidof(CoreProfiler)) 
	{
		static CoreProfilerFactory factory;
		return factory.QueryInterface(riid, ppv);
	}
	return CLASS_E_CLASSNOTAVAILABLE;
}


HRESULT __stdcall CoreProfilerFactory::QueryInterface(REFIID riid, void** ppvObject) {
	if (ppvObject == nullptr)
		return E_POINTER;

	if (riid == __uuidof(IUnknown) || riid == __uuidof(IClassFactory)) {
		*ppvObject = static_cast<IClassFactory*>(this);
		return S_OK;
	}
	return E_NOINTERFACE;
}

ULONG __stdcall CoreProfilerFactory::AddRef(void) {
	return 2;
}

ULONG __stdcall CoreProfilerFactory::Release(void) {
	return 1;
}

HRESULT __stdcall CoreProfilerFactory::CreateInstance(IUnknown* pUnkOuter, REFIID riid, void** ppvObject) {
	auto profiler = new CoreProfiler();
	if (profiler == nullptr)
		return E_OUTOFMEMORY;

	auto hr = profiler->QueryInterface(riid, ppvObject);
	profiler->Release();

	return hr;
}

///==================
CoreProfiler::CoreProfiler()
{
	ClrLogger::StartClrLogger();
}

CoreProfiler::~CoreProfiler()
{
	ClrLogger::StopClrLogger();
}

HRESULT __stdcall CoreProfiler::QueryInterface(REFIID riid, void** ppvObject) {
	if (ppvObject == nullptr)
		return E_POINTER;

	if (riid == __uuidof(IUnknown) ||
		riid == __uuidof(ICorProfilerCallback) ||
		riid == __uuidof(ICorProfilerCallback2) ||
		riid == __uuidof(ICorProfilerCallback3) ||
		riid == __uuidof(ICorProfilerCallback4) ||
		riid == __uuidof(ICorProfilerCallback5) ||
		riid == __uuidof(ICorProfilerCallback6) ||
		riid == __uuidof(ICorProfilerCallback7) ||
		riid == __uuidof(ICorProfilerCallback8)) {
		AddRef();
		*ppvObject = static_cast<ICorProfilerCallback8*>(this);
		return S_OK;
	}

	return E_NOINTERFACE;
}

ULONG __stdcall CoreProfiler::AddRef(void) {
	return ++_refCount;
}

ULONG __stdcall CoreProfiler::Release(void) {
	auto count = --_refCount;
	if (count == 0)
		delete this;

	return count;
}

HRESULT CoreProfiler::Initialize(IUnknown* pICorProfilerInfoUnk) {
	
	pICorProfilerInfoUnk->QueryInterface(&_info);
	assert(_info);

	auto hr = _info->SetEventMask(
		//COR_PRF_MONITOR_ALL |
		COR_PRF_MONITOR_MODULE_LOADS |
		COR_PRF_MONITOR_ASSEMBLY_LOADS |
		COR_PRF_MONITOR_GC |
		COR_PRF_MONITOR_CLASS_LOADS |
		COR_PRF_MONITOR_THREADS |
		//COR_PRF_MONITOR_EXCEPTIONS |
		//COR_PRF_MONITOR_JIT_COMPILATION  |
		COR_PRF_MONITOR_OBJECT_ALLOCATED |
		COR_PRF_ENABLE_OBJECT_ALLOCATED 
	);

	return hr;
}

HRESULT CoreProfiler::Shutdown() {
	_info.Release();

	return S_OK;
}

HRESULT CoreProfiler::AppDomainCreationStarted(AppDomainID appDomainId) {
	return S_OK;
}

HRESULT CoreProfiler::AppDomainCreationFinished(AppDomainID appDomainId, HRESULT hrStatus) {
	return S_OK;
}

HRESULT CoreProfiler::AppDomainShutdownStarted(AppDomainID appDomainId) {
	return S_OK;
}

HRESULT CoreProfiler::AppDomainShutdownFinished(AppDomainID appDomainId, HRESULT hrStatus) {
	return S_OK;
}

HRESULT CoreProfiler::AssemblyLoadStarted(AssemblyID assemblyId) {
	return S_OK;
}

HRESULT CoreProfiler::AssemblyLoadFinished(AssemblyID assemblyId, HRESULT hrStatus) {
	WCHAR name[512];
	ULONG size;
	AppDomainID ad;
	ModuleID module;
	if (SUCCEEDED(_info->GetAssemblyInfo(assemblyId, sizeof(name) / sizeof(name[0]), &size, name, &ad, &module))) {
		
	}

	return S_OK;
}

HRESULT CoreProfiler::AssemblyUnloadStarted(AssemblyID assemblyId) {
	return S_OK;
}

HRESULT CoreProfiler::AssemblyUnloadFinished(AssemblyID assemblyId, HRESULT hrStatus) {
	return S_OK;
}

HRESULT CoreProfiler::ModuleLoadStarted(ModuleID moduleId) {
	return S_OK;
}

HRESULT CoreProfiler::ModuleLoadFinished(ModuleID moduleId, HRESULT hrStatus) {
	return S_OK;
}

HRESULT CoreProfiler::ModuleUnloadStarted(ModuleID moduleId) {
	return S_OK;
}

HRESULT CoreProfiler::ModuleUnloadFinished(ModuleID moduleId, HRESULT hrStatus) {
	return S_OK;
}

HRESULT CoreProfiler::ModuleAttachedToAssembly(ModuleID moduleId, AssemblyID AssemblyId) {
	return S_OK;
}

HRESULT CoreProfiler::ClassLoadStarted(ClassID classId) {
	return S_OK;
}

HRESULT CoreProfiler::ClassLoadFinished(ClassID classId, HRESULT hrStatus) {
	ModuleID module;
	mdTypeDef type;
	if (SUCCEEDED(_info->GetClassIDInfo(classId, &module, &type))) {
		auto name = GetTypeName(type, module);
	}

	return S_OK;
}

HRESULT CoreProfiler::ClassUnloadStarted(ClassID classId) {
	return S_OK;
}

HRESULT CoreProfiler::ClassUnloadFinished(ClassID classId, HRESULT hrStatus) {
	return S_OK;
}

HRESULT CoreProfiler::FunctionUnloadStarted(FunctionID functionId) {
	return S_OK;
}

HRESULT CoreProfiler::JITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock) {
	//printf("JIT compilation started: %s", GetMethodName(functionId));

	return S_OK;
}

HRESULT CoreProfiler::JITCompilationFinished(FunctionID functionId, HRESULT hrStatus, BOOL fIsSafeToBlock) {
	//printf("JIT compilation finished: %s", GetMethodName(functionId));

	return S_OK;
}

HRESULT CoreProfiler::JITCachedFunctionSearchStarted(FunctionID functionId, BOOL* pbUseCachedFunction) {
	return S_OK;
}

HRESULT CoreProfiler::JITCachedFunctionSearchFinished(FunctionID functionId, COR_PRF_JIT_CACHE result) {
	return S_OK;
}

HRESULT CoreProfiler::JITFunctionPitched(FunctionID functionId) {
	return S_OK;
}

HRESULT CoreProfiler::JITInlining(FunctionID callerId, FunctionID calleeId, BOOL* pfShouldInline) {
	return S_OK;
}

HRESULT CoreProfiler::ThreadCreated(ThreadID threadId) {
	return S_OK;
}

HRESULT CoreProfiler::ThreadDestroyed(ThreadID threadId) {
	return S_OK;
}

HRESULT CoreProfiler::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId) {
	//printf("Thread 0x%p assigned to OS thread %d", (void*)managedThreadId, osThreadId);
	return S_OK;
}

HRESULT CoreProfiler::RemotingClientInvocationStarted() {
	return S_OK;
}

HRESULT CoreProfiler::RemotingClientSendingMessage(GUID* pCookie, BOOL fIsAsync) {
	return S_OK;
}

HRESULT CoreProfiler::RemotingClientReceivingReply(GUID* pCookie, BOOL fIsAsync) {
	return S_OK;
}

HRESULT CoreProfiler::RemotingClientInvocationFinished() {
	return S_OK;
}

HRESULT CoreProfiler::RemotingServerReceivingMessage(GUID* pCookie, BOOL fIsAsync) {
	return S_OK;
}

HRESULT CoreProfiler::RemotingServerInvocationStarted() {
	return S_OK;
}

HRESULT CoreProfiler::RemotingServerInvocationReturned() {
	return S_OK;
}

HRESULT CoreProfiler::RemotingServerSendingReply(GUID* pCookie, BOOL fIsAsync) {
	return S_OK;
}

HRESULT CoreProfiler::UnmanagedToManagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason) {
	return S_OK;
}

HRESULT CoreProfiler::ManagedToUnmanagedTransition(FunctionID functionId, COR_PRF_TRANSITION_REASON reason) {
	return S_OK;
}

HRESULT CoreProfiler::RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason) {
	return S_OK;
}

HRESULT CoreProfiler::RuntimeSuspendFinished() {
	return S_OK;
}

HRESULT CoreProfiler::RuntimeSuspendAborted() {
	return S_OK;
}

HRESULT CoreProfiler::RuntimeResumeStarted() {
	return S_OK;
}

HRESULT CoreProfiler::RuntimeResumeFinished() {
	return S_OK;
}

HRESULT CoreProfiler::RuntimeThreadSuspended(ThreadID threadId) {
	return S_OK;
}

HRESULT CoreProfiler::RuntimeThreadResumed(ThreadID threadId) {
	return S_OK;
}

HRESULT CoreProfiler::MovedReferences(ULONG cMovedObjectIDRanges, ObjectID* oldObjectIDRangeStart, ObjectID* newObjectIDRangeStart, ULONG* cObjectIDRangeLength) {
	return S_OK;
}

HRESULT CoreProfiler::ObjectAllocated(ObjectID objectId, ClassID classId) {
	if (ClrLogger::bMessageBox)
	{
		::MessageBox(NULL, "ObjectAllocated", "block", MB_OK);
	}
	ModuleID module;
	mdTypeDef type;
	if (SUCCEEDED(_info->GetClassIDInfo(classId, &module, &type))) {
		auto name = GetTypeName(type, module);
		ClrString info(name);
		ClrLogger::PushLogInfo(EClrLogStringType::ObjectAlloc, info);
	}
	return S_OK;
}

HRESULT CoreProfiler::ObjectsAllocatedByClass(ULONG cClassCount, ClassID* classIds, ULONG* cObjects) 
{
	ClrString info("");
	for (ULONG i = 0; i < cClassCount; i++)
	{
		ModuleID module;
		mdTypeDef type;
		if (SUCCEEDED(_info->GetClassIDInfo(classIds[i], &module, &type)))
		{
			auto name = GetTypeName(type, module);
			info.Append(name);
			info.Append(",");
		}
	}
	ClrLogger::PushLogInfo(EClrLogStringType::ObjectsAllocdByClass, info);
	
	return S_OK;
}

HRESULT CoreProfiler::ObjectReferences(ObjectID objectId, ClassID classId, ULONG cObjectRefs, ObjectID* objectRefIds) {
	return S_OK;
}

HRESULT CoreProfiler::RootReferences(ULONG cRootRefs, ObjectID* rootRefIds) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionThrown(ObjectID thrownObjectId) {
	ClassID classid;
	_info->GetClassFromObject(thrownObjectId, &classid);
	ModuleID module;
	mdTypeDef type;
	_info->GetClassIDInfo(classid, &module, &type);
	/*ClrString info("Exception ");
	info.Append(GetTypeName(type, module));
	info.Append(" thrown");
	ClrLogger::PushLogInfo(info);*/

	//std::vector<std::string> data;
	//if (SUCCEEDED(_info->DoStackSnapshot(0, StackSnapshotCB, 0, &data, nullptr, 0))) {
	//	// TODO
	//}

	return S_OK;
}

HRESULT CoreProfiler::ExceptionSearchFunctionEnter(FunctionID functionId) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionSearchFunctionLeave() {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionSearchFilterEnter(FunctionID functionId) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionSearchFilterLeave() {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionSearchCatcherFound(FunctionID functionId) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionOSHandlerEnter(UINT_PTR __unused) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionOSHandlerLeave(UINT_PTR __unused) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionUnwindFunctionEnter(FunctionID functionId) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionUnwindFunctionLeave() {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionUnwindFinallyEnter(FunctionID functionId) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionUnwindFinallyLeave() {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionCatcherEnter(FunctionID functionId, ObjectID objectId) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionCatcherLeave() {
	return S_OK;
}

HRESULT CoreProfiler::COMClassicVTableCreated(ClassID wrappedClassId, const GUID& implementedIID, void* pVTable, ULONG cSlots) {
	return S_OK;
}

HRESULT CoreProfiler::COMClassicVTableDestroyed(ClassID wrappedClassId, const GUID& implementedIID, void* pVTable) {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionCLRCatcherFound() {
	return S_OK;
}

HRESULT CoreProfiler::ExceptionCLRCatcherExecute() {
	return S_OK;
}

HRESULT CoreProfiler::ThreadNameChanged(ThreadID threadId, ULONG cchName, WCHAR* name) {
	return S_OK;
}

HRESULT CoreProfiler::GarbageCollectionStarted(int cGenerations, BOOL* generationCollected, COR_PRF_GC_REASON reason) {
	/*printf("GC started. Gen0=%s, Gen1=%s, Gen2=%s",
		generationCollected[0] ? "Yes" : "No", generationCollected[1] ? "Yes" : "No", generationCollected[2] ? "Yes" : "No");*/
	ClrString info("GC started.");
	if (generationCollected[0])
	{
		info.Append("Gen0 = Yes,");
	}
	if (generationCollected[1])
	{
		info.Append("Gen1 = Yes,");
	}
	if (generationCollected[2])
	{
		info.Append("Gen2 = Yes");
	}
	

	ClrLogger::PushLogInfo(EClrLogStringType::GCStart, info);
	return S_OK;
}

HRESULT CoreProfiler::SurvivingReferences(ULONG cSurvivingObjectIDRanges, ObjectID* objectIDRangeStart, ULONG* cObjectIDRangeLength) {
	return S_OK;
}

HRESULT CoreProfiler::GarbageCollectionFinished() 
{
	ClrLogger::PushLogInfo(EClrLogStringType::GCFinish, "GC finished");

	return S_OK;
}

HRESULT CoreProfiler::FinalizeableObjectQueued(DWORD finalizerFlags, ObjectID objectID) {
	return S_OK;
}

HRESULT CoreProfiler::RootReferences2(ULONG cRootRefs, ObjectID* rootRefIds, COR_PRF_GC_ROOT_KIND* rootKinds, COR_PRF_GC_ROOT_FLAGS* rootFlags, UINT_PTR* rootIds) {
	return S_OK;
}

HRESULT CoreProfiler::HandleCreated(GCHandleID handleId, ObjectID initialObjectId) {
	return S_OK;
}

HRESULT CoreProfiler::HandleDestroyed(GCHandleID handleId) {
	return S_OK;
}

HRESULT CoreProfiler::InitializeForAttach(IUnknown* pCorProfilerInfoUnk, void* pvClientData, UINT cbClientData) {
	return S_OK;
}

HRESULT CoreProfiler::ProfilerAttachComplete() {
	return S_OK;
}

HRESULT CoreProfiler::ProfilerDetachSucceeded() {
	return S_OK;
}

HRESULT CoreProfiler::ReJITCompilationStarted(FunctionID functionId, ReJITID rejitId, BOOL fIsSafeToBlock) {
	return S_OK;
}

HRESULT CoreProfiler::GetReJITParameters(ModuleID moduleId, mdMethodDef methodId, ICorProfilerFunctionControl* pFunctionControl) {
	return S_OK;
}

HRESULT CoreProfiler::ReJITCompilationFinished(FunctionID functionId, ReJITID rejitId, HRESULT hrStatus, BOOL fIsSafeToBlock) {
	return S_OK;
}

HRESULT CoreProfiler::ReJITError(ModuleID moduleId, mdMethodDef methodId, FunctionID functionId, HRESULT hrStatus) {
	return S_OK;
}

HRESULT CoreProfiler::MovedReferences2(ULONG cMovedObjectIDRanges, ObjectID* oldObjectIDRangeStart, ObjectID* newObjectIDRangeStart, SIZE_T* cObjectIDRangeLength) {
	return S_OK;
}

HRESULT CoreProfiler::SurvivingReferences2(ULONG cSurvivingObjectIDRanges, ObjectID* objectIDRangeStart, SIZE_T* cObjectIDRangeLength) {
	return S_OK;
}

HRESULT CoreProfiler::ConditionalWeakTableElementReferences(ULONG cRootRefs, ObjectID* keyRefIds, ObjectID* valueRefIds, GCHandleID* rootIds) {
	return S_OK;
}

HRESULT CoreProfiler::GetAssemblyReferences(const WCHAR* wszAssemblyPath, ICorProfilerAssemblyReferenceProvider* pAsmRefProvider) {
	return S_OK;
}

HRESULT CoreProfiler::ModuleInMemorySymbolsUpdated(ModuleID moduleId) {
	return S_OK;
}

HRESULT CoreProfiler::DynamicMethodJITCompilationStarted(FunctionID functionId, BOOL fIsSafeToBlock, LPCBYTE pILHeader, ULONG cbILHeader) {
	return S_OK;
}

HRESULT CoreProfiler::DynamicMethodJITCompilationFinished(FunctionID functionId, HRESULT hrStatus, BOOL fIsSafeToBlock) {
	return S_OK;
}

const char* CoreProfiler::GetTypeName(mdTypeDef type, ModuleID module) const 
{
	CComPtr<IMetaDataImport> spMetadata;
	if (SUCCEEDED(_info->GetModuleMetaData(module, ofRead, IID_IMetaDataImport, reinterpret_cast<IUnknown**>(&spMetadata)))) 
	{
		WCHAR name[256];
		ULONG nameSize = 256;
		DWORD flags;
		mdTypeDef baseType;
		if (SUCCEEDED(spMetadata->GetTypeDefProps(type, name, 256, &nameSize, &flags, &baseType)))
		{
			static thread_local char ascii_name[256];
			for (int i = 0; i < (int)nameSize; i++)
			{
				ascii_name[i] = (char)name[i];
			}
			ascii_name[nameSize] = '\0';
			return ascii_name;
		}
	}
	return "";
}

const char* CoreProfiler::GetMethodName(FunctionID function) const 
{
	ModuleID module;
	mdToken token;
	mdTypeDef type;
	ClassID classId;
	if (FAILED(_info->GetFunctionInfo(function, &classId, &module, &token)))
		return "";

	CComPtr<IMetaDataImport> spMetadata;
	if (FAILED(_info->GetModuleMetaData(module, ofRead, IID_IMetaDataImport, reinterpret_cast<IUnknown**>(&spMetadata))))
		return "";
	PCCOR_SIGNATURE sig;
	ULONG blobSize, size, attributes;
	WCHAR name[256];
	DWORD flags;
	ULONG codeRva;
	if (FAILED(spMetadata->GetMethodProps(token, &type, name, 256, &size, &attributes, &sig, &blobSize, &codeRva, &flags)))
		return "";

	return GetTypeName(type, module);// + "::" + OS::UnicodeToAnsi(name);
}