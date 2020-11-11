#pragma once
#include "imgui.h"

NS_BEGIN

TR_CALLBACK()
typedef bool(*items_getter)(void* data, int idx, const char** out_text);
TR_CALLBACK()
typedef float(*values_getter)(void* data, int idx);

TR_CLASS(SV_NameSpace = EngineNS, SV_UsingNS = ImGui, SV_ReflectAll)
class ImGuiAPI
{
public:
	static void* CreateContext(ImFontAtlas* shared_font_atlas)
	{
		return ImGui::CreateContext(shared_font_atlas);
	}
	static void DestroyContext(void* ctx)
	{
		return ImGui::DestroyContext((ImGuiContext*)ctx);
	}
	static void* GetCurrentContext()
	{
		return ImGui::GetCurrentContext();
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
	static bool          BeginChild(const char* str_id, const ImVec2* size, bool border, ImGuiWindowFlags_ flags)
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
	static void          SetNextWindowPos(const ImVec2* pos, ImGuiCond_ cond, const ImVec2* pivot)
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
	static void          Text(const char* fmt)
	{
		return ImGui::Text(fmt);
	}
	static void          TextColored(const ImVec4* col, const char* fmt)
	{
		return ImGui::TextColored(*col, fmt);
	}
	static void          TextDisabled(const char* fmt)
	{
		return ImGui::TextDisabled(fmt);
	}
	static void          TextWrapped(const char* fmt)
	{
		return ImGui::TextWrapped(fmt);
	}
	static void          LabelText(const char* label, const char* fmt)
	{
		return ImGui::LabelText(label, fmt);
	}
	static void          BulletText(const char* fmt)
	{
		return ImGui::BulletText(fmt);
	}
	// Widgets: Main
	static bool          Button(const char* label, const ImVec2* size)
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
	static void          Image(ImTextureID user_texture_id, const ImVec2* size, const ImVec2* uv0, const ImVec2* uv1, const ImVec4* tint_col, const ImVec4* border_col)
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
	static bool          InputText(const char* label, char* buf, UINT buf_size, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0, ImGuiInputTextCallback callback = NULL, void* user_data = NULL)
	{
		return ImGui::InputText(label, buf, (UINT)buf_size, flags, callback, user_data);
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
	static bool          InputFloat2(const char* label, float* v, const char* format = "%.3f", ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputFloat2(label, v, format, flags);
	}
	static bool          InputFloat3(const char* label, float* v, const char* format = "%.3f", ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputFloat3(label, v, format, flags);
	}
	static bool          InputFloat4(const char* label, float* v, const char* format = "%.3f", ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputFloat4(label, v, format, flags);
	}
	static bool          InputInt(const char* label, int* v, int step = 1, int step_fast = 100, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputInt(label, v, step, step_fast, flags);
	}
	static bool          InputInt2(const char* label, int* v, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputInt2(label, v, flags);
	}
	static bool          InputInt3(const char* label, int* v, ImGuiInputTextFlags_ flags = (ImGuiInputTextFlags_)0)
	{
		return ImGui::InputInt3(label, v, flags);
	}
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
	static bool          ColorEdit3(const char* label, float* col, ImGuiColorEditFlags_ flags = (ImGuiColorEditFlags_)0)
	{
		return ImGui::ColorEdit3(label, col, flags);
	}
	static bool          ColorEdit4(const char* label, float* col, ImGuiColorEditFlags_ flags = (ImGuiColorEditFlags_)0)
	{
		return ImGui::ColorEdit4(label, col, flags);
	}
	static bool          ColorPicker3(const char* label, float* col, ImGuiColorEditFlags_ flags = (ImGuiColorEditFlags_)0)
	{
		return ImGui::ColorPicker3(label, col, flags);
	}
	static bool          ColorPicker4(const char* label, float* col, ImGuiColorEditFlags_ flags = (ImGuiColorEditFlags_)0, const float* ref_col = NULL)
	{
		return ImGui::ColorPicker4(label, col, flags, ref_col);
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
		return ImGui::TreeNode(str_id, fmt);
	}
	static bool          TreeNode(const void* ptr_id, const char* fmt)
	{
		return ImGui::TreeNode(ptr_id, fmt);
	}
	static bool          TreeNodeEx(const char* label, ImGuiTreeNodeFlags_ flags)
	{
		return ImGui::TreeNodeEx(label, flags);
	}
	static bool          TreeNodeEx(const char* str_id, ImGuiTreeNodeFlags_ flags, const char* fmt)
	{
		return ImGui::TreeNodeEx(str_id, flags, fmt);
	}
	static bool          TreeNodeEx(const void* ptr_id, ImGuiTreeNodeFlags_ flags, const char* fmt)
	{
		return ImGui::TreeNodeEx(ptr_id, flags, fmt);
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
	static bool          Selectable(const char* label, bool selected, ImGuiSelectableFlags_ flags, const ImVec2* size)
	{
		return ImGui::Selectable(label, selected, flags, *size);
	}
	static bool          Selectable(const char* label, bool* p_selected, ImGuiSelectableFlags_ flags, const ImVec2* size)
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
		return ImGui::SetTooltip(fmt);
	}
	// Popups, Modals
	static bool          BeginPopup(const char* str_id, ImGuiWindowFlags_ flags = (ImGuiWindowFlags_)0)
	{
		return ImGui::BeginPopup(str_id, flags);
	}
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
		return ImGui::EndPopup();
	}
	static void          OpenPopupOnItemClick(const char* str_id = NULL, ImGuiPopupFlags_ popup_flags = (ImGuiPopupFlags_)1)
	{
		return ImGui::EndPopup();
	}
	static void          CloseCurrentPopup()
	{
		return ImGui::EndPopup();
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
};

StructBegin(ImVec2,)
StructEnd(void)

StructBegin(ImVec4, )
StructEnd(void)

NS_END