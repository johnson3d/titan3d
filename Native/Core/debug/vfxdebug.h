// vfxdebug.cpp
// 
// VictoryCore Code
// debuging and memory manager
//
// Author : johnson3d
// More author :
// Create time : 2002-6-13
// Modify time : 2002-6-23
//-----------------------------------------------------------------------------

#pragma once

#include "../../BaseHead.h"

#if defined(UNNEW)
#	undef VFX_DEBUG_MEMORY		
#elif defined(DEBUG_MEMORY)
#	define VFX_DEBUG_MEMORY		
#else
#	if defined(_DEBUG)
#		define VFX_DEBUG_MEMORY		
#	else
#		undef VFX_DEBUG_MEMORY		
#	endif
#endif

void NoWin_Assert(const char* str, const char* file, int line);

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wundefined-bool-conversion"
#endif

# define ASSERT(d) if(d){  } else {NoWin_Assert(#d,__FILE__,__LINE__);}

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif

extern bool GEnableCheckThread;
extern vBOOL GIsSyncTick;

#	if _DEBUG
#		if !defined(TRACE)
#			define TRACE					_vfxTrace
#		endif
#		if !defined(VERIFY)
#			define VERIFY(f)				ASSERT(f)
#		endif
#	else   // _DEBUG
#		if !defined(VERIFY)
#			define VERIFY(f)				((void)(f))
#		endif
#		if !defined(TRACE)
#			define TRACE					_vfxTrace
#		endif
#	endif // !_DEBUG


 void * _vfxMemoryNew(size_t nSize,const char * file,size_t line);
 void _vfxMemoryDelete(void * memory,const char * file,size_t line);

#include "vfxnew.h"

 void _vfxLevelTraceA(const char* file, int line, UINT level, const char* lpszFormat, ...);
// void __cdecl _vfxTraceW(const wchar_t* lpszFormat, ...);
// void __cdecl _vfxLevelTraceW(UINT_PTR level, const wchar_t* lpszFormat, ...);

enum ELevelTraceType
{
	ELTT_info = 0,
	ELTT_Warning,
	ELTT_Error,

	ELTT_Default = 3,
	
	ELTT_Graphics,
	ELTT_Network,
	ELTT_SceneGraph,
	ELTT_Memory,
	ELTT_Media,
	ELTT_Physics,
	ELTT_Resource,
	ELTT_SystemCore,
	ELTT_VR,
	ELTT_Input,
};

#	define _vfxTrace		_vfxTraceA
#	define _vfxLevelTrace(file,line,ttype,lpszFormat,...)	_vfxLevelTraceA(file,line,ttype,lpszFormat,__VA_ARGS__)

#undef VFX_TRACE
	#define VFX_TRACE						_vfxTrace

#undef VFX_LTRACE
	#define VFX_LTRACE(ttype,...)				_vfxLevelTraceA(__FILE__,__LINE__,ttype,__VA_ARGS__)

#undef VFX_UNUSED
	#define VFX_UNUSED(x)

#ifdef _DEBUG
	#ifndef INSURE
	#define INSURE(express_bool) {if(!(express_bool)){VFX_LTRACE(ELevelTraceType::ELTT_Error, "This is an Engine Error!");ASSERT(FALSE); }}
	//#define INSURE(express_bool, ...) {if(!(express_bool)){VFX_LTRACE(ELevelTraceType::ELTT_Error, ...);ASSERT(FALSE); }}
	#endif
#else
	#ifndef INSURE
	#define INSURE(express_bool) 
	//#define INSURE(express_bool, ...) 
	#endif
#endif

namespace VMem
{
	struct VMemAllocator
	{
		static void * Malloc(size_t size)
		{
			return _vfxMemoryNew(size, __FILE__, __LINE__);
		}
		static void Free(void* p)
		{
			_vfxMemoryDelete(p, NULL, 0);
		}
	};

	template <class T, class M> 
	class malloc_allocator
	{
	public:
		typedef T					value_type;
		typedef value_type*			pointer;
		typedef const value_type*	const_pointer;
		typedef value_type&			reference;
		typedef const value_type&	const_reference;
		typedef std::size_t			size_type;
		typedef std::ptrdiff_t		difference_type;

		template <class U>
		struct rebind { typedef malloc_allocator<U, M> other; };
		malloc_allocator() {}
		malloc_allocator(const malloc_allocator&) {}
		template <class U>
		malloc_allocator(const malloc_allocator<U, M>&) {}
		~malloc_allocator() {}

		pointer address(reference x) const {
			return &x;
		}
		const_pointer address(const_reference x) const {
			return x;
		}

		pointer allocate(size_type n, const_pointer = 0) {
			//void* p = std::malloc(n * sizeof(T));
			void* p = M::Malloc(n * sizeof(T));
			if (!p)
			{
				//throw std::bad_alloc();
				return nullptr;
			}
			return static_cast<pointer>(p);
		}
		void deallocate(pointer p, size_type size) {
			//std::free(p);
			M::Free(p);
		}
		size_type max_size() const {
			return static_cast<size_type>(-1) / sizeof(T);
		}
		void construct(pointer p, const value_type& x) {
			::new(p,__FILE__,__LINE__) value_type(x);
		}
		void destroy(pointer p) {
			p->~value_type();
		}
	private:
		void operator = (const malloc_allocator&);
	/*public:
		bool operator==(const malloc_allocator<T, M>& r)
		{
			return true;
		}
		bool operator!=(const malloc_allocator<T, M>&) {
			return false;
		}*/
	};

	template <class T, class M>
	inline bool operator==(
		const malloc_allocator<T, M>& _Left,
		const malloc_allocator<T, M>& _Right) VGLIBCXX_NOEXCEPT
	{	// test errors for equality
		return true;
	}

	template <class T, class M>
	inline bool operator!=(
		const malloc_allocator<T, M>& _Left,
		const malloc_allocator<T, M>& _Right) VGLIBCXX_NOEXCEPT
	{	// test errors for equality
		return false;
	}

	template<class M> 
	class malloc_allocator<void,M>
	{
		typedef void		value_type;
		typedef void*		pointer;
		typedef const void*	const_pointer;
		template <class U>
		struct rebind
		{
			typedef malloc_allocator<U,M> other;
		};
	};

	#define V_STL_USE_ALLOCATOR

	template<typename K, typename V, class M= VMemAllocator>
#if defined(V_STL_USE_ALLOCATOR)
	class map : public std::map<K, V, std::less<K>, malloc_allocator<std::pair<const K, V>, M> >
#else
	class map : public std::map<K, V, std::less<K>>
#endif
	{

	};

	template<typename V, class M = VMemAllocator>
#if defined(V_STL_USE_ALLOCATOR)
	class vector : public std::vector<V, malloc_allocator<V, M> >
#else
	class vector : public std::vector<V>
#endif
	{

	};

	template<typename V, class M = VMemAllocator>
#if defined(V_STL_USE_ALLOCATOR)
	class list : public std::list<V, malloc_allocator<V, M> >
#else
	class list : public std::list<V>
#endif
	{

	};

	/*template<class T>
	inline bool operator==(const malloc_allocator<T>& l, const malloc_allocator<T>& r) {
	return true;
	}
	template<class T>
	inline bool operator!=(const malloc_allocator<T>&, const malloc_allocator<T>&) {
	return false;
	}*/

}

class VOutputConfig
{
public:
	typedef void(*FnOutputFunc)(const char* lpOutputString);
	typedef void(*FnOutputFuncW)(const wchar_t* lpOutputString);
protected:
	VOutputConfig();
	~VOutputConfig();

	std::vector<FnOutputFunc>		m_Funcs;
	std::vector<FnOutputFuncW>		m_FuncsW;
public:
	static  VOutputConfig* Ptr();

	 void AddFunc(FnOutputFunc fun);
	 void RemoveFunc(FnOutputFunc fun);
	 void RemoveAll();
	 void Execute(const char* lpOutputString);

	// void AddFuncW(FnOutputFuncW fun);
	// void RemoveFuncW(FnOutputFuncW fun);
	// void RemoveAllW();
	// void ExecuteW( const wchar_t* lpOutputString );
};
