// dllmain.cpp : 定义 DLL 应用程序的入口点。
#include "pch.h"

extern "C" HRESULT __stdcall DllGetClassObject(REFCLSID rclsid, REFIID riid, void** ppv);

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
        /*case DLL_PROCESS_ATTACH:
            pthread_win32_process_attach_np();
            break;
        case DLL_THREAD_ATTACH:
            pthread_win32_process_attach_np();
            break;
        case DLL_THREAD_DETACH:
            pthread_win32_process_detach_np();
            break;
        case DLL_PROCESS_DETACH:
            pthread_win32_process_detach_np();
            break;*/
    default:
        break;
    }
    return TRUE;
}

