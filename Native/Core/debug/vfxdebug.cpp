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
//#define new VNEW

bool GEnableCheckThread = false;
vBOOL GIsSyncTick = TRUE;

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
#if defined(PLATFORM_WIN) && !defined(NDEBUG)
		auto wstr = VStringA_Ansi2Unicode(str);
		auto wfile = VStringA_Ansi2Unicode(file);
		_wassert(wstr.c_str(), wfile.c_str(), line);
#endif
	}
}

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

typedef void(WINAPI *FWriteLogString)(const char* threadName, const char* logStr, int level, const char* file, int line);
typedef void(WINAPI *FMessageBoxString)(const char* logStr, const char* flush);
FWriteLogString _WriteLogString = nullptr;
FMessageBoxString _MessageBoxString =nullptr;

extern const char* GetCurrentThreadName();

void Call_WriteLogString(const char* msg, int level, const char* file, int line)
{
	if (_WriteLogString)
	{
		auto tname = GetCurrentThreadName();
		_WriteLogString(tname, msg, level, file, line);
	}
	else
	{
		OutputDebugStringA(msg);
	}
	/*if (_WriteLogTest)
	_WriteLogTest(msg,a);

	if (_WriteLogTest)
	_WriteLogTest(msg, a);*/
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

//void VOutputConfig::AddFuncW(FnOutputFuncW fun)
//{
//	m_FuncsW.push_back( fun );
//}
//
//void VOutputConfig::RemoveFuncW(FnOutputFuncW fun)
//{
//	for( std::vector<FnOutputFuncW>::iterator it=m_FuncsW.begin() ; it!=m_FuncsW.end() ; ++it )
//	{
//		if( (*it)==fun )
//		{
//			m_FuncsW.erase(it);
//			return;
//		}
//	}
//}
//
//void VOutputConfig::RemoveAllW()
//{
//	m_FuncsW.clear();
//}
//
//void VOutputConfig::ExecuteW( LPCSTR lpOutputString )
//{
//	for( std::vector<FnOutputFuncW>::iterator it=m_FuncsW.begin() ; it!=m_FuncsW.end() ; ++it )
//	{
//		(*it)(lpOutputString);
//	}
//}

namespace __vfxdebug
{
	//UINT_PTR __VFX_TRACE_LEVEL	= 0;
	
#if	 defined(VFX_LOGO_FILE) || defined(VFX_MULT_LOGO_FILE)
	class __vfxLogoFile
	{
		VFile_Base *	m_pFile;
	public:
		CRITICAL_SECTION	m_cs;
		inline void Lock(){::EnterCriticalSection((CRITICAL_SECTION *)&m_cs);}
		inline void Unlock(){::LeaveCriticalSection((CRITICAL_SECTION *)&m_cs);}

		void SetIO(VFile_Base * pFile)
		{
			delete m_pFile;
			m_pFile = pFile;
		}
		void OpenLogo()
		{
			ASSERT(m_pFile == NULL);

			m_pFile = new ViseFile;
			TCHAR buff[MAX_PATH];
			GetModuleFileName(GetModuleHandle(NULL),buff,MAX_PATH);
			_tcscat(buff,vT(".log"));

			vBOOL bOpened = TRUE;
#if	 defined(VFX_MULT_LOGO_FILE)
			if(!m_pFile->Open(buff,VFile::modeWrite | VFile::shareDenyNone))
#endif
				m_pFile->Open(buff,VFile::modeCreate | VFile::modeWrite | VFile::shareDenyNone);
			m_pFile->Seek(0,VFile_Base::end);
		}
		__vfxLogoFile()
		{	
			delete new char;
			m_pFile = NULL;
			::InitializeCriticalSection((CRITICAL_SECTION *)&m_cs);
		}
		~__vfxLogoFile()
		{	
			destructor();
			::DeleteCriticalSection((CRITICAL_SECTION *)&m_cs);
		}
		void destructor()
		{
			if(m_pFile != NULL)
			{
				m_pFile->Flush();
				m_pFile->Close();
				delete m_pFile;
				m_pFile = NULL;
			}
		}
		virtual UINT_PTR Write(const void* lpBuf, UINT_PTR nCount)
		{
			UINT_PTR dwRet = 0;
			__try
			{
				__try
				{
					Lock();
					if(m_pFile == NULL)
						OpenLogo();
					if(m_pFile != NULL)
						dwRet = m_pFile->Write(lpBuf,nCount);
				}
				__finally
				{
					Unlock();
				}
			}
			__except(EXCEPTION_EXECUTE_HANDLER){
				dwRet = 0;
			}
			return dwRet;
		}
		virtual vBOOL Flush()
		{
			vBOOL bRet = FALSE;
			__try
			{
				__try
				{
					Lock();
					if(m_pFile != NULL)
						bRet = m_pFile->Flush();
				}
				__finally
				{
					Unlock();
				}
			}
			__except(EXCEPTION_EXECUTE_HANDLER){
				bRet = FALSE;
			}

			return bRet;
		}
		
	};

	__vfxLogoFile & __vfxgetlogofile()
	{
		static __vfxLogoFile	__logofile;
		return __logofile;
	}
 
#endif

	void __vfxDestructorOutputDebug()
	{
#if	 defined(VFX_LOGO_FILE) || defined(VFX_MULT_LOGO_FILE)
		__vfxgetlogofile().destructor();
#endif
	}
	
	void __vfxOutputDebugStringA(LPCSTR lpOutputString)
	{
		OutputDebugStringA(lpOutputString);
		VOutputConfig::Ptr()->Execute( lpOutputString );

	#if	 defined(VFX_LOGO_FILE) || defined(VFX_MULT_LOGO_FILE)
		__try
		{
			time_t tNow = time(NULL);
			if(g_OutputDebugTime != tNow)
			{
				g_OutputDebugTime = tNow;
				tm * tmNow = localtime(&tNow);
				sprintf(g_szOutputDebugTime,"[%.4d-%.2d-%.2d %.2d:%.2d:%.2d]",
					tmNow->tm_year+1900,tmNow->tm_mon+1,tmNow->tm_mday,
					tmNow->tm_hour,tmNow->tm_min,tmNow->tm_sec);
			}
			__vfxgetlogofile().Lock();
			__vfxgetlogofile().Write(g_szOutputDebugTime,21);

			size_t n = strlen(lpOutputString);
			if(n > 1 && lpOutputString[n - 1] == '\n' && lpOutputString[n - 2] != '\r')
			{
				__vfxgetlogofile().Write(lpOutputString,n - 1);
				__vfxgetlogofile().Write("\r\n",2);
			}
			else
				__vfxgetlogofile().Write(lpOutputString,n);
			//__vfxgetlogofile().Flush();
		}
		__finally
		{
			__vfxgetlogofile().Unlock();
		}
	#endif
	}

	//void __vfxOutputDebugStringW(LPCSTR lpOutputString)
	//{
	//	::OutputDebugStringW(lpOutputString);
	//	VOutputConfig::Ptr()->ExecuteW( lpOutputString );

	//#if	 defined(VFX_LOGO_FILE) || defined(VFX_MULT_LOGO_FILE)
	//	__try
	//	{
	//		__vfxgetlogofile().Lock();
	//		size_t n = wcslen(lpOutputString);
	//		if(n > 1 && lpOutputString[n - 1] == L'\n' && lpOutputString[n - 2] != L'\r')
	//		{
	//			__vfxgetlogofile().Write(lpOutputString,n - 1);
	//			__vfxgetlogofile().Write(L"\r\n",4);
	//		}
	//		else
	//			__vfxgetlogofile().Write(lpOutputString,n);
	//		//__vfxgetlogofile().Flush();
	//	}
	//	__finally
	//	{
	//		__vfxgetlogofile().Unlock();
	//	}
	//#endif
	//}
}

 void WINAPI _vfxLevelTraceA(const char* file, int line, UINT level, LPCSTR lpszFormat, ...)
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

#ifdef ANDROID
#	pragma clang diagnostic ignored "-Wformat-security"
		//__android_log_print(ANDROID_LOG_INFO, "Core.Android", Tmp);
#elif IOS

      //  printf("%s\n",pArgs);
        //printf(Tmp);
		//__android_log_print(ANDROID_LOG_INFO, "Core.Android", Tmp);
#endif
		Call_WriteLogString(Tmp, level, file, line);
		free(Tmp);
	}
}

// void WINAPI _vfxTraceW(LPCSTR lpszFormat, ...)
//{
//#if _DEBUG
//	va_list pArgs;
//	va_start(pArgs,lpszFormat);
//
//	CHAR Tmp[1024];
//	CHAR ttmp[1024];
//
//	int nBuf = ::_vsnwprintf(ttmp,1024,lpszFormat,pArgs);
//	ASSERT(nBuf < sizeof(ttmp));
//
//	WideCharToMultiByte( CP_ACP, 0, ttmp, -1, Tmp, 1024, NULL, NULL );
//#if	 defined(VFX_LOGO_FILE) || defined(VFX_MULT_LOGO_FILE)
//	if(IsDebuggerPresent())
//		::OutputDebugStringA(Tmp);
//	else
//		__vfxdebug::__vfxOutputDebugStringA(Tmp);
//#else
//	::OutputDebugStringA(Tmp);
//#endif
//	va_end(pArgs);
//#endif
//}
//
// void WINAPI _vfxLevelTraceW(UINT_PTR level, LPCSTR lpszFormat, ...)
//{
//	/*const vIID	aaa = 0x6cfdc8104192e636;
//	auto sss = VStringA_FormatV("%lld", aaa);*/
//	CHAR* Tmp = new CHAR[1024];
//	CHAR* Tmp2 = new CHAR[1024];
//	memset(Tmp, 0, 1024);
//	memset(Tmp2, 0, 1024);
//	_vfxUnicode2Ansi(lpszFormat, Tmp, -1);
//	
//	if(level >= __vfxdebug::__VFX_TRACE_LEVEL)
//	{	
//		va_list pArgs;
//		va_start(pArgs,lpszFormat);
//		vsnprintf(Tmp2, 1024, Tmp, pArgs);
//		va_end(pArgs);
//
//		Call_WriteLogString(Tmp2,0);
//	}
//	delete[] Tmp;
//	delete[] Tmp2;
//}

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

 vBOOL _vfxAssertThis(LPVOID p,LPCSTR lpszFileName,UINT_PTR dwLine)
{
	if(p == NULL || ::IsBadWritePtr(p,4) != 0)
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

extern "C"
{
	VFX_API void Debug_SetWriteLogStringCallback(FWriteLogString wls)
	{
		_WriteLogString = wls;
	}
	VFX_API void Debug_UnSetWriteLogStringCallback()
	{
		_WriteLogString = nullptr;
	}
    
	VFX_API void Debug_SetMessageBoxCallback(FMessageBoxString mbs)
    {
        _MessageBoxString = mbs;
    }
	VFX_API void Debug_UnSetMessageBoxCallback()
    {
        _MessageBoxString = nullptr;
    }

	VFX_API void Debug_SetSyncTickState(vBOOL isSync)
	{
		GIsSyncTick = isSync;
	}

	VFX_API void Debug_SetAssertEvent(FAssertEvent evt)
	{
		GAssertEvent = evt;
	}
}
