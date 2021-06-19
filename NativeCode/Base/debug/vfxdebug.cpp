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
//#include "../precompile.h"
#include "vfxdebug.h"
#include <assert.h>
#include "../string/vfxstring.h"
#include "../CoreSDK.h"
//#define new VNEW

#if !defined(PLATFORM_WIN)
void OutputDebugStringA(LPCSTR lpOutputString)
{
	printf("%s", lpOutputString);
}

inline vBOOL IsBadReadPtr(CONST VOID* lp, UINT_PTR ucb)
{
	return FALSE;
}

int MessageBoxA(
	HWND hWnd,
	LPCSTR lpText,
	LPCSTR lpCaption,
	UINT uType)
{
	return 0;
}
#endif

typedef void (WINAPI *FAssertEvent)(const char* str, const char* file, int line);

FAssertEvent GAssertEvent = nullptr;

void NoWin_Assert(const char* str, const char* file, int line)
{
	if (GAssertEvent != nullptr)
	{
		GAssertEvent(str, file, line);
	}
	else
	{
		VFX_LTRACE(ELTT_Error, "%s(%d):%s\r\n", file, line, str);
	}
}

extern const char* GetCurrentThreadName();

void Call_WriteLogString(const char* msg, ELevelTraceType level, const char* file, int line)
{
	if (EngineNS::CoreSDK::mWriteLogString)
	{
		auto tname = GetCurrentThreadName();
		EngineNS::CoreSDK::mWriteLogString(tname, msg, level, file, line);
	}
	else
	{
		OutputDebugStringA(msg); 
	}
}

VOutputConfig::VOutputConfig()
{
	
}

VOutputConfig::~VOutputConfig()
{
	
}

VOutputConfig* VOutputConfig::Ptr()
{
	static VOutputConfig Instance;
	return &Instance;
}

void VOutputConfig::AddFunc(FnOutputFunc fun)
{
	m_Funcs.push_back( fun );
}

void VOutputConfig::RemoveFunc(FnOutputFunc fun)
{
	for( std::vector<FnOutputFunc>::iterator it=m_Funcs.begin() ; it!=m_Funcs.end() ; ++it )
	{
		if( (*it)==fun )
		{
			m_Funcs.erase(it);
			return;
		}
	}
}

void VOutputConfig::RemoveAll()
{
	m_Funcs.clear();
}

void VOutputConfig::Execute( LPCSTR lpOutputString )
{
	for( std::vector<FnOutputFunc>::iterator it=m_Funcs.begin() ; it!=m_Funcs.end() ; ++it )
	{
		(*it)(lpOutputString);
	}
}

 void WINAPI _vfxLevelTraceA(const char* file, int line, ELevelTraceType level, LPCSTR lpszFormat, ...)
{
	//if(level >= __vfxdebug::__VFX_TRACE_LEVEL)
	{
		char* Tmp = (char*)malloc(4096 * 4);
		va_list pArgs;
		va_start(pArgs,lpszFormat);

		vsnprintf(Tmp,4096*4,lpszFormat,pArgs);
		//assert(nBuf < sizeof(Tmp));

		//__vfxdebug::__vfxOutputDebugStringA(Tmp);
		
		va_end(pArgs);

		Call_WriteLogString(Tmp, level, file, line);
		free(Tmp);
	}
}

 vBOOL _vfxAssertValid(LPVOID p,LPCSTR lpszFileName,UINT_PTR dwLine)
{
	if(p == NULL || ::IsBadReadPtr(p,1) != 0)
	{
		CHAR Tmp[1024];
		::sprintf(Tmp,"Assert Error At:\nfile:%s\nline:%u\nSelect Yes to debug",lpszFileName,(UINT)dwLine);
		if(::MessageBoxA(NULL,Tmp,"VectoryCore Error",MB_YESNO | MB_ICONERROR | MB_DEFBUTTON1 | MB_SYSTEMMODAL | MB_TOPMOST) == IDYES)
		{
#ifdef _DEBUG
			//_wassert(0,lpszFileName,(UINT)dwLine);
#endif
			return TRUE;
		}
		return FALSE;
	}
	return FALSE;
}
