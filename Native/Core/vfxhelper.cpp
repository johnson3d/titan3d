// vfxhelper.cpp
// 
// VictoryCore Code
// ¸¨Öúº¯Êý¿â
//
// Author : johnson3d
// More author :
// Create time : 2002-6-13
// Modify time :
//-----------------------------------------------------------------------------
#include "precompile.h"
#include "vfxhelper.h"

//#ifdef ANDROID
//#include "../../../external/external.Android/support/include/iconv.h"
//#elif defined(IOS)
//#include <locale.h>
//#endif

//#define IOS_HACK

//this is purely for a shit game called TianDaKeng to use;
#ifdef IOS_HACK
    #include "dlfcn.h"
#endif

#define new VNEW

#undef _MAX_PATH
#define _MAX_PATH		1024
#undef _MAX_FNAME
#define _MAX_FNAME		1024



typedef int (WINAPI *FTestCppCallDelegate)(void* item, int type);

extern "C"
{
	// void PerfCounter_QueryPerformanceFrequency(INT64* freq)
	//{
	//	::QueryPerformanceFrequency((LARGE_INTEGER *)&(*freq));
	//}
	// void PerfCounter_QueryPerformanceCounter(INT64* tick)
	//{
	//	::QueryPerformanceCounter((LARGE_INTEGER *)&(*tick));
	//}
	 INT64 HighPrecision_GetTickCount()
	{//Ç§·ÖÖ®Ò»ºÁÃë
#if defined(PLATFORM_WIN)
		static INT64 freq = 0;
		if(freq==0)
		{
			INT64 seg = 0;
			for(int i=0;i<100;i++)
			{
				INT64 temp;
				::QueryPerformanceFrequency((LARGE_INTEGER *)&temp);
				seg += temp;
			}
			freq = seg/(1000000*100);
		}
		INT64 now;
		::QueryPerformanceCounter((LARGE_INTEGER *)&now);

		return now/freq;
#elif defined(PLATFORM_DROID)
		struct timespec time1 = { 0, 0 };
		clock_gettime(CLOCK_MONOTONIC, &time1);
		INT64 ret = (INT64)(time1.tv_sec) * 1000 * 1000;
		ret += time1.tv_nsec / 1000;
		return ret;
#elif defined(PLATFORM_IOS)
        struct timeval tv={0,0};
        gettimeofday(&tv,NULL);
		INT64 ret = (INT64)(tv.tv_sec) * 1000 * 1000;
		ret  += tv.tv_usec;
		return ret;
#endif
		return 0;
	}

	 INT64 vfxGetTickCount()
	{
		//auto time = clock();
		auto time = HighPrecision_GetTickCount() / 1000;
		return time;
	}

	 INT64 Test_CppCallDelegate(FTestCppCallDelegate cb, int count)
	{
		auto t1 = HighPrecision_GetTickCount();
		for (int i = 0; i < count; i++)
		{
			cb(NULL, i);
		}
		auto t2 = HighPrecision_GetTickCount();
		return t2 - t1;
	}

	 int Test_CSharpCallCpp(int a, float b)
	{
		return 1;
	}
    
    
#ifdef IOS_HACK
     int GetIOSNetSignalLevel()
    {
        void* LibHandle = dlopen("/System/Library/Frameworks/CoreTelephony.framework/CoreTelephony",RTLD_LAZY);
        int (*CTGetSignalStrength)();
        CTGetSignalStrength = (int(*)())dlsym(LibHandle, "CTGetSignalStrength");
        
#ifdef DEBUG
        if(CTGetSignalStrength == NULL)
        {
            ASSERT(FALSE);
        }
#endif
        
        int SignalLevel = CTGetSignalStrength();
        dlclose(LibHandle);
        
        return SignalLevel;
    }
    
#endif
};





