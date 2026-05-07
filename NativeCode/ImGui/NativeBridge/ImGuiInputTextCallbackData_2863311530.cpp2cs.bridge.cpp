//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiInputTextCallbackData_Visitor
	{
		static void UnsafeCallConstructor(ImGuiInputTextCallbackData* self)
		{
			#undef new
			new (self)ImGuiInputTextCallbackData();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImGuiInputTextCallbackData* self)
		{
		}
		static inline int FieldGet__EventFlag(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->EventFlag;
		}
		static inline void FieldSet__EventFlag(ImGuiInputTextCallbackData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->EventFlag = value;
		}
		static inline int FieldGet__Flags(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Flags;
		}
		static inline void FieldSet__Flags(ImGuiInputTextCallbackData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Flags = value;
		}
		static inline void* FieldGet__UserData(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->UserData;
		}
		static inline void FieldSet__UserData(ImGuiInputTextCallbackData* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->UserData = value;
		}
		static inline unsigned int FieldGet__ID(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->ID;
		}
		static inline void FieldSet__ID(ImGuiInputTextCallbackData* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ID = value;
		}
		static inline ImGuiKey FieldGet__EventKey(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImGuiKey>();
			}
			return (ImGuiKey)self->EventKey;
		}
		static inline void FieldSet__EventKey(ImGuiInputTextCallbackData* self, ImGuiKey value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->EventKey = value;
		}
		static inline ImWchar FieldGet__EventChar(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImWchar>();
			}
			return (ImWchar)self->EventChar;
		}
		static inline void FieldSet__EventChar(ImGuiInputTextCallbackData* self, ImWchar value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->EventChar = value;
		}
		static inline bool FieldGet__EventActivated(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->EventActivated;
		}
		static inline void FieldSet__EventActivated(ImGuiInputTextCallbackData* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->EventActivated = value;
		}
		static inline bool FieldGet__BufDirty(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->BufDirty;
		}
		static inline void FieldSet__BufDirty(ImGuiInputTextCallbackData* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BufDirty = value;
		}
		static inline char* FieldGet__Buf(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->Buf;
		}
		static inline void FieldSet__Buf(ImGuiInputTextCallbackData* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Buf = value;
		}
		static inline int FieldGet__BufTextLen(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->BufTextLen;
		}
		static inline void FieldSet__BufTextLen(ImGuiInputTextCallbackData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BufTextLen = value;
		}
		static inline int FieldGet__BufSize(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->BufSize;
		}
		static inline void FieldSet__BufSize(ImGuiInputTextCallbackData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->BufSize = value;
		}
		static inline int FieldGet__CursorPos(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->CursorPos;
		}
		static inline void FieldSet__CursorPos(ImGuiInputTextCallbackData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CursorPos = value;
		}
		static inline int FieldGet__SelectionStart(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->SelectionStart;
		}
		static inline void FieldSet__SelectionStart(ImGuiInputTextCallbackData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SelectionStart = value;
		}
		static inline int FieldGet__SelectionEnd(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->SelectionEnd;
		}
		static inline void FieldSet__SelectionEnd(ImGuiInputTextCallbackData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SelectionEnd = value;
		}
		static inline void DeleteChars(ImGuiInputTextCallbackData* self, int pos,int bytes_count)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->DeleteChars(pos, bytes_count);
		}
		static inline void InsertChars(ImGuiInputTextCallbackData* self, int pos,const char* text,const char* text_end)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->InsertChars(pos, text, text_end);
		}
		static inline void SelectAll(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SelectAll();
		}
		static inline void SetSelection(ImGuiInputTextCallbackData* self, int s,int e)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetSelection(s, e);
		}
		static inline void ClearSelection(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearSelection();
		}
		static inline bool HasSelection(ImGuiInputTextCallbackData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->HasSelection();
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_UnsafeCallConstructor_2960189489(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_UnsafeCallDestructor(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiInputTextCallbackData_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiInputTextCallbackData>();
}


extern "C" VFX_API int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventFlag(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__EventFlag(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventFlag(ImGuiInputTextCallbackData* self, int value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__EventFlag(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__Flags(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__Flags(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__Flags(ImGuiInputTextCallbackData* self, int value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__Flags(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__UserData(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__UserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__UserData(ImGuiInputTextCallbackData* self, void* value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__UserData(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__ID(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__ID(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__ID(ImGuiInputTextCallbackData* self, unsigned int value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__ID(self, value);
}
extern "C" VFX_API ImGuiKey TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventKey(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__EventKey(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventKey(ImGuiInputTextCallbackData* self, ImGuiKey value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__EventKey(self, value);
}
extern "C" VFX_API ImWchar16 TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventChar(ImGuiInputTextCallbackData* self)
{
	auto tmp_result = ImGuiInputTextCallbackData_Visitor::FieldGet__EventChar(self);
	return EngineNS::VReturnValueMarshal<ImWchar,ImWchar16>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventChar(ImGuiInputTextCallbackData* self, ImWchar value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__EventChar(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__EventActivated(ImGuiInputTextCallbackData* self)
{
	auto tmp_result = ImGuiInputTextCallbackData_Visitor::FieldGet__EventActivated(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__EventActivated(ImGuiInputTextCallbackData* self, bool value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__EventActivated(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufDirty(ImGuiInputTextCallbackData* self)
{
	auto tmp_result = ImGuiInputTextCallbackData_Visitor::FieldGet__BufDirty(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufDirty(ImGuiInputTextCallbackData* self, bool value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__BufDirty(self, value);
}
extern "C" VFX_API char* TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__Buf(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__Buf(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__Buf(ImGuiInputTextCallbackData* self, char* value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__Buf(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufTextLen(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__BufTextLen(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufTextLen(ImGuiInputTextCallbackData* self, int value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__BufTextLen(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__BufSize(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__BufSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__BufSize(ImGuiInputTextCallbackData* self, int value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__BufSize(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__CursorPos(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__CursorPos(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__CursorPos(ImGuiInputTextCallbackData* self, int value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__CursorPos(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__SelectionStart(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__SelectionStart(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__SelectionStart(ImGuiInputTextCallbackData* self, int value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__SelectionStart(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldGet__SelectionEnd(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::FieldGet__SelectionEnd(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_FieldSet__SelectionEnd(ImGuiInputTextCallbackData* self, int value)
{
	ImGuiInputTextCallbackData_Visitor::FieldSet__SelectionEnd(self, value);
}


extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_DeleteChars_3539386109(ImGuiInputTextCallbackData* self, int pos,int bytes_count)
{
	return ImGuiInputTextCallbackData_Visitor::DeleteChars(self, pos, bytes_count);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_InsertChars_3984929356(ImGuiInputTextCallbackData* self, int pos,const char* text,const char* text_end)
{
	return ImGuiInputTextCallbackData_Visitor::InsertChars(self, pos, text, text_end);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_SelectAll_2960189489(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::SelectAll(self);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_SetSelection_3539386109(ImGuiInputTextCallbackData* self, int s,int e)
{
	return ImGuiInputTextCallbackData_Visitor::SetSelection(self, s, e);
}
extern "C" VFX_API void TitanImGui_ImGuiInputTextCallbackData_Visitor_ClearSelection_2960189489(ImGuiInputTextCallbackData* self)
{
	return ImGuiInputTextCallbackData_Visitor::ClearSelection(self);
}
extern "C" VFX_API char TitanImGui_ImGuiInputTextCallbackData_Visitor_HasSelection_82051314(ImGuiInputTextCallbackData* self)
{
	auto tmp_result = ImGuiInputTextCallbackData_Visitor::HasSelection(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
#endif//HasModule_ImGui
