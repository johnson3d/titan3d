//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiSizeCallbackData_Visitor
	{
		static void UnsafeCallDestructor(ImGuiSizeCallbackData* self)
		{
		}
		static inline void* FieldGet__UserData(ImGuiSizeCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->UserData;
		}
		static inline void FieldSet__UserData(ImGuiSizeCallbackData* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->UserData = value;
		}
		static inline ImVec2 FieldGet__Pos(ImGuiSizeCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->Pos;
		}
		static inline void FieldSet__Pos(ImGuiSizeCallbackData* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Pos = value;
		}
		static inline ImVec2 FieldGet__CurrentSize(ImGuiSizeCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->CurrentSize;
		}
		static inline void FieldSet__CurrentSize(ImGuiSizeCallbackData* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CurrentSize = value;
		}
		static inline ImVec2 FieldGet__DesiredSize(ImGuiSizeCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->DesiredSize;
		}
		static inline void FieldSet__DesiredSize(ImGuiSizeCallbackData* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DesiredSize = value;
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiSizeCallbackData_Visitor_UnsafeCallDestructor(ImGuiSizeCallbackData* self)
{
	return ImGuiSizeCallbackData_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiSizeCallbackData_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiSizeCallbackData>();
}


extern "C" VFX_API void* TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__UserData(ImGuiSizeCallbackData* self)
{
	return ImGuiSizeCallbackData_Visitor::FieldGet__UserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__UserData(ImGuiSizeCallbackData* self, void* value)
{
	ImGuiSizeCallbackData_Visitor::FieldSet__UserData(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__Pos(ImGuiSizeCallbackData* self)
{
	auto tmp_result = ImGuiSizeCallbackData_Visitor::FieldGet__Pos(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__Pos(ImGuiSizeCallbackData* self, ImVec2 value)
{
	ImGuiSizeCallbackData_Visitor::FieldSet__Pos(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__CurrentSize(ImGuiSizeCallbackData* self)
{
	auto tmp_result = ImGuiSizeCallbackData_Visitor::FieldGet__CurrentSize(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__CurrentSize(ImGuiSizeCallbackData* self, ImVec2 value)
{
	ImGuiSizeCallbackData_Visitor::FieldSet__CurrentSize(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__DesiredSize(ImGuiSizeCallbackData* self)
{
	auto tmp_result = ImGuiSizeCallbackData_Visitor::FieldGet__DesiredSize(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__DesiredSize(ImGuiSizeCallbackData* self, ImVec2 value)
{
	ImGuiSizeCallbackData_Visitor::FieldSet__DesiredSize(self, value);
}


#endif//HasModule_ImGui
