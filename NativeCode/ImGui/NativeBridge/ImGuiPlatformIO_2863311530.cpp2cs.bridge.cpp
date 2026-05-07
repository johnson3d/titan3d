//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiPlatformIO_Visitor
	{
		static inline ImGuiPlatformIO* CreateInstance()
		{
			return new ImGuiPlatformIO();
		}
		static inline void* FieldGet__Platform_GetClipboardTextFn(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_GetClipboardTextFn;
		}
		static inline void FieldSet__Platform_GetClipboardTextFn(ImGuiPlatformIO* self, char* (*Platform_GetClipboardTextFn)())
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_GetClipboardTextFn) = (void*)Platform_GetClipboardTextFn;
		}
		static inline void* FieldGet__Platform_SetClipboardTextFn(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_SetClipboardTextFn;
		}
		static inline void FieldSet__Platform_SetClipboardTextFn(ImGuiPlatformIO* self, void (*Platform_SetClipboardTextFn)())
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_SetClipboardTextFn) = (void*)Platform_SetClipboardTextFn;
		}
		static inline void* FieldGet__Platform_ClipboardUserData(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_ClipboardUserData;
		}
		static inline void FieldSet__Platform_ClipboardUserData(ImGuiPlatformIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Platform_ClipboardUserData = value;
		}
		static inline void* FieldGet__Platform_OpenInShellFn(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_OpenInShellFn;
		}
		static inline void FieldSet__Platform_OpenInShellFn(ImGuiPlatformIO* self, bool (*Platform_OpenInShellFn)())
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_OpenInShellFn) = (void*)Platform_OpenInShellFn;
		}
		static inline void* FieldGet__Platform_OpenInShellUserData(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_OpenInShellUserData;
		}
		static inline void FieldSet__Platform_OpenInShellUserData(ImGuiPlatformIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Platform_OpenInShellUserData = value;
		}
		static inline void* FieldGet__Platform_SetImeDataFn(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_SetImeDataFn;
		}
		static inline void FieldSet__Platform_SetImeDataFn(ImGuiPlatformIO* self, void (*Platform_SetImeDataFn)())
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_SetImeDataFn) = (void*)Platform_SetImeDataFn;
		}
		static inline void* FieldGet__Platform_ImeUserData(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_ImeUserData;
		}
		static inline void FieldSet__Platform_ImeUserData(ImGuiPlatformIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Platform_ImeUserData = value;
		}
		static inline ImWchar FieldGet__Platform_LocaleDecimalPoint(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar>();
			}
			return (ImWchar)self->Platform_LocaleDecimalPoint;
		}
		static inline void FieldSet__Platform_LocaleDecimalPoint(ImGuiPlatformIO* self, ImWchar value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Platform_LocaleDecimalPoint = value;
		}
		static inline int FieldGet__Renderer_TextureMaxWidth(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Renderer_TextureMaxWidth;
		}
		static inline void FieldSet__Renderer_TextureMaxWidth(ImGuiPlatformIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Renderer_TextureMaxWidth = value;
		}
		static inline int FieldGet__Renderer_TextureMaxHeight(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Renderer_TextureMaxHeight;
		}
		static inline void FieldSet__Renderer_TextureMaxHeight(ImGuiPlatformIO* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Renderer_TextureMaxHeight = value;
		}
		static inline void* FieldGet__Renderer_RenderState(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Renderer_RenderState;
		}
		static inline void FieldSet__Renderer_RenderState(ImGuiPlatformIO* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Renderer_RenderState = value;
		}
		static inline void* FieldGet__Platform_CreateWindow(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_CreateWindow;
		}
		static inline void FieldSet__Platform_CreateWindow(ImGuiPlatformIO* self, void (*Platform_CreateWindow)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_CreateWindow) = (void*)Platform_CreateWindow;
		}
		static inline void* FieldGet__Platform_DestroyWindow(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_DestroyWindow;
		}
		static inline void FieldSet__Platform_DestroyWindow(ImGuiPlatformIO* self, void (*Platform_DestroyWindow)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_DestroyWindow) = (void*)Platform_DestroyWindow;
		}
		static inline void* FieldGet__Platform_ShowWindow(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_ShowWindow;
		}
		static inline void FieldSet__Platform_ShowWindow(ImGuiPlatformIO* self, void (*Platform_ShowWindow)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_ShowWindow) = (void*)Platform_ShowWindow;
		}
		static inline void* FieldGet__Platform_SetWindowPos(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_SetWindowPos;
		}
		static inline void FieldSet__Platform_SetWindowPos(ImGuiPlatformIO* self, void (*Platform_SetWindowPos)(ImGuiViewport* arg0,ImVec2 arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_SetWindowPos) = (void*)Platform_SetWindowPos;
		}
		static inline void* FieldGet__Platform_GetWindowPos(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_GetWindowPos;
		}
		static inline void FieldSet__Platform_GetWindowPos(ImGuiPlatformIO* self, ImVec2 (*Platform_GetWindowPos)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_GetWindowPos) = (void*)Platform_GetWindowPos;
		}
		static inline void* FieldGet__Platform_SetWindowSize(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_SetWindowSize;
		}
		static inline void FieldSet__Platform_SetWindowSize(ImGuiPlatformIO* self, void (*Platform_SetWindowSize)(ImGuiViewport* arg0,ImVec2 arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_SetWindowSize) = (void*)Platform_SetWindowSize;
		}
		static inline void* FieldGet__Platform_GetWindowSize(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_GetWindowSize;
		}
		static inline void FieldSet__Platform_GetWindowSize(ImGuiPlatformIO* self, ImVec2 (*Platform_GetWindowSize)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_GetWindowSize) = (void*)Platform_GetWindowSize;
		}
		static inline void* FieldGet__Platform_GetWindowFramebufferScale(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_GetWindowFramebufferScale;
		}
		static inline void FieldSet__Platform_GetWindowFramebufferScale(ImGuiPlatformIO* self, ImVec2 (*Platform_GetWindowFramebufferScale)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_GetWindowFramebufferScale) = (void*)Platform_GetWindowFramebufferScale;
		}
		static inline void* FieldGet__Platform_SetWindowFocus(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_SetWindowFocus;
		}
		static inline void FieldSet__Platform_SetWindowFocus(ImGuiPlatformIO* self, void (*Platform_SetWindowFocus)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_SetWindowFocus) = (void*)Platform_SetWindowFocus;
		}
		static inline void* FieldGet__Platform_GetWindowFocus(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_GetWindowFocus;
		}
		static inline void FieldSet__Platform_GetWindowFocus(ImGuiPlatformIO* self, bool (*Platform_GetWindowFocus)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_GetWindowFocus) = (void*)Platform_GetWindowFocus;
		}
		static inline void* FieldGet__Platform_GetWindowMinimized(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_GetWindowMinimized;
		}
		static inline void FieldSet__Platform_GetWindowMinimized(ImGuiPlatformIO* self, bool (*Platform_GetWindowMinimized)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_GetWindowMinimized) = (void*)Platform_GetWindowMinimized;
		}
		static inline void* FieldGet__Platform_SetWindowTitle(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_SetWindowTitle;
		}
		static inline void FieldSet__Platform_SetWindowTitle(ImGuiPlatformIO* self, void (*Platform_SetWindowTitle)(ImGuiViewport* arg0,char* arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_SetWindowTitle) = (void*)Platform_SetWindowTitle;
		}
		static inline void* FieldGet__Platform_SetWindowAlpha(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_SetWindowAlpha;
		}
		static inline void FieldSet__Platform_SetWindowAlpha(ImGuiPlatformIO* self, void (*Platform_SetWindowAlpha)(ImGuiViewport* arg0,float arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_SetWindowAlpha) = (void*)Platform_SetWindowAlpha;
		}
		static inline void* FieldGet__Platform_UpdateWindow(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_UpdateWindow;
		}
		static inline void FieldSet__Platform_UpdateWindow(ImGuiPlatformIO* self, void (*Platform_UpdateWindow)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_UpdateWindow) = (void*)Platform_UpdateWindow;
		}
		static inline void* FieldGet__Platform_RenderWindow(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_RenderWindow;
		}
		static inline void FieldSet__Platform_RenderWindow(ImGuiPlatformIO* self, void (*Platform_RenderWindow)(ImGuiViewport* arg0,void* arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_RenderWindow) = (void*)Platform_RenderWindow;
		}
		static inline void* FieldGet__Platform_SwapBuffers(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_SwapBuffers;
		}
		static inline void FieldSet__Platform_SwapBuffers(ImGuiPlatformIO* self, void (*Platform_SwapBuffers)(ImGuiViewport* arg0,void* arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_SwapBuffers) = (void*)Platform_SwapBuffers;
		}
		static inline void* FieldGet__Platform_GetWindowDpiScale(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_GetWindowDpiScale;
		}
		static inline void FieldSet__Platform_GetWindowDpiScale(ImGuiPlatformIO* self, float (*Platform_GetWindowDpiScale)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_GetWindowDpiScale) = (void*)Platform_GetWindowDpiScale;
		}
		static inline void* FieldGet__Platform_OnChangedViewport(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_OnChangedViewport;
		}
		static inline void FieldSet__Platform_OnChangedViewport(ImGuiPlatformIO* self, void (*Platform_OnChangedViewport)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_OnChangedViewport) = (void*)Platform_OnChangedViewport;
		}
		static inline void* FieldGet__Platform_GetWindowWorkAreaInsets(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_GetWindowWorkAreaInsets;
		}
		static inline void FieldSet__Platform_GetWindowWorkAreaInsets(ImGuiPlatformIO* self, ImVec4 (*Platform_GetWindowWorkAreaInsets)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_GetWindowWorkAreaInsets) = (void*)Platform_GetWindowWorkAreaInsets;
		}
		static inline void* FieldGet__Platform_CreateVkSurface(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Platform_CreateVkSurface;
		}
		static inline void FieldSet__Platform_CreateVkSurface(ImGuiPlatformIO* self, int (*Platform_CreateVkSurface)(ImGuiViewport* arg0,unsigned long long arg1,void* arg2,unsigned long long* arg3))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Platform_CreateVkSurface) = (void*)Platform_CreateVkSurface;
		}
		static inline void* FieldGet__Renderer_CreateWindow(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Renderer_CreateWindow;
		}
		static inline void FieldSet__Renderer_CreateWindow(ImGuiPlatformIO* self, void (*Renderer_CreateWindow)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Renderer_CreateWindow) = (void*)Renderer_CreateWindow;
		}
		static inline void* FieldGet__Renderer_DestroyWindow(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Renderer_DestroyWindow;
		}
		static inline void FieldSet__Renderer_DestroyWindow(ImGuiPlatformIO* self, void (*Renderer_DestroyWindow)(ImGuiViewport* arg0))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Renderer_DestroyWindow) = (void*)Renderer_DestroyWindow;
		}
		static inline void* FieldGet__Renderer_SetWindowSize(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Renderer_SetWindowSize;
		}
		static inline void FieldSet__Renderer_SetWindowSize(ImGuiPlatformIO* self, void (*Renderer_SetWindowSize)(ImGuiViewport* arg0,ImVec2 arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Renderer_SetWindowSize) = (void*)Renderer_SetWindowSize;
		}
		static inline void* FieldGet__Renderer_RenderWindow(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Renderer_RenderWindow;
		}
		static inline void FieldSet__Renderer_RenderWindow(ImGuiPlatformIO* self, void (*Renderer_RenderWindow)(ImGuiViewport* arg0,void* arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Renderer_RenderWindow) = (void*)Renderer_RenderWindow;
		}
		static inline void* FieldGet__Renderer_SwapBuffers(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Renderer_SwapBuffers;
		}
		static inline void FieldSet__Renderer_SwapBuffers(ImGuiPlatformIO* self, void (*Renderer_SwapBuffers)(ImGuiViewport* arg0,void* arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->Renderer_SwapBuffers) = (void*)Renderer_SwapBuffers;
		}
		static inline void ClearPlatformHandlers(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearPlatformHandlers();
		}
		static inline void ClearRendererHandlers(ImGuiPlatformIO* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearRendererHandlers();
		}
	};
}


extern "C" VFX_API ImGuiPlatformIO* TitanImGui_ImGuiPlatformIO_Visitor_CreateInstance_2960189489()
{
	return ImGuiPlatformIO_Visitor::CreateInstance();
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiPlatformIO_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiPlatformIO>();
}


extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetClipboardTextFn(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_GetClipboardTextFn(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetClipboardTextFn(ImGuiPlatformIO* self, char* (*Platform_GetClipboardTextFn)())
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_GetClipboardTextFn(self, Platform_GetClipboardTextFn);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetClipboardTextFn(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_SetClipboardTextFn(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetClipboardTextFn(ImGuiPlatformIO* self, void (*Platform_SetClipboardTextFn)())
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_SetClipboardTextFn(self, Platform_SetClipboardTextFn);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ClipboardUserData(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_ClipboardUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ClipboardUserData(ImGuiPlatformIO* self, void* value)
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_ClipboardUserData(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OpenInShellFn(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_OpenInShellFn(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OpenInShellFn(ImGuiPlatformIO* self, bool (*Platform_OpenInShellFn)())
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_OpenInShellFn(self, Platform_OpenInShellFn);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OpenInShellUserData(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_OpenInShellUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OpenInShellUserData(ImGuiPlatformIO* self, void* value)
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_OpenInShellUserData(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetImeDataFn(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_SetImeDataFn(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetImeDataFn(ImGuiPlatformIO* self, void (*Platform_SetImeDataFn)())
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_SetImeDataFn(self, Platform_SetImeDataFn);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ImeUserData(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_ImeUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ImeUserData(ImGuiPlatformIO* self, void* value)
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_ImeUserData(self, value);
}
extern "C" VFX_API ImWchar16 TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_LocaleDecimalPoint(ImGuiPlatformIO* self)
{
	auto tmp_result = ImGuiPlatformIO_Visitor::FieldGet__Platform_LocaleDecimalPoint(self);
	return EngineNS::VReturnValueMarshal<ImWchar,ImWchar16>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_LocaleDecimalPoint(ImGuiPlatformIO* self, ImWchar value)
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_LocaleDecimalPoint(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_TextureMaxWidth(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Renderer_TextureMaxWidth(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_TextureMaxWidth(ImGuiPlatformIO* self, int value)
{
	ImGuiPlatformIO_Visitor::FieldSet__Renderer_TextureMaxWidth(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_TextureMaxHeight(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Renderer_TextureMaxHeight(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_TextureMaxHeight(ImGuiPlatformIO* self, int value)
{
	ImGuiPlatformIO_Visitor::FieldSet__Renderer_TextureMaxHeight(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_RenderState(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Renderer_RenderState(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_RenderState(ImGuiPlatformIO* self, void* value)
{
	ImGuiPlatformIO_Visitor::FieldSet__Renderer_RenderState(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_CreateWindow(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_CreateWindow(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_CreateWindow(ImGuiPlatformIO* self, void (*Platform_CreateWindow)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_CreateWindow(self, Platform_CreateWindow);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_DestroyWindow(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_DestroyWindow(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_DestroyWindow(ImGuiPlatformIO* self, void (*Platform_DestroyWindow)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_DestroyWindow(self, Platform_DestroyWindow);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_ShowWindow(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_ShowWindow(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_ShowWindow(ImGuiPlatformIO* self, void (*Platform_ShowWindow)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_ShowWindow(self, Platform_ShowWindow);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowPos(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_SetWindowPos(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowPos(ImGuiPlatformIO* self, void (*Platform_SetWindowPos)(ImGuiViewport* arg0,ImVec2 arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_SetWindowPos(self, Platform_SetWindowPos);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowPos(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_GetWindowPos(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowPos(ImGuiPlatformIO* self, ImVec2 (*Platform_GetWindowPos)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_GetWindowPos(self, Platform_GetWindowPos);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowSize(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_SetWindowSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowSize(ImGuiPlatformIO* self, void (*Platform_SetWindowSize)(ImGuiViewport* arg0,ImVec2 arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_SetWindowSize(self, Platform_SetWindowSize);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowSize(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_GetWindowSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowSize(ImGuiPlatformIO* self, ImVec2 (*Platform_GetWindowSize)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_GetWindowSize(self, Platform_GetWindowSize);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowFramebufferScale(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_GetWindowFramebufferScale(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowFramebufferScale(ImGuiPlatformIO* self, ImVec2 (*Platform_GetWindowFramebufferScale)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_GetWindowFramebufferScale(self, Platform_GetWindowFramebufferScale);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowFocus(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_SetWindowFocus(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowFocus(ImGuiPlatformIO* self, void (*Platform_SetWindowFocus)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_SetWindowFocus(self, Platform_SetWindowFocus);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowFocus(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_GetWindowFocus(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowFocus(ImGuiPlatformIO* self, bool (*Platform_GetWindowFocus)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_GetWindowFocus(self, Platform_GetWindowFocus);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowMinimized(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_GetWindowMinimized(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowMinimized(ImGuiPlatformIO* self, bool (*Platform_GetWindowMinimized)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_GetWindowMinimized(self, Platform_GetWindowMinimized);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowTitle(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_SetWindowTitle(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowTitle(ImGuiPlatformIO* self, void (*Platform_SetWindowTitle)(ImGuiViewport* arg0,char* arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_SetWindowTitle(self, Platform_SetWindowTitle);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SetWindowAlpha(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_SetWindowAlpha(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SetWindowAlpha(ImGuiPlatformIO* self, void (*Platform_SetWindowAlpha)(ImGuiViewport* arg0,float arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_SetWindowAlpha(self, Platform_SetWindowAlpha);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_UpdateWindow(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_UpdateWindow(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_UpdateWindow(ImGuiPlatformIO* self, void (*Platform_UpdateWindow)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_UpdateWindow(self, Platform_UpdateWindow);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_RenderWindow(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_RenderWindow(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_RenderWindow(ImGuiPlatformIO* self, void (*Platform_RenderWindow)(ImGuiViewport* arg0,void* arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_RenderWindow(self, Platform_RenderWindow);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_SwapBuffers(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_SwapBuffers(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_SwapBuffers(ImGuiPlatformIO* self, void (*Platform_SwapBuffers)(ImGuiViewport* arg0,void* arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_SwapBuffers(self, Platform_SwapBuffers);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowDpiScale(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_GetWindowDpiScale(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowDpiScale(ImGuiPlatformIO* self, float (*Platform_GetWindowDpiScale)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_GetWindowDpiScale(self, Platform_GetWindowDpiScale);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_OnChangedViewport(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_OnChangedViewport(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_OnChangedViewport(ImGuiPlatformIO* self, void (*Platform_OnChangedViewport)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_OnChangedViewport(self, Platform_OnChangedViewport);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_GetWindowWorkAreaInsets(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_GetWindowWorkAreaInsets(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_GetWindowWorkAreaInsets(ImGuiPlatformIO* self, ImVec4 (*Platform_GetWindowWorkAreaInsets)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_GetWindowWorkAreaInsets(self, Platform_GetWindowWorkAreaInsets);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Platform_CreateVkSurface(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Platform_CreateVkSurface(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Platform_CreateVkSurface(ImGuiPlatformIO* self, int (*Platform_CreateVkSurface)(ImGuiViewport* arg0,unsigned long long arg1,void* arg2,unsigned long long* arg3))
{
	ImGuiPlatformIO_Visitor::FieldSet__Platform_CreateVkSurface(self, Platform_CreateVkSurface);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_CreateWindow(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Renderer_CreateWindow(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_CreateWindow(ImGuiPlatformIO* self, void (*Renderer_CreateWindow)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Renderer_CreateWindow(self, Renderer_CreateWindow);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_DestroyWindow(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Renderer_DestroyWindow(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_DestroyWindow(ImGuiPlatformIO* self, void (*Renderer_DestroyWindow)(ImGuiViewport* arg0))
{
	ImGuiPlatformIO_Visitor::FieldSet__Renderer_DestroyWindow(self, Renderer_DestroyWindow);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_SetWindowSize(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Renderer_SetWindowSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_SetWindowSize(ImGuiPlatformIO* self, void (*Renderer_SetWindowSize)(ImGuiViewport* arg0,ImVec2 arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Renderer_SetWindowSize(self, Renderer_SetWindowSize);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_RenderWindow(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Renderer_RenderWindow(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_RenderWindow(ImGuiPlatformIO* self, void (*Renderer_RenderWindow)(ImGuiViewport* arg0,void* arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Renderer_RenderWindow(self, Renderer_RenderWindow);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformIO_Visitor_FieldGet__Renderer_SwapBuffers(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::FieldGet__Renderer_SwapBuffers(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_FieldSet__Renderer_SwapBuffers(ImGuiPlatformIO* self, void (*Renderer_SwapBuffers)(ImGuiViewport* arg0,void* arg1))
{
	ImGuiPlatformIO_Visitor::FieldSet__Renderer_SwapBuffers(self, Renderer_SwapBuffers);
}


extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_ClearPlatformHandlers_2960189489(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::ClearPlatformHandlers(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformIO_Visitor_ClearRendererHandlers_2960189489(ImGuiPlatformIO* self)
{
	return ImGuiPlatformIO_Visitor::ClearRendererHandlers(self);
}
#endif//HasModule_ImGui
