//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 72, Pack = 8)]
public unsafe partial struct ImDrawData : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public bool m_Valid;
	[System.Runtime.InteropServices.FieldOffset(4)]
	public int m_CmdListsCount;
	[System.Runtime.InteropServices.FieldOffset(8)]
	public int m_TotalIdxCount;
	[System.Runtime.InteropServices.FieldOffset(12)]
	public int m_TotalVtxCount;
	[System.Runtime.InteropServices.FieldOffset(32)]
	public EngineNS.Vector2 m_DisplayPos;
	[System.Runtime.InteropServices.FieldOffset(40)]
	public EngineNS.Vector2 m_DisplaySize;
	[System.Runtime.InteropServices.FieldOffset(48)]
	public EngineNS.Vector2 m_FramebufferScale;
	[System.Runtime.InteropServices.FieldOffset(56)]
	public ImGuiViewport* m_OwnerViewport;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallConstructor()
	{
		fixed (ImDrawData* mPointer = &this)
		{
			TitanImGui_ImDrawData_Visitor_UnsafeCallConstructor_2960189489(mPointer);
		}
	}
	public void UnsafeCallDestructor()
	{
		fixed (ImDrawData* mPointer = &this)
		{
			TitanImGui_ImDrawData_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public bool Valid
	{
		get
		{
			fixed (ImDrawData* mPointer = &this)
			{
				return TitanImGui_ImDrawData_Visitor_FieldGet__Valid(mPointer) == 0 ? false : true;
			}
		}
		set
		{
			fixed (ImDrawData* mPointer = &this)
			{
				TitanImGui_ImDrawData_Visitor_FieldSet__Valid(mPointer, value);
			}
		}
	}
	public int CmdListsCount
	{
		get
		{
			fixed (ImDrawData* mPointer = &this)
			{
				return TitanImGui_ImDrawData_Visitor_FieldGet__CmdListsCount(mPointer);
			}
		}
		set
		{
			fixed (ImDrawData* mPointer = &this)
			{
				TitanImGui_ImDrawData_Visitor_FieldSet__CmdListsCount(mPointer, value);
			}
		}
	}
	public int TotalIdxCount
	{
		get
		{
			fixed (ImDrawData* mPointer = &this)
			{
				return TitanImGui_ImDrawData_Visitor_FieldGet__TotalIdxCount(mPointer);
			}
		}
		set
		{
			fixed (ImDrawData* mPointer = &this)
			{
				TitanImGui_ImDrawData_Visitor_FieldSet__TotalIdxCount(mPointer, value);
			}
		}
	}
	public int TotalVtxCount
	{
		get
		{
			fixed (ImDrawData* mPointer = &this)
			{
				return TitanImGui_ImDrawData_Visitor_FieldGet__TotalVtxCount(mPointer);
			}
		}
		set
		{
			fixed (ImDrawData* mPointer = &this)
			{
				TitanImGui_ImDrawData_Visitor_FieldSet__TotalVtxCount(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 DisplayPos
	{
		get
		{
			fixed (ImDrawData* mPointer = &this)
			{
				return TitanImGui_ImDrawData_Visitor_FieldGet__DisplayPos(mPointer);
			}
		}
		set
		{
			fixed (ImDrawData* mPointer = &this)
			{
				TitanImGui_ImDrawData_Visitor_FieldSet__DisplayPos(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 DisplaySize
	{
		get
		{
			fixed (ImDrawData* mPointer = &this)
			{
				return TitanImGui_ImDrawData_Visitor_FieldGet__DisplaySize(mPointer);
			}
		}
		set
		{
			fixed (ImDrawData* mPointer = &this)
			{
				TitanImGui_ImDrawData_Visitor_FieldSet__DisplaySize(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 FramebufferScale
	{
		get
		{
			fixed (ImDrawData* mPointer = &this)
			{
				return TitanImGui_ImDrawData_Visitor_FieldGet__FramebufferScale(mPointer);
			}
		}
		set
		{
			fixed (ImDrawData* mPointer = &this)
			{
				TitanImGui_ImDrawData_Visitor_FieldSet__FramebufferScale(mPointer, value);
			}
		}
	}
	public ImGuiViewport* OwnerViewport
	{
		get
		{
			fixed (ImDrawData* mPointer = &this)
			{
				return TitanImGui_ImDrawData_Visitor_FieldGet__OwnerViewport(mPointer);
			}
		}
		set
		{
			fixed (ImDrawData* mPointer = &this)
			{
				TitanImGui_ImDrawData_Visitor_FieldSet__OwnerViewport(mPointer, value);
			}
		}
	}
	#endregion
	#region Function
	public void Clear()
	{
		fixed (ImDrawData* mPointer = &this)
		{
			TitanImGui_ImDrawData_Visitor_Clear_2960189489(mPointer);
		}
	}
	public void AddDrawList(ImDrawList draw_list)
	{
		fixed (ImDrawData* mPointer = &this)
		{
			TitanImGui_ImDrawData_Visitor_AddDrawList_64523837(mPointer, draw_list);
		}
	}
	public void DeIndexAllBuffers()
	{
		fixed (ImDrawData* mPointer = &this)
		{
			TitanImGui_ImDrawData_Visitor_DeIndexAllBuffers_2960189489(mPointer);
		}
	}
	public void ScaleClipRects(EngineNS.Vector2* fb_scale)
	{
		fixed (ImDrawData* mPointer = &this)
		{
			TitanImGui_ImDrawData_Visitor_ScaleClipRects_2086025684(mPointer, fb_scale);
		}
	}
	public void ScaleClipRects( in EngineNS.Vector2 fb_scale)
	{
		fixed(EngineNS.Vector2* pinned_fb_scale = &fb_scale)
		{
			ScaleClipRects(pinned_fb_scale);
		}
	}
	public ImDrawList** GetCmdLists()
	{
		fixed (ImDrawData* mPointer = &this)
		{
			return TitanImGui_ImDrawData_Visitor_GetCmdLists_1014668303(mPointer);
		}
	}
	#endregion
	#region Core SDK
	const string ModuleNC = EngineNS.CoreSDK.CoreModule;
	//Constructor&Cast
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawData_Visitor_UnsafeCallConstructor_2960189489(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawData_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static sbyte TitanImGui_ImDrawData_Visitor_FieldGet__Valid(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawData_Visitor_FieldSet__Valid(void* self, bool value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImDrawData_Visitor_FieldGet__CmdListsCount(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawData_Visitor_FieldSet__CmdListsCount(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImDrawData_Visitor_FieldGet__TotalIdxCount(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawData_Visitor_FieldSet__TotalIdxCount(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static int TitanImGui_ImDrawData_Visitor_FieldGet__TotalVtxCount(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawData_Visitor_FieldSet__TotalVtxCount(void* self, int value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImDrawData_Visitor_FieldGet__DisplayPos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawData_Visitor_FieldSet__DisplayPos(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImDrawData_Visitor_FieldGet__DisplaySize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawData_Visitor_FieldSet__DisplaySize(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImDrawData_Visitor_FieldGet__FramebufferScale(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawData_Visitor_FieldSet__FramebufferScale(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static ImGuiViewport* TitanImGui_ImDrawData_Visitor_FieldGet__OwnerViewport(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawData_Visitor_FieldSet__OwnerViewport(void* self, ImGuiViewport* value);
	//Functions
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawData_Visitor_Clear_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawData_Visitor_AddDrawList_64523837(void* Self, ImDrawList draw_list);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawData_Visitor_DeIndexAllBuffers_2960189489(void* Self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static void TitanImGui_ImDrawData_Visitor_ScaleClipRects_2086025684(void* Self, EngineNS.Vector2* fb_scale);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	extern static ImDrawList** TitanImGui_ImDrawData_Visitor_GetCmdLists_1014668303(void* Self);
	//Cast
	#endregion
}
