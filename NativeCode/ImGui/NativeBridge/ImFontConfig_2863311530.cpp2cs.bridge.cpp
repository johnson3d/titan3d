//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImFontConfig_Visitor
	{
		static void UnsafeCallConstructor(ImFontConfig* self)
		{
			#undef new
			new (self)ImFontConfig();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImFontConfig* self)
		{
		}
		static inline char* FieldGet__Name(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->Name;
		}
		static inline void FieldSet__Name(ImFontConfig* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 40; i++)
			{
				self->Name[i] = value[i];
			}
		}
		static inline void* FieldGet__FontData(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->FontData;
		}
		static inline void FieldSet__FontData(ImFontConfig* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontData = value;
		}
		static inline int FieldGet__FontDataSize(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->FontDataSize;
		}
		static inline void FieldSet__FontDataSize(ImFontConfig* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontDataSize = value;
		}
		static inline bool FieldGet__FontDataOwnedByAtlas(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->FontDataOwnedByAtlas;
		}
		static inline void FieldSet__FontDataOwnedByAtlas(ImFontConfig* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontDataOwnedByAtlas = value;
		}
		static inline bool FieldGet__MergeMode(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->MergeMode;
		}
		static inline void FieldSet__MergeMode(ImFontConfig* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MergeMode = value;
		}
		static inline bool FieldGet__PixelSnapH(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->PixelSnapH;
		}
		static inline void FieldSet__PixelSnapH(ImFontConfig* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PixelSnapH = value;
		}
		static inline ImWchar FieldGet__EllipsisChar(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar>();
			}
			return (ImWchar)self->EllipsisChar;
		}
		static inline void FieldSet__EllipsisChar(ImFontConfig* self, ImWchar value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->EllipsisChar = value;
		}
		static inline float FieldGet__SizePixels(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->SizePixels;
		}
		static inline void FieldSet__SizePixels(ImFontConfig* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SizePixels = value;
		}
		static inline ImWchar* FieldGet__GlyphRanges(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GlyphRanges;
		}
		static inline void FieldSet__GlyphRanges(ImFontConfig* self, ImWchar* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->GlyphRanges = value;
		}
		static inline ImWchar* FieldGet__GlyphExcludeRanges(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GlyphExcludeRanges;
		}
		static inline void FieldSet__GlyphExcludeRanges(ImFontConfig* self, ImWchar* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->GlyphExcludeRanges = value;
		}
		static inline ImVec2 FieldGet__GlyphOffset(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->GlyphOffset;
		}
		static inline void FieldSet__GlyphOffset(ImFontConfig* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->GlyphOffset = value;
		}
		static inline float FieldGet__GlyphMinAdvanceX(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->GlyphMinAdvanceX;
		}
		static inline void FieldSet__GlyphMinAdvanceX(ImFontConfig* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->GlyphMinAdvanceX = value;
		}
		static inline float FieldGet__GlyphMaxAdvanceX(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->GlyphMaxAdvanceX;
		}
		static inline void FieldSet__GlyphMaxAdvanceX(ImFontConfig* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->GlyphMaxAdvanceX = value;
		}
		static inline float FieldGet__GlyphExtraAdvanceX(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->GlyphExtraAdvanceX;
		}
		static inline void FieldSet__GlyphExtraAdvanceX(ImFontConfig* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->GlyphExtraAdvanceX = value;
		}
		static inline unsigned int FieldGet__FontNo(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->FontNo;
		}
		static inline void FieldSet__FontNo(ImFontConfig* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontNo = value;
		}
		static inline unsigned int FieldGet__FontLoaderFlags(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->FontLoaderFlags;
		}
		static inline void FieldSet__FontLoaderFlags(ImFontConfig* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontLoaderFlags = value;
		}
		static inline float FieldGet__RasterizerMultiply(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->RasterizerMultiply;
		}
		static inline void FieldSet__RasterizerMultiply(ImFontConfig* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->RasterizerMultiply = value;
		}
		static inline float FieldGet__RasterizerDensity(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->RasterizerDensity;
		}
		static inline void FieldSet__RasterizerDensity(ImFontConfig* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->RasterizerDensity = value;
		}
		static inline float FieldGet__ExtraSizeScale(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ExtraSizeScale;
		}
		static inline void FieldSet__ExtraSizeScale(ImFontConfig* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ExtraSizeScale = value;
		}
		static inline int FieldGet__Flags(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Flags;
		}
		static inline void FieldSet__Flags(ImFontConfig* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Flags = value;
		}
		static inline ImFont* FieldGet__DstFont(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->DstFont;
		}
		static inline void FieldSet__DstFont(ImFontConfig* self, ImFont* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DstFont = value;
		}
		static inline void* FieldGet__FontLoaderData(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->FontLoaderData;
		}
		static inline void FieldSet__FontLoaderData(ImFontConfig* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontLoaderData = value;
		}
		static inline bool FieldGet__PixelSnapV(ImFontConfig* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->PixelSnapV;
		}
		static inline void FieldSet__PixelSnapV(ImFontConfig* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PixelSnapV = value;
		}
	};
}


extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_UnsafeCallConstructor_2960189489(ImFontConfig* self)
{
	return ImFontConfig_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_UnsafeCallDestructor(ImFontConfig* self)
{
	return ImFontConfig_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImFontConfig_Visitor_GetTypeRtti()
{
	return GetClassObject<ImFontConfig>();
}


extern "C" VFX_API char* TitanImGui_ImFontConfig_Visitor_FieldGet__Name(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__Name(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__Name(ImFontConfig* self, char* value)
{
	ImFontConfig_Visitor::FieldSet__Name(self, value);
}
extern "C" VFX_API void* TitanImGui_ImFontConfig_Visitor_FieldGet__FontData(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__FontData(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__FontData(ImFontConfig* self, void* value)
{
	ImFontConfig_Visitor::FieldSet__FontData(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontConfig_Visitor_FieldGet__FontDataSize(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__FontDataSize(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__FontDataSize(ImFontConfig* self, int value)
{
	ImFontConfig_Visitor::FieldSet__FontDataSize(self, value);
}
extern "C" VFX_API char TitanImGui_ImFontConfig_Visitor_FieldGet__FontDataOwnedByAtlas(ImFontConfig* self)
{
	auto tmp_result = ImFontConfig_Visitor::FieldGet__FontDataOwnedByAtlas(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__FontDataOwnedByAtlas(ImFontConfig* self, bool value)
{
	ImFontConfig_Visitor::FieldSet__FontDataOwnedByAtlas(self, value);
}
extern "C" VFX_API char TitanImGui_ImFontConfig_Visitor_FieldGet__MergeMode(ImFontConfig* self)
{
	auto tmp_result = ImFontConfig_Visitor::FieldGet__MergeMode(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__MergeMode(ImFontConfig* self, bool value)
{
	ImFontConfig_Visitor::FieldSet__MergeMode(self, value);
}
extern "C" VFX_API char TitanImGui_ImFontConfig_Visitor_FieldGet__PixelSnapH(ImFontConfig* self)
{
	auto tmp_result = ImFontConfig_Visitor::FieldGet__PixelSnapH(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__PixelSnapH(ImFontConfig* self, bool value)
{
	ImFontConfig_Visitor::FieldSet__PixelSnapH(self, value);
}
extern "C" VFX_API ImWchar16 TitanImGui_ImFontConfig_Visitor_FieldGet__EllipsisChar(ImFontConfig* self)
{
	auto tmp_result = ImFontConfig_Visitor::FieldGet__EllipsisChar(self);
	return EngineNS::VReturnValueMarshal<ImWchar,ImWchar16>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__EllipsisChar(ImFontConfig* self, ImWchar value)
{
	ImFontConfig_Visitor::FieldSet__EllipsisChar(self, value);
}
extern "C" VFX_API float TitanImGui_ImFontConfig_Visitor_FieldGet__SizePixels(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__SizePixels(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__SizePixels(ImFontConfig* self, float value)
{
	ImFontConfig_Visitor::FieldSet__SizePixels(self, value);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphRanges(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__GlyphRanges(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphRanges(ImFontConfig* self, ImWchar* value)
{
	ImFontConfig_Visitor::FieldSet__GlyphRanges(self, value);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphExcludeRanges(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__GlyphExcludeRanges(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphExcludeRanges(ImFontConfig* self, ImWchar* value)
{
	ImFontConfig_Visitor::FieldSet__GlyphExcludeRanges(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphOffset(ImFontConfig* self)
{
	auto tmp_result = ImFontConfig_Visitor::FieldGet__GlyphOffset(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphOffset(ImFontConfig* self, ImVec2 value)
{
	ImFontConfig_Visitor::FieldSet__GlyphOffset(self, value);
}
extern "C" VFX_API float TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphMinAdvanceX(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__GlyphMinAdvanceX(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphMinAdvanceX(ImFontConfig* self, float value)
{
	ImFontConfig_Visitor::FieldSet__GlyphMinAdvanceX(self, value);
}
extern "C" VFX_API float TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphMaxAdvanceX(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__GlyphMaxAdvanceX(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphMaxAdvanceX(ImFontConfig* self, float value)
{
	ImFontConfig_Visitor::FieldSet__GlyphMaxAdvanceX(self, value);
}
extern "C" VFX_API float TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphExtraAdvanceX(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__GlyphExtraAdvanceX(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphExtraAdvanceX(ImFontConfig* self, float value)
{
	ImFontConfig_Visitor::FieldSet__GlyphExtraAdvanceX(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImFontConfig_Visitor_FieldGet__FontNo(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__FontNo(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__FontNo(ImFontConfig* self, unsigned int value)
{
	ImFontConfig_Visitor::FieldSet__FontNo(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImFontConfig_Visitor_FieldGet__FontLoaderFlags(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__FontLoaderFlags(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__FontLoaderFlags(ImFontConfig* self, unsigned int value)
{
	ImFontConfig_Visitor::FieldSet__FontLoaderFlags(self, value);
}
extern "C" VFX_API float TitanImGui_ImFontConfig_Visitor_FieldGet__RasterizerMultiply(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__RasterizerMultiply(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__RasterizerMultiply(ImFontConfig* self, float value)
{
	ImFontConfig_Visitor::FieldSet__RasterizerMultiply(self, value);
}
extern "C" VFX_API float TitanImGui_ImFontConfig_Visitor_FieldGet__RasterizerDensity(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__RasterizerDensity(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__RasterizerDensity(ImFontConfig* self, float value)
{
	ImFontConfig_Visitor::FieldSet__RasterizerDensity(self, value);
}
extern "C" VFX_API float TitanImGui_ImFontConfig_Visitor_FieldGet__ExtraSizeScale(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__ExtraSizeScale(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__ExtraSizeScale(ImFontConfig* self, float value)
{
	ImFontConfig_Visitor::FieldSet__ExtraSizeScale(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontConfig_Visitor_FieldGet__Flags(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__Flags(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__Flags(ImFontConfig* self, int value)
{
	ImFontConfig_Visitor::FieldSet__Flags(self, value);
}
extern "C" VFX_API ImFont* TitanImGui_ImFontConfig_Visitor_FieldGet__DstFont(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__DstFont(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__DstFont(ImFontConfig* self, ImFont* value)
{
	ImFontConfig_Visitor::FieldSet__DstFont(self, value);
}
extern "C" VFX_API void* TitanImGui_ImFontConfig_Visitor_FieldGet__FontLoaderData(ImFontConfig* self)
{
	return ImFontConfig_Visitor::FieldGet__FontLoaderData(self);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__FontLoaderData(ImFontConfig* self, void* value)
{
	ImFontConfig_Visitor::FieldSet__FontLoaderData(self, value);
}
extern "C" VFX_API char TitanImGui_ImFontConfig_Visitor_FieldGet__PixelSnapV(ImFontConfig* self)
{
	auto tmp_result = ImFontConfig_Visitor::FieldGet__PixelSnapV(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontConfig_Visitor_FieldSet__PixelSnapV(ImFontConfig* self, bool value)
{
	ImFontConfig_Visitor::FieldSet__PixelSnapV(self, value);
}


#endif//HasModule_ImGui
