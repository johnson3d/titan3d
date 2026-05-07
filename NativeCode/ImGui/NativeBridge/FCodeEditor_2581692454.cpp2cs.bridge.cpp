//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../ColorTextEditor/TextEditor.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct FCodeEditor_Visitor
	{
		static inline EngineNS::FCodeEditor* CreateInstance()
		{
			return new EngineNS::FCodeEditor();
		}
		static inline EngineNS::VIUnknown* CastTo_EngineNS_VIUnknown(EngineNS::FCodeEditor* self)
		{
			return static_cast<EngineNS::VIUnknown*>(self);
		}
		static inline void SetLanguage(EngineNS::FCodeEditor* self, const char* type)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetLanguage(type);
		}
		static inline void PushPreprocIdentifier(EngineNS::FCodeEditor* self, const char* name,const char* value)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PushPreprocIdentifier(name, value);
		}
		static inline void PushIdentifier(EngineNS::FCodeEditor* self, const char* name,const char* value)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PushIdentifier(name, value);
		}
		static inline void ApplyLangDefine(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ApplyLangDefine();
		}
		static inline void PushErrorMarker(EngineNS::FCodeEditor* self, int index,const char* info)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->PushErrorMarker(index, info);
		}
		static inline void ApplyErrorMarkers(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ApplyErrorMarkers();
		}
		static inline void Render(EngineNS::FCodeEditor* self, const char* aTitle,const ImVec2* aSize,bool aBorder)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Render(aTitle, *aSize, aBorder);
		}
		static inline void SetText(EngineNS::FCodeEditor* self, const char* aText)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetText(aText);
		}
		static inline void GetText(EngineNS::FCodeEditor* self, EngineNS::IBlobObject* blob)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->GetText(blob);
		}
		static inline char* GetTextPointer(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->GetTextPointer();
		}
		static inline void Undo(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Undo();
		}
		static inline void Redo(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Redo();
		}
		static inline void Copy(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Copy();
		}
		static inline void Cut(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Cut();
		}
		static inline void Delete(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Delete();
		}
		static inline void Paste(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Paste();
		}
		static inline void SelectAll(EngineNS::FCodeEditor* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SelectAll();
		}
		static inline void SetViewStyle(EngineNS::FCodeEditor* self, const char* view)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetViewStyle(view);
		}
	};
}


extern "C" VFX_API EngineNS::FCodeEditor* TitanImGui_FCodeEditor_Visitor_CreateInstance_2960189489()
{
	return FCodeEditor_Visitor::CreateInstance();
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_FCodeEditor_Visitor_GetTypeRtti()
{
	return GetClassObject<EngineNS::FCodeEditor>();
}
extern "C" VFX_API EngineNS::VIUnknown* TitanImGui_FCodeEditor_Visitor_CastTo_EngineNS_VIUnknown(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::CastTo_EngineNS_VIUnknown(self);
}




extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_SetLanguage_2602414842(EngineNS::FCodeEditor* self, const char* type)
{
	return FCodeEditor_Visitor::SetLanguage(self, type);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_PushPreprocIdentifier_568371421(EngineNS::FCodeEditor* self, const char* name,const char* value)
{
	return FCodeEditor_Visitor::PushPreprocIdentifier(self, name, value);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_PushIdentifier_568371421(EngineNS::FCodeEditor* self, const char* name,const char* value)
{
	return FCodeEditor_Visitor::PushIdentifier(self, name, value);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_ApplyLangDefine_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::ApplyLangDefine(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_PushErrorMarker_85734681(EngineNS::FCodeEditor* self, int index,const char* info)
{
	return FCodeEditor_Visitor::PushErrorMarker(self, index, info);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_ApplyErrorMarkers_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::ApplyErrorMarkers(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_Render_1399928045(EngineNS::FCodeEditor* self, const char* aTitle,const ImVec2* aSize,bool aBorder)
{
	return FCodeEditor_Visitor::Render(self, aTitle, aSize, aBorder);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_SetText_2602414842(EngineNS::FCodeEditor* self, const char* aText)
{
	return FCodeEditor_Visitor::SetText(self, aText);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_GetText_2413885944(EngineNS::FCodeEditor* self, EngineNS::IBlobObject* blob)
{
	return FCodeEditor_Visitor::GetText(self, blob);
}
extern "C" VFX_API char* TitanImGui_FCodeEditor_Visitor_GetTextPointer_2396230038(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::GetTextPointer(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_Undo_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::Undo(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_Redo_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::Redo(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_Copy_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::Copy(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_Cut_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::Cut(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_Delete_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::Delete(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_Paste_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::Paste(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_SelectAll_2960189489(EngineNS::FCodeEditor* self)
{
	return FCodeEditor_Visitor::SelectAll(self);
}
extern "C" VFX_API void TitanImGui_FCodeEditor_Visitor_SetViewStyle_2602414842(EngineNS::FCodeEditor* self, const char* view)
{
	return FCodeEditor_Visitor::SetViewStyle(self, view);
}
#endif//HasModule_ImGui
