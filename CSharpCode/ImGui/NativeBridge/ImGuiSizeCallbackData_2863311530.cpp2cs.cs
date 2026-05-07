//generated from legacy CppWeaving ImGui surface; native entrypoints routed through TitanImGuiBridge
using System;
using System.Runtime.InteropServices;
#if DEBUG
	using SuppressGC = EngineNS.Rtti.TtDummyAttribute;
#else
	using SuppressGC = System.Runtime.InteropServices.SuppressGCTransitionAttribute;
#endif


[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 32, Pack = 8)]
public unsafe partial struct ImGuiSizeCallbackData : EngineNS.IPtrType
{
	#region StructLayout
	[System.Runtime.InteropServices.FieldOffset(0)]
	public void* m_UserData;
	[System.Runtime.InteropServices.FieldOffset(8)]
	public EngineNS.Vector2 m_Pos;
	[System.Runtime.InteropServices.FieldOffset(16)]
	public EngineNS.Vector2 m_CurrentSize;
	[System.Runtime.InteropServices.FieldOffset(24)]
	public EngineNS.Vector2 m_DesiredSize;
	#endregion
	public IntPtr NativePointer { get => IntPtr.Zero; set {} }
	#region Constructor&Cast
	public void UnsafeCallDestructor()
	{
		fixed (ImGuiSizeCallbackData* mPointer = &this)
		{
			TitanImGui_ImGuiSizeCallbackData_Visitor_UnsafeCallDestructor(mPointer);
		}
	}
	#endregion
	#region Fields
	public void* UserData
	{
		get
		{
			fixed (ImGuiSizeCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__UserData(mPointer);
			}
		}
		set
		{
			fixed (ImGuiSizeCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__UserData(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 Pos
	{
		get
		{
			fixed (ImGuiSizeCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__Pos(mPointer);
			}
		}
		set
		{
			fixed (ImGuiSizeCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__Pos(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 CurrentSize
	{
		get
		{
			fixed (ImGuiSizeCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__CurrentSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiSizeCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__CurrentSize(mPointer, value);
			}
		}
	}
	public EngineNS.Vector2 DesiredSize
	{
		get
		{
			fixed (ImGuiSizeCallbackData* mPointer = &this)
			{
				return TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__DesiredSize(mPointer);
			}
		}
		set
		{
			fixed (ImGuiSizeCallbackData* mPointer = &this)
			{
				TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__DesiredSize(mPointer, value);
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
	extern static void TitanImGui_ImGuiSizeCallbackData_Visitor_UnsafeCallDestructor(void* self);
	//Fields
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void* TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__UserData(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__UserData(void* self, void* value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__Pos(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__Pos(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__CurrentSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__CurrentSize(void* self, EngineNS.Vector2 value);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static EngineNS.Vector2 TitanImGui_ImGuiSizeCallbackData_Visitor_FieldGet__DesiredSize(void* self);
	[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	[SuppressGC]
	extern static void TitanImGui_ImGuiSizeCallbackData_Visitor_FieldSet__DesiredSize(void* self, EngineNS.Vector2 value);
	//Functions
	//Cast
	#endregion
}
