#pragma once
#include "../../BaseHead.h"

#if defined WIN
#define PT_CDECL __cdecl
#else
#define PT_CDECL 
#endif

typedef void*(PT_CDECL * FThreadStarter)(void* lpThreadParameter);

extern void* GRenderThreadId;
extern void* GLogicThreadId;
extern void* GLoadThreadId;
extern void* GraphicsThreadId;

class vfxThread
{
public:
	pthread_t 		mThread;

	vfxThread()
	{
#if defined(PLATFORM_WIN)
		mThread.p = NULL;
		mThread.x = 0;
#else
		mThread = 0;
#endif
	}
	~vfxThread()
	{
		Terminate();
	}

	void Start(LPCSTR name, size_t stack, FThreadStarter fun, void* parameter);
	void Terminate();
	void SleepMe(unsigned int time);

	static bool IsGraphicsThread();

	static void* GetCurrentThreadId()
	{
#if defined(PLATFORM_WIN)
		return (void*)pthread_self().p;
#else
		return (void*)pthread_self();
#endif
	}
};
