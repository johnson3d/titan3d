//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiStyle_Visitor
	{
		static void UnsafeCallConstructor(ImGuiStyle* self)
		{
			#undef new
			new (self)ImGuiStyle();
			#define new VNEW
		}
		static void UnsafeCallDestructor(ImGuiStyle* self)
		{
		}
		static inline float FieldGet__FontSizeBase(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->FontSizeBase;
		}
		static inline void FieldSet__FontSizeBase(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontSizeBase = value;
		}
		static inline float FieldGet__FontScaleMain(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->FontScaleMain;
		}
		static inline void FieldSet__FontScaleMain(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontScaleMain = value;
		}
		static inline float FieldGet__FontScaleDpi(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->FontScaleDpi;
		}
		static inline void FieldSet__FontScaleDpi(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FontScaleDpi = value;
		}
		static inline float FieldGet__Alpha(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->Alpha;
		}
		static inline void FieldSet__Alpha(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->Alpha = value;
		}
		static inline float FieldGet__DisabledAlpha(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->DisabledAlpha;
		}
		static inline void FieldSet__DisabledAlpha(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DisabledAlpha = value;
		}
		static inline ImVec2 FieldGet__WindowPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->WindowPadding;
		}
		static inline void FieldSet__WindowPadding(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowPadding = value;
		}
		static inline float FieldGet__WindowRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->WindowRounding;
		}
		static inline void FieldSet__WindowRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowRounding = value;
		}
		static inline float FieldGet__WindowBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->WindowBorderSize;
		}
		static inline void FieldSet__WindowBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowBorderSize = value;
		}
		static inline float FieldGet__WindowBorderHoverPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->WindowBorderHoverPadding;
		}
		static inline void FieldSet__WindowBorderHoverPadding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowBorderHoverPadding = value;
		}
		static inline ImVec2 FieldGet__WindowMinSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->WindowMinSize;
		}
		static inline void FieldSet__WindowMinSize(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowMinSize = value;
		}
		static inline ImVec2 FieldGet__WindowTitleAlign(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->WindowTitleAlign;
		}
		static inline void FieldSet__WindowTitleAlign(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowTitleAlign = value;
		}
		static inline ImGuiDir FieldGet__WindowMenuButtonPosition(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImGuiDir>();
			}
			return (ImGuiDir)self->WindowMenuButtonPosition;
		}
		static inline void FieldSet__WindowMenuButtonPosition(ImGuiStyle* self, ImGuiDir value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->WindowMenuButtonPosition = value;
		}
		static inline float FieldGet__ChildRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ChildRounding;
		}
		static inline void FieldSet__ChildRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ChildRounding = value;
		}
		static inline float FieldGet__ChildBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ChildBorderSize;
		}
		static inline void FieldSet__ChildBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ChildBorderSize = value;
		}
		static inline float FieldGet__PopupRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->PopupRounding;
		}
		static inline void FieldSet__PopupRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PopupRounding = value;
		}
		static inline float FieldGet__PopupBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->PopupBorderSize;
		}
		static inline void FieldSet__PopupBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->PopupBorderSize = value;
		}
		static inline ImVec2 FieldGet__FramePadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->FramePadding;
		}
		static inline void FieldSet__FramePadding(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FramePadding = value;
		}
		static inline float FieldGet__FrameRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->FrameRounding;
		}
		static inline void FieldSet__FrameRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FrameRounding = value;
		}
		static inline float FieldGet__FrameBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->FrameBorderSize;
		}
		static inline void FieldSet__FrameBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->FrameBorderSize = value;
		}
		static inline ImVec2 FieldGet__ItemSpacing(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->ItemSpacing;
		}
		static inline void FieldSet__ItemSpacing(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ItemSpacing = value;
		}
		static inline ImVec2 FieldGet__ItemInnerSpacing(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->ItemInnerSpacing;
		}
		static inline void FieldSet__ItemInnerSpacing(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ItemInnerSpacing = value;
		}
		static inline ImVec2 FieldGet__CellPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->CellPadding;
		}
		static inline void FieldSet__CellPadding(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CellPadding = value;
		}
		static inline ImVec2 FieldGet__TouchExtraPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->TouchExtraPadding;
		}
		static inline void FieldSet__TouchExtraPadding(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TouchExtraPadding = value;
		}
		static inline float FieldGet__IndentSpacing(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->IndentSpacing;
		}
		static inline void FieldSet__IndentSpacing(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->IndentSpacing = value;
		}
		static inline float FieldGet__ColumnsMinSpacing(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ColumnsMinSpacing;
		}
		static inline void FieldSet__ColumnsMinSpacing(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ColumnsMinSpacing = value;
		}
		static inline float FieldGet__ScrollbarSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ScrollbarSize;
		}
		static inline void FieldSet__ScrollbarSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ScrollbarSize = value;
		}
		static inline float FieldGet__ScrollbarRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ScrollbarRounding;
		}
		static inline void FieldSet__ScrollbarRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ScrollbarRounding = value;
		}
		static inline float FieldGet__ScrollbarPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ScrollbarPadding;
		}
		static inline void FieldSet__ScrollbarPadding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ScrollbarPadding = value;
		}
		static inline float FieldGet__GrabMinSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->GrabMinSize;
		}
		static inline void FieldSet__GrabMinSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->GrabMinSize = value;
		}
		static inline float FieldGet__GrabRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->GrabRounding;
		}
		static inline void FieldSet__GrabRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->GrabRounding = value;
		}
		static inline float FieldGet__LogSliderDeadzone(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->LogSliderDeadzone;
		}
		static inline void FieldSet__LogSliderDeadzone(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->LogSliderDeadzone = value;
		}
		static inline float FieldGet__ImageRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ImageRounding;
		}
		static inline void FieldSet__ImageRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ImageRounding = value;
		}
		static inline float FieldGet__ImageBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ImageBorderSize;
		}
		static inline void FieldSet__ImageBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ImageBorderSize = value;
		}
		static inline float FieldGet__TabRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TabRounding;
		}
		static inline void FieldSet__TabRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabRounding = value;
		}
		static inline float FieldGet__TabBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TabBorderSize;
		}
		static inline void FieldSet__TabBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabBorderSize = value;
		}
		static inline float FieldGet__TabMinWidthBase(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TabMinWidthBase;
		}
		static inline void FieldSet__TabMinWidthBase(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabMinWidthBase = value;
		}
		static inline float FieldGet__TabMinWidthShrink(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TabMinWidthShrink;
		}
		static inline void FieldSet__TabMinWidthShrink(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabMinWidthShrink = value;
		}
		static inline float FieldGet__TabCloseButtonMinWidthSelected(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TabCloseButtonMinWidthSelected;
		}
		static inline void FieldSet__TabCloseButtonMinWidthSelected(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabCloseButtonMinWidthSelected = value;
		}
		static inline float FieldGet__TabCloseButtonMinWidthUnselected(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TabCloseButtonMinWidthUnselected;
		}
		static inline void FieldSet__TabCloseButtonMinWidthUnselected(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabCloseButtonMinWidthUnselected = value;
		}
		static inline float FieldGet__TabBarBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TabBarBorderSize;
		}
		static inline void FieldSet__TabBarBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabBarBorderSize = value;
		}
		static inline float FieldGet__TabBarOverlineSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TabBarOverlineSize;
		}
		static inline void FieldSet__TabBarOverlineSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TabBarOverlineSize = value;
		}
		static inline float FieldGet__TableAngledHeadersAngle(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TableAngledHeadersAngle;
		}
		static inline void FieldSet__TableAngledHeadersAngle(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TableAngledHeadersAngle = value;
		}
		static inline ImVec2 FieldGet__TableAngledHeadersTextAlign(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->TableAngledHeadersTextAlign;
		}
		static inline void FieldSet__TableAngledHeadersTextAlign(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TableAngledHeadersTextAlign = value;
		}
		static inline int FieldGet__TreeLinesFlags(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->TreeLinesFlags;
		}
		static inline void FieldSet__TreeLinesFlags(ImGuiStyle* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TreeLinesFlags = value;
		}
		static inline float FieldGet__TreeLinesSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TreeLinesSize;
		}
		static inline void FieldSet__TreeLinesSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TreeLinesSize = value;
		}
		static inline float FieldGet__TreeLinesRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->TreeLinesRounding;
		}
		static inline void FieldSet__TreeLinesRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->TreeLinesRounding = value;
		}
		static inline float FieldGet__DragDropTargetRounding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->DragDropTargetRounding;
		}
		static inline void FieldSet__DragDropTargetRounding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DragDropTargetRounding = value;
		}
		static inline float FieldGet__DragDropTargetBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->DragDropTargetBorderSize;
		}
		static inline void FieldSet__DragDropTargetBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DragDropTargetBorderSize = value;
		}
		static inline float FieldGet__DragDropTargetPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->DragDropTargetPadding;
		}
		static inline void FieldSet__DragDropTargetPadding(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DragDropTargetPadding = value;
		}
		static inline float FieldGet__ColorMarkerSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->ColorMarkerSize;
		}
		static inline void FieldSet__ColorMarkerSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ColorMarkerSize = value;
		}
		static inline ImGuiDir FieldGet__ColorButtonPosition(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImGuiDir>();
			}
			return (ImGuiDir)self->ColorButtonPosition;
		}
		static inline void FieldSet__ColorButtonPosition(ImGuiStyle* self, ImGuiDir value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ColorButtonPosition = value;
		}
		static inline ImVec2 FieldGet__ButtonTextAlign(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->ButtonTextAlign;
		}
		static inline void FieldSet__ButtonTextAlign(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->ButtonTextAlign = value;
		}
		static inline ImVec2 FieldGet__SelectableTextAlign(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->SelectableTextAlign;
		}
		static inline void FieldSet__SelectableTextAlign(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SelectableTextAlign = value;
		}
		static inline float FieldGet__SeparatorSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->SeparatorSize;
		}
		static inline void FieldSet__SeparatorSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SeparatorSize = value;
		}
		static inline float FieldGet__SeparatorTextBorderSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->SeparatorTextBorderSize;
		}
		static inline void FieldSet__SeparatorTextBorderSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SeparatorTextBorderSize = value;
		}
		static inline ImVec2 FieldGet__SeparatorTextAlign(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->SeparatorTextAlign;
		}
		static inline void FieldSet__SeparatorTextAlign(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SeparatorTextAlign = value;
		}
		static inline ImVec2 FieldGet__SeparatorTextPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->SeparatorTextPadding;
		}
		static inline void FieldSet__SeparatorTextPadding(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->SeparatorTextPadding = value;
		}
		static inline ImVec2 FieldGet__DisplayWindowPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->DisplayWindowPadding;
		}
		static inline void FieldSet__DisplayWindowPadding(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DisplayWindowPadding = value;
		}
		static inline ImVec2 FieldGet__DisplaySafeAreaPadding(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec2>();
			}
			return (ImVec2)self->DisplaySafeAreaPadding;
		}
		static inline void FieldSet__DisplaySafeAreaPadding(ImGuiStyle* self, ImVec2 value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DisplaySafeAreaPadding = value;
		}
		static inline bool FieldGet__DockingNodeHasCloseButton(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->DockingNodeHasCloseButton;
		}
		static inline void FieldSet__DockingNodeHasCloseButton(ImGuiStyle* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DockingNodeHasCloseButton = value;
		}
		static inline float FieldGet__DockingSeparatorSize(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->DockingSeparatorSize;
		}
		static inline void FieldSet__DockingSeparatorSize(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->DockingSeparatorSize = value;
		}
		static inline float FieldGet__MouseCursorScale(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->MouseCursorScale;
		}
		static inline void FieldSet__MouseCursorScale(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->MouseCursorScale = value;
		}
		static inline bool FieldGet__AntiAliasedLines(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->AntiAliasedLines;
		}
		static inline void FieldSet__AntiAliasedLines(ImGuiStyle* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->AntiAliasedLines = value;
		}
		static inline bool FieldGet__AntiAliasedLinesUseTex(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->AntiAliasedLinesUseTex;
		}
		static inline void FieldSet__AntiAliasedLinesUseTex(ImGuiStyle* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->AntiAliasedLinesUseTex = value;
		}
		static inline bool FieldGet__AntiAliasedFill(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<bool>();
			}
			return (bool)self->AntiAliasedFill;
		}
		static inline void FieldSet__AntiAliasedFill(ImGuiStyle* self, bool value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->AntiAliasedFill = value;
		}
		static inline float FieldGet__CurveTessellationTol(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->CurveTessellationTol;
		}
		static inline void FieldSet__CurveTessellationTol(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CurveTessellationTol = value;
		}
		static inline float FieldGet__CircleTessellationMaxError(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->CircleTessellationMaxError;
		}
		static inline void FieldSet__CircleTessellationMaxError(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->CircleTessellationMaxError = value;
		}
		static inline ImVec4* FieldGet__Colors(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<ImVec4*>();
			}
			return (ImVec4*)self->Colors;
		}
		static inline void FieldSet__Colors(ImGuiStyle* self, ImVec4* value)
		{
			if(self==nullptr)
			{
				return;
			}
			for (int i = 0; i < 62; i++)
			{
				self->Colors[i] = value[i];
			}
		}
		static inline float FieldGet__HoverStationaryDelay(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->HoverStationaryDelay;
		}
		static inline void FieldSet__HoverStationaryDelay(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->HoverStationaryDelay = value;
		}
		static inline float FieldGet__HoverDelayShort(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->HoverDelayShort;
		}
		static inline void FieldSet__HoverDelayShort(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->HoverDelayShort = value;
		}
		static inline float FieldGet__HoverDelayNormal(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->HoverDelayNormal;
		}
		static inline void FieldSet__HoverDelayNormal(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->HoverDelayNormal = value;
		}
		static inline int FieldGet__HoverFlagsForTooltipMouse(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->HoverFlagsForTooltipMouse;
		}
		static inline void FieldSet__HoverFlagsForTooltipMouse(ImGuiStyle* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->HoverFlagsForTooltipMouse = value;
		}
		static inline int FieldGet__HoverFlagsForTooltipNav(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<int>();
			}
			return (int)self->HoverFlagsForTooltipNav;
		}
		static inline void FieldSet__HoverFlagsForTooltipNav(ImGuiStyle* self, int value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->HoverFlagsForTooltipNav = value;
		}
		static inline float FieldGet___MainScale(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->_MainScale;
		}
		static inline void FieldSet___MainScale(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->_MainScale = value;
		}
		static inline float FieldGet___NextFrameFontSizeBase(ImGuiStyle* self)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<float>();
			}
			return (float)self->_NextFrameFontSizeBase;
		}
		static inline void FieldSet___NextFrameFontSizeBase(ImGuiStyle* self, float value)
		{
			if(self==nullptr)
			{
				return;
			}
			self->_NextFrameFontSizeBase = value;
		}
		static inline void ScaleAllSizes(ImGuiStyle* self, float scale_factor)
		{
			if(self==nullptr)
			{
				return EngineNS::VGetTypeDefault<void>();
			}
			return (void)self->ScaleAllSizes(scale_factor);
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_UnsafeCallConstructor_2960189489(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::UnsafeCallConstructor(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_UnsafeCallDestructor(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiStyle_Visitor_GetTypeRtti()
{
	return GetClassObject<ImGuiStyle>();
}


extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__FontSizeBase(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__FontSizeBase(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__FontSizeBase(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__FontSizeBase(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__FontScaleMain(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__FontScaleMain(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__FontScaleMain(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__FontScaleMain(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__FontScaleDpi(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__FontScaleDpi(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__FontScaleDpi(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__FontScaleDpi(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__Alpha(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__Alpha(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__Alpha(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__Alpha(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__DisabledAlpha(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__DisabledAlpha(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__DisabledAlpha(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__DisabledAlpha(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowPadding(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__WindowPadding(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowPadding(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__WindowPadding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__WindowRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__WindowRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__WindowBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__WindowBorderSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowBorderHoverPadding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__WindowBorderHoverPadding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowBorderHoverPadding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__WindowBorderHoverPadding(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowMinSize(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__WindowMinSize(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowMinSize(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__WindowMinSize(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowTitleAlign(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__WindowTitleAlign(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowTitleAlign(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__WindowTitleAlign(self, value);
}
extern "C" VFX_API ImGuiDir TitanImGui_ImGuiStyle_Visitor_FieldGet__WindowMenuButtonPosition(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__WindowMenuButtonPosition(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__WindowMenuButtonPosition(ImGuiStyle* self, ImGuiDir value)
{
	ImGuiStyle_Visitor::FieldSet__WindowMenuButtonPosition(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ChildRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ChildRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ChildRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ChildRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ChildBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ChildBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ChildBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ChildBorderSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__PopupRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__PopupRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__PopupRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__PopupRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__PopupBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__PopupBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__PopupBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__PopupBorderSize(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__FramePadding(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__FramePadding(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__FramePadding(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__FramePadding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__FrameRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__FrameRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__FrameRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__FrameRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__FrameBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__FrameBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__FrameBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__FrameBorderSize(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__ItemSpacing(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__ItemSpacing(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ItemSpacing(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__ItemSpacing(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__ItemInnerSpacing(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__ItemInnerSpacing(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ItemInnerSpacing(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__ItemInnerSpacing(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__CellPadding(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__CellPadding(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__CellPadding(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__CellPadding(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__TouchExtraPadding(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__TouchExtraPadding(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TouchExtraPadding(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__TouchExtraPadding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__IndentSpacing(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__IndentSpacing(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__IndentSpacing(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__IndentSpacing(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ColumnsMinSpacing(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ColumnsMinSpacing(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ColumnsMinSpacing(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ColumnsMinSpacing(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ScrollbarSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ScrollbarSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ScrollbarRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ScrollbarRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ScrollbarPadding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ScrollbarPadding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ScrollbarPadding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ScrollbarPadding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__GrabMinSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__GrabMinSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__GrabMinSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__GrabMinSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__GrabRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__GrabRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__GrabRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__GrabRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__LogSliderDeadzone(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__LogSliderDeadzone(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__LogSliderDeadzone(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__LogSliderDeadzone(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ImageRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ImageRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ImageRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ImageRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ImageBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ImageBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ImageBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ImageBorderSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TabRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TabRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TabBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TabBorderSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabMinWidthBase(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TabMinWidthBase(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabMinWidthBase(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TabMinWidthBase(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabMinWidthShrink(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TabMinWidthShrink(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabMinWidthShrink(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TabMinWidthShrink(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabCloseButtonMinWidthSelected(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TabCloseButtonMinWidthSelected(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabCloseButtonMinWidthSelected(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TabCloseButtonMinWidthSelected(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabCloseButtonMinWidthUnselected(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TabCloseButtonMinWidthUnselected(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabCloseButtonMinWidthUnselected(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TabCloseButtonMinWidthUnselected(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBarBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TabBarBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBarBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TabBarBorderSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TabBarOverlineSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TabBarOverlineSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TabBarOverlineSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TabBarOverlineSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TableAngledHeadersAngle(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TableAngledHeadersAngle(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TableAngledHeadersAngle(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TableAngledHeadersAngle(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__TableAngledHeadersTextAlign(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__TableAngledHeadersTextAlign(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TableAngledHeadersTextAlign(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__TableAngledHeadersTextAlign(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesFlags(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TreeLinesFlags(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesFlags(ImGuiStyle* self, int value)
{
	ImGuiStyle_Visitor::FieldSet__TreeLinesFlags(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TreeLinesSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TreeLinesSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__TreeLinesRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__TreeLinesRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__TreeLinesRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__TreeLinesRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetRounding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__DragDropTargetRounding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetRounding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__DragDropTargetRounding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__DragDropTargetBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__DragDropTargetBorderSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__DragDropTargetPadding(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__DragDropTargetPadding(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__DragDropTargetPadding(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__DragDropTargetPadding(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__ColorMarkerSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ColorMarkerSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ColorMarkerSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__ColorMarkerSize(self, value);
}
extern "C" VFX_API ImGuiDir TitanImGui_ImGuiStyle_Visitor_FieldGet__ColorButtonPosition(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__ColorButtonPosition(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ColorButtonPosition(ImGuiStyle* self, ImGuiDir value)
{
	ImGuiStyle_Visitor::FieldSet__ColorButtonPosition(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__ButtonTextAlign(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__ButtonTextAlign(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__ButtonTextAlign(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__ButtonTextAlign(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__SelectableTextAlign(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__SelectableTextAlign(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__SelectableTextAlign(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__SelectableTextAlign(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__SeparatorSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__SeparatorSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextBorderSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__SeparatorTextBorderSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextBorderSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__SeparatorTextBorderSize(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextAlign(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__SeparatorTextAlign(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextAlign(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__SeparatorTextAlign(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__SeparatorTextPadding(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__SeparatorTextPadding(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__SeparatorTextPadding(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__SeparatorTextPadding(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__DisplayWindowPadding(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__DisplayWindowPadding(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__DisplayWindowPadding(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__DisplayWindowPadding(self, value);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiStyle_Visitor_FieldGet__DisplaySafeAreaPadding(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__DisplaySafeAreaPadding(self);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__DisplaySafeAreaPadding(ImGuiStyle* self, ImVec2 value)
{
	ImGuiStyle_Visitor::FieldSet__DisplaySafeAreaPadding(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiStyle_Visitor_FieldGet__DockingNodeHasCloseButton(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__DockingNodeHasCloseButton(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__DockingNodeHasCloseButton(ImGuiStyle* self, bool value)
{
	ImGuiStyle_Visitor::FieldSet__DockingNodeHasCloseButton(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__DockingSeparatorSize(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__DockingSeparatorSize(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__DockingSeparatorSize(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__DockingSeparatorSize(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__MouseCursorScale(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__MouseCursorScale(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__MouseCursorScale(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__MouseCursorScale(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedLines(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__AntiAliasedLines(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedLines(ImGuiStyle* self, bool value)
{
	ImGuiStyle_Visitor::FieldSet__AntiAliasedLines(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedLinesUseTex(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__AntiAliasedLinesUseTex(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedLinesUseTex(ImGuiStyle* self, bool value)
{
	ImGuiStyle_Visitor::FieldSet__AntiAliasedLinesUseTex(self, value);
}
extern "C" VFX_API char TitanImGui_ImGuiStyle_Visitor_FieldGet__AntiAliasedFill(ImGuiStyle* self)
{
	auto tmp_result = ImGuiStyle_Visitor::FieldGet__AntiAliasedFill(self);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__AntiAliasedFill(ImGuiStyle* self, bool value)
{
	ImGuiStyle_Visitor::FieldSet__AntiAliasedFill(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__CurveTessellationTol(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__CurveTessellationTol(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__CurveTessellationTol(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__CurveTessellationTol(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__CircleTessellationMaxError(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__CircleTessellationMaxError(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__CircleTessellationMaxError(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__CircleTessellationMaxError(self, value);
}
extern "C" VFX_API ImVec4* TitanImGui_ImGuiStyle_Visitor_FieldGet__Colors(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__Colors(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__Colors(ImGuiStyle* self, ImVec4* value)
{
	ImGuiStyle_Visitor::FieldSet__Colors(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverStationaryDelay(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__HoverStationaryDelay(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverStationaryDelay(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__HoverStationaryDelay(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverDelayShort(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__HoverDelayShort(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverDelayShort(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__HoverDelayShort(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverDelayNormal(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__HoverDelayNormal(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverDelayNormal(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet__HoverDelayNormal(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverFlagsForTooltipMouse(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__HoverFlagsForTooltipMouse(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverFlagsForTooltipMouse(ImGuiStyle* self, int value)
{
	ImGuiStyle_Visitor::FieldSet__HoverFlagsForTooltipMouse(self, value);
}
extern "C" VFX_API int TitanImGui_ImGuiStyle_Visitor_FieldGet__HoverFlagsForTooltipNav(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet__HoverFlagsForTooltipNav(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet__HoverFlagsForTooltipNav(ImGuiStyle* self, int value)
{
	ImGuiStyle_Visitor::FieldSet__HoverFlagsForTooltipNav(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet___MainScale(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet___MainScale(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet___MainScale(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet___MainScale(self, value);
}
extern "C" VFX_API float TitanImGui_ImGuiStyle_Visitor_FieldGet___NextFrameFontSizeBase(ImGuiStyle* self)
{
	return ImGuiStyle_Visitor::FieldGet___NextFrameFontSizeBase(self);
}
extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_FieldSet___NextFrameFontSizeBase(ImGuiStyle* self, float value)
{
	ImGuiStyle_Visitor::FieldSet___NextFrameFontSizeBase(self, value);
}


extern "C" VFX_API void TitanImGui_ImGuiStyle_Visitor_ScaleAllSizes_1759962673(ImGuiStyle* self, float scale_factor)
{
	return ImGuiStyle_Visitor::ScaleAllSizes(self, scale_factor);
}
#endif//HasModule_ImGui
