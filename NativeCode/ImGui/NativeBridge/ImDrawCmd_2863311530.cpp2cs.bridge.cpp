//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImDrawCmd_Visitor
	{
		static void UnsafeCallConstructor(ImDrawCmd* self)
		{
			#undef new
			new (self)ImDrawCmd();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImDrawCmd* self)
		{
		}
		static inline ImVec4 FieldGet__ClipRect(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec4>();
			}
			return (ImVec4)self->ClipRect;
		}
		static inline void FieldSet__ClipRect(ImDrawCmd* self, ImVec4 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ClipRect = value;
		}
		static inline ImTextureRef FieldGet__TexRef(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImTextureRef>();
			}
			return (ImTextureRef)self->TexRef;
		}
		static inline void FieldSet__TexRef(ImDrawCmd* self, ImTextureRef value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TexRef = value;
		}
		static inline unsigned int FieldGet__VtxOffset(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->VtxOffset;
		}
		static inline void FieldSet__VtxOffset(ImDrawCmd* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->VtxOffset = value;
		}
		static inline unsigned int FieldGet__IdxOffset(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->IdxOffset;
		}
		static inline void FieldSet__IdxOffset(ImDrawCmd* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IdxOffset = value;
		}
		static inline unsigned int FieldGet__ElemCount(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->ElemCount;
		}
		static inline void FieldSet__ElemCount(ImDrawCmd* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ElemCount = value;
		}
		static inline void* FieldGet__UserCallback(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->UserCallback;
		}
		static inline void FieldSet__UserCallback(ImDrawCmd* self, void (*UserCallback)(ImDrawList* arg0,ImDrawCmd* arg1))
		{
			if(self==nullptr)
			{
				return;
			}
			*(void**)&(self->UserCallback) = (void*)UserCallback;
		}
		static inline void* FieldGet__UserCallbackData(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->UserCallbackData;
		}
		static inline void FieldSet__UserCallbackData(ImDrawCmd* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->UserCallbackData = value;
		}
		static inline int FieldGet__UserCallbackDataSize(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->UserCallbackDataSize;
		}
		static inline void FieldSet__UserCallbackDataSize(ImDrawCmd* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->UserCallbackDataSize = value;
		}
		static inline int FieldGet__UserCallbackDataOffset(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->UserCallbackDataOffset;
		}
		static inline void FieldSet__UserCallbackDataOffset(ImDrawCmd* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->UserCallbackDataOffset = value;
		}
		static inline unsigned long long GetTexID(ImDrawCmd* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned long long>();
			}
			return (unsigned long long)self->GetTexID();
		}
	};
}


extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_UnsafeCallConstructor_2960189489(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_UnsafeCallDestructor(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImDrawCmd_Visitor_GetTypeRtti()
{
	return GetClassObject<ImDrawCmd>();
}


extern "C" VFX_API v3dVector4_t TitanImGui_ImDrawCmd_Visitor_FieldGet__ClipRect(ImDrawCmd* self)
{
	auto tmp_result = ImDrawCmd_Visitor::FieldGet__ClipRect(self);
	return EngineNS::VReturnValueMarshal<ImVec4,v3dVector4_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__ClipRect(ImDrawCmd* self, ImVec4 value)
{
	ImDrawCmd_Visitor::FieldSet__ClipRect(self, value);
}
extern "C" VFX_API ImTextureRef_PodType TitanImGui_ImDrawCmd_Visitor_FieldGet__TexRef(ImDrawCmd* self)
{
	auto tmp_result = ImDrawCmd_Visitor::FieldGet__TexRef(self);
	return EngineNS::VReturnValueMarshal<ImTextureRef,ImTextureRef_PodType>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__TexRef(ImDrawCmd* self, ImTextureRef value)
{
	ImDrawCmd_Visitor::FieldSet__TexRef(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImDrawCmd_Visitor_FieldGet__VtxOffset(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::FieldGet__VtxOffset(self);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__VtxOffset(ImDrawCmd* self, unsigned int value)
{
	ImDrawCmd_Visitor::FieldSet__VtxOffset(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImDrawCmd_Visitor_FieldGet__IdxOffset(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::FieldGet__IdxOffset(self);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__IdxOffset(ImDrawCmd* self, unsigned int value)
{
	ImDrawCmd_Visitor::FieldSet__IdxOffset(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImDrawCmd_Visitor_FieldGet__ElemCount(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::FieldGet__ElemCount(self);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__ElemCount(ImDrawCmd* self, unsigned int value)
{
	ImDrawCmd_Visitor::FieldSet__ElemCount(self, value);
}
extern "C" VFX_API void* TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallback(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::FieldGet__UserCallback(self);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallback(ImDrawCmd* self, void (*UserCallback)(ImDrawList* arg0,ImDrawCmd* arg1))
{
	ImDrawCmd_Visitor::FieldSet__UserCallback(self, UserCallback);
}
extern "C" VFX_API void* TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackData(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::FieldGet__UserCallbackData(self);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackData(ImDrawCmd* self, void* value)
{
	ImDrawCmd_Visitor::FieldSet__UserCallbackData(self, value);
}
extern "C" VFX_API int TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackDataSize(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::FieldGet__UserCallbackDataSize(self);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackDataSize(ImDrawCmd* self, int value)
{
	ImDrawCmd_Visitor::FieldSet__UserCallbackDataSize(self, value);
}
extern "C" VFX_API int TitanImGui_ImDrawCmd_Visitor_FieldGet__UserCallbackDataOffset(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::FieldGet__UserCallbackDataOffset(self);
}
extern "C" VFX_API void TitanImGui_ImDrawCmd_Visitor_FieldSet__UserCallbackDataOffset(ImDrawCmd* self, int value)
{
	ImDrawCmd_Visitor::FieldSet__UserCallbackDataOffset(self, value);
}


extern "C" VFX_API unsigned long long TitanImGui_ImDrawCmd_Visitor_GetTexID_2645814042(ImDrawCmd* self)
{
	return ImDrawCmd_Visitor::GetTexID(self);
}
#endif//HasModule_ImGui
