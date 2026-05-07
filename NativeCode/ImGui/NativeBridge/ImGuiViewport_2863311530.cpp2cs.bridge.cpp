//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiViewport_Visitor
	{
		static void UnsafeCallConstructor(ImGuiViewport* self)
		{
			#undef new
			new (self)ImGuiViewport();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImGuiViewport* self)
		{
		}
		static inline unsigned int FieldGet__ID(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->ID;
		}
		static inline void FieldSet__ID(ImGuiViewport* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ID = value;
		}
		static inline int FieldGet__Flags(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->Flags;
		}
		static inline void FieldSet__Flags(ImGuiViewport* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Flags = value;
		}
		static inline ImVec2 FieldGet__Pos(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->Pos;
		}
		static inline void FieldSet__Pos(ImGuiViewport* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Pos = value;
		}
		static inline ImVec2 FieldGet__Size(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->Size;
		}
		static inline void FieldSet__Size(ImGuiViewport* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Size = value;
		}
		static inline ImVec2 FieldGet__FramebufferScale(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->FramebufferScale;
		}
		static inline void FieldSet__FramebufferScale(ImGuiViewport* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FramebufferScale = value;
		}
		static inline ImVec2 FieldGet__WorkPos(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->WorkPos;
		}
		static inline void FieldSet__WorkPos(ImGuiViewport* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WorkPos = value;
		}
		static inline ImVec2 FieldGet__WorkSize(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->WorkSize;
		}
		static inline void FieldSet__WorkSize(ImGuiViewport* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WorkSize = value;
		}
		static inline float FieldGet__DpiScale(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->DpiScale;
		}
		static inline void FieldSet__DpiScale(ImGuiViewport* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DpiScale = value;
		}
		static inline unsigned int FieldGet__ParentViewportId(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->ParentViewportId;
		}
		static inline void FieldSet__ParentViewportId(ImGuiViewport* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ParentViewportId = value;
		}
		static inline ImGuiViewport* FieldGet__ParentViewport(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImGuiViewport*>();
			}
			return (ImGuiViewport*)self->ParentViewport;
		}
		static inline void FieldSet__ParentViewport(ImGuiViewport* self, ImGuiViewport* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ParentViewport = value;
		}
		static inline ImDrawData* FieldGet__DrawData(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImDrawData*>();
			}
			return (ImDrawData*)self->DrawData;
		}
		static inline void FieldSet__DrawData(ImGuiViewport* self, ImDrawData* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DrawData = value;
		}
		static inline void* FieldGet__RendererUserData(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->RendererUserData;
		}
		static inline void FieldSet__RendererUserData(ImGuiViewport* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->RendererUserData = value;
		}
		static inline void* FieldGet__PlatformUserData(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->PlatformUserData;
		}
		static inline void FieldSet__PlatformUserData(ImGuiViewport* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PlatformUserData = value;
		}
		static inline void* FieldGet__PlatformHandle(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->PlatformHandle;
		}
		static inline void FieldSet__PlatformHandle(ImGuiViewport* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PlatformHandle = value;
		}
		static inline void* FieldGet__PlatformHandleRaw(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->PlatformHandleRaw;
		}
		static inline void FieldSet__PlatformHandleRaw(ImGuiViewport* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PlatformHandleRaw = value;
		}
		static inline bool FieldGet__PlatformWindowCreated(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->PlatformWindowCreated;
		}
		static inline void FieldSet__PlatformWindowCreated(ImGuiViewport* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PlatformWindowCreated = value;
		}
		static inline bool FieldGet__PlatformRequestMove(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->PlatformRequestMove;
		}
		static inline void FieldSet__PlatformRequestMove(ImGuiViewport* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PlatformRequestMove = value;
		}
		static inline bool FieldGet__PlatformRequestResize(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->PlatformRequestResize;
		}
		static inline void FieldSet__PlatformRequestResize(ImGuiViewport* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PlatformRequestResize = value;
		}
		static inline bool FieldGet__PlatformRequestClose(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->PlatformRequestClose;
		}
		static inline void FieldSet__PlatformRequestClose(ImGuiViewport* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PlatformRequestClose = value;
		}
		static inline ImVec2 GetCenter(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->GetCenter();
		}
		static inline ImVec2 GetWorkCenter(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->GetWorkCenter();
		}
		static inline char* GetDebugName(ImGuiViewport* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->GetDebugName();
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_UnsafeCallConstructor_2960189489(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_UnsafeCallDestructor(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiViewport_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiViewport>();
}


extern "C" VFX_API unsigned int TitanImGui_ImGuiViewport_Visitor_FieldGet__ID(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__ID(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__ID(ImGuiViewport* self, unsigned int value)
{
	ImGuiViewport_Visitor::FieldSet__ID(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiViewport_Visitor_FieldGet__Flags(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__Flags(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__Flags(ImGuiViewport* self, int value)
{
	ImGuiViewport_Visitor::FieldSet__Flags(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiViewport_Visitor_FieldGet__Pos(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__Pos(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__Pos(ImGuiViewport* self, ImVec2 value)
{
	ImGuiViewport_Visitor::FieldSet__Pos(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiViewport_Visitor_FieldGet__Size(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__Size(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__Size(ImGuiViewport* self, ImVec2 value)
{
	ImGuiViewport_Visitor::FieldSet__Size(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiViewport_Visitor_FieldGet__FramebufferScale(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__FramebufferScale(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__FramebufferScale(ImGuiViewport* self, ImVec2 value)
{
	ImGuiViewport_Visitor::FieldSet__FramebufferScale(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiViewport_Visitor_FieldGet__WorkPos(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__WorkPos(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__WorkPos(ImGuiViewport* self, ImVec2 value)
{
	ImGuiViewport_Visitor::FieldSet__WorkPos(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiViewport_Visitor_FieldGet__WorkSize(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__WorkSize(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__WorkSize(ImGuiViewport* self, ImVec2 value)
{
	ImGuiViewport_Visitor::FieldSet__WorkSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiViewport_Visitor_FieldGet__DpiScale(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__DpiScale(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__DpiScale(ImGuiViewport* self, float value)
{
	ImGuiViewport_Visitor::FieldSet__DpiScale(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiViewport_Visitor_FieldGet__ParentViewportId(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__ParentViewportId(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__ParentViewportId(ImGuiViewport* self, unsigned int value)
{
	ImGuiViewport_Visitor::FieldSet__ParentViewportId(self, value);
}
extern "C" VFX_API ImGuiViewport* TitanImGui_ImGuiViewport_Visitor_FieldGet__ParentViewport(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__ParentViewport(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__ParentViewport(ImGuiViewport* self, ImGuiViewport* value)
{
	ImGuiViewport_Visitor::FieldSet__ParentViewport(self, value);
}
extern "C" VFX_API ImDrawData* TitanImGui_ImGuiViewport_Visitor_FieldGet__DrawData(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__DrawData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__DrawData(ImGuiViewport* self, ImDrawData* value)
{
	ImGuiViewport_Visitor::FieldSet__DrawData(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiViewport_Visitor_FieldGet__RendererUserData(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__RendererUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__RendererUserData(ImGuiViewport* self, void* value)
{
	ImGuiViewport_Visitor::FieldSet__RendererUserData(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformUserData(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__PlatformUserData(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformUserData(ImGuiViewport* self, void* value)
{
	ImGuiViewport_Visitor::FieldSet__PlatformUserData(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformHandle(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__PlatformHandle(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformHandle(ImGuiViewport* self, void* value)
{
	ImGuiViewport_Visitor::FieldSet__PlatformHandle(self, value);
}
extern "C" VFX_API void* TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformHandleRaw(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::FieldGet__PlatformHandleRaw(self);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformHandleRaw(ImGuiViewport* self, void* value)
{
	ImGuiViewport_Visitor::FieldSet__PlatformHandleRaw(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformWindowCreated(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__PlatformWindowCreated(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformWindowCreated(ImGuiViewport* self, bool value)
{
	ImGuiViewport_Visitor::FieldSet__PlatformWindowCreated(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestMove(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__PlatformRequestMove(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestMove(ImGuiViewport* self, bool value)
{
	ImGuiViewport_Visitor::FieldSet__PlatformRequestMove(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestResize(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__PlatformRequestResize(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestResize(ImGuiViewport* self, bool value)
{
	ImGuiViewport_Visitor::FieldSet__PlatformRequestResize(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiViewport_Visitor_FieldGet__PlatformRequestClose(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::FieldGet__PlatformRequestClose(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiViewport_Visitor_FieldSet__PlatformRequestClose(ImGuiViewport* self, bool value)
{
	ImGuiViewport_Visitor::FieldSet__PlatformRequestClose(self, value);
}


extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiViewport_Visitor_GetCenter_3443252160(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::GetCenter(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiViewport_Visitor_GetWorkCenter_3443252160(ImGuiViewport* self)
{
	auto tmp_result = ImGuiViewport_Visitor::GetWorkCenter(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API char* TitanImGui_ImGuiViewport_Visitor_GetDebugName_721684103(ImGuiViewport* self)
{
	return ImGuiViewport_Visitor::GetDebugName(self);
}
#endif//HasModule_ImGui
