#pragma once
#include "BaseHead.h"
#include "CoreRtti.h"
#include "TypeUtility.h"
#include "debug/vfxdebug.h"

NS_BEGIN

namespace NxRHI
{
	struct FShaderDesc;
}
struct UAnyValue;

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*FWriteLogString)(const void* threadName, const void* logStr, ELevelTraceType level, const void* file, int line);

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(* FOnNativeMemAlloc)(size_t size, const char* file, size_t line, size_t id);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(* FOnNativeMemFree)(size_t size, const char* file, size_t line, size_t id);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(* FOnNativeMemLeak)(void* ptr, size_t size, const char* file, size_t line, size_t id, const char* debugInfo);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void* (*FCreateManagedObject)(const char* fullname, UAnyValue* args, int NumOfArg, int refType);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (*FFreeManagedObjectGCHandle)(void* handle);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void* (*FGetManagedObjectFromGCHandle)(void* handle);

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (*FOnShaderTranslated)(EngineNS::NxRHI::FShaderDesc* shaderDesc);

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (* FAssertEvent)(const void* str, const void* file, int line);

class TR_CLASS(SV_LayoutStruct = 8)
	CoreSDK
{
public:
	static FWriteLogString mWriteLogString;
	static FAssertEvent mAssertEvent;
public:
	static int GetPixelFormatByteWidth(EPixelFormat fmt);
	static void Print2Console(TR_META(SV_NoStringConverter = true) char* txt, bool newLine);
	static void Print2Console2(const char* txt, bool newLine);
	static void SetAssertEvent(FAssertEvent fn);

	static void InitF2MManager();
	static void FinalF2MManager();
	static void UpdateEngineFrame(UINT64 frame);
	static void IUnknown_Release(void* unk);
	static int IUnknown_AddRef(void* unk);
	static void SetWriteLogStringCallback(FWriteLogString wls);
	static void StartNativeMemWatcher();
	static void DumpNativeMemoryState(const char* name, vBOOL dumpUnknown);
	static void* MemoryCopy(void* tar, void* src, UINT size);
	static int MemoryCmp(void* tar, void* src, UINT size);
	static void* MemorySet(void* tar, int val, UINT size);
	static void SDK_StrCpy(void* tar, const void* src, UINT tarSize);
	static UINT SDK_StrLen(const void* s);
	static int SDK_StrCmp(const void* s1, const void* s2);

	static void* Alloc(unsigned int size, const char* file, int line);
	static void Free(void* ptr);

	static size_t NativeMemoryUsed();
	static size_t NativeMemoryMax();
	static size_t NativeMemoryAllocTimes();
	static void SetMemAllocCallBack(FOnNativeMemAlloc cb);
	static void SetMemFreeCallBack(FOnNativeMemFree cb);
	static void SetMemLeakCallBack(FOnNativeMemLeak cb);
	static void SetMemDebugInfo(void* memory, const char* info);

	static FCreateManagedObject CreateManagedObject;
	static void SetCreateManagedObjectFunction(FCreateManagedObject fn) {
		CreateManagedObject = fn;
	}
	static FFreeManagedObjectGCHandle FreeManagedObjectGCHandle;
	static void SetFreeManagedObjectGCHandle(FFreeManagedObjectGCHandle fn) {
		FreeManagedObjectGCHandle = fn;
	}
	static FGetManagedObjectFromGCHandle GetManagedObjectFromGCHandle;
	static void SetGetManagedObjectFromGCHandle(FGetManagedObjectFromGCHandle fn) {
		GetManagedObjectFromGCHandle = fn;
	}

	static FOnShaderTranslated OnShaderTranslated;
	static void SetOnShaderTranslated(FOnShaderTranslated fn)
	{
		OnShaderTranslated = fn;
	}
	static void TestStdString(const std::string& a)
	{

	}
	static const char* TestGetStdString()
	{
		return "hoho";
	}
};

class TR_CLASS(SV_Dispose = delete self)
	BigStackBuffer
{
	BYTE*		mBuffer;
	int			mSize;
public:
	BigStackBuffer(int size)
	{
		mBuffer = new BYTE[size];
		memset(mBuffer, 0, size);
		mSize = size;
	}
	BigStackBuffer(int size, const char* text)
	{
		if ((int)strlen(text) > size)
			size = (int)strlen(text) * 2;
		mBuffer = new BYTE[size];
		memset(mBuffer, 0, size);
		mSize = size;

		strcpy((char*)mBuffer, text);
	}
	~BigStackBuffer()
	{
		delete[] mBuffer;
		mBuffer = nullptr;
	}
	void* GetBuffer() {
		return mBuffer;
	}
	int GetSize() const{
		return mSize;
	}
};

NS_END

