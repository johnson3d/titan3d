//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


public unsafe partial struct ImFont : EngineNS.IPtrType
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 80, Pack = 8)]
	public struct CppStructLayout
	{
		[System.Runtime.InteropServices.FieldOffset(8)]
		public ImFontAtlas* OwnerAtlas;
		[System.Runtime.InteropServices.FieldOffset(16)]
		public int Flags;
		[System.Runtime.InteropServices.FieldOffset(20)]
		public float CurrentRasterizerDensity;
		[System.Runtime.InteropServices.FieldOffset(24)]
		public uint FontId;
		[System.Runtime.InteropServices.FieldOffset(28)]
		public float LegacySize;
		[System.Runtime.InteropServices.FieldOffset(48)]
		public Wchar16 EllipsisChar;
		[System.Runtime.InteropServices.FieldOffset(50)]
		public Wchar16 FallbackChar;
		[System.Runtime.InteropServices.FieldOffset(52)]
		public byte* Used8kPagesMap;
		[System.Runtime.InteropServices.FieldOffset(53)]
		public bool EllipsisAutoBake;
		[System.Runtime.InteropServices.FieldOffset(56)]
		public ImGuiStorage RemapPairs;
		[System.Runtime.InteropServices.FieldOffset(72)]
		public float Scale;
	}
	private void* mPointer;
	public CppStructLayout* UnsafeAsLayout { get => (CppStructLayout*)mPointer; }
	public ImFont(void* p) { mPointer = p; }
	public void UnsafeSetPointer(void* p) { mPointer = p; }
	public IntPtr NativePointer { get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }
	public ImFont* CppPointer { get => (ImFont*)mPointer; }
	public bool IsValidPointer { get => mPointer != (void*)0; }
	public static implicit operator ImFont* (ImFont v)
	{
		return (ImFont*)v.mPointer;
	}
	#region Constructor&Cast
	public static EngineNS.FRttiStruct GetTypeRtti()
	{
		return new EngineNS.FRttiStruct(TitanImGui_ImFont_Visitor_GetTypeRtti());
	}
	public static ImFont CreateInstance()
	{
		return new ImFont(TitanImGui_ImFont_Visitor_CreateInstance_2960189489());
	}
	#endregion
	#region Fields
	public ImFontAtlas OwnerAtlas
	{
		get
		{
			return new ImFontAtlas(TitanImGui_ImFont_Visitor_FieldGet__OwnerAtlas(mPointer));
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__OwnerAtlas(mPointer, value);
		}
	}
	public int Flags
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__Flags(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__Flags(mPointer, value);
		}
	}
	public float CurrentRasterizerDensity
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__CurrentRasterizerDensity(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__CurrentRasterizerDensity(mPointer, value);
		}
	}
	public uint FontId
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__FontId(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__FontId(mPointer, value);
		}
	}
	public float LegacySize
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__LegacySize(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__LegacySize(mPointer, value);
		}
	}
	public Wchar16 EllipsisChar
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__EllipsisChar(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__EllipsisChar(mPointer, value);
		}
	}
	public Wchar16 FallbackChar
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__FallbackChar(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__FallbackChar(mPointer, value);
		}
	}
	public byte* Used8kPagesMap
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__Used8kPagesMap(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__Used8kPagesMap(mPointer, value);
		}
	}
	public bool EllipsisAutoBake
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__EllipsisAutoBake(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__EllipsisAutoBake(mPointer, value);
		}
	}
	public ImGuiStorage RemapPairs
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__RemapPairs(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__RemapPairs(mPointer, value);
		}
	}
	public float Scale
	{
		get
		{
			return TitanImGui_ImFont_Visitor_FieldGet__Scale(mPointer);
		}
		set
		{
			TitanImGui_ImFont_Visitor_FieldSet__Scale(mPointer, value);
		}
	}
	#endregion
	#region Function
	public bool IsGlyphInFont(Wchar16 c)
	{
		return TitanImGui_ImFont_Visitor_IsGlyphInFont_1393165236(mPointer, c) == 0 ? false : true;
	}
	public bool IsLoaded()
	{
		return TitanImGui_ImFont_Visitor_IsLoaded_82051314(mPointer) == 0 ? false : true;
	}
	public string GetDebugName()
	{
		return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImFont_Visitor_GetDebugName_721684103(mPointer));
	}
	public EngineNS.Vector2 CalcTextSizeA(float size,float max_width,float wrap_width,string text_begin,string text_end,sbyte** out_remaining)
	{
		return TitanImGui_ImFont_Visitor_CalcTextSizeA_2915923972(mPointer, size, max_width, wrap_width, text_begin, text_end, out_remaining);
	}
	public string CalcWordWrapPosition(float size,string text,string text_end,float wrap_width)
	{
		return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImFont_Visitor_CalcWordWrapPosition_3038693210(mPointer, size, text, text_end, wrap_width));
	}
	public void RenderChar(ImDrawList draw_list,float size,EngineNS.Vector2* pos,uint col,Wchar16 c,EngineNS.Vector4* cpu_fine_clip)
	{
		TitanImGui_ImFont_Visitor_RenderChar_2818842580(mPointer, draw_list, size, pos, col, c, cpu_fine_clip);
	}
	public void RenderChar(ImDrawList draw_list,float size, in EngineNS.Vector2 pos,uint col,Wchar16 c, in EngineNS.Vector4 cpu_fine_clip)
	{
		fixed(EngineNS.Vector2* pinned_pos = &pos)
		fixed(EngineNS.Vector4* pinned_cpu_fine_clip = &cpu_fine_clip)
		{
			RenderChar(draw_list, size, pinned_pos, col, c, pinned_cpu_fine_clip);
		}
	}
	public void RenderText(ImDrawList draw_list,float size,EngineNS.Vector2* pos,uint col,EngineNS.Vector4* clip_rect,string text_begin,string text_end,float wrap_width,int flags)
	{
		TitanImGui_ImFont_Visitor_RenderText_3447178593(mPointer, draw_list, size, pos, col, clip_rect, text_begin, text_end, wrap_width, flags);
	}
	public void RenderText(ImDrawList draw_list,float size, in EngineNS.Vector2 pos,uint col, in EngineNS.Vector4 clip_rect,string text_begin,string text_end,float wrap_width,int flags)
	{
		fixed(EngineNS.Vector2* pinned_pos = &pos)
		fixed(EngineNS.Vector4* pinned_clip_rect = &clip_rect)
		{
			RenderText(draw_list, size, pinned_pos, col, pinned_clip_rect, text_begin, text_end, wrap_width, flags);
		}
	}
	public string CalcWordWrapPositionA(float scale,string text,string text_end,float wrap_width)
	{
		return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImFont_Visitor_CalcWordWrapPositionA_3038693210(mPointer, scale, text, text_end, wrap_width));
	}
	public void ClearOutputData()
	{
		TitanImGui_ImFont_Visitor_ClearOutputData_2960189489(mPointer);
	}
	public void AddRemapChar(Wchar16 from_codepoint,Wchar16 to_codepoint)
	{
		TitanImGui_ImFont_Visitor_AddRemapChar_1017310781(mPointer, from_codepoint, to_codepoint);
	}
	public bool IsGlyphRangeUnused(uint c_begin,uint c_last)
	{
		return TitanImGui_ImFont_Visitor_IsGlyphRangeUnused_2243309331(mPointer, c_begin, c_last) == 0 ? false : true;
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.FRttiStruct* TitanImGui_ImFont_Visitor_GetTypeRtti();
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFont_Visitor_CreateInstance_2960189489();
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImFontAtlas* TitanImGui_ImFont_Visitor_FieldGet__OwnerAtlas(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__OwnerAtlas(void* self, ImFontAtlas* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFont_Visitor_FieldGet__Flags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__Flags(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFont_Visitor_FieldGet__CurrentRasterizerDensity(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__CurrentRasterizerDensity(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImFont_Visitor_FieldGet__FontId(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__FontId(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFont_Visitor_FieldGet__LegacySize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__LegacySize(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static Wchar16 TitanImGui_ImFont_Visitor_FieldGet__EllipsisChar(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__EllipsisChar(void* self, Wchar16 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static Wchar16 TitanImGui_ImFont_Visitor_FieldGet__FallbackChar(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__FallbackChar(void* self, Wchar16 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static byte* TitanImGui_ImFont_Visitor_FieldGet__Used8kPagesMap(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__Used8kPagesMap(void* self, byte* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFont_Visitor_FieldGet__EllipsisAutoBake(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__EllipsisAutoBake(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiStorage TitanImGui_ImFont_Visitor_FieldGet__RemapPairs(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__RemapPairs(void* self, ImGuiStorage value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFont_Visitor_FieldGet__Scale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFont_Visitor_FieldSet__Scale(void* self, float value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImFont_Visitor_IsGlyphInFont_1393165236(void* Self, Wchar16 c);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImFont_Visitor_IsLoaded_82051314(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte* TitanImGui_ImFont_Visitor_GetDebugName_721684103(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.Vector2 TitanImGui_ImFont_Visitor_CalcTextSizeA_2915923972(void* Self, float size,float max_width,float wrap_width,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_begin,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_end,sbyte** out_remaining);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte* TitanImGui_ImFont_Visitor_CalcWordWrapPosition_3038693210(void* Self, float size,[MarshalAs(UnmanagedType.LPUTF8Str)] string text,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_end,float wrap_width);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFont_Visitor_RenderChar_2818842580(void* Self, ImDrawList draw_list,float size,EngineNS.Vector2* pos,uint col,Wchar16 c,EngineNS.Vector4* cpu_fine_clip);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFont_Visitor_RenderText_3447178593(void* Self, ImDrawList draw_list,float size,EngineNS.Vector2* pos,uint col,EngineNS.Vector4* clip_rect,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_begin,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_end,float wrap_width,int flags);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte* TitanImGui_ImFont_Visitor_CalcWordWrapPositionA_3038693210(void* Self, float scale,[MarshalAs(UnmanagedType.LPUTF8Str)] string text,[MarshalAs(UnmanagedType.LPUTF8Str)] string text_end,float wrap_width);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFont_Visitor_ClearOutputData_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFont_Visitor_AddRemapChar_1017310781(void* Self, Wchar16 from_codepoint,Wchar16 to_codepoint);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImFont_Visitor_IsGlyphRangeUnused_2243309331(void* Self, uint c_begin,uint c_last);
	//Cast
	#endregion
}
