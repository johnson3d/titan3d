//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImTextureRef_Visitor
	{
		static void UnsafeCallConstructor(ImTextureRef* self)
		{
			#undef new
			new (self)ImTextureRef();
			#define new VNEW
		}
		static void UnsafeCallConstructor(ImTextureRef* self, unsigned long long tex_id)
		{
			#undef new
			new (self)ImTextureRef(tex_id);
			#define new VNEW
		}
		static void UnsafeCallConstructor(ImTextureRef* self, void* tex_id)
		{
			#undef new
			new (self)ImTextureRef(tex_id);
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImTextureRef* self)
		{
		}
		static inline unsigned long long FieldGet___TexID(ImTextureRef* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned long long>();
			}
			return (unsigned long long)self->_TexID;
		}
		static inline void FieldSet___TexID(ImTextureRef* self, unsigned long long value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->_TexID = value;
		}
		static inline unsigned long long GetTexID(ImTextureRef* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned long long>();
			}
			return (unsigned long long)self->GetTexID();
		}
	};
}


extern "C" VFX_API void TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_2960189489(ImTextureRef* self)
{
	return ImTextureRef_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_3315747347(ImTextureRef* self, unsigned long long tex_id)
{
	return ImTextureRef_Visitor::UnsafeCallConstructor(self, tex_id);
}
extern "C" VFX_API void TitanImGui_ImTextureRef_Visitor_UnsafeCallConstructor_3034592143(ImTextureRef* self, void* tex_id)
{
	return ImTextureRef_Visitor::UnsafeCallConstructor(self, tex_id);
}
extern "C" VFX_API void TitanImGui_ImTextureRef_Visitor_UnsafeCallDestructor(ImTextureRef* self)
{
	return ImTextureRef_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImTextureRef_Visitor_GetTypeRtti()
{
	return GetClassObject<ImTextureRef>();
}


extern "C" VFX_API unsigned long long TitanImGui_ImTextureRef_Visitor_FieldGet___TexID(ImTextureRef* self)
{
	return ImTextureRef_Visitor::FieldGet___TexID(self);
}
extern "C" VFX_API void TitanImGui_ImTextureRef_Visitor_FieldSet___TexID(ImTextureRef* self, unsigned long long value)
{
	ImTextureRef_Visitor::FieldSet___TexID(self, value);
}


extern "C" VFX_API unsigned long long TitanImGui_ImTextureRef_Visitor_GetTexID_2645814042(ImTextureRef* self)
{
	return ImTextureRef_Visitor::GetTexID(self);
}
#endif//HasModule_ImGui
