#include "vfxnew.h"
#include "vfxdebug.h"
#include "vfxMemPoolObject.h"
#include "../thread/vfxcritical.h"
#include "../r2m/F2MManager.h"
#include <thread>

#include "vfxmemory.h"

#include "../CoreRtti.h"

#if !defined(PLATFORM_WIN)
#pragma GCC diagnostic ignored "-Wunused-const-variable"
#endif

namespace VMem
{
	extern MemPoolManager* MemPoolManager_obj;
}

 void * _vfxMemoryAlloc(size_t size, const char * file, size_t line);
 void _vfxMemoryFree(void * memory, const char * file, size_t line);

namespace VFX_Memory
{
	pthread_mutex_t g_memory_mutex = PTHREAD_MUTEX_INITIALIZER;
	static void memory_lock()
	{
		pthread_mutex_lock(&g_memory_mutex);
	}
	static void memory_unlock()
	{
		pthread_mutex_unlock(&g_memory_mutex);
	}
	
	class auto_memory_lock
	{
	public:
		auto_memory_lock()
		{
			memory_lock();
		}
		~auto_memory_lock()
		{
			memory_unlock();
		}
	};

	extern size_t g_alloc_times;
	//extern INT64 g_alloc_break_times;
	extern size_t g_memory_obj;
	extern size_t g_memoey_used;
	extern size_t g_memory_max;
}

using namespace VFX_Memory;

extern "C"  void VFX_API vfxMemory_DumpMemoryState(LPCSTR name, vBOOL dumpUnknown);
void exit_fn1(void)
{
	vfxMemory_DumpMemoryState("AppExit: Finalizer__memory_init", TRUE);
}

large_alloc* ConstructLargeAlloc();
small_alloc* ConstructSmallAlloc();

__memory_init::__memory_init()
{
	if (small_alloc::__psmall_alloc == 0)
	{
		small_alloc::__psmall_alloc = ConstructSmallAlloc();
		large_alloc::__plarge_alloc = ConstructLargeAlloc();
		VMem::MemPoolManager::GetInstance();
	}
}

typedef void(WINAPI *FMemoryFinalizer)();
FMemoryFinalizer OnMemoryFinal = nullptr;

__memory_init::~__memory_init()
{
	if (OnMemoryFinal != nullptr)
		OnMemoryFinal();
	
	EngineNS::F2MManager::Instance.Cleanup();
	
	VMem::MemPoolManager::GetInstance()->FinalCleanup();
	vfxMTLockerManager::Instance.Cleanup();
	EngineNS::CoreRttiManager::GetInstance()->Finalize();

	vfxMemory_DumpMemoryState("Finalizer__memory_init", TRUE);
}

//#define NOTUSE_MEMMGR

#ifdef WIN64
const size_t c_uMalloc0xCC = 0xCCCCCCCCCCCCCCCC;
#else
const size_t c_uMalloc0xCC = 0xCCCCCCCC;
#endif
	
#if defined(NOTUSE_MEMMGR)
	#define MaxMemNumber 2000000
	UINT GMemIndex = 0;

	struct MemHead
	{
		MemHead()
		{
			next = NULL;
			prev = NULL;
			file = NULL;
			line = 0;
			size = 0;
			dccc = c_uMalloc0xCC;
			dccc1 = c_uMalloc0xCC;
		}
		size_t		dccc1;
		MemHead*	next;
		MemHead*	prev;

		const char* file;
		size_t		line;

		size_t		index;

		size_t		id;
		size_t		size;
		size_t		dccc;
	};
	
	MemHead GMemHead;
	bool	IsChecking = false;

	struct MemInfo
	{
		const char* file;
		size_t		line;
		size_t		id;
		size_t		size;
		LPCSTR		debuginfo;

		MemHead*	HeadPtr;

		static MemInfo* GAllMems[MaxMemNumber];
		static void Add(MemHead* mem)
		{
			if (mem->id == 0)
			{
				memset(GAllMems, 0, sizeof(GAllMems));
			}
			int count = 0;
			while (true)
			{
				if (GMemIndex >= MaxMemNumber)
				{
					GMemIndex = 0;
				}
				if (GAllMems[GMemIndex] == NULL)
					break;
				GMemIndex++;
				count++;
				
				if (g_memory_obj>MaxMemNumber || count >= MaxMemNumber)
				{
					ASSERT(false);
					UINT limitter = MaxMemNumber;
					__MemoryTrace("Allocted Mem %d > %d\r\n", count, limitter);
					mem->index = -1;
					return;
				}
			}
			mem->index = GMemIndex;
			GAllMems[GMemIndex] = (MemInfo*)::malloc(sizeof(MemInfo));
			GAllMems[GMemIndex]->file = mem->file;
			GAllMems[GMemIndex]->line = mem->line;
			GAllMems[GMemIndex]->id = mem->id;
			GAllMems[GMemIndex]->size = mem->size;
			GAllMems[GMemIndex]->debuginfo = NULL;
			GAllMems[GMemIndex]->HeadPtr = mem;
			GMemIndex++;
		}
		static void Remove(MemHead* mem)
		{
			UINT index = (UINT)mem->index;
			if (index == -1)
				return;

			ASSERT(index < MaxMemNumber);
			mem->index = -1;
			if (GAllMems[index])
			{
				GAllMems[index]->file = NULL;
				GAllMems[index]->line = 0;
				GAllMems[index]->id = 0;
				GAllMems[index]->size = 0;
				GAllMems[index]->HeadPtr = NULL;
				::free(GAllMems[index]);
				GAllMems[index] = NULL;
			}
		}

		bool IsValid(int index) const
		{
			bool bRet = TRUE;
			if (HeadPtr != NULL)
			{
				if (HeadPtr->file != file ||
					HeadPtr->line != line ||
					HeadPtr->id != id ||
					HeadPtr->size != size ||
					HeadPtr->index!=index)
					bRet = FALSE;
			}
			return bRet;
		}
	};
	MemInfo* MemInfo::GAllMems[MaxMemNumber];
	
	void VMem_DumpChain(vBOOL dumpUnknown)
	{
		MemHead* pStart = GMemHead.next;
		if (pStart == NULL)
			return;
		IsChecking = true;
		size_t count = 0;
		const char* file = "";
		size_t line = 0;
		size_t id = 0;
		size_t size = 0;
		for (; pStart != NULL; pStart = pStart->next)
		{
			const char* curfile = pStart->file;
			size_t curline = pStart->line;
			size_t cursize = pStart->size;
			size_t curid = pStart->id;
			count++;
			if (count > g_memory_obj *2)
				break;
			
			file = pStart->file;
			line = pStart->line;
			id = pStart->id;
			size = pStart->size;

			MemInfo* memInfo = NULL;
			if (pStart->index < MaxMemNumber)
			{
				memInfo = MemInfo::GAllMems[pStart->index];
			}
			if (memInfo == NULL && pStart->index!=-1)
			{
				VFX_Memory::__MemoryTrace("memroy interrupted");
				break;
			}
			if (memInfo)
			{
				if (memInfo->HeadPtr != pStart)
				{
					VFX_Memory::__MemoryTrace("memory interrupted: %s:%d", memInfo->file, memInfo->line);
					break;
				}
			}
			if (pStart->dccc != c_uMalloc0xCC || pStart->dccc1 != c_uMalloc0xCC)
			{
				if (memInfo)
				{
					VFX_Memory::__MemoryTrace("memory H: %s:%d", memInfo->file, memInfo->line);
				}
				else
				{
					VFX_Memory::__MemoryTrace("memory H: %s:%d", pStart->file, pStart->line);
				}
			}
			auto pdccc = (size_t*)((BYTE*)pStart + sizeof(MemHead) + pStart->size);
			if (*pdccc != c_uMalloc0xCC)
			{
				if (memInfo)
				{
					VFX_Memory::__MemoryTrace("memory T: %s:%d", memInfo->file, memInfo->line);
				}
				else
				{
					VFX_Memory::__MemoryTrace("memory T: %s:%d", pStart->file, pStart->line);
				}
			}

			if (dumpUnknown || pStart->line != 0)
			{
				__MemoryTrace("%s(%Id) : Memory leak! There have %Id bytes memory had not be delete. Alloc ID : %Id Info : %s\n"
					, pStart->file, pStart->line, pStart->size, pStart->id, memInfo->debuginfo);
			}

			if (GOnMemLeakCallBack)
			{	
				if(memInfo)
					GOnMemLeakCallBack((BYTE*)memInfo->HeadPtr + sizeof(MemHead), memInfo->size, memInfo->file, memInfo->line, memInfo->id, memInfo->debuginfo);
				else
					GOnMemLeakCallBack((BYTE*)pStart + sizeof(MemHead), pStart->size, pStart->file, pStart->line, pStart->id, "");
			}
		}
		IsChecking = false;
	}
	void VMem_CheckMemChain()
	{
		for (int i = 0; i < MaxMemNumber; i++)
		{
			auto info = MemInfo::GAllMems[i];
			if(info==0)
				continue;

			bool isOverRanged = false;
			if (info->IsValid(i) == false)
			{
				isOverRanged = true;
				VFX_Memory::__MemoryTrace("MemInfo Invalid");
			}

			auto pStart = info->HeadPtr;
			if (pStart->dccc != c_uMalloc0xCC || pStart->dccc1 != c_uMalloc0xCC)
			{
				isOverRanged = true;
				VFX_Memory::__MemoryTrace("memory H: %s:%d", info->file, info->line);
			}
			auto pdccc = (size_t*)((BYTE*)pStart + sizeof(MemHead) + info->size);
			if (*pdccc != c_uMalloc0xCC)
			{
				isOverRanged = true;
				VFX_Memory::__MemoryTrace("memory T: %s:%d", info->file, info->line);
			}

			if (isOverRanged && GOnMemLeakCallBack)
				GOnMemLeakCallBack((BYTE*)info->HeadPtr + sizeof(MemHead), info->size, info->file, info->line, info->id, "OverRange");
		}
	}
	void * _vfxMemoryNew(size_t nSize, const char * file, size_t line)
	{
		static __memory_init obj;

		auto_memory_lock	lock;
		//CheckMemChain();
		if (IsChecking)
		{
			ASSERT(false);
		}
		g_memory_obj++;

		auto pMem = (MemHead*)::malloc(nSize + sizeof(MemHead) + sizeof(size_t));
		//mmap(pMem, sizeof(MemHead), PROT_NONE, MAP_FIXED | MAP_PRIVATE, NULL, NULL);
		pMem->file = file;
		pMem->line = line;
		pMem->id = g_alloc_times++;
		pMem->size = nSize;
		pMem->dccc = c_uMalloc0xCC;
		pMem->dccc1 = c_uMalloc0xCC;
		auto ret = (BYTE*)pMem + sizeof(MemHead);
		size_t* pdccc = (size_t*)&ret[nSize];
		*pdccc = c_uMalloc0xCC;

		auto savedNext = GMemHead.next;
		GMemHead.next = pMem;
		pMem->prev = &GMemHead;
		pMem->next = savedNext;
		if(savedNext)
			savedNext->prev = pMem;

		g_memoey_used += nSize;
		if (g_memory_max < g_memoey_used)
			g_memory_max = g_memoey_used;
		MemInfo::Add(pMem);

		if (GOnMemAllocCallBack)
		{
			GOnMemAllocCallBack(nSize, file, line, pMem->id);
		}

		return ret;
	}
	void _vfxMemoryDelete(void * memory, const char * file, size_t line)
	{
		if (memory==NULL)
			return;
		auto_memory_lock	lock;
		if (IsChecking)
		{
			ASSERT(false);
		}
		g_memory_obj--;
		auto pMem = (MemHead*)((BYTE*)memory - sizeof(MemHead));
		if (pMem->dccc1 != c_uMalloc0xCC)
		{
			VMem_CheckMemChain();
			return;
		}
		if (pMem->dccc != c_uMalloc0xCC)
		{
			VMem_CheckMemChain(); //VFX_Memory::__MemoryTrace("memory H: %s:%d", file, line);
			return;
		}
		auto pdccc = (size_t*)((BYTE*)memory + pMem->size);
		if (*pdccc != c_uMalloc0xCC)
		{
			VMem_CheckMemChain(); //VFX_Memory::__MemoryTrace("memory T: %s:%d", file, line);
			return;
		}

		if (pMem->index < MaxMemNumber)
		{
			MemInfo* memInfo = MemInfo::GAllMems[pMem->index];
			if (memInfo->debuginfo)
			{
				::free((void*)memInfo->debuginfo);
				memInfo->debuginfo = NULL;
			}
		}

		if (GOnMemFreeCallBack)
		{
			GOnMemFreeCallBack(pMem->size, pMem->file, pMem->line, pMem->id);
		}

		g_memoey_used -= pMem->size;
		MemInfo::Remove(pMem);
		
		auto savedPrev = pMem->prev;
		auto savedNext = pMem->next;
		
		if (pMem->next)
			pMem->next->prev = pMem->prev;
		if (pMem->prev)
			pMem->prev->next = pMem->next;
		
		memset(pMem, 0xBB, sizeof(MemHead) + pMem->size + sizeof(size_t));
		
		::free(pMem);
	}
#else
	void * _vfxMemoryNew(size_t nSize, const char * file, size_t line)
	{
		static __memory_init obj;
		auto_memory_lock	lock;
		return _vfxMemoryAlloc(nSize, file, line);
	} 
	void _vfxMemoryDelete(void * memory, const char * file, size_t line)
	{
		auto_memory_lock	lock;
		_vfxMemoryFree(memory,file,line);
	}
#endif

#define Inner_Alloc _vfxMemoryNew
#define Inner_Free _vfxMemoryDelete

#if defined(GNU_NEW)
	void* operator new(std::size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc)
	{
		return Inner_Alloc(size, file, line);
	}
	void* operator new(std::size_t size) VGLIBCXX_THROW(std::bad_alloc)
	{
		return Inner_Alloc(size, NULL, 0);
	}
	void* operator new[](std::size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc)
	{
		return Inner_Alloc(size, file, line);
	}
	void* operator new[](std::size_t size) VGLIBCXX_THROW(std::bad_alloc)
	{
		return Inner_Alloc(size, NULL, 0);
	}
	void operator delete(void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, file, line);
	}
	void operator delete(void* p) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void operator delete(void* p, std::size_t) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void operator delete[](void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, file, line);
	}
	void operator delete[](void* p) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void* operator new(std::size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
	{
		return Inner_Alloc(size, NULL, 0);
	}
	void* operator new[](std::size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
	{
		return Inner_Alloc(size, NULL, 0);
	}
	void operator delete(void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void operator delete[](void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void* operator new(std::size_t size, void* __p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
	{
		return __p;
	}
	void operator delete  (void*, void*, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
	{
		//
	}
	/*void* operator new(std::size_t, void* __p) _GLIBCXX_USE_NOEXCEPT
	{
		return __p;
	}
	void* operator new[](std::size_t, void* __p) _GLIBCXX_USE_NOEXCEPT
	{ 
		return __p; 
	}
	void operator delete  (void*, void*) _GLIBCXX_USE_NOEXCEPT 
	{
	}
	void operator delete[](void*, void*) _GLIBCXX_USE_NOEXCEPT
	{
	}*/
#elif defined(APPLE_NEW)
void* operator new(std::size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc)
{
    return Inner_Alloc(size, file, line);
}
void* operator new(std::size_t size) VGLIBCXX_THROW(std::bad_alloc)
{
    return Inner_Alloc(size, NULL, 0);
}
void* operator new[](std::size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc)
{
    return Inner_Alloc(size, file, line);
}
void* operator new[](std::size_t size) VGLIBCXX_THROW(std::bad_alloc)
{
    return Inner_Alloc(size, NULL, 0);
}
void operator delete(void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
{
    Inner_Free(p, file, line);
}
void operator delete(void* p)VGLIBCXX_USE_NOEXCEPT
{
    Inner_Free(p, NULL, 0);
}
void operator delete(void* p, std::size_t) VGLIBCXX_USE_NOEXCEPT
{
    Inner_Free(p, NULL, 0);
}
void operator delete[](void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
{
    Inner_Free(p, file, line);
}
void operator delete[](void* p) VGLIBCXX_USE_NOEXCEPT
{
    Inner_Free(p, NULL, 0);
}
void* operator new(std::size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
{
    return Inner_Alloc(size, NULL, 0);
}
void* operator new[](std::size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
{
    return Inner_Alloc(size, NULL, 0);
}
void operator delete(void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
{
    Inner_Free(p, NULL, 0);
}
void operator delete[](void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
{
    Inner_Free(p, NULL, 0);
}
void* operator new(std::size_t size, void* __p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
{
    return __p;
}
void operator delete  (void*, void*, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
{
    //
}
/*void* operator new(std::size_t, void* __p) _GLIBCXX_USE_NOEXCEPT
	{
 return __p;
	}
	void* operator new[](std::size_t, void* __p) _GLIBCXX_USE_NOEXCEPT
	{
 return __p;
	}
	void operator delete  (void*, void*) _GLIBCXX_USE_NOEXCEPT
	{
	}
	void operator delete[](void*, void*) _GLIBCXX_USE_NOEXCEPT
	{
	}*/
#elif defined(MS_NEW)
	void* operator new(size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc)
	{
		return Inner_Alloc(size, file, line);
	}
	void* operator new(size_t size) VGLIBCXX_THROW(std::bad_alloc)
	{
		return Inner_Alloc(size, __FILE__, __LINE__);
	}
	void* operator new[](size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc)
	{
		return Inner_Alloc(size, file, line);
	}
	void* operator new[](size_t size) VGLIBCXX_THROW(std::bad_alloc)
	{
		return Inner_Alloc(size, __FILE__, __LINE__);
	}
	void operator delete(void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, file, line);
	}
	void operator delete(void* p) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void operator delete(void* p, std::size_t) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void operator delete[](void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, file, line);
	}
	void operator delete[](void* p) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void* operator new(size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
	{
		return Inner_Alloc(size, NULL, 0);
	}
	void* operator new[](size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
	{
		return Inner_Alloc(size, NULL, 0);
	}
	void operator delete(void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void operator delete[](void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT
	{
		Inner_Free(p, NULL, 0);
	}
	void* operator new(std::size_t size, void* __p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
	{
		return __p;
	}
	void operator delete  (void*, void*, const char* file, int line) VGLIBCXX_USE_NOEXCEPT
	{
		//
	}
#endif


extern "C"
{
	VFX_API void vfxMemory_StartWatcher()
	{
		atexit(exit_fn1);
	}
	VFX_API void vfxMemory_DumpMemoryState(LPCSTR name, vBOOL dumpUnknown)
	{
		if (name != NULL)
		{
			__MemoryTrace("%s Begin _vfxDumpMemoryState\n", name);
		}
		//Dump Memory
		{
			auto_memory_lock	lock;
#if defined(NOTUSE_MEMMGR)
			VMem_DumpChain(dumpUnknown);
#else
			large_alloc::getalloc().Dump(dumpUnknown);
			small_alloc::getalloc().Dump(dumpUnknown);
#endif		
		}
		//Dump PooledObject
		{
			VMem::MemPoolManager::GetInstance()->Dump();
		}
		if (name != NULL)
		{
			__MemoryTrace("%s End _vfxDumpMemoryState\n", name);
		}
	}
	VFX_API void vfxMemory_CheckMemoryState(LPCSTR name)
	{
		if (name != NULL)
		{
			__MemoryTrace("%s Begin _vfxCheckMemoryState\n", name);
		}
		// check memory
		{
			auto_memory_lock	lock;

#if defined(NOTUSE_MEMMGR)
			VMem_CheckMemChain();
#else
			large_alloc::getalloc().Check();
			small_alloc::getalloc().Check();
#endif
		}
		//check PooledObject
		{
			VMem::MemPoolManager::GetInstance()->Check();
		}
		if (name != NULL)
		{
			__MemoryTrace("%s End _vfxCheckMemoryState\n", name);
		}
	}
	VFX_API void vfxMemory_SetDebugInfo(void* memory, LPCSTR info)
	{
#if defined(NOTUSE_MEMMGR)
		if (memory == 0)
			return;

		auto_memory_lock	lock;
		MemHead* p = (MemHead*)((INT_PTR)memory - sizeof(MemHead));
		if (p->index >= MaxMemNumber)
			return;

		MemInfo* memInfo = MemInfo::GAllMems[ p->index ];
		if (memInfo == NULL)
			return;
		auto size = strlen(info);
		char* str = (char*)::malloc(size + 1);
		memset(str, 0, size + 1);
		memcpy(str, info, size);
		if (memInfo->debuginfo)
		{
			::free((void*)memInfo->debuginfo);
		}
		memInfo->debuginfo = str;
		return;
#else
		if (memory == 0)
			return;

		auto_memory_lock	lock;

		_small_cookie * p = reinterpret_cast<_small_cookie *>
			((INT_PTR)memory - sizeof(_small_cookie) + sizeof(_small_cookie *));

		auto size = strlen(info);
		char* str = (char*)VFX_Memory::__alloc(size + 1);
		memset(str, 0, size + 1);
		memcpy(str, info, size);
		if (p->debuginfo)
		{
			VFX_Memory::__free((void*)p->debuginfo);
		}
		p->debuginfo = str;
#endif
	}
		LPCSTR vfxMemory_GetDebugInfo(void* memory)
	{
#if defined(NOTUSE_MEMMGR)
		if (memory == 0)
			return "";

		auto_memory_lock	lock;
		MemHead* p = (MemHead*)((INT_PTR)memory - sizeof(MemHead));
		if (p->index >= MaxMemNumber)
			return "";

		MemInfo* memInfo = MemInfo::GAllMems[p->index];
		if (memInfo == NULL)
			return "";
		return memInfo->debuginfo;
#else
		if (memory == 0)
			return "";

		auto_memory_lock	lock;

		_small_cookie * p = reinterpret_cast<_small_cookie *>
			((INT_PTR)memory - sizeof(_small_cookie) + sizeof(_small_cookie *));

		return p->debuginfo;
#endif
	}
}

