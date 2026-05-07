//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImFont_Visitor
	{
		static inline ImFont* CreateInstance()
		{
			return new ImFont();
		}
		static inline ImFontAtlas* FieldGet__OwnerAtlas(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFontAtlas*>();
			}
			return (ImFontAtlas*)self->OwnerAtlas;
		}
		static inline void FieldSet__OwnerAtlas(ImFont* self, ImFontAtlas* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->OwnerAtlas = value;
		}
		static inline int FieldGet__Flags(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Flags;
		}
		static inline void FieldSet__Flags(ImFont* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Flags = value;
		}
		static inline float FieldGet__CurrentRasterizerDensity(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->CurrentRasterizerDensity;
		}
		static inline void FieldSet__CurrentRasterizerDensity(ImFont* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CurrentRasterizerDensity = value;
		}
		static inline unsigned int FieldGet__FontId(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->FontId;
		}
		static inline void FieldSet__FontId(ImFont* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontId = value;
		}
		static inline float FieldGet__LegacySize(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->LegacySize;
		}
		static inline void FieldSet__LegacySize(ImFont* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->LegacySize = value;
		}
		static inline ImWchar FieldGet__EllipsisChar(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar>();
			}
			return (ImWchar)self->EllipsisChar;
		}
		static inline void FieldSet__EllipsisChar(ImFont* self, ImWchar value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->EllipsisChar = value;
		}
		static inline ImWchar FieldGet__FallbackChar(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar>();
			}
			return (ImWchar)self->FallbackChar;
		}
		static inline void FieldSet__FallbackChar(ImFont* self, ImWchar value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FallbackChar = value;
		}
		static inline unsigned char* FieldGet__Used8kPagesMap(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned char*>();
			}
			return (unsigned char*)self->Used8kPagesMap;
		}
		static inline void FieldSet__Used8kPagesMap(ImFont* self, unsigned char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 1; i++)
			{
				self->Used8kPagesMap[i] = value[i];
			}
		}
		static inline bool FieldGet__EllipsisAutoBake(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->EllipsisAutoBake;
		}
		static inline void FieldSet__EllipsisAutoBake(ImFont* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->EllipsisAutoBake = value;
		}
		static inline ImGuiStorage FieldGet__RemapPairs(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImGuiStorage>();
			}
			return (ImGuiStorage)self->RemapPairs;
		}
		static inline void FieldSet__RemapPairs(ImFont* self, ImGuiStorage value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->RemapPairs = value;
		}
		static inline float FieldGet__Scale(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->Scale;
		}
		static inline void FieldSet__Scale(ImFont* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Scale = value;
		}
		static inline bool IsGlyphInFont(ImFont* self, ImWchar c)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsGlyphInFont(c);
		}
		static inline bool IsLoaded(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsLoaded();
		}
		static inline char* GetDebugName(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->GetDebugName();
		}
		static inline ImVec2 CalcTextSizeA(ImFont* self, float size,float max_width,float wrap_width,const char* text_begin,const char* text_end,const char** out_remaining)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->CalcTextSizeA(size, max_width, wrap_width, text_begin, text_end, out_remaining);
		}
		static inline char* CalcWordWrapPosition(ImFont* self, float size,const char* text,const char* text_end,float wrap_width)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->CalcWordWrapPosition(size, text, text_end, wrap_width);
		}
		static inline void RenderChar(ImFont* self, ImDrawList* draw_list,float size,const ImVec2* pos,unsigned int col,ImWchar c,const ImVec4* cpu_fine_clip)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->RenderChar(draw_list, size, *pos, col, c, cpu_fine_clip);
		}
		static inline void RenderText(ImFont* self, ImDrawList* draw_list,float size,const ImVec2* pos,unsigned int col,const ImVec4* clip_rect,const char* text_begin,const char* text_end,float wrap_width,int flags)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->RenderText(draw_list, size, *pos, col, *clip_rect, text_begin, text_end, wrap_width, flags);
		}
		static inline char* CalcWordWrapPositionA(ImFont* self, float scale,const char* text,const char* text_end,float wrap_width)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->CalcWordWrapPositionA(scale, text, text_end, wrap_width);
		}
		static inline void ClearOutputData(ImFont* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearOutputData();
		}
		static inline void AddRemapChar(ImFont* self, ImWchar from_codepoint,ImWchar to_codepoint)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddRemapChar(from_codepoint, to_codepoint);
		}
		static inline bool IsGlyphRangeUnused(ImFont* self, unsigned int c_begin,unsigned int c_last)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsGlyphRangeUnused(c_begin, c_last);
		}
	};
}


extern "C" VFX_API ImFont* TitanImGui_ImFont_Visitor_CreateInstance_2960189489()
{
	return ImFont_Visitor::CreateInstance();
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImFont_Visitor_GetTypeRtti()
{
	return GetClassObject<ImFont>();
}


extern "C" VFX_API ImFontAtlas* TitanImGui_ImFont_Visitor_FieldGet__OwnerAtlas(ImFont* self)
{
	return ImFont_Visitor::FieldGet__OwnerAtlas(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__OwnerAtlas(ImFont* self, ImFontAtlas* value)
{
	ImFont_Visitor::FieldSet__OwnerAtlas(self, value);
}
extern "C" VFX_API int TitanImGui_ImFont_Visitor_FieldGet__Flags(ImFont* self)
{
	return ImFont_Visitor::FieldGet__Flags(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__Flags(ImFont* self, int value)
{
	ImFont_Visitor::FieldSet__Flags(self, value);
}
extern "C" VFX_API float TitanImGui_ImFont_Visitor_FieldGet__CurrentRasterizerDensity(ImFont* self)
{
	return ImFont_Visitor::FieldGet__CurrentRasterizerDensity(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__CurrentRasterizerDensity(ImFont* self, float value)
{
	ImFont_Visitor::FieldSet__CurrentRasterizerDensity(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImFont_Visitor_FieldGet__FontId(ImFont* self)
{
	return ImFont_Visitor::FieldGet__FontId(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__FontId(ImFont* self, unsigned int value)
{
	ImFont_Visitor::FieldSet__FontId(self, value);
}
extern "C" VFX_API float TitanImGui_ImFont_Visitor_FieldGet__LegacySize(ImFont* self)
{
	return ImFont_Visitor::FieldGet__LegacySize(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__LegacySize(ImFont* self, float value)
{
	ImFont_Visitor::FieldSet__LegacySize(self, value);
}
extern "C" VFX_API ImWchar16 TitanImGui_ImFont_Visitor_FieldGet__EllipsisChar(ImFont* self)
{
	auto tmp_result = ImFont_Visitor::FieldGet__EllipsisChar(self);
	return EngineNS::VReturnValueMarshal<ImWchar,ImWchar16>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__EllipsisChar(ImFont* self, ImWchar value)
{
	ImFont_Visitor::FieldSet__EllipsisChar(self, value);
}
extern "C" VFX_API ImWchar16 TitanImGui_ImFont_Visitor_FieldGet__FallbackChar(ImFont* self)
{
	auto tmp_result = ImFont_Visitor::FieldGet__FallbackChar(self);
	return EngineNS::VReturnValueMarshal<ImWchar,ImWchar16>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__FallbackChar(ImFont* self, ImWchar value)
{
	ImFont_Visitor::FieldSet__FallbackChar(self, value);
}
extern "C" VFX_API unsigned char* TitanImGui_ImFont_Visitor_FieldGet__Used8kPagesMap(ImFont* self)
{
	return ImFont_Visitor::FieldGet__Used8kPagesMap(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__Used8kPagesMap(ImFont* self, unsigned char* value)
{
	ImFont_Visitor::FieldSet__Used8kPagesMap(self, value);
}
extern "C" VFX_API char TitanImGui_ImFont_Visitor_FieldGet__EllipsisAutoBake(ImFont* self)
{
	auto tmp_result = ImFont_Visitor::FieldGet__EllipsisAutoBake(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__EllipsisAutoBake(ImFont* self, bool value)
{
	ImFont_Visitor::FieldSet__EllipsisAutoBake(self, value);
}
extern "C" VFX_API ImGuiStorage TitanImGui_ImFont_Visitor_FieldGet__RemapPairs(ImFont* self)
{
	return ImFont_Visitor::FieldGet__RemapPairs(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__RemapPairs(ImFont* self, ImGuiStorage value)
{
	ImFont_Visitor::FieldSet__RemapPairs(self, value);
}
extern "C" VFX_API float TitanImGui_ImFont_Visitor_FieldGet__Scale(ImFont* self)
{
	return ImFont_Visitor::FieldGet__Scale(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_FieldSet__Scale(ImFont* self, float value)
{
	ImFont_Visitor::FieldSet__Scale(self, value);
}


extern "C" VFX_API char TitanImGui_ImFont_Visitor_IsGlyphInFont_1393165236(ImFont* self, ImWchar c)
{
	auto tmp_result = ImFont_Visitor::IsGlyphInFont(self, c);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImFont_Visitor_IsLoaded_82051314(ImFont* self)
{
	auto tmp_result = ImFont_Visitor::IsLoaded(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char* TitanImGui_ImFont_Visitor_GetDebugName_721684103(ImFont* self)
{
	return ImFont_Visitor::GetDebugName(self);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImFont_Visitor_CalcTextSizeA_2915923972(ImFont* self, float size,float max_width,float wrap_width,const char* text_begin,const char* text_end,const char** out_remaining)
{
	auto tmp_result = ImFont_Visitor::CalcTextSizeA(self, size, max_width, wrap_width, text_begin, text_end, out_remaining);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API char* TitanImGui_ImFont_Visitor_CalcWordWrapPosition_3038693210(ImFont* self, float size,const char* text,const char* text_end,float wrap_width)
{
	return ImFont_Visitor::CalcWordWrapPosition(self, size, text, text_end, wrap_width);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_RenderChar_2818842580(ImFont* self, ImDrawList* draw_list,float size,const ImVec2* pos,unsigned int col,ImWchar c,const ImVec4* cpu_fine_clip)
{
	return ImFont_Visitor::RenderChar(self, draw_list, size, pos, col, c, cpu_fine_clip);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_RenderText_3447178593(ImFont* self, ImDrawList* draw_list,float size,const ImVec2* pos,unsigned int col,const ImVec4* clip_rect,const char* text_begin,const char* text_end,float wrap_width,int flags)
{
	return ImFont_Visitor::RenderText(self, draw_list, size, pos, col, clip_rect, text_begin, text_end, wrap_width, flags);
}
extern "C" VFX_API char* TitanImGui_ImFont_Visitor_CalcWordWrapPositionA_3038693210(ImFont* self, float scale,const char* text,const char* text_end,float wrap_width)
{
	return ImFont_Visitor::CalcWordWrapPositionA(self, scale, text, text_end, wrap_width);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_ClearOutputData_2960189489(ImFont* self)
{
	return ImFont_Visitor::ClearOutputData(self);
}
extern "C" VFX_API void TitanImGui_ImFont_Visitor_AddRemapChar_1017310781(ImFont* self, ImWchar from_codepoint,ImWchar to_codepoint)
{
	return ImFont_Visitor::AddRemapChar(self, from_codepoint, to_codepoint);
}
extern "C" VFX_API char TitanImGui_ImFont_Visitor_IsGlyphRangeUnused_2243309331(ImFont* self, unsigned int c_begin,unsigned int c_last)
{
	auto tmp_result = ImFont_Visitor::IsGlyphRangeUnused(self, c_begin, c_last);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
#endif//HasModule_ImGui
