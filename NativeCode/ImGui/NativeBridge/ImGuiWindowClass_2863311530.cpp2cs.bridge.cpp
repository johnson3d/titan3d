//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiWindowClass_Visitor
	{
		static void UnsafeCallConstructor(ImGuiWindowClass* self)
		{
			#undef new
			new (self)ImGuiWindowClass();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImGuiWindowClass* self)
		{
		}
		static inline unsigned int FieldGet__ClassId(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->ClassId;
		}
		static inline void FieldSet__ClassId(ImGuiWindowClass* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ClassId = value;
		}
		static inline unsigned int FieldGet__ParentViewportId(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->ParentViewportId;
		}
		static inline void FieldSet__ParentViewportId(ImGuiWindowClass* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ParentViewportId = value;
		}
		static inline unsigned int FieldGet__FocusRouteParentWindowId(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->FocusRouteParentWindowId;
		}
		static inline void FieldSet__FocusRouteParentWindowId(ImGuiWindowClass* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FocusRouteParentWindowId = value;
		}
		static inline int FieldGet__ViewportFlagsOverrideSet(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->ViewportFlagsOverrideSet;
		}
		static inline void FieldSet__ViewportFlagsOverrideSet(ImGuiWindowClass* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ViewportFlagsOverrideSet = value;
		}
		static inline int FieldGet__ViewportFlagsOverrideClear(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->ViewportFlagsOverrideClear;
		}
		static inline void FieldSet__ViewportFlagsOverrideClear(ImGuiWindowClass* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ViewportFlagsOverrideClear = value;
		}
		static inline int FieldGet__TabItemFlagsOverrideSet(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TabItemFlagsOverrideSet;
		}
		static inline void FieldSet__TabItemFlagsOverrideSet(ImGuiWindowClass* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabItemFlagsOverrideSet = value;
		}
		static inline int FieldGet__DockNodeFlagsOverrideSet(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->DockNodeFlagsOverrideSet;
		}
		static inline void FieldSet__DockNodeFlagsOverrideSet(ImGuiWindowClass* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DockNodeFlagsOverrideSet = value;
		}
		static inline bool FieldGet__DockingAlwaysTabBar(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->DockingAlwaysTabBar;
		}
		static inline void FieldSet__DockingAlwaysTabBar(ImGuiWindowClass* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DockingAlwaysTabBar = value;
		}
		static inline bool FieldGet__DockingAllowUnclassed(ImGuiWindowClass* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->DockingAllowUnclassed;
		}
		static inline void FieldSet__DockingAllowUnclassed(ImGuiWindowClass* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DockingAllowUnclassed = value;
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_UnsafeCallConstructor_2960189489(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_UnsafeCallDestructor(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiWindowClass_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiWindowClass>();
}


extern "C" VFX_API unsigned int TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ClassId(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::FieldGet__ClassId(self);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ClassId(ImGuiWindowClass* self, unsigned int value)
{
	ImGuiWindowClass_Visitor::FieldSet__ClassId(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ParentViewportId(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::FieldGet__ParentViewportId(self);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ParentViewportId(ImGuiWindowClass* self, unsigned int value)
{
	ImGuiWindowClass_Visitor::FieldSet__ParentViewportId(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiWindowClass_Visitor_FieldGet__FocusRouteParentWindowId(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::FieldGet__FocusRouteParentWindowId(self);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__FocusRouteParentWindowId(ImGuiWindowClass* self, unsigned int value)
{
	ImGuiWindowClass_Visitor::FieldSet__FocusRouteParentWindowId(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ViewportFlagsOverrideSet(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::FieldGet__ViewportFlagsOverrideSet(self);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ViewportFlagsOverrideSet(ImGuiWindowClass* self, int value)
{
	ImGuiWindowClass_Visitor::FieldSet__ViewportFlagsOverrideSet(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiWindowClass_Visitor_FieldGet__ViewportFlagsOverrideClear(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::FieldGet__ViewportFlagsOverrideClear(self);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__ViewportFlagsOverrideClear(ImGuiWindowClass* self, int value)
{
	ImGuiWindowClass_Visitor::FieldSet__ViewportFlagsOverrideClear(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiWindowClass_Visitor_FieldGet__TabItemFlagsOverrideSet(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::FieldGet__TabItemFlagsOverrideSet(self);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__TabItemFlagsOverrideSet(ImGuiWindowClass* self, int value)
{
	ImGuiWindowClass_Visitor::FieldSet__TabItemFlagsOverrideSet(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockNodeFlagsOverrideSet(ImGuiWindowClass* self)
{
	return ImGuiWindowClass_Visitor::FieldGet__DockNodeFlagsOverrideSet(self);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockNodeFlagsOverrideSet(ImGuiWindowClass* self, int value)
{
	ImGuiWindowClass_Visitor::FieldSet__DockNodeFlagsOverrideSet(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockingAlwaysTabBar(ImGuiWindowClass* self)
{
	auto tmp_result = ImGuiWindowClass_Visitor::FieldGet__DockingAlwaysTabBar(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockingAlwaysTabBar(ImGuiWindowClass* self, bool value)
{
	ImGuiWindowClass_Visitor::FieldSet__DockingAlwaysTabBar(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiWindowClass_Visitor_FieldGet__DockingAllowUnclassed(ImGuiWindowClass* self)
{
	auto tmp_result = ImGuiWindowClass_Visitor::FieldGet__DockingAllowUnclassed(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiWindowClass_Visitor_FieldSet__DockingAllowUnclassed(ImGuiWindowClass* self, bool value)
{
	ImGuiWindowClass_Visitor::FieldSet__DockingAllowUnclassed(self, value);
}


#endif//HasModule_ImGui
