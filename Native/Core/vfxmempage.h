// vfxmempage.h
// 
// VictoryCore Code
//
// Author : johnson
// More author : 
// Create time : 2002-10-11   15:07
// Modify time :
//-----------------------------------------------------------------------------
#ifndef __vfxmempage_H__2002_11_1
#define __vfxmempage_H__2002_11_1

#pragma once

#include "debug/vfxdebug.h"

namespace VFX_MEMPAGE
{
	class __VMemPageList {
		struct _page {
			_page * next;
			BYTE	data[1];
		};
		_page *		nextpage;

	public:
		inline void FreePool()
		{
			for (_page * page = nextpage; page;)
			{
				_page * tmp = page;
				page = tmp->next;
				//delete[] reinterpret_cast<BYTE *>(tmp);
				_vfxMemoryDelete(tmp, __FILE__, __LINE__);
			}
			nextpage = NULL;
		}
		inline void * AllocPool(INT_PTR nSize)
		{
			_page * page = reinterpret_cast<_page *>(_vfxMemoryNew(sizeof(_page) + nSize, __FILE__,__LINE__));
			page->next = nextpage;
			nextpage = page;
			return page->data;
		}

		__VMemPageList() :nextpage(NULL)
		{
#if defined(VFX_DEBUG_MEMORY)
			delete new char[0];
#else
			new char[0];
#endif
		}
		~__VMemPageList()
		{
			FreePool();
		}
	};

	template <bool _sel>
	struct _VMemPageList
	{
	private:
		static __VMemPageList		list;
	public:
		static inline void * AllocPool(INT_PTR nSize)
		{
			return list.AllocPool(nSize);
		}
	};

#if defined(VFX_DEBUG_MEMORY)
	class _small_cookie
	{
	public:
		const char * file;
		unsigned int line;
		size_t		 id;
		size_t		 size;
		union
		{
			_small_cookie * next;
			char	data[1];
		};
	};
#endif

	template<size_t _D>
	struct _node
	{
#if defined(VFX_DEBUG_MEMORY)
		INT_PTR *		chunk;
		DWORD			crch;
#endif
		union
		{
			_node<_D> *	next;
			BYTE		obj[1];
		};
	};
	template<>
	struct _node<0>
	{
		union
		{
			_node<0> *	next;
			BYTE		obj[1];
		};
	};

	template<size_t _D>
	struct _page
	{
		_node<_D>		node[1];

		_page(INT_PTR nSize, UINT_PTR PS)
		{
			_node<_D> * nd = node;
			for (UINT_PTR i = 0; i < PS - 1; ++i)
			{
#if defined(VFX_DEBUG_MEMORY)
				nd->chunk = NULL;
#endif
				nd->next = reinterpret_cast<_node<_D> *>(reinterpret_cast<INT_PTR>(nd) + nSize);
				nd = nd->next;
			}
			nd->next = NULL;
		}

		template <class _Ty>
		void *operator new(size_t, _Ty* p)
		{
			return p;
		}
	};
	template<>
	struct _page<0>
	{
		_node<0>		node[1];

		_page(INT_PTR nSize, UINT_PTR PS)
		{
			_node<0> * nd = node;
			for (INT_PTR i = 0; i < (INT_PTR)PS - 1; ++i)
			{
				nd->next = reinterpret_cast<_node<0> *>(reinterpret_cast<INT_PTR>(nd) + nSize);
				nd = nd->next;
			}
			nd->next = NULL;
		}

		template <class _Ty>
		void *operator new(size_t, _Ty* p)
		{
			return p;
		}
	};
}

typedef VFX_MEMPAGE::_VMemPageList<true>		VMemPageList;

struct __MemPageAutoLock
{
	std::atomic<long> &	__lock;

	__MemPageAutoLock(std::atomic<long> & t)
		:__lock(t)
	{
		while (__lock.exchange(1)) Sleep(0);
	};
	~__MemPageAutoLock() {
		__lock.exchange(0);
	}
};

template<UINT PageSize = 128>
class VMemPage
{
#if !defined(PLATFORM_WIN)

	void OutputDebugStringA(LPCSTR lpOutputString)
	{
	}

	void OutputDebugStringW(LPCSTR lpOutputString)
	{
	}

	vBOOL IsBadReadPtr(CONST VOID *lp, UINT_PTR ucb)
	{
		return TRUE;
	}

	vBOOL IsBadWritePtr(LPVOID lp, UINT_PTR ucb)
	{
		return TRUE;
	}

	int MessageBoxA(HWND hWnd, LPCSTR lpText, LPCSTR lpCaption, UINT uType)
	{
		return 0;
	}

	int MessageBoxW(HWND hWnd, LPCSTR lpText, LPCSTR lpCaption, UINT uType)
	{
		return 0;
	}
#endif
public:
#ifdef VFX_DEBUG_MEMORY
	typedef VFX_MEMPAGE::_node<1>		node_type;
	typedef VFX_MEMPAGE::_page<1>		page_type;
	typedef VFX_MEMPAGE::_small_cookie		_small_cookie;
#else
	typedef VFX_MEMPAGE::_node<0>		node_type;
	typedef VFX_MEMPAGE::_page<0>		page_type;
#endif

#if defined(VFX_DEBUG_MEMORY)
	void * Alloc(size_t size, LPCSTR file, INT line)
#else
	void * Alloc(size_t size)
#endif
	{
		try
		{
#ifdef _MT
			__MemPageAutoLock	__lock(lLock);
#endif

			if (nextnode == NULL)
			{	
				alignedsize = (size + sizeof(node_type *) - 1) & ~(sizeof(node_type *) - 1);
#if defined(VFX_DEBUG_MEMORY)
				orisize = size;

				size_t _size = alignedsize + sizeof(node_type) - sizeof(node_type *) + sizeof(DWORD);
#else
				size_t _size = alignedsize + sizeof(node_type) - sizeof(node_type *);
#endif
				nextnode = reinterpret_cast<node_type *>\
					(new(VMemPageList::AllocPool(_size * PageSize)) page_type(_size, PageSize));
			}
			node_type * tmp = nextnode;
			nextnode = tmp->next;
#if defined(VFX_DEBUG_MEMORY)
			tmp->chunk = new(file, line) INT_PTR;
			tmp->crch = 0xCCCCCCCC;
			*(DWORD*)(tmp->obj + size) = 0xCCCCCCCC;
#endif
			return tmp->obj;
		}
		catch (...)
		{
			throw;
		}
	}
	void Free(void * memory)
	{
		if (memory == NULL)
			return;
		try
		{
#ifdef _MT
			__MemPageAutoLock	__lock(lLock);
#endif

			node_type * tmp = reinterpret_cast<node_type *>\
				(reinterpret_cast<INT_PTR>(memory) - sizeof(node_type) + sizeof(node_type *));

#if defined(VFX_DEBUG_MEMORY)
			_small_cookie * p = reinterpret_cast<_small_cookie*>
				((INT_PTR)tmp->chunk - sizeof(_small_cookie) + sizeof(_small_cookie *));
			if (tmp->crch != 0xCCCCCCCC)
			{
				if (IsBadReadPtr(p->file, 4) == 0)
					_vfxTraceA("%s(%Id) : Verify pointer(0x%p) failed!!! Alloc ID : %Id\n"
						, p->file, p->line, p->data, p->id);
				else
					_vfxTraceA("Unkown position : Verify pointer(0x%p) failed!!! Alloc ID : %Id\n"
						, p->data, p->id);
			}
			if (*(DWORD*)(tmp->obj + orisize) != 0xCCCCCCCC)
			{
				if (IsBadReadPtr(p->file, 4) == 0)
					_vfxTraceA("%s(%Id) : Verify pointer(0x%p) failed! Alloc ID : %Id\n"
						, p->file, p->line, p->data, p->id);
				else
					_vfxTraceA("Unkown position : Verify pointer(0x%p) failed! Alloc ID : %Id\n"
						, p->data, p->id);
			}
			delete tmp->chunk;
#endif

			tmp->next = nextnode;
			nextnode = tmp;
		}
		catch (...)
		{
			throw;
		}
	}

	VMemPage()
	{
#if defined(VFX_DEBUG_MEMORY)
		orisize = 0;
#endif
		alignedsize = 0;
#ifdef _MT
		lLock = 0;
#endif
	}

private:
	node_type *				nextnode;
#if defined(VFX_DEBUG_MEMORY)
	size_t					orisize;
#endif
	size_t					alignedsize;
#ifdef _MT
	std::atomic<long>		lLock;
#endif
};

template<class T, UINT PS = 128>
class VMemPageBase
{
private:
	static VMemPage<PS>		pool;
public:

#if defined(VFX_DEBUG_MEMORY) && defined(PLATFORM_WIN)
	inline void * _cdecl operator new (size_t size, LPCSTR file, INT line)
	{
		return pool.Alloc(size, file, line);
	}
	inline void _cdecl operator delete (void * p, LPCSTR, INT)
	{
		pool.Free(p);
	}
	inline void _cdecl operator delete (void * p, size_t, LPCSTR, INT)
	{
		pool.Free(p);
	}
#endif
	inline void * operator new (size_t size)
	{
#if defined(VFX_DEBUG_MEMORY) && defined(PLATFORM_WIN)
		return pool.Alloc(size, 0, 0);
#else
		return pool.Alloc(size);
#endif
	}
	inline void operator delete (void * p)
	{
		pool.Free(p);
	}
	inline void operator delete (void * p, size_t)
	{
		pool.Free(p);
	}
};

template<class T, UINT PS>
VMemPage<PS> VMemPageBase<T, PS>::pool;
template <bool _sel>
VFX_MEMPAGE::__VMemPageList VFX_MEMPAGE::_VMemPageList<_sel>::list;

//-----------------------------------------------------------------------------

template<typename T>
struct VMemoryHeap
{
public:
	typedef VFX_MEMPAGE::_node<0>			node_type;
	typedef VFX_MEMPAGE::_page<0>			page_type;

	VMemoryHeap(UINT ps = 128)
		:nextnode(NULL), PageSize(ps)
	{}

	inline void * AllocPool(INT_PTR nSize)
	{
		return list.AllocPool(nSize);
	}
	inline void FreePool()
	{
		list.FreePool();
		nextnode = NULL;
	}

	void * _Alloc()
	{
		if (nextnode == NULL)
		{
			size_t _size = sizeof(T) + sizeof(node_type) - sizeof(node_type *);
			nextnode = reinterpret_cast<node_type *>\
				(new(AllocPool(_size * PageSize)) page_type(_size, PageSize));
		}
		node_type * tmp = nextnode;
		nextnode = tmp->next;
		return tmp->obj;
	}
	void _Free(void * p)
	{
		if (p == NULL)
			return;

		node_type * tmp = reinterpret_cast<node_type *>\
			(reinterpret_cast<INT_PTR>(p) - sizeof(node_type) + sizeof(node_type *));
		tmp->next = nextnode;
		nextnode = tmp;
	}
	T * Alloc()
	{
		return (T*)new(_Alloc()) VNewWrapper<T>;
	}
	void Free(T * p)
	{
		if (p == NULL)
			return;

		p->~T();

		node_type * tmp = reinterpret_cast<node_type *>\
			(reinterpret_cast<INT_PTR>(p) - sizeof(node_type) + sizeof(node_type *));
		tmp->next = nextnode;
		nextnode = tmp;
	}
private:
	VFX_MEMPAGE::__VMemPageList		list;
	UINT_PTR				PageSize;
	node_type *			nextnode;
};

template<typename HEAP>
struct VMemoryHeapBase
{
public:
#ifndef UNNEW
	inline void * operator new (size_t size, HEAP & heap)
	{
		return heap._Alloc();
	}
	inline void operator delete (void * p, HEAP & heap)
	{
		heap._Free(p);
	}
	inline void operator delete (void * p, size_t, HEAP & heap)
	{
		heap._Free(p);
	}
#endif
};

#define HEAP_NEW(heap)			new(heap)

#endif	//__vfxmempage_H__2002_11_1
