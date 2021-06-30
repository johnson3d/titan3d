#pragma once
#include "imgui.h"
#ifndef IMGUI_DEFINE_MATH_OPERATORS
#define IMGUI_DEFINE_MATH_OPERATORS
#endif
#include "imgui_internal.h"

NS_BEGIN

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef bool(*items_getter)(void* data, int idx, const char** out_text);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef float(*values_getter)(void* data, int idx);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void* (*alloc_func)(size_t sz, void* user_data);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*free_func)(void* ptr, void* user_data);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*Renderer_CreateWindow)(ImGuiViewport* vp);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*Renderer_DestroyWindow)(ImGuiViewport* vp);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*Renderer_SetWindowSize)(ImGuiViewport* vp, ImVec2 size);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*Renderer_RenderWindow)(ImGuiViewport* vp, void* render_arg);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*Renderer_SwapBuffers)(ImGuiViewport* vp, void* render_arg);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef const char* (*FGetClipboardTextFn)(void* user_data);
TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void(*FSetClipboardTextFn)(void* user_data, const char* text);

struct TR_CLASS(SV_LayoutStruct = 8)
ImGuiViewportDataSDL2
{
	ImGuiViewportDataSDL2()
	{
		Window = nullptr;
		WindowID = 0;
		WindowOwned = false;
	}
	void* Window;
	UInt32 WindowID;
	bool WindowOwned;
};

struct TR_CLASS(SV_LayoutStruct = 8)
	ImGuiTableRowData
{
	ImGuiTableRowData()
	{
		IndentTextureId = nullptr;
		MinHeight = 0;
		CellPaddingYEnd = 0;
		CellPaddingYBegin = 0;
		IndentImageWidth = 0;
		IndentTextureUVMin = ImVec2(0, 0);
		IndentTextureUVMax = ImVec2(0, 0);
		IndentColor = 0xFFFFFFFF;
		HoverColor = 0xFFFFFFFF;
		Flags = 0;
	}
	ImTextureID IndentTextureId;
	float MinHeight;
	float CellPaddingYEnd;
	float CellPaddingYBegin;
	float IndentImageWidth;
	ImVec2 IndentTextureUVMin;
	ImVec2 IndentTextureUVMax;
	ImU32 IndentColor;
	ImU32 HoverColor;
	ImGuiTableRowFlags Flags;
};

class TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = ImGui, SV_LayoutStruct=8)
ImGuiAPI
{
public:
	static void* CreateContext(ImFontAtlas* shared_font_atlas = NULL)
	{
		return ImGui::CreateContext(shared_font_atlas);
	}
	static void DestroyContext(void* ctx = NULL)
	{
		return ImGui::DestroyContext((ImGuiContext*)ctx);
	}
	static void* GetCurrentContext()
	{
		return ImGui::GetCurrentContext();
	}
	static void SetCurrentContext(void* ctx)
	{
		return ImGui::SetCurrentContext((ImGuiContext*)ctx);
	}
	static ImGuiIO* GetIO()
	{
		return &ImGui::GetIO();
	}
	static ImGuiStyle* GetStyle()
	{
		return &ImGui::GetStyle();
	}
	static void NewFrame()
	{
		return ImGui::NewFrame();
	}
	static void EndFrame()
	{
		return ImGui::EndFrame();
	}
	static void Render()
	{
		return ImGui::Render();
	}
	static ImDrawData* GetDrawData()
	{
		return ImGui::GetDrawData();
	}
	// Demo, Debug, Information
	static void          ShowDemoWindow(bool* p_open = NULL)
	{
		return ImGui::ShowDemoWindow(p_open);
	}
	static void          ShowAboutWindow(bool* p_open = NULL)
	{
		return ImGui::ShowAboutWindow(p_open);
	}
	static void          ShowMetricsWindow(bool* p_open = NULL)
	{
		return ImGui::ShowMetricsWindow(p_open);
	}
	static void          ShowStyleEditor(ImGuiStyle* refValue = NULL)
	{
		return ImGui::ShowStyleEditor(refValue);
	}
	static bool          ShowStyleSelector(const char* label)
	{
		return ImGui::ShowStyleSelector(label);
	}
	static void          ShowFontSelector(const char* label)
	{
		return ImGui::ShowFontSelector(label);
	}
	static void          ShowUserGuide()
	{
		return ImGui::ShowUserGuide();
	}
	static const char*   GetVersion()
	{
		return ImGui::GetVersion();
	}
	// Styles
	static void          StyleColorsDark(ImGuiStyle* dst = NULL)
	{
		return ImGui::StyleColorsDark(dst);
	}
	static void          StyleColorsClassic(ImGuiStyle* dst = NULL)
	{
		return ImGui::StyleColorsClassic(dst);
	}
	static void          StyleColorsLight(ImGuiStyle* dst = NULL)
	{
		return ImGui::StyleColorsLight(dst);
	}
	// Windows
	static bool          Begin(const char* name, bool* p_open, ImGuiWindowFlags_ flags)
	{
		return ImGui::Begin(name, p_open, flags);
	}
	static void          End()
	{
		return ImGui::End();
	}
	// Child Windows
	static bool          BeginChild(const char* str_id, const ImVec2* size/* = &ImVec2(0, 0)*/, bool border = false, ImGuiWindowFlags_ flags = (ImGuiWindowFlags_)0)
	{
		return ImGui::BeginChild(str_id, *size, border, flags);
	}
	static bool          BeginChild(unsigned int id, const ImVec2* size, bool border, ImGuiWindowFlags_ flags)
	{
		return ImGui::BeginChild(id, *size, border, flags);
	}
	static void          EndChild()
	{
		return ImGui::EndChild();
	}
	// Windows Utilities
	static bool          IsWindowAppearing()
	{
		return ImGui::IsWindowAppearing();
	}
	static bool          IsWindowCollapsed()
	{
		return ImGui::IsWindowCollapsed();
	}
	static bool          IsWindowFocused(ImGuiFocusedFlags_ flags)
	{
		return ImGui::IsWindowFocused(flags);
	}
	static bool          IsWindowHovered(ImGuiHoveredFlags_ flags)
	{
		return ImGui::IsWindowHovered(flags);
	}
	static ImDrawList*   GetWindowDrawList()
	{
		return ImGui::GetWindowDrawList();
	}
	static float         GetWindowDpiScale()
	{
		return ImGui::GetWindowDpiScale();
	}
	static ImGuiViewport* GetWindowViewport()
	{
		return ImGui::GetWindowViewport();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetWindowPos()
	{
		return ImGui::GetWindowPos();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetWindowSize()
	{
		return ImGui::GetWindowSize();
	}
	static float         GetWindowWidth()
	{
		return ImGui::GetWindowWidth();
	}
	static float         GetWindowHeight()
	{
		return ImGui::GetWindowHeight();
	}
	// Prefer using SetNextXXX functions (before Begin) rather that SetXXX functions (after Begin).
	static void          SetNextWindowPos(const ImVec2* pos, ImGuiCond_ cond, const ImVec2* pivot/* = &ImVec2(0, 0)*/)
	{
		return ImGui::SetNextWindowPos(*pos, cond, *pivot);
	}
	static void          SetNextWindowSize(const ImVec2* size, ImGuiCond_ cond)
	{
		return ImGui::SetNextWindowSize(*size, cond);
	}
	static void          SetNextWindowSizeConstraints(const ImVec2* size_min, const ImVec2* size_max, ImGuiSizeCallback custom_callback, void* custom_callback_data)
	{
		return ImGui::SetNextWindowSizeConstraints(*size_min, *size_max, custom_callback, custom_callback_data);
	}
	static void          SetNextWindowContentSize(const ImVec2* size)
	{
		return ImGui::SetNextWindowContentSize(*size);
	}
	static void          SetNextWindowCollapsed(bool collapsed, ImGuiCond_ cond)
	{
		return ImGui::SetNextWindowCollapsed(collapsed, cond);
	}
	static void          SetNextWindowFocus()
	{
		return ImGui::SetNextWindowFocus();
	}
	static void          SetNextWindowBgAlpha(float alpha)
	{
		return ImGui::SetNextWindowBgAlpha(alpha);
	}
	static void          SetNextWindowViewport(ImGuiID viewport_id)
	{
		return ImGui::SetNextWindowViewport(viewport_id);
	}
	static void          SetWindowPos(const ImVec2* pos, ImGuiCond_ cond)
	{
		return ImGui::SetWindowPos(*pos, cond);
	}
	static void          SetWindowSize(const ImVec2* size, ImGuiCond_ cond)
	{
		return ImGui::SetWindowSize(*size, cond);
	}
	static void          SetWindowCollapsed(bool collapsed, ImGuiCond_ cond)
	{
		return ImGui::SetWindowCollapsed(collapsed, cond);
	}
	static void          SetWindowFocus()
	{
		return ImGui::SetWindowFocus();
	}
	static void          SetWindowFontScale(float scale)
	{
		return ImGui::SetWindowFontScale(scale);
	}
	static void          SetWindowPos(const char* name, const ImVec2* pos, ImGuiCond_ cond)
	{
		return ImGui::SetWindowPos(name, *pos, cond);
	}
	static void          SetWindowSize(const char* name, const ImVec2* size, ImGuiCond_ cond)
	{
		return ImGui::SetWindowSize(name, *size, cond);
	}
	static void          SetWindowCollapsed(const char* name, bool collapsed, ImGuiCond_ cond)
	{
		return ImGui::SetWindowCollapsed(name, collapsed, cond);
	}
	static void          SetWindowFocus(const char* name)
	{
		return ImGui::SetWindowFocus(name);
	}
	// Content region
	TR_FUNCTION(SV_ReturnConverter=v3dVector2_t)
	static ImVec2        GetContentRegionMax()
	{
		return ImGui::GetContentRegionMax();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetContentRegionAvail()
	{
		return ImGui::GetContentRegionAvail();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetWindowContentRegionMin()
	{
		return ImGui::GetWindowContentRegionMin();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetWindowContentRegionMax()
	{
		return ImGui::GetWindowContentRegionMax();
	}
	static float         GetWindowContentRegionWidth()
	{
		return ImGui::GetWindowContentRegionWidth();
	}
	// Windows Scrolling
	static float         GetScrollX()
	{
		return ImGui::GetWindowContentRegionWidth();
	}
	static float         GetScrollY()
	{
		return ImGui::GetWindowContentRegionWidth();
	}
	static float         GetScrollMaxX()
	{
		return ImGui::GetWindowContentRegionWidth();
	}
	static float         GetScrollMaxY()
	{
		return ImGui::GetWindowContentRegionWidth();
	}
	static void          SetScrollX(float scroll_x)
	{
		return ImGui::SetScrollX(scroll_x);
	}
	static void          SetScrollY(float scroll_y)
	{
		return ImGui::SetScrollY(scroll_y);
	}
	static void          SetScrollHereX(float center_x_ratio = 0.5f)
	{
		return ImGui::SetScrollHereX(center_x_ratio);
	}
	static void          SetScrollHereY(float center_y_ratio = 0.5f)
	{
		return ImGui::SetScrollHereY(center_y_ratio);
	}
	static void          SetScrollFromPosX(float local_x, float center_x_ratio = 0.5f)
	{
		return ImGui::SetScrollFromPosX(local_x, center_x_ratio);
	}
	static void          SetScrollFromPosY(float local_y, float center_y_ratio = 0.5f)
	{
		return ImGui::SetScrollFromPosY(local_y, center_y_ratio);
	}
	// Parameters stacks (shared)
	static void          PushFont(ImFont* font)
	{
		return ImGui::PushFont(font);
	}
	static void          PopFont()
	{
		return ImGui::PopFont();
	}
	static void          PushStyleColor(ImGuiCol_ idx, ImU32 col)
	{
		return ImGui::PushStyleColor(idx, col);
	}
	static void          PushStyleColor(ImGuiCol_ idx, const ImVec4* col)
	{
		return ImGui::PushStyleColor(idx, *col);
	}
	static void          PopStyleColor(int count = 1)
	{
		return ImGui::PopStyleColor(count);
	}
	static void          PushStyleVar(ImGuiStyleVar_ idx, float val)
	{
		return ImGui::PushStyleVar(idx, val);
	}
	static void          PushStyleVar(ImGuiStyleVar_ idx, const ImVec2* val)
	{
		return ImGui::PushStyleVar(idx, *val);
	}
	static void          PopStyleVar(int count = 1)
	{
		return ImGui::PopStyleVar(count);
	}
	static const ImVec4* GetStyleColorVec4(ImGuiCol_ idx)
	{
		return &ImGui::GetStyleColorVec4(idx);
	}
	static ImFont*       GetFont()
	{
		return ImGui::GetFont();
	}
	static float         GetFontSize()
	{
		return ImGui::GetFontSize();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetFontTexUvWhitePixel()
	{
		return ImGui::GetFontTexUvWhitePixel();
	}
	static ImU32         GetColorU32(ImGuiCol_ idx, float alpha_mul = 1.0f)
	{
		return ImGui::GetColorU32(idx, alpha_mul);
	}
	static ImU32         GetColorU32(const ImVec4* col)
	{
		return ImGui::GetColorU32(*col);
	}
	static ImU32         GetColorU32(ImU32 col)
	{
		return ImGui::GetColorU32(col);
	}
	// Parameters stacks (current window)
	static void          PushItemWidth(float item_width)
	{
		return ImGui::PushItemWidth(item_width);
	}
	static void          PopItemWidth()
	{
		return ImGui::PopItemWidth();
	}
	static void          SetNextItemWidth(float item_width)
	{
		return ImGui::SetNextItemWidth(item_width);
	}
	static float         CalcItemWidth()
	{
		return ImGui::CalcItemWidth();
	}
	static void          PushTextWrapPos(float wrap_local_pos_x = 0.0f)
	{
		return ImGui::PushTextWrapPos(wrap_local_pos_x);
	}
	static void          PopTextWrapPos()
	{
		return ImGui::PopTextWrapPos();
	}
	static void          PushAllowKeyboardFocus(bool allow_keyboard_focus)
	{
		return ImGui::PushAllowKeyboardFocus(allow_keyboard_focus);
	}
	static void          PopAllowKeyboardFocus()
	{
		return ImGui::PopAllowKeyboardFocus();
	}
	static void          PushButtonRepeat(bool repeat)
	{
		return ImGui::PushButtonRepeat(repeat);
	}
	static void          PopButtonRepeat()
	{
		return ImGui::PopButtonRepeat();
	}
	// Cursor / Layout
	static void          Separator()
	{
		return ImGui::Separator();
	}
	static void          SameLine(float offset_from_start_x = 0.0f, float spacing = -1.0f)
	{
		return ImGui::SameLine(offset_from_start_x, spacing);
	}
	static void          NewLine()
	{
		return ImGui::NewLine();
	}
	static void          Spacing()
	{
		return ImGui::Spacing();
	}
	static void          Dummy(const ImVec2* size)
	{
		return ImGui::Dummy(*size);
	}
	static void          Indent(float indent_w = 0.0f)
	{
		return ImGui::Indent(indent_w);
	}
	static void          Unindent(float indent_w = 0.0f)
	{
		return ImGui::Unindent(indent_w);
	}
	static void          BeginGroup()
	{
		return ImGui::BeginGroup();
	}
	static void          EndGroup()
	{
		return ImGui::EndGroup();
	}
	TR_FUNCTION(SV_ReturnConverter=v3dVector2_t)
	static ImVec2        GetCursorPos()
	{
		return ImGui::GetCursorPos();
	}
	static float         GetCursorPosX()
	{
		return ImGui::GetCursorPosX();
	}
	static float         GetCursorPosY()
	{
		return ImGui::GetCursorPosY();
	}
	static void          SetCursorPos(const ImVec2* local_pos)
	{
		return ImGui::SetCursorPos(*local_pos);
	}
	static void          SetCursorPosX(float local_x)
	{
		return ImGui::SetCursorPosX(local_x);
	}
	static void          SetCursorPosY(float local_y)
	{
		return ImGui::SetCursorPosY(local_y);
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetCursorStartPos()
	{
		return ImGui::GetCursorStartPos();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetCursorScreenPos()
	{
		return ImGui::GetCursorScreenPos();
	}
	static void          SetCursorScreenPos(const ImVec2* pos)
	{
		return ImGui::SetCursorScreenPos(*pos);
	}
	static void          AlignTextToFramePadding()
	{
		return ImGui::AlignTextToFramePadding();
	}
	static float         GetTextLineHeight()
	{
		return ImGui::GetTextLineHeight();
	}
	static float         GetTextLineHeightWithSpacing()
	{
		return ImGui::GetTextLineHeightWithSpacing();
	}
	static float         GetFrameHeight()
	{
		return ImGui::GetFrameHeight();
	}
	static float         GetFrameHeightWithSpacing()
	{
		return ImGui::GetFrameHeightWithSpacing();
	}
	// ID stack/scopes
	static void          PushID(const char* str_id)
	{
		return ImGui::PushID(str_id);
	}
	static void          PushID(const char* str_id_begin, const char* str_id_end)
	{
		return ImGui::PushID(str_id_begin, str_id_end);
	}
	static void          PushID(const void* ptr_id)
	{
		return ImGui::PushID(ptr_id);
	}
	static void          PushID(int int_id)
	{
		return ImGui::PushID(int_id);
	}
	static void          PopID()
	{
		return ImGui::PopID();
	}
	static ImGuiID       GetID(const char* str_id)
	{
		return ImGui::GetID(str_id);
	}
	static ImGuiID       GetID(const char* str_id_begin, const char* str_id_end)
	{
		return ImGui::GetID(str_id_begin, str_id_end);
	}
	static ImGuiID       GetID(const void* ptr_id)
	{
		return ImGui::GetID(ptr_id);
	}
	// Widgets: Text
	static void          TextUnformatted(const char* text)
	{
		return ImGui::TextUnformatted(text);
	}
	static void          TextAsiPointer(void* fmt)
	{
		if (fmt == nullptr)
			return ImGui::Text("");
		return ImGui::Text("%s", fmt);
	}
	static void          Text(const char* fmt)
	{
		if (fmt == nullptr)
			return ImGui::Text("");
		return ImGui::Text("%s", fmt);
	}
	static void          TextColored(const ImVec4* col, const char* fmt)
	{
		return ImGui::TextColored(*col, "%s", fmt);
	}
	static void          TextDisabled(const char* fmt)
	{
		return ImGui::TextDisabled("%s", fmt);
	}
	static void          TextWrapped(const char* fmt)
	{
		return ImGui::TextWrapped("%s", fmt);
	}
	static void          LabelText(const char* label, const char* fmt)
	{
		return ImGui::LabelText(label, "%s", fmt);
	}
	static void          BulletText(const char* fmt)
	{
		return ImGui::BulletText("%s", fmt);
	}
	// Widgets: Main
	static bool          Button(const char* label, const ImVec2* size/* = &ImVec2(0, 0)*/)
	{
		return ImGui::Button(label, *size);
	}
	static bool          SmallButton(const char* label)
	{
		return ImGui::SmallButton(label);
	}
	static bool          InvisibleButton(const char* str_id, const ImVec2* size, ImGuiButtonFlags_ flags)
	{
		return ImGui::InvisibleButton(str_id, *size, flags);
	}
	static bool          ArrowButton(const char* str_id, ImGuiDir_ dir)
	{
		return ImGui::ArrowButton(str_id, dir);
	}
	static void          Image(ImTextureID user_texture_id, const ImVec2* size, const ImVec2* uv0/* = &ImVec2(0, 0)*/, const ImVec2* uv1/* = &ImVec2(1, 1)*/, const ImVec4* tint_col/* = &ImVec4(1, 1, 1, 1)*/, const ImVec4* border_col/* = &ImVec4(0, 0, 0, 0)*/)
	{
		return ImGui::Image(user_texture_id, *size, *uv0, *uv1, *tint_col, *border_col);
	}
	static bool          ImageButton(ImTextureID user_texture_id, const ImVec2* size, const ImVec2* uv0, const ImVec2* uv1, int frame_padding, const ImVec4* bg_col, const ImVec4* tint_col)
	{
		return ImGui::ImageButton(user_texture_id, *size, *uv0, *uv1, frame_padding, *bg_col, *tint_col);
	}
	static bool          Checkbox(const char* label, bool* v)
	{
		return ImGui::Checkbox(label, v);
	}
	static bool          CheckboxFlags(const char* label, int* flags, int flags_value)
	{
		return ImGui::CheckboxFlags(label, flags, flags_value);
	}
	static bool          CheckboxFlags(const char* label, unsigned int* flags, unsigned int flags_value)
	{
		return ImGui::CheckboxFlags(label, flags, flags_value);
	}
	static bool          RadioButton(const char* label, bool active)
	{
		return ImGui::RadioButton(label, active);
	}
	static bool          RadioButton(const char* label, int* v, int v_button)
	{
		return ImGui::RadioButton(label, v, v_button);
	}
	static void          ProgressBar(float fraction, const ImVec2* size_arg, const char* overlay)
	{
		return ImGui::ProgressBar(fraction, *size_arg, overlay);
	}
	static void          Bullet()
	{
		return ImGui::Bullet();
	}
	// Widgets: Combo Box
	static bool          BeginCombo(const char* label, const char* preview_value, ImGuiComboFlags_ flags)
	{
		return ImGui::BeginCombo(label, preview_value, flags);
	}
	static void          EndCombo()
	{
		return ImGui::EndCombo();
	}
	static bool          Combo(const char* label, int* current_item, const char** items, int items_count, int popup_max_height_in_items)
	{
		return ImGui::Combo(label, current_item, items, items_count, popup_max_height_in_items);
	}
	static bool          Combo(const char* label, int* current_item, const char* items_separated_by_zeros, int popup_max_height_in_items = -1)
	{
		return ImGui::Combo(label, current_item, items_separated_by_zeros, popup_max_height_in_items);
	}
	static bool          Combo(const char* label, int* current_item, items_getter fn_getter, void* data, int items_count, int popup_max_height_in_items = -1)
	{
		return ImGui::Combo(label, current_item, fn_getter, data, items_count, popup_max_height_in_items);
	}
	// Widgets: Drag Sliders
	static bool          DragFloat(const char* label, float* v, float v_speed, float v_min, float v_max, const char* format, ImGuiSliderFlags_ flags)
	{
		return ImGui::DragFloat(label, v, v_speed, v_min, v_max, format, flags);
	}
	static bool          DragFloat2(const char* label, float* v, float v_speed, float v_min, float v_max, const char* format, ImGuiSliderFlags_ flags)
	{
		return ImGui::DragFloat2(label, v, v_speed, v_min, v_max, format, flags);
	}
	static bool          DragFloat3(const char* label, float* v, float v_speed, float v_min, float v_max, const char* format, ImGuiSliderFlags_ flags)
	{
		return ImGui::DragFloat3(label, v, v_speed, v_min, v_max, format, flags);
	}
	static bool          DragFloat4(const char* label, float* v, float v_speed, float v_min, float v_max, const char* format, ImGuiSliderFlags_ flags)
	{
		return ImGui::DragFloat4(label, v, v_speed, v_min, v_max, format, flags);
	}
	static bool          DragFloatRange2(const char* label, float* v_current_min, float* v_current_max, float v_speed = 1.0f, float v_min = 0.0f, float v_max = 0.0f, const char* format = "%.3f", const char* format_max = NULL, ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::DragFloatRange2(label, v_current_min, v_current_max, v_speed, v_min, v_max, format, format_max, flags);
	}
	static bool          DragInt(const char* label, int* v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::DragInt(label, v, v_speed, v_min, v_max, format, flags);
	}
	static bool          DragInt2(const char* label, int* v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::DragInt2(label, v, v_speed, v_min, v_max, format, flags);
	}
	static bool          DragInt3(const char* label, int* v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::DragInt3(label, v, v_speed, v_min, v_max, format, flags);
	}
	static bool          DragInt4(const char* label, int* v, float v_speed = 1.0f, int v_min = 0, int v_max = 0, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::DragInt4(label, v, v_speed, v_min, v_max, format, flags);
	}
	static bool          DragIntRange2(const char* label, int* v_current_min, int* v_current_max, float v_speed = 1.0f, int v_min = 0, int v_max = 0, const char* format = "%d", const char* format_max = NULL, ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::DragIntRange2(label, v_current_min, v_current_max, v_speed, v_min, v_max, format, format_max, flags);
	}
	static bool          DragScalar(const char* label, ImGuiDataType_ data_type, void* p_data, float v_speed, const void* p_min = NULL, const void* p_max = NULL, const char* format = NULL, ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::DragScalar(label, data_type, p_data, v_speed, p_min, p_max, format, flags);
	}
	static bool          DragScalarN(const char* label, ImGuiDataType_ data_type, void* p_data, int components, float v_speed, const void* p_min = NULL, const void* p_max = NULL, const char* format = NULL, ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::DragScalarN(label, data_type, p_data, components, v_speed, p_min, p_max, format, flags);
	}
	// Widgets: Regular Sliders
	static bool          SliderFloat(const char* label, float* v, float v_min, float v_max, const char* format = "%.3f", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderFloat(label, v, v_min, v_max, format, flags);
	}
	static bool          SliderFloat2(const char* label, float* v, float v_min, float v_max, const char* format = "%.3f", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderFloat2(label, v, v_min, v_max, format, flags);
	}
	static bool          SliderFloat3(const char* label, float* v, float v_min, float v_max, const char* format = "%.3f", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderFloat3(label, v, v_min, v_max, format, flags);
	}
	static bool          SliderFloat4(const char* label, float* v, float v_min, float v_max, const char* format = "%.3f", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderFloat4(label, v, v_min, v_max, format, flags);
	}
	static bool          SliderAngle(const char* label, float* v_rad, float v_degrees_min = -360.0f, float v_degrees_max = +360.0f, const char* format = "%.0f deg", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderAngle(label, v_rad, v_degrees_min, v_degrees_max, format, flags);
	}
	static bool          SliderInt(const char* label, int* v, int v_min, int v_max, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderInt(label, v, v_min, v_max, format, flags);
	}
	static bool          SliderInt2(const char* label, int* v, int v_min, int v_max, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderInt2(label, v, v_min, v_max, format, flags);
	}
	static bool          SliderInt3(const char* label, int* v, int v_min, int v_max, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderInt3(label, v, v_min, v_max, format, flags);
	}
	static bool          SliderInt4(const char* label, int* v, int v_min, int v_max, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderInt4(label, v, v_min, v_max, format, flags);
	}
	static bool          SliderScalar(const char* label, ImGuiDataType_ data_type, void* p_data, const void* p_min, const void* p_max, const char* format = NULL, ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderScalar(label, data_type, p_data, p_min, p_max, format, flags);
	}
	static bool          SliderScalarN(const char* label, ImGuiDataType_ data_type, void* p_data, int components, const void* p_min, const void* p_max, const char* format = NULL, ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::SliderScalarN(label, data_type, p_data, components, p_min, p_max, format, flags);
	}
	static bool          VSliderFloat(const char* label, const ImVec2* size, float* v, float v_min, float v_max, const char* format = "%.3f", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::VSliderFloat(label, *size, v, v_min, v_max, format, flags);
	}
	static bool          VSliderInt(const char* label, const ImVec2* size, int* v, int v_min, int v_max, const char* format = "%d", ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::VSliderInt(label, *size, v, v_min, v_max, format, flags);
	}
	static bool          VSliderScalar(const char* label, const ImVec2* size, ImGuiDataType_ data_type, void* p_data, const void* p_min, const void* p_max, const char* format = NULL, ImGuiSliderFlags_ flags = (ImGuiSliderFlags_)0)
	{
		return ImGui::VSliderScalar(label, *size, data_type, p_data, p_min, p_max, format, flags);
	}
	// Widgets: Input with Keyboard
	static bool          InputText(const char* label, void* buf, UINT buf_size, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0, ImGuiInputTextCallback callback = NULL, void* user_data = NULL)
	{
		return ImGui::InputText(label, (char*)buf, (UINT)buf_size, flags, callback, user_data);
	}
	static bool          InputTextNoName(const char* label, void* buf, UINT buf_size, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0, ImGuiInputTextCallback callback = NULL, void* user_data = NULL)
	{
		//提供一个版本直接c++操作不要显示名字，这主要减少c#的字符串+操作导致的垃圾产生的。
		std::string labelName = "##";
		labelName += label;
		return ImGui::InputText(labelName.c_str(), (char*)buf, (UINT)buf_size, flags, callback, user_data);
	}
	static bool          InputTextMultiline(const char* label, char* buf, UINT buf_size, const ImVec2* size, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0, ImGuiInputTextCallback callback = NULL, void* user_data = NULL)
	{
		return ImGui::InputTextMultiline(label, buf, (UINT)buf_size, *size, flags, callback, user_data);
	}
	static bool          InputTextWithHint(const char* label, const char* hint, char* buf, UINT buf_size, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0, ImGuiInputTextCallback callback = NULL, void* user_data = NULL)
	{
		return ImGui::InputTextWithHint(label, hint, buf, (UINT)buf_size, flags, callback, user_data);
	}
	static bool          InputFloat(const char* label, float* v, float step = 0.0f, float step_fast = 0.0f, const char* format = "%.3f", ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputFloat(label, v, step, step_fast, format, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = v)
	static bool          InputFloat2(const char* label, float* v, const char* format = "%.3f", ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputFloat2(label, v, format, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = v)
	static bool          InputFloat3(const char* label, float* v, const char* format = "%.3f", ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputFloat3(label, v, format, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = v)
	static bool          InputFloat4(const char* label, float* v, const char* format = "%.3f", ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputFloat4(label, v, format, flags);
	}
	static bool          InputInt(const char* label, int* v, int step = 1, int step_fast = 100, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputInt(label, v, step, step_fast, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = v)
	static bool          InputInt2(const char* label, int* v, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputInt2(label, v, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = v)
	static bool          InputInt3(const char* label, int* v, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputInt3(label, v, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = v)
	static bool          InputInt4(const char* label, int* v, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputInt4(label, v, flags);
	}
	static bool          InputDouble(const char* label, double* v, double step = 0.0, double step_fast = 0.0, const char* format = "%.6f", ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputDouble(label, v, step, step_fast, format, flags);
	}
	static bool          InputScalar(const char* label, ImGuiDataType_ data_type, void* p_data, const void* p_step = NULL, const void* p_step_fast = NULL, const char* format = NULL, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputScalar(label, data_type, p_data, p_step, p_step_fast, format, flags);
	}
	static bool          InputScalarN(const char* label, ImGuiDataType_ data_type, void* p_data, int components, const void* p_step = NULL, const void* p_step_fast = NULL, const char* format = NULL, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputScalarN(label, data_type, p_data, components, p_step, p_step_fast, format, flags);
	}
	// Widgets: Color Editor/Picker (tip: the ColorEdit* functions have a little color square that can be left-clicked to open a picker, and right-clicked to open an option menu.)
	TR_FUNCTION(SV_NoStarToRef = col)
	static bool          ColorEdit3(const char* label, float* col, ImGuiColorEditFlags_ flags = (ImGuiColorEditFlags_)0)
	{
		return ImGui::ColorEdit3(label, col, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = col+ref_col)
	static bool          ColorEdit4(const char* label, float* col, ImGuiColorEditFlags_ flags = (ImGuiColorEditFlags_)0)
	{
		return ImGui::ColorEdit4(label, col, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = col)
	static bool          ColorPicker3(const char* label, float* col, ImGuiColorEditFlags_ flags = (ImGuiColorEditFlags_)0)
	{
		return ImGui::ColorPicker3(label, col, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = col+_col2)
	static bool          ColorPicker4(const char* label, float* col, ImGuiColorEditFlags_ flags = (ImGuiColorEditFlags_)0, const float* _col2 = NULL)
	{
		return ImGui::ColorPicker4(label, col, flags, _col2);
	}
	static bool          ColorButton(const char* desc_id, const ImVec4* col, ImGuiColorEditFlags_ flags, ImVec2* size)
	{
		return ImGui::ColorButton(desc_id, *col, flags, *size);
	}
	static void          SetColorEditOptions(ImGuiColorEditFlags_ flags)
	{
		return ImGui::SetColorEditOptions(flags);
	}
	// Widgets: Trees
	static bool          TreeNode(const char* label)
	{
		return ImGui::TreeNode(label);
	}
	static bool          TreeNode(const char* str_id, const char* fmt)
	{
		return ImGui::TreeNode(str_id, "%s", fmt);
	}
	static bool          TreeNode(const void* ptr_id, const char* fmt)
	{
		return ImGui::TreeNode(ptr_id, "%s", fmt);
	}
	static bool          TreeNodeEx(const char* label, ImGuiTreeNodeFlags_ flags)
	{
		return ImGui::TreeNodeEx(label, flags);
	}
	static bool          TreeNodeEx(const char* str_id, ImGuiTreeNodeFlags_ flags, const char* fmt)
	{
		return ImGui::TreeNodeEx(str_id, flags, "%s", fmt);
	}
	static bool          TreeNodeEx(const void* ptr_id, ImGuiTreeNodeFlags_ flags, const char* fmt)
	{
		return ImGui::TreeNodeEx(ptr_id, flags, "%s", fmt);
	}
	static void          TreePush(const char* str_id)
	{
		return ImGui::TreePush(str_id);
	}
	static void          TreePush(const void* ptr_id = NULL)
	{
		return ImGui::TreePush(ptr_id);
	}
	static void          TreePop()
	{
		return ImGui::TreePop();
	}
	static float         GetTreeNodeToLabelSpacing()
	{
		return ImGui::GetTreeNodeToLabelSpacing();
	}
	static bool          CollapsingHeader(const char* label, ImGuiTreeNodeFlags_ flags = (ImGuiTreeNodeFlags_)0)
	{
		return ImGui::CollapsingHeader(label, flags);
	}
	static bool          CollapsingHeader(const char* label, bool* p_open, ImGuiTreeNodeFlags_ flags = (ImGuiTreeNodeFlags_)0)
	{
		return ImGui::CollapsingHeader(label, p_open, flags);
	}
	static void          SetNextItemOpen(bool is_open, ImGuiCond_ cond = (ImGuiCond_)0)
	{
		return ImGui::SetNextItemOpen(is_open, cond);
	}
	// Widgets: Selectables
	static bool          Selectable(const char* label, bool selected, ImGuiSelectableFlags_ flags/* = (ImGuiSelectableFlags_)0*/, const ImVec2* size/* = &ImVec2(0, 0)*/)
	{
		return ImGui::Selectable(label, selected, flags, *size);
	}
	static bool          Selectable(const char* label, bool* p_selected, ImGuiSelectableFlags_ flags/* = (ImGuiSelectableFlags_)0*/, const ImVec2* size/* = &ImVec2(0, 0)*/)
	{
		return ImGui::Selectable(label, p_selected, flags, *size);
	}
	// Widgets: List Boxes
	static bool          ListBox(const char* label, int* current_item, const char** items, int items_count, int height_in_items = -1)
	{
		return ImGui::ListBox(label, current_item, items, items_count, height_in_items);
	}
	static bool          ListBox(const char* label, int* current_item, items_getter fn_getter, void* data, int items_count, int height_in_items = -1)
	{
		return ImGui::ListBox(label, current_item, fn_getter, data, items_count, height_in_items);
	}
	static bool          ListBoxHeader(const char* label, const ImVec2* size) 
	{
		return ImGui::ListBoxHeader(label, *size);
	}
	static bool          ListBoxHeader(const char* label, int items_count, int height_in_items = -1)
	{
		return ImGui::ListBoxHeader(label, items_count, height_in_items);
	}
	static void          ListBoxFooter()
	{
		return ImGui::ListBoxFooter();
	}
	// Widgets: Data Plotting
	static void          PlotLines(const char* label, const float* values, int values_count, int values_offset = 0, const char* overlay_text = NULL, float scale_min = FLT_MAX, float scale_max = FLT_MAX, ImVec2 graph_size = ImVec2(0, 0), int stride = sizeof(float))
	{
		return ImGui::PlotLines(label, values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
	}
	static void          PlotLines(const char* label, values_getter fn_getter, void* data, int values_count, int values_offset = 0, const char* overlay_text = NULL, float scale_min = FLT_MAX, float scale_max = FLT_MAX, ImVec2 graph_size = ImVec2(0, 0))
	{
		return ImGui::PlotLines(label, fn_getter, data, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size);
	}
	static void          PlotHistogram(const char* label, const float* values, int values_count, int values_offset = 0, const char* overlay_text = NULL, float scale_min = FLT_MAX, float scale_max = FLT_MAX, ImVec2 graph_size = ImVec2(0, 0), int stride = sizeof(float))
	{
		return ImGui::PlotHistogram(label, values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
	}
	static void          PlotHistogram(const char* label, values_getter fn_getter, void* data, int values_count, int values_offset = 0, const char* overlay_text = NULL, float scale_min = FLT_MAX, float scale_max = FLT_MAX, ImVec2 graph_size = ImVec2(0, 0))
	{
		return ImGui::PlotHistogram(label, fn_getter, data, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size);
	}
	// Widgets: Value() Helpers.
	static void          Value(const char* prefix, bool b)
	{
		return ImGui::Value(prefix, b);
	}
	static void          Value(const char* prefix, int v)
	{
		return ImGui::Value(prefix, v);
	}
	static void          Value(const char* prefix, unsigned int v)
	{
		return ImGui::Value(prefix, v);
	}
	static void          Value(const char* prefix, float v, const char* float_format = NULL)
	{
		return ImGui::Value(prefix, v, float_format);
	}
	// Widgets: Menus
	static bool          BeginMenuBar()
	{
		return ImGui::BeginMenuBar();
	}
	static void          EndMenuBar()
	{
		return ImGui::EndMenuBar();
	}
	static bool          BeginMainMenuBar()
	{
		return ImGui::BeginMainMenuBar();
	}
	static void          EndMainMenuBar()
	{
		return ImGui::EndMainMenuBar();
	}
	static bool          BeginMenu(const char* label, bool enabled = true)
	{
		return ImGui::BeginMenu(label, enabled);
	}
	static void          EndMenu()
	{
		return ImGui::EndMenu();
	}
	static bool          MenuItem(const char* label, const char* shortcut = NULL, bool selected = false, bool enabled = true)
	{
		return ImGui::MenuItem(label, shortcut, selected, enabled);
	}
	static bool          MenuItem(const char* label, const char* shortcut, bool* p_selected, bool enabled = true)
	{
		return ImGui::MenuItem(label, shortcut, p_selected, enabled);
	}
	// Tooltips
	static void          BeginTooltip()
	{
		return ImGui::BeginTooltip();
	}
	static void          EndTooltip()
	{
		return ImGui::EndTooltip();
	}
	static void          SetTooltip(const char* fmt)
	{
		return ImGui::SetTooltip("%s", fmt);
	}
	// Popups, Modals
	static bool          BeginPopup(const char* str_id, ImGuiWindowFlags_ flags = (ImGuiWindowFlags_)0)
	{
		return ImGui::BeginPopup(str_id, flags);
	}
	TR_FUNCTION(SV_NoStarToRef = p_open)
	static bool          BeginPopupModal(const char* name, bool* p_open = NULL, ImGuiWindowFlags_ flags = (ImGuiWindowFlags_)0)
	{
		return ImGui::BeginPopupModal(name, p_open, flags);
	}
	static void          EndPopup()
	{
		return ImGui::EndPopup();
	}
	static void          OpenPopup(const char* str_id, ImGuiPopupFlags_ popup_flags = (ImGuiPopupFlags_)0)
	{
		return ImGui::OpenPopup(str_id, popup_flags);
	}
	static void          OpenPopupOnItemClick(const char* str_id = NULL, ImGuiPopupFlags_ popup_flags = (ImGuiPopupFlags_)1)
	{
		return ImGui::OpenPopupOnItemClick(str_id, popup_flags);
	}
	static void          CloseCurrentPopup()
	{
		return ImGui::CloseCurrentPopup();
	}
	static bool          BeginPopupContextItem(const char* str_id = NULL, ImGuiPopupFlags_ popup_flags = (ImGuiPopupFlags_)1)
	{
		return ImGui::BeginPopupContextItem(str_id, popup_flags);
	}
	static bool          BeginPopupContextWindow(const char* str_id = NULL, ImGuiPopupFlags_ popup_flags = (ImGuiPopupFlags_)1)
	{
		return ImGui::BeginPopupContextWindow(str_id, popup_flags);
	}
	static bool          BeginPopupContextVoid(const char* str_id = NULL, ImGuiPopupFlags_ popup_flags = (ImGuiPopupFlags_)1)
	{
		return ImGui::BeginPopupContextVoid(str_id, popup_flags);
	}
	static bool          IsPopupOpen(const char* str_id, ImGuiPopupFlags_ flags = (ImGuiPopupFlags_)0)
	{
		return ImGui::IsPopupOpen(str_id, flags);
	}
	// Columns
	static void          Columns(int count = 1, const char* id = NULL, bool border = true)
	{
		return ImGui::Columns(count, id, border);
	}
	static void          NextColumn()
	{
		return ImGui::NextColumn();
	}
	static int           GetColumnIndex()
	{
		return ImGui::GetColumnIndex();
	}
	static float         GetColumnWidth(int column_index = -1)
	{
		return ImGui::GetColumnWidth(column_index);
	}
	static void          SetColumnWidth(int column_index, float width)
	{
		return ImGui::SetColumnWidth(column_index, width);
	}
	static float         GetColumnOffset(int column_index = -1)
	{
		return ImGui::GetColumnOffset(column_index);
	}
	static void          SetColumnOffset(int column_index, float offset_x)
	{
		return ImGui::SetColumnOffset(column_index, offset_x);
	}
	static int           GetColumnsCount()
	{
		return ImGui::GetColumnsCount();
	}
	//Table
	static bool          BeginTable(const char* str_id, int column, ImGuiTableFlags flags = 0, const ImVec2& outer_size = ImVec2(0.0f, 0.0f), float inner_width = 0.0f)
	{
		return ImGui::BeginTable(str_id, column, flags, outer_size, inner_width);
	}
	static void          EndTable() 
	{
		return ImGui::EndTable();
	}
	static void          TableNextRow(ImGuiTableRowFlags row_flags = 0, float min_row_height = 0.0f)
	{
		return ImGui::TableNextRow(row_flags, min_row_height);
	}
	static bool          TableNextColumn()
	{
		return ImGui::TableNextColumn();
	}
	static bool          TableSetColumnIndex(int column_n)
	{
		return ImGui::TableSetColumnIndex(column_n);
	}
	static void          TableSetupColumn(const char* label, ImGuiTableColumnFlags flags = 0, float init_width_or_weight = 0.0f, ImGuiID user_id = 0)
	{
		return ImGui::TableSetupColumn(label, flags, init_width_or_weight, user_id);
	}
	static void          TableSetupScrollFreeze(int cols, int rows)
	{
		return ImGui::TableSetupScrollFreeze(cols, rows);
	}
	static void          TableHeadersRow()
	{
		return ImGui::TableHeadersRow();
	}
	static void          TableHeader(const char* label)
	{
		return ImGui::TableHeader(label);
	}
	static ImGuiTableSortSpecs* TableGetSortSpecs()
	{
		return ImGui::TableGetSortSpecs();
	}
	static int                   TableGetColumnCount()
	{
		return ImGui::TableGetColumnCount();
	}
	static int                   TableGetColumnIndex()
	{
		return ImGui::TableGetColumnIndex();
	}
	static int                   TableGetRowIndex()
	{
		return ImGui::TableGetRowIndex();
	}
	static const char* TableGetColumnName(int column_n = -1)
	{
		return ImGui::TableGetColumnName(column_n);
	}
	static ImGuiTableColumnFlags TableGetColumnFlags(int column_n = -1)
	{
		return ImGui::TableGetColumnFlags(column_n);
	}
	static void                  TableSetColumnEnabled(int column_n, bool v)
	{
		return ImGui::TableSetColumnEnabled(column_n, v);
	}
	static void                  TableSetBgColor(ImGuiTableBgTarget target, ImU32 color, int column_n = -1)
	{
		return ImGui::TableSetBgColor(target, color, column_n);
	}
	// Tab Bars, Tabs
	static bool          BeginTabBar(const char* str_id, ImGuiTabBarFlags_ flags)
	{
		return ImGui::BeginTabBar(str_id, flags);
	}
	static void          EndTabBar()
	{
		return ImGui::EndTabBar();
	}
	TR_FUNCTION(SV_NoStarToRef = p_open)
	static bool          BeginTabItem(const char* label, bool* p_open = NULL, ImGuiTabItemFlags_ flags = (ImGuiTabItemFlags_)0)
	{
		return ImGui::BeginTabItem(label, p_open, flags);
	}
	static void          EndTabItem()
	{
		return ImGui::EndTabItem();
	}
	static bool          TabItemButton(const char* label, ImGuiTabItemFlags_ flags = (ImGuiTabItemFlags_)0)
	{
		return ImGui::TabItemButton(label, flags);
	}
	static void          SetTabItemClosed(const char* tab_or_docked_window_label)
	{
		return ImGui::SetTabItemClosed(tab_or_docked_window_label);
	}
	// Docking
	static ImGuiID       DockSpace(ImGuiID id, const ImVec2& size = ImVec2(0, 0), ImGuiDockNodeFlags flags = 0, const ImGuiWindowClass* window_class = NULL)
	{
		return ImGui::DockSpace(id, size, flags, window_class);
	}
	static ImGuiID       DockSpaceOverViewport(ImGuiViewport* viewport = NULL, ImGuiDockNodeFlags flags = 0, const ImGuiWindowClass* window_class = NULL)
	{
		return ImGui::DockSpaceOverViewport(viewport, flags, window_class);
	}
	static void          SetNextWindowDockID(ImGuiID dock_id, ImGuiCond_ cond = (ImGuiCond_)0)
	{
		return ImGui::SetNextWindowDockID(dock_id, cond);
	}
	static void          SetNextWindowClass(const ImGuiWindowClass* window_class)
	{
		return ImGui::SetNextWindowClass(window_class);
	}
	static ImGuiID       GetWindowDockID()
	{
		return ImGui::GetWindowDockID();
	}
	static bool          IsWindowDocked()
	{
		return ImGui::IsWindowDocked();
	}
	// Logging/Capture
	static void          LogToTTY(int auto_open_depth = -1)
	{
		return ImGui::LogToTTY(auto_open_depth);
	}
	static void          LogToFile(int auto_open_depth = -1, const char* filename = NULL)
	{
		return ImGui::LogToFile(auto_open_depth, filename);
	}
	static void          LogToClipboard(int auto_open_depth = -1)
	{
		return ImGui::LogToClipboard(auto_open_depth);
	}
	static void          LogFinish()
	{
		return ImGui::LogFinish();
	}
	static void          LogButtons()
	{
		return ImGui::LogButtons();
	}
	static void          LogText(const char* fmt)
	{
		return ImGui::LogText("%s", fmt);
	}
	// Drag and Drop
	static bool          BeginDragDropSource(ImGuiDragDropFlags_ flags = (ImGuiDragDropFlags_)0)
	{
		return ImGui::BeginDragDropSource(flags);
	}
	static bool          SetDragDropPayload(const char* type, const void* data, UINT sz, ImGuiCond_ cond = (ImGuiCond_)0)
	{
		return ImGui::SetDragDropPayload(type, data, (UINT)sz, cond);
	}
	static void          EndDragDropSource()
	{
		return ImGui::EndDragDropSource();
	}
	static bool                  BeginDragDropTarget()
	{
		return ImGui::BeginDragDropTarget();
	}
	static const ImGuiPayload*   AcceptDragDropPayload(const char* type, ImGuiDragDropFlags_ flags = (ImGuiDragDropFlags_)0)
	{
		return ImGui::AcceptDragDropPayload(type, flags);
	}
	static void                  EndDragDropTarget()
	{
		return ImGui::EndDragDropTarget();
	}
	static const ImGuiPayload*   GetDragDropPayload()
	{
		return ImGui::GetDragDropPayload();
	}
	// Clipping
	static void          PushClipRect(const ImVec2* clip_rect_min, const ImVec2* clip_rect_max, bool intersect_with_current_clip_rect)
	{
		return ImGui::PushClipRect(*clip_rect_min, *clip_rect_max, intersect_with_current_clip_rect);
	}
	static void          PopClipRect()
	{
		return ImGui::PopClipRect();
	}
	// Focus, Activation
	static void          SetItemDefaultFocus()
	{
		return ImGui::SetItemDefaultFocus();
	}
	static void          SetKeyboardFocusHere(int offset = 0)
	{
		return ImGui::SetKeyboardFocusHere(offset);
	}
	// Item/Widgets Utilities
	static bool          IsItemHovered(ImGuiHoveredFlags_ flags = (ImGuiHoveredFlags_)0)
	{
		return ImGui::IsItemHovered(flags);
	}
	static bool          IsItemActive()
	{
		return ImGui::IsItemActive();
	}
	static bool          IsItemFocused()
	{
		return ImGui::IsItemFocused();
	}
	static bool          IsItemClicked(ImGuiMouseButton_ mouse_button = (ImGuiMouseButton_)0)
	{
		return ImGui::IsItemClicked(mouse_button);
	}
	static bool          IsItemVisible()
	{
		return ImGui::IsItemVisible();
	}
	static bool          IsItemEdited()
	{
		return ImGui::IsItemEdited();
	}
	static bool          IsItemActivated()
	{
		return ImGui::IsItemActivated();
	}
	static bool          IsItemDeactivated()
	{
		return ImGui::IsItemDeactivated();
	}
	static bool          IsItemDeactivatedAfterEdit()
	{
		return ImGui::IsItemDeactivatedAfterEdit();
	}
	static bool          IsItemToggledOpen()
	{
		return ImGui::IsItemToggledOpen();
	}
	static bool          IsAnyItemHovered()
	{
		return ImGui::IsAnyItemHovered();
	}
	static bool          IsAnyItemActive()
	{
		return ImGui::IsAnyItemActive();
	}
	static bool          IsAnyItemFocused()
	{
		return ImGui::IsAnyItemFocused();
	}
	TR_FUNCTION(SV_ReturnConverter=v3dVector2_t)
	static ImVec2        GetItemRectMin()
	{
		return ImGui::GetItemRectMin();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetItemRectMax()
	{
		return ImGui::GetItemRectMax();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetItemRectSize()
	{
		return ImGui::GetItemRectSize();
	}
	static void          SetItemAllowOverlap()
	{
		return ImGui::SetItemAllowOverlap();
	}
	// Miscellaneous Utilities
	static bool          IsRectVisible(const ImVec2* size)
	{
		return ImGui::IsRectVisible(*size);
	}
	static bool          IsRectVisible(const ImVec2* rect_min, const ImVec2* rect_max)
	{
		return ImGui::IsRectVisible(* rect_min, * rect_max);
	}
	static double        GetTime()
	{
		return ImGui::GetTime();
	}
	static int           GetFrameCount()
	{
		return ImGui::GetFrameCount();
	}
	static ImDrawList*   GetBackgroundDrawList()
	{
		return ImGui::GetBackgroundDrawList();
	}
	static ImDrawList*   GetForegroundDrawList()
	{
		return ImGui::GetForegroundDrawList();
	}
	static ImDrawList*   GetBackgroundDrawList(ImGuiViewport* viewport)
	{
		return ImGui::GetBackgroundDrawList(viewport);
	}
	static ImDrawList*   GetForegroundDrawList(ImGuiViewport* viewport)
	{
		return ImGui::GetForegroundDrawList(viewport);
	}
	static void* GetDrawListSharedData()
	{
		return ImGui::GetDrawListSharedData();
	}
	static const char*   GetStyleColorName(ImGuiCol_ idx)
	{
		return ImGui::GetStyleColorName(idx);
	}
	static void          SetStateStorage(ImGuiStorage* storage)
	{
		return ImGui::SetStateStorage(storage);
	}
	static ImGuiStorage* GetStateStorage()
	{
		return ImGui::GetStateStorage();
	}
	static void          CalcListClipping(int items_count, float items_height, int* out_items_display_start, int* out_items_display_end)
	{
		return ImGui::CalcListClipping(items_count, items_height, out_items_display_start, out_items_display_end);
	}
	static bool          BeginChildFrame(ImGuiID id, const ImVec2* size, ImGuiWindowFlags_ flags = (ImGuiWindowFlags_)0)
	{
		return ImGui::BeginChildFrame(id, *size, flags);
	}
	static void          EndChildFrame()
	{
		return ImGui::EndChildFrame();
	}
	// Text Utilities
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        CalcTextSize(const char* text, bool hide_text_after_double_hash = false, float wrap_width = -1.0f)
	{
		return ImGui::CalcTextSize(text, nullptr, hide_text_after_double_hash, wrap_width);
	}
	// Color Utilities
	TR_FUNCTION(SV_ReturnConverter = v3dVector4_t)
	static ImVec4        ColorConvertU32ToFloat4(ImU32 inValue)
	{
		return ImGui::ColorConvertU32ToFloat4(inValue);
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImU32         ColorConvertFloat4ToU32(const ImVec4* inValue)
	{
		return ImGui::ColorConvertFloat4ToU32(*inValue);
	}
	static void          ColorConvertRGBtoHSV(float r, float g, float b, float* out_h, float* out_s, float* out_v)
	{
		return ImGui::ColorConvertRGBtoHSV(r, g, b, *out_h, *out_s, *out_v);
	}
	static void          ColorConvertHSVtoRGB(float h, float s, float v, float* out_r, float* out_g, float* out_b)
	{
		return ImGui::ColorConvertHSVtoRGB(h, s, v, *out_r, *out_g, *out_b);
	}
	// Inputs Utilities: Keyboard
	static int           GetKeyIndex(ImGuiKey_ imgui_key)
	{
		return ImGui::GetKeyIndex(imgui_key);
	}
	static bool          IsKeyDown(int user_key_index)
	{
		return ImGui::IsKeyDown(user_key_index);
	}
	static bool          IsKeyPressed(int user_key_index, bool repeat = true)
	{
		return ImGui::IsKeyPressed(user_key_index, repeat);
	}
	static bool          IsKeyReleased(int user_key_index)
	{
		return ImGui::IsKeyReleased(user_key_index);
	}
	static int           GetKeyPressedAmount(int key_index, float repeat_delay, float rate)
	{
		return ImGui::GetKeyPressedAmount(key_index, repeat_delay, rate);
	}
	static void          CaptureKeyboardFromApp(bool want_capture_keyboard_value = true)
	{
		return ImGui::CaptureKeyboardFromApp(want_capture_keyboard_value);
	}
	// Inputs Utilities: Mouse
	static bool          IsMouseDown(ImGuiMouseButton_ button)
	{
		return ImGui::IsMouseDown(button);
	}
	static bool          IsMouseClicked(ImGuiMouseButton_ button, bool repeat = false)
	{
		return ImGui::IsMouseClicked(button, repeat);
	}
	static bool          IsMouseReleased(ImGuiMouseButton_ button)
	{
		return ImGui::IsMouseReleased(button);
	}
	static bool          IsMouseDoubleClicked(ImGuiMouseButton_ button)
	{
		return ImGui::IsMouseDoubleClicked(button);
	}
	static bool          IsMouseHoveringRect(const ImVec2* r_min, const ImVec2* r_max, bool clip = true)
	{
		return ImGui::IsMouseHoveringRect(* r_min, * r_max, clip);
	}
	static bool          IsMousePosValid(const ImVec2* mouse_pos = NULL)
	{
		return ImGui::IsMousePosValid(mouse_pos);
	}
	static bool          IsAnyMouseDown()
	{
		return ImGui::IsAnyMouseDown();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetMousePos()
	{
		return ImGui::GetMousePos();
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetMousePosOnOpeningCurrentPopup()
	{
		return ImGui::GetMousePosOnOpeningCurrentPopup();
	}
	static bool          IsMouseDragging(ImGuiMouseButton_ button, float lock_threshold = -1.0f)
	{
		return ImGui::IsMouseDragging(button, lock_threshold);
	}
	TR_FUNCTION(SV_ReturnConverter = v3dVector2_t)
	static ImVec2        GetMouseDragDelta(ImGuiMouseButton_ button, float lock_threshold = -1.0f)
	{
		return ImGui::GetMouseDragDelta(button, lock_threshold);
	}
	static void          ResetMouseDragDelta(ImGuiMouseButton_ button)
	{
		return ImGui::ResetMouseDragDelta(button);
	}
	static ImGuiMouseCursor_ GetMouseCursor()
	{
		return (ImGuiMouseCursor_)ImGui::GetMouseCursor();
	}
	static void          SetMouseCursor(ImGuiMouseCursor_ cursor_type)
	{
		return ImGui::SetMouseCursor(cursor_type);
	}
	static void          CaptureMouseFromApp(bool want_capture_mouse_value = true)
	{
		return ImGui::CaptureMouseFromApp(want_capture_mouse_value);
	}
	// Clipboard Utilities
	static const char*   GetClipboardText()
	{
		return ImGui::GetClipboardText();
	}
	static void          SetClipboardText(const char* text)
	{
		return ImGui::SetClipboardText(text);
	}
	// Settings/.Ini Utilities
	static void          LoadIniSettingsFromDisk(const char* ini_filename)
	{
		return ImGui::LoadIniSettingsFromDisk(ini_filename);
	}
	static void          LoadIniSettingsFromMemory(const char* ini_data, UINT ini_size = 0)
	{
		return ImGui::LoadIniSettingsFromMemory(ini_data, ini_size);
	}
	static void          SaveIniSettingsToDisk(const char* ini_filename)
	{
		return ImGui::SaveIniSettingsToDisk(ini_filename);
	}
	static const char*   SaveIniSettingsToMemory(UINT* out_ini_size = NULL)
	{
		size_t sz;
		auto ret = ImGui::SaveIniSettingsToMemory(&sz);
		if(out_ini_size!=NULL)
			*out_ini_size = (UINT)sz;
		return ret;
	}
	// Debug Utilities
	static bool          DebugCheckVersionAndDataLayout(const char* version_str, size_t sz_io, size_t sz_style, size_t sz_vec2, size_t sz_vec4, size_t sz_drawvert, size_t sz_drawidx)
	{
		return ImGui::DebugCheckVersionAndDataLayout(version_str, sz_io, sz_style, sz_vec2, sz_vec4, sz_drawvert, sz_drawidx);
	}
	// Memory Allocators
	static void          SetAllocatorFunctions(alloc_func fn_alloc_func, free_func fn_free_func, void* user_data = NULL)
	{
		return ImGui::SetAllocatorFunctions(fn_alloc_func, fn_free_func, user_data);
	}
	static void*         MemAlloc(size_t size)
	{
		return ImGui::MemAlloc(size);
	}
	static void          MemFree(void* ptr)
	{
		return ImGui::MemFree(ptr);
	}
	// (Optional) Platform/OS interface for multi-viewport support
	static ImGuiPlatformIO*  GetPlatformIO()
	{
		return &ImGui::GetPlatformIO();
	}
	static ImGuiViewport*    GetMainViewport()
	{
		return ImGui::GetMainViewport();
	}
	static void              UpdatePlatformWindows()
	{
		return ImGui::UpdatePlatformWindows();
	}
	static void              RenderPlatformWindowsDefault(void* platform_render_arg = NULL, void* renderer_render_arg = NULL)
	{
		return ImGui::RenderPlatformWindowsDefault(platform_render_arg, renderer_render_arg);
	}
	static void              DestroyPlatformWindows()
	{
		return ImGui::DestroyPlatformWindows();
	}
	static ImGuiViewport*    FindViewportByID(ImGuiID id)
	{
		return ImGui::FindViewportByID(id);
	}
	static ImGuiViewport*    FindViewportByPlatformHandle(void* platform_handle)
	{
		return ImGui::FindViewportByPlatformHandle(platform_handle);
	}
	// (Optional) Renderer functions (e.g. DirectX, OpenGL, Vulkan)
	static void Set_Renderer_CreateWindow(ImGuiPlatformIO* PlatformIO, Renderer_CreateWindow fn)
	{
		PlatformIO->Renderer_CreateWindow = fn;
	}
	static void Set_Renderer_DestroyWindow(ImGuiPlatformIO* PlatformIO, Renderer_DestroyWindow fn)
	{
		PlatformIO->Renderer_DestroyWindow = fn;
	}
	static void Set_Renderer_SetWindowSize(ImGuiPlatformIO* PlatformIO, Renderer_SetWindowSize fn)
	{
		PlatformIO->Renderer_SetWindowSize = fn;
	}
	static void Set_Renderer_RenderWindow(ImGuiPlatformIO* PlatformIO, Renderer_RenderWindow fn)
	{
		PlatformIO->Renderer_RenderWindow = fn;
	}
	static void Set_Renderer_SwapBuffers(ImGuiPlatformIO* PlatformIO, Renderer_SwapBuffers fn)
	{
		PlatformIO->Renderer_SwapBuffers = fn;
	}
	static void		ImGui_NativeWindow_EnableDpiAwareness();
	static bool     ImGui_NativeWindow_Init(void* hwnd);
	static void     ImGui_NativeWindow_Shutdown();
	static void     ImGui_NativeWindow_NewFrame();

	static void PlatformIO_Monitor_Resize(ImGuiPlatformIO* io, int size)
	{
		io->Monitors.resize(size);
	}
	static void PlatformIO_Monitor_PushBack(ImGuiPlatformIO* io, ImGuiPlatformMonitor monitor)
	{
		io->Monitors.push_back(monitor);
	}

	static int PlatformIO_Viewports_Size(ImGuiPlatformIO* io)
	{
		return io->Viewports.size();
	}
	static ImGuiViewport* PlatformIO_Viewports_Get(ImGuiPlatformIO* io, int index)
	{
		return io->Viewports[index];
	}

	// Ext Controls
	TR_FUNCTION(SV_NoStarToRef = buffer)
	static bool		TextInputComboBox(const char* id, void* buffer, UINT maxInputSize, const char** items, UINT item_len, short showMaxItems);

	TR_FUNCTION(SV_NoStarToRef = buffer)
	static void		GetClipboardTextSetter(ImGuiIO* io, FGetClipboardTextFn fn)
	{
		io->GetClipboardTextFn = fn;
	}
	TR_FUNCTION(SV_NoStarToRef = buffer)
	static void		SetClipboardTextSetter(ImGuiIO* io, FSetClipboardTextFn fn)
	{
		typedef void(*TFunType)(void* user_data, const char* text);
		io->SetClipboardTextFn = (TFunType)fn;
	}

	static void      ItemSize(const ImVec2* min, const ImVec2* max, float text_baseline_y)
	{
		ImRect rect(*min, *max);
		ImGui::ItemSize(rect, text_baseline_y);
	}
	static void      ItemSize(const ImVec2* size, float text_baseline_y)
	{
		ImGui::ItemSize(*size, text_baseline_y);
	}
	static bool		 ItemAdd(const ImVec2* bbMin, const ImVec2* bbMax, ImGuiID id, const ImVec2* nav_bb_min, const ImVec2* nav_bb_max, int flags)
	{
		ImRect bb(*bbMin, *bbMax);
		if (nav_bb_min == nullptr || nav_bb_max == nullptr)
		{
			return ImGui::ItemAdd(bb, id, nullptr, flags);
		}
		else
		{
			ImRect nav_bb(*nav_bb_min, *nav_bb_max);
			return ImGui::ItemAdd(bb, id, &nav_bb, flags);
		}
	}
	static bool		 ItemAdd(const ImVec2* bbMin, const ImVec2* bbMax, ImGuiID id, int flags)
	{
		ImRect bb(*bbMin, *bbMax);
		return ImGui::ItemAdd(bb, id, nullptr, flags);
	}
	static bool GetTableWorkRect(ImVec2* min, ImVec2* max)
	{
		if (min == nullptr || max == nullptr)
			return false;
		ImGuiTable* table = ImGui::GetCurrentTable();
		if (table == nullptr)
			return false;
		*min = table->WorkRect.Min;
		*max = table->WorkRect.Max;
		return true;
	}
	static bool GetTableRowStartY(float* yValue)
	{
		if (yValue == nullptr)
			return false;
		ImGuiTable* table = ImGui::GetCurrentTable();
		if (table == nullptr)
			return false;
		*yValue = table->RowPosY1;
		return true;
	}
	static bool GetTableRowEndY(float* yValue)
	{
		if (yValue == nullptr)
			return false;
		ImGuiTable* table = ImGui::GetCurrentTable();
		if (table == nullptr)
			return false;
		*yValue = table->RowPosY2;
		return true;
	}
	static bool IsHoverCurrentWindow()
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return false;
		return context->CurrentWindow == context->HoveredWindow;
	}
	static bool IsMouseHoveringRectInCurrentWindow(const ImVec2* r_min, const ImVec2* r_max, bool clip = true)
	{
		return ImGui::IsMouseHoveringRect(*r_min, *r_max, clip) && IsHoverCurrentWindow();
	}
	static bool IsMouseDownInRectInCurrentWindow(const ImVec2* r_min, const ImVec2* r_max, ImGuiMouseButton_ button, bool clip = true)
	{
		return ImGui::IsMouseHoveringRect(*r_min, *r_max, clip) && IsHoverCurrentWindow() && IsMouseDown(button);
	}
	static bool IsMouseClickedInRectInCurrentWindow(const ImVec2* r_min, const ImVec2* r_max, ImGuiMouseButton_ button, bool clip = true)
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return false;
		IM_ASSERT(button >= 0 && button < IM_ARRAYSIZE(context->IO.MouseDown));
		return ImGui::IsMouseHoveringRect(*r_min, *r_max, clip) && IsHoverCurrentWindow() && context->IO.MouseClicked[button];
	}
	static bool IsMouseDoubleClickedInRectInCurrentWindow(const ImVec2* r_min, const ImVec2* r_max, ImGuiMouseButton_ button, bool clip = true)
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return false;
		IM_ASSERT(button >= 0 && button < IM_ARRAYSIZE(context->IO.MouseDown));
		return ImGui::IsMouseHoveringRect(*r_min, *r_max, clip) && IsHoverCurrentWindow() && context->IO.MouseDoubleClicked[button];
	}
	static bool IsMouseDragPastThreshold(ImGuiMouseButton_ button, float lock_threshold = -1.0f)
	{
		return ImGui::IsMouseDragPastThreshold(button, lock_threshold);
	}
	static bool IsCurrentWindowSkipItems()
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		return window->SkipItems;
	}
	static bool          ItemHoverable(const ImVec2* bbMin, const ImVec2* bbMax, ImGuiID id)
	{
		if (bbMin == nullptr || bbMax == nullptr)
			return false;

		ImRect rect(*bbMin, *bbMax);
		return ImGui::ItemHoverable(rect, id);
	}
	static bool             TempInputIsActive(ImGuiID id)
	{
		return ImGui::TempInputIsActive(id);
	}
	static bool LastItemStatusFlagsHasFocused()
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window == nullptr)
			return false;
		return (window->DC.LastItemStatusFlags & ImGuiItemStatusFlags_Focused) != 0;
	}
	static void SetActiveID(ImGuiID id)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window != nullptr)
			ImGui::SetActiveID(id, window);
	}
	static ImGuiID GetActiveID()
	{
		return ImGui::GetActiveID();
	}
	static void SetFocusID(ImGuiID id)
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window != nullptr)
			ImGui::SetFocusID(id, window);
	}
	static void SetNavInputID(ImGuiID id)
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return;
		context->NavInputId = id;
	}
	static void SetTempInputID(ImGuiID id)
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return;
		context->TempInputId = id;
	}
	static ImGuiID GetTempInputID()
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return 0;
		return context->TempInputId;
	}
	static void ClearActiveID()
	{
		ImGui::ClearActiveID();
	}
	static void FocusCurrentWindow()
	{
		ImGuiWindow* window = ImGui::GetCurrentWindow();
		if (window != nullptr)
			ImGui::FocusWindow(window);
	}
	static bool IsIDNavActivated(ImGuiID id)
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return false;
		return context->NavActivateId == id;
	}
	static bool IsIDNavInput(ImGuiID id)
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return false;
		return context->NavInputId == id;
	}
	static bool DragBehavior(ImGuiID id, ImGuiDataType data_type, void* p_v, float v_speed, const void* p_min, const void* p_max, const char* format, ImGuiSliderFlags_ flags)
	{
		return ImGui::DragBehavior(id, data_type, p_v, v_speed, p_min, p_max, format, flags);
	}
	static bool DragActiveIdUpdate(ImGuiID id)
	{
		auto context = ImGui::GetCurrentContext();
		if (context == nullptr)
			return false;
		if (context->ActiveId == id)
		{
			if (context->ActiveIdSource == ImGuiInputSource_Mouse && !context->IO.MouseDown[0])
				ImGui::ClearActiveID();
			else if (context->ActiveIdSource == ImGuiInputSource_Nav && context->NavActivatePressedId == id && !context->ActiveIdIsJustActivated)
				ImGui::ClearActiveID();
		}
		if (context->ActiveId != id)
			return false;
		return true;
	}

	static bool ButtonBehavior(const ImVec2* min, const ImVec2* max, ImGuiID id, bool* out_hovered, bool* out_held, ImGuiButtonFlags_ flags)
	{
		ImRect rect(*min, *max);
		return ImGui::ButtonBehavior(rect, id, out_hovered, out_held, flags);
	}

	static bool DragScalar2(const char* label, ImGuiDataType data_type, void* p_data, float v_speed, const void* p_min, const void* p_max, const char* format, ImGuiSliderFlags_ flags);
	static bool DragScalarN2(const char* label, ImGuiDataType data_type, void* p_data, int components, float v_speed, const void* p_min, const void* p_max, const char* format, ImGuiSliderFlags_ flags);

	static void RenderFrame(ImVec2* p_min, ImVec2* p_max, ImU32 fill_col, bool border, float rounding)
	{
		ImGuiContext& g = *GImGui;
		ImGuiWindow* window = g.CurrentWindow;
		window->DrawList->AddRectFilled(*p_min, *p_max, fill_col, rounding);
		const float border_size = g.Style.FrameBorderSize;
		if (border && border_size > 0.0f)
		{
			window->DrawList->AddRect(*p_min + ImVec2(1, 1), *p_max + ImVec2(1, 1), GetColorU32(ImGuiCol_BorderShadow), rounding, 0, border_size);
			window->DrawList->AddRect(*p_min, *p_max, GetColorU32(ImGuiCol_Border), rounding, 0, border_size);
		}
	}

	static ImVec2 CalcItemSize(ImVec2* size, float default_w, float default_h)
	{
		return ImGui::CalcItemSize(*size, default_w, default_h);
	}

	static bool CollapsingHeader_SpanAllColumns(const char* label, ImGuiTreeNodeFlags_ flags);
	static void TableNextRow(const ImGuiTableRowData* rowData);
	static void TableNextRow_FirstColumn(const ImGuiTableRowData* rowData);
	static void TableSetCellPaddingY(float value)
	{
		ImGuiContext& g = *GImGui;
		ImGuiTable* table = g.CurrentTable;
		if (table == nullptr)
			return;

		table->CellPaddingY = value;
	}
	static bool CheckBoxTristate(const char* label, int* v_tristate)
	{
		bool ret = false;
		if (*v_tristate == -1)
		{
			ImGui::PushItemFlag(ImGuiItemFlags_MixedValue, true);
			bool b = false;
			ret = ImGui::Checkbox(label, &b);
			if (ret)
				*v_tristate = 1;
			ImGui::PopItemFlag();
		}
		else
		{
			bool b = (*v_tristate != 0);
			ret = ImGui::Checkbox(label, &b);
			if (ret)
				*v_tristate = (int)b;
		}
		return ret;
	}
};

StructBegin(ImVec2, )
StructEnd(void)

StructBegin(ImVec4, )
StructEnd(void)

VTypeHelperDefine(items_getter, sizeof(void*));
VTypeHelperDefine(values_getter, sizeof(void*));
VTypeHelperDefine(alloc_func, sizeof(void*));
VTypeHelperDefine(free_func, sizeof(void*));
VTypeHelperDefine(ImGuiSizeCallback, sizeof(void*));
VTypeHelperDefine(ImGuiInputTextCallback, sizeof(void*));
VTypeHelperDefine(ImDrawCallback, sizeof(void*));

VTypeHelperDefine(Renderer_CreateWindow, sizeof(void*));
//VTypeHelperDefine(Renderer_DestroyWindow, sizeof(void*));
VTypeHelperDefine(Renderer_SetWindowSize, sizeof(void*));
VTypeHelperDefine(Renderer_RenderWindow, sizeof(void*));
//VTypeHelperDefine(Renderer_SwapBuffers, sizeof(void*));

VTypeHelperDefine(FGetClipboardTextFn, sizeof(void*));
VTypeHelperDefine(FSetClipboardTextFn, sizeof(void*));

StructBegin(items_getter, EngineNS)
StructEnd(void)

StructBegin(values_getter, EngineNS)
StructEnd(void)

StructBegin(alloc_func, EngineNS)
StructEnd(void)

StructBegin(free_func, EngineNS)
StructEnd(void)

StructBegin(ImGuiSizeCallback, )
StructEnd(void)

StructBegin(ImGuiInputTextCallback, )
StructEnd(void)

StructBegin(ImDrawCallback, )
StructEnd(void)

StructBegin(Renderer_CreateWindow, EngineNS)
StructEnd(void)

StructBegin(Renderer_SetWindowSize, EngineNS)
StructEnd(void)

StructBegin(Renderer_RenderWindow, EngineNS)
StructEnd(void)

StructBegin(FGetClipboardTextFn, EngineNS)
StructEnd(void)

//StructBegin(FSetClipboardTextFn, EngineNS)
//StructEnd(void)

NS_END