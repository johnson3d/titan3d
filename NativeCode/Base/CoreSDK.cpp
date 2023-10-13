#include "CoreSDK.h"
#include "IUnknown.h"
#include "debug/vfxmemory.h"
#include "r2m/F2MManager.h"
#include "../../3rd/native/zstd/lib/zstd.h"


#define new VNEW

extern "C" void vfxMemory_StartWatcher();
extern "C" size_t SDK_vfxMemory_MemoryUsed();
extern "C" size_t SDK_vfxMemory_MemoryMax();
extern "C" size_t SDK_vfxMemory_MemoryAllocTimes();
extern "C" void vfxMemory_DumpMemoryState(LPCSTR name, vBOOL dumpUnknown);
extern "C" void SDK_vfxMemory_SetMemAllocCallBack(VFX_Memory::FOnMemAlloc cb);
extern "C" void SDK_vfxMemory_SetMemFreeCallBack(VFX_Memory::FOnMemFree cb);
extern "C" void SDK_vfxMemory_SetMemLeakCallBack(VFX_Memory::FOnMemLeak cb);

extern "C" VFX_API void* SDK_Core_GetObjectPointer(void* obj)
{
	return obj;
}

extern "C" VFX_API void* SDK_Core_GetObjectFromPointer(void* ptr)
{
	return ptr;
}

//extern "C" VFX_API void TestObjectParameter(void* o, int a)
//{
//	if (o == nullptr)
//	{
//		return;
//	}
//}

//struct UCs2CppBase
//{
//	void* mCSharpHandle;
//	//VNameString mCSFullName;
//};
//
//namespace EngineNS::UTest
//{
//	struct UTestCs2CppBuilder : public UCs2CppBase
//	{
//		struct FCsMethods
//		{
//			FCsMethods()
//			{
//				memset(this, 0 ,sizeof(FCsMethods));
//			}
//			typedef int (*Func0)(void* self, float a, int b);
//			Func0 fn_Func0;
//		};
//		static FCsMethods CsMethods;
//		int Func0(float a, int b)
//		{
//			return CsMethods.fn_Func0(mCSharpHandle, a, b);
//		}
//	};
//}
//
//extern "C" VFX_API void UTestCs2CppBuilder_FCsMethods_SetFunc0(EngineNS::UTest::UTestCs2CppBuilder::FCsMethods::Func0 fn)
//{
//	EngineNS::UTest::UTestCs2CppBuilder::CsMethods.fn_Func0 = fn;
//}

NS_BEGIN

FAssertEvent CoreSDK::mAssertEvent = nullptr;
FWriteLogString CoreSDK::mWriteLogString = nullptr;
FCreateManagedObject CoreSDK::CreateManagedObject = nullptr;
FFreeManagedObjectGCHandle CoreSDK::FreeManagedObjectGCHandle = nullptr;
FGetManagedObjectFromGCHandle CoreSDK::GetManagedObjectFromGCHandle = nullptr;
FOnShaderTranslated CoreSDK::OnShaderTranslated = nullptr;
FOnGpuDeviceRemoved CoreSDK::OnGpuDeviceRemoved = nullptr;

int CoreSDK::GetPixelFormatByteWidth(EPixelFormat fmt)
{
	return GetPixelByteWidth(fmt);
}

void CoreSDK::SetAssertEvent(FAssertEvent fn)
{
	mAssertEvent = fn;
}

void CoreSDK::InitF2MManager()
{
	F2MManager::Instance = new F2MManager();
}

void CoreSDK::FinalF2MManager()
{
	Safe_Delete(F2MManager::Instance);
}

void CoreSDK::StartNativeMemWatcher()
{
	vfxMemory_StartWatcher();	
}

size_t CoreSDK::NativeMemoryUsed()
{
	return SDK_vfxMemory_MemoryUsed();
}
size_t CoreSDK::NativeMemoryMax()
{
	return SDK_vfxMemory_MemoryMax();
}
size_t CoreSDK::NativeMemoryAllocTimes()
{
	return SDK_vfxMemory_MemoryAllocTimes();
}

void CoreSDK::SetMemAllocCallBack(FOnNativeMemAlloc cb)
{
	SDK_vfxMemory_SetMemAllocCallBack(cb);
}

void CoreSDK::SetMemFreeCallBack(FOnNativeMemFree cb)
{
	SDK_vfxMemory_SetMemFreeCallBack(cb);
}

void CoreSDK::SetMemLeakCallBack(FOnNativeMemLeak cb)
{
	SDK_vfxMemory_SetMemLeakCallBack(cb);
}

extern "C" VFX_API void vfxMemory_SetDebugInfo(void* memory, LPCSTR info);
void CoreSDK::SetMemDebugInfo(void* memory, const char* info)
{
	vfxMemory_SetDebugInfo(memory, info);
}

void CoreSDK::UpdateEngineFrame(UINT64 frame)
{
	IWeakReference::EngineCurrentFrame = frame;
}

void CoreSDK::IUnknown_Release(void* unk)
{
	auto p = (IWeakReference*)unk;
	Safe_Release(p);
}

int CoreSDK::IUnknown_AddRef(void* unk)
{
	auto p = (IWeakReference*)unk;
	return p->AddRef();
}
int CoreSDK::IUnknown_UnsafeGetRefCount(void* unk)
{
	auto p = (IWeakReference*)unk;
	return p->UnsafeGetRefCount();
}

void CoreSDK::SetWriteLogStringCallback(FWriteLogString wls)
{
	mWriteLogString = wls;
}

void CoreSDK::DumpNativeMemoryState(const char* name, vBOOL dumpUnknown)
{
	vfxMemory_DumpMemoryState(name, dumpUnknown);
}

void CoreSDK::SDK_StrCpy(void* tar, const void* src, UINT tarSize)
{
	if (src == nullptr)
	{
		((char*)tar)[0] = 0;
		return;
	}
	if (strlen((const char*)src) >= tarSize)
	{
		memcpy((char*)tar, (const char*)src, (size_t)(tarSize - 1));
		((char*)tar)[tarSize - 1] = '\0';
	}
	else
	{
		strcpy((char*)tar, (const char*)src);
	}
}

UINT CoreSDK::SDK_StrLen(const void* s)
{
	if (s == nullptr)
		return 0;
	return (UINT)strlen((const char*)s);
}

int CoreSDK::SDK_StrCmp(const void* s1, const void* s2)
{
	if (s1 == 0 && s2 == 0)
		return 0;
	else if (s1 != 0 && s2 == 0)
		return 1;
	else if (s1 == 0 && s2 != 0)
		return -11;
	else
		return strcmp((const char*)s1, (const char*)s2);
}

void* CoreSDK::MemoryCopy(void* tar, void* src, UINT size)
{
	return memcpy(tar, src, size);
}
int CoreSDK::MemoryCmp(void* tar, void* src, UINT size)
{
	return memcmp(tar, src, size);
}
void* CoreSDK::MemorySet(void* tar, int val, UINT size)
{
	return memset(tar, val, size);
}

void* CoreSDK::Alloc(unsigned int size, const char* file, int line)
{
#undef new 
	return new(file, line) unsigned char[size];
#define new VNEW
}

void CoreSDK::Free(void* ptr)
{
	delete[] (unsigned char*)ptr;
}
void CoreSDK::MessageDialog(char* txt)
{
#if PLATFORM_WIN
	::MessageBoxA(nullptr, txt, "CoreSDK MessageBox", MB_OK);
#endif
}
void CoreSDK::Print2Console(TR_META(SV_NoStringConverter = true) char* txt, bool newLine) {
	printf("%s", txt);
	if (newLine)
		printf("\n");
}
void CoreSDK::Print2Console2(const char* txt, bool newLine) {
	printf("%s", txt);
	if (newLine)
		printf("\n");
}

UINT64 CoreSDK::Compress_ZSTD(void* dst, UINT64 dstCapacity,
	const void* src, UINT64 srcSize,
	int compressionLevel)
{
	return ZSTD_compress(dst, (size_t)dstCapacity, src, (size_t)srcSize, compressionLevel);
}

UINT64 CoreSDK::Decompress_ZSTD(void* dst, UINT64 dstCapacity,
	const void* src, UINT64 compressedSize)
{
	return ZSTD_decompress(dst, (size_t)dstCapacity, src, (size_t)compressedSize);
}

UINT64 CoreSDK::GetFrameContentSize_ZSTD(const void* src, UINT64 srcSize)
{
	return ZSTD_getFrameContentSize(src, (size_t)srcSize);
}

UINT64 CoreSDK::CompressBound_ZSTD(UINT64 srcSize)
{
	return ZSTD_compressBound((size_t)srcSize);
}

struct FMemTypeIter
{
	std::map<std::string, AutoRef<FNativeMemType>>::iterator mIterator;
};

FNativeMemType* FNativeMemCapture::GetOrNewMemType(int size, const char* name, int line)
{
	std::string key = VStringA_FormatV("[%d]%s(%d)", size, name, line);
	auto iter = mMemTypes.find(key);
	if (iter != mMemTypes.end())
		return iter->second;
	auto tmp = MakeWeakRef(new FNativeMemType());
	tmp->File = name;
	tmp->Line = line;
	tmp->Size = size;
	mMemTypes.insert(std::make_pair(key, tmp));
	return tmp;
}
FNativeMemType* FNativeMemCapture::FindMemType(int size, const char* name, int line)
{
	std::string key = VStringA_FormatV("[%d]%s(%d)", size, name, line);
	auto iter = mMemTypes.find(key);
	if (iter != mMemTypes.end())
		return iter->second;
	return nullptr;
}

void* FNativeMemCapture::NewIterate()
{
	if (mMemTypes.empty())
		return nullptr;
	auto iter = new FMemTypeIter();
	iter->mIterator = mMemTypes.begin();
	return iter;
}

void FNativeMemCapture::DestroyIterate(void* iter)
{
	auto i = (FMemTypeIter*)iter;
	delete i;
}

bool FNativeMemCapture::NextIterate(void* iter)
{
	if (iter == nullptr)
		return false;
	auto i = (FMemTypeIter*)iter;
	i->mIterator++;
	if (i->mIterator == mMemTypes.end())
		return false;
	return true;
}

FNativeMemType* FNativeMemCapture::GetMemType(void* iter) 
{
	if (iter == nullptr)
		return nullptr;
	auto i = (FMemTypeIter*)iter;
	return i->mIterator->second;
}

NS_END
