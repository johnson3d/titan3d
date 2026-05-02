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

static thread_local bool has_set_thread_name = false;
static thread_local char current_thread_name[256]{};
const char* vfxThread::GetCurrentThreadName()
{
	if (has_set_thread_name == true)
		return current_thread_name;
	has_set_thread_name = true;
#if defined(PLATFORM_WIN)
	// Windows 10 1607+ 方式
	using GetThreadDescriptionFunc = HRESULT(WINAPI*)(HANDLE, PWSTR*);
	static auto pGetThreadDescription =
		reinterpret_cast<GetThreadDescriptionFunc>(
			GetProcAddress(GetModuleHandle("kernel32.dll"), "GetThreadDescription"));

	if (pGetThreadDescription) {
		PWSTR wname;
		if (SUCCEEDED(pGetThreadDescription(GetCurrentThread(), &wname))) {
			wcstombs(current_thread_name, wname, 256);
			LocalFree(wname);
			return current_thread_name;
		}
	}

#elif defined(__linux__)
	if (pthread_getname_np(pthread_self(), current_thread_name, 16) == 0) {
		return current_thread_name;
	}

#elif defined(__APPLE__)
	pthread_getname_np(pthread_self(), current_thread_name, 64);
	return current_thread_name;
#endif

	auto id = std::hash<std::thread::id>{}(std::this_thread::get_id());
	sprintf_s(current_thread_name, 256, "Thread-%zu", id);
	return current_thread_name;
}

//const char* GetCurrentThreadName()
//{
//	auto thread = vfxThread::GetCurrentThreadId();
//	if (thread == GRenderThreadId)
//		return "RThread";
//	else if (thread == GLogicThreadId)
//		return "LThread";
//	else if (thread == GLoadThreadId)
//		return "IOThread";
//	else
//		return "UnkownThread";
//}

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

