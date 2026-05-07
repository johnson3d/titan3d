//generated from legacy CppWeaving ImGui surface; owned by TitanImGuiBridge
#include "../../../Core.Window/pch.h"
#if defined(HasModule_ImGui)
#include "../imgui_binding.h"
#include "../TitanImGuiBridge.h"


#define new VNEW


namespace 
{
	struct ImGuiAPI_Visitor
	{
		static void UnsafeCallDestructor(EngineNS::ImGuiAPI* self)
		{
		}
		static inline void* CreateContext( ImFontAtlas* shared_font_atlas)
		{
			return (void*)EngineNS::ImGuiAPI::CreateContext(shared_font_atlas);
		}
		static inline void DestroyContext( void* ctx)
		{
			return (void)EngineNS::ImGuiAPI::DestroyContext(ctx);
		}
		static inline void* GetCurrentContext()
		{
			return (void*)EngineNS::ImGuiAPI::GetCurrentContext();
		}
		static inline void SetCurrentContext( void* ctx)
		{
			return (void)EngineNS::ImGuiAPI::SetCurrentContext(ctx);
		}
		static inline void SetIniFilename( const char* ini_filename)
		{
			return (void)EngineNS::ImGuiAPI::SetIniFilename(ini_filename);
		}
		static inline ImGuiIO* GetIO()
		{
			return (ImGuiIO*)EngineNS::ImGuiAPI::GetIO();
		}
		static inline ImGuiStyle* GetStyle()
		{
			return (ImGuiStyle*)EngineNS::ImGuiAPI::GetStyle();
		}
		static inline void NewFrame()
		{
			return (void)EngineNS::ImGuiAPI::NewFrame();
		}
		static inline void EndFrame()
		{
			return (void)EngineNS::ImGuiAPI::EndFrame();
		}
		static inline void Render()
		{
			return (void)EngineNS::ImGuiAPI::Render();
		}
		static inline ImDrawData* GetDrawData()
		{
			return (ImDrawData*)EngineNS::ImGuiAPI::GetDrawData();
		}
		static inline int DrawData_Textures_Size( ImDrawData* draw_data)
		{
			return (int)EngineNS::ImGuiAPI::DrawData_Textures_Size(draw_data);
		}
		static inline void* DrawData_Textures_Get( ImDrawData* draw_data,int index)
		{
			return (void*)EngineNS::ImGuiAPI::DrawData_Textures_Get(draw_data, index);
		}
		static inline int PlatformIO_Textures_Size( ImGuiPlatformIO* io)
		{
			return (int)EngineNS::ImGuiAPI::PlatformIO_Textures_Size(io);
		}
		static inline void* PlatformIO_Textures_Get( ImGuiPlatformIO* io,int index)
		{
			return (void*)EngineNS::ImGuiAPI::PlatformIO_Textures_Get(io, index);
		}
		static inline int ImTextureData_GetStatus( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetStatus(texture_ptr);
		}
		static inline void ImTextureData_SetStatus( void* texture_ptr,int status)
		{
			return (void)EngineNS::ImGuiAPI::ImTextureData_SetStatus(texture_ptr, status);
		}
		static inline void* ImTextureData_GetBackendUserData( void* texture_ptr)
		{
			return (void*)EngineNS::ImGuiAPI::ImTextureData_GetBackendUserData(texture_ptr);
		}
		static inline void ImTextureData_SetBackendUserData( void* texture_ptr,void* backend_user_data)
		{
			return (void)EngineNS::ImGuiAPI::ImTextureData_SetBackendUserData(texture_ptr, backend_user_data);
		}
		static inline unsigned long long ImTextureData_GetTexID( void* texture_ptr)
		{
			return (unsigned long long)EngineNS::ImGuiAPI::ImTextureData_GetTexID(texture_ptr);
		}
		static inline void ImTextureData_SetTexID( void* texture_ptr,unsigned long long tex_id)
		{
			return (void)EngineNS::ImGuiAPI::ImTextureData_SetTexID(texture_ptr, tex_id);
		}
		static inline int ImTextureData_GetFormat( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetFormat(texture_ptr);
		}
		static inline int ImTextureData_GetWidth( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetWidth(texture_ptr);
		}
		static inline int ImTextureData_GetHeight( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetHeight(texture_ptr);
		}
		static inline int ImTextureData_GetBytesPerPixel( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetBytesPerPixel(texture_ptr);
		}
		static inline int ImTextureData_GetPitch( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetPitch(texture_ptr);
		}
		static inline int ImTextureData_GetSizeInBytes( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetSizeInBytes(texture_ptr);
		}
		static inline void* ImTextureData_GetPixels( void* texture_ptr)
		{
			return (void*)EngineNS::ImGuiAPI::ImTextureData_GetPixels(texture_ptr);
		}
		static inline void* ImTextureData_GetPixelsAt( void* texture_ptr,int x,int y)
		{
			return (void*)EngineNS::ImGuiAPI::ImTextureData_GetPixelsAt(texture_ptr, x, y);
		}
		static inline int ImTextureData_GetUpdatesSize( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetUpdatesSize(texture_ptr);
		}
		static inline int ImTextureData_GetUnusedFrames( void* texture_ptr)
		{
			return (int)EngineNS::ImGuiAPI::ImTextureData_GetUnusedFrames(texture_ptr);
		}
		static inline bool ImTextureData_GetWantDestroyNextFrame( void* texture_ptr)
		{
			return (bool)EngineNS::ImGuiAPI::ImTextureData_GetWantDestroyNextFrame(texture_ptr);
		}
		static inline char* ImTextureData_GetStatusName( int status)
		{
			return (char*)EngineNS::ImGuiAPI::ImTextureData_GetStatusName(status);
		}
		static inline char* ImTextureData_GetFormatName( int format)
		{
			return (char*)EngineNS::ImGuiAPI::ImTextureData_GetFormatName(format);
		}
		static inline void ShowDemoWindow( bool* p_open)
		{
			return (void)EngineNS::ImGuiAPI::ShowDemoWindow(p_open);
		}
		static inline void ShowAboutWindow( bool* p_open)
		{
			return (void)EngineNS::ImGuiAPI::ShowAboutWindow(p_open);
		}
		static inline void ShowMetricsWindow( bool* p_open)
		{
			return (void)EngineNS::ImGuiAPI::ShowMetricsWindow(p_open);
		}
		static inline void ShowStyleEditor( ImGuiStyle* refValue)
		{
			return (void)EngineNS::ImGuiAPI::ShowStyleEditor(refValue);
		}
		static inline bool ShowStyleSelector( const char* label)
		{
			return (bool)EngineNS::ImGuiAPI::ShowStyleSelector(label);
		}
		static inline void ShowFontSelector( const char* label)
		{
			return (void)EngineNS::ImGuiAPI::ShowFontSelector(label);
		}
		static inline void ShowUserGuide()
		{
			return (void)EngineNS::ImGuiAPI::ShowUserGuide();
		}
		static inline char* GetVersion()
		{
			return (char*)EngineNS::ImGuiAPI::GetVersion();
		}
		static inline void StyleColorsDark( ImGuiStyle* dst)
		{
			return (void)EngineNS::ImGuiAPI::StyleColorsDark(dst);
		}
		static inline void StyleColorsClassic( ImGuiStyle* dst)
		{
			return (void)EngineNS::ImGuiAPI::StyleColorsClassic(dst);
		}
		static inline void StyleColorsLight( ImGuiStyle* dst)
		{
			return (void)EngineNS::ImGuiAPI::StyleColorsLight(dst);
		}
		static inline bool Begin( const char* name,bool* p_open,ImGuiWindowFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::Begin(name, p_open, flags);
		}
		static inline void End()
		{
			return (void)EngineNS::ImGuiAPI::End();
		}
		static inline bool BeginChild( const char* str_id,const ImVec2* size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginChild(str_id, size, child_flags, flags);
		}
		static inline bool BeginChild( unsigned int id,const ImVec2* size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginChild(id, size, child_flags, flags);
		}
		static inline void EndChild()
		{
			return (void)EngineNS::ImGuiAPI::EndChild();
		}
		static inline bool IsWindowAppearing()
		{
			return (bool)EngineNS::ImGuiAPI::IsWindowAppearing();
		}
		static inline bool IsWindowCollapsed()
		{
			return (bool)EngineNS::ImGuiAPI::IsWindowCollapsed();
		}
		static inline bool IsWindowFocused( ImGuiFocusedFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::IsWindowFocused(flags);
		}
		static inline bool IsWindowHovered( ImGuiHoveredFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::IsWindowHovered(flags);
		}
		static inline ImDrawList* GetWindowDrawList()
		{
			return (ImDrawList*)EngineNS::ImGuiAPI::GetWindowDrawList();
		}
		static inline float GetWindowDpiScale()
		{
			return (float)EngineNS::ImGuiAPI::GetWindowDpiScale();
		}
		static inline ImGuiViewport* GetWindowViewport()
		{
			return (ImGuiViewport*)EngineNS::ImGuiAPI::GetWindowViewport();
		}
		static inline ImVec2 GetWindowPos()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetWindowPos();
		}
		static inline ImVec2 GetWindowSize()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetWindowSize();
		}
		static inline float GetWindowWidth()
		{
			return (float)EngineNS::ImGuiAPI::GetWindowWidth();
		}
		static inline float GetWindowHeight()
		{
			return (float)EngineNS::ImGuiAPI::GetWindowHeight();
		}
		static inline void SetNextWindowPos( const ImVec2* pos,ImGuiCond_ cond,const ImVec2* pivot)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowPos(pos, cond, pivot);
		}
		static inline void SetNextWindowSize( const ImVec2* size,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowSize(size, cond);
		}
		static inline void SetNextWindowSizeConstraints( const ImVec2* size_min,const ImVec2* size_max,void (*custom_callback)(ImGuiSizeCallbackData *),void* custom_callback_data)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowSizeConstraints(size_min, size_max, custom_callback, custom_callback_data);
		}
		static inline void SetNextWindowContentSize( const ImVec2* size)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowContentSize(size);
		}
		static inline void SetNextWindowCollapsed( bool collapsed,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowCollapsed(collapsed, cond);
		}
		static inline void SetNextWindowFocus()
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowFocus();
		}
		static inline void SetNextWindowBgAlpha( float alpha)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowBgAlpha(alpha);
		}
		static inline void SetNextWindowViewport( unsigned int viewport_id)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowViewport(viewport_id);
		}
		static inline void SetWindowPos( const ImVec2* pos,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetWindowPos(pos, cond);
		}
		static inline void SetWindowSize( const ImVec2* size,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetWindowSize(size, cond);
		}
		static inline void SetWindowCollapsed( bool collapsed,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetWindowCollapsed(collapsed, cond);
		}
		static inline void SetWindowFocus()
		{
			return (void)EngineNS::ImGuiAPI::SetWindowFocus();
		}
		static inline void SetWindowFocus( const char* name)
		{
			return (void)EngineNS::ImGuiAPI::SetWindowFocus(name);
		}
		static inline void SetWindowFontScale( float scale)
		{
			return (void)EngineNS::ImGuiAPI::SetWindowFontScale(scale);
		}
		static inline void SetWindowPos( const char* name,const ImVec2* pos,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetWindowPos(name, pos, cond);
		}
		static inline void SetWindowSize( const char* name,const ImVec2* size,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetWindowSize(name, size, cond);
		}
		static inline void SetWindowCollapsed( const char* name,bool collapsed,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetWindowCollapsed(name, collapsed, cond);
		}
		static inline ImVec2 GetContentRegionMax()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetContentRegionMax();
		}
		static inline ImVec2 GetContentRegionAvail()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetContentRegionAvail();
		}
		static inline ImVec2 GetWindowContentRegionMin()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetWindowContentRegionMin();
		}
		static inline ImVec2 GetWindowContentRegionMax()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetWindowContentRegionMax();
		}
		static inline float GetWindowContentRegionWidth()
		{
			return (float)EngineNS::ImGuiAPI::GetWindowContentRegionWidth();
		}
		static inline float GetScrollX()
		{
			return (float)EngineNS::ImGuiAPI::GetScrollX();
		}
		static inline float GetScrollY()
		{
			return (float)EngineNS::ImGuiAPI::GetScrollY();
		}
		static inline float GetScrollMaxX()
		{
			return (float)EngineNS::ImGuiAPI::GetScrollMaxX();
		}
		static inline float GetScrollMaxY()
		{
			return (float)EngineNS::ImGuiAPI::GetScrollMaxY();
		}
		static inline void SetScrollX( float scroll_x)
		{
			return (void)EngineNS::ImGuiAPI::SetScrollX(scroll_x);
		}
		static inline void SetScrollY( float scroll_y)
		{
			return (void)EngineNS::ImGuiAPI::SetScrollY(scroll_y);
		}
		static inline void SetScrollHereX( float center_x_ratio)
		{
			return (void)EngineNS::ImGuiAPI::SetScrollHereX(center_x_ratio);
		}
		static inline void SetScrollHereY( float center_y_ratio)
		{
			return (void)EngineNS::ImGuiAPI::SetScrollHereY(center_y_ratio);
		}
		static inline void SetScrollFromPosX( float local_x,float center_x_ratio)
		{
			return (void)EngineNS::ImGuiAPI::SetScrollFromPosX(local_x, center_x_ratio);
		}
		static inline void SetScrollFromPosY( float local_y,float center_y_ratio)
		{
			return (void)EngineNS::ImGuiAPI::SetScrollFromPosY(local_y, center_y_ratio);
		}
		static inline void PushFont( ImFont* font)
		{
			return (void)EngineNS::ImGuiAPI::PushFont(font);
		}
		static inline void PushFontWithSize( ImFont* font,float font_size_base_unscaled)
		{
			return (void)EngineNS::ImGuiAPI::PushFontWithSize(font, font_size_base_unscaled);
		}
		static inline void PopFont()
		{
			return (void)EngineNS::ImGuiAPI::PopFont();
		}
		static inline void PushStyleColor( ImGuiCol_ idx,unsigned int col)
		{
			return (void)EngineNS::ImGuiAPI::PushStyleColor(idx, col);
		}
		static inline void PushStyleColor( ImGuiCol_ idx,const ImVec4* col)
		{
			return (void)EngineNS::ImGuiAPI::PushStyleColor(idx, col);
		}
		static inline void PopStyleColor( int count)
		{
			return (void)EngineNS::ImGuiAPI::PopStyleColor(count);
		}
		static inline void PushStyleVar( ImGuiStyleVar_ idx,float val)
		{
			return (void)EngineNS::ImGuiAPI::PushStyleVar(idx, val);
		}
		static inline void PushStyleVar( ImGuiStyleVar_ idx,const ImVec2* val)
		{
			return (void)EngineNS::ImGuiAPI::PushStyleVar(idx, val);
		}
		static inline void PopStyleVar( int count)
		{
			return (void)EngineNS::ImGuiAPI::PopStyleVar(count);
		}
		static inline ImVec4* GetStyleColorVec4( ImGuiCol_ idx)
		{
			return (ImVec4*)EngineNS::ImGuiAPI::GetStyleColorVec4(idx);
		}
		static inline ImFont* GetFont()
		{
			return (ImFont*)EngineNS::ImGuiAPI::GetFont();
		}
		static inline float GetFontSize()
		{
			return (float)EngineNS::ImGuiAPI::GetFontSize();
		}
		static inline ImVec2 GetFontTexUvWhitePixel()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetFontTexUvWhitePixel();
		}
		static inline unsigned int GetColorU32( ImGuiCol_ idx,float alpha_mul)
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetColorU32(idx, alpha_mul);
		}
		static inline unsigned int GetColorU32( const ImVec4* col)
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetColorU32(col);
		}
		static inline unsigned int GetColorU32( unsigned int col)
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetColorU32(col);
		}
		static inline void PushItemWidth( float item_width)
		{
			return (void)EngineNS::ImGuiAPI::PushItemWidth(item_width);
		}
		static inline void PopItemWidth()
		{
			return (void)EngineNS::ImGuiAPI::PopItemWidth();
		}
		static inline void SetNextItemWidth( float item_width)
		{
			return (void)EngineNS::ImGuiAPI::SetNextItemWidth(item_width);
		}
		static inline float CalcItemWidth()
		{
			return (float)EngineNS::ImGuiAPI::CalcItemWidth();
		}
		static inline void PushTextWrapPos( float wrap_local_pos_x)
		{
			return (void)EngineNS::ImGuiAPI::PushTextWrapPos(wrap_local_pos_x);
		}
		static inline void PopTextWrapPos()
		{
			return (void)EngineNS::ImGuiAPI::PopTextWrapPos();
		}
		static inline void PushAllowKeyboardFocus( bool allow_keyboard_focus)
		{
			return (void)EngineNS::ImGuiAPI::PushAllowKeyboardFocus(allow_keyboard_focus);
		}
		static inline void PopAllowKeyboardFocus()
		{
			return (void)EngineNS::ImGuiAPI::PopAllowKeyboardFocus();
		}
		static inline void PushButtonRepeat( bool repeat)
		{
			return (void)EngineNS::ImGuiAPI::PushButtonRepeat(repeat);
		}
		static inline void PopButtonRepeat()
		{
			return (void)EngineNS::ImGuiAPI::PopButtonRepeat();
		}
		static inline void Separator()
		{
			return (void)EngineNS::ImGuiAPI::Separator();
		}
		static inline void SameLine( float offset_from_start_x,float spacing)
		{
			return (void)EngineNS::ImGuiAPI::SameLine(offset_from_start_x, spacing);
		}
		static inline void NewLine()
		{
			return (void)EngineNS::ImGuiAPI::NewLine();
		}
		static inline void Spacing()
		{
			return (void)EngineNS::ImGuiAPI::Spacing();
		}
		static inline void Dummy( const ImVec2* size)
		{
			return (void)EngineNS::ImGuiAPI::Dummy(size);
		}
		static inline void Indent( float indent_w)
		{
			return (void)EngineNS::ImGuiAPI::Indent(indent_w);
		}
		static inline void Unindent( float indent_w)
		{
			return (void)EngineNS::ImGuiAPI::Unindent(indent_w);
		}
		static inline void BeginGroup()
		{
			return (void)EngineNS::ImGuiAPI::BeginGroup();
		}
		static inline void EndGroup()
		{
			return (void)EngineNS::ImGuiAPI::EndGroup();
		}
		static inline ImVec2 GetCursorPos()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetCursorPos();
		}
		static inline float GetCursorPosX()
		{
			return (float)EngineNS::ImGuiAPI::GetCursorPosX();
		}
		static inline float GetCursorPosY()
		{
			return (float)EngineNS::ImGuiAPI::GetCursorPosY();
		}
		static inline void SetCursorPos( const ImVec2* local_pos)
		{
			return (void)EngineNS::ImGuiAPI::SetCursorPos(local_pos);
		}
		static inline void SetCursorPosX( float local_x)
		{
			return (void)EngineNS::ImGuiAPI::SetCursorPosX(local_x);
		}
		static inline void SetCursorPosY( float local_y)
		{
			return (void)EngineNS::ImGuiAPI::SetCursorPosY(local_y);
		}
		static inline ImVec2 GetCursorStartPos()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetCursorStartPos();
		}
		static inline ImVec2 GetCursorScreenPos()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetCursorScreenPos();
		}
		static inline void SetCursorScreenPos( const ImVec2* pos)
		{
			return (void)EngineNS::ImGuiAPI::SetCursorScreenPos(pos);
		}
		static inline void AlignTextToFramePadding()
		{
			return (void)EngineNS::ImGuiAPI::AlignTextToFramePadding();
		}
		static inline float GetTextLineHeight()
		{
			return (float)EngineNS::ImGuiAPI::GetTextLineHeight();
		}
		static inline float GetTextLineHeightWithSpacing()
		{
			return (float)EngineNS::ImGuiAPI::GetTextLineHeightWithSpacing();
		}
		static inline float GetFrameHeight()
		{
			return (float)EngineNS::ImGuiAPI::GetFrameHeight();
		}
		static inline float GetFrameHeightWithSpacing()
		{
			return (float)EngineNS::ImGuiAPI::GetFrameHeightWithSpacing();
		}
		static inline void PushID( const char* str_id)
		{
			return (void)EngineNS::ImGuiAPI::PushID(str_id);
		}
		static inline void PushID( const char* str_id_begin,const char* str_id_end)
		{
			return (void)EngineNS::ImGuiAPI::PushID(str_id_begin, str_id_end);
		}
		static inline void PushID( const void* ptr_id)
		{
			return (void)EngineNS::ImGuiAPI::PushID(ptr_id);
		}
		static inline void PushID( int int_id)
		{
			return (void)EngineNS::ImGuiAPI::PushID(int_id);
		}
		static inline void PopID()
		{
			return (void)EngineNS::ImGuiAPI::PopID();
		}
		static inline unsigned int GetID( const char* str_id)
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetID(str_id);
		}
		static inline unsigned int GetID( const char* str_id_begin,const char* str_id_end)
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetID(str_id_begin, str_id_end);
		}
		static inline unsigned int GetID( const void* ptr_id)
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetID(ptr_id);
		}
		static inline void TextUnformatted( const char* text)
		{
			return (void)EngineNS::ImGuiAPI::TextUnformatted(text);
		}
		static inline void TextAsPointer( const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::TextAsPointer(fmt);
		}
		static inline void Text( const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::Text(fmt);
		}
		static inline void TextColored( const ImVec4* col,const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::TextColored(col, fmt);
		}
		static inline void TextDisabled( const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::TextDisabled(fmt);
		}
		static inline void TextWrapped( const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::TextWrapped(fmt);
		}
		static inline void LabelText( const char* label,const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::LabelText(label, fmt);
		}
		static inline void BulletText( const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::BulletText(fmt);
		}
		static inline bool Button( const char* label,const ImVec2* size)
		{
			return (bool)EngineNS::ImGuiAPI::Button(label, size);
		}
		static inline bool SmallButton( const char* label)
		{
			return (bool)EngineNS::ImGuiAPI::SmallButton(label);
		}
		static inline bool InvisibleButton( const char* str_id,const ImVec2* size,ImGuiButtonFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InvisibleButton(str_id, size, flags);
		}
		static inline bool ArrowButton( const char* str_id,ImGuiDir dir)
		{
			return (bool)EngineNS::ImGuiAPI::ArrowButton(str_id, dir);
		}
		static inline void Arrow( ImDrawList* draw_list,const ImVec2* pos,unsigned int col,ImGuiDir dir,float scale)
		{
			return (void)EngineNS::ImGuiAPI::Arrow(draw_list, pos, col, dir, scale);
		}
		static inline void Image( unsigned long long user_texture_id,const ImVec2* size,const ImVec2* uv0,const ImVec2* uv1,const ImVec4* tint_col,const ImVec4* border_col)
		{
			return (void)EngineNS::ImGuiAPI::Image(user_texture_id, size, uv0, uv1, tint_col, border_col);
		}
		static inline bool ImageButton( const char* name,unsigned long long user_texture_id,const ImVec2* size,const ImVec2* uv0,const ImVec2* uv1,const ImVec4* bg_col,const ImVec4* tint_col)
		{
			return (bool)EngineNS::ImGuiAPI::ImageButton(name, user_texture_id, size, uv0, uv1, bg_col, tint_col);
		}
		static inline bool Checkbox( const char* label,bool* v)
		{
			return (bool)EngineNS::ImGuiAPI::Checkbox(label, v);
		}
		static inline bool CheckboxFlags( const char* label,int* flags,int flags_value)
		{
			return (bool)EngineNS::ImGuiAPI::CheckboxFlags(label, flags, flags_value);
		}
		static inline bool CheckboxFlags( const char* label,unsigned int* flags,unsigned int flags_value)
		{
			return (bool)EngineNS::ImGuiAPI::CheckboxFlags(label, flags, flags_value);
		}
		static inline bool RadioButton( const char* label,bool active)
		{
			return (bool)EngineNS::ImGuiAPI::RadioButton(label, active);
		}
		static inline bool RadioButton( const char* label,int* v,int v_button)
		{
			return (bool)EngineNS::ImGuiAPI::RadioButton(label, v, v_button);
		}
		static inline void ProgressBar( float fraction,const ImVec2* size_arg,const char* overlay)
		{
			return (void)EngineNS::ImGuiAPI::ProgressBar(fraction, size_arg, overlay);
		}
		static inline void Bullet()
		{
			return (void)EngineNS::ImGuiAPI::Bullet();
		}
		static inline bool BeginCombo( const char* label,const char* preview_value,ImGuiComboFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginCombo(label, preview_value, flags);
		}
		static inline bool BeginCombo( const char* label,const char* preview_value,ImGuiComboFlags_ flags,ImGuiWindowFlags_ winFlags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginCombo(label, preview_value, flags, winFlags);
		}
		static inline void EndCombo()
		{
			return (void)EngineNS::ImGuiAPI::EndCombo();
		}
		static inline bool Combo( const char* label,int* current_item,const char** items,int items_count,int popup_max_height_in_items)
		{
			return (bool)EngineNS::ImGuiAPI::Combo(label, current_item, items, items_count, popup_max_height_in_items);
		}
		static inline bool Combo( const char* label,int* current_item,const char* items_separated_by_zeros,int popup_max_height_in_items)
		{
			return (bool)EngineNS::ImGuiAPI::Combo(label, current_item, items_separated_by_zeros, popup_max_height_in_items);
		}
		static inline bool Combo( const char* label,int* current_item,const char *(*fn_getter)(void *, int),void* data,int items_count,int popup_max_height_in_items)
		{
			return (bool)EngineNS::ImGuiAPI::Combo(label, current_item, fn_getter, data, items_count, popup_max_height_in_items);
		}
		static inline bool DragFloat( const char* label,float* v,float v_speed,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragFloat(label, v, v_speed, v_min, v_max, format, flags);
		}
		static inline bool DragFloat2( const char* label,float* v,float v_speed,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragFloat2(label, v, v_speed, v_min, v_max, format, flags);
		}
		static inline bool DragFloat3( const char* label,float* v,float v_speed,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragFloat3(label, v, v_speed, v_min, v_max, format, flags);
		}
		static inline bool DragFloat4( const char* label,float* v,float v_speed,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragFloat4(label, v, v_speed, v_min, v_max, format, flags);
		}
		static inline bool DragFloatRange2( const char* label,float* v_current_min,float* v_current_max,float v_speed,float v_min,float v_max,const char* format,const char* format_max,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragFloatRange2(label, v_current_min, v_current_max, v_speed, v_min, v_max, format, format_max, flags);
		}
		static inline bool DragInt( const char* label,int* v,float v_speed,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragInt(label, v, v_speed, v_min, v_max, format, flags);
		}
		static inline bool DragInt2( const char* label,int* v,float v_speed,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragInt2(label, v, v_speed, v_min, v_max, format, flags);
		}
		static inline bool DragInt3( const char* label,int* v,float v_speed,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragInt3(label, v, v_speed, v_min, v_max, format, flags);
		}
		static inline bool DragInt4( const char* label,int* v,float v_speed,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragInt4(label, v, v_speed, v_min, v_max, format, flags);
		}
		static inline bool DragIntRange2( const char* label,int* v_current_min,int* v_current_max,float v_speed,int v_min,int v_max,const char* format,const char* format_max,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragIntRange2(label, v_current_min, v_current_max, v_speed, v_min, v_max, format, format_max, flags);
		}
		static inline bool DragScalar( const char* label,ImGuiDataType_ data_type,void* p_data,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragScalar(label, data_type, p_data, v_speed, p_min, p_max, format, flags);
		}
		static inline bool DragScalarN( const char* label,ImGuiDataType_ data_type,void* p_data,int components,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragScalarN(label, data_type, p_data, components, v_speed, p_min, p_max, format, flags);
		}
		static inline bool SliderFloat( const char* label,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderFloat(label, v, v_min, v_max, format, flags);
		}
		static inline bool SliderFloat2( const char* label,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderFloat2(label, v, v_min, v_max, format, flags);
		}
		static inline bool SliderFloat3( const char* label,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderFloat3(label, v, v_min, v_max, format, flags);
		}
		static inline bool SliderFloat4( const char* label,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderFloat4(label, v, v_min, v_max, format, flags);
		}
		static inline bool SliderAngle( const char* label,float* v_rad,float v_degrees_min,float v_degrees_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderAngle(label, v_rad, v_degrees_min, v_degrees_max, format, flags);
		}
		static inline bool SliderInt( const char* label,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderInt(label, v, v_min, v_max, format, flags);
		}
		static inline bool SliderInt2( const char* label,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderInt2(label, v, v_min, v_max, format, flags);
		}
		static inline bool SliderInt3( const char* label,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderInt3(label, v, v_min, v_max, format, flags);
		}
		static inline bool SliderInt4( const char* label,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderInt4(label, v, v_min, v_max, format, flags);
		}
		static inline bool SliderScalar( const char* label,ImGuiDataType_ data_type,void* p_data,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderScalar(label, data_type, p_data, p_min, p_max, format, flags);
		}
		static inline bool SliderScalarN( const char* label,ImGuiDataType_ data_type,void* p_data,int components,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::SliderScalarN(label, data_type, p_data, components, p_min, p_max, format, flags);
		}
		static inline bool VSliderFloat( const char* label,const ImVec2* size,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::VSliderFloat(label, size, v, v_min, v_max, format, flags);
		}
		static inline bool VSliderInt( const char* label,const ImVec2* size,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::VSliderInt(label, size, v, v_min, v_max, format, flags);
		}
		static inline bool VSliderScalar( const char* label,const ImVec2* size,ImGuiDataType_ data_type,void* p_data,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::VSliderScalar(label, size, data_type, p_data, p_min, p_max, format, flags);
		}
		static inline bool InputText( const char* label,void* buf,unsigned int buf_size,ImGuiInputTextFlags_ flags,int (*callback)(ImGuiInputTextCallbackData *),void* user_data)
		{
			return (bool)EngineNS::ImGuiAPI::InputText(label, buf, buf_size, flags, callback, user_data);
		}
		static inline bool InputTextNoName( const char* label,void* buf,unsigned int buf_size,ImGuiInputTextFlags_ flags,int (*callback)(ImGuiInputTextCallbackData *),void* user_data)
		{
			return (bool)EngineNS::ImGuiAPI::InputTextNoName(label, buf, buf_size, flags, callback, user_data);
		}
		static inline bool InputTextMultiline( const char* label,char* buf,unsigned int buf_size,const ImVec2* size,ImGuiInputTextFlags_ flags,int (*callback)(ImGuiInputTextCallbackData *),void* user_data)
		{
			return (bool)EngineNS::ImGuiAPI::InputTextMultiline(label, buf, buf_size, size, flags, callback, user_data);
		}
		static inline bool InputTextWithHint( const char* label,const char* hint,char* buf,unsigned int buf_size,ImGuiInputTextFlags_ flags,int (*callback)(ImGuiInputTextCallbackData *),void* user_data)
		{
			return (bool)EngineNS::ImGuiAPI::InputTextWithHint(label, hint, buf, buf_size, flags, callback, user_data);
		}
		static inline bool InputFloat( const char* label,float* v,float step,float step_fast,const char* format,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputFloat(label, v, step, step_fast, format, flags);
		}
		static inline bool InputFloat2( const char* label,float* v,const char* format,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputFloat2(label, v, format, flags);
		}
		static inline bool InputFloat3( const char* label,float* v,const char* format,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputFloat3(label, v, format, flags);
		}
		static inline bool InputFloat4( const char* label,float* v,const char* format,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputFloat4(label, v, format, flags);
		}
		static inline bool InputInt( const char* label,int* v,int step,int step_fast,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputInt(label, v, step, step_fast, flags);
		}
		static inline bool InputInt2( const char* label,int* v,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputInt2(label, v, flags);
		}
		static inline bool InputInt3( const char* label,int* v,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputInt3(label, v, flags);
		}
		static inline bool InputInt4( const char* label,int* v,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputInt4(label, v, flags);
		}
		static inline bool InputDouble( const char* label,double* v,double step,double step_fast,const char* format,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputDouble(label, v, step, step_fast, format, flags);
		}
		static inline bool InputScalar( const char* label,ImGuiDataType_ data_type,void* p_data,const void* p_step,const void* p_step_fast,const char* format,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputScalar(label, data_type, p_data, p_step, p_step_fast, format, flags);
		}
		static inline bool InputScalarN( const char* label,ImGuiDataType_ data_type,void* p_data,int components,const void* p_step,const void* p_step_fast,const char* format,ImGuiInputTextFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::InputScalarN(label, data_type, p_data, components, p_step, p_step_fast, format, flags);
		}
		static inline bool ColorEdit3( const char* label,float* col,ImGuiColorEditFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::ColorEdit3(label, col, flags);
		}
		static inline bool ColorEdit4( const char* label,float* col,ImGuiColorEditFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::ColorEdit4(label, col, flags);
		}
		static inline bool ColorPicker3( const char* label,float* col,ImGuiColorEditFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::ColorPicker3(label, col, flags);
		}
		static inline bool ColorPicker4( const char* label,float* col,ImGuiColorEditFlags_ flags,const float* _col2)
		{
			return (bool)EngineNS::ImGuiAPI::ColorPicker4(label, col, flags, _col2);
		}
		static inline bool ColorButton( const char* desc_id,const ImVec4* col,ImGuiColorEditFlags_ flags,ImVec2* size)
		{
			return (bool)EngineNS::ImGuiAPI::ColorButton(desc_id, col, flags, size);
		}
		static inline void SetColorEditOptions( ImGuiColorEditFlags_ flags)
		{
			return (void)EngineNS::ImGuiAPI::SetColorEditOptions(flags);
		}
		static inline bool TreeNode( const char* label)
		{
			return (bool)EngineNS::ImGuiAPI::TreeNode(label);
		}
		static inline bool TreeNode( const char* str_id,const char* fmt)
		{
			return (bool)EngineNS::ImGuiAPI::TreeNode(str_id, fmt);
		}
		static inline bool TreeNode( const void* ptr_id,const char* fmt)
		{
			return (bool)EngineNS::ImGuiAPI::TreeNode(ptr_id, fmt);
		}
		static inline bool TreeNodeEx( const char* label,ImGuiTreeNodeFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::TreeNodeEx(label, flags);
		}
		static inline bool TreeNodeEx( const char* str_id,ImGuiTreeNodeFlags_ flags,const char* fmt)
		{
			return (bool)EngineNS::ImGuiAPI::TreeNodeEx(str_id, flags, fmt);
		}
		static inline bool TreeNodeEx( const void* ptr_id,ImGuiTreeNodeFlags_ flags,const char* fmt)
		{
			return (bool)EngineNS::ImGuiAPI::TreeNodeEx(ptr_id, flags, fmt);
		}
		static inline void TreePush( const char* str_id)
		{
			return (void)EngineNS::ImGuiAPI::TreePush(str_id);
		}
		static inline void TreePush( const void* ptr_id)
		{
			return (void)EngineNS::ImGuiAPI::TreePush(ptr_id);
		}
		static inline void TreePop()
		{
			return (void)EngineNS::ImGuiAPI::TreePop();
		}
		static inline float GetTreeNodeToLabelSpacing()
		{
			return (float)EngineNS::ImGuiAPI::GetTreeNodeToLabelSpacing();
		}
		static inline bool CollapsingHeader( const char* label,ImGuiTreeNodeFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::CollapsingHeader(label, flags);
		}
		static inline bool CollapsingHeader( const char* label,bool* p_open,ImGuiTreeNodeFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::CollapsingHeader(label, p_open, flags);
		}
		static inline void SetNextItemOpen( bool is_open,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetNextItemOpen(is_open, cond);
		}
		static inline bool Selectable( const char* label,bool selected,ImGuiSelectableFlags_ flags,const ImVec2* size)
		{
			return (bool)EngineNS::ImGuiAPI::Selectable(label, selected, flags, size);
		}
		static inline bool Selectable( const char* label,bool* p_selected,ImGuiSelectableFlags_ flags,const ImVec2* size)
		{
			return (bool)EngineNS::ImGuiAPI::Selectable(label, p_selected, flags, size);
		}
		static inline bool ListBox( const char* label,int* current_item,const char** items,int items_count,int height_in_items)
		{
			return (bool)EngineNS::ImGuiAPI::ListBox(label, current_item, items, items_count, height_in_items);
		}
		static inline bool ListBox( const char* label,int* current_item,const char *(*fn_getter)(void *, int),void* data,int items_count,int height_in_items)
		{
			return (bool)EngineNS::ImGuiAPI::ListBox(label, current_item, fn_getter, data, items_count, height_in_items);
		}
		static inline void PlotLines( const char* label,const float* values,int values_count,int values_offset,const char* overlay_text,float scale_min,float scale_max,ImVec2 graph_size,int stride)
		{
			return (void)EngineNS::ImGuiAPI::PlotLines(label, values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
		}
		static inline void PlotLines( const char* label,float (*fn_getter)(void *, int),void* data,int values_count,int values_offset,const char* overlay_text,float scale_min,float scale_max,ImVec2 graph_size)
		{
			return (void)EngineNS::ImGuiAPI::PlotLines(label, fn_getter, data, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size);
		}
		static inline void PlotHistogram( const char* label,const float* values,int values_count,int values_offset,const char* overlay_text,float scale_min,float scale_max,ImVec2 graph_size,int stride)
		{
			return (void)EngineNS::ImGuiAPI::PlotHistogram(label, values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
		}
		static inline void PlotHistogram( const char* label,float (*fn_getter)(void *, int),void* data,int values_count,int values_offset,const char* overlay_text,float scale_min,float scale_max,ImVec2 graph_size)
		{
			return (void)EngineNS::ImGuiAPI::PlotHistogram(label, fn_getter, data, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size);
		}
		static inline void Value( const char* prefix,bool b)
		{
			return (void)EngineNS::ImGuiAPI::Value(prefix, b);
		}
		static inline void Value( const char* prefix,int v)
		{
			return (void)EngineNS::ImGuiAPI::Value(prefix, v);
		}
		static inline void Value( const char* prefix,unsigned int v)
		{
			return (void)EngineNS::ImGuiAPI::Value(prefix, v);
		}
		static inline void Value( const char* prefix,float v,const char* float_format)
		{
			return (void)EngineNS::ImGuiAPI::Value(prefix, v, float_format);
		}
		static inline bool BeginMenuBar()
		{
			return (bool)EngineNS::ImGuiAPI::BeginMenuBar();
		}
		static inline void EndMenuBar()
		{
			return (void)EngineNS::ImGuiAPI::EndMenuBar();
		}
		static inline bool BeginMainMenuBar()
		{
			return (bool)EngineNS::ImGuiAPI::BeginMainMenuBar();
		}
		static inline void EndMainMenuBar()
		{
			return (void)EngineNS::ImGuiAPI::EndMainMenuBar();
		}
		static inline bool BeginMenu( const char* label,bool enabled)
		{
			return (bool)EngineNS::ImGuiAPI::BeginMenu(label, enabled);
		}
		static inline void EndMenu()
		{
			return (void)EngineNS::ImGuiAPI::EndMenu();
		}
		static inline bool MenuItem( const char* label,const char* shortcut,bool selected,bool enabled)
		{
			return (bool)EngineNS::ImGuiAPI::MenuItem(label, shortcut, selected, enabled);
		}
		static inline bool MenuItem( const char* label,const char* shortcut,bool* p_selected,bool enabled)
		{
			return (bool)EngineNS::ImGuiAPI::MenuItem(label, shortcut, p_selected, enabled);
		}
		static inline bool BeginTooltip()
		{
			return (bool)EngineNS::ImGuiAPI::BeginTooltip();
		}
		static inline void EndTooltip()
		{
			return (void)EngineNS::ImGuiAPI::EndTooltip();
		}
		static inline void SetTooltip( const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::SetTooltip(fmt);
		}
		static inline bool BeginPopup( const char* str_id,ImGuiWindowFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginPopup(str_id, flags);
		}
		static inline bool BeginPopupModal( const char* name,bool* p_open,ImGuiWindowFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginPopupModal(name, p_open, flags);
		}
		static inline void EndPopup()
		{
			return (void)EngineNS::ImGuiAPI::EndPopup();
		}
		static inline void OpenPopup( const char* str_id,ImGuiPopupFlags_ popup_flags)
		{
			return (void)EngineNS::ImGuiAPI::OpenPopup(str_id, popup_flags);
		}
		static inline void OpenPopupOnItemClick( const char* str_id,ImGuiPopupFlags_ popup_flags)
		{
			return (void)EngineNS::ImGuiAPI::OpenPopupOnItemClick(str_id, popup_flags);
		}
		static inline void CloseCurrentPopup()
		{
			return (void)EngineNS::ImGuiAPI::CloseCurrentPopup();
		}
		static inline bool BeginPopupContextItem( const char* str_id,ImGuiPopupFlags_ popup_flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginPopupContextItem(str_id, popup_flags);
		}
		static inline bool BeginPopupContextWindow( const char* str_id,ImGuiPopupFlags_ popup_flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginPopupContextWindow(str_id, popup_flags);
		}
		static inline bool BeginPopupContextVoid( const char* str_id,ImGuiPopupFlags_ popup_flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginPopupContextVoid(str_id, popup_flags);
		}
		static inline bool IsPopupOpen( const char* str_id,ImGuiPopupFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::IsPopupOpen(str_id, flags);
		}
		static inline void Columns( int count,const char* id,bool border)
		{
			return (void)EngineNS::ImGuiAPI::Columns(count, id, border);
		}
		static inline void NextColumn()
		{
			return (void)EngineNS::ImGuiAPI::NextColumn();
		}
		static inline int GetColumnIndex()
		{
			return (int)EngineNS::ImGuiAPI::GetColumnIndex();
		}
		static inline float GetColumnWidth( int column_index)
		{
			return (float)EngineNS::ImGuiAPI::GetColumnWidth(column_index);
		}
		static inline void SetColumnWidth( int column_index,float width)
		{
			return (void)EngineNS::ImGuiAPI::SetColumnWidth(column_index, width);
		}
		static inline float GetColumnOffset( int column_index)
		{
			return (float)EngineNS::ImGuiAPI::GetColumnOffset(column_index);
		}
		static inline void SetColumnOffset( int column_index,float offset_x)
		{
			return (void)EngineNS::ImGuiAPI::SetColumnOffset(column_index, offset_x);
		}
		static inline int GetColumnsCount()
		{
			return (int)EngineNS::ImGuiAPI::GetColumnsCount();
		}
		static inline bool BeginTable( const char* str_id,int column,int flags,const ImVec2* outer_size,float inner_width)
		{
			return (bool)EngineNS::ImGuiAPI::BeginTable(str_id, column, flags, *outer_size, inner_width);
		}
		static inline void EndTable()
		{
			return (void)EngineNS::ImGuiAPI::EndTable();
		}
		static inline void TableNextRow( int row_flags,float min_row_height)
		{
			return (void)EngineNS::ImGuiAPI::TableNextRow(row_flags, min_row_height);
		}
		static inline bool TableNextColumn()
		{
			return (bool)EngineNS::ImGuiAPI::TableNextColumn();
		}
		static inline bool TableSetColumnIndex( int column_n)
		{
			return (bool)EngineNS::ImGuiAPI::TableSetColumnIndex(column_n);
		}
		static inline void TableSetupColumn( const char* label,int flags,float init_width_or_weight,unsigned int user_id)
		{
			return (void)EngineNS::ImGuiAPI::TableSetupColumn(label, flags, init_width_or_weight, user_id);
		}
		static inline void TableSetupScrollFreeze( int cols,int rows)
		{
			return (void)EngineNS::ImGuiAPI::TableSetupScrollFreeze(cols, rows);
		}
		static inline void TableHeadersRow()
		{
			return (void)EngineNS::ImGuiAPI::TableHeadersRow();
		}
		static inline void TableHeader( const char* label)
		{
			return (void)EngineNS::ImGuiAPI::TableHeader(label);
		}
		static inline int TableGetColumnCount()
		{
			return (int)EngineNS::ImGuiAPI::TableGetColumnCount();
		}
		static inline int TableGetColumnIndex()
		{
			return (int)EngineNS::ImGuiAPI::TableGetColumnIndex();
		}
		static inline int TableGetRowIndex()
		{
			return (int)EngineNS::ImGuiAPI::TableGetRowIndex();
		}
		static inline char* TableGetColumnName( int column_n)
		{
			return (char*)EngineNS::ImGuiAPI::TableGetColumnName(column_n);
		}
		static inline int TableGetColumnFlags( int column_n)
		{
			return (int)EngineNS::ImGuiAPI::TableGetColumnFlags(column_n);
		}
		static inline void TableSetColumnEnabled( int column_n,bool v)
		{
			return (void)EngineNS::ImGuiAPI::TableSetColumnEnabled(column_n, v);
		}
		static inline void TableSetBgColor( int target,unsigned int color,int column_n)
		{
			return (void)EngineNS::ImGuiAPI::TableSetBgColor(target, color, column_n);
		}
		static inline bool BeginTabBar( const char* str_id,ImGuiTabBarFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginTabBar(str_id, flags);
		}
		static inline void EndTabBar()
		{
			return (void)EngineNS::ImGuiAPI::EndTabBar();
		}
		static inline bool BeginTabItem( const char* label,bool* p_open,ImGuiTabItemFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginTabItem(label, p_open, flags);
		}
		static inline void EndTabItem()
		{
			return (void)EngineNS::ImGuiAPI::EndTabItem();
		}
		static inline bool TabItemButton( const char* label,ImGuiTabItemFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::TabItemButton(label, flags);
		}
		static inline void SetTabItemClosed( const char* tab_or_docked_window_label)
		{
			return (void)EngineNS::ImGuiAPI::SetTabItemClosed(tab_or_docked_window_label);
		}
		static inline unsigned int DockSpace( unsigned int id,const ImVec2* size,int flags,const ImGuiWindowClass* window_class)
		{
			return (unsigned int)EngineNS::ImGuiAPI::DockSpace(id, *size, flags, window_class);
		}
		static inline unsigned int DockSpaceOverViewport( unsigned int dock_id,ImGuiViewport* viewport,int flags,const ImGuiWindowClass* window_class)
		{
			return (unsigned int)EngineNS::ImGuiAPI::DockSpaceOverViewport(dock_id, viewport, flags, window_class);
		}
		static inline void SetNextWindowDockID( unsigned int dock_id,ImGuiCond_ cond)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowDockID(dock_id, cond);
		}
		static inline void SetNextWindowClass( const ImGuiWindowClass* window_class)
		{
			return (void)EngineNS::ImGuiAPI::SetNextWindowClass(window_class);
		}
		static inline unsigned int GetWindowDockID()
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetWindowDockID();
		}
		static inline bool IsWindowDocked()
		{
			return (bool)EngineNS::ImGuiAPI::IsWindowDocked();
		}
		static inline void DockBuilderDockWindow( const char* window_name,unsigned int node_id)
		{
			return (void)EngineNS::ImGuiAPI::DockBuilderDockWindow(window_name, node_id);
		}
		static inline unsigned int DockBuilderAddNode( unsigned int node_id,int flags)
		{
			return (unsigned int)EngineNS::ImGuiAPI::DockBuilderAddNode(node_id, flags);
		}
		static inline void DockBuilderRemoveNode( unsigned int node_id)
		{
			return (void)EngineNS::ImGuiAPI::DockBuilderRemoveNode(node_id);
		}
		static inline void DockBuilderRemoveNodeDockedWindows( unsigned int node_id,bool clear_settings_refs)
		{
			return (void)EngineNS::ImGuiAPI::DockBuilderRemoveNodeDockedWindows(node_id, clear_settings_refs);
		}
		static inline void DockBuilderRemoveNodeChildNodes( unsigned int node_id)
		{
			return (void)EngineNS::ImGuiAPI::DockBuilderRemoveNodeChildNodes(node_id);
		}
		static inline void DockBuilderSetNodePos( unsigned int node_id,ImVec2 pos)
		{
			return (void)EngineNS::ImGuiAPI::DockBuilderSetNodePos(node_id, pos);
		}
		static inline void DockBuilderSetNodeSize( unsigned int node_id,ImVec2 size)
		{
			return (void)EngineNS::ImGuiAPI::DockBuilderSetNodeSize(node_id, size);
		}
		static inline unsigned int DockBuilderSplitNode( unsigned int node_id,ImGuiDir split_dir,float size_ratio_for_node_at_dir,unsigned int* out_id_at_dir,unsigned int* out_id_at_opposite_dir)
		{
			return (unsigned int)EngineNS::ImGuiAPI::DockBuilderSplitNode(node_id, split_dir, size_ratio_for_node_at_dir, out_id_at_dir, out_id_at_opposite_dir);
		}
		static inline void DockBuilderCopyWindowSettings( const char* src_name,const char* dst_name)
		{
			return (void)EngineNS::ImGuiAPI::DockBuilderCopyWindowSettings(src_name, dst_name);
		}
		static inline void DockBuilderFinish( unsigned int node_id)
		{
			return (void)EngineNS::ImGuiAPI::DockBuilderFinish(node_id);
		}
		static inline void LogToTTY( int auto_open_depth)
		{
			return (void)EngineNS::ImGuiAPI::LogToTTY(auto_open_depth);
		}
		static inline void LogToFile( int auto_open_depth,const char* filename)
		{
			return (void)EngineNS::ImGuiAPI::LogToFile(auto_open_depth, filename);
		}
		static inline void LogToClipboard( int auto_open_depth)
		{
			return (void)EngineNS::ImGuiAPI::LogToClipboard(auto_open_depth);
		}
		static inline void LogFinish()
		{
			return (void)EngineNS::ImGuiAPI::LogFinish();
		}
		static inline void LogButtons()
		{
			return (void)EngineNS::ImGuiAPI::LogButtons();
		}
		static inline void LogText( const char* fmt)
		{
			return (void)EngineNS::ImGuiAPI::LogText(fmt);
		}
		static inline bool BeginDragDropSource( ImGuiDragDropFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::BeginDragDropSource(flags);
		}
		static inline bool SetDragDropPayload( const char* type,const void* data,unsigned int sz,ImGuiCond_ cond)
		{
			return (bool)EngineNS::ImGuiAPI::SetDragDropPayload(type, data, sz, cond);
		}
		static inline void EndDragDropSource()
		{
			return (void)EngineNS::ImGuiAPI::EndDragDropSource();
		}
		static inline bool BeginDragDropTarget()
		{
			return (bool)EngineNS::ImGuiAPI::BeginDragDropTarget();
		}
		static inline ImGuiPayload* AcceptDragDropPayload( const char* type,ImGuiDragDropFlags_ flags)
		{
			return (ImGuiPayload*)EngineNS::ImGuiAPI::AcceptDragDropPayload(type, flags);
		}
		static inline void EndDragDropTarget()
		{
			return (void)EngineNS::ImGuiAPI::EndDragDropTarget();
		}
		static inline ImGuiPayload* GetDragDropPayload()
		{
			return (ImGuiPayload*)EngineNS::ImGuiAPI::GetDragDropPayload();
		}
		static inline void PushClipRect( const ImVec2* clip_rect_min,const ImVec2* clip_rect_max,bool intersect_with_current_clip_rect)
		{
			return (void)EngineNS::ImGuiAPI::PushClipRect(clip_rect_min, clip_rect_max, intersect_with_current_clip_rect);
		}
		static inline void PopClipRect()
		{
			return (void)EngineNS::ImGuiAPI::PopClipRect();
		}
		static inline void SetItemDefaultFocus()
		{
			return (void)EngineNS::ImGuiAPI::SetItemDefaultFocus();
		}
		static inline void SetKeyboardFocusHere( int offset)
		{
			return (void)EngineNS::ImGuiAPI::SetKeyboardFocusHere(offset);
		}
		static inline bool IsItemHovered( ImGuiHoveredFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::IsItemHovered(flags);
		}
		static inline bool IsItemActive()
		{
			return (bool)EngineNS::ImGuiAPI::IsItemActive();
		}
		static inline bool IsItemFocused()
		{
			return (bool)EngineNS::ImGuiAPI::IsItemFocused();
		}
		static inline bool IsItemClicked( ImGuiMouseButton_ mouse_button)
		{
			return (bool)EngineNS::ImGuiAPI::IsItemClicked(mouse_button);
		}
		static inline bool IsItemDoubleClicked( ImGuiMouseButton_ mouse_button)
		{
			return (bool)EngineNS::ImGuiAPI::IsItemDoubleClicked(mouse_button);
		}
		static inline bool IsItemVisible()
		{
			return (bool)EngineNS::ImGuiAPI::IsItemVisible();
		}
		static inline bool IsItemEdited()
		{
			return (bool)EngineNS::ImGuiAPI::IsItemEdited();
		}
		static inline bool IsItemActivated()
		{
			return (bool)EngineNS::ImGuiAPI::IsItemActivated();
		}
		static inline bool IsItemDeactivated()
		{
			return (bool)EngineNS::ImGuiAPI::IsItemDeactivated();
		}
		static inline bool IsItemDeactivatedAfterEdit()
		{
			return (bool)EngineNS::ImGuiAPI::IsItemDeactivatedAfterEdit();
		}
		static inline bool IsItemToggledOpen()
		{
			return (bool)EngineNS::ImGuiAPI::IsItemToggledOpen();
		}
		static inline bool IsAnyItemHovered()
		{
			return (bool)EngineNS::ImGuiAPI::IsAnyItemHovered();
		}
		static inline bool IsAnyItemActive()
		{
			return (bool)EngineNS::ImGuiAPI::IsAnyItemActive();
		}
		static inline bool IsAnyItemFocused()
		{
			return (bool)EngineNS::ImGuiAPI::IsAnyItemFocused();
		}
		static inline ImVec2 GetItemRectMin()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetItemRectMin();
		}
		static inline ImVec2 GetItemRectMax()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetItemRectMax();
		}
		static inline ImVec2 GetItemRectSize()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetItemRectSize();
		}
		static inline bool IsRectVisible( const ImVec2* size)
		{
			return (bool)EngineNS::ImGuiAPI::IsRectVisible(size);
		}
		static inline bool IsRectVisible( const ImVec2* rect_min,const ImVec2* rect_max)
		{
			return (bool)EngineNS::ImGuiAPI::IsRectVisible(rect_min, rect_max);
		}
		static inline double GetTime()
		{
			return (double)EngineNS::ImGuiAPI::GetTime();
		}
		static inline int GetFrameCount()
		{
			return (int)EngineNS::ImGuiAPI::GetFrameCount();
		}
		static inline ImDrawList* GetBackgroundDrawList()
		{
			return (ImDrawList*)EngineNS::ImGuiAPI::GetBackgroundDrawList();
		}
		static inline ImDrawList* GetForegroundDrawList()
		{
			return (ImDrawList*)EngineNS::ImGuiAPI::GetForegroundDrawList();
		}
		static inline ImDrawList* GetBackgroundDrawList( ImGuiViewport* viewport)
		{
			return (ImDrawList*)EngineNS::ImGuiAPI::GetBackgroundDrawList(viewport);
		}
		static inline ImDrawList* GetForegroundDrawList( ImGuiViewport* viewport)
		{
			return (ImDrawList*)EngineNS::ImGuiAPI::GetForegroundDrawList(viewport);
		}
		static inline void* GetDrawListSharedData()
		{
			return (void*)EngineNS::ImGuiAPI::GetDrawListSharedData();
		}
		static inline char* GetStyleColorName( ImGuiCol_ idx)
		{
			return (char*)EngineNS::ImGuiAPI::GetStyleColorName(idx);
		}
		static inline void SetStateStorage( ImGuiStorage* storage)
		{
			return (void)EngineNS::ImGuiAPI::SetStateStorage(storage);
		}
		static inline ImGuiStorage* GetStateStorage()
		{
			return (ImGuiStorage*)EngineNS::ImGuiAPI::GetStateStorage();
		}
		static inline void SetNextItemAllowOverlap()
		{
			return (void)EngineNS::ImGuiAPI::SetNextItemAllowOverlap();
		}
		static inline ImVec2 CalcTextSize( const char* text,bool hide_text_after_double_hash,float wrap_width)
		{
			return (ImVec2)EngineNS::ImGuiAPI::CalcTextSize(text, hide_text_after_double_hash, wrap_width);
		}
		static inline ImVec4 ColorConvertU32ToFloat4( unsigned int inValue)
		{
			return (ImVec4)EngineNS::ImGuiAPI::ColorConvertU32ToFloat4(inValue);
		}
		static inline unsigned int ColorConvertFloat4ToU32( const ImVec4* inValue)
		{
			return (unsigned int)EngineNS::ImGuiAPI::ColorConvertFloat4ToU32(inValue);
		}
		static inline void ColorConvertRGBtoHSV( float r,float g,float b,float* out_h,float* out_s,float* out_v)
		{
			return (void)EngineNS::ImGuiAPI::ColorConvertRGBtoHSV(r, g, b, out_h, out_s, out_v);
		}
		static inline void ColorConvertHSVtoRGB( float h,float s,float v,float* out_r,float* out_g,float* out_b)
		{
			return (void)EngineNS::ImGuiAPI::ColorConvertHSVtoRGB(h, s, v, out_r, out_g, out_b);
		}
		static inline bool IsKeyDown( ImGuiKey user_key_index)
		{
			return (bool)EngineNS::ImGuiAPI::IsKeyDown(user_key_index);
		}
		static inline bool IsKeyPressed( ImGuiKey user_key_index,bool repeat)
		{
			return (bool)EngineNS::ImGuiAPI::IsKeyPressed(user_key_index, repeat);
		}
		static inline bool IsKeyReleased( ImGuiKey user_key_index)
		{
			return (bool)EngineNS::ImGuiAPI::IsKeyReleased(user_key_index);
		}
		static inline int GetKeyPressedAmount( ImGuiKey key_index,float repeat_delay,float rate)
		{
			return (int)EngineNS::ImGuiAPI::GetKeyPressedAmount(key_index, repeat_delay, rate);
		}
		static inline bool IsMouseDown( ImGuiMouseButton_ button)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseDown(button);
		}
		static inline bool IsMouseClicked( ImGuiMouseButton_ button,bool repeat)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseClicked(button, repeat);
		}
		static inline bool IsMouseReleased( ImGuiMouseButton_ button)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseReleased(button);
		}
		static inline bool IsMouseDoubleClicked( ImGuiMouseButton_ button)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseDoubleClicked(button);
		}
		static inline int GetMouseClickedCount( ImGuiMouseButton_ button)
		{
			return (int)EngineNS::ImGuiAPI::GetMouseClickedCount(button);
		}
		static inline bool IsMouseHoveringRect( const ImVec2* r_min,const ImVec2* r_max,bool clip)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseHoveringRect(r_min, r_max, clip);
		}
		static inline bool IsMousePosValid( const ImVec2* mouse_pos)
		{
			return (bool)EngineNS::ImGuiAPI::IsMousePosValid(mouse_pos);
		}
		static inline bool IsAnyMouseDown()
		{
			return (bool)EngineNS::ImGuiAPI::IsAnyMouseDown();
		}
		static inline ImVec2 GetMousePos()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetMousePos();
		}
		static inline ImVec2 GetMousePosOnOpeningCurrentPopup()
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetMousePosOnOpeningCurrentPopup();
		}
		static inline bool IsMouseDragging( ImGuiMouseButton_ button,float lock_threshold)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseDragging(button, lock_threshold);
		}
		static inline ImVec2 GetMouseDragDelta( ImGuiMouseButton_ button,float lock_threshold)
		{
			return (ImVec2)EngineNS::ImGuiAPI::GetMouseDragDelta(button, lock_threshold);
		}
		static inline void ResetMouseDragDelta( ImGuiMouseButton_ button)
		{
			return (void)EngineNS::ImGuiAPI::ResetMouseDragDelta(button);
		}
		static inline ImGuiMouseCursor_ GetMouseCursor()
		{
			return (ImGuiMouseCursor_)EngineNS::ImGuiAPI::GetMouseCursor();
		}
		static inline void SetMouseCursor( ImGuiMouseCursor_ cursor_type)
		{
			return (void)EngineNS::ImGuiAPI::SetMouseCursor(cursor_type);
		}
		static inline char* GetClipboardText()
		{
			return (char*)EngineNS::ImGuiAPI::GetClipboardText();
		}
		static inline void SetClipboardText( const char* text)
		{
			return (void)EngineNS::ImGuiAPI::SetClipboardText(text);
		}
		static inline void LoadIniSettingsFromDisk( const char* ini_filename)
		{
			return (void)EngineNS::ImGuiAPI::LoadIniSettingsFromDisk(ini_filename);
		}
		static inline void LoadIniSettingsFromMemory( const char* ini_data,unsigned int ini_size)
		{
			return (void)EngineNS::ImGuiAPI::LoadIniSettingsFromMemory(ini_data, ini_size);
		}
		static inline void SaveIniSettingsToDisk( const char* ini_filename)
		{
			return (void)EngineNS::ImGuiAPI::SaveIniSettingsToDisk(ini_filename);
		}
		static inline char* SaveIniSettingsToMemory( unsigned int* out_ini_size)
		{
			return (char*)EngineNS::ImGuiAPI::SaveIniSettingsToMemory(out_ini_size);
		}
		static inline bool DebugCheckVersionAndDataLayout( const char* version_str,size_t sz_io,size_t sz_style,size_t sz_vec2,size_t sz_vec4,size_t sz_drawvert,size_t sz_drawidx)
		{
			return (bool)EngineNS::ImGuiAPI::DebugCheckVersionAndDataLayout(version_str, sz_io, sz_style, sz_vec2, sz_vec4, sz_drawvert, sz_drawidx);
		}
		static inline void SetAllocatorFunctions( void *(*fn_alloc_func)(size_t, void *),void (*fn_free_func)(void *, void *),void* user_data)
		{
			return (void)EngineNS::ImGuiAPI::SetAllocatorFunctions(fn_alloc_func, fn_free_func, user_data);
		}
		static inline void* MemAlloc( size_t size)
		{
			return (void*)EngineNS::ImGuiAPI::MemAlloc(size);
		}
		static inline void MemFree( void* ptr)
		{
			return (void)EngineNS::ImGuiAPI::MemFree(ptr);
		}
		static inline ImGuiPlatformIO* GetPlatformIO()
		{
			return (ImGuiPlatformIO*)EngineNS::ImGuiAPI::GetPlatformIO();
		}
		static inline ImGuiViewport* GetMainViewport()
		{
			return (ImGuiViewport*)EngineNS::ImGuiAPI::GetMainViewport();
		}
		static inline void UpdatePlatformWindows()
		{
			return (void)EngineNS::ImGuiAPI::UpdatePlatformWindows();
		}
		static inline void RenderPlatformWindowsDefault( void* platform_render_arg,void* renderer_render_arg)
		{
			return (void)EngineNS::ImGuiAPI::RenderPlatformWindowsDefault(platform_render_arg, renderer_render_arg);
		}
		static inline void DestroyPlatformWindows()
		{
			return (void)EngineNS::ImGuiAPI::DestroyPlatformWindows();
		}
		static inline ImGuiViewport* FindViewportByID( unsigned int id)
		{
			return (ImGuiViewport*)EngineNS::ImGuiAPI::FindViewportByID(id);
		}
		static inline ImGuiViewport* FindViewportByPlatformHandle( void* platform_handle)
		{
			return (ImGuiViewport*)EngineNS::ImGuiAPI::FindViewportByPlatformHandle(platform_handle);
		}
		static inline void Set_Renderer_CreateWindow( ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *))
		{
			return (void)EngineNS::ImGuiAPI::Set_Renderer_CreateWindow(PlatformIO, fn);
		}
		static inline void Set_Renderer_DestroyWindow( ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *))
		{
			return (void)EngineNS::ImGuiAPI::Set_Renderer_DestroyWindow(PlatformIO, fn);
		}
		static inline void Set_Renderer_SetWindowSize( ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *, ImVec2))
		{
			return (void)EngineNS::ImGuiAPI::Set_Renderer_SetWindowSize(PlatformIO, fn);
		}
		static inline void Set_Renderer_RenderWindow( ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *, void *))
		{
			return (void)EngineNS::ImGuiAPI::Set_Renderer_RenderWindow(PlatformIO, fn);
		}
		static inline void Set_Renderer_SwapBuffers( ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *, void *))
		{
			return (void)EngineNS::ImGuiAPI::Set_Renderer_SwapBuffers(PlatformIO, fn);
		}
		static inline void PlatformIO_Monitor_Resize( ImGuiPlatformIO* io,int size)
		{
			return (void)EngineNS::ImGuiAPI::PlatformIO_Monitor_Resize(io, size);
		}
		static inline void PlatformIO_Monitor_PushBack( ImGuiPlatformIO* io,ImGuiPlatformMonitor monitor)
		{
			return (void)EngineNS::ImGuiAPI::PlatformIO_Monitor_PushBack(io, monitor);
		}
		static inline int PlatformIO_Viewports_Size( ImGuiPlatformIO* io)
		{
			return (int)EngineNS::ImGuiAPI::PlatformIO_Viewports_Size(io);
		}
		static inline ImGuiViewport* PlatformIO_Viewports_Get( ImGuiPlatformIO* io,int index)
		{
			return (ImGuiViewport*)EngineNS::ImGuiAPI::PlatformIO_Viewports_Get(io, index);
		}
		static inline bool TextInputComboBox( const char* id,void* buffer,unsigned int maxInputSize,const char** items,unsigned int item_len,short showMaxItems)
		{
			return (bool)EngineNS::ImGuiAPI::TextInputComboBox(id, buffer, maxInputSize, items, item_len, showMaxItems);
		}
		static inline void GetClipboardTextSetter( ImGuiIO* io,const char *(*fn)(void *))
		{
			return (void)EngineNS::ImGuiAPI::GetClipboardTextSetter(io, fn);
		}
		static inline void SetClipboardTextSetter( ImGuiIO* io,void (*fn)(void *, const char *))
		{
			return (void)EngineNS::ImGuiAPI::SetClipboardTextSetter(io, fn);
		}
		static inline void ItemSize( const ImVec2* min,const ImVec2* max,float text_baseline_y)
		{
			return (void)EngineNS::ImGuiAPI::ItemSize(min, max, text_baseline_y);
		}
		static inline void ItemSize( const ImVec2* size,float text_baseline_y)
		{
			return (void)EngineNS::ImGuiAPI::ItemSize(size, text_baseline_y);
		}
		static inline bool ItemAdd( const ImVec2* bbMin,const ImVec2* bbMax,unsigned int id,const ImVec2* nav_bb_min,const ImVec2* nav_bb_max,int flags)
		{
			return (bool)EngineNS::ImGuiAPI::ItemAdd(bbMin, bbMax, id, nav_bb_min, nav_bb_max, flags);
		}
		static inline bool ItemAdd( const ImVec2* bbMin,const ImVec2* bbMax,unsigned int id,int flags)
		{
			return (bool)EngineNS::ImGuiAPI::ItemAdd(bbMin, bbMax, id, flags);
		}
		static inline bool GetTableWorkRect( ImVec2* min,ImVec2* max)
		{
			return (bool)EngineNS::ImGuiAPI::GetTableWorkRect(min, max);
		}
		static inline bool GetTableRowStartY( float* yValue)
		{
			return (bool)EngineNS::ImGuiAPI::GetTableRowStartY(yValue);
		}
		static inline bool GetTableRowEndY( float* yValue)
		{
			return (bool)EngineNS::ImGuiAPI::GetTableRowEndY(yValue);
		}
		static inline bool IsHoverCurrentWindow()
		{
			return (bool)EngineNS::ImGuiAPI::IsHoverCurrentWindow();
		}
		static inline bool IsMouseHoveringRectInCurrentWindow( const ImVec2* r_min,const ImVec2* r_max,bool clip)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseHoveringRectInCurrentWindow(r_min, r_max, clip);
		}
		static inline bool IsMouseDownInRectInCurrentWindow( const ImVec2* r_min,const ImVec2* r_max,ImGuiMouseButton_ button,bool clip)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseDownInRectInCurrentWindow(r_min, r_max, button, clip);
		}
		static inline bool IsMouseClickedInRectInCurrentWindow( const ImVec2* r_min,const ImVec2* r_max,ImGuiMouseButton_ button,bool clip)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseClickedInRectInCurrentWindow(r_min, r_max, button, clip);
		}
		static inline bool IsMouseDoubleClickedInRectInCurrentWindow( const ImVec2* r_min,const ImVec2* r_max,ImGuiMouseButton_ button,bool clip)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseDoubleClickedInRectInCurrentWindow(r_min, r_max, button, clip);
		}
		static inline bool IsMouseDragPastThreshold( ImGuiMouseButton_ button,float lock_threshold)
		{
			return (bool)EngineNS::ImGuiAPI::IsMouseDragPastThreshold(button, lock_threshold);
		}
		static inline bool IsCurrentWindowSkipItems()
		{
			return (bool)EngineNS::ImGuiAPI::IsCurrentWindowSkipItems();
		}
		static inline bool ItemHoverable( const ImVec2* bbMin,const ImVec2* bbMax,unsigned int id,int item_flags)
		{
			return (bool)EngineNS::ImGuiAPI::ItemHoverable(bbMin, bbMax, id, item_flags);
		}
		static inline bool TempInputIsActive( unsigned int id)
		{
			return (bool)EngineNS::ImGuiAPI::TempInputIsActive(id);
		}
		static inline void SetActiveID( unsigned int id)
		{
			return (void)EngineNS::ImGuiAPI::SetActiveID(id);
		}
		static inline unsigned int GetActiveID()
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetActiveID();
		}
		static inline void SetFocusID( unsigned int id)
		{
			return (void)EngineNS::ImGuiAPI::SetFocusID(id);
		}
		static inline void SetTempInputID( unsigned int id)
		{
			return (void)EngineNS::ImGuiAPI::SetTempInputID(id);
		}
		static inline unsigned int GetTempInputID()
		{
			return (unsigned int)EngineNS::ImGuiAPI::GetTempInputID();
		}
		static inline void ClearActiveID()
		{
			return (void)EngineNS::ImGuiAPI::ClearActiveID();
		}
		static inline void FocusCurrentWindow()
		{
			return (void)EngineNS::ImGuiAPI::FocusCurrentWindow();
		}
		static inline bool IsIDNavActivated( unsigned int id)
		{
			return (bool)EngineNS::ImGuiAPI::IsIDNavActivated(id);
		}
		static inline bool IsIDNavInput( unsigned int id)
		{
			return (bool)EngineNS::ImGuiAPI::IsIDNavInput(id);
		}
		static inline bool DragBehavior( unsigned int id,int data_type,void* p_v,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragBehavior(id, data_type, p_v, v_speed, p_min, p_max, format, flags);
		}
		static inline bool DragActiveIdUpdate( unsigned int id)
		{
			return (bool)EngineNS::ImGuiAPI::DragActiveIdUpdate(id);
		}
		static inline bool ButtonBehavior( const ImVec2* min,const ImVec2* max,unsigned int id,bool* out_hovered,bool* out_held,bool pressOnRelease,ImGuiButtonFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::ButtonBehavior(min, max, id, out_hovered, out_held, pressOnRelease, flags);
		}
		static inline bool DragScalar2( const char* label,int data_type,void* p_data,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragScalar2(label, data_type, p_data, v_speed, p_min, p_max, format, flags);
		}
		static inline bool DragScalarN2( const char* label,int data_type,void* p_data,int components,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::DragScalarN2(label, data_type, p_data, components, v_speed, p_min, p_max, format, flags);
		}
		static inline void RenderFrame( ImVec2* p_min,ImVec2* p_max,unsigned int fill_col,bool border,float rounding)
		{
			return (void)EngineNS::ImGuiAPI::RenderFrame(p_min, p_max, fill_col, border, rounding);
		}
		static inline ImVec2 CalcItemSize( ImVec2* size,float default_w,float default_h)
		{
			return (ImVec2)EngineNS::ImGuiAPI::CalcItemSize(size, default_w, default_h);
		}
		static inline bool CollapsingHeader_SpanAllColumns( const char* label,ImGuiTreeNodeFlags_ flags)
		{
			return (bool)EngineNS::ImGuiAPI::CollapsingHeader_SpanAllColumns(label, flags);
		}
		static inline bool IsInTable()
		{
			return (bool)EngineNS::ImGuiAPI::IsInTable();
		}
		static inline void TableNextRow( const EngineNS::ImGuiTableRowData* rowData)
		{
			return (void)EngineNS::ImGuiAPI::TableNextRow(rowData);
		}
		static inline void TableNextRow_FirstColumn( const EngineNS::ImGuiTableRowData* rowData)
		{
			return (void)EngineNS::ImGuiAPI::TableNextRow_FirstColumn(rowData);
		}
		static inline bool CheckBoxTristate( const char* label,int* v_tristate)
		{
			return (bool)EngineNS::ImGuiAPI::CheckBoxTristate(label, v_tristate);
		}
		static inline bool ToggleButton( const char* label,bool* v,const ImVec2* size_arg,int flags)
		{
			return (bool)EngineNS::ImGuiAPI::ToggleButton(label, v, size_arg, flags);
		}
		static inline void SetKeyOwner( ImGuiKey key,unsigned int owner_id,int flags)
		{
			return (void)EngineNS::ImGuiAPI::SetKeyOwner(key, owner_id, flags);
		}
		static inline void MakeTabVisible( const char* window_name)
		{
			return (void)EngineNS::ImGuiAPI::MakeTabVisible(window_name);
		}
		static inline bool IsFirstFrame( const char* window_name)
		{
			return (bool)EngineNS::ImGuiAPI::IsFirstFrame(window_name);
		}
		static inline bool IsLastFrame( const char* window_name)
		{
			return (bool)EngineNS::ImGuiAPI::IsLastFrame(window_name);
		}
		static inline ImFont* GetDrawListFont( ImDrawList* drawList)
		{
			return (ImFont*)EngineNS::ImGuiAPI::GetDrawListFont(drawList);
		}
		static inline float GetDrawListFontSize( ImDrawList* drawList)
		{
			return (float)EngineNS::ImGuiAPI::GetDrawListFontSize(drawList);
		}
	};
}


extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_UnsafeCallDestructor(EngineNS::ImGuiAPI* self)
{
	return ImGuiAPI_Visitor::UnsafeCallDestructor(self);
}


extern "C" VFX_API EngineNS::FRttiStruct* TitanImGui_ImGuiAPI_Visitor_GetTypeRtti()
{
	return GetClassObject<EngineNS::ImGuiAPI>();
}




extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_CreateContext_3448116237(ImFontAtlas* shared_font_atlas)
{
	return ImGuiAPI_Visitor::CreateContext(shared_font_atlas);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DestroyContext_3034592143(void* ctx)
{
	return ImGuiAPI_Visitor::DestroyContext(ctx);
}
extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_GetCurrentContext_302642963()
{
	return ImGuiAPI_Visitor::GetCurrentContext();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetCurrentContext_3034592143(void* ctx)
{
	return ImGuiAPI_Visitor::SetCurrentContext(ctx);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetIniFilename_2602414842(const char* ini_filename)
{
	return ImGuiAPI_Visitor::SetIniFilename(ini_filename);
}
extern "C" VFX_API ImGuiIO* TitanImGui_ImGuiAPI_Visitor_GetIO_289112634()
{
	return ImGuiAPI_Visitor::GetIO();
}
extern "C" VFX_API ImGuiStyle* TitanImGui_ImGuiAPI_Visitor_GetStyle_1766641991()
{
	return ImGuiAPI_Visitor::GetStyle();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_NewFrame_2960189489()
{
	return ImGuiAPI_Visitor::NewFrame();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndFrame_2960189489()
{
	return ImGuiAPI_Visitor::EndFrame();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Render_2960189489()
{
	return ImGuiAPI_Visitor::Render();
}
extern "C" VFX_API ImDrawData* TitanImGui_ImGuiAPI_Visitor_GetDrawData_2577559959()
{
	return ImGuiAPI_Visitor::GetDrawData();
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_DrawData_Textures_Size_2524852328(ImDrawData* draw_data)
{
	return ImGuiAPI_Visitor::DrawData_Textures_Size(draw_data);
}
extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_DrawData_Textures_Get_3628729028(ImDrawData* draw_data,int index)
{
	return ImGuiAPI_Visitor::DrawData_Textures_Get(draw_data, index);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_PlatformIO_Textures_Size_2566682924(ImGuiPlatformIO* io)
{
	return ImGuiAPI_Visitor::PlatformIO_Textures_Size(io);
}
extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_PlatformIO_Textures_Get_955167742(ImGuiPlatformIO* io,int index)
{
	return ImGuiAPI_Visitor::PlatformIO_Textures_Get(io, index);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetStatus_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetStatus(texture_ptr);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetStatus_3475646452(void* texture_ptr,int status)
{
	return ImGuiAPI_Visitor::ImTextureData_SetStatus(texture_ptr, status);
}
extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetBackendUserData_1752491917(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetBackendUserData(texture_ptr);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetBackendUserData_760721789(void* texture_ptr,void* backend_user_data)
{
	return ImGuiAPI_Visitor::ImTextureData_SetBackendUserData(texture_ptr, backend_user_data);
}
extern "C" VFX_API unsigned long long TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetTexID_2855664449(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetTexID(texture_ptr);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetTexID_3373007845(void* texture_ptr,unsigned long long tex_id)
{
	return ImGuiAPI_Visitor::ImTextureData_SetTexID(texture_ptr, tex_id);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetFormat_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetFormat(texture_ptr);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetWidth_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetWidth(texture_ptr);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetHeight_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetHeight(texture_ptr);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetBytesPerPixel_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetBytesPerPixel(texture_ptr);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPitch_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetPitch(texture_ptr);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetSizeInBytes_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetSizeInBytes(texture_ptr);
}
extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPixels_1752491917(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetPixels(texture_ptr);
}
extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPixelsAt_1414832493(void* texture_ptr,int x,int y)
{
	return ImGuiAPI_Visitor::ImTextureData_GetPixelsAt(texture_ptr, x, y);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetUpdatesSize_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetUpdatesSize(texture_ptr);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetUnusedFrames_1896157666(void* texture_ptr)
{
	return ImGuiAPI_Visitor::ImTextureData_GetUnusedFrames(texture_ptr);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetWantDestroyNextFrame_342119689(void* texture_ptr)
{
	auto tmp_result = ImGuiAPI_Visitor::ImTextureData_GetWantDestroyNextFrame(texture_ptr);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetStatusName_4173149781(int status)
{
	return ImGuiAPI_Visitor::ImTextureData_GetStatusName(status);
}
extern "C" VFX_API char* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetFormatName_4173149781(int format)
{
	return ImGuiAPI_Visitor::ImTextureData_GetFormatName(format);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ShowDemoWindow_1193269193(bool* p_open)
{
	return ImGuiAPI_Visitor::ShowDemoWindow(p_open);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ShowAboutWindow_1193269193(bool* p_open)
{
	return ImGuiAPI_Visitor::ShowAboutWindow(p_open);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ShowMetricsWindow_1193269193(bool* p_open)
{
	return ImGuiAPI_Visitor::ShowMetricsWindow(p_open);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ShowStyleEditor_3987680467(ImGuiStyle* refValue)
{
	return ImGuiAPI_Visitor::ShowStyleEditor(refValue);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ShowStyleSelector_1080422500(const char* label)
{
	auto tmp_result = ImGuiAPI_Visitor::ShowStyleSelector(label);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ShowFontSelector_2602414842(const char* label)
{
	return ImGuiAPI_Visitor::ShowFontSelector(label);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ShowUserGuide_2960189489()
{
	return ImGuiAPI_Visitor::ShowUserGuide();
}
extern "C" VFX_API char* TitanImGui_ImGuiAPI_Visitor_GetVersion_2396230038()
{
	return ImGuiAPI_Visitor::GetVersion();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_StyleColorsDark_3987680467(ImGuiStyle* dst)
{
	return ImGuiAPI_Visitor::StyleColorsDark(dst);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_StyleColorsClassic_3987680467(ImGuiStyle* dst)
{
	return ImGuiAPI_Visitor::StyleColorsClassic(dst);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_StyleColorsLight_3987680467(ImGuiStyle* dst)
{
	return ImGuiAPI_Visitor::StyleColorsLight(dst);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_Begin_2979558607(const char* name,bool* p_open,ImGuiWindowFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::Begin(name, p_open, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_End_2960189489()
{
	return ImGuiAPI_Visitor::End();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginChild_2739961343(const char* str_id,const ImVec2* size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginChild(str_id, size, child_flags, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginChild_2598737060(unsigned int id,const ImVec2* size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginChild(id, size, child_flags, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndChild_2960189489()
{
	return ImGuiAPI_Visitor::EndChild();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsWindowAppearing_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsWindowAppearing();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsWindowCollapsed_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsWindowCollapsed();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsWindowFocused_1151558301(ImGuiFocusedFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::IsWindowFocused(flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsWindowHovered_2491699375(ImGuiHoveredFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::IsWindowHovered(flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetWindowDrawList_2196389917()
{
	return ImGuiAPI_Visitor::GetWindowDrawList();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetWindowDpiScale_3743936629()
{
	return ImGuiAPI_Visitor::GetWindowDpiScale();
}
extern "C" VFX_API ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_GetWindowViewport_4006837304()
{
	return ImGuiAPI_Visitor::GetWindowViewport();
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetWindowPos_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetWindowPos();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetWindowSize_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetWindowSize();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetWindowWidth_3743936629()
{
	return ImGuiAPI_Visitor::GetWindowWidth();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetWindowHeight_3743936629()
{
	return ImGuiAPI_Visitor::GetWindowHeight();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowPos_1923501243(const ImVec2* pos,ImGuiCond_ cond,const ImVec2* pivot)
{
	return ImGuiAPI_Visitor::SetNextWindowPos(pos, cond, pivot);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowSize_1791316118(const ImVec2* size,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetNextWindowSize(size, cond);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowSizeConstraints_684930918(const ImVec2* size_min,const ImVec2* size_max,void (*custom_callback)(ImGuiSizeCallbackData *),void* custom_callback_data)
{
	return ImGuiAPI_Visitor::SetNextWindowSizeConstraints(size_min, size_max, custom_callback, custom_callback_data);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowContentSize_4055394152(const ImVec2* size)
{
	return ImGuiAPI_Visitor::SetNextWindowContentSize(size);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowCollapsed_3341245801(bool collapsed,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetNextWindowCollapsed(collapsed, cond);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowFocus_2960189489()
{
	return ImGuiAPI_Visitor::SetNextWindowFocus();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowBgAlpha_1759962673(float alpha)
{
	return ImGuiAPI_Visitor::SetNextWindowBgAlpha(alpha);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowViewport_2252480719(unsigned int viewport_id)
{
	return ImGuiAPI_Visitor::SetNextWindowViewport(viewport_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowPos_1791316118(const ImVec2* pos,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetWindowPos(pos, cond);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowSize_1791316118(const ImVec2* size,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetWindowSize(size, cond);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowCollapsed_3341245801(bool collapsed,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetWindowCollapsed(collapsed, cond);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowFocus_2960189489()
{
	return ImGuiAPI_Visitor::SetWindowFocus();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowFocus_2602414842(const char* name)
{
	return ImGuiAPI_Visitor::SetWindowFocus(name);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowFontScale_1759962673(float scale)
{
	return ImGuiAPI_Visitor::SetWindowFontScale(scale);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowPos_1093786109(const char* name,const ImVec2* pos,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetWindowPos(name, pos, cond);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowSize_1093786109(const char* name,const ImVec2* size,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetWindowSize(name, size, cond);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetWindowCollapsed_3917756216(const char* name,bool collapsed,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetWindowCollapsed(name, collapsed, cond);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetContentRegionMax_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetContentRegionMax();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetContentRegionAvail_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetContentRegionAvail();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionMin_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetWindowContentRegionMin();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionMax_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetWindowContentRegionMax();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionWidth_3743936629()
{
	return ImGuiAPI_Visitor::GetWindowContentRegionWidth();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetScrollX_3743936629()
{
	return ImGuiAPI_Visitor::GetScrollX();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetScrollY_3743936629()
{
	return ImGuiAPI_Visitor::GetScrollY();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetScrollMaxX_3743936629()
{
	return ImGuiAPI_Visitor::GetScrollMaxX();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetScrollMaxY_3743936629()
{
	return ImGuiAPI_Visitor::GetScrollMaxY();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetScrollX_1759962673(float scroll_x)
{
	return ImGuiAPI_Visitor::SetScrollX(scroll_x);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetScrollY_1759962673(float scroll_y)
{
	return ImGuiAPI_Visitor::SetScrollY(scroll_y);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetScrollHereX_1759962673(float center_x_ratio)
{
	return ImGuiAPI_Visitor::SetScrollHereX(center_x_ratio);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetScrollHereY_1759962673(float center_y_ratio)
{
	return ImGuiAPI_Visitor::SetScrollHereY(center_y_ratio);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetScrollFromPosX_996365349(float local_x,float center_x_ratio)
{
	return ImGuiAPI_Visitor::SetScrollFromPosX(local_x, center_x_ratio);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetScrollFromPosY_996365349(float local_y,float center_y_ratio)
{
	return ImGuiAPI_Visitor::SetScrollFromPosY(local_y, center_y_ratio);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushFont_2187443828(ImFont* font)
{
	return ImGuiAPI_Visitor::PushFont(font);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushFontWithSize_41148096(ImFont* font,float font_size_base_unscaled)
{
	return ImGuiAPI_Visitor::PushFontWithSize(font, font_size_base_unscaled);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopFont_2960189489()
{
	return ImGuiAPI_Visitor::PopFont();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushStyleColor_2781163097(ImGuiCol_ idx,unsigned int col)
{
	return ImGuiAPI_Visitor::PushStyleColor(idx, col);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushStyleColor_506689308(ImGuiCol_ idx,const ImVec4* col)
{
	return ImGuiAPI_Visitor::PushStyleColor(idx, col);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopStyleColor_4038704236(int count)
{
	return ImGuiAPI_Visitor::PopStyleColor(count);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushStyleVar_1680981031(ImGuiStyleVar_ idx,float val)
{
	return ImGuiAPI_Visitor::PushStyleVar(idx, val);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushStyleVar_3737822352(ImGuiStyleVar_ idx,const ImVec2* val)
{
	return ImGuiAPI_Visitor::PushStyleVar(idx, val);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopStyleVar_4038704236(int count)
{
	return ImGuiAPI_Visitor::PopStyleVar(count);
}
extern "C" VFX_API ImVec4* TitanImGui_ImGuiAPI_Visitor_GetStyleColorVec4_3399559918(ImGuiCol_ idx)
{
	return ImGuiAPI_Visitor::GetStyleColorVec4(idx);
}
extern "C" VFX_API ImFont* TitanImGui_ImGuiAPI_Visitor_GetFont_2919724924()
{
	return ImGuiAPI_Visitor::GetFont();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetFontSize_3743936629()
{
	return ImGuiAPI_Visitor::GetFontSize();
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetFontTexUvWhitePixel_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetFontTexUvWhitePixel();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetColorU32_2992096237(ImGuiCol_ idx,float alpha_mul)
{
	return ImGuiAPI_Visitor::GetColorU32(idx, alpha_mul);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetColorU32_2189523068(const ImVec4* col)
{
	return ImGuiAPI_Visitor::GetColorU32(col);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetColorU32_194637277(unsigned int col)
{
	return ImGuiAPI_Visitor::GetColorU32(col);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushItemWidth_1759962673(float item_width)
{
	return ImGuiAPI_Visitor::PushItemWidth(item_width);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopItemWidth_2960189489()
{
	return ImGuiAPI_Visitor::PopItemWidth();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextItemWidth_1759962673(float item_width)
{
	return ImGuiAPI_Visitor::SetNextItemWidth(item_width);
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_CalcItemWidth_3743936629()
{
	return ImGuiAPI_Visitor::CalcItemWidth();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushTextWrapPos_1759962673(float wrap_local_pos_x)
{
	return ImGuiAPI_Visitor::PushTextWrapPos(wrap_local_pos_x);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopTextWrapPos_2960189489()
{
	return ImGuiAPI_Visitor::PopTextWrapPos();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushAllowKeyboardFocus_2077628183(bool allow_keyboard_focus)
{
	return ImGuiAPI_Visitor::PushAllowKeyboardFocus(allow_keyboard_focus);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopAllowKeyboardFocus_2960189489()
{
	return ImGuiAPI_Visitor::PopAllowKeyboardFocus();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushButtonRepeat_2077628183(bool repeat)
{
	return ImGuiAPI_Visitor::PushButtonRepeat(repeat);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopButtonRepeat_2960189489()
{
	return ImGuiAPI_Visitor::PopButtonRepeat();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Separator_2960189489()
{
	return ImGuiAPI_Visitor::Separator();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SameLine_996365349(float offset_from_start_x,float spacing)
{
	return ImGuiAPI_Visitor::SameLine(offset_from_start_x, spacing);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_NewLine_2960189489()
{
	return ImGuiAPI_Visitor::NewLine();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Spacing_2960189489()
{
	return ImGuiAPI_Visitor::Spacing();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Dummy_4055394152(const ImVec2* size)
{
	return ImGuiAPI_Visitor::Dummy(size);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Indent_1759962673(float indent_w)
{
	return ImGuiAPI_Visitor::Indent(indent_w);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Unindent_1759962673(float indent_w)
{
	return ImGuiAPI_Visitor::Unindent(indent_w);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_BeginGroup_2960189489()
{
	return ImGuiAPI_Visitor::BeginGroup();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndGroup_2960189489()
{
	return ImGuiAPI_Visitor::EndGroup();
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetCursorPos_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetCursorPos();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetCursorPosX_3743936629()
{
	return ImGuiAPI_Visitor::GetCursorPosX();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetCursorPosY_3743936629()
{
	return ImGuiAPI_Visitor::GetCursorPosY();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetCursorPos_4055394152(const ImVec2* local_pos)
{
	return ImGuiAPI_Visitor::SetCursorPos(local_pos);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetCursorPosX_1759962673(float local_x)
{
	return ImGuiAPI_Visitor::SetCursorPosX(local_x);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetCursorPosY_1759962673(float local_y)
{
	return ImGuiAPI_Visitor::SetCursorPosY(local_y);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetCursorStartPos_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetCursorStartPos();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetCursorScreenPos_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetCursorScreenPos();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetCursorScreenPos_4055394152(const ImVec2* pos)
{
	return ImGuiAPI_Visitor::SetCursorScreenPos(pos);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_AlignTextToFramePadding_2960189489()
{
	return ImGuiAPI_Visitor::AlignTextToFramePadding();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetTextLineHeight_3743936629()
{
	return ImGuiAPI_Visitor::GetTextLineHeight();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetTextLineHeightWithSpacing_3743936629()
{
	return ImGuiAPI_Visitor::GetTextLineHeightWithSpacing();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetFrameHeight_3743936629()
{
	return ImGuiAPI_Visitor::GetFrameHeight();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetFrameHeightWithSpacing_3743936629()
{
	return ImGuiAPI_Visitor::GetFrameHeightWithSpacing();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushID_2602414842(const char* str_id)
{
	return ImGuiAPI_Visitor::PushID(str_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushID_568371421(const char* str_id_begin,const char* str_id_end)
{
	return ImGuiAPI_Visitor::PushID(str_id_begin, str_id_end);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushID_1819065180(const void* ptr_id)
{
	return ImGuiAPI_Visitor::PushID(ptr_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushID_4038704236(int int_id)
{
	return ImGuiAPI_Visitor::PushID(int_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopID_2960189489()
{
	return ImGuiAPI_Visitor::PopID();
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetID_1084213664(const char* str_id)
{
	return ImGuiAPI_Visitor::GetID(str_id);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetID_2265815331(const char* str_id_begin,const char* str_id_end)
{
	return ImGuiAPI_Visitor::GetID(str_id_begin, str_id_end);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetID_2215092506(const void* ptr_id)
{
	return ImGuiAPI_Visitor::GetID(ptr_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TextUnformatted_2602414842(const char* text)
{
	return ImGuiAPI_Visitor::TextUnformatted(text);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TextAsPointer_2602414842(const char* fmt)
{
	return ImGuiAPI_Visitor::TextAsPointer(fmt);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Text_2602414842(const char* fmt)
{
	return ImGuiAPI_Visitor::Text(fmt);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TextColored_2591110465(const ImVec4* col,const char* fmt)
{
	return ImGuiAPI_Visitor::TextColored(col, fmt);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TextDisabled_2602414842(const char* fmt)
{
	return ImGuiAPI_Visitor::TextDisabled(fmt);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TextWrapped_2602414842(const char* fmt)
{
	return ImGuiAPI_Visitor::TextWrapped(fmt);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LabelText_568371421(const char* label,const char* fmt)
{
	return ImGuiAPI_Visitor::LabelText(label, fmt);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_BulletText_2602414842(const char* fmt)
{
	return ImGuiAPI_Visitor::BulletText(fmt);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_Button_3507648889(const char* label,const ImVec2* size)
{
	auto tmp_result = ImGuiAPI_Visitor::Button(label, size);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SmallButton_1080422500(const char* label)
{
	auto tmp_result = ImGuiAPI_Visitor::SmallButton(label);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InvisibleButton_2525803684(const char* str_id,const ImVec2* size,ImGuiButtonFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InvisibleButton(str_id, size, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ArrowButton_4112088628(const char* str_id,ImGuiDir dir)
{
	auto tmp_result = ImGuiAPI_Visitor::ArrowButton(str_id, dir);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Arrow_543170368(ImDrawList* draw_list,const ImVec2* pos,unsigned int col,ImGuiDir dir,float scale)
{
	return ImGuiAPI_Visitor::Arrow(draw_list, pos, col, dir, scale);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Image_3718021420(unsigned long long user_texture_id,const ImVec2* size,const ImVec2* uv0,const ImVec2* uv1,const ImVec4* tint_col,const ImVec4* border_col)
{
	return ImGuiAPI_Visitor::Image(user_texture_id, size, uv0, uv1, tint_col, border_col);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ImageButton_550850427(const char* name,unsigned long long user_texture_id,const ImVec2* size,const ImVec2* uv0,const ImVec2* uv1,const ImVec4* bg_col,const ImVec4* tint_col)
{
	auto tmp_result = ImGuiAPI_Visitor::ImageButton(name, user_texture_id, size, uv0, uv1, bg_col, tint_col);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_Checkbox_2817596850(const char* label,bool* v)
{
	auto tmp_result = ImGuiAPI_Visitor::Checkbox(label, v);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_CheckboxFlags_1998082816(const char* label,int* flags,int flags_value)
{
	auto tmp_result = ImGuiAPI_Visitor::CheckboxFlags(label, flags, flags_value);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_CheckboxFlags_145502690(const char* label,unsigned int* flags,unsigned int flags_value)
{
	auto tmp_result = ImGuiAPI_Visitor::CheckboxFlags(label, flags, flags_value);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_RadioButton_3037903904(const char* label,bool active)
{
	auto tmp_result = ImGuiAPI_Visitor::RadioButton(label, active);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_RadioButton_1998082816(const char* label,int* v,int v_button)
{
	auto tmp_result = ImGuiAPI_Visitor::RadioButton(label, v, v_button);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ProgressBar_2910944627(float fraction,const ImVec2* size_arg,const char* overlay)
{
	return ImGuiAPI_Visitor::ProgressBar(fraction, size_arg, overlay);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Bullet_2960189489()
{
	return ImGuiAPI_Visitor::Bullet();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginCombo_3858370926(const char* label,const char* preview_value,ImGuiComboFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginCombo(label, preview_value, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginCombo_31333123(const char* label,const char* preview_value,ImGuiComboFlags_ flags,ImGuiWindowFlags_ winFlags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginCombo(label, preview_value, flags, winFlags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndCombo_2960189489()
{
	return ImGuiAPI_Visitor::EndCombo();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_Combo_1210971960(const char* label,int* current_item,const char** items,int items_count,int popup_max_height_in_items)
{
	auto tmp_result = ImGuiAPI_Visitor::Combo(label, current_item, items, items_count, popup_max_height_in_items);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_Combo_161697717(const char* label,int* current_item,const char* items_separated_by_zeros,int popup_max_height_in_items)
{
	auto tmp_result = ImGuiAPI_Visitor::Combo(label, current_item, items_separated_by_zeros, popup_max_height_in_items);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_Combo_3739767375(const char* label,int* current_item,const char *(*fn_getter)(void *, int),void* data,int items_count,int popup_max_height_in_items)
{
	auto tmp_result = ImGuiAPI_Visitor::Combo(label, current_item, fn_getter, data, items_count, popup_max_height_in_items);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragFloat_3863841807(const char* label,float* v,float v_speed,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragFloat(label, v, v_speed, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragFloat2_3863841807(const char* label,float* v,float v_speed,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragFloat2(label, v, v_speed, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragFloat3_3863841807(const char* label,float* v,float v_speed,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragFloat3(label, v, v_speed, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragFloat4_3863841807(const char* label,float* v,float v_speed,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragFloat4(label, v, v_speed, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragFloatRange2_3996816114(const char* label,float* v_current_min,float* v_current_max,float v_speed,float v_min,float v_max,const char* format,const char* format_max,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragFloatRange2(label, v_current_min, v_current_max, v_speed, v_min, v_max, format, format_max, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragInt_3407096620(const char* label,int* v,float v_speed,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragInt(label, v, v_speed, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragInt2_3407096620(const char* label,int* v,float v_speed,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragInt2(label, v, v_speed, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragInt3_3407096620(const char* label,int* v,float v_speed,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragInt3(label, v, v_speed, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragInt4_3407096620(const char* label,int* v,float v_speed,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragInt4(label, v, v_speed, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragIntRange2_52041202(const char* label,int* v_current_min,int* v_current_max,float v_speed,int v_min,int v_max,const char* format,const char* format_max,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragIntRange2(label, v_current_min, v_current_max, v_speed, v_min, v_max, format, format_max, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragScalar_3791056475(const char* label,ImGuiDataType_ data_type,void* p_data,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragScalar(label, data_type, p_data, v_speed, p_min, p_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragScalarN_576259350(const char* label,ImGuiDataType_ data_type,void* p_data,int components,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragScalarN(label, data_type, p_data, components, v_speed, p_min, p_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderFloat_281608583(const char* label,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderFloat(label, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderFloat2_281608583(const char* label,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderFloat2(label, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderFloat3_281608583(const char* label,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderFloat3(label, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderFloat4_281608583(const char* label,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderFloat4(label, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderAngle_281608583(const char* label,float* v_rad,float v_degrees_min,float v_degrees_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderAngle(label, v_rad, v_degrees_min, v_degrees_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderInt_4038701670(const char* label,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderInt(label, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderInt2_4038701670(const char* label,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderInt2(label, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderInt3_4038701670(const char* label,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderInt3(label, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderInt4_4038701670(const char* label,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderInt4(label, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderScalar_2997903363(const char* label,ImGuiDataType_ data_type,void* p_data,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderScalar(label, data_type, p_data, p_min, p_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SliderScalarN_4051498348(const char* label,ImGuiDataType_ data_type,void* p_data,int components,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::SliderScalarN(label, data_type, p_data, components, p_min, p_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_VSliderFloat_2008079404(const char* label,const ImVec2* size,float* v,float v_min,float v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::VSliderFloat(label, size, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_VSliderInt_1088819369(const char* label,const ImVec2* size,int* v,int v_min,int v_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::VSliderInt(label, size, v, v_min, v_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_VSliderScalar_1401309246(const char* label,const ImVec2* size,ImGuiDataType_ data_type,void* p_data,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::VSliderScalar(label, size, data_type, p_data, p_min, p_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputText_530115699(const char* label,void* buf,unsigned int buf_size,ImGuiInputTextFlags_ flags,int (*callback)(ImGuiInputTextCallbackData *),void* user_data)
{
	auto tmp_result = ImGuiAPI_Visitor::InputText(label, buf, buf_size, flags, callback, user_data);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputTextNoName_530115699(const char* label,void* buf,unsigned int buf_size,ImGuiInputTextFlags_ flags,int (*callback)(ImGuiInputTextCallbackData *),void* user_data)
{
	auto tmp_result = ImGuiAPI_Visitor::InputTextNoName(label, buf, buf_size, flags, callback, user_data);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputTextMultiline_1095533176(const char* label,char* buf,unsigned int buf_size,const ImVec2* size,ImGuiInputTextFlags_ flags,int (*callback)(ImGuiInputTextCallbackData *),void* user_data)
{
	auto tmp_result = ImGuiAPI_Visitor::InputTextMultiline(label, buf, buf_size, size, flags, callback, user_data);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputTextWithHint_433703410(const char* label,const char* hint,char* buf,unsigned int buf_size,ImGuiInputTextFlags_ flags,int (*callback)(ImGuiInputTextCallbackData *),void* user_data)
{
	auto tmp_result = ImGuiAPI_Visitor::InputTextWithHint(label, hint, buf, buf_size, flags, callback, user_data);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputFloat_3636630689(const char* label,float* v,float step,float step_fast,const char* format,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputFloat(label, v, step, step_fast, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputFloat2_1399196753(const char* label,float* v,const char* format,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputFloat2(label, v, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputFloat3_1399196753(const char* label,float* v,const char* format,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputFloat3(label, v, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputFloat4_1399196753(const char* label,float* v,const char* format,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputFloat4(label, v, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputInt_4119295329(const char* label,int* v,int step,int step_fast,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputInt(label, v, step, step_fast, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputInt2_772478873(const char* label,int* v,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputInt2(label, v, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputInt3_772478873(const char* label,int* v,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputInt3(label, v, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputInt4_772478873(const char* label,int* v,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputInt4(label, v, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputDouble_4137560224(const char* label,double* v,double step,double step_fast,const char* format,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputDouble(label, v, step, step_fast, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputScalar_1030920285(const char* label,ImGuiDataType_ data_type,void* p_data,const void* p_step,const void* p_step_fast,const char* format,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputScalar(label, data_type, p_data, p_step, p_step_fast, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_InputScalarN_3243426466(const char* label,ImGuiDataType_ data_type,void* p_data,int components,const void* p_step,const void* p_step_fast,const char* format,ImGuiInputTextFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::InputScalarN(label, data_type, p_data, components, p_step, p_step_fast, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ColorEdit3_833964466(const char* label,float* col,ImGuiColorEditFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::ColorEdit3(label, col, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ColorEdit4_833964466(const char* label,float* col,ImGuiColorEditFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::ColorEdit4(label, col, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ColorPicker3_833964466(const char* label,float* col,ImGuiColorEditFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::ColorPicker3(label, col, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ColorPicker4_2514436871(const char* label,float* col,ImGuiColorEditFlags_ flags,const float* _col2)
{
	auto tmp_result = ImGuiAPI_Visitor::ColorPicker4(label, col, flags, _col2);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ColorButton_4115823885(const char* desc_id,const ImVec4* col,ImGuiColorEditFlags_ flags,ImVec2* size)
{
	auto tmp_result = ImGuiAPI_Visitor::ColorButton(desc_id, col, flags, size);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetColorEditOptions_4022841967(ImGuiColorEditFlags_ flags)
{
	return ImGuiAPI_Visitor::SetColorEditOptions(flags);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TreeNode_1080422500(const char* label)
{
	auto tmp_result = ImGuiAPI_Visitor::TreeNode(label);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TreeNode_459238835(const char* str_id,const char* fmt)
{
	auto tmp_result = ImGuiAPI_Visitor::TreeNode(str_id, fmt);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TreeNode_3207787083(const void* ptr_id,const char* fmt)
{
	auto tmp_result = ImGuiAPI_Visitor::TreeNode(ptr_id, fmt);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2036359645(const char* label,ImGuiTreeNodeFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::TreeNodeEx(label, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2368787316(const char* str_id,ImGuiTreeNodeFlags_ flags,const char* fmt)
{
	auto tmp_result = ImGuiAPI_Visitor::TreeNodeEx(str_id, flags, fmt);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2410412098(const void* ptr_id,ImGuiTreeNodeFlags_ flags,const char* fmt)
{
	auto tmp_result = ImGuiAPI_Visitor::TreeNodeEx(ptr_id, flags, fmt);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TreePush_2602414842(const char* str_id)
{
	return ImGuiAPI_Visitor::TreePush(str_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TreePush_1819065180(const void* ptr_id)
{
	return ImGuiAPI_Visitor::TreePush(ptr_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TreePop_2960189489()
{
	return ImGuiAPI_Visitor::TreePop();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetTreeNodeToLabelSpacing_3743936629()
{
	return ImGuiAPI_Visitor::GetTreeNodeToLabelSpacing();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_2036359645(const char* label,ImGuiTreeNodeFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::CollapsingHeader(label, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_2891925945(const char* label,bool* p_open,ImGuiTreeNodeFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::CollapsingHeader(label, p_open, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextItemOpen_3341245801(bool is_open,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetNextItemOpen(is_open, cond);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_Selectable_1745889278(const char* label,bool selected,ImGuiSelectableFlags_ flags,const ImVec2* size)
{
	auto tmp_result = ImGuiAPI_Visitor::Selectable(label, selected, flags, size);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_Selectable_778409408(const char* label,bool* p_selected,ImGuiSelectableFlags_ flags,const ImVec2* size)
{
	auto tmp_result = ImGuiAPI_Visitor::Selectable(label, p_selected, flags, size);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ListBox_1210971960(const char* label,int* current_item,const char** items,int items_count,int height_in_items)
{
	auto tmp_result = ImGuiAPI_Visitor::ListBox(label, current_item, items, items_count, height_in_items);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ListBox_3739767375(const char* label,int* current_item,const char *(*fn_getter)(void *, int),void* data,int items_count,int height_in_items)
{
	auto tmp_result = ImGuiAPI_Visitor::ListBox(label, current_item, fn_getter, data, items_count, height_in_items);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PlotLines_3358809591(const char* label,const float* values,int values_count,int values_offset,const char* overlay_text,float scale_min,float scale_max,ImVec2 graph_size,int stride)
{
	return ImGuiAPI_Visitor::PlotLines(label, values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PlotLines_403696211(const char* label,float (*fn_getter)(void *, int),void* data,int values_count,int values_offset,const char* overlay_text,float scale_min,float scale_max,ImVec2 graph_size)
{
	return ImGuiAPI_Visitor::PlotLines(label, fn_getter, data, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PlotHistogram_3358809591(const char* label,const float* values,int values_count,int values_offset,const char* overlay_text,float scale_min,float scale_max,ImVec2 graph_size,int stride)
{
	return ImGuiAPI_Visitor::PlotHistogram(label, values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PlotHistogram_403696211(const char* label,float (*fn_getter)(void *, int),void* data,int values_count,int values_offset,const char* overlay_text,float scale_min,float scale_max,ImVec2 graph_size)
{
	return ImGuiAPI_Visitor::PlotHistogram(label, fn_getter, data, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Value_3791955190(const char* prefix,bool b)
{
	return ImGuiAPI_Visitor::Value(prefix, b);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Value_2553264241(const char* prefix,int v)
{
	return ImGuiAPI_Visitor::Value(prefix, v);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Value_641062864(const char* prefix,unsigned int v)
{
	return ImGuiAPI_Visitor::Value(prefix, v);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Value_3548670745(const char* prefix,float v,const char* float_format)
{
	return ImGuiAPI_Visitor::Value(prefix, v, float_format);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginMenuBar_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::BeginMenuBar();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndMenuBar_2960189489()
{
	return ImGuiAPI_Visitor::EndMenuBar();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginMainMenuBar_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::BeginMainMenuBar();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndMainMenuBar_2960189489()
{
	return ImGuiAPI_Visitor::EndMainMenuBar();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginMenu_3037903904(const char* label,bool enabled)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginMenu(label, enabled);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndMenu_2960189489()
{
	return ImGuiAPI_Visitor::EndMenu();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_MenuItem_1086805043(const char* label,const char* shortcut,bool selected,bool enabled)
{
	auto tmp_result = ImGuiAPI_Visitor::MenuItem(label, shortcut, selected, enabled);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_MenuItem_3678366017(const char* label,const char* shortcut,bool* p_selected,bool enabled)
{
	auto tmp_result = ImGuiAPI_Visitor::MenuItem(label, shortcut, p_selected, enabled);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginTooltip_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::BeginTooltip();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndTooltip_2960189489()
{
	return ImGuiAPI_Visitor::EndTooltip();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetTooltip_2602414842(const char* fmt)
{
	return ImGuiAPI_Visitor::SetTooltip(fmt);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginPopup_2063302891(const char* str_id,ImGuiWindowFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginPopup(str_id, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginPopupModal_2979558607(const char* name,bool* p_open,ImGuiWindowFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginPopupModal(name, p_open, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndPopup_2960189489()
{
	return ImGuiAPI_Visitor::EndPopup();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_OpenPopup_780651675(const char* str_id,ImGuiPopupFlags_ popup_flags)
{
	return ImGuiAPI_Visitor::OpenPopup(str_id, popup_flags);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_OpenPopupOnItemClick_780651675(const char* str_id,ImGuiPopupFlags_ popup_flags)
{
	return ImGuiAPI_Visitor::OpenPopupOnItemClick(str_id, popup_flags);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_CloseCurrentPopup_2960189489()
{
	return ImGuiAPI_Visitor::CloseCurrentPopup();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginPopupContextItem_3980950421(const char* str_id,ImGuiPopupFlags_ popup_flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginPopupContextItem(str_id, popup_flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginPopupContextWindow_3980950421(const char* str_id,ImGuiPopupFlags_ popup_flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginPopupContextWindow(str_id, popup_flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginPopupContextVoid_3980950421(const char* str_id,ImGuiPopupFlags_ popup_flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginPopupContextVoid(str_id, popup_flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsPopupOpen_3980950421(const char* str_id,ImGuiPopupFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::IsPopupOpen(str_id, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Columns_1241066619(int count,const char* id,bool border)
{
	return ImGuiAPI_Visitor::Columns(count, id, border);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_NextColumn_2960189489()
{
	return ImGuiAPI_Visitor::NextColumn();
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_GetColumnIndex_2704135706()
{
	return ImGuiAPI_Visitor::GetColumnIndex();
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetColumnWidth_1859829344(int column_index)
{
	return ImGuiAPI_Visitor::GetColumnWidth(column_index);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetColumnWidth_2140556032(int column_index,float width)
{
	return ImGuiAPI_Visitor::SetColumnWidth(column_index, width);
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetColumnOffset_1859829344(int column_index)
{
	return ImGuiAPI_Visitor::GetColumnOffset(column_index);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetColumnOffset_2140556032(int column_index,float offset_x)
{
	return ImGuiAPI_Visitor::SetColumnOffset(column_index, offset_x);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_GetColumnsCount_2704135706()
{
	return ImGuiAPI_Visitor::GetColumnsCount();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginTable_592367644(const char* str_id,int column,int flags,const ImVec2* outer_size,float inner_width)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginTable(str_id, column, flags, outer_size, inner_width);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndTable_2960189489()
{
	return ImGuiAPI_Visitor::EndTable();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableNextRow_2950496009(int row_flags,float min_row_height)
{
	return ImGuiAPI_Visitor::TableNextRow(row_flags, min_row_height);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TableNextColumn_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::TableNextColumn();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TableSetColumnIndex_1125491426(int column_n)
{
	auto tmp_result = ImGuiAPI_Visitor::TableSetColumnIndex(column_n);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableSetupColumn_2358737958(const char* label,int flags,float init_width_or_weight,unsigned int user_id)
{
	return ImGuiAPI_Visitor::TableSetupColumn(label, flags, init_width_or_weight, user_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableSetupScrollFreeze_3539386109(int cols,int rows)
{
	return ImGuiAPI_Visitor::TableSetupScrollFreeze(cols, rows);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableHeadersRow_2960189489()
{
	return ImGuiAPI_Visitor::TableHeadersRow();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableHeader_2602414842(const char* label)
{
	return ImGuiAPI_Visitor::TableHeader(label);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_TableGetColumnCount_2704135706()
{
	return ImGuiAPI_Visitor::TableGetColumnCount();
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_TableGetColumnIndex_2704135706()
{
	return ImGuiAPI_Visitor::TableGetColumnIndex();
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_TableGetRowIndex_2704135706()
{
	return ImGuiAPI_Visitor::TableGetRowIndex();
}
extern "C" VFX_API char* TitanImGui_ImGuiAPI_Visitor_TableGetColumnName_4173149781(int column_n)
{
	return ImGuiAPI_Visitor::TableGetColumnName(column_n);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_TableGetColumnFlags_3804733202(int column_n)
{
	return ImGuiAPI_Visitor::TableGetColumnFlags(column_n);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableSetColumnEnabled_2814434660(int column_n,bool v)
{
	return ImGuiAPI_Visitor::TableSetColumnEnabled(column_n, v);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableSetBgColor_3948758695(int target,unsigned int color,int column_n)
{
	return ImGuiAPI_Visitor::TableSetBgColor(target, color, column_n);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginTabBar_697337101(const char* str_id,ImGuiTabBarFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginTabBar(str_id, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndTabBar_2960189489()
{
	return ImGuiAPI_Visitor::EndTabBar();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginTabItem_1317613329(const char* label,bool* p_open,ImGuiTabItemFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginTabItem(label, p_open, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndTabItem_2960189489()
{
	return ImGuiAPI_Visitor::EndTabItem();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TabItemButton_1489982093(const char* label,ImGuiTabItemFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::TabItemButton(label, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetTabItemClosed_2602414842(const char* tab_or_docked_window_label)
{
	return ImGuiAPI_Visitor::SetTabItemClosed(tab_or_docked_window_label);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_DockSpace_737848097(unsigned int id,const ImVec2* size,int flags,const ImGuiWindowClass* window_class)
{
	return ImGuiAPI_Visitor::DockSpace(id, size, flags, window_class);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_DockSpaceOverViewport_3195982265(unsigned int dock_id,ImGuiViewport* viewport,int flags,const ImGuiWindowClass* window_class)
{
	return ImGuiAPI_Visitor::DockSpaceOverViewport(dock_id, viewport, flags, window_class);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowDockID_3620127785(unsigned int dock_id,ImGuiCond_ cond)
{
	return ImGuiAPI_Visitor::SetNextWindowDockID(dock_id, cond);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextWindowClass_2280511539(const ImGuiWindowClass* window_class)
{
	return ImGuiAPI_Visitor::SetNextWindowClass(window_class);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetWindowDockID_3529484159()
{
	return ImGuiAPI_Visitor::GetWindowDockID();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsWindowDocked_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsWindowDocked();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DockBuilderDockWindow_2353073608(const char* window_name,unsigned int node_id)
{
	return ImGuiAPI_Visitor::DockBuilderDockWindow(window_name, node_id);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_DockBuilderAddNode_1731294598(unsigned int node_id,int flags)
{
	return ImGuiAPI_Visitor::DockBuilderAddNode(node_id, flags);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNode_2252480719(unsigned int node_id)
{
	return ImGuiAPI_Visitor::DockBuilderRemoveNode(node_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNodeDockedWindows_1053065717(unsigned int node_id,bool clear_settings_refs)
{
	return ImGuiAPI_Visitor::DockBuilderRemoveNodeDockedWindows(node_id, clear_settings_refs);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNodeChildNodes_2252480719(unsigned int node_id)
{
	return ImGuiAPI_Visitor::DockBuilderRemoveNodeChildNodes(node_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DockBuilderSetNodePos_3396750409(unsigned int node_id,ImVec2 pos)
{
	return ImGuiAPI_Visitor::DockBuilderSetNodePos(node_id, pos);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DockBuilderSetNodeSize_3396750409(unsigned int node_id,ImVec2 size)
{
	return ImGuiAPI_Visitor::DockBuilderSetNodeSize(node_id, size);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_DockBuilderSplitNode_230192101(unsigned int node_id,ImGuiDir split_dir,float size_ratio_for_node_at_dir,unsigned int* out_id_at_dir,unsigned int* out_id_at_opposite_dir)
{
	return ImGuiAPI_Visitor::DockBuilderSplitNode(node_id, split_dir, size_ratio_for_node_at_dir, out_id_at_dir, out_id_at_opposite_dir);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DockBuilderCopyWindowSettings_568371421(const char* src_name,const char* dst_name)
{
	return ImGuiAPI_Visitor::DockBuilderCopyWindowSettings(src_name, dst_name);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DockBuilderFinish_2252480719(unsigned int node_id)
{
	return ImGuiAPI_Visitor::DockBuilderFinish(node_id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LogToTTY_4038704236(int auto_open_depth)
{
	return ImGuiAPI_Visitor::LogToTTY(auto_open_depth);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LogToFile_85734681(int auto_open_depth,const char* filename)
{
	return ImGuiAPI_Visitor::LogToFile(auto_open_depth, filename);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LogToClipboard_4038704236(int auto_open_depth)
{
	return ImGuiAPI_Visitor::LogToClipboard(auto_open_depth);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LogFinish_2960189489()
{
	return ImGuiAPI_Visitor::LogFinish();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LogButtons_2960189489()
{
	return ImGuiAPI_Visitor::LogButtons();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LogText_2602414842(const char* fmt)
{
	return ImGuiAPI_Visitor::LogText(fmt);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginDragDropSource_4036233765(ImGuiDragDropFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::BeginDragDropSource(flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_SetDragDropPayload_411387055(const char* type,const void* data,unsigned int sz,ImGuiCond_ cond)
{
	auto tmp_result = ImGuiAPI_Visitor::SetDragDropPayload(type, data, sz, cond);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndDragDropSource_2960189489()
{
	return ImGuiAPI_Visitor::EndDragDropSource();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_BeginDragDropTarget_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::BeginDragDropTarget();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API ImGuiPayload* TitanImGui_ImGuiAPI_Visitor_AcceptDragDropPayload_311770(const char* type,ImGuiDragDropFlags_ flags)
{
	return ImGuiAPI_Visitor::AcceptDragDropPayload(type, flags);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_EndDragDropTarget_2960189489()
{
	return ImGuiAPI_Visitor::EndDragDropTarget();
}
extern "C" VFX_API ImGuiPayload* TitanImGui_ImGuiAPI_Visitor_GetDragDropPayload_2736951583()
{
	return ImGuiAPI_Visitor::GetDragDropPayload();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PushClipRect_229364439(const ImVec2* clip_rect_min,const ImVec2* clip_rect_max,bool intersect_with_current_clip_rect)
{
	return ImGuiAPI_Visitor::PushClipRect(clip_rect_min, clip_rect_max, intersect_with_current_clip_rect);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PopClipRect_2960189489()
{
	return ImGuiAPI_Visitor::PopClipRect();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetItemDefaultFocus_2960189489()
{
	return ImGuiAPI_Visitor::SetItemDefaultFocus();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetKeyboardFocusHere_4038704236(int offset)
{
	return ImGuiAPI_Visitor::SetKeyboardFocusHere(offset);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemHovered_2491699375(ImGuiHoveredFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemHovered(flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemActive_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemActive();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemFocused_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemFocused();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemClicked_361246070(ImGuiMouseButton_ mouse_button)
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemClicked(mouse_button);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemDoubleClicked_361246070(ImGuiMouseButton_ mouse_button)
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemDoubleClicked(mouse_button);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemVisible_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemVisible();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemEdited_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemEdited();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemActivated_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemActivated();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemDeactivated_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemDeactivated();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemDeactivatedAfterEdit_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemDeactivatedAfterEdit();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsItemToggledOpen_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsItemToggledOpen();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsAnyItemHovered_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsAnyItemHovered();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsAnyItemActive_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsAnyItemActive();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsAnyItemFocused_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsAnyItemFocused();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetItemRectMin_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetItemRectMin();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetItemRectMax_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetItemRectMax();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetItemRectSize_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetItemRectSize();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsRectVisible_4039732974(const ImVec2* size)
{
	auto tmp_result = ImGuiAPI_Visitor::IsRectVisible(size);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsRectVisible_690184979(const ImVec2* rect_min,const ImVec2* rect_max)
{
	auto tmp_result = ImGuiAPI_Visitor::IsRectVisible(rect_min, rect_max);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API double TitanImGui_ImGuiAPI_Visitor_GetTime_4162959082()
{
	return ImGuiAPI_Visitor::GetTime();
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_GetFrameCount_2704135706()
{
	return ImGuiAPI_Visitor::GetFrameCount();
}
extern "C" VFX_API ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetBackgroundDrawList_2196389917()
{
	return ImGuiAPI_Visitor::GetBackgroundDrawList();
}
extern "C" VFX_API ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetForegroundDrawList_2196389917()
{
	return ImGuiAPI_Visitor::GetForegroundDrawList();
}
extern "C" VFX_API ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetBackgroundDrawList_1456041080(ImGuiViewport* viewport)
{
	return ImGuiAPI_Visitor::GetBackgroundDrawList(viewport);
}
extern "C" VFX_API ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetForegroundDrawList_1456041080(ImGuiViewport* viewport)
{
	return ImGuiAPI_Visitor::GetForegroundDrawList(viewport);
}
extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_GetDrawListSharedData_302642963()
{
	return ImGuiAPI_Visitor::GetDrawListSharedData();
}
extern "C" VFX_API char* TitanImGui_ImGuiAPI_Visitor_GetStyleColorName_1027210462(ImGuiCol_ idx)
{
	return ImGuiAPI_Visitor::GetStyleColorName(idx);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetStateStorage_1198396241(ImGuiStorage* storage)
{
	return ImGuiAPI_Visitor::SetStateStorage(storage);
}
extern "C" VFX_API ImGuiStorage* TitanImGui_ImGuiAPI_Visitor_GetStateStorage_479204201()
{
	return ImGuiAPI_Visitor::GetStateStorage();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetNextItemAllowOverlap_2960189489()
{
	return ImGuiAPI_Visitor::SetNextItemAllowOverlap();
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_CalcTextSize_2234397086(const char* text,bool hide_text_after_double_hash,float wrap_width)
{
	auto tmp_result = ImGuiAPI_Visitor::CalcTextSize(text, hide_text_after_double_hash, wrap_width);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector4_t TitanImGui_ImGuiAPI_Visitor_ColorConvertU32ToFloat4_4044967189(unsigned int inValue)
{
	auto tmp_result = ImGuiAPI_Visitor::ColorConvertU32ToFloat4(inValue);
	return EngineNS::VReturnValueMarshal<ImVec4,v3dVector4_t>(tmp_result);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_ColorConvertFloat4ToU32_2189523068(const ImVec4* inValue)
{
	return ImGuiAPI_Visitor::ColorConvertFloat4ToU32(inValue);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ColorConvertRGBtoHSV_3904097195(float r,float g,float b,float* out_h,float* out_s,float* out_v)
{
	return ImGuiAPI_Visitor::ColorConvertRGBtoHSV(r, g, b, out_h, out_s, out_v);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ColorConvertHSVtoRGB_3904097195(float h,float s,float v,float* out_r,float* out_g,float* out_b)
{
	return ImGuiAPI_Visitor::ColorConvertHSVtoRGB(h, s, v, out_r, out_g, out_b);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsKeyDown_664544503(ImGuiKey user_key_index)
{
	auto tmp_result = ImGuiAPI_Visitor::IsKeyDown(user_key_index);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsKeyPressed_1790704165(ImGuiKey user_key_index,bool repeat)
{
	auto tmp_result = ImGuiAPI_Visitor::IsKeyPressed(user_key_index, repeat);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsKeyReleased_664544503(ImGuiKey user_key_index)
{
	auto tmp_result = ImGuiAPI_Visitor::IsKeyReleased(user_key_index);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_GetKeyPressedAmount_1868481138(ImGuiKey key_index,float repeat_delay,float rate)
{
	return ImGuiAPI_Visitor::GetKeyPressedAmount(key_index, repeat_delay, rate);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseDown_361246070(ImGuiMouseButton_ button)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseDown(button);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseClicked_3257368754(ImGuiMouseButton_ button,bool repeat)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseClicked(button, repeat);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseReleased_361246070(ImGuiMouseButton_ button)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseReleased(button);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseDoubleClicked_361246070(ImGuiMouseButton_ button)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseDoubleClicked(button);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_GetMouseClickedCount_3860407425(ImGuiMouseButton_ button)
{
	return ImGuiAPI_Visitor::GetMouseClickedCount(button);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseHoveringRect_1978284909(const ImVec2* r_min,const ImVec2* r_max,bool clip)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseHoveringRect(r_min, r_max, clip);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMousePosValid_4039732974(const ImVec2* mouse_pos)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMousePosValid(mouse_pos);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsAnyMouseDown_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsAnyMouseDown();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetMousePos_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetMousePos();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetMousePosOnOpeningCurrentPopup_558510083()
{
	auto tmp_result = ImGuiAPI_Visitor::GetMousePosOnOpeningCurrentPopup();
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseDragging_2505297786(ImGuiMouseButton_ button,float lock_threshold)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseDragging(button, lock_threshold);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_GetMouseDragDelta_4262098684(ImGuiMouseButton_ button,float lock_threshold)
{
	auto tmp_result = ImGuiAPI_Visitor::GetMouseDragDelta(button, lock_threshold);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ResetMouseDragDelta_269877056(ImGuiMouseButton_ button)
{
	return ImGuiAPI_Visitor::ResetMouseDragDelta(button);
}
extern "C" VFX_API ImGuiMouseCursor_ TitanImGui_ImGuiAPI_Visitor_GetMouseCursor_345306920()
{
	return ImGuiAPI_Visitor::GetMouseCursor();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetMouseCursor_3384190518(ImGuiMouseCursor_ cursor_type)
{
	return ImGuiAPI_Visitor::SetMouseCursor(cursor_type);
}
extern "C" VFX_API char* TitanImGui_ImGuiAPI_Visitor_GetClipboardText_2396230038()
{
	return ImGuiAPI_Visitor::GetClipboardText();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetClipboardText_2602414842(const char* text)
{
	return ImGuiAPI_Visitor::SetClipboardText(text);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LoadIniSettingsFromDisk_2602414842(const char* ini_filename)
{
	return ImGuiAPI_Visitor::LoadIniSettingsFromDisk(ini_filename);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_LoadIniSettingsFromMemory_3999832562(const char* ini_data,unsigned int ini_size)
{
	return ImGuiAPI_Visitor::LoadIniSettingsFromMemory(ini_data, ini_size);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SaveIniSettingsToDisk_2602414842(const char* ini_filename)
{
	return ImGuiAPI_Visitor::SaveIniSettingsToDisk(ini_filename);
}
extern "C" VFX_API char* TitanImGui_ImGuiAPI_Visitor_SaveIniSettingsToMemory_814920808(unsigned int* out_ini_size)
{
	return ImGuiAPI_Visitor::SaveIniSettingsToMemory(out_ini_size);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DebugCheckVersionAndDataLayout_204246052(const char* version_str,size_t sz_io,size_t sz_style,size_t sz_vec2,size_t sz_vec4,size_t sz_drawvert,size_t sz_drawidx)
{
	auto tmp_result = ImGuiAPI_Visitor::DebugCheckVersionAndDataLayout(version_str, sz_io, sz_style, sz_vec2, sz_vec4, sz_drawvert, sz_drawidx);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetAllocatorFunctions_3510213992(void *(*fn_alloc_func)(size_t, void *),void (*fn_free_func)(void *, void *),void* user_data)
{
	return ImGuiAPI_Visitor::SetAllocatorFunctions(fn_alloc_func, fn_free_func, user_data);
}
extern "C" VFX_API void* TitanImGui_ImGuiAPI_Visitor_MemAlloc_1539568017(size_t size)
{
	return ImGuiAPI_Visitor::MemAlloc(size);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_MemFree_3034592143(void* ptr)
{
	return ImGuiAPI_Visitor::MemFree(ptr);
}
extern "C" VFX_API ImGuiPlatformIO* TitanImGui_ImGuiAPI_Visitor_GetPlatformIO_971686321()
{
	return ImGuiAPI_Visitor::GetPlatformIO();
}
extern "C" VFX_API ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_GetMainViewport_4006837304()
{
	return ImGuiAPI_Visitor::GetMainViewport();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_UpdatePlatformWindows_2960189489()
{
	return ImGuiAPI_Visitor::UpdatePlatformWindows();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_RenderPlatformWindowsDefault_760721789(void* platform_render_arg,void* renderer_render_arg)
{
	return ImGuiAPI_Visitor::RenderPlatformWindowsDefault(platform_render_arg, renderer_render_arg);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_DestroyPlatformWindows_2960189489()
{
	return ImGuiAPI_Visitor::DestroyPlatformWindows();
}
extern "C" VFX_API ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_FindViewportByID_1024491770(unsigned int id)
{
	return ImGuiAPI_Visitor::FindViewportByID(id);
}
extern "C" VFX_API ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_FindViewportByPlatformHandle_4148205652(void* platform_handle)
{
	return ImGuiAPI_Visitor::FindViewportByPlatformHandle(platform_handle);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_CreateWindow_3699537699(ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *))
{
	return ImGuiAPI_Visitor::Set_Renderer_CreateWindow(PlatformIO, fn);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_DestroyWindow_1336333681(ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *))
{
	return ImGuiAPI_Visitor::Set_Renderer_DestroyWindow(PlatformIO, fn);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_SetWindowSize_3092220344(ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *, ImVec2))
{
	return ImGuiAPI_Visitor::Set_Renderer_SetWindowSize(PlatformIO, fn);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_RenderWindow_2851598185(ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *, void *))
{
	return ImGuiAPI_Visitor::Set_Renderer_RenderWindow(PlatformIO, fn);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_SwapBuffers_593121533(ImGuiPlatformIO* PlatformIO,void (*fn)(ImGuiViewport *, void *))
{
	return ImGuiAPI_Visitor::Set_Renderer_SwapBuffers(PlatformIO, fn);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PlatformIO_Monitor_Resize_1567199252(ImGuiPlatformIO* io,int size)
{
	return ImGuiAPI_Visitor::PlatformIO_Monitor_Resize(io, size);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_PlatformIO_Monitor_PushBack_1937089321(ImGuiPlatformIO* io,ImGuiPlatformMonitor monitor)
{
	return ImGuiAPI_Visitor::PlatformIO_Monitor_PushBack(io, monitor);
}
extern "C" VFX_API int TitanImGui_ImGuiAPI_Visitor_PlatformIO_Viewports_Size_2566682924(ImGuiPlatformIO* io)
{
	return ImGuiAPI_Visitor::PlatformIO_Viewports_Size(io);
}
extern "C" VFX_API ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_PlatformIO_Viewports_Get_2057355863(ImGuiPlatformIO* io,int index)
{
	return ImGuiAPI_Visitor::PlatformIO_Viewports_Get(io, index);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TextInputComboBox_899144233(const char* id,void* buffer,unsigned int maxInputSize,const char** items,unsigned int item_len,short showMaxItems)
{
	auto tmp_result = ImGuiAPI_Visitor::TextInputComboBox(id, buffer, maxInputSize, items, item_len, showMaxItems);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_GetClipboardTextSetter_881800347(ImGuiIO* io,const char *(*fn)(void *))
{
	return ImGuiAPI_Visitor::GetClipboardTextSetter(io, fn);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetClipboardTextSetter_716060207(ImGuiIO* io,void (*fn)(void *, const char *))
{
	return ImGuiAPI_Visitor::SetClipboardTextSetter(io, fn);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ItemSize_1990263409(const ImVec2* min,const ImVec2* max,float text_baseline_y)
{
	return ImGuiAPI_Visitor::ItemSize(min, max, text_baseline_y);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ItemSize_2116332084(const ImVec2* size,float text_baseline_y)
{
	return ImGuiAPI_Visitor::ItemSize(size, text_baseline_y);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ItemAdd_1999737362(const ImVec2* bbMin,const ImVec2* bbMax,unsigned int id,const ImVec2* nav_bb_min,const ImVec2* nav_bb_max,int flags)
{
	auto tmp_result = ImGuiAPI_Visitor::ItemAdd(bbMin, bbMax, id, nav_bb_min, nav_bb_max, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ItemAdd_3017646930(const ImVec2* bbMin,const ImVec2* bbMax,unsigned int id,int flags)
{
	auto tmp_result = ImGuiAPI_Visitor::ItemAdd(bbMin, bbMax, id, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_GetTableWorkRect_2631331155(ImVec2* min,ImVec2* max)
{
	auto tmp_result = ImGuiAPI_Visitor::GetTableWorkRect(min, max);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_GetTableRowStartY_2989815025(float* yValue)
{
	auto tmp_result = ImGuiAPI_Visitor::GetTableRowStartY(yValue);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_GetTableRowEndY_2989815025(float* yValue)
{
	auto tmp_result = ImGuiAPI_Visitor::GetTableRowEndY(yValue);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsHoverCurrentWindow_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsHoverCurrentWindow();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseHoveringRectInCurrentWindow_1978284909(const ImVec2* r_min,const ImVec2* r_max,bool clip)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseHoveringRectInCurrentWindow(r_min, r_max, clip);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseDownInRectInCurrentWindow_3682308850(const ImVec2* r_min,const ImVec2* r_max,ImGuiMouseButton_ button,bool clip)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseDownInRectInCurrentWindow(r_min, r_max, button, clip);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseClickedInRectInCurrentWindow_3682308850(const ImVec2* r_min,const ImVec2* r_max,ImGuiMouseButton_ button,bool clip)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseClickedInRectInCurrentWindow(r_min, r_max, button, clip);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseDoubleClickedInRectInCurrentWindow_3682308850(const ImVec2* r_min,const ImVec2* r_max,ImGuiMouseButton_ button,bool clip)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseDoubleClickedInRectInCurrentWindow(r_min, r_max, button, clip);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsMouseDragPastThreshold_2505297786(ImGuiMouseButton_ button,float lock_threshold)
{
	auto tmp_result = ImGuiAPI_Visitor::IsMouseDragPastThreshold(button, lock_threshold);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsCurrentWindowSkipItems_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsCurrentWindowSkipItems();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ItemHoverable_1371559414(const ImVec2* bbMin,const ImVec2* bbMax,unsigned int id,int item_flags)
{
	auto tmp_result = ImGuiAPI_Visitor::ItemHoverable(bbMin, bbMax, id, item_flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_TempInputIsActive_900990169(unsigned int id)
{
	auto tmp_result = ImGuiAPI_Visitor::TempInputIsActive(id);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetActiveID_2252480719(unsigned int id)
{
	return ImGuiAPI_Visitor::SetActiveID(id);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetActiveID_3529484159()
{
	return ImGuiAPI_Visitor::GetActiveID();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetFocusID_2252480719(unsigned int id)
{
	return ImGuiAPI_Visitor::SetFocusID(id);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetTempInputID_2252480719(unsigned int id)
{
	return ImGuiAPI_Visitor::SetTempInputID(id);
}
extern "C" VFX_API unsigned int TitanImGui_ImGuiAPI_Visitor_GetTempInputID_3529484159()
{
	return ImGuiAPI_Visitor::GetTempInputID();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_ClearActiveID_2960189489()
{
	return ImGuiAPI_Visitor::ClearActiveID();
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_FocusCurrentWindow_2960189489()
{
	return ImGuiAPI_Visitor::FocusCurrentWindow();
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsIDNavActivated_900990169(unsigned int id)
{
	auto tmp_result = ImGuiAPI_Visitor::IsIDNavActivated(id);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsIDNavInput_900990169(unsigned int id)
{
	auto tmp_result = ImGuiAPI_Visitor::IsIDNavInput(id);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragBehavior_310431169(unsigned int id,int data_type,void* p_v,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragBehavior(id, data_type, p_v, v_speed, p_min, p_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragActiveIdUpdate_900990169(unsigned int id)
{
	auto tmp_result = ImGuiAPI_Visitor::DragActiveIdUpdate(id);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ButtonBehavior_1167781018(const ImVec2* min,const ImVec2* max,unsigned int id,bool* out_hovered,bool* out_held,bool pressOnRelease,ImGuiButtonFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::ButtonBehavior(min, max, id, out_hovered, out_held, pressOnRelease, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragScalar2_220901116(const char* label,int data_type,void* p_data,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragScalar2(label, data_type, p_data, v_speed, p_min, p_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_DragScalarN2_3862891275(const char* label,int data_type,void* p_data,int components,float v_speed,const void* p_min,const void* p_max,const char* format,ImGuiSliderFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::DragScalarN2(label, data_type, p_data, components, v_speed, p_min, p_max, format, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_RenderFrame_539721179(ImVec2* p_min,ImVec2* p_max,unsigned int fill_col,bool border,float rounding)
{
	return ImGuiAPI_Visitor::RenderFrame(p_min, p_max, fill_col, border, rounding);
}
extern "C" VFX_API v3dVector2_t TitanImGui_ImGuiAPI_Visitor_CalcItemSize_3095670423(ImVec2* size,float default_w,float default_h)
{
	auto tmp_result = ImGuiAPI_Visitor::CalcItemSize(size, default_w, default_h);
	return EngineNS::VReturnValueMarshal<ImVec2,v3dVector2_t>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_SpanAllColumns_2036359645(const char* label,ImGuiTreeNodeFlags_ flags)
{
	auto tmp_result = ImGuiAPI_Visitor::CollapsingHeader_SpanAllColumns(label, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsInTable_1117990983()
{
	auto tmp_result = ImGuiAPI_Visitor::IsInTable();
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableNextRow_589053033(const EngineNS::ImGuiTableRowData* rowData)
{
	return ImGuiAPI_Visitor::TableNextRow(rowData);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_TableNextRow_FirstColumn_589053033(const EngineNS::ImGuiTableRowData* rowData)
{
	return ImGuiAPI_Visitor::TableNextRow_FirstColumn(rowData);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_CheckBoxTristate_263170625(const char* label,int* v_tristate)
{
	auto tmp_result = ImGuiAPI_Visitor::CheckBoxTristate(label, v_tristate);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_ToggleButton_853224535(const char* label,bool* v,const ImVec2* size_arg,int flags)
{
	auto tmp_result = ImGuiAPI_Visitor::ToggleButton(label, v, size_arg, flags);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_SetKeyOwner_2771753469(ImGuiKey key,unsigned int owner_id,int flags)
{
	return ImGuiAPI_Visitor::SetKeyOwner(key, owner_id, flags);
}
extern "C" VFX_API void TitanImGui_ImGuiAPI_Visitor_MakeTabVisible_2602414842(const char* window_name)
{
	return ImGuiAPI_Visitor::MakeTabVisible(window_name);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsFirstFrame_1080422500(const char* window_name)
{
	auto tmp_result = ImGuiAPI_Visitor::IsFirstFrame(window_name);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API char TitanImGui_ImGuiAPI_Visitor_IsLastFrame_1080422500(const char* window_name)
{
	auto tmp_result = ImGuiAPI_Visitor::IsLastFrame(window_name);
	return EngineNS::VReturnValueMarshal<bool,char>(tmp_result);
}
extern "C" VFX_API ImFont* TitanImGui_ImGuiAPI_Visitor_GetDrawListFont_4089666882(ImDrawList* drawList)
{
	return ImGuiAPI_Visitor::GetDrawListFont(drawList);
}
extern "C" VFX_API float TitanImGui_ImGuiAPI_Visitor_GetDrawListFontSize_1773646793(ImDrawList* drawList)
{
	return ImGuiAPI_Visitor::GetDrawListFontSize(drawList);
}
#endif//HasModule_ImGui
