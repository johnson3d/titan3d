#include "vfxthread.h"

#if defined(PLATFORM_DROID)
#include <sys/prctl.h>
#elif defined(PLATFORM_IOS)
//#include <sys/prctl.h>
#elif defined(PLATFORM_WIN)
#pragma comment(lib,"pthreadVC2.lib")
#endif

#define new VNEW

void* GRenderThreadId = NULL;
void* GLogicThreadId = NULL;
void* GLoadThreadId = NULL;
void* GraphicsThreadId = nullptr;

bool vfxThread::IsGraphicsThread()
{
	return GetCurrentThreadId() == GraphicsThreadId;
}

const char* GetCurrentThreadName()
{
	auto thread = vfxThread::GetCurrentThreadId();
	if (thread == GRenderThreadId)
		return "RThread";
	else if (thread == GLogicThreadId)
		return "LThread";
	else if (thread == GLoadThreadId)
		return "IOThread";
	else
		return "UnkownThread";
}

void vfxThread::Start(LPCSTR name, size_t stack, FThreadStarter fun, void* parameter)
{
	pthread_attr_t attr;
	pthread_attr_init(&attr);
	pthread_attr_setstacksize(&attr, stack);
	//attr.stack_size = stack;
	pthread_create(&mThread, &attr, fun, parameter);
}

void vfxThread::Terminate()
{
	//pthread_cancel(mThread);
}

void vfxThread::SleepMe(unsigned int time)
{
	Sleep(time);
}

extern "C"
{
	VFX_API void Thread_SetName(LPCSTR name)
	{
#if defined(PLATFORM_WIN)
		//::GetThread
#elif defined(PLATFORM_IOS)
#else
		//prctl(PR_SET_NAME, name);
#endif
	}

	VFX_API void Thread_StartLogicThread()
	{
		GLogicThreadId = vfxThread::GetCurrentThreadId();
	}
	VFX_API void Thread_StartRHIThread()
	{
		GRenderThreadId = vfxThread::GetCurrentThreadId();
	}
	VFX_API void Thread_StartIOThread()
	{
		GLoadThreadId = vfxThread::GetCurrentThreadId();
	}
}
