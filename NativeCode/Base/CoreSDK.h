#pragma once
#include "BaseHead.h"
#include "CoreRtti.h"
#include "TypeUtility.h"
#include "debug/vfxdebug.h"

NS_BEGIN

template<class TYPE>
inline void Safe_Delete(TYPE * & p) {
	if (p == NULL)
	{
		return;
	}
	TYPE* refPtr = p;
	p = NULL;
	delete refPtr;
	refPtr = NULL;
}

template<class TYPE>
inline void Safe_DeleteArray(TYPE * & p) {
	if (p == NULL)
	{
		return;
	}
	TYPE* refPtr = p;
	p = NULL;
	delete[] refPtr;
	refPtr = NULL;
}

template<typename T>
void Safe_Release(T*& p)
{
	if (p == NULL)
		return;
	p->Release();
	p = nullptr;
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
class IShaderDesc;
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (*FOnShaderTranslated)(IShaderDesc* shaderDesc);

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS, SV_LayoutStruct=8)
CoreSDK
{
public:
	static FWriteLogString mWriteLogString;
public:
	static void UpdateEngineTick(INT64 tick);
	static void IUnknown_Release(void* unk);
	static int IUnknown_AddRef(void* unk);
	static void SetWriteLogStringCallback(FWriteLogString wls);
	static void StartNativeMemWatcher();
	static void DumpNativeMemoryState(const char* name, vBOOL dumpUnknown);
	static void* MemoryCopy(void* tar, void* src, UINT size);
	static int MemoryCmp(void* tar, void* src, UINT size);
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

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = EngineNS)
BigStackBuffer
{
	BYTE*		mBuffer;
	int			mSize;
public:
	TR_CONSTRUCTOR()
	BigStackBuffer(int size)
	{
		mBuffer = new BYTE[size];
		memset(mBuffer, 0, size);
		mSize = size;
	}
	TR_CONSTRUCTOR()
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
	TR_FUNCTION()
	void* GetBuffer() {
		return mBuffer;
	}
	TR_FUNCTION()
	int GetSize() const{
		return mSize;
	}
	TR_FUNCTION()
	void DestroyMe()
	{
		delete this;
	}
};

VTypeHelperDefine(FWriteLogString, sizeof(void*));
VTypeHelperDefine(FOnNativeMemAlloc, sizeof(void*));
//VTypeHelperDefine(FOnNativeMemFree, sizeof(void*));
VTypeHelperDefine(FOnNativeMemLeak, sizeof(void*));
VTypeHelperDefine(FCreateManagedObject, sizeof(void*));
VTypeHelperDefine(FFreeManagedObjectGCHandle, sizeof(void*));
VTypeHelperDefine(FGetManagedObjectFromGCHandle, sizeof(void*));
VTypeHelperDefine(FOnShaderTranslated, sizeof(void*));

StructBegin(FWriteLogString, EngineNS)
StructEnd(void)

StructBegin(FOnNativeMemAlloc, EngineNS)
StructEnd(void)

//StructBegin(FOnNativeMemFree, EngineNS)
//StructEnd(void)

StructBegin(FOnNativeMemLeak, EngineNS)
StructEnd(void)

StructBegin(FCreateManagedObject, EngineNS)
StructEnd(void)

StructBegin(FFreeManagedObjectGCHandle, EngineNS)
StructEnd(void)

StructBegin(FGetManagedObjectFromGCHandle, EngineNS)
StructEnd(void)

StructBegin(FOnShaderTranslated, EngineNS)
StructEnd(void)

NS_END

