#pragma once

#include "cor.h"
#include "corprof.h"
#include "metahost.h"
#include "atlsafe.h"
#include "atlcom.h"

#define THREADING_MODEL "Both"
#define PROGID_PREFIX "Objects Profiler"
#define COCLASS_DESCRIPTION "Titan3d Profiler"
#define PROFILER_GUID "{A347C588-0436-471C-991A-31C70507AA09}"
#define PROFILER_GUID_WCHAR L"{A347C588-0436-471C-991A-31C70507AA09}"


// {8C29BC4E-1F57-461a-9B51-1200C32E6F1F}

//[
//	coclass,
//	threading(both),
//	vi_progid("SimpleProfiler"),
//	progid("SimpleProfiler.1"),
//	version(1.0),
//	uuid("{68130646-C19E-459E-8BF4-5137ED7B3FD7}"),
//	helpstring("Simple CLR Profiler class")
//]
//__declspec(uuid("68130646-C19E-459E-8BF4-5137ED7B3FD7"))
class CLRProfilerCallbackImpl : public ICorProfilerCallback3
{
	long m_refCount;
public:
	CLRProfilerCallbackImpl();
   ~CLRProfilerCallbackImpl();

   static HRESULT __stdcall CreateObject(REFIID riid, void **ppInterface);

   virtual HRESULT __stdcall QueryInterface(REFIID riid, void **ppInterface) override;
   virtual ULONG __stdcall AddRef() override;
   virtual ULONG __stdcall Release() override;

   // ICorProfilerCallback interface implementation
   // STARTUP/SHUTDOWN EVENTS
   STDMETHOD(Initialize)(IUnknown *pICorProfilerInfoUnk);
   STDMETHOD(Shutdown)();
   // APPLICATION DOMAIN EVENTS
   STDMETHOD(AppDomainCreationStarted)(AppDomainID appDomainID);
   STDMETHOD(AppDomainCreationFinished)(AppDomainID appDomainID, HRESULT hrStatus);
   STDMETHOD(AppDomainShutdownStarted)(AppDomainID appDomainID);
   STDMETHOD(AppDomainShutdownFinished)(AppDomainID appDomainID, HRESULT hrStatus);
   // ASSEMBLY EVENTS
   STDMETHOD(AssemblyLoadStarted)(AssemblyID assemblyID);
   STDMETHOD(AssemblyLoadFinished)(AssemblyID assemblyID, HRESULT hrStatus);
   STDMETHOD(AssemblyUnloadStarted)(AssemblyID assemblyID);
   STDMETHOD(AssemblyUnloadFinished)(AssemblyID assemblyID, HRESULT hrStatus);
   // MODULE EVENTS
   STDMETHOD(ModuleLoadStarted)(ModuleID moduleID);
   STDMETHOD(ModuleLoadFinished)(ModuleID moduleID, HRESULT hrStatus);
   STDMETHOD(ModuleUnloadStarted)(ModuleID moduleID);
   STDMETHOD(ModuleUnloadFinished)(ModuleID moduleID, HRESULT hrStatus);
   STDMETHOD(ModuleAttachedToAssembly)(ModuleID moduleID, AssemblyID assemblyID);
   // CLASS EVENTS
   STDMETHOD(ClassLoadStarted)(ClassID classID);
   STDMETHOD(ClassLoadFinished)(ClassID classID, HRESULT hrStatus);
   STDMETHOD(ClassUnloadStarted)(ClassID classID);
   STDMETHOD(ClassUnloadFinished)(ClassID classID, HRESULT hrStatus);
   STDMETHOD(FunctionUnloadStarted)(FunctionID functionID);
   // JIT EVENTS
   STDMETHOD(JITCompilationStarted)(FunctionID functionID, BOOL fIsSafeToBlock);
   STDMETHOD(JITCompilationFinished)(FunctionID functionID, HRESULT hrStatus, BOOL fIsSafeToBlock);
   STDMETHOD(JITCachedFunctionSearchStarted)(FunctionID functionID, BOOL *pbUseCachedFunction);
   STDMETHOD(JITCachedFunctionSearchFinished)(FunctionID functionID, COR_PRF_JIT_CACHE result);
   STDMETHOD(JITFunctionPitched)(FunctionID functionID);
   STDMETHOD(JITInlining)(FunctionID callerID, FunctionID calleeID, BOOL *pfShouldInline);
   // THREAD EVENTS
   STDMETHOD(ThreadCreated)(ThreadID threadID);
   STDMETHOD(ThreadDestroyed)(ThreadID threadID);
   STDMETHOD(ThreadAssignedToOSThread)(ThreadID managedThreadID, DWORD osThreadID);
   // REMOTING EVENTS
   // Client-side events
   STDMETHOD(RemotingClientInvocationStarted)();
   STDMETHOD(RemotingClientSendingMessage)(GUID *pCookie, BOOL fIsAsync);
   STDMETHOD(RemotingClientReceivingReply)(GUID *pCookie, BOOL fIsAsync);
   STDMETHOD(RemotingClientInvocationFinished)();
   // Server-side events
   STDMETHOD(RemotingServerReceivingMessage)(GUID *pCookie, BOOL fIsAsync);
   STDMETHOD(RemotingServerInvocationStarted)();
   STDMETHOD(RemotingServerInvocationReturned)();
   STDMETHOD(RemotingServerSendingReply)(GUID *pCookie, BOOL fIsAsync);
   // CONTEXT EVENTS
   STDMETHOD(UnmanagedToManagedTransition)(FunctionID functionID, COR_PRF_TRANSITION_REASON reason);
   STDMETHOD(ManagedToUnmanagedTransition)(FunctionID functionID, COR_PRF_TRANSITION_REASON reason);
   // SUSPENSION EVENTS
   STDMETHOD(RuntimeSuspendStarted)(COR_PRF_SUSPEND_REASON suspendReason);
   STDMETHOD(RuntimeSuspendFinished)();
   STDMETHOD(RuntimeSuspendAborted)();
   STDMETHOD(RuntimeResumeStarted)();
   STDMETHOD(RuntimeResumeFinished)();
   STDMETHOD(RuntimeThreadSuspended)(ThreadID threadid);
   STDMETHOD(RuntimeThreadResumed)(ThreadID threadid);
   // GC EVENTS
   STDMETHOD(MovedReferences)(ULONG cmovedObjectIDRanges, ObjectID oldObjectIDRangeStart[], ObjectID newObjectIDRangeStart[], ULONG cObjectIDRangeLength[]);
   STDMETHOD(ObjectAllocated)(ObjectID objectID, ClassID classID);
   STDMETHOD(ObjectsAllocatedByClass)(ULONG classCount, ClassID classIDs[], ULONG objects[]);
   STDMETHOD(ObjectReferences)(ObjectID objectID, ClassID classID, ULONG cObjectRefs, ObjectID objectRefIDs[]);
   STDMETHOD(RootReferences)(ULONG cRootRefs, ObjectID rootRefIDs[]);
   // EXCEPTION EVENTS
   // Exception creation
   STDMETHOD(ExceptionThrown)(ObjectID thrownObjectID);
   // Search phase
   STDMETHOD(ExceptionSearchFunctionEnter)(FunctionID functionID);
   STDMETHOD(ExceptionSearchFunctionLeave)();
   STDMETHOD(ExceptionSearchFilterEnter)(FunctionID functionID);
   STDMETHOD(ExceptionSearchFilterLeave)();
   STDMETHOD(ExceptionSearchCatcherFound)(FunctionID functionID);
   STDMETHOD(ExceptionCLRCatcherFound)();
   STDMETHOD(ExceptionCLRCatcherExecute)();
   STDMETHOD(ExceptionOSHandlerEnter)(FunctionID functionID);
   STDMETHOD(ExceptionOSHandlerLeave)(FunctionID functionID);
   // Unwind phase
   STDMETHOD(ExceptionUnwindFunctionEnter)(FunctionID functionID);
   STDMETHOD(ExceptionUnwindFunctionLeave)();
   STDMETHOD(ExceptionUnwindFinallyEnter)(FunctionID functionID);
   STDMETHOD(ExceptionUnwindFinallyLeave)();
   STDMETHOD(ExceptionCatcherEnter)(FunctionID functionID, ObjectID objectID);
   STDMETHOD(ExceptionCatcherLeave)();
   // COM CLASSIC VTable
   STDMETHOD(COMClassicVTableCreated)(ClassID wrappedClassID, REFGUID implementedIID, void *pVTable, ULONG cSlots);
   STDMETHOD(COMClassicVTableDestroyed)(ClassID wrappedClassID, REFGUID implementedIID, void *pVTable);
   // End of ICorProfilerCallback interface implementation

   // ICorProfilerCallback2 interface implementation
   STDMETHOD(ThreadNameChanged)(ThreadID threadId, ULONG cchName, WCHAR name[]);
   STDMETHOD(GarbageCollectionStarted)(int cGenerations, BOOL generationCollected[], COR_PRF_GC_REASON reason);
   STDMETHOD(SurvivingReferences)(ULONG cSurvivingObjectIDRanges, ObjectID objectIDRangeStart[], ULONG cObjectIDRangeLength[]);
   STDMETHOD(GarbageCollectionFinished)();
   STDMETHOD(FinalizeableObjectQueued)(DWORD finalizerFlags, ObjectID objectID);
   STDMETHOD(RootReferences2)(ULONG cRootRefs, ObjectID rootRefIds[], COR_PRF_GC_ROOT_KIND rootKinds[], COR_PRF_GC_ROOT_FLAGS rootFlags[], UINT_PTR rootIds[]);
   STDMETHOD(HandleCreated)(GCHandleID handleId, ObjectID initialObjectId);
   STDMETHOD(HandleDestroyed)(GCHandleID handleId);
   // End of ICorProfilerCallback2 interface implementation

   // ICorProfilerCallback3 interface implementation
   STDMETHOD (InitializeForAttach)(IUnknown *pCorProfilerInfoUnk, void *pvClientData, UINT cbClientData);
   STDMETHOD(ProfilerAttachComplete)();
   STDMETHOD(ProfilerDetachSucceeded)();
   // End of ICorProfilerCallback3 interface implementation
};

class CLRProfiler
{
public:
	HRESULT CallAttachProfiler(HANDLE hProcess, DWORD dwProfileeProcID, DWORD dwMillisecondsTimeout,
		GUID profilerCLSID, LPCWSTR wszProfilerPath);
};
