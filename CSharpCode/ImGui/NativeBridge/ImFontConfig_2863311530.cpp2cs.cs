//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 160, Pack = 8)]
public unsafe partial struct ImFontConfig : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public sbyte* m_Name;
	[System.Runtime.InteropServices.FieldOffset(40)]
	public void* m_FontData;
	[System.Runtime.InteropServices.FieldOffset(48)]
	public int m_FontDataSize;
	[System.Runtime.InteropServices.FieldOffset(52)]
	public bool m_FontDataOwnedByAtlas;
	[System.Runtime.InteropServices.FieldOffset(53)]
	public bool m_MergeMode;
	[System.Runtime.InteropServices.FieldOffset(54)]
	public bool m_PixelSnapH;
	[System.Runtime.InteropServices.FieldOffset(58)]
	public Wchar16 m_EllipsisChar;
	[System.Runtime.InteropServices.FieldOffset(60)]
	public float m_SizePixels;
	[System.Runtime.InteropServices.FieldOffset(64)]
	public Wchar16* m_GlyphRanges;
	[System.Runtime.InteropServices.FieldOffset(72)]
	public Wchar16* m_GlyphExcludeRanges;
	[System.Runtime.InteropServices.FieldOffset(80)]
	public EngineNS.Vector2 m_GlyphOffset;
	[System.Runtime.InteropServices.FieldOffset(88)]
	public float m_GlyphMinAdvanceX;
	[System.Runtime.InteropServices.FieldOffset(92)]
	public float m_GlyphMaxAdvanceX;
	[System.Runtime.InteropServices.FieldOffset(96)]
	public float m_GlyphExtraAdvanceX;
	[System.Runtime.InteropServices.FieldOffset(100)]
	public uint m_FontNo;
	[System.Runtime.InteropServices.FieldOffset(104)]
	public uint m_FontLoaderFlags;
	[System.Runtime.InteropServices.FieldOffset(108)]
	public float m_RasterizerMultiply;
	[System.Runtime.InteropServices.FieldOffset(112)]
	public float m_RasterizerDensity;
	[System.Runtime.InteropServices.FieldOffset(116)]
	public float m_ExtraSizeScale;
	[System.Runtime.InteropServices.FieldOffset(120)]
	public int m_Flags;
	[System.Runtime.InteropServices.FieldOffset(128)]
	public ImFont* m_DstFont;
	[System.Runtime.InteropServices.FieldOffset(144)]
	public void* m_FontLoaderData;
	[System.Runtime.InteropServices.FieldOffset(152)]
	public bool m_PixelSnapV;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImFontConfig* mPointer = &this)
		{
			TitanImGui_ImFontConfig_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImFontConfig* mPointer = &this)
		{
			TitanImGui_ImFontConfig_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public sbyte* Name
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__Name(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__Name(mPointer, value);
			}
		}
	}
	public void* FontData
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__FontData(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__FontData(mPointer, value);
			}
		}
	}
	public int FontDataSize
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__FontDataSize(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__FontDataSize(mPointer, value);
			}
		}
	}
	public bool FontDataOwnedByAtlas
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__FontDataOwnedByAtlas(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__FontDataOwnedByAtlas(mPointer, value);
			}
		}
	}
	public bool MergeMode
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__MergeMode(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__MergeMode(mPointer, value);
			}
		}
	}
	public bool PixelSnapH
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__PixelSnapH(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__PixelSnapH(mPointer, value);
			}
		}
	}
	public Wchar16 EllipsisChar
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__EllipsisChar(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__EllipsisChar(mPointer, value);
			}
		}
	}
	public float SizePixels
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__SizePixels(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__SizePixels(mPointer, value);
			}
		}
	}
	public Wchar16* GlyphRanges
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphRanges(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphRanges(mPointer, value);
			}
		}
	}
	public Wchar16* GlyphExcludeRanges
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphExcludeRanges(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphExcludeRanges(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 GlyphOffset
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphOffset(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphOffset(mPointer, value);
			}
		}
	}
	public float GlyphMinAdvanceX
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphMinAdvanceX(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphMinAdvanceX(mPointer, value);
			}
		}
	}
	public float GlyphMaxAdvanceX
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphMaxAdvanceX(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphMaxAdvanceX(mPointer, value);
			}
		}
	}
	public float GlyphExtraAdvanceX
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphExtraAdvanceX(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphExtraAdvanceX(mPointer, value);
			}
		}
	}
	public uint FontNo
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__FontNo(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__FontNo(mPointer, value);
			}
		}
	}
	public uint FontLoaderFlags
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__FontLoaderFlags(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__FontLoaderFlags(mPointer, value);
			}
		}
	}
	public float RasterizerMultiply
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__RasterizerMultiply(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__RasterizerMultiply(mPointer, value);
			}
		}
	}
	public float RasterizerDensity
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__RasterizerDensity(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__RasterizerDensity(mPointer, value);
			}
		}
	}
	public float ExtraSizeScale
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__ExtraSizeScale(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__ExtraSizeScale(mPointer, value);
			}
		}
	}
	public int Flags
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__Flags(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__Flags(mPointer, value);
			}
		}
	}
	public ImFont DstFont
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return new ImFont(TitanImGui_ImFontConfig_Visitor_FieldGet__DstFont(mPointer));
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__DstFont(mPointer, value);
			}
		}
	}
	public void* FontLoaderData
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__FontLoaderData(mPointer);
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__FontLoaderData(mPointer, value);
			}
		}
	}
	public bool PixelSnapV
	{
		get
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				return TitanImGui_ImFontConfig_Visitor_FieldGet__PixelSnapV(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImFontConfig* mPointer = &this)
			{
				TitanImGui_ImFontConfig_Visitor_FieldSet__PixelSnapV(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontConfig_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImFontConfig_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte* TitanImGui_ImFontConfig_Visitor_FieldGet__Name(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__Name(void* self, sbyte* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImFontConfig_Visitor_FieldGet__FontData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__FontData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontConfig_Visitor_FieldGet__FontDataSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__FontDataSize(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFontConfig_Visitor_FieldGet__FontDataOwnedByAtlas(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__FontDataOwnedByAtlas(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFontConfig_Visitor_FieldGet__MergeMode(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__MergeMode(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFontConfig_Visitor_FieldGet__PixelSnapH(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__PixelSnapH(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static Wchar16 TitanImGui_ImFontConfig_Visitor_FieldGet__EllipsisChar(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__EllipsisChar(void* self, Wchar16 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFontConfig_Visitor_FieldGet__SizePixels(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__SizePixels(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static Wchar16* TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphRanges(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphRanges(void* self, Wchar16* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static Wchar16* TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphExcludeRanges(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphExcludeRanges(void* self, Wchar16* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphOffset(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphOffset(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphMinAdvanceX(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphMinAdvanceX(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphMaxAdvanceX(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphMaxAdvanceX(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFontConfig_Visitor_FieldGet__GlyphExtraAdvanceX(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__GlyphExtraAdvanceX(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImFontConfig_Visitor_FieldGet__FontNo(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__FontNo(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImFontConfig_Visitor_FieldGet__FontLoaderFlags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__FontLoaderFlags(void* self, uint value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFontConfig_Visitor_FieldGet__RasterizerMultiply(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__RasterizerMultiply(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFontConfig_Visitor_FieldGet__RasterizerDensity(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__RasterizerDensity(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static float TitanImGui_ImFontConfig_Visitor_FieldGet__ExtraSizeScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__ExtraSizeScale(void* self, float value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImFontConfig_Visitor_FieldGet__Flags(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__Flags(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImFont* TitanImGui_ImFontConfig_Visitor_FieldGet__DstFont(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__DstFont(void* self, ImFont* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImFontConfig_Visitor_FieldGet__FontLoaderData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__FontLoaderData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImFontConfig_Visitor_FieldGet__PixelSnapV(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImFontConfig_Visitor_FieldSet__PixelSnapV(void* self, bool value);
	//Functions
	//Cast
	#endregion
}
