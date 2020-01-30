#pragma once

#if defined(PLATFORM_WIN)
#include <WinSock2.h>
#include <mswsock.h>
#include <windows.h>
#include <shlwapi.h>
#include <direct.h>
#include <io.h>

#pragma warning(disable:4819)
//#include "3rd/iconv/iconv.h"

#define HAVE_STRUCT_TIMESPEC
#define _TIMESPEC_DEFINED
#include "3rd/pthread/include/pthread.h"

#elif defined(PLATFORM_DROID)
#include <jni.h>
#include <errno.h>
#include <android/log.h>
#include <android/asset_manager.h>
#include <android/native_window.h>
#include <android/native_window_jni.h>
#include <pthread.h>
#include <sys/stat.h>

//#include "3rd/iconv/iconv.h"
#include "vfxtypes_nw.h"
#elif defined(PLATFORM_IOS)
#include <objc/objc.h>
#include <pthread.h>
//#include <iconv.h>
#include <sys/time.h>
#include <sys/mman.h>

#include "vfxtypes_nw.h"
#endif

#ifdef abs
#undef abs
#endif

typedef int	vBOOL;

#if !defined(PLATFORM_WIN)
#  define WINAPI 
#endif

#define __voffsetof(_Struct,_Member) ((size_t)(&(((_Struct*)nullptr)->_Member)))
#define __vstatic_cast(_Pointor,_Struct,_Member) ((_Struct*)(((size_t)(_Pointor))-__voffsetof(_Struct,_Member)))
#	define vT(x)	x

#ifndef _countof
#	define _countof(array)	(sizeof(array) / sizeof(array[0]))
#endif


#include <vector>
#include <list>
#include <map>
#include <atomic>
#include <cstdint>
#include <fstream>
#include <sstream>
#include <streambuf>
#include <cstdio>
#include <iostream>
#include <functional>
#include <cmath>
#include <assert.h>

#include "Core/debug/vfxnew.h"

#include "IUnknown.h"

#define ENGINENS_BUFFER_SIZE 65536

inline void _vfxTraceA(LPCSTR lpszFormat, ...)
{
	va_list pArgs;
	va_start(pArgs, lpszFormat);

	CHAR Tmp[ENGINENS_BUFFER_SIZE];

	/*int nBuf = */::vsnprintf(Tmp, ENGINENS_BUFFER_SIZE, lpszFormat, pArgs);
	va_end(pArgs);

#if defined(PLATFORM_WIN)
	::OutputDebugStringA(Tmp);
#endif
}

#undef VFX_API
#if defined(PLATFORM_WIN) && defined(AS_DLLMODULE)
#	if !defined(VFX_EXPORTS)
#		define VFX_API			__declspec(dllimport)
#	else
#		define VFX_API			__declspec(dllexport)
#	endif
#elif defined(PLATFORM_DROID)
#		define VFX_API
#elif defined(PLATFORM_IOS)
#		define VFX_API
#else
#		define VFX_API
#endif