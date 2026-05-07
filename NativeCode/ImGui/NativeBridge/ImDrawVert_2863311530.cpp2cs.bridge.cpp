//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImDrawVert_Visitor
	{
		static void UnsafeCallDestructor(ImDrawVert* self)
		{
		}
		static inline ImVec2 FieldGet__pos(ImDrawVert* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->pos;
		}
		static inline void FieldSet__pos(ImDrawVert* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->pos = value;
		}
		static inline ImVec2 FieldGet__uv(ImDrawVert* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->uv;
		}
		static inline void FieldSet__uv(ImDrawVert* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->uv = value;
		}
		static inline unsigned int FieldGet__col(ImDrawVert* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->col;
		}
		static inline void FieldSet__col(ImDrawVert* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->col = value;
		}
	};
}


extern "C" VFX_API void TitanImGui_ImDrawVert_Visitor_UnsafeCallDestructor(ImDrawVert* self)
{
	return ImDrawVert_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImDrawVert_Visitor_GetTypeRtti()
{
	return GetClassObject<ImDrawVert>();
}


extern "C" VFX_API v3dVector2_t TitanImGui_ImDrawVert_Visitor_FieldGet__pos(ImDrawVert* self)
{
	auto tmp_result = ImDrawVert_Visitor::FieldGet__pos(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawVert_Visitor_FieldSet__pos(ImDrawVert* self, ImVec2 value)
{
	ImDrawVert_Visitor::FieldSet__pos(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImDrawVert_Visitor_FieldGet__uv(ImDrawVert* self)
{
	auto tmp_result = ImDrawVert_Visitor::FieldGet__uv(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawVert_Visitor_FieldSet__uv(ImDrawVert* self, ImVec2 value)
{
	ImDrawVert_Visitor::FieldSet__uv(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImDrawVert_Visitor_FieldGet__col(ImDrawVert* self)
{
	return ImDrawVert_Visitor::FieldGet__col(self);
}
extern "C" VFX_API void TitanImGui_ImDrawVert_Visitor_FieldSet__col(ImDrawVert* self, unsigned int value)
{
	ImDrawVert_Visitor::FieldSet__col(self, value);
}


#endif//HasModule_ImGui
