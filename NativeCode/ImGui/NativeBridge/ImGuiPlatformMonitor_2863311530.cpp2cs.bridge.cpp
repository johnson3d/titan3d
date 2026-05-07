//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiPlatformMonitor_Visitor
	{
		static void UnsafeCallConstructor(ImGuiPlatformMonitor* self)
		{
			#undef new
			new (self)ImGuiPlatformMonitor();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImGuiPlatformMonitor* self)
		{
		}
		static inline ImVec2 FieldGet__MainPos(ImGuiPlatformMonitor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->MainPos;
		}
		static inline void FieldSet__MainPos(ImGuiPlatformMonitor* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MainPos = value;
		}
		static inline ImVec2 FieldGet__MainSize(ImGuiPlatformMonitor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->MainSize;
		}
		static inline void FieldSet__MainSize(ImGuiPlatformMonitor* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MainSize = value;
		}
		static inline ImVec2 FieldGet__WorkPos(ImGuiPlatformMonitor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->WorkPos;
		}
		static inline void FieldSet__WorkPos(ImGuiPlatformMonitor* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WorkPos = value;
		}
		static inline ImVec2 FieldGet__WorkSize(ImGuiPlatformMonitor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->WorkSize;
		}
		static inline void FieldSet__WorkSize(ImGuiPlatformMonitor* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WorkSize = value;
		}
		static inline float FieldGet__DpiScale(ImGuiPlatformMonitor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->DpiScale;
		}
		static inline void FieldSet__DpiScale(ImGuiPlatformMonitor* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DpiScale = value;
		}
		static inline void* FieldGet__PlatformHandle(ImGuiPlatformMonitor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->PlatformHandle;
		}
		static inline void FieldSet__PlatformHandle(ImGuiPlatformMonitor* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PlatformHandle = value;
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiPlatformMonitor_Visitor_UnsafeCallConstructor_2960189489(ImGuiPlatformMonitor* self)
{
	return ImGuiPlatformMonitor_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformMonitor_Visitor_UnsafeCallDestructor(ImGuiPlatformMonitor* self)
{
	return ImGuiPlatformMonitor_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiPlatformMonitor_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiPlatformMonitor>();
}


extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__MainPos(ImGuiPlatformMonitor* self)
{
	auto tmp_result = ImGuiPlatformMonitor_Visitor::FieldGet__MainPos(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__MainPos(ImGuiPlatformMonitor* self, ImVec2 value)
{
	ImGuiPlatformMonitor_Visitor::FieldSet__MainPos(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__MainSize(ImGuiPlatformMonitor* self)
{
	auto tmp_result = ImGuiPlatformMonitor_Visitor::FieldGet__MainSize(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__MainSize(ImGuiPlatformMonitor* self, ImVec2 value)
{
	ImGuiPlatformMonitor_Visitor::FieldSet__MainSize(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__WorkPos(ImGuiPlatformMonitor* self)
{
	auto tmp_result = ImGuiPlatformMonitor_Visitor::FieldGet__WorkPos(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__WorkPos(ImGuiPlatformMonitor* self, ImVec2 value)
{
	ImGuiPlatformMonitor_Visitor::FieldSet__WorkPos(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__WorkSize(ImGuiPlatformMonitor* self)
{
	auto tmp_result = ImGuiPlatformMonitor_Visitor::FieldGet__WorkSize(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__WorkSize(ImGuiPlatformMonitor* self, ImVec2 value)
{
	ImGuiPlatformMonitor_Visitor::FieldSet__WorkSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__DpiScale(ImGuiPlatformMonitor* self)
{
	return ImGuiPlatformMonitor_Visitor::FieldGet__DpiScale(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__DpiScale(ImGuiPlatformMonitor* self, float value)
{
	ImGuiPlatformMonitor_Visitor::FieldSet__DpiScale(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiPlatformMonitor_Visitor_FieldGet__PlatformHandle(ImGuiPlatformMonitor* self)
{
	return ImGuiPlatformMonitor_Visitor::FieldGet__PlatformHandle(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPlatformMonitor_Visitor_FieldSet__PlatformHandle(ImGuiPlatformMonitor* self, void* value)
{
	ImGuiPlatformMonitor_Visitor::FieldSet__PlatformHandle(self, value);
}


#endif//HasModule_ImGui
