#include "imgui_binding.h"

#if defined(PLATFORM_WIN)
#include "imgui_impl_win32.cpp"
#endif

NS_BEGIN

StructImpl(ImVec2)
StructImpl(ImVec4)

StructImpl(items_getter)
StructImpl(values_getter)
StructImpl(alloc_func)
StructImpl(free_func)
StructImpl(ImGuiSizeCallback)
StructImpl(ImGuiInputTextCallback)

StructImpl(Renderer_CreateWindow)
StructImpl(Renderer_SetWindowSize)
StructImpl(Renderer_RenderWindow)

//AuxRttiStruct<void __cdecl(struct ImGuiSizeCallbackData *)> AuxRttiStruct<void __cdecl(struct ImGuiSizeCallbackData *)>::Instance;
//AuxRttiStruct<bool __cdecl(void *, int, char const * *)> AuxRttiStruct<bool __cdecl(void *, int, char const * *)>::Instance;
//AuxRttiStruct<int __cdecl(struct ImGuiInputTextCallbackData *)> AuxRttiStruct<int __cdecl(struct ImGuiInputTextCallbackData *)>::Instance;
//AuxRttiStruct<float __cdecl(void *, int)> AuxRttiStruct<float __cdecl(void *, int)>::Instance;
//AuxRttiStruct<void * __cdecl(unsigned __int64, void *)> AuxRttiStruct<void * __cdecl(unsigned __int64, void *)>::Instance;
//AuxRttiStruct<void __cdecl(void *, void *)> AuxRttiStruct<void __cdecl(void *, void *)>::Instance;

void	ImGuiAPI::ImGui_NativeWindow_EnableDpiAwareness()
{
#if defined(PLATFORM_WIN)
	ImGui_ImplWin32_EnableDpiAwareness();
#endif
}

bool     ImGuiAPI::ImGui_NativeWindow_Init(void* hwnd)
{
#if defined(PLATFORM_WIN)
	return ImGui_ImplWin32_Init(hwnd);
#else
	return false;
#endif
}
void     ImGuiAPI::ImGui_NativeWindow_Shutdown()
{
#if defined(PLATFORM_WIN)
	ImGui_ImplWin32_Shutdown();
#endif
}
void     ImGuiAPI::ImGui_NativeWindow_NewFrame()
{
#if defined(PLATFORM_WIN)
	ImGui_ImplWin32_NewFrame();
#endif
}

NS_END

