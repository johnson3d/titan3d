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

#if defined(PLATFORM_DROID)
#include <new>
#define GNU_NEW
#elif defined(PLATFORM_IOS)
#include <new>
#define APPLE_NEW
#elif defined(PLATFORM_WIN)
#include <new>
#define MS_NEW
#endif

#include <vector>

#if defined(PLATFORM_WIN)
#  define VGLIBCXX_NOEXCEPT noexcept
#  define VGLIBCXX_USE_NOEXCEPT noexcept
#  define VGLIBCXX_THROW(_EXC)
#else
#  define VGLIBCXX_NOEXCEPT
#  define VGLIBCXX_USE_NOEXCEPT _NOEXCEPT
#  define VGLIBCXX_THROW(_EXC)
#endif

#if defined(NONEW)
#undef GNU_NEW
#undef MS_NEW
#endif

#if defined(GNU_NEW)
void* operator new(std::size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc);
void* operator new(std::size_t size) VGLIBCXX_THROW(std::bad_alloc);
void* operator new[](std::size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc);
void* operator new[](std::size_t size) VGLIBCXX_THROW(std::bad_alloc);
void operator delete(void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p)VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p, std::size_t) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p) VGLIBCXX_USE_NOEXCEPT;
void* operator new(std::size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void* operator new[](std::size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;

void* operator new(std::size_t size, void* __p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete  (void*, void*, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
//void* operator new(std::size_t, void* __p) _GLIBCXX_USE_NOEXCEPT;
//inline void* operator new[](std::size_t, void* __p) _GLIBCXX_USE_NOEXCEPT;
//
//inline void operator delete  (void*, void*) _GLIBCXX_USE_NOEXCEPT;
//inline void operator delete[](void*, void*) _GLIBCXX_USE_NOEXCEPT;
#elif defined(APPLE_NEW)
void* operator new(std::size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc);
void* operator new(std::size_t size) VGLIBCXX_THROW(std::bad_alloc);
void* operator new[](std::size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc);
void* operator new[](std::size_t size) VGLIBCXX_THROW(std::bad_alloc);
void operator delete(void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p)VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p, std::size_t) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p) VGLIBCXX_USE_NOEXCEPT;
void* operator new(std::size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void* operator new[](std::size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;

void* operator new(std::size_t size, void* __p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete  (void*, void*, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
#elif defined(MS_NEW)
void* operator new(size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc);
void* operator new(size_t size) VGLIBCXX_THROW(std::bad_alloc);
void* operator new[](size_t size, const char* file, int line) VGLIBCXX_THROW(std::bad_alloc);
void* operator new[](size_t size) VGLIBCXX_THROW(std::bad_alloc);
void operator delete(void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p) VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p, size_t) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p) VGLIBCXX_USE_NOEXCEPT;
void* operator new(size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void* operator new[](size_t size, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void operator delete(void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;
void operator delete[](void* p, const std::nothrow_t&) VGLIBCXX_USE_NOEXCEPT;

void* operator new(std::size_t size, void* __p, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
void operator delete  (void*, void*, const char* file, int line) VGLIBCXX_USE_NOEXCEPT;
#endif

#if defined(NONEW)
	#define VNEW new
#else
	#define VNEW new(__FILE__,__LINE__)
#endif

