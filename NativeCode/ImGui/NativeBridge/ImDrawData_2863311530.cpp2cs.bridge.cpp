//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImDrawData_Visitor
	{
		static void UnsafeCallConstructor(ImDrawData* self)
		{
			#undef new
			new (self)ImDrawData();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImDrawData* self)
		{
		}
		static inline bool FieldGet__Valid(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->Valid;
		}
		static inline void FieldSet__Valid(ImDrawData* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Valid = value;
		}
		static inline int FieldGet__CmdListsCount(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->CmdListsCount;
		}
		static inline void FieldSet__CmdListsCount(ImDrawData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CmdListsCount = value;
		}
		static inline int FieldGet__TotalIdxCount(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TotalIdxCount;
		}
		static inline void FieldSet__TotalIdxCount(ImDrawData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TotalIdxCount = value;
		}
		static inline int FieldGet__TotalVtxCount(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TotalVtxCount;
		}
		static inline void FieldSet__TotalVtxCount(ImDrawData* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TotalVtxCount = value;
		}
		static inline ImVec2 FieldGet__DisplayPos(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->DisplayPos;
		}
		static inline void FieldSet__DisplayPos(ImDrawData* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DisplayPos = value;
		}
		static inline ImVec2 FieldGet__DisplaySize(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->DisplaySize;
		}
		static inline void FieldSet__DisplaySize(ImDrawData* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DisplaySize = value;
		}
		static inline ImVec2 FieldGet__FramebufferScale(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->FramebufferScale;
		}
		static inline void FieldSet__FramebufferScale(ImDrawData* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FramebufferScale = value;
		}
		static inline ImGuiViewport* FieldGet__OwnerViewport(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImGuiViewport*>();
			}
			return (ImGuiViewport*)self->OwnerViewport;
		}
		static inline void FieldSet__OwnerViewport(ImDrawData* self, ImGuiViewport* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->OwnerViewport = value;
		}
		static inline void Clear(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Clear();
		}
		static inline void AddDrawList(ImDrawData* self, ImDrawList* draw_list)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->AddDrawList(draw_list);
		}
		static inline void DeIndexAllBuffers(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->DeIndexAllBuffers();
		}
		static inline void ScaleClipRects(ImDrawData* self, const ImVec2* fb_scale)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ScaleClipRects(*fb_scale);
		}
		static inline ImDrawList** GetCmdLists(ImDrawData* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImDrawList**>();
			}
			return (ImDrawList**)self->GetCmdLists();
		}
	};
}


extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_UnsafeCallConstructor_2960189489(ImDrawData* self)
{
	return ImDrawData_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_UnsafeCallDestructor(ImDrawData* self)
{
	return ImDrawData_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImDrawData_Visitor_GetTypeRtti()
{
	return GetClassObject<ImDrawData>();
}


extern "C" VFX_API char TitanImGui_ImDrawData_Visitor_FieldGet__Valid(ImDrawData* self)
{
	auto tmp_result = ImDrawData_Visitor::FieldGet__Valid(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_FieldSet__Valid(ImDrawData* self, bool value)
{
	ImDrawData_Visitor::FieldSet__Valid(self, value);
}
extern "C" VFX_API int TitanImGui_ImDrawData_Visitor_FieldGet__CmdListsCount(ImDrawData* self)
{
	return ImDrawData_Visitor::FieldGet__CmdListsCount(self);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_FieldSet__CmdListsCount(ImDrawData* self, int value)
{
	ImDrawData_Visitor::FieldSet__CmdListsCount(self, value);
}
extern "C" VFX_API int TitanImGui_ImDrawData_Visitor_FieldGet__TotalIdxCount(ImDrawData* self)
{
	return ImDrawData_Visitor::FieldGet__TotalIdxCount(self);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_FieldSet__TotalIdxCount(ImDrawData* self, int value)
{
	ImDrawData_Visitor::FieldSet__TotalIdxCount(self, value);
}
extern "C" VFX_API int TitanImGui_ImDrawData_Visitor_FieldGet__TotalVtxCount(ImDrawData* self)
{
	return ImDrawData_Visitor::FieldGet__TotalVtxCount(self);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_FieldSet__TotalVtxCount(ImDrawData* self, int value)
{
	ImDrawData_Visitor::FieldSet__TotalVtxCount(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImDrawData_Visitor_FieldGet__DisplayPos(ImDrawData* self)
{
	auto tmp_result = ImDrawData_Visitor::FieldGet__DisplayPos(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_FieldSet__DisplayPos(ImDrawData* self, ImVec2 value)
{
	ImDrawData_Visitor::FieldSet__DisplayPos(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImDrawData_Visitor_FieldGet__DisplaySize(ImDrawData* self)
{
	auto tmp_result = ImDrawData_Visitor::FieldGet__DisplaySize(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_FieldSet__DisplaySize(ImDrawData* self, ImVec2 value)
{
	ImDrawData_Visitor::FieldSet__DisplaySize(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImDrawData_Visitor_FieldGet__FramebufferScale(ImDrawData* self)
{
	auto tmp_result = ImDrawData_Visitor::FieldGet__FramebufferScale(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_FieldSet__FramebufferScale(ImDrawData* self, ImVec2 value)
{
	ImDrawData_Visitor::FieldSet__FramebufferScale(self, value);
}
extern "C" VFX_API ImGuiViewport* TitanImGui_ImDrawData_Visitor_FieldGet__OwnerViewport(ImDrawData* self)
{
	return ImDrawData_Visitor::FieldGet__OwnerViewport(self);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_FieldSet__OwnerViewport(ImDrawData* self, ImGuiViewport* value)
{
	ImDrawData_Visitor::FieldSet__OwnerViewport(self, value);
}


extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_Clear_2960189489(ImDrawData* self)
{
	return ImDrawData_Visitor::Clear(self);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_AddDrawList_64523837(ImDrawData* self, ImDrawList* draw_list)
{
	return ImDrawData_Visitor::AddDrawList(self, draw_list);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_DeIndexAllBuffers_2960189489(ImDrawData* self)
{
	return ImDrawData_Visitor::DeIndexAllBuffers(self);
}
extern "C" VFX_API void TitanImGui_ImDrawData_Visitor_ScaleClipRects_2086025684(ImDrawData* self, const ImVec2* fb_scale)
{
	return ImDrawData_Visitor::ScaleClipRects(self, fb_scale);
}
extern "C" VFX_API ImDrawList** TitanImGui_ImDrawData_Visitor_GetCmdLists_1014668303(ImDrawData* self)
{
	return ImDrawData_Visitor::GetCmdLists(self);
}
#endif//HasModule_ImGui
