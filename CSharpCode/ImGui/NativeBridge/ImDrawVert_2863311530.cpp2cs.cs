//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 20, Pack = 4)]
public unsafe partial struct ImDrawVert : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public EngineNS.Vector2 m_pos;
	[System.Runtime.InteropServices.FieldOffset(8)]
	public EngineNS.Vector2 m_uv;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public uint m_col;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallDestructor()
	{
		fixed (ImDrawVert* mPointer = &this)
		{
			TitanImGui_ImDrawVert_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public EngineNS.Vector2 pos
	{
		get
		{
			fixed (ImDrawVert* mPointer = &this)
			{
				return TitanImGui_ImDrawVert_Visitor_FieldGet__pos(mPointer);
			}
		}
		set
		{
			fixed (ImDrawVert* mPointer = &this)
			{
				TitanImGui_ImDrawVert_Visitor_FieldSet__pos(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 uv
	{
		get
		{
			fixed (ImDrawVert* mPointer = &this)
			{
				return TitanImGui_ImDrawVert_Visitor_FieldGet__uv(mPointer);
			}
		}
		set
		{
			fixed (ImDrawVert* mPointer = &this)
			{
				TitanImGui_ImDrawVert_Visitor_FieldSet__uv(mPointer, value);
			}
		}
	}
	public uint col
	{
		get
		{
			fixed (ImDrawVert* mPointer = &this)
			{
				return TitanImGui_ImDrawVert_Visitor_FieldGet__col(mPointer);
			}
		}
		set
		{
			fixed (ImDrawVert* mPointer = &this)
			{
				TitanImGui_ImDrawVert_Visitor_FieldSet__col(mPointer, value);
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
	extern static void TitanImGui_ImDrawVert_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImDrawVert_Visitor_FieldGet__pos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawVert_Visitor_FieldSet__pos(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImDrawVert_Visitor_FieldGet__uv(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawVert_Visitor_FieldSet__uv(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static uint TitanImGui_ImDrawVert_Visitor_FieldGet__col(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImDrawVert_Visitor_FieldSet__col(void* self, uint value);
	//Functions
	//Cast
	#endregion
}
