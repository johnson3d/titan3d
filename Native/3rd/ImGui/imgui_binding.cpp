#include "imgui_binding.h"

NS_BEGIN

StructImpl(ImVec2)
StructImpl(ImVec4)

//StructImpl(items_getter)
//StructImpl(values_getter)
//StructImpl(alloc_func)
//StructImpl(free_func)
//StructImpl(ImGuiSizeCallback)
//StructImpl(ImGuiInputTextCallback)

AuxRttiStruct<void __cdecl(struct ImGuiSizeCallbackData *)> AuxRttiStruct<void __cdecl(struct ImGuiSizeCallbackData *)>::Instance;
AuxRttiStruct<bool __cdecl(void *, int, char const * *)> AuxRttiStruct<bool __cdecl(void *, int, char const * *)>::Instance;
AuxRttiStruct<int __cdecl(struct ImGuiInputTextCallbackData *)> AuxRttiStruct<int __cdecl(struct ImGuiInputTextCallbackData *)>::Instance;
AuxRttiStruct<float __cdecl(void *, int)> AuxRttiStruct<float __cdecl(void *, int)>::Instance;
AuxRttiStruct<void * __cdecl(unsigned __int64, void *)> AuxRttiStruct<void * __cdecl(unsigned __int64, void *)>::Instance;
AuxRttiStruct<void __cdecl(void *, void *)> AuxRttiStruct<void __cdecl(void *, void *)>::Instance;

NS_END
