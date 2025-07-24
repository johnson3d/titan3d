#pragma once
#include "../BaseHead.h"
#include <thread>

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

    static std::string GetCurrentThreadName()
    {
#if defined(PLATFORM_WIN)
    // Windows 10 1607+ 方式
        using GetThreadDescriptionFunc = HRESULT(WINAPI*)(HANDLE, PWSTR*);
        static auto pGetThreadDescription =
            reinterpret_cast<GetThreadDescriptionFunc>(
                GetProcAddress(GetModuleHandle("kernel32.dll"), "GetThreadDescription"));

        if (pGetThreadDescription) {
            PWSTR wname;
            if (SUCCEEDED(pGetThreadDescription(GetCurrentThread(), &wname))) {
                char name[256];
                wcstombs(name, wname, 256);
                LocalFree(wname);
                return name;
            }
        }

#elif defined(__linux__)
        char name[16] = { 0 }; // Linux 限制为 16 字符
        if (pthread_getname_np(pthread_self(), name, 16) == 0) {
            return name;
        }

#elif defined(__APPLE__)
        char name[64] = { 0 }; // macOS 限制为 64 字符
        pthread_getname_np(pthread_self(), name, 64);
        return name;
#endif

        return "Thread-" + std::to_string(
            std::hash<std::thread::id>{}(std::this_thread::get_id()));
    }
};
