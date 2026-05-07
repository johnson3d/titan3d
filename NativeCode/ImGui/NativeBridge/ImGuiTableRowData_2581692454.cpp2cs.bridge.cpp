//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui_binding.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiTableRowData_Visitor
	{
		static void UnsafeCallConstructor(EngineNS::ImGuiTableRowData* self)
		{
			#undef new
			new (self)EngineNS::ImGuiTableRowData();
			#define new VNEW
		}
		static void UnsafeCallDestructor(EngineNS::ImGuiTableRowData* self)
		{
		}
		static inline unsigned long long FieldGet__IndentTextureId(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned long long>();
			}
			return (unsigned long long)self->IndentTextureId;
		}
		static inline void FieldSet__IndentTextureId(EngineNS::ImGuiTableRowData* self, unsigned long long value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IndentTextureId = value;
		}
		static inline float FieldGet__MinHeight(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->MinHeight;
		}
		static inline void FieldSet__MinHeight(EngineNS::ImGuiTableRowData* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MinHeight = value;
		}
		static inline float FieldGet__CellPaddingYEnd(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->CellPaddingYEnd;
		}
		static inline void FieldSet__CellPaddingYEnd(EngineNS::ImGuiTableRowData* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CellPaddingYEnd = value;
		}
		static inline float FieldGet__CellPaddingYBegin(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->CellPaddingYBegin;
		}
		static inline void FieldSet__CellPaddingYBegin(EngineNS::ImGuiTableRowData* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CellPaddingYBegin = value;
		}
		static inline float FieldGet__IndentImageWidth(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->IndentImageWidth;
		}
		static inline void FieldSet__IndentImageWidth(EngineNS::ImGuiTableRowData* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IndentImageWidth = value;
		}
		static inline ImVec2 FieldGet__IndentTextureUVMin(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->IndentTextureUVMin;
		}
		static inline void FieldSet__IndentTextureUVMin(EngineNS::ImGuiTableRowData* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IndentTextureUVMin = value;
		}
		static inline ImVec2 FieldGet__IndentTextureUVMax(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->IndentTextureUVMax;
		}
		static inline void FieldSet__IndentTextureUVMax(EngineNS::ImGuiTableRowData* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IndentTextureUVMax = value;
		}
		static inline unsigned int FieldGet__IndentColor(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->IndentColor;
		}
		static inline void FieldSet__IndentColor(EngineNS::ImGuiTableRowData* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IndentColor = value;
		}
		static inline unsigned int FieldGet__HoverColor(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->HoverColor;
		}
		static inline void FieldSet__HoverColor(EngineNS::ImGuiTableRowData* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->HoverColor = value;
		}
		static inline int FieldGet__Flags(EngineNS::ImGuiTableRowData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Flags;
		}
		static inline void FieldSet__Flags(EngineNS::ImGuiTableRowData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Flags = value;
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_UnsafeCallConstructor_2960189489(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_UnsafeCallDestructor(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiTableRowData_Visitor_GetTypeRtti()
{
	return GetClassObject<EngineNS::ImGuiTableRowData>();
}


extern "C" VFX_API unsigned long long TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureId(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::FieldGet__IndentTextureId(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureId(EngineNS::ImGuiTableRowData* self, unsigned long long value)
{
	ImGuiTableRowData_Visitor::FieldSet__IndentTextureId(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiTableRowData_Visitor_FieldGet__MinHeight(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::FieldGet__MinHeight(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__MinHeight(EngineNS::ImGuiTableRowData* self, float value)
{
	ImGuiTableRowData_Visitor::FieldSet__MinHeight(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiTableRowData_Visitor_FieldGet__CellPaddingYEnd(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::FieldGet__CellPaddingYEnd(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__CellPaddingYEnd(EngineNS::ImGuiTableRowData* self, float value)
{
	ImGuiTableRowData_Visitor::FieldSet__CellPaddingYEnd(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiTableRowData_Visitor_FieldGet__CellPaddingYBegin(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::FieldGet__CellPaddingYBegin(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__CellPaddingYBegin(EngineNS::ImGuiTableRowData* self, float value)
{
	ImGuiTableRowData_Visitor::FieldSet__CellPaddingYBegin(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentImageWidth(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::FieldGet__IndentImageWidth(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentImageWidth(EngineNS::ImGuiTableRowData* self, float value)
{
	ImGuiTableRowData_Visitor::FieldSet__IndentImageWidth(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureUVMin(EngineNS::ImGuiTableRowData* self)
{
	auto tmp_result = ImGuiTableRowData_Visitor::FieldGet__IndentTextureUVMin(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureUVMin(EngineNS::ImGuiTableRowData* self, ImVec2 value)
{
	ImGuiTableRowData_Visitor::FieldSet__IndentTextureUVMin(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentTextureUVMax(EngineNS::ImGuiTableRowData* self)
{
	auto tmp_result = ImGuiTableRowData_Visitor::FieldGet__IndentTextureUVMax(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentTextureUVMax(EngineNS::ImGuiTableRowData* self, ImVec2 value)
{
	ImGuiTableRowData_Visitor::FieldSet__IndentTextureUVMax(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiTableRowData_Visitor_FieldGet__IndentColor(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::FieldGet__IndentColor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__IndentColor(EngineNS::ImGuiTableRowData* self, unsigned int value)
{
	ImGuiTableRowData_Visitor::FieldSet__IndentColor(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiTableRowData_Visitor_FieldGet__HoverColor(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::FieldGet__HoverColor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__HoverColor(EngineNS::ImGuiTableRowData* self, unsigned int value)
{
	ImGuiTableRowData_Visitor::FieldSet__HoverColor(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiTableRowData_Visitor_FieldGet__Flags(EngineNS::ImGuiTableRowData* self)
{
	return ImGuiTableRowData_Visitor::FieldGet__Flags(self);
}
extern "C" VFX_API void TitanImGui_ImGuiTableRowData_Visitor_FieldSet__Flags(EngineNS::ImGuiTableRowData* self, int value)
{
	ImGuiTableRowData_Visitor::FieldSet__Flags(self, value);
}


#endif//HasModule_ImGui
