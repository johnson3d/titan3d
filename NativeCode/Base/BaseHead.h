#pragma once

//#if _HAS_CXX20 
	#define _SILENCE_CXX20_CISO646_REMOVED_WARNING
	//#define _SILENCE_ALL_CXX20_DEPRECATION_WARNINGS
//#endif

#if defined(PLATFORM_WIN)

#define HasModule_Windows
#define HasModule_ImGui
#define HasModule_Base
#define HasModule_Math
#define HasModule_D3D11
#define HasModule_OpenGL
#define HasModule_RHI
#define HasModule_VirtualDevice
#define HasModule_CrossShaderCompiler
#define HasModule_Image_Shared
#define HasModule_Pipeline
#define HasModule_Mesh
#define HasModule_FBX
#define HasModule_TextConverter
#define HasModule_Animation
#define HasModule_TcpClient
#define HasModule_TcpServer
#define HasModule_PhysicsCore
#define HasModule_MeshSimplify
#define HasModule_Terrain
#define HasModule_RenderDoc
#define HasModule_Particle
#define HasModule_MathLib
#define HasModule_NextRHI
#define HasModule_Dx11
#define HasModule_Dx12
#define HasModule_Vulkan
#define HasModule_NullDevice
#define HasModule_Canvas
#define HasModule_TextFont
#define HasModule_TextureCompress
#define HasModule_EtcLib
#define HasModule_GpuDump
#define HasModule_ImageDecoder
#define HasModule_PythonRuntime
#define HasModule_Quark
#define HasModule_NxPhysics

#include <WinSock2.h>
#include <mswsock.h>
#include <windows.h>
#include <shlwapi.h>
#include <direct.h>
#include <io.h>

#define HAVE_STRUCT_TIMESPEC
#define _TIMESPEC_DEFINED
#include "../../3rd/native/pthread/include/pthread.h"

#pragma warning(disable:4819)

#elif defined(PLATFORM_DROID)

#define HasModule_Android
#define HasModule_ImGui
#define HasModule_Base
#define HasModule_Math
#define HasModule_OpenGL
#define HasModule_RHI
#define HasModule_CrossShaderCompiler
#define HasModule_Image_Shared
#define HasModule_Pipeline
#define HasModule_Mesh
#define HasModule_TextConverter
#define HasModule_Animation
#define HasModule_TcpClient
#define HasModule_PhysicsCore
#define HasModule_MeshSimplify
#define HasModule_Terrain
#define HasModule_Particle
#define HasModule_MathLib
#define HasModule_NextRHI
#define HasModule_Vulkan
#define HasModule_Canvas
#define HasModule_TextFont

#include <jni.h>
#include <errno.h>
#include <android/log.h>
#include <android/asset_manager.h>
#include <android/native_window.h>
#include <android/native_window_jni.h>
#include <pthread.h>
#include <sys/stat.h>

#include "vfxtypes_nw.h"
#elif defined(PLATFORM_IOS)
#include <objc/objc.h>
#include <pthread.h>
//#include <iconv.h>
#include <sys/time.h>
#include <sys/mman.h>

#include "vfxtypes_nw.h"
#endif

#include <vector>
#include <list>
#include <map>
#include <atomic>
#include <queue>
#include <cstdint>
#include <fstream>
#include <sstream>
#include <streambuf>
#include <cstdio>
#include <iostream>
#include <functional>
#include <cmath>
#include <algorithm>
#include <assert.h>
#include <clocale>	/// std::setlocale
#include <cstdlib>	/// std::wcstombs, std::mbstowcs

#include "CommonTypes.h"

#ifdef abs
#undef abs
#endif

#if !defined(PLATFORM_WIN)
#  define WINAPI 
#endif

#define __voffsetof(_Struct,_Member) ((size_t)(&(((_Struct*)nullptr)->_Member)))
#define __vstatic_cast(_Pointor,_Struct,_Member) ((_Struct*)(((size_t)(_Pointor))-__voffsetof(_Struct,_Member)))
#	define vT(x)	x

#ifndef _countof
#	define _countof(array)	(sizeof(array) / sizeof(array[0]))
#endif

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

//#define NEW_INHEAD new(__FILE__, __LINE__)
#define NEW_INHEAD new