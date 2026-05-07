//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImFontAtlas_Visitor
	{
		static inline ImFontAtlas* CreateInstance()
		{
			return new ImFontAtlas();
		}
		static void Dispose(ImFontAtlas* self)
		{
			delete self;
		}
		static inline int FieldGet__Flags(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Flags;
		}
		static inline void FieldSet__Flags(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Flags = value;
		}
		static inline int FieldGet__TexGlyphPadding(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TexGlyphPadding;
		}
		static inline void FieldSet__TexGlyphPadding(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexGlyphPadding = value;
		}
		static inline int FieldGet__TexMinWidth(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TexMinWidth;
		}
		static inline void FieldSet__TexMinWidth(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexMinWidth = value;
		}
		static inline int FieldGet__TexMinHeight(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TexMinHeight;
		}
		static inline void FieldSet__TexMinHeight(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexMinHeight = value;
		}
		static inline int FieldGet__TexMaxWidth(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TexMaxWidth;
		}
		static inline void FieldSet__TexMaxWidth(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexMaxWidth = value;
		}
		static inline int FieldGet__TexMaxHeight(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TexMaxHeight;
		}
		static inline void FieldSet__TexMaxHeight(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexMaxHeight = value;
		}
		static inline void* FieldGet__UserData(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->UserData;
		}
		static inline void FieldSet__UserData(ImFontAtlas* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->UserData = value;
		}
		static inline bool FieldGet__Locked(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->Locked;
		}
		static inline void FieldSet__Locked(ImFontAtlas* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Locked = value;
		}
		static inline bool FieldGet__RendererHasTextures(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->RendererHasTextures;
		}
		static inline void FieldSet__RendererHasTextures(ImFontAtlas* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->RendererHasTextures = value;
		}
		static inline bool FieldGet__TexIsBuilt(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->TexIsBuilt;
		}
		static inline void FieldSet__TexIsBuilt(ImFontAtlas* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexIsBuilt = value;
		}
		static inline bool FieldGet__TexPixelsUseColors(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->TexPixelsUseColors;
		}
		static inline void FieldSet__TexPixelsUseColors(ImFontAtlas* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexPixelsUseColors = value;
		}
		static inline ImVec2 FieldGet__TexUvScale(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->TexUvScale;
		}
		static inline void FieldSet__TexUvScale(ImFontAtlas* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexUvScale = value;
		}
		static inline ImVec2 FieldGet__TexUvWhitePixel(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->TexUvWhitePixel;
		}
		static inline void FieldSet__TexUvWhitePixel(ImFontAtlas* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexUvWhitePixel = value;
		}
		static inline ImVec4* FieldGet__TexUvLines(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec4*>();
			}
			return (ImVec4*)self->TexUvLines;
		}
		static inline void FieldSet__TexUvLines(ImFontAtlas* self, ImVec4* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 33; i++)
			{
				self->TexUvLines[i] = value[i];
			}
		}
		static inline int FieldGet__TexNextUniqueID(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TexNextUniqueID;
		}
		static inline void FieldSet__TexNextUniqueID(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexNextUniqueID = value;
		}
		static inline int FieldGet__FontNextUniqueID(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->FontNextUniqueID;
		}
		static inline void FieldSet__FontNextUniqueID(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontNextUniqueID = value;
		}
		static inline char* FieldGet__FontLoaderName(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->FontLoaderName;
		}
		static inline void FieldSet__FontLoaderName(ImFontAtlas* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontLoaderName = value;
		}
		static inline void* FieldGet__FontLoaderData(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->FontLoaderData;
		}
		static inline void FieldSet__FontLoaderData(ImFontAtlas* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontLoaderData = value;
		}
		static inline unsigned int FieldGet__FontLoaderFlags(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->FontLoaderFlags;
		}
		static inline void FieldSet__FontLoaderFlags(ImFontAtlas* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontLoaderFlags = value;
		}
		static inline int FieldGet__RefCount(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->RefCount;
		}
		static inline void FieldSet__RefCount(ImFontAtlas* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->RefCount = value;
		}
		static inline ImFont* AddFont(ImFontAtlas* self, const ImFontConfig* font_cfg)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->AddFont(font_cfg);
		}
		static inline ImFont* AddFontDefault(ImFontAtlas* self, const ImFontConfig* font_cfg)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->AddFontDefault(font_cfg);
		}
		static inline ImFont* AddFontDefaultVector(ImFontAtlas* self, const ImFontConfig* font_cfg)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->AddFontDefaultVector(font_cfg);
		}
		static inline ImFont* AddFontDefaultBitmap(ImFontAtlas* self, const ImFontConfig* font_cfg)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->AddFontDefaultBitmap(font_cfg);
		}
		static inline ImFont* AddFontFromFileTTF(ImFontAtlas* self, const char* filename,float size_pixels,const ImFontConfig* font_cfg,const ImWchar* glyph_ranges)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->AddFontFromFileTTF(filename, size_pixels, font_cfg, glyph_ranges);
		}
		static inline ImFont* AddFontFromMemoryTTF(ImFontAtlas* self, void* font_data,int font_data_size,float size_pixels,const ImFontConfig* font_cfg,const ImWchar* glyph_ranges)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->AddFontFromMemoryTTF(font_data, font_data_size, size_pixels, font_cfg, glyph_ranges);
		}
		static inline ImFont* AddFontFromMemoryCompressedTTF(ImFontAtlas* self, const void* compressed_font_data,int compressed_font_data_size,float size_pixels,const ImFontConfig* font_cfg,const ImWchar* glyph_ranges)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->AddFontFromMemoryCompressedTTF(compressed_font_data, compressed_font_data_size, size_pixels, font_cfg, glyph_ranges);
		}
		static inline ImFont* AddFontFromMemoryCompressedBase85TTF(ImFontAtlas* self, const char* compressed_font_data_base85,float size_pixels,const ImFontConfig* font_cfg,const ImWchar* glyph_ranges)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImFont*>();
			}
			return (ImFont*)self->AddFontFromMemoryCompressedBase85TTF(compressed_font_data_base85, size_pixels, font_cfg, glyph_ranges);
		}
		static inline void RemoveFont(ImFontAtlas* self, ImFont* font)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->RemoveFont(font);
		}
		static inline void Clear(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Clear();
		}
		static inline void CompactCache(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->CompactCache();
		}
		static inline void ClearInputData(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearInputData();
		}
		static inline void ClearFonts(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearFonts();
		}
		static inline void ClearTexData(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearTexData();
		}
		static inline bool Build(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->Build();
		}
		static inline void GetTexDataAsAlpha8(ImFontAtlas* self, unsigned char** out_pixels,int* out_width,int* out_height,int* out_bytes_per_pixel)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->GetTexDataAsAlpha8(out_pixels, out_width, out_height, out_bytes_per_pixel);
		}
		static inline void GetTexDataAsRGBA32(ImFontAtlas* self, unsigned char** out_pixels,int* out_width,int* out_height,int* out_bytes_per_pixel)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->GetTexDataAsRGBA32(out_pixels, out_width, out_height, out_bytes_per_pixel);
		}
		static inline void SetTexID(ImFontAtlas* self, unsigned long long id)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetTexID(id);
		}
		static inline void SetTexID(ImFontAtlas* self, ImTextureRef id)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetTexID(id);
		}
		static inline bool IsBuilt(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsBuilt();
		}
		static inline ImWchar* GetGlyphRangesDefault(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesDefault();
		}
		static inline ImWchar* GetGlyphRangesGreek(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesGreek();
		}
		static inline ImWchar* GetGlyphRangesKorean(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesKorean();
		}
		static inline ImWchar* GetGlyphRangesJapanese(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesJapanese();
		}
		static inline ImWchar* GetGlyphRangesChineseFull(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesChineseFull();
		}
		static inline ImWchar* GetGlyphRangesChineseSimplifiedCommon(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesChineseSimplifiedCommon();
		}
		static inline ImWchar* GetGlyphRangesCyrillic(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesCyrillic();
		}
		static inline ImWchar* GetGlyphRangesThai(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesThai();
		}
		static inline ImWchar* GetGlyphRangesVietnamese(ImFontAtlas* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar*>();
			}
			return (ImWchar*)self->GetGlyphRangesVietnamese();
		}
		static inline void RemoveCustomRect(ImFontAtlas* self, int id)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->RemoveCustomRect(id);
		}
		static inline int AddCustomRectRegular(ImFontAtlas* self, int w,int h)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->AddCustomRectRegular(w, h);
		}
		static inline int AddCustomRectFontGlyph(ImFontAtlas* self, ImFont* font,ImWchar codepoint,int w,int h,float advance_x,const ImVec2* offset)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->AddCustomRectFontGlyph(font, codepoint, w, h, advance_x, *offset);
		}
		static inline int AddCustomRectFontGlyphForSize(ImFontAtlas* self, ImFont* font,float font_size,ImWchar codepoint,int w,int h,float advance_x,const ImVec2* offset)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->AddCustomRectFontGlyphForSize(font, font_size, codepoint, w, h, advance_x, *offset);
		}
	};
}


extern "C" VFX_API ImFontAtlas* TitanImGui_ImFontAtlas_Visitor_CreateInstance_2960189489()
{
	return ImFontAtlas_Visitor::CreateInstance();
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_Dispose(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::Dispose(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImFontAtlas_Visitor_GetTypeRtti()
{
	return GetClassObject<ImFontAtlas>();
}


extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__Flags(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__Flags(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__Flags(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__Flags(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexGlyphPadding(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__TexGlyphPadding(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexGlyphPadding(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__TexGlyphPadding(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMinWidth(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__TexMinWidth(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMinWidth(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__TexMinWidth(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMinHeight(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__TexMinHeight(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMinHeight(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__TexMinHeight(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMaxWidth(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__TexMaxWidth(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMaxWidth(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__TexMaxWidth(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMaxHeight(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__TexMaxHeight(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMaxHeight(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__TexMaxHeight(self, value);
}
extern "C" VFX_API void* TitanImGui_ImFontAtlas_Visitor_FieldGet__UserData(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__UserData(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__UserData(ImFontAtlas* self, void* value)
{
	ImFontAtlas_Visitor::FieldSet__UserData(self, value);
}
extern "C" VFX_API char TitanImGui_ImFontAtlas_Visitor_FieldGet__Locked(ImFontAtlas* self)
{
	auto tmp_result = ImFontAtlas_Visitor::FieldGet__Locked(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__Locked(ImFontAtlas* self, bool value)
{
	ImFontAtlas_Visitor::FieldSet__Locked(self, value);
}
extern "C" VFX_API char TitanImGui_ImFontAtlas_Visitor_FieldGet__RendererHasTextures(ImFontAtlas* self)
{
	auto tmp_result = ImFontAtlas_Visitor::FieldGet__RendererHasTextures(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__RendererHasTextures(ImFontAtlas* self, bool value)
{
	ImFontAtlas_Visitor::FieldSet__RendererHasTextures(self, value);
}
extern "C" VFX_API char TitanImGui_ImFontAtlas_Visitor_FieldGet__TexIsBuilt(ImFontAtlas* self)
{
	auto tmp_result = ImFontAtlas_Visitor::FieldGet__TexIsBuilt(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexIsBuilt(ImFontAtlas* self, bool value)
{
	ImFontAtlas_Visitor::FieldSet__TexIsBuilt(self, value);
}
extern "C" VFX_API char TitanImGui_ImFontAtlas_Visitor_FieldGet__TexPixelsUseColors(ImFontAtlas* self)
{
	auto tmp_result = ImFontAtlas_Visitor::FieldGet__TexPixelsUseColors(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexPixelsUseColors(ImFontAtlas* self, bool value)
{
	ImFontAtlas_Visitor::FieldSet__TexPixelsUseColors(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvScale(ImFontAtlas* self)
{
	auto tmp_result = ImFontAtlas_Visitor::FieldGet__TexUvScale(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvScale(ImFontAtlas* self, ImVec2 value)
{
	ImFontAtlas_Visitor::FieldSet__TexUvScale(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvWhitePixel(ImFontAtlas* self)
{
	auto tmp_result = ImFontAtlas_Visitor::FieldGet__TexUvWhitePixel(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvWhitePixel(ImFontAtlas* self, ImVec2 value)
{
	ImFontAtlas_Visitor::FieldSet__TexUvWhitePixel(self, value);
}
extern "C" VFX_API ImVec4* TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvLines(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__TexUvLines(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvLines(ImFontAtlas* self, ImVec4* value)
{
	ImFontAtlas_Visitor::FieldSet__TexUvLines(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexNextUniqueID(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__TexNextUniqueID(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexNextUniqueID(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__TexNextUniqueID(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__FontNextUniqueID(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__FontNextUniqueID(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__FontNextUniqueID(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__FontNextUniqueID(self, value);
}
extern "C" VFX_API char* TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderName(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__FontLoaderName(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderName(ImFontAtlas* self, char* value)
{
	ImFontAtlas_Visitor::FieldSet__FontLoaderName(self, value);
}
extern "C" VFX_API void* TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderData(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__FontLoaderData(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderData(ImFontAtlas* self, void* value)
{
	ImFontAtlas_Visitor::FieldSet__FontLoaderData(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderFlags(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__FontLoaderFlags(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderFlags(ImFontAtlas* self, unsigned int value)
{
	ImFontAtlas_Visitor::FieldSet__FontLoaderFlags(self, value);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_FieldGet__RefCount(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::FieldGet__RefCount(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_FieldSet__RefCount(ImFontAtlas* self, int value)
{
	ImFontAtlas_Visitor::FieldSet__RefCount(self, value);
}


extern "C" VFX_API ImFont* TitanImGui_ImFontAtlas_Visitor_AddFont_2476619678(ImFontAtlas* self, const ImFontConfig* font_cfg)
{
	return ImFontAtlas_Visitor::AddFont(self, font_cfg);
}
extern "C" VFX_API ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontDefault_2476619678(ImFontAtlas* self, const ImFontConfig* font_cfg)
{
	return ImFontAtlas_Visitor::AddFontDefault(self, font_cfg);
}
extern "C" VFX_API ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontDefaultVector_2476619678(ImFontAtlas* self, const ImFontConfig* font_cfg)
{
	return ImFontAtlas_Visitor::AddFontDefaultVector(self, font_cfg);
}
extern "C" VFX_API ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontDefaultBitmap_2476619678(ImFontAtlas* self, const ImFontConfig* font_cfg)
{
	return ImFontAtlas_Visitor::AddFontDefaultBitmap(self, font_cfg);
}
extern "C" VFX_API ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontFromFileTTF_293350025(ImFontAtlas* self, const char* filename,float size_pixels,const ImFontConfig* font_cfg,const ImWchar* glyph_ranges)
{
	return ImFontAtlas_Visitor::AddFontFromFileTTF(self, filename, size_pixels, font_cfg, glyph_ranges);
}
extern "C" VFX_API ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryTTF_561799227(ImFontAtlas* self, void* font_data,int font_data_size,float size_pixels,const ImFontConfig* font_cfg,const ImWchar* glyph_ranges)
{
	return ImFontAtlas_Visitor::AddFontFromMemoryTTF(self, font_data, font_data_size, size_pixels, font_cfg, glyph_ranges);
}
extern "C" VFX_API ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryCompressedTTF_1500108386(ImFontAtlas* self, const void* compressed_font_data,int compressed_font_data_size,float size_pixels,const ImFontConfig* font_cfg,const ImWchar* glyph_ranges)
{
	return ImFontAtlas_Visitor::AddFontFromMemoryCompressedTTF(self, compressed_font_data, compressed_font_data_size, size_pixels, font_cfg, glyph_ranges);
}
extern "C" VFX_API ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryCompressedBase85TTF_293350025(ImFontAtlas* self, const char* compressed_font_data_base85,float size_pixels,const ImFontConfig* font_cfg,const ImWchar* glyph_ranges)
{
	return ImFontAtlas_Visitor::AddFontFromMemoryCompressedBase85TTF(self, compressed_font_data_base85, size_pixels, font_cfg, glyph_ranges);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_RemoveFont_2187443828(ImFontAtlas* self, ImFont* font)
{
	return ImFontAtlas_Visitor::RemoveFont(self, font);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_Clear_2960189489(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::Clear(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_CompactCache_2960189489(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::CompactCache(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_ClearInputData_2960189489(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::ClearInputData(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_ClearFonts_2960189489(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::ClearFonts(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_ClearTexData_2960189489(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::ClearTexData(self);
}
extern "C" VFX_API char TitanImGui_ImFontAtlas_Visitor_Build_1117990983(ImFontAtlas* self)
{
	auto tmp_result = ImFontAtlas_Visitor::Build(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_GetTexDataAsAlpha8_3038095861(ImFontAtlas* self, unsigned char** out_pixels,int* out_width,int* out_height,int* out_bytes_per_pixel)
{
	return ImFontAtlas_Visitor::GetTexDataAsAlpha8(self, out_pixels, out_width, out_height, out_bytes_per_pixel);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_GetTexDataAsRGBA32_3038095861(ImFontAtlas* self, unsigned char** out_pixels,int* out_width,int* out_height,int* out_bytes_per_pixel)
{
	return ImFontAtlas_Visitor::GetTexDataAsRGBA32(self, out_pixels, out_width, out_height, out_bytes_per_pixel);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_SetTexID_3315747347(ImFontAtlas* self, unsigned long long id)
{
	return ImFontAtlas_Visitor::SetTexID(self, id);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_SetTexID_2544618575(ImFontAtlas* self, ImTextureRef id)
{
	return ImFontAtlas_Visitor::SetTexID(self, id);
}
extern "C" VFX_API char TitanImGui_ImFontAtlas_Visitor_IsBuilt_82051314(ImFontAtlas* self)
{
	auto tmp_result = ImFontAtlas_Visitor::IsBuilt(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesDefault_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesDefault(self);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesGreek_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesGreek(self);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesKorean_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesKorean(self);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesJapanese_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesJapanese(self);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesChineseFull_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesChineseFull(self);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesChineseSimplifiedCommon_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesChineseSimplifiedCommon(self);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesCyrillic_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesCyrillic(self);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesThai_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesThai(self);
}
extern "C" VFX_API ImWchar* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesVietnamese_1075134865(ImFontAtlas* self)
{
	return ImFontAtlas_Visitor::GetGlyphRangesVietnamese(self);
}
extern "C" VFX_API void TitanImGui_ImFontAtlas_Visitor_RemoveCustomRect_3788306356(ImFontAtlas* self, int id)
{
	return ImFontAtlas_Visitor::RemoveCustomRect(self, id);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_AddCustomRectRegular_2078294092(ImFontAtlas* self, int w,int h)
{
	return ImFontAtlas_Visitor::AddCustomRectRegular(self, w, h);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_AddCustomRectFontGlyph_1754010483(ImFontAtlas* self, ImFont* font,ImWchar codepoint,int w,int h,float advance_x,const ImVec2* offset)
{
	return ImFontAtlas_Visitor::AddCustomRectFontGlyph(self, font, codepoint, w, h, advance_x, offset);
}
extern "C" VFX_API int TitanImGui_ImFontAtlas_Visitor_AddCustomRectFontGlyphForSize_986229207(ImFontAtlas* self, ImFont* font,float font_size,ImWchar codepoint,int w,int h,float advance_x,const ImVec2* offset)
{
	return ImFontAtlas_Visitor::AddCustomRectFontGlyphForSize(self, font, font_size, codepoint, w, h, advance_x, offset);
}
#endif//HasModule_ImGui
