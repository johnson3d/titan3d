//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui_binding.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiViewportDataSDL2_Visitor
	{
		static void UnsafeCallConstructor(EngineNS::ImGuiViewportDataSDL2* self)
		{
			#undef new
			new (self)EngineNS::ImGuiViewportDataSDL2();
			#define new VNEW
		}
		static void UnsafeCallDestructor(EngineNS::ImGuiViewportDataSDL2* self)
		{
		}
		static inline void* FieldGet__Window(EngineNS::ImGuiViewportDataSDL2* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Window;
		}
		static inline void FieldSet__Window(EngineNS::ImGuiViewportDataSDL2* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Window = value;
		}
		static inline unsigned int FieldGet__WindowID(EngineNS::ImGuiViewportDataSDL2* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->WindowID;
		}
		static inline void FieldSet__WindowID(EngineNS::ImGuiViewportDataSDL2* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowID = value;
		}
		static inline bool FieldGet__WindowOwned(EngineNS::ImGuiViewportDataSDL2* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WindowOwned;
		}
		static inline void FieldSet__WindowOwned(EngineNS::ImGuiViewportDataSDL2* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowOwned = value;
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiViewportDataSDL2_Visitor_UnsafeCallConstructor_2960189489(EngineNS::ImGuiViewportDataSDL2* self)
{
	return ImGuiViewportDataSDL2_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewportDataSDL2_Visitor_UnsafeCallDestructor(EngineNS::ImGuiViewportDataSDL2* self)
{
	return ImGuiViewportDataSDL2_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiViewportDataSDL2_Visitor_GetTypeRtti()
{
	return GetClassObject<EngineNS::ImGuiViewportDataSDL2>();
}


extern "C" VFX_API void* TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__Window(EngineNS::ImGuiViewportDataSDL2* self)
{
	return ImGuiViewportDataSDL2_Visitor::FieldGet__Window(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__Window(EngineNS::ImGuiViewportDataSDL2* self, void* value)
{
	ImGuiViewportDataSDL2_Visitor::FieldSet__Window(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__WindowID(EngineNS::ImGuiViewportDataSDL2* self)
{
	return ImGuiViewportDataSDL2_Visitor::FieldGet__WindowID(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__WindowID(EngineNS::ImGuiViewportDataSDL2* self, unsigned int value)
{
	ImGuiViewportDataSDL2_Visitor::FieldSet__WindowID(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldGet__WindowOwned(EngineNS::ImGuiViewportDataSDL2* self)
{
	auto tmp_result = ImGuiViewportDataSDL2_Visitor::FieldGet__WindowOwned(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewportDataSDL2_Visitor_FieldSet__WindowOwned(EngineNS::ImGuiViewportDataSDL2* self, bool value)
{
	ImGuiViewportDataSDL2_Visitor::FieldSet__WindowOwned(self, value);
}


#endif//HasModule_ImGui
