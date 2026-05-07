//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


public unsafe partial struct ImFontAtlas : EngineNS.IPtrType, IDisposable
{
	[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 760, Pack = 8)]
	public struct CppStructLayout
	{
		[System.Runtime.InteropServices.FieldOffset(0)]
		public int Flags;
		[System.Runtime.InteropServices.FieldOffset(8)]
		public int TexGlyphPadding;
		[System.Runtime.InteropServices.FieldOffset(12)]
		public int TexMinWidth;
		[System.Runtime.InteropServices.FieldOffset(16)]
		public int TexMinHeight;
		[System.Runtime.InteropServices.FieldOffset(20)]
		public int TexMaxWidth;
		[System.Runtime.InteropServices.FieldOffset(24)]
		public int TexMaxHeight;
		[System.Runtime.InteropServices.FieldOffset(32)]
		public void* UserData;
		[System.Runtime.InteropServices.FieldOffset(80)]
		public bool Locked;
		[System.Runtime.InteropServices.FieldOffset(81)]
		public bool RendererHasTextures;
		[System.Runtime.InteropServices.FieldOffset(82)]
		public bool TexIsBuilt;
		[System.Runtime.InteropServices.FieldOffset(83)]
		public bool TexPixelsUseColors;
		[System.Runtime.InteropServices.FieldOffset(84)]
		public EngineNS.Vector2 TexUvScale;
		[System.Runtime.InteropServices.FieldOffset(92)]
		public EngineNS.Vector2 TexUvWhitePixel;
		[System.Runtime.InteropServices.FieldOffset(136)]
		public EngineNS.Vector4* TexUvLines;
		[System.Runtime.InteropServices.FieldOffset(664)]
		public int TexNextUniqueID;
		[System.Runtime.InteropServices.FieldOffset(668)]
		public int FontNextUniqueID;
		[System.Runtime.InteropServices.FieldOffset(704)]
		public sbyte* FontLoaderName;
		[System.Runtime.InteropServices.FieldOffset(712)]
		public void* FontLoaderData;
		[System.Runtime.InteropServices.FieldOffset(720)]
		public uint FontLoaderFlags;
		[System.Runtime.InteropServices.FieldOffset(724)]
		public int RefCount;
	}
	private void* mPointer;
	public CppStructLayout* UnsafeAsLayout { get => (CppStructLayout*)mPointer; }
	public ImFontAtlas(void* p) { mPointer = p; }
	public void UnsafeSetPointer(void* p) { mPointer = p; }
	public IntPtr NativePointer { get => (IntPtr)mPointer; set => mPointer = value.ToPointer(); }
	public ImFontAtlas* CppPointer { get => (ImFontAtlas*)mPointer; }
	public bool IsValidPointer { get => mPointer != (void*)0; }
	public static implicit operator ImFontAtlas* (ImFontAtlas v)
	{
		return (ImFontAtlas*)v.mPointer;
	}
	#region Constructor&Cast
	public static EngineNS.FRttiStruct GetTypeRtti()
	{
		return new EngineNS.FRttiStruct(TitanImGui_ImFontAtlas_Visitor_GetTypeRtti());
	}
	public static ImFontAtlas CreateInstance()
	{
		return new ImFontAtlas(TitanImGui_ImFontAtlas_Visitor_CreateInstance_2960189489());
	}
	public void Dispose()
	{
		TitanImGui_ImFontAtlas_Visitor_Dispose(mPointer);
	}
	#endregion
	#region Fields
	public int Flags
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__Flags(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__Flags(mPointer, value);
		}
	}
	public int TexGlyphPadding
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexGlyphPadding(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexGlyphPadding(mPointer, value);
		}
	}
	public int TexMinWidth
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMinWidth(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMinWidth(mPointer, value);
		}
	}
	public int TexMinHeight
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMinHeight(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMinHeight(mPointer, value);
		}
	}
	public int TexMaxWidth
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMaxWidth(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMaxWidth(mPointer, value);
		}
	}
	public int TexMaxHeight
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMaxHeight(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMaxHeight(mPointer, value);
		}
	}
	public void* UserData
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__UserData(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__UserData(mPointer, value);
		}
	}
	public bool Locked
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__Locked(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__Locked(mPointer, value);
		}
	}
	public bool RendererHasTextures
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__RendererHasTextures(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__RendererHasTextures(mPointer, value);
		}
	}
	public bool TexIsBuilt
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexIsBuilt(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexIsBuilt(mPointer, value);
		}
	}
	public bool TexPixelsUseColors
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexPixelsUseColors(mPointer) == 0 ? false : true;
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexPixelsUseColors(mPointer, value);
		}
	}
	public EngineNS.Vector2 TexUvScale
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvScale(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvScale(mPointer, value);
		}
	}
	public EngineNS.Vector2 TexUvWhitePixel
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvWhitePixel(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvWhitePixel(mPointer, value);
		}
	}
	public EngineNS.Vector4* TexUvLines
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvLines(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvLines(mPointer, value);
		}
	}
	public int TexNextUniqueID
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__TexNextUniqueID(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__TexNextUniqueID(mPointer, value);
		}
	}
	public int FontNextUniqueID
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__FontNextUniqueID(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__FontNextUniqueID(mPointer, value);
		}
	}
	public string FontLoaderName
	{
		get
		{
			return EngineNS.Rtti.TtNativeCoreProvider.MarshalPtrUtf8((IntPtr)TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderName(mPointer));
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderName(mPointer, value);
		}
	}
	public void* FontLoaderData
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderData(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderData(mPointer, value);
		}
	}
	public uint FontLoaderFlags
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderFlags(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderFlags(mPointer, value);
		}
	}
	public int RefCount
	{
		get
		{
			return TitanImGui_ImFontAtlas_Visitor_FieldGet__RefCount(mPointer);
		}
		set
		{
			TitanImGui_ImFontAtlas_Visitor_FieldSet__RefCount(mPointer, value);
		}
	}
	#endregion
	#region Function
	public ImFont AddFont(ImFontConfig* font_cfg)
	{
		return new ImFont(TitanImGui_ImFontAtlas_Visitor_AddFont_2476619678(mPointer, font_cfg));
	}
	public ImFont AddFont( in ImFontConfig font_cfg)
	{
		fixed(ImFontConfig* pinned_font_cfg = &font_cfg)
		{
			return AddFont(pinned_font_cfg);
		}
	}
	public ImFont AddFontDefault(ImFontConfig* font_cfg)
	{
		return new ImFont(TitanImGui_ImFontAtlas_Visitor_AddFontDefault_2476619678(mPointer, font_cfg));
	}
	public ImFont AddFontDefault( in ImFontConfig font_cfg)
	{
		fixed(ImFontConfig* pinned_font_cfg = &font_cfg)
		{
			return AddFontDefault(pinned_font_cfg);
		}
	}
	public ImFont AddFontDefaultVector(ImFontConfig* font_cfg)
	{
		return new ImFont(TitanImGui_ImFontAtlas_Visitor_AddFontDefaultVector_2476619678(mPointer, font_cfg));
	}
	public ImFont AddFontDefaultVector( in ImFontConfig font_cfg)
	{
		fixed(ImFontConfig* pinned_font_cfg = &font_cfg)
		{
			return AddFontDefaultVector(pinned_font_cfg);
		}
	}
	public ImFont AddFontDefaultBitmap(ImFontConfig* font_cfg)
	{
		return new ImFont(TitanImGui_ImFontAtlas_Visitor_AddFontDefaultBitmap_2476619678(mPointer, font_cfg));
	}
	public ImFont AddFontDefaultBitmap( in ImFontConfig font_cfg)
	{
		fixed(ImFontConfig* pinned_font_cfg = &font_cfg)
		{
			return AddFontDefaultBitmap(pinned_font_cfg);
		}
	}
	public ImFont AddFontFromFileTTF(string filename,float size_pixels,ImFontConfig* font_cfg,Wchar16* glyph_ranges)
	{
		return new ImFont(TitanImGui_ImFontAtlas_Visitor_AddFontFromFileTTF_293350025(mPointer, filename, size_pixels, font_cfg, glyph_ranges));
	}
	public ImFont AddFontFromFileTTF(string filename,float size_pixels, in ImFontConfig font_cfg, in Wchar16 glyph_ranges)
	{
		fixed(ImFontConfig* pinned_font_cfg = &font_cfg)
		fixed(Wchar16* pinned_glyph_ranges = &glyph_ranges)
		{
			return AddFontFromFileTTF(filename, size_pixels, pinned_font_cfg, pinned_glyph_ranges);
		}
	}
	public ImFont AddFontFromMemoryTTF(void* font_data,int font_data_size,float size_pixels,ImFontConfig* font_cfg,Wchar16* glyph_ranges)
	{
		return new ImFont(TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryTTF_561799227(mPointer, font_data, font_data_size, size_pixels, font_cfg, glyph_ranges));
	}
	public ImFont AddFontFromMemoryTTF(void* font_data,int font_data_size,float size_pixels, in ImFontConfig font_cfg, in Wchar16 glyph_ranges)
	{
		fixed(ImFontConfig* pinned_font_cfg = &font_cfg)
		fixed(Wchar16* pinned_glyph_ranges = &glyph_ranges)
		{
			return AddFontFromMemoryTTF(font_data, font_data_size, size_pixels, pinned_font_cfg, pinned_glyph_ranges);
		}
	}
	public ImFont AddFontFromMemoryCompressedTTF(void* compressed_font_data,int compressed_font_data_size,float size_pixels,ImFontConfig* font_cfg,Wchar16* glyph_ranges)
	{
		return new ImFont(TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryCompressedTTF_1500108386(mPointer, compressed_font_data, compressed_font_data_size, size_pixels, font_cfg, glyph_ranges));
	}
	public ImFont AddFontFromMemoryCompressedTTF(void* compressed_font_data,int compressed_font_data_size,float size_pixels, in ImFontConfig font_cfg, in Wchar16 glyph_ranges)
	{
		fixed(ImFontConfig* pinned_font_cfg = &font_cfg)
		fixed(Wchar16* pinned_glyph_ranges = &glyph_ranges)
		{
			return AddFontFromMemoryCompressedTTF(compressed_font_data, compressed_font_data_size, size_pixels, pinned_font_cfg, pinned_glyph_ranges);
		}
	}
	public ImFont AddFontFromMemoryCompressedBase85TTF(string compressed_font_data_base85,float size_pixels,ImFontConfig* font_cfg,Wchar16* glyph_ranges)
	{
		return new ImFont(TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryCompressedBase85TTF_293350025(mPointer, compressed_font_data_base85, size_pixels, font_cfg, glyph_ranges));
	}
	public ImFont AddFontFromMemoryCompressedBase85TTF(string compressed_font_data_base85,float size_pixels, in ImFontConfig font_cfg, in Wchar16 glyph_ranges)
	{
		fixed(ImFontConfig* pinned_font_cfg = &font_cfg)
		fixed(Wchar16* pinned_glyph_ranges = &glyph_ranges)
		{
			return AddFontFromMemoryCompressedBase85TTF(compressed_font_data_base85, size_pixels, pinned_font_cfg, pinned_glyph_ranges);
		}
	}
	public void RemoveFont(ImFont font)
	{
		TitanImGui_ImFontAtlas_Visitor_RemoveFont_2187443828(mPointer, font);
	}
	public void Clear()
	{
		TitanImGui_ImFontAtlas_Visitor_Clear_2960189489(mPointer);
	}
	public void CompactCache()
	{
		TitanImGui_ImFontAtlas_Visitor_CompactCache_2960189489(mPointer);
	}
	public void ClearInputData()
	{
		TitanImGui_ImFontAtlas_Visitor_ClearInputData_2960189489(mPointer);
	}
	public void ClearFonts()
	{
		TitanImGui_ImFontAtlas_Visitor_ClearFonts_2960189489(mPointer);
	}
	public void ClearTexData()
	{
		TitanImGui_ImFontAtlas_Visitor_ClearTexData_2960189489(mPointer);
	}
	public bool Build()
	{
		return TitanImGui_ImFontAtlas_Visitor_Build_1117990983(mPointer) == 0 ? false : true;
	}
	public void GetTexDataAsAlpha8(byte** out_pixels,int* out_width,int* out_height,int* out_bytes_per_pixel)
	{
		TitanImGui_ImFontAtlas_Visitor_GetTexDataAsAlpha8_3038095861(mPointer, out_pixels, out_width, out_height, out_bytes_per_pixel);
	}
	public void GetTexDataAsAlpha8(byte** out_pixels, ref int out_width, ref int out_height, ref int out_bytes_per_pixel)
	{
		fixed(int* pinned_out_width = &out_width)
		fixed(int* pinned_out_height = &out_height)
		fixed(int* pinned_out_bytes_per_pixel = &out_bytes_per_pixel)
		{
			GetTexDataAsAlpha8(out_pixels, pinned_out_width, pinned_out_height, pinned_out_bytes_per_pixel);
		}
	}
	public void GetTexDataAsRGBA32(byte** out_pixels,int* out_width,int* out_height,int* out_bytes_per_pixel)
	{
		TitanImGui_ImFontAtlas_Visitor_GetTexDataAsRGBA32_3038095861(mPointer, out_pixels, out_width, out_height, out_bytes_per_pixel);
	}
	public void GetTexDataAsRGBA32(byte** out_pixels, ref int out_width, ref int out_height, ref int out_bytes_per_pixel)
	{
		fixed(int* pinned_out_width = &out_width)
		fixed(int* pinned_out_height = &out_height)
		fixed(int* pinned_out_bytes_per_pixel = &out_bytes_per_pixel)
		{
			GetTexDataAsRGBA32(out_pixels, pinned_out_width, pinned_out_height, pinned_out_bytes_per_pixel);
		}
	}
	public void SetTexID(ulong id)
	{
		TitanImGui_ImFontAtlas_Visitor_SetTexID_3315747347(mPointer, id);
	}
	public void SetTexID(ImTextureRef id)
	{
		TitanImGui_ImFontAtlas_Visitor_SetTexID_2544618575(mPointer, id);
	}
	public bool IsBuilt()
	{
		return TitanImGui_ImFontAtlas_Visitor_IsBuilt_82051314(mPointer) == 0 ? false : true;
	}
	public Wchar16* GetGlyphRangesDefault()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesDefault_1075134865(mPointer);
	}
	public Wchar16* GetGlyphRangesGreek()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesGreek_1075134865(mPointer);
	}
	public Wchar16* GetGlyphRangesKorean()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesKorean_1075134865(mPointer);
	}
	public Wchar16* GetGlyphRangesJapanese()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesJapanese_1075134865(mPointer);
	}
	public Wchar16* GetGlyphRangesChineseFull()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesChineseFull_1075134865(mPointer);
	}
	public Wchar16* GetGlyphRangesChineseSimplifiedCommon()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesChineseSimplifiedCommon_1075134865(mPointer);
	}
	public Wchar16* GetGlyphRangesCyrillic()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesCyrillic_1075134865(mPointer);
	}
	public Wchar16* GetGlyphRangesThai()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesThai_1075134865(mPointer);
	}
	public Wchar16* GetGlyphRangesVietnamese()
	{
		return TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesVietnamese_1075134865(mPointer);
	}
	public void RemoveCustomRect(int id)
	{
		TitanImGui_ImFontAtlas_Visitor_RemoveCustomRect_3788306356(mPointer, id);
	}
	public int AddCustomRectRegular(int w,int h)
	{
		return TitanImGui_ImFontAtlas_Visitor_AddCustomRectRegular_2078294092(mPointer, w, h);
	}
	public int AddCustomRectFontGlyph(ImFont font,Wchar16 codepoint,int w,int h,float advance_x,EngineNS.Vector2* offset)
	{
		return TitanImGui_ImFontAtlas_Visitor_AddCustomRectFontGlyph_1754010483(mPointer, font, codepoint, w, h, advance_x, offset);
	}
	public int AddCustomRectFontGlyph(ImFont font,Wchar16 codepoint,int w,int h,float advance_x, in EngineNS.Vector2 offset)
	{
		fixed(EngineNS.Vector2* pinned_offset = &offset)
		{
			return AddCustomRectFontGlyph(font, codepoint, w, h, advance_x, pinned_offset);
		}
	}
	public int AddCustomRectFontGlyphForSize(ImFont font,float font_size,Wchar16 codepoint,int w,int h,float advance_x,EngineNS.Vector2* offset)
	{
		return TitanImGui_ImFontAtlas_Visitor_AddCustomRectFontGlyphForSize_986229207(mPointer, font, font_size, codepoint, w, h, advance_x, offset);
	}
	public int AddCustomRectFontGlyphForSize(ImFont font,float font_size,Wchar16 codepoint,int w,int h,float advance_x, in EngineNS.Vector2 offset)
	{
		fixed(EngineNS.Vector2* pinned_offset = &offset)
		{
			return AddCustomRectFontGlyphForSize(font, font_size, codepoint, w, h, advance_x, pinned_offset);
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static EngineNS.FRttiStruct* TitanImGui_ImFontAtlas_Visitor_GetTypeRtti();
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFontAtlas* TitanImGui_ImFontAtlas_Visitor_CreateInstance_2960189489();
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_Dispose(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__Flags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__Flags(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexGlyphPadding(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexGlyphPadding(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMinWidth(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMinWidth(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMinHeight(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMinHeight(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMaxWidth(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMaxWidth(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexMaxHeight(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexMaxHeight(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImFontAtlas_Visitor_FieldGet__UserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__UserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFontAtlas_Visitor_FieldGet__Locked(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__Locked(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFontAtlas_Visitor_FieldGet__RendererHasTextures(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__RendererHasTextures(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFontAtlas_Visitor_FieldGet__TexIsBuilt(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexIsBuilt(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFontAtlas_Visitor_FieldGet__TexPixelsUseColors(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexPixelsUseColors(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvScale(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvWhitePixel(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvWhitePixel(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector4* TitanImGui_ImFontAtlas_Visitor_FieldGet__TexUvLines(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexUvLines(void* self, EngineNS.Vector4* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__TexNextUniqueID(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__TexNextUniqueID(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__FontNextUniqueID(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__FontNextUniqueID(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderName(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderName(void* self, [MarshalAs(UnmanagedType.LPUTF8Str)] string value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImFontAtlas_Visitor_FieldGet__FontLoaderFlags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__FontLoaderFlags(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontAtlas_Visitor_FieldGet__RefCount(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontAtlas_Visitor_FieldSet__RefCount(void* self, int value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFontAtlas_Visitor_AddFont_2476619678(void* Self, ImFontConfig* font_cfg);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontDefault_2476619678(void* Self, ImFontConfig* font_cfg);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontDefaultVector_2476619678(void* Self, ImFontConfig* font_cfg);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontDefaultBitmap_2476619678(void* Self, ImFontConfig* font_cfg);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontFromFileTTF_293350025(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string filename,float size_pixels,ImFontConfig* font_cfg,Wchar16* glyph_ranges);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryTTF_561799227(void* Self, void* font_data,int font_data_size,float size_pixels,ImFontConfig* font_cfg,Wchar16* glyph_ranges);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryCompressedTTF_1500108386(void* Self, void* compressed_font_data,int compressed_font_data_size,float size_pixels,ImFontConfig* font_cfg,Wchar16* glyph_ranges);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImFont* TitanImGui_ImFontAtlas_Visitor_AddFontFromMemoryCompressedBase85TTF_293350025(void* Self, [MarshalAs(UnmanagedType.LPUTF8Str)] string compressed_font_data_base85,float size_pixels,ImFontConfig* font_cfg,Wchar16* glyph_ranges);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_RemoveFont_2187443828(void* Self, ImFont font);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_Clear_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_CompactCache_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_ClearInputData_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_ClearFonts_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_ClearTexData_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImFontAtlas_Visitor_Build_1117990983(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_GetTexDataAsAlpha8_3038095861(void* Self, byte** out_pixels,int* out_width,int* out_height,int* out_bytes_per_pixel);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_GetTexDataAsRGBA32_3038095861(void* Self, byte** out_pixels,int* out_width,int* out_height,int* out_bytes_per_pixel);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_SetTexID_3315747347(void* Self, ulong id);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_SetTexID_2544618575(void* Self, ImTextureRef id);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static sbyte TitanImGui_ImFontAtlas_Visitor_IsBuilt_82051314(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesDefault_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesGreek_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesKorean_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesJapanese_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesChineseFull_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesChineseSimplifiedCommon_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesCyrillic_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesThai_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static Wchar16* TitanImGui_ImFontAtlas_Visitor_GetGlyphRangesVietnamese_1075134865(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontAtlas_Visitor_RemoveCustomRect_3788306356(void* Self, int id);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static int TitanImGui_ImFontAtlas_Visitor_AddCustomRectRegular_2078294092(void* Self, int w,int h);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static int TitanImGui_ImFontAtlas_Visitor_AddCustomRectFontGlyph_1754010483(void* Self, ImFont font,Wchar16 codepoint,int w,int h,float advance_x,EngineNS.Vector2* offset);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static int TitanImGui_ImFontAtlas_Visitor_AddCustomRectFontGlyphForSize_986229207(void* Self, ImFont font,float font_size,Wchar16 codepoint,int w,int h,float advance_x,EngineNS.Vector2* offset);
	//Cast
	#endregion
}
