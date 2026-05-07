//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiStorage_Visitor
	{
		static inline void Clear(ImGuiStorage* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Clear();
		}
		static inline int GetInt(ImGuiStorage* self, unsigned int key,int default_val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->GetInt(key, default_val);
		}
		static inline void SetInt(ImGuiStorage* self, unsigned int key,int val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetInt(key, val);
		}
		static inline bool GetBool(ImGuiStorage* self, unsigned int key,bool default_val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->GetBool(key, default_val);
		}
		static inline void SetBool(ImGuiStorage* self, unsigned int key,bool val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetBool(key, val);
		}
		static inline float GetFloat(ImGuiStorage* self, unsigned int key,float default_val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->GetFloat(key, default_val);
		}
		static inline void SetFloat(ImGuiStorage* self, unsigned int key,float val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetFloat(key, val);
		}
		static inline void* GetVoidPtr(ImGuiStorage* self, unsigned int key)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->GetVoidPtr(key);
		}
		static inline void SetVoidPtr(ImGuiStorage* self, unsigned int key,void* val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetVoidPtr(key, val);
		}
		static inline int* GetIntRef(ImGuiStorage* self, unsigned int key,int default_val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int*>();
			}
			return (int*)self->GetIntRef(key, default_val);
		}
		static inline bool* GetBoolRef(ImGuiStorage* self, unsigned int key,bool default_val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool*>();
			}
			return (bool*)self->GetBoolRef(key, default_val);
		}
		static inline float* GetFloatRef(ImGuiStorage* self, unsigned int key,float default_val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float*>();
			}
			return (float*)self->GetFloatRef(key, default_val);
		}
		static inline void** GetVoidPtrRef(ImGuiStorage* self, unsigned int key,void* default_val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void**>();
			}
			return (void**)self->GetVoidPtrRef(key, default_val);
		}
		static inline void BuildSortByKey(ImGuiStorage* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->BuildSortByKey();
		}
		static inline void SetAllInt(ImGuiStorage* self, int val)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->SetAllInt(val);
		}
	};
}




extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiStorage_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiStorage>();
}




extern "C" VFX_API void TitanImGui_ImGuiStorage_Visitor_Clear_2960189489(ImGuiStorage* self)
{
	return ImGuiStorage_Visitor::Clear(self);
}
extern "C" VFX_API int TitanImGui_ImGuiStorage_Visitor_GetInt_1250669178(ImGuiStorage* self, unsigned int key,int default_val)
{
	return ImGuiStorage_Visitor::GetInt(self, key, default_val);
}
extern "C" VFX_API void TitanImGui_ImGuiStorage_Visitor_SetInt_3129034236(ImGuiStorage* self, unsigned int key,int val)
{
	return ImGuiStorage_Visitor::SetInt(self, key, val);
}
extern "C" VFX_API char TitanImGui_ImGuiStorage_Visitor_GetBool_2657485946(ImGuiStorage* self, unsigned int key,bool default_val)
{
	auto tmp_result = ImGuiStorage_Visitor::GetBool(self, key, default_val);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStorage_Visitor_SetBool_1053065717(ImGuiStorage* self, unsigned int key,bool val)
{
	return ImGuiStorage_Visitor::SetBool(self, key, val);
}
extern "C" VFX_API float TitanImGui_ImGuiStorage_Visitor_GetFloat_3416669274(ImGuiStorage* self, unsigned int key,float default_val)
{
	return ImGuiStorage_Visitor::GetFloat(self, key, default_val);
}
extern "C" VFX_API void TitanImGui_ImGuiStorage_Visitor_SetFloat_3128544667(ImGuiStorage* self, unsigned int key,float val)
{
	return ImGuiStorage_Visitor::SetFloat(self, key, val);
}
extern "C" VFX_API void* TitanImGui_ImGuiStorage_Visitor_GetVoidPtr_2257549434(ImGuiStorage* self, unsigned int key)
{
	return ImGuiStorage_Visitor::GetVoidPtr(self, key);
}
extern "C" VFX_API void TitanImGui_ImGuiStorage_Visitor_SetVoidPtr_3144869721(ImGuiStorage* self, unsigned int key,void* val)
{
	return ImGuiStorage_Visitor::SetVoidPtr(self, key, val);
}
extern "C" VFX_API int* TitanImGui_ImGuiStorage_Visitor_GetIntRef_1484994661(ImGuiStorage* self, unsigned int key,int default_val)
{
	return ImGuiStorage_Visitor::GetIntRef(self, key, default_val);
}
extern "C" VFX_API bool* TitanImGui_ImGuiStorage_Visitor_GetBoolRef_1816378645(ImGuiStorage* self, unsigned int key,bool default_val)
{
	return ImGuiStorage_Visitor::GetBoolRef(self, key, default_val);
}
extern "C" VFX_API float* TitanImGui_ImGuiStorage_Visitor_GetFloatRef_26904213(ImGuiStorage* self, unsigned int key,float default_val)
{
	return ImGuiStorage_Visitor::GetFloatRef(self, key, default_val);
}
extern "C" VFX_API void** TitanImGui_ImGuiStorage_Visitor_GetVoidPtrRef_949922101(ImGuiStorage* self, unsigned int key,void* default_val)
{
	return ImGuiStorage_Visitor::GetVoidPtrRef(self, key, default_val);
}
extern "C" VFX_API void TitanImGui_ImGuiStorage_Visitor_BuildSortByKey_2960189489(ImGuiStorage* self)
{
	return ImGuiStorage_Visitor::BuildSortByKey(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStorage_Visitor_SetAllInt_4038704236(ImGuiStorage* self, int val)
{
	return ImGuiStorage_Visitor::SetAllInt(self, val);
}
#endif//HasModule_ImGui
