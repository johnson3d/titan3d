// vfxmemory.h
// 
// VictoryCore Code
// memory allocer
//
// NOTICE:
// 
// Author : johnson
// More author :
// Create time : 2002-6-13
// Modify time :
//-----------------------------------------------------------------------------
#ifndef __VFX_MEMORY_H__
#define __VFX_MEMORY_H__

#pragma once
#include "../../BaseHead.h"
#include "vfxdebug.h"

namespace	VFX_Memory
{
	typedef void(WINAPI *FOnMemAlloc)(size_t size, const char* file, size_t line, size_t id);
	typedef void(WINAPI *FOnMemFree)(size_t size, const char* file, size_t line, size_t id);
	typedef void(WINAPI *FOnMemLeak)(void* ptr, size_t size, const char* file, size_t line, size_t id, const char* debugInfo);

	extern FOnMemAlloc GOnMemAllocCallBack;
	extern FOnMemFree GOnMemFreeCallBack;
	extern FOnMemLeak GOnMemLeakCallBack;

	void __MemoryTrace(const char * lpszFormat, ...);
	inline void * __alloc(size_t size){	
		auto ret = malloc(size);
		if (ret == NULL)
		{
			__MemoryTrace("Out Of Memory:VFX_Memory::__alloc");
			__MemoryTrace("Out Of Memory:VFX_Memory::__alloc->%d", size);
		}
		return ret;
	}
	inline void __free(void * pmem){	
		free(pmem); 
	}
	inline void * __alloc(size_t size,const char* file,size_t line){	
		auto ret = malloc(size);
		if (ret == NULL)
		{
			__MemoryTrace("Out Of Memory:VFX_Memory::__alloc");
			__MemoryTrace("Out Of Memory:VFX_Memory::__alloc->%s:%d:%d", file, line, size);
		}
		return ret;
	}

	
//		{	return _malloc_dbg(size,_NORMAL_BLOCK,file,line); }

	enum
	{
		__align_size = 8,
		__pool_size = 128,
		__max_bytes = 256
	};

	inline size_t round_up(size_t bytes)
	{
		return (bytes + __align_size - 1) & ~((size_t)__align_size - 1);
	}

	template<class T>
	class vfxMArray
	{
		T *			m_pBuffer;
		size_t			m_nSize;
		size_t			m_nNumber;
	public:
		vfxMArray()
		:m_pBuffer(0),m_nSize(0),m_nNumber(0)
		{	}
		void Construct()
		{
			m_pBuffer = 0;
			m_nSize = 0;
			m_nNumber = 0;
		}
		~vfxMArray()
		{	m_nSize = m_nNumber = 0;__free(m_pBuffer); }
		T * GetData()
		{	return m_pBuffer; }
		size_t GetNumber()
		{	return m_nNumber; }
		T & operator [](size_t n)
		{
			return m_pBuffer[n];
		}
		void Add(T & ref);
	};

	template<class T>
	void vfxMArray<T>::Add(T & ref)
	{
		if(m_nNumber >= m_nSize)
		{
			m_nSize += __pool_size;
			T * pNew = reinterpret_cast<T*>(__alloc(sizeof(T) * m_nSize));
			memcpy(pNew,m_pBuffer,m_nNumber * sizeof(T));
			__free(m_pBuffer);
			m_pBuffer = pNew;
		}
		m_pBuffer[m_nNumber] = ref;
		m_nNumber++;
	}

	class _small_cookie
	{
	public:
		size_t			dccc1;
		const char *	file;
		size_t			line;
		size_t			id;
		LPCSTR			debuginfo;
		size_t			size;
		#define DCCCNUM 1
		size_t			dccc[DCCCNUM];
		inline void SetDCCC(size_t cc)
		{
			dccc1 = cc;
			for (int i = 0; i < DCCCNUM; i++)
			{
				dccc[i] = cc;
			}
		}
		inline bool TestDCCC(size_t cc)
		{
			if (dccc1 != cc)
				return false;
			for (int i = 0; i < DCCCNUM; i++)
			{
				if (dccc[i] != cc)
					return false;
			}
			return true;
		}
		union
		{
			_small_cookie * next;
			char	data[1];
		};
	};

	class _large_cookie
	{
	public:
		_large_cookie * prev;
		_large_cookie * next;
		size_t			dccc;
		_small_cookie	cookie;
	};

	class __memory_init;

	class small_alloc
	{
		static small_alloc	*	__psmall_alloc;

		struct FreePoint
		{
			_small_cookie*			freePoint;
		};
		FreePoint					pool_chunk[__max_bytes / __align_size];
		vfxMArray<_small_cookie *>	poolArray;
		vfxMArray<size_t>			sizeArray;

		void dump_memory(_small_cookie * check,size_t size, vBOOL dumpUnknown);
		small_alloc();

		_small_cookie*	alloc_pool(size_t size);
	public:
		inline vfxMArray<size_t>& GetSizeArray() {
			return sizeArray;
		}
		void Construct_small_alloc();
	public:
		void Dump(vBOOL dumpUnknown);
	public:
		~small_alloc();
		void destructor();
	
		void * alloc(size_t size,const char * file,size_t line);
		void free(void * memory,const char * file,size_t line);
		void Check();

		static small_alloc & getalloc(){
			return *__psmall_alloc; 
		}
		
		friend class __memory_init;
	};

	class large_alloc
	{
		static large_alloc	*	__plarge_alloc;
		
		_large_cookie		header;
		
		large_alloc();
	public:
		void Construct_large_alloc();
	public:
		void Dump(vBOOL dumpUnknown);
	public:
		~large_alloc();
		void destructor();

		void * alloc(size_t size,const char * file,size_t line);
		void free(void * memory,const char * file,size_t line);
		void Check();

		static large_alloc & getalloc(){	
			return *__plarge_alloc; 
		}

		friend class __memory_init;
	};

	class __memory_init
	{
	public:
		__memory_init();
		~__memory_init();
	};

	extern "C"
	{
		 void vfxMemory_SetDebugInfo(void* memory, LPCSTR info);
	}
}

#endif //end __VFX_MEMORY_H__
