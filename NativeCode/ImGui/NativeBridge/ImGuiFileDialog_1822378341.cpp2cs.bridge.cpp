//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui_filedialog.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiFileDialog_Visitor
	{
		static inline ImGui::ImGuiFileDialog* CreateInstance()
		{
			return new ImGui::ImGuiFileDialog();
		}
		static void Dispose(ImGui::ImGuiFileDialog* self)
		{
			delete self;
		}
		static inline void OpenDialog(ImGui::ImGuiFileDialog* self, const char* vKey,const char* vTitle,const char* vFilters,const char* vPath)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->OpenDialog(vKey, vTitle, vFilters, vPath);
		}
		static inline void OpenModal(ImGui::ImGuiFileDialog* self, const char* vKey,const char* vTitle,const char* vFilters,const char* vPath)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->OpenModal(vKey, vTitle, vFilters, vPath);
		}
		static inline void OpenModalWithMutiSelect(ImGui::ImGuiFileDialog* self, const char* vKey,const char* vTitle,const char* vFilters,const char* vPath,int vCountSelectionMax)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->OpenModalWithMutiSelect(vKey, vTitle, vFilters, vPath, vCountSelectionMax);
		}
		static inline bool DisplayDialog(ImGui::ImGuiFileDialog* self, const char* vKey)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->DisplayDialog(vKey);
		}
		static inline void CloseDialog(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->CloseDialog();
		}
		static inline bool IsOk(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsOk();
		}
		static inline bool WasKeyOpenedThisFrame(ImGui::ImGuiFileDialog* self, const char* vKey)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WasKeyOpenedThisFrame(vKey);
		}
		static inline bool WasOpenedThisFrame(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->WasOpenedThisFrame();
		}
		static inline bool IsKeyOpened(ImGui::ImGuiFileDialog* self, const char* vCurrentOpenedKey)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsKeyOpened(vCurrentOpenedKey);
		}
		static inline bool IsOpened(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsOpened();
		}
		static inline int GetSelectedCount(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->GetSelectedCount();
		}
		static inline char* GetFilePathByIndex(ImGui::ImGuiFileDialog* self, int index)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->GetFilePathByIndex(index);
		}
		static inline char* GetFilePathName(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->GetFilePathName();
		}
		static inline char* GetCurrentFileName(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->GetCurrentFileName();
		}
		static inline char* GetCurrentPath(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->GetCurrentPath();
		}
		static inline char* GetCurrentFilter(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->GetCurrentFilter();
		}
		static inline void* GetUserDatas(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->GetUserDatas();
		}
		static inline void SetFileStyle(ImGui::ImGuiFileDialog* self, const char* vFilter,ImVec4 vColor,const char* vIconText)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetFileStyle(vFilter, vColor, vIconText);
		}
		static inline bool GetFileStyle(ImGui::ImGuiFileDialog* self, const char* vFilter,ImVec4* vOutColor,char** vOutIconText)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->GetFileStyle(vFilter, vOutColor, vOutIconText);
		}
		static inline void ClearFilesStyle(ImGui::ImGuiFileDialog* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ClearFilesStyle();
		}
	};
}


extern "C" VFX_API ImGui::ImGuiFileDialog* TitanImGui_ImGuiFileDialog_Visitor_CreateInstance_2960189489()
{
	return ImGuiFileDialog_Visitor::CreateInstance();
}
extern "C" VFX_API void TitanImGui_ImGuiFileDialog_Visitor_Dispose(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::Dispose(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiFileDialog_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGui::ImGuiFileDialog>();
}




extern "C" VFX_API void TitanImGui_ImGuiFileDialog_Visitor_OpenDialog_3146595389(ImGui::ImGuiFileDialog* self, const char* vKey,const char* vTitle,const char* vFilters,const char* vPath)
{
	return ImGuiFileDialog_Visitor::OpenDialog(self, vKey, vTitle, vFilters, vPath);
}
extern "C" VFX_API void TitanImGui_ImGuiFileDialog_Visitor_OpenModal_3146595389(ImGui::ImGuiFileDialog* self, const char* vKey,const char* vTitle,const char* vFilters,const char* vPath)
{
	return ImGuiFileDialog_Visitor::OpenModal(self, vKey, vTitle, vFilters, vPath);
}
extern "C" VFX_API void TitanImGui_ImGuiFileDialog_Visitor_OpenModalWithMutiSelect_1897522092(ImGui::ImGuiFileDialog* self, const char* vKey,const char* vTitle,const char* vFilters,const char* vPath,int vCountSelectionMax)
{
	return ImGuiFileDialog_Visitor::OpenModalWithMutiSelect(self, vKey, vTitle, vFilters, vPath, vCountSelectionMax);
}
extern "C" VFX_API char TitanImGui_ImGuiFileDialog_Visitor_DisplayDialog_1080422500(ImGui::ImGuiFileDialog* self, const char* vKey)
{
	auto tmp_result = ImGuiFileDialog_Visitor::DisplayDialog(self, vKey);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiFileDialog_Visitor_CloseDialog_2960189489(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::CloseDialog(self);
}
extern "C" VFX_API char TitanImGui_ImGuiFileDialog_Visitor_IsOk_1117990983(ImGui::ImGuiFileDialog* self)
{
	auto tmp_result = ImGuiFileDialog_Visitor::IsOk(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiFileDialog_Visitor_WasKeyOpenedThisFrame_1080422500(ImGui::ImGuiFileDialog* self, const char* vKey)
{
	auto tmp_result = ImGuiFileDialog_Visitor::WasKeyOpenedThisFrame(self, vKey);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiFileDialog_Visitor_WasOpenedThisFrame_1117990983(ImGui::ImGuiFileDialog* self)
{
	auto tmp_result = ImGuiFileDialog_Visitor::WasOpenedThisFrame(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiFileDialog_Visitor_IsKeyOpened_1080422500(ImGui::ImGuiFileDialog* self, const char* vCurrentOpenedKey)
{
	auto tmp_result = ImGuiFileDialog_Visitor::IsKeyOpened(self, vCurrentOpenedKey);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiFileDialog_Visitor_IsOpened_1117990983(ImGui::ImGuiFileDialog* self)
{
	auto tmp_result = ImGuiFileDialog_Visitor::IsOpened(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API int TitanImGui_ImGuiFileDialog_Visitor_GetSelectedCount_2704135706(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::GetSelectedCount(self);
}
extern "C" VFX_API char* TitanImGui_ImGuiFileDialog_Visitor_GetFilePathByIndex_4173149781(ImGui::ImGuiFileDialog* self, int index)
{
	return ImGuiFileDialog_Visitor::GetFilePathByIndex(self, index);
}
extern "C" VFX_API char* TitanImGui_ImGuiFileDialog_Visitor_GetFilePathName_2396230038(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::GetFilePathName(self);
}
extern "C" VFX_API char* TitanImGui_ImGuiFileDialog_Visitor_GetCurrentFileName_2396230038(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::GetCurrentFileName(self);
}
extern "C" VFX_API char* TitanImGui_ImGuiFileDialog_Visitor_GetCurrentPath_2396230038(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::GetCurrentPath(self);
}
extern "C" VFX_API char* TitanImGui_ImGuiFileDialog_Visitor_GetCurrentFilter_2396230038(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::GetCurrentFilter(self);
}
extern "C" VFX_API void* TitanImGui_ImGuiFileDialog_Visitor_GetUserDatas_302642963(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::GetUserDatas(self);
}
extern "C" VFX_API void TitanImGui_ImGuiFileDialog_Visitor_SetFileStyle_2880679569(ImGui::ImGuiFileDialog* self, const char* vFilter,ImVec4 vColor,const char* vIconText)
{
	return ImGuiFileDialog_Visitor::SetFileStyle(self, vFilter, vColor, vIconText);
}
extern "C" VFX_API char TitanImGui_ImGuiFileDialog_Visitor_GetFileStyle_786079400(ImGui::ImGuiFileDialog* self, const char* vFilter,ImVec4* vOutColor,char** vOutIconText)
{
	auto tmp_result = ImGuiFileDialog_Visitor::GetFileStyle(self, vFilter, vOutColor, vOutIconText);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiFileDialog_Visitor_ClearFilesStyle_2960189489(ImGui::ImGuiFileDialog* self)
{
	return ImGuiFileDialog_Visitor::ClearFilesStyle(self);
}
#endif//HasModule_ImGui
