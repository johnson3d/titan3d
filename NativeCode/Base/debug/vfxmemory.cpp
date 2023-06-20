// vfxmemory.cpp
// 
// VictoryCore Code
// memory allocer
//
// Author : johnson
// More author :
// Create time : 2002-6-13
// Modify time :
//-----------------------------------------------------------------------------

#include "vfxmemory.h"
#include "vfxMemPoolObject.h"

#include "../CoreSDK.h"

#if !defined(PLATFORM_WIN)
	inline vBOOL IsBadReadPtr(CONST VOID *lp, UINT_PTR ucb)
	{
		return FALSE;
	}
#endif


#	ifndef ASSERT
#		include <cassert>
#		define ASSERT assert
#	endif

namespace VFX_Memory
{
	FOnMemAlloc GOnMemAllocCallBack = NULL;
	FOnMemFree GOnMemFreeCallBack = NULL;
	FOnMemLeak GOnMemLeakCallBack = NULL;
	void __MemoryTrace(const char * lpszFormat, ...)
	{
		va_list pArgs;
		va_start(pArgs, lpszFormat);

		char Tmp[2048];
		vsnprintf(Tmp, 1024, lpszFormat, pArgs);
		VFX_LTRACE(ELTT_Memory, "%s\n", Tmp);
		va_end(pArgs);
	}

	#ifdef WIN64
		const size_t c_uMalloc0xCC	= 0xCCCCCCCCCCCCCCCC;
	#else
		const size_t c_uMalloc0xCC	= 0xCCCCCCCC;
	#endif
	size_t g_alloc_times = 0;
	INT64 g_alloc_break_times = -1;
	size_t g_memory_obj = 0;
	size_t g_memoey_used = 0;
	size_t g_memory_max = 0;

	small_alloc	*	small_alloc::__psmall_alloc = 0;

	small_alloc::small_alloc()
	{
		memset(pool_chunk,0,sizeof(pool_chunk));
	}

	void small_alloc::Construct_small_alloc()
	{
		memset(pool_chunk,0,sizeof(pool_chunk));
		poolArray.Construct();
	
		sizeArray.Construct();
	}

	small_alloc::~small_alloc()
	{
		//MessageBoxA(NULL,"","",MB_OK);
		destructor();
	}

	void small_alloc::destructor()
	{
		Dump(TRUE);

		size_t size = poolArray.GetNumber();
		_small_cookie **	p = poolArray.GetData();
	
		for (size_t i = 0; i < size; ++i, ++p)
		{
			__free(*p);
		}
	}

	_small_cookie * small_alloc::alloc_pool(size_t size)
	{
		size = round_up(size);
		size_t headsize = round_up(size + sizeof(_small_cookie) - sizeof(_small_cookie *)) + sizeof(size_t) * DCCCNUM;
		size_t newsize = headsize;
		_small_cookie * p = reinterpret_cast<_small_cookie *>(__alloc(newsize * __pool_size));
		//UINT_PTR tail = (INT_PTR)p + newsize * __pool_size;

		{
			_small_cookie * pp = reinterpret_cast<_small_cookie*>(p);
			_small_cookie * next;
			for (size_t i = 0; i < __pool_size - 1; ++i, pp = next)
			{
				next = reinterpret_cast<_small_cookie*>((INT_PTR)pp + newsize);
				pp->next = next;
				pp->size = size;
				pp->file = 0;
				pp->line = 0;
				pp->id = (size_t)-1;
				pp->debuginfo = NULL;
				pp->dccc1 = c_uMalloc0xCC;
				pp->SetDCCC(c_uMalloc0xCC);
			}
			pp->next = 0;
			pp->size = size;
			pp->file = 0;
			pp->line = 0;
			pp->id = (size_t)-1;
			pp->debuginfo = NULL;
			pp->dccc1 = c_uMalloc0xCC;
			pp->SetDCCC(c_uMalloc0xCC);
		}

		poolArray.Add(p);
		sizeArray.Add(size);
		return p;
	}
	
	void * small_alloc::alloc(size_t size,const char * file,size_t line)
	{    
		ASSERT(size <= __max_bytes);
		if (size <= 0)
		{
			size = 1;
		}
		auto& freePoint = pool_chunk[(size - 1) / __align_size];
		bool IsAllocPool = false;
		if (freePoint.freePoint == 0)
		{
			IsAllocPool = true;
			freePoint.freePoint = alloc_pool(size);
		}
		//UINT_PTR _ptr = (UINT_PTR)freePoint.freePoint;
		
		auto free_list = freePoint.freePoint;
		void * ret = free_list->data;
		
		free_list->file = file;
		free_list->line = line;
		free_list->size = size;
		free_list->id = g_alloc_times++;
		//free_list->dccc = c_uMalloc0xCC;
		/*if (free_list->id == 263246)
		{
			ASSERT(false);
		}*/
		free_list->SetDCCC(c_uMalloc0xCC);
		free_list->debuginfo = NULL;
		g_memoey_used += free_list->size;
		g_memory_obj++;
		if(g_memory_max < g_memoey_used)
			g_memory_max = g_memoey_used;

		if (GOnMemAllocCallBack)
		{
			GOnMemAllocCallBack(size, file, line, free_list->id);
		}

		freePoint.freePoint = free_list->next;
		
		//*(size_t *)((INT_PTR)ret + size) = c_uMalloc0xCC;
		auto pTailDCCC = (size_t *)((INT_PTR)ret + size);
		for (int i = 0; i < DCCCNUM; i++)
		{
			pTailDCCC[i] = c_uMalloc0xCC;
		}
		return ret;
	}

	inline bool TestTailDCCC(size_t* pDCCC)
	{
		for (int i = 0; i < DCCCNUM; i++)
		{
			if (pDCCC[i] != c_uMalloc0xCC)
				return false;
		}
		return true;
	}

	void small_alloc::free(void * memory,const char * file,size_t line)
	{
		_small_cookie * pcook = reinterpret_cast<_small_cookie *>
			((INT_PTR)memory - sizeof(_small_cookie) + sizeof(_small_cookie *));
		size_t size = pcook->size;
		ASSERT(size <= __max_bytes);

		if (pcook->debuginfo)
		{
			VFX_Memory::__free((void*)pcook->debuginfo);
			pcook->debuginfo = NULL;
		}

		//size_t * pdccc = reinterpret_cast<size_t*>((INT_PTR)memory + size);
		//if(*pdccc != c_uMalloc0xCC)
		size_t * pdccc = reinterpret_cast<size_t*>((INT_PTR)memory + size);
		if(TestTailDCCC(pdccc)==false)
		{
			if(file)
				__MemoryTrace("%s(%d) : Free Verify pointer(0x%p) failed!\n",file, (int)line,memory);
			if(IsBadReadPtr(pcook->file,4) == 0)
				__MemoryTrace("%s(%d) : Free Verify pointer(0x%p) failed! Alloc ID : %d\n",pcook->file, (int)pcook->line,memory,pcook->id);
			else
				__MemoryTrace("Unkown position : Free Verify pointer(0x%p) failed! Alloc ID : %d\n",memory,pcook->id);
			return;
		}
		if (GOnMemFreeCallBack)
		{
			GOnMemFreeCallBack(size, pcook->file, pcook->line, pcook->id);
		}
		memset(memory,0xCC,size);

		g_memory_obj--;
		g_memoey_used -= size;
	
		auto& freePoint = pool_chunk[(pcook->size - 1) / __align_size];
		pcook->next = freePoint.freePoint;
		
		freePoint.freePoint = pcook;
		pcook->id = (size_t)-1;
	}

	void small_alloc::dump_memory(_small_cookie * check,size_t leak_size, vBOOL dumpUnknown)
	{
		ASSERT(leak_size > 0);

		size_t size = round_up(leak_size + sizeof(_small_cookie) - sizeof(_small_cookie *)) + sizeof(size_t) * DCCCNUM;
		for(size_t i = 0 ; i < __pool_size ; ++i)
		{
			if (check->id != (size_t)-1)
			{
				if (GOnMemLeakCallBack)
					GOnMemLeakCallBack(check->data, check->size, check->file, check->line, check->id, check->debuginfo);

				__MemoryTrace("%s(%d) : Memory leak! There have %d bytes memory had not be delete. Alloc ID : %d Info : %s\n"
					, check->file, (int)check->line, (int)check->size, (int)check->id, check->debuginfo);

				if (dumpUnknown)
				{
					__MemoryTrace("Unkown position : Memory leak! There have %d bytes memory had not be delete. Alloc ID : %d\n"
						, (int)check->size, (int)check->id);
				}
				//if (check->line!=0)// && IsBadReadPtr(check->file,4) == 0)
				//{
				//	__MemoryTrace("%s(%d) : Memory leak! There have %d bytes memory had not be delete. Alloc ID : %d Info : %s\n"
				//		, check->file, (int)check->line, (int)check->size, (int)check->id, check->debuginfo);
				//}	
				//else
				//{
				//	if (dumpUnknown)
				//	{
				//		__MemoryTrace("Unkown position : Memory leak! There have %d bytes memory had not be delete. Alloc ID : %d\n"
				//			, (int)check->size, (int)check->id);
				//	}
				//}
			}
			check = reinterpret_cast<_small_cookie*>((INT_PTR)check + size);
		}
	}

	void small_alloc::Dump(vBOOL dumpUnknown)
	{
		for(size_t i = 0; i < poolArray.GetNumber(); ++i)
			dump_memory(poolArray[i],sizeArray[i], dumpUnknown);
	}
	void small_alloc::Dump(EngineNS::FNativeMemCapture* capture)
	{
		for (size_t i = 0; i < poolArray.GetNumber(); ++i)
		{
			_small_cookie* check = poolArray[i];
			size_t size = round_up(sizeArray[i] + sizeof(_small_cookie) - sizeof(_small_cookie*)) + sizeof(size_t) * DCCCNUM;
			for (size_t i = 0; i < __pool_size; ++i)
			{
				auto type = capture->GetOrNewMemType((int)check->size, check->file, (int)check->line);
				ASSERT(type->Size == check->size);
				/*if (type->Size == 0)
				{
					type->Size = check->size;
				}*/
				type->Count++;
				check = reinterpret_cast<_small_cookie*>((INT_PTR)check + size);
			}
		}
	}
	void small_alloc::Check()
	{
		for(size_t n=0; n<poolArray.GetNumber(); ++n)
		{
			size_t size = sizeArray[n];
			size_t newsize = round_up(size + sizeof(_small_cookie) - sizeof(_small_cookie *)) + sizeof(size_t) * DCCCNUM;
			_small_cookie * pcook = poolArray[n];

			for(size_t i=0; i<__pool_size; ++i)
			{
				//if(pcook->dccc != c_uMalloc0xCC && pcook->id != (size_t)-1)
				if (pcook->TestDCCC(c_uMalloc0xCC)==false && pcook->id != (size_t)-1)
				{
					if(IsBadReadPtr(pcook->file,4) == 0)
						__MemoryTrace("%s(%d) : Verify pointer(0x%p) failed!!! Alloc ID : %d\n",
									pcook->file, (int)pcook->line,pcook->data,pcook->id);
					else
						__MemoryTrace("Unkown position : Verify pointer(0x%p) failed!!! Alloc ID : %d\n",
									pcook->data,pcook->id);
				}
				size_t * pdccc = reinterpret_cast<size_t*>(((INT_PTR)pcook->data) + pcook->size);
				//if(*pdccc != c_uMalloc0xCC && pcook->id != (size_t)-1)
				if(TestTailDCCC(pdccc)==false && pcook->id != (size_t)-1)
				{
					if(IsBadReadPtr(pcook->file,4) == 0)
						__MemoryTrace("%s(%d) : Check Verify pointer(0x%p) failed! Alloc ID : %d\n",
									pcook->file, (int)pcook->line,pcook->data,pcook->id);
					else
						__MemoryTrace("Unkown position : Check Verify pointer(0x%p) failed! Alloc ID : %d\n",
									pcook->data,pcook->id);
				}

				pcook = (_small_cookie *)((INT_PTR)pcook + newsize);
			}
		}
	}
	//-----------------------------------------------------------------------------
	large_alloc	*	large_alloc::__plarge_alloc = 0;

	large_alloc::large_alloc()
	{
		Construct_large_alloc();
	}

	void large_alloc::Construct_large_alloc()
	{
		memset(&header, 0, sizeof(header));
	}

	large_alloc::~large_alloc()
	{
		destructor();
	}

	void large_alloc::destructor()
	{
		Dump(TRUE);
	}

	void * large_alloc::alloc(size_t size,const char * file,size_t line)
	{	
		size = round_up(size);
		void* _allocAddress = __alloc(size + sizeof(_large_cookie) + sizeof(size_t),file,line);
		if(_allocAddress==NULL)
			return NULL;
		_large_cookie * pcook = reinterpret_cast<_large_cookie *>(_allocAddress);

		size_t * pdccc = reinterpret_cast<size_t *>((INT_PTR)pcook->cookie.data + size);
		*pdccc = c_uMalloc0xCC;
		pcook->dccc = c_uMalloc0xCC;
		pcook->next = header.next;
		pcook->prev = &header;
		if(header.next)
			header.next->prev = pcook;
		header.next = pcook;
		pcook->cookie.file = file;
		pcook->cookie.line = line;
		pcook->cookie.size = size;
		pcook->cookie.id = g_alloc_times++;
		pcook->cookie.SetDCCC(c_uMalloc0xCC);
		pcook->cookie.debuginfo = NULL;
		
		if (size == 2320)
		{
			int xxx = 0;
		}

        static size_t debugSizeMin=0;
        static size_t debugSizeMax=4096;
        if(file == NULL && (size>=debugSizeMin && size<=debugSizeMax) )
        {
            int i = 0;
            i++;
        }
		{
			//auto_memory_lock	lock;
			g_memory_obj++;
		}

		g_memoey_used += size;
		if(g_memory_max < g_memoey_used)
			g_memory_max = g_memoey_used;

		if (GOnMemAllocCallBack)
		{
			GOnMemAllocCallBack(size, file, line, pcook->cookie.id);
		}

		return pcook->cookie.data;
	}

	void large_alloc::free(void * memory,const char * file,size_t line)
	{
		_large_cookie * pcook = reinterpret_cast<_large_cookie *>
			((INT_PTR)memory - sizeof(_large_cookie) + sizeof(_large_cookie *));
		if (pcook->cookie.debuginfo)
		{
			__free((void*)pcook->cookie.debuginfo);
			pcook->cookie.debuginfo = NULL;
		}
		if(pcook->dccc != c_uMalloc0xCC || pcook->cookie.TestDCCC(c_uMalloc0xCC)==false)
		{
			if(file)
				__MemoryTrace(vT("%s(%d) : Free Verify pointer(0x%p) failed!!!\n"),file, (int)line,memory);
			if(IsBadReadPtr(pcook->cookie.file,4) == 0)
				__MemoryTrace(vT("%s(%d) : Free Verify pointer(0x%p) failed!!! Alloc ID : %d\n"),pcook->cookie.file, (int)pcook->cookie.line,memory, (int)pcook->cookie.id);
			else
				__MemoryTrace(vT("Unkown position : Free Verify pointer(0x%p) failed!!! Alloc ID : %d\n"),memory, (int)pcook->cookie.id);
			return ;
		}
		size_t * pdccc = reinterpret_cast<size_t *>((INT_PTR)memory + pcook->cookie.size);
		if(*pdccc != c_uMalloc0xCC)
		{
			if(file)
				__MemoryTrace(vT("%s(%d) : Free Verify pointer(0x%p) failed!\n"),file, (int)line,memory);
			if(IsBadReadPtr(pcook->cookie.file,4) == 0)
				__MemoryTrace(vT("%s(%d) : Free Verify pointer(0x%p) failed! Alloc ID : %d\n"),pcook->cookie.file, (int)pcook->cookie.line,memory,pcook->cookie.id);
			else
				__MemoryTrace(vT("Unkown position : Free Verify pointer(0x%p) failed! Alloc ID : %d\n"),memory,pcook->cookie.id);
		}

		if (GOnMemFreeCallBack)
		{
			GOnMemFreeCallBack(pcook->cookie.size, pcook->cookie.file, pcook->cookie.line, pcook->cookie.id);
		}

		memset(memory,0xCC,pcook->cookie.size);

		{
			//auto_memory_lock	lock;
			g_memory_obj--;
		}
	
		g_memoey_used -= pcook->cookie.size;
		if(pcook->prev)
			pcook->prev->next = pcook->next;
		if(pcook->next)
			pcook->next->prev = pcook->prev;
		__free(pcook);
	}

	void large_alloc::Dump(vBOOL dumpUnknown)
	{
		_large_cookie * p = header.next;
		while(p)
		{
			if (GOnMemLeakCallBack)
			{
				GOnMemLeakCallBack(p->cookie.data, p->cookie.size, p->cookie.file, p->cookie.line, p->cookie.id, p->cookie.debuginfo);
			}
			if (p->cookie.line!=0)
			{	
				__MemoryTrace(vT("%s(%d) : Memory leak! There have %d bytes(%d K) memory had not be delete. Alloc ID : %d Info : %s\n")
					, p->cookie.file, (int)p->cookie.line, (int)p->cookie.size, (int)(p->cookie.size + 1023) / 1024, (int)p->cookie.id, p->cookie.debuginfo);
			}
			else
			{
				if (dumpUnknown)
				{
					__MemoryTrace(vT("Unkown position(0x%p) : Memory leak! There have %d bytes(%d K) memory had not be delete. Alloc ID : %d\n")
						, p->cookie.data, (int)p->cookie.size, (int)(p->cookie.size + 1023) / 1024, (int)p->cookie.id);
				}
			}
			if(p->dccc != c_uMalloc0xCC||p->cookie.TestDCCC(c_uMalloc0xCC)==false)
			{
				if(IsBadReadPtr(p,p->cookie.size) == 0)
					__MemoryTrace(vT("%s(%d) : Verify pointer(0x%p) failed!!! Alloc ID : %d\n")
					,p->cookie.file,(int)p->cookie.line,p->cookie.data,(int)p->cookie.id);
				else
					__MemoryTrace(vT("Unkown position : Verify pointer(0x%p) failed!!! Alloc ID : %d\n")
					,p->cookie.data,(int)p->cookie.id);
			}
			size_t * pdccc = reinterpret_cast<size_t *>((INT_PTR)p->cookie.data + p->cookie.size);
			if(*pdccc != c_uMalloc0xCC)
			{
				if(IsBadReadPtr(p,p->cookie.size) == 0)
					__MemoryTrace(vT("%s(%d) : Verify pointer(0x%p) failed! Alloc ID : %d\n")
					,p->cookie.file,(int)p->cookie.line,p->cookie.data,(int)p->cookie.id);
				else
					__MemoryTrace(vT("Unkown position : Verify pointer(0x%p) failed! Alloc ID : %d\n")
					,p->cookie.data,(int)p->cookie.id);
			}

			p = p->next;
		}
	}

	void large_alloc::Check()
	{
		_large_cookie * p = header.next;
		while(p)
		{
			if(p->dccc != c_uMalloc0xCC||p->cookie.TestDCCC(c_uMalloc0xCC)==false)
			{
				if(IsBadReadPtr(p,p->cookie.size) == 0)
					__MemoryTrace(vT("%s(%d) : Check Verify pointer(0x%p) failed!!! Alloc ID : %d\n")
					,p->cookie.file,(int)p->cookie.line,p->cookie.data,(int)p->cookie.id);
				else
					__MemoryTrace(vT("Unkown position : Check Verify pointer(0x%p) failed!!! Alloc ID : %d\n")
					,p->cookie.data,(int)p->cookie.id);
			}
			size_t * pdccc = reinterpret_cast<size_t *>((INT_PTR)p->cookie.data + p->cookie.size);
			if(*pdccc != c_uMalloc0xCC)
			{
				if(IsBadReadPtr(p,p->cookie.size) == 0)
					__MemoryTrace(vT("%s(%d) : Check Verify pointer(0x%p) failed! Alloc ID : %d\n")
					,p->cookie.file,(int)p->cookie.line,p->cookie.data,(int)p->cookie.id);
				else
					__MemoryTrace(vT("Unkown position : Check Verify pointer(0x%p) failed! Alloc ID : %d\n")
					,p->cookie.data,p->cookie.id);
			}
			p = p->next;
		}
	}


	//-----------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------
	//static large_alloc large_alloc::getalloc();
	//static small_alloc small_alloc::getalloc();

	void _vfxDestructorMemory()
	{
		small_alloc::getalloc().destructor();
		large_alloc::getalloc().destructor();
	}

}

using namespace VFX_Memory;

//#pragma init_seg(compiler)
//__memory_init	memory_init;

 void * _vfxMemoryAlloc(size_t size,const char * file,size_t line)
{
	if (size > __max_bytes)
	{
		auto ret = large_alloc::getalloc().alloc(size, file, line);
		return ret;
	}	
	else
	{
		auto ret = small_alloc::getalloc().alloc(size, file, line);
		return ret;
	}
}

 void _vfxMemoryFree(void * memory,const char * file,size_t line)
{
	if(memory == 0)
		return ;

	_small_cookie * p = reinterpret_cast<_small_cookie *> 
		((INT_PTR)memory - sizeof(_small_cookie) + sizeof(_small_cookie *));
	
	if(p->size > __max_bytes)
		large_alloc::getalloc().free(memory,file,line);
	else
		small_alloc::getalloc().free(memory,file,line);
}

large_alloc* ConstructLargeAlloc()
{
	large_alloc* result = (large_alloc*)malloc(sizeof(large_alloc));
	result->Construct_large_alloc();
	return result;
}

small_alloc* ConstructSmallAlloc()
{
	small_alloc* result = (small_alloc*)malloc(sizeof(small_alloc));
	result->Construct_small_alloc();
	return result;
}

NS_BEGIN

void FNativeMemCapture::CaptureNativeMemoryState()
{
	{
		auto cur = large_alloc::getalloc().GetHeader();

		_large_cookie* p = cur->next;
		while (p)
		{
			auto type = this->GetOrNewMemType((int)p->cookie.size, p->cookie.file, (int)p->cookie.line);
			ASSERT(type->Size == p->cookie.size);
			/*if (type->Size == 0)
			{
				type->Size = p->cookie.size;
			}*/
			type->Count++;
			p = p->next;
		}
	}
	{
		small_alloc::getalloc().Dump(this);
	}
}

NS_END

//void __memory_init::FinalDump()
//{
//	if(small_alloc::__psmall_alloc == 0)
//	{
//		small_alloc::__psmall_alloc->destructor();
//		free(small_alloc::__psmall_alloc);
//		small_alloc::__psmall_alloc = 0;
//	}
//
//	if(large_alloc::__plarge_alloc)
//	{
//		large_alloc::__plarge_alloc->destructor();
//		free(large_alloc::__plarge_alloc);
//		large_alloc::__plarge_alloc = 0;
//	}
//}

extern "C"
{
	size_t SDK_vfxMemory_MemoryUsed()
	{
		return g_memoey_used;
	}
 
	size_t SDK_vfxMemory_MemoryMax()
	{
		return g_memory_max; 
	} 

	size_t SDK_vfxMemory_MemoryAllocTimes()
	{
		return g_alloc_times;
	}

	void SDK_vfxMemory_SetMemAllocCallBack(FOnMemAlloc cb)
	{
		GOnMemAllocCallBack = cb;
	}
	
	void SDK_vfxMemory_SetMemFreeCallBack(FOnMemFree cb)
	{
		GOnMemFreeCallBack = cb;
	}

	void SDK_vfxMemory_SetMemLeakCallBack(FOnMemLeak cb)
	{
		GOnMemLeakCallBack = cb;
	}

	VFX_API UINT SDK_vfxGetSmallMemoryPoolSize()
	{
		UINT sum = 0;
		vfxMArray<size_t>& poolSize = small_alloc::getalloc().GetSizeArray();
		for (size_t i = 0; i < poolSize.GetNumber(); i++)
		{
			size_t size = poolSize[i];
			size_t headsize = round_up(size + sizeof(_small_cookie) - sizeof(_small_cookie *)) + sizeof(size_t) * DCCCNUM;
			sum += (UINT)(headsize * __pool_size);
		}
		return sum;
	}
};
