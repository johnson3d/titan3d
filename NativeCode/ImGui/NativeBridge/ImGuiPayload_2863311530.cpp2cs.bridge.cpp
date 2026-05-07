//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiPayload_Visitor
	{
		static void UnsafeCallConstructor(ImGuiPayload* self)
		{
			#undef new
			new (self)ImGuiPayload();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImGuiPayload* self)
		{
		}
		static inline void* FieldGet__Data(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void*>();
			}
			return (void*)self->Data;
		}
		static inline void FieldSet__Data(ImGuiPayload* self, void* value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Data = value;
		}
		static inline int FieldGet__DataSize(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->DataSize;
		}
		static inline void FieldSet__DataSize(ImGuiPayload* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DataSize = value;
		}
		static inline unsigned int FieldGet__SourceId(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->SourceId;
		}
		static inline void FieldSet__SourceId(ImGuiPayload* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SourceId = value;
		}
		static inline unsigned int FieldGet__SourceParentId(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<unsigned int>();
			}
			return (unsigned int)self->SourceParentId;
		}
		static inline void FieldSet__SourceParentId(ImGuiPayload* self, unsigned int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SourceParentId = value;
		}
		static inline int FieldGet__DataFrameCount(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->DataFrameCount;
		}
		static inline void FieldSet__DataFrameCount(ImGuiPayload* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DataFrameCount = value;
		}
		static inline char* FieldGet__DataType(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<char*>();
			}
			return (char*)self->DataType;
		}
		static inline void FieldSet__DataType(ImGuiPayload* self, char* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 33; i++)
			{
				self->DataType[i] = value[i];
			}
		}
		static inline bool FieldGet__Preview(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->Preview;
		}
		static inline void FieldSet__Preview(ImGuiPayload* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Preview = value;
		}
		static inline bool FieldGet__Delivery(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->Delivery;
		}
		static inline void FieldSet__Delivery(ImGuiPayload* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Delivery = value;
		}
		static inline void Clear(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->Clear();
		}
		static inline bool IsDataType(ImGuiPayload* self, const char* type)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsDataType(type);
		}
		static inline bool IsPreview(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsPreview();
		}
		static inline bool IsDelivery(ImGuiPayload* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->IsDelivery();
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_UnsafeCallConstructor_2960189489(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_UnsafeCallDestructor(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiPayload_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiPayload>();
}


extern "C" VFX_API void* TitanImGui_ImGuiPayload_Visitor_FieldGet__Data(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::FieldGet__Data(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_FieldSet__Data(ImGuiPayload* self, void* value)
{
	ImGuiPayload_Visitor::FieldSet__Data(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiPayload_Visitor_FieldGet__DataSize(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::FieldGet__DataSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_FieldSet__DataSize(ImGuiPayload* self, int value)
{
	ImGuiPayload_Visitor::FieldSet__DataSize(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiPayload_Visitor_FieldGet__SourceId(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::FieldGet__SourceId(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_FieldSet__SourceId(ImGuiPayload* self, unsigned int value)
{
	ImGuiPayload_Visitor::FieldSet__SourceId(self, value);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiPayload_Visitor_FieldGet__SourceParentId(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::FieldGet__SourceParentId(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_FieldSet__SourceParentId(ImGuiPayload* self, unsigned int value)
{
	ImGuiPayload_Visitor::FieldSet__SourceParentId(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiPayload_Visitor_FieldGet__DataFrameCount(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::FieldGet__DataFrameCount(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_FieldSet__DataFrameCount(ImGuiPayload* self, int value)
{
	ImGuiPayload_Visitor::FieldSet__DataFrameCount(self, value);
}
extern "C" VFX_API char* TitanImGui_ImGuiPayload_Visitor_FieldGet__DataType(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::FieldGet__DataType(self);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_FieldSet__DataType(ImGuiPayload* self, char* value)
{
	ImGuiPayload_Visitor::FieldSet__DataType(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiPayload_Visitor_FieldGet__Preview(ImGuiPayload* self)
{
	auto tmp_result = ImGuiPayload_Visitor::FieldGet__Preview(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_FieldSet__Preview(ImGuiPayload* self, bool value)
{
	ImGuiPayload_Visitor::FieldSet__Preview(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiPayload_Visitor_FieldGet__Delivery(ImGuiPayload* self)
{
	auto tmp_result = ImGuiPayload_Visitor::FieldGet__Delivery(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_FieldSet__Delivery(ImGuiPayload* self, bool value)
{
	ImGuiPayload_Visitor::FieldSet__Delivery(self, value);
}


extern "C" VFX_API void TitanImGui_ImGuiPayload_Visitor_Clear_2960189489(ImGuiPayload* self)
{
	return ImGuiPayload_Visitor::Clear(self);
}
extern "C" VFX_API char TitanImGui_ImGuiPayload_Visitor_IsDataType_1209308473(ImGuiPayload* self, const char* type)
{
	auto tmp_result = ImGuiPayload_Visitor::IsDataType(self, type);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiPayload_Visitor_IsPreview_82051314(ImGuiPayload* self)
{
	auto tmp_result = ImGuiPayload_Visitor::IsPreview(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiPayload_Visitor_IsDelivery_82051314(ImGuiPayload* self)
{
	auto tmp_result = ImGuiPayload_Visitor::IsDelivery(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
#endif//HasModule_ImGui
