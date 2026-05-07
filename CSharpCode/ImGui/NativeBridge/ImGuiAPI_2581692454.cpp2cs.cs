//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


namespace EngineNS
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 1, Pack = 1)]
	public unsafe partial struct ImGuiAPI : EngineNS.IPtrType
	{
		#region StructLayout
		#endregion
		public IntPtr NativePointer { get => IntPtr.Zero; set {} }
		#region Constructor&Cast
		public void UnsafeCallDestructor()
		{
			fixed (ImGuiAPI* mPointer = &this)
			{
				TitanImGui_ImGuiAPI_Visitor_UnsafeCallDestructor(mPointer);
			}
		}
		#endregion
		#region Fields
		#endregion
		#region Function
		public static void* CreateContext(ImFontAtlas shared_font_atlas)
		{
			return TitanImGui_ImGuiAPI_Visitor_CreateContext_3448116237(shared_font_atlas);
		}
		public static void DestroyContext(void* ctx)
		{
			TitanImGui_ImGuiAPI_Visitor_DestroyContext_3034592143(ctx);
		}
		public static void* GetCurrentContext()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetCurrentContext_302642963();
		}
		public static void SetCurrentContext(void* ctx)
		{
			TitanImGui_ImGuiAPI_Visitor_SetCurrentContext_3034592143(ctx);
		}
		public static void SetIniFilename(string ini_filename)
		{
			TitanImGui_ImGuiAPI_Visitor_SetIniFilename_2602414842(ini_filename);
		}
		public static ImGuiIO GetIO()
		{
			return new ImGuiIO(TitanImGui_ImGuiAPI_Visitor_GetIO_289112634());
		}
		public static ImGuiStyle* GetStyle()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetStyle_1766641991();
		}
		public static void NewFrame()
		{
			TitanImGui_ImGuiAPI_Visitor_NewFrame_2960189489();
		}
		public static void EndFrame()
		{
			TitanImGui_ImGuiAPI_Visitor_EndFrame_2960189489();
		}
		public static void Render()
		{
			TitanImGui_ImGuiAPI_Visitor_Render_2960189489();
		}
		public static ImDrawData* GetDrawData()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetDrawData_2577559959();
		}
		public static int DrawData_Textures_Size(ImDrawData* draw_data)
		{
			return TitanImGui_ImGuiAPI_Visitor_DrawData_Textures_Size_2524852328(draw_data);
		}
		public static int DrawData_Textures_Size( ref ImDrawData draw_data)
		{
			fixed(ImDrawData* pinned_draw_data = &draw_data)
			{
				return DrawData_Textures_Size(pinned_draw_data);
			}
		}
		public static void* DrawData_Textures_Get(ImDrawData* draw_data,int index)
		{
			return TitanImGui_ImGuiAPI_Visitor_DrawData_Textures_Get_3628729028(draw_data, index);
		}
		public static void* DrawData_Textures_Get( ref ImDrawData draw_data,int index)
		{
			fixed(ImDrawData* pinned_draw_data = &draw_data)
			{
				return DrawData_Textures_Get(pinned_draw_data, index);
			}
		}
		public static int PlatformIO_Textures_Size(ImGuiPlatformIO io)
		{
			return TitanImGui_ImGuiAPI_Visitor_PlatformIO_Textures_Size_2566682924(io);
		}
		public static void* PlatformIO_Textures_Get(ImGuiPlatformIO io,int index)
		{
			return TitanImGui_ImGuiAPI_Visitor_PlatformIO_Textures_Get_955167742(io, index);
		}
		public static int ImTextureData_GetStatus(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetStatus_1896157666(texture_ptr);
		}
		public static void ImTextureData_SetStatus(void* texture_ptr,int status)
		{
			TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetStatus_3475646452(texture_ptr, status);
		}
		public static void* ImTextureData_GetBackendUserData(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetBackendUserData_1752491917(texture_ptr);
		}
		public static void ImTextureData_SetBackendUserData(void* texture_ptr,void* backend_user_data)
		{
			TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetBackendUserData_760721789(texture_ptr, backend_user_data);
		}
		public static ulong ImTextureData_GetTexID(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetTexID_2855664449(texture_ptr);
		}
		public static void ImTextureData_SetTexID(void* texture_ptr,ulong tex_id)
		{
			TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetTexID_3373007845(texture_ptr, tex_id);
		}
		public static int ImTextureData_GetFormat(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetFormat_1896157666(texture_ptr);
		}
		public static int ImTextureData_GetWidth(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetWidth_1896157666(texture_ptr);
		}
		public static int ImTextureData_GetHeight(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetHeight_1896157666(texture_ptr);
		}
		public static int ImTextureData_GetBytesPerPixel(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetBytesPerPixel_1896157666(texture_ptr);
		}
		public static int ImTextureData_GetPitch(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPitch_1896157666(texture_ptr);
		}
		public static int ImTextureData_GetSizeInBytes(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetSizeInBytes_1896157666(texture_ptr);
		}
		public static void* ImTextureData_GetPixels(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPixels_1752491917(texture_ptr);
		}
		public static void* ImTextureData_GetPixelsAt(void* texture_ptr,int x,int y)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPixelsAt_1414832493(texture_ptr, x, y);
		}
		public static int ImTextureData_GetUpdatesSize(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetUpdatesSize_1896157666(texture_ptr);
		}
		public static int ImTextureData_GetUnusedFrames(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetUnusedFrames_1896157666(texture_ptr);
		}
		public static bool ImTextureData_GetWantDestroyNextFrame(void* texture_ptr)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetWantDestroyNextFrame_342119689(texture_ptr) == 0 ? false : true;
		}
		public static string ImTextureData_GetStatusName(int status)
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetStatusName_4173149781(status));
		}
		public static string ImTextureData_GetFormatName(int format)
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetFormatName_4173149781(format));
		}
		public static void ShowDemoWindow(bool* p_open)
		{
			TitanImGui_ImGuiAPI_Visitor_ShowDemoWindow_1193269193(p_open);
		}
		public static void ShowDemoWindow( ref bool p_open)
		{
			fixed(bool* pinned_p_open = &p_open)
			{
				ShowDemoWindow(pinned_p_open);
			}
		}
		public static void ShowAboutWindow(bool* p_open)
		{
			TitanImGui_ImGuiAPI_Visitor_ShowAboutWindow_1193269193(p_open);
		}
		public static void ShowAboutWindow( ref bool p_open)
		{
			fixed(bool* pinned_p_open = &p_open)
			{
				ShowAboutWindow(pinned_p_open);
			}
		}
		public static void ShowMetricsWindow(bool* p_open)
		{
			TitanImGui_ImGuiAPI_Visitor_ShowMetricsWindow_1193269193(p_open);
		}
		public static void ShowMetricsWindow( ref bool p_open)
		{
			fixed(bool* pinned_p_open = &p_open)
			{
				ShowMetricsWindow(pinned_p_open);
			}
		}
		public static void ShowStyleEditor(ImGuiStyle* refValue)
		{
			TitanImGui_ImGuiAPI_Visitor_ShowStyleEditor_3987680467(refValue);
		}
		public static void ShowStyleEditor( ref ImGuiStyle refValue)
		{
			fixed(ImGuiStyle* pinned_refValue = &refValue)
			{
				ShowStyleEditor(pinned_refValue);
			}
		}
		public static bool ShowStyleSelector(string label)
		{
			return TitanImGui_ImGuiAPI_Visitor_ShowStyleSelector_1080422500(label) == 0 ? false : true;
		}
		public static void ShowFontSelector(string label)
		{
			TitanImGui_ImGuiAPI_Visitor_ShowFontSelector_2602414842(label);
		}
		public static void ShowUserGuide()
		{
			TitanImGui_ImGuiAPI_Visitor_ShowUserGuide_2960189489();
		}
		public static string GetVersion()
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiAPI_Visitor_GetVersion_2396230038());
		}
		public static void StyleColorsDark(ImGuiStyle* dst)
		{
			TitanImGui_ImGuiAPI_Visitor_StyleColorsDark_3987680467(dst);
		}
		public static void StyleColorsDark( ref ImGuiStyle dst)
		{
			fixed(ImGuiStyle* pinned_dst = &dst)
			{
				StyleColorsDark(pinned_dst);
			}
		}
		public static void StyleColorsClassic(ImGuiStyle* dst)
		{
			TitanImGui_ImGuiAPI_Visitor_StyleColorsClassic_3987680467(dst);
		}
		public static void StyleColorsClassic( ref ImGuiStyle dst)
		{
			fixed(ImGuiStyle* pinned_dst = &dst)
			{
				StyleColorsClassic(pinned_dst);
			}
		}
		public static void StyleColorsLight(ImGuiStyle* dst)
		{
			TitanImGui_ImGuiAPI_Visitor_StyleColorsLight_3987680467(dst);
		}
		public static void StyleColorsLight( ref ImGuiStyle dst)
		{
			fixed(ImGuiStyle* pinned_dst = &dst)
			{
				StyleColorsLight(pinned_dst);
			}
		}
		public static bool Begin(string name,bool* p_open,ImGuiWindowFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_Begin_2979558607(name, p_open, flags) == 0 ? false : true;
		}
		public static bool Begin(string name, ref bool p_open,ImGuiWindowFlags_ flags)
		{
			fixed(bool* pinned_p_open = &p_open)
			{
				return Begin(name, pinned_p_open, flags);
			}
		}
		public static void End()
		{
			TitanImGui_ImGuiAPI_Visitor_End_2960189489();
		}
		public static bool BeginChild(string str_id,EngineNS.Vector2* size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginChild_2739961343(str_id, size, child_flags, flags) == 0 ? false : true;
		}
		public static bool BeginChild(string str_id, in EngineNS.Vector2 size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return BeginChild(str_id, pinned_size, child_flags, flags);
			}
		}
		public static bool BeginChild(uint id,EngineNS.Vector2* size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginChild_2598737060(id, size, child_flags, flags) == 0 ? false : true;
		}
		public static bool BeginChild(uint id, in EngineNS.Vector2 size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return BeginChild(id, pinned_size, child_flags, flags);
			}
		}
		public static void EndChild()
		{
			TitanImGui_ImGuiAPI_Visitor_EndChild_2960189489();
		}
		public static bool IsWindowAppearing()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsWindowAppearing_1117990983() == 0 ? false : true;
		}
		public static bool IsWindowCollapsed()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsWindowCollapsed_1117990983() == 0 ? false : true;
		}
		public static bool IsWindowFocused(ImGuiFocusedFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsWindowFocused_1151558301(flags) == 0 ? false : true;
		}
		public static bool IsWindowHovered(ImGuiHoveredFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsWindowHovered_2491699375(flags) == 0 ? false : true;
		}
		public static ImDrawList GetWindowDrawList()
		{
			return new ImDrawList(TitanImGui_ImGuiAPI_Visitor_GetWindowDrawList_2196389917());
		}
		public static float GetWindowDpiScale()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowDpiScale_3743936629();
		}
		public static ImGuiViewport* GetWindowViewport()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowViewport_4006837304();
		}
		public static EngineNS.Vector2 GetWindowPos()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowPos_558510083();
		}
		public static EngineNS.Vector2 GetWindowSize()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowSize_558510083();
		}
		public static float GetWindowWidth()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowWidth_3743936629();
		}
		public static float GetWindowHeight()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowHeight_3743936629();
		}
		public static void SetNextWindowPos(EngineNS.Vector2* pos,ImGuiCond_ cond,EngineNS.Vector2* pivot)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowPos_1923501243(pos, cond, pivot);
		}
		public static void SetNextWindowPos( in EngineNS.Vector2 pos,ImGuiCond_ cond, in EngineNS.Vector2 pivot)
		{
			fixed(EngineNS.Vector2* pinned_pos = &pos)
			fixed(EngineNS.Vector2* pinned_pivot = &pivot)
			{
				SetNextWindowPos(pinned_pos, cond, pinned_pivot);
			}
		}
		public static void SetNextWindowSize(EngineNS.Vector2* size,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowSize_1791316118(size, cond);
		}
		public static void SetNextWindowSize( in EngineNS.Vector2 size,ImGuiCond_ cond)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				SetNextWindowSize(pinned_size, cond);
			}
		}
		public unsafe delegate void FDelegate_ImGuiSizeCallback(ImGuiSizeCallbackData* arg0);
		public static void SetNextWindowSizeConstraints(EngineNS.Vector2* size_min,EngineNS.Vector2* size_max,FDelegate_ImGuiSizeCallback custom_callback,void* custom_callback_data)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowSizeConstraints_684930918(size_min, size_max, custom_callback, custom_callback_data);
		}
		public static void SetNextWindowSizeConstraints( in EngineNS.Vector2 size_min, in EngineNS.Vector2 size_max,FDelegate_ImGuiSizeCallback custom_callback,void* custom_callback_data)
		{
			fixed(EngineNS.Vector2* pinned_size_min = &size_min)
			fixed(EngineNS.Vector2* pinned_size_max = &size_max)
			{
				SetNextWindowSizeConstraints(pinned_size_min, pinned_size_max, custom_callback, custom_callback_data);
			}
		}
		public static void SetNextWindowContentSize(EngineNS.Vector2* size)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowContentSize_4055394152(size);
		}
		public static void SetNextWindowContentSize( in EngineNS.Vector2 size)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				SetNextWindowContentSize(pinned_size);
			}
		}
		public static void SetNextWindowCollapsed(bool collapsed,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowCollapsed_3341245801(collapsed, cond);
		}
		public static void SetNextWindowFocus()
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowFocus_2960189489();
		}
		public static void SetNextWindowBgAlpha(float alpha)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowBgAlpha_1759962673(alpha);
		}
		public static void SetNextWindowViewport(uint viewport_id)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowViewport_2252480719(viewport_id);
		}
		public static void SetWindowPos(EngineNS.Vector2* pos,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowPos_1791316118(pos, cond);
		}
		public static void SetWindowPos( in EngineNS.Vector2 pos,ImGuiCond_ cond)
		{
			fixed(EngineNS.Vector2* pinned_pos = &pos)
			{
				SetWindowPos(pinned_pos, cond);
			}
		}
		public static void SetWindowSize(EngineNS.Vector2* size,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowSize_1791316118(size, cond);
		}
		public static void SetWindowSize( in EngineNS.Vector2 size,ImGuiCond_ cond)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				SetWindowSize(pinned_size, cond);
			}
		}
		public static void SetWindowCollapsed(bool collapsed,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowCollapsed_3341245801(collapsed, cond);
		}
		public static void SetWindowFocus()
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowFocus_2960189489();
		}
		public static void SetWindowFocus(string name)
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowFocus_2602414842(name);
		}
		public static void SetWindowFontScale(float scale)
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowFontScale_1759962673(scale);
		}
		public static void SetWindowPos(string name,EngineNS.Vector2* pos,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowPos_1093786109(name, pos, cond);
		}
		public static void SetWindowPos(string name, in EngineNS.Vector2 pos,ImGuiCond_ cond)
		{
			fixed(EngineNS.Vector2* pinned_pos = &pos)
			{
				SetWindowPos(name, pinned_pos, cond);
			}
		}
		public static void SetWindowSize(string name,EngineNS.Vector2* size,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowSize_1093786109(name, size, cond);
		}
		public static void SetWindowSize(string name, in EngineNS.Vector2 size,ImGuiCond_ cond)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				SetWindowSize(name, pinned_size, cond);
			}
		}
		public static void SetWindowCollapsed(string name,bool collapsed,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetWindowCollapsed_3917756216(name, collapsed, cond);
		}
		public static EngineNS.Vector2 GetContentRegionMax()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetContentRegionMax_558510083();
		}
		public static EngineNS.Vector2 GetContentRegionAvail()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetContentRegionAvail_558510083();
		}
		public static EngineNS.Vector2 GetWindowContentRegionMin()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionMin_558510083();
		}
		public static EngineNS.Vector2 GetWindowContentRegionMax()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionMax_558510083();
		}
		public static float GetWindowContentRegionWidth()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionWidth_3743936629();
		}
		public static float GetScrollX()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetScrollX_3743936629();
		}
		public static float GetScrollY()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetScrollY_3743936629();
		}
		public static float GetScrollMaxX()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetScrollMaxX_3743936629();
		}
		public static float GetScrollMaxY()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetScrollMaxY_3743936629();
		}
		public static void SetScrollX(float scroll_x)
		{
			TitanImGui_ImGuiAPI_Visitor_SetScrollX_1759962673(scroll_x);
		}
		public static void SetScrollY(float scroll_y)
		{
			TitanImGui_ImGuiAPI_Visitor_SetScrollY_1759962673(scroll_y);
		}
		public static void SetScrollHereX(float center_x_ratio)
		{
			TitanImGui_ImGuiAPI_Visitor_SetScrollHereX_1759962673(center_x_ratio);
		}
		public static void SetScrollHereY(float center_y_ratio)
		{
			TitanImGui_ImGuiAPI_Visitor_SetScrollHereY_1759962673(center_y_ratio);
		}
		public static void SetScrollFromPosX(float local_x,float center_x_ratio)
		{
			TitanImGui_ImGuiAPI_Visitor_SetScrollFromPosX_996365349(local_x, center_x_ratio);
		}
		public static void SetScrollFromPosY(float local_y,float center_y_ratio)
		{
			TitanImGui_ImGuiAPI_Visitor_SetScrollFromPosY_996365349(local_y, center_y_ratio);
		}
		public static void PushFont(ImFont font)
		{
			TitanImGui_ImGuiAPI_Visitor_PushFont_2187443828(font);
		}
		public static void PushFontWithSize(ImFont font,float font_size_base_unscaled)
		{
			TitanImGui_ImGuiAPI_Visitor_PushFontWithSize_41148096(font, font_size_base_unscaled);
		}
		public static void PopFont()
		{
			TitanImGui_ImGuiAPI_Visitor_PopFont_2960189489();
		}
		public static void PushStyleColor(ImGuiCol_ idx,uint col)
		{
			TitanImGui_ImGuiAPI_Visitor_PushStyleColor_2781163097(idx, col);
		}
		public static void PushStyleColor(ImGuiCol_ idx,EngineNS.Vector4* col)
		{
			TitanImGui_ImGuiAPI_Visitor_PushStyleColor_506689308(idx, col);
		}
		public static void PushStyleColor(ImGuiCol_ idx, in EngineNS.Vector4 col)
		{
			fixed(EngineNS.Vector4* pinned_col = &col)
			{
				PushStyleColor(idx, pinned_col);
			}
		}
		public static void PopStyleColor(int count)
		{
			TitanImGui_ImGuiAPI_Visitor_PopStyleColor_4038704236(count);
		}
		public static void PushStyleVar(ImGuiStyleVar_ idx,float val)
		{
			TitanImGui_ImGuiAPI_Visitor_PushStyleVar_1680981031(idx, val);
		}
		public static void PushStyleVar(ImGuiStyleVar_ idx,EngineNS.Vector2* val)
		{
			TitanImGui_ImGuiAPI_Visitor_PushStyleVar_3737822352(idx, val);
		}
		public static void PushStyleVar(ImGuiStyleVar_ idx, in EngineNS.Vector2 val)
		{
			fixed(EngineNS.Vector2* pinned_val = &val)
			{
				PushStyleVar(idx, pinned_val);
			}
		}
		public static void PopStyleVar(int count)
		{
			TitanImGui_ImGuiAPI_Visitor_PopStyleVar_4038704236(count);
		}
		public static EngineNS.Vector4* GetStyleColorVec4(ImGuiCol_ idx)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetStyleColorVec4_3399559918(idx);
		}
		public static ImFont GetFont()
		{
			return new ImFont(TitanImGui_ImGuiAPI_Visitor_GetFont_2919724924());
		}
		public static float GetFontSize()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetFontSize_3743936629();
		}
		public static EngineNS.Vector2 GetFontTexUvWhitePixel()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetFontTexUvWhitePixel_558510083();
		}
		public static uint GetColorU32(ImGuiCol_ idx,float alpha_mul)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetColorU32_2992096237(idx, alpha_mul);
		}
		public static uint GetColorU32(EngineNS.Vector4* col)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetColorU32_2189523068(col);
		}
		public static uint GetColorU32( in EngineNS.Vector4 col)
		{
			fixed(EngineNS.Vector4* pinned_col = &col)
			{
				return GetColorU32(pinned_col);
			}
		}
		public static uint GetColorU32(uint col)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetColorU32_194637277(col);
		}
		public static void PushItemWidth(float item_width)
		{
			TitanImGui_ImGuiAPI_Visitor_PushItemWidth_1759962673(item_width);
		}
		public static void PopItemWidth()
		{
			TitanImGui_ImGuiAPI_Visitor_PopItemWidth_2960189489();
		}
		public static void SetNextItemWidth(float item_width)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextItemWidth_1759962673(item_width);
		}
		public static float CalcItemWidth()
		{
			return TitanImGui_ImGuiAPI_Visitor_CalcItemWidth_3743936629();
		}
		public static void PushTextWrapPos(float wrap_local_pos_x)
		{
			TitanImGui_ImGuiAPI_Visitor_PushTextWrapPos_1759962673(wrap_local_pos_x);
		}
		public static void PopTextWrapPos()
		{
			TitanImGui_ImGuiAPI_Visitor_PopTextWrapPos_2960189489();
		}
		public static void PushAllowKeyboardFocus(bool allow_keyboard_focus)
		{
			TitanImGui_ImGuiAPI_Visitor_PushAllowKeyboardFocus_2077628183(allow_keyboard_focus);
		}
		public static void PopAllowKeyboardFocus()
		{
			TitanImGui_ImGuiAPI_Visitor_PopAllowKeyboardFocus_2960189489();
		}
		public static void PushButtonRepeat(bool repeat)
		{
			TitanImGui_ImGuiAPI_Visitor_PushButtonRepeat_2077628183(repeat);
		}
		public static void PopButtonRepeat()
		{
			TitanImGui_ImGuiAPI_Visitor_PopButtonRepeat_2960189489();
		}
		public static void Separator()
		{
			TitanImGui_ImGuiAPI_Visitor_Separator_2960189489();
		}
		public static void SameLine(float offset_from_start_x,float spacing)
		{
			TitanImGui_ImGuiAPI_Visitor_SameLine_996365349(offset_from_start_x, spacing);
		}
		public static void NewLine()
		{
			TitanImGui_ImGuiAPI_Visitor_NewLine_2960189489();
		}
		public static void Spacing()
		{
			TitanImGui_ImGuiAPI_Visitor_Spacing_2960189489();
		}
		public static void Dummy(EngineNS.Vector2* size)
		{
			TitanImGui_ImGuiAPI_Visitor_Dummy_4055394152(size);
		}
		public static void Dummy( in EngineNS.Vector2 size)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				Dummy(pinned_size);
			}
		}
		public static void Indent(float indent_w)
		{
			TitanImGui_ImGuiAPI_Visitor_Indent_1759962673(indent_w);
		}
		public static void Unindent(float indent_w)
		{
			TitanImGui_ImGuiAPI_Visitor_Unindent_1759962673(indent_w);
		}
		public static void BeginGroup()
		{
			TitanImGui_ImGuiAPI_Visitor_BeginGroup_2960189489();
		}
		public static void EndGroup()
		{
			TitanImGui_ImGuiAPI_Visitor_EndGroup_2960189489();
		}
		public static EngineNS.Vector2 GetCursorPos()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetCursorPos_558510083();
		}
		public static float GetCursorPosX()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetCursorPosX_3743936629();
		}
		public static float GetCursorPosY()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetCursorPosY_3743936629();
		}
		public static void SetCursorPos(EngineNS.Vector2* local_pos)
		{
			TitanImGui_ImGuiAPI_Visitor_SetCursorPos_4055394152(local_pos);
		}
		public static void SetCursorPos( in EngineNS.Vector2 local_pos)
		{
			fixed(EngineNS.Vector2* pinned_local_pos = &local_pos)
			{
				SetCursorPos(pinned_local_pos);
			}
		}
		public static void SetCursorPosX(float local_x)
		{
			TitanImGui_ImGuiAPI_Visitor_SetCursorPosX_1759962673(local_x);
		}
		public static void SetCursorPosY(float local_y)
		{
			TitanImGui_ImGuiAPI_Visitor_SetCursorPosY_1759962673(local_y);
		}
		public static EngineNS.Vector2 GetCursorStartPos()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetCursorStartPos_558510083();
		}
		public static EngineNS.Vector2 GetCursorScreenPos()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetCursorScreenPos_558510083();
		}
		public static void SetCursorScreenPos(EngineNS.Vector2* pos)
		{
			TitanImGui_ImGuiAPI_Visitor_SetCursorScreenPos_4055394152(pos);
		}
		public static void SetCursorScreenPos( in EngineNS.Vector2 pos)
		{
			fixed(EngineNS.Vector2* pinned_pos = &pos)
			{
				SetCursorScreenPos(pinned_pos);
			}
		}
		public static void AlignTextToFramePadding()
		{
			TitanImGui_ImGuiAPI_Visitor_AlignTextToFramePadding_2960189489();
		}
		public static float GetTextLineHeight()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetTextLineHeight_3743936629();
		}
		public static float GetTextLineHeightWithSpacing()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetTextLineHeightWithSpacing_3743936629();
		}
		public static float GetFrameHeight()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetFrameHeight_3743936629();
		}
		public static float GetFrameHeightWithSpacing()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetFrameHeightWithSpacing_3743936629();
		}
		public static void PushID(string str_id)
		{
			TitanImGui_ImGuiAPI_Visitor_PushID_2602414842(str_id);
		}
		public static void PushID(string str_id_begin,string str_id_end)
		{
			TitanImGui_ImGuiAPI_Visitor_PushID_568371421(str_id_begin, str_id_end);
		}
		public static void PushID(void* ptr_id)
		{
			TitanImGui_ImGuiAPI_Visitor_PushID_1819065180(ptr_id);
		}
		public static void PushID(int int_id)
		{
			TitanImGui_ImGuiAPI_Visitor_PushID_4038704236(int_id);
		}
		public static void PopID()
		{
			TitanImGui_ImGuiAPI_Visitor_PopID_2960189489();
		}
		public static uint GetID(string str_id)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetID_1084213664(str_id);
		}
		public static uint GetID(string str_id_begin,string str_id_end)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetID_2265815331(str_id_begin, str_id_end);
		}
		public static uint GetID(void* ptr_id)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetID_2215092506(ptr_id);
		}
		public static void TextUnformatted(string text)
		{
			TitanImGui_ImGuiAPI_Visitor_TextUnformatted_2602414842(text);
		}
		public static void TextAsPointer(sbyte* fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_TextAsPointer_2602414842(fmt);
		}
		public static void TextAsPointer( in sbyte fmt)
		{
			fixed(sbyte* pinned_fmt = &fmt)
			{
				TextAsPointer(pinned_fmt);
			}
		}
		public static void Text(string fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_Text_2602414842(fmt);
		}
		public static void TextColored(EngineNS.Vector4* col,string fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_TextColored_2591110465(col, fmt);
		}
		public static void TextColored( in EngineNS.Vector4 col,string fmt)
		{
			fixed(EngineNS.Vector4* pinned_col = &col)
			{
				TextColored(pinned_col, fmt);
			}
		}
		public static void TextDisabled(string fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_TextDisabled_2602414842(fmt);
		}
		public static void TextWrapped(string fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_TextWrapped_2602414842(fmt);
		}
		public static void LabelText(string label,string fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_LabelText_568371421(label, fmt);
		}
		public static void BulletText(string fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_BulletText_2602414842(fmt);
		}
		public static bool Button(string label,EngineNS.Vector2* size)
		{
			return TitanImGui_ImGuiAPI_Visitor_Button_3507648889(label, size) == 0 ? false : true;
		}
		public static bool Button(string label, in EngineNS.Vector2 size)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return Button(label, pinned_size);
			}
		}
		public static bool SmallButton(string label)
		{
			return TitanImGui_ImGuiAPI_Visitor_SmallButton_1080422500(label) == 0 ? false : true;
		}
		public static bool InvisibleButton(string str_id,EngineNS.Vector2* size,ImGuiButtonFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InvisibleButton_2525803684(str_id, size, flags) == 0 ? false : true;
		}
		public static bool InvisibleButton(string str_id, in EngineNS.Vector2 size,ImGuiButtonFlags_ flags)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return InvisibleButton(str_id, pinned_size, flags);
			}
		}
		public static bool ArrowButton(string str_id,ImGuiDir dir)
		{
			return TitanImGui_ImGuiAPI_Visitor_ArrowButton_4112088628(str_id, dir) == 0 ? false : true;
		}
		public static void Arrow(ImDrawList draw_list,EngineNS.Vector2* pos,uint col,ImGuiDir dir,float scale)
		{
			TitanImGui_ImGuiAPI_Visitor_Arrow_543170368(draw_list, pos, col, dir, scale);
		}
		public static void Arrow(ImDrawList draw_list, in EngineNS.Vector2 pos,uint col,ImGuiDir dir,float scale)
		{
			fixed(EngineNS.Vector2* pinned_pos = &pos)
			{
				Arrow(draw_list, pinned_pos, col, dir, scale);
			}
		}
		public static void Image(ulong user_texture_id,EngineNS.Vector2* size,EngineNS.Vector2* uv0,EngineNS.Vector2* uv1,EngineNS.Vector4* tint_col,EngineNS.Vector4* border_col)
		{
			TitanImGui_ImGuiAPI_Visitor_Image_3718021420(user_texture_id, size, uv0, uv1, tint_col, border_col);
		}
		public static void Image(ulong user_texture_id, in EngineNS.Vector2 size, in EngineNS.Vector2 uv0, in EngineNS.Vector2 uv1, in EngineNS.Vector4 tint_col, in EngineNS.Vector4 border_col)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			fixed(EngineNS.Vector2* pinned_uv0 = &uv0)
			fixed(EngineNS.Vector2* pinned_uv1 = &uv1)
			fixed(EngineNS.Vector4* pinned_tint_col = &tint_col)
			fixed(EngineNS.Vector4* pinned_border_col = &border_col)
			{
				Image(user_texture_id, pinned_size, pinned_uv0, pinned_uv1, pinned_tint_col, pinned_border_col);
			}
		}
		public static bool ImageButton(string name,ulong user_texture_id,EngineNS.Vector2* size,EngineNS.Vector2* uv0,EngineNS.Vector2* uv1,EngineNS.Vector4* bg_col,EngineNS.Vector4* tint_col)
		{
			return TitanImGui_ImGuiAPI_Visitor_ImageButton_550850427(name, user_texture_id, size, uv0, uv1, bg_col, tint_col) == 0 ? false : true;
		}
		public static bool ImageButton(string name,ulong user_texture_id, in EngineNS.Vector2 size, in EngineNS.Vector2 uv0, in EngineNS.Vector2 uv1, in EngineNS.Vector4 bg_col, in EngineNS.Vector4 tint_col)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			fixed(EngineNS.Vector2* pinned_uv0 = &uv0)
			fixed(EngineNS.Vector2* pinned_uv1 = &uv1)
			fixed(EngineNS.Vector4* pinned_bg_col = &bg_col)
			fixed(EngineNS.Vector4* pinned_tint_col = &tint_col)
			{
				return ImageButton(name, user_texture_id, pinned_size, pinned_uv0, pinned_uv1, pinned_bg_col, pinned_tint_col);
			}
		}
		public static bool Checkbox(string label,bool* v)
		{
			return TitanImGui_ImGuiAPI_Visitor_Checkbox_2817596850(label, v) == 0 ? false : true;
		}
		public static bool Checkbox(string label, ref bool v)
		{
			fixed(bool* pinned_v = &v)
			{
				return Checkbox(label, pinned_v);
			}
		}
		public static bool CheckboxFlags(string label,int* flags,int flags_value)
		{
			return TitanImGui_ImGuiAPI_Visitor_CheckboxFlags_1998082816(label, flags, flags_value) == 0 ? false : true;
		}
		public static bool CheckboxFlags(string label, ref int flags,int flags_value)
		{
			fixed(int* pinned_flags = &flags)
			{
				return CheckboxFlags(label, pinned_flags, flags_value);
			}
		}
		public static bool CheckboxFlags(string label,uint* flags,uint flags_value)
		{
			return TitanImGui_ImGuiAPI_Visitor_CheckboxFlags_145502690(label, flags, flags_value) == 0 ? false : true;
		}
		public static bool CheckboxFlags(string label, ref uint flags,uint flags_value)
		{
			fixed(uint* pinned_flags = &flags)
			{
				return CheckboxFlags(label, pinned_flags, flags_value);
			}
		}
		public static bool RadioButton(string label,bool active)
		{
			return TitanImGui_ImGuiAPI_Visitor_RadioButton_3037903904(label, active) == 0 ? false : true;
		}
		public static bool RadioButton(string label,int* v,int v_button)
		{
			return TitanImGui_ImGuiAPI_Visitor_RadioButton_1998082816(label, v, v_button) == 0 ? false : true;
		}
		public static bool RadioButton(string label, ref int v,int v_button)
		{
			fixed(int* pinned_v = &v)
			{
				return RadioButton(label, pinned_v, v_button);
			}
		}
		public static void ProgressBar(float fraction,EngineNS.Vector2* size_arg,string overlay)
		{
			TitanImGui_ImGuiAPI_Visitor_ProgressBar_2910944627(fraction, size_arg, overlay);
		}
		public static void ProgressBar(float fraction, in EngineNS.Vector2 size_arg,string overlay)
		{
			fixed(EngineNS.Vector2* pinned_size_arg = &size_arg)
			{
				ProgressBar(fraction, pinned_size_arg, overlay);
			}
		}
		public static void Bullet()
		{
			TitanImGui_ImGuiAPI_Visitor_Bullet_2960189489();
		}
		public static bool BeginCombo(string label,string preview_value,ImGuiComboFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginCombo_3858370926(label, preview_value, flags) == 0 ? false : true;
		}
		public static bool BeginCombo(string label,string preview_value,ImGuiComboFlags_ flags,ImGuiWindowFlags_ winFlags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginCombo_31333123(label, preview_value, flags, winFlags) == 0 ? false : true;
		}
		public static void EndCombo()
		{
			TitanImGui_ImGuiAPI_Visitor_EndCombo_2960189489();
		}
		public static bool Combo(string label,int* current_item,sbyte** items,int items_count,int popup_max_height_in_items)
		{
			return TitanImGui_ImGuiAPI_Visitor_Combo_1210971960(label, current_item, items, items_count, popup_max_height_in_items) == 0 ? false : true;
		}
		public static bool Combo(string label, ref int current_item,sbyte** items,int items_count,int popup_max_height_in_items)
		{
			fixed(int* pinned_current_item = &current_item)
			{
				return Combo(label, pinned_current_item, items, items_count, popup_max_height_in_items);
			}
		}
		public static bool Combo(string label,int* current_item,string items_separated_by_zeros,int popup_max_height_in_items)
		{
			return TitanImGui_ImGuiAPI_Visitor_Combo_161697717(label, current_item, items_separated_by_zeros, popup_max_height_in_items) == 0 ? false : true;
		}
		public static bool Combo(string label, ref int current_item,string items_separated_by_zeros,int popup_max_height_in_items)
		{
			fixed(int* pinned_current_item = &current_item)
			{
				return Combo(label, pinned_current_item, items_separated_by_zeros, popup_max_height_in_items);
			}
		}
		public unsafe delegate sbyte* FDelegate_items_getter(void* arg0,int arg1);
		public static bool Combo(string label,int* current_item,FDelegate_items_getter fn_getter,void* data,int items_count,int popup_max_height_in_items)
		{
			return TitanImGui_ImGuiAPI_Visitor_Combo_3739767375(label, current_item, fn_getter, data, items_count, popup_max_height_in_items) == 0 ? false : true;
		}
		public static bool Combo(string label, ref int current_item,FDelegate_items_getter fn_getter,void* data,int items_count,int popup_max_height_in_items)
		{
			fixed(int* pinned_current_item = &current_item)
			{
				return Combo(label, pinned_current_item, fn_getter, data, items_count, popup_max_height_in_items);
			}
		}
		public static bool DragFloat(string label,float* v,float v_speed,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragFloat_3863841807(label, v, v_speed, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool DragFloat(string label, ref float v,float v_speed,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return DragFloat(label, pinned_v, v_speed, v_min, v_max, format, flags);
			}
		}
		public static bool DragFloat2(string label,float* v,float v_speed,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragFloat2_3863841807(label, v, v_speed, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool DragFloat2(string label, ref float v,float v_speed,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return DragFloat2(label, pinned_v, v_speed, v_min, v_max, format, flags);
			}
		}
		public static bool DragFloat3(string label,float* v,float v_speed,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragFloat3_3863841807(label, v, v_speed, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool DragFloat3(string label, ref float v,float v_speed,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return DragFloat3(label, pinned_v, v_speed, v_min, v_max, format, flags);
			}
		}
		public static bool DragFloat4(string label,float* v,float v_speed,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragFloat4_3863841807(label, v, v_speed, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool DragFloat4(string label, ref float v,float v_speed,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return DragFloat4(label, pinned_v, v_speed, v_min, v_max, format, flags);
			}
		}
		public static bool DragFloatRange2(string label,float* v_current_min,float* v_current_max,float v_speed,float v_min,float v_max,string format,string format_max,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragFloatRange2_3996816114(label, v_current_min, v_current_max, v_speed, v_min, v_max, format, format_max, flags) == 0 ? false : true;
		}
		public static bool DragFloatRange2(string label, ref float v_current_min, ref float v_current_max,float v_speed,float v_min,float v_max,string format,string format_max,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v_current_min = &v_current_min)
			fixed(float* pinned_v_current_max = &v_current_max)
			{
				return DragFloatRange2(label, pinned_v_current_min, pinned_v_current_max, v_speed, v_min, v_max, format, format_max, flags);
			}
		}
		public static bool DragInt(string label,int* v,float v_speed,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragInt_3407096620(label, v, v_speed, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool DragInt(string label, ref int v,float v_speed,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return DragInt(label, pinned_v, v_speed, v_min, v_max, format, flags);
			}
		}
		public static bool DragInt2(string label,int* v,float v_speed,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragInt2_3407096620(label, v, v_speed, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool DragInt2(string label, ref int v,float v_speed,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return DragInt2(label, pinned_v, v_speed, v_min, v_max, format, flags);
			}
		}
		public static bool DragInt3(string label,int* v,float v_speed,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragInt3_3407096620(label, v, v_speed, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool DragInt3(string label, ref int v,float v_speed,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return DragInt3(label, pinned_v, v_speed, v_min, v_max, format, flags);
			}
		}
		public static bool DragInt4(string label,int* v,float v_speed,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragInt4_3407096620(label, v, v_speed, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool DragInt4(string label, ref int v,float v_speed,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return DragInt4(label, pinned_v, v_speed, v_min, v_max, format, flags);
			}
		}
		public static bool DragIntRange2(string label,int* v_current_min,int* v_current_max,float v_speed,int v_min,int v_max,string format,string format_max,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragIntRange2_52041202(label, v_current_min, v_current_max, v_speed, v_min, v_max, format, format_max, flags) == 0 ? false : true;
		}
		public static bool DragIntRange2(string label, ref int v_current_min, ref int v_current_max,float v_speed,int v_min,int v_max,string format,string format_max,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v_current_min = &v_current_min)
			fixed(int* pinned_v_current_max = &v_current_max)
			{
				return DragIntRange2(label, pinned_v_current_min, pinned_v_current_max, v_speed, v_min, v_max, format, format_max, flags);
			}
		}
		public static bool DragScalar(string label,ImGuiDataType_ data_type,void* p_data,float v_speed,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragScalar_3791056475(label, data_type, p_data, v_speed, p_min, p_max, format, flags) == 0 ? false : true;
		}
		public static bool DragScalarN(string label,ImGuiDataType_ data_type,void* p_data,int components,float v_speed,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragScalarN_576259350(label, data_type, p_data, components, v_speed, p_min, p_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderFloat(string label,float* v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderFloat_281608583(label, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderFloat(string label, ref float v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return SliderFloat(label, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool SliderFloat2(string label,float* v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderFloat2_281608583(label, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderFloat2(string label, ref float v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return SliderFloat2(label, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool SliderFloat3(string label,float* v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderFloat3_281608583(label, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderFloat3(string label, ref float v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return SliderFloat3(label, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool SliderFloat4(string label,float* v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderFloat4_281608583(label, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderFloat4(string label, ref float v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return SliderFloat4(label, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool SliderAngle(string label,float* v_rad,float v_degrees_min,float v_degrees_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderAngle_281608583(label, v_rad, v_degrees_min, v_degrees_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderAngle(string label, ref float v_rad,float v_degrees_min,float v_degrees_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(float* pinned_v_rad = &v_rad)
			{
				return SliderAngle(label, pinned_v_rad, v_degrees_min, v_degrees_max, format, flags);
			}
		}
		public static bool SliderInt(string label,int* v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderInt_4038701670(label, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderInt(string label, ref int v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return SliderInt(label, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool SliderInt2(string label,int* v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderInt2_4038701670(label, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderInt2(string label, ref int v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return SliderInt2(label, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool SliderInt3(string label,int* v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderInt3_4038701670(label, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderInt3(string label, ref int v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return SliderInt3(label, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool SliderInt4(string label,int* v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderInt4_4038701670(label, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderInt4(string label, ref int v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return SliderInt4(label, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool SliderScalar(string label,ImGuiDataType_ data_type,void* p_data,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderScalar_2997903363(label, data_type, p_data, p_min, p_max, format, flags) == 0 ? false : true;
		}
		public static bool SliderScalarN(string label,ImGuiDataType_ data_type,void* p_data,int components,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_SliderScalarN_4051498348(label, data_type, p_data, components, p_min, p_max, format, flags) == 0 ? false : true;
		}
		public static bool VSliderFloat(string label,EngineNS.Vector2* size,float* v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_VSliderFloat_2008079404(label, size, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool VSliderFloat(string label, in EngineNS.Vector2 size, ref float v,float v_min,float v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			fixed(float* pinned_v = &v)
			{
				return VSliderFloat(label, pinned_size, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool VSliderInt(string label,EngineNS.Vector2* size,int* v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_VSliderInt_1088819369(label, size, v, v_min, v_max, format, flags) == 0 ? false : true;
		}
		public static bool VSliderInt(string label, in EngineNS.Vector2 size, ref int v,int v_min,int v_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			fixed(int* pinned_v = &v)
			{
				return VSliderInt(label, pinned_size, pinned_v, v_min, v_max, format, flags);
			}
		}
		public static bool VSliderScalar(string label,EngineNS.Vector2* size,ImGuiDataType_ data_type,void* p_data,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_VSliderScalar_1401309246(label, size, data_type, p_data, p_min, p_max, format, flags) == 0 ? false : true;
		}
		public static bool VSliderScalar(string label, in EngineNS.Vector2 size,ImGuiDataType_ data_type,void* p_data,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return VSliderScalar(label, pinned_size, data_type, p_data, p_min, p_max, format, flags);
			}
		}
		public unsafe delegate int FDelegate_ImGuiInputTextCallback(ImGuiInputTextCallbackData* arg0);
		public static bool InputText(string label,void* buf,uint buf_size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputText_530115699(label, buf, buf_size, flags, callback, user_data) == 0 ? false : true;
		}
		public static bool InputTextNoName(string label,void* buf,uint buf_size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputTextNoName_530115699(label, buf, buf_size, flags, callback, user_data) == 0 ? false : true;
		}
		public static bool InputTextMultiline(string label,string buf,uint buf_size,EngineNS.Vector2* size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputTextMultiline_1095533176(label, buf, buf_size, size, flags, callback, user_data) == 0 ? false : true;
		}
		public static bool InputTextMultiline(string label,string buf,uint buf_size, in EngineNS.Vector2 size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return InputTextMultiline(label, buf, buf_size, pinned_size, flags, callback, user_data);
			}
		}
		public static bool InputTextWithHint(string label,string hint,string buf,uint buf_size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputTextWithHint_433703410(label, hint, buf, buf_size, flags, callback, user_data) == 0 ? false : true;
		}
		public static bool InputFloat(string label,float* v,float step,float step_fast,string format,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputFloat_3636630689(label, v, step, step_fast, format, flags) == 0 ? false : true;
		}
		public static bool InputFloat(string label, ref float v,float step,float step_fast,string format,ImGuiInputTextFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return InputFloat(label, pinned_v, step, step_fast, format, flags);
			}
		}
		public static bool InputFloat2(string label,float* v,string format,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputFloat2_1399196753(label, v, format, flags) == 0 ? false : true;
		}
		public static bool InputFloat2(string label, ref float v,string format,ImGuiInputTextFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return InputFloat2(label, pinned_v, format, flags);
			}
		}
		public static bool InputFloat3(string label,float* v,string format,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputFloat3_1399196753(label, v, format, flags) == 0 ? false : true;
		}
		public static bool InputFloat3(string label, ref float v,string format,ImGuiInputTextFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return InputFloat3(label, pinned_v, format, flags);
			}
		}
		public static bool InputFloat4(string label,float* v,string format,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputFloat4_1399196753(label, v, format, flags) == 0 ? false : true;
		}
		public static bool InputFloat4(string label, ref float v,string format,ImGuiInputTextFlags_ flags)
		{
			fixed(float* pinned_v = &v)
			{
				return InputFloat4(label, pinned_v, format, flags);
			}
		}
		public static bool InputInt(string label,int* v,int step,int step_fast,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputInt_4119295329(label, v, step, step_fast, flags) == 0 ? false : true;
		}
		public static bool InputInt(string label, ref int v,int step,int step_fast,ImGuiInputTextFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return InputInt(label, pinned_v, step, step_fast, flags);
			}
		}
		public static bool InputInt2(string label,int* v,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputInt2_772478873(label, v, flags) == 0 ? false : true;
		}
		public static bool InputInt2(string label, ref int v,ImGuiInputTextFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return InputInt2(label, pinned_v, flags);
			}
		}
		public static bool InputInt3(string label,int* v,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputInt3_772478873(label, v, flags) == 0 ? false : true;
		}
		public static bool InputInt3(string label, ref int v,ImGuiInputTextFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return InputInt3(label, pinned_v, flags);
			}
		}
		public static bool InputInt4(string label,int* v,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputInt4_772478873(label, v, flags) == 0 ? false : true;
		}
		public static bool InputInt4(string label, ref int v,ImGuiInputTextFlags_ flags)
		{
			fixed(int* pinned_v = &v)
			{
				return InputInt4(label, pinned_v, flags);
			}
		}
		public static bool InputDouble(string label,double* v,double step,double step_fast,string format,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputDouble_4137560224(label, v, step, step_fast, format, flags) == 0 ? false : true;
		}
		public static bool InputDouble(string label, ref double v,double step,double step_fast,string format,ImGuiInputTextFlags_ flags)
		{
			fixed(double* pinned_v = &v)
			{
				return InputDouble(label, pinned_v, step, step_fast, format, flags);
			}
		}
		public static bool InputScalar(string label,ImGuiDataType_ data_type,void* p_data,void* p_step,void* p_step_fast,string format,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputScalar_1030920285(label, data_type, p_data, p_step, p_step_fast, format, flags) == 0 ? false : true;
		}
		public static bool InputScalarN(string label,ImGuiDataType_ data_type,void* p_data,int components,void* p_step,void* p_step_fast,string format,ImGuiInputTextFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_InputScalarN_3243426466(label, data_type, p_data, components, p_step, p_step_fast, format, flags) == 0 ? false : true;
		}
		public static bool ColorEdit3(string label,float* col,ImGuiColorEditFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_ColorEdit3_833964466(label, col, flags) == 0 ? false : true;
		}
		public static bool ColorEdit3(string label, ref float col,ImGuiColorEditFlags_ flags)
		{
			fixed(float* pinned_col = &col)
			{
				return ColorEdit3(label, pinned_col, flags);
			}
		}
		public static bool ColorEdit4(string label,float* col,ImGuiColorEditFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_ColorEdit4_833964466(label, col, flags) == 0 ? false : true;
		}
		public static bool ColorEdit4(string label, ref float col,ImGuiColorEditFlags_ flags)
		{
			fixed(float* pinned_col = &col)
			{
				return ColorEdit4(label, pinned_col, flags);
			}
		}
		public static bool ColorPicker3(string label,float* col,ImGuiColorEditFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_ColorPicker3_833964466(label, col, flags) == 0 ? false : true;
		}
		public static bool ColorPicker3(string label, ref float col,ImGuiColorEditFlags_ flags)
		{
			fixed(float* pinned_col = &col)
			{
				return ColorPicker3(label, pinned_col, flags);
			}
		}
		public static bool ColorPicker4(string label,float* col,ImGuiColorEditFlags_ flags,float* _col2)
		{
			return TitanImGui_ImGuiAPI_Visitor_ColorPicker4_2514436871(label, col, flags, _col2) == 0 ? false : true;
		}
		public static bool ColorPicker4(string label, ref float col,ImGuiColorEditFlags_ flags, in float _col2)
		{
			fixed(float* pinned_col = &col)
			fixed(float* pinned__col2 = &_col2)
			{
				return ColorPicker4(label, pinned_col, flags, pinned__col2);
			}
		}
		public static bool ColorButton(string desc_id,EngineNS.Vector4* col,ImGuiColorEditFlags_ flags,EngineNS.Vector2* size)
		{
			return TitanImGui_ImGuiAPI_Visitor_ColorButton_4115823885(desc_id, col, flags, size) == 0 ? false : true;
		}
		public static bool ColorButton(string desc_id, in EngineNS.Vector4 col,ImGuiColorEditFlags_ flags, ref EngineNS.Vector2 size)
		{
			fixed(EngineNS.Vector4* pinned_col = &col)
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return ColorButton(desc_id, pinned_col, flags, pinned_size);
			}
		}
		public static void SetColorEditOptions(ImGuiColorEditFlags_ flags)
		{
			TitanImGui_ImGuiAPI_Visitor_SetColorEditOptions_4022841967(flags);
		}
		public static bool TreeNode(string label)
		{
			return TitanImGui_ImGuiAPI_Visitor_TreeNode_1080422500(label) == 0 ? false : true;
		}
		public static bool TreeNode(string str_id,string fmt)
		{
			return TitanImGui_ImGuiAPI_Visitor_TreeNode_459238835(str_id, fmt) == 0 ? false : true;
		}
		public static bool TreeNode(void* ptr_id,string fmt)
		{
			return TitanImGui_ImGuiAPI_Visitor_TreeNode_3207787083(ptr_id, fmt) == 0 ? false : true;
		}
		public static bool TreeNodeEx(string label,ImGuiTreeNodeFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2036359645(label, flags) == 0 ? false : true;
		}
		public static bool TreeNodeEx(string str_id,ImGuiTreeNodeFlags_ flags,string fmt)
		{
			return TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2368787316(str_id, flags, fmt) == 0 ? false : true;
		}
		public static bool TreeNodeEx(void* ptr_id,ImGuiTreeNodeFlags_ flags,string fmt)
		{
			return TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2410412098(ptr_id, flags, fmt) == 0 ? false : true;
		}
		public static void TreePush(string str_id)
		{
			TitanImGui_ImGuiAPI_Visitor_TreePush_2602414842(str_id);
		}
		public static void TreePush(void* ptr_id)
		{
			TitanImGui_ImGuiAPI_Visitor_TreePush_1819065180(ptr_id);
		}
		public static void TreePop()
		{
			TitanImGui_ImGuiAPI_Visitor_TreePop_2960189489();
		}
		public static float GetTreeNodeToLabelSpacing()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetTreeNodeToLabelSpacing_3743936629();
		}
		public static bool CollapsingHeader(string label,ImGuiTreeNodeFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_2036359645(label, flags) == 0 ? false : true;
		}
		public static bool CollapsingHeader(string label,bool* p_open,ImGuiTreeNodeFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_2891925945(label, p_open, flags) == 0 ? false : true;
		}
		public static bool CollapsingHeader(string label, ref bool p_open,ImGuiTreeNodeFlags_ flags)
		{
			fixed(bool* pinned_p_open = &p_open)
			{
				return CollapsingHeader(label, pinned_p_open, flags);
			}
		}
		public static void SetNextItemOpen(bool is_open,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextItemOpen_3341245801(is_open, cond);
		}
		public static bool Selectable(string label,bool selected,ImGuiSelectableFlags_ flags,EngineNS.Vector2* size)
		{
			return TitanImGui_ImGuiAPI_Visitor_Selectable_1745889278(label, selected, flags, size) == 0 ? false : true;
		}
		public static bool Selectable(string label,bool selected,ImGuiSelectableFlags_ flags, in EngineNS.Vector2 size)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return Selectable(label, selected, flags, pinned_size);
			}
		}
		public static bool Selectable(string label,bool* p_selected,ImGuiSelectableFlags_ flags,EngineNS.Vector2* size)
		{
			return TitanImGui_ImGuiAPI_Visitor_Selectable_778409408(label, p_selected, flags, size) == 0 ? false : true;
		}
		public static bool Selectable(string label, ref bool p_selected,ImGuiSelectableFlags_ flags, in EngineNS.Vector2 size)
		{
			fixed(bool* pinned_p_selected = &p_selected)
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return Selectable(label, pinned_p_selected, flags, pinned_size);
			}
		}
		public static bool ListBox(string label,int* current_item,sbyte** items,int items_count,int height_in_items)
		{
			return TitanImGui_ImGuiAPI_Visitor_ListBox_1210971960(label, current_item, items, items_count, height_in_items) == 0 ? false : true;
		}
		public static bool ListBox(string label, ref int current_item,sbyte** items,int items_count,int height_in_items)
		{
			fixed(int* pinned_current_item = &current_item)
			{
				return ListBox(label, pinned_current_item, items, items_count, height_in_items);
			}
		}
		public static bool ListBox(string label,int* current_item,FDelegate_items_getter fn_getter,void* data,int items_count,int height_in_items)
		{
			return TitanImGui_ImGuiAPI_Visitor_ListBox_3739767375(label, current_item, fn_getter, data, items_count, height_in_items) == 0 ? false : true;
		}
		public static bool ListBox(string label, ref int current_item,FDelegate_items_getter fn_getter,void* data,int items_count,int height_in_items)
		{
			fixed(int* pinned_current_item = &current_item)
			{
				return ListBox(label, pinned_current_item, fn_getter, data, items_count, height_in_items);
			}
		}
		public static void PlotLines(string label,float* values,int values_count,int values_offset,string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size,int stride)
		{
			TitanImGui_ImGuiAPI_Visitor_PlotLines_3358809591(label, values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
		}
		public static void PlotLines(string label, in float values,int values_count,int values_offset,string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size,int stride)
		{
			fixed(float* pinned_values = &values)
			{
				PlotLines(label, pinned_values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
			}
		}
		public unsafe delegate float FDelegate_values_getter(void* arg0,int arg1);
		public static void PlotLines(string label,FDelegate_values_getter fn_getter,void* data,int values_count,int values_offset,string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size)
		{
			TitanImGui_ImGuiAPI_Visitor_PlotLines_403696211(label, fn_getter, data, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size);
		}
		public static void PlotHistogram(string label,float* values,int values_count,int values_offset,string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size,int stride)
		{
			TitanImGui_ImGuiAPI_Visitor_PlotHistogram_3358809591(label, values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
		}
		public static void PlotHistogram(string label, in float values,int values_count,int values_offset,string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size,int stride)
		{
			fixed(float* pinned_values = &values)
			{
				PlotHistogram(label, pinned_values, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size, stride);
			}
		}
		public static void PlotHistogram(string label,FDelegate_values_getter fn_getter,void* data,int values_count,int values_offset,string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size)
		{
			TitanImGui_ImGuiAPI_Visitor_PlotHistogram_403696211(label, fn_getter, data, values_count, values_offset, overlay_text, scale_min, scale_max, graph_size);
		}
		public static void Value(string prefix,bool b)
		{
			TitanImGui_ImGuiAPI_Visitor_Value_3791955190(prefix, b);
		}
		public static void Value(string prefix,int v)
		{
			TitanImGui_ImGuiAPI_Visitor_Value_2553264241(prefix, v);
		}
		public static void Value(string prefix,uint v)
		{
			TitanImGui_ImGuiAPI_Visitor_Value_641062864(prefix, v);
		}
		public static void Value(string prefix,float v,string float_format)
		{
			TitanImGui_ImGuiAPI_Visitor_Value_3548670745(prefix, v, float_format);
		}
		public static bool BeginMenuBar()
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginMenuBar_1117990983() == 0 ? false : true;
		}
		public static void EndMenuBar()
		{
			TitanImGui_ImGuiAPI_Visitor_EndMenuBar_2960189489();
		}
		public static bool BeginMainMenuBar()
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginMainMenuBar_1117990983() == 0 ? false : true;
		}
		public static void EndMainMenuBar()
		{
			TitanImGui_ImGuiAPI_Visitor_EndMainMenuBar_2960189489();
		}
		public static bool BeginMenu(string label,bool enabled)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginMenu_3037903904(label, enabled) == 0 ? false : true;
		}
		public static void EndMenu()
		{
			TitanImGui_ImGuiAPI_Visitor_EndMenu_2960189489();
		}
		public static bool MenuItem(string label,string shortcut,bool selected,bool enabled)
		{
			return TitanImGui_ImGuiAPI_Visitor_MenuItem_1086805043(label, shortcut, selected, enabled) == 0 ? false : true;
		}
		public static bool MenuItem(string label,string shortcut,bool* p_selected,bool enabled)
		{
			return TitanImGui_ImGuiAPI_Visitor_MenuItem_3678366017(label, shortcut, p_selected, enabled) == 0 ? false : true;
		}
		public static bool MenuItem(string label,string shortcut, ref bool p_selected,bool enabled)
		{
			fixed(bool* pinned_p_selected = &p_selected)
			{
				return MenuItem(label, shortcut, pinned_p_selected, enabled);
			}
		}
		public static bool BeginTooltip()
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginTooltip_1117990983() == 0 ? false : true;
		}
		public static void EndTooltip()
		{
			TitanImGui_ImGuiAPI_Visitor_EndTooltip_2960189489();
		}
		public static void SetTooltip(string fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_SetTooltip_2602414842(fmt);
		}
		public static bool BeginPopup(string str_id,ImGuiWindowFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginPopup_2063302891(str_id, flags) == 0 ? false : true;
		}
		public static bool BeginPopupModal(string name,bool* p_open,ImGuiWindowFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginPopupModal_2979558607(name, p_open, flags) == 0 ? false : true;
		}
		public static bool BeginPopupModal(string name, ref bool p_open,ImGuiWindowFlags_ flags)
		{
			fixed(bool* pinned_p_open = &p_open)
			{
				return BeginPopupModal(name, pinned_p_open, flags);
			}
		}
		public static void EndPopup()
		{
			TitanImGui_ImGuiAPI_Visitor_EndPopup_2960189489();
		}
		public static void OpenPopup(string str_id,ImGuiPopupFlags_ popup_flags)
		{
			TitanImGui_ImGuiAPI_Visitor_OpenPopup_780651675(str_id, popup_flags);
		}
		public static void OpenPopupOnItemClick(string str_id,ImGuiPopupFlags_ popup_flags)
		{
			TitanImGui_ImGuiAPI_Visitor_OpenPopupOnItemClick_780651675(str_id, popup_flags);
		}
		public static void CloseCurrentPopup()
		{
			TitanImGui_ImGuiAPI_Visitor_CloseCurrentPopup_2960189489();
		}
		public static bool BeginPopupContextItem(string str_id,ImGuiPopupFlags_ popup_flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginPopupContextItem_3980950421(str_id, popup_flags) == 0 ? false : true;
		}
		public static bool BeginPopupContextWindow(string str_id,ImGuiPopupFlags_ popup_flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginPopupContextWindow_3980950421(str_id, popup_flags) == 0 ? false : true;
		}
		public static bool BeginPopupContextVoid(string str_id,ImGuiPopupFlags_ popup_flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginPopupContextVoid_3980950421(str_id, popup_flags) == 0 ? false : true;
		}
		public static bool IsPopupOpen(string str_id,ImGuiPopupFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsPopupOpen_3980950421(str_id, flags) == 0 ? false : true;
		}
		public static void Columns(int count,string id,bool border)
		{
			TitanImGui_ImGuiAPI_Visitor_Columns_1241066619(count, id, border);
		}
		public static void NextColumn()
		{
			TitanImGui_ImGuiAPI_Visitor_NextColumn_2960189489();
		}
		public static int GetColumnIndex()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetColumnIndex_2704135706();
		}
		public static float GetColumnWidth(int column_index)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetColumnWidth_1859829344(column_index);
		}
		public static void SetColumnWidth(int column_index,float width)
		{
			TitanImGui_ImGuiAPI_Visitor_SetColumnWidth_2140556032(column_index, width);
		}
		public static float GetColumnOffset(int column_index)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetColumnOffset_1859829344(column_index);
		}
		public static void SetColumnOffset(int column_index,float offset_x)
		{
			TitanImGui_ImGuiAPI_Visitor_SetColumnOffset_2140556032(column_index, offset_x);
		}
		public static int GetColumnsCount()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetColumnsCount_2704135706();
		}
		public static bool BeginTable(string str_id,int column,ImGuiTableFlags_ flags,EngineNS.Vector2* outer_size,float inner_width)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginTable_592367644(str_id, column, flags, outer_size, inner_width) == 0 ? false : true;
		}
		public static bool BeginTable(string str_id,int column,ImGuiTableFlags_ flags, in EngineNS.Vector2 outer_size,float inner_width)
		{
			fixed(EngineNS.Vector2* pinned_outer_size = &outer_size)
			{
				return BeginTable(str_id, column, flags, pinned_outer_size, inner_width);
			}
		}
		public static void EndTable()
		{
			TitanImGui_ImGuiAPI_Visitor_EndTable_2960189489();
		}
		public static void TableNextRow(ImGuiTableRowFlags_ row_flags,float min_row_height)
		{
			TitanImGui_ImGuiAPI_Visitor_TableNextRow_2950496009(row_flags, min_row_height);
		}
		public static bool TableNextColumn()
		{
			return TitanImGui_ImGuiAPI_Visitor_TableNextColumn_1117990983() == 0 ? false : true;
		}
		public static bool TableSetColumnIndex(int column_n)
		{
			return TitanImGui_ImGuiAPI_Visitor_TableSetColumnIndex_1125491426(column_n) == 0 ? false : true;
		}
		public static void TableSetupColumn(string label,ImGuiTableColumnFlags_ flags,float init_width_or_weight,uint user_id)
		{
			TitanImGui_ImGuiAPI_Visitor_TableSetupColumn_2358737958(label, flags, init_width_or_weight, user_id);
		}
		public static void TableSetupScrollFreeze(int cols,int rows)
		{
			TitanImGui_ImGuiAPI_Visitor_TableSetupScrollFreeze_3539386109(cols, rows);
		}
		public static void TableHeadersRow()
		{
			TitanImGui_ImGuiAPI_Visitor_TableHeadersRow_2960189489();
		}
		public static void TableHeader(string label)
		{
			TitanImGui_ImGuiAPI_Visitor_TableHeader_2602414842(label);
		}
		public static int TableGetColumnCount()
		{
			return TitanImGui_ImGuiAPI_Visitor_TableGetColumnCount_2704135706();
		}
		public static int TableGetColumnIndex()
		{
			return TitanImGui_ImGuiAPI_Visitor_TableGetColumnIndex_2704135706();
		}
		public static int TableGetRowIndex()
		{
			return TitanImGui_ImGuiAPI_Visitor_TableGetRowIndex_2704135706();
		}
		public static string TableGetColumnName(int column_n)
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiAPI_Visitor_TableGetColumnName_4173149781(column_n));
		}
		public static int TableGetColumnFlags(int column_n)
		{
			return TitanImGui_ImGuiAPI_Visitor_TableGetColumnFlags_3804733202(column_n);
		}
		public static void TableSetColumnEnabled(int column_n,bool v)
		{
			TitanImGui_ImGuiAPI_Visitor_TableSetColumnEnabled_2814434660(column_n, v);
		}
		public static void TableSetBgColor(ImGuiTableBgTarget_ target,uint color,int column_n)
		{
			TitanImGui_ImGuiAPI_Visitor_TableSetBgColor_3948758695(target, color, column_n);
		}
		public static bool BeginTabBar(string str_id,ImGuiTabBarFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginTabBar_697337101(str_id, flags) == 0 ? false : true;
		}
		public static void EndTabBar()
		{
			TitanImGui_ImGuiAPI_Visitor_EndTabBar_2960189489();
		}
		public static bool BeginTabItem(string label,bool* p_open,ImGuiTabItemFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginTabItem_1317613329(label, p_open, flags) == 0 ? false : true;
		}
		public static bool BeginTabItem(string label, ref bool p_open,ImGuiTabItemFlags_ flags)
		{
			fixed(bool* pinned_p_open = &p_open)
			{
				return BeginTabItem(label, pinned_p_open, flags);
			}
		}
		public static void EndTabItem()
		{
			TitanImGui_ImGuiAPI_Visitor_EndTabItem_2960189489();
		}
		public static bool TabItemButton(string label,ImGuiTabItemFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_TabItemButton_1489982093(label, flags) == 0 ? false : true;
		}
		public static void SetTabItemClosed(string tab_or_docked_window_label)
		{
			TitanImGui_ImGuiAPI_Visitor_SetTabItemClosed_2602414842(tab_or_docked_window_label);
		}
		public static uint DockSpace(uint id,EngineNS.Vector2* size,ImGuiDockNodeFlags_ flags,ImGuiWindowClass* window_class)
		{
			return TitanImGui_ImGuiAPI_Visitor_DockSpace_737848097(id, size, flags, window_class);
		}
		public static uint DockSpace(uint id, in EngineNS.Vector2 size,ImGuiDockNodeFlags_ flags, in ImGuiWindowClass window_class)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			fixed(ImGuiWindowClass* pinned_window_class = &window_class)
			{
				return DockSpace(id, pinned_size, flags, pinned_window_class);
			}
		}
		public static uint DockSpaceOverViewport(uint dock_id,ImGuiViewport* viewport,ImGuiDockNodeFlags_ flags,ImGuiWindowClass* window_class)
		{
			return TitanImGui_ImGuiAPI_Visitor_DockSpaceOverViewport_3195982265(dock_id, viewport, flags, window_class);
		}
		public static uint DockSpaceOverViewport(uint dock_id, ref ImGuiViewport viewport,ImGuiDockNodeFlags_ flags, in ImGuiWindowClass window_class)
		{
			fixed(ImGuiViewport* pinned_viewport = &viewport)
			fixed(ImGuiWindowClass* pinned_window_class = &window_class)
			{
				return DockSpaceOverViewport(dock_id, pinned_viewport, flags, pinned_window_class);
			}
		}
		public static void SetNextWindowDockID(uint dock_id,ImGuiCond_ cond)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowDockID_3620127785(dock_id, cond);
		}
		public static void SetNextWindowClass(ImGuiWindowClass* window_class)
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextWindowClass_2280511539(window_class);
		}
		public static void SetNextWindowClass( in ImGuiWindowClass window_class)
		{
			fixed(ImGuiWindowClass* pinned_window_class = &window_class)
			{
				SetNextWindowClass(pinned_window_class);
			}
		}
		public static uint GetWindowDockID()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetWindowDockID_3529484159();
		}
		public static bool IsWindowDocked()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsWindowDocked_1117990983() == 0 ? false : true;
		}
		public static void DockBuilderDockWindow(string window_name,uint node_id)
		{
			TitanImGui_ImGuiAPI_Visitor_DockBuilderDockWindow_2353073608(window_name, node_id);
		}
		public static uint DockBuilderAddNode(uint node_id,ImGuiDockNodeFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DockBuilderAddNode_1731294598(node_id, flags);
		}
		public static void DockBuilderRemoveNode(uint node_id)
		{
			TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNode_2252480719(node_id);
		}
		public static void DockBuilderRemoveNodeDockedWindows(uint node_id,bool clear_settings_refs)
		{
			TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNodeDockedWindows_1053065717(node_id, clear_settings_refs);
		}
		public static void DockBuilderRemoveNodeChildNodes(uint node_id)
		{
			TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNodeChildNodes_2252480719(node_id);
		}
		public static void DockBuilderSetNodePos(uint node_id,EngineNS.Vector2 pos)
		{
			TitanImGui_ImGuiAPI_Visitor_DockBuilderSetNodePos_3396750409(node_id, pos);
		}
		public static void DockBuilderSetNodeSize(uint node_id,EngineNS.Vector2 size)
		{
			TitanImGui_ImGuiAPI_Visitor_DockBuilderSetNodeSize_3396750409(node_id, size);
		}
		public static uint DockBuilderSplitNode(uint node_id,ImGuiDir split_dir,float size_ratio_for_node_at_dir,uint* out_id_at_dir,uint* out_id_at_opposite_dir)
		{
			return TitanImGui_ImGuiAPI_Visitor_DockBuilderSplitNode_230192101(node_id, split_dir, size_ratio_for_node_at_dir, out_id_at_dir, out_id_at_opposite_dir);
		}
		public static uint DockBuilderSplitNode(uint node_id,ImGuiDir split_dir,float size_ratio_for_node_at_dir, ref uint out_id_at_dir, ref uint out_id_at_opposite_dir)
		{
			fixed(uint* pinned_out_id_at_dir = &out_id_at_dir)
			fixed(uint* pinned_out_id_at_opposite_dir = &out_id_at_opposite_dir)
			{
				return DockBuilderSplitNode(node_id, split_dir, size_ratio_for_node_at_dir, pinned_out_id_at_dir, pinned_out_id_at_opposite_dir);
			}
		}
		public static void DockBuilderCopyWindowSettings(string src_name,string dst_name)
		{
			TitanImGui_ImGuiAPI_Visitor_DockBuilderCopyWindowSettings_568371421(src_name, dst_name);
		}
		public static void DockBuilderFinish(uint node_id)
		{
			TitanImGui_ImGuiAPI_Visitor_DockBuilderFinish_2252480719(node_id);
		}
		public static void LogToTTY(int auto_open_depth)
		{
			TitanImGui_ImGuiAPI_Visitor_LogToTTY_4038704236(auto_open_depth);
		}
		public static void LogToFile(int auto_open_depth,string filename)
		{
			TitanImGui_ImGuiAPI_Visitor_LogToFile_85734681(auto_open_depth, filename);
		}
		public static void LogToClipboard(int auto_open_depth)
		{
			TitanImGui_ImGuiAPI_Visitor_LogToClipboard_4038704236(auto_open_depth);
		}
		public static void LogFinish()
		{
			TitanImGui_ImGuiAPI_Visitor_LogFinish_2960189489();
		}
		public static void LogButtons()
		{
			TitanImGui_ImGuiAPI_Visitor_LogButtons_2960189489();
		}
		public static void LogText(string fmt)
		{
			TitanImGui_ImGuiAPI_Visitor_LogText_2602414842(fmt);
		}
		public static bool BeginDragDropSource(ImGuiDragDropFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginDragDropSource_4036233765(flags) == 0 ? false : true;
		}
		public static bool SetDragDropPayload(string type,void* data,uint sz,ImGuiCond_ cond)
		{
			return TitanImGui_ImGuiAPI_Visitor_SetDragDropPayload_411387055(type, data, sz, cond) == 0 ? false : true;
		}
		public static void EndDragDropSource()
		{
			TitanImGui_ImGuiAPI_Visitor_EndDragDropSource_2960189489();
		}
		public static bool BeginDragDropTarget()
		{
			return TitanImGui_ImGuiAPI_Visitor_BeginDragDropTarget_1117990983() == 0 ? false : true;
		}
		public static ImGuiPayload* AcceptDragDropPayload(string type,ImGuiDragDropFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_AcceptDragDropPayload_311770(type, flags);
		}
		public static void EndDragDropTarget()
		{
			TitanImGui_ImGuiAPI_Visitor_EndDragDropTarget_2960189489();
		}
		public static ImGuiPayload* GetDragDropPayload()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetDragDropPayload_2736951583();
		}
		public static void PushClipRect(EngineNS.Vector2* clip_rect_min,EngineNS.Vector2* clip_rect_max,bool intersect_with_current_clip_rect)
		{
			TitanImGui_ImGuiAPI_Visitor_PushClipRect_229364439(clip_rect_min, clip_rect_max, intersect_with_current_clip_rect);
		}
		public static void PushClipRect( in EngineNS.Vector2 clip_rect_min, in EngineNS.Vector2 clip_rect_max,bool intersect_with_current_clip_rect)
		{
			fixed(EngineNS.Vector2* pinned_clip_rect_min = &clip_rect_min)
			fixed(EngineNS.Vector2* pinned_clip_rect_max = &clip_rect_max)
			{
				PushClipRect(pinned_clip_rect_min, pinned_clip_rect_max, intersect_with_current_clip_rect);
			}
		}
		public static void PopClipRect()
		{
			TitanImGui_ImGuiAPI_Visitor_PopClipRect_2960189489();
		}
		public static void SetItemDefaultFocus()
		{
			TitanImGui_ImGuiAPI_Visitor_SetItemDefaultFocus_2960189489();
		}
		public static void SetKeyboardFocusHere(int offset)
		{
			TitanImGui_ImGuiAPI_Visitor_SetKeyboardFocusHere_4038704236(offset);
		}
		public static bool IsItemHovered(ImGuiHoveredFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemHovered_2491699375(flags) == 0 ? false : true;
		}
		public static bool IsItemActive()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemActive_1117990983() == 0 ? false : true;
		}
		public static bool IsItemFocused()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemFocused_1117990983() == 0 ? false : true;
		}
		public static bool IsItemClicked(ImGuiMouseButton_ mouse_button)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemClicked_361246070(mouse_button) == 0 ? false : true;
		}
		public static bool IsItemDoubleClicked(ImGuiMouseButton_ mouse_button)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemDoubleClicked_361246070(mouse_button) == 0 ? false : true;
		}
		public static bool IsItemVisible()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemVisible_1117990983() == 0 ? false : true;
		}
		public static bool IsItemEdited()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemEdited_1117990983() == 0 ? false : true;
		}
		public static bool IsItemActivated()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemActivated_1117990983() == 0 ? false : true;
		}
		public static bool IsItemDeactivated()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemDeactivated_1117990983() == 0 ? false : true;
		}
		public static bool IsItemDeactivatedAfterEdit()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemDeactivatedAfterEdit_1117990983() == 0 ? false : true;
		}
		public static bool IsItemToggledOpen()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsItemToggledOpen_1117990983() == 0 ? false : true;
		}
		public static bool IsAnyItemHovered()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsAnyItemHovered_1117990983() == 0 ? false : true;
		}
		public static bool IsAnyItemActive()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsAnyItemActive_1117990983() == 0 ? false : true;
		}
		public static bool IsAnyItemFocused()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsAnyItemFocused_1117990983() == 0 ? false : true;
		}
		public static EngineNS.Vector2 GetItemRectMin()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetItemRectMin_558510083();
		}
		public static EngineNS.Vector2 GetItemRectMax()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetItemRectMax_558510083();
		}
		public static EngineNS.Vector2 GetItemRectSize()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetItemRectSize_558510083();
		}
		public static bool IsRectVisible(EngineNS.Vector2* size)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsRectVisible_4039732974(size) == 0 ? false : true;
		}
		public static bool IsRectVisible( in EngineNS.Vector2 size)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return IsRectVisible(pinned_size);
			}
		}
		public static bool IsRectVisible(EngineNS.Vector2* rect_min,EngineNS.Vector2* rect_max)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsRectVisible_690184979(rect_min, rect_max) == 0 ? false : true;
		}
		public static bool IsRectVisible( in EngineNS.Vector2 rect_min, in EngineNS.Vector2 rect_max)
		{
			fixed(EngineNS.Vector2* pinned_rect_min = &rect_min)
			fixed(EngineNS.Vector2* pinned_rect_max = &rect_max)
			{
				return IsRectVisible(pinned_rect_min, pinned_rect_max);
			}
		}
		public static double GetTime()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetTime_4162959082();
		}
		public static int GetFrameCount()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetFrameCount_2704135706();
		}
		public static ImDrawList GetBackgroundDrawList()
		{
			return new ImDrawList(TitanImGui_ImGuiAPI_Visitor_GetBackgroundDrawList_2196389917());
		}
		public static ImDrawList GetForegroundDrawList()
		{
			return new ImDrawList(TitanImGui_ImGuiAPI_Visitor_GetForegroundDrawList_2196389917());
		}
		public static ImDrawList GetBackgroundDrawList(ImGuiViewport* viewport)
		{
			return new ImDrawList(TitanImGui_ImGuiAPI_Visitor_GetBackgroundDrawList_1456041080(viewport));
		}
		public static ImDrawList GetBackgroundDrawList( ref ImGuiViewport viewport)
		{
			fixed(ImGuiViewport* pinned_viewport = &viewport)
			{
				return GetBackgroundDrawList(pinned_viewport);
			}
		}
		public static ImDrawList GetForegroundDrawList(ImGuiViewport* viewport)
		{
			return new ImDrawList(TitanImGui_ImGuiAPI_Visitor_GetForegroundDrawList_1456041080(viewport));
		}
		public static ImDrawList GetForegroundDrawList( ref ImGuiViewport viewport)
		{
			fixed(ImGuiViewport* pinned_viewport = &viewport)
			{
				return GetForegroundDrawList(pinned_viewport);
			}
		}
		public static void* GetDrawListSharedData()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetDrawListSharedData_302642963();
		}
		public static string GetStyleColorName(ImGuiCol_ idx)
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiAPI_Visitor_GetStyleColorName_1027210462(idx));
		}
		public static void SetStateStorage(ImGuiStorage storage)
		{
			TitanImGui_ImGuiAPI_Visitor_SetStateStorage_1198396241(storage);
		}
		public static ImGuiStorage GetStateStorage()
		{
			return new ImGuiStorage(TitanImGui_ImGuiAPI_Visitor_GetStateStorage_479204201());
		}
		public static void SetNextItemAllowOverlap()
		{
			TitanImGui_ImGuiAPI_Visitor_SetNextItemAllowOverlap_2960189489();
		}
		public static EngineNS.Vector2 CalcTextSize(string text,bool hide_text_after_double_hash,float wrap_width)
		{
			return TitanImGui_ImGuiAPI_Visitor_CalcTextSize_2234397086(text, hide_text_after_double_hash, wrap_width);
		}
		public static EngineNS.Vector4 ColorConvertU32ToFloat4(uint inValue)
		{
			return TitanImGui_ImGuiAPI_Visitor_ColorConvertU32ToFloat4_4044967189(inValue);
		}
		public static uint ColorConvertFloat4ToU32(EngineNS.Vector4* inValue)
		{
			return TitanImGui_ImGuiAPI_Visitor_ColorConvertFloat4ToU32_2189523068(inValue);
		}
		public static uint ColorConvertFloat4ToU32( in EngineNS.Vector4 inValue)
		{
			fixed(EngineNS.Vector4* pinned_inValue = &inValue)
			{
				return ColorConvertFloat4ToU32(pinned_inValue);
			}
		}
		public static void ColorConvertRGBtoHSV(float r,float g,float b,float* out_h,float* out_s,float* out_v)
		{
			TitanImGui_ImGuiAPI_Visitor_ColorConvertRGBtoHSV_3904097195(r, g, b, out_h, out_s, out_v);
		}
		public static void ColorConvertRGBtoHSV(float r,float g,float b, ref float out_h, ref float out_s, ref float out_v)
		{
			fixed(float* pinned_out_h = &out_h)
			fixed(float* pinned_out_s = &out_s)
			fixed(float* pinned_out_v = &out_v)
			{
				ColorConvertRGBtoHSV(r, g, b, pinned_out_h, pinned_out_s, pinned_out_v);
			}
		}
		public static void ColorConvertHSVtoRGB(float h,float s,float v,float* out_r,float* out_g,float* out_b)
		{
			TitanImGui_ImGuiAPI_Visitor_ColorConvertHSVtoRGB_3904097195(h, s, v, out_r, out_g, out_b);
		}
		public static void ColorConvertHSVtoRGB(float h,float s,float v, ref float out_r, ref float out_g, ref float out_b)
		{
			fixed(float* pinned_out_r = &out_r)
			fixed(float* pinned_out_g = &out_g)
			fixed(float* pinned_out_b = &out_b)
			{
				ColorConvertHSVtoRGB(h, s, v, pinned_out_r, pinned_out_g, pinned_out_b);
			}
		}
		public static bool IsKeyDown(ImGuiKey user_key_index)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsKeyDown_664544503(user_key_index) == 0 ? false : true;
		}
		public static bool IsKeyPressed(ImGuiKey user_key_index,bool repeat)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsKeyPressed_1790704165(user_key_index, repeat) == 0 ? false : true;
		}
		public static bool IsKeyReleased(ImGuiKey user_key_index)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsKeyReleased_664544503(user_key_index) == 0 ? false : true;
		}
		public static int GetKeyPressedAmount(ImGuiKey key_index,float repeat_delay,float rate)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetKeyPressedAmount_1868481138(key_index, repeat_delay, rate);
		}
		public static bool IsMouseDown(ImGuiMouseButton_ button)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseDown_361246070(button) == 0 ? false : true;
		}
		public static bool IsMouseClicked(ImGuiMouseButton_ button,bool repeat)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseClicked_3257368754(button, repeat) == 0 ? false : true;
		}
		public static bool IsMouseReleased(ImGuiMouseButton_ button)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseReleased_361246070(button) == 0 ? false : true;
		}
		public static bool IsMouseDoubleClicked(ImGuiMouseButton_ button)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseDoubleClicked_361246070(button) == 0 ? false : true;
		}
		public static int GetMouseClickedCount(ImGuiMouseButton_ button)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetMouseClickedCount_3860407425(button);
		}
		public static bool IsMouseHoveringRect(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,bool clip)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseHoveringRect_1978284909(r_min, r_max, clip) == 0 ? false : true;
		}
		public static bool IsMouseHoveringRect( in EngineNS.Vector2 r_min, in EngineNS.Vector2 r_max,bool clip)
		{
			fixed(EngineNS.Vector2* pinned_r_min = &r_min)
			fixed(EngineNS.Vector2* pinned_r_max = &r_max)
			{
				return IsMouseHoveringRect(pinned_r_min, pinned_r_max, clip);
			}
		}
		public static bool IsMousePosValid(EngineNS.Vector2* mouse_pos)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMousePosValid_4039732974(mouse_pos) == 0 ? false : true;
		}
		public static bool IsMousePosValid( in EngineNS.Vector2 mouse_pos)
		{
			fixed(EngineNS.Vector2* pinned_mouse_pos = &mouse_pos)
			{
				return IsMousePosValid(pinned_mouse_pos);
			}
		}
		public static bool IsAnyMouseDown()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsAnyMouseDown_1117990983() == 0 ? false : true;
		}
		public static EngineNS.Vector2 GetMousePos()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetMousePos_558510083();
		}
		public static EngineNS.Vector2 GetMousePosOnOpeningCurrentPopup()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetMousePosOnOpeningCurrentPopup_558510083();
		}
		public static bool IsMouseDragging(ImGuiMouseButton_ button,float lock_threshold)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseDragging_2505297786(button, lock_threshold) == 0 ? false : true;
		}
		public static EngineNS.Vector2 GetMouseDragDelta(ImGuiMouseButton_ button,float lock_threshold)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetMouseDragDelta_4262098684(button, lock_threshold);
		}
		public static void ResetMouseDragDelta(ImGuiMouseButton_ button)
		{
			TitanImGui_ImGuiAPI_Visitor_ResetMouseDragDelta_269877056(button);
		}
		public static ImGuiMouseCursor_ GetMouseCursor()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetMouseCursor_345306920();
		}
		public static void SetMouseCursor(ImGuiMouseCursor_ cursor_type)
		{
			TitanImGui_ImGuiAPI_Visitor_SetMouseCursor_3384190518(cursor_type);
		}
		public static string GetClipboardText()
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiAPI_Visitor_GetClipboardText_2396230038());
		}
		public static void SetClipboardText(string text)
		{
			TitanImGui_ImGuiAPI_Visitor_SetClipboardText_2602414842(text);
		}
		public static void LoadIniSettingsFromDisk(string ini_filename)
		{
			TitanImGui_ImGuiAPI_Visitor_LoadIniSettingsFromDisk_2602414842(ini_filename);
		}
		public static void LoadIniSettingsFromMemory(string ini_data,uint ini_size)
		{
			TitanImGui_ImGuiAPI_Visitor_LoadIniSettingsFromMemory_3999832562(ini_data, ini_size);
		}
		public static void SaveIniSettingsToDisk(string ini_filename)
		{
			TitanImGui_ImGuiAPI_Visitor_SaveIniSettingsToDisk_2602414842(ini_filename);
		}
		public static string SaveIniSettingsToMemory(uint* out_ini_size)
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImGuiAPI_Visitor_SaveIniSettingsToMemory_814920808(out_ini_size));
		}
		public static string SaveIniSettingsToMemory( ref uint out_ini_size)
		{
			fixed(uint* pinned_out_ini_size = &out_ini_size)
			{
				return SaveIniSettingsToMemory(pinned_out_ini_size);
			}
		}
		public static bool DebugCheckVersionAndDataLayout(string version_str,IntPtr sz_io,IntPtr sz_style,IntPtr sz_vec2,IntPtr sz_vec4,IntPtr sz_drawvert,IntPtr sz_drawidx)
		{
			return TitanImGui_ImGuiAPI_Visitor_DebugCheckVersionAndDataLayout_204246052(version_str, sz_io, sz_style, sz_vec2, sz_vec4, sz_drawvert, sz_drawidx) == 0 ? false : true;
		}
		public unsafe delegate void* FDelegate_alloc_func(IntPtr arg0,void* arg1);
		public unsafe delegate void FDelegate_free_func(void* arg0,void* arg1);
		public static void SetAllocatorFunctions(FDelegate_alloc_func fn_alloc_func,FDelegate_free_func fn_free_func,void* user_data)
		{
			TitanImGui_ImGuiAPI_Visitor_SetAllocatorFunctions_3510213992(fn_alloc_func, fn_free_func, user_data);
		}
		public static void* MemAlloc(IntPtr size)
		{
			return TitanImGui_ImGuiAPI_Visitor_MemAlloc_1539568017(size);
		}
		public static void MemFree(void* ptr)
		{
			TitanImGui_ImGuiAPI_Visitor_MemFree_3034592143(ptr);
		}
		public static ImGuiPlatformIO GetPlatformIO()
		{
			return new ImGuiPlatformIO(TitanImGui_ImGuiAPI_Visitor_GetPlatformIO_971686321());
		}
		public static ImGuiViewport* GetMainViewport()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetMainViewport_4006837304();
		}
		public static void UpdatePlatformWindows()
		{
			TitanImGui_ImGuiAPI_Visitor_UpdatePlatformWindows_2960189489();
		}
		public static void RenderPlatformWindowsDefault(void* platform_render_arg,void* renderer_render_arg)
		{
			TitanImGui_ImGuiAPI_Visitor_RenderPlatformWindowsDefault_760721789(platform_render_arg, renderer_render_arg);
		}
		public static void DestroyPlatformWindows()
		{
			TitanImGui_ImGuiAPI_Visitor_DestroyPlatformWindows_2960189489();
		}
		public static ImGuiViewport* FindViewportByID(uint id)
		{
			return TitanImGui_ImGuiAPI_Visitor_FindViewportByID_1024491770(id);
		}
		public static ImGuiViewport* FindViewportByPlatformHandle(void* platform_handle)
		{
			return TitanImGui_ImGuiAPI_Visitor_FindViewportByPlatformHandle_4148205652(platform_handle);
		}
		public unsafe delegate void FDelegate_Renderer_CreateWindow(ImGuiViewport* arg0);
		public static void Set_Renderer_CreateWindow(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_CreateWindow fn)
		{
			TitanImGui_ImGuiAPI_Visitor_Set_Renderer_CreateWindow_3699537699(PlatformIO, fn);
		}
		public static void Set_Renderer_DestroyWindow(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_CreateWindow fn)
		{
			TitanImGui_ImGuiAPI_Visitor_Set_Renderer_DestroyWindow_1336333681(PlatformIO, fn);
		}
		public unsafe delegate void FDelegate_Renderer_SetWindowSize(ImGuiViewport* arg0,EngineNS.Vector2 arg1);
		public static void Set_Renderer_SetWindowSize(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_SetWindowSize fn)
		{
			TitanImGui_ImGuiAPI_Visitor_Set_Renderer_SetWindowSize_3092220344(PlatformIO, fn);
		}
		public unsafe delegate void FDelegate_Renderer_RenderWindow(ImGuiViewport* arg0,void* arg1);
		public static void Set_Renderer_RenderWindow(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_RenderWindow fn)
		{
			TitanImGui_ImGuiAPI_Visitor_Set_Renderer_RenderWindow_2851598185(PlatformIO, fn);
		}
		public static void Set_Renderer_SwapBuffers(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_RenderWindow fn)
		{
			TitanImGui_ImGuiAPI_Visitor_Set_Renderer_SwapBuffers_593121533(PlatformIO, fn);
		}
		public static void PlatformIO_Monitor_Resize(ImGuiPlatformIO io,int size)
		{
			TitanImGui_ImGuiAPI_Visitor_PlatformIO_Monitor_Resize_1567199252(io, size);
		}
		public static void PlatformIO_Monitor_PushBack(ImGuiPlatformIO io,ImGuiPlatformMonitor monitor)
		{
			TitanImGui_ImGuiAPI_Visitor_PlatformIO_Monitor_PushBack_1937089321(io, monitor);
		}
		public static int PlatformIO_Viewports_Size(ImGuiPlatformIO io)
		{
			return TitanImGui_ImGuiAPI_Visitor_PlatformIO_Viewports_Size_2566682924(io);
		}
		public static ImGuiViewport* PlatformIO_Viewports_Get(ImGuiPlatformIO io,int index)
		{
			return TitanImGui_ImGuiAPI_Visitor_PlatformIO_Viewports_Get_2057355863(io, index);
		}
		public static bool TextInputComboBox(string id,void* buffer,uint maxInputSize,sbyte** items,uint item_len,short showMaxItems)
		{
			return TitanImGui_ImGuiAPI_Visitor_TextInputComboBox_899144233(id, buffer, maxInputSize, items, item_len, showMaxItems) == 0 ? false : true;
		}
		public unsafe delegate sbyte* FDelegate_FGetClipboardTextFn(void* arg0);
		public static void GetClipboardTextSetter(ImGuiIO io,FDelegate_FGetClipboardTextFn fn)
		{
			TitanImGui_ImGuiAPI_Visitor_GetClipboardTextSetter_881800347(io, fn);
		}
		public unsafe delegate void FDelegate_FSetClipboardTextFn(void* arg0,sbyte* arg1);
		public static void SetClipboardTextSetter(ImGuiIO io,FDelegate_FSetClipboardTextFn fn)
		{
			TitanImGui_ImGuiAPI_Visitor_SetClipboardTextSetter_716060207(io, fn);
		}
		public static void ItemSize(EngineNS.Vector2* min,EngineNS.Vector2* max,float text_baseline_y)
		{
			TitanImGui_ImGuiAPI_Visitor_ItemSize_1990263409(min, max, text_baseline_y);
		}
		public static void ItemSize( in EngineNS.Vector2 min, in EngineNS.Vector2 max,float text_baseline_y)
		{
			fixed(EngineNS.Vector2* pinned_min = &min)
			fixed(EngineNS.Vector2* pinned_max = &max)
			{
				ItemSize(pinned_min, pinned_max, text_baseline_y);
			}
		}
		public static void ItemSize(EngineNS.Vector2* size,float text_baseline_y)
		{
			TitanImGui_ImGuiAPI_Visitor_ItemSize_2116332084(size, text_baseline_y);
		}
		public static void ItemSize( in EngineNS.Vector2 size,float text_baseline_y)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				ItemSize(pinned_size, text_baseline_y);
			}
		}
		public static bool ItemAdd(EngineNS.Vector2* bbMin,EngineNS.Vector2* bbMax,uint id,EngineNS.Vector2* nav_bb_min,EngineNS.Vector2* nav_bb_max,int flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_ItemAdd_1999737362(bbMin, bbMax, id, nav_bb_min, nav_bb_max, flags) == 0 ? false : true;
		}
		public static bool ItemAdd( in EngineNS.Vector2 bbMin, in EngineNS.Vector2 bbMax,uint id, in EngineNS.Vector2 nav_bb_min, in EngineNS.Vector2 nav_bb_max,int flags)
		{
			fixed(EngineNS.Vector2* pinned_bbMin = &bbMin)
			fixed(EngineNS.Vector2* pinned_bbMax = &bbMax)
			fixed(EngineNS.Vector2* pinned_nav_bb_min = &nav_bb_min)
			fixed(EngineNS.Vector2* pinned_nav_bb_max = &nav_bb_max)
			{
				return ItemAdd(pinned_bbMin, pinned_bbMax, id, pinned_nav_bb_min, pinned_nav_bb_max, flags);
			}
		}
		public static bool ItemAdd(EngineNS.Vector2* bbMin,EngineNS.Vector2* bbMax,uint id,int flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_ItemAdd_3017646930(bbMin, bbMax, id, flags) == 0 ? false : true;
		}
		public static bool ItemAdd( in EngineNS.Vector2 bbMin, in EngineNS.Vector2 bbMax,uint id,int flags)
		{
			fixed(EngineNS.Vector2* pinned_bbMin = &bbMin)
			fixed(EngineNS.Vector2* pinned_bbMax = &bbMax)
			{
				return ItemAdd(pinned_bbMin, pinned_bbMax, id, flags);
			}
		}
		public static bool GetTableWorkRect(EngineNS.Vector2* min,EngineNS.Vector2* max)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetTableWorkRect_2631331155(min, max) == 0 ? false : true;
		}
		public static bool GetTableWorkRect( ref EngineNS.Vector2 min, ref EngineNS.Vector2 max)
		{
			fixed(EngineNS.Vector2* pinned_min = &min)
			fixed(EngineNS.Vector2* pinned_max = &max)
			{
				return GetTableWorkRect(pinned_min, pinned_max);
			}
		}
		public static bool GetTableRowStartY(float* yValue)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetTableRowStartY_2989815025(yValue) == 0 ? false : true;
		}
		public static bool GetTableRowStartY( ref float yValue)
		{
			fixed(float* pinned_yValue = &yValue)
			{
				return GetTableRowStartY(pinned_yValue);
			}
		}
		public static bool GetTableRowEndY(float* yValue)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetTableRowEndY_2989815025(yValue) == 0 ? false : true;
		}
		public static bool GetTableRowEndY( ref float yValue)
		{
			fixed(float* pinned_yValue = &yValue)
			{
				return GetTableRowEndY(pinned_yValue);
			}
		}
		public static bool IsHoverCurrentWindow()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsHoverCurrentWindow_1117990983() == 0 ? false : true;
		}
		public static bool IsMouseHoveringRectInCurrentWindow(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,bool clip)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseHoveringRectInCurrentWindow_1978284909(r_min, r_max, clip) == 0 ? false : true;
		}
		public static bool IsMouseHoveringRectInCurrentWindow( in EngineNS.Vector2 r_min, in EngineNS.Vector2 r_max,bool clip)
		{
			fixed(EngineNS.Vector2* pinned_r_min = &r_min)
			fixed(EngineNS.Vector2* pinned_r_max = &r_max)
			{
				return IsMouseHoveringRectInCurrentWindow(pinned_r_min, pinned_r_max, clip);
			}
		}
		public static bool IsMouseDownInRectInCurrentWindow(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,ImGuiMouseButton_ button,bool clip)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseDownInRectInCurrentWindow_3682308850(r_min, r_max, button, clip) == 0 ? false : true;
		}
		public static bool IsMouseDownInRectInCurrentWindow( in EngineNS.Vector2 r_min, in EngineNS.Vector2 r_max,ImGuiMouseButton_ button,bool clip)
		{
			fixed(EngineNS.Vector2* pinned_r_min = &r_min)
			fixed(EngineNS.Vector2* pinned_r_max = &r_max)
			{
				return IsMouseDownInRectInCurrentWindow(pinned_r_min, pinned_r_max, button, clip);
			}
		}
		public static bool IsMouseClickedInRectInCurrentWindow(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,ImGuiMouseButton_ button,bool clip)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseClickedInRectInCurrentWindow_3682308850(r_min, r_max, button, clip) == 0 ? false : true;
		}
		public static bool IsMouseClickedInRectInCurrentWindow( in EngineNS.Vector2 r_min, in EngineNS.Vector2 r_max,ImGuiMouseButton_ button,bool clip)
		{
			fixed(EngineNS.Vector2* pinned_r_min = &r_min)
			fixed(EngineNS.Vector2* pinned_r_max = &r_max)
			{
				return IsMouseClickedInRectInCurrentWindow(pinned_r_min, pinned_r_max, button, clip);
			}
		}
		public static bool IsMouseDoubleClickedInRectInCurrentWindow(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,ImGuiMouseButton_ button,bool clip)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseDoubleClickedInRectInCurrentWindow_3682308850(r_min, r_max, button, clip) == 0 ? false : true;
		}
		public static bool IsMouseDoubleClickedInRectInCurrentWindow( in EngineNS.Vector2 r_min, in EngineNS.Vector2 r_max,ImGuiMouseButton_ button,bool clip)
		{
			fixed(EngineNS.Vector2* pinned_r_min = &r_min)
			fixed(EngineNS.Vector2* pinned_r_max = &r_max)
			{
				return IsMouseDoubleClickedInRectInCurrentWindow(pinned_r_min, pinned_r_max, button, clip);
			}
		}
		public static bool IsMouseDragPastThreshold(ImGuiMouseButton_ button,float lock_threshold)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsMouseDragPastThreshold_2505297786(button, lock_threshold) == 0 ? false : true;
		}
		public static bool IsCurrentWindowSkipItems()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsCurrentWindowSkipItems_1117990983() == 0 ? false : true;
		}
		public static bool ItemHoverable(EngineNS.Vector2* bbMin,EngineNS.Vector2* bbMax,uint id,int item_flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_ItemHoverable_1371559414(bbMin, bbMax, id, item_flags) == 0 ? false : true;
		}
		public static bool ItemHoverable( in EngineNS.Vector2 bbMin, in EngineNS.Vector2 bbMax,uint id,int item_flags)
		{
			fixed(EngineNS.Vector2* pinned_bbMin = &bbMin)
			fixed(EngineNS.Vector2* pinned_bbMax = &bbMax)
			{
				return ItemHoverable(pinned_bbMin, pinned_bbMax, id, item_flags);
			}
		}
		public static bool TempInputIsActive(uint id)
		{
			return TitanImGui_ImGuiAPI_Visitor_TempInputIsActive_900990169(id) == 0 ? false : true;
		}
		public static void SetActiveID(uint id)
		{
			TitanImGui_ImGuiAPI_Visitor_SetActiveID_2252480719(id);
		}
		public static uint GetActiveID()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetActiveID_3529484159();
		}
		public static void SetFocusID(uint id)
		{
			TitanImGui_ImGuiAPI_Visitor_SetFocusID_2252480719(id);
		}
		public static void SetTempInputID(uint id)
		{
			TitanImGui_ImGuiAPI_Visitor_SetTempInputID_2252480719(id);
		}
		public static uint GetTempInputID()
		{
			return TitanImGui_ImGuiAPI_Visitor_GetTempInputID_3529484159();
		}
		public static void ClearActiveID()
		{
			TitanImGui_ImGuiAPI_Visitor_ClearActiveID_2960189489();
		}
		public static void FocusCurrentWindow()
		{
			TitanImGui_ImGuiAPI_Visitor_FocusCurrentWindow_2960189489();
		}
		public static bool IsIDNavActivated(uint id)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsIDNavActivated_900990169(id) == 0 ? false : true;
		}
		public static bool IsIDNavInput(uint id)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsIDNavInput_900990169(id) == 0 ? false : true;
		}
		public static bool DragBehavior(uint id,ImGuiDataType_ data_type,void* p_v,float v_speed,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragBehavior_310431169(id, data_type, p_v, v_speed, p_min, p_max, format, flags) == 0 ? false : true;
		}
		public static bool DragActiveIdUpdate(uint id)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragActiveIdUpdate_900990169(id) == 0 ? false : true;
		}
		public static bool ButtonBehavior(EngineNS.Vector2* min,EngineNS.Vector2* max,uint id,bool* out_hovered,bool* out_held,bool pressOnRelease,ImGuiButtonFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_ButtonBehavior_1167781018(min, max, id, out_hovered, out_held, pressOnRelease, flags) == 0 ? false : true;
		}
		public static bool ButtonBehavior( in EngineNS.Vector2 min, in EngineNS.Vector2 max,uint id, ref bool out_hovered, ref bool out_held,bool pressOnRelease,ImGuiButtonFlags_ flags)
		{
			fixed(EngineNS.Vector2* pinned_min = &min)
			fixed(EngineNS.Vector2* pinned_max = &max)
			fixed(bool* pinned_out_hovered = &out_hovered)
			fixed(bool* pinned_out_held = &out_held)
			{
				return ButtonBehavior(pinned_min, pinned_max, id, pinned_out_hovered, pinned_out_held, pressOnRelease, flags);
			}
		}
		public static bool DragScalar2(string label,ImGuiDataType_ data_type,void* p_data,float v_speed,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragScalar2_220901116(label, data_type, p_data, v_speed, p_min, p_max, format, flags) == 0 ? false : true;
		}
		public static bool DragScalarN2(string label,ImGuiDataType_ data_type,void* p_data,int components,float v_speed,void* p_min,void* p_max,string format,ImGuiSliderFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_DragScalarN2_3862891275(label, data_type, p_data, components, v_speed, p_min, p_max, format, flags) == 0 ? false : true;
		}
		public static void RenderFrame(EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,uint fill_col,bool border,float rounding)
		{
			TitanImGui_ImGuiAPI_Visitor_RenderFrame_539721179(p_min, p_max, fill_col, border, rounding);
		}
		public static void RenderFrame( ref EngineNS.Vector2 p_min, ref EngineNS.Vector2 p_max,uint fill_col,bool border,float rounding)
		{
			fixed(EngineNS.Vector2* pinned_p_min = &p_min)
			fixed(EngineNS.Vector2* pinned_p_max = &p_max)
			{
				RenderFrame(pinned_p_min, pinned_p_max, fill_col, border, rounding);
			}
		}
		public static EngineNS.Vector2 CalcItemSize(EngineNS.Vector2* size,float default_w,float default_h)
		{
			return TitanImGui_ImGuiAPI_Visitor_CalcItemSize_3095670423(size, default_w, default_h);
		}
		public static EngineNS.Vector2 CalcItemSize( ref EngineNS.Vector2 size,float default_w,float default_h)
		{
			fixed(EngineNS.Vector2* pinned_size = &size)
			{
				return CalcItemSize(pinned_size, default_w, default_h);
			}
		}
		public static bool CollapsingHeader_SpanAllColumns(string label,ImGuiTreeNodeFlags_ flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_SpanAllColumns_2036359645(label, flags) == 0 ? false : true;
		}
		public static bool IsInTable()
		{
			return TitanImGui_ImGuiAPI_Visitor_IsInTable_1117990983() == 0 ? false : true;
		}
		public static void TableNextRow(EngineNS.ImGuiTableRowData* rowData)
		{
			TitanImGui_ImGuiAPI_Visitor_TableNextRow_589053033(rowData);
		}
		public static void TableNextRow( in EngineNS.ImGuiTableRowData rowData)
		{
			fixed(EngineNS.ImGuiTableRowData* pinned_rowData = &rowData)
			{
				TableNextRow(pinned_rowData);
			}
		}
		public static void TableNextRow_FirstColumn(EngineNS.ImGuiTableRowData* rowData)
		{
			TitanImGui_ImGuiAPI_Visitor_TableNextRow_FirstColumn_589053033(rowData);
		}
		public static void TableNextRow_FirstColumn( in EngineNS.ImGuiTableRowData rowData)
		{
			fixed(EngineNS.ImGuiTableRowData* pinned_rowData = &rowData)
			{
				TableNextRow_FirstColumn(pinned_rowData);
			}
		}
		public static bool CheckBoxTristate(string label,int* v_tristate)
		{
			return TitanImGui_ImGuiAPI_Visitor_CheckBoxTristate_263170625(label, v_tristate) == 0 ? false : true;
		}
		public static bool CheckBoxTristate(string label, ref int v_tristate)
		{
			fixed(int* pinned_v_tristate = &v_tristate)
			{
				return CheckBoxTristate(label, pinned_v_tristate);
			}
		}
		public static bool ToggleButton(string label,bool* v,EngineNS.Vector2* size_arg,int flags)
		{
			return TitanImGui_ImGuiAPI_Visitor_ToggleButton_853224535(label, v, size_arg, flags) == 0 ? false : true;
		}
		public static bool ToggleButton(string label, ref bool v, in EngineNS.Vector2 size_arg,int flags)
		{
			fixed(bool* pinned_v = &v)
			fixed(EngineNS.Vector2* pinned_size_arg = &size_arg)
			{
				return ToggleButton(label, pinned_v, pinned_size_arg, flags);
			}
		}
		public static void SetKeyOwner(ImGuiKey key,uint owner_id,int flags)
		{
			TitanImGui_ImGuiAPI_Visitor_SetKeyOwner_2771753469(key, owner_id, flags);
		}
		public static void MakeTabVisible(string window_name)
		{
			TitanImGui_ImGuiAPI_Visitor_MakeTabVisible_2602414842(window_name);
		}
		public static bool IsFirstFrame(string window_name)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsFirstFrame_1080422500(window_name) == 0 ? false : true;
		}
		public static bool IsLastFrame(string window_name)
		{
			return TitanImGui_ImGuiAPI_Visitor_IsLastFrame_1080422500(window_name) == 0 ? false : true;
		}
		public static ImFont GetDrawListFont(ImDrawList drawList)
		{
			return new ImFont(TitanImGui_ImGuiAPI_Visitor_GetDrawListFont_4089666882(drawList));
		}
		public static float GetDrawListFontSize(ImDrawList drawList)
		{
			return TitanImGui_ImGuiAPI_Visitor_GetDrawListFontSize_1773646793(drawList);
		}
		#endregion
		#region Core SDK
		const string ModuleNC = EngineNS.CoreSDK.CoreModule;
		//Constructor&Cast
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_UnsafeCallDestructor(void* self);
		//Fields
		//Functions
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_CreateContext_3448116237(ImFontAtlas shared_font_atlas);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DestroyContext_3034592143(void* ctx);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_GetCurrentContext_302642963();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetCurrentContext_3034592143(void* ctx);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetIniFilename_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string ini_filename);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiIO* TitanImGui_ImGuiAPI_Visitor_GetIO_289112634();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiStyle* TitanImGui_ImGuiAPI_Visitor_GetStyle_1766641991();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_NewFrame_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndFrame_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Render_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImDrawData* TitanImGui_ImGuiAPI_Visitor_GetDrawData_2577559959();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_DrawData_Textures_Size_2524852328(ImDrawData* draw_data);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_DrawData_Textures_Get_3628729028(ImDrawData* draw_data,int index);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_PlatformIO_Textures_Size_2566682924(ImGuiPlatformIO io);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_PlatformIO_Textures_Get_955167742(ImGuiPlatformIO io,int index);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetStatus_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetStatus_3475646452(void* texture_ptr,int status);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetBackendUserData_1752491917(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetBackendUserData_760721789(void* texture_ptr,void* backend_user_data);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ulong TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetTexID_2855664449(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ImTextureData_SetTexID_3373007845(void* texture_ptr,ulong tex_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetFormat_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetWidth_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetHeight_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetBytesPerPixel_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPitch_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetSizeInBytes_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPixels_1752491917(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetPixelsAt_1414832493(void* texture_ptr,int x,int y);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetUpdatesSize_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetUnusedFrames_1896157666(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetWantDestroyNextFrame_342119689(void* texture_ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetStatusName_4173149781(int status);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiAPI_Visitor_ImTextureData_GetFormatName_4173149781(int format);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ShowDemoWindow_1193269193(bool* p_open);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ShowAboutWindow_1193269193(bool* p_open);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ShowMetricsWindow_1193269193(bool* p_open);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ShowStyleEditor_3987680467(ImGuiStyle* refValue);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ShowStyleSelector_1080422500([MarshalAs(UnmanagedType.LPUTF8Str)] string label);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ShowFontSelector_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string label);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ShowUserGuide_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiAPI_Visitor_GetVersion_2396230038();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_StyleColorsDark_3987680467(ImGuiStyle* dst);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_StyleColorsClassic_3987680467(ImGuiStyle* dst);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_StyleColorsLight_3987680467(ImGuiStyle* dst);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_Begin_2979558607([MarshalAs(UnmanagedType.LPUTF8Str)] string name,bool* p_open,ImGuiWindowFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_End_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginChild_2739961343([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,EngineNS.Vector2* size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginChild_2598737060(uint id,EngineNS.Vector2* size,ImGuiChildFlags_ child_flags,ImGuiWindowFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndChild_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsWindowAppearing_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsWindowCollapsed_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsWindowFocused_1151558301(ImGuiFocusedFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsWindowHovered_2491699375(ImGuiHoveredFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetWindowDrawList_2196389917();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetWindowDpiScale_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_GetWindowViewport_4006837304();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetWindowPos_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetWindowSize_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetWindowWidth_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetWindowHeight_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowPos_1923501243(EngineNS.Vector2* pos,ImGuiCond_ cond,EngineNS.Vector2* pivot);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowSize_1791316118(EngineNS.Vector2* size,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowSizeConstraints_684930918(EngineNS.Vector2* size_min,EngineNS.Vector2* size_max,FDelegate_ImGuiSizeCallback custom_callback,void* custom_callback_data);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowContentSize_4055394152(EngineNS.Vector2* size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowCollapsed_3341245801(bool collapsed,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowFocus_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowBgAlpha_1759962673(float alpha);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowViewport_2252480719(uint viewport_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowPos_1791316118(EngineNS.Vector2* pos,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowSize_1791316118(EngineNS.Vector2* size,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowCollapsed_3341245801(bool collapsed,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowFocus_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowFocus_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowFontScale_1759962673(float scale);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowPos_1093786109([MarshalAs(UnmanagedType.LPUTF8Str)] string name,EngineNS.Vector2* pos,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowSize_1093786109([MarshalAs(UnmanagedType.LPUTF8Str)] string name,EngineNS.Vector2* size,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetWindowCollapsed_3917756216([MarshalAs(UnmanagedType.LPUTF8Str)] string name,bool collapsed,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetContentRegionMax_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetContentRegionAvail_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionMin_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionMax_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetWindowContentRegionWidth_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetScrollX_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetScrollY_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetScrollMaxX_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetScrollMaxY_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetScrollX_1759962673(float scroll_x);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetScrollY_1759962673(float scroll_y);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetScrollHereX_1759962673(float center_x_ratio);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetScrollHereY_1759962673(float center_y_ratio);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetScrollFromPosX_996365349(float local_x,float center_x_ratio);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetScrollFromPosY_996365349(float local_y,float center_y_ratio);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushFont_2187443828(ImFont font);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushFontWithSize_41148096(ImFont font,float font_size_base_unscaled);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopFont_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushStyleColor_2781163097(ImGuiCol_ idx,uint col);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushStyleColor_506689308(ImGuiCol_ idx,EngineNS.Vector4* col);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopStyleColor_4038704236(int count);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushStyleVar_1680981031(ImGuiStyleVar_ idx,float val);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushStyleVar_3737822352(ImGuiStyleVar_ idx,EngineNS.Vector2* val);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopStyleVar_4038704236(int count);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector4* TitanImGui_ImGuiAPI_Visitor_GetStyleColorVec4_3399559918(ImGuiCol_ idx);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImFont* TitanImGui_ImGuiAPI_Visitor_GetFont_2919724924();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetFontSize_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetFontTexUvWhitePixel_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetColorU32_2992096237(ImGuiCol_ idx,float alpha_mul);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetColorU32_2189523068(EngineNS.Vector4* col);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetColorU32_194637277(uint col);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushItemWidth_1759962673(float item_width);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopItemWidth_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextItemWidth_1759962673(float item_width);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_CalcItemWidth_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushTextWrapPos_1759962673(float wrap_local_pos_x);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopTextWrapPos_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushAllowKeyboardFocus_2077628183(bool allow_keyboard_focus);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopAllowKeyboardFocus_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushButtonRepeat_2077628183(bool repeat);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopButtonRepeat_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Separator_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SameLine_996365349(float offset_from_start_x,float spacing);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_NewLine_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Spacing_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Dummy_4055394152(EngineNS.Vector2* size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Indent_1759962673(float indent_w);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Unindent_1759962673(float indent_w);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_BeginGroup_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndGroup_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetCursorPos_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetCursorPosX_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetCursorPosY_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetCursorPos_4055394152(EngineNS.Vector2* local_pos);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetCursorPosX_1759962673(float local_x);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetCursorPosY_1759962673(float local_y);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetCursorStartPos_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetCursorScreenPos_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetCursorScreenPos_4055394152(EngineNS.Vector2* pos);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_AlignTextToFramePadding_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetTextLineHeight_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetTextLineHeightWithSpacing_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetFrameHeight_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetFrameHeightWithSpacing_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushID_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushID_568371421([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id_begin,[MarshalAs(UnmanagedType.LPUTF8Str)] string str_id_end);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushID_1819065180(void* ptr_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushID_4038704236(int int_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopID_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetID_1084213664([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetID_2265815331([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id_begin,[MarshalAs(UnmanagedType.LPUTF8Str)] string str_id_end);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetID_2215092506(void* ptr_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TextUnformatted_2602414842([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))] string text);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TextAsPointer_2602414842(sbyte* fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Text_2602414842([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TextColored_2591110465(EngineNS.Vector4* col,[MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TextDisabled_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TextWrapped_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LabelText_568371421([MarshalAs(UnmanagedType.LPUTF8Str)] string label,[MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_BulletText_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_Button_3507648889([MarshalAs(UnmanagedType.LPUTF8Str)] string label,EngineNS.Vector2* size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SmallButton_1080422500([MarshalAs(UnmanagedType.LPUTF8Str)] string label);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InvisibleButton_2525803684([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,EngineNS.Vector2* size,ImGuiButtonFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ArrowButton_4112088628([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiDir dir);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Arrow_543170368(ImDrawList draw_list,EngineNS.Vector2* pos,uint col,ImGuiDir dir,float scale);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Image_3718021420(ulong user_texture_id,EngineNS.Vector2* size,EngineNS.Vector2* uv0,EngineNS.Vector2* uv1,EngineNS.Vector4* tint_col,EngineNS.Vector4* border_col);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ImageButton_550850427([MarshalAs(UnmanagedType.LPUTF8Str)] string name,ulong user_texture_id,EngineNS.Vector2* size,EngineNS.Vector2* uv0,EngineNS.Vector2* uv1,EngineNS.Vector4* bg_col,EngineNS.Vector4* tint_col);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_Checkbox_2817596850([MarshalAs(UnmanagedType.LPUTF8Str)] string label,bool* v);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_CheckboxFlags_1998082816([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* flags,int flags_value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_CheckboxFlags_145502690([MarshalAs(UnmanagedType.LPUTF8Str)] string label,uint* flags,uint flags_value);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_RadioButton_3037903904([MarshalAs(UnmanagedType.LPUTF8Str)] string label,bool active);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_RadioButton_1998082816([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,int v_button);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ProgressBar_2910944627(float fraction,EngineNS.Vector2* size_arg,[MarshalAs(UnmanagedType.LPUTF8Str)] string overlay);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Bullet_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginCombo_3858370926([MarshalAs(UnmanagedType.LPUTF8Str)] string label,[MarshalAs(UnmanagedType.LPUTF8Str)] string preview_value,ImGuiComboFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginCombo_31333123([MarshalAs(UnmanagedType.LPUTF8Str)] string label,[MarshalAs(UnmanagedType.LPUTF8Str)] string preview_value,ImGuiComboFlags_ flags,ImGuiWindowFlags_ winFlags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndCombo_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_Combo_1210971960([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* current_item,sbyte** items,int items_count,int popup_max_height_in_items);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_Combo_161697717([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* current_item,[MarshalAs(UnmanagedType.LPUTF8Str)] string items_separated_by_zeros,int popup_max_height_in_items);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_Combo_3739767375([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* current_item,FDelegate_items_getter fn_getter,void* data,int items_count,int popup_max_height_in_items);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragFloat_3863841807([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float v_speed,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragFloat2_3863841807([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float v_speed,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragFloat3_3863841807([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float v_speed,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragFloat4_3863841807([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float v_speed,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragFloatRange2_3996816114([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v_current_min,float* v_current_max,float v_speed,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,[MarshalAs(UnmanagedType.LPUTF8Str)] string format_max,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragInt_3407096620([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,float v_speed,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragInt2_3407096620([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,float v_speed,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragInt3_3407096620([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,float v_speed,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragInt4_3407096620([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,float v_speed,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragIntRange2_52041202([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v_current_min,int* v_current_max,float v_speed,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,[MarshalAs(UnmanagedType.LPUTF8Str)] string format_max,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragScalar_3791056475([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiDataType_ data_type,void* p_data,float v_speed,void* p_min,void* p_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragScalarN_576259350([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiDataType_ data_type,void* p_data,int components,float v_speed,void* p_min,void* p_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderFloat_281608583([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderFloat2_281608583([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderFloat3_281608583([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderFloat4_281608583([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderAngle_281608583([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v_rad,float v_degrees_min,float v_degrees_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderInt_4038701670([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderInt2_4038701670([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderInt3_4038701670([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderInt4_4038701670([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderScalar_2997903363([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiDataType_ data_type,void* p_data,void* p_min,void* p_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SliderScalarN_4051498348([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiDataType_ data_type,void* p_data,int components,void* p_min,void* p_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_VSliderFloat_2008079404([MarshalAs(UnmanagedType.LPUTF8Str)] string label,EngineNS.Vector2* size,float* v,float v_min,float v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_VSliderInt_1088819369([MarshalAs(UnmanagedType.LPUTF8Str)] string label,EngineNS.Vector2* size,int* v,int v_min,int v_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_VSliderScalar_1401309246([MarshalAs(UnmanagedType.LPUTF8Str)] string label,EngineNS.Vector2* size,ImGuiDataType_ data_type,void* p_data,void* p_min,void* p_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputText_530115699([MarshalAs(UnmanagedType.LPUTF8Str)] string label,void* buf,uint buf_size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputTextNoName_530115699([MarshalAs(UnmanagedType.LPUTF8Str)] string label,void* buf,uint buf_size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputTextMultiline_1095533176([MarshalAs(UnmanagedType.LPUTF8Str)] string label,[MarshalAs(UnmanagedType.LPUTF8Str)] string buf,uint buf_size,EngineNS.Vector2* size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputTextWithHint_433703410([MarshalAs(UnmanagedType.LPUTF8Str)] string label,[MarshalAs(UnmanagedType.LPUTF8Str)] string hint,[MarshalAs(UnmanagedType.LPUTF8Str)] string buf,uint buf_size,ImGuiInputTextFlags_ flags,FDelegate_ImGuiInputTextCallback callback,void* user_data);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputFloat_3636630689([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,float step,float step_fast,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputFloat2_1399196753([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputFloat3_1399196753([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputFloat4_1399196753([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* v,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputInt_4119295329([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,int step,int step_fast,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputInt2_772478873([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputInt3_772478873([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputInt4_772478873([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputDouble_4137560224([MarshalAs(UnmanagedType.LPUTF8Str)] string label,double* v,double step,double step_fast,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputScalar_1030920285([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiDataType_ data_type,void* p_data,void* p_step,void* p_step_fast,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_InputScalarN_3243426466([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiDataType_ data_type,void* p_data,int components,void* p_step,void* p_step_fast,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiInputTextFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ColorEdit3_833964466([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* col,ImGuiColorEditFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ColorEdit4_833964466([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* col,ImGuiColorEditFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ColorPicker3_833964466([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* col,ImGuiColorEditFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ColorPicker4_2514436871([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* col,ImGuiColorEditFlags_ flags,float* _col2);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ColorButton_4115823885([MarshalAs(UnmanagedType.LPUTF8Str)] string desc_id,EngineNS.Vector4* col,ImGuiColorEditFlags_ flags,EngineNS.Vector2* size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetColorEditOptions_4022841967(ImGuiColorEditFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TreeNode_1080422500([MarshalAs(UnmanagedType.LPUTF8Str)] string label);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TreeNode_459238835([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,[MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TreeNode_3207787083(void* ptr_id,[MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2036359645([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiTreeNodeFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2368787316([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiTreeNodeFlags_ flags,[MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TreeNodeEx_2410412098(void* ptr_id,ImGuiTreeNodeFlags_ flags,[MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TreePush_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TreePush_1819065180(void* ptr_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TreePop_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetTreeNodeToLabelSpacing_3743936629();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_2036359645([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiTreeNodeFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_2891925945([MarshalAs(UnmanagedType.LPUTF8Str)] string label,bool* p_open,ImGuiTreeNodeFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextItemOpen_3341245801(bool is_open,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_Selectable_1745889278([MarshalAs(UnmanagedType.LPUTF8Str)] string label,bool selected,ImGuiSelectableFlags_ flags,EngineNS.Vector2* size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_Selectable_778409408([MarshalAs(UnmanagedType.LPUTF8Str)] string label,bool* p_selected,ImGuiSelectableFlags_ flags,EngineNS.Vector2* size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ListBox_1210971960([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* current_item,sbyte** items,int items_count,int height_in_items);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ListBox_3739767375([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* current_item,FDelegate_items_getter fn_getter,void* data,int items_count,int height_in_items);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PlotLines_3358809591([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* values,int values_count,int values_offset,[MarshalAs(UnmanagedType.LPUTF8Str)] string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size,int stride);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PlotLines_403696211([MarshalAs(UnmanagedType.LPUTF8Str)] string label,FDelegate_values_getter fn_getter,void* data,int values_count,int values_offset,[MarshalAs(UnmanagedType.LPUTF8Str)] string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PlotHistogram_3358809591([MarshalAs(UnmanagedType.LPUTF8Str)] string label,float* values,int values_count,int values_offset,[MarshalAs(UnmanagedType.LPUTF8Str)] string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size,int stride);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PlotHistogram_403696211([MarshalAs(UnmanagedType.LPUTF8Str)] string label,FDelegate_values_getter fn_getter,void* data,int values_count,int values_offset,[MarshalAs(UnmanagedType.LPUTF8Str)] string overlay_text,float scale_min,float scale_max,EngineNS.Vector2 graph_size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Value_3791955190([MarshalAs(UnmanagedType.LPUTF8Str)] string prefix,bool b);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Value_2553264241([MarshalAs(UnmanagedType.LPUTF8Str)] string prefix,int v);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Value_641062864([MarshalAs(UnmanagedType.LPUTF8Str)] string prefix,uint v);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Value_3548670745([MarshalAs(UnmanagedType.LPUTF8Str)] string prefix,float v,[MarshalAs(UnmanagedType.LPUTF8Str)] string float_format);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginMenuBar_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndMenuBar_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginMainMenuBar_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndMainMenuBar_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginMenu_3037903904([MarshalAs(UnmanagedType.LPUTF8Str)] string label,bool enabled);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndMenu_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_MenuItem_1086805043([MarshalAs(UnmanagedType.LPUTF8Str)] string label,[MarshalAs(UnmanagedType.LPUTF8Str)] string shortcut,bool selected,bool enabled);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_MenuItem_3678366017([MarshalAs(UnmanagedType.LPUTF8Str)] string label,[MarshalAs(UnmanagedType.LPUTF8Str)] string shortcut,bool* p_selected,bool enabled);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginTooltip_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndTooltip_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetTooltip_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginPopup_2063302891([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiWindowFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginPopupModal_2979558607([MarshalAs(UnmanagedType.LPUTF8Str)] string name,bool* p_open,ImGuiWindowFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndPopup_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_OpenPopup_780651675([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiPopupFlags_ popup_flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_OpenPopupOnItemClick_780651675([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiPopupFlags_ popup_flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_CloseCurrentPopup_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginPopupContextItem_3980950421([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiPopupFlags_ popup_flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginPopupContextWindow_3980950421([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiPopupFlags_ popup_flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginPopupContextVoid_3980950421([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiPopupFlags_ popup_flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsPopupOpen_3980950421([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiPopupFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Columns_1241066619(int count,[MarshalAs(UnmanagedType.LPUTF8Str)] string id,bool border);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_NextColumn_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_GetColumnIndex_2704135706();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetColumnWidth_1859829344(int column_index);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetColumnWidth_2140556032(int column_index,float width);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetColumnOffset_1859829344(int column_index);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetColumnOffset_2140556032(int column_index,float offset_x);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_GetColumnsCount_2704135706();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginTable_592367644([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,int column,ImGuiTableFlags_ flags,EngineNS.Vector2* outer_size,float inner_width);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndTable_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableNextRow_2950496009(ImGuiTableRowFlags_ row_flags,float min_row_height);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TableNextColumn_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TableSetColumnIndex_1125491426(int column_n);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableSetupColumn_2358737958([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiTableColumnFlags_ flags,float init_width_or_weight,uint user_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableSetupScrollFreeze_3539386109(int cols,int rows);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableHeadersRow_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableHeader_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string label);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_TableGetColumnCount_2704135706();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_TableGetColumnIndex_2704135706();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_TableGetRowIndex_2704135706();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiAPI_Visitor_TableGetColumnName_4173149781(int column_n);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_TableGetColumnFlags_3804733202(int column_n);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableSetColumnEnabled_2814434660(int column_n,bool v);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableSetBgColor_3948758695(ImGuiTableBgTarget_ target,uint color,int column_n);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginTabBar_697337101([MarshalAs(UnmanagedType.LPUTF8Str)] string str_id,ImGuiTabBarFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndTabBar_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginTabItem_1317613329([MarshalAs(UnmanagedType.LPUTF8Str)] string label,bool* p_open,ImGuiTabItemFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndTabItem_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TabItemButton_1489982093([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiTabItemFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetTabItemClosed_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string tab_or_docked_window_label);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_DockSpace_737848097(uint id,EngineNS.Vector2* size,ImGuiDockNodeFlags_ flags,ImGuiWindowClass* window_class);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_DockSpaceOverViewport_3195982265(uint dock_id,ImGuiViewport* viewport,ImGuiDockNodeFlags_ flags,ImGuiWindowClass* window_class);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowDockID_3620127785(uint dock_id,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextWindowClass_2280511539(ImGuiWindowClass* window_class);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetWindowDockID_3529484159();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsWindowDocked_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DockBuilderDockWindow_2353073608([MarshalAs(UnmanagedType.LPUTF8Str)] string window_name,uint node_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_DockBuilderAddNode_1731294598(uint node_id,ImGuiDockNodeFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNode_2252480719(uint node_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNodeDockedWindows_1053065717(uint node_id,bool clear_settings_refs);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DockBuilderRemoveNodeChildNodes_2252480719(uint node_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DockBuilderSetNodePos_3396750409(uint node_id,EngineNS.Vector2 pos);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DockBuilderSetNodeSize_3396750409(uint node_id,EngineNS.Vector2 size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_DockBuilderSplitNode_230192101(uint node_id,ImGuiDir split_dir,float size_ratio_for_node_at_dir,uint* out_id_at_dir,uint* out_id_at_opposite_dir);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DockBuilderCopyWindowSettings_568371421([MarshalAs(UnmanagedType.LPUTF8Str)] string src_name,[MarshalAs(UnmanagedType.LPUTF8Str)] string dst_name);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DockBuilderFinish_2252480719(uint node_id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LogToTTY_4038704236(int auto_open_depth);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LogToFile_85734681(int auto_open_depth,[MarshalAs(UnmanagedType.LPUTF8Str)] string filename);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LogToClipboard_4038704236(int auto_open_depth);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LogFinish_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LogButtons_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LogText_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string fmt);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginDragDropSource_4036233765(ImGuiDragDropFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_SetDragDropPayload_411387055([MarshalAs(UnmanagedType.LPUTF8Str)] string type,void* data,uint sz,ImGuiCond_ cond);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndDragDropSource_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_BeginDragDropTarget_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiPayload* TitanImGui_ImGuiAPI_Visitor_AcceptDragDropPayload_311770([MarshalAs(UnmanagedType.LPUTF8Str)] string type,ImGuiDragDropFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_EndDragDropTarget_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiPayload* TitanImGui_ImGuiAPI_Visitor_GetDragDropPayload_2736951583();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PushClipRect_229364439(EngineNS.Vector2* clip_rect_min,EngineNS.Vector2* clip_rect_max,bool intersect_with_current_clip_rect);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PopClipRect_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetItemDefaultFocus_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetKeyboardFocusHere_4038704236(int offset);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemHovered_2491699375(ImGuiHoveredFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemActive_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemFocused_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemClicked_361246070(ImGuiMouseButton_ mouse_button);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemDoubleClicked_361246070(ImGuiMouseButton_ mouse_button);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemVisible_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemEdited_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemActivated_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemDeactivated_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemDeactivatedAfterEdit_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsItemToggledOpen_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsAnyItemHovered_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsAnyItemActive_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsAnyItemFocused_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetItemRectMin_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetItemRectMax_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetItemRectSize_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsRectVisible_4039732974(EngineNS.Vector2* size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsRectVisible_690184979(EngineNS.Vector2* rect_min,EngineNS.Vector2* rect_max);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static double TitanImGui_ImGuiAPI_Visitor_GetTime_4162959082();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_GetFrameCount_2704135706();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetBackgroundDrawList_2196389917();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetForegroundDrawList_2196389917();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetBackgroundDrawList_1456041080(ImGuiViewport* viewport);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImDrawList* TitanImGui_ImGuiAPI_Visitor_GetForegroundDrawList_1456041080(ImGuiViewport* viewport);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_GetDrawListSharedData_302642963();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiAPI_Visitor_GetStyleColorName_1027210462(ImGuiCol_ idx);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetStateStorage_1198396241(ImGuiStorage storage);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiStorage* TitanImGui_ImGuiAPI_Visitor_GetStateStorage_479204201();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetNextItemAllowOverlap_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_CalcTextSize_2234397086([MarshalAs(UnmanagedType.LPUTF8Str)] string text,bool hide_text_after_double_hash,float wrap_width);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector4 TitanImGui_ImGuiAPI_Visitor_ColorConvertU32ToFloat4_4044967189(uint inValue);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_ColorConvertFloat4ToU32_2189523068(EngineNS.Vector4* inValue);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ColorConvertRGBtoHSV_3904097195(float r,float g,float b,float* out_h,float* out_s,float* out_v);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ColorConvertHSVtoRGB_3904097195(float h,float s,float v,float* out_r,float* out_g,float* out_b);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsKeyDown_664544503(ImGuiKey user_key_index);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsKeyPressed_1790704165(ImGuiKey user_key_index,bool repeat);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsKeyReleased_664544503(ImGuiKey user_key_index);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_GetKeyPressedAmount_1868481138(ImGuiKey key_index,float repeat_delay,float rate);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseDown_361246070(ImGuiMouseButton_ button);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseClicked_3257368754(ImGuiMouseButton_ button,bool repeat);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseReleased_361246070(ImGuiMouseButton_ button);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseDoubleClicked_361246070(ImGuiMouseButton_ button);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_GetMouseClickedCount_3860407425(ImGuiMouseButton_ button);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseHoveringRect_1978284909(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,bool clip);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMousePosValid_4039732974(EngineNS.Vector2* mouse_pos);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsAnyMouseDown_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetMousePos_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetMousePosOnOpeningCurrentPopup_558510083();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseDragging_2505297786(ImGuiMouseButton_ button,float lock_threshold);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_GetMouseDragDelta_4262098684(ImGuiMouseButton_ button,float lock_threshold);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ResetMouseDragDelta_269877056(ImGuiMouseButton_ button);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiMouseCursor_ TitanImGui_ImGuiAPI_Visitor_GetMouseCursor_345306920();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetMouseCursor_3384190518(ImGuiMouseCursor_ cursor_type);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiAPI_Visitor_GetClipboardText_2396230038();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetClipboardText_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string text);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LoadIniSettingsFromDisk_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string ini_filename);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_LoadIniSettingsFromMemory_3999832562([MarshalAs(UnmanagedType.LPUTF8Str)] string ini_data,uint ini_size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SaveIniSettingsToDisk_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string ini_filename);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte* TitanImGui_ImGuiAPI_Visitor_SaveIniSettingsToMemory_814920808(uint* out_ini_size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DebugCheckVersionAndDataLayout_204246052([MarshalAs(UnmanagedType.LPUTF8Str)] string version_str,IntPtr sz_io,IntPtr sz_style,IntPtr sz_vec2,IntPtr sz_vec4,IntPtr sz_drawvert,IntPtr sz_drawidx);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetAllocatorFunctions_3510213992(FDelegate_alloc_func fn_alloc_func,FDelegate_free_func fn_free_func,void* user_data);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void* TitanImGui_ImGuiAPI_Visitor_MemAlloc_1539568017(IntPtr size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_MemFree_3034592143(void* ptr);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiPlatformIO* TitanImGui_ImGuiAPI_Visitor_GetPlatformIO_971686321();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_GetMainViewport_4006837304();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_UpdatePlatformWindows_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_RenderPlatformWindowsDefault_760721789(void* platform_render_arg,void* renderer_render_arg);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_DestroyPlatformWindows_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_FindViewportByID_1024491770(uint id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_FindViewportByPlatformHandle_4148205652(void* platform_handle);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_CreateWindow_3699537699(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_CreateWindow fn);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_DestroyWindow_1336333681(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_CreateWindow fn);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_SetWindowSize_3092220344(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_SetWindowSize fn);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_RenderWindow_2851598185(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_RenderWindow fn);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_Set_Renderer_SwapBuffers_593121533(ImGuiPlatformIO PlatformIO,FDelegate_Renderer_RenderWindow fn);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PlatformIO_Monitor_Resize_1567199252(ImGuiPlatformIO io,int size);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_PlatformIO_Monitor_PushBack_1937089321(ImGuiPlatformIO io,ImGuiPlatformMonitor monitor);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static int TitanImGui_ImGuiAPI_Visitor_PlatformIO_Viewports_Size_2566682924(ImGuiPlatformIO io);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImGuiViewport* TitanImGui_ImGuiAPI_Visitor_PlatformIO_Viewports_Get_2057355863(ImGuiPlatformIO io,int index);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TextInputComboBox_899144233([MarshalAs(UnmanagedType.LPUTF8Str)] string id,void* buffer,uint maxInputSize,sbyte** items,uint item_len,short showMaxItems);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_GetClipboardTextSetter_881800347(ImGuiIO io,FDelegate_FGetClipboardTextFn fn);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetClipboardTextSetter_716060207(ImGuiIO io,FDelegate_FSetClipboardTextFn fn);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ItemSize_1990263409(EngineNS.Vector2* min,EngineNS.Vector2* max,float text_baseline_y);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ItemSize_2116332084(EngineNS.Vector2* size,float text_baseline_y);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ItemAdd_1999737362(EngineNS.Vector2* bbMin,EngineNS.Vector2* bbMax,uint id,EngineNS.Vector2* nav_bb_min,EngineNS.Vector2* nav_bb_max,int flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ItemAdd_3017646930(EngineNS.Vector2* bbMin,EngineNS.Vector2* bbMax,uint id,int flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_GetTableWorkRect_2631331155(EngineNS.Vector2* min,EngineNS.Vector2* max);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_GetTableRowStartY_2989815025(float* yValue);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_GetTableRowEndY_2989815025(float* yValue);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsHoverCurrentWindow_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseHoveringRectInCurrentWindow_1978284909(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,bool clip);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseDownInRectInCurrentWindow_3682308850(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,ImGuiMouseButton_ button,bool clip);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseClickedInRectInCurrentWindow_3682308850(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,ImGuiMouseButton_ button,bool clip);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseDoubleClickedInRectInCurrentWindow_3682308850(EngineNS.Vector2* r_min,EngineNS.Vector2* r_max,ImGuiMouseButton_ button,bool clip);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsMouseDragPastThreshold_2505297786(ImGuiMouseButton_ button,float lock_threshold);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsCurrentWindowSkipItems_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ItemHoverable_1371559414(EngineNS.Vector2* bbMin,EngineNS.Vector2* bbMax,uint id,int item_flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_TempInputIsActive_900990169(uint id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetActiveID_2252480719(uint id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetActiveID_3529484159();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetFocusID_2252480719(uint id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetTempInputID_2252480719(uint id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static uint TitanImGui_ImGuiAPI_Visitor_GetTempInputID_3529484159();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_ClearActiveID_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_FocusCurrentWindow_2960189489();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsIDNavActivated_900990169(uint id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsIDNavInput_900990169(uint id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragBehavior_310431169(uint id,ImGuiDataType_ data_type,void* p_v,float v_speed,void* p_min,void* p_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragActiveIdUpdate_900990169(uint id);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ButtonBehavior_1167781018(EngineNS.Vector2* min,EngineNS.Vector2* max,uint id,bool* out_hovered,bool* out_held,bool pressOnRelease,ImGuiButtonFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragScalar2_220901116([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiDataType_ data_type,void* p_data,float v_speed,void* p_min,void* p_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_DragScalarN2_3862891275([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiDataType_ data_type,void* p_data,int components,float v_speed,void* p_min,void* p_max,[MarshalAs(UnmanagedType.LPUTF8Str)] string format,ImGuiSliderFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_RenderFrame_539721179(EngineNS.Vector2* p_min,EngineNS.Vector2* p_max,uint fill_col,bool border,float rounding);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static EngineNS.Vector2 TitanImGui_ImGuiAPI_Visitor_CalcItemSize_3095670423(EngineNS.Vector2* size,float default_w,float default_h);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_CollapsingHeader_SpanAllColumns_2036359645([MarshalAs(UnmanagedType.LPUTF8Str)] string label,ImGuiTreeNodeFlags_ flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsInTable_1117990983();
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableNextRow_589053033(EngineNS.ImGuiTableRowData* rowData);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_TableNextRow_FirstColumn_589053033(EngineNS.ImGuiTableRowData* rowData);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_CheckBoxTristate_263170625([MarshalAs(UnmanagedType.LPUTF8Str)] string label,int* v_tristate);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_ToggleButton_853224535([MarshalAs(UnmanagedType.LPUTF8Str)] string label,bool* v,EngineNS.Vector2* size_arg,int flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_SetKeyOwner_2771753469(ImGuiKey key,uint owner_id,int flags);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static void TitanImGui_ImGuiAPI_Visitor_MakeTabVisible_2602414842([MarshalAs(UnmanagedType.LPUTF8Str)] string window_name);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsFirstFrame_1080422500([MarshalAs(UnmanagedType.LPUTF8Str)] string window_name);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static sbyte TitanImGui_ImGuiAPI_Visitor_IsLastFrame_1080422500([MarshalAs(UnmanagedType.LPUTF8Str)] string window_name);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static ImFont* TitanImGui_ImGuiAPI_Visitor_GetDrawListFont_4089666882(ImDrawList drawList);
		[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		extern static float TitanImGui_ImGuiAPI_Visitor_GetDrawListFontSize_1773646793(ImDrawList drawList);
		//Cast
		#endregion
	}
}
